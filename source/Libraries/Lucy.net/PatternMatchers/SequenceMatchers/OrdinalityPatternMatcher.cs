using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucy.PatternMatchers
{
    /// <summary>
    /// Matches ZeroOrMore (token)* ordinality
    /// </summary>
    public class OrdinalityPatternMatcher : PatternMatcher
    {
        public OrdinalityPatternMatcher()
        {
        }

        public OrdinalityPatternMatcher(Ordinality ordinality, int maxMatches = Byte.MaxValue)
        {
            this.MaxMatches = maxMatches;
            this.Ordinality = ordinality;
        }

        public OrdinalityPatternMatcher(Ordinality ordinality, IEnumerable<PatternMatcher> patternMatchers, int maxMatches = Byte.MaxValue)
        {
            this.MaxMatches = maxMatches;
            this.Ordinality = ordinality;
            PatternMatchers.AddRange(patternMatchers.OrderBy(p => p.ContainsWildcard()));
        }

        public int MaxMatches { get; set; }

        public Ordinality Ordinality { get; set; } = Ordinality.One;

        public List<PatternMatcher> PatternMatchers { get; set; } = new List<PatternMatcher>();

        /// <summary>
        /// If a matcher in the sequence doesn't match, then it doesn't match
        /// </summary>
        /// <param name="context"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public override MatchResult Matches(MatchContext context, TokenEntity startToken, PatternMatcher nextPatternMatcher)
        {
            var tokenEntity = startToken;
            int start = startToken?.Start ?? 0;
            int end = 0;
            int minMatches = 1;
            int maxMatches = this.MaxMatches;
            switch (Ordinality)
            {
                case Ordinality.One:
                    maxMatches = 1;
                    break;
                case Ordinality.OneOrMore:
                    break;
                case Ordinality.ZeroOrMore:
                    minMatches = 0;
                    break;
                case Ordinality.ZeroOrOne:
                    minMatches = 0;
                    maxMatches = 1;
                    break;
            }

            int matched = 0;
            MatchResult matchResult = null;
            bool found;
            do
            {
                found = false;
                for (int iPattern = 0; iPattern < PatternMatchers.Count; iPattern++)
                {
                    var patternMatcher = PatternMatchers[iPattern];
                    var result = patternMatcher.Matches(context, tokenEntity, nextPatternMatcher);

                    if (result.Matched)
                    {
                        found = true;
                        matched++;
                        matchResult = result;
                        end = Math.Max(result.End, end);
                        tokenEntity = matchResult.NextToken;
                        if (matched == maxMatches)
                        {
                            // then we are done;
                            return new MatchResult(true, this, tokenEntity, start, end)
                            {
                                NextPatternMatch = result.NextPatternMatch
                            };
                        }
                        break;
                    }
                    else
                    {
                        if (matchResult != null)
                        {
                            matchResult.NextPatternMatch = result.NextPatternMatch;
                        }
                    }
                }
            } while (found && tokenEntity != null);

            if (matched < minMatches)
            {
                // not matched.
                return new MatchResult(false, this)
                {
                    NextPatternMatch = matchResult?.NextPatternMatch
                };
            }

            return new MatchResult(true, this, tokenEntity, start, end)
            {
                NextPatternMatch = matchResult?.NextPatternMatch
            };
        }

        public override bool ContainsWildcard() => (this.PatternMatchers.Where(p => p.ContainsWildcard()).Any());

        public override IEnumerable<string> GetEntityReferences()
        {
            foreach (var patternMatcher in this.PatternMatchers)
            {
                foreach (var dependency in patternMatcher.GetEntityReferences())
                {
                    yield return dependency;
                }
            }
        }

        public override string ToString() => $"{Ordinality}{(this.MaxMatches < 16 ? this.MaxMatches.ToString() : String.Empty)}({string.Join(",", PatternMatchers.Select(p => p.ToString()))})";

        public override IEnumerable<string> GenerateExamples(LucyEngine engine)
        {
            // yield a zero answer.
            yield return string.Empty;

        }

        public override string GenerateExample(LucyEngine engine)
        {
            return String.Empty;
        }
    }
}
