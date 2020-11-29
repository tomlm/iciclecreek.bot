using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Phonetic;
using Lucene.Net.Analysis.Phonetic.Language.Bm;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;
using Lucy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy
{
    public class LucyRecognizer : Recognizer
    {
        private LucyEngine _engine = null;

        public LucyRecognizer([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(callerPath, callerLine)
        {
        }

        /// <summary>
        /// Gets or sets the ResourceID for the Lucy model.
        /// </summary>
        [JsonProperty("resourceId")]
        public StringExpression ResourceId { get; set; }

        [JsonProperty("externalEntityRecognizer")]
        public Recognizer ExternalEntityRecognizer { get; set; }

        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            if (this._engine == null)
            {
                var modelId = ResourceId.GetValue(dialogContext.State);
                var resourceExplorer = dialogContext.Context.TurnState.Get<ResourceExplorer>();
                var modelResource = resourceExplorer.GetResource(modelId);
                var model = JsonConvert.DeserializeObject<LucyModel>(await modelResource.ReadTextAsync());
                this._engine = new LucyEngine(model);
            }

            List<LucyEntity> externalEntities = new List<LucyEntity>();
            if (ExternalEntityRecognizer != null)
            {
                var results = await ExternalEntityRecognizer.RecognizeAsync(dialogContext, activity, cancellationToken, telemetryProperties, telemetryMetrics).ConfigureAwait(false);

                dynamic instanceData = results.Entities["$instanceData"];
                foreach (var prop in results.Entities.Properties().Where(p => p.Name != "$instance"))
                {
                    // get resolution
                    JArray resolutions = (JArray)prop.Value;
                    for (int i = 0; i < resolutions.Count; i++)
                    {
                        dynamic resolution = resolutions[i];
                        dynamic metadata = instanceData[i];

                        externalEntities.Add(new LucyEntity()
                        {
                            Type = prop.Name,
                            Start = metadata.StartIndex,
                            End = metadata.EndIndex,
                            Resolution = resolution,
                            Score = metadata.Score,
                            Text = activity.Text.Substring((int)metadata.StartIndex, metadata.EndIndex - metadata.StartIndex)
                        });
                    }
                }
            }

            var recognizerResult = new RecognizerResult();
            var lucyEntities = _engine.MatchEntities(activity.Text, activity.Locale, externalEntities);
            foreach (var lucyEntity in lucyEntities)
            {

            }

            return recognizerResult;
        }
    }
}
