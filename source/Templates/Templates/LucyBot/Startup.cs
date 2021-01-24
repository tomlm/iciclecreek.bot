using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.Azure.Queues;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(LucyBot.Startup))]

namespace LucyBot
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var context = builder.GetContext();
            var configuration = context.Configuration;
            var services = builder.Services;

            // Adaptive component registration
            ComponentRegistration.Add(new DialogsComponentRegistration());
            ComponentRegistration.Add(new DeclarativeComponentRegistration());
            ComponentRegistration.Add(new AdaptiveComponentRegistration());
            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());
            ComponentRegistration.Add(new LucyComponentRegistration());

            services.AddLogging();
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
            services.AddSingleton(sp => new AuthenticationConfiguration { ClaimsValidator = new AllowedCallersClaimsValidator(configuration) });
            services.AddSingleton<IStorage>((s) => new BlobsStorage(configuration.GetValue<string>("AzureWebJobsStorage"), configuration.GetValue<string>("BotId").ToLower()));
            services.AddSingleton<UserState>(s => new UserState(s.GetService<IStorage>()));
            services.AddSingleton<ConversationState>(s => new ConversationState(s.GetService<IStorage>()));
            services.AddSingleton<QueueStorage>((s) => (QueueStorage)new AzureQueueStorage(configuration.GetValue<string>("AzureWebJobsStorage"), "activities"));

            // skill support
            // Register the skills client and skills request handler.
            services.AddSingleton<SkillConversationIdFactoryBase, SkillConversationIdFactory>();
            services.AddHttpClient<SkillHttpClient>();
            services.AddSingleton<SkillHandler>();

            // bot framework adapter defintion
            services.AddSingleton<BotAdapter>(s => (BotAdapter)s.GetService<IBotFrameworkHttpAdapter>());
            services.AddSingleton<IBotFrameworkHttpAdapter>(s =>
                {
                    // create botframework adapter for processing conversations with users.
                    var adapter = new BotFrameworkHttpAdapter(s.GetService<ICredentialProvider>())
                             .Use(new RegisterClassMiddleware<IConfiguration>(s.GetService<IConfiguration>()))
                             .UseStorage(s.GetService<IStorage>())
                             .UseBotState(s.GetService<UserState>(), s.GetService<ConversationState>())
                             .UseDebugger(5001)
                             .Use(new RegisterClassMiddleware<QueueStorage>(s.GetService<QueueStorage>()));

                    adapter.OnTurnError = async (turnContext, exception) =>
                    {
                        await turnContext.TraceActivityAsync("OnTurnError Trace", exception.ToString(), "https://www.botframework.com/schemas/error", "TurnError");
                        System.Diagnostics.Trace.Fail(exception.Message, exception.ToString());
                        await turnContext.SendActivityAsync(exception.Message).ConfigureAwait(false);
                        var conversationState = turnContext.TurnState.Get<ConversationState>();
                        await conversationState.ClearStateAsync(turnContext).ConfigureAwait(false);
                        await conversationState.SaveChangesAsync(turnContext).ConfigureAwait(false);
                    };

                    return (IBotFrameworkHttpAdapter)adapter;
                });

            // bot
            services.AddSingleton<IBot>((s) =>
            {
                // create bot using resourceexplorer and .dialog file
                var config = s.GetService<IConfiguration>();
                string rootPath = GetDialogsFolder(config, context.ApplicationRootPath);
                var resourceExplorer = new ResourceExplorer()
                    .AddFolder(rootPath);

                var bot = new Bot()
                    .UseResourceExplorer(resourceExplorer)
                    .UseLanguageGeneration();

                bot.InitialTurnState.Set<BotFrameworkClient>(s.GetService<SkillHttpClient>());
                bot.InitialTurnState.Set(s.GetService<SkillConversationIdFactoryBase>());
                
                bot.RootDialog = resourceExplorer.LoadType<AdaptiveDialog>(resourceExplorer.GetResource("LucyBot.dialog"));
                resourceExplorer.Changed += (sender, e) =>
                {
                    Console.WriteLine("Resources changed, reloading...");
                    bot.RootDialog = resourceExplorer.LoadType<AdaptiveDialog>(resourceExplorer.GetResource("LucyBot.dialog"));
                };
                return (IBot)bot;
            });
        }
        private string GetDialogsFolder(IConfiguration config, string root)
        {
            if (config.GetValue<string>("AzureWebJobsStorage") == "UseDevelopmentStorage=true")
            {
                // we want the source dialogs folder, not the output content dialogs so we can edit and reload automatically
                for (int i = 0; i < 3; i++)
                {
                    root = Path.GetDirectoryName(root);
                    var dialogsPath = Path.Combine(root, "LucyBot");
                    if (Directory.Exists(dialogsPath))
                    {
                        return dialogsPath;
                    }
                }
            }
            return Path.Combine(root, "LucyBot");
        }
    }

    public class Bot : DialogManager, IBot
    {
        Task IBot.OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return base.OnTurnAsync(turnContext, cancellationToken);
        }
    }
}
