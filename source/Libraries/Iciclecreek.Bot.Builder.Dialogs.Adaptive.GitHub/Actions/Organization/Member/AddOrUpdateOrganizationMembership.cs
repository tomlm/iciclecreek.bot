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

namespace GitHubClient.Organization.Member
{
    /// <summary>
    /// Action to call GitHubClient.Organization.Member.AddOrUpdateOrganizationMembership() API.
    /// </summary>
    public class AddOrUpdateOrganizationMembership : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.Organization.Member.AddOrUpdateOrganizationMembership";

        /// <summary>
        /// Initializes a new instance of the <see cref="AddOrUpdateOrganizationMembership"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public AddOrUpdateOrganizationMembership([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument user.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("user")]
        public StringExpression User  { get; set; }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument addOrUpdateRequest.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("addOrUpdateRequest")]
        public ObjectExpression<Octokit.OrganizationMembershipUpdate> AddOrUpdateRequest  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Org != null && User != null && AddOrUpdateRequest != null)
            {
                var orgValue = Org.GetValue(dc);
                var userValue = User.GetValue(dc);
                var addOrUpdateRequestValue = AddOrUpdateRequest.GetValue(dc);
                return await gitHubClient.Organization.Member.AddOrUpdateOrganizationMembership(orgValue, userValue, addOrUpdateRequestValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [org,user,addOrUpdateRequest] arguments missing for GitHubClient.Organization.Member.AddOrUpdateOrganizationMembership");
        }
    }
}
