using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iciclecreek.Bot.Dialogs.Javascript
{
    public class JavascriptComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        public IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            yield break;
        }

        public IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
        {
            resourceExplorer.AddResourceType("js");
            yield return new DeclarativeType<CallJavascript>(CallJavascript.Kind);
        }
    }
}
