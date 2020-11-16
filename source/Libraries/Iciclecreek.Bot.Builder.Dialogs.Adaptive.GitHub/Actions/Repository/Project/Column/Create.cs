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
    /// Action to call GitHubClient.Repository.Project.Column.Create() API.
    /// </summary>
    public class Create : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Repository.Project.Column.Create";

        /// <summary>
        /// Initializes a new instance of the <see cref="Create"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public Create([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument newProjectColumn.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("newProjectColumn")]
        public ObjectExpression<Octokit.NewProjectColumn> NewProjectColumn  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (ProjectId != null && NewProjectColumn != null)
            {
                var projectIdValue = ProjectId.GetValue(dc.State);
                var newProjectColumnValue = NewProjectColumn.GetValue(dc.State);
                return await gitHubClient.Repository.Project.Column.Create((Int32)projectIdValue, newProjectColumnValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [projectId,newProjectColumn] arguments missing for GitHubClient.Repository.Project.Column.Create");
        }
    }
}
