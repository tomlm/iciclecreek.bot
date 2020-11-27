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

        public override MatchResult Matches(MatchContext context, int start)
        {
            var matchResult = new MatchResult();

            var token = context.FindNextTextEntity(start);
            if (context.IsTokenMatched(token))
            {
                matchResult.Matched = true;
                matchResult.NextStart = token.End;
            }
            return matchResult;
        }

        public override bool IsWildcard() => true;

        public override string ToString() => "AnyEntity";
    }
}
