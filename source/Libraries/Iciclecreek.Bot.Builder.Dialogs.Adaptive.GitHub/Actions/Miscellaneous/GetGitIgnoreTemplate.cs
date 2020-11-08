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
    /// Action to call GitHubClient.Miscellaneous.GetGitIgnoreTemplate() API.
    /// </summary>
    public class GetGitIgnoreTemplate : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.Miscellaneous.GetGitIgnoreTemplate";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetGitIgnoreTemplate"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetGitIgnoreTemplate([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument templateName.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("templateName")]
        public StringExpression TemplateName  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (TemplateName != null)
            {
                var templateNameValue = TemplateName.GetValue(dc);
                return await gitHubClient.Miscellaneous.GetGitIgnoreTemplate(templateNameValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [templateName] arguments missing for GitHubClient.Miscellaneous.GetGitIgnoreTemplate");
        }
    }
}
