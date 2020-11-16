//using System;
//using System.Collections.Generic;
//using System.Text;
//using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

//namespace Iciclecreek.Bot.Builder.Dialogs.Database.AzureStorage
//{
//    /// <summary>
//    /// Class which gives ResourceExplorer access to resources which are stored in file system.
//    /// </summary>
//    public class BlobResourceProvider : ResourceProvider
//    {
//        private Dictionary<string, BlobResource> resources = new Dictionary<string, BlobResource>();

//        /// <summary>
//        /// Initializes a new instance of the <see cref="FolderResourceProvider"/> class.
//        /// </summary>
//        /// <param name="resourceExplorer">resourceExplorer.</param>
//        /// <param name="folder">Folder.</param>
//        /// <param name="includeSubFolders">Should include sub folders.</param>
//        /// <param name="monitorChanges">Should monitor changes.</param>
//        public BlobResourceProvider(ResourceExplorer resourceExplorer, string connectionString, string container)
//            : base(resourceExplorer)
//        {
//        }

//        /// <summary>
//        /// Refresh any cached content and look for new content.
//        /// </summary>
//        public override void Refresh()
//        {
//            this.resources.Clear();
//        }

//        /// <summary>
//        /// GetResource by id.
//        /// </summary>
//        /// <param name="id">Resource ID.</param>
//        /// <param name="resource">the found resource.</param>
//        /// <returns>true if resource was found.</returns>
//        public override bool TryGetResource(string id, out Resource resource)
//        {
//            lock (this.resources)
//            {
//                if (this.resources.TryGetValue(id, out BlobResource blobResource))
//                {
//                    resource = blobResource;
//                    return true;
//                }

//                resource = null;
//                return false;
//            }
//        }

//        /// <summary>
//        /// Get Resources by extension.
//        /// </summary>
//        /// <param name="extension">Resource extension.</param>
//        /// <returns>Collection of resources.</returns>
//        public override IEnumerable<Resource> GetResources(string extension)
//        {
//            extension = $".{extension.TrimStart('.').ToLowerInvariant()}";

//            lock (this.resources)
//            {
//                return this.resources.Values; //.Where(pair => pair.Key.ToLowerInvariant().EndsWith(extension, StringComparison.Ordinal)).Select(pair => pair.Value).ToList();
//            }
//        }

//        /// <summary>
//        /// Returns a string that represents the current object.
//        /// </summary>
//        /// <returns>A string that represents the current object.</returns>
//        public override string ToString()
//        {
//            return this.Id;
//        }
//    }
//}
