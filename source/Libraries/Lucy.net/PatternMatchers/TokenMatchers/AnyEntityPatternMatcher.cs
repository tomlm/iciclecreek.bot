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
    public class AnyEntityPatternMatcher : PatternMatcher
    {
        public AnyEntityPatternMatcher()
        {
        }

        public override MatchResult Matches(MatchContext context, LucyEntity tokenEntity)
        {
            var matchResult = new MatchResult();

            if (tokenEntity != null)
            {
                if (context.IsTokenMatched(tokenEntity))
                {
                    matchResult.Matched = true;
                    matchResult.End = tokenEntity.End;
                    matchResult.NextToken = context.GetNextTokenEntity(tokenEntity);
                }
            }
            return matchResult;
        }

        public override string ToString() => "AnyEntity";
    }
}
