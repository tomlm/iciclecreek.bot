using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Store.Azure;
using Lucene.Net.Util;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.IO;
using System.Threading;

[assembly: FunctionsStartup(typeof(BeBot.Indexer.Startup))]

namespace BeBot.Indexer
{

    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddMemoryCache()
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
                .AddSingleton<IndexWriter>((sp) =>
                {
                    while (true)
                    {
                        try
                        {
                            return new IndexWriter(sp.GetService<AzureDirectory>(), new IndexWriterConfig(LuceneVersion.LUCENE_48, new StandardAnalyzer(LuceneVersion.LUCENE_48)));
                        }
                        catch (LockObtainFailedException)
                        {
                            Console.WriteLine("Lock is taken, waiting for timeout...");
                            Thread.Sleep(1000);
                        }
                    };
                });

        }
    }
}
