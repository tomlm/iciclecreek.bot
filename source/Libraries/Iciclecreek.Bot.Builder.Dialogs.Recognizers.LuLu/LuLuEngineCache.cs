using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Lucene.Net.Store;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.LuLu
{
    public static class QLuceneEngineCache
    {
        private static ConcurrentDictionary<string, QLuceneEngine> engines = new ConcurrentDictionary<string, QLuceneEngine>();

        /// <summary>
        /// GetEngine from cache (must already have been built by someone)
        /// </summary>
        /// <param name="knowledgebase">cache key</param>
        /// <returns>null if no engine has been created.</returns>
        public static QLuceneEngine GetEngine(string knowledgebase)
        {
            return engines[knowledgebase];
        }

        /// <summary>
        /// Get a prebuilt engine catalog
        /// </summary>
        /// <param name="knowledgebase">cache key</param>
        /// <param name="getDirectory">directory with prebuilt catalog.</param>
        /// <returns></returns>
        public static QLuceneEngine GetEngine(string knowledgebase, Func<Directory> getDirectory)
        {
            if (engines.TryGetValue(knowledgebase, out var engine))
            {
                return engine;
            }

            lock (engines)
            {
                if (engines.TryGetValue(knowledgebase, out engine))
                {
                    return engine;
                }

                engines[knowledgebase] = new QLuceneEngine(getDirectory());
                return engines[knowledgebase];
            }
        }

        /// <summary>
        /// Get or create the catalog for qnajson
        /// </summary>
        /// <param name="knowledgebase">key to cache</param>
        /// <param name="qnaJson">qnaJson to use</param>
        /// <param name="getDirectory">directory to use if index needs to be created. (Default is RAMDirectory)</param>
        /// <returns></returns>
        public static QLuceneEngine GetEngine(string knowledgebase, string qnaJson)
        {
            if (engines.TryGetValue(knowledgebase, out var engine))
            {
                return engine;
            }

            lock (engines)
            {
                if (engines.TryGetValue(knowledgebase, out engine))
                {
                    return engine;
                }

                Directory directory = new RAMDirectory();
                QLuceneEngine.CreateCatalog(qnaJson, directory);
                engines[knowledgebase] = new QLuceneEngine(directory);
                return engines[knowledgebase];
            }
        }

        /// <summary>
        /// Get engine from resource.Id
        /// </summary>
        /// <param name="dialogContext"></param>
        /// <param name="kbResourceId">resource.qna</param>
        /// <returns></returns>
        public static async Task<QLuceneEngine> GetEngine(DialogContext dialogContext, string kbResourceId)
        {
            if (!engines.TryGetValue(kbResourceId, out var engine))
            {
                var resourceExplorer = dialogContext.Context.TurnState.Get<ResourceExplorer>();

                var resource = resourceExplorer.GetResource(kbResourceId);

                // resource.catalog
                var dirInfo = new System.IO.DirectoryInfo(System.IO.Path.Combine(resource.FullName) + ".catalog");
                if (dirInfo.Exists)
                {
                    // open existing catalog
                    engines[kbResourceId] = new QLuceneEngine(FSDirectory.Open(dirInfo));
                    return engines[kbResourceId];
                }
                else
                {
                    // create ram catalog on the fly
                    string json;
                    Resource resourceJson;
                    resourceExplorer.TryGetResource(kbResourceId + ".json", out resourceJson);
                    if (resourceJson != null)
                        json = await resourceJson.ReadTextAsync().ConfigureAwait(false);
                    else
                        json = await resource.ReadTextAsync().ConfigureAwait(false);

                    var directory = new RAMDirectory();
                    QLuceneEngine.CreateCatalog(json, directory);
                    engines[kbResourceId] = new QLuceneEngine(directory);
                    return engines[kbResourceId];
                }

            }

            return engine;
        }
    }
}
