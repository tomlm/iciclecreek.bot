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
    /// Action to call GitHubClient.Repository.Branch.UpdateRequiredStatusChecks() API.
    /// </summary>
    public class UpdateRequiredStatusChecks : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Repository.Branch.UpdateRequiredStatusChecks";

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateRequiredStatusChecks"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public UpdateRequiredStatusChecks([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument update.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("update")]
        public ObjectExpression<Octokit.BranchProtectionRequiredStatusChecksUpdate> Update  { get; set; }

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
            if (Owner != null && Name != null && Branch != null && Update != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var branchValue = Branch.GetValue(dc);
                var updateValue = Update.GetValue(dc);
                return await gitHubClient.Repository.Branch.UpdateRequiredStatusChecks(ownerValue, nameValue, branchValue, updateValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && Branch != null && Update != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var branchValue = Branch.GetValue(dc);
                var updateValue = Update.GetValue(dc);
                return await gitHubClient.Repository.Branch.UpdateRequiredStatusChecks((Int64)repositoryIdValue, branchValue, updateValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [branch,update] arguments missing for GitHubClient.Repository.Branch.UpdateRequiredStatusChecks");
        }
    }
}
