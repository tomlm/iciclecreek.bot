using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Debugging;

namespace Iciclecreek.AdaptiveExpressions
{
    /// <summary>
    /// Class which contains registration of components for CosmosDB.
    /// </summary>
    public class PythonComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        // BotComponent
        //public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        //{
        //    var resourceExplorer = services.BuildServiceProvider().GetRequiredService<ResourceExplorer>();
        //    PythonFunctions.AddPythonFunctions(resourceExplorer);
        //}

        public IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            yield break;
        }

        public IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
        {
            PythonFunctions.AddPythonFunctions(resourceExplorer);
            yield break;
        }
    }
}
