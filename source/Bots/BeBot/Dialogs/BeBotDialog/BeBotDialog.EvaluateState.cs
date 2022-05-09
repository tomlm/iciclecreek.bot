using Iciclecreek.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BeBot.Dialogs
{
    public partial class BeBotDialog : IcyDialog
    {
        protected override async Task<DialogTurnResult> OnEvaluateStateAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            return
                await OnEvaluateUserAliasStateAsync(dc, cancellationToken) ??
                await OnEvaluateSetPlanStateAsync(dc, cancellationToken) ??
                await Task.Run<DialogTurnResult>(async () =>
                {
                    await dc.SendReplyText(cancellationToken, "What can I do for you?");
                    return await dc.WaitForInputAsync(cancellationToken);
                });
        }

        protected virtual async Task<DialogTurnResult> OnEvaluateUserAliasStateAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            // --- user.alias missing 
            if (String.IsNullOrEmpty(dc.State.GetStringValue("user.alias")))
            {
                return await dc.AskQuestionAsync("UserAlias", BeBotDialogText.AskUserAlias);
            }

            // --- user.alias changed
            if (dc.IsStateChanged("user.alias"))
            {
                dc.AppendReplyText(BeBotDialogText.UserAliasChangedReplies);
            }
            return null;
        }

        protected virtual async Task<DialogTurnResult> OnEvaluateSetPlanStateAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            // -- look for setPlan record
            if (dc.State.GetValue<object>("dialog.SetPlan") != null)
            {
                var dates = dc.State.GetStringValue("dialog.SetPlan.When");
                if (dates == null)
                {
                    return await dc.AskQuestionAsync("SetPlanWhen", "When will you be in this week?");
                }

                var location = dc.State.GetStringValue("dialog.SetPlan.Where");
                if (location == null)
                {
                    return await dc.AskQuestionAsync("SetPlanWhere", "Where will you be? (home, work, ...)");
                }
                // do setplan work;

                dc.AppendReplyText("Plan: ${dialog.SetPlan}");

                // remove temp SetPlan record.
                dc.State.RemoveValue("dialog.SetPlan");
            }

            return null;
        }

    }
}
