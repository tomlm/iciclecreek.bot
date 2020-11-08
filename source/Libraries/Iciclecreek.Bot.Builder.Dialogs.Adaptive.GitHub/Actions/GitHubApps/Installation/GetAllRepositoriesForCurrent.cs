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

namespace GitHubClient.GitHubApps.Installation
{
    /// <summary>
    /// Action to call GitHubClient.GitHubApps.Installation.GetAllRepositoriesForCurrent() API.
    /// </summary>
    public class GetAllRepositoriesForCurrent : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.GitHubApps.Installation.GetAllRepositoriesForCurrent";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllRepositoriesForCurrent"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAllRepositoriesForCurrent([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

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
            if (Options != null)
            {
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.GitHubApps.Installation.GetAllRepositoriesForCurrent(optionsValue).ConfigureAwait(false);
            }
            else
            return await gitHubClient.GitHubApps.Installation.GetAllRepositoriesForCurrent().ConfigureAwait(false);
        }
    }
}
