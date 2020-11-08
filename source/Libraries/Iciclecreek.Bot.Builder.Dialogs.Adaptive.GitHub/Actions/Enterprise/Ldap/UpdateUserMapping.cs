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

namespace GitHubClient.Enterprise.Ldap
{
    /// <summary>
    /// Action to call GitHubClient.Enterprise.Ldap.UpdateUserMapping() API.
    /// </summary>
    public class UpdateUserMapping : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.Enterprise.Ldap.UpdateUserMapping";

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateUserMapping"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public UpdateUserMapping([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument userName.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("userName")]
        public StringExpression UserName  { get; set; }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument newLdapMapping.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("newLdapMapping")]
        public ObjectExpression<Octokit.NewLdapMapping> NewLdapMapping  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (UserName != null && NewLdapMapping != null)
            {
                var userNameValue = UserName.GetValue(dc);
                var newLdapMappingValue = NewLdapMapping.GetValue(dc);
                return await gitHubClient.Enterprise.Ldap.UpdateUserMapping(userNameValue, newLdapMappingValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [userName,newLdapMapping] arguments missing for GitHubClient.Enterprise.Ldap.UpdateUserMapping");
        }
    }
}
