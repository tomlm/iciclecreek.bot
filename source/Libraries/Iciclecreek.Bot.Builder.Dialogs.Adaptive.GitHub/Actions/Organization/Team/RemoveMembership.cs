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
    /// Action to call GitHubClient.Organization.Team.RemoveMembership() API.
    /// </summary>
    public class RemoveMembership : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Organization.Team.RemoveMembership";

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveMembership"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public RemoveMembership([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument login.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("login")]
        public StringExpression Login  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Id != null && Login != null)
            {
                var idValue = Id.GetValue(dc.State);
                var loginValue = Login.GetValue(dc.State);
                return await gitHubClient.Organization.Team.RemoveMembership((Int32)idValue, loginValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [id,login] arguments missing for GitHubClient.Organization.Team.RemoveMembership");
        }
    }
}
