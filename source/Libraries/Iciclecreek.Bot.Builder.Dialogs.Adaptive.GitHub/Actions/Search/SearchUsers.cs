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
    /// Action to call GitHubClient.Search.SearchUsers() API.
    /// </summary>
    public class SearchUsers : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.Search.SearchUsers";

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchUsers"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public SearchUsers([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        public ObjectExpression<Octokit.SearchUsersRequest> Search  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Search != null)
            {
                var searchValue = Search.GetValue(dc);
                return await gitHubClient.Search.SearchUsers(searchValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [search] arguments missing for GitHubClient.Search.SearchUsers");
        }
    }
}
