using Iciclecreek.Bot.Builder.Dialogs;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Lucy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YamlConverter;

namespace BeBot.Dialogs
{
    public class BeBotDialog : IcyDialog
    {

        public BeBotDialog()
        {

            var yaml = new StreamReader(typeof(BeBotDialog).Assembly.GetManifestResourceStream($"{typeof(BeBotDialog).FullName}.{typeof(BeBotDialog).Name}.yaml")).ReadToEnd();
            var yamlShared = new StreamReader(typeof(BeBotDialog).Assembly.GetManifestResourceStream($"BeBot.Dialogs.Shared.yaml")).ReadToEnd();

            this.Recognizer = new LucyRecognizer()
            {
                ExternalEntityRecognizer = BeBotHelp.GetSharedRecognizer(),
                Intents = new List<string>()
                {
                    "Help", "Cancel", "Greeting", "Goodbye", "Thanks",
                    "ChangeAlias", "SetPlan", "WhoQuery", "WhereQuery"
                },
                Model = YamlConvert.DeserializeObject<LucyDocument>($"{yaml}\n\n{yamlShared}")
            };

        }

        // ----------------------- ACTIVITIES -----------------------
        #region ACTIVITIES
        protected async override Task<DialogTurnResult> OnMessageActivityAsync(DialogContext dc, IMessageActivity messageActivity, CancellationToken cancellationToken)
        {
            if (!dc.State.TryGetValue("user.welcomed", out var val))
            {
                dc.AppendReplyText(Welcome);
                await dc.SendReplyText(Help);
                dc.State.SetValue("user.welcomed", true);
            }

            return await base.OnMessageActivityAsync(dc, messageActivity, cancellationToken);
        }
        #endregion

        // ----------------------- EVALUATE -----------------------
        #region EVALUATE
        protected override async Task<DialogTurnResult> OnEvaluateStateAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            // --- user.alias missing 
            if (String.IsNullOrEmpty(dc.State.GetStringValue("user.alias")))
            {
                return await dc.AskQuestionAsync("UserAlias", UserAlias_Ask);
            }

            // --- user.alias changed
            if (dc.IsStateChanged("user.alias"))
            {
                dc.AppendReplyText(UserAlias_Changed);
            }

            await dc.SendReplyText(cancellationToken, WhatNext);
            return await dc.WaitForInputAsync(cancellationToken);
        }
        #endregion

        // ----------------------- INTENTS ------------------------
        #region INTENTS
        protected override async Task<DialogTurnResult> OnUnrecognizedIntentAsync(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            dc.AppendReplyText(UnrecognizedResponse);
            await dc.SendReplyText(Help);
            return await this.OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnCancelIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            dc.ClearQuestion();
            return await dc.WaitForInputAsync();
        }

        protected async Task<DialogTurnResult> OnSetPlanIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
            => await dc.RouteDialogAsync<SetPlanDialog>(null, cancellationToken);

        protected async Task<DialogTurnResult> OnWhoQueryIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
            => await dc.RouteDialogAsync<WhoQueryDialog>(null, cancellationToken);

        protected async Task<DialogTurnResult> OnWhereQueryIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
            => await dc.RouteDialogAsync<WhereQueryDialog>(null, cancellationToken);

        protected virtual async Task<DialogTurnResult> OnChangeAliasIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            var alias = recognizerResult.GetEntities<string>("$..alias").FirstOrDefault() ?? String.Empty;
            if (!String.IsNullOrEmpty(alias))
            {
                dc.State.SetValue("user.alias", alias);
            }
            else
            {
                // we have intent but no name, ask for it.
                return await dc.AskQuestionAsync("UserAlias", UserAlias_Ask);
            }

            return await OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnUserAliasAnswer(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            // look for a @mention
            var alias = recognizerResult.GetEntities<string>("$..mention..value").FirstOrDefault();
            if (alias == null)
            {
                alias = messageActivity.Text.Trim();
            }

            if (!String.IsNullOrEmpty(alias))
            {
                // assume it is a text response with single word
                if (alias.Contains(" "))
                {
                    dc.AppendReplyText(UserAlias_Bad);
                    return await dc.AskQuestionAsync("UserAlias", UserAlias_Ask);
                }

                // save new alias
                dc.State.SetValue("user.alias", alias.TrimStart('@'));
            }

            return await OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnHelpIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            switch (dc.GetLastQuestion())
            {
                case "UserAlias":
                    dc.AppendReplyText(UserAlias_Help);
                    return await dc.AskQuestionAsync("UserAlias", UserAlias_Ask);

                default:
                    dc.AppendReplyText(Help);
                    break;
            }

            await dc.SendReplyText(cancellationToken);
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
        #endregion

        // ----------------------- TEXT ------------------------
        #region TEXT
        public static string[] Welcome = new string[]
        {
@"### Welcome!
I'm **BeBot**, the hybrid worker bot.

Every Sunday I'll ask you what your plans are for the week.

"
        };

        public static readonly string[] Help = new string[]
        {
$@"

You can **set your schedule** by saying stuff like this:
* *I will be in city center Monday and Friday.*
* *My plan is to be at work on Mondays and Thursdays.*

You can ask **who** and **where** questions about your coworkers schedule:
* *Who will be in city center Monday?*
* *Where are lilich and sgellock today?*

"
        };

        public static readonly string[] UnrecognizedResponse = new string[]
        {
            " No capiche...",
            " I'm sorry, I didn't understand that response.",
            " Hmmm...I don't get it.",
            " Do you need help? Because I didn't understand that.",
            " Oh oh, is your cat typing again?  That made no sense to me.",
            " Apprently I'm not smart enough to understand you.",
        };

        public static readonly string[] WhatNext = new string[]
        {
            " What can I do for you?",
            " What is your wish?",
            " Your wish is my command, so what is your wish?",
            " Ask me to do something, I dare ya! What do you wanna do?",
            " What do you wanna do now?",
        };

        public static readonly string[] UserAlias_Help = new string[]
        {
             " I need to know your alias to operate correctly.",
             " Bots like me work better with aliases, because we don't really understand names.",
             " I'm a dummy, I don't understand names, I need an alias to work.",
        };
        public static readonly string[] UserAlias_Ask = new string[]
        {
            " What is your alias?",
            " I need to know your alias. Can you please provide it?"
        };

        public static readonly string[] UserAlias_Changed = new string[]
        {
            " Your alias is now @${user.alias}",
            " Got it, @${user.alias}",
            " Cool, cool, cool, I know your alias is @${user.alias}.",
            " Roger dodger @${user.alias} is it.",
            " Hail @${user.alias}!"
        };

        public static readonly string[] UserAlias_Bad = new string[]
        {
            "\n\nI didn't understand your response as an alias.  I'm looking for something like *tomlm* or *@tomlm*"
        };
        #endregion
    }
}
