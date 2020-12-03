using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucy.PatternMatchers
{
    /// <summary>
    /// Matches ZeroOrMore (token)* ordinality
    /// </summary>
    public class ZeroOrMorePatternMatcher : PatternMatcher
    {
        public ZeroOrMorePatternMatcher()
        {
        }

        public ZeroOrMorePatternMatcher(IEnumerable<PatternMatcher> patternMatchers, byte maxTokens)
        {
            PatternMatchers.AddRange(patternMatchers.OrderBy(p => p.ContainsWildcard()));
            this.MaxTokens = maxTokens;
        }

        public List<PatternMatcher> PatternMatchers { get; set; } = new List<PatternMatcher>();

        /// <summary>
        /// Always returns true, but will advance start for each match.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public override MatchResult Matches(MatchContext context, LucyEntity tokenEntity)
        {
            if (tokenEntity != null)
            {
                foreach (var patternMatcher in PatternMatchers)
                {
                    var matchResult = patternMatcher.Matches(context, tokenEntity);
                    if (matchResult.Matched)
                    {
                        matchResult.Repeat = matchResult.NextToken != null;
                        return matchResult;
                    }
                }
            }

            return new MatchResult()
            {
                Matched = true,
                NextToken = tokenEntity
            };
        }

        public override bool ContainsWildcard() => (this.PatternMatchers.Where(p => p.ContainsWildcard()).Any());

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

        public override string ToString() => $"ZeroOrMore{(this.MaxTokens < 255 ? this.MaxTokens.ToString() : String.Empty)}({string.Join(",", PatternMatchers.Select(p => p.ToString()))})";

        public override IEnumerable<string> GenerateExamples(LucyEngine engine)
        {
            // yield a zero answer.
            yield return string.Empty;

            for (int numExamples = 0; numExamples < PatternMatchers.Count; numExamples++)
            {
                StringBuilder sb = new StringBuilder();
                // pick a random pattern
                var pm = PatternMatchers[rnd.Next(PatternMatchers.Count)];
                var examples = pm.GenerateExamples(engine).ToList();
                // pick a random example
                sb.Append($" {examples[rnd.Next(examples.Count)]}");
                yield return sb.ToString().Trim();
            }
        }

        public override string GenerateExample(LucyEngine engine)
        {
            if (rnd.Next(2) == 0)
            {
                return String.Empty;
            }

            StringBuilder sb = new StringBuilder();
            for (int numExamples = Math.Min(this.MaxTokens, (byte)5); numExamples > 0; numExamples--)
            {
                // pick a random pattern
                var pm = PatternMatchers[rnd.Next(PatternMatchers.Count)];

                // pick a random example
                sb.Append($" {pm.GenerateExample(engine)}");
            }
            return sb.ToString();
        }
    }
}
