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
    /// Action to call GitHubClient.Activity.Notifications.MarkAsRead() API.
    /// </summary>
    public class MarkAsRead : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Activity.Notifications.MarkAsRead";

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkAsRead"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public MarkAsRead([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument markAsReadRequest.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("markAsReadRequest")]
        public ObjectExpression<Octokit.MarkAsReadRequest> MarkAsReadRequest  { get; set; }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument id.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("id")]
        public IntExpression Id  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (MarkAsReadRequest != null)
            {
                var markAsReadRequestValue = MarkAsReadRequest.GetValue(dc.State);
                return gitHubClient.Activity.Notifications.MarkAsRead(markAsReadRequestValue);
            }
            if (Id != null)
            {
                var idValue = Id.GetValue(dc.State);
                return gitHubClient.Activity.Notifications.MarkAsRead((Int32)idValue);
            }
            else
            return gitHubClient.Activity.Notifications.MarkAsRead();
        }
    }
}
