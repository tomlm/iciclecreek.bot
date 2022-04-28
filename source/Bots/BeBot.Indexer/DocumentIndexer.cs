using Lucene.Net.Documents;
using Lucene.Net.Documents.Extensions;
using Lucene.Net.Index;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace BeBot.Indexer
{
    public class DocumentIndexer
    {
        private IndexWriter _writer;

        public DocumentIndexer(IndexWriter indexWriter)
        {
            _writer = indexWriter;
        }

        [FunctionName("Index")]
        public void Run([QueueTrigger("documents", Connection = "AzureWebJobsStorage")] string json, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {json}");
            dynamic doc = JObject.Parse(json);
            Document document = new Document();
            document.AddTextField("aaId", (string)doc.aaId, Field.Store.YES);
            document.AddTextField("name", (string)doc.name, Field.Store.YES);
            document.AddTextField("location", (string)doc.location, Field.Store.YES);
            document.AddTextField("date", (string)doc.date, Field.Store.YES);
        }
    }
}
