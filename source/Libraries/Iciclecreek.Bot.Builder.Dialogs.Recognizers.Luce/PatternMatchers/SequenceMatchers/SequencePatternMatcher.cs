using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Luce;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Miscellaneous;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Luce.PatternMatchers
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
            // try to match each element in the sequence.
            foreach (var patternMatcher in PatternMatchers)
            {
                MatchResult matchResult = null;
                do
                {
                    var result = patternMatcher.Matches(context, start);
                    // if the element did not match, then sequence is bad, return failure
                    if (!result.Matched)
                    {
                        // unless we already have a result from this pattern matcher, then we can continue on.
                        if (matchResult != null)
                        {
                            break;
                        }
                        return result;
                    }
                    matchResult = result;
                    start = matchResult.NextStart;
                } while (matchResult.Repeat);
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

        public override string ToString() => $"Sequence({string.Join(",", PatternMatchers.Select(p => p.ToString()))})";
    }
}
