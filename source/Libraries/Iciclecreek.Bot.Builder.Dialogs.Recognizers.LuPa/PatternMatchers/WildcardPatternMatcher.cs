using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa.PatternMatchers.Matchers;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa.PatternMatchers
{
    /// <summary>
    /// Will match any unclaimed tokens
    /// </summary>
    public class WildcardPatternMatcher : PatternMatcher
    {
        public const string ENTITYTYPE = "@wildcard";

        public WildcardPatternMatcher()
        {
        }

        public override MatchResult Matches(MatchContext context, int start)
        {
            var matchResult = new MatchResult();
            var entityToken = context.FindNextEntities(ENTITYTYPE, start).FirstOrDefault();
            if (entityToken != null)
            {
                matchResult.Matched = true;
                matchResult.NextStart = entityToken.End;
            }
            return matchResult;
        }

        public override string ToString() => ENTITYTYPE;
    }
}
