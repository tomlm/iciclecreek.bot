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

namespace GitHubClient.Organization.OutsideCollaborator
{
    /// <summary>
    /// Action to call GitHubClient.Organization.OutsideCollaborator.ConvertFromMember() API.
    /// </summary>
    public class ConvertFromMember : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Organization.OutsideCollaborator.ConvertFromMember";

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertFromMember"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public ConvertFromMember([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument org.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("org")]
        public StringExpression Org  { get; set; }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument user.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("user")]
        public StringExpression User  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Org != null && User != null)
            {
                var orgValue = Org.GetValue(dc.State);
                var userValue = User.GetValue(dc.State);
                return await gitHubClient.Organization.OutsideCollaborator.ConvertFromMember(orgValue, userValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [org,user] arguments missing for GitHubClient.Organization.OutsideCollaborator.ConvertFromMember");
        }
    }
}
