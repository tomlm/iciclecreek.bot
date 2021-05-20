// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Iciclecreek.Bot.Builder.Dialogs.Recognizers;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucene;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Obsolete;
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
#pragma warning disable CS0618 // Type or member is obsolete
            ComponentRegistration.Add(new DialogsComponentRegistration());
            ComponentRegistration.Add(new DeclarativeComponentRegistration());
            ComponentRegistration.Add(new AdaptiveComponentRegistration());
            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());
            ComponentRegistration.Add(new AdaptiveTestingComponentRegistration());
            ComponentRegistration.Add(new DeclarativeComponentRegistrationBridge<QLuceneBotComponent>());
            ComponentRegistration.Add(new DeclarativeComponentRegistrationBridge<LucyBotComponent>());
            ComponentRegistration.Add(new DeclarativeComponentRegistrationBridge<LuceneDynamicListBotComponent>());
            ComponentRegistration.Add(new DeclarativeComponentRegistrationBridge<LuisBotComponent>());
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Adding IConfiguration in sample test server.  Otherwise this appears to be 
            // registered.
            services.AddSingleton<IConfiguration>(this.Configuration);
/*            LuceneDynamicListBotComponent lucene = new LuceneDynamicListBotComponent();
            lucene.ConfigureServices(services, this.Configuration);*/
        }
    }
}
