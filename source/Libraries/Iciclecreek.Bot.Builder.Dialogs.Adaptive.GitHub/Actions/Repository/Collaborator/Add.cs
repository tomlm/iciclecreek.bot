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

namespace GitHubClient.Repository.Collaborator
{
    /// <summary>
    /// Action to call GitHubClient.Repository.Collaborator.Add() API.
    /// </summary>
    public class Add : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Repository.Collaborator.Add";

        /// <summary>
        /// Initializes a new instance of the <see cref="Add"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public Add([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument user.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("user")]
        public StringExpression User  { get; set; }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument permission.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("permission")]
        public ObjectExpression<Octokit.CollaboratorRequest> Permission  { get; set; }

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
            if (Owner != null && Name != null && User != null && Permission != null)
            {
                var ownerValue = Owner.GetValue(dc.State);
                var nameValue = Name.GetValue(dc.State);
                var userValue = User.GetValue(dc.State);
                var permissionValue = Permission.GetValue(dc.State);
                return await gitHubClient.Repository.Collaborator.Add(ownerValue, nameValue, userValue, permissionValue).ConfigureAwait(false);
            }
            if (Owner != null && Name != null && User != null)
            {
                var ownerValue = Owner.GetValue(dc.State);
                var nameValue = Name.GetValue(dc.State);
                var userValue = User.GetValue(dc.State);
                return gitHubClient.Repository.Collaborator.Add(ownerValue, nameValue, userValue);
            }
            if (RepositoryId != null && User != null && Permission != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc.State);
                var userValue = User.GetValue(dc.State);
                var permissionValue = Permission.GetValue(dc.State);
                return await gitHubClient.Repository.Collaborator.Add((Int64)repositoryIdValue, userValue, permissionValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && User != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc.State);
                var userValue = User.GetValue(dc.State);
                return gitHubClient.Repository.Collaborator.Add((Int64)repositoryIdValue, userValue);
            }

            throw new ArgumentNullException("Required [user] arguments missing for GitHubClient.Repository.Collaborator.Add");
        }
    }
}
