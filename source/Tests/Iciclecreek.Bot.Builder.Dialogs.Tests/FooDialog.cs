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

        protected async override Task<DialogTurnResult> OnBeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            await dc.SendReplyText($"Foo {options}");
            return await dc.WaitForInputAsync();
        }


        protected virtual async Task<DialogTurnResult> OnYoIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            var options = dc.GetOptions<int>();
            await dc.SendReplyText($"Yo {options}");
            return await dc.WaitForInputAsync();
        }

        protected virtual async Task<DialogTurnResult> OnEndIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            var options = dc.GetOptions<int>();
            await dc.SendReplyText($"End {options}");
            return await dc.EndDialogAsync();
        }
    }
}
