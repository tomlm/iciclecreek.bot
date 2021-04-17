using Iciclecreek.Bot.Builder;
using Iciclecreek.Bot.Builder.Adapters;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
            var userStoragePath = Path.Combine(Path.GetTempPath(), rootDialog);
            Directory.CreateDirectory(userStoragePath);

            //setup our DI
            IConfiguration configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "defaultRootDialog", rootDialog },
                    { "applicationRoot", folder}
                })
                .AddCommandLine(args)
                .Build();

            IServiceCollection services = new ServiceCollection();
            // use file storage for userstate, memory for conversationstate
            services.AddSingleton((sp) => new UserState(new FileStorage(userStoragePath)));
            services.AddBotRuntime(configuration);
            var sp = services.BuildServiceProvider();

            // run bot on console adapter
            await new ConsoleAdapter()
                .StartConversation((tc, ct) => sp.GetService<IBot>().OnTurnAsync(tc, ct), text);
        }
    }
}
