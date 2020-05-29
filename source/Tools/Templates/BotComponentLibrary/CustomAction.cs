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
    [DisplayName("Custom Action")]
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

        [Description("Disable this action")]
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        [Required]
        [DisplayName("Name")]
        [Description("This is the name property.")]
        [JsonProperty("name")]
        public StringExpression Name { get; set; }

        [DisplayName("Age")]
        [Description("This is the age property.")]
        [Required]
        [Range(0, 120)]
        [JsonProperty("age")]
        public IntExpression Age { get; set; }

        [DisplayName("Color")]
        [Description("This is the color property.")]
        [JsonProperty("color")]
        public EnumExpression<ConsoleColor> Color { get; set; }

        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, Object options = null, CancellationToken cancellationToken = default)
        {
            if (Disabled != null && this.Disabled.GetValue(dc))
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            string text = "";
            
            // bind values
            var name = Name?.GetValue(dc);
            if (name != null)
            {
                text += $"Your name is:{name}. ";
            }

            var age = Age?.GetValue(dc);
            if (age != null)
            {
                text += $"Your age is:{age}. ";
            }

            var color = Color?.GetValue(dc);
            if (color != null)
            {
                text += $"Your color is:{color}. ";
            }

            // send a response.
            await dc.Context.SendActivityAsync(text);

            return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false); 
        }
    }
}
