using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Lucyne.Net.Analysis;
using Lucyne.Net.Analysis.Phonetic;
using Lucyne.Net.Analysis.Phonetic.Language.Bm;
using Lucyne.Net.Analysis.Standard;
using Lucyne.Net.Analysis.Util;
using Lucyne.Net.Util;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy
{
    public class LucyRecognizer : Recognizer
    {
        public LucyRecognizer([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(callerPath, callerLine)
        {
        }

        [JsonProperty("model")]
        public ObjectExpression<LucyModel> Model { get; set; }

        [JsonProperty("externalEntityRecognizer")]
        public Recognizer ExternalEntityRecognizer { get; set; }

        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            RecognizerResult results = null;
            if (ExternalEntityRecognizer != null)
            {
                results = await ExternalEntityRecognizer.RecognizeAsync(dialogContext, activity, cancellationToken, telemetryProperties, telemetryMetrics).ConfigureAwait(false);
            }

            return results;
        }

        protected async virtual Task<RecognizerResult> _RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default)
        {
            await Task.Delay(0);
            return null;
        }
    }
}
