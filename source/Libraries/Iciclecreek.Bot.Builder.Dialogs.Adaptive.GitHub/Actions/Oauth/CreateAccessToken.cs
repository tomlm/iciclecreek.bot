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
    /// Action to call GitHubClient.Oauth.CreateAccessToken() API.
    /// </summary>
    public class CreateAccessToken : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Oauth.CreateAccessToken";

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAccessToken"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public CreateAccessToken([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        public ObjectExpression<Octokit.OauthTokenRequest> Request  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Request != null)
            {
                var requestValue = Request.GetValue(dc.State);
                return await gitHubClient.Oauth.CreateAccessToken(requestValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [request] arguments missing for GitHubClient.Oauth.CreateAccessToken");
        }
    }
}
