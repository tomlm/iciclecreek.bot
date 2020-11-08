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

namespace GitHubClient.Enterprise.PreReceiveEnvironment
{
    /// <summary>
    /// Action to call GitHubClient.Enterprise.PreReceiveEnvironment.Edit() API.
    /// </summary>
    public class Edit : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.Enterprise.PreReceiveEnvironment.Edit";

        /// <summary>
        /// Initializes a new instance of the <see cref="Edit"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public Edit([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument environmentId.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("environmentId")]
        public IntExpression EnvironmentId  { get; set; }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument updatePreReceiveEnvironment.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("updatePreReceiveEnvironment")]
        public ObjectExpression<Octokit.UpdatePreReceiveEnvironment> UpdatePreReceiveEnvironment  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (EnvironmentId != null && UpdatePreReceiveEnvironment != null)
            {
                var environmentIdValue = EnvironmentId.GetValue(dc);
                var updatePreReceiveEnvironmentValue = UpdatePreReceiveEnvironment.GetValue(dc);
                return await gitHubClient.Enterprise.PreReceiveEnvironment.Edit((Int64)environmentIdValue, updatePreReceiveEnvironmentValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [environmentId,updatePreReceiveEnvironment] arguments missing for GitHubClient.Enterprise.PreReceiveEnvironment.Edit");
        }
    }
}
