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

namespace GitHubClient.Issue.Milestone
{
    /// <summary>
    /// Action to call GitHubClient.Issue.Milestone.GetAllForRepository() API.
    /// </summary>
    public class GetAllForRepository : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.Issue.Milestone.GetAllForRepository";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllForRepository"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAllForRepository([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (OPTIONAL) Gets or sets the expression for api argument request.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("request")]
        public ObjectExpression<Octokit.MilestoneRequest> Request  { get; set; }

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
            if (Owner != null && Name != null && Request != null && Options != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var requestValue = Request.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Issue.Milestone.GetAllForRepository(ownerValue, nameValue, requestValue, optionsValue).ConfigureAwait(false);
            }
            if (Owner != null && Name != null && Options != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Issue.Milestone.GetAllForRepository(ownerValue, nameValue, optionsValue).ConfigureAwait(false);
            }
            if (Owner != null && Name != null && Request != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var requestValue = Request.GetValue(dc);
                return await gitHubClient.Issue.Milestone.GetAllForRepository(ownerValue, nameValue, requestValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && Request != null && Options != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var requestValue = Request.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Issue.Milestone.GetAllForRepository((Int64)repositoryIdValue, requestValue, optionsValue).ConfigureAwait(false);
            }
            if (Owner != null && Name != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                return await gitHubClient.Issue.Milestone.GetAllForRepository(ownerValue, nameValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && Options != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Issue.Milestone.GetAllForRepository((Int64)repositoryIdValue, optionsValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && Request != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var requestValue = Request.GetValue(dc);
                return await gitHubClient.Issue.Milestone.GetAllForRepository((Int64)repositoryIdValue, requestValue).ConfigureAwait(false);
            }
            if (RepositoryId != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                return await gitHubClient.Issue.Milestone.GetAllForRepository((Int64)repositoryIdValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [] arguments missing for GitHubClient.Issue.Milestone.GetAllForRepository");
        }
    }
}
