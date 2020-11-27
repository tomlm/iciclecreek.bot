using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucy.PatternMatchers
{
    /// <summary>
    /// Will match if there is an existing entity @foo at given start location
    /// </summary>
    public class EntityPatternMatcher : PatternMatcher
    {
        public EntityPatternMatcher(string entityType)
        {
            this.EntityType = entityType.TrimStart('@');
        }

        public string EntityType { get; set; }

        public override MatchResult Matches(MatchContext context, int start)
        {
            var matchResult = new MatchResult();
            var entityToken = context.FindNextEntityOfType(EntityType, start);
            if (entityToken != null)
            {
                // add the matched entity to the children of the currentEntity.
                context.CurrentEntity.Children.Add(entityToken);

                matchResult.Matched = true;
                matchResult.NextStart = entityToken.End;
            }

            return matchResult;
        }

        public override string ToString() => $"@{EntityType}";

        public override IEnumerable<string> GetEntityReferences()
        {
            yield return this.EntityType;
        }
    }
}
