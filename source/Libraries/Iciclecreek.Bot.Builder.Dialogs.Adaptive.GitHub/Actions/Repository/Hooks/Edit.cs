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

namespace GitHubClient.Repository.Hooks
{
    /// <summary>
    /// Action to call GitHubClient.Repository.Hooks.Edit() API.
    /// </summary>
    public class Edit : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Repository.Hooks.Edit";

        /// <summary>
        /// Initializes a new instance of the <see cref="Edit"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public Edit([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument hookId.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("hookId")]
        public IntExpression HookId  { get; set; }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument hook.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("hook")]
        public ObjectExpression<Octokit.EditRepositoryHook> Hook  { get; set; }

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
            if (Owner != null && Name != null && HookId != null && Hook != null)
            {
                var ownerValue = Owner.GetValue(dc.State);
                var nameValue = Name.GetValue(dc.State);
                var hookIdValue = HookId.GetValue(dc.State);
                var hookValue = Hook.GetValue(dc.State);
                return await gitHubClient.Repository.Hooks.Edit(ownerValue, nameValue, (Int32)hookIdValue, hookValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && HookId != null && Hook != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc.State);
                var hookIdValue = HookId.GetValue(dc.State);
                var hookValue = Hook.GetValue(dc.State);
                return await gitHubClient.Repository.Hooks.Edit((Int64)repositoryIdValue, (Int32)hookIdValue, hookValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [hookId,hook] arguments missing for GitHubClient.Repository.Hooks.Edit");
        }
    }
}
