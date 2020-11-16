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
    /// Action to call GitHubClient.Repository.Project.CreateForRepository() API.
    /// </summary>
    public class CreateForRepository : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Repository.Project.CreateForRepository";

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateForRepository"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public CreateForRepository([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument repositoryId.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("repositoryId")]
        public IntExpression RepositoryId  { get; set; }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument newProject.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("newProject")]
        public ObjectExpression<Octokit.NewProject> NewProject  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (RepositoryId != null && NewProject != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc.State);
                var newProjectValue = NewProject.GetValue(dc.State);
                return await gitHubClient.Repository.Project.CreateForRepository((Int64)repositoryIdValue, newProjectValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [repositoryId,newProject] arguments missing for GitHubClient.Repository.Project.CreateForRepository");
        }
    }
}
