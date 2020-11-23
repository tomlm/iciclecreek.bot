using System;
using System.Collections.Generic;
using System.Text;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa
{
    public class IntentModel
    {
        public string Intent { get; set; }

        public List<string> Examples { get; set; } = new List<string>();
    }
}
