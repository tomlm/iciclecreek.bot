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

namespace GitHubClient.Activity.Events
{
    /// <summary>
    /// Action to call GitHubClient.Activity.Events.GetAllUserReceivedPublic() API.
    /// </summary>
    public class GetAllUserReceivedPublic : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Activity.Events.GetAllUserReceivedPublic";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllUserReceivedPublic"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAllUserReceivedPublic([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
            if (User != null && Options != null)
            {
                var userValue = User.GetValue(dc.State);
                var optionsValue = Options.GetValue(dc.State);
                return await gitHubClient.Activity.Events.GetAllUserReceivedPublic(userValue, optionsValue).ConfigureAwait(false);
            }
            if (User != null)
            {
                var userValue = User.GetValue(dc.State);
                return await gitHubClient.Activity.Events.GetAllUserReceivedPublic(userValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [user] arguments missing for GitHubClient.Activity.Events.GetAllUserReceivedPublic");
        }
    }
}
