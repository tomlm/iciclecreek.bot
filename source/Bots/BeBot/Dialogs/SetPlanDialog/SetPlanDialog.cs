using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iciclecreek.Bot.Builder.Dialogs;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Lucene.Net.Search;
using Lucy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json.Linq;
using YamlConverter;

namespace BeBot.Dialogs
{
    public class SetPlanDialog : IcyDialog
    {
        private readonly IConfiguration _configuration;
        private readonly IndexSearcher _searcher;
        private readonly CloudQueueClient _cloudQueue;

        public SetPlanDialog(IConfiguration configuration, CloudQueueClient cloudQueueClient)
        {
            var yaml = new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(SetPlanDialog).FullName}.{typeof(SetPlanDialog).Name}.yaml")).ReadToEnd();
            var yamlShared = new StreamReader(typeof(BeBotDialog).Assembly.GetManifestResourceStream($"BeBot.Dialogs.Shared.yaml")).ReadToEnd();
            this.Recognizer = new LucyRecognizer()
            {
                ExternalEntityRecognizer = BeBotHelp.GetSharedRecognizer(),
                Intents = new List<string>()
                {
                    "Help", "Cancel", "Greeting", "Goodbye", "Thanks",
                    "SetPlan", "Dates"
                },
                Model = YamlConvert.DeserializeObject<LucyDocument>($"{yaml}\n\n{yamlShared}")
            };

            this._configuration = configuration;
            this._cloudQueue = cloudQueueClient;

        }
        // ------------------------ INTENTS ----------------------
        #region INTENTS
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

        protected async Task<DialogTurnResult> OnSetPlanIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            //StringBuilder sb = new StringBuilder();
            //sb.AppendLine($"## SetPlan");
            //sb.AppendLine($"Place: {place}");
            //sb.AppendLine(VisualizeDates(dates.Where(dtv => dtv.GetDate() >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))));
            //await dc.ReplyData(sb.ToString());

            string where = BeBotHelp.GetPlace(recognizerResult);
            if (where != null)
            {
                dc.State.SetValue("this.Where", where);
            }

            var when = BeBotHelp.GetDateEntities(recognizerResult).Where(en => en.GetDate().HasValue || en.GetDate().HasValue == false || en.GetDate() >= DateTime.Now);
            if (when.Any())
            {
                dc.State.SetValue("this.When", when);
            }

            return await OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnWhenAnswer(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            // interpret text as dates answer
            var when = recognizerResult.GetEntities<DateTimexValue>("$..Dates..values").Where(en => en.GetDate().HasValue || en.GetDate() >= DateTime.Now);
            if (when.Any())
            {
                dc.State.SetValue("this.When", when);
            }
            else
            {
                dc.AppendReplyText(When_Bad);
                return await dc.AskQuestionAsync("When", When_Ask);
            }

            return await this.OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnWhereAnswer(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            // interpret text as location answer.
            string where = recognizerResult.GetEntities<string>("$..Place").FirstOrDefault();
            if (where == null)
            {
                where = NormalizePlace(messageActivity.Text.Trim());
            }

            if (!String.IsNullOrEmpty(where))
            {
                dc.State.SetValue("this.Where", where);
            }
            else
            {
                dc.AppendReplyText(Where_Bad);
                return await dc.AskQuestionAsync("Where", Where_Ask);
            }

            return await OnEvaluateStateAsync(dc, cancellationToken);
        }
        #endregion

        // ------------------------ EVALUATE ----------------------
        #region EVALUATE

        protected override async Task<DialogTurnResult> OnEvaluateStateAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            if (dc.IsStateChanged("this.When") || dc.IsStateChanged("this.Where"))
            {
                dc.AppendReplyText(When_Changed);
            }

            // -- look for setPlan record
            var dates = dc.State.GetValue<JArray>("this.When");
            if (dates == null || !dates.Any())
            {
                return await dc.AskQuestionAsync("When", When_Ask);
            }

            var location = dc.State.GetStringValue("this.Where");
            if (location == null)
            {
                return await dc.AskQuestionAsync("Where", Where_Ask);
            }

            // do setplan work;
            dc.AppendReplyText("Plan: ${dialog.SetPlan}");
            return await dc.EndDialogAsync(null, cancellationToken);
        }
        #endregion

        // ----------------------- TEXT -----------------------
        #region TEXT
        public static readonly string[] HelpText = new string[]
        {
$@"

### Setting a plan 
To set the plan for the week you need to tell me when and where you are going to be.

Examples:
* *I'll be in City Center on Monday, Tuesday and Friday*
* *I'll be at work on Mondays and Tuesdays*
* *I will be in city center on Monday and in red west c on thursdays.*
"
        };

        public static readonly string[] When_Ask = new string[]
        {
            "\n\nWhen will you be in this week?",
            "\n\nWhen do you think you will be in this week?",
            "\n\nWhat days are we talking about?"
        };

        public static readonly string[] When_Bad = new string[]
        {
            "\n\nI didn't understand your response as dates.  I'm looking for something like *monday and friday*",
            "\n\nHmmm, I didn't find a date.  A typical response would be like *monday and friday*"
        };

        public static readonly string[] When_Changed = new string[]
        {
            "\n\nWhen: ${this.when}",
        };

        public static readonly string[] Where_Ask = new string[]
        {
            "\n\nWhere will you be? (home, work, ...)",
            "\n\nWhich location are you going to? (home, work, ...)",
            "\n\nGive me a hint...what location will be working at? (home, work, ...)"
        };

        public static readonly string[] Where_Bad = new string[]
        {
            "\n\nI didn't understand your response as a location.  I'm looking for something like 'work' or 'building 123'"
        };

        public static readonly string[] Where_Changed = new string[]
        {
            "\n\nWhere: ${where}",
        };
        #endregion


        private static string NormalizePlace(string place)
        {
            if (place != null)
            {
                // normalize home and work
                if (place.Contains("home"))
                {
                    place = "Home";
                }

                if (place.Contains("work"))
                {
                    place = "Work";
                }
            }

            return place;
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
