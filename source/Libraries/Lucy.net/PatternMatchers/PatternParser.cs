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
        public PatternMatcher Parse(string pattern, bool defaultFuzzyMatch = false, Ordinality ordinality = Ordinality.One, int maxMatches = 16)
        {
            OrdinalityPatternMatcher ordinalityPatternMatcher = new OrdinalityPatternMatcher(ordinality, maxMatches);
            SequencePatternMatcher sequence = new SequencePatternMatcher();
            StringBuilder sb = new StringBuilder();
            var fuzzyMatch = defaultFuzzyMatch;
            var chars = pattern.GetEnumerator();
            while (chars.MoveNext())
            {
                char ch = chars.Current;
                bool repeatChar;
                do
                {
                    repeatChar = false;

                    switch (ch)
                    {
                        case '(':
                            Ordinality modifierOrdinality = Ordinality.One;
                            AddTextToSequence(sequence, sb, fuzzyMatch);

                            var subText = GetPatternGroup(chars).Trim();

                            bool inModifiers = true;
                            while (inModifiers && chars.MoveNext())
                            {
                                ch = chars.Current;
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
                                            maxMatches = num;
                                        }
                                        else
                                        {
                                            var patternMatcher = Parse(subText, fuzzyMatch, modifierOrdinality, maxMatches);
                                            sequence.PatternMatchers.Add(patternMatcher);

                                            // break out of modifier loop
                                            inModifiers = false;
                                            repeatChar = true;
                                        }
                                        break;
                                }
                            }
                            if (inModifiers)
                            {
                                // paren was end of string.
                                var patternMatcher = Parse(subText, fuzzyMatch, modifierOrdinality, maxMatches);
                                sequence.PatternMatchers.Add(patternMatcher);
                            }
                            maxMatches = 16;
                            fuzzyMatch = defaultFuzzyMatch;
                            break;

                        case '|':
                            {
                                AddTextToSequence(sequence, sb, fuzzyMatch);
                                if (sequence.PatternMatchers.Count == 1)
                                {
                                    ordinalityPatternMatcher.PatternMatchers.Add(sequence.PatternMatchers.Single());
                                }
                                else
                                {
                                    ordinalityPatternMatcher.PatternMatchers.Add(sequence);
                                }
                                sequence = new SequencePatternMatcher();
                            }
                            break;

                        default:
                            sb.Append(ch);
                            break;
                    }
                } while (repeatChar);
            }

            AddTextToSequence(sequence, sb, fuzzyMatch);

            if (sequence.PatternMatchers.Any())
            {
                if (sequence.PatternMatchers.Count == 1)
                {
                    ordinalityPatternMatcher.PatternMatchers.Add(sequence.PatternMatchers.Single());
                }
                else
                {
                    ordinalityPatternMatcher.PatternMatchers.Add(sequence);
                }
            }

            // if this is a oneOf, maxMatches with a single pattern matcher, just the inner patternmatcher.
            PatternMatcher result = ordinalityPatternMatcher;
            if (ordinalityPatternMatcher.PatternMatchers.Count == 1 && ordinalityPatternMatcher.Ordinality == Ordinality.One)
            {
                result = ordinalityPatternMatcher.PatternMatchers.Single();
            }

            // if it is a sequence with only one patternMatcher, just the inner patternmatcher
            if (result is SequencePatternMatcher spm && spm.PatternMatchers.Count == 1)
            {
                result = spm.PatternMatchers.Single();
            }

            return result;
        }

        private void AddTextToSequence(SequencePatternMatcher sequence, StringBuilder sb, bool fuzzyMatch)
        {
            var variation = sb.ToString().Trim();
            if (variation.Length > 0)
            {
                var patternMatcher = CreateTextPatternMatcher(variation, fuzzyMatch);
                sequence.PatternMatchers.Add(patternMatcher);
                sb.Clear();
            }
        }

        public string GetPatternGroup(CharEnumerator chars)
        {
            StringBuilder sb = new StringBuilder();
            int parenCount = 1;
            while (chars.MoveNext())
            {
                char ch = chars.Current;

                // we are in subparen content.
                switch (ch)
                {
                    case '(':
                        parenCount++;
                        break;

                    case ')':
                        parenCount--;
                        if (parenCount == 0)
                        {
                            return sb.ToString();
                        }
                        break;
                }
                sb.Append(ch);
            }
            throw new ArgumentOutOfRangeException("Missing matching paren");
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
                            // handle datetime.subrange
                            if (end < text.Length && text[end] == '.')
                            {
                                tokenStream.IncrementToken();
                                end = offsetAtt.EndOffset;
                                tokenText = text.Substring(start, end - start);
                            }
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

            if (sequence.PatternMatchers.Count == 1)
            {
                return sequence.PatternMatchers.Single();
            }

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
