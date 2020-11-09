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
    /// Action to call GitHubClient.Repository.Create() API.
    /// </summary>
    public class Create : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Repository.Create";

        /// <summary>
        /// Initializes a new instance of the <see cref="Create"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public Create([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument organizationLogin.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("organizationLogin")]
        public StringExpression OrganizationLogin  { get; set; }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument newRepository.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("newRepository")]
        public ObjectExpression<Octokit.NewRepository> NewRepository  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (OrganizationLogin != null && NewRepository != null)
            {
                var organizationLoginValue = OrganizationLogin.GetValue(dc);
                var newRepositoryValue = NewRepository.GetValue(dc);
                return await gitHubClient.Repository.Create(organizationLoginValue, newRepositoryValue).ConfigureAwait(false);
            }
            if (NewRepository != null)
            {
                var newRepositoryValue = NewRepository.GetValue(dc);
                return await gitHubClient.Repository.Create(newRepositoryValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [newRepository] arguments missing for GitHubClient.Repository.Create");
        }
    }
}
