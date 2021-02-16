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
using System.Linq;
using System.Threading.Tasks;

namespace RunBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("runbot root.dialog [firstText]");
                return;
            }

            string rootDialog = Path.GetFileName(args[0]);
            string text = args.Skip(1).FirstOrDefault();
            string folder = Path.GetDirectoryName(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, args[0])));

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .AddJsonFile(Path.Combine(folder, "settings", "appsettings.json"))
                .Build();

            ComponentRegistration.Add(new DialogsComponentRegistration());
            ComponentRegistration.Add(new DeclarativeComponentRegistration());
            ComponentRegistration.Add(new AdaptiveComponentRegistration());
            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());
            ComponentRegistration.Add(new LucyComponentRegistration());
            ComponentRegistration.Add(new ConsoleComponentRegistration());

            var userStoragePath = Path.Combine(Path.GetTempPath(), rootDialog);
            Directory.CreateDirectory(userStoragePath);
            var adapter = new ConsoleAdapter()
                .Use(new RegisterClassMiddleware<IConfiguration>(configuration))
                .UseStorage(new MemoryStorage())
                .UseBotState(new ConversationState(new MemoryStorage()), new UserState(new FileStorage(userStoragePath)));

            var resourceExplorer = new ResourceExplorer()
                .AddFolder(folder);

            var bot = new DialogManager() { RootDialog = resourceExplorer.LoadType<AdaptiveDialog>(resourceExplorer.GetResource(rootDialog)) }
                .UseResourceExplorer(resourceExplorer)
                .UseLanguageGeneration();

            await ((ConsoleAdapter)adapter).StartConversation(bot.OnTurnAsync, text);
        }
    }
}
