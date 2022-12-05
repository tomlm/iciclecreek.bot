using Iciclecreek.Bot.Builder.Dialogs;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Lucy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using OpenAI;
using PragmaticSegmenterNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YamlConverter;
using static System.Net.Mime.MediaTypeNames;

namespace KnowBot.Dialogs
{
    public class KnowBotDialog : IcyDialog
    {
        private OpenAIClient _openAI;

        public KnowBotDialog(IConfiguration configuration)
        {
            var openAIKey = configuration.GetValue<string>("OpenAIKey");
            _openAI = new OpenAIClient(new OpenAIAuthentication(openAIKey), new Engine("text-davinci-003"));

            var yaml = new StreamReader(typeof(KnowBotDialog).Assembly.GetManifestResourceStream($"{typeof(KnowBotDialog).FullName}.{typeof(KnowBotDialog).Name}.yaml")).ReadToEnd();
            var yamlShared = new StreamReader(typeof(KnowBotDialog).Assembly.GetManifestResourceStream($"KnowBot.Dialogs.Shared.yaml")).ReadToEnd();

            this.Recognizer = new LucyRecognizer()
            {
                ExternalEntityRecognizer = BotHelp.GetSharedRecognizer(),
                Intents = new List<string>()
                {
                    "Help", "Reset", "ShowFacts"
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
            return await dc.WaitForInputAsync(cancellationToken);
        }
        #endregion

        // ----------------------- INTENTS ------------------------
        #region INTENTS
        protected override async Task<DialogTurnResult> OnUnrecognizedIntentAsync(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            string text = messageActivity.Text.Trim();
            if (!String.IsNullOrEmpty(text))
            {
                if (!Char.IsPunctuation(text.Last()))
                {
                    text += ".";
                }

                if (!dc.State.TryGetValue("user.facts", out string facts))
                {
                    facts = "";
                    if (!String.IsNullOrEmpty(messageActivity.From.Name) && messageActivity.From.Name != "User")
                    {
                        facts = $"My name is {messageActivity.From.Name}";
                    }
                }

                var statements = Segmenter.Segment(text);
                var factStatements = statements.Where(u => !u.Trim().EndsWith("?"));
                var questions = statements.Where(u => u.Trim().EndsWith("?"));
                if (factStatements.Any() && !questions.Any())
                {
                    facts = $"{facts}\n{String.Join('\n', factStatements)}";
                    dc.AppendReplyText($"Got it. Tell me more...");
                    dc.State.SetValue("user.facts", facts);
                }

                if (questions.Any())
                {
                    var result = await _openAI.CompletionEndpoint.CreateCompletionAsync(new CompletionRequest()
                    {
                        Prompt = $"{facts}\n{String.Join('\n', questions)}",
                        Temperature = 0.4,
                        TopP = 1,
                        MaxTokens = 64,
                        FrequencyPenalty = 0,
                        PresencePenalty = 0,
                    });

                    dc.AppendReplyText(result.Completions.FirstOrDefault() ?? "Hmmm. I guess I don't have an answer for that");
                }
            }
            await dc.SendReplyText();
            return await this.OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnShowFactsIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            if (!dc.State.TryGetValue("user.facts", out string factsText))
            {
                factsText = "";
            }
            var facts = Segmenter.Segment(factsText);

            foreach (var fact in facts)
            {
                dc.AppendReplyText($"* {fact}");
            }
            await dc.SendReplyText();
            return await this.OnEvaluateStateAsync(dc, cancellationToken);
        }


        protected async Task<DialogTurnResult> OnResetIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            dc.ClearQuestion();
            dc.State.SetValue("user.facts", "");
            dc.AppendReplyText("Ok, I have forgetten everything you told me.");
            await dc.SendReplyText(Welcome);
            return await this.OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnHelpIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            await dc.SendReplyText(Help);
            return await this.OnEvaluateStateAsync(dc, cancellationToken);
        }
        #endregion

        // ----------------------- TEXT ------------------------
        #region TEXT
        public static string[] Welcome = new string[]
        {
@"### Welcome!
I'm the **KnowBot**.  

Tell me about yourself.  I will learn from what you tell me.

Then you can ask questions about what I've learned.

"
        };

        public static readonly string[] Help = new string[]
        {
$@"

Any facts you tell me I will remember.  Any questions you have about the facts I will answer.

"
        };
        #endregion
    }
}
