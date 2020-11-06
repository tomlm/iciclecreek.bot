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
	public class OnGitHubStatusEvent: OnGitHubEvent
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
            actionCondition = Expression.Parse("turn.activity.value.action == null");
            var propertyCondition = Expression.AndExpression(
                Expression.Parse("turn.activity.value.branches != null"),
                Expression.Parse("turn.activity.value.commit != null"),
                Expression.Parse("turn.activity.value.context != null"),
                Expression.Parse("turn.activity.value.created_at != null"),
                Expression.Parse("turn.activity.value.description != null"),
                Expression.Parse("turn.activity.value.id != null"),
                Expression.Parse("turn.activity.value.name != null"),
                Expression.Parse("turn.activity.value.repository != null"),
                Expression.Parse("turn.activity.value.sender != null"),
                Expression.Parse("turn.activity.value.sha != null"),
                Expression.Parse("turn.activity.value.state != null"),
                Expression.Parse("turn.activity.value.target_url != null"),
                Expression.Parse("turn.activity.value.updated_at != null")
            );
            return Expression.AndExpression(base.GetExpression(), actionCondition, propertyCondition);
		}
	}
}
