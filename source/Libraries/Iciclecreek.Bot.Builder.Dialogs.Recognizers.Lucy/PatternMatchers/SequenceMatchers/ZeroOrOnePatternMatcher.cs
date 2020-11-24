using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy.PatternMatchers
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
            PatternMatchers.AddRange(patternMatchers);
        }

        public List<PatternMatcher> PatternMatchers { get; set; } = new List<PatternMatcher>();

        /// <summary>
        /// Always returns true, but will find the largest span for the matchresult.start 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public override MatchResult Matches(MatchContext context, int start)
        {
            foreach (var patternMatch in PatternMatchers)
            {
                var matchResult = patternMatch.Matches(context, start);
                if (matchResult.Matched)
                {
                    return matchResult;
                }
            }

            return new MatchResult()
            {
                Matched = true,
                NextStart = start
            };
        }

        public override bool IsWildcard() => (this.PatternMatchers.Count == 1 && this.PatternMatchers.Where(p => p.IsWildcard()).Any());

        public override string ToString() => $"ZeroOrOne({string.Join(",", PatternMatchers.Select(p => p.ToString()))})";
    }
}
