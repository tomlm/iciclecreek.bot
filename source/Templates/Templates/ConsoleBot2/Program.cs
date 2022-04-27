// See https://aka.ms/new-console-template for more information
using System.Reflection;
using Iciclecreek.Bot.Builder;
using Iciclecreek.Bot.Builder.Adapters;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleBot2
{
    class Program
    {
        static async Task Main(string[] args)
        {

            var appName = Assembly.GetExecutingAssembly().GetName().Name ?? throw new ArgumentNullException();
            var userStoragePath = Path.Combine(Path.GetTempPath(), appName);
            Directory.CreateDirectory(userStoragePath);

            var configuration = new ConfigurationBuilder()
                .Build();

            var services = new ServiceCollection();
            services
                .AddSingleton<IStorage, MemoryStorage>()
                .AddSingleton<ConversationState>()
                .AddSingleton<UserState>(sp => new UserState(new FileStorage(userStoragePath)))
                .AddSingleton<ConsoleAdapter>()
                .AddSingleton<IBot, Bot>();

            //services
            //    .AddBotRuntime(configuration);

            var sp = services.BuildServiceProvider();
            var bot = sp.GetService<IBot>();
            var consoleAdapter = sp.GetService<ConsoleAdapter>();
            await consoleAdapter.StartConversation(bot.OnTurnAsync, String.Join(" ", args));
        }
    }
}
