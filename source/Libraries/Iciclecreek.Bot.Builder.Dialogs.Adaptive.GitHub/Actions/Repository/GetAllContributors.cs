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
    /// Action to call GitHubClient.Repository.GetAllContributors() API.
    /// </summary>
    public class GetAllContributors : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.Repository.GetAllContributors";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllContributors"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAllContributors([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (OPTIONAL) Gets or sets the expression for api argument includeAnonymous.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("includeAnonymous")]
        public BoolExpression IncludeAnonymous  { get; set; }

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
            if (Owner != null && Name != null && IncludeAnonymous != null && Options != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var includeAnonymousValue = IncludeAnonymous.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Repository.GetAllContributors(ownerValue, nameValue, includeAnonymousValue, optionsValue).ConfigureAwait(false);
            }
            if (Owner != null && Name != null && Options != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Repository.GetAllContributors(ownerValue, nameValue, optionsValue).ConfigureAwait(false);
            }
            if (Owner != null && Name != null && IncludeAnonymous != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var includeAnonymousValue = IncludeAnonymous.GetValue(dc);
                return await gitHubClient.Repository.GetAllContributors(ownerValue, nameValue, includeAnonymousValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && IncludeAnonymous != null && Options != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var includeAnonymousValue = IncludeAnonymous.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Repository.GetAllContributors((Int64)repositoryIdValue, includeAnonymousValue, optionsValue).ConfigureAwait(false);
            }
            if (Owner != null && Name != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                return await gitHubClient.Repository.GetAllContributors(ownerValue, nameValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && Options != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Repository.GetAllContributors((Int64)repositoryIdValue, optionsValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && IncludeAnonymous != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var includeAnonymousValue = IncludeAnonymous.GetValue(dc);
                return await gitHubClient.Repository.GetAllContributors((Int64)repositoryIdValue, includeAnonymousValue).ConfigureAwait(false);
            }
            if (RepositoryId != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                return await gitHubClient.Repository.GetAllContributors((Int64)repositoryIdValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [] arguments missing for GitHubClient.Repository.GetAllContributors");
        }
    }
}
