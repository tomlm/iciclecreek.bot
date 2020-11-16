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

namespace GitHubClient.Issue.Assignee
{
    /// <summary>
    /// Action to call GitHubClient.Issue.Assignee.CheckAssignee() API.
    /// </summary>
    public class CheckAssignee : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Issue.Assignee.CheckAssignee";

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckAssignee"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public CheckAssignee([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument assignee.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("assignee")]
        public StringExpression Assignee  { get; set; }

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
            if (Owner != null && Name != null && Assignee != null)
            {
                var ownerValue = Owner.GetValue(dc.State);
                var nameValue = Name.GetValue(dc.State);
                var assigneeValue = Assignee.GetValue(dc.State);
                return await gitHubClient.Issue.Assignee.CheckAssignee(ownerValue, nameValue, assigneeValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && Assignee != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc.State);
                var assigneeValue = Assignee.GetValue(dc.State);
                return await gitHubClient.Issue.Assignee.CheckAssignee((Int64)repositoryIdValue, assigneeValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [assignee] arguments missing for GitHubClient.Issue.Assignee.CheckAssignee");
        }
    }
}
