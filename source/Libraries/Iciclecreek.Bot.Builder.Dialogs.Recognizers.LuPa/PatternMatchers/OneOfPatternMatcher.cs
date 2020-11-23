using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa.PatternMatchers
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
            MatchResult matchResult = new MatchResult();

            foreach (var patternMatch in PatternMatchers)
            {
                var result = patternMatch.Matches(context, start);
                if (result.Matched)
                {
                    matchResult.Matched = true;
                }

                // keep longest token span which matches
                if (result.Matched && result.NextStart > matchResult.NextStart)
                {
                    matchResult.NextStart = result.NextStart;
                }
            }

            return matchResult;
        }

        public override string ToString() => $"OneOf({string.Join(",", PatternMatchers.Select(p => p.ToString()))})";

    }
}
