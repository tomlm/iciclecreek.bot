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

namespace GitHubClient.Organization.OutsideCollaborator
{
    /// <summary>
    /// Action to call GitHubClient.Organization.OutsideCollaborator.GetAll() API.
    /// </summary>
    public class GetAll : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.Organization.OutsideCollaborator.GetAll";

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
        /// (REQUIRED) Gets or sets the expression for api argument org.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("org")]
        public StringExpression Org  { get; set; }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument filter.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("filter")]
        public ObjectExpression<Octokit.OrganizationMembersFilter> Filter  { get; set; }

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
            if (Org != null && Filter != null && Options != null)
            {
                var orgValue = Org.GetValue(dc);
                var filterValue = Filter.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Organization.OutsideCollaborator.GetAll(orgValue, filterValue, optionsValue).ConfigureAwait(false);
            }
            if (Org != null && Options != null)
            {
                var orgValue = Org.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Organization.OutsideCollaborator.GetAll(orgValue, optionsValue).ConfigureAwait(false);
            }
            if (Org != null && Filter != null)
            {
                var orgValue = Org.GetValue(dc);
                var filterValue = Filter.GetValue(dc);
                return await gitHubClient.Organization.OutsideCollaborator.GetAll(orgValue, filterValue).ConfigureAwait(false);
            }
            if (Org != null)
            {
                var orgValue = Org.GetValue(dc);
                return await gitHubClient.Organization.OutsideCollaborator.GetAll(orgValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [org] arguments missing for GitHubClient.Organization.OutsideCollaborator.GetAll");
        }
    }
}
