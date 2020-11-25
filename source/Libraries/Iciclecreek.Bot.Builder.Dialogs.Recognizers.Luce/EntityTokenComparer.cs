using System;
using System.Collections.Generic;
using System.Text;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Luce.PatternMatchers
{
    public class EntityTokenComparer : EqualityComparer<LuceEntity>
    {
        public override bool Equals(LuceEntity token1, LuceEntity token2)
        {
            if (token2 == null && token1 == null)
                return true;
            else if (token1 == null || token2 == null)
                return false;
            else if (String.Equals(token1.Type, token2.Type, StringComparison.OrdinalIgnoreCase) &&
                     token1.Start == token2.Start &&
                     token1.End == token2.End)
                return true;
            else
                return false;
        }

        public override int GetHashCode(LuceEntity token)
        {
            int hCode = token.Type.GetHashCode() ^ token.Start ^ token.End;
            return hCode.GetHashCode();
        }
    }
}
