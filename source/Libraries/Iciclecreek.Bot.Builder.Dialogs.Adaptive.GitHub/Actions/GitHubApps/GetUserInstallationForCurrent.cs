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
    /// Action to call GitHubClient.GitHubApps.GetUserInstallationForCurrent() API.
    /// </summary>
    public class GetUserInstallationForCurrent : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.GitHubApps.GetUserInstallationForCurrent";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetUserInstallationForCurrent"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetUserInstallationForCurrent([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument user.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("user")]
        public StringExpression User  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (User != null)
            {
                var userValue = User.GetValue(dc.State);
                return await gitHubClient.GitHubApps.GetUserInstallationForCurrent(userValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [user] arguments missing for GitHubClient.GitHubApps.GetUserInstallationForCurrent");
        }
    }
}
