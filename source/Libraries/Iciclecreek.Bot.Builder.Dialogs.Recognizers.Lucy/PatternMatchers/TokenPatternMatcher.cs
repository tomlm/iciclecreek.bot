using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy.PatternMatchers.Matchers
{
    /// <summary>
    /// Will match if htere is a @Token that matches.
    /// </summary>
    public class TokenPatternMatcher : PatternMatcher
    {
        public const string ENTITYTYPE = "@Token";

        public TokenPatternMatcher(string textToken)
        {
            this.Token = textToken;
        }

        public string Token { get; set; }

        public bool FuzzyMatch { get; set; }

        public override MatchResult Matches(MatchContext context, int start)
        {
            var matchResult = new MatchResult();
            var entityToken = context.FindNextEntities(ENTITYTYPE, start).FirstOrDefault();
            if (entityToken?.Text == Token)
            {
                matchResult.Matched = true;
                matchResult.NextStart = entityToken.End;
            }

            return matchResult;
        }

        public override string ToString() => $"Token({Token})";

    }
}
