using Iciclecreek.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BeBot.Dialogs
{
    public partial class BeBotDialog : IcyDialog
    {
        protected override async Task<DialogTurnResult> OnEvaluateStateAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            // --- user.alias missing 
            if (String.IsNullOrEmpty(dc.State.GetStringValue("user.alias")))
            {
                return await dc.AskQuestionAsync("UserAlias", BeBotDialogText.UserAlias_Ask);
            }

            // --- user.alias changed
            if (dc.IsStateChanged("user.alias"))
            {
                dc.AppendReplyText(BeBotDialogText.UserAlias_Changed);
            }

            // -- look for setPlan record
            if (dc.State.GetValue<object>("dialog.SetPlan") != null)
            {
                var dates = dc.State.GetValue<JArray>("dialog.SetPlan.When");
                if (dates == null || !dates.Any())
                {
                    return await dc.AskQuestionAsync("SetPlanWhen", BeBotDialogText.SetPlanWhen_Ask);
                }

                var location = dc.State.GetStringValue("dialog.SetPlan.Where");
                if (location == null)
                {
                    return await dc.AskQuestionAsync("SetPlanWhere", BeBotDialogText.SetPlanWhere_Ask);
                }
                // do setplan work;

                dc.AppendReplyText("Plan: ${dialog.SetPlan}");

                // remove temp SetPlan record.
                dc.State.RemoveValue("dialog.SetPlan");
            }

            await dc.SendReplyText(cancellationToken, BeBotDialogText.OpenQuestion);
            return await dc.WaitForInputAsync(cancellationToken);
        }
    }
}
