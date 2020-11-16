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
    /// Action to call GitHubClient.Enterprise.Ldap.UpdateTeamMapping() API.
    /// </summary>
    public class UpdateTeamMapping : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Enterprise.Ldap.UpdateTeamMapping";

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateTeamMapping"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public UpdateTeamMapping([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument newLdapMapping.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("newLdapMapping")]
        public ObjectExpression<Octokit.NewLdapMapping> NewLdapMapping  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (TeamId != null && NewLdapMapping != null)
            {
                var teamIdValue = TeamId.GetValue(dc.State);
                var newLdapMappingValue = NewLdapMapping.GetValue(dc.State);
                return await gitHubClient.Enterprise.Ldap.UpdateTeamMapping((Int32)teamIdValue, newLdapMappingValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [teamId,newLdapMapping] arguments missing for GitHubClient.Enterprise.Ldap.UpdateTeamMapping");
        }
    }
}
