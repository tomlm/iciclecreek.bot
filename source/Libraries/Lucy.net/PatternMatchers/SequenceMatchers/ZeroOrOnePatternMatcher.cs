using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucy.PatternMatchers
{
    /// <summary>
    /// Matches ZeroOrOne (token|token|token)? ordinality
    /// </summary>
    class ZeroOrOnePatternMatcher : PatternMatcher
    {
        public ZeroOrOnePatternMatcher()
        {
        }

        public ZeroOrOnePatternMatcher(IEnumerable<PatternMatcher> patternMatchers)
        {
            PatternMatchers.AddRange(patternMatchers.OrderBy(p => p.IsWildcard()));
        }

        public List<PatternMatcher> PatternMatchers { get; set; } = new List<PatternMatcher>();

        /// <summary>
        /// Always returns true, but will find the largest span for the matchresult.start 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public override MatchResult Matches(MatchContext context, LucyEntity tokenEntity)
        {
            foreach (var patternMatch in PatternMatchers)
            {
                var matchResult = patternMatch.Matches(context, tokenEntity);
                if (matchResult.Matched)
                {
                    return matchResult;
                }
            }

            return new MatchResult()
            {
                Matched = true,
                NextToken = tokenEntity
            };
        }

        public override bool IsWildcard() => (this.PatternMatchers.Where(p => p.IsWildcard()).Any());

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

        public override string ToString() => $"ZeroOrOne({string.Join(",", PatternMatchers.Select(p => p.ToString()))})";
    }
}
