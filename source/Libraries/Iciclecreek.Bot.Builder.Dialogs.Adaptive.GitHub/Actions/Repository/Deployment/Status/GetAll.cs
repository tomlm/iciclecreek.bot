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
    /// Action to call GitHubClient.Repository.Deployment.Status.GetAll() API.
    /// </summary>
    public class GetAll : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Repository.Deployment.Status.GetAll";

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
        /// (OPTIONAL) Gets or sets the expression for api argument options.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("options")]
        public ObjectExpression<Octokit.ApiOptions> Options  { get; set; }

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
            if (Owner != null && Name != null && DeploymentId != null && Options != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var deploymentIdValue = DeploymentId.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Repository.Deployment.Status.GetAll(ownerValue, nameValue, (Int32)deploymentIdValue, optionsValue).ConfigureAwait(false);
            }
            if (Owner != null && Name != null && DeploymentId != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var deploymentIdValue = DeploymentId.GetValue(dc);
                return await gitHubClient.Repository.Deployment.Status.GetAll(ownerValue, nameValue, (Int32)deploymentIdValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && DeploymentId != null && Options != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var deploymentIdValue = DeploymentId.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Repository.Deployment.Status.GetAll((Int64)repositoryIdValue, (Int32)deploymentIdValue, optionsValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && DeploymentId != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var deploymentIdValue = DeploymentId.GetValue(dc);
                return await gitHubClient.Repository.Deployment.Status.GetAll((Int64)repositoryIdValue, (Int32)deploymentIdValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [deploymentId] arguments missing for GitHubClient.Repository.Deployment.Status.GetAll");
        }
    }
}
