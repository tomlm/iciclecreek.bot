using System.Linq;
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
    public class PathChangedDialog : IcyDialog
    {
        public PathChangedDialog()
        {
            this.Recognizer = new LucyRecognizer()
            {
                Intents = new string[] { "ChangeInfo", "Greeting"},
                Model = YamlConvert.DeserializeObject<LucyDocument>(
@"
locale: en
entities:
  - name: Greeting
    patterns:
      - Hi

  - name: ChangeInfo
    patterns:
      - (@ChangeName|@ChangeAge|___)+

  - name: ChangeName
    patterns:
      - name is (name:___)2
      - call me (name:___)2

  - name: ChangeAge
    patterns: 
      - age is @number
      - '@number (years)? old'
      - I'm @number
")
            };
        }

        protected override async Task<DialogTurnResult> OnEvaluateStateAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            // greet user if we haven't yet.
            if (!dc.State.TryGetValue<bool>("dialog.greeted", out var greeted))
            {
                dc.AppendReplyText("Hi!");
                dc.State.SetValue("dialog.greeted", true);
            }
            // ----- get state ----
            dc.State.TryGetValue<string>("dialog.name", out var name);
            dc.State.TryGetValue<int?>("dialog.age", out var age);

            // ----- handle state change ---
            // handle dialog.name updated.

            if (!string.IsNullOrEmpty(name) && dc.IsStateChanged("dialog.name"))
            {
                dc.AppendReplyText($"\n\nYour name is {name}.");
            }

            // handle dialog.age updated.
            if (age.HasValue && dc.IsStateChanged("dialog.age"))
            {
                dc.AppendReplyText($"\n\nYour age is {age.Value}.");
            }

            // ----- prompt for missing data ----
            if (name == null && age == null)
            {
                dc.AppendReplyText("\n\nWhat is your name and age?");
            }
            else if (name == null)
            {
                dc.AppendReplyText("\n\nWhat is your name?");
            }
            else if (age == null)
            {
                dc.AppendReplyText("\n\nWhat is your age?");
            }

            await dc.SendReplyText(cancellationToken);

            return await dc.WaitForInputAsync(cancellationToken);
        }

        protected virtual async Task<DialogTurnResult> OnGreetingIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            dc.AppendReplyText("Hi!");
            dc.State.SetValue("dialog.greeted", true);
            return await OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected virtual async Task<DialogTurnResult> OnChangeInfoIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            // if there is a name
            var name = recognizerResult.GetEntities<string>("$..ChangeName..name").FirstOrDefault();
            if (!string.IsNullOrEmpty(name))
            {
                // save it
                dc.State.SetValue("dialog.name", name);
            }

            // if there is an age 
            var age = recognizerResult.GetEntities<int?>("$..ChangeAge..number..value").FirstOrDefault();
            if (age.HasValue)
            {
                // save it
                dc.State.SetValue("dialog.age", age.Value);
            }

            return await OnEvaluateStateAsync(dc, cancellationToken);
        }
    }
}
