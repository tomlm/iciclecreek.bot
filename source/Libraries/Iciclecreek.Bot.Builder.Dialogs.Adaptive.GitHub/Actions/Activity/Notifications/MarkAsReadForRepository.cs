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
    /// Action to call GitHubClient.Activity.Notifications.MarkAsReadForRepository() API.
    /// </summary>
    public class MarkAsReadForRepository : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.Activity.Notifications.MarkAsReadForRepository";

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkAsReadForRepository"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public MarkAsReadForRepository([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument owner.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("owner")]
        public StringExpression Owner  { get; set; }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument name.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("name")]
        public StringExpression Name  { get; set; }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument markAsReadRequest.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("markAsReadRequest")]
        public ObjectExpression<Octokit.MarkAsReadRequest> MarkAsReadRequest  { get; set; }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument repositoryId.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("repositoryId")]
        public IntExpression RepositoryId  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Owner != null && Name != null && MarkAsReadRequest != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var markAsReadRequestValue = MarkAsReadRequest.GetValue(dc);
                return gitHubClient.Activity.Notifications.MarkAsReadForRepository(ownerValue, nameValue, markAsReadRequestValue);
            }
            if (Owner != null && Name != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                return gitHubClient.Activity.Notifications.MarkAsReadForRepository(ownerValue, nameValue);
            }
            if (RepositoryId != null && MarkAsReadRequest != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var markAsReadRequestValue = MarkAsReadRequest.GetValue(dc);
                return gitHubClient.Activity.Notifications.MarkAsReadForRepository((Int64)repositoryIdValue, markAsReadRequestValue);
            }
            if (RepositoryId != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                return gitHubClient.Activity.Notifications.MarkAsReadForRepository((Int64)repositoryIdValue);
            }

            throw new ArgumentNullException("Required [] arguments missing for GitHubClient.Activity.Notifications.MarkAsReadForRepository");
        }
    }
}
