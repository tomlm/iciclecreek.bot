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
    /// Action to call GitHubClient.Migration.Migrations.Start() API.
    /// </summary>
    public class Start : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Migration.Migrations.Start";

        /// <summary>
        /// Initializes a new instance of the <see cref="Start"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public Start([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument migration.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("migration")]
        public ObjectExpression<Octokit.StartMigrationRequest> Migration  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Org != null && Migration != null)
            {
                var orgValue = Org.GetValue(dc.State);
                var migrationValue = Migration.GetValue(dc.State);
                return await gitHubClient.Migration.Migrations.Start(orgValue, migrationValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [org,migration] arguments missing for GitHubClient.Migration.Migrations.Start");
        }
    }
}
