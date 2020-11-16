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
    /// Action to call GitHubClient.User.GitSshKey.Create() API.
    /// </summary>
    public class Create : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.User.GitSshKey.Create";

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
        /// (REQUIRED) Gets or sets the expression for api argument newKey.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("newKey")]
        public ObjectExpression<Octokit.NewPublicKey> NewKey  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (NewKey != null)
            {
                var newKeyValue = NewKey.GetValue(dc.State);
                return await gitHubClient.User.GitSshKey.Create(newKeyValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [newKey] arguments missing for GitHubClient.User.GitSshKey.Create");
        }
    }
}
