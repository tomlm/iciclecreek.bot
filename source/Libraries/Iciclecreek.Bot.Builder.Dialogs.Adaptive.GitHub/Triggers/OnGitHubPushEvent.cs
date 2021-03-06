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
    /// Trigger on github 'push' webhook event.
    /// </summary>
	public class OnGitHubPushEvent : OnGitHubEvent
    {
        private Expression _expression = null;

        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public new const string Kind = "GitHub.OnPushEvent";

        /// <summary>
        /// Initializes a new instance of the <see cref="OnGitHubPushEvent"/> class.
        /// </summary>
        /// <param name="actions">Optional, list of <see cref="Dialog"/> actions.</param>
        /// <param name="condition">Optional, condition which needs to be met for the actions to be executed.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public OnGitHubPushEvent(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(actions: actions, condition: condition, callerPath: callerPath, callerLine: callerLine)
        {
        }


        /// <summary>
        /// Gets this activity's representing expresion.
        /// </summary>
        /// <returns>An <see cref="Expression"/> representing the activity.</returns>
        public override Expression GetExpression()
        {
            if (_expression == null)
            {
                Expression actionCondition;
                actionCondition = Expression.Parse("!exists(turn.activity.value.action)");
                var propertyCondition = Expression.AndExpression(
                    Expression.Parse("exists(turn.activity.value.after)"),
                    Expression.Parse("exists(turn.activity.value.before)"),
                    Expression.Parse("exists(turn.activity.value.commits)"),
                    Expression.Parse("exists(turn.activity.value.compare)"),
                    Expression.Parse("exists(turn.activity.value.created)"),
                    Expression.Parse("exists(turn.activity.value.deleted)"),
                    Expression.Parse("exists(turn.activity.value.forced)"),
                    Expression.Parse("exists(turn.activity.value.pusher)"),
                    Expression.Parse("exists(turn.activity.value.ref)"),
                    Expression.Parse("exists(turn.activity.value.repository)"),
                    Expression.Parse("exists(turn.activity.value.sender)")
                );
                _expression = Expression.AndExpression(base.GetExpression(), actionCondition, propertyCondition);
            }

            return _expression;
        }
    }
}
