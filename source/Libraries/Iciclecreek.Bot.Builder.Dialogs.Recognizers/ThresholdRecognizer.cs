using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers
{
    /// <summary>
    /// Fire a ChooseIntent event when the configured recgonizer returns more than 1 intent with score differences less then the threshold.
    /// </summary>
    public class ThresholdRecognizer : Recognizer
    {
        [JsonProperty("$kind")]
        public const string Kind = "Iciclecreek.ThresholdRecognizer";

        [JsonConstructor]
        public ThresholdRecognizer([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
        }

        /// <summary>
        /// Gets or sets the threshold
        /// </summary>
        /// <value>Sets the threshold for intents to be considered ambigious because the scores are this close or smaller.</value>
        public float Threshold { get; set; } = 0.1f;

        /// <summary>
        /// Recognizer to apply threshold to.
        /// </summary>
        public Recognizer Recognizer { get; set; }

        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            if (dialogContext == null)
            {
                throw new ArgumentNullException(nameof(dialogContext));
            }

            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            if (Recognizer == null)
            {
                throw new ArgumentNullException(nameof(Recognizer));
            }

            // run all of the recognizers in parallel
            var result = await Recognizer.RecognizeAsync(dialogContext, activity, cancellationToken, telemetryProperties, telemetryMetrics).ConfigureAwait(false);
            string text = null;
            result.Properties["id"] = Recognizer.Id;

            List<JObject> candidates = new List<JObject>();
            var (topIntent, topScore) = result.GetTopScoringIntent();
            List<RecognizerResult> results = new List<RecognizerResult>();

            foreach (var intent in result.Intents)
            {
                if ((topScore - intent.Value.Score) <= Threshold)
                {
                    text = result.Text;
                    dynamic candidate = new JObject();
                    candidate.id = intent.Key;
                    candidate.intent = intent.Key;
                    candidate.score = intent.Value.Score;
                    candidate.result = JObject.FromObject(new RecognizerResult()
                    {
                        Text = result.Text,
                        AlteredText = result.AlteredText,
                        Intents = new Dictionary<string, IntentScore>() { { intent.Key, intent.Value } },
                        Properties = result.Properties
                    });
                    candidates.Add(candidate);
                }
            }

            if (candidates.Count > 1)
            {
                // return ChooseIntent with Candidtes array
                return new RecognizerResult()
                {
                    Text = text,
                    Intents = new Dictionary<string, IntentScore>() { { "ChooseIntent", new IntentScore() { Score = 1.0 } } },
                    Properties = new Dictionary<string, object>() { { "candidates", candidates } },
                };
            }

            // just return the recognizer result, it's fine, there is no ambiguity
            return result;
        }
    }
}
