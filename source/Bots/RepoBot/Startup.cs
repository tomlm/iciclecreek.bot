// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Iciclecreek.Bot.Builder.Dialogs.Adaptive.GitHub;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;

[assembly: FunctionsStartup(typeof(RepoBot.Startup))]

namespace RepoBot
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var context = builder.GetContext();
            bool isAzure = Environment.GetEnvironmentVariable("HOME") != null;

            // Adaptive component registration
            ComponentRegistration.Add(new DialogsComponentRegistration());
            ComponentRegistration.Add(new DeclarativeComponentRegistration());
            ComponentRegistration.Add(new AdaptiveComponentRegistration());
            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());

            builder.Services.AddLogging();
            builder.Services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
            builder.Services.AddSingleton<AuthenticationConfiguration>();
            builder.Services.AddSingleton<IStorage>((s) =>
            {
                var botSettings = s.GetService<BotSettings>();
                if (botSettings?.StorageConnectionString != null)
                {
                    return new AzureBlobStorage(botSettings?.StorageConnectionString, "bot");
                }
                else
                {
                    return new MemoryStorage();
                }
            });
            builder.Services.AddSingleton<UserState>(s => new UserState(s.GetService<IStorage>()));
            builder.Services.AddSingleton<ConversationState>(s => new ConversationState(s.GetService<IStorage>()));
            builder.Services.AddSingleton((s) => new ResourceExplorer().AddFolder(context.ApplicationRootPath));
            builder.Services.AddSingleton<GitHubAdapter>();
            builder.Services.AddSingleton<IBotFrameworkHttpAdapter>(s =>
            {
                // Retrieve required dependencies
                IConfiguration configuration = s.GetService<IConfiguration>();
                IStorage storage = s.GetService<IStorage>();
                UserState userState = s.GetService<UserState>();
                ConversationState conversationState = s.GetService<ConversationState>();
                TelemetryInitializerMiddleware telemetryInitializerMiddleware = s.GetService<TelemetryInitializerMiddleware>();
                var botSettings = new BotSettings();
                configuration.GetSection(nameof(BotSettings)).Bind(botSettings);
                var adapter = new BotFrameworkHttpAdapter(new ConfigurationCredentialProvider(configuration))
                  .UseStorage(storage)
                  .UseBotState(userState, conversationState)
                  .Use(new RegisterClassMiddleware<IConfiguration>(configuration))
                  // .Use(s.GetService<TelemetryInitializerMiddleware>())
                  // .Use(new TranscriptLoggerMiddleware(new AzureBlobTranscriptStore(botSettings?.BlobStorageConnectionString, "transcripts")))
                  .Use(new ShowTypingMiddleware());

                adapter.OnTurnError = async (turnContext, exception) =>
                {
                    await turnContext.SendActivityAsync(exception.Message).ConfigureAwait(false);
                    await conversationState.ClearStateAsync(turnContext).ConfigureAwait(false);
                    await conversationState.SaveChangesAsync(turnContext).ConfigureAwait(false);
                };

                return (IBotFrameworkHttpAdapter)adapter;
            });
            builder.Services.AddSingleton<IBot>((s) =>
            {
                var botSettings = s.GetService<BotSettings>();
                var resourceExplorer = s.GetService<ResourceExplorer>();

                var bot = new Bot()
                    .UseResourceExplorer(resourceExplorer)
                    .UseLanguageGeneration();

                bot.RootDialog = resourceExplorer.LoadType<AdaptiveDialog>(resourceExplorer.GetResource("RepoBot.dialog"));
                return (IBot)bot;
            });
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();

            var builtConfig = builder.ConfigurationBuilder.Build();
            var keyVaultEndpoint = builtConfig["AzureKeyVaultEndpoint"];

            if (!string.IsNullOrEmpty(keyVaultEndpoint))
            {
                // using Key Vault, either local dev or deployed
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

                builder.ConfigurationBuilder
                    .AddJsonFile(Path.Combine(context.ApplicationRootPath, "local.settings.json"), true)
                    .AddAzureKeyVault(keyVaultEndpoint)
                    .AddEnvironmentVariables()
                    .Build();
            }
            else
            {
                // local dev no Key Vault
                builder.ConfigurationBuilder
                   .AddJsonFile(Path.Combine(context.ApplicationRootPath, "local.settings.json"), true)
                   .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                   .AddEnvironmentVariables()
                   .Build();
            }
        }

    }
}
