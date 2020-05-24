using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Iciclecreek.Bot.Builder.Dialogs.Database.SqlClient
{
    /// <summary>
    /// Execute SQL against SqlClient.
    /// </summary>
    public class ExecuteSql : Dialog
    {
        [JsonProperty("$kind")]
        public const string Kind = "Iciclecreek.Sql.Execute";

        [JsonConstructor]
        public ExecuteSql([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// SQL operation
        /// </summary>
        [JsonProperty("statements")]
        public List<string> Statements{ get; set; }

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

            var connectionString = ConnectionString.GetValue(dc.State);
            var sqlText = string.Join("\n", Statements);

            // Best practice is to scope the SqlConnection to a "using" block
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Connect to the database
                conn.Open();

                // Read rows
                SqlCommand selectCommand = new SqlCommand(sqlText, conn);
                SqlDataReader results = selectCommand.ExecuteReader();

                if (this.ResultProperty != null)
                {
                    dc.State.SetValue(this.ResultProperty.GetValue(dc.State), results);
                }

                return await dc.EndDialogAsync(result: results, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
