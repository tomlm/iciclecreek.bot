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
    /// Action to call GitHubClient.GitHubApps.GetInstallation() API.
    /// </summary>
    public class GetInstallation : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.GitHubApps.GetInstallation";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetInstallation"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetInstallation([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (InstallationId != null)
            {
                var installationIdValue = InstallationId.GetValue(dc.State);
                return await gitHubClient.GitHubApps.GetInstallation((Int64)installationIdValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [installationId] arguments missing for GitHubClient.GitHubApps.GetInstallation");
        }
    }
}
