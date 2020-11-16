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

namespace GitHubClient.User.Followers
{
    /// <summary>
    /// Action to call GitHubClient.User.Followers.IsFollowing() API.
    /// </summary>
    public class IsFollowing : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.User.Followers.IsFollowing";

        /// <summary>
        /// Initializes a new instance of the <see cref="IsFollowing"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public IsFollowing([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument following.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("following")]
        public StringExpression Following  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Login != null && Following != null)
            {
                var loginValue = Login.GetValue(dc.State);
                var followingValue = Following.GetValue(dc.State);
                return await gitHubClient.User.Followers.IsFollowing(loginValue, followingValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [login,following] arguments missing for GitHubClient.User.Followers.IsFollowing");
        }
    }
}
