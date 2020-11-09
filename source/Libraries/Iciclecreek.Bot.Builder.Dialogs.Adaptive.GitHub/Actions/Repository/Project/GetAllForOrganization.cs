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

namespace GitHubClient.Repository.Project
{
    /// <summary>
    /// Action to call GitHubClient.Repository.Project.GetAllForOrganization() API.
    /// </summary>
    public class GetAllForOrganization : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Repository.Project.GetAllForOrganization";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllForOrganization"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAllForOrganization([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument organization.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("organization")]
        public StringExpression Organization  { get; set; }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument request.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("request")]
        public ObjectExpression<Octokit.ProjectRequest> Request  { get; set; }

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
            if (Organization != null && Request != null && Options != null)
            {
                var organizationValue = Organization.GetValue(dc);
                var requestValue = Request.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Repository.Project.GetAllForOrganization(organizationValue, requestValue, optionsValue).ConfigureAwait(false);
            }
            if (Organization != null && Request != null)
            {
                var organizationValue = Organization.GetValue(dc);
                var requestValue = Request.GetValue(dc);
                return await gitHubClient.Repository.Project.GetAllForOrganization(organizationValue, requestValue).ConfigureAwait(false);
            }
            if (Organization != null && Options != null)
            {
                var organizationValue = Organization.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Repository.Project.GetAllForOrganization(organizationValue, optionsValue).ConfigureAwait(false);
            }
            if (Organization != null)
            {
                var organizationValue = Organization.GetValue(dc);
                return await gitHubClient.Repository.Project.GetAllForOrganization(organizationValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [organization] arguments missing for GitHubClient.Repository.Project.GetAllForOrganization");
        }
    }
}
