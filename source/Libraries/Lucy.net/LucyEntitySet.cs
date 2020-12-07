using System;
using System.Collections.Generic;
using System.Text;
using Lucy.PatternMatchers;

namespace Lucy
{
    public class LucyEntitySet : HashSet<LucyEntity>
    {
        public LucyEntitySet()
        {
        }

        public LucyEntitySet(IEnumerable<LucyEntity> entities)
            : base(entities)
        {
        }
    }
}
