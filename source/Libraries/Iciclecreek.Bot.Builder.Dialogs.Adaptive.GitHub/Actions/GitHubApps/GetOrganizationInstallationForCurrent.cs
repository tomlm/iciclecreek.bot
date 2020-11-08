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
    /// Action to call GitHubClient.GitHubApps.GetOrganizationInstallationForCurrent() API.
    /// </summary>
    public class GetOrganizationInstallationForCurrent : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.GitHubApps.GetOrganizationInstallationForCurrent";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetOrganizationInstallationForCurrent"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetOrganizationInstallationForCurrent([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument organization.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("organization")]
        public StringExpression Organization  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Organization != null)
            {
                var organizationValue = Organization.GetValue(dc);
                return await gitHubClient.GitHubApps.GetOrganizationInstallationForCurrent(organizationValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [organization] arguments missing for GitHubClient.GitHubApps.GetOrganizationInstallationForCurrent");
        }
    }
}
