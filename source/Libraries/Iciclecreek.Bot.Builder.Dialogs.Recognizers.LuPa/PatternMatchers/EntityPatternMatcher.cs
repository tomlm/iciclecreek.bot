using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa.PatternMatchers
{
    /// <summary>
    /// Will match if there is an existing entity @foo at given start location
    /// </summary>
    public class EntityPatternMatcher : PatternMatcher
    {
        public EntityPatternMatcher(string entityType)
        {
            this.EntityType = entityType;
        }

        public string EntityType { get; set; }

        public override MatchResult Matches(MatchContext utterance, int start)
        {
            var matchResult = new MatchResult();
            var entityToken = utterance.FindNextEntities(EntityType, start).FirstOrDefault();
            if (entityToken != null)
            {
                matchResult.Matched = true;
                matchResult.NextStart = entityToken.End;
            }

            return matchResult;
        }

        public override string ToString() => $"EntityPattern({EntityType})";
    }
}
