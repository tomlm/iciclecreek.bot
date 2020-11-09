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
    /// Action to call GitHubClient.Gist.Edit() API.
    /// </summary>
    public class Edit : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Gist.Edit";

        /// <summary>
        /// Initializes a new instance of the <see cref="Edit"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public Edit([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument id.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("id")]
        public StringExpression Id  { get; set; }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument gistUpdate.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("gistUpdate")]
        public ObjectExpression<Octokit.GistUpdate> GistUpdate  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Id != null && GistUpdate != null)
            {
                var idValue = Id.GetValue(dc);
                var gistUpdateValue = GistUpdate.GetValue(dc);
                return await gitHubClient.Gist.Edit(idValue, gistUpdateValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [id,gistUpdate] arguments missing for GitHubClient.Gist.Edit");
        }
    }
}
