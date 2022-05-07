using Iciclecreek.Bot.Builder.Dialogs;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Lucene.Net.Search;
using Lucy;
using Microsoft.AspNetCore.Mvc.WebApiCompatShim;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YamlConverter;

namespace BeBot.Dialogs
{
    public partial class BeBotDialog : IcyDialog
    {
        private readonly IConfiguration _configuration;
        private readonly IndexSearcher _searcher;
        private readonly CloudQueueClient _cloudQueue;

        public BeBotDialog(IConfiguration configuration, CloudQueueClient cloudQueueClient, IndexSearcher searcher)
        {
            var yaml = new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(BeBotDialog).FullName}.yaml")).ReadToEnd();
            this.Recognizer = new LucyRecognizer()
            {
                Intents = new List<string>() { "Help", "Greeting", "Goodbye", "ChangeAlias", "WhereQuery", "WhoQuery", "SetPlan", "Thanks" },
                Model = YamlConvert.DeserializeObject<LucyDocument>(yaml)
            };

            this._configuration = configuration;
            this._searcher = searcher;
            this._cloudQueue = cloudQueueClient;
        }

        protected override Task<DialogTurnResult> OnMessageActivityAsync(DialogContext dc, IMessageActivity messageActivity, CancellationToken cancellationToken)
        {
            if (!dc.State.TryGetValue("user.welcomed", out var val))
            {
                dc.SendReplyText(HelpText);
                dc.State.SetValue("user.welcomed", true);
            }
            return base.OnMessageActivityAsync(dc, messageActivity, cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnEvaluateAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            // --- user.alias missing 
            if (String.IsNullOrEmpty(dc.State.GetStringValue("user.alias")))
            {
                await dc.SendReplyText(ExplainAlias);
                return await PromptAsync<TextPrompt>(dc, "user.alias", new PromptOptions() { Prompt = dc.CreateReplyActivity("What is your alias?") });
            }

            // --- user.alias changed
            if (dc.IsStateChanged("user.alias"))
            {
                dc.AppendReplyText(AckAliasChanged);
            }

            await dc.SendReplyText(cancellationToken);
            return await dc.WaitForInputAsync(cancellationToken);
        }

        protected async Task<DialogTurnResult> OnHelpIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            await dc.SendReplyText(HelpText);
            return await dc.WaitForInputAsync(cancellationToken);
        }

        protected async Task<DialogTurnResult> OnGreetingIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            dc.AppendReplyText(Greetings);
            return await OnEvaluateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnThanksIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            dc.AppendReplyText(ThanksResponse);
            return await OnEvaluateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnGoodbyeIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            await dc.SendReplyText(Goodbyes);
            return await dc.WaitForInputAsync(cancellationToken);
        }

        protected virtual async Task<DialogTurnResult> OnChangeAliasIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            var alias = recognizerResult.GetEntities<string>("$..alias").FirstOrDefault() ?? String.Empty;
            if (!String.IsNullOrEmpty(alias))
            {
                dc.State.SetValue("user.alias", alias);
                return await OnEvaluateAsync(dc, cancellationToken);
            }
            else
            {
                // handle "I want to change my alias" with no alias specified.
                return await PromptAsync<TextPrompt>(dc, "user.alias", new PromptOptions() { Prompt = dc.CreateReplyActivity("What is your alias?") });
            }
        }

        protected virtual async Task<DialogTurnResult> OnWhoQueryIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            var place = recognizerResult.GetEntities<string>("$..Place").FirstOrDefault();
            var dates = new HashSet<DateTimexValue>(recognizerResult.GetEntities<DateTimexValue>("$..dates..values"));
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"## WhoQuery");
            sb.AppendLine($"Place: {place}");
            sb.AppendLine(VisualizeDates(dates.Where(dtv => dtv.GetDate() >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))));
            await dc.ReplyData(sb.ToString());
            return await OnEvaluateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnWhereQueryIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            var person = recognizerResult.GetEntities<string>("$..Person").FirstOrDefault();
            var dates = new HashSet<DateTimexValue>(recognizerResult.GetEntities<DateTimexValue>("$..dates..values"));
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"## WhereQuery");
            sb.AppendLine($"Person: {person}");
            sb.AppendLine(VisualizeDates(dates.Where(dtv => dtv.GetDate() >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))));
            await dc.ReplyData(sb.ToString());
            return await OnEvaluateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnSetPlanIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            var place = recognizerResult.GetEntities<string>("$..Place").FirstOrDefault();
            var dates = new HashSet<DateTimexValue>(recognizerResult.GetEntities<DateTimexValue>("$..dates..values"));
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"## SetPlan");
            sb.AppendLine($"Place: {place}");
            sb.AppendLine(VisualizeDates(dates.Where(dtv => dtv.GetDate() >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))));
            await dc.ReplyData(sb.ToString());
            return await OnEvaluateAsync(dc, cancellationToken);
        }

        private static string VisualizeDates(IEnumerable<DateTimexValue> dates)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Dates:");
            sb.AppendLine("```");
            sb.AppendLine(YamlConvert.SerializeObject(dates));
            sb.AppendLine("```");
            return sb.ToString();
        }
    }
}
