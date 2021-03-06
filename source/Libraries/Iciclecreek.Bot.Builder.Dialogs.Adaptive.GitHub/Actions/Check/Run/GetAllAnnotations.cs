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

namespace GitHubClient.Check.Run
{
    /// <summary>
    /// Action to call GitHubClient.Check.Run.GetAllAnnotations() API.
    /// </summary>
    public class GetAllAnnotations : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Check.Run.GetAllAnnotations";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllAnnotations"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAllAnnotations([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument checkRunId.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("checkRunId")]
        public IntExpression CheckRunId  { get; set; }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument options.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("options")]
        public ObjectExpression<Octokit.ApiOptions> Options  { get; set; }

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
            if (Owner != null && Name != null && CheckRunId != null && Options != null)
            {
                var ownerValue = Owner.GetValue(dc.State);
                var nameValue = Name.GetValue(dc.State);
                var checkRunIdValue = CheckRunId.GetValue(dc.State);
                var optionsValue = Options.GetValue(dc.State);
                return await gitHubClient.Check.Run.GetAllAnnotations(ownerValue, nameValue, (Int64)checkRunIdValue, optionsValue).ConfigureAwait(false);
            }
            if (Owner != null && Name != null && CheckRunId != null)
            {
                var ownerValue = Owner.GetValue(dc.State);
                var nameValue = Name.GetValue(dc.State);
                var checkRunIdValue = CheckRunId.GetValue(dc.State);
                return await gitHubClient.Check.Run.GetAllAnnotations(ownerValue, nameValue, (Int64)checkRunIdValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && CheckRunId != null && Options != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc.State);
                var checkRunIdValue = CheckRunId.GetValue(dc.State);
                var optionsValue = Options.GetValue(dc.State);
                return await gitHubClient.Check.Run.GetAllAnnotations((Int64)repositoryIdValue, (Int64)checkRunIdValue, optionsValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && CheckRunId != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc.State);
                var checkRunIdValue = CheckRunId.GetValue(dc.State);
                return await gitHubClient.Check.Run.GetAllAnnotations((Int64)repositoryIdValue, (Int64)checkRunIdValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [checkRunId] arguments missing for GitHubClient.Check.Run.GetAllAnnotations");
        }
    }
}
