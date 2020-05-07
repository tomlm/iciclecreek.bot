using AdaptiveExpressions;
using Humanizer;
using Iciclecreek.Bot.Expressions.Humanizer;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Iciclecreek.AdaptiveExpressions.Tests
{
    [TestClass]
    public class HumanizerTests
    {
        public static IConfiguration DefaultConfiguration { get; set; } = new ConfigurationBuilder().AddInMemoryCollection().Build();

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            HumanizerFunctions.Register();
        }

        [TestMethod]
        public void ApplyCaseTests()
        {
            var state = new object();

            Assert.AreEqual("THIS IS A TEST", Expression.Parse($"humanizer.allCaps('this is a test')").TryEvaluate(state).value);
            Assert.AreEqual("this is a test", Expression.Parse($"humanizer.lowerCase('THIS IS A TEST')").TryEvaluate(state).value);
            Assert.AreEqual("This is a test", Expression.Parse($"humanizer.sentence('this is a test')").TryEvaluate(state).value);
            Assert.AreEqual("This Is A Test", Expression.Parse($"humanizer.title('this is a test')").TryEvaluate(state).value);
        }

        [TestMethod]
        public void DateTimeTests()
        {
            var start = DateTime.UtcNow - TimeSpan.FromHours(1);
            var state = new
            {
                date = start,
                target = start - TimeSpan.FromDays(365)
            };

            Assert.AreEqual(state.date.Humanize(), Expression.Parse($"humanizer.datetime(date)").TryEvaluate(state).value);
            Assert.AreEqual(state.date.Humanize(true, state.target), Expression.Parse($"humanizer.datetime(date, target )").TryEvaluate(state).value);
            Assert.AreEqual(state.target.ToOrdinalWords(), Expression.Parse($"humanizer.datetimeOrdinal(target)").TryEvaluate(state).value);
            Assert.AreEqual(state.target.ToOrdinalWords(GrammaticalCase.Genitive), Expression.Parse($"humanizer.datetimeOrdinal(target, 'Genitive')").TryEvaluate(state).value);
        }

        [TestMethod]
        public void BytesTests()
        {
            var state = new Object();
            Assert.AreEqual(256.Bits().ToString(), Expression.Parse($"humanizer.bits(256)").TryEvaluate(state).value);
            Assert.AreEqual(256.Bytes().ToString(), Expression.Parse($"humanizer.bytes(256)").TryEvaluate(state).value);
            Assert.AreEqual(65.Kilobytes().ToString(), Expression.Parse($"humanizer.kilobytes(65)").TryEvaluate(state).value);
            Assert.AreEqual(5.Megabytes().ToString(), Expression.Parse($"humanizer.megabytes(5)").TryEvaluate(state).value);
            Assert.AreEqual(5.Gigabytes().ToString(), Expression.Parse($"humanizer.gigabytes(5)").TryEvaluate(state).value);
            Assert.AreEqual(5.3.Terabytes().ToString(), Expression.Parse($"humanizer.terabytes(5.3)").TryEvaluate(state).value);
        }

        [TestMethod]
        public void HeadingTests()
        {
            var state = new
            {
                direction = "E",
                degrees = (double)110
            };

            Assert.AreEqual(state.direction.FromAbbreviatedHeading(), Expression.Parse($"humanizer.heading2degrees(direction)").TryEvaluate(state).value);
            Assert.AreEqual(state.degrees.ToHeading(), Expression.Parse($"humanizer.degrees2heading(degrees)").TryEvaluate(state).value);
            Assert.AreEqual(state.degrees.ToHeading(HeadingStyle.Abbreviated), Expression.Parse($"humanizer.degrees2heading(degrees, 'Abbreviated')").TryEvaluate(state).value);
            Assert.AreEqual(state.degrees.ToHeading(HeadingStyle.Full), Expression.Parse($"humanizer.degrees2heading(degrees, 'Full')").TryEvaluate(state).value);
        }

        [TestMethod]
        public void InflectorTests()
        {
            var state = new
            {
                person = "chris",
                singular = "cat",
                plural = "cats",
                test = "This is a test"
            };

            Assert.AreEqual(state.singular.Pluralize(), Expression.Parse($"humanizer.pluralize(singular)").TryEvaluate(state).value);
            Assert.AreEqual(state.singular.Pluralize(true), Expression.Parse($"humanizer.pluralize(singular, true)").TryEvaluate(state).value);
            Assert.AreEqual(state.person.Pluralize(false), Expression.Parse($"humanizer.pluralize(person)").TryEvaluate(state).value);

            Assert.AreEqual(state.plural.Singularize(), Expression.Parse($"humanizer.Singularize(plural)").TryEvaluate(state).value);
            Assert.AreEqual(state.plural.Singularize(false), Expression.Parse($"humanizer.Singularize(plural, false)").TryEvaluate(state).value);
            Assert.AreEqual(state.plural.Singularize(false, true), Expression.Parse($"humanizer.Singularize(plural, false, true)").TryEvaluate(state).value);

            Assert.AreEqual(state.test.Camelize(), Expression.Parse($"humanizer.camelize(test)").TryEvaluate(state).value);
            Assert.AreEqual(state.test.Hyphenate(), Expression.Parse($"humanizer.hyphenate(test)").TryEvaluate(state).value);
            Assert.AreEqual(state.test.Dasherize(), Expression.Parse($"humanizer.dasherize(test)").TryEvaluate(state).value);
            Assert.AreEqual(state.test.Kebaberize(), Expression.Parse($"humanizer.kebaberize(test)").TryEvaluate(state).value);
            Assert.AreEqual(state.test.Pascalize(), Expression.Parse($"humanizer.pascalize(test)").TryEvaluate(state).value);
            Assert.AreEqual(state.test.Titleize(), Expression.Parse($"humanizer.titleize(test)").TryEvaluate(state).value);
        }
    }
}
