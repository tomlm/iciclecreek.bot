using AdaptiveExpressions;
using Humanizer;
using Iciclecreek.Bot.Expressions.Humanizer;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;

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

        [TestInitialize]
        public void TestInitialize()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
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
        public void HumanizeDateTimeTests()
        {
            var start = DateTime.UtcNow - TimeSpan.FromHours(1);
            var state = new
            {
                date = start,
                target = start - TimeSpan.FromDays(365)
            };

            Assert.AreEqual(state.date.Humanize(), Expression.Parse($"humanizer.humanizeDateTime(date)").TryEvaluate(state).value);
            Assert.AreEqual(state.date.Humanize(true, state.target), Expression.Parse($"humanizer.humanizeDateTime(date, target )").TryEvaluate(state).value);

            var state2 = JObject.FromObject(state);

            Assert.AreEqual(state.date.Humanize(), Expression.Parse($"humanizer.humanizeDateTime(date)").TryEvaluate(state2).value);
            Assert.AreEqual(state.date.Humanize(true, state.target), Expression.Parse($"humanizer.humanizeDateTime(date, target )").TryEvaluate(state2).value);

            var culture = CultureInfo.GetCultureInfo("fr");
            Assert.AreEqual(state.date.Humanize(culture: culture), Expression.Parse($"humanizer.humanizeDateTime(date, 'fr')").TryEvaluate(state).value);
            Assert.AreEqual(state.date.Humanize(true, state.target, culture: culture), Expression.Parse($"humanizer.humanizeDateTime(date, target, 'fr')").TryEvaluate(state).value);

            Thread.CurrentThread.CurrentCulture = culture;
            Assert.AreEqual(state.date.Humanize(culture: culture), Expression.Parse($"humanizer.humanizeDateTime(date)").TryEvaluate(state).value);
            Assert.AreEqual(state.date.Humanize(true, state.target, culture: culture), Expression.Parse($"humanizer.humanizeDateTime(date, target )").TryEvaluate(state).value);
        }

        [TestMethod]
        public void DateTimeToOrdinalWordsTests()
        {
            var start = DateTime.UtcNow - TimeSpan.FromHours(1);
            var state = new
            {
                date = start,
                target = start - TimeSpan.FromDays(365)
            };

            // Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("fr");

            Assert.AreEqual(state.target.ToOrdinalWords(), Expression.Parse($"humanizer.dateTimeToOrdinalWords(target)").TryEvaluate(state).value);
            Assert.AreEqual(state.target.ToOrdinalWords(GrammaticalCase.Genitive), Expression.Parse($"humanizer.dateTimeToOrdinalWords(target, 'Genitive')").TryEvaluate(state).value);

            var state2 = JObject.FromObject(state);

            Assert.AreEqual(state.target.ToOrdinalWords(), Expression.Parse($"humanizer.dateTimeToOrdinalWords(target)").TryEvaluate(state2).value);
            Assert.AreEqual(state.target.ToOrdinalWords(GrammaticalCase.Genitive), Expression.Parse($"humanizer.dateTimeToOrdinalWords(target, 'Genitive')").TryEvaluate(state2).value);
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

            var culture = CultureInfo.GetCultureInfo("fr");
            Assert.AreEqual(state.degrees.ToHeading(culture: culture), Expression.Parse($"humanizer.degrees2heading(degrees, 'fr')").TryEvaluate(state).value);
            Assert.AreEqual(state.degrees.ToHeading(HeadingStyle.Abbreviated, culture: culture), Expression.Parse($"humanizer.degrees2heading(degrees, 'Abbreviated', 'fr')").TryEvaluate(state).value);
            Assert.AreEqual(state.degrees.ToHeading(HeadingStyle.Full, culture: culture), Expression.Parse($"humanizer.degrees2heading(degrees, 'Full', 'fr')").TryEvaluate(state).value);

            Thread.CurrentThread.CurrentCulture = culture;
            Assert.AreEqual(state.degrees.ToHeading(culture: culture), Expression.Parse($"humanizer.degrees2heading(degrees)").TryEvaluate(state).value);
            Assert.AreEqual(state.degrees.ToHeading(HeadingStyle.Abbreviated, culture: culture), Expression.Parse($"humanizer.degrees2heading(degrees, 'Abbreviated')").TryEvaluate(state).value);
            Assert.AreEqual(state.degrees.ToHeading(HeadingStyle.Full, culture: culture), Expression.Parse($"humanizer.degrees2heading(degrees, 'Full')").TryEvaluate(state).value);
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

        [TestMethod]
        public void MetricTests()
        {
            var state = new
            {
                value = 103.342234234d,
                metric = 103.342234234d.ToMetric()
            };

            Assert.AreEqual(state.value.ToMetric(), Expression.Parse($"humanizer.number2metric(value)").TryEvaluate(state).value);
            Assert.AreEqual(state.value.ToMetric(decimals: 2), Expression.Parse($"humanizer.number2metric(value, 2)").TryEvaluate(state).value);
            Assert.AreEqual(state.value.ToMetric(true, decimals: 2), Expression.Parse($"humanizer.number2metric(value, 2, true)").TryEvaluate(state).value);
            Assert.AreEqual(state.value.ToMetric(true, false, decimals: 2), Expression.Parse($"humanizer.number2metric(value, 2, true, false)").TryEvaluate(state).value);
            Assert.AreEqual(state.metric.FromMetric(), Expression.Parse($"humanizer.metric2number(metric)").TryEvaluate(state).value);
        }

        [TestMethod]
        public void HumanizeTimeSpanTests()
        {
            var state = new
            {
                span = TimeSpan.FromDays(103.43),
                value = 1303.43d
            };

            Assert.AreEqual(state.span.Humanize(), Expression.Parse($"humanizer.humanizeTimeSpan(span)").TryEvaluate(state).value);
            Assert.AreEqual(state.span.Humanize(2), Expression.Parse($"humanizer.humanizeTimeSpan(span,2)").TryEvaluate(state).value);
            Assert.AreEqual(state.span.Humanize(3), Expression.Parse($"humanizer.humanizeTimeSpan(span,3)").TryEvaluate(state).value);
            Assert.AreEqual(state.value.Weeks().Humanize(), Expression.Parse($"humanizer.humanizeTimeSpan(humanizer.weeks(value))").TryEvaluate(state).value);
            Assert.AreEqual(state.value.Days().Humanize(), Expression.Parse($"humanizer.humanizeTimeSpan(humanizer.days(value))").TryEvaluate(state).value);
            Assert.AreEqual(state.value.Hours().Humanize(), Expression.Parse($"humanizer.humanizeTimeSpan(humanizer.hours(value))").TryEvaluate(state).value);
            Assert.AreEqual(state.value.Minutes().Humanize(), Expression.Parse($"humanizer.humanizeTimeSpan(humanizer.minutes(value))").TryEvaluate(state).value);
            Assert.AreEqual(state.value.Seconds().Humanize(), Expression.Parse($"humanizer.humanizeTimeSpan(humanizer.seconds(value))").TryEvaluate(state).value);
            Assert.AreEqual(state.value.Milliseconds().Humanize(), Expression.Parse($"humanizer.humanizeTimeSpan(humanizer.milliseconds(value))").TryEvaluate(state).value);

            Assert.AreEqual(state.value.Weeks().Humanize(3), Expression.Parse($"humanizer.humanizeTimeSpan(humanizer.weeks(value),3)").TryEvaluate(state).value);
            Assert.AreEqual(state.value.Days().Humanize(3), Expression.Parse($"humanizer.humanizeTimeSpan(humanizer.days(value),3)").TryEvaluate(state).value);
            Assert.AreEqual(state.value.Hours().Humanize(3), Expression.Parse($"humanizer.humanizeTimeSpan(humanizer.hours(value),3)").TryEvaluate(state).value);
            Assert.AreEqual(state.value.Minutes().Humanize(3), Expression.Parse($"humanizer.humanizeTimeSpan(humanizer.minutes(value),3)").TryEvaluate(state).value);
            Assert.AreEqual(state.value.Seconds().Humanize(3), Expression.Parse($"humanizer.humanizeTimeSpan(humanizer.seconds(value),3)").TryEvaluate(state).value);
            Assert.AreEqual(state.value.Milliseconds().Humanize(3), Expression.Parse($"humanizer.humanizeTimeSpan(humanizer.milliseconds(value),3)").TryEvaluate(state).value);

            var state2 = JObject.FromObject(state);
            Assert.AreEqual(state.span.Humanize(), Expression.Parse($"humanizer.humanizeTimeSpan(span)").TryEvaluate(state2).value);
            Assert.AreEqual(state.span.Humanize(2), Expression.Parse($"humanizer.humanizeTimeSpan(span,2)").TryEvaluate(state2).value);
            Assert.AreEqual(state.span.Humanize(3), Expression.Parse($"humanizer.humanizeTimeSpan(span,3)").TryEvaluate(state2).value);
            Assert.AreEqual(state.value.Weeks().Humanize(3), Expression.Parse($"humanizer.humanizeTimeSpan(humanizer.weeks(value), 3)").TryEvaluate(state2).value);
            Assert.AreEqual(state.value.Days().Humanize(3), Expression.Parse($"humanizer.humanizeTimeSpan(humanizer.days(value), 3)").TryEvaluate(state2).value);
            Assert.AreEqual(state.value.Hours().Humanize(3), Expression.Parse($"humanizer.humanizeTimeSpan(humanizer.hours(value), 3)").TryEvaluate(state2).value);
            Assert.AreEqual(state.value.Minutes().Humanize(3), Expression.Parse($"humanizer.humanizeTimeSpan(humanizer.minutes(value), 3)").TryEvaluate(state2).value);
            Assert.AreEqual(state.value.Seconds().Humanize(3), Expression.Parse($"humanizer.humanizeTimeSpan(humanizer.seconds(value), 3)").TryEvaluate(state2).value);
            Assert.AreEqual(state.value.Milliseconds().Humanize(3), Expression.Parse($"humanizer.humanizeTimeSpan(humanizer.milliseconds(value), 3)").TryEvaluate(state2).value);

            var culture = CultureInfo.GetCultureInfo("fr");
            Assert.AreEqual(state.span.Humanize(culture: culture), Expression.Parse($"humanizer.humanizeTimeSpan(span,'fr')").TryEvaluate(state).value);
            Assert.AreEqual(state.span.Humanize(2, culture: culture), Expression.Parse($"humanizer.humanizeTimeSpan(span,2,'fr')").TryEvaluate(state).value);
            Assert.AreEqual(state.span.Humanize(3, culture: culture), Expression.Parse($"humanizer.humanizeTimeSpan(span,3,'fr')").TryEvaluate(state).value);

            Thread.CurrentThread.CurrentCulture = culture;
            Assert.AreEqual(state.span.Humanize(culture: culture), Expression.Parse($"humanizer.humanizeTimeSpan(span)").TryEvaluate(state).value);
            Assert.AreEqual(state.span.Humanize(2, culture: culture), Expression.Parse($"humanizer.humanizeTimeSpan(span,2)").TryEvaluate(state).value);
            Assert.AreEqual(state.span.Humanize(3, culture: culture), Expression.Parse($"humanizer.humanizeTimeSpan(span,3)").TryEvaluate(state).value);

        }

        [TestMethod]
        public void Number2WordsTests()
        {
            var state = new
            {
                integer = 13,
                str = "13"
            };

            Assert.AreEqual(state.integer.ToWords(), Expression.Parse($"humanizer.number2words(integer)").TryEvaluate(state).value);
            Assert.AreEqual(state.integer.ToWords(GrammaticalGender.Neuter), Expression.Parse($"humanizer.number2words(integer, 'Neuter')").TryEvaluate(state).value);
            Assert.AreEqual(state.integer.ToWords(GrammaticalGender.Masculine), Expression.Parse($"humanizer.number2words(integer, 'Masculine')").TryEvaluate(state).value);
            Assert.AreEqual(state.integer.ToWords(GrammaticalGender.Feminine), Expression.Parse($"humanizer.number2words(integer, 'Feminine')").TryEvaluate(state).value);

            Assert.AreEqual(state.integer.ToOrdinalWords(), Expression.Parse($"humanizer.number2ordinal(integer)").TryEvaluate(state).value);
            Assert.AreEqual(state.integer.ToOrdinalWords(GrammaticalGender.Neuter), Expression.Parse($"humanizer.number2ordinal(integer, 'Neuter')").TryEvaluate(state).value);
            Assert.AreEqual(state.integer.ToOrdinalWords(GrammaticalGender.Masculine), Expression.Parse($"humanizer.number2ordinal(integer, 'Masculine')").TryEvaluate(state).value);
            Assert.AreEqual(state.integer.ToOrdinalWords(GrammaticalGender.Feminine), Expression.Parse($"humanizer.number2ordinal(integer, 'Feminine')").TryEvaluate(state).value);

            Assert.AreEqual(state.str.Ordinalize(), Expression.Parse($"humanizer.ordinalize(str)").TryEvaluate(state).value);
            Assert.AreEqual(state.str.Ordinalize(GrammaticalGender.Neuter), Expression.Parse($"humanizer.ordinalize(str, 'Neuter')").TryEvaluate(state).value);
            Assert.AreEqual(state.str.Ordinalize(GrammaticalGender.Masculine), Expression.Parse($"humanizer.ordinalize(str, 'Masculine')").TryEvaluate(state).value);
            Assert.AreEqual(state.str.Ordinalize(GrammaticalGender.Feminine), Expression.Parse($"humanizer.ordinalize(str, 'Feminine')").TryEvaluate(state).value);

            var culture = CultureInfo.GetCultureInfo("fr");
            Assert.AreEqual(state.integer.ToWords(culture: culture), Expression.Parse($"humanizer.number2words(integer, 'fr')").TryEvaluate(state).value);
            Assert.AreEqual(state.integer.ToWords(GrammaticalGender.Masculine, culture: culture), Expression.Parse($"humanizer.number2words(integer, 'Masculine', 'fr')").TryEvaluate(state).value);

            Assert.AreEqual(state.integer.ToOrdinalWords(culture: culture), Expression.Parse($"humanizer.number2ordinal(integer, 'fr')").TryEvaluate(state).value);
            Assert.AreEqual(state.integer.ToOrdinalWords(GrammaticalGender.Masculine, culture: culture), Expression.Parse($"humanizer.number2ordinal(integer, 'Masculine', 'fr')").TryEvaluate(state).value);

            Assert.AreEqual(state.str.Ordinalize(culture: culture), Expression.Parse($"humanizer.ordinalize(str, 'fr')").TryEvaluate(state).value);
            Assert.AreEqual(state.str.Ordinalize(GrammaticalGender.Masculine, culture: culture), Expression.Parse($"humanizer.ordinalize(str, 'Masculine', 'fr')").TryEvaluate(state).value);

            Thread.CurrentThread.CurrentCulture = culture;
            Assert.AreEqual(state.integer.ToWords(culture: culture), Expression.Parse($"humanizer.number2words(integer)").TryEvaluate(state).value);
            Assert.AreEqual(state.integer.ToWords(GrammaticalGender.Masculine, culture: culture), Expression.Parse($"humanizer.number2words(integer, 'Masculine')").TryEvaluate(state).value);

            Assert.AreEqual(state.integer.ToOrdinalWords(culture: culture), Expression.Parse($"humanizer.number2ordinal(integer)").TryEvaluate(state).value);
            Assert.AreEqual(state.integer.ToOrdinalWords(GrammaticalGender.Masculine, culture: culture), Expression.Parse($"humanizer.number2ordinal(integer, 'Masculine')").TryEvaluate(state).value);

            Assert.AreEqual(state.str.Ordinalize(culture: culture), Expression.Parse($"humanizer.ordinalize(str)").TryEvaluate(state).value);
            Assert.AreEqual(state.str.Ordinalize(GrammaticalGender.Masculine, culture: culture), Expression.Parse($"humanizer.ordinalize(str, 'Masculine')").TryEvaluate(state).value);
        }

        [TestMethod]
        public void RomanTests()
        {
            for (int i = 1; i < 5; i++)
            {
                var state = new
                {
                    integer = i,
                    roman = i.ToRoman()
                };

                Assert.AreEqual(state.integer.ToRoman(), Expression.Parse($"humanizer.toRoman(integer)").TryEvaluate(state).value);
                Assert.AreEqual(state.roman.FromRoman(), Expression.Parse($"humanizer.fromRoman(roman)").TryEvaluate(state).value);
            }
        }

        [TestMethod]
        public void ToQuantityTest()
        {
            for (int i = 0; i < 5; i++)
            {
                var state = new
                {
                    integer = i,
                    name = "turtle"
                };

                Assert.AreEqual(state.name.ToQuantity(state.integer), Expression.Parse($"humanizer.toQuantity(name, integer)").TryEvaluate(state).value);
                Assert.AreEqual(state.name.ToQuantity(state.integer, ShowQuantityAs.None), Expression.Parse($"humanizer.toQuantity(name, integer, 'None')").TryEvaluate(state).value);
                Assert.AreEqual(state.name.ToQuantity(state.integer, ShowQuantityAs.Numeric), Expression.Parse($"humanizer.toQuantity(name, integer, 'Numeric')").TryEvaluate(state).value);
                Assert.AreEqual(state.name.ToQuantity(state.integer, ShowQuantityAs.Words), Expression.Parse($"humanizer.toQuantity(name, integer, 'Words')").TryEvaluate(state).value);
            }

            Assert.AreEqual("cat".ToQuantity(10000, "N0"), Expression.Parse($"humanizer.toQuantity('cat', 10000, 'N0')").TryEvaluate(new object()).value);
        }

        [TestMethod]
        public void TruncateTests()
        {
            var state = new
            {
                value = "This is a long string with lots of text in it."
            };

            for (int i = 10; i < 15; i++)
            {
                Assert.AreEqual(state.value.Truncate(i), Expression.Parse($"humanizer.truncate(value, {i})").TryEvaluate(state).value);
                Assert.AreEqual(state.value.Truncate(i, "..."), Expression.Parse($"humanizer.truncate(value, {i}, '...')").TryEvaluate(state).value);
                Assert.AreEqual(state.value.Truncate(i, truncator: Truncator.FixedNumberOfWords), Expression.Parse($"humanizer.truncateWords(value, {i})").TryEvaluate(state).value);
                Assert.AreEqual(state.value.Truncate(i, "...", truncator: Truncator.FixedNumberOfWords), Expression.Parse($"humanizer.truncateWords(value, {i}, '...')").TryEvaluate(state).value);
            }
        }

        [TestMethod]
        public void TupelizeTests()
        {
            for (int i = 1; i < 100; i++)
            {
                var state = new { value = i };
                Assert.AreEqual(state.value.Tupleize(), Expression.Parse($"humanizer.tupleize(value)").TryEvaluate(state).value);
            }
        }

    }
}
