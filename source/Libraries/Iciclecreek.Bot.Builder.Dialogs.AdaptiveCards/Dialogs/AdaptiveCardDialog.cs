using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using AdaptiveCards.Templating;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Iciclecreek.Bot.Builder.Dialogs.Database.AdaptiveCards.Dialogs
{
    /// <summary>
    /// AdaptiveCardDialog is a dialog which responds to Invoke(Action.Execute) activities only.
    /// </summary>
    /// <remarks>
    /// This class has a Template and Data Expression Property, on every OnInvokeActivity() the flow is executed it will
    /// automatically data bind the template(data) and return that as the invoke response.
    /// </remarks>
    public class AdaptiveCardDialog : DialogContainer, IDialogDependencies
    {
        private readonly string changeTurnKey = Guid.NewGuid().ToString();

        private const string AdaptiveKey = "_adaptive";
        private object syncLock = new object();
        private bool installedDependencies;
        private TriggerSelector selector = new MostSpecificSelector { Selector = new FirstSelector() };

        public const string ActionContentType = "adaptiveCard/action";

        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Iciclecreek.AdaptiveCardDialog";

        public AdaptiveCardDialog(string dialogId = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(dialogId)
        {
            RegisterSourceLocation(callerPath, callerLine);
        }


        /// <summary>
        /// CardId - this card will only respond to invoke activities for actions which match this card id.
        /// </summary>
        public StringExpression CardId { get; set; }

        /// <summary>
        /// The card template.
        /// </summary>
        [JsonProperty("template")]
        public AdaptiveCardTemplate Template { get; set; }

        /// <summary>
        /// Gets or sets optional Data to bind to as the root of the data for the template.
        /// </summary>
        /// <value>
        /// The optional data object to have as the root for binding. Default: All memory scopes (minus dialogclass/class scopes)
        /// </value>
        [JsonProperty("data")]
        public ObjectExpression<object> Data { get; set; }

        /// <summary>
        /// Gets or sets trigger handlers to respond to conditions and modify the data that the template is bound it.
        /// </summary>
        /// <value>
        /// Trigger handlers to respond to conditions. 
        /// </value>
        [JsonProperty("triggers")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public virtual List<OnCondition> Triggers { get; set; } = new List<OnCondition>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets language Generator override.
        /// </summary>
        /// <value>
        /// Language Generator override.
        /// </value>
        [JsonProperty("generator")]
        public LanguageGenerator Generator { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} should not ever be a cancellation token");
            }

            EnsureDependenciesInstalled();

            await this.CheckForVersionChangeAsync(dc, cancellationToken).ConfigureAwait(false);

            var cardId = CardId?.GetValue(dc.State);

            var activeDialogState = dc.ActiveDialog.State as Dictionary<string, object>;
            activeDialogState[AdaptiveKey] = new AdaptiveDialogState();

            var properties = new Dictionary<string, string>()
                {
                    { "DialogId", Id },
                    { "Kind", Kind }
                };
            TelemetryClient.TrackEvent("AdaptiveCardDialogStart", properties);

            // select trigger
            if (!dc.State.TryGetValue<DialogEvent>("turn.dialogEvent", out DialogEvent dialogEvent))
            {
                dialogEvent = new DialogEvent { Name = AdaptiveEvents.ActivityReceived, Value = dc.Context.Activity, Bubble = false };
                // If AdaptiveCardDialog is root dialog there may not be a dialogEvent, and conditions are 
                // looking for this dialogevent as a condition.
                dc.State.SetValue("turn.dialogEvent", dialogEvent);
            }

            var actionContext = ToActionContext(dc);
            var selection = await this.selector.SelectAsync(actionContext, cancellationToken).ConfigureAwait(false);
            if (selection.Any())
            {
                var condition = selection[0];

                await actionContext.DebuggerStepAsync(condition, dialogEvent, cancellationToken).ConfigureAwait(false);
                System.Diagnostics.Trace.TraceInformation($"Executing AdaptiveCardDialog: {Id} Rule[{condition.Id}]: {condition.GetType().Name}: {condition.GetExpression()}");

                var changes = await condition.ExecuteAsync(actionContext);
                if (changes != null && changes.Any())
                {
                    actionContext.QueueChanges(changes[0]);
                    await actionContext.ApplyChangesAsync(cancellationToken).ConfigureAwait(false);
                    var actionDC = CreateChildContext(actionContext);

                    // execute the sequence, the action should be an actionScope, so we simply start the actionAcope from the selected changelist.
                    // NOTE: We don't do any of the changelist dialog management stuff because we are always single turn response with no dialog stack.
                    var result = await actionDC.BeginDialogAsync(changes[0].Actions[0].DialogId);
                    if (result.Status == DialogTurnStatus.Waiting)
                    {
                        throw new NotSupportedException("You can't wait in an invoke activity");
                    }
                }
            }

            // --- data bind the template ----
            if (this.Template == null)
            {
                throw new Exception($"{this.Id}: a template was not provided or is not valid JSON.");
            }

            // Get data
            var data = this.Data?.GetValue(dc.State);
            if (data == null)
            {
                // template library barfs on dialogclass and class memory scopes because it tries to serialize them.
                data = dc.State.Where(kv => kv.Key != "dialogclass" && kv.Key != "class").ToDictionary(kv => kv.Key, kv2 => kv2.Value);
            }

            // bind the card and convert to JObject
            var cardJson = this.Template.Expand(data);
            var card = !String.IsNullOrEmpty(cardJson) ? JObject.Parse(cardJson) : null;

            // stamp dialogId and cardId on all Action.Execute nodes.
            // set data.cardId = cardId on Action.Execute Nodes
            foreach (var action in card.SelectTokens("$..[?(@.type=='Action.Execute')]").OfType<JObject>())
            {
                ObjectPath.SetPathValue(action, "data.dialogId", this.Id);
                ObjectPath.SetPathValue(action, "data.cardId", cardId);
            }

            // Send invoke response (new card)
            var response = new JObject()
            {
                { "statusCode", 200 },
                { "type", AdaptiveCard.ContentType},
                { "value", card }
            };
            var activity = new Activity(type: ActivityTypesEx.InvokeResponse, value: new InvokeResponse() { Status = 200, Body = response });
            await dc.Context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);

            return await dc.EndDialogAsync(null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a child <see cref="DialogContext"/> for the given context.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <returns>The child <see cref="DialogContext"/> or null if no <see cref="AdaptiveDialogState.Actions"/> are found for the given context.</returns>
        public override DialogContext CreateChildContext(DialogContext dc)
        {
            var activeDialogState = dc.ActiveDialog.State as Dictionary<string, object>;
            AdaptiveDialogState state = null;

            if (activeDialogState.TryGetValue(AdaptiveKey, out var currentState))
            {
                state = currentState as AdaptiveDialogState;
            }

            if (state == null)
            {
                state = new AdaptiveDialogState();
                activeDialogState[AdaptiveKey] = state;
            }

            if (state.Actions != null && state.Actions.Any())
            {
                var childContext = new DialogContext(this.Dialogs, dc, state.Actions.First());
                OnSetScopedServices(childContext);
                return childContext;
            }

            return null;
        }

        public IEnumerable<Dialog> GetDependencies()
        {
            EnsureDependenciesInstalled();
            yield break;
        }

        protected virtual void EnsureDependenciesInstalled()
        {
            if (!installedDependencies)
            {
                lock (this.syncLock)
                {
                    if (!installedDependencies)
                    {
                        installedDependencies = true;

                        var id = 0;
                        foreach (var trigger in Triggers)
                        {
                            if (trigger is IDialogDependencies depends)
                            {
                                foreach (var dlg in depends.GetDependencies())
                                {
                                    Dialogs.Add(dlg);
                                }
                            }

                            if (trigger.Priority == null)
                            {
                                // Constant expression defined from order
                                trigger.Priority = id;
                            }

                            if (trigger.Id == null)
                            {
                                trigger.Id = id++.ToString(CultureInfo.InvariantCulture);
                            }
                        }

                        this.selector.Initialize(Triggers, evaluate: true);
                    }
                }
            }
        }

        /// <summary>
        /// OnSetScopedServices provides ability to set scoped services for the current dialogContext.
        /// </summary>
        /// <remarks>
        /// USe dialogContext.Services.Set(object) to set a scoped object that will be inherited by all children dialogContexts.
        /// </remarks>
        /// <param name="dialogContext">dialog Context.</param>
        protected virtual void OnSetScopedServices(DialogContext dialogContext)
        {
            if (Generator != null)
            {
                dialogContext.Services.Set(this.Generator);
            }
        }

        private ActionContext ToActionContext(DialogContext dc)
        {
            var activeDialogState = dc.ActiveDialog.State as Dictionary<string, object>;
            var state = activeDialogState[AdaptiveKey] as AdaptiveDialogState;

            if (state == null)
            {
                state = new AdaptiveDialogState();
                activeDialogState[AdaptiveKey] = state;
            }

            if (state.Actions == null)
            {
                state.Actions = new List<ActionState>();
            }

            var actionContext = new ActionContext(dc.Dialogs, dc, new DialogState { DialogStack = dc.Stack }, state.Actions, changeTurnKey);
            actionContext.Parent = dc.Parent;
            return actionContext;
        }
    }
}
