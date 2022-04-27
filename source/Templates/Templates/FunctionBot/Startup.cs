using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Extensions;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Empty.Startup))]

namespace Empty
{

    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IBot, FunctionBot>();
            builder.Services.AddBotRuntime(builder.GetContext().Configuration);
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder configurationBuilder)
        {
            FunctionsHostBuilderContext context = configurationBuilder.GetContext();

            string applicationRoot = context.ApplicationRootPath;
            string environmentName = context.EnvironmentName;
            string settingsDirectory = "settings";

            configurationBuilder.ConfigurationBuilder.AddBotRuntimeConfiguration(
                applicationRoot,
                settingsDirectory,
                environmentName);
        }
    }
}