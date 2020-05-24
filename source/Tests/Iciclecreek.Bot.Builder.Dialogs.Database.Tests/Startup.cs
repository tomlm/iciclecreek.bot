// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Iciclecreek.Bot.Builder.Dialogs.Database.Cosmos;
using Iciclecreek.Bot.Builder.Dialogs.Database.SqlClient;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iciclecreek.Bot.Builder.Dialogs.Database.Tests
{
    [TestClass]
    internal class Startup
    {
        public Startup()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        [AssemblyInitialize]
        public static void Initialize(TestContext testContext)
        {
            ComponentRegistration.Add(new DeclarativeComponentRegistration());
            ComponentRegistration.Add(new AdaptiveComponentRegistration());
            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());
            ComponentRegistration.Add(new AdaptiveTestingComponentRegistration());
            ComponentRegistration.Add(new CosmosDBComponentRegistration());
            ComponentRegistration.Add(new SqlClientComponentRegistration());
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Adding IConfiguration in sample test server.  Otherwise this appears to be 
            // registered.
            services.AddSingleton<IConfiguration>(this.Configuration);
        }
    }
}
