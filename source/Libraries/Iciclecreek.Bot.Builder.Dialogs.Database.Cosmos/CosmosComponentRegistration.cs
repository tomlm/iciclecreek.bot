using AdaptiveExpressions.Converters;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iciclecreek.Bot.Builder.Dialogs.Database.Cosmos
{
    /// <summary>
    /// Class which contains registration of components for CosmosDB.
    /// </summary>
    public class CosmosComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        /// <summary>
        /// Gets declarative type registrations for QnAMAker.
        /// </summary>
        /// <param name="resourceExplorer">resourceExplorer to use for resolving references.</param>
        /// <returns>enumeration of DeclarativeTypes.</returns>
        public IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
        {
            // cosmos
            yield return new DeclarativeType<CreateDatabase>(CreateDatabase.Kind);
            yield return new DeclarativeType<DeleteDatabase>(DeleteDatabase.Kind);

            yield return new DeclarativeType<CreateContainer>(CreateContainer.Kind);
            yield return new DeclarativeType<DeleteContainer>(DeleteContainer.Kind);

            yield return new DeclarativeType<CreateItem>(CreateItem.Kind);
            yield return new DeclarativeType<UpsertItem>(UpsertItem.Kind);
            yield return new DeclarativeType<QueryItems>(QueryItems.Kind);
            yield return new DeclarativeType<GetItem>(GetItem.Kind);
            yield return new DeclarativeType<DeleteItem>(DeleteItem.Kind);
            yield return new DeclarativeType<ReplaceItem>(ReplaceItem.Kind);

            // graph

            // mongo

        }

        /// <summary>
        /// Gets JsonConverters for DeclarativeTypes for QnAMaker.
        /// </summary>
        /// <param name="resourceExplorer">resourceExplorer to use for resolving references.</param>
        /// <param name="sourceContext">SourceContext to build debugger source map.</param>
        /// <returns>enumeration of json converters.</returns>
        public IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            yield return new ObjectExpressionConverter<object>();
        }
    }
}