using Iciclecreek.Bot.Builder.Dialogs;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Lucene.Net.Search;
using Lucy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json.Linq;
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
                Intents = new List<string>() { "Greeting", "Goodbye", "ChangeAlias", "WhereQuery", "WhoQuery", "SetPlan", "Thanks" },
                Model = YamlConvert.DeserializeObject<LucyDocument>(yaml)
            };

            this._configuration = configuration;
            this._searcher = searcher;
            this._cloudQueue = cloudQueueClient;
        }

        protected override async Task<DialogTurnResult> OnEvaluateAsync(DialogContext dc, CancellationToken cancellationToken)
        {

            if (String.IsNullOrEmpty(dc.State.GetStringValue("user.alias")))
            {
                await dc.SendReplyText(ExplainAlias);
                return await PromptAsync<TextPrompt>(dc, "user.alias", new PromptOptions() { Prompt = dc.CreateReplyActivity("What is your alias?") });
            }

            await dc.SendReplyText(cancellationToken);
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

        protected override async Task<DialogTurnResult> OnPromptCompletedAsync(DialogContext dc, string property, object result, CancellationToken cancellationToken = default)
        {
            switch (property)
            {
                case "user.alias":
                    dc.State.SetValue(property, result);
                    dc.AppendReplyText(SetAliasResponse);
                    break;
            }

            return await OnEvaluateAsync(dc, cancellationToken);
        }

        protected virtual async Task<DialogTurnResult> OnChangeAliasIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            var entities = StripInstance(recognizerResult);
            var alias = entities.SelectToken("$..alias")?.FirstOrDefault()?.ToString() ?? String.Empty;
            if (!String.IsNullOrEmpty(alias))
            {

                dc.State.SetValue("user.alias", alias);
                dc.AppendReplyText(SetAliasResponse);
                return await OnEvaluateAsync(dc, cancellationToken);
            }
            return await PromptAsync<TextPrompt>(dc, "user.alias", new PromptOptions() { Prompt = dc.CreateReplyActivity("What is your alias?") });
        }


        protected virtual async Task<DialogTurnResult> OnWhoQueryIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            var entities = StripInstance(recognizerResult);
            var place = entities.SelectToken("$..Place")?.FirstOrDefault()?.ToString() ?? String.Empty;
            ExtractDateEntities(entities, out var dates);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"## WhoQuery");
            sb.AppendLine($"Place: {place}");
            sb.AppendLine(VisualizeDates(dates));
            await dc.ReplyData(sb.ToString());
            return await OnEvaluateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnWhereQueryIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            var entities = StripInstance(recognizerResult);
            var person = entities.SelectToken("$..Person")?.Select(s => s.ToString()).First() ?? String.Empty;
            ExtractDateEntities(entities, out var dates);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"## WhereQuery");
            sb.AppendLine($"Person: {person}");
            sb.AppendLine(VisualizeDates(dates));
            await dc.ReplyData(sb.ToString());
            return await OnEvaluateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnSetPlanIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            var entities = StripInstance(recognizerResult);
            var place = entities.SelectToken("$..Place")?.FirstOrDefault()?.ToString() ?? String.Empty;
            ExtractDateEntities(entities, out var dates);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"## SetPlan");
            sb.AppendLine($"Place: {place}");
            sb.AppendLine(VisualizeDates(dates));
            await dc.ReplyData(sb.ToString());
            return await OnEvaluateAsync(dc, cancellationToken);
        }

        private static string VisualizeDates(HashSet<DateTimexValue> dates)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Dates:");
            sb.AppendLine("```");
            sb.AppendLine(YamlConvert.SerializeObject(dates));
            sb.AppendLine("```");
            return sb.ToString();
        }

        private static JObject StripInstance(RecognizerResult recognizerResult)
        {
            return (JObject)recognizerResult.Entities.RemoveFields("$instance");
        }

        private static void ExtractDateEntities(JObject entities, out HashSet<DateTimexValue> dates)
        {
            dates = new HashSet<DateTimexValue>(new DateTimeValueComparer());
            foreach (var date in entities.SelectTokens("$..dates"))
            {
                foreach (var valueArray in date.SelectTokens("$..values").Cast<JArray>())
                {
                    foreach (var dateTimeValue in valueArray.Select(j => j.ToObject<DateTimexValue>()))
                    {
                        if (dateTimeValue.Date == null || dateTimeValue.Date >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))
                        {
                            dates.Add(dateTimeValue);
                        }
                    }
                }
            }
        }
    }
}
