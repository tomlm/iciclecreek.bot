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

namespace GitHubClient.Enterprise.Ldap
{
    /// <summary>
    /// Action to call GitHubClient.Enterprise.Ldap.QueueSyncTeamMapping() API.
    /// </summary>
    public class QueueSyncTeamMapping : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Enterprise.Ldap.QueueSyncTeamMapping";

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueSyncTeamMapping"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public QueueSyncTeamMapping([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument teamId.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("teamId")]
        public IntExpression TeamId  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (TeamId != null)
            {
                var teamIdValue = TeamId.GetValue(dc);
                return await gitHubClient.Enterprise.Ldap.QueueSyncTeamMapping((Int32)teamIdValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [teamId] arguments missing for GitHubClient.Enterprise.Ldap.QueueSyncTeamMapping");
        }
    }
}
