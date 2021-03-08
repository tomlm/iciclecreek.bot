﻿using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Runtime.Plugins;
using Newtonsoft.Json;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucene
{
    /// <summary>
    /// Class which contains registration of components for Icicilecreek custom recognizers
    /// </summary>
    public class LuceneComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes, IBotPlugin
    {
        public void Load(IBotPluginLoadContext context)
        {
            ComponentRegistration.Add(new LuceneComponentRegistration());
        }

        /// <summary>
        /// Gets declarative type registrations.
        /// </summary>
        /// <param name="resourceExplorer">resourceExplorer to use for resolving references.</param>
        /// <returns>enumeration of DeclarativeTypes.</returns>
        public IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
        {
            yield return new DeclarativeType<DynamicListRecognizer>(DynamicListRecognizer.Kind);
        }

        /// <summary>
        /// Gets JsonConverters for DeclarativeTypes.
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