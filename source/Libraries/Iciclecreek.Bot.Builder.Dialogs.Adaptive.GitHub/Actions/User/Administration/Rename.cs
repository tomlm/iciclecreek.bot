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
    /// Action to call GitHubClient.User.Administration.Rename() API.
    /// </summary>
    public class Rename : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.User.Administration.Rename";

        /// <summary>
        /// Initializes a new instance of the <see cref="Rename"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public Rename([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument userRename.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("userRename")]
        public ObjectExpression<Octokit.UserRename> UserRename  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Login != null && UserRename != null)
            {
                var loginValue = Login.GetValue(dc);
                var userRenameValue = UserRename.GetValue(dc);
                return await gitHubClient.User.Administration.Rename(loginValue, userRenameValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [login,userRename] arguments missing for GitHubClient.User.Administration.Rename");
        }
    }
}
