using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucy.PatternMatchers
{
    /// <summary>
    /// Matches One (token|token|token) ordinality
    /// </summary>
    class OneOfPatternMatcher : PatternMatcher
    {
        public OneOfPatternMatcher()
        {
        }

        public OneOfPatternMatcher(IEnumerable<PatternMatcher> patternMatchers)
        {
            PatternMatchers.AddRange(patternMatchers.OrderBy(p => p.IsWildcard()));
        }

        public List<PatternMatcher> PatternMatchers { get; set; } = new List<PatternMatcher>();

        /// <summary>
        /// Run all of the pattern matchers, as soon as first one triggers we match
        /// </summary>
        /// <param name="context"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public override MatchResult Matches(MatchContext context, LucyEntity tokenEntity)
        {
            foreach (var patternMatch in PatternMatchers)
            {
                var result = patternMatch.Matches(context, tokenEntity);
                if (result.Matched)
                {
                    return result;
                }
            }

            return new MatchResult();
        }

        public override bool IsWildcard() => (this.PatternMatchers.Where(p => p.IsWildcard()).Any());

        public override IEnumerable<string> GetEntityReferences()
        {
            foreach(var patternMatcher in this.PatternMatchers)
            {
                foreach (var dependency in patternMatcher.GetEntityReferences())
                {
                    yield return dependency;
                }
            }
        }

        public override string ToString() => $"OneOf({string.Join(",", PatternMatchers.Select(p => p.ToString()))})";

    }
}
