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

namespace GitHubClient.Repository.PullRequest
{
    /// <summary>
    /// Action to call GitHubClient.Repository.PullRequest.Update() API.
    /// </summary>
    public class Update : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Repository.PullRequest.Update";

        /// <summary>
        /// Initializes a new instance of the <see cref="Update"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public Update([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument number.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("number")]
        public IntExpression Number  { get; set; }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument pullRequestUpdate.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("pullRequestUpdate")]
        public ObjectExpression<Octokit.PullRequestUpdate> PullRequestUpdate  { get; set; }

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
            if (Owner != null && Name != null && Number != null && PullRequestUpdate != null)
            {
                var ownerValue = Owner.GetValue(dc.State);
                var nameValue = Name.GetValue(dc.State);
                var numberValue = Number.GetValue(dc.State);
                var pullRequestUpdateValue = PullRequestUpdate.GetValue(dc.State);
                return await gitHubClient.Repository.PullRequest.Update(ownerValue, nameValue, (Int32)numberValue, pullRequestUpdateValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && Number != null && PullRequestUpdate != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc.State);
                var numberValue = Number.GetValue(dc.State);
                var pullRequestUpdateValue = PullRequestUpdate.GetValue(dc.State);
                return await gitHubClient.Repository.PullRequest.Update((Int64)repositoryIdValue, (Int32)numberValue, pullRequestUpdateValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [number,pullRequestUpdate] arguments missing for GitHubClient.Repository.PullRequest.Update");
        }
    }
}
