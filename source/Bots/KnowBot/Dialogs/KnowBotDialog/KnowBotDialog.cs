﻿using Iciclecreek.Bot.Builder.Dialogs;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Lucy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Completions;
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
            _openAI = new OpenAIClient(new OpenAIAuthentication(openAIKey), new OpenAIClientSettings("text-davinci-003"));

            var yaml = new StreamReader(typeof(KnowBotDialog).Assembly.GetManifestResourceStream($"{typeof(KnowBotDialog).FullName}.{typeof(KnowBotDialog).Name}.yaml")).ReadToEnd();
            var yamlShared = new StreamReader(typeof(KnowBotDialog).Assembly.GetManifestResourceStream($"KnowBot.Dialogs.Shared.yaml")).ReadToEnd();

            this.Recognizer = new LucyRecognizer()
            {
                ExternalEntityRecognizer = BotHelp.GetSharedRecognizer(),
                Intents = new List<string>()
                {
                    "Help", "Reset", "ShowFacts" //,"Greeting"
                },
                Model = YamlConvert.DeserializeObject<LucyDocument>($"{yaml}\n\n{yamlShared}")
            };
        }

        // ----------------------- ACTIVITIES -----------------------
        #region ACTIVITIES
        protected async override Task<DialogTurnResult> OnConversationUpdateActivityAsync(DialogContext dc, IConversationUpdateActivity conversationUpdateActivity, CancellationToken cancellationToken)
        {
            return await this.OnEvaluateStateAsync(dc, cancellationToken);
        }

        #endregion

        // ----------------------- EVALUATE -----------------------
        #region EVALUATE
        protected override async Task<DialogTurnResult> OnEvaluateStateAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            if (!dc.State.TryGetValue("user.welcomed", out var val))
            {
                await dc.SendReplyText(Welcome);
                await dc.SendReplyText("Tell me about yourself! What are your interests, backgrounds, etc?");
                dc.State.SetValue("user.welcomed", true);
            }

            return await dc.WaitForInputAsync(cancellationToken);
        }
        #endregion

        private bool IsQuestion(string text)
        {
            text = text.Trim().ToLower();
            if (text.EndsWith("?"))
                return true;

            if (text.StartsWith("who ") ||
                text.StartsWith("what ") ||
                text.StartsWith("where ") ||
                text.StartsWith("how "))
                return true;

            return false;
        }

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

#if !gen1
                string facts = GetFacts(dc, messageActivity);
                facts += $"User: {text}\n";

                CompletionResult result = await GetAnswer(facts);
                var answer = result.Completions.FirstOrDefault();
                if (answer != null)
                {
                    facts += $"{answer.Text}\n";
                    var answerText = answer.Text.Trim();
                    if (answerText.StartsWith("Bot:"))
                        dc.AppendReplyText(answerText.Substring(4).TrimStart());
                    else
                        dc.AppendReplyText(answerText);
                }
                dc.State.SetValue("user.facts", facts);

#else
                if (!dc.State.TryGetValue("user.facts", out string facts))
                {
                    facts = "";

                    if (!String.IsNullOrEmpty(messageActivity.From.Name))
                    {
                        string name = messageActivity.From.Name.ToLower();
                        if (name != "user" && name != "you")
                        {
                            facts = $"My name is {messageActivity.From.Name}";
                        }
                    }
                }

                bool needResponse = true;
                var statements = Segmenter.Segment(text);
                var factStatements = statements.Where(u => !IsQuestion(u) && u.Split(' ', ',', '\'', '.', '!').Length > 2).ToList();
                var questions = statements.Where(u => IsQuestion(u)).ToList();
                if (factStatements.Any())
                {
                    facts = $"{facts}\n{String.Join('\n', factStatements)}";
                    if (!questions.Any())
                    {
                        dc.AppendReplyText($"Got it. ");
                        dc.AppendReplyText(Prompts);
                        needResponse = false;
                    }

                    dc.State.SetValue("user.facts", facts);
                }

                if (questions.Any())
                {
                    CompletionResult result = await GetAnswer(facts, questions);
                    var answer = result.Completions.FirstOrDefault() ?? "Hmmm. I guess I don't have an answer for that";
                    dc.AppendReplyText(answer
                            .Replace("I am ", "You are ")
                            .Replace("My ", "Your ")
                            .Replace("my ", "your ")
                            .Replace("myself ", "you ")
                            .Replace("I ", "You "));
                }
                else if (needResponse)
                {
                    questions.Add(messageActivity.Text);
                    CompletionResult result = await GetAnswer(facts, questions);
                    var answer = result.Completions.FirstOrDefault() ?? "Hmmm...";
                    dc.AppendReplyText(answer);
                }

#endif
            }
            await dc.SendReplyText();
            return await this.OnEvaluateStateAsync(dc, cancellationToken);
        }

        private static string GetFacts(DialogContext dc, IMessageActivity messageActivity)
        {
            if (!dc.State.TryGetValue("user.facts", out string facts))
            {
                facts = "NOTE: When the user asks a question the bot responds with the answer to the question. When the user says a factual statement the bot is interested in getting to know the user and so asks questions to get to know the user.";

                if (!String.IsNullOrEmpty(messageActivity.From.Name))
                {
                    string name = messageActivity.From.Name.ToLower();
                    if (name != "user" && name != "you")
                    {
                        facts += $"User: My name is {messageActivity.From.Name}.\n";
                    }
                }
            }

            return facts;
        }

        private async Task<CompletionResult> GetAnswer(string prompt) => await _openAI.CompletionsEndpoint.CreateCompletionAsync(new CompletionRequest()
        {
            Prompt = prompt,
            Temperature = 0.4,
            TopP = 1,
            MaxTokens = 300,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        });

        protected async Task<DialogTurnResult> OnShowFactsIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
#if !gen1
            string facts = GetFacts(dc, messageActivity);
            facts += $"User: Please summarize everything you know about user as a bulleted list grouped by person name.\n";

            CompletionResult result = await GetAnswer(facts);
            var answer = result.Completions.FirstOrDefault();
            if (answer != null)
            {
                var answerText = answer.Text.Trim();
                if (answerText.StartsWith("Bot:"))
                    dc.AppendReplyText(answerText.Substring(4).TrimStart());
                else
                    dc.AppendReplyText(answerText);
                await dc.SendReplyText();
            }
#else
            if (dc.State.TryGetValue("user.facts", out string factsText) && !String.IsNullOrEmpty(factsText))
            {
                //var facts = Segmenter.Segment(factsText);
                //foreach (var fact in facts)
                //{
                //    dc.AppendReplyText($"* {fact}\n");
                //}
                await dc.SendReplyText(factsText);
            }
            else
            {
                dc.AppendReplyText("I don't know anything about you yet.");
                await dc.SendReplyText(Prompts);
            }
#endif
            return await this.OnEvaluateStateAsync(dc, cancellationToken);
        }


        protected async Task<DialogTurnResult> OnResetIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            dc.ClearQuestion();
            dc.State.RemoveValue("user.facts");
            dc.State.RemoveValue("user.welcomed");
            await dc.SendReplyText("Ok, I have forgetten everything you told me.");
            return await this.OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnHelpIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            await dc.SendReplyText(Help);
            return await this.OnEvaluateStateAsync(dc, cancellationToken);
        }

        protected async Task<DialogTurnResult> OnGreetingIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken = default)
        {
            if (recognizerResult.Intents.First().Value.Score > 0.5)
            {
                await dc.SendReplyText(Greetings);
                return await this.OnEvaluateStateAsync(dc, cancellationToken);
            }
            else
            {
                return await OnUnrecognizedIntentAsync(dc, messageActivity, recognizerResult, cancellationToken);
            }
        }
#endregion

        // ----------------------- TEXT ------------------------
#region TEXT
        public static string[] Welcome = new string[]
        {
@"# Welcome!
I'm the **KnowBot**.  

I will learn from what you tell me.

Then you can ask questions about what I've learned.

Any facts you tell me I will remember.  Any questions you ask I will answer from my memory.
"
        };

        public static string[] Prompts = new string[]
{
    "Tell me more...",
    "Super interesting, can you tell me more?",
    "I'd love to learn more about your work...",
    "Tell me about where you live...",
    "Tell me about where you are from...",
    "Tell me about your hobbies...",
    "Tell me about your work...",
    "Tell me about your parents...",
    "Tell me about your family...",
    //"Where do you live?",
    //"Where did you grow up?",
    //"Where did you go to school?",
    //"Where did you grow up?",
    //"What do you want to do in your life?",
    //"Where would you like to travel?",
    //"What do you do for fun?",
    //"What are your interests?",
    //"What hobbies do you have?",
    //"When did you graduate?",
    //"How do you like your coffee?",
    //"What is your favorite food?",
    //"What is your favorite movie?",
    //"What is your favorite book?",
    //"What is your favorite place?",
};

        public static readonly string[] Help = new string[]
        {
$@"

Any facts you tell me I will remember.  Any questions you ask I will answer from my memory.

* **Show facts** - to see what I have remembered
* **Reset** - To make me forget everything I have remembered.
"
        };

        public static readonly string[] Greetings = new string[]
        {
            "Hi!","Hello!","Good Day!", "Greetings!"
        };

#endregion
    }
}
