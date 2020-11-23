using System;
using System.Collections.Generic;
using System.Text;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa
{
    public interface IExamples
    {
        IEnumerable<String> GetExamples();
    }
}
