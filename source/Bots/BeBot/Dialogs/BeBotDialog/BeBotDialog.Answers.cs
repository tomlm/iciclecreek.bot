using Iciclecreek.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BeBot.Dialogs
{
    public partial class BeBotDialog : IcyDialog
    {

        protected async Task<DialogTurnResult> OnUserAliasAnswer(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            var alias = recognizerResult.GetEntities<string>("$..mention..value").FirstOrDefault();
            if (alias == null)
            {
                alias = messageActivity.Text.Trim();
            }

            if (!String.IsNullOrEmpty(alias))
            {
                if (alias.Contains(" "))
                {
                    dc.AppendReplyText(BeBotDialogText.UserAlias_Bad);
                    return await dc.AskQuestionAsync("UserAlias", BeBotDialogText.UserAlias_Ask);
                }
                dc.State.SetValue("user.alias", alias.TrimStart('@'));
            }

            return await OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnSetPlanWhenAnswer(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            // interpret text as dates answer
            var when = GetDateEntities(recognizerResult).Where(en => en.GetDate().HasValue || en.GetDate() >= DateTime.Now);
            if (when.Any())
            {
                dc.State.SetValue("dialog.SetPlan.When", when);
            }
            else
            {
                dc.AppendReplyText(BeBotDialogText.SetPlanWhen_Bad);
                return await dc.AskQuestionAsync("SetPlanWhen", BeBotDialogText.SetPlanWhen_Ask);
            }

            return await OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnSetPlanWhereAnswer(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            // interpret text as location answer.
            string where = GetPlace(recognizerResult);
            if (where == null)
            {
                where = NormalizePlace(messageActivity.Text.Trim());
            }

            if (!String.IsNullOrEmpty(where))
            {
                dc.State.SetValue("dialog.SetPlan.Where", where);
            }
            else
            {
                dc.AppendReplyText(BeBotDialogText.SetPlanWhere_Bad);
                return await dc.AskQuestionAsync("SetPlanWhere", BeBotDialogText.SetPlanWhere_Ask);
            }

            return await OnEvaluateStateAsync(dc, cancellationToken);
        }

    }
}
