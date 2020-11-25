using System;
using System.Collections.Generic;
using System.Text;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Luce
{
    public class IntentModel
    {
        public IntentModel()
        {
        }

        public string Intent { get; set; }

        public List<string> Examples { get; set; } = new List<string>();

        public override string ToString() => $"Intent: {Intent}";
    }
}
