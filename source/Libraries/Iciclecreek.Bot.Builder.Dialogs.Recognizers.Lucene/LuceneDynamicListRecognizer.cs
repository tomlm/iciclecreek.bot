using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder;
using Luis = Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using System.IO;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Phonetic;
using Lucene.Net.Analysis.Phonetic.Language.Bm;
using Utils = Lucene.Net.Util;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using F23.StringSimilarity;
using Microsoft.Bot.Builder.AI.LuisV3;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucene
{
    /// <summary>
    /// Recognizer implementation which uses Lucene TokenAnalyzer to recognize DynamicLists client side. 
    /// This supports fuzzy "sounds-like" matching using Beider-Morse techniques.
    /// </summary>
    public class LuceneDynamicListRecognizer : Recognizer
    {
        private Analyzer exactAnalyzer;
        private Analyzer fuzzyAnalyzer;

        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Iciclecreek.LuceneDynamicListRecognizer";

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexRecognizer"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public LuceneDynamicListRecognizer([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(callerPath, callerLine)
        {
            this.exactAnalyzer = new StandardAnalyzer(Utils.LuceneVersion.LUCENE_48);
            this.fuzzyAnalyzer = Analyzer.NewAnonymous((field, textReader) =>
            {
                Tokenizer tokenizer = new StandardTokenizer(Utils.LuceneVersion.LUCENE_48, textReader);
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
        /// Gets or sets the dynamic lists to recognize
        /// </summary>
        /// <value>
        /// The dynamic lists
        /// </value>
        [JsonProperty("dynamicLists")]
        public ArrayExpression<Luis.DynamicList> DynamicLists { get; set; }

        /// <summary>
        /// Gets or sets whether to use Beider-Morse sounds like functionality to recognize entities.
        /// </summary>
        /// <value>the boolean value or expression to evaluate.</value>
        [JsonProperty("fuzzyMatch")]
        public BoolExpression FuzzyMatch { get; set; } = true;

        /// <summary>
        /// Runs current DialogContext.TurnContext.Activity through a recognizer and returns a <see cref="RecognizerResult"/>.
        /// </summary>
        /// <param name="dialogContext">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="activity"><see cref="Activity"/> to recognize.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/> of the task.</param>
        /// <param name="telemetryProperties">Optional, additional properties to be logged to telemetry with the LuisResult event.</param>
        /// <param name="telemetryMetrics">Optional, additional metrics to be logged to telemetry with the LuisResult event.</param>
        /// <returns>Analysis of utterance.</returns>
        public override async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
        {
            // Identify matched intents
            var text = activity.Text ?? string.Empty;
            var locale = activity.Locale ?? "en-us";

            var recognizerResult = new RecognizerResult()
            {
                Text = text,
                Entities = new JObject()
            };

            if (string.IsNullOrWhiteSpace(text))
            {
                // nothing to recognize, return empty recognizerResult
                return recognizerResult;
            }

            var dynamicLists = DynamicLists.GetValue(dialogContext.State);
            var fuzzyMatch = FuzzyMatch.GetValue(dialogContext.State);

            dynamic entities = recognizerResult.Entities;

            foreach (var dynamicList in dynamicLists)
            {
                var matches = Search(text, dynamicList.List, fuzzyMatch);

                if (matches.Any())
                {
                    var clusters = matches.GroupBy(match => $"{match.StartOffset}-{match.EndOffset}");

                    entities["$instance"] = new JObject();
                    entities[dynamicList.Entity] = new JArray();
                    entities["$instance"][dynamicList.Entity] = new JArray();

                    foreach (var cluster in clusters)
                    {
                        // add all ids with exact same start/end
                        entities[dynamicList.Entity].Add(JArray.FromObject(cluster.Select(cl => cl.Id).ToArray()));

                        dynamic instance = new JObject();
                        var match = cluster.First();
                        instance.startIndex = match.StartOffset;
                        instance.endIndex = match.EndOffset;
                        instance.score = match.Score;
                        instance.text = text.Substring(match.StartOffset, match.EndOffset - match.StartOffset);
                        instance.type = dynamicList.Entity;
                        entities["$instance"][dynamicList.Entity].Add(instance);
                    }
                }
            }

            // if no match return None intent
            recognizerResult.Intents.Add("None", new IntentScore() { Score = 1.0 });

            await dialogContext.Context.TraceActivityAsync(nameof(LuceneDynamicListRecognizer), JObject.FromObject(recognizerResult), "RecognizerResult", "DynamicListRecognizerResult", cancellationToken).ConfigureAwait(false);

            this.TrackRecognizerResult(dialogContext, "DynamicListRecognizerResult", this.FillRecognizerResultTelemetryProperties(recognizerResult, telemetryProperties), telemetryMetrics);     

            return recognizerResult;
        }

        public List<string> ResolveIds(List<string> names, Luis.DynamicList dynamicList, bool fuzzyMatch=true)
        {
            List<string> res = new List<string>();

            foreach (var name in names)
            {
                var matches = Search(name, dynamicList.List, fuzzyMatch);

                if (matches.Any())
                {
                    var clusters = matches.GroupBy(match => (match.EndOffset - match.StartOffset));
                    var max_span_cluster = clusters.OrderByDescending(cl => cl.Key).First();
                    var ids = max_span_cluster.Select(tm => tm.Id).ToArray();
                    var name_ids = $"{name} ({string.Join(" or ", ids)})";
                    res.Add(name_ids);

                } else
                {
                    res.Add(name);
                }
            }

            return res;
        }

        /// <summary>
        /// Identify mention and id of meeting members from utterance,
        /// collect them as dynamicLists to be attached to LuisAdaptiveRecognizer
        /// </summary>
        /// <param name="utterance">User utterance</param>
        /// <param name="memberDynamicList">Known meeting members (id, name etc) from Teams</param>
        /// <param name="fuzzyMatch">default true to enable approximate matching</param>
        public List<Luis.DynamicList> GetMatchedEntityDynamicLists(string utterance, Luis.DynamicList memberDynamicList,bool fuzzyMatch=true)
        {
            var matches = Search(utterance, memberDynamicList.List, fuzzyMatch);

            if (matches.Any() == false) return null;

            var idMentionDynamicLists = new List<Luis.DynamicList>();
                
            var idMentionDynamicList = new Luis.DynamicList();
            idMentionDynamicList.Entity = "MatchedMember";
            idMentionDynamicList.List = new List<ListElement>();

            var idIdx = new Dictionary<string, int>();
            var newIdx = 0;

            foreach (var match in matches)
            {
                var id = match.Id;
                var mention = utterance.Substring(match.StartOffset, match.EndOffset - match.StartOffset);

                if (idIdx.ContainsKey(id))
                {
                    var existingIdx = idIdx[id];
                    idMentionDynamicList.List[existingIdx].Synonyms.Add(mention);
                } else
                {
                    idIdx[id] = newIdx;
                    newIdx += 1;

                    var person = new ListElement()
                    {
                        CanonicalForm = id,
                        Synonyms = new List<string>()
                    };
                    person.Synonyms.Add(mention);
                    idMentionDynamicList.List.Add(person);
                }
            }

            idMentionDynamicLists.Add(idMentionDynamicList);
            
            return idMentionDynamicLists;
        }

        private List<TokenMatch> Search(string search, IList<ListElement> list, bool useFuzzyMatch)
        {
            // canonicalForm => listElement
            var listElements = list.ToDictionary(el => el.CanonicalForm, el => el);

            // exact token => canonical id
            var exactIndex = new Dictionary<string, HashSet<string>>();

            // fuzy token => canonical Id
            var fuzzyIndex = new Dictionary<string, HashSet<string>>();

            // create exact and fuzzy indices
            foreach (var listElement in listElements.Values)
            {
                BuildIndex(listElement.CanonicalForm, (string)listElement.CanonicalForm, exactAnalyzer, exactIndex);
                foreach (var synonym in listElement.Synonyms)
                {
                    BuildIndex(synonym, (string)listElement.CanonicalForm, exactAnalyzer, exactIndex);
                    if (synonym.IndexOf('@') > 0)
                    {
                        BuildIndex(synonym.Split('@').FirstOrDefault(), (string)listElement.CanonicalForm, exactAnalyzer, exactIndex);
                    }
                    else
                    {
                        BuildIndex(synonym, (string)listElement.CanonicalForm, exactAnalyzer, exactIndex);

                        // build fuzzy index
                        if (useFuzzyMatch)
                        {
                            BuildIndex(synonym, (string)listElement.CanonicalForm, fuzzyAnalyzer, fuzzyIndex);
                        }
                    }
                }
            }

            // SEARCH
            List<TokenMatch> matches = SearchAnalyzer(search, exactAnalyzer, exactIndex);

            if (useFuzzyMatch)
            {
                // then merge fuzzy match results..
                var fuzzyMatches = SearchAnalyzer(search, fuzzyAnalyzer, fuzzyIndex);
                foreach (var fuzzyMatch in fuzzyMatches)
                {
                    MergeMatch(matches, fuzzyMatch);
                }
            }

            // compute score as edit distance between first synonym and matched text
            var levenshtein = new Levenshtein();
            foreach (var match in matches)
            {
                string text = search.Substring(match.StartOffset, match.EndOffset - match.StartOffset).ToLower();
                var listElement = listElements[match.Id];
                string firstSynonym = listElement.Synonyms.First().ToLower();
                match.Score = ((float)firstSynonym.Length - (float)levenshtein.Distance(firstSynonym, text)) / firstSynonym.Length;
                //match.Score = listElement.Synonyms.Select(syn => ((float)syn.Length - (float)levenshtein.Distance(syn.ToLower(), text)) / syn.Length).Max();
            }

            //// now adjust for overlapping scores
            //foreach (var match in matches)
            //{
            //    var longestOverlappingToken = matches.Where(m => m != match && m.StartOffset < match.EndOffset && m.EndOffset > match.StartOffset).OrderByDescending(m => m.EndOffset - m.StartOffset).FirstOrDefault();
            //    if (longestOverlappingToken != null)
            //    {
            //        string longestText = search.Substring(match.StartOffset, match.EndOffset - match.StartOffset).ToLower();
            //        string text2 = search.Substring(longestOverlappingToken.StartOffset, longestOverlappingToken.EndOffset - longestOverlappingToken.StartOffset).ToLower();
            //        if (text2.Length > longestText.Length)
            //        {
            //            longestText = text2;
            //        }
            //        var listElement = listElements[match.Id];
            //        match.Score = listElement.Synonyms.Select(syn => ((float)longestText.Length - (float)levenshtein.Distance(syn.ToLower(), longestText)) / longestText.Length).Max();
            //    }
            //}

            return matches;
        }

        private class TokenMatch
        {
            public string Id { get; set; }

            public float Score { get; set; }

            public int StartOffset { get; set; }

            public int EndOffset { get; set; }
        }

        private void BuildIndex(string text, string id, Analyzer analyzer, Dictionary<string, HashSet<string>> index)
        {
            if (!String.IsNullOrEmpty(text))
            {
                using (TextReader reader = new StringReader(text))
                {
                    using (var tokenStream = analyzer.GetTokenStream("name", reader))
                    {
                        var termAtt = tokenStream.GetAttribute<ICharTermAttribute>();
                        var offsetAtt = tokenStream.GetAttribute<IOffsetAttribute>();
                        tokenStream.Reset();

                        while (tokenStream.IncrementToken())
                        {
                            if (!index.TryGetValue(termAtt.ToString(), out var values))
                            {
                                values = new HashSet<string>();
                                index[termAtt.ToString()] = values;
                            }

                            if (!values.Contains(id))
                            {
                                values.Add(id);
                            }
                        }
                    }
                }
            }
        }

        private List<TokenMatch> SearchAnalyzer(string search, Analyzer analyzer, Dictionary<string, HashSet<string>> index)
        {
            List<TokenMatch> matches = new List<TokenMatch>();
            using (TextReader reader = new StringReader(search))
            {
                using (var tokenStream = analyzer.GetTokenStream("name", reader))
                {
                    var termAtt = tokenStream.GetAttribute<ICharTermAttribute>();
                    var offsetAtt = tokenStream.GetAttribute<IOffsetAttribute>();
                    tokenStream.Reset();

                    while (tokenStream.IncrementToken())
                    {
                        var token = termAtt.ToString();
                        if (token.Length > 1)
                        {
                            if (index.TryGetValue(token, out var ids))
                            {
                                foreach (var id in ids)
                                {
                                    var match = new TokenMatch()
                                    {
                                        Id = id,
                                        StartOffset = offsetAtt.StartOffset,
                                        EndOffset = offsetAtt.EndOffset
                                    };
                                    MergeMatch(matches, match);
                                }
                            }
                        }
                    }
                }
            }

            return matches;
        }

        private void MergeMatch(List<TokenMatch> matches, TokenMatch mergeMatch)
        {
            // if fuzzyMatch overlaps exactmatch for the same id
            foreach (var match in matches.Where(match => match.Id == mergeMatch.Id))
            {
                if (mergeMatch.StartOffset == match.StartOffset && mergeMatch.EndOffset == match.EndOffset)
                {
                    // we already have this one
                    return;
                }

                // if new match is following an exact match for the same id
                if (mergeMatch.StartOffset == (match.EndOffset + 1))
                {
                    // merge them
                    match.EndOffset = mergeMatch.EndOffset;
                    return;
                }

                // if new match follows an exact match for the same id
                if (mergeMatch.EndOffset == (match.StartOffset - 1))
                {
                    // merge them
                    match.StartOffset = mergeMatch.StartOffset;
                    return;
                }

                // if it's bigger on both ends
                if (mergeMatch.StartOffset < match.StartOffset && mergeMatch.EndOffset > match.EndOffset)
                {
                    // take the bigger token
                    match.StartOffset = mergeMatch.StartOffset;
                    match.EndOffset = mergeMatch.EndOffset;
                    return;
                }

                // if it's inside the current token
                if (mergeMatch.StartOffset >= match.StartOffset && mergeMatch.EndOffset <= match.EndOffset)
                {
                    // then current token is the bigger token
                    return;
                }

                // if offset overlapping at start or end
                if ((mergeMatch.StartOffset <= match.StartOffset && mergeMatch.EndOffset >= match.StartOffset && mergeMatch.EndOffset <= match.EndOffset) ||
                    (mergeMatch.StartOffset >= match.StartOffset && mergeMatch.StartOffset < match.EndOffset && mergeMatch.EndOffset >= match.EndOffset))
                {
                    // expand to capture the overlap.
                    match.StartOffset = Math.Min(match.StartOffset, mergeMatch.StartOffset);
                    match.EndOffset = Math.Max(match.EndOffset, mergeMatch.EndOffset);
                    return;
                }
            }

            // else it's unique, add it
            matches.Add(mergeMatch);
        }
    }
}
