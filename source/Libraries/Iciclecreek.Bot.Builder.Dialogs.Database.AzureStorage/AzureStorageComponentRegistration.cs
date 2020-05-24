using Iciclecreek.Bot.Builder.Dialogs.Database.AzureStorage.Table;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iciclecreek.Bot.Builder.Dialogs.Database.AzureStorage
{
    /// <summary>
    /// Class which contains registration of components for CosmosDB.
    /// </summary>
    public class AzureStorageComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        /// <summary>
        /// Gets declarative type registrations for QnAMAker.
        /// </summary>
        /// <param name="resourceExplorer">resourceExplorer to use for resolving references.</param>
        /// <returns>enumeration of DeclarativeTypes.</returns>
        public IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
        {
            // table
            yield return new DeclarativeType<CreateTable>(CreateTable.Kind);
            yield return new DeclarativeType<DeleteTable>(DeleteTable.Kind);
            yield return new DeclarativeType<RetrieveEntity>(RetrieveEntity.Kind);
            yield return new DeclarativeType<EntityOperation>(EntityOperation.Kind);

            // blob

        }

        /// <summary>
        /// Gets JsonConverters for DeclarativeTypes for QnAMaker.
        /// </summary>
        /// <param name="resourceExplorer">resourceExplorer to use for resolving references.</param>
        /// <param name="sourceContext">SourceContext to build debugger source map.</param>
        /// <returns>enumeration of json converters.</returns>
        public IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            yield break;
        }
    }
}