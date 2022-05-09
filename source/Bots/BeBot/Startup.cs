using BeBot.Dialogs;
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

[assembly: FunctionsStartup(typeof(BeBot.Startup))]

namespace BeBot
{

    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddMemoryCache()
                .AddSingleton<IStorage>(sp => new BlobsStorage(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), nameof(BeBot).ToLower()))
                .AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>()
                .AddSingleton<IMiddleware, NormalizeMentionsMiddleware>()
                .AddSingleton<IBotFrameworkHttpAdapter, FunctionAdapter>()
                .AddDialog<BeBotDialog>()
                .AddDialog<SetPlanDialog>()
                .AddSingleton<CloudStorageAccount>((sp) => CloudStorageAccount.Parse(builder.GetContext().Configuration.GetValue<string>("AzureWebJobsStorage")))
                .AddSingleton<CloudQueueClient>((sp) => sp.GetService<CloudStorageAccount>().CreateCloudQueueClient())
                .AddSingleton<AzureDirectory>((sp) =>
                {
                    string catalog = nameof(BeBot).ToLower();
                    var tempFolder = Path.Combine(Path.GetTempPath(), catalog);
                    if (!System.IO.Directory.Exists(tempFolder))
                        System.IO.Directory.CreateDirectory(tempFolder);
                    var cacheDirectory = SimpleFSDirectory.Open(new System.IO.DirectoryInfo(tempFolder));
                    return new AzureDirectory(builder.GetContext().Configuration.GetValue<string>("AzureWebJobsStorage"), catalog, cacheDirectory);
                })
                .AddTransient<IndexSearcher>((sp) =>
                {
                    var cache = sp.GetService<IMemoryCache>();
                    return cache.GetOrCreate<IndexSearcher>("searcher", (ce) =>
                    {
                        ce.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                        var azureDirectory = sp.GetService<AzureDirectory>();
                        try
                        {
                            IndexReader indexReader = DirectoryReader.Open(azureDirectory);
                            return new IndexSearcher(indexReader);
                        }
                        catch (IndexNotFoundException)
                        {
                            using (var writer = new IndexWriter(sp.GetService<AzureDirectory>(), new IndexWriterConfig(LuceneVersion.LUCENE_48, new StandardAnalyzer(LuceneVersion.LUCENE_48))))
                            {
                                var document = new Document();
                                document.AddStringField("id", "ignorethis", Field.Store.NO);
                                writer.AddDocument(document);
                            }
                            IndexReader indexReader = DirectoryReader.Open(azureDirectory);
                            return new IndexSearcher(indexReader);
                        }
                    });
                })
                .AddPrompts()
                .AddIcyBot();

        }
    }
}
