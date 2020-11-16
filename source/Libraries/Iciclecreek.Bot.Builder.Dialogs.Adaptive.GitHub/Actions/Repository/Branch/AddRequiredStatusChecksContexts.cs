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

namespace GitHubClient.Repository.Branch
{
    /// <summary>
    /// Action to call GitHubClient.Repository.Branch.AddRequiredStatusChecksContexts() API.
    /// </summary>
    public class AddRequiredStatusChecksContexts : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Repository.Branch.AddRequiredStatusChecksContexts";

        /// <summary>
        /// Initializes a new instance of the <see cref="AddRequiredStatusChecksContexts"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public AddRequiredStatusChecksContexts([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument branch.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("branch")]
        public StringExpression Branch  { get; set; }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument contexts.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("contexts")]
        public ArrayExpression<String> Contexts  { get; set; }

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
            if (Owner != null && Name != null && Branch != null && Contexts != null)
            {
                var ownerValue = Owner.GetValue(dc.State);
                var nameValue = Name.GetValue(dc.State);
                var branchValue = Branch.GetValue(dc.State);
                var contextsValue = Contexts.GetValue(dc.State);
                return await gitHubClient.Repository.Branch.AddRequiredStatusChecksContexts(ownerValue, nameValue, branchValue, contextsValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && Branch != null && Contexts != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc.State);
                var branchValue = Branch.GetValue(dc.State);
                var contextsValue = Contexts.GetValue(dc.State);
                return await gitHubClient.Repository.Branch.AddRequiredStatusChecksContexts((Int64)repositoryIdValue, branchValue, contextsValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [branch,contexts] arguments missing for GitHubClient.Repository.Branch.AddRequiredStatusChecksContexts");
        }
    }
}
