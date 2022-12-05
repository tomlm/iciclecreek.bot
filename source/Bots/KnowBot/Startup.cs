using KnowBot.Dialogs;
using Iciclecreek.Bot.Builder.Dialogs;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Documents.Extensions;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Store.Azure;
using Lucene.Net.Util;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.IO;
using System.Threading;

[assembly: FunctionsStartup(typeof(KnowBot.Startup))]

namespace KnowBot
{

    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddMemoryCache()
                .AddSingleton<IStorage>(sp => new BlobsStorage(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), nameof(BotHelp).ToLower()))
                .AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>()
                .AddSingleton<IMiddleware, NormalizeMentionsMiddleware>()
                .AddSingleton<IBotFrameworkHttpAdapter, FunctionAdapter>()
                .AddDialog<KnowBotDialog>()
                .AddSingleton<CloudStorageAccount>((sp) => CloudStorageAccount.Parse(builder.GetContext().Configuration.GetValue<string>("AzureWebJobsStorage")))
                .AddSingleton<CloudQueueClient>((sp) => sp.GetService<CloudStorageAccount>().CreateCloudQueueClient())
                .AddPrompts()
                .AddIcyBot();

        }
    }
}
