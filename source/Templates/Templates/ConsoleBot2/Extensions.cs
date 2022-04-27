using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;
using Microsoft.Identity.Client;

namespace ConsoleBot2
{
    public static class Extensions
    {
        public static async Task EndConversation(this ITurnContext turnContext) => await turnContext.SendActivityAsync(Activity.CreateEndOfConversationActivity());

    }
}
