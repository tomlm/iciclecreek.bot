using System;
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

        /// <summary>
        /// Lookahead matcher...when this matches the wildcard matcher is done.
        /// </summary>
        public PatternMatcher EntityMatcher { get; set; }

        /// <summary>
        /// Wildcard matcher to process
        /// </summary>
        public PatternMatcher WildcardMatcher { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public override MatchResult Matches(MatchContext context, LucyEntity tokenEntity)
        {
            var matchResult = EntityMatcher.Matches(context, tokenEntity);

            // if it matched AND moved forward, then we are done
            if (matchResult.Matched && matchResult.NextToken != tokenEntity)
            {
                // NOTE: The entity matcher is a look ahead.  When it matches we return "not found" to terminate the wildcard matching.
                // If any results have been found with the wildcard matching, then token will advance to this matcher again, at which
                // point it will be processed normally
                return new MatchResult();
            }

            if (this.WildcardMatcher != null)
            {
                matchResult = this.WildcardMatcher.Matches(context, tokenEntity);
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
