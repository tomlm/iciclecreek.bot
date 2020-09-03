﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Lucene.Net.Store;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.QLucene
{
    public class QLuceneRecognizer : Recognizer
    {
        private static ConcurrentDictionary<string, QLuceneEngine> engines = new ConcurrentDictionary<string, QLuceneEngine>();
        private object _monitor = new object();

        private const string IntentPrefix = "intent=";

        /// <summary>
        /// Key used when adding the intent to the <see cref="RecognizerResult"/> intents collection.
        /// </summary>
        public const string QnAMatchIntent = "QnAMatch";

        public const string Kind = "Iciclecreek.QLuceneRecognizer";

        public QLuceneRecognizer()
        {

        }

        /// <summary>
        /// ResourceId of the qna file. Example: foo.en-us.qna
        /// </summary>
        /// <remarks>This will look for the foo.en-us.qna.json as generated by QLuBuild, and if no json is found it will assume this path is the json file.</remarks>
        [JsonProperty("resourceId")]
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the whether to include the dialog name metadata for QnA context.
        /// </summary>
        /// <value>
        /// A bool or boolean expression.
        /// </value>
        [DefaultValue(true)]
        [JsonProperty("includeDialogNameInMetadata")]
        public BoolExpression IncludeDialogNameInMetadata { get; set; } = true;

        /// <summary>
        /// Gets or sets an expression to evaluate to set additional metadata name value pairs.
        /// </summary>
        /// <value>An expression to evaluate for pairs of metadata.</value>
        [JsonProperty("metadata")]
        public ArrayExpression<Metadata> Metadata { get; set; }

        /// <summary>
        /// Gets or sets an expression to evaluate to set the context.
        /// </summary>
        /// <value>An expression to evaluate to QnARequestContext to pass as context.</value>
        [JsonProperty("context")]
        public ObjectExpression<QnARequestContext> Context { get; set; }

        /// <summary>
        /// Gets or sets the threshold score to filter results.
        /// </summary>
        /// <value>
        /// The threshold for the results.
        /// </value>
        [DefaultValue(0.3F)]
        [JsonProperty("threshold")]
        public NumberExpression Threshold { get; set; } = 0.3F;

        public async override Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<System.String, System.String> telemetryProperties = null, Dictionary<System.String, System.Double> telemetryMetrics = null)
        {
            var qluceneEngine = await GetEngine(dialogContext).ConfigureAwait(false);

            var threshold = Threshold.GetValue(dialogContext);
            var recognizerResult = new RecognizerResult
            {
                Text = activity.Text,
                Intents = new Dictionary<string, IntentScore>(),
            };

            if (string.IsNullOrEmpty(activity.Text))
            {
                recognizerResult.Intents.Add("None", new IntentScore());
                return recognizerResult;
            }

            var filters = new List<Metadata>();
            if (IncludeDialogNameInMetadata.GetValue(dialogContext.State))
            {
                filters.Add(new Metadata
                {
                    Name = "dialogName",
                    Value = dialogContext.ActiveDialog.Id
                });
            }

            // if there is $qna.metadata set add to filters
            var externalMetadata = Metadata?.GetValue(dialogContext.State);
            if (externalMetadata != null)
            {
                filters.AddRange(externalMetadata);
            }

            var topAnswer = qluceneEngine.GetAnswers(activity.Text, threshold: threshold);

            if (topAnswer != null)
            {
                if (topAnswer.Answer.Trim().ToUpperInvariant().StartsWith(IntentPrefix.ToUpperInvariant(), StringComparison.Ordinal))
                {
                    recognizerResult.Intents.Add(topAnswer.Answer.Trim().Substring(IntentPrefix.Length).Trim(), new IntentScore { Score = topAnswer.Score });
                }
                else
                {
                    recognizerResult.Intents.Add(QnAMatchIntent, new IntentScore { Score = topAnswer.Score });
                }

                var answerArray = new JArray();
                answerArray.Add(topAnswer.Answer);
                ObjectPath.SetPathValue(recognizerResult, "entities.answer", answerArray);

                var instance = new JArray();
                var data = JObject.FromObject(topAnswer);
                data["startIndex"] = 0;
                data["endIndex"] = activity.Text.Length;
                instance.Add(data);
                ObjectPath.SetPathValue(recognizerResult, "entities.$instance.answer", instance);

                // recognizerResult.Properties["answers"] = answers;
            }
            else
            {
                recognizerResult.Intents.Add("None", new IntentScore { Score = 1.0f });
            }

            return recognizerResult;
        }

        private async Task<QLuceneEngine> GetEngine(DialogContext dialogContext)
        {
            QLuceneEngine engine;

            if (engines.TryGetValue(this.ResourceId, out engine))
            {
                return engine;
            }

            try
            {
                Monitor.Enter(_monitor);
                if (engines.TryGetValue(this.ResourceId, out engine))
                {
                    return engine;
                }
                var resourceExplorer = dialogContext.Context.TurnState.Get<ResourceExplorer>();
                Resource resource;
                if (!resourceExplorer.TryGetResource(this.ResourceId + ".json", out resource))
                {
                    resource = resourceExplorer.GetResource(this.ResourceId);
                }

                var json = await resource.ReadTextAsync().ConfigureAwait(false);

                Directory directory = new RAMDirectory();
                QLuceneEngine.CreateCatalog(json, directory);
                engines[ResourceId] = new QLuceneEngine(directory);
                return engines[ResourceId];
            }
            finally
            {
                Monitor.Exit(_monitor);
            }
        }
    }
}
