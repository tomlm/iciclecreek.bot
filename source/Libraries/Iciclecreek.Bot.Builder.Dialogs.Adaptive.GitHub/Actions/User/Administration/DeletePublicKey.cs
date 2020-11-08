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

namespace GitHubClient.User.Administration
{
    /// <summary>
    /// Action to call GitHubClient.User.Administration.DeletePublicKey() API.
    /// </summary>
    public class DeletePublicKey : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.User.Administration.DeletePublicKey";

        /// <summary>
        /// Initializes a new instance of the <see cref="DeletePublicKey"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public DeletePublicKey([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument keyId.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("keyId")]
        public IntExpression KeyId  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (KeyId != null)
            {
                var keyIdValue = KeyId.GetValue(dc);
                return gitHubClient.User.Administration.DeletePublicKey((Int32)keyIdValue);
            }

            throw new ArgumentNullException("Required [keyId] arguments missing for GitHubClient.User.Administration.DeletePublicKey");
        }
    }
}
