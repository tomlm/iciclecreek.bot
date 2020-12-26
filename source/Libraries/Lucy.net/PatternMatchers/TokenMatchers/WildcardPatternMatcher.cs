using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Lucy.PatternMatchers.Matchers;
using Newtonsoft.Json.Linq;

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

        public override MatchResult Matches(MatchContext context, TokenEntity startToken, PatternMatcher nextPatternMatcher)
        {
            var tokenEntity = startToken;

            if (tokenEntity != null)
            {
                if (nextPatternMatcher != null)
                {
                    MatchResult nextPatternMatch = nextPatternMatcher?.Matches(context, tokenEntity, null);
                    if (nextPatternMatch.Matched && nextPatternMatch.NextToken != tokenEntity)
                    {
                        return new MatchResult(false, this)
                        {
                            NextPatternMatch = nextPatternMatch
                        };
                    }
                }

                if (!context.IsTokenMatched(tokenEntity))
                {
                    // if last child is a wildcard and it's end matches the last token's end
                    // then we will merge the wildcards together.
                    var previousToken = context.GetPrevTokenEntity(tokenEntity);
                    var wildcardEntity = context.CurrentEntity.Children.FirstOrDefault(wildcard => wildcard.Type == this.entityType && wildcard.End == previousToken.End);
                    if (wildcardEntity != null)
                    {
                        var newEntity = new LucyEntity()
                        {
                            Type = entityType,
                            Start = wildcardEntity.Start,
                            End = tokenEntity.End,
                            Score = ((float)tokenEntity.End - wildcardEntity.Start) / context.Text.Length / 2,
                            Text = context.Text.Substring(wildcardEntity.Start, tokenEntity.End - wildcardEntity.Start),
                            Resolution = context.Text.Substring(wildcardEntity.Start, tokenEntity.End - wildcardEntity.Start),
                        };

                        // remove old entity
                        context.CurrentEntity.Children.Remove(wildcardEntity);

                        // add new merged wildcard entity "joe" "smith" => "joe smith"
                        context.AddToCurrentEntity(newEntity);
                    }
                    else
                    {
                        var newEntity = new LucyEntity()
                        {
                            Type = entityType,
                            Start = tokenEntity.Start,
                            End = tokenEntity.End,
                            Score = ((float)tokenEntity.End - tokenEntity.Start) / context.Text.Length / 2,
                            Text = context.Text.Substring(tokenEntity.Start, tokenEntity.End - tokenEntity.Start),
                            Resolution = context.Text.Substring(tokenEntity.Start, tokenEntity.End - tokenEntity.Start)
                        };
                        context.AddToCurrentEntity(newEntity);
                    }

                    return new MatchResult(true, this, context.GetNextTokenEntity(tokenEntity), tokenEntity.Start, tokenEntity.End);
                }
            }

            return new MatchResult(false, this);
        }

        public override bool ContainsWildcard() => true;

        public override IEnumerable<string> GetEntityReferences()
        {
            yield return ENTITYTYPE;
        }

        public override string ToString() => $"{((this.entityType != ENTITYTYPE) ? this.entityType + ":" : ENTITYTYPE)}___";

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
