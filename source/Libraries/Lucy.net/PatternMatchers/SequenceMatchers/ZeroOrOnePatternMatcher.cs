using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucy.PatternMatchers
{
    /// <summary>
    /// Matches ZeroOrOne (token|token|token)? ordinality
    /// </summary>
    class ZeroOrOnePatternMatcher : PatternMatcher
    {
        public ZeroOrOnePatternMatcher()
        {
        }

        public ZeroOrOnePatternMatcher(IEnumerable<PatternMatcher> patternMatchers)
        {
            PatternMatchers.AddRange(patternMatchers.OrderBy(p => p.ContainsWildcard()));
        }

        public List<PatternMatcher> PatternMatchers { get; set; } = new List<PatternMatcher>();

        /// <summary>
        /// Always returns true, but will find the largest span for the matchresult.start 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public override MatchResult Matches(MatchContext context, LucyEntity tokenEntity)
        {
            if (tokenEntity != null)
            {
                foreach (var patternMatch in PatternMatchers)
                {
                    var matchResult = patternMatch.Matches(context, tokenEntity);
                    if (matchResult.Matched)
                    {
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

        public override string ToString() => $"ZeroOrOne({string.Join(",", PatternMatchers.Select(p => p.ToString()))})";

        public override IEnumerable<string> GenerateExamples(LucyEngine engine)
        {
            yield return String.Empty;

            foreach (var pm in PatternMatchers)
            {
                foreach (var example in pm.GenerateExamples(engine))
                {
                    yield return example.Trim();
                }
            }
        }

        public override string GenerateExample(LucyEngine engine)
        {
            if (rnd.Next(2) == 0)
                return String.Empty;
            return PatternMatchers[rnd.Next(PatternMatchers.Count)].GenerateExample(engine);
        }
    }
}
