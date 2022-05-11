using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Iciclecreek.Bot.Builder.Dialogs;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Lucene.Net.Search;
using Lucy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Queue;
using YamlConverter;

namespace BeBot.Dialogs
{
    public class WhoQueryDialog : IcyDialog
    {
        private readonly IConfiguration _configuration;
        private readonly IndexSearcher _searcher;
        private readonly CloudQueueClient _cloudQueue;

        public WhoQueryDialog(IConfiguration configuration, CloudQueueClient cloudQueueClient, IndexSearcher searcher)
        {
            var yaml = new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(WhoQueryDialog).FullName}.{typeof(WhoQueryDialog).Name}.yaml")).ReadToEnd();
            var yamlShared = new StreamReader(typeof(BeBotDialog).Assembly.GetManifestResourceStream($"BeBot.Dialogs.Shared.yaml")).ReadToEnd();
            this.Recognizer = new LucyRecognizer()
            {
                ExternalEntityRecognizer = BeBotHelp.GetSharedRecognizer(),
                Intents = new List<string>() { "Help", "Cancel", "Greeting", "Goodbye", "Thanks", "WhQuery", "Dates" },
                Model = YamlConvert.DeserializeObject<LucyDocument>($"{yaml}\n\n{yamlShared}")
            };

            this._configuration = configuration;
            this._searcher = searcher;
            this._cloudQueue = cloudQueueClient;

        }

        // ----------------------- INTENTS ------------------------
        protected async override Task<DialogTurnResult> OnUnrecognizedIntentAsync(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            dc.AppendReplyText("I'm sorry, I didn't understand that.");
            await dc.SendReplyText(HelpText);
            return await dc.EndDialogAsync(null, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnHelpIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            await dc.SendReplyText(HelpText);
            return await dc.WaitForInputAsync(cancellationToken);
        }

        protected async Task<DialogTurnResult> OnGreetingIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            dc.AppendReplyText(SharedText.GreetingReplies);
            return await OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnThanksIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            dc.AppendReplyText(SharedText.ThanksReplies);
            return await dc.WaitForInputAsync(cancellationToken);
        }

        protected async Task<DialogTurnResult> OnGoodbyeIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            await dc.SendReplyText(SharedText.GoodbyeReplies);
            return await dc.WaitForInputAsync(cancellationToken);
        }

        protected async Task<DialogTurnResult> OnCancelIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            dc.AppendReplyText(SharedText.CancelReplies);
            return await dc.EndDialogAsync(null, cancellationToken);
        }

        // ----------------------- TEXT ------------------------
        public static readonly string[] HelpText = new string[]
        {
$@"

### Who Query
You can query for people are going to be at a location using a **who** query.

Examples:
* *who will be in city center on Friday?*
"
        };

    }
}
