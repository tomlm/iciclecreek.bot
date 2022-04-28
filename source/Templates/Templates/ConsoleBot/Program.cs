using ConsoleBot.Dialogs;
using Iciclecreek.Bot.Builder;
using Iciclecreek.Bot.Builder.Adapters;
using Iciclecreek.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ConsoleBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var appRoot = Path.GetDirectoryName(assembly.Location);
            var resourceRoot = Path.Combine(appRoot, "consolebot");
            var appName = assembly.GetName().Name;
            var userStoragePath = Path.Combine(Path.GetTempPath(), appName);
            Directory.CreateDirectory(userStoragePath);

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var services = new ServiceCollection();
            services.AddLogging()
                .AddSingleton<ConversationState>(sp => new ConversationState(new MemoryStorage()))
                .AddSingleton<UserState>(sp => new UserState(new FileStorage(userStoragePath)))
                .AddSingleton<Dialog, RootDialog>()
                .AddSingleton<ConsoleAdapter>()
                .AddBot();

            var sp = services.BuildServiceProvider();
            var bot = sp.GetService<IBot>();
            var consoleAdapter = sp.GetService<ConsoleAdapter>();
            await consoleAdapter.StartConversation(bot.OnTurnAsync, String.Join(" ", args));
        }
    }
}
