
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Tests
{
    [TestClass]
    public class ThresholdRecognizerTests
    {
        internal class TestRecognizer : Recognizer
        {
            private float distance;

            public TestRecognizer(float distance)
            {
                this.distance = distance;
            }

            public TestRecognizer([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0) : base(callerPath, callerLine)
            {
            }

            public async override Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, Activity activity, CancellationToken cancellationToken = default, Dictionary<string, string> telemetryProperties = null, Dictionary<string, double> telemetryMetrics = null)
            {
                var result = new RecognizerResult();
                if (activity.Text.Contains("foo"))
                {
                    result.Intents.Add("foo", new IntentScore() { Score = (float)1.0 });
                }

                if (activity.Text.Contains("bar"))
                {
                    result.Intents.Add("bar", new IntentScore() { Score = (float)1.0 - this.distance });
                }

                if (!result.Intents.Any())
                {
                    result.Intents.Add("None", new IntentScore() { Score = 1.0 });
                }

                return result;
            }
        }

        [TestMethod]
        public async Task TestChooseIntentFired_thresholds()
        {
            await GetTestScript(new ThresholdRecognizer()
            {
                Recognizer = new TestRecognizer((float)0.05)
            })
                .Send("foo")
                    .AssertReply("foo")
                .Send("bar")
                    .AssertReply("bar")
                .Send("foo bar")
                    .AssertReply("Which intent? (1) foo or (2) bar")
                .Send("1")
                    .AssertReply("foo")
                .Send("foo bar")
                    .AssertReply("Which intent? (1) foo or (2) bar")
                .Send("2")
                    .AssertReply("bar")
                .ExecuteAsync(new ResourceExplorer());

            await GetTestScript(new ThresholdRecognizer()
            {
                Recognizer = new TestRecognizer((float)0.3)
            })
                .Send("foo")
                    .AssertReply("foo")
                .Send("bar")
                    .AssertReply("bar")
                .Send("foo bar")
                    .AssertReply("foo")
                .ExecuteAsync(new ResourceExplorer());

            await GetTestScript(new ThresholdRecognizer()
            {
                Threshold = 0.5F,
                Recognizer = new TestRecognizer(0.3F)
            })
                .Send("foo")
                    .AssertReply("foo")
                .Send("bar")
                    .AssertReply("bar")
                .Send("foo bar")
                    .AssertReply("Which intent? (1) foo or (2) bar")
                .Send("1")
                    .AssertReply("foo")
                .Send("foo bar")
                    .AssertReply("Which intent? (1) foo or (2) bar")
                .Send("2")
                    .AssertReply("bar")
                .ExecuteAsync(new ResourceExplorer());
        }

        [TestMethod]
        public async Task TestChooseIntentFired_DifferentRecognizers()
        {
            await GetTestScript(new ThresholdRecognizer()
            {
                Recognizer = new RecognizerSet()
                {
                    Recognizers = new List<Recognizer>()
                    {
                        new RegexRecognizer()
                        {
                            Intents = new List<IntentPattern>()
                            {
                                new IntentPattern("foo", "foo")
                            }
                        },
                        new RegexRecognizer()
                        {
                            Intents = new List<IntentPattern>()
                            {
                                new IntentPattern("bar", "bar")
                            }
                        }
                    }
                }
            })
                .Send("foo")
                        .AssertReply("foo")
                    .Send("bar")
                        .AssertReply("bar")
                    .Send("foo bar")
                        .AssertReply("Which intent? (1) foo or (2) bar")
                    .Send("1")
                        .AssertReply("foo")
                    .Send("foo bar")
                        .AssertReply("Which intent? (1) foo or (2) bar")
                    .Send("2")
                        .AssertReply("bar")
                    .ExecuteAsync(new ResourceExplorer());
            ;
        }

        private static TestScript GetTestScript(Recognizer recognizer) =>
            new TestScript()
            {
                Dialog = new AdaptiveDialog()
                {
                    Recognizer = recognizer,
                    AutoEndDialog = false,
                    Triggers = new List<OnCondition>()
                      {
                          new OnChooseIntent()
                          {
                               Intents = new List<string> { "foo","bar" },
                               Actions = new List<Dialog>()
                               {
                                    new SetProperty()
                                    {
                                        Property = "dialog.candidates",
                                        Value = $"=turn.recognized.candidates"
                                    },
                                    new ChoiceInput()
                                    {
                                         Choices = "=select($candidates, result, result.intent)",
                                         Prompt = new ActivityTemplate("Which intent?")
                                    },
                                    new EmitEvent()
                                    {
                                        EventName = AdaptiveEvents.RecognizedIntent,
                                        EventValue = "=first(where($candidates, c, c.intent == turn.lastResult)).result"
                                    }
                               }
                          },
                          new OnIntent()
                          {
                              Intent = "foo",
                              Actions = new List<Dialog>()
                              {
                                  new SendActivity("foo")
                              }
                          },
                          new OnIntent()
                          {
                              Intent = "bar",
                              Actions = new List<Dialog>()
                              {
                                  new SendActivity("bar")
                              }
                          },
                          new OnUnknownIntent()
                          {
                              Actions= new List<Dialog>()
                              {
                                  new SendActivity("none")
                              }
                          }
                     }
                }
            };
    }
}
