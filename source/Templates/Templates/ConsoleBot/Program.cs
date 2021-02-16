using Iciclecreek.Bot.Builder;
using Iciclecreek.Bot.Builder.Adapters;
using Iciclecreek.Bot.Builder.Dialogs.Adaptive;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
             var configuration = new ConfigurationBuilder().AddEnvironmentVariables().AddCommandLine(args).Build();
            ComponentRegistration.Add(new DialogsComponentRegistration());
            ComponentRegistration.Add(new DeclarativeComponentRegistration());
            ComponentRegistration.Add(new AdaptiveComponentRegistration());
            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());
            ComponentRegistration.Add(new LucyComponentRegistration());
            ComponentRegistration.Add(new ConsoleComponentRegistration());

            var appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            var userStoragePath = Path.Combine(Path.GetTempPath(), appName);
            Directory.CreateDirectory(userStoragePath);
            var adapter = new ConsoleAdapter()
                .Use(new RegisterClassMiddleware<IConfiguration>(configuration))
                .UseStorage(new MemoryStorage())
                .UseBotState(new ConversationState(new MemoryStorage()), new UserState(new FileStorage(userStoragePath)));

            var resourceExplorer = new ResourceExplorer()
                .AddFolder(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "ConsoleBot"));

            var bot = new DialogManager() { RootDialog = resourceExplorer.LoadType<AdaptiveDialog>(resourceExplorer.GetResource("ConsoleBot.dialog")) }
                .UseResourceExplorer(resourceExplorer)
                .UseLanguageGeneration();

            await ((ConsoleAdapter)adapter).StartConversation(bot.OnTurnAsync, String.Join(" ", args));
        }
    }
}
