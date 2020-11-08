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

namespace GitHubClient.Gist.Comment
{
    /// <summary>
    /// Action to call GitHubClient.Gist.Comment.GetAllForGist() API.
    /// </summary>
    public class GetAllForGist : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.Gist.Comment.GetAllForGist";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllForGist"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAllForGist([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument gistId.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("gistId")]
        public StringExpression GistId  { get; set; }

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
            if (GistId != null && Options != null)
            {
                var gistIdValue = GistId.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Gist.Comment.GetAllForGist(gistIdValue, optionsValue).ConfigureAwait(false);
            }
            if (GistId != null)
            {
                var gistIdValue = GistId.GetValue(dc);
                return await gitHubClient.Gist.Comment.GetAllForGist(gistIdValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [gistId] arguments missing for GitHubClient.Gist.Comment.GetAllForGist");
        }
    }
}
