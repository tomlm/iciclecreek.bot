using Iciclecreek.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeBot.Dialogs
{
    public partial class BeBotDialog : IcyDialog
    {
        protected async override Task<DialogTurnResult> OnUnrecognizedIntentAsync(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            dc.AppendReplyText(BeBotDialogText.UnrecognizedResponse);
            return await this.OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnHelpIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            switch (dc.GetLastQuestion())
            {
                case "UserAlias":
                    await dc.SendReplyText(BeBotDialogText.UserAlias_Help);
                    break;

                default:
                    await dc.SendReplyText(BeBotDialogText.Help);
                    break;
            }

            return await dc.WaitForInputAsync(cancellationToken);
        }

        protected async Task<DialogTurnResult> OnCancelIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            dc.ClearQuestion();
            dc.State.RemoveValue("dialog.SetPlan");
            dc.AppendReplyText(BeBotDialogText.CancelReplies);
            return await OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnGreetingIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            dc.AppendReplyText(BeBotDialogText.GreetingReplies);
            return await OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnThanksIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            dc.AppendReplyText(BeBotDialogText.ThanksReplies);
            return await dc.WaitForInputAsync(cancellationToken);
        }

        protected async Task<DialogTurnResult> OnGoodbyeIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            await dc.SendReplyText(BeBotDialogText.GoodbyeReplies);
            return await dc.WaitForInputAsync(cancellationToken);
        }

        protected virtual async Task<DialogTurnResult> OnChangeAliasIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            var alias = recognizerResult.GetEntities<string>("$..alias").FirstOrDefault() ?? String.Empty;
            if (!String.IsNullOrEmpty(alias))
            {
                dc.State.SetValue("user.alias", alias);
            }
            else
            {
                // we have intent but no name, prompt for it.
                return await dc.AskQuestionAsync("UserAlias", BeBotDialogText.UserAlias_Ask);
            }

            return await OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnSetPlanIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            //StringBuilder sb = new StringBuilder();
            //sb.AppendLine($"## SetPlan");
            //sb.AppendLine($"Place: {place}");
            //sb.AppendLine(VisualizeDates(dates.Where(dtv => dtv.GetDate() >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))));
            //await dc.ReplyData(sb.ToString());

            // create setplan record
            if (!dc.State.ContainsKey("dialog.SetPlan"))
            {
                dc.State.SetValue("dialog.SetPlan", new object());
            }

            string where = GetPlace(recognizerResult);
            if (where != null)
            {
                dc.State.SetValue("dialog.SetPlan.Where", where);
            }

            var when = GetDateEntities(recognizerResult);
            if (when.Any())
            {
                dc.State.SetValue("dialog.SetPlan.When", when);
            }

            return await OnEvaluateStateAsync(dc, cancellationToken);
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
            return await OnEvaluateStateAsync(dc, cancellationToken);
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
            return await OnEvaluateStateAsync(dc, cancellationToken);
        }
    }
}
