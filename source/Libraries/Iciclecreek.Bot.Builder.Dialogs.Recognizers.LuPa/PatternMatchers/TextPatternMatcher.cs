using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa.PatternMatchers.Matchers
{
    /// <summary>
    /// Will match if htere is a @Text that matches.
    /// </summary>
    public class TextPatternMatcher : PatternMatcher
    {
        public const string ENTITYTYPE = "@Text";

        public TextPatternMatcher(string textToken)
        {
            this.Token = textToken;
        }

        public string Token { get; set; }

        public bool FuzzyMatch { get; set; }

        public override MatchResult Matches(MatchContext utterance, int start)
        {
            var matchResult = new MatchResult();
            var entityToken = utterance.FindNextEntities(ENTITYTYPE, start).FirstOrDefault();
            if (entityToken.Text == Token)
            {
                matchResult.Matched = true;
                matchResult.NextStart = entityToken.End;
            }

            return matchResult;
        }

        public override string ToString() => $"Text({Token})";

    }
}
