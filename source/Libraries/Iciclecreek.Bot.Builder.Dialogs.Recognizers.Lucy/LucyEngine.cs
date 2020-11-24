using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy.PatternMatchers;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy.PatternMatchers.Matchers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Phonetic;
using Lucene.Net.Analysis.Phonetic.Language.Bm;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;
using NuGet.Packaging;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy
{
    /// <summary>
    /// LucyEngine uses a LucyModel to do LU pattern matching for entities.
    /// </summary>
    public class LucyEngine
    {
        private LucyModel _lupaModel;
        private Analyzer _exactAnalyzer;
        private Analyzer _fuzzyAnalyzer;

        public LucyEngine(LucyModel model, Analyzer exactAnalyzer = null, Analyzer fuzzyAnalyzer = null)
        {
            this._lupaModel = model;

            this._exactAnalyzer = exactAnalyzer ??
                new StandardAnalyzer(LuceneVersion.LUCENE_48, stopWords: CharArraySet.UnmodifiableSet(new CharArraySet(LuceneVersion.LUCENE_48, Array.Empty<string>(), false)));

            this._fuzzyAnalyzer = fuzzyAnalyzer ??
                Analyzer.NewAnonymous((field, textReader) =>
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

            LoadModel();
        }

        /// <summary>
        /// Patterns to match
        /// </summary>
        public List<EntityPattern> EntityPatterns { get; set; } = new List<EntityPattern>();

        /// <summary>
        /// Wildcard Patterns to match
        /// </summary>
        public List<EntityPattern> WildcardEntityPatterns { get; set; } = new List<EntityPattern>();

        /// <summary>
        /// Match entities in given text
        /// </summary>
        /// <param name="text">text to match against.</param>
        /// <param name="externalEntities">externally provided entities</param>
        /// <param name="includeInternal">include tokens in results</param>
        /// <returns>entities</returns>
        public IEnumerable<LucyEntity> MatchEntities(string text, IEnumerable<LucyEntity> externalEntities = null, bool includeInternal = false)
        {
            var context = new MatchContext()
            {
                Text = text
            };

            if (externalEntities != null)
            {
                context.Entities.AddRange(externalEntities);
            }

            // add all  @Token and @FuzzyToken entities
            context.Entities.AddRange(Tokenize(text));

            int count = 0;
            do
            {
                count = context.Entities.Count;

                // foreach text token
                foreach (var textEntity in context.Entities
                                            .Where(entity => String.Equals(entity.Type, TokenPatternMatcher.ENTITYTYPE, StringComparison.OrdinalIgnoreCase))
                                            .OrderBy(entity => entity.Start))
                {
                    // foreach entity pattern
                    foreach (var entityPattern in EntityPatterns)
                    {
                        ProcessEntityPattern(context, textEntity, entityPattern);
                    }
                }

                if (count == context.Entities.Count && WildcardEntityPatterns.Any())
                {
                    // process wildcard patterns
                    foreach (var textEntity in context.Entities
                                                .Where(entity => String.Equals(entity.Type, TokenPatternMatcher.ENTITYTYPE, StringComparison.OrdinalIgnoreCase))
                                                .OrderBy(entity => entity.Start))
                    {
                        foreach (var entityPattern in WildcardEntityPatterns)
                        {
                            ProcessEntityPattern(context, textEntity, entityPattern);
                        }
                    }
                }
            } while (count != context.Entities.Count);

            // filter out internal entities
            if (includeInternal)
            {
                return context.Entities;
            }

            // merge entities which are overlapping.
            var mergedEntities = new HashSet<LucyEntity>(new EntityTokenComparer());
            foreach (var entity1 in context.Entities.Where(entity => entity.Type[0] != '^'))
            {
                var alternateEntities = context.Entities.Where(e => e.Type == entity1.Type && e != entity1 && !mergedEntities.Contains(entity1)).ToList();
                if (alternateEntities.Count() == 0)
                {
                    mergedEntities.Add(entity1);
                }
                else
                {
                    // if no alterantes say "don't keep it" then we add it
                    if (!alternateEntities.Any(entity2 => ShouldDropEntity(entity1, entity2)))
                    {
                        mergedEntities.Add(entity1);
                    }
                }
            }

            return mergedEntities;
        }

        public IEnumerable<LucyEntity> Tokenize(string text)
        {
            var tokens = new List<Token>();

            using (TextReader reader = new StringReader(text))
            {
                using (var tokenStream = _exactAnalyzer.GetTokenStream("name", reader))
                {
                    var termAtt = tokenStream.GetAttribute<ICharTermAttribute>();
                    var offsetAtt = tokenStream.GetAttribute<IOffsetAttribute>();
                    tokenStream.Reset();

                    while (tokenStream.IncrementToken())
                    {
                        string token = termAtt.ToString();
                        bool skipFuzzy = false;
                        if (offsetAtt.StartOffset > 0 && text[offsetAtt.StartOffset - 1] == '@')
                        {
                            token = text.Substring(offsetAtt.StartOffset - 1, offsetAtt.EndOffset - offsetAtt.StartOffset + 1);
                            skipFuzzy = true;
                        }
                        else if (offsetAtt.StartOffset > 0 && text[offsetAtt.StartOffset - 1] == '$')
                        {
                            token = text.Substring(offsetAtt.StartOffset - 1, offsetAtt.EndOffset - offsetAtt.StartOffset + 1);
                            skipFuzzy = true;
                        }

                        yield return new LucyEntity()
                        {
                            Type = TokenPatternMatcher.ENTITYTYPE,
                            Text = token,
                            Start = offsetAtt.StartOffset,
                            End = offsetAtt.EndOffset
                        };

                        if (_fuzzyAnalyzer != null && !skipFuzzy)
                        {
                            using (TextReader fuzzyReader = new StringReader(token))
                            {
                                // get fuzzyText
                                using (var fuzzyTokenStream = _fuzzyAnalyzer.GetTokenStream("name", fuzzyReader))
                                {
                                    var fuzzyTermAtt = fuzzyTokenStream.GetAttribute<ICharTermAttribute>();
                                    fuzzyTokenStream.Reset();
                                    fuzzyTokenStream.IncrementToken();
                                    yield return new LucyEntity()
                                    {
                                        Type = FuzzyTokenPatternMatcher.ENTITYTYPE,
                                        Text = fuzzyTermAtt.ToString(),
                                        Start = offsetAtt.StartOffset,
                                        End = offsetAtt.EndOffset
                                    };
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Format entities as fixed width string.
        /// </summary>
        /// <param name="text">original text</param>
        /// <param name="entities">entities</param>
        /// <returns></returns>
        public static string VisualizeResultsAsSpans(string text, IEnumerable<LucyEntity> entities)
        {
            if (!entities.Any())
            {
                return "No entities found";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(text);
            foreach (var grp in entities.GroupBy(entity => entity.Type.ToLower())
                                                .OrderBy(grp => grp.Max(v => v.End - v.Start))
                                                .ThenBy(grp => grp.Min(v => v.Start)))
            {
                sb.Append(FormatEntityLine(text, grp));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Format entities as fixed width string.
        /// </summary>
        /// <param name="text">original text</param>
        /// <param name="entities">entities</param>
        /// <returns></returns>
        public static string VizualizeResultsAsHierarchy(string text, IEnumerable<LucyEntity> entities)
        {
            if (!entities.Any())
            {
                return "No entities found";
            }

            StringBuilder sb = new StringBuilder();

            foreach (var entity in entities)
            {
                sb.AppendLine(FormatEntityChildren(string.Empty, entity));
            }

            return sb.ToString();
        }

        private static string FormatEntityChildren(string prefix, LucyEntity entity)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{prefix} @{entity}");

            if (String.IsNullOrEmpty(prefix))
            {
                prefix = "=> ";
            }

            foreach (var child in entity.Children)
            {
                sb.Append(FormatEntityChildren("    " + prefix, child));
            }
            return sb.ToString();
        }

        private static string FormatEntityLine(string text, IEnumerable<LucyEntity> entities)
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

        private void ProcessEntityPattern(MatchContext context, LucyEntity textEntity, EntityPattern entityPattern)
        {
            context.EntityPattern = entityPattern;
            context.CurrentEntity = new LucyEntity()
            {
                Type = entityPattern.Name,
                Resolution = entityPattern.Resolution,
                Start = textEntity.Start
            };

            // see if it matches at this textEntity starting position.
            var matchResult = entityPattern.PatternMatcher.Matches(context, textEntity.Start);
            //Trace.TraceInformation($"[{textEntity.Start}] {context.EntityPattern} => {matchResult.Matched}");

            // if it matches
            if (matchResult.Matched && matchResult.NextStart != textEntity.Start)
            {
                // add it to the entities.
                context.CurrentEntity.End = matchResult.NextStart;
                if (context.CurrentEntity.Resolution == null && !context.CurrentEntity.Children.Any())
                {
                    context.CurrentEntity.Resolution = context.Text.Substring(context.CurrentEntity.Start, context.CurrentEntity.End - context.CurrentEntity.Start);
                }

                if (!context.Entities.Contains(context.CurrentEntity))
                {
                    context.Entities.Add(context.CurrentEntity);
                    // Trace.TraceInformation($"\n [{textEntity.Start}] {context.EntityPattern} => {matchResult.Matched} {context.CurrentEntity}");
                }

                foreach (var childEntity in context.CurrentEntity.Children)
                {
                    if (!context.Entities.Contains(childEntity))
                    {
                        context.Entities.Add(childEntity);
                    }
                }
            }
        }

        private void LoadModel()
        {
            foreach (var entityModel in _lupaModel.Entities)
            {
                foreach (var patternModel in entityModel.Patterns)
                {
                    var resolution = entityModel.Patterns.Any(p => p.IsNormalized()) ? patternModel.First() : null;
                    foreach (var pattern in patternModel)
                    {
                        var expandedPattern = ExpandMacros(pattern);
                        var patternMatcher = PatternMatcher.Parse(expandedPattern, this._exactAnalyzer, this._fuzzyAnalyzer, entityModel.FuzzyMatch);
                        if (patternMatcher != null)
                        {
                            // Trace.TraceInformation($"{expandedPattern} => {patternMatcher}");
                            if (expandedPattern.Contains("___"))
                            {
                                // we want to process wildcard patterns last
                                WildcardEntityPatterns.Add(new EntityPattern(entityModel.Name, resolution, patternMatcher));
                            }
                            else
                            {
                                EntityPatterns.Add(new EntityPattern(entityModel.Name, resolution, patternMatcher));
                            }
                        }
                    }
                }
            }
        }

        private string ExpandMacros(string pattern)
        {
            var tokens = Tokenize(pattern);

            foreach (var token in tokens.Where(t => t.Text.FirstOrDefault() == '$').OrderByDescending(t => t.Start))
            {
                if (_lupaModel.Macros.TryGetValue(token.Text, out string value))
                {
                    pattern = $"{pattern.Substring(0, token.Start)}{value}{pattern.Substring(token.End)}";
                }
            }
            return pattern;
        }

        private bool ShouldDropEntity(LucyEntity entity1, LucyEntity entity2)
        {
            // if entity2 is bigger on both ends
            if (entity2.Start < entity1.Start && entity2.End > entity1.End)
            {
                return true;
            }

            // if it's inside the current token
            if (entity2.Start >= entity1.Start && entity2.End <= entity1.End)
            {
                return false;
            }
            // if offset overlapping at start or end
            if ((entity2.Start <= entity1.Start && entity2.End >= entity1.Start && entity2.End <= entity1.End) ||
                (entity2.Start >= entity1.Start && entity2.Start < entity1.End && entity2.End >= entity1.End))
            {
                var entity1Length = entity1.End - entity1.Start;
                var entity2Length = entity2.End - entity2.Start;
                if (entity1Length > entity2Length)
                {
                    return false;
                }
                else if (entity2Length > entity1Length)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
