using Iciclecreek.Bot.Builder;
using Iciclecreek.Bot.Builder.Adapters;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Extensions;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var resourceRoot = Path.Combine(appRoot, "ConsoleBot");
            var appName = assembly.GetName().Name;
            var userStoragePath = Path.Combine(Path.GetTempPath(), appName);
            Directory.CreateDirectory(userStoragePath);

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .AddBotRuntimeConfiguration(resourceRoot, Path.Combine(resourceRoot, "settings"), string.Empty)
                .Build();

            var services = new ServiceCollection();
            services.AddLogging()
                .AddSingleton<IStorage, MemoryStorage>()
                .AddSingleton<ConversationState>()
                .AddSingleton<UserState>(sp => new UserState(new FileStorage(userStoragePath)))
                .AddSingleton<ResourceExplorer>(sp =>
                {
                    var resourcePath = Path.Combine(appRoot, "ConsoleBot");
                    var resourceExplorer = new ResourceExplorer()
                        .AddFolder(resourcePath);
                    resourceExplorer.AddResourceType(".yaml");
                    return resourceExplorer;
                })
                .AddSingleton<ConsoleAdapter>();

            services
                .AddBotRuntime(configuration);

            var sp = services.BuildServiceProvider();
            var bot = sp.GetService<IBot>();
            var consoleAdapter = sp.GetService<ConsoleAdapter>();
            await consoleAdapter.StartConversation(bot.OnTurnAsync, String.Join(" ", args));
        }
    }
}
