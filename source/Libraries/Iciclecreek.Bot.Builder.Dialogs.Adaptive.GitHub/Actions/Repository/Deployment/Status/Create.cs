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

namespace GitHubClient.Repository.Deployment.Status
{
    /// <summary>
    /// Action to call GitHubClient.Repository.Deployment.Status.Create() API.
    /// </summary>
    public class Create : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Repository.Deployment.Status.Create";

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
        /// (OPTIONAL) Gets or sets the expression for api argument owner.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("owner")]
        public StringExpression Owner  { get; set; }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument name.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("name")]
        public StringExpression Name  { get; set; }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument deploymentId.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("deploymentId")]
        public IntExpression DeploymentId  { get; set; }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument newDeploymentStatus.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("newDeploymentStatus")]
        public ObjectExpression<Octokit.NewDeploymentStatus> NewDeploymentStatus  { get; set; }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument repositoryId.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("repositoryId")]
        public IntExpression RepositoryId  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Owner != null && Name != null && DeploymentId != null && NewDeploymentStatus != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var deploymentIdValue = DeploymentId.GetValue(dc);
                var newDeploymentStatusValue = NewDeploymentStatus.GetValue(dc);
                return await gitHubClient.Repository.Deployment.Status.Create(ownerValue, nameValue, (Int32)deploymentIdValue, newDeploymentStatusValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && DeploymentId != null && NewDeploymentStatus != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var deploymentIdValue = DeploymentId.GetValue(dc);
                var newDeploymentStatusValue = NewDeploymentStatus.GetValue(dc);
                return await gitHubClient.Repository.Deployment.Status.Create((Int64)repositoryIdValue, (Int32)deploymentIdValue, newDeploymentStatusValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [deploymentId,newDeploymentStatus] arguments missing for GitHubClient.Repository.Deployment.Status.Create");
        }
    }
}
