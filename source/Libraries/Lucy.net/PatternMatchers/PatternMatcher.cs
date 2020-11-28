using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Lucy.PatternMatchers.Matchers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;

namespace Lucy.PatternMatchers
{
    public abstract class PatternMatcher
    {
        /// <summary>
        /// Match and add entity as appropriate.
        /// </summary>
        /// <param name="matchContext">match context.</param>
        /// <param name="tokenEntity">token entity</param>
        /// <returns>matchresult</returns>
        public abstract MatchResult Matches(MatchContext matchContext, LucyEntity tokenEntity);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern">pattern to parse</param>
        /// <param name="exactAnalyzer">exact analyzer to use</param>
        /// <param name="fuzzyAnalyzer">fuzzy analyzer to use</param>
        /// <param name="defaultFuzzyMatch">if true changes default for text token to fuzzyMatch</param>
        /// <returns></returns>
        public static PatternMatcher Parse(string pattern, Analyzer exactAnalyzer, Analyzer fuzzyAnalyzer, bool defaultFuzzyMatch = false)
        {
            SequencePatternMatcher sequence = new SequencePatternMatcher();
            bool inVariations = false;
            bool inModifiers = false;
            bool modifierFuzzyMatch = defaultFuzzyMatch;
            Ordinality modifierOrdinality = Ordinality.One;
            List<string> variations = new List<string>();
            StringBuilder sb = new StringBuilder();
            foreach (char ch in pattern)
            {
                if (!inVariations)
                {
                    switch (ch)
                    {
                        case '(':
                            if (sb.Length > 0)
                            {
                                AddPatternMatchersForText(sequence.PatternMatchers, sb.ToString().Trim(), defaultFuzzyMatch, exactAnalyzer, fuzzyAnalyzer);
                                sb.Clear();
                            }

                            inVariations = true;
                            inModifiers = false;
                            modifierOrdinality = Ordinality.One;
                            modifierFuzzyMatch = defaultFuzzyMatch;
                            variations.Clear();
                            break;

                        default:
                            sb.Append(ch);
                            break;
                    }
                }
                else
                {
                    if (inModifiers == false)
                    {
                        switch (ch)
                        {
                            case '|':
                                variations.Add(sb.ToString());
                                sb.Clear();
                                break;

                            case ')':
                                if (sb.Length > 0)
                                {
                                    variations.Add(sb.ToString());
                                }
                                sb.Clear();
                                inModifiers = true;
                                break;

                            default:
                                sb.Append(ch);
                                break;
                        }
                    }
                    else if (inModifiers)
                    {
                        switch (ch)
                        {
                            case '~':
                                modifierFuzzyMatch = !defaultFuzzyMatch;
                                break;

                            case '?':
                                modifierOrdinality = Ordinality.ZeroOrOne;
                                break;

                            case '+':
                                modifierOrdinality = Ordinality.OneOrMore;
                                break;

                            case '*':
                                modifierOrdinality = Ordinality.ZeroOrMore;
                                break;

                            default:
                                if (variations.Any())
                                {
                                    AddVariations(exactAnalyzer, fuzzyAnalyzer, sequence, modifierFuzzyMatch, modifierOrdinality, variations);
                                    inVariations = false;
                                    inModifiers = false;
                                    modifierOrdinality = Ordinality.One;
                                    variations.Clear();
                                    sb.Clear();
                                }
                                break;
                        }
                    }
                }
            }

            if (inVariations)
            {
                if (inModifiers && variations.Any())
                {
                    AddVariations(exactAnalyzer, fuzzyAnalyzer, sequence, modifierFuzzyMatch, modifierOrdinality, variations);
                }
                else
                {
                    throw new Exception("Closing paren not found!");
                }
            }

            if (sb.Length > 0)
            {
                AddPatternMatchersForText(sequence.PatternMatchers, sb.ToString().Trim(), defaultFuzzyMatch, exactAnalyzer, fuzzyAnalyzer);
            }

            if (sequence.PatternMatchers.Count == 0)
            {
                return null;
            }

            if (sequence.PatternMatchers.Count == 1)
            {
                // Trace.TraceInformation($"{pattern}:\n\t{sequence.PatternMatchers.Single()}");
                return sequence.PatternMatchers.Single();
            }

            // Trace.TraceInformation($"{pattern}:\n\t{sequence}");
            sequence.ResolveFallbackMatchers();
            return sequence;
        }

        public virtual bool IsWildcard()
        {
            return false;
        }

        public virtual IEnumerable<string> GetEntityReferences()
        {
            yield break;
        }

        private static void AddPatternMatchersForText(List<PatternMatcher> patternMatchers, string text, bool defaultFuzzyMatch, Analyzer exactAnalyzer, Analyzer fuzzyAnalyzer)
        {
            if (!String.IsNullOrEmpty(text))
            {
                var patternMatcher = CreateTextPatternMatcher(text, exactAnalyzer, fuzzyAnalyzer, defaultFuzzyMatch);
                if (patternMatcher != null)
                {
                    patternMatchers.Add(patternMatcher);
                }
            }
        }

        private static void AddVariations(Analyzer exactAnalyzer, Analyzer fuzzyAnalyzer, SequencePatternMatcher sequence, bool modifierFuzzyMatch, Ordinality modifierOrdinality, List<string> variations)
        {
            var patternMatchers = CreateVariationsPatternMatchers(variations, exactAnalyzer, fuzzyAnalyzer, modifierFuzzyMatch);

            switch (modifierOrdinality)
            {
                case Ordinality.ZeroOrOne:
                    sequence.PatternMatchers.Add(new ZeroOrOnePatternMatcher(patternMatchers));
                    break;
                case Ordinality.ZeroOrMore:
                    sequence.PatternMatchers.Add(new ZeroOrMorePatternMatcher(patternMatchers));
                    break;
                case Ordinality.One:
                    if (patternMatchers.Count == 1)
                    {
                        sequence.PatternMatchers.Add(patternMatchers.Single());
                    }
                    else
                    {
                        sequence.PatternMatchers.Add(new OneOfPatternMatcher(patternMatchers));
                    }
                    break;
                case Ordinality.OneOrMore:
                    sequence.PatternMatchers.Add(new OneOrMorePatternMatcher(patternMatchers));
                    break;
            }
        }

        private static List<PatternMatcher> CreateVariationsPatternMatchers(IEnumerable<string> variations, Analyzer exactAnalyzer, Analyzer fuzzyAnalyzer, bool fuzzy = false)
        {
            var patternMatchers = new List<PatternMatcher>();
            foreach (var variation in variations.Select(variation => variation.Trim()))
            {
                AddPatternMatchersForText(patternMatchers, variation.Trim(), fuzzy, exactAnalyzer, fuzzyAnalyzer);
            }
            return patternMatchers;
        }

        private const string NAMEDWILDCARD = ":" + WildcardPatternMatcher.ENTITYTYPE;
        private const string NAMEDWILDCARD_TOKEN = "_" + WildcardPatternMatcher.ENTITYTYPE;

        private static PatternMatcher CreateTextPatternMatcher(string text, Analyzer exactAnalyzer, Analyzer fuzzyAnalyzer, bool fuzzyMatch)
        {
            // massage wildcards text so it survives tokenization "foo:___" => foo_wildcard
            text = text.Replace("___", WildcardPatternMatcher.ENTITYTYPE);
            text = text.Replace(NAMEDWILDCARD, NAMEDWILDCARD_TOKEN);

            var sequence = new SequencePatternMatcher();
            using (TextReader reader = new StringReader(text))
            {
                using (var tokenStream = exactAnalyzer.GetTokenStream("name", reader))
                {
                    var termAtt = tokenStream.GetAttribute<ICharTermAttribute>();
                    var offsetAtt = tokenStream.GetAttribute<IOffsetAttribute>();
                    tokenStream.Reset();

                    while (tokenStream.IncrementToken())
                    {
                        string token = termAtt.ToString();
                        var start = offsetAtt.StartOffset;
                        var end = offsetAtt.EndOffset;
                        string sourceToken = text.Substring(start, end - start);

                        if (start > 0 && text[start - 1] == '@')
                        {
                            sequence.PatternMatchers.Add(new EntityPatternMatcher(sourceToken));
                        }
                        else if (token.EndsWith(WildcardPatternMatcher.ENTITYTYPE))
                        {
                            var sb = new StringBuilder(token);
                            if (token.EndsWith(NAMEDWILDCARD_TOKEN))
                                sb[token.LastIndexOf('_')] = ':';
                            sequence.PatternMatchers.Add(new WildcardPatternMatcher(sb.ToString()));
                        }
                        else
                        {

                            TokenPatternMatcher tokenPatternMatcher = new TokenPatternMatcher(text, token);
                            if (fuzzyMatch)
                            {
                                AddFuzzyMatchTokens(tokenPatternMatcher, text, fuzzyAnalyzer);
                            }
                            sequence.PatternMatchers.Add(tokenPatternMatcher);
                        }
                    }
                }
            }

            if (sequence.PatternMatchers.Count == 0)
            {
                return null;
            }

            if (sequence.PatternMatchers.Count == 1)
            {
                return sequence.PatternMatchers.Single();
            }

            sequence.ResolveFallbackMatchers();
            return sequence;
        }

        private static void AddFuzzyMatchTokens(TokenPatternMatcher tokenPatternMatcher, string text, Analyzer fuzzyAnalyzer)
        {
            using (var tokenStream = fuzzyAnalyzer.GetTokenStream("name", text))
            {
                var termAtt = tokenStream.GetAttribute<ICharTermAttribute>();
                tokenStream.Reset();
                while (tokenStream.IncrementToken())
                {
                    string token = termAtt.ToString();
                    tokenPatternMatcher.FuzzyTokens.Add(token);
                }
            }
        }

    }
}
