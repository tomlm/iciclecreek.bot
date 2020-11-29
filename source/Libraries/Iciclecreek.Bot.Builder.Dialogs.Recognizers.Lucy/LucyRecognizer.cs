using System;
using System.Collections.Generic;
using System.IO;
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
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy
{
    public class LucyRecognizer : Recognizer
    {
        private LucyEngine _engine = null;
        private JsonConverter patternModelConverter = new PatternModelConverter();

        private IDeserializer yamlDeserializer = new DeserializerBuilder()
                                                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                                    .Build();
        private ISerializer yamlSerializer = new SerializerBuilder()
                                                .JsonCompatible()
                                                .Build();



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
                var yaml = await modelResource.ReadTextAsync();
                var yobj = yamlDeserializer.Deserialize(new StringReader(yaml));
                var json = yamlSerializer.Serialize(yobj);
                var model = JsonConvert.DeserializeObject<LucyModel>(json, patternModelConverter);
                this._engine = new LucyEngine(model);
            }

            List<LucyEntity> externalEntities = new List<LucyEntity>();
            if (ExternalEntityRecognizer != null)
            {
                var results = await ExternalEntityRecognizer.RecognizeAsync(dialogContext, activity, cancellationToken, telemetryProperties, telemetryMetrics).ConfigureAwait(false);

                externalEntities = GetEntitiesFromObject(activity, results.Entities).ToList();
            }

            var recognizerResult = new RecognizerResult();
            var lucyEntities = _engine.MatchEntities(activity.Text, activity.Locale, externalEntities);
            foreach (var lucyEntity in lucyEntities)
            {
                // todo
            }

            return recognizerResult;
        }

        private static IEnumerable<LucyEntity> GetEntitiesFromObject(Activity activity, JObject entity)
        {
            dynamic instance = entity.Property("$instance");
            if (instance != null)
            {
                foreach (var prop in instance.Value.Properties())
                {
                    dynamic metadatas = prop.Value;
                    dynamic resolutions = entity[prop.Name];

                    // get resolution
                    for (int i = 0; i < resolutions.Count; i++)
                    {
                        dynamic resolution = (JToken)resolutions[i];
                        dynamic metadata = metadatas[i];

                        var start = (int)metadata.startIndex;
                        var end = (int)metadata.endIndex;
                        var newEntity = new LucyEntity()
                        {
                            Type = metadata.type,
                            Start = start,
                            End = end,
                            // Score = metadata.Score,
                            Text = activity.Text.Substring(start, end - start)
                        };

                        if (resolution is JObject && resolution.Property("$instance") != null)
                        {
                            newEntity.Children.AddRange(GetEntitiesFromObject(activity, resolution));
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
