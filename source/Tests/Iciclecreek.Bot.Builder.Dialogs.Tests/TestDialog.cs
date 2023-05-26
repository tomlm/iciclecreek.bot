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
            AddDialog(new FooDialog());

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

        protected override Task<RecognizerResult> RecognizeAsync(DialogContext dc, IMessageActivity activity, CancellationToken cancellationToken = default)
            => base.RecognizeAsync(dc, activity, cancellationToken);

        protected override async Task<DialogTurnResult> OnEndOfConversationActivityAsync(DialogContext dc, IEndOfConversationActivity endOfConversationActivity, CancellationToken cancellationToken)
        {
            await dc.SendReplyText(cancellationToken, "EndOfConversation");
            return await dc.CancelAllDialogsAsync();
        }

        protected async Task<DialogTurnResult> OnGreetingIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            await dc.SendReplyText(cancellationToken, $"Hello {dc.GetOptions<object>()}");
            return await dc.WaitForInputAsync(cancellationToken);
        }

        protected async Task<DialogTurnResult> OnFooIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            await dc.SendReplyText(cancellationToken);
            return await dc.BeginDialogAsync<FooDialog>(1, cancellationToken: cancellationToken);
        }

        protected async Task<DialogTurnResult> OnGoodbyeIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            await dc.SendReplyText(cancellationToken, $"Goodbye {dc.GetOptions<object>()}");
            return await dc.EndDialogAsync();
        }
    }
}
