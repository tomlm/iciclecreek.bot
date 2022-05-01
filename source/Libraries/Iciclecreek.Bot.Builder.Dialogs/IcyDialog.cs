﻿#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Iciclecreek.Bot.Builder.Dialogs
{
    /// <summary>
    /// IcyDialog - A Slick dialog for creating pure code based dialogs with recognizers and strong type handlers
    /// </summary>
    /// <remarks>
    ///  1. hides BeginDialog/ContinueDialog and models the dialog simply as OnTurnAsync()
    ///     - dialog Options are accessible via GetOptions() method
    ///  2. The default OnTurnAsync() will dispatch to strongly typed virtual methods ala ActivityHandler, but with DialogContext instead of TurnContext:
    ///     - OnMessageActivityAsync(dc)
    ///     - OnEndOfConversationAsync(dc)
    ///     - OnMessageReactionActivityAsync(dc)
    ///     - OnAdaptiveCardInvoke(dc) 
    ///     - etc.
    ///  3. The default OnMessageActivity will invoke Recognizer and route them using OnRecognizedIntentAsync()/OnUnrecognizedIntentAsync() method
    ///  4. The default OnRecognizedIntentAsync() implementation will find methods using the following naming pattern:
    ///     
    ///     protected Task&ltDialogTurnResult&gt; OnXXXIntent(DialogContext dc, IMessageActivity messageActivity, TopScore topSCore, CancellationToken ct);
    ///     
    ///     Examples:
    ///     - "Greeting" intent => OnGreetingIntent(dc, IMessageActivity, topScore, cancellationToken)
    ///     - "Goodbye" intent => OnGoodbyeIntent(dc, IMessageActivity, topScore, cancellationToken)
    ///     - "None" or empty intents => OnUnrecognizedIntent(dc, IMessageActivity, cancallationToken)
    ///  5. Adds PromptAsync(propertyName,...) and OnPromptCompletedAsync() and OnPromptCanceldAsync() methods as replacement for BeginDialog/ResumeDialog()
    /// </remarks>
    public class IcyDialog : Dialog
    {
        private const string PROPERTY_KEY = "this.property";
        private Dictionary<string, MethodInfo> _intentMethods = new Dictionary<string, MethodInfo>();

        public IcyDialog(string dialogId = null)
            : base(dialogId)
        {
            _intentMethods = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                .Where(mi => mi.Name.StartsWith("On") && mi.Name.EndsWith("Intent") && mi.GetParameters().Count() == 4)
                .ToDictionary(mi => mi.Name, mi => mi);
        }

        /// <summary>
        /// (Optional) Recognizer for intents
        /// </summary>
        public Recognizer? Recognizer { get; set; } = null;

        #region DIALOG METHODS
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            dc.SaveOptions(options);
            var dtr = await OnBeginDialogAsync(dc, options, cancellationToken);
            if (dtr != null)
            {
                return dtr;
            }

            return await OnTurnAsync(dc, options, cancellationToken: cancellationToken);
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            var options = dc.GetOptions<object>();
            return await OnTurnAsync(dc, options, cancellationToken: cancellationToken);
        }

        public async override Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default)
        {
            DialogTurnResult dtr = await OnResumeDialogAsync(dc, reason, result, cancellationToken);

            if (dtr == null)
            {
                dtr = await this.OnEvaluateAsync(dc, cancellationToken);
            }

            return dtr ?? await dc.WaitForInputAsync();
        }
        #endregion

        #region DIALOG EVENTS
        /// <summary>
        /// OnBeginDialogAsync() - Called when dialog is invoked
        /// </summary>
        /// <remarks>
        /// Default behavior is to do nothing and continue execution
        ///
        /// Override this method to send an activity when dialog begins regardless of the activity that triggered it.
        /// </remarks>
        /// <param name="dc">dc</param>
        /// <param name="options">options</param>
        /// <param name="cancellationToken"></param>
        /// <returns>null value if no dialog action taken, otherwise dialogturnresult to indicate dialog action that was taken</returns>
        protected async virtual Task<DialogTurnResult> OnBeginDialogAsync(DialogContext dc, object options, CancellationToken cancellationToken = default)
        {
            return null;
        }

        /// <summary>
        /// OnResumeDialogAsync() - called when child dialog is completed and this dialog is resumed
        /// </summary>
        /// <param name="dc">dialog context</param>
        /// <param name="reason">reason </param>
        /// <param name="result">result </param>
        /// <param name="cancellationToken">cancellationToken </param>
        /// <returns>null value if no dialog action taken, otherwise dialogturnresult to indicate dialog action that was taken</returns>
        protected virtual async Task<DialogTurnResult> OnResumeDialogAsync(DialogContext dc, DialogReason reason, object result, CancellationToken cancellationToken)
        {
            if (ObjectPath.TryGetPathValue<string>(dc.State, PROPERTY_KEY, out var property))
            {
                ObjectPath.RemovePathValue(dc.State, PROPERTY_KEY);
                if (reason == DialogReason.EndCalled)
                {
                    return await this.OnPromptCompletedAsync(dc, property, result, cancellationToken);
                }
                else if (reason == DialogReason.CancelCalled)
                {
                    return await this.OnPromptCanceledAsync(dc, property, cancellationToken);
                }
            }

            return null;
        }

        /// <summary>
        /// OnPromptCompletedAsync() - Called when a child dialog completes 
        /// </summary>
        /// <remarks>
        /// Default behavior is to use to store the result in the property name and to continue execution
        ///
        /// You can override this to acknolwedge the value that came back
        /// 
        /// NOTE: if the is property is not pathed it will be stored in "turn.{resultName}"
        /// NOTE 2: If the property is missing it will be in "turn.lastresult" just like normal
        /// </remarks>
        /// <param name="dc">dc</param>
        /// <param name="reason">reason </param>
        /// <param name="property">resultName</param>
        /// <param name="result">result</param>
        /// <param name="cancellationToken"></param>
        /// <returns>null value if no dialog action taken, otherwise dialogturnresult to indicate dialog action that was taken</returns>
        protected async virtual Task<DialogTurnResult> OnPromptCompletedAsync(DialogContext dc, string property, object result, CancellationToken cancellationToken = default)
        {
            if (property.Contains('.'))
            {
                ObjectPath.SetPathValue(dc.State, property, result);
            }
            else
            {
                ObjectPath.SetPathValue(dc.State, $"turn.{property}", result);
            }

            return null;
        }

        /// <summary>
        /// OnPromptCanceledAsync() - Called when a child Dialog is canceled
        /// </summary>
        /// <remarks>
        /// default behavior is to do nothing and continue execution.
        ///
        /// Override this method to handle cancellation of a given property dialog.
        /// </remarks>
        /// <param name="dc"></param>
        /// <param name="property"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async virtual Task<DialogTurnResult> OnPromptCanceledAsync(DialogContext dc, string property, CancellationToken cancellationToken = default)
        {
            return null;
        }

        /// <summary>
        /// OnEvaluateAsync() called at the end of every turn when no dialog action is performed 
        /// </summary>
        /// <remarks>
        /// Default is to wait for input.
        /// 
        /// Override this method to inspect memory and decide to prompt for missing data, etc.
        /// </remarks>
        /// <param name="dc">dialogcontext</param>
        /// <param name="ct">ct</param>
        /// <returns>null value if no dialog action taken, otherwise dialogturnresult to indicate dialog action that was taken</returns>
        protected async virtual Task<DialogTurnResult> OnEvaluateAsync(DialogContext dc, CancellationToken ct)
        {
            return null;
        }

        /// <summary>
        /// OnTurnAsync() - Default OnTurn handler for dialog maps activities to strongly typed methods, OnMessageActivityAsync(), OnTypingActivityAsync() etc.
        /// </summary>
        /// <remarks>
        /// Default behavior is to the following 
        /// OnTurnAsync()
        ///     call activity handler =>
        ///         if NULL => OnEvaluateAsync()
        ///
        ///  override this to add new activity types.
        /// </remarks>
        /// <param name="dc">dc</param>
        /// <param name="options">options used to create dialog</param>
        /// <param name="cancellationToken">ct</param>
        /// <returns>dialog turn result task</returns>
        public virtual async Task<DialogTurnResult> OnTurnAsync(DialogContext dc, object options, CancellationToken cancellationToken = default)
        {
            DialogTurnResult dtr;
            switch (dc.Context.Activity.Type)
            {
                case ActivityTypes.Message:
                    dtr = await OnMessageActivityAsync(dc, dc.Context.Activity.AsMessageActivity(), cancellationToken);
                    break;

                case ActivityTypes.ConversationUpdate:
                    dtr = await OnConversationUpdateActivityAsync(dc, dc.Context.Activity.AsConversationUpdateActivity(), cancellationToken);
                    break;

                case ActivityTypes.MessageReaction:
                    dtr = await OnMessageReactionActivityAsync(dc, dc.Context.Activity.AsMessageReactionActivity(), cancellationToken);
                    break;

                case ActivityTypes.Event:
                    dtr = await OnEventActivityAsync(dc, dc.Context.Activity.AsEventActivity(), cancellationToken);
                    break;

                case ActivityTypes.Invoke:
                    dtr = await OnInvokeActivityAsync(dc, dc.Context.Activity.AsInvokeActivity(), cancellationToken);
                    break;

                case ActivityTypes.EndOfConversation:
                    dtr = await OnEndOfConversationActivityAsync(dc, dc.Context.Activity.AsEndOfConversationActivity(), cancellationToken);
                    break;

                case ActivityTypes.Typing:
                    dtr = await OnTypingActivityAsync(dc, dc.Context.Activity.AsTypingActivity(), cancellationToken);
                    break;

                case ActivityTypes.InstallationUpdate:
                    dtr = await OnInstallationUpdateActivityAsync(dc, dc.Context.Activity.AsInstallationUpdateActivity(), cancellationToken);
                    break;

                case ActivityTypes.Command:
                    dtr = await OnCommandActivityAsync(dc, dc.Context.Activity.AsCommandActivity(), cancellationToken);
                    break;

                case ActivityTypes.CommandResult:
                    dtr = await OnCommandResultActivityAsync(dc, dc.Context.Activity.AsCommandResultActivity(), cancellationToken);
                    break;

                default:
                    dtr = await OnUnrecognizedActivityTypeAsync(dc, dc.Context.Activity, cancellationToken);
                    break;
            }

            if (dtr == null)
            {
                dtr = await this.OnEvaluateAsync(dc, cancellationToken);
            }

            return dtr ?? await dc.WaitForInputAsync();
        }
        #endregion

        #region ACTIVITY HANDLERS
        /// <summary>
        /// OnMessageActivityAsync() - The default handler for MessageActivity 
        /// </summary>
        /// <remarks>
        /// The default implementation runs Recognizer against the message text and
        ///     OnRecognizedIntentAsync/OnUnrecognizedIntentAsync
        ///         if NULL => OnEvaluateAsync()
        /// override this to specilize the calling to the recognizer.
        /// </remarks>
        /// <param name="dc">Dialog Context</param>
        /// <param name="messageActivity">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>null value if no dialog action taken, otherwise dialogturnresult to indicate dialog action that was taken</returns>
        protected async virtual Task<DialogTurnResult> OnMessageActivityAsync(DialogContext dc, IMessageActivity messageActivity, CancellationToken cancellationToken)
        {
            if (!String.IsNullOrEmpty(messageActivity.Text) && this.Recognizer != null)
            {
                var recognizerResult = await Recognizer.RecognizeAsync(dc, dc.Context.Activity, cancellationToken);
                if (recognizerResult.Intents.Any())
                {
                    return await OnRecognizedIntentAsync(dc, messageActivity, recognizerResult, cancellationToken);
                }
            }

            return await OnUnrecognizedIntentAsync(dc, messageActivity, cancellationToken);
        }


        /// <summary>
        /// OnRecognizedIntentAsync() - called when an intent is recogized using the Recognizer
        /// </summary>
        /// <remarks>
        /// The default implementation uses the name of the intent to look up method
        ///   "Greeting" => OnGreetingIntent(dc, activity, recognizerResult, ct);
        ///   "none" | unrecognized => OnUnrecognizedIntent(dc, activity, ct);
        /// if intent signature is not found, it will call OnUnrecognizedIntenatAsync()
        /// 
        /// override OnRecognizedIntentAsync method to do you own intent mapping.
        /// </remarks>
        /// <param name="dc">dc</param>
        /// <param name="messageActivity">messageActivity</param>
        /// <param name="recognizerResult">The intent recognizer result</param>
        /// <param name="cancellationToken">ct</param>
        /// <returns>null value if no dialog action taken, otherwise dialogturnresult to indicate dialog action that was taken</returns>
        protected async virtual Task<DialogTurnResult> OnRecognizedIntentAsync(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            var intent = recognizerResult.Intents.First();
            if (_intentMethods.TryGetValue($"On{intent.Key}Intent", out var mi))
            {
                return await (Task<DialogTurnResult>)mi.Invoke(this, new object[] { dc, messageActivity, recognizerResult, cancellationToken });
            }

            return await OnUnrecognizedIntentAsync(dc, messageActivity, cancellationToken);
        }

        /// <summary>
        /// OnUnrecognizedIntentAsync() - called when no intent was recognized or there was no method to process the intent.
        /// <see cref="ActivityTypes.Message"/> activities, such as the conversational logic.
        /// </summary>
        /// <remarks>
        /// The default implementation says "I"m sorry, I didn't understand that."
        ///     and returns NULL to continue execution
        ///  override this to change the message/behavior and/or to call into other dialogs
        /// </remarks>
        /// <param name="dc">Dialog Context</param>
        /// <param name="messageActivity">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>null value if no dialog action taken, otherwise dialogturnresult to indicate dialog action that was taken</returns>
        protected async virtual Task<DialogTurnResult> OnUnrecognizedIntentAsync(DialogContext dc, IMessageActivity messageActivity, CancellationToken cancellationToken)
        {
            await dc.Context.SendActivityAsync("I'm sorry, I didn't understand that.");
            return null;
        }

        /// <summary>
        /// OnConversationUpdateActivityAsync() - Invoked when a conversation update activity is received 
        /// <see cref="OnTurnAsync(ITurnContext, CancellationToken)"/> is used.
        /// </summary>
        /// <remarks>
        /// Default behavior is to do nothing and wait for input
        /// 
        /// override this to handle conversation update activity.
        /// </remarks>
        /// <param name="dc">Dialog Context</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>null value if no dialog action taken, otherwise dialogturnresult to indicate dialog action that was taken</returns>
        protected virtual Task<DialogTurnResult> OnConversationUpdateActivityAsync(DialogContext dc, IConversationUpdateActivity conversationUpdateActivity, CancellationToken cancellationToken)
        {
            return dc.WaitForInputAsync();
        }

        /// <summary>
        /// OnMessageReactionActivityAsync() - Invoked when an MessageReaction activity is received from the connector
        /// </summary>
        /// <remarks>
        /// Default is to do nothing and wait for input
        ///
        /// override this to change behavior.
        /// 
        /// Message reactions correspond to the user adding a 'like' or 'sad' etc. (often an emoji) to a
        /// previously sent activity. Message reactions are only supported by a few channels.
        /// The activity that the message reaction corresponds to is indicated in the replyToId property.
        /// The value of this property is the activity id of a previously sent activity given back to the
        /// bot as the response from a send call.
        /// </remarks>
        /// <param name="dc">Dialog Context</param>
        /// <param name="messageReactionActivity">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>null value if no dialog action taken, otherwise dialogturnresult to indicate dialog action that was taken</returns>
        protected virtual Task<DialogTurnResult> OnMessageReactionActivityAsync(DialogContext dc, IMessageReactionActivity messageReactionActivity, CancellationToken cancellationToken)
        {
            return dc.WaitForInputAsync();
        }

        /// <summary>
        /// OnEventActivityAsync() - Invoked when an event activity is received from the connector 
        /// <remarks>
        /// Default is to do nothing and wait for input
        ///
        /// override this to change behavior.
        /// 
        /// Event activities can be used to communicate many different things.
        /// By default, this method will call <see cref="OnTokenResponseEventAsync(ITurnContext{IEventActivity}, CancellationToken)"/> if the
        /// activity's name is <c>tokens/response</c> or <see cref="OnEventAsync(ITurnContext{IEventActivity}, CancellationToken)"/> otherwise.
        /// A <c>tokens/response</c> event can be triggered by an <see cref="OAuthCard"/>.
        /// </remarks>
        /// <param name="dc">Dialog Context</param>
        /// <param name="eventActivity">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>null value if no dialog action taken, otherwise dialogturnresult to indicate dialog action that was taken</returns>
        protected virtual Task<DialogTurnResult> OnEventActivityAsync(DialogContext dc, IEventActivity eventActivity, CancellationToken cancellationToken)
        {
            if (eventActivity.Name == SignInConstants.TokenResponseEventName)
            {
                return OnTokenResponseEventAsync(dc, eventActivity, cancellationToken);
            }

            return dc.WaitForInputAsync();
        }

        /// <summary>
        /// OnTokenResponseEventAsync() - Invoked EventActivity with a <c>tokens/response</c> event is received when the base behavior of
        /// <see cref="OnEventActivityAsync(ITurnContext{IEventActivity}, CancellationToken)"/> is used.
        /// </summary>
        /// <remarks>
        /// If using an <c>OAuthPrompt</c>, override this method to forward this <see cref="Activity"/> to the current dialog.
        /// By default, this method does nothing.
        /// </remarks>
        /// <param name="dc">Dialog Context</param>
        /// <param name="eventActivity">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>null value if no dialog action taken, otherwise dialogturnresult to indicate dialog action that was taken</returns>
        protected virtual Task<DialogTurnResult> OnTokenResponseEventAsync(DialogContext dc, IEventActivity eventActivity, CancellationToken cancellationToken)
        {
            return dc.WaitForInputAsync();
        }

        /// <summary>
        /// OnInvokeActivityAsync() - Invoked when an invoke activity is received from the connector 
        /// </summary>
        /// <remarks>
        /// Invoke activities can be used to communicate many different things.
        /// 
        /// By default, this method will call <see cref="OnSignInInvokeAsync(ITurnContext{IInvokeActivity}, CancellationToken)"/> if the
        /// activity's name is <c>signin/verifyState</c> or <c>signin/tokenExchange</c>.
        /// A <c>signin/verifyState</c> or <c>signin/tokenExchange</c> invoke can be triggered by an <see cref="OAuthCard"/>.
        /// </remarks>
        /// <param name="dc">Dialog Context</param>
        /// <param name="invokeActivity">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>null value if no dialog action taken, otherwise dialogturnresult to indicate dialog action that was taken</returns>
        protected virtual async Task<DialogTurnResult> OnInvokeActivityAsync(DialogContext dc, IInvokeActivity invokeActivity, CancellationToken cancellationToken)
        {
            InvokeResponse invokeResponse = null;

            try
            {
                switch (invokeActivity.Name)
                {
                    case "application/search":
                        var searchInvokeValue = GetSearchInvokeValue(invokeActivity);
                        invokeResponse = CreateInvokeResponse(await OnSearchInvokeAsync(dc, invokeActivity, searchInvokeValue, cancellationToken));
                        break;


                    case "adaptiveCard/action":
                        var invokeValue = GetAdaptiveCardInvokeValue(invokeActivity);
                        invokeResponse = CreateInvokeResponse(await OnAdaptiveCardInvokeAsync(dc, invokeActivity, invokeValue, cancellationToken));
                        break;

                    case SignInConstants.VerifyStateOperationName:
                    case SignInConstants.TokenExchangeOperationName:
                        await OnSignInInvokeAsync(dc, invokeActivity, cancellationToken);
                        invokeResponse = CreateInvokeResponse();
                        break;

                    default:
                        throw new InvokeResponseException(HttpStatusCode.NotImplemented);
                }

            }
            catch (InvokeResponseException e)
            {
                invokeResponse = e.CreateInvokeResponse();
            }

            if (invokeResponse != null && dc.Context.TurnState.Get<Activity>(BotFrameworkAdapter.InvokeResponseKey) == null)
            {
                await dc.Context.SendActivityAsync(new Activity { Value = invokeResponse, Type = ActivityTypesEx.InvokeResponse }, cancellationToken);
            }

            return await dc.WaitForInputAsync();
        }

        /// <summary>
        /// OnSignInInvokeAsync() - Invoked when a <c>signin/verifyState</c> or <c>signin/tokenExchange</c> event is received 
        /// </summary>
        /// <remarks>
        /// If using an <c>OAuthPrompt</c>, override this method to forward this <see cref="Activity"/> to the current dialog.
        /// By default, this method throws an exception 
        /// </remarks>
        /// <param name="dc">dialog context</param>
        /// <param name="invokeActivity">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task<InvokeResponse> OnSignInInvokeAsync(DialogContext dc, IInvokeActivity invokeActivity, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// OnAdaptiveCardInvokeAsync() - Invoked when the bot is sent an Adaptive Card Action Execute.
        /// </summary>
        /// <remarks>
        /// By default, this method throws an exception 
        /// </remarks>
        /// <param name="dc">dialog context</param>
        /// <param name="activity">A strongly-typed context object for this turn.</param>
        /// <param name="invokeValue">A strongly-typed object from the incoming activity's Value.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task<AdaptiveCardInvokeResponse> OnAdaptiveCardInvokeAsync(DialogContext dc, IInvokeActivity activity, AdaptiveCardInvokeValue invokeValue, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// OnSearchInvokeAsync() - Invoked when the bot is sent an 'invoke' activity having name of 'application/search'.
        /// </summary>
        /// <remarks>
        /// By default, this method throws an exception 
        /// </remarks>
        /// <param name="dc">dialog context</param>
        /// <param name="activity">A strongly-typed context object for this turn.</param>
        /// <param name="invokeValue">A strongly-typed object from the incoming activity's Value.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task<SearchInvokeResponse> OnSearchInvokeAsync(DialogContext dc, IInvokeActivity activity, SearchInvokeValue invokeValue, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// OnEndOfConversationActivityAsync() - Override this in a derived class to provide logic specific to
        /// <see cref="ActivityTypes.EndOfConversation"/> activities, such as the conversational logic.
        /// </summary>
        /// <remarks>
        /// The default behavior is to cancel all dialogs
        /// </remarks>
        /// <param name="dc">dialog context</param>
        /// <param name="endOfConversationActivity">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>null value if no dialog action taken, otherwise dialogturnresult to indicate dialog action that was taken</returns>
        protected virtual Task<DialogTurnResult> OnEndOfConversationActivityAsync(DialogContext dc, IEndOfConversationActivity endOfConversationActivity, CancellationToken cancellationToken)
        {
            return dc.CancelAllDialogsAsync();
        }

        /// <summary>
        /// OnTypingActivityAsync() - Override this in a derived class to provide logic specific to
        /// <see cref="ActivityTypes.Typing"/> activities, such as the conversational logic.
        /// </summary>
        /// <remarks>
        /// The default behavior is to wait for input.
        /// </remarks>
        /// <param name="dc">dialog context</param>
        /// <param name="typingActivity">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>null value if no dialog action taken, otherwise dialogturnresult to indicate dialog action that was taken</returns>
        protected virtual Task<DialogTurnResult> OnTypingActivityAsync(DialogContext dc, ITypingActivity typingActivity, CancellationToken cancellationToken)
        {
            return dc.WaitForInputAsync();
        }

        /// <summary>
        /// OnInstallationUpdateActivityAsync() - Override this in a derived class to provide logic specific to
        /// <see cref="ActivityTypes.InstallationUpdate"/> activities.
        /// </summary>
        /// <remarks>
        /// The default behavior is to wait for input.
        /// </remarks>
        /// <param name="dc">dialog context</param>
        /// <param name="installationUpdateActivity">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>null value if no dialog action taken, otherwise dialogturnresult to indicate dialog action that was taken</returns>
        protected virtual Task<DialogTurnResult> OnInstallationUpdateActivityAsync(DialogContext dc, IInstallationUpdateActivity installationUpdateActivity, CancellationToken cancellationToken)
        {
            return dc.WaitForInputAsync();
        }

        /// <summary>
        /// OnCommandActivityAsync() - Invoked when a command activity is received when the base behavior of
        /// <see cref="OnTurnAsync(ITurnContext, CancellationToken)"/> is used.
        /// </summary>
        /// <remarks>
        /// the default behavior is to wait for input.
        /// 
        /// Commands are requests to perform an action and receivers typically respond with
        /// one or more commandResult activities. Receivers are also expected to explicitly
        /// reject unsupported command activities.
        /// </remarks>
        /// <param name="dc">dialog Context</param>
        /// <param name="commandActivity">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>null value if no dialog action taken, otherwise dialogturnresult to indicate dialog action that was taken</returns>
        protected virtual Task<DialogTurnResult> OnCommandActivityAsync(DialogContext dc, ICommandActivity commandActivity, CancellationToken cancellationToken)
        {
            return dc.WaitForInputAsync();
        }

        /// <summary>
        /// OnCommandResultActivityAsync() - Invoked when a CommandResult activity is received when the base behavior of
        /// <see cref="OnTurnAsync(ITurnContext, CancellationToken)"/> is used.
        /// </summary>
        /// <remarks>
        /// The default behavior is to wait for input.
        /// 
        /// CommandResult activities can be used to communicate the result of a command execution.
        /// </remarks>
        /// <param name="dc">dialog Context</param>
        /// <param name="commandResultActivity">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>null value if no dialog action taken, otherwise dialogturnresult to indicate dialog action that was taken</returns>
        protected virtual Task<DialogTurnResult> OnCommandResultActivityAsync(DialogContext dc, ICommandResultActivity commandResultActivity, CancellationToken cancellationToken)
        {
            return dc.WaitForInputAsync();
        }

        /// <summary>
        /// OnUnrecognizedActivityTypeAsync() - Invoked when an activity of an unknown type is invoked
        /// </summary>
        /// <remarks>
        /// By default, this method does nothing and waits for input.
        /// </remarks>
        /// <param name="dc">dialog context</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>null value if no dialog action taken, otherwise dialogturnresult to indicate dialog action that was taken</returns>
        protected virtual Task<DialogTurnResult> OnUnrecognizedActivityTypeAsync(DialogContext dc, Activity activity, CancellationToken cancellationToken)
        {
            return dc.WaitForInputAsync();
        }
        #endregion

        #region HELPER METHODS
        /// <summary>
        /// PromptAsync() - Begins a dialog and stores the result from the dialog in property path
        /// </summary>
        /// <typeparam name="DialogT"></typeparam>
        /// <param name="dc">dc</param>
        /// <param name="property">name of the property for the dialog result (Ex: "this.name")</param>
        /// <param name="options">options for the dialog</param>
        /// <param name="cancellationToken">ct</param>
        /// <returns>dtr</returns>
        protected Task<DialogTurnResult> PromptAsync<DialogT>(DialogContext dc, string property, object options, CancellationToken cancellationToken = default)
            where DialogT : Dialog
        {
            return PromptAsync(dc, typeof(DialogT).Name, property, options, cancellationToken);
        }

        /// <summary>
        /// PromptAsync() - Begins a dialog and stores the result from the dialog in property path
        /// </summary>
        /// <param name="dc">dc</param>
        /// <param name="dialogId">dialogId</param>
        /// <param name="property">name of the property for the dialog result (Ex: "this.name")</param>
        /// <param name="options">options for the dialog</param>
        /// <param name="cancellationToken">ct</param>
        /// <returns>dtr</returns>
        protected async Task<DialogTurnResult> PromptAsync(DialogContext dc, string dialogId, string property, object options, CancellationToken cancellationToken = default)
        {
            ObjectPath.SetPathValue(dc.State, PROPERTY_KEY, property ?? dialogId);
            return await dc.BeginDialogAsync(dialogId, options, cancellationToken);
        }

        /// <summary>
        /// An <see cref="InvokeResponse"/> factory that initializes the body to the parameter passed and status equal to OK.
        /// </summary>
        /// <param name="body">JSON serialized content from a POST response.</param>
        /// <returns>A new <see cref="InvokeResponse"/> object.</returns>
        protected static InvokeResponse CreateInvokeResponse(object body = null)
        {
            return new InvokeResponse { Status = (int)HttpStatusCode.OK, Body = body };
        }

        private static SearchInvokeValue GetSearchInvokeValue(IInvokeActivity activity)
        {
            if (activity.Value == null)
            {
                var response = CreateAdaptiveCardInvokeErrorResponse(HttpStatusCode.BadRequest, "BadRequest", "Missing value property for search");
                throw new InvokeResponseException(HttpStatusCode.BadRequest, response);
            }

            var obj = activity.Value as JObject;
            if (obj == null)
            {
                var response = CreateAdaptiveCardInvokeErrorResponse(HttpStatusCode.BadRequest, "BadRequest", "Value property is not properly formed for search");
                throw new InvokeResponseException(HttpStatusCode.BadRequest, response);
            }

            SearchInvokeValue invokeValue = null;

            try
            {
                invokeValue = obj.ToObject<SearchInvokeValue>();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                var errorResponse = CreateAdaptiveCardInvokeErrorResponse(HttpStatusCode.BadRequest, "BadRequest", "Value property is not valid for search");
                throw new InvokeResponseException(HttpStatusCode.BadRequest, errorResponse);
            }

            ValidateSearchInvokeValue(invokeValue, activity.ChannelId);
            return invokeValue;
        }

        private static void ValidateSearchInvokeValue(SearchInvokeValue searchInvokeValue, string channelId)
        {
            string missingField = null;

            if (string.IsNullOrEmpty(searchInvokeValue.Kind))
            {
                // Teams does not always send the 'kind' field. Default to 'search'.
                if (Microsoft.Bot.Connector.Channels.Msteams.Equals(channelId, StringComparison.OrdinalIgnoreCase))
                {
                    searchInvokeValue.Kind = SearchInvokeTypes.Search;
                }
                else
                {
                    missingField = "kind";
                }
            }

            if (string.IsNullOrEmpty(searchInvokeValue.QueryText))
            {
                missingField = "queryText";
            }

            if (missingField != null)
            {
                var errorResponse = CreateAdaptiveCardInvokeErrorResponse(HttpStatusCode.BadRequest, "BadRequest", $"Missing {missingField} property for search");
                throw new InvokeResponseException(HttpStatusCode.BadRequest, errorResponse);
            }
        }

        private static AdaptiveCardInvokeValue GetAdaptiveCardInvokeValue(IInvokeActivity activity)
        {
            if (activity.Value == null)
            {
                var response = CreateAdaptiveCardInvokeErrorResponse(HttpStatusCode.BadRequest, "BadRequest", "Missing value property");
                throw new InvokeResponseException(HttpStatusCode.BadRequest, response);
            }

            var obj = activity.Value as JObject;
            if (obj == null)
            {
                var response = CreateAdaptiveCardInvokeErrorResponse(HttpStatusCode.BadRequest, "BadRequest", "Value property is not properly formed");
                throw new InvokeResponseException(HttpStatusCode.BadRequest, response);
            }

            AdaptiveCardInvokeValue invokeValue = null;

            try
            {
                invokeValue = obj.ToObject<AdaptiveCardInvokeValue>();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                var response = CreateAdaptiveCardInvokeErrorResponse(HttpStatusCode.BadRequest, "BadRequest", "Value property is not properly formed");
                throw new InvokeResponseException(HttpStatusCode.BadRequest, response);
            }

            if (invokeValue.Action == null)
            {
                var response = CreateAdaptiveCardInvokeErrorResponse(HttpStatusCode.BadRequest, "BadRequest", "Missing action property");
                throw new InvokeResponseException(HttpStatusCode.BadRequest, response);
            }

            if (invokeValue.Action.Type != "Action.Execute")
            {
                var response = CreateAdaptiveCardInvokeErrorResponse(HttpStatusCode.BadRequest, "NotSupported", $"The action '{invokeValue.Action.Type}'is not supported.");
                throw new InvokeResponseException(HttpStatusCode.BadRequest, response);
            }

            return invokeValue;
        }

        private static AdaptiveCardInvokeResponse CreateAdaptiveCardInvokeErrorResponse(HttpStatusCode statusCode, string code, string message)
        {
            return new AdaptiveCardInvokeResponse()
            {
                StatusCode = (int)statusCode,
                Type = "application/vnd.microsoft.error",
                Value = new Error()
                {
                    Code = code,
                    Message = message
                }
            };
        }

        /// <summary>
        /// A custom exception for invoke response errors.
        /// </summary>
#pragma warning disable CA1064 // Exceptions should be public (we can't change this without breaking binary compat, we may consider making this type public in the future)
        protected class InvokeResponseException : Exception
#pragma warning restore CA1064 // Exceptions should be public
        {
            private readonly HttpStatusCode _statusCode;
            private readonly object _body;

            /// <summary>
            /// Initializes a new instance of the <see cref="InvokeResponseException"/> class.
            /// </summary>
            /// <param name="statusCode">The Http status code of the error.</param>
            /// <param name="body">The body of the exception. Default is null.</param>
            public InvokeResponseException(HttpStatusCode statusCode, object body = null)
            {
                _statusCode = statusCode;
                _body = body;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="InvokeResponseException"/> class.
            /// </summary>
            public InvokeResponseException()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="InvokeResponseException"/> class.
            /// </summary>
            /// <param name="message">The message that explains the reason for the exception, or an empty string.</param>
            public InvokeResponseException(string message)
                : base(message)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="InvokeResponseException"/> class.
            /// </summary>
            /// <param name="message">The message that explains the reason for the exception, or an empty string.</param>
            /// <param name="innerException">Gets the System.Exception instance that caused the current exception.</param>
            public InvokeResponseException(string message, Exception innerException)
                : base(message, innerException)
            {
            }

            /// <summary>
            /// A factory method that creates a new <see cref="InvokeResponse"/> object with the status code and body of the current object..
            /// </summary>
            /// <returns>A new <see cref="InvokeResponse"/> object.</returns>
            public InvokeResponse CreateInvokeResponse()
            {
                return new InvokeResponse
                {
                    Status = (int)_statusCode,
                    Body = _body
                };
            }
        }
        #endregion
    }
}
