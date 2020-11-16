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
    /// Action to call GitHubClient.Check.Suite.GetAllForReference() API.
    /// </summary>
    public class GetAllForReference : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Check.Suite.GetAllForReference";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllForReference"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAllForReference([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument reference.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("reference")]
        public StringExpression Reference  { get; set; }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument request.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("request")]
        public ObjectExpression<Octokit.CheckSuiteRequest> Request  { get; set; }

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
            if (Owner != null && Name != null && Reference != null && Request != null && Options != null)
            {
                var ownerValue = Owner.GetValue(dc.State);
                var nameValue = Name.GetValue(dc.State);
                var referenceValue = Reference.GetValue(dc.State);
                var requestValue = Request.GetValue(dc.State);
                var optionsValue = Options.GetValue(dc.State);
                return await gitHubClient.Check.Suite.GetAllForReference(ownerValue, nameValue, referenceValue, requestValue, optionsValue).ConfigureAwait(false);
            }
            if (Owner != null && Name != null && Reference != null && Request != null)
            {
                var ownerValue = Owner.GetValue(dc.State);
                var nameValue = Name.GetValue(dc.State);
                var referenceValue = Reference.GetValue(dc.State);
                var requestValue = Request.GetValue(dc.State);
                return await gitHubClient.Check.Suite.GetAllForReference(ownerValue, nameValue, referenceValue, requestValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && Reference != null && Request != null && Options != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc.State);
                var referenceValue = Reference.GetValue(dc.State);
                var requestValue = Request.GetValue(dc.State);
                var optionsValue = Options.GetValue(dc.State);
                return await gitHubClient.Check.Suite.GetAllForReference((Int64)repositoryIdValue, referenceValue, requestValue, optionsValue).ConfigureAwait(false);
            }
            if (Owner != null && Name != null && Reference != null)
            {
                var ownerValue = Owner.GetValue(dc.State);
                var nameValue = Name.GetValue(dc.State);
                var referenceValue = Reference.GetValue(dc.State);
                return await gitHubClient.Check.Suite.GetAllForReference(ownerValue, nameValue, referenceValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && Reference != null && Request != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc.State);
                var referenceValue = Reference.GetValue(dc.State);
                var requestValue = Request.GetValue(dc.State);
                return await gitHubClient.Check.Suite.GetAllForReference((Int64)repositoryIdValue, referenceValue, requestValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && Reference != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc.State);
                var referenceValue = Reference.GetValue(dc.State);
                return await gitHubClient.Check.Suite.GetAllForReference((Int64)repositoryIdValue, referenceValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [reference] arguments missing for GitHubClient.Check.Suite.GetAllForReference");
        }
    }
}
