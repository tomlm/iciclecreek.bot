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
    /// Action to call GitHubClient.Repository.Invitation.Decline() API.
    /// </summary>
    public class Decline : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Repository.Invitation.Decline";

        /// <summary>
        /// Initializes a new instance of the <see cref="Decline"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public Decline([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

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
            if (InvitationId != null)
            {
                var invitationIdValue = InvitationId.GetValue(dc.State);
                return await gitHubClient.Repository.Invitation.Decline((Int32)invitationIdValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [invitationId] arguments missing for GitHubClient.Repository.Invitation.Decline");
        }
    }
}
