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
using RichardSzalay.MockHttp;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Tests
{
    [TestClass]
    public class PersonNameEntityRecognizerTests
    {
        public TestContext TestContext { get; set; }

        private static EntityRecognizerSet entitySet = new EntityRecognizerSet()
            {
                new PersonNameEntityRecognizer()
            };

        private static DialogContext dc = GetTurnContext("about: Thomas Laird-McConnell");

        [TestMethod]
        public async Task PersonNameEntityRecognizerTests_TestWithDC()
        {
            var entities = await entitySet.RecognizeEntitiesAsync(dc);
            ValidateEntities(entities);
        }

        [TestMethod]
        public async Task PersonNameEntityRecognizerTests_TestWithActivity()
        {
            var entities = await entitySet.RecognizeEntitiesAsync(dc, MessageFactory.Text("about: Thomas Laird-McConnell"));
            ValidateEntities(entities);
        }

        [TestMethod]
        public async Task PersonNameEntityRecognizerTests_TestWithText()
        {
            var entities = await entitySet.RecognizeEntitiesAsync(dc, "about: Thomas Laird-McConnell", "en-us");
            ValidateEntities(entities);
        }

        [TestMethod]
        public async Task PersonNameEntityRecognizerTests_TestWithSeperator()
        {
            var entities = await entitySet.RecognizeEntitiesAsync(dc, "about: Thomas, Laird-McConnell", "en-us");
            var givenName = entities.SingleOrDefault(e => e.Type == "givenName");
            var surname1 = entities.Where(e => e.Type == "surname").First();
            var surname2 = entities.Where(e => e.Type == "surname").Skip(1).Take(1).Single();
            var fullName = entities.SingleOrDefault(e => e.Type == "fullName");
            Assert.IsNotNull(givenName);
            Assert.IsNotNull(surname1);
            Assert.IsNotNull(surname2);
            Assert.IsNull(fullName);
            Assert.AreEqual("Thomas", (string)givenName.Properties["text"]);
            Assert.AreEqual("Thomas", (string)surname1.Properties["text"]);
            Assert.AreEqual("Laird-McConnell", (string)surname2.Properties["text"]);
        }

        [TestMethod]
        public async Task PersonNameEntityRecognizerTests_CommonNames()
        {
            var entities = await entitySet.RecognizeEntitiesAsync(dc, "about: is smith", "en-us");
            var givenName = entities.SingleOrDefault(e => e.Type == "givenName");
            var surname = entities.SingleOrDefault(e => e.Type == "surname");
            var fullName = entities.SingleOrDefault(e => e.Type == "fullName");
            Assert.IsNull(givenName);
            Assert.IsNotNull(surname);
            Assert.IsNull(fullName);
            Assert.AreEqual("smith", (string)surname.Properties["text"]);

            entities = await entitySet.RecognizeEntitiesAsync(dc, "about: Is Hill", "en-us");
            givenName = entities.SingleOrDefault(e => e.Type == "givenName");
            surname = entities.SingleOrDefault(e => e.Type == "surname");
            fullName = entities.SingleOrDefault(e => e.Type == "fullName");
            Assert.IsNotNull(givenName);
            Assert.IsNotNull(surname);
            Assert.IsNotNull(fullName);
            Assert.AreEqual("Is", (string)givenName.Properties["text"]);
            Assert.AreEqual("Hill", (string)surname.Properties["text"]);
            Assert.AreEqual("Is Hill", (string)fullName.Properties["text"]);
        }


        [TestMethod]
        public async Task PersonNameEntityRecognizerTests_HttpTest()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .When(HttpMethod.Get, "http://foo.com/customGivenNames.csv")
                .Respond("text/csv", System.IO.File.ReadAllText(@"..\..\..\customGivenNames.csv".Replace('\\', System.IO.Path.DirectorySeparatorChar)));
            mockHttp
                .When(HttpMethod.Get, "http://foo.com/customSurnames.csv")
                .Respond("text/csv", System.IO.File.ReadAllText(@"..\..\..\customSurnames.csv".Replace('\\', System.IO.Path.DirectorySeparatorChar)));

            EntityRecognizerSet entitySetHttp = new EntityRecognizerSet()
            {
                new PersonNameEntityRecognizer(new HttpClient(mockHttp))
                {
                    GivenNamesUrl = "http://foo.com/customGivenNames.csv",
                    SurnamesUrl = "http://foo.com/customSurnames.csv"
                }
            };

            var entities = await entitySetHttp.RecognizeEntitiesAsync(dc, "tom mcconnell, frank laird, smith", "en-us");
            var givenNames = entities.Where(e => e.Type == "givenName");
            var surNames = entities.Where(e => e.Type == "surname");
            var fullNames = entities.Where(e => e.Type == "fullName");
            Assert.AreEqual(1, givenNames.Count());
            Assert.AreEqual(2, surNames.Count());
            Assert.AreEqual(1, fullNames.Count());
        }

        [TestMethod]
        public async Task PersonNameEntityRecognizerTests_FileTest()
        {
            EntityRecognizerSet entitySetHttp = new EntityRecognizerSet()
            {
                new PersonNameEntityRecognizer()
                {
                    GivenNamesUrl = @"..\../..\customGivenNames.csv",
                    SurnamesUrl = @"../..\..\customSurnames.csv"
                }
            };

            var entities = await entitySetHttp.RecognizeEntitiesAsync(dc, "tom mcconnell, frank laird, smith", "en-us");
            var givenNames = entities.Where(e => e.Type == "givenName");
            var surNames = entities.Where(e => e.Type == "surname");
            var fullNames = entities.Where(e => e.Type == "fullName");
            Assert.AreEqual(1, givenNames.Count());
            Assert.AreEqual(2, surNames.Count());
            Assert.AreEqual(1, fullNames.Count());
        }

        private static void ValidateEntities(IList<Entity> entities)
        {
            var givenName = entities.SingleOrDefault(e => e.Type == "givenName");
            var surname1 = entities.Where(e => e.Type == "surname").First();
            var surname2 = entities.Where(e => e.Type == "surname").Skip(1).Take(1).Single();
            var fullName = entities.SingleOrDefault(e => e.Type == "fullName");
            Assert.IsNotNull(givenName);
            Assert.IsNotNull(surname1);
            Assert.IsNotNull(surname2);
            Assert.IsNotNull(fullName);
            Assert.AreEqual("Thomas", (string)givenName.Properties["text"]);
            Assert.AreEqual(7, (int)givenName.Properties["start"]);
            Assert.AreEqual(13, (int)givenName.Properties["end"]);
            Assert.AreEqual("Thomas", (string)surname1.Properties["text"]);
            Assert.AreEqual(7, (int)surname1.Properties["start"]);
            Assert.AreEqual(13, (int)surname1.Properties["end"]);
            Assert.AreEqual("Laird-McConnell", (string)surname2.Properties["text"]);
            Assert.AreEqual(14, (int)surname2.Properties["start"]);
            Assert.AreEqual(29, (int)surname2.Properties["end"]);
            Assert.AreEqual("Thomas Laird-McConnell", (string)fullName.Properties["text"]);
            Assert.AreEqual(7, (int)fullName.Properties["start"]);
            Assert.AreEqual(29, (int)fullName.Properties["end"]);
        }

        private static DialogContext GetTurnContext(string text, string locale = "en-us") => new DialogContext(
                new DialogSet(),
                new TurnContext(
                    new TestAdapter(),
                    new Activity(type: ActivityTypes.Message, text: text, locale: locale)),
                new DialogState());

    }
}
