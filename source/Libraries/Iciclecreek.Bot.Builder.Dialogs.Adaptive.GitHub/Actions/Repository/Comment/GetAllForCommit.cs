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
    /// Action to call GitHubClient.Repository.Comment.GetAllForCommit() API.
    /// </summary>
    public class GetAllForCommit : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.Repository.Comment.GetAllForCommit";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllForCommit"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAllForCommit([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
            if (Owner != null && Name != null && Sha != null && Options != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var shaValue = Sha.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Repository.Comment.GetAllForCommit(ownerValue, nameValue, shaValue, optionsValue).ConfigureAwait(false);
            }
            if (Owner != null && Name != null && Sha != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var shaValue = Sha.GetValue(dc);
                return await gitHubClient.Repository.Comment.GetAllForCommit(ownerValue, nameValue, shaValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && Sha != null && Options != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var shaValue = Sha.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Repository.Comment.GetAllForCommit((Int64)repositoryIdValue, shaValue, optionsValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && Sha != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var shaValue = Sha.GetValue(dc);
                return await gitHubClient.Repository.Comment.GetAllForCommit((Int64)repositoryIdValue, shaValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [sha] arguments missing for GitHubClient.Repository.Comment.GetAllForCommit");
        }
    }
}
