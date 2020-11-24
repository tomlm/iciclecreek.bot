using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy.PatternMatchers
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
            MatchResult matchResult = new MatchResult();

            bool found = false;
            do
            {
                found = false;
                foreach (var patternMatcher in PatternMatchers)
                {
                    var result = patternMatcher.Matches(context, start);
                    if (result.Matched)
                    {
                        found = true;
                        start = result.NextStart;

                        if (result.NextStart > matchResult.NextStart)
                        {
                            matchResult.NextStart = result.NextStart;
                        }

                        matchResult.Matched = true;
                        break;
                    }
                }

            } while (found);

            return matchResult;
        }

        public override bool IsWildcard() => (this.PatternMatchers.Count == 1 && this.PatternMatchers.Where(p => p.IsWildcard()).Any());

        public override string ToString() => $"OneOrMore({string.Join(",", PatternMatchers.Select(p => p.ToString()))})";
    }
}
