using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Phonetic;
using Lucene.Net.Analysis.Phonetic.Language.Bm;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.LuLu
{
    public class LuLuRecognizer : Recognizer
    {
        private Analyzer exactAnalyzer;
        private Analyzer fuzzyAnalyzer;

        public const string Kind = "Iciclecreek.LuLuRecognizer";

        public LuLuRecognizer([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(callerPath, callerLine)
        {
            this.exactAnalyzer = new StandardAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48);
            this.fuzzyAnalyzer = Analyzer.NewAnonymous((field, textReader) =>
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
            });
        }

        /// <summary>
        /// Gets or sets the .LU resource to load.
        /// </summary>
        public string Resource { get; set; }

        // public Recognizer ExternalEntityRecognizer { get; set; }

        public async override Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            var result = new RecognizerResult();

            // standardize input tokens


            // look for simple entities

            // look for phraseList expansions

            // look for 

            // call external entity recognizer


            return result;
        }
    }
}
