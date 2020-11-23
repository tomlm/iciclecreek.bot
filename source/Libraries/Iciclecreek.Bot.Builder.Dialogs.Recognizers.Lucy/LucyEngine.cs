using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Atn;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy.PatternMatchers;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy.PatternMatchers.Matchers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.TokenAttributes;
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

        public LucyEngine(Analyzer exactAnalyzer, Analyzer fuzzyAnalyzer)
        {
            this._lupaModel = new LucyModel();
            this._exactAnalyzer = exactAnalyzer;
            this._fuzzyAnalyzer = fuzzyAnalyzer;

            LoadModel();
        }

        public LucyEngine(LucyModel model, Analyzer exactAnalyzer, Analyzer fuzzyAnalyzer)
        {
            this._lupaModel = model;
            this._exactAnalyzer = exactAnalyzer;
            this._fuzzyAnalyzer = fuzzyAnalyzer;

            LoadModel();
        }

        public List<EntityPattern> EntityPatterns { get; set; } = new List<EntityPattern>();

        public bool IncludeInternalEntites { get; set; }

        public IEnumerable<LucyEntity> MatchEntities(string text, IEnumerable<LucyEntity> externalEntities = null)
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
                                            .Where(entity => entity.Type == TokenPatternMatcher.ENTITYTYPE)
                                            .OrderBy(entity => entity.Start))
                {
                    // foreach entity pattern
                    foreach (var entityPattern in EntityPatterns)
                    {
                        // see if it matches at this textEntity starting position.
                        var matchResult = entityPattern.PatternMatcher.Matches(context, textEntity.Start);

                        // if it matches
                        if (matchResult.Matched)
                        {
                            // add it to the entities.
                            context.Entities.Add(new LucyEntity()
                            {
                                Type = entityPattern.Name,
                                Resolution = entityPattern.Resolution,
                                Start = textEntity.Start,
                                End = matchResult.NextStart
                            });
                        }
                    }
                }

                if (count == context.Entities.Count)
                {
                    // add in wildcard tokens

                    // get all tokenentities
                    foreach (var tokenEntity in context.Entities
                            .Where(entity => entity.Type == TokenPatternMatcher.ENTITYTYPE)
                            .OrderBy(entity => entity.Start).ToList())
                    {
                        // if there are no entities for a token
                        if (!context.Entities.Any(entity => entity.Type != TokenPatternMatcher.ENTITYTYPE &&
                                                            entity.Type != FuzzyTokenPatternMatcher.ENTITYTYPE &&
                                                            entity.Start == tokenEntity.Start))
                        {
                            // then make it a wildcard entity
                            context.Entities.Add(new LucyEntity()
                            {
                                Type = WildcardPatternMatcher.ENTITYTYPE,
                                Resolution = tokenEntity.Text,
                                Start = tokenEntity.Start,
                                Text = tokenEntity.Text,
                                End = tokenEntity.End
                            });
                        }
                    }
                }
            } while (count != context.Entities.Count);

            // filter out internal entities
            if (IncludeInternalEntites)
            {
                return context.Entities;
            }

            return context.Entities.Where(entity => entity.Type != TokenPatternMatcher.ENTITYTYPE &&
                                                    entity.Type != FuzzyTokenPatternMatcher.ENTITYTYPE &&
                                                    entity.Type != WildcardPatternMatcher.ENTITYTYPE).ToList();
        }

        /// <summary>
        /// Format entities as fixed width string.
        /// </summary>
        /// <param name="text">original text</param>
        /// <param name="entities">entities</param>
        /// <returns></returns>
        public static string FormatResults(string text, IEnumerable<LucyEntity> entities)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(text);
            foreach (var grp in entities.GroupBy(entity => entity.Type))
            {
                sb.AppendLine(FormatEntities(text, grp));
            }

            return sb.ToString();
        }

        private static string FormatEntities(string text, IEnumerable<LucyEntity> entities)
        {
            StringBuilder sb = new StringBuilder();
            int last = 0;
            foreach (var entity in entities)
            {
                sb.Append(new String(' ', entity.Start - last));
                if (entity.End == entity.Start + 1)
                {
                    sb.Append("^");
                }
                else
                {
                    sb.Append($"^{new String('_', entity.End - entity.Start - 2)}^");
                }
                last = entity.End;
            }
            sb.Append($"{new String(' ', text.Length - last)} {entities.First().Type}");
            return sb.ToString();
        }

        private void LoadModel()
        {
            foreach (var entityModel in _lupaModel.Entities)
            {
                foreach (var patternModel in entityModel.Patterns)
                {
                    var resolution = patternModel.IsNormalized() ? patternModel.First() : entityModel.Name;
                    foreach (var pattern in patternModel)
                    {
                        var expandedPattern = ExpandMacros(pattern);
                        var patternMatcher = PatternMatcher.Parse(expandedPattern, this._exactAnalyzer, this._fuzzyAnalyzer, entityModel.FuzzyMatch);
                        EntityPatterns.Add(new EntityPattern(entityModel.Name, resolution, patternMatcher));
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
    }
}
