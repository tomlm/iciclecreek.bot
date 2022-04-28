using Iciclecreek.Bot.Builder.Dialogs;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Lucene.Net.Search;
using Lucy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
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
            ExtractDateEntities(entities, out var dates, out var timexes);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"## WhoQuery");
            sb.AppendLine($"Place: {place}");
            sb.AppendLine(VisualizeDates(dates, timexes));
            await ReplyText(dc, sb.ToString());
            return await dc.WaitForInputAsync();
        }

        public async Task<DialogTurnResult> OnWhereQueryIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellation = default)
        {
            var entities = StripInstance(recognizerResult);
            var person = entities.SelectToken("$..Person")?.Select(s => s.ToString()).First() ?? String.Empty;
            ExtractDateEntities(entities, out var dates, out var timexes);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"## WhereQuery");
            sb.AppendLine($"Person: {person}");
            sb.AppendLine(VisualizeDates(dates, timexes));
            await ReplyText(dc, sb.ToString());
            return await dc.WaitForInputAsync();
        }

        public async Task<DialogTurnResult> OnSetPlanIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellation = default)
        {
            var entities = StripInstance(recognizerResult);
            var place = entities.SelectToken("$..Place")?.FirstOrDefault()?.ToString() ?? String.Empty;
            ExtractDateEntities(entities, out var dates, out var timexes);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"## SetPlan");
            sb.AppendLine($"Place: {place}");
            sb.AppendLine(VisualizeDates(dates, timexes));
            await ReplyText(dc, sb.ToString());
            return await dc.WaitForInputAsync();
        }

        private static string VisualizeDates(HashSet<string> dates, HashSet<string> timexes)
        {
            StringBuilder sb = new StringBuilder();
            if (dates.Any())
            {
                sb.AppendLine("Dates:");
                sb.AppendLine($"* { string.Join("\n* ", dates)}");
            }
            sb.AppendLine("Timex:");
            if (timexes.Any())
            {
                sb.AppendLine($"* {string.Join("\n* ", timexes)}");
            }

            return sb.ToString();
        }

        private static JObject StripInstance(RecognizerResult recognizerResult)
        {
            return (JObject)recognizerResult.Entities.RemoveFields("$instance");
        }

        private static void ExtractDateEntities(JObject entities, out HashSet<string> dates, out HashSet<string> timexes)
        {
            dates = new HashSet<string>();
            timexes = new HashSet<string>();
            foreach (var date in entities.SelectTokens("$..datetime"))
            {
                foreach (var value in date.SelectTokens("$..value"))
                {
                    if (DateTime.TryParse(value.ToString(), out var dt) && dt > DateTime.Now)
                    {
                        dates.Add(value.ToString());
                    }
                    else
                    {
                    }
                }
                foreach (var value in date.SelectTokens("$..timex").Select(jt => jt.ToString()))
                {
                    switch (value)
                    {
                        case "XXXX-WXX-1":
                            timexes.Add("Monday");
                            break;
                        case "XXXX-WXX-2":
                            timexes.Add("Tuesday");
                            break;
                        case "XXXX-WXX-3":
                            timexes.Add("Wednesday");
                            break;
                        case "XXXX-WXX-4":
                            timexes.Add("Thursday");
                            break;
                        case "XXXX-WXX-5":
                            timexes.Add("Friday");
                            break;
                        case "XXXX-WXX-6":
                            timexes.Add("Saturday");
                            break;
                        case "XXXX-WXX-7":
                            timexes.Add("Sunday");
                            break;
                        default:
                            timexes.Add(value.ToString());
                            break;
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
