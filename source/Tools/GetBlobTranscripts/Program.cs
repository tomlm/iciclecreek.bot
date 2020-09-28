using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Azure;
using Newtonsoft.Json;

namespace Iciclecreek.Bot.Tools
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("getBlobTranscripts container channel conversationId");
                Console.WriteLine();
                Console.WriteLine("set TRANSCRIPT_CONNECTIONSTRING to blob storage connection string");
                return;
            }

            var container = args[0];
            var channelId = args[1];
            var conversationId = args[2];
            var connectionString = Environment.GetEnvironmentVariable("TRANSCRIPT_CONNECTIONSTRING");
            var transcriptStore = new AzureBlobTranscriptStore(connectionString, container);
            string continuationToken = null;

            Console.WriteLine("[");
            do
            {
                var result = await transcriptStore.GetTranscriptActivitiesAsync(channelId, conversationId, continuationToken);
                foreach (var item in result.Items)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(item, Formatting.Indented));
                }

                continuationToken = result.ContinuationToken;
            } while (continuationToken != null);

            Console.WriteLine("]");
        }
    }
}
