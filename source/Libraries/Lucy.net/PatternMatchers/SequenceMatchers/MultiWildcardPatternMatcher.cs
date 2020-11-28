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
            this.EntityMatcher = entityMatcher;
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
            MatchResult matchResult = new MatchResult();
            if (tokenEntity != null)
            {
                if (EntityMatcher != null)
                {
                    matchResult = EntityMatcher.Matches(context, tokenEntity);

                    // if it matched AND moved forward, then we are done
                    if (matchResult.Matched)
                    {
                        if (matchResult.Matched && matchResult.NextToken != tokenEntity)
                        {
                            return matchResult;
                        }
                    }
                }

                if (this.WildcardMatcher != null)
                {
                    matchResult = this.WildcardMatcher.Matches(context, tokenEntity);
                }
            }

            return matchResult;
        }

        public override IEnumerable<string> GetEntityReferences()
        {
            if (this.EntityMatcher != null)
            {
                foreach (var dependency in this.EntityMatcher.GetEntityReferences())
                {
                    yield return dependency;
                }
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
