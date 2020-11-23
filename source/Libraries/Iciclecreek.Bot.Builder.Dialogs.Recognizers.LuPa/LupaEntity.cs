using System;
using System.Collections.Generic;
using System.Text;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa
{
    /// <summary>
    /// Entities which are tracked in a MatchContext
    /// </summary>
    public class LupaEntity
    {
        // name of the entity type
        public string Type { get; set; }

        // original text
        public string Text { get; set; }

        // normalized text
        public string Resolution { get; set; }

        public float Score { get; set; }

        // start index
        public int Start { get; set; }

        // index of first char outside of token, length = end-start
        public int End { get; set; }
    }
}
