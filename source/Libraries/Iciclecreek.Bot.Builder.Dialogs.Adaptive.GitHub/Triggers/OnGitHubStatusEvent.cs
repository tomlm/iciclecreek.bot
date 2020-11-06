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
    /// Trigger on github 'status' webhook event.
    /// </summary>
	public class OnGitHubStatusEvent : OnGitHubEvent
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public new const string Kind = "Iciclecreek.OnGitHubStatusEvent";

        /// <summary>
        /// Initializes a new instance of the <see cref="OnGitHubStatusEvent"/> class.
        /// </summary>
        /// <param name="actions">Optional, list of <see cref="Dialog"/> actions.</param>
        /// <param name="condition">Optional, condition which needs to be met for the actions to be executed.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public OnGitHubStatusEvent(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(actions: actions, condition: condition, callerPath: callerPath, callerLine: callerLine)
        {
        }


        /// <summary>
        /// Gets this activity's representing expresion.
        /// </summary>
        /// <returns>An <see cref="Expression"/> representing the activity.</returns>
        public override Expression GetExpression()
        {
            Expression actionCondition;
            actionCondition = Expression.Parse("!exists(turn.activity.value.action)");
            var propertyCondition = Expression.AndExpression(
                Expression.Parse("exists(turn.activity.value.branches)"),
                Expression.Parse("exists(turn.activity.value.commit)"),
                Expression.Parse("exists(turn.activity.value.context)"),
                Expression.Parse("exists(turn.activity.value.created_at)"),
                Expression.Parse("exists(turn.activity.value.id)"),
                Expression.Parse("exists(turn.activity.value.name)"),
                Expression.Parse("exists(turn.activity.value.repository)"),
                Expression.Parse("exists(turn.activity.value.sender)"),
                Expression.Parse("exists(turn.activity.value.sha)"),
                Expression.Parse("exists(turn.activity.value.state)"),
                Expression.Parse("exists(turn.activity.value.updated_at)")
            );
            return Expression.AndExpression(base.GetExpression(), actionCondition, propertyCondition);
        }
    }
}
