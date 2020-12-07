using System;
using System.Collections.Generic;
using System.Text;

namespace Lucy.PatternMatchers
{
    public class LucyEntityComparer : EqualityComparer<LucyEntity>
    {
        public override bool Equals(LucyEntity token1, LucyEntity token2)
        {
            if (token2 == null && token1 == null)
                return true;
            else if (token1 == null || token2 == null)
                return false;
            else if (String.Equals(token1.Type, token2.Type, StringComparison.OrdinalIgnoreCase) &&
                     token1.Start == token2.Start &&
                     token1.End == token2.End &&
                     token1.Children.Count == token2.Children.Count)
            {
                foreach (var child in token1.Children)
                {
                    if (!token2.Children.Contains(child))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
                return false;
        }

        public override int GetHashCode(LucyEntity token)
        {
            int hCode = token.Type.GetHashCode() ^ token.Start ^ token.End;
            foreach(var child in token.Children)
            {
                hCode ^= child.GetHashCode();
            }

            return hCode.GetHashCode();
        }
    }
}
