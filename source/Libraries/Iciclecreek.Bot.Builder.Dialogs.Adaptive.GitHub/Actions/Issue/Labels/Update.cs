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

namespace GitHubClient.Issue.Labels
{
    /// <summary>
    /// Action to call GitHubClient.Issue.Labels.Update() API.
    /// </summary>
    public class Update : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Issue.Labels.Update";

        /// <summary>
        /// Initializes a new instance of the <see cref="Update"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public Update([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument labelName.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("labelName")]
        public StringExpression LabelName  { get; set; }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument labelUpdate.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("labelUpdate")]
        public ObjectExpression<Octokit.LabelUpdate> LabelUpdate  { get; set; }

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
            if (Owner != null && Name != null && LabelName != null && LabelUpdate != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var labelNameValue = LabelName.GetValue(dc);
                var labelUpdateValue = LabelUpdate.GetValue(dc);
                return await gitHubClient.Issue.Labels.Update(ownerValue, nameValue, labelNameValue, labelUpdateValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && LabelName != null && LabelUpdate != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var labelNameValue = LabelName.GetValue(dc);
                var labelUpdateValue = LabelUpdate.GetValue(dc);
                return await gitHubClient.Issue.Labels.Update((Int64)repositoryIdValue, labelNameValue, labelUpdateValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [labelName,labelUpdate] arguments missing for GitHubClient.Issue.Labels.Update");
        }
    }
}
