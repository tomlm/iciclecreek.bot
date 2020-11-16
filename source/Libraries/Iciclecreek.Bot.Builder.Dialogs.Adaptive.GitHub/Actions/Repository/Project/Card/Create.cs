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

namespace GitHubClient.Repository.Project.Card
{
    /// <summary>
    /// Action to call GitHubClient.Repository.Project.Card.Create() API.
    /// </summary>
    public class Create : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Repository.Project.Card.Create";

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
        /// (REQUIRED) Gets or sets the expression for api argument columnId.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("columnId")]
        public IntExpression ColumnId  { get; set; }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument newProjectCard.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("newProjectCard")]
        public ObjectExpression<Octokit.NewProjectCard> NewProjectCard  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (ColumnId != null && NewProjectCard != null)
            {
                var columnIdValue = ColumnId.GetValue(dc.State);
                var newProjectCardValue = NewProjectCard.GetValue(dc.State);
                return await gitHubClient.Repository.Project.Card.Create((Int32)columnIdValue, newProjectCardValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [columnId,newProjectCard] arguments missing for GitHubClient.Repository.Project.Card.Create");
        }
    }
}
