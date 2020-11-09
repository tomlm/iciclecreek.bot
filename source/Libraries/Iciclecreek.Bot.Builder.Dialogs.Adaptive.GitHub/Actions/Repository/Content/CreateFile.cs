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

namespace GitHubClient.Repository.Content
{
    /// <summary>
    /// Action to call GitHubClient.Repository.Content.CreateFile() API.
    /// </summary>
    public class CreateFile : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Repository.Content.CreateFile";

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateFile"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public CreateFile([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (REQUIRED) Gets or sets the expression for api argument path.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("path")]
        public StringExpression Path  { get; set; }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument request.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("request")]
        public ObjectExpression<Octokit.CreateFileRequest> Request  { get; set; }

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
            if (Owner != null && Name != null && Path != null && Request != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var pathValue = Path.GetValue(dc);
                var requestValue = Request.GetValue(dc);
                return await gitHubClient.Repository.Content.CreateFile(ownerValue, nameValue, pathValue, requestValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && Path != null && Request != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var pathValue = Path.GetValue(dc);
                var requestValue = Request.GetValue(dc);
                return await gitHubClient.Repository.Content.CreateFile((Int64)repositoryIdValue, pathValue, requestValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [path,request] arguments missing for GitHubClient.Repository.Content.CreateFile");
        }
    }
}
