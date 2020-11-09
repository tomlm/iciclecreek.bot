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
    /// Action to call GitHubClient.Organization.Team.AddOrEditMembership() API.
    /// </summary>
    public class AddOrEditMembership : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Organization.Team.AddOrEditMembership";

        /// <summary>
        /// Initializes a new instance of the <see cref="AddOrEditMembership"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public AddOrEditMembership([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument request.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("request")]
        public ObjectExpression<Octokit.UpdateTeamMembership> Request  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Id != null && Login != null && Request != null)
            {
                var idValue = Id.GetValue(dc);
                var loginValue = Login.GetValue(dc);
                var requestValue = Request.GetValue(dc);
                return await gitHubClient.Organization.Team.AddOrEditMembership((Int32)idValue, loginValue, requestValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [id,login,request] arguments missing for GitHubClient.Organization.Team.AddOrEditMembership");
        }
    }
}
