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

namespace GitHubClient.Git.Reference
{
    /// <summary>
    /// Action to call GitHubClient.Git.Reference.GetAllForSubNamespace() API.
    /// </summary>
    public class GetAllForSubNamespace : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Git.Reference.GetAllForSubNamespace";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllForSubNamespace"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAllForSubNamespace([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument subNamespace.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("subNamespace")]
        public StringExpression SubNamespace  { get; set; }

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
            if (Owner != null && Name != null && SubNamespace != null && Options != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var subNamespaceValue = SubNamespace.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Git.Reference.GetAllForSubNamespace(ownerValue, nameValue, subNamespaceValue, optionsValue).ConfigureAwait(false);
            }
            if (Owner != null && Name != null && SubNamespace != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var subNamespaceValue = SubNamespace.GetValue(dc);
                return await gitHubClient.Git.Reference.GetAllForSubNamespace(ownerValue, nameValue, subNamespaceValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && SubNamespace != null && Options != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var subNamespaceValue = SubNamespace.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Git.Reference.GetAllForSubNamespace((Int64)repositoryIdValue, subNamespaceValue, optionsValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && SubNamespace != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var subNamespaceValue = SubNamespace.GetValue(dc);
                return await gitHubClient.Git.Reference.GetAllForSubNamespace((Int64)repositoryIdValue, subNamespaceValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [subNamespace] arguments missing for GitHubClient.Git.Reference.GetAllForSubNamespace");
        }
    }
}
