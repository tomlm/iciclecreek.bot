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
    /// Action to call GitHubClient.Repository.Transfer() API.
    /// </summary>
    public class Transfer : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Repository.Transfer";

        /// <summary>
        /// Initializes a new instance of the <see cref="Transfer"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public Transfer([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument repositoryTransfer.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("repositoryTransfer")]
        public ObjectExpression<Octokit.RepositoryTransfer> RepositoryTransfer  { get; set; }

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
            if (Owner != null && Name != null && RepositoryTransfer != null)
            {
                var ownerValue = Owner.GetValue(dc.State);
                var nameValue = Name.GetValue(dc.State);
                var repositoryTransferValue = RepositoryTransfer.GetValue(dc.State);
                return await gitHubClient.Repository.Transfer(ownerValue, nameValue, repositoryTransferValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && RepositoryTransfer != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc.State);
                var repositoryTransferValue = RepositoryTransfer.GetValue(dc.State);
                return await gitHubClient.Repository.Transfer((Int64)repositoryIdValue, repositoryTransferValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [repositoryTransfer] arguments missing for GitHubClient.Repository.Transfer");
        }
    }
}
