using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa;
using Lucene.Net.Analysis;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa.PatternMatchers
{
    /// <summary>
    /// PatternMatcher which evaluates a sequence of PatterMatchers
    /// </summary>
    public class SequencePatternMatcher : PatternMatcher
    {
        /// <summary>
        /// Evaluates a sequence of matchers
        /// </summary>
        public SequencePatternMatcher()
        {
        }

        public SequencePatternMatcher(IEnumerable<PatternMatcher> patternMatchers)
        {
            this.PatternMatchers.AddRange(patternMatchers);
        }

        public List<PatternMatcher> PatternMatchers { get; set; } = new List<PatternMatcher>();

        /// <summary>
        /// If a matcher in the sequence doesn't match, then it doesn't match
        /// </summary>
        /// <param name="utterance"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public override MatchResult Matches(MatchContext utterance, int start)
        {
            foreach (var patternMatcher in PatternMatchers)
            {
                var matchResult = patternMatcher.Matches(utterance, start);
                if (!matchResult.Matched)
                {
                    return matchResult;
                }
                start = matchResult.NextStart;
            }

            return new MatchResult()
            {
                Matched = true,
                NextStart = start
            };
        }

        public override string ToString() => $"Sequence({string.Join(",", PatternMatchers.Select(p => p.ToString()))})";
    }
}
