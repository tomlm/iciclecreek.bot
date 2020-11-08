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

namespace GitHubClient.Organization.Team
{
    /// <summary>
    /// Action to call GitHubClient.Organization.Team.AddRepository() API.
    /// </summary>
    public class AddRepository : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.Organization.Team.AddRepository";

        /// <summary>
        /// Initializes a new instance of the <see cref="AddRepository"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public AddRepository([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument id.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("id")]
        public IntExpression Id  { get; set; }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument organization.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("organization")]
        public StringExpression Organization  { get; set; }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument repoName.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("repoName")]
        public StringExpression RepoName  { get; set; }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument permission.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("permission")]
        public ObjectExpression<Octokit.RepositoryPermissionRequest> Permission  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Id != null && Organization != null && RepoName != null && Permission != null)
            {
                var idValue = Id.GetValue(dc);
                var organizationValue = Organization.GetValue(dc);
                var repoNameValue = RepoName.GetValue(dc);
                var permissionValue = Permission.GetValue(dc);
                return await gitHubClient.Organization.Team.AddRepository((Int32)idValue, organizationValue, repoNameValue, permissionValue).ConfigureAwait(false);
            }
            if (Id != null && Organization != null && RepoName != null)
            {
                var idValue = Id.GetValue(dc);
                var organizationValue = Organization.GetValue(dc);
                var repoNameValue = RepoName.GetValue(dc);
                return await gitHubClient.Organization.Team.AddRepository((Int32)idValue, organizationValue, repoNameValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [id,organization,repoName] arguments missing for GitHubClient.Organization.Team.AddRepository");
        }
    }
}
