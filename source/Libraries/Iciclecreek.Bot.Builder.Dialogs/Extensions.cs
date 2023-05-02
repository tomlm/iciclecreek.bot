using System.Runtime.CompilerServices;
using Microsoft.Bot.Schema;

namespace Iciclecreek.Bot.Builder.Dialogs
{
    public static class Extensions
    {

        /// <summary>
        /// Creates a new message activity as a response to this activity.
        /// </summary>
        /// <param name="text">The text of the reply.</param>
        /// <param name="locale">The language code for the <paramref name="text"/>.</param>
        /// <returns>The new message activity.</returns>
        /// <remarks>The new activity sets up routing information based on this activity.</remarks>
        public static IMessageActivity CreateReply(this IActivity activity, string text = null, string locale = null)
        {
            return ((Activity)activity).CreateReply(text, locale).AsMessageActivity();
        }

        /// <summary>
        /// Creates a new trace activity based on this activity.
        /// </summary>
        /// <param name="name">The name of the trace operation to create.</param>
        /// <param name="value">Optional, the content for this trace operation.</param>
        /// <param name="valueType">Optional, identifier for the format of the <paramref name="value"/>.
        /// Default is the name of type of the <paramref name="value"/>.</param>
        /// <param name="label">Optional, a descriptive label for this trace operation.</param>
        /// <returns>The new trace activity.</returns>
        public static ITraceActivity CreateTrace(this IActivity activity, string name, object value = null, string valueType = null, [CallerMemberName] string label = null)
        {
            return ((Activity)activity).CreateTrace(name, value, valueType, label);
        }

        /// <summary>
        /// Returns this activity as an <see cref="IMessageActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a message activity; or null.</returns>
        public static IMessageActivity AsMessageActivity(this IActivity activity)
        {
            return ((Activity)activity).AsMessageActivity();
        }

        /// <summary>
        /// Returns this activity as an <see cref="IContactRelationUpdateActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a contact relation update activity; or null.</returns>
        public static IContactRelationUpdateActivity AsContactRelationUpdateActivity(this IActivity activity)
        {
            return ((Activity)activity).AsContactRelationUpdateActivity();
        }

        /// <summary>
        /// Returns this activity as an <see cref="IInstallationUpdateActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as an installation update activity; or null.</returns>
        public static IInstallationUpdateActivity AsInstallationUpdateActivity(this IActivity activity)
        {
            return ((Activity)activity).AsInstallationUpdateActivity();
        }

        /// <summary>
        /// Returns this activity as an <see cref="IConversationUpdateActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a conversation update activity; or null.</returns>
        public static IConversationUpdateActivity AsConversationUpdateActivity(this IActivity activity)
        {
            return ((Activity)activity).AsConversationUpdateActivity();
        }

        /// <summary>
        /// Returns this activity as an <see cref="ITypingActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a typing activity; or null.</returns>
        public static ITypingActivity AsTypingActivity(this IActivity activity)
        {
            return ((Activity)activity).AsTypingActivity();
        }

        /// <summary>
        /// Returns this activity as an <see cref="IEndOfConversationActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as an end of conversation activity; or null.</returns>
        public static IEndOfConversationActivity AsEndOfConversationActivity(this IActivity activity)
        {
            return ((Activity)activity).AsEndOfConversationActivity();
        }

        /// <summary>
        /// Returns this activity as an <see cref="IEventActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as an event activity; or null.</returns>
        public static IEventActivity AsEventActivity(this IActivity activity)
        {
            return ((Activity)activity).AsEventActivity();
        }

        /// <summary>
        /// Returns this activity as an <see cref="IInvokeActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as an invoke activity; or null.</returns>
        public static IInvokeActivity AsInvokeActivity(this IActivity activity)
        {
            return ((Activity)activity).AsInvokeActivity();
        }

        /// <summary>
        /// Returns this activity as an <see cref="IMessageUpdateActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a message update request; or null.</returns>
        public static IMessageUpdateActivity AsMessageUpdateActivity(this IActivity activity)
        {
            return ((Activity)activity).AsMessageUpdateActivity();
        }

        /// <summary>
        /// Returns this activity as an <see cref="IMessageDeleteActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a message delete request; or null.</returns>
        public static IMessageDeleteActivity AsMessageDeleteActivity(this IActivity activity)
        {
            return ((Activity)activity).AsMessageDeleteActivity();
        }

        /// <summary>
        /// Returns this activity as an <see cref="IMessageReactionActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a message reaction activity; or null.</returns>
        public static IMessageReactionActivity AsMessageReactionActivity(this IActivity activity)
        {
            return ((Activity)activity).AsMessageReactionActivity();
        }

        /// <summary>
        /// Returns this activity as an <see cref="ISuggestionActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a suggestion activity; or null.</returns>
        public static ISuggestionActivity AsSuggestionActivity(this IActivity activity)
        {
            return ((Activity)activity).AsSuggestionActivity();
        }

        /// <summary>
        /// Returns this activity as an <see cref="ITraceActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a trace activity; or null.</returns>
        public static ITraceActivity AsTraceActivity(this IActivity activity)
        {
            return ((Activity)activity).AsTraceActivity();
        }

        /// <summary>
        /// Returns this activity as an <see cref="IHandoffActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a handoff activity; or null.</returns>
        public static IHandoffActivity AsHandoffActivity(this IActivity activity)
        {
            return ((Activity)activity).AsHandoffActivity();
        }

        /// <summary>
        /// Returns this activity as an <see cref="ICommandActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a command activity; or null.</returns>
        public static ICommandActivity AsCommandActivity(this IActivity activity)
        {
            return ((Activity)activity).AsCommandActivity();
        }

        /// <summary>
        /// Returns this activity as an <see cref="ICommandResultActivity"/> object; or null, if this is not that type of activity.
        /// </summary>
        /// <returns>This activity as a command result activity; or null.</returns>
        public static ICommandResultActivity AsCommandResultActivity(this IActivity activity)
        {
            return ((Activity)activity).AsCommandResultActivity();
        }

        /// <summary>
        /// Indicates whether this activity has content.
        /// </summary>
        /// <returns>True, if this activity has any content to send; otherwise, false.</returns>
        /// <remarks>This method is defined on the <see cref="Activity"/> class, but is only intended
        /// for use with a message activity, where the activity <see cref="Activity.Type"/> is set to
        /// <see cref="ActivityTypes.Message"/>.</remarks>
        public static bool HasContent(this IActivity activity)
        {
            return ((Activity)activity).HasContent();
        }

        /// <summary>
        /// Resolves the mentions from the entities of this activity.
        /// </summary>
        /// <returns>The array of mentions; or an empty array, if none are found.</returns>
        /// <remarks>This method is defined on the <see cref="Activity"/> class, but is only intended
        /// for use with a message activity, where the activity <see cref="Activity.Type"/> is set to
        /// <see cref="ActivityTypes.Message"/>.</remarks>
        /// <seealso cref="Entities"/>
        /// <seealso cref="Mention"/>
        public static Mention[] GetMentions(this IActivity activity)
        {
            return ((Activity)activity).GetMentions();
        }

        /// <summary>
        /// Gets the channel data for this activity as a strongly-typed object.
        /// </summary>
        /// <typeparam name="TypeT">The type of the object to return.</typeparam>
        /// <returns>The strongly-typed object; or the type's default value, if the <see cref="ChannelData"/> is null.</returns>
        /// <seealso cref="ChannelData"/>
        /// <seealso cref="TryGetChannelData{TypeT}(out TypeT)"/>
#pragma warning disable CA1715 // Identifiers should have correct prefix (we can't change it without breaking binary compatibility)
        public static TypeT GetChannelData<TypeT>(this IActivity activity)
#pragma warning restore CA1715 // Identifiers should have correct prefix
        {
            return ((Activity)activity).GetChannelData<TypeT>();
        }

        /// <summary>
        /// Gets the channel data for this activity as a strongly-typed object.
        /// A return value idicates whether the operation succeeded.
        /// </summary>
        /// <typeparam name="TypeT">The type of the object to return.</typeparam>
        /// <param name="instance">When this method returns, contains the strongly-typed object if the operation succeeded,
        /// or the type's default value if the operation failed.</param>
        /// <returns>
        /// <c>true</c> if the operation succeeded; otherwise, <c>false</c>.
        /// </returns>
        /// <seealso cref="ChannelData"/>
        /// <seealso cref="GetChannelData{TypeT}"/>
#pragma warning disable CA1715 // Identifiers should have correct prefix (we can't change it without breaking binary compatibility)
        public static bool TryGetChannelData<TypeT>(this IActivity activity, out TypeT instance)
#pragma warning restore CA1715 // Identifiers should have correct prefix
        {
            return ((Activity)activity).TryGetChannelData<TypeT>(out instance);
        }

        /// <summary>
        /// Creates a <see cref="ConversationReference"/> based on this activity.
        /// </summary>
        /// <returns>A conversation reference for the conversation that contains this activity.</returns>
        public static ConversationReference GetConversationReference(this IActivity activity)
        {
            return ((Activity)activity).GetConversationReference();
        }

        /// <summary>
        /// Create a ConversationReference based on this Activity's Conversation info and the ResourceResponse from sending an activity.
        /// </summary>
        /// <param name="reply">ResourceResponse returned from sendActivity.</param>
        /// <returns>A ConversationReference that can be stored and used later to delete or update the activity.</returns>
        public static ConversationReference GetReplyConversationReference(this IActivity activity,ResourceResponse reply)
        {
            return ((Activity)activity).GetReplyConversationReference(reply);
        }

        /// <summary>
        /// Updates this activity with the delivery information from an existing <see cref="ConversationReference"/>.
        /// </summary>
        /// <param name="reference">The existing conversation reference.</param>
        /// <param name="isIncoming">Optional, <c>true</c> to treat the activity as an
        /// incoming activity, where the bot is the recipient; otherwise, <c>false</c>.
        /// Default is <c>false</c>, and the activity will show the bot as the sender.</param>
        /// <remarks>Call <see cref="GetConversationReference()"/> on an incoming
        /// activity to get a conversation reference that you can then use to update an
        /// outgoing activity with the correct delivery information.
        /// </remarks>
        /// <returns>This activy, updated with the delivery information.</returns>
        public static Activity ApplyConversationReference(this IActivity activity, ConversationReference reference, bool isIncoming = false)
        {
            return ((Activity)activity).ApplyConversationReference(reference, isIncoming);
        }

        /// <summary>
        /// Determine if the Activity was sent via an Http/Https connection or Streaming
        /// This can be determined by looking at the ServiceUrl property:
        /// (1) All channels that send messages via http/https are not streaming
        /// (2) Channels that send messages via streaming have a ServiceUrl that does not begin with http/https.
        /// </summary>
        /// <returns>True if the Activity originated from a streaming connection.</returns>
        public static bool IsFromStreamingConnection(this IActivity activity)
        {
            return ((Activity)activity).IsFromStreamingConnection();
        }
    }
}
