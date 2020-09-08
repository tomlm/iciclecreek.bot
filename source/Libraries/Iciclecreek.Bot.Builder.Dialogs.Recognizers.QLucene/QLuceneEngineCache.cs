using System.Collections.Concurrent;
using System.Threading.Tasks;
using Lucene.Net.Store;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.QLucene
{
    public static class QLuceneEngineCache
    {
        private static ConcurrentDictionary<string, QLuceneEngine> engines = new ConcurrentDictionary<string, QLuceneEngine>();

        public static QLuceneEngine GetEngine(string key)
        {
            return engines[key];
        }

        public static QLuceneEngine GetEngine(string key, string qnaJson)
        {
            if (engines.TryGetValue(key, out var engine))
            {
                return engine;
            }

            lock (engines)
            {
                if (engines.TryGetValue(key, out engine))
                {
                    return engine;
                }

                Directory directory = new RAMDirectory();
                QLuceneEngine.CreateCatalog(qnaJson, directory);
                engines[key] = new QLuceneEngine(directory);
                return engines[key];
            }
        }

        public static async Task<QLuceneEngine> GetEngine(DialogContext dialogContext, string resourceId)
        {
            if (!engines.TryGetValue(resourceId, out var engine))
            {
                var resourceExplorer = dialogContext.Context.TurnState.Get<ResourceExplorer>();
                Resource resource;
                if (!resourceExplorer.TryGetResource(resourceId + ".json", out resource))
                {
                    resource = resourceExplorer.GetResource(resourceId);
                }

                var json = await resource.ReadTextAsync().ConfigureAwait(false);

                return GetEngine(resourceId, json);
            }

            return engine;
        }
    }
}
