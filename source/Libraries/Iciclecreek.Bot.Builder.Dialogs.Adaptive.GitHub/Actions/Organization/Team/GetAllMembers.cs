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
    /// Action to call GitHubClient.Organization.Team.GetAllMembers() API.
    /// </summary>
    public class GetAllMembers : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.Organization.Team.GetAllMembers";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllMembers"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAllMembers([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (OPTIONAL) Gets or sets the expression for api argument request.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("request")]
        public ObjectExpression<Octokit.TeamMembersRequest> Request  { get; set; }

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
            if (Id != null && Request != null && Options != null)
            {
                var idValue = Id.GetValue(dc);
                var requestValue = Request.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Organization.Team.GetAllMembers((Int32)idValue, requestValue, optionsValue).ConfigureAwait(false);
            }
            if (Id != null && Options != null)
            {
                var idValue = Id.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Organization.Team.GetAllMembers((Int32)idValue, optionsValue).ConfigureAwait(false);
            }
            if (Id != null && Request != null)
            {
                var idValue = Id.GetValue(dc);
                var requestValue = Request.GetValue(dc);
                return await gitHubClient.Organization.Team.GetAllMembers((Int32)idValue, requestValue).ConfigureAwait(false);
            }
            if (Id != null)
            {
                var idValue = Id.GetValue(dc);
                return await gitHubClient.Organization.Team.GetAllMembers((Int32)idValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [id] arguments missing for GitHubClient.Organization.Team.GetAllMembers");
        }
    }
}
