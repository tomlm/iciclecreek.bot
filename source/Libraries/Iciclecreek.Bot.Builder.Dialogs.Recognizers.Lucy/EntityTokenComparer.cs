using System;
using System.Collections.Generic;
using System.Text;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy.PatternMatchers
{
    public class EntityTokenComparer : EqualityComparer<LucyEntity>
    {
        public override bool Equals(LucyEntity token1, LucyEntity token2)
        {
            if (token2 == null && token1 == null)
                return true;
            else if (token1 == null || token2 == null)
                return false;
            else if (token1.Type == token2.Type &&
                     token1.Start == token2.Start &&
                     token1.End == token2.End)
                return true;
            else
                return false;
        }

        public override int GetHashCode(LucyEntity token)
        {
            int hCode = token.Type.GetHashCode() ^ token.Start ^ token.End;
            return hCode.GetHashCode();
        }
    }
}
