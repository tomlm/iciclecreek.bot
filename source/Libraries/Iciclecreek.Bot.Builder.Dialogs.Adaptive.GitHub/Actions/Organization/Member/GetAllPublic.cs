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
    /// Action to call GitHubClient.Organization.Member.GetAllPublic() API.
    /// </summary>
    public class GetAllPublic : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.Organization.Member.GetAllPublic";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllPublic"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAllPublic([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
            if (Org != null && Options != null)
            {
                var orgValue = Org.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Organization.Member.GetAllPublic(orgValue, optionsValue).ConfigureAwait(false);
            }
            if (Org != null)
            {
                var orgValue = Org.GetValue(dc);
                return await gitHubClient.Organization.Member.GetAllPublic(orgValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [org] arguments missing for GitHubClient.Organization.Member.GetAllPublic");
        }
    }
}
