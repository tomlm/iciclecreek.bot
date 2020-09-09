
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.QLucene;
using Lucene.Net.Store;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Tests
{
    [TestClass]
    public class QluceneRecognizerTests
    {
        public static ResourceExplorer ResourceExplorer { get; set; }

        public static Directory IndexDirectory { get; set; }

        public static string Json { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext context)
        {
            ComponentRegistration.Add(new DeclarativeComponentRegistration());
            ComponentRegistration.Add(new DialogsComponentRegistration());
            ComponentRegistration.Add(new AdaptiveComponentRegistration());
            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());
            ComponentRegistration.Add(new AdaptiveTestingComponentRegistration());
            ComponentRegistration.Add(new QLuceneComponentRegistration());

            var parent = Environment.CurrentDirectory;
            while (!string.IsNullOrEmpty(parent))
            {
                if (System.IO.Directory.EnumerateFiles(parent, "*proj").Any())
                {
                    break;
                }
                else
                {
                    parent = System.IO.Path.GetDirectoryName(parent);
                }
            }

            ResourceExplorer = new ResourceExplorer();
            ResourceExplorer.AddFolder(parent, monitorChanges: false);

            var resource = ResourceExplorer.GetResource("test-ram.en-us.qna.json");
            Json = await resource.ReadTextAsync();
            IndexDirectory = new RAMDirectory();
            QLuceneEngine.CreateCatalog(Json, IndexDirectory);
        }

        [TestMethod]
        public async Task TestQlucene()
        {
            var qlu = new QLuceneEngine(IndexDirectory);

            Assert.AreEqual("The only thing I'm committed to is being a great friend.", qlu.GetAnswers("Do you have a boyfriend").Answer);
            Assert.AreEqual("People made me out of code and a dash of ingenuity.", qlu.GetAnswers("Who created you?").Answer);
            Assert.AreEqual("Definitely didn't see that coming!", qlu.GetAnswers("Do you want to get married ? ").Answer);

            var answer = qlu.GetAnswers("help");
            Assert.AreEqual(228, answer.Id);
            Assert.AreEqual(1, answer.Context.Prompts.Length);
            Assert.AreEqual("Learn more", answer.Context.Prompts[0].DisplayText);

            answer = qlu.GetAnswers("Learn more", context: new QnARequestContext()
            {
                PreviousQnAId = 228,
                PreviousUserQuery = "help"
            });
            Assert.AreEqual(227, answer.Id);
        }

        [TestMethod]
        public async Task TestQluceneRecognizer_ram()
        {
            var script = ResourceExplorer.LoadType<TestScript>("QLucene_TestRecognizer_ram.test.dialog");
            await script.ExecuteAsync(ResourceExplorer);
        }

        [TestMethod]
        public async Task TestQluceneRecognizer_cached()
        {
            var script = ResourceExplorer.LoadType<TestScript>("QLucene_TestRecognizer_cached.test.dialog");
            await script.ExecuteAsync(ResourceExplorer);
        }

        [TestMethod]
        public async Task TestQluceneRecognizerMultiLanguage()
        {
            var script = ResourceExplorer.LoadType<TestScript>("QLucene_TestMultiRecognizer.test.dialog");
            await script.ExecuteAsync(ResourceExplorer);
        }

        [TestMethod]
        public async Task TestQLuceneRecognizerCode()
        {
            var resource = ResourceExplorer.GetResource("test-ram.en-us.qna.json");
            var json = await resource.ReadTextAsync();

            await new TestScript()
            {
                Dialog = new AdaptiveDialog()
                {
                    Recognizer = new QLuceneRecognizer(resource.Id, json),
                    Triggers = new List<OnCondition>()
                    {
                        new OnIntent()
                        {
                            Intent = "QnAMatch",
                            Actions = new List<Dialog>()
                            {
                                new SendActivity("${turn.recognized.entities.answer[0]}")
                            }
                        }
                    }
                }
            }
            .Send("Do you have a boyfriend")
                .AssertReply("The only thing I'm committed to is being a great friend.")
             .Send("Who created you?")
                .AssertReply("People made me out of code and a dash of ingenuity.")
            .Send("Do you want to get married ?")
                .AssertReply("Definitely didn't see that coming!")
            .ExecuteAsync(ResourceExplorer);
        }
    }
}
