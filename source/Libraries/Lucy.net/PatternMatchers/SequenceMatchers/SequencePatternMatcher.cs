using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Lucy;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Miscellaneous;

namespace Lucy.PatternMatchers
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
        public override MatchResult Matches(MatchContext context, LucyEntity tokenEntity)
        {
            // try to match each element in the sequence.
            int end = 0;
            foreach (var patternMatcher in PatternMatchers)
            {
                MatchResult matchResult = null;
                do
                {
                    var result = patternMatcher.Matches(context, tokenEntity);
                    // if the element did not match, then sequence is bad, return failure
                    if (result.Matched == false)
                    {
                        // unless we already have a result from this pattern matcher, then we can continue on.
                        if (matchResult != null)
                        {
                            break;
                        }
                        return result;
                    }
                    matchResult = result;
                    tokenEntity = matchResult.NextToken;
                    end = Math.Max(result.End, end);
                } while (matchResult.Repeat);
            }

            return new MatchResult()
            {
                Matched = true,
                End = end,
                NextToken = tokenEntity
            };
        }

        internal void ResolveFallbackMatchers()
        {
            if (this.PatternMatchers.Any(pm => pm.ContainsWildcard()))
            {
                List<PatternMatcher> newSequence = new List<PatternMatcher>();
                PatternMatcher wildcard = null;
                foreach (var pattern in PatternMatchers)
                {
                    if (pattern.ContainsWildcard() && !(pattern is WildcardPatternMatcher))
                    {
                        wildcard = pattern;
                    }
                    else
                    {
                        if (wildcard != null)
                        {
                            newSequence.Add(new MultiWildcardPatternMatcher(wildcard, pattern));
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
                    newSequence.Add(new MultiWildcardPatternMatcher(wildcard));
                }

                this.PatternMatchers = newSequence;
            }
        }

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

        public override bool ContainsWildcard() => (this.PatternMatchers.Where(p => p.ContainsWildcard()).Any());

        public override string ToString() => $"Sequence({string.Join(",", PatternMatchers.Select(p => p.ToString()))})";
    }
}
