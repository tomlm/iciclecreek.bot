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
    /// Action to call GitHubClient.Repository.Content.GetArchive() API.
    /// </summary>
    public class GetArchive : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHubClient.Repository.Content.GetArchive";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetArchive"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public GetArchive([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// (OPTIONAL) Gets or sets the expression for api argument archiveFormat.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("archiveFormat")]
        public ObjectExpression<Octokit.ArchiveFormat> ArchiveFormat  { get; set; }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument reference.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("reference")]
        public StringExpression Reference  { get; set; }

        /// <summary>
        /// (OPTIONAL) Gets or sets the expression for api argument timeout.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [JsonProperty("timeout")]
        public ObjectExpression<System.TimeSpan> Timeout  { get; set; }

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
            if (Owner != null && Name != null && ArchiveFormat != null && Reference != null && Timeout != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var archiveFormatValue = ArchiveFormat.GetValue(dc);
                var referenceValue = Reference.GetValue(dc);
                var timeoutValue = Timeout.GetValue(dc);
                return await gitHubClient.Repository.Content.GetArchive(ownerValue, nameValue, archiveFormatValue, referenceValue, timeoutValue).ConfigureAwait(false);
            }
            if (Owner != null && Name != null && ArchiveFormat != null && Reference != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var archiveFormatValue = ArchiveFormat.GetValue(dc);
                var referenceValue = Reference.GetValue(dc);
                return await gitHubClient.Repository.Content.GetArchive(ownerValue, nameValue, archiveFormatValue, referenceValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && ArchiveFormat != null && Reference != null && Timeout != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var archiveFormatValue = ArchiveFormat.GetValue(dc);
                var referenceValue = Reference.GetValue(dc);
                var timeoutValue = Timeout.GetValue(dc);
                return await gitHubClient.Repository.Content.GetArchive((Int64)repositoryIdValue, archiveFormatValue, referenceValue, timeoutValue).ConfigureAwait(false);
            }
            if (Owner != null && Name != null && ArchiveFormat != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                var archiveFormatValue = ArchiveFormat.GetValue(dc);
                return await gitHubClient.Repository.Content.GetArchive(ownerValue, nameValue, archiveFormatValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && ArchiveFormat != null && Reference != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var archiveFormatValue = ArchiveFormat.GetValue(dc);
                var referenceValue = Reference.GetValue(dc);
                return await gitHubClient.Repository.Content.GetArchive((Int64)repositoryIdValue, archiveFormatValue, referenceValue).ConfigureAwait(false);
            }
            if (Owner != null && Name != null)
            {
                var ownerValue = Owner.GetValue(dc);
                var nameValue = Name.GetValue(dc);
                return await gitHubClient.Repository.Content.GetArchive(ownerValue, nameValue).ConfigureAwait(false);
            }
            if (RepositoryId != null && ArchiveFormat != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                var archiveFormatValue = ArchiveFormat.GetValue(dc);
                return await gitHubClient.Repository.Content.GetArchive((Int64)repositoryIdValue, archiveFormatValue).ConfigureAwait(false);
            }
            if (RepositoryId != null)
            {
                var repositoryIdValue = RepositoryId.GetValue(dc);
                return await gitHubClient.Repository.Content.GetArchive((Int64)repositoryIdValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [] arguments missing for GitHubClient.Repository.Content.GetArchive");
        }
    }
}
