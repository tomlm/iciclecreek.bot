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

namespace GitHubClient.Enterprise.PreReceiveEnvironment
{
    /// <summary>
    /// Action to call GitHubClient.Enterprise.PreReceiveEnvironment.DownloadStatus() API.
    /// </summary>
    public class DownloadStatus : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Enterprise.PreReceiveEnvironment.DownloadStatus";

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadStatus"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public DownloadStatus([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument environmentId.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("environmentId")]
        public IntExpression EnvironmentId  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (EnvironmentId != null)
            {
                var environmentIdValue = EnvironmentId.GetValue(dc.State);
                return await gitHubClient.Enterprise.PreReceiveEnvironment.DownloadStatus((Int64)environmentIdValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [environmentId] arguments missing for GitHubClient.Enterprise.PreReceiveEnvironment.DownloadStatus");
        }
    }
}
