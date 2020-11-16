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
    /// Action to call GitHubClient.Check.Suite.UpdatePreferences() API.
    /// </summary>
    public class UpdatePreferences : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Check.Suite.UpdatePreferences";

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatePreferences"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public UpdatePreferences([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument preferences.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("preferences")]
        public ObjectExpression<Octokit.CheckSuitePreferences> Preferences  { get; set; }

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
            if (Owner != null && Name != null && Preferences != null)
            {
                var ownerValue = Owner.GetValue(dc.State);
                var nameValue = Name.GetValue(dc.State);
                var preferencesValue = Preferences.GetValue(dc.State);
                return await gitHubClient.Check.Suite.UpdatePreferences(ownerValue, nameValue, preferencesValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && Preferences != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc.State);
                var preferencesValue = Preferences.GetValue(dc.State);
                return await gitHubClient.Check.Suite.UpdatePreferences((Int64)repositoryIdValue, preferencesValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [preferences] arguments missing for GitHubClient.Check.Suite.UpdatePreferences");
        }
    }
}
