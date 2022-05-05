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
    internal class PromptTest : IcyDialog
    {
        public PromptTest()
        {
            this.Recognizer = new LucyRecognizer()
            {
                Intents = new List<string>() { "Greeting", "QueryName" },
                Model = YamlConvert.DeserializeObject<LucyDocument>(
@"
entities:
  - name: Greeting
    patterns:
      - hi

  - name: QueryName
    patterns:
      - what is my name
")
            };
        }



        protected async virtual Task<DialogTurnResult> OnGreetingIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            dc.AppendReplyText($"Hi!");
            return await this.OnEvaluateAsync(dc, cancellationToken);
        }

        protected async virtual Task<DialogTurnResult> OnQueryNameIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            var name = dc.State.GetValue<string>("this.name");
            if (name == null)
            {
                dc.AppendReplyText($"I don't know your name.");
            }
            else
            {
                dc.AppendReplyText($"Your name is {name}.");
            }
            return await this.OnEvaluateAsync(dc, cancellationToken);
        }

        protected async override Task<DialogTurnResult> OnEvaluateAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            // if we are missing this.name, prompt for it.
            dc.State.TryGetValue<string>("this.name", out var name);
            if (String.IsNullOrEmpty(name))
            {
                return await PromptAsync<TextPrompt>(dc, "this.name", new PromptOptions() { Prompt = dc.CreateReplyActivity("What is your name?") });
            }

            // if we are missing... prompt for it.
            // ...

            // if we are all done, let's end the dialog...
            // return dc.EndDialogAsync(this);

            await dc.SendReplyText();

            return await dc.WaitForInputAsync();
        }

        // hook this to use the name we got from the prompt in a greeting back to the user.
        protected override async Task<DialogTurnResult> OnPromptCompletedAsync(DialogContext dc, string property, object result, CancellationToken cancellationToken = default)
        {
            switch (property)
            {
                case "this.name":
                    dc.AppendReplyText($"Nice to meet you {result}!");
                    break;
            }
            return await base.OnPromptCompletedAsync(dc, property, result, cancellationToken); ;
        }
    }
}
