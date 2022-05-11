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
using System.Threading;
using System.Threading.Tasks;
using YamlConverter;

namespace BeBot.Dialogs
{
    public partial class BeBotDialog : IcyDialog
    {
        private readonly IConfiguration _configuration;
        private readonly IndexSearcher _searcher;
        private readonly CloudQueueClient _cloudQueue;

        public BeBotDialog(IConfiguration configuration, CloudQueueClient cloudQueueClient, IndexSearcher searcher)
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

            this._configuration = configuration;
            this._searcher = searcher;
            this._cloudQueue = cloudQueueClient;
        }

        // ----------------------- ACTIVITIES -----------------------
        protected async override Task<DialogTurnResult> OnMessageActivityAsync(DialogContext dc, IMessageActivity messageActivity, CancellationToken cancellationToken)
        {
            if (!dc.State.TryGetValue("user.welcomed", out var val))
            {
                dc.AppendReplyText(BeBotDialogText.Welcome);
                await dc.SendReplyText(BeBotDialogText.Help);
                dc.State.SetValue("user.welcomed", true);
            }

            return await base.OnMessageActivityAsync(dc, messageActivity, cancellationToken);
        }

        // ----------------------- EVALUATE -----------------------
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

            await dc.SendReplyText(cancellationToken, BeBotDialogText.WhatNext);
            return await dc.WaitForInputAsync(cancellationToken);
        }

        // ----------------------- INTENTS ------------------------
        protected async override Task<DialogTurnResult> OnUnrecognizedIntentAsync(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            dc.AppendReplyText(BeBotDialogText.UnrecognizedResponse);
            await dc.SendReplyText(BeBotDialogText.Help);
            return await this.OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnCancelIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            dc.ClearQuestion();
            return await dc.WaitForInputAsync();
        }

        protected async Task<DialogTurnResult> OnSetPlanIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            return await dc.RouteDialogAsync<SetPlanDialog>(null, cancellationToken);
        }

        protected virtual async Task<DialogTurnResult> OnWhoQueryIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            return await dc.RouteDialogAsync<WhoQueryDialog>(null, cancellationToken);
        }
        protected async Task<DialogTurnResult> OnWhereQueryIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            return await dc.RouteDialogAsync<WhereQueryDialog>(null, cancellationToken);
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
                // we have intent but no name, ask for it.
                return await dc.AskQuestionAsync("UserAlias", BeBotDialogText.UserAlias_Ask);
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
                    dc.AppendReplyText(BeBotDialogText.UserAlias_Bad);
                    return await dc.AskQuestionAsync("UserAlias", BeBotDialogText.UserAlias_Ask);
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
                    dc.AppendReplyText(BeBotDialogText.UserAlias_Help);
                    return await dc.AskQuestionAsync("UserAlias", BeBotDialogText.UserAlias_Ask);

                default:
                    dc.AppendReplyText(BeBotDialogText.Help);
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
    }
}
