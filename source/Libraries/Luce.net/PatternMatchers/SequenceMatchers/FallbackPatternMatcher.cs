using System.Collections.Generic;

namespace Luce.PatternMatchers
{
    /// <summary>
    /// PatternMatcher which evaluates if the primary fails will evaluate the fallback matcher. 
    /// </summary>
    /// <remarks>
    /// This is used primarily for wildcardmatchers as the fallback
    /// </remarks>
    public class FallbackPatternMatcher : PatternMatcher
    {
        /// <summary>
        /// Evaluates a sequence of matchers
        /// </summary>
        public FallbackPatternMatcher()
        {
        }

        public FallbackPatternMatcher(PatternMatcher primaryMatcher, PatternMatcher fallbackMatcher)
        {
            this.PrimaryMatcher = primaryMatcher;
            this.FallbackMatcher = fallbackMatcher;
        }

        public PatternMatcher PrimaryMatcher { get; set; }

        public PatternMatcher FallbackMatcher { get; set; }


        /// <summary>
        /// If a matcher in the sequence doesn't match, then it doesn't match
        /// </summary>
        /// <param name="context"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public override MatchResult Matches(MatchContext context, int start)
        {
            var matchResult = PrimaryMatcher.Matches(context, start);
            // if it matched AND moved forward, then we want to fallback
            if (matchResult.Matched && matchResult.NextStart > start)
            {
                return matchResult;
            }

            if (this.FallbackMatcher != null)
            {
                matchResult = this.FallbackMatcher.Matches(context, start);
            }

            return matchResult;
        }

        public override IEnumerable<string> GetEntityReferences()
        {
            foreach(var dependency in this.PrimaryMatcher.GetEntityReferences())
            {
                yield return dependency;
            }

            if (this.FallbackMatcher != null)
            {
                foreach (var dependency in this.FallbackMatcher.GetEntityReferences())
                {
                    yield return dependency;
                }
            }
        }

        public override string ToString() => $"Fallback({PrimaryMatcher},{FallbackMatcher})";
    }
}
