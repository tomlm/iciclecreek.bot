using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;

namespace Iciclecreek.Bot.Builder.Dialogs.AdaptiveCards.Memory
{
    /// <summary>
    /// Persistant Memory scope bound to action.cardId value.
    /// </summary>
    public class CardMemoryScope : BotStateMemoryScope<CardState>
    {
        public CardMemoryScope()
            : base("card")
        {
        }
    }
}
