using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace ConsoleBot2
{
#pragma warning disable CS0618 // Type or member is obsolete
    public class Bot : DialogManager, IBot
#pragma warning restore CS0618 // Type or member is obsolete
    {
        public Bot()
        {
            this.RootDialog = new IntentDialog("", null); 
        }

        Task IBot.OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken) => base.OnTurnAsync(turnContext, cancellationToken);
    }
}
