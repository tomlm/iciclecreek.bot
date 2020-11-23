using System;
using System.Collections.Generic;
using System.Text;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy
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
