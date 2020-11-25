using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Luce.PatternMatchers
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
            foreach (var patternMatcher in PatternMatchers)
            {
                var matchResult = patternMatcher.Matches(context, start);
                if (matchResult.Matched)
                {
                    matchResult.Repeat = true;
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

        public override IEnumerable<string> GetEntityTypeDependencies()
        {
            foreach (var patternMatcher in this.PatternMatchers)
            {
                foreach (var dependency in patternMatcher.GetEntityTypeDependencies())
                {
                    yield return dependency;
                }
            }
        }

        public override string ToString() => $"ZeroOrMore({string.Join(",", PatternMatchers.Select(p => p.ToString()))})";
    }
}
