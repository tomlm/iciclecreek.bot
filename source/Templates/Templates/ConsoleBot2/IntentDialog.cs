using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;

namespace ConsoleBot2
{
    public class IntentDialog : CoreDialog
    {
        public IntentDialog(string dialogId, IRecognizer reocgnizer)
            : base(dialogId)
        {
            this.Reoc
        }

        public IRecognizer Recognizer { get; private set; }
    }
}
