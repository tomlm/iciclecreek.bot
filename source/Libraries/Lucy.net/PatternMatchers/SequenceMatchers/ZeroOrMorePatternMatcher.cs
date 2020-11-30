using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucy.PatternMatchers
{
    /// <summary>
    /// Matches ZeroOrMore (token)* ordinality
    /// </summary>
    public class ZeroOrMorePatternMatcher : PatternMatcher
    {
        public ZeroOrMorePatternMatcher()
        {
        }

        public ZeroOrMorePatternMatcher(IEnumerable<PatternMatcher> patternMatchers, byte maxTokens)
        {
            PatternMatchers.AddRange(patternMatchers.OrderBy(p => p.ContainsWildcard()));
            this.MaxTokens = maxTokens;
        }

        public List<PatternMatcher> PatternMatchers { get; set; } = new List<PatternMatcher>();

        /// <summary>
        /// Always returns true, but will advance start for each match.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public override MatchResult Matches(MatchContext context, LucyEntity tokenEntity)
        {
            foreach (var patternMatcher in PatternMatchers)
            {
                var matchResult = patternMatcher.Matches(context, tokenEntity);
                if (matchResult.Matched)
                {
                    matchResult.Repeat = matchResult.NextToken != null;
                    return matchResult;
                }
            }

            return new MatchResult()
            {
                Matched = true,
                NextToken = tokenEntity
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

        public override string ToString() => $"ZeroOrMore{(this.MaxTokens < 255 ? this.MaxTokens.ToString(): String.Empty)}({string.Join(",", PatternMatchers.Select(p => p.ToString()))})";
    }
}
