using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Miscellaneous;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy.PatternMatchers
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
        /// <param name="context"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public override MatchResult Matches(MatchContext context, int start)
        {
            foreach (var patternMatcher in PatternMatchers)
            {
                var matchResult = patternMatcher.Matches(context, start);
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

        internal void ResolveFallbackMatchers()
        {
            if (this.PatternMatchers.Any(pm => pm.IsWildcard()))
            {
                List<PatternMatcher> newSequence = new List<PatternMatcher>();
                PatternMatcher wildcard = null;
                foreach (var pattern in PatternMatchers)
                {
                    if (pattern.IsWildcard())
                    {
                        wildcard = pattern;
                    }
                    else
                    {
                        if (wildcard != null)
                        {
                            newSequence.Add(new FallbackPatternMatcher(pattern, wildcard));
                            wildcard = null;
                        }
                        else
                        {
                            newSequence.Add(pattern);
                        }
                    }
                }

                if (wildcard != null)
                {
                    newSequence.Add(new FallbackPatternMatcher(wildcard, null));
                }

                this.PatternMatchers = newSequence;
            }
        }

        public override string ToString() => $"Sequence({string.Join(",", PatternMatchers.Select(p => p.ToString()))})";
    }
}
