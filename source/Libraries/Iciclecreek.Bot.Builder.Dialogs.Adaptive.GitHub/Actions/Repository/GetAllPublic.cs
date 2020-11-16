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

namespace GitHubClient.Repository
{
    /// <summary>
    /// Action to call GitHubClient.Repository.GetAllPublic() API.
    /// </summary>
    public class GetAllPublic : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Repository.GetAllPublic";

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
        /// (OPTIONAL) Gets or sets the expression for api argument request.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("request")]
        public ObjectExpression<Octokit.PublicRepositoryRequest> Request  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Request != null)
            {
                var requestValue = Request.GetValue(dc.State);
                return await gitHubClient.Repository.GetAllPublic(requestValue).ConfigureAwait(false);
            }
            else
            return await gitHubClient.Repository.GetAllPublic().ConfigureAwait(false);
        }
    }
}
