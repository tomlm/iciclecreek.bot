using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;

namespace Iciclecreek.Bot.Builder
{
    /// <summary>
    /// Models IStorage around a File System
    /// </summary>
    /// <remarks>
    /// This is great to use for console apps but is not great under high concurrency as the file system will end up with contention.
    /// </remarks>
    public class FileStorage : IStorage
    {
        private static JsonSerializerSettings serializationSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        protected string folder;
        protected int ETag = 0;

        public FileStorage(string folder)
        {
            this.folder = folder;
        }

        public Task DeleteAsync(string[] keys, CancellationToken cancellationToken = default) 
        {
            foreach (var key in keys)
            {
                File.Delete(Path.Combine(folder, key));
            }
            return Task.CompletedTask;
        }

        public async Task<IDictionary<string, object>> ReadAsync(string[] keys, CancellationToken cancellationToken = default)
        {
            var storeItems = new Dictionary<string, object>(keys.Length);

            foreach (var key in keys)
            {
                var item = await ReadIStoreItem(key).ConfigureAwait(false);
                if (item != null)
                {
                    storeItems.Add(key, item);
                }
            }

            return storeItems;
        }

        public async Task WriteAsync(IDictionary<string, object> changes, CancellationToken cancellationToken = default) 
        {
            // Similar to the Read method, the funky threading in here is due to 
            // concurrency and async methods. 
            // 
            // When this method is called, it may happen (in parallel) from any number of
            // thread. 
            //
            // If an operation is in progress, the Open will fail with an  
            // IOException. If this happens,the best thing to do is simply wait a moment
            // and retry. The Retry MUST go through the ETag processing again. 
            //
            // Alternate approach in here would be to use a SemaphoreSlim and use the async/await
            // constructs.

            foreach (var change in changes)
            {
                DateTime start = DateTime.UtcNow;
                while (true)
                {
                    try
                    {
                        object newValue = change.Value;
                        object oldValue = await this.ReadIStoreItem(change.Key).ConfigureAwait(false);
                        IStoreItem newStoreItem = newValue as IStoreItem;
                        IStoreItem oldStoreItem = oldValue as IStoreItem;
                        if (oldValue == null ||
                            newStoreItem?.ETag == "*" ||
                            oldStoreItem?.ETag == newStoreItem?.ETag)
                        {
                            string key = SanitizeKey(change.Key);
                            string path = Path.Combine(this.folder, key);
                            var oldTag = newStoreItem?.ETag;
                            if (newStoreItem != null)
                                newStoreItem.ETag = Guid.NewGuid().ToString("n");
                            var json = JsonConvert.SerializeObject(newValue, serializationSettings);
                            if (newStoreItem != null)
                                newStoreItem.ETag = oldTag;
                            using (TextWriter file = new StreamWriter(path))
                            {
                                await file.WriteAsync(json).ConfigureAwait(false);
                                break;
                            }
                        }
                        else
                        {
                            throw new Exception($"etag conflict key={change}");
                        }
                    }
                    catch (IOException)
                    {
                        if ((DateTime.UtcNow - start).TotalSeconds < 5)
                            await Task.Delay(0).ConfigureAwait(false);
                        else
                            throw;
                    }
                }
            }

        }

        private static Lazy<Dictionary<char, string>> badChars = new Lazy<Dictionary<char, string>>(() =>
        {
            char[] badChars = Path.GetInvalidFileNameChars();
            var dict = new Dictionary<char, string>();
            foreach (var badChar in badChars)
                dict[badChar] = '%' + ((int)badChar).ToString("x2");
            return dict;
        });

        private string SanitizeKey(string key)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char ch in key)
            {
                if (badChars.Value.TryGetValue(ch, out string val))
                    sb.Append(val);
                else
                    sb.Append(ch);
            }
            return sb.ToString();
        }

        private async Task<object> ReadIStoreItem(string key)
        {
            // The funky threading in here is due to concurrency and async methods. 
            // When this method is called, it may happen (in parallel) from any number of
            // thread. 
            //
            // If a write operation is in progress, the "OpenRead" will fail with an 
            // IOException. If this happens,the best thing to do is simply wait a moment
            // and retry. From the Docs:
            //      This method is equivalent to the FileStream(String, FileMode, 
            //      FileAccess, FileShare) constructor overload with a FileMode value 
            //      of Open, a FileAccess value of Read and a FileShare value of Read.

            key = SanitizeKey(key);
            string path = Path.Combine(this.folder, key);
            string json;
            DateTime start = DateTime.UtcNow;
            while (true)
            {
                try
                {
                    using (TextReader file = new StreamReader(File.OpenRead(path)))
                    {
                        json = await file.ReadToEndAsync().ConfigureAwait(false);
                    }

                    return JsonConvert.DeserializeObject(json, serializationSettings);
                }
                catch (FileNotFoundException)
                {
                    return null;
                }
                catch (IOException)
                {
                    if ((DateTime.UtcNow - start).TotalSeconds < 5)
                        await Task.Delay(0).ConfigureAwait(false);
                    else
                        throw;
                }
            }
        }
    }
}
