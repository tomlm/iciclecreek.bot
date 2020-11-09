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

namespace GitHubClient.PullRequest.Review
{
    /// <summary>
    /// Action to call GitHubClient.PullRequest.Review.GetAllComments() API.
    /// </summary>
    public class GetAllComments : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.PullRequest.Review.GetAllComments";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllComments"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAllComments([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument reviewId.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("reviewId")]
        public IntExpression ReviewId  { get; set; }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument options.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("options")]
        public ObjectExpression<Octokit.ApiOptions> Options  { get; set; }

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
            if (Owner != null && Name != null && Number != null && ReviewId != null && Options != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var numberValue = Number.GetValue(dc);
                var reviewIdValue = ReviewId.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.PullRequest.Review.GetAllComments(ownerValue, nameValue, (Int32)numberValue, (Int64)reviewIdValue, optionsValue).ConfigureAwait(false);
            }
            if (Owner != null && Name != null && Number != null && ReviewId != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var numberValue = Number.GetValue(dc);
                var reviewIdValue = ReviewId.GetValue(dc);
                return await gitHubClient.PullRequest.Review.GetAllComments(ownerValue, nameValue, (Int32)numberValue, (Int64)reviewIdValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && Number != null && ReviewId != null && Options != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var numberValue = Number.GetValue(dc);
                var reviewIdValue = ReviewId.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.PullRequest.Review.GetAllComments((Int64)repositoryIdValue, (Int32)numberValue, (Int64)reviewIdValue, optionsValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && Number != null && ReviewId != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var numberValue = Number.GetValue(dc);
                var reviewIdValue = ReviewId.GetValue(dc);
                return await gitHubClient.PullRequest.Review.GetAllComments((Int64)repositoryIdValue, (Int32)numberValue, (Int64)reviewIdValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [number,reviewId] arguments missing for GitHubClient.PullRequest.Review.GetAllComments");
        }
    }
}
