using Iciclecreek.Bot.Builder.Dialogs.Adaptive.GitHub;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.Azure.Queues;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using System.Threading;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(RepoBot.Startup))]

namespace RepoBot
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var context = builder.GetContext();
            var services = builder.Services;

            // Adaptive component registration
            ComponentRegistration.Add(new DialogsComponentRegistration());
            ComponentRegistration.Add(new DeclarativeComponentRegistration());
            ComponentRegistration.Add(new AdaptiveComponentRegistration());
            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());
            ComponentRegistration.Add(new GithubComponentRegistration());

            services.AddSingleton<Settings>((s) =>
            {
                var botSettings = new Settings();
                s.GetService<IConfiguration>().Bind(botSettings);
                return botSettings;
            });
            services.AddLogging();
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
            services.AddSingleton<AuthenticationConfiguration>();
            services.AddSingleton<Octokit.GitHubClient>((s) => new Octokit.GitHubClient(new ProductHeaderValue(s.GetService<Settings>().MicrosoftAppId, "1.0")));
            services.AddSingleton<IStorage>((s) => new BlobsStorage(s.GetService<Settings>().AzureWebJobsStorage, s.GetService<Settings>().BotId.ToLower()));
            services.AddSingleton<UserState>(s => new UserState(s.GetService<IStorage>()));
            services.AddSingleton<ConversationState>(s => new ConversationState(s.GetService<IStorage>()));
            services.AddSingleton<QueueStorage>((s) => (QueueStorage)new AzureQueueStorage(s.GetService<Settings>().AzureWebJobsStorage, "activities"));

            // skill support
            // Register the skills client and skills request handler.
            //services.AddSingleton<SkillConversationIdFactoryBase, SkillConversationIdFactory>();
            //services.AddHttpClient<BotFrameworkClient, SkillHttpClient>();
            //services.AddSingleton<ChannelServiceHandler, SkillHandler>();

            // bot framework adapter defintion
            services.AddSingleton<BotFrameworkHttpAdapter>(s =>
            {
                // create botframework adapter for processing conversations with users.
                var adapter = new BotFrameworkHttpAdapter(s.GetService<ICredentialProvider>())
                    .Use(new RegisterClassMiddleware<IConfiguration>(s.GetService<IConfiguration>()))
                    .UseStorage(s.GetService<IStorage>())
                    .UseBotState(s.GetService<UserState>(), s.GetService<ConversationState>())
                    .Use(new RegisterClassMiddleware<QueueStorage>(s.GetService<QueueStorage>()))
                    .Use(new ShowTypingMiddleware());

                adapter.OnTurnError = async (turnContext, exception) =>
                {
                    await turnContext.SendActivityAsync(exception.Message).ConfigureAwait(false);
                    var conversationState = turnContext.TurnState.Get<ConversationState>();
                    await conversationState.ClearStateAsync(turnContext).ConfigureAwait(false);
                    await conversationState.SaveChangesAsync(turnContext).ConfigureAwait(false);
                };

                return (BotFrameworkHttpAdapter)adapter;
            });

            // add github adapter
            services.AddSingleton<GitHubAdapter>((s) =>
            {
                // create github adapter for processing webhooks
                return (GitHubAdapter)new GitHubAdapter(s.GetService<IConfiguration>(), s.GetService<Octokit.GitHubClient>())
                    .UseStorage(s.GetService<IStorage>())
                    .UseBotState(s.GetService<UserState>(), s.GetService<ConversationState>())
                    .Use(new RegisterClassMiddleware<QueueStorage>(s.GetService<QueueStorage>()));
            });
            
            // bot
            services.AddSingleton<IBot>((s) =>
            {
                // create bot using resourceexplorer and .dialog file
                var resourceExplorer = new ResourceExplorer()
                    .AddFolder(context.ApplicationRootPath);

                var bot = new Bot()
                    .UseResourceExplorer(resourceExplorer)
                    .UseLanguageGeneration();

                bot.RootDialog = resourceExplorer.LoadType<AdaptiveDialog>(resourceExplorer.GetResource("RepoBot.dialog"));
                return (IBot)bot;
            });


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
