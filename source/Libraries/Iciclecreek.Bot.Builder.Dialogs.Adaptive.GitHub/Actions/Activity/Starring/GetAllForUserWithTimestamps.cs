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

namespace GitHubClient.Activity.Starring
{
    /// <summary>
    /// Action to call GitHubClient.Activity.Starring.GetAllForUserWithTimestamps() API.
    /// </summary>
    public class GetAllForUserWithTimestamps : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Activity.Starring.GetAllForUserWithTimestamps";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllForUserWithTimestamps"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAllForUserWithTimestamps([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument user.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("user")]
        public StringExpression User  { get; set; }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument request.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("request")]
        public ObjectExpression<Octokit.StarredRequest> Request  { get; set; }

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
            if (User != null && Request != null && Options != null)
            {
                var userValue = User.GetValue(dc);
                var requestValue = Request.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Activity.Starring.GetAllForUserWithTimestamps(userValue, requestValue, optionsValue).ConfigureAwait(false);
            }
            if (User != null && Options != null)
            {
                var userValue = User.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Activity.Starring.GetAllForUserWithTimestamps(userValue, optionsValue).ConfigureAwait(false);
            }
            if (User != null && Request != null)
            {
                var userValue = User.GetValue(dc);
                var requestValue = Request.GetValue(dc);
                return await gitHubClient.Activity.Starring.GetAllForUserWithTimestamps(userValue, requestValue).ConfigureAwait(false);
            }
            if (User != null)
            {
                var userValue = User.GetValue(dc);
                return await gitHubClient.Activity.Starring.GetAllForUserWithTimestamps(userValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [user] arguments missing for GitHubClient.Activity.Starring.GetAllForUserWithTimestamps");
        }
    }
}
