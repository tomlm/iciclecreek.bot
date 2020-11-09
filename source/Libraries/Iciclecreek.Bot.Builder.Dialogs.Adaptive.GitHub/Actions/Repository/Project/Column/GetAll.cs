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

namespace GitHubClient.Repository.Project.Column
{
    /// <summary>
    /// Action to call GitHubClient.Repository.Project.Column.GetAll() API.
    /// </summary>
    public class GetAll : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Repository.Project.Column.GetAll";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAll"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAll([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument projectId.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("projectId")]
        public IntExpression ProjectId  { get; set; }

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
            if (ProjectId != null && Options != null)
            {
                var projectIdValue = ProjectId.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Repository.Project.Column.GetAll((Int32)projectIdValue, optionsValue).ConfigureAwait(false);
            }
            if (ProjectId != null)
            {
                var projectIdValue = ProjectId.GetValue(dc);
                return await gitHubClient.Repository.Project.Column.GetAll((Int32)projectIdValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [projectId] arguments missing for GitHubClient.Repository.Project.Column.GetAll");
        }
    }
}
