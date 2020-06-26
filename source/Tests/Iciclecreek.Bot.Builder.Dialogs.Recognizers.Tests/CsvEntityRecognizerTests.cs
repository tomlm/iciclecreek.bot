using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;
using Entity = Microsoft.Bot.Schema.Entity;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Tests
{
    [TestClass]
    public class CsvEntityRecognizerTests
    {
        public TestContext TestContext { get; set; }
        private static DialogContext dc = GetTurnContext("");

        [TestMethod]
        public async Task CsvEntityRecognizerTests_NoUrl()
        {
            var er = new CsvEntityRecognizer() { };

            try
            {
                await er.RecognizeEntitiesAsync(dc, new List<Entity>());
                Assert.Fail("Should have thrown exception");
            }
            catch (ArgumentNullException)
            {
            }
        }

        [TestMethod]
        public async Task CsvEntityRecognizerTests_BadUrl()
        {
            var er = new CsvEntityRecognizer() { Url = "foo://" };
            try
            {
                await er.RecognizeEntitiesAsync(dc, new List<Entity>());
                Assert.Fail("Should have thrown exception");
            }
            catch (Exception)
            {
            }
        }

        [TestMethod]
        public async Task CsvEntityRecognizerTests_FileTest()
        {
            EntityRecognizerSet entitySetLocalFile = new EntityRecognizerSet()
            {
                new CsvEntityRecognizer()
                {
                    Url = "../../../test.csv"
                }
            };


            var entities = await entitySetLocalFile.RecognizeEntitiesAsync(dc, "please recognize x1, x2, y1, y2...", "en-us");
            ValidateEntities(entities);
        }

        [TestMethod]
        public async Task CsvEntityRecognizerTests_HttpTest()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .When(HttpMethod.Get, "http://foo.com/test.csv")
                .Respond("text/csv", System.IO.File.ReadAllText(@"..\..\..\test.csv".Replace('\\', System.IO.Path.DirectorySeparatorChar)));

            EntityRecognizerSet entitySetHttp = new EntityRecognizerSet()
            {
                new CsvEntityRecognizer(new HttpClient(mockHttp))
                {
                    Url = "http://foo.com/test.csv"
                }
            };

            var entities = await entitySetHttp.RecognizeEntitiesAsync(dc, "please recognize x1, x2, y1, y2...", "en-us");
            ValidateEntities(entities);
        }

        private static void ValidateEntities(IList<Entity> entities)
        {
            var xs = entities.Where(e => e.Type == "x");
            var ys = entities.Where(e => e.Type == "y");
            var zs = entities.Where(e => e.Type == "z");
            Assert.AreEqual(2, xs.Count());
            Assert.AreEqual(2, ys.Count());
            Assert.AreEqual(0, zs.Count());
            dynamic x1 = JObject.FromObject(xs.Take(1).Single());
            dynamic x2 = JObject.FromObject(xs.Skip(1).Take(1).Single());
            Assert.AreEqual(15, (int)x1.resolution.value);
            Assert.AreEqual("foo", (string)x2.resolution.value);
            dynamic y1 = JObject.FromObject(ys.Take(1).Single());
            dynamic y2 = JObject.FromObject(ys.Skip(1).Take(1).Single());
            Assert.AreEqual(2.5F, (float)y1.resolution.value);
            Assert.AreEqual("bar", (string)y2.resolution.value);
        }

        private static DialogContext GetTurnContext(string text, string locale = "en-us") => new DialogContext(
                new DialogSet(),
                new TurnContext(
                    new TestAdapter(),
                    new Activity(type: ActivityTypes.Message, text: text, locale: locale)),
                new DialogState());

    }
}
