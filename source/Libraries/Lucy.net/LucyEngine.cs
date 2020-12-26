using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Lucy.PatternMatchers;
using Lucy.PatternMatchers.Matchers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Ar;
using Lucene.Net.Analysis.Ca;
using Lucene.Net.Analysis.Cjk;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Cz;
using Lucene.Net.Analysis.Da;
using Lucene.Net.Analysis.De;
using Lucene.Net.Analysis.El;
using Lucene.Net.Analysis.En;
using Lucene.Net.Analysis.Es;
using Lucene.Net.Analysis.Eu;
using Lucene.Net.Analysis.Fa;
using Lucene.Net.Analysis.Fi;
using Lucene.Net.Analysis.Fr;
using Lucene.Net.Analysis.Ga;
using Lucene.Net.Analysis.Gl;
using Lucene.Net.Analysis.Hi;
using Lucene.Net.Analysis.Hu;
using Lucene.Net.Analysis.Hy;
using Lucene.Net.Analysis.Id;
using Lucene.Net.Analysis.It;
using Lucene.Net.Analysis.Lv;
using Lucene.Net.Analysis.Nl;
using Lucene.Net.Analysis.No;
using Lucene.Net.Analysis.Phonetic;
using Lucene.Net.Analysis.Phonetic.Language.Bm;
using Lucene.Net.Analysis.Pt;
using Lucene.Net.Analysis.Ro;
using Lucene.Net.Analysis.Ru;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Sv;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Analysis.Tr;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;
using Newtonsoft.Json.Linq;
using builtin = Microsoft.Recognizers.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Recognizers.Text.Number.Chinese;

namespace Lucy
{
    /// <summary>
    /// LucyEngine uses a LucyModel to do LU pattern matching for entities.
    /// </summary>
    public class LucyEngine
    {
        private Random _rnd = new Random();
        private LucyModel _lucyModel;
        private Analyzer _simpleAnalyzer = new SimpleAnalyzer(LuceneVersion.LUCENE_48);
        private Analyzer _exactAnalyzer;
        private Analyzer _fuzzyAnalyzer;
        private PatternParser _patternParser;
        private QuotedTextEntityRecognizer _quotedTextEntityRecognizer = new QuotedTextEntityRecognizer();

        private static HashSet<string> builtinEntities { get; set; } = new HashSet<string>()
        {
            "age", "boolean", "currency", "datetime", "dimension", "email", "guid", "hashtag",
            "ip", "mention", "number", "numberrange", "ordinal", "percentage", "phonenumber", "temperature", "url",
            "quotedtext", "integer", "fraction", "decimal"
        };

        public LucyEngine(LucyModel model, Analyzer exactAnalyzer = null, Analyzer fuzzyAnalyzer = null, bool useAllBuiltIns = false)
        {
            this._lucyModel = model;

            this._exactAnalyzer = exactAnalyzer ?? GetAnalyzerForLocale(model.Locale);

            this._fuzzyAnalyzer = exactAnalyzer ?? fuzzyAnalyzer ??
                Analyzer.NewAnonymous((field, textReader) =>
                {
                    Tokenizer tokenizer = new StandardTokenizer(LuceneVersion.LUCENE_48, textReader);
                    TokenStream stream = new DoubleMetaphoneFilter(tokenizer, 6, false);
                    //TokenStream stream = new BeiderMorseFilterFactory(new Dictionary<string, string>()
                    //    {
                    //        { "nameType", NameType.GENERIC.ToString()},
                    //        { "ruleType", RuleType.APPROX.ToString() },
                    //        { "languageSet", "auto"}
                    //    }).Create(tokenizer);
                    return new TokenStreamComponents(tokenizer, stream);
                });

            this._patternParser = new PatternParser(this._exactAnalyzer, this._fuzzyAnalyzer); ;

            LoadModel();

            if (useAllBuiltIns)
            {
                BuiltinEntities = new HashSet<string>(builtinEntities);
                // add default pattern for datetime = (all permutations of datetime)
                EntityPatterns.Add(new EntityPattern("datetime", _patternParser.Parse("(@datetimeV2.date|@datetimeV2.time|@datetimeV2.datetime|@datetimeV2.daterange|@datetimeV2.timerange|@datetimeV2.datetimerange|@datetimeV2.duration)")));
            }

            ValidateModel();
        }

        /// <summary>
        /// The locale for this model
        /// </summary>
        public string Locale { get; set; } = "en";

        public HashSet<string> BuiltinEntities { get; set; } = new HashSet<string>();

        /// <summary>
        /// Patterns to match
        /// </summary>
        public List<EntityPattern> EntityPatterns { get; set; } = new List<EntityPattern>();

        /// <summary>
        /// Wildcard Patterns to match
        /// </summary>
        public List<EntityPattern> WildcardEntityPatterns { get; set; } = new List<EntityPattern>();

        /// <summary>
        /// Regex patterns to match
        /// </summary>
        public List<RegexPatternMatcher> RegexEntityPatterns { get; set; } = new List<RegexPatternMatcher>();

        /// <summary>
        /// Warning messages
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Match entities in given text
        /// </summary>
        /// <param name="text">text to match against.</param>
        /// <param name="culture">culture</param>
        /// <param name="externalEntities">externally provided entities</param>
        /// <param name="includeInternal">include tokens in results</param>
        /// <returns>entities</returns>
        public IList<LucyEntity> MatchEntities(string text, IEnumerable<LucyEntity> externalEntities = null, bool includeInternal = false)
        {
            var context = new MatchContext()
            {
                Text = text,
            };

            if (externalEntities != null)
            {
                foreach (var externalEntity in externalEntities)
                {
                    context.AddNewEntity(externalEntity);
                }
                context.ProcessNewEntities();
            }

            if (this.BuiltinEntities.Any())
            {
                AddBuiltInEntities(context, text, Locale);
                context.ProcessNewEntities();
            }

            // Add regex pattern entities
            if (this.RegexEntityPatterns.Any())
            {
                foreach (var regex in this.RegexEntityPatterns)
                {
                    foreach (var entity in regex.Matches(text))
                    {
                        context.NewEntities.Add(entity);
                    }
                }
                context.ProcessNewEntities();
            }

            // add all @Token entities
            context.TokenEntities.AddRange(Tokenize(text));

            int count = 0;
            do
            {
                count = context.Entities.Count;

                // foreach text token
                foreach (var tokenEntity in context.TokenEntities)
                {
                    // foreach entity pattern
                    foreach (var entityPattern in EntityPatterns)
                    {
                        ProcessEntityPattern(context, tokenEntity, entityPattern);
                    }
                }

                context.ProcessNewEntities();

                // if entities were added we need to run wildcard matchers
                if (count == context.Entities.Count && WildcardEntityPatterns.Any())
                {
                    // process wildcard patterns
                    foreach (var textEntity in context.TokenEntities)
                    {
                        foreach (var entityPattern in WildcardEntityPatterns)
                        {
                            ProcessEntityPattern(context, textEntity, entityPattern);
                        }
                    }
                    context.ProcessNewEntities();
                }
            } while (count != context.Entities.Count);

            context.MergeEntities(context.Entities);
            context.ResolveEntities(context.Entities);

            // only include tokenEntities if they ask for them
            if (includeInternal)
            {
                var merged = new List<LucyEntity>(context.TokenEntities);
                merged.AddRange(context.Entities);
                return merged;
            }

            return context.Entities.OrderByDescending(e => e.Score).ToList();
        }

        public IEnumerable<string> GenerateExamples(string entityType)
        {
            var patterns = new List<EntityPattern>(this.EntityPatterns.Where(et => et.Name == entityType));
            patterns.AddRange(this.WildcardEntityPatterns.Where(et => et.Name == entityType));
            foreach (var ep in patterns)
            {
                foreach (var example in ep.PatternMatcher.GenerateExamples(this))
                {
                    if (!String.IsNullOrWhiteSpace(example))
                    {
                        yield return example.Trim();
                    }
                }
            }
        }

        public string GenerateExample(string entityType)
        {
            //var patterns = new List<EntityPattern>(this.EntityPatterns.Where(et => et.Name == entityType));
            //patterns.AddRange(this.WildcardEntityPatterns.Where(et => et.Name == entityType));
            //var ep = patterns[_rnd.Next(patterns.Count)];
            //return ep.PatternMatcher.GenerateExample(this);
            return string.Empty;
        }

        public IEnumerable<TokenEntity> Tokenize(string text)
        {
            TokenEntity previous = null;
            using (var tokenStream = _exactAnalyzer.GetTokenStream("name", text))
            {
                var termAtt = tokenStream.GetAttribute<ICharTermAttribute>();
                var offsetAtt = tokenStream.GetAttribute<IOffsetAttribute>();
                tokenStream.Reset();

                while (tokenStream.IncrementToken())
                {
                    string token = termAtt.ToString();
                    bool skipFuzzy = false;
                    var start = offsetAtt.StartOffset;
                    var end = offsetAtt.EndOffset;

                    if (start > 0 && text[start - 1] == '@')
                    {
                        token = text.Substring(start - 1, end - start + 1);
                        skipFuzzy = true;
                    }
                    else if (start > 0 && text[start - 1] == '$')
                    {
                        token = text.Substring(start - 1, end - start + 1);
                        skipFuzzy = true;
                    }

                    var resolution = new TokenResolution()
                    {
                        Token = token
                    };

                    var tokenEntity = new TokenEntity()
                    {
                        Type = TokenPatternMatcher.ENTITYTYPE,
                        Text = text.Substring(start, end - start),
                        Start = offsetAtt.StartOffset,
                        End = end,
                        Resolution = resolution,
                        Previous = previous
                    };

                    if (_fuzzyAnalyzer != null && !skipFuzzy)
                    {
                        // get fuzzyText
                        using (var fuzzyTokenStream = _fuzzyAnalyzer.GetTokenStream("name", tokenEntity.Text))
                        {
                            var fuzzyTermAtt = fuzzyTokenStream.GetAttribute<ICharTermAttribute>();
                            fuzzyTokenStream.Reset();
                            while (fuzzyTokenStream.IncrementToken())
                            {
                                resolution.FuzzyTokens.Add(fuzzyTermAtt.ToString());
                            }
                        }
                    }
                    if (previous != null)
                    {
                        previous.Next = tokenEntity;
                    }
                    previous = tokenEntity;
                    yield return tokenEntity;
                }
            }
        }

        public static string VisualEntities(string text, IEnumerable<LucyEntity> entities, bool showSpans = true, bool showHierarchy = true)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var entity in entities)
            {
                sb.AppendLine(VisualizeEntity(text, entity, showSpans, showHierarchy));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Format entities as fixed width string.
        /// </summary>
        /// <param name="text">original text</param>
        /// <param name="entities">entities</param>
        /// <returns></returns>
        public static string VisualizeEntity(string text, LucyEntity entity, bool showSpans = true, bool showHierarchy = true)
        {
            if (entity == null)
            {
                return "No entity";
            }

            StringBuilder sb = new StringBuilder();
            if (showSpans)
            {

                sb.AppendLine($"==== {entity.Type} ({entity.Score})");
                sb.AppendLine(text);
                var allEntities = new List<LucyEntity>(entity.GetAllEntities())
                {
                    entity
                };

                foreach (var grp in allEntities.GroupBy(e => e.Type.ToLower())
                                                    .OrderBy(grp => grp.Max(v => v.End - v.Start))
                                                    .ThenBy(grp => grp.Min(v => v.Start)))
                {
                    sb.Append(FormatEntitiesOfSameTypeAsLine(text, grp));
                }

                sb.AppendLine();
            }

            if (showHierarchy)
            {
                sb.AppendLine(FormatEntityChildren(string.Empty, entity));
            }

            return sb.ToString();
        }

        private static string FormatEntityChildren(string prefix, LucyEntity entity)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{prefix} {entity}");

            if (String.IsNullOrEmpty(prefix))
            {
                prefix = "=> ";
            }

            foreach (var child in entity.Children.OrderBy(e => e.Start))
            {
                sb.Append(FormatEntityChildren("    " + prefix, child));
            }
            return sb.ToString();
        }

        private static string FormatEntitiesOfSameTypeAsLine(string text, IEnumerable<LucyEntity> entities)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var tier in ArrangeEntitiesIntoNonOverlappingTies(entities))
            {
                int lastEnd = 0;
                foreach (var entity in tier)
                {
                    sb.Append(new String(' ', entity.Start - lastEnd));
                    if (entity.End == entity.Start + 1)
                    {
                        sb.Append("^");
                    }
                    else
                    {
                        sb.Append($"^{new String('_', entity.End - entity.Start - 2)}^");
                    }
                    lastEnd = entity.End;
                }
                sb.AppendLine($"{new String(' ', text.Length - lastEnd)} @{entities.First().Type}");
            }

            return sb.ToString();
        }

        private static List<List<LucyEntity>> ArrangeEntitiesIntoNonOverlappingTies(IEnumerable<LucyEntity> entities)
        {
            var candidates = entities.ToList();
            var tiers = new List<List<LucyEntity>>();

            List<LucyEntity> tier = new List<LucyEntity>();
            List<LucyEntity> nextTier = new List<LucyEntity>();
            while (true)
            {
                foreach (var entity in candidates)
                {
                    // if collision
                    if (tier.Any(tierEntity =>
                        String.Equals(tierEntity.Type, entity.Type, StringComparison.OrdinalIgnoreCase) &&
                        (
                            // inside 
                            (tierEntity.Start >= entity.Start && tierEntity.End <= entity.End) ||
                            // overlaps on start
                            (tierEntity.Start <= entity.Start && tierEntity.End >= entity.Start && tierEntity.End <= entity.End) ||
                            // overlaps on End
                            (tierEntity.Start >= entity.Start && tierEntity.Start <= entity.End && tierEntity.End >= entity.End) ||
                            // bigger overlap
                            (tierEntity.Start <= entity.Start && tierEntity.End >= entity.End)
                        )))
                    {
                        nextTier.Add(entity);
                    }
                    else
                    {
                        tier.Add(entity);
                    }
                }

                tiers.Add(tier.OrderBy(e => e.Start).ToList());

                if (nextTier.Any())
                {
                    candidates = nextTier;
                    tier = new List<LucyEntity>();
                    nextTier = new List<LucyEntity>();
                }
                else
                {
                    return tiers;
                }
            }
        }

        private void ProcessEntityPattern(MatchContext context, TokenEntity textEntity, EntityPattern entityPattern)
        {
            context.EntityPattern = entityPattern;
            context.CurrentEntity = new LucyEntity()
            {
                Type = entityPattern.Name,
                Resolution = entityPattern.Resolution,
                Start = textEntity.Start
            };

            // see if it matches at this textEntity starting position.
            var matchResult = entityPattern.PatternMatcher.Matches(context, textEntity, nextPatternMatcher: null);
            //System.Diagnostics.Trace.TraceInformation($"[{textEntity.Start}] {context.EntityPattern} => \"{textEntity}\" {matchResult.Matched}");

            // if it matches
            if (matchResult.Matched && matchResult.NextToken != textEntity)
            {
                // add it to the entities.
                context.CurrentEntity.End = matchResult.End;
                context.CurrentEntity.Text = context.Text.Substring(context.CurrentEntity.Start, context.CurrentEntity.End - context.CurrentEntity.Start);

                //if (context.CurrentEntity.Children.Any())
                //{
                //    context.CurrentEntity.Resolution = null;
                //}
                //else
                if (context.CurrentEntity.Resolution == null)
                {
                    if (entityPattern.Resolution != null)
                    {
                        context.CurrentEntity.Resolution = entityPattern.Resolution;
                    }
                    context.CurrentEntity.Children.RemoveWhere(et => et.Type == WildcardPatternMatcher.ENTITYTYPE);
                }

                context.MergeEntities(context.CurrentEntity.Children);
                context.ResolveEntities(context.CurrentEntity.Children);

                context.CurrentEntity.Score = context.CurrentEntity.GetAllEntities().Count() + ((float)(context.CurrentEntity.End - context.CurrentEntity.Start) / context.Text.Length);

                context.AddNewEntity(context.CurrentEntity);
                // Trace.TraceInformation($"\n [{textEntity.Start}] {context.EntityPattern} => {matchResult.Matched} {context.CurrentEntity}");

                //foreach (var childEntity in context.CurrentEntity.Children)
                //{
                //    context.AddNewEntity(childEntity);
                //}
            }
        }

        private void LoadModel()
        {
            if (_lucyModel.Macros == null)
            {
                _lucyModel.Macros = new Dictionary<string, string>();
            }

            if (_lucyModel.Entities != null)
            {
                foreach (var entityModel in _lucyModel.Entities)
                {
                    if (entityModel.Patterns != null)
                    {
                        foreach (var patternModel in entityModel.Patterns)
                        {
                            var first = patternModel.First();
                            string resolution = first.Any(ch => ch == '@' || ch == '|' || ch == '+' || ch == '*' || ch == '?') || first.Contains("___") ? null : first.Trim('~', '(', ')');

                            foreach (var pattern in patternModel.Select(pat => ExpandMacros(pat)).OrderByDescending(pat => pat.Length))
                            {
                                if (pattern.StartsWith('/') && pattern.EndsWith('/'))
                                {
                                    RegexEntityPatterns.Add(new RegexPatternMatcher(entityModel.Name, pattern.Trim('/')));
                                }
                                else
                                {
                                    var patternMatcher = _patternParser.Parse(pattern, entityModel.FuzzyMatch);
                                    if (patternMatcher != null)
                                    {
                                        var ignoreWords = entityModel.Ignore.Select(ignoreText => ((TokenResolution)Tokenize(ignoreText).First().Resolution).Token);

                                        // Trace.TraceInformation($"{expandedPattern} => {patternMatcher}");
                                        if (patternMatcher.ContainsWildcard())
                                        {
                                            // we want to process wildcard patterns last
                                            WildcardEntityPatterns.Add(new EntityPattern(entityModel.Name, resolution, patternMatcher, ignoreWords));
                                        }
                                        else
                                        {
                                            EntityPatterns.Add(new EntityPattern(entityModel.Name, resolution, patternMatcher, ignoreWords));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Auto detect all references to built in entities
                foreach (var pattern in this.EntityPatterns.ToList())
                {
                    foreach (var reference in pattern.PatternMatcher.GetEntityReferences().Select(r => r.TrimStart('@')))
                    {
                        if (reference == "datetime" || reference == "datetimeV2")
                        {
                            this.BuiltinEntities.Add("datetime");

                            // add default pattern for datetime = (all permutations of datetime)
                            EntityPatterns.Add(new EntityPattern("datetime", _patternParser.Parse("(@datetimeV2.date|@datetimeV2.time|@datetimeV2.datetime|@datetimeV2.daterange|@datetimeV2.timerange|@datetimeV2.datetimerange|@datetimeV2.duration)")));
                        }

                        if (builtinEntities.Contains(reference) ||
                            builtinEntities.Contains(reference.Split('.').First()))
                        {
                            this.BuiltinEntities.Add(reference);
                        }
                    }
                }
            }
        }

        private void ValidateModel()
        {
            HashSet<string> entityDefinitions = new HashSet<string>(builtinEntities)
            {
                "datetime", "datetimeV2", "datetimeV2.date", "datetimeV2.time", "datetimeV2.datetime",
                "datetimeV2.daterange", "datetimeV2.timerange", "datetimeV2.datetimerange",
                "datetimeV2.duration", "ordinal.relative", "wildcard"
            };

            foreach (var externlEntity in this._lucyModel.ExternalEntities)
            {
                entityDefinitions.Add(externlEntity);
            }

            HashSet<string> entityReferences = new HashSet<string>();
            foreach (var pattern in this.EntityPatterns)
            {
                entityDefinitions.Add(pattern.Name);
                foreach (var reference in pattern.PatternMatcher.GetEntityReferences())
                {
                    entityReferences.Add(reference);
                }
            }

            foreach (var pattern in this.WildcardEntityPatterns)
            {
                entityDefinitions.Add(pattern.Name);
                foreach (var reference in pattern.PatternMatcher.GetEntityReferences())
                {
                    entityReferences.Add(reference);
                }
            }

            foreach (var entityRef in entityReferences)
            {
                if (!entityDefinitions.Contains(entityRef))
                {
                    Warnings.Add($"WARNING: @{entityRef} does not exist.");
                }
            }
        }

        private class Occurence
        {
            public int Start { get; set; }
            public int End { get; set; }
            public string Value { get; set; }
        }

        private string ExpandMacros(string pattern)
        {
            using (var tokenStream = _simpleAnalyzer.GetTokenStream("name", pattern))
            {
                var termAtt = tokenStream.GetAttribute<ICharTermAttribute>();
                var offsetAtt = tokenStream.GetAttribute<IOffsetAttribute>();
                tokenStream.Reset();

                Stack<Occurence> occurences = new Stack<Occurence>();
                while (tokenStream.IncrementToken())
                {
                    var start = offsetAtt.StartOffset;
                    var end = offsetAtt.EndOffset;

                    // if a $ reference
                    if (start > 0 && pattern[start - 1] == '$')
                    {
                        start--;
                        var macroName = pattern.Substring(start, end - start);
                        if (this._lucyModel.Macros.TryGetValue(macroName, out string value))
                        {
                            occurences.Push(new Occurence() { Start = start, Value = value, End = end });
                        }
                        else
                        {
                            Warnings.Add($"WARNING: {macroName} is not defined.");
                        }
                    }
                }

                while (occurences.Count > 0)
                {
                    var occurence = occurences.Pop();
                    pattern = $"{pattern.Substring(0, occurence.Start)}{occurence.Value}{pattern.Substring(occurence.End)}";
                }
            }

            return pattern;
        }

        private void AddBuiltInEntities(MatchContext context, string text, string culture)
        {
            List<builtin.ModelResult> results = null;
            foreach (var name in BuiltinEntities)
            {
                switch (name)
                {
                    case "age":
                        results = builtin.NumberWithUnit.NumberWithUnitRecognizer.RecognizeAge(text, culture);
                        break;
                    case "boolean":
                        results = builtin.Choice.ChoiceRecognizer.RecognizeBoolean(text, culture); ;
                        break;
                    case "currency":
                        results = builtin.NumberWithUnit.NumberWithUnitRecognizer.RecognizeCurrency(text, culture);
                        break;
                    case "datetime":
                        results = builtin.DateTime.DateTimeRecognizer.RecognizeDateTime(text, culture);
                        break;
                    case "dimension":
                        results = builtin.NumberWithUnit.NumberWithUnitRecognizer.RecognizeDimension(text, culture);
                        break;
                    case "email":
                        results = builtin.Sequence.SequenceRecognizer.RecognizeEmail(text, culture);
                        break;
                    case "guid":
                        results = builtin.Sequence.SequenceRecognizer.RecognizeGUID(text, culture);
                        break;
                    case "hashtag":
                        results = builtin.Sequence.SequenceRecognizer.RecognizeHashtag(text, culture);
                        break;
                    case "ip":
                        results = builtin.Sequence.SequenceRecognizer.RecognizeIpAddress(text, culture);
                        break;
                    case "mention":
                        results = builtin.Sequence.SequenceRecognizer.RecognizeMention(text, culture);
                        break;
                    case "fraction":
                    case "decimal":
                    case "integer":
                    case "number":
                        results = builtin.Number.NumberRecognizer.RecognizeNumber(text, culture);
                        break;
                    case "numberrange":
                        results = builtin.Number.NumberRecognizer.RecognizeNumberRange(text, culture);
                        break;
                    case "ordinal":
                        results = builtin.Number.NumberRecognizer.RecognizeOrdinal(text, culture);
                        break;
                    case "percentage":
                        results = builtin.Number.NumberRecognizer.RecognizePercentage(text, culture);
                        break;
                    case "phonenumber":
                        results = builtin.Sequence.SequenceRecognizer.RecognizePhoneNumber(text, culture);
                        break;
                    case "quotedtext":
                        foreach (var quote in _quotedTextEntityRecognizer.Recognize(text, culture))
                        {
                            quote.Score = (float)(quote.End - quote.Start) / text.Length;
                            context.AddNewEntity(quote);
                        }
                        return;
                    case "temperature":
                        results = builtin.NumberWithUnit.NumberWithUnitRecognizer.RecognizeTemperature(text, culture); ;
                        break;
                    case "url":
                        results = builtin.Sequence.SequenceRecognizer.RecognizeURL(text, culture);
                        break;
                }

                foreach (var result in results)
                {
                    var type = result.TypeName;
                    if (type == "number")
                    {
                        var subType = (string)result.Resolution["subtype"];
                        if (builtinEntities.Contains(subType))
                        {
                            context.AddNewEntity(new LucyEntity()
                            {
                                Text = result.Text,
                                Type = subType,
                                Start = result.Start,
                                End = result.End + 1,
                                Resolution = result.Resolution["value"],
                                Score = ((float)(result.End + 1) - result.Start) / text.Length
                            });
                        }
                    }

                    context.AddNewEntity(new LucyEntity()
                    {
                        Text = result.Text,
                        Type = result.TypeName,
                        Start = result.Start,
                        End = result.End + 1,
                        Resolution = result.Resolution,
                        Score = ((float)(result.End + 1) - result.Start) / text.Length
                    });
                }
            }
        }

        private Analyzer GetAnalyzerForLocale(string locale = "en")
        {
            locale = locale.Split('-').First();
            switch (locale)
            {
                case "ar":
                    return new ArabicAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "hy":
                    return new ArmenianAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "eu":
                    return new BasqueAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "ca":
                    return new CatalanAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "cs":
                    return new CzechAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "da":
                    return new DanishAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "nl":
                    return new DutchAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "en":
                    return new EnglishAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "fi":
                    return new FinnishAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "fr":
                    return new FrenchAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "gl":
                    return new GalicianAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "de":
                    return new GermanAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "el":
                    return new GreekAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "hi":
                    return new HindiAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "hu":
                    return new HungarianAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "ch":
                case "ja":
                case "ko":
                    return new CJKAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "id":
                    return new IndonesianAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "ga":
                    return new IrishAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "it":
                    return new ItalianAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "lv":
                    return new LatvianAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "no":
                    return new NorwegianAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "fa":
                    return new PersianAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "pt":
                    return new PortugueseAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "ro":
                    return new RomanianAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "ru":
                    return new RussianAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "es":
                    return new SpanishAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "sv":
                    return new SwedishAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                case "tr":
                    return new TurkishAnalyzer(LuceneVersion.LUCENE_48, stopwords: CharArraySet.EMPTY_SET);
                default:
                    return new StandardAnalyzer(LuceneVersion.LUCENE_48, stopWords: CharArraySet.EMPTY_SET);
            }
        }
    }
}

