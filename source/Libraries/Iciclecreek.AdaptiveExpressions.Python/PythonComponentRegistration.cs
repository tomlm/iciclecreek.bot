using Microsoft.Bot.Builder.Runtime.Plugins;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

namespace Iciclecreek.AdaptiveExpressions
{
    /// <summary>
    /// Class which contains registration of components for CosmosDB.
    /// </summary>
    public class PythonComponentRegistration : IBotPlugin
    {
        public void Load(IBotPluginLoadContext context)
        {
            var resourceExplorer = context.Services.BuildServiceProvider().GetRequiredService<ResourceExplorer>();
            PythonFunctions.AddPythonFunctions(resourceExplorer);
        }
    }
}
