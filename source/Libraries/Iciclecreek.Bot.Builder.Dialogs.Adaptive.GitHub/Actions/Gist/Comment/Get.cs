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
    /// Action to call GitHubClient.Gist.Comment.Get() API.
    /// </summary>
    public class Get : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Gist.Comment.Get";

        /// <summary>
        /// Initializes a new instance of the <see cref="Get"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public Get([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument commentId.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("commentId")]
        public IntExpression CommentId  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (GistId != null && CommentId != null)
            {
                var gistIdValue = GistId.GetValue(dc);
                var commentIdValue = CommentId.GetValue(dc);
                return await gitHubClient.Gist.Comment.Get(gistIdValue, (Int32)commentIdValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [gistId,commentId] arguments missing for GitHubClient.Gist.Comment.Get");
        }
    }
}
