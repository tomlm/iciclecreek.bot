using Iciclecreek.Bot.Builder.Dialogs;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Lucene.Net.Search;
using Lucy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Queue;
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
                    dc.AppendReplyText("\n\nI didn't understand your response as an alias.  I'm looking for something like 'tomlm' or '@tomlm'");
                    return await dc.AskQuestionAsync("UserAlias", "\n\nWhat is your alias?");
                }
                dc.State.SetValue("user.alias", alias.TrimStart('@'));
            }

            return await OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnSetPlanWhenAnswer(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            // interpret text as dates answer
            var when = GetDateEntities(recognizerResult);
            if (when.Any())
            {
                dc.State.SetValue("dialog.SetPlan.When", when);
            }
            else
            {
                dc.AppendReplyText("\n\nI didn't understand your response as dates.  I'm looking for something like 'monday and friday'");
                return await dc.AskQuestionAsync("SetPlanWhen", "\n\nWhen will you be in this week?");
            }

            return await OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnSetPlanLocationAnswer(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
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
                dc.AppendReplyText("\n\nI didn't understand your response as a location.  I'm looking for something like 'work' or 'building 123'");
                return await dc.AskQuestionAsync("SetPlanWhere", "\n\nWhere will you be? (home, work, ...)");
            }

            return await OnEvaluateStateAsync(dc, cancellationToken);
        }

    }
}
