using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    internal class AnswerTestDialog : IcyDialog
    {
        public AnswerTestDialog()
        {
            this.Recognizer = new LucyRecognizer()
            {
                Intents = new List<string>() { "Greeting", "SetName" },
                Model = YamlConvert.DeserializeObject<LucyDocument>(
@"
entities:
  - name: Greeting
    patterns:
      - hi

  - name: SetName
    patterns:
      - my name is (name:___)+
      - change my name 
      - change my name to (name:___)+
")
            };
        }



        protected async virtual Task<DialogTurnResult> OnGreetingIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            dc.AppendReplyText($"Hi!");
            return await this.OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected async virtual Task<DialogTurnResult> OnSetNameIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            var name = recognizerResult.GetEntities<string>("$..name").FirstOrDefault();
            if (!String.IsNullOrEmpty(name))
            {
                dc.State.SetValue("this.name", name);
            }
            else
            {
                // we have the intent but no name, so ask for it.
                return await dc.AskQuestionAsync("Name", "\n\nWhat is your name?");
            }

            return await this.OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected async virtual Task<DialogTurnResult> OnNameAnswer(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            if (!String.IsNullOrEmpty(messageActivity.Text))
            {
                dc.State.SetValue("this.name", messageActivity.Text.Trim());
            }

            return await this.OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected async override Task<DialogTurnResult> OnEvaluateStateAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            // if we are missing this.name, prompt for it.
            dc.State.TryGetValue<string>("this.name", out var name);
            if (String.IsNullOrEmpty(name))
            {
                return await dc.AskQuestionAsync("Name", "\n\nWhat is your name?");
            }

            if (dc.IsStateChanged("this.name"))
            {
                dc.AppendReplyText("Hello ${this.name}!");
            }
            
            await dc.SendReplyText();
            return await dc.WaitForInputAsync();
        }
    }
}
