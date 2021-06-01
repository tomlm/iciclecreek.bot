using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;

namespace Iciclecreek.Bot.Builder.Dialogs.AdaptiveCards.Actions
{
    public class DispatchCardActions : Dialog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Get"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public DispatchCardActions([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            if (options != null)
            {
                throw new NotSupportedException($"{nameof(options)} is not supported by this action.");
            }

            if (this.Disabled != null && this.Disabled.GetValue(dc.State) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            dynamic value = dc.Context.Activity?.Value;
            var dialogId = (string)value?.action?.data?.cardId;
            if (!String.IsNullOrEmpty(dialogId))
            {
                return await dc.BeginDialogAsync(dialogId, null, cancellationToken).ConfigureAwait(false);
            }

            return await dc.EndDialogAsync(null, cancellationToken: cancellationToken);
        }
    }
}
