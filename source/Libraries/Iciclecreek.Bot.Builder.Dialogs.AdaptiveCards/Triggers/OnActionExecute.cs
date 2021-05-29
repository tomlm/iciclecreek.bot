using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AdaptiveExpressions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Newtonsoft.Json;

namespace Iciclecreek.Bot.Builder.Dialogs.AdaptiveCards
{
    /// <summary>
    /// Actions triggered when an InvokeActivity for AdaptiveCard Action.Execute
    /// </summary>
    public class OnActionExecute : OnInvokeActivity
    {
        public const string ActionContentType = "adaptiveCard/action";
        public const string AdaptiveCardContentType = "application/vnd.microsoft.card.adaptive";

        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public new const string Kind = "IcicleCreek.OnActionExecute";

        /// <summary>
        /// Initializes a new instance of the <see cref="OnInvokeActivity"/> class.
        /// </summary>
        /// <param name="actions">Optional, list of <see cref="Dialog"/> actions.</param>
        /// <param name="condition">Optional, condition which needs to be met for the actions to be executed.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public OnActionExecute(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(actions: actions, condition: condition, callerPath: callerPath, callerLine: callerLine)
        {
        }

        /// <summary>
        /// Gets or sets the action verb to trigger on.  If not set, then this will trigger on any Action.Execute not matched.
        /// </summary>
        public string Verb { get; set; }

        /// <summary>
        /// Gets the identity for the activity.
        /// </summary>
        /// <returns>Identity.</returns>
        public override string GetIdentity()
        {
            if (Condition != null)
            {
                return $"OnActionExecute({Verb})[{Condition}]";
            }

            return $"OnActionExecute({Verb})";
        }

        protected override ActionChangeList OnCreateChangeList(ActionContext actionContext, System.Object dialogOptions = null)
        {
            var changeList = base.OnCreateChangeList(actionContext, dialogOptions);
            changeList.Turn = changeList.Turn ?? new Dictionary<string, object>();
            changeList.Turn["action"] = actionContext.State.GetValue<object>($"{TurnPath.Activity}.value.action");
            return changeList;
        }

        /// <inheritdoc/>
        protected override Expression CreateExpression()
        {
            var expression = Expression.AndExpression(Expression.Parse($"{TurnPath.Activity}.name == '{ActionContentType}'"), base.CreateExpression());
            if (!String.IsNullOrEmpty(this.Verb))
            {
                // add constraints for verb (action.verb ==> turn.activity.value.action.verb)
                expression = Expression.AndExpression(Expression.Parse($"{TurnPath.Activity}.value.action.verb == '{this.Verb.Trim()}'"), expression);
            }
            return expression;
        }
    }
}
