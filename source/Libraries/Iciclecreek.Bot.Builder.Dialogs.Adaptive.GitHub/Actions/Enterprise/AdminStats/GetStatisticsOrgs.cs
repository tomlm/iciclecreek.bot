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

namespace GitHubClient.Enterprise.AdminStats
{
    /// <summary>
    /// Action to call GitHubClient.Enterprise.AdminStats.GetStatisticsOrgs() API.
    /// </summary>
    public class GetStatisticsOrgs : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Enterprise.AdminStats.GetStatisticsOrgs";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetStatisticsOrgs"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetStatisticsOrgs([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await gitHubClient.Enterprise.AdminStats.GetStatisticsOrgs().ConfigureAwait(false);
        }
    }
}
