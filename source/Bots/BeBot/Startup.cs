using BeBot.Dialogs;
using Iciclecreek.Bot.Builder.Dialogs;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

[assembly: FunctionsStartup(typeof(BeBot.Startup))]

namespace BeBot
{

    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddSingleton<IStorage>(sp => new BlobsStorage(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), nameof(BeBot).ToLower()))
                .AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>()
                .AddSingleton<IBotFrameworkHttpAdapter, FunctionAdapter>()
                .AddSingleton<Dialog, RootDialog>()
                .AddBot();
        }
    }
}
