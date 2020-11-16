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

namespace GitHubClient.Issue
{
    /// <summary>
    /// Action to call GitHubClient.Issue.GetAllForCurrent() API.
    /// </summary>
    public class GetAllForCurrent : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Issue.GetAllForCurrent";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllForCurrent"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAllForCurrent([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        public ObjectExpression<Octokit.IssueRequest> Request  { get; set; }

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
            if (Request != null && Options != null)
            {
                var requestValue = Request.GetValue(dc.State);
                var optionsValue = Options.GetValue(dc.State);
                return await gitHubClient.Issue.GetAllForCurrent(requestValue, optionsValue).ConfigureAwait(false);
            }
            if (Options != null)
            {
                var optionsValue = Options.GetValue(dc.State);
                return await gitHubClient.Issue.GetAllForCurrent(optionsValue).ConfigureAwait(false);
            }
            if (Request != null)
            {
                var requestValue = Request.GetValue(dc.State);
                return await gitHubClient.Issue.GetAllForCurrent(requestValue).ConfigureAwait(false);
            }
            else
            return await gitHubClient.Issue.GetAllForCurrent().ConfigureAwait(false);
        }
    }
}
