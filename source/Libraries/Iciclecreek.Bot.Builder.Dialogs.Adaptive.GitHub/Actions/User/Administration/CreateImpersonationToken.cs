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
    /// Action to call GitHubClient.User.Administration.CreateImpersonationToken() API.
    /// </summary>
    public class CreateImpersonationToken : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.User.Administration.CreateImpersonationToken";

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateImpersonationToken"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public CreateImpersonationToken([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument login.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("login")]
        public StringExpression Login  { get; set; }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument newImpersonationToken.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("newImpersonationToken")]
        public ObjectExpression<Octokit.NewImpersonationToken> NewImpersonationToken  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Login != null && NewImpersonationToken != null)
            {
                var loginValue = Login.GetValue(dc);
                var newImpersonationTokenValue = NewImpersonationToken.GetValue(dc);
                return await gitHubClient.User.Administration.CreateImpersonationToken(loginValue, newImpersonationTokenValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [login,newImpersonationToken] arguments missing for GitHubClient.User.Administration.CreateImpersonationToken");
        }
    }
}
