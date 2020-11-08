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

namespace GitHubClient.Repository.Invitation
{
    /// <summary>
    /// Action to call GitHubClient.Repository.Invitation.Delete() API.
    /// </summary>
    public class Delete : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.Repository.Invitation.Delete";

        /// <summary>
        /// Initializes a new instance of the <see cref="Delete"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public Delete([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument invitationId.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("invitationId")]
        public IntExpression InvitationId  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (RepositoryId != null && InvitationId != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var invitationIdValue = InvitationId.GetValue(dc);
                return await gitHubClient.Repository.Invitation.Delete((Int64)repositoryIdValue, (Int32)invitationIdValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [repositoryId,invitationId] arguments missing for GitHubClient.Repository.Invitation.Delete");
        }
    }
}
