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
    /// Action to call GitHubClient.Repository.Project.Card.GetAll() API.
    /// </summary>
    public class GetAll : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.Repository.Project.Card.GetAll";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAll"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetAll([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (OPTIONAL) Gets or sets the expression for api argument request.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("request")]
        public ObjectExpression<Octokit.ProjectCardRequest> Request  { get; set; }

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
            if (ColumnId != null && Request != null && Options != null)
            {
                var columnIdValue = ColumnId.GetValue(dc);
                var requestValue = Request.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Repository.Project.Card.GetAll((Int32)columnIdValue, requestValue, optionsValue).ConfigureAwait(false);
            }
            if (ColumnId != null && Options != null)
            {
                var columnIdValue = ColumnId.GetValue(dc);
                var optionsValue = Options.GetValue(dc);
                return await gitHubClient.Repository.Project.Card.GetAll((Int32)columnIdValue, optionsValue).ConfigureAwait(false);
            }
            if (ColumnId != null && Request != null)
            {
                var columnIdValue = ColumnId.GetValue(dc);
                var requestValue = Request.GetValue(dc);
                return await gitHubClient.Repository.Project.Card.GetAll((Int32)columnIdValue, requestValue).ConfigureAwait(false);
            }
            if (ColumnId != null)
            {
                var columnIdValue = ColumnId.GetValue(dc);
                return await gitHubClient.Repository.Project.Card.GetAll((Int32)columnIdValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [columnId] arguments missing for GitHubClient.Repository.Project.Card.GetAll");
        }
    }
}
