using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Jint;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Iciclecreek.Bot.Dialogs.Javascript
{
    public class CallJavascript : Dialog
    {
        private string script = null;

        public const string Kind = "Iciclecreek.CallJavascript";

        [JsonConstructor]
        public CallJavascript([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            // enable instances of this command as debug break point
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Gets or sets javascript bound to memory run function.
        /// </summary>
        /// <example>
        /// example script:
        /// function doAction(options) {
        ///    if (options.age > 7 && _user.IsVip)
        ///        return memory.user.age* 7;
        ///    return 0;
        /// }.
        /// </example>
        /// <value>
        /// Javascript doAction(options) function.
        /// </value>
        [JsonProperty("script")]
        public string Script { get; set; }


        /// <summary>
        /// Name of the function to call.
        /// </summary>
        /// <remarks>Default is function docAction(options)</remarks>
        public string Function { get; set; } = "doAction";

        /// <summary>
        /// Gets or sets configurable options for the dialog. 
        /// </summary>
        /// <value>
        /// Configurable options for the dialog. 
        /// </value>
        [JsonProperty("options")]
        public ObjectExpression<object> Options { get; set; } = new ObjectExpression<object>();

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        /// <summary>
        /// Gets or sets the property path to store the dialog result in.
        /// </summary>
        /// <value>
        /// The property path to store the dialog result in.
        /// </value>
        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (this.Disabled != null && this.Disabled.GetValue(dc.State) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            var resourceExplorer = dc.Services.Get<ResourceExplorer>();
            await LoadScript(resourceExplorer);

            // use bindingOptions to bind to the bound options
            var boundOptions = BindOptions(dc, options);
            dc.State.SetValue(ThisPath.Options, boundOptions);

            var engine = new Engine();
            foreach (var scope in dc.State.Where(ms => ms.Key != "this"))
            {
                engine.SetValue(scope.Key, scope.Value);
            }

            var result = engine.Execute(this.script)
                .GetValue(this.Function)
                .Invoke(Jint.Native.JsValue.FromObject(engine, boundOptions ?? new object()))
                .ToObject();

            if (this.ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State), result);
            }

            return await dc.EndDialogAsync(result, cancellationToken: cancellationToken);
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}({this.Function})";
        }

        protected object BindOptions(DialogContext dc, object options)
        {
            // binding options are static definition of options with overlay of passed in options);
            var bindingOptions = (JObject)ObjectPath.Merge(this.Options.GetValue(dc.State), options ?? new JObject());
            var boundOptions = new JObject();

            foreach (var binding in bindingOptions)
            {
                // evalute the value
                var (value, error) = new ValueExpression(binding.Value).TryGetValue(dc.State);

                if (error != null)
                {
                    throw new Exception(error);
                }

                // and store in options as the result
                ObjectPath.SetPathValue(boundOptions, binding.Key, value);
            }

            return boundOptions;
        }

        private async Task LoadScript(ResourceExplorer resourceExplorer)
        {
            if (this.script == null)
            {
                if (resourceExplorer.TryGetResource(this.Script, out var resource))
                {
                    this.script = await resource.ReadTextAsync();
                }
                else
                {
                    this.script = this.Script;
                }
            }
        }
    }
}
