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

namespace GitHubClient.User.GitSshKey
{
    /// <summary>
    /// Action to call GitHubClient.User.GitSshKey.Delete() API.
    /// </summary>
    public class Delete : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.User.GitSshKey.Delete";

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
        /// (REQUIRED) Gets or sets the expression for api argument id.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("id")]
        public IntExpression Id  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Id != null)
            {
                var idValue = Id.GetValue(dc.State);
                return gitHubClient.User.GitSshKey.Delete((Int32)idValue);
            }

            throw new ArgumentNullException("Required [id] arguments missing for GitHubClient.User.GitSshKey.Delete");
        }
    }
}
