using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy.PatternMatchers.Matchers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy.PatternMatchers
{
    public abstract class PatternMatcher
    {
        /// <summary>
        /// See if matcher is true or not
        /// </summary>
        /// <param name="matchContext">match context.</param>
        /// <param name="start">start index</param>
        /// <returns>-1 if not match, else new start index</returns>
        public abstract MatchResult Matches(MatchContext matchContext, int start);

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
            pattern = pattern.Replace("___", $"@{WildcardPatternMatcher.ENTITYTYPE}");
            bool inVariations = false;
            bool inModifiers = false;
            bool modifierFuzzyMatch = false;
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
                            modifierFuzzyMatch = false;
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
                                    FinishVariations(exactAnalyzer, fuzzyAnalyzer, sequence, modifierFuzzyMatch, modifierOrdinality, variations);
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
                    FinishVariations(exactAnalyzer, fuzzyAnalyzer, sequence, modifierFuzzyMatch, modifierOrdinality, variations);
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
                //Trace.TraceInformation($"{pattern}:\n\t{sequence.PatternMatchers.Single()}");
                return sequence.PatternMatchers.Single();
            }
            //Trace.TraceInformation($"{pattern}:\n\t{sequence}");
            return sequence;
        }

        private static void AddPatternMatchersForText(List<PatternMatcher> patternMatchers, string text, bool defaultFuzzyMatch, Analyzer exactAnalyzer, Analyzer fuzzyAnalyzer)
        {
            if (!String.IsNullOrEmpty(text))
            {
                if (defaultFuzzyMatch)
                {
                    var patternMatcher = CreateFuzzyTextPatternMatcher(text, fuzzyAnalyzer);
                    if (patternMatcher != null)
                    {
                        patternMatchers.Add(patternMatcher);
                    }
                }
                else
                {
                    var patternMatcher = CreateTextPatternMatcher(text, exactAnalyzer);
                    if (patternMatcher != null)
                    {
                        patternMatchers.Add(patternMatcher);
                    }
                }
            }
        }

        private static void FinishVariations(Analyzer exactAnalyzer, Analyzer fuzzyAnalyzer, SequencePatternMatcher sequence, bool modifierFuzzyMatch, Ordinality modifierOrdinality, List<string> variations)
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
                if (variation.FirstOrDefault() == '@')
                {
                    if (variation == $"@{WildcardPatternMatcher.ENTITYTYPE}")
                    {
                        patternMatchers.Add(new WildcardPatternMatcher());
                    }
                    else
                    {
                        patternMatchers.Add(new EntityPatternMatcher(variation));
                    }
                }
                else
                {
                    AddPatternMatchersForText(patternMatchers, variation.Trim(), fuzzy, exactAnalyzer, fuzzyAnalyzer);
                }
            }
            return patternMatchers;
        }

        private static PatternMatcher CreateTextPatternMatcher(string text, Analyzer analyzer)
        {
            var sequence = new SequencePatternMatcher();
            using (TextReader reader = new StringReader(text))
            {
                using (var tokenStream = analyzer.GetTokenStream("name", reader))
                {
                    var termAtt = tokenStream.GetAttribute<ICharTermAttribute>();
                    var offsetAtt = tokenStream.GetAttribute<IOffsetAttribute>();
                    tokenStream.Reset();

                    while (tokenStream.IncrementToken())
                    {
                        string token = termAtt.ToString();

                        if (offsetAtt.StartOffset > 0 && text[offsetAtt.StartOffset - 1] == '@')
                        {
                            if (token == WildcardPatternMatcher.ENTITYTYPE)
                            {
                                sequence.PatternMatchers.Add(new WildcardPatternMatcher());
                            }
                            else
                            {
                                sequence.PatternMatchers.Add(new EntityPatternMatcher(token));
                            }
                        }
                        else
                        {
                            sequence.PatternMatchers.Add(new TokenPatternMatcher(token));
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

            return sequence;
        }

        // NOTE: FuzzyAnalyzer can return multiple possible tokens for the same segment, so we collect them into One() clauses
        // Sequence(One(x1,x2,x3),One(y1,y2,y3))
        private static PatternMatcher CreateFuzzyTextPatternMatcher(string text, Analyzer analyzer)
        {
            var sequence = new SequencePatternMatcher();
            OneOfPatternMatcher oneOf = null;
            var start = -1;
            using (TextReader reader = new StringReader(text))
            {
                using (var tokenStream = analyzer.GetTokenStream("name", reader))
                {
                    var termAtt = tokenStream.GetAttribute<ICharTermAttribute>();
                    var offsetAtt = tokenStream.GetAttribute<IOffsetAttribute>();
                    tokenStream.Reset();

                    while (tokenStream.IncrementToken())
                    {
                        string token = termAtt.ToString();
                        var offset = offsetAtt.StartOffset;
                        if (start != offset)
                        {
                            start = offset;
                            if (oneOf != null)
                            {
                                sequence.PatternMatchers.Add(oneOf);
                            }
                            oneOf = new OneOfPatternMatcher();
                        }

                        oneOf.PatternMatchers.Add(new FuzzyTokenPatternMatcher(token));
                    }

                    if (oneOf != null && oneOf.PatternMatchers.Any())
                    {
                        if (oneOf.PatternMatchers.Count == 1)
                        {
                            sequence.PatternMatchers.Add(oneOf.PatternMatchers.Single());
                        }
                        else
                        {
                            sequence.PatternMatchers.Add(oneOf);
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
            return sequence;
        }

    }
}
