using System.Collections.Generic;

namespace Lucy.PatternMatchers
{
    /// <summary>
    /// PatternMatcher which evaluates if the primary fails will evaluate the fallback matcher. 
    /// </summary>
    /// <remarks>
    /// This is used primarily for wildcardmatchers as the fallback
    /// </remarks>
    public class MultiWildcardPatternMatcher : PatternMatcher
    {
        /// <summary>
        /// Evaluates a sequence of matchers
        /// </summary>
        public MultiWildcardPatternMatcher()
        {
        }

        public MultiWildcardPatternMatcher(PatternMatcher wildcardMatcher, PatternMatcher entityMatcher = null)
        {
            this.WildcardMatcher = wildcardMatcher;
            this.EntityMatcher = entityMatcher ?? new AnyEntityPatternMatcher();
        }

        public PatternMatcher EntityMatcher { get; set; }

        public PatternMatcher WildcardMatcher { get; set; }


        /// <summary>
        /// If a matcher in the sequence doesn't match, then it doesn't match
        /// </summary>
        /// <param name="context"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public override MatchResult Matches(MatchContext context, int start)
        {
            var matchResult = EntityMatcher.Matches(context, start);
            // if it matched AND moved forward, then we want to fallback
            if (matchResult.Matched && matchResult.NextStart > start)
            {
                // not matched, we are done.
                return new MatchResult();
            }

            if (this.WildcardMatcher != null)
            {
                matchResult = this.WildcardMatcher.Matches(context, start);
            }

            return matchResult;
        }

        public override IEnumerable<string> GetEntityReferences()
        {
            foreach (var dependency in this.EntityMatcher.GetEntityReferences())
            {
                yield return dependency;
            }

            if (this.WildcardMatcher != null)
            {
                foreach (var dependency in this.WildcardMatcher.GetEntityReferences())
                {
                    yield return dependency;
                }
            }
        }

        public override string ToString() => $"MultiWildcard({WildcardMatcher}, {EntityMatcher})";
    }
}
