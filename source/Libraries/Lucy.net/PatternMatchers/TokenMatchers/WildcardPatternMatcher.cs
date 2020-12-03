﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

            if (tokenEntity != null && !context.IsTokenMatched(tokenEntity))
            {
                // if last child is a wildcard and it's end matches the last token's end
                // then we will merge the wildcards together.
                var previousToken = context.GetPreviousTokenEntity(tokenEntity);
                var wildcardEntity = context.CurrentEntity.Children.FirstOrDefault(wildcard => wildcard.Type == this.entityType && wildcard.End == previousToken.End);
                if (wildcardEntity != null)
                {
                    wildcardEntity.End = tokenEntity.End;
                }
                else
                {
                    wildcardEntity = new LucyEntity()
                    {
                        Type = entityType,
                        Start = tokenEntity.Start
                    };
                    context.AddToCurrentEntity(wildcardEntity);
                }

                // update wildcardToken by including the next token.
                wildcardEntity.End = tokenEntity.End;
                wildcardEntity.Text = context.Text.Substring(wildcardEntity.Start, wildcardEntity.End - wildcardEntity.Start);
                wildcardEntity.Resolution = wildcardEntity.Text;

                matchResult.Matched = true;
                matchResult.End = tokenEntity.End;
                matchResult.NextToken = context.GetNextTokenEntity(tokenEntity);
            }
            return matchResult;
        }

        public override bool ContainsWildcard() => true;

        public override IEnumerable<string> GetEntityReferences()
        {
            yield return ENTITYTYPE;
        }

        public override string ToString() => $"{((this.entityType != ENTITYTYPE) ? this.entityType+":" : ENTITYTYPE)}___";

        public override IEnumerable<string> GenerateExamples(LucyEngine engine)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = rnd.Next(7) + 3; i > 0; i--)
            {
                sb.Append((char)('a' + rnd.Next(26)));
            }
            yield return $"{sb}".Trim();
        }

        public override string GenerateExample(LucyEngine engine) => GenerateExamples(engine).First();
    }
}
