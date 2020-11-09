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

namespace GitHubClient.GitHubApps.Installation
{
    /// <summary>
    /// Action to call GitHubClient.GitHubApps.Installation.GetAllRepositoriesForCurrentUser() API.
    /// </summary>
    public class GetAllRepositoriesForCurrentUser : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.GitHubApps.Installation.GetAllRepositoriesForCurrentUser";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllRepositoriesForCurrentUser"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAllRepositoriesForCurrentUser([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument installationId.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("installationId")]
        public IntExpression InstallationId  { get; set; }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument options.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("options")]
        public ObjectExpression<Octokit.ApiOptions> Options  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (InstallationId != null && Options != null)
            {
                var installationIdValue = InstallationId.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.GitHubApps.Installation.GetAllRepositoriesForCurrentUser((Int64)installationIdValue, optionsValue).ConfigureAwait(false);
            }
            if (InstallationId != null)
            {
                var installationIdValue = InstallationId.GetValue(dc);
                return await gitHubClient.GitHubApps.Installation.GetAllRepositoriesForCurrentUser((Int64)installationIdValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [installationId] arguments missing for GitHubClient.GitHubApps.Installation.GetAllRepositoriesForCurrentUser");
        }
    }
}
