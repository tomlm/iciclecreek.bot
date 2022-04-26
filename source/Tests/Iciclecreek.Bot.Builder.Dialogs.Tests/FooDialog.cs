using System.Threading;
using System.Threading.Tasks;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Lucy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using YamlConverter;

namespace Iciclecreek.Bot.Builder.Dialogs.Tests
{
    public class FooDialog : IcyDialog
    {
        public FooDialog()
        {
            this.Recognizer = new LucyRecognizer()
            {
                Intents = new string[] { "Yo", "End" },
                Model = YamlConvert.DeserializeObject<LucyDocument>(
@"
locale: en
entities:
  - name: Yo
    patterns:
      - yo

  - name: End
    patterns:
      - end
")
            };
        }

        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            dc.SaveOptions(options);
            await dc.SendActivityAsync($"Foo {options}");
            return await dc.WaitForInputAsync();
        }


        protected virtual Task<DialogTurnResult> OnYoIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            var options = dc.GetOptions<int>();
            dc.SendActivityAsync($"Yo {options}");
            return dc.WaitForInputAsync();
        }

        protected virtual Task<DialogTurnResult> OnEndIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            var options = dc.GetOptions<int>();
            dc.SendActivityAsync($"End {options}");
            return dc.EndDialogAsync();
        }
    }
}
