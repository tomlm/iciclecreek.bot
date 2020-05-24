using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace Iciclecreek.Bot.Builder.Dialogs.Database.AzureStorage.Table
{
    /// <summary>
    /// Execute SQL against SqlClient.
    /// </summary>
    public class DeleteTable : Dialog
    {
        [JsonProperty("$kind")]
        public const string Kind = "Iciclecreek.Table.DeleteTable";

        [JsonConstructor]
        public DeleteTable([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets the disabled state for the action.
        /// </summary>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        /// <summary>
        /// Gets or sets the ConnectionString for querying the database.
        /// </summary>
        [JsonProperty("connectionString")]
        public StringExpression ConnectionString { get; set; }

        /// <summary>
        /// Table name
        /// </summary>
        [JsonProperty("table")]
        public StringExpression Table { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this.Disabled != null && this.Disabled.GetValue(dc.State) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var connectionString = ConnectionString.GetValue(dc.State);
            var tableName = Table.GetValue(dc.State);

            // Retrieve storage account information from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create a table client for interacting with the table service
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            // Create a table client for interacting with the table service 
            CloudTable table = tableClient.GetTableReference(tableName);
            var result = await table.DeleteIfExistsAsync().ConfigureAwait(false);

            return await dc.EndDialogAsync(result: result, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
	}
}
