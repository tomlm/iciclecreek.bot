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
    public class TestDialog : IcyDialog
    {
        public TestDialog()
        {
            // create a recognizer
            this.Recognizer = new LucyRecognizer()
            {
                Intents = new string[] { "Greeting", "Goodbye", "Foo" },
                Model = YamlConvert.DeserializeObject<LucyDocument>(
@"
locale: en
entities:
  - name: Greeting
    patterns:
      - hi

  - name: Goodbye
    patterns:
      - goodbye
      - bye
      - see you

  - name: Foo
    patterns:
      - foo
")
            };
        }

        protected override async Task<DialogTurnResult> OnEndOfConversationActivityAsync(DialogContext dc, IEndOfConversationActivity endOfConversationActivity, CancellationToken cancellationToken)
        {
            await dc.SendActivityAsync("EndOfConversation");
            return await dc.CancelAllDialogsAsync();
        }

        protected async Task<DialogTurnResult> OnGreetingIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            await dc.SendActivityAsync($"Hello {dc.GetOptions<object>()}");
            return await dc.WaitForInputAsync();
        }

        protected async Task<DialogTurnResult> OnFooIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            return await dc.BeginDialog<FooDialog>(1, cancellationToken: cancellationToken);
        }

        protected async Task<DialogTurnResult> OnGoodbyeIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            await dc.SendActivityAsync($"Goodbye {dc.GetOptions<object>()}");
            return await dc.EndDialogAsync();
        }
    }
}
