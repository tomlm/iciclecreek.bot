using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Runtime.Plugins;

namespace Iciclecreek.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Class which contains registration of components for CosmosDB.
    /// </summary>
    public class ConsoleComponentRegistration : ComponentRegistration, IBotPlugin
    {
        public void Load(IBotPluginLoadContext context)
        {
            ComponentRegistration.Add(new ConsoleComponentRegistration());
        }
    }
}
