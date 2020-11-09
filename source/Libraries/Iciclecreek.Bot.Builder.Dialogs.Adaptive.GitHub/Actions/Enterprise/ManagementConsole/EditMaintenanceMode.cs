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

namespace GitHubClient.Enterprise.ManagementConsole
{
    /// <summary>
    /// Action to call GitHubClient.Enterprise.ManagementConsole.EditMaintenanceMode() API.
    /// </summary>
    public class EditMaintenanceMode : GitHubAction
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "GitHub.Enterprise.ManagementConsole.EditMaintenanceMode";

        /// <summary>
        /// Initializes a new instance of the <see cref="EditMaintenanceMode"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public EditMaintenanceMode([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
           this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument maintenance.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("maintenance")]
        public ObjectExpression<Octokit.UpdateMaintenanceRequest> Maintenance  { get; set; }

        /// <summary>
        /// (REQUIRED) Gets or sets the expression for api argument managementConsolePassword.
        /// </summary>
        /// <value>
        /// The value or expression to bind to the value for the argument.
        /// </value>
        [Required()]
        [JsonProperty("managementConsolePassword")]
        public StringExpression ManagementConsolePassword  { get; set; }

        /// <inheritdoc/>
        protected override async Task<object> CallGitHubApi(DialogContext dc, Octokit.GitHubClient gitHubClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Maintenance != null && ManagementConsolePassword != null)
            {
                var maintenanceValue = Maintenance.GetValue(dc);
                var managementConsolePasswordValue = ManagementConsolePassword.GetValue(dc);
                return await gitHubClient.Enterprise.ManagementConsole.EditMaintenanceMode(maintenanceValue, managementConsolePasswordValue).ConfigureAwait(false);
            }

            throw new ArgumentNullException("Required [maintenance,managementConsolePassword] arguments missing for GitHubClient.Enterprise.ManagementConsole.EditMaintenanceMode");
        }
    }
}
