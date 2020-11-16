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

namespace GitHubClient.Repository.Comment
{
    /// <summary>
    /// Action to call GitHubClient.Repository.Comment.Create() API.
    /// </summary>
    public class Create : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Repository.Comment.Create";

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
        /// (REQUIRED) Gets or sets the expression for api argument sha.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("sha")]
        public StringExpression Sha  { get; set; }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument newCommitComment.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("newCommitComment")]
        public ObjectExpression<Octokit.NewCommitComment> NewCommitComment  { get; set; }

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
            if (Owner != null && Name != null && Sha != null && NewCommitComment != null)
            {
                var ownerValue = Owner.GetValue(dc.State);
                var nameValue = Name.GetValue(dc.State);
                var shaValue = Sha.GetValue(dc.State);
                var newCommitCommentValue = NewCommitComment.GetValue(dc.State);
                return await gitHubClient.Repository.Comment.Create(ownerValue, nameValue, shaValue, newCommitCommentValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && Sha != null && NewCommitComment != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc.State);
                var shaValue = Sha.GetValue(dc.State);
                var newCommitCommentValue = NewCommitComment.GetValue(dc.State);
                return await gitHubClient.Repository.Comment.Create((Int64)repositoryIdValue, shaValue, newCommitCommentValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [sha,newCommitComment] arguments missing for GitHubClient.Repository.Comment.Create");
        }
    }
}
