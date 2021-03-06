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

namespace GitHubClient.Repository.Project
{
    /// <summary>
    /// Action to call GitHubClient.Repository.Project.Update() API.
    /// </summary>
    public class Update : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Repository.Project.Update";

        /// <summary>
        /// Initializes a new instance of the <see cref="Update"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public Update([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument projectUpdate.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("projectUpdate")]
        public ObjectExpression<Octokit.ProjectUpdate> ProjectUpdate  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Id != null && ProjectUpdate != null)
            {
                var idValue = Id.GetValue(dc.State);
                var projectUpdateValue = ProjectUpdate.GetValue(dc.State);
                return await gitHubClient.Repository.Project.Update((Int32)idValue, projectUpdateValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [id,projectUpdate] arguments missing for GitHubClient.Repository.Project.Update");
        }
    }
}
