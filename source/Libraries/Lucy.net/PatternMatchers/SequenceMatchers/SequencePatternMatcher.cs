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
            for (int iPattern = 0; iPattern < PatternMatchers.Count; iPattern++)
            {
                MatchResult matchResult = null;
                var patternMatcher = PatternMatchers[iPattern];
                var maxTokens = patternMatcher.MaxTokens;
                do
                {
                    if (patternMatcher.ContainsWildcard())
                    {
                        // look ahead to non wild card
                        var endPatternMatcher = PatternMatchers.Skip(iPattern).Where(pm => !pm.ContainsWildcard()).FirstOrDefault();
                        if (endPatternMatcher != null)
                        {
                            matchResult = endPatternMatcher.Matches(context, tokenEntity);
                            // if it matched AND moved forward, then we are done
                            if (matchResult.Matched)
                            {
                                if (matchResult.Matched && matchResult.NextToken != tokenEntity)
                                {
                                    // skip forward to endPatternMatcher and run again...
                                    iPattern = PatternMatchers.IndexOf(endPatternMatcher) - 1;
                                    continue;
                                }
                            }
                        }
                    }

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
                } while (matchResult.Repeat && --maxTokens > 0);
            }

            return new MatchResult()
            {
                Matched = true,
                End = end,
                NextToken = tokenEntity
            };
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
                    foreach(var previousExample in examples)
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
