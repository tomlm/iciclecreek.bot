using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;
using Lucy.PatternMatchers.Matchers;

namespace Lucy.PatternMatchers
{
    public class PatternParser
    {
        private Analyzer exactAnalyzer;
        private Analyzer fuzzyAnalyzer;

        internal PatternParser(Analyzer exactAnalyzer, Analyzer fuzzyAnalyzer)
        {
            this.exactAnalyzer = exactAnalyzer;
            this.fuzzyAnalyzer = fuzzyAnalyzer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern">pattern to parse</param>
        /// <returns></returns>
        public PatternMatcher Parse(string pattern, bool defaultFuzzyMatch = false)
        {
            SequencePatternMatcher sequence = new SequencePatternMatcher();
            List<string> variations = null;
            StringBuilder sb = new StringBuilder();
            var inVariations = false;
            bool inModifiers = false;
            byte maxTokens = byte.MaxValue;
            var modifierOrdinality = Ordinality.One;
            var fuzzyMatch = defaultFuzzyMatch;
            var chars = pattern.GetEnumerator();
            while (chars.MoveNext())
            {
                char ch = chars.Current;
                bool repeatChar = false;
                do
                {
                    repeatChar = false;
                    if (!inVariations)
                    {
                        switch (ch)
                        {
                            case '(':
                                if (sb.Length > 0)
                                {
                                    AddPatternMatchersForText(sequence.PatternMatchers, sb.ToString().Trim(), fuzzyMatch);
                                    sb.Clear();
                                }

                                variations = new List<string>();
                                maxTokens = byte.MaxValue;
                                inVariations = true;
                                inModifiers = false;
                                modifierOrdinality = Ordinality.One;
                                fuzzyMatch = defaultFuzzyMatch;
                                break;

                            default:
                                sb.Append(ch);
                                break;
                        }
                    }
                    else
                    {
                        if (!inModifiers)
                        {
                            // we are parsing paren content.
                            switch (ch)
                            {
                                case '(':
                                    var subText = GetSubParens(chars, out char lastChar).Trim();
                                    if (!String.IsNullOrEmpty(subText))
                                    {
                                        variations.Add(subText);
                                    }
                                    ch = lastChar;
                                    repeatChar = true;
                                    break;
                                case '|':
                                    if (sb.Length > 0)
                                    {
                                        variations.Add(sb.ToString());
                                    }
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
                                    fuzzyMatch = !defaultFuzzyMatch;
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
                                    if (byte.TryParse(ch.ToString(), out byte num))
                                    {
                                        maxTokens = num;
                                    }
                                    else if (variations.Any())
                                    {
                                        AddVariations(sequence, variations, modifierOrdinality, fuzzyMatch, maxTokens);
                                        // we need to reprocess this char with new state.
                                        variations.Clear();
                                        sb.Clear();
                                        inVariations = false;
                                        inModifiers = false;
                                        modifierOrdinality = Ordinality.One;
                                        repeatChar = true;
                                    }
                                    break;
                            }
                            break;
                        }
                    }
                } while (repeatChar);
            }

            if (inVariations)
            {
                if (inModifiers && variations.Any())
                {
                    AddVariations(sequence, variations, modifierOrdinality, fuzzyMatch, maxTokens);
                }
                else
                {
                    throw new Exception("Closing paren not found!");
                }
            }

            if (sb.Length > 0)
            {
                AddPatternMatchersForText(sequence.PatternMatchers, sb.ToString().Trim(), fuzzyMatch);
            }

            if (sequence.PatternMatchers.Count == 0)
            {
                return null;
            }

            //if (sequence.PatternMatchers.Count == 1)
            //{
            //    // Trace.TraceInformation($"{pattern}:\n\t{sequence.PatternMatchers.Single()}");
            //    return sequence.PatternMatchers.Single();
            //}

            // Trace.TraceInformation($"{pattern}:\n\t{sequence}");
            sequence.FixupWildcardPatterns();
            return sequence;
        }

        public string GetSubParens(CharEnumerator chars, out char ch)
        {
            ch = ' ';
            bool inModifiers = false;
            StringBuilder sb = new StringBuilder("(");
            while (chars.MoveNext())
            {
                ch = chars.Current;
                if (!inModifiers)
                {

                    // we are in subparen content.
                    switch (ch)
                    {
                        case ')':
                            inModifiers = true;
                            sb.Append(ch);
                            break;
                        default:
                            sb.Append(ch);
                            break;
                    }
                }
                else
                {
                    switch (ch)
                    {
                        case '~':
                        case '?':
                        case '*':
                        case '+':
                            sb.Append(ch);
                            break;
                        default:
                            return sb.ToString();
                    }
                }
            }
            return sb.ToString();
        }

        private void AddPatternMatchersForText(List<PatternMatcher> patternMatchers, string text, bool fuzzyMatch)
        {
            if (!String.IsNullOrEmpty(text))
            {
                var patternMatcher = CreateTextPatternMatcher(text, fuzzyMatch);
                if (patternMatcher != null)
                {
                    patternMatchers.Add(patternMatcher);
                }
            }
        }

        private void AddVariations(SequencePatternMatcher sequence, List<string> variations, Ordinality modifierOrdinality, bool fuzzyMatch, byte maxTokens)
        {
            switch (modifierOrdinality)
            {
                case Ordinality.ZeroOrOne:
                    sequence.PatternMatchers.Add(new ZeroOrOnePatternMatcher(variations.OrderByDescending(v => v.Length).Select(v => Parse(v, fuzzyMatch))));
                    break;
                case Ordinality.ZeroOrMore:
                    sequence.PatternMatchers.Add(new ZeroOrMorePatternMatcher(variations.OrderByDescending(v => v.Length).Select(v => Parse(v, fuzzyMatch)), maxTokens: maxTokens));
                    break;
                case Ordinality.One:
                    if (variations.Count == 1)
                    {
                        sequence.PatternMatchers.Add(variations.Select(v => Parse(v, fuzzyMatch)).Single());
                    }
                    else
                    {
                        sequence.PatternMatchers.Add(new OneOfPatternMatcher(variations.OrderByDescending(v => v.Length).Select(v => Parse(v, fuzzyMatch))));
                    }
                    break;
                case Ordinality.OneOrMore:
                    sequence.PatternMatchers.Add(new OneOrMorePatternMatcher(variations.OrderByDescending(v => v.Length).Select(v => Parse(v, fuzzyMatch)), maxTokens: maxTokens));
                    break;
            }
        }

        private const string NAMEDWILDCARD = ":" + WildcardPatternMatcher.ENTITYTYPE;
        private const string NAMEDWILDCARD_TOKEN = "_" + WildcardPatternMatcher.ENTITYTYPE;

        private PatternMatcher CreateTextPatternMatcher(string text, bool fuzzyMatch)
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
                        string tokenText = text.Substring(start, end - start);

                        if (start > 0 && text[start - 1] == '@')
                        {
                            sequence.PatternMatchers.Add(new EntityPatternMatcher(tokenText));
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
                            TokenPatternMatcher tokenPatternMatcher = new TokenPatternMatcher(tokenText, token);
                            if (fuzzyMatch)
                            {
                                AddFuzzyMatchTokens(tokenPatternMatcher, text);
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

            //if (sequence.PatternMatchers.Count == 1)
            //{
            //    return sequence.PatternMatchers.Single();
            //}

            sequence.FixupWildcardPatterns();
            return sequence;
        }

        private void AddFuzzyMatchTokens(TokenPatternMatcher tokenPatternMatcher, string text)
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
