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

namespace GitHubClient.Check.Suite
{
    /// <summary>
    /// Action to call GitHubClient.Check.Suite.Rerequest() API.
    /// </summary>
    public class Rerequest : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Check.Suite.Rerequest";

        /// <summary>
        /// Initializes a new instance of the <see cref="Rerequest"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public Rerequest([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument checkSuiteId.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("checkSuiteId")]
        public IntExpression CheckSuiteId  { get; set; }

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
            if (Owner != null && Name != null && CheckSuiteId != null)
            {
                var ownerValue = Owner.GetValue(dc.State);
                var nameValue = Name.GetValue(dc.State);
                var checkSuiteIdValue = CheckSuiteId.GetValue(dc.State);
                return await gitHubClient.Check.Suite.Rerequest(ownerValue, nameValue, (Int64)checkSuiteIdValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && CheckSuiteId != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc.State);
                var checkSuiteIdValue = CheckSuiteId.GetValue(dc.State);
                return await gitHubClient.Check.Suite.Rerequest((Int64)repositoryIdValue, (Int64)checkSuiteIdValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [checkSuiteId] arguments missing for GitHubClient.Check.Suite.Rerequest");
        }
    }
}
