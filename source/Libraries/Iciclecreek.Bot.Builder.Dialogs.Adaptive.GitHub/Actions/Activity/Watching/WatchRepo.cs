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

namespace GitHubClient.Activity.Watching
{
    /// <summary>
    /// Action to call GitHubClient.Activity.Watching.WatchRepo() API.
    /// </summary>
    public class WatchRepo : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Activity.Watching.WatchRepo";

        /// <summary>
        /// Initializes a new instance of the <see cref="WatchRepo"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public WatchRepo([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument newSubscription.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("newSubscription")]
        public ObjectExpression<Octokit.NewSubscription> NewSubscription  { get; set; }

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
            if (Owner != null && Name != null && NewSubscription != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var newSubscriptionValue = NewSubscription.GetValue(dc);
                return await gitHubClient.Activity.Watching.WatchRepo(ownerValue, nameValue, newSubscriptionValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && NewSubscription != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var newSubscriptionValue = NewSubscription.GetValue(dc);
                return await gitHubClient.Activity.Watching.WatchRepo((Int64)repositoryIdValue, newSubscriptionValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [newSubscription] arguments missing for GitHubClient.Activity.Watching.WatchRepo");
        }
    }
}
