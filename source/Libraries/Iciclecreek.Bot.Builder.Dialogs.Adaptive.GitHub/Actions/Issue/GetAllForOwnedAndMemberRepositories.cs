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
    /// Action to call GitHubClient.Issue.GetAllForOwnedAndMemberRepositories() API.
    /// </summary>
    public class GetAllForOwnedAndMemberRepositories : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.Issue.GetAllForOwnedAndMemberRepositories";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllForOwnedAndMemberRepositories"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAllForOwnedAndMemberRepositories([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
                var requestValue = Request.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Issue.GetAllForOwnedAndMemberRepositories(requestValue, optionsValue).ConfigureAwait(false);
            }
            if (Options != null)
            {
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Issue.GetAllForOwnedAndMemberRepositories(optionsValue).ConfigureAwait(false);
            }
            if (Request != null)
            {
                var requestValue = Request.GetValue(dc);
                return await gitHubClient.Issue.GetAllForOwnedAndMemberRepositories(requestValue).ConfigureAwait(false);
            }
            else
            return await gitHubClient.Issue.GetAllForOwnedAndMemberRepositories().ConfigureAwait(false);
        }
    }
}
