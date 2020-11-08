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
    /// Action to call GitHubClient.Repository.Project.CreateForOrganization() API.
    /// </summary>
    public class CreateForOrganization : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.Repository.Project.CreateForOrganization";

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateForOrganization"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public CreateForOrganization([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument organization.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("organization")]
        public StringExpression Organization  { get; set; }

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
            if (Organization != null && NewProject != null)
            {
                var organizationValue = Organization.GetValue(dc);
                var newProjectValue = NewProject.GetValue(dc);
                return await gitHubClient.Repository.Project.CreateForOrganization(organizationValue, newProjectValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [organization,newProject] arguments missing for GitHubClient.Repository.Project.CreateForOrganization");
        }
    }
}
