using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucy.PatternMatchers
{
    /// <summary>
    /// Matches OneOrMore (token)+ ordinality
    /// </summary>
    public class OneOrMorePatternMatcher : PatternMatcher
    {
        public OneOrMorePatternMatcher()
        {
        }

        public OneOrMorePatternMatcher(IEnumerable<PatternMatcher> patternMatchers)
        {
            PatternMatchers.AddRange(patternMatchers);
        }

        public List<PatternMatcher> PatternMatchers { get; set; } = new List<PatternMatcher>();

        /// <summary>
        /// Returns true if thre is 1 match, and always advacnces the start
        /// </summary>
        /// <param name="context"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public override MatchResult Matches(MatchContext context, int start)
        {
            foreach (var patternMatcher in PatternMatchers)
            {
                var matchResult = patternMatcher.Matches(context, start);
                if (matchResult.Matched)
                {
                    matchResult.Repeat = true;
                    return matchResult;
                }
            }

            return new MatchResult();
        }

        public override bool IsWildcard() => (this.PatternMatchers.Count == 1 && this.PatternMatchers.Where(p => p.IsWildcard()).Any());

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

        public override string ToString() => $"OneOrMore({string.Join(",", PatternMatchers.Select(p => p.ToString()))})";
    }
}
