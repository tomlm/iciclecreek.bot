using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder;

namespace Iciclecreek.Bot.Builder.Dialogs.AdaptiveCards.Memory
{
    /// <summary>
    /// Defines a state management object for card state.
    /// </summary>
    /// <remarks>
    /// Card state is available in any turn that the bot is conversing with that card on that
    /// channel, regardless of the conversation.
    /// </remarks>
    public class CardState : BotState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardState"/> class.
        /// </summary>
        /// <param name="storage">The storage layer to use.</param>
        public CardState(IStorage storage)
            : base(storage, nameof(CardState))
        {
        }

        /// <summary>
        /// Gets the key to use when reading and writing state to and from storage.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <returns>The storage key.</returns>
        /// <remarks>
        /// Card state includes the channel ID and card ID as part of its storage key.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <see cref="ITurnContext.Activity"/> for the
        /// current turn is missing <see cref="Schema.Activity.ChannelId"/> or
        /// <see cref="Schema.Activity.From"/> information, or the sender's
        /// <see cref="Schema.ConversationAccount.Id"/> is missing.</exception>
        protected override string GetStorageKey(ITurnContext turnContext)
        {
            dynamic value = turnContext.Activity?.Value;
            if (value?.action?.cardId != null)
            {
                return $"cards/{value?.action?.cardId}";
            }
            return null;
        }
    }
}
