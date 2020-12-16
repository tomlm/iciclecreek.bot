using System;
using System.Collections.Generic;
using System.Text;

namespace Lucy
{
    public class TokenEntity : LucyEntity
    {
        public TokenEntity Next { get; set; }

        public TokenEntity Previous { get; set; }
    }
}
