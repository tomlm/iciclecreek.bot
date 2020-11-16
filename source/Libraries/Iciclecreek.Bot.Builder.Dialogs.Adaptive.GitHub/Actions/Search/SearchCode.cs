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

namespace GitHubClient.Search
{
    /// <summary>
    /// Action to call GitHubClient.Search.SearchCode() API.
    /// </summary>
    public class SearchCode : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Search.SearchCode";

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchCode"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public SearchCode([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument search.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("search")]
        public ObjectExpression<Octokit.SearchCodeRequest> Search  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Search != null)
            {
                var searchValue = Search.GetValue(dc.State);
                return await gitHubClient.Search.SearchCode(searchValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [search] arguments missing for GitHubClient.Search.SearchCode");
        }
    }
}
