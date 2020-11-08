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
    /// Action to call GitHubClient.Enterprise.PreReceiveEnvironment.Create() API.
    /// </summary>
    public class Create : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.Enterprise.PreReceiveEnvironment.Create";

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
        /// (REQUIRED) Gets or sets the expression for api argument newPreReceiveEnvironment.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("newPreReceiveEnvironment")]
        public ObjectExpression<Octokit.NewPreReceiveEnvironment> NewPreReceiveEnvironment  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (NewPreReceiveEnvironment != null)
            {
                var newPreReceiveEnvironmentValue = NewPreReceiveEnvironment.GetValue(dc);
                return await gitHubClient.Enterprise.PreReceiveEnvironment.Create(newPreReceiveEnvironmentValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [newPreReceiveEnvironment] arguments missing for GitHubClient.Enterprise.PreReceiveEnvironment.Create");
        }
    }
}
