using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa.PatternMatchers;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa.PatternMatchers.Matchers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Util;
using NuGet.Packaging;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa
{
    /// <summary>
    /// LupaEngine uses a LupaModel to do LU pattern matching for entities.
    /// </summary>
    public class LupaEngine
    {
        private LupaModel _lupaModel;
        private Analyzer _exactAnalyzer;
        private Analyzer _fuzzyAnalyzer;

        public LupaEngine(Analyzer exactAnalyzer, Analyzer fuzzyAnalyzer)
        {
            this._lupaModel = new LupaModel();
            this._exactAnalyzer = exactAnalyzer;
            this._fuzzyAnalyzer = fuzzyAnalyzer;

            LoadModel();
        }

        public LupaEngine(LupaModel model, Analyzer exactAnalyzer, Analyzer fuzzyAnalyzer)
        {
            this._lupaModel = model;
            this._exactAnalyzer = exactAnalyzer;
            this._fuzzyAnalyzer = fuzzyAnalyzer;

            LoadModel();
        }

        public List<EntityPattern> EntityPatterns { get; set; } = new List<EntityPattern>();

        public IEnumerable<LupaEntity> MatchEntities(string text, IEnumerable<LupaEntity> externalEntities = null)
        {
            HashSet<LupaEntity> entities = new HashSet<LupaEntity>();

            var matchContext = new MatchContext()
            {
                Text = text
            };

            if (externalEntities != null)
            {
                matchContext.Entities.AddRange(externalEntities);
            }

            // add all  @Text and @FuzzyText entities
            matchContext.Entities.AddRange(Tokenize(text));

            // foreach text token
            foreach (var textEntity in matchContext.Entities
                                        .Where(entity => entity.Type == TextPatternMatcher.ENTITYTYPE)
                                        .OrderBy(entity => entity.Start))
            {
                // foreach entity pattern
                foreach (var entityPattern in EntityPatterns)
                {
                    // see if it matches at this textEntity starting position.
                    var matchResult = entityPattern.PatternMatcher.Matches(matchContext, textEntity.Start);

                    // if it matches
                    if (matchResult.Matched)
                    {
                        // add it to the entities.
                        matchContext.Entities.Add(new LupaEntity()
                        {
                            Type = entityPattern.Name,
                            Resolution = entityPattern.Resolution,
                            Start = textEntity.Start,
                            End = matchResult.NextStart
                        });
                    }
                }
            }

            return matchContext.Entities;
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

        public IEnumerable<LupaEntity> Tokenize(string text)
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

                        yield return new LupaEntity()
                        {
                            Type = TextPatternMatcher.ENTITYTYPE,
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
                                    yield return new LupaEntity()
                                    {
                                        Type = FuzzyTextPatternMatcher.ENTITYTYPE,
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
