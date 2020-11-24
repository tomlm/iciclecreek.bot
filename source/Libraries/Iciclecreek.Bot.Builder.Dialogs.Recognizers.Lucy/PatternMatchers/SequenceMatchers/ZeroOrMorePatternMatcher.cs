using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy.PatternMatchers
{
    /// <summary>
    /// Matches ZeroOrMore (token)* ordinality
    /// </summary>
    public class ZeroOrMorePatternMatcher : PatternMatcher
    {
        public ZeroOrMorePatternMatcher()
        {
        }

        public ZeroOrMorePatternMatcher(IEnumerable<PatternMatcher> patternMatchers)
        {
            PatternMatchers.AddRange(patternMatchers);
        }

        public List<PatternMatcher> PatternMatchers { get; set; } = new List<PatternMatcher>();

        /// <summary>
        /// Always returns true, but will advance start for each match.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public override MatchResult Matches(MatchContext context, int start)
        {
            MatchResult matchResult = new MatchResult()
            {
                Matched = true,
                NextStart = start
            };

            bool found = false;
            do
            {
                found = false;

                foreach (var patternMatcher in PatternMatchers)
                {
                    var result = patternMatcher.Matches(context, start);
                    if (result.Matched)
                    {
                        start = result.NextStart;
                        matchResult.NextStart = result.NextStart;
                        found = true;
                    }
                }
            }
            while (found);

            return matchResult;
        }

        public override bool IsWildcard() => (this.PatternMatchers.Count == 1 && this.PatternMatchers.Where(p => p.IsWildcard()).Any());

        public override string ToString() => $"ZeroOrMore({string.Join(",", PatternMatchers.Select(p => p.ToString()))})";
    }
}
