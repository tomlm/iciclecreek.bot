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

namespace GitHubClient.PullRequest
{
    /// <summary>
    /// Action to call GitHubClient.PullRequest.Create() API.
    /// </summary>
    public class Create : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.PullRequest.Create";

        /// <summary>
        /// Initializes a new instance of the <see cref="Create"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public Create([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument newPullRequest.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("newPullRequest")]
        public ObjectExpression<Octokit.NewPullRequest> NewPullRequest  { get; set; }

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
            if (Owner != null && Name != null && NewPullRequest != null)
            {
                var ownerValue = Owner.GetValue(dc.State);
                var nameValue = Name.GetValue(dc.State);
                var newPullRequestValue = NewPullRequest.GetValue(dc.State);
                return await gitHubClient.PullRequest.Create(ownerValue, nameValue, newPullRequestValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && NewPullRequest != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc.State);
                var newPullRequestValue = NewPullRequest.GetValue(dc.State);
                return await gitHubClient.PullRequest.Create((Int64)repositoryIdValue, newPullRequestValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [newPullRequest] arguments missing for GitHubClient.PullRequest.Create");
        }
    }
}
