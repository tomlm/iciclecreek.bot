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
    /// Trigger on github 'security_advisory' webhook event.
    /// </summary>
	public class OnGitHubSecurityAdvisoryEvent : OnGitHubEvent
	{
		/// <summary>
		/// Class identifier.
		/// </summary>
		[JsonProperty("$kind")]
		public new const string Kind = "Iciclecreek.OnGitHubSecurityAdvisoryEvent";

		/// <summary>
		/// Initializes a new instance of the <see cref="OnGitHubSecurityAdvisoryEvent"/> class.
		/// </summary>
		/// <param name="action">Optional, action value to trigger on.</param>
		/// <param name="actions">Optional, list of <see cref="Dialog"/> actions.</param>
		/// <param name="condition">Optional, condition which needs to be met for the actions to be executed.</param>
		/// <param name="callerPath">Optional, source file full path.</param>
		/// <param name="callerLine">Optional, line number in source file.</param>
		[JsonConstructor]
		public OnGitHubSecurityAdvisoryEvent(string action=null, List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
			: base(action: action, actions: actions, condition: condition, callerPath: callerPath, callerLine: callerLine)
		{
		}

        /// <summary>
        /// Gets or sets the action to filter on.
        /// </summary>
        public string Action {get;set;}

		/// <summary>
		/// Gets this activity's representing expresion.
		/// </summary>
		/// <returns>An <see cref="Expression"/> representing the activity.</returns>
		public override Expression GetExpression()
		{
            Expression actionCondition;
            if (!String.IsNullOrEmpty(this.Action))
            {
                actionCondition = Expression.Parse($"turn.activity.value.action == '{this.Action}''");
            }
            else
            {
                actionCondition = Expression.OrExpression(
                    Expression.Parse("turn.activity.value.action == 'performed'"),
                    Expression.Parse("turn.activity.value.action == 'published'"),
                    Expression.Parse("turn.activity.value.action == 'updated'")
                );
            }
            var propertyCondition = Expression.AndExpression(
                Expression.Parse("exists(turn.activity.value.action)"),
                Expression.Parse("exists(turn.activity.value.security_advisory)")
            );
            return Expression.AndExpression(base.GetExpression(), actionCondition, propertyCondition);
		}
	}
}
