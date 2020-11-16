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
    /// Action to call GitHubClient.Gist.Comment.Update() API.
    /// </summary>
    public class Update : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Gist.Comment.Update";

        /// <summary>
        /// Initializes a new instance of the <see cref="Update"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public Update([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument comment.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("comment")]
        public StringExpression Comment  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (GistId != null && CommentId != null && Comment != null)
            {
                var gistIdValue = GistId.GetValue(dc.State);
                var commentIdValue = CommentId.GetValue(dc.State);
                var commentValue = Comment.GetValue(dc.State);
                return await gitHubClient.Gist.Comment.Update(gistIdValue, (Int32)commentIdValue, commentValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [gistId,commentId,comment] arguments missing for GitHubClient.Gist.Comment.Update");
        }
    }
}
