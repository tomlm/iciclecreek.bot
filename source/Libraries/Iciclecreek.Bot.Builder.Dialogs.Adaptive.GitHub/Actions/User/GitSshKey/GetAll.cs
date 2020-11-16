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
    /// Action to call GitHubClient.User.GitSshKey.GetAll() API.
    /// </summary>
    public class GetAll : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.User.GitSshKey.GetAll";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAll"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAll([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument userName.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("userName")]
        public StringExpression UserName  { get; set; }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument options.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("options")]
        public ObjectExpression<Octokit.ApiOptions> Options  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (UserName != null && Options != null)
            {
                var userNameValue = UserName.GetValue(dc.State);
                var optionsValue = Options.GetValue(dc.State);
                return await gitHubClient.User.GitSshKey.GetAll(userNameValue, optionsValue).ConfigureAwait(false);
            }
            if (UserName != null)
            {
                var userNameValue = UserName.GetValue(dc.State);
                return await gitHubClient.User.GitSshKey.GetAll(userNameValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [userName] arguments missing for GitHubClient.User.GitSshKey.GetAll");
        }
    }
}
