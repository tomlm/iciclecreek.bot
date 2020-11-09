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

namespace GitHubClient.Oauth
{
    /// <summary>
    /// Action to call GitHubClient.Oauth.GetGitHubLoginUrl() API.
    /// </summary>
    public class GetGitHubLoginUrl : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Oauth.GetGitHubLoginUrl";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetGitHubLoginUrl"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetGitHubLoginUrl([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument request.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("request")]
        public ObjectExpression<Octokit.OauthLoginRequest> Request  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Request != null)
            {
                var requestValue = Request.GetValue(dc);
                return gitHubClient.Oauth.GetGitHubLoginUrl(requestValue);
            }

            throw new ArgumentNullException("Required [request] arguments missing for GitHubClient.Oauth.GetGitHubLoginUrl");
        }
    }
}
