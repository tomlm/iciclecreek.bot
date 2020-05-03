using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Iciclecreek.Bot.Dialogs.Javascript
{
    public class JavascriptComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        static JavascriptComponentRegistration()
        {
        }

        public IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            yield break;
        }

        public IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
        {
            JavascriptFunctions.AddJavascriptFunctions(resourceExplorer);

            // yield return new DeclarativeType<CallJavascript>(CallJavascript.Kind);
            yield break;
        }
    }
}
