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

namespace GitHubClient.Gist
{
    /// <summary>
    /// Action to call GitHubClient.Gist.GetAllPublic() API.
    /// </summary>
    public class GetAllPublic : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.Gist.GetAllPublic";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllPublic"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAllPublic([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument since.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("since")]
        public ObjectExpression<DateTime> Since  { get; set; }

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
            if (Since != null && Options != null)
            {
                var sinceValue = Since.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Gist.GetAllPublic(sinceValue, optionsValue).ConfigureAwait(false);
            }
            if (Options != null)
            {
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Gist.GetAllPublic(optionsValue).ConfigureAwait(false);
            }
            if (Since != null)
            {
                var sinceValue = Since.GetValue(dc);
                return await gitHubClient.Gist.GetAllPublic(sinceValue).ConfigureAwait(false);
            }
            else
            return await gitHubClient.Gist.GetAllPublic().ConfigureAwait(false);
        }
    }
}
