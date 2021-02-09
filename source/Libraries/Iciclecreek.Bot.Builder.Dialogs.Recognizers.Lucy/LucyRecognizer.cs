using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Lucy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy
{
    /// <summary>
    /// Bot Framework ecgonizer which uses a Lucy model to recognize entities.
    /// </summary>
    public class LucyRecognizer : Recognizer
    {
        private LucyEngine _engine = null;

        public const string MatchedIntent = "Matched";

        [JsonProperty("$kind")]
        public const string Kind = "Iciclecreek.LucyRecognizer";

        public LucyRecognizer([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(callerPath, callerLine)
        {
        }

        /// <summary>
        /// Lucy model
        /// </summary>
        [JsonProperty("model")]
        public LucyDocument Model { get; set; }

        /// <summary>
        /// Gets or sets the ResourceID for the Lucy model (if not already defined in the Model property).
        /// </summary>
        [JsonProperty("resourceId")]
        public StringExpression ResourceId { get; set; }

        [JsonProperty("externalEntityRecognizer")]
        public Recognizer ExternalEntityRecognizer { get; set; }

        /// <summary>
        /// Gets or sets Intents to emit as intents when matched. (Default intent is "Matched")
        /// </summary>
        [JsonProperty("intents")]
        public ArrayExpression<string> Intents { get; set; } = new ArrayExpression<string>();

        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            if (this._engine == null)
            {
                if (this.Model == null)
                {
                    var resourceExplorer = dialogContext.Context.TurnState.Get<ResourceExplorer>() ?? throw new ArgumentException("No resource Explorer was found in dialog context");
                    var modelId = ResourceId.GetValue(dialogContext.State);
                    var modelResource = resourceExplorer.GetResource(modelId) ?? throw new ArgumentException($"{modelId} not found");
                    var yamlOrJson = await modelResource.ReadTextAsync();
                    this._engine = new LucyEngine(yamlOrJson);
                }
                else
                {
                    this._engine = new LucyEngine(this.Model);
                }
            }

            List<LucyEntity> externalEntities = new List<LucyEntity>();
            if (ExternalEntityRecognizer != null)
            {
                var results = await ExternalEntityRecognizer.RecognizeAsync(dialogContext, activity, cancellationToken, telemetryProperties, telemetryMetrics).ConfigureAwait(false);

                externalEntities = GetEntitiesFromObject(activity, results.Entities).ToList();
            }

            var recognizerResult = new RecognizerResult();
            var lucyEntities = _engine.MatchEntities(activity.Text ?? string.Empty, externalEntities);
            recognizerResult.Entities = GetRecognizerEntities(lucyEntities);

            var intents = this.Intents.GetValue(dialogContext.State) ?? new List<string>();
            if (intents.Any())
            {
                foreach (var lucyEntity in lucyEntities.Where(entity => intents.Contains(entity.Type)))
                {
                    recognizerResult.Intents.Add(lucyEntity.Type, new IntentScore() { Score = lucyEntity.Score });
                }
            }
            else if (recognizerResult.Entities.Count > externalEntities.Count + 1)
            {
                recognizerResult.Intents.Add(MatchedIntent, new IntentScore() { Score = lucyEntities.Max(e => e.Score) });
            }

            if (!recognizerResult.Intents.Any())
            {
                recognizerResult.Intents.Add(NoneIntent, new IntentScore { Score = 1.0f });
            }

            await dialogContext.Context.TraceActivityAsync(nameof(LucyRecognizer), JObject.FromObject(recognizerResult), "RecognizerResult", "Lucy RecognizerResult", cancellationToken).ConfigureAwait(false);
            this.TrackRecognizerResult(dialogContext, "LucyRecognizerResult", this.FillRecognizerResultTelemetryProperties(recognizerResult, telemetryProperties), telemetryMetrics);

            return recognizerResult;
        }

        private static JObject GetRecognizerEntities(IEnumerable<LucyEntity> lucyEntities)
        {
            dynamic entitiesObject = new JObject();
            dynamic instancesObject = new JObject();

            foreach (var grp in lucyEntities.GroupBy(le => le.Type))
            {
                entitiesObject[grp.Key] = JArray.FromObject(grp.Select(lucyEntity =>
                {
                    if (lucyEntity.Children.Any())
                    {
                        return GetRecognizerEntities(lucyEntity.Children);
                    }
                    return JToken.FromObject(lucyEntity.Resolution ?? String.Empty);
                }));

                instancesObject[grp.Key] = JArray.FromObject(grp.Select(lucyEntity =>
                {
                    dynamic instance = new JObject();
                    instance.type = lucyEntity.Type;
                    instance.startIndex = lucyEntity.Start;
                    instance.endIndex = lucyEntity.End;
                    instance.text = lucyEntity.Text;
                    return instance;
                }));
            }
            entitiesObject["$instance"] = instancesObject;
            return entitiesObject;
        }

        private static IEnumerable<LucyEntity> GetEntitiesFromObject(Activity activity, JObject entitiesObject)
        {
            dynamic instanceObject = entitiesObject.Property("$instance");
            if (instanceObject != null)
            {
                foreach (var prop in instanceObject.Value.Properties())
                {
                    dynamic instances = prop.Value;
                    dynamic resolutions = entitiesObject[prop.Name];

                    // get resolution
                    for (int i = 0; i < resolutions.Count; i++)
                    {
                        dynamic resolution = (JToken)resolutions[i];
                        dynamic instance = instances[i];

                        var start = (int)instance.startIndex;
                        var end = (int)instance.endIndex;
                        var newEntity = new LucyEntity()
                        {
                            Type = ((string)instance.type).StartsWith("builtin.") ? ((string)instance.type).Replace("builtin.", "") : instance.type,
                            Start = start,
                            End = end,
                            // Score = metadata.Score,
                            Text = activity.Text?.Substring(start, end - start)
                        };

                        if (resolution is JObject && resolution.Property("$instance") != null)
                        {
                            foreach (var entity in GetEntitiesFromObject(activity, resolution))
                            {
                                newEntity.Children.Add(entity);
                            }
                        }
                        else
                        {
                            newEntity.Resolution = resolution;
                        }

                        yield return newEntity;
                    }
                }
            }
        }
    }
}
