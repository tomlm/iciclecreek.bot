using Iciclecreek.Bot.Builder.Dialogs;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Lucene.Net.Search;
using Lucy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
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
    public class BeBotDialog : IcyDialog
    {
        private readonly IConfiguration _configuration;
        private readonly IndexSearcher _searcher;
        private readonly CloudQueueClient _cloudQueue;

        public BeBotDialog(IConfiguration configuration, CloudQueueClient cloudQueueClient, IndexSearcher searcher)
        {
            var yaml = new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(BeBotDialog).FullName}.yaml")).ReadToEnd();
            this.Recognizer = new LucyRecognizer()
            {
                Intents = new List<string>() { "Greeting", "Goodbye", "WhereQuery", "WhoQuery", "SetPlan" },
                Model = YamlConvert.DeserializeObject<LucyDocument>(yaml)
            };

            this._configuration = configuration;
            this._searcher = searcher;
            this._cloudQueue = cloudQueueClient;
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
            var entities = StripInstance(recognizerResult);
            var place = entities.SelectToken("$..Place")?.FirstOrDefault()?.ToString() ?? String.Empty;
            ExtractDateEntities(entities, out var dates);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"## WhoQuery");
            sb.AppendLine($"Place: {place}");
            sb.AppendLine(VisualizeDates(dates));
            await ReplyText(dc, sb.ToString());
            return await dc.WaitForInputAsync();
        }

        public async Task<DialogTurnResult> OnWhereQueryIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellation = default)
        {
            var entities = StripInstance(recognizerResult);
            var person = entities.SelectToken("$..Person")?.Select(s => s.ToString()).First() ?? String.Empty;
            ExtractDateEntities(entities, out var dates);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"## WhereQuery");
            sb.AppendLine($"Person: {person}");
            sb.AppendLine(VisualizeDates(dates));
            await ReplyText(dc, sb.ToString());
            return await dc.WaitForInputAsync();
        }

        public async Task<DialogTurnResult> OnSetPlanIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellation = default)
        {
            var entities = StripInstance(recognizerResult);
            var place = entities.SelectToken("$..Place")?.FirstOrDefault()?.ToString() ?? String.Empty;
            ExtractDateEntities(entities, out var dates);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"## SetPlan");
            sb.AppendLine($"Place: {place}");
            sb.AppendLine(VisualizeDates(dates));
            await ReplyText(dc, sb.ToString());
            return await dc.WaitForInputAsync();
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

        protected Task ReplyText(DialogContext dc, string text, object data = null)
        {
            var activity = Activity.CreateMessageActivity();
            activity.Text = text;
            activity.Value = data;
            return dc.SendActivityAsync(activity);
        }
    }
}
