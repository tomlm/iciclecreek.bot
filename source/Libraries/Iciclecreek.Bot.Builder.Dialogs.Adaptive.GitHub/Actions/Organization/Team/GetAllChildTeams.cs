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
    /// Action to call GitHubClient.Organization.Team.GetAllChildTeams() API.
    /// </summary>
    public class GetAllChildTeams : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Organization.Team.GetAllChildTeams";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllChildTeams"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAllChildTeams([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
            if (Id != null && Options != null)
            {
                var idValue = Id.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Organization.Team.GetAllChildTeams((Int32)idValue, optionsValue).ConfigureAwait(false);
            }
            if (Id != null)
            {
                var idValue = Id.GetValue(dc);
                return await gitHubClient.Organization.Team.GetAllChildTeams((Int32)idValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [id] arguments missing for GitHubClient.Organization.Team.GetAllChildTeams");
        }
    }
}
