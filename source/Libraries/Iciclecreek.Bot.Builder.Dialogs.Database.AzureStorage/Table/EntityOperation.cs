using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json.Linq;

namespace Iciclecreek.Bot.Builder.Dialogs.Database.AzureStorage.Table
{
    /// <summary>
    /// Execute SQL against SqlClient.
    /// </summary>
    public class EntityOperation : BaseTableOperation
    {
        [JsonProperty("$kind")]
        public const string Kind = "Iciclecreek.Table.EntityOperation";

        [JsonConstructor]
        public EntityOperation([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets the disabled state for the action.
        /// </summary>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        /// <summary>
        /// Operation to perform
        /// </summary>
        [JsonProperty("operation")]
        public EnumExpression<TableOperationType> Operation { get; set; }

        /// <summary>
        /// Entity to do operation on
        /// </summary>
        [JsonProperty("entity")]
        public ObjectExpression<object> Entity { get; set; }

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
            var operation = Operation.GetValue(dc.State);
            var entity = JObjectToEntity((JObject)Entity.GetValue(dc.State));

            TableResult results = null;
            switch (operation)
            {
                case TableOperationType.Insert:
                    results = await table.ExecuteAsync(TableOperation.Insert(entity)).ConfigureAwait(false);
                    break;
                case TableOperationType.InsertOrMerge:
                    results = await table.ExecuteAsync(TableOperation.InsertOrMerge(entity)).ConfigureAwait(false);
                    break;
                case TableOperationType.InsertOrReplace:
                    results = await table.ExecuteAsync(TableOperation.InsertOrReplace(entity)).ConfigureAwait(false);
                    break;
                case TableOperationType.Merge:
                    results = await table.ExecuteAsync(TableOperation.Merge(entity)).ConfigureAwait(false);
                    break;
                case TableOperationType.Replace:
                    results = await table.ExecuteAsync(TableOperation.Replace(entity)).ConfigureAwait(false);
                    break;
                case TableOperationType.Delete:
                    results = await table.ExecuteAsync(TableOperation.Delete(entity)).ConfigureAwait(false);
                    break;
            }

            var result = EntityToJObject((DynamicTableEntity)results.Result);
            if (this.ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State), result);
            }

            return await dc.EndDialogAsync(result: result, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
	}
}
