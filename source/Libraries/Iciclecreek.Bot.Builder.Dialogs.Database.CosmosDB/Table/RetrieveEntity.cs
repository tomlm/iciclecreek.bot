using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Documents;
using Newtonsoft.Json.Linq;

namespace Iciclecreek.Bot.Builder.Dialogs.Database.Cosmos.Table
{
    /// <summary>
    /// Execute SQL against SqlClient.
    /// </summary>
    public class RetrieveEntity : BaseTableOperation
    {
        [JsonProperty("$kind")]
        public const string Kind = "Iciclecreek.Table.RetrieveEntity";

        [JsonConstructor]
        public RetrieveEntity([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

		/// <summary>
		/// Gets or sets the disabled state for the action.
		/// </summary>
		[JsonProperty("disabled")]
		public BoolExpression Disabled { get; set; }

		/// <summary>
		/// parititionkey to lookup
		/// </summary>
		[JsonProperty("partitionKey")]
		public StringExpression PartitionKey { get; set; }

		/// <summary>
		/// Entity to do operation on
		/// </summary>
		[JsonProperty("rowKey")]
		public StringExpression RowKey { get; set; }

		/// <summary>
		/// Gets or sets the property path to store the query result in.
		/// </summary>
		/// <value>
		/// The property path to store the dialog result in.
		/// </value>
		[JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
			if (this.Disabled != null && this.Disabled.GetValue(dc.State) == true)
			{
				return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
			}

			CloudTable table = GetCloudTable(dc);
			var partitionKey = PartitionKey.GetValue(dc);
			var rowKey = RowKey.GetValue(dc);

            TableResult results = await table.ExecuteAsync(TableOperation.Retrieve(partitionKey, rowKey)).ConfigureAwait(false);
			var result = EntityToJObject((DynamicTableEntity)results.Result);
			if (this.ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State), result);
            }

            return await dc.EndDialogAsync(result: result, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
	}
}
