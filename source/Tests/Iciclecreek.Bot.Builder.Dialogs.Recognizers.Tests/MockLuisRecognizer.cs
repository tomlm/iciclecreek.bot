using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Tests
{
    class MockLuisRecognizer : Recognizer
    {
        public async override Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            var resourceExplorer = dialogContext.Context.TurnState.Get<ResourceExplorer>();
            var resource = resourceExplorer.GetResource("luisresults.json");
            var json = await resource.ReadTextAsync();
            return JsonConvert.DeserializeObject<RecognizerResult>(json);
        }
    }
}
