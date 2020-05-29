using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BotComponentLibrary
{
    [Description("This is a custom action")]
    public class CustomAction : Dialog
    {
        [JsonProperty("$kind")]
        public const string Kind = "Contoso.CustomAction";

        [JsonConstructor]
        public CustomAction([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets the disabled state for the action.
        /// </summary>
        [JsonProperty("disabled")]
        [Description("Disable this action")]
        public BoolExpression Disabled { get; set; }

        /// <summary>
        /// Gets or sets the name property.
        /// </summary>
        [JsonProperty("name")]
        [Required]
        public StringExpression Name { get; set; }

        /// <summary>
        /// Gets or sets the name property.
        /// </summary>
        [JsonProperty("age")]
        [Required]
        public IntExpression Age { get; set; }

        public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, Object options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
