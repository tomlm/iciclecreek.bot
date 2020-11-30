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

        public OneOrMorePatternMatcher(IEnumerable<PatternMatcher> patternMatchers, byte maxTokens)
        {
            PatternMatchers.AddRange(patternMatchers.OrderBy(p => p.ContainsWildcard()));
            this.MaxTokens = maxTokens;
        }

        public List<PatternMatcher> PatternMatchers { get; set; } = new List<PatternMatcher>();

        /// <summary>
        /// Repeats as long as there is a match and we haven't run out of tokens.
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

            return new MatchResult();
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

        public override string ToString() => $"OneOrMore{this.MaxTokens}({string.Join(",", PatternMatchers.Select(p => p.ToString()))})";
    }
}
