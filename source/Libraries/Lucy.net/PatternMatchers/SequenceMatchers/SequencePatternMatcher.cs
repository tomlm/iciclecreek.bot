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
        public override MatchResult Matches(MatchContext context, TokenEntity startToken, PatternMatcher nextPatternMatcher)
        {
            var tokenEntity = startToken;
            // try to match each element in the sequence.
            int start = startToken?.Start ?? 0;
            int end = 0;
            for (int iPattern = 0; iPattern < PatternMatchers.Count; iPattern++)
            {
                var matchResult = new MatchResult(false, this, tokenEntity);
                var patternMatcher = PatternMatchers[iPattern];
                if (patternMatcher.ContainsWildcard())
                {
                    // look ahead to non wild card
                    nextPatternMatcher = PatternMatchers.Skip(iPattern).Where(pm => !pm.ContainsWildcard()).FirstOrDefault();

                    // run wildcard pattern matcher
                    matchResult = patternMatcher.Matches(context, tokenEntity, nextPatternMatcher);

                    // if the match was not the wildcard pattern, then advance to that.
                    if (matchResult.NextPatternMatch != null)
                    {
                        Debug.Assert(matchResult.NextPatternMatch.PatternMatcher == nextPatternMatcher);
                        matchResult = matchResult.NextPatternMatch;
                        iPattern = PatternMatchers.IndexOf(matchResult.PatternMatcher);
                        Debug.Assert(iPattern >= 0);
                    }
                }
                else
                {
                    matchResult = patternMatcher.Matches(context, tokenEntity, nextPatternMatcher);
                }

                // if the element did not match, then sequence is bad, return failure
                if (matchResult.Matched == false)
                {
                    return new MatchResult(false, this, tokenEntity);
                }

                tokenEntity = matchResult.NextToken;
                end = Math.Max(matchResult.End, end);
            }

            return new MatchResult(true, this, tokenEntity, start, end);
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

        public override IEnumerable<string> GenerateExamples(LucyEngine engine)
        {
            List<string> examples = new List<string>()
            {
                String.Empty
            };
            foreach (var pm in PatternMatchers)
            {
                List<string> newExamples = new List<string>();

                foreach (var example in pm.GenerateExamples(engine))
                {
                    foreach (var previousExample in examples)
                    {
                        newExamples.Add($"{previousExample} {example}".Trim());
                    }
                }
                examples = newExamples.Distinct().ToList();
            }
            return examples;
        }

        public override string GenerateExample(LucyEngine engine)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var patternMatcher in this.PatternMatchers)
            {
                sb.Append($" {patternMatcher.GenerateExample(engine)}");
            }
            return sb.ToString().Trim();
        }
    }
}
