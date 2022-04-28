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

namespace Dialogs
{
    public class LucyDialog : IcyDialog
    {
        public LucyDialog()
        {
            var yaml = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream($"LucyBot.Dialogs.RootDialog.yaml")).ReadToEnd();
            this.Recognizer = new LucyRecognizer()
            {
                Intents = new List<string>() { "Greeting", "Goodbye" },
                Model = YamlConvert.DeserializeObject<LucyDocument>(yaml)
            };
        }

        public async Task<DialogTurnResult> OnGreetingIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellation = default)
        {
            await dc.SendActivityAsync("Hi!");
            return await dc.WaitForInputAsync();
        }

        public async Task<DialogTurnResult> OnGoodbyeIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellation = default)
        {
            await dc.SendActivityAsync("See you later!");
            return await dc.WaitForInputAsync();
        }
    }
}
