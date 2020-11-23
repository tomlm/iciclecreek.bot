using System;
using System.Collections.Generic;
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
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy
{
    public class LucyRecognizer : Recognizer
    {
        private Lazy<Analyzer> exactAnalyzer;

        private Lazy<Analyzer> fuzzyAnalyzer;

        public LucyRecognizer([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(callerPath, callerLine)
        {
            exactAnalyzer = new Lazy<Analyzer>(() => new StandardAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48, CreateStopWords()));

            fuzzyAnalyzer = new Lazy<Analyzer>(() => Analyzer.NewAnonymous((field, textReader) =>
                {
                    Tokenizer tokenizer = new StandardTokenizer(LuceneVersion.LUCENE_48, textReader);
                    // TokenStream stream = new DoubleMetaphoneFilter(tokenizer, 6, false);
                    var factory = new BeiderMorseFilterFactory(new Dictionary<string, string>()
                    {
                        { "nameType", NameType.GENERIC.ToString()},
                        { "ruleType", RuleType.APPROX.ToString() },
                        { "languageSet", "auto"}
                    });
                    TokenStream stream = factory.Create(tokenizer);
                    return new TokenStreamComponents(tokenizer, stream);
                })
            );
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

        private CharArraySet CreateStopWords(string[] stopWords = null)
        {
            return CharArraySet.UnmodifiableSet(new CharArraySet(LuceneVersion.LUCENE_48, stopWords ?? Array.Empty<string>(), false));
        }
    }
}
