using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Tests
{
    [TestClass]
    public class RecognizersTests
    {
        private Recognizer _recognizer = new RegexRecognizer() { Entities = new EntityRecognizerSet() { new QuotedTextEntityRecognizer() } };

        public async Task<RecognizerResult> Recognize(string text, string locale)
        {
            var activity = (Activity)Microsoft.Bot.Schema.Activity.CreateMessageActivity();
            activity.Locale = locale;
            activity.Text = text;
            var dc = new DialogContext(new DialogSet(), new TurnContext(new TestAdapter(), (Activity)activity), new DialogState());
            var entities = new List<Entity>();
            return await _recognizer.RecognizeAsync(dc, activity, default(CancellationToken));
        }

        [TestMethod]
        public async Task TestQuotedEntity_NullLocale()
        {
            var result = await Recognize("this is a `Isn't this cool?` „another quoted string”", null);
            dynamic quotedText = result.Entities["QuotedText"];
            Assert.AreEqual(1, quotedText.Count);
            Assert.AreEqual("Isn't this cool?", quotedText[0].ToString());
        }

        [TestMethod]
        public async Task TestQuotedEntity_English()
        {
            var result = await Recognize("this is a `Isn't this cool?` „another quoted string”", "en");
            dynamic quotedText = result.Entities["QuotedText"];
            Assert.AreEqual(1, quotedText.Count);
            Assert.AreEqual("Isn't this cool?", quotedText[0].ToString());
        }

        [TestMethod]
        public async Task TestQuotedEntity_Africaans()
        {
            var result = await Recognize("this is a `Isn't this cool?` „another quoted string”", "af");
            dynamic quotedText = result.Entities["QuotedText"];
            Assert.AreEqual(2, quotedText.Count);
            Assert.AreEqual("another quoted string", quotedText[0].ToString());
            Assert.AreEqual("Isn't this cool?", quotedText[1].ToString());
        }

        [TestMethod]
        public async Task TestQuotedEntity_Overlapping()
        {
            var result = await Recognize("this is a `this is \"a test\"` ", "en");
            dynamic quotedText = result.Entities["QuotedText"];
            Assert.AreEqual(2, quotedText.Count);
            Assert.AreEqual("a test", quotedText[0].ToString());
            Assert.AreEqual("this is \"a test\"", quotedText[1].ToString());
        }

        [TestMethod]
        public async Task TestQuotedEntity_OverlappingOffset()
        {
            var result = await Recognize("this is a `this is \"a` test\" ", "en");
            dynamic quotedText = result.Entities["QuotedText"];
            Assert.AreEqual(2, quotedText.Count);
            Assert.AreEqual("a` test", quotedText[0].ToString());
            Assert.AreEqual("this is \"a", quotedText[1].ToString());
        }

    }
}
