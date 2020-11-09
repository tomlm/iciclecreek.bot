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

namespace GitHubClient.Reaction.Issue
{
    /// <summary>
    /// Action to call GitHubClient.Reaction.Issue.GetAll() API.
    /// </summary>
    public class GetAll : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Reaction.Issue.GetAll";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAll"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAll([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument number.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("number")]
        public IntExpression Number  { get; set; }

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
            if (Owner != null && Name != null && Number != null && Options != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var numberValue = Number.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Reaction.Issue.GetAll(ownerValue, nameValue, (Int32)numberValue, optionsValue).ConfigureAwait(false);
            }
            if (Owner != null && Name != null && Number != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var numberValue = Number.GetValue(dc);
                return await gitHubClient.Reaction.Issue.GetAll(ownerValue, nameValue, (Int32)numberValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && Number != null && Options != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var numberValue = Number.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Reaction.Issue.GetAll((Int64)repositoryIdValue, (Int32)numberValue, optionsValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && Number != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var numberValue = Number.GetValue(dc);
                return await gitHubClient.Reaction.Issue.GetAll((Int64)repositoryIdValue, (Int32)numberValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [number] arguments missing for GitHubClient.Reaction.Issue.GetAll");
        }
    }
}
