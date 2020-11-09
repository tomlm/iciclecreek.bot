using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Dialogs;
using Octokit;
using System.ComponentModel.DataAnnotations;

namespace GitHubClient.Activity.Notifications
{
    /// <summary>
    /// Action to call GitHubClient.Activity.Notifications.SetThreadSubscription() API.
    /// </summary>
    public class SetThreadSubscription : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Activity.Notifications.SetThreadSubscription";

        /// <summary>
        /// Initializes a new instance of the <see cref="SetThreadSubscription"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public SetThreadSubscription([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument id.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("id")]
        public IntExpression Id  { get; set; }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument threadSubscription.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("threadSubscription")]
        public ObjectExpression<Octokit.NewThreadSubscription> ThreadSubscription  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Id != null && ThreadSubscription != null)
            {
                var idValue = Id.GetValue(dc);
                var threadSubscriptionValue = ThreadSubscription.GetValue(dc);
                return await gitHubClient.Activity.Notifications.SetThreadSubscription((Int32)idValue, threadSubscriptionValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [id,threadSubscription] arguments missing for GitHubClient.Activity.Notifications.SetThreadSubscription");
        }
    }
}
