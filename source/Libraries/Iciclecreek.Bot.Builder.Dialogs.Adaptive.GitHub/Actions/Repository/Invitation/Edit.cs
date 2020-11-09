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
    /// Action to call GitHubClient.Repository.Invitation.Edit() API.
    /// </summary>
    public class Edit : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Repository.Invitation.Edit";

        /// <summary>
        /// Initializes a new instance of the <see cref="Edit"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public Edit([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument permissions.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("permissions")]
        public ObjectExpression<Octokit.InvitationUpdate> Permissions  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (RepositoryId != null && InvitationId != null && Permissions != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var invitationIdValue = InvitationId.GetValue(dc);
                var permissionsValue = Permissions.GetValue(dc);
                return await gitHubClient.Repository.Invitation.Edit((Int64)repositoryIdValue, (Int32)invitationIdValue, permissionsValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [repositoryId,invitationId,permissions] arguments missing for GitHubClient.Repository.Invitation.Edit");
        }
    }
}
