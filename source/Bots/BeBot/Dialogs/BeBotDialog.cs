using Iciclecreek.Bot.Builder.Dialogs;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Lucy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using YamlConverter;

namespace BeBot.Dialogs
{
    public class BeBotDialog : IcyDialog
    {
        public BeBotDialog()
        {
            var yaml = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(BeBotDialog).FullName}.yaml")).ReadToEnd();
            this.Recognizer = new LucyRecognizer()
            {
                Intents = new List<string>() { "Greeting", "Goodbye", "WhereQuery", "WhoQuery", "SetPlan" },
                Model = YamlConvert.DeserializeObject<LucyDocument>(yaml)
            };
        }

        public async Task<DialogTurnResult> OnGreetingIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellation = default)
        {
            await ReplyText(dc, "Hi!");
            return await dc.WaitForInputAsync();
        }

        public async Task<DialogTurnResult> OnGoodbyeIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellation = default)
        {
            await ReplyText(dc, "See you later!");
            return await dc.WaitForInputAsync();
        }

        public async Task<DialogTurnResult> OnWhoQueryIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellation = default)
        {
            await ReplyText(dc, "WhoQuery", recognizerResult);
            return await dc.WaitForInputAsync();
        }

        public async Task<DialogTurnResult> OnWhereQueryIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellation = default)
        {
            await ReplyText(dc, "WhereQuery", recognizerResult);
            return await dc.WaitForInputAsync();
        }

        public async Task<DialogTurnResult> OnSetPlanIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellation = default)
        {
            await ReplyText(dc, "SetPlan", recognizerResult);
            return await dc.WaitForInputAsync();
        }

        protected Task ReplyText(DialogContext dc, string text, object data = null)
        {
            var activity = Activity.CreateMessageActivity();
            activity.Text = text;
            activity.Value = data;
            return dc.SendActivityAsync(activity);
        }
    }
}
