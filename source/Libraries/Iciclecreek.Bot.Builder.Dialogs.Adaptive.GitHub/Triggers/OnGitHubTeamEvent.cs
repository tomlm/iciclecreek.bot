using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using AdaptiveExpressions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Newtonsoft.Json;

namespace Iciclecreek.Bot.Builder.Dialogs.Adaptive.GitHub.Triggers
{
    /// <summary>
    /// Trigger on github 'team' webhook event.
    /// </summary>
	public class OnGitHubTeamEvent : OnGitHubEvent
    {
        private Expression _expression = null;

        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public new const string Kind = "GitHub.OnTeamEvent";

        /// <summary>
        /// Initializes a new instance of the <see cref="OnGitHubTeamEvent"/> class.
        /// </summary>
        /// <param name="action">Optional, action value to trigger on.</param>
        /// <param name="actions">Optional, list of <see cref="Dialog"/> actions.</param>
        /// <param name="condition">Optional, condition which needs to be met for the actions to be executed.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public OnGitHubTeamEvent(string action = null, List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(action: action, actions: actions, condition: condition, callerPath: callerPath, callerLine: callerLine)
        {
        }

        /// <summary>
        /// Gets or sets the action to filter on.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Gets this activity's representing expresion.
        /// </summary>
        /// <returns>An <see cref="Expression"/> representing the activity.</returns>
        public override Expression GetExpression()
        {
            if (_expression == null)
            {
                Expression actionCondition;
                if (!String.IsNullOrEmpty(this.Action))
                {
                    actionCondition = Expression.Parse($"turn.activity.value.action == '{this.Action}''");
                }
                else
                {
                    actionCondition = Expression.OrExpression(
                        Expression.Parse("turn.activity.value.action == 'added_to_repository'"),
                        Expression.Parse("turn.activity.value.action == 'created'"),
                        Expression.Parse("turn.activity.value.action == 'deleted'"),
                        Expression.Parse("turn.activity.value.action == 'edited'"),
                        Expression.Parse("turn.activity.value.action == 'removed_from_repository'")
                    );
                }
                var propertyCondition = Expression.AndExpression(
                    Expression.Parse("exists(turn.activity.value.action)"),
                    Expression.Parse("exists(turn.activity.value.organization)"),
                    Expression.Parse("exists(turn.activity.value.sender)"),
                    Expression.Parse("exists(turn.activity.value.team)")
                );
                _expression = Expression.AndExpression(base.GetExpression(), actionCondition, propertyCondition);
            }

            return _expression;
        }
    }
}
