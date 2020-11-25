using System;
using System.Collections.Generic;
using System.Text;

namespace Luce
{
    public enum Ordinality
    {
        // (token)
        One,

        // (token)?
        ZeroOrOne,

        // (token)+
        OneOrMore,

        // (token)*
        ZeroOrMore,
    }
}
