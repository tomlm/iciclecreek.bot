using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iciclecreek.Bot.Builder.Dialogs;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Lucy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Schema;
using YamlConverter;

namespace BeBot.Dialogs
{
    public partial class SetPlanDialog : IcyDialog
    {
        public SetPlanDialog()
        {
            var yaml = new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(SetPlanDialog).FullName}.{typeof(SetPlanDialog).Name}.yaml")).ReadToEnd();
            this.Recognizer = new LucyRecognizer()
            {
                Intents = new List<string>() { "Help", "Cancel", "SetPlan", "Dates" },
                Model = YamlConvert.DeserializeObject<LucyDocument>(yaml)
            };
        }

        protected override async Task<DialogTurnResult> OnEvaluateStateAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            // if we are evaluating then we should clear the question.
            dc.ClearQuestion();

            //if (dc.IsStateChanged("dialog.dates"))
            //{
            //    var dates = dc.State.GetValue<string>("dialog.dates");
            //    var result = await this.Recognizer.RecognizeAsync(dc, )
            //}

            // --- dialog.dates is missing 
            if (String.IsNullOrEmpty(dc.State.GetStringValue("dialog.dates")))
            {
                return await dc.AskQuestionAsync("When", "When will you be in this week?");
            }

            if (String.IsNullOrEmpty(dc.State.GetStringValue("dialog.location")))
            {
                return await dc.AskQuestionAsync("Where", "Where will you be? (home, work, ...)");
            }

            await dc.SendReplyText(cancellationToken);
            return await dc.WaitForInputAsync(cancellationToken);
        }

        protected async override Task<DialogTurnResult> OnUnrecognizedIntentAsync(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            switch(dc.GetLastQuestion())
            {
                case "when":
                    // interpret text as dates answer

                    return await OnEvaluateStateAsync(dc, cancellationToken);

                case "where":
                    // interpret text as location answer.
                    return await OnEvaluateStateAsync(dc, cancellationToken);

                default:
                    return await base.OnUnrecognizedIntentAsync(dc, messageActivity, recognizerResult, cancellationToken);
            }
        }

        protected async Task<DialogTurnResult> OnHelpIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            await dc.SendReplyText(HelpText);
            return await dc.WaitForInputAsync(cancellationToken);
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

            // handle missing entities.
            if (place == null || dates == null)
            {
                dc.AppendReplyText("\n\nYou did not provide enough information:");
                if (place == null)
                {
                    dc.AppendReplyText(@"\n* You did not specify a place.");
                }
                if (!dates.Any())
                {
                    dc.AppendReplyText(@"\n* You did not specify a date.");
                }
                dc.AppendReplyText("\n\nPlease try again.");
                return await dc.WaitForInputAsync(cancellationToken);
            }

            // nomralize home and work
            if (place.Contains("home"))
            {
                place = "Home";
            }

            if (place.Contains("work"))
            {
                place = "Work";
            }

            return await OnEvaluateStateAsync(dc, cancellationToken);
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
