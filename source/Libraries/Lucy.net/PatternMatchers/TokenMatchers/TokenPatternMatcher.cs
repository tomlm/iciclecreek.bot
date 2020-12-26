using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucy.PatternMatchers.Matchers
{
    /// <summary>
    /// Will match if htere is a @Token that matches.
    /// </summary>
    public class TokenPatternMatcher : PatternMatcher
    {
        public const string ENTITYTYPE = "Token";

        public TokenPatternMatcher(string text, string token)
        {
            this.Text = text;
            this.Token = token;
        }

        public string Text { get; set; }

        public string Token { get; set; }

        public HashSet<String> FuzzyTokens { get; set; } = new HashSet<string>();

        public override MatchResult Matches(MatchContext context, TokenEntity startToken, PatternMatcher nextPatternMatcher)
        {
            var tokenEntity = startToken;
            if (tokenEntity != null)
            {
                var resolution = tokenEntity?.Resolution as TokenResolution;
                if (resolution != null)
                {
                    // see if it matches the normal token
                    if (this.Token == resolution.Token)
                    {
                        return new MatchResult(true, this, context.GetNextTokenEntity(tokenEntity), tokenEntity.Start, tokenEntity.End);
                    }

                    // if we have fuzzyTokens, see if it matches any of the fuzzy tokens.
                    if (this.FuzzyTokens.Any())
                    {
                        foreach (var fuzzyToken in FuzzyTokens)
                        {
                            foreach (var fuzzyToken2 in resolution.FuzzyTokens)
                            {
                                if (fuzzyToken2 == fuzzyToken)
                                {
                                    return new MatchResult(true, this, context.GetNextTokenEntity(tokenEntity), tokenEntity.Start, tokenEntity.End);
                                }
                            }
                        }
                    }
                }
            }

            return new MatchResult(false, this);
        }

        public override string ToString() => $"{(FuzzyTokens.Any() ? $"{Token}~" : Token)}";

        public override IEnumerable<string> GenerateExamples(LucyEngine engine)
        {
            yield return Text.Trim();
        }

        public override string GenerateExample(LucyEngine engine)
        {
            return Text.Trim();
        }

    }
}
