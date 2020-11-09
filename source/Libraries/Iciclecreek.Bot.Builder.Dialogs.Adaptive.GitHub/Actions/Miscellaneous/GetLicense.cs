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

namespace GitHubClient.Miscellaneous
{
    /// <summary>
    /// Action to call GitHubClient.Miscellaneous.GetLicense() API.
    /// </summary>
    public class GetLicense : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Miscellaneous.GetLicense";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetLicense"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetLicense([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument key.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("key")]
        public StringExpression Key  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Key != null)
            {
                var keyValue = Key.GetValue(dc);
                return await gitHubClient.Miscellaneous.GetLicense(keyValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [key] arguments missing for GitHubClient.Miscellaneous.GetLicense");
        }
    }
}
