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

namespace GitHubClient.Miscellaneous
{
    /// <summary>
    /// Action to call GitHubClient.Miscellaneous.RenderRawMarkdown() API.
    /// </summary>
    public class RenderRawMarkdown : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Miscellaneous.RenderRawMarkdown";

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderRawMarkdown"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public RenderRawMarkdown([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument markdown.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("markdown")]
        public StringExpression Markdown  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Markdown != null)
            {
                var markdownValue = Markdown.GetValue(dc);
                return await gitHubClient.Miscellaneous.RenderRawMarkdown(markdownValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [markdown] arguments missing for GitHubClient.Miscellaneous.RenderRawMarkdown");
        }
    }
}
