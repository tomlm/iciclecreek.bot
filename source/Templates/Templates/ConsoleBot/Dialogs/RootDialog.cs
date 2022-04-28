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

namespace ConsoleBot.Dialogs
{
    public class RootDialog : IcyDialog
    {
        public RootDialog()
        {
            var yaml = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(RootDialog).FullName}.yaml")).ReadToEnd();
            this.Recognizer = new LucyRecognizer()
            {
                Intents = new List<string>() { "Greeting", "Goodbye"},
                Model = YamlConvert.DeserializeObject<LucyDocument>(yaml)
            };
        }

        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            dc.SaveOptions(options);
            await dc.SendActivityAsync(@"Welcome to console bot.  What can I do for you?");
            return await dc.WaitForInputAsync();
        }

        public async Task<DialogTurnResult> OnGreetingIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellation = default)
        {
            var options = dc.GetOptions<object>();
            await dc.SendActivityAsync("Hi!");
            return await dc.WaitForInputAsync();
        }

        public async Task<DialogTurnResult> OnGoodbyeIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellation = default)
        {
            var options = dc.GetOptions<object>();
            await dc.SendActivityAsync("See you later!");
            await dc.SendActivityAsync(Activity.CreateEndOfConversationActivity());
            return await dc.EndDialogAsync();
        }

    }
}
