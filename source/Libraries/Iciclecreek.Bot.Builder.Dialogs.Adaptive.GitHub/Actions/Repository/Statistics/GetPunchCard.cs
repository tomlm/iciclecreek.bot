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

namespace GitHubClient.Repository.Statistics
{
    /// <summary>
    /// Action to call GitHubClient.Repository.Statistics.GetPunchCard() API.
    /// </summary>
    public class GetPunchCard : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Repository.Statistics.GetPunchCard";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPunchCard"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetPunchCard([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument owner.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("owner")]
        public StringExpression Owner  { get; set; }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument name.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("name")]
        public StringExpression Name  { get; set; }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument cancellationToken.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("cancellationToken")]
        public ObjectExpression<System.Threading.CancellationToken> CancellationToken  { get; set; }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument repositoryId.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("repositoryId")]
        public IntExpression RepositoryId  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Owner != null && Name != null && CancellationToken != null)
            {
                var ownerValue = Owner.GetValue(dc.State);
                var nameValue = Name.GetValue(dc.State);
                var cancellationTokenValue = CancellationToken.GetValue(dc.State);
                return await gitHubClient.Repository.Statistics.GetPunchCard(ownerValue, nameValue, cancellationTokenValue).ConfigureAwait(false);
            }
            if (Owner != null && Name != null)
            {
                var ownerValue = Owner.GetValue(dc.State);
                var nameValue = Name.GetValue(dc.State);
                return await gitHubClient.Repository.Statistics.GetPunchCard(ownerValue, nameValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && CancellationToken != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc.State);
                var cancellationTokenValue = CancellationToken.GetValue(dc.State);
                return await gitHubClient.Repository.Statistics.GetPunchCard((Int64)repositoryIdValue, cancellationTokenValue).ConfigureAwait(false);
            }
            if (RepositoryId != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc.State);
                return await gitHubClient.Repository.Statistics.GetPunchCard((Int64)repositoryIdValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [] arguments missing for GitHubClient.Repository.Statistics.GetPunchCard");
        }
    }
}
