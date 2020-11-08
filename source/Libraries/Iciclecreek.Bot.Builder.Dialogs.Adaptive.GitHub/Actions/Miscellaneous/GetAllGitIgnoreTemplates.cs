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
    /// Action to call GitHubClient.Miscellaneous.GetAllGitIgnoreTemplates() API.
    /// </summary>
    public class GetAllGitIgnoreTemplates : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.Miscellaneous.GetAllGitIgnoreTemplates";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllGitIgnoreTemplates"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAllGitIgnoreTemplates([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await gitHubClient.Miscellaneous.GetAllGitIgnoreTemplates().ConfigureAwait(false);
        }
    }
}
