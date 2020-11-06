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
    /// Trigger on github 'content_reference' webhook event.
    /// </summary>
	public class OnGitHubContentReferenceEvent : OnGitHubEvent
	{
		/// <summary>
		/// Class identifier.
		/// </summary>
		[JsonProperty("$kind")]
		public new const string Kind = "Iciclecreek.OnGitHubContentReferenceEvent";

		/// <summary>
		/// Initializes a new instance of the <see cref="OnGitHubContentReferenceEvent"/> class.
		/// </summary>
		/// <param name="actions">Optional, list of <see cref="Dialog"/> actions.</param>
		/// <param name="condition">Optional, condition which needs to be met for the actions to be executed.</param>
		/// <param name="callerPath">Optional, source file full path.</param>
		/// <param name="callerLine">Optional, line number in source file.</param>
		[JsonConstructor]
		public OnGitHubContentReferenceEvent(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
            actionCondition = Expression.Parse("turn.activity.value.action == 'created'");
            var propertyCondition = Expression.AndExpression(
                Expression.Parse("exists(turn.activity.value.action)"),
                Expression.Parse("exists(turn.activity.value.content_reference)"),
                Expression.Parse("exists(turn.activity.value.installation)"),
                Expression.Parse("exists(turn.activity.value.repository)"),
                Expression.Parse("exists(turn.activity.value.sender)")
            );
            return Expression.AndExpression(base.GetExpression(), actionCondition, propertyCondition);
		}
	}
}
