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
    internal class ParseContext
    {
        private Analyzer exactAnalyzer;
        private Analyzer fuzzyAnalyzer;
        private SequencePatternMatcher sequence = new SequencePatternMatcher();
        private bool defaultFuzzyMatch = false;
        private List<string> variations = null;
        private StringBuilder sb = new StringBuilder();
        private Stack<ParseContext> parseScopes = new Stack<ParseContext>();
        private bool modifierFuzzyMatch = false;

        internal ParseContext(Analyzer exactAnalyzer, Analyzer fuzzyAnalyzer, bool defaultFuzzyMatch)
        {
            this.exactAnalyzer = exactAnalyzer;
            this.fuzzyAnalyzer = fuzzyAnalyzer;
            this.defaultFuzzyMatch = defaultFuzzyMatch;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern">pattern to parse</param>
        /// <returns></returns>
        internal PatternMatcher Parse(string pattern)
        {
            bool inVariations = false;
            bool inModifiers = false;
            var modifierOrdinality = Ordinality.One;
            modifierFuzzyMatch = this.defaultFuzzyMatch;
            StringBuilder sb = new StringBuilder();
            foreach (char ch in pattern)
            {
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
                                    AddPatternMatchersForText(sequence.PatternMatchers, sb.ToString().Trim());
                                    sb.Clear();
                                }

                                variations = new List<string>();
                                inVariations = true;
                                inModifiers = false;
                                modifierOrdinality = Ordinality.One;
                                modifierFuzzyMatch = defaultFuzzyMatch;
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
                                        AddVariations(modifierOrdinality);
                                        inVariations = false;
                                        inModifiers = false;
                                        modifierOrdinality = Ordinality.One;
                                        variations.Clear();
                                        sb.Clear();

                                        // we need to reprocess this char with new state.
                                        repeatChar = true;
                                    }
                                    break;
                            }
                        }
                    }
                } while (repeatChar);
            }

            if (inVariations)
            {
                if (inModifiers && variations.Any())
                {
                    AddVariations(modifierOrdinality);
                }
                else
                {
                    throw new Exception("Closing paren not found!");
                }
            }

            if (sb.Length > 0)
            {
                AddPatternMatchersForText(sequence.PatternMatchers, sb.ToString().Trim());
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

        public virtual bool ContainsWildcard()
        {
            return false;
        }

        public virtual IEnumerable<string> GetEntityReferences()
        {
            yield break;
        }

        private void AddPatternMatchersForText(List<PatternMatcher> patternMatchers, string text)
        {
            if (!String.IsNullOrEmpty(text))
            {
                var patternMatcher = CreateTextPatternMatcher(text, this.modifierFuzzyMatch);
                if (patternMatcher != null)
                {
                    patternMatchers.Add(patternMatcher);
                }
            }
        }

        private void AddVariations(Ordinality modifierOrdinality)
        {
            var patternMatchers = new List<PatternMatcher>();
            foreach (var variation in variations.Select(variation => variation.Trim()))
            {
                AddPatternMatchersForText(patternMatchers, variation.Trim());
            }

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

            if (sequence.PatternMatchers.Count == 1)
            {
                return sequence.PatternMatchers.Single();
            }

            sequence.ResolveFallbackMatchers();
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
