using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucy.PatternMatchers.Matchers;

namespace Lucy.PatternMatchers
{
    /// <summary>
    /// Will match any unclaimed tokens
    /// </summary>
    public class WildcardPatternMatcher : PatternMatcher
    {
        public const string ENTITYTYPE = "wildcard";

        private string entityType = ENTITYTYPE;

        public WildcardPatternMatcher(string variation = null)
        {
            if (variation != null && variation.IndexOf(":") > 0)
            {
                entityType = variation.Split(':').First().Trim();
            }
        }

        public override MatchResult Matches(MatchContext context, LucyEntity tokenEntity)
        {
            var matchResult = new MatchResult();

            if (tokenEntity != null)
            {
                // we add wildcardtoken on first token, and then get it and keep appending until we decide we are done.
                if (context.CurrentWildcard == null)
                {
                    context.CurrentWildcard = new LucyEntity()
                    {
                        Type = entityType,
                        Start = tokenEntity.Start
                    };
                }

                // update wildcardToken by including the next token.
                context.CurrentWildcard.End = tokenEntity.End;
                context.CurrentWildcard.Resolution = context.Text.Substring(context.CurrentWildcard.Start, context.CurrentWildcard.End - context.CurrentWildcard.Start);
                context.CurrentWildcard.Text = context.Text.Substring(context.CurrentWildcard.Start, context.CurrentWildcard.End - context.CurrentWildcard.Start);

                // update parent token 
                context.CurrentEntity.End = tokenEntity.End;

                matchResult.Matched = true;
                matchResult.End = tokenEntity.End;
                matchResult.NextToken = context.GetNextTokenEntity(tokenEntity);
            }
            return matchResult;
        }

        public override bool IsWildcard() => true;

        public override IEnumerable<string> GetEntityReferences()
        {
            yield return ENTITYTYPE;
        }

        public override string ToString() => "___";
    }
}
