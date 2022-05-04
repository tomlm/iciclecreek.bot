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
                Intents = new List<string>() { "Greeting", "Goodbye", "ChangeAlias", "WhereQuery", "WhoQuery", "SetPlan", "Thanks" },
                Model = YamlConvert.DeserializeObject<LucyDocument>(yaml)
            };

            this._configuration = configuration;
            this._searcher = searcher;
            this._cloudQueue = cloudQueueClient;
        }

        protected async Task<DialogTurnResult> OnGreetingIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            await dc.ReplyText("Hi ${activity.from.name}!", "Hello ${activity.from.name}!", "Hola ${activity.from.name}!", "Greetings ${activity.from.name}!");
            return await OnEvaluateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnThanksIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            await dc.ReplyText("No problem!", "My pleasure", "You are welcome.", "De nada", "It's a pleasure to help you.");
            return await dc.WaitForInputAsync();
        }

        protected async Task<DialogTurnResult> OnGoodbyeIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            await dc.ReplyText("See you later!", "Goodbye!", "Bye!", "See ya!", "Catch you later aligator...", "Hasta la pasta", "A river ditchy!");
            return await dc.WaitForInputAsync();
        }

        protected override async Task<DialogTurnResult> OnEvaluateAsync(DialogContext dc, CancellationToken ct)
        {
            if (String.IsNullOrEmpty(dc.State.GetStringValue("user.alias")))
            {
                await dc.ReplyText("I need to some information to be a good little BeBot.", "We need to do some stuff first", "Gotta get some info...");
                return await PromptAsync<TextPrompt>(dc, "user.alias", new PromptOptions() { Prompt = dc.CreateReply("What is your alias?") });
            }

            return await base.OnEvaluateAsync(dc, ct);
        }

        protected override async Task<DialogTurnResult> OnPromptCompletedAsync(DialogContext dc, string property, object result, CancellationToken cancellationToken = default)
        {
            dc.State.SetValue(property, result);
            await ReplyUserAlias(dc);
            return await OnEvaluateAsync(dc, cancellationToken);
        }

        protected virtual async Task<DialogTurnResult> OnChangeAliasIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            var entities = StripInstance(recognizerResult);
            var alias = entities.SelectToken("$..alias")?.FirstOrDefault()?.ToString() ?? String.Empty;
            dc.State.SetValue("user.alias", alias);
            await ReplyUserAlias(dc);
            return await dc.WaitForInputAsync();
        }

        private static async Task ReplyUserAlias(DialogContext dc) =>
            await dc.ReplyText("Your alias is now ${user.alias}",
                        "Got it, ${user.alias}",
                        "Cool, cool, cool, alias is now ${user.alias}.",
                        "Roger dodger ${user.alias}.",
                        "Hail ${user.alias}!");

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
            return await dc.WaitForInputAsync();
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
            return await dc.WaitForInputAsync();
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
    }
}
