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

namespace GitHubClient.GitHubApps
{
    /// <summary>
    /// Action to call GitHubClient.GitHubApps.GetRepositoryInstallationForCurrent() API.
    /// </summary>
    public class GetRepositoryInstallationForCurrent : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.GitHubApps.GetRepositoryInstallationForCurrent";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetRepositoryInstallationForCurrent"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetRepositoryInstallationForCurrent([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (OPTIONAL) Gets or sets the expression for api argument repo.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("repo")]
        public StringExpression Repo  { get; set; }

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
            if (Owner != null && Repo != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var repoValue = Repo.GetValue(dc);
                return await gitHubClient.GitHubApps.GetRepositoryInstallationForCurrent(ownerValue, repoValue).ConfigureAwait(false);
            }
            if (RepositoryId != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                return await gitHubClient.GitHubApps.GetRepositoryInstallationForCurrent((Int64)repositoryIdValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [] arguments missing for GitHubClient.GitHubApps.GetRepositoryInstallationForCurrent");
        }
    }
}
