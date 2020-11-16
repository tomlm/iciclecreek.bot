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

namespace GitHubClient.Migration.Migrations
{
    /// <summary>
    /// Action to call GitHubClient.Migration.Migrations.DeleteArchive() API.
    /// </summary>
    public class DeleteArchive : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Migration.Migrations.DeleteArchive";

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteArchive"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public DeleteArchive([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument id.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("id")]
        public IntExpression Id  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Org != null && Id != null)
            {
                var orgValue = Org.GetValue(dc.State);
                var idValue = Id.GetValue(dc.State);
                return gitHubClient.Migration.Migrations.DeleteArchive(orgValue, (Int32)idValue);
            }

            throw new ArgumentNullException("Required [org,id] arguments missing for GitHubClient.Migration.Migrations.DeleteArchive");
        }
    }
}
