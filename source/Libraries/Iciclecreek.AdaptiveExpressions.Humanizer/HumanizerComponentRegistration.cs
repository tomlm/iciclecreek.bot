using Microsoft.Bot.Builder.Runtime.Plugins;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Iciclecreek.Bot.Expressions.Humanizer
{
    /// <summary>
    /// Class which contains registration of components for CosmosDB.
    /// </summary>
    public class HumanizerComponentRegistration : IBotPlugin
    {
        public void Load(IBotPluginLoadContext context)
        {
            HumanizerFunctions.Register();
        }
    }
}
