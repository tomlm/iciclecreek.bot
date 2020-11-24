using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy.PatternMatchers;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy.PatternMatchers.Matchers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy.Tests
{
    [TestClass]
    public class PatternMatcherTests
    {
        [TestMethod]
        public void CreatesTextTokens()
        {
            var engine = new LucyEngine(new LucyModel());

            string text = "this is a test";
            var results = engine.MatchEntities(text, includeInternal: true);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));

            var entities = results.Where(e => e.Type == TokenPatternMatcher.ENTITYTYPE).ToList();
            Assert.AreEqual(4, entities.Count);
            Assert.AreEqual("this", entities[0].Text);
            Assert.AreEqual("is", entities[1].Text);
            Assert.AreEqual("a", entities[2].Text);
            Assert.AreEqual("test", entities[3].Text);

            entities = results.Where(e => e.Type == FuzzyTokenPatternMatcher.ENTITYTYPE).ToList();
            Assert.AreEqual(4, entities.Count);
            Assert.AreEqual("this", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
            Assert.AreEqual("is", text.Substring(entities[1].Start, entities[1].End - entities[1].Start));
            Assert.AreEqual("a", text.Substring(entities[2].Start, entities[2].End - entities[2].Start));
            Assert.AreEqual("test", text.Substring(entities[3].Start, entities[3].End - entities[3].Start));
        }

        [TestMethod]
        public void TokenPatternMatcherTests()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel()
                    {
                        Name = "@test",
                        Patterns = new List<PatternModel>()
                        {
                            new PatternModel("test")
                        }
                    }
                }
            });

            string text = "this is a test";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));

            var entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("test", entities[0].Type);
            Assert.AreEqual("test", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void TokenPatternMatcherTestsNoAt()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel()
                    {
                        Name = "test",
                        Patterns = new List<PatternModel>()
                        {
                            new PatternModel("test")
                        }
                    }
                }
            });

            string text = "this is a test";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));

            var entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("test", entities[0].Type);
            Assert.AreEqual("test", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void FuzzyTokenPatternMatcherTests()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel()
                    {
                        Name = "@test",
                        FuzzyMatch = true,
                        Patterns = new List<PatternModel>()
                        {
                            "test"
                        }
                    }
                }
            });

            string text = "this is a tesst";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));

            var entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("test", entities[0].Type);
            Assert.AreEqual("tesst", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void PatternParser_FuzzyModifierTests()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel()
                    {
                        Name = "@test",
                        Patterns = new List<PatternModel>()
                        {
                            "(test)~"
                        }
                    }
                }
            });

            string text = "this is a tesst";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));

            var entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("test", entities[0].Type);
            Assert.AreEqual("tesst", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void PatternParser_OneOfModifierTests()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel()
                    {
                        Name = "@test",
                        Patterns = new List<PatternModel>()
                        {
                            "a (dog|cat|test)"
                        }
                    }
                }
            });

            string text = "this is a test dog frog";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));

            var entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("test", entities[0].Type);
            Assert.AreEqual("a test", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));

            text = "this is a nottest notdog notfrog";
            results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));

            entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(0, entities.Count);
        }

        [TestMethod]
        public void PatternParser_OneOrMoreModifierTests()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel()
                    {
                        Name = "@test",
                        Patterns = new List<PatternModel>()
                        {
                            "a (dog|cat|test)+"
                        }
                    }
                }
            });

            string text = "this is a test dog frog";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));

            var entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("test", entities[0].Type);
            Assert.AreEqual("a test dog", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void PatternParser_ZeroOrMoreModifierTests()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel()
                    {
                        Name = "@test",
                        Patterns = new List<PatternModel>()
                        {
                            "a (dog|cat|test)*"
                        }
                    }
                }
            });

            string text = "this is a frog frog frog frog";
            var results = engine.MatchEntities(text, includeInternal: true);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));

            var entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("test", entities[0].Type);
            Assert.AreEqual("a", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));

            text = "this is a test dog frog";
            results = engine.MatchEntities(text, null);

            entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("test", entities[0].Type);
            Assert.AreEqual("a test dog", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void PatternParser_ZeroOrOneModifierTests()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel()
                    {
                        Name = "@test",
                        Patterns = new List<PatternModel>()
                        {
                            "a (dog|cat|test)?"
                        }
                    }
                }
            });

            string text = "this is a frog frog frog frog";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));

            var entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("test", entities[0].Type);
            Assert.AreEqual("a", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));

            text = "this is a test dog frog";
            results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));

            entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("test", entities[0].Type);
            Assert.AreEqual("a test", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void CanonicalValuesTest()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel()
                    {
                        Name = "@test",
                        Patterns = new List<PatternModel>()
                        {
                            new string[] { "LAX", "los angeles" },
                            new string[] { "DSM", "(des moines)~" },
                            new string[] { "SEA", "seattle" },
                            new string[] { "OHR", "o'hare", "ohare" },
                            new string[] { "MID", "midway"},
                        }
                    }
                }
            });

            string text = "flight from seattle to dez moiynes";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));

            var entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(2, entities.Count);
            Assert.AreEqual("test", entities[0].Type);
            Assert.AreEqual("SEA", entities[0].Resolution);
            Assert.AreEqual("seattle", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));

            Assert.AreEqual("test", entities[1].Type);
            Assert.AreEqual("DSM", entities[1].Resolution);
            Assert.AreEqual("dez moiynes", text.Substring(entities[1].Start, entities[1].End - entities[1].Start));
        }

        [TestMethod]
        public void NestedPatternTests()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@boxsize",Patterns = new List<PatternModel>(){ "box (is|equals)? @dimensions"} },
                    new EntityModel() { Name = "@height", Patterns = new List<PatternModel>() { "(@length) (height|tall)" } },
                    new EntityModel() { Name = "@width", Patterns = new List<PatternModel>() { "(@length) (width|wide)" } },
                    new EntityModel() { Name = "@length", Patterns = new List<PatternModel>() { "@number @units" } },
                    new EntityModel() { Name = "@number", Patterns = new List<PatternModel>() { "(0|1|2|3|4|5|6|7|8|9|10)" } },
                    new EntityModel() { Name = "@units", Patterns = new List<PatternModel>() { "(inches|feet|yards|meters)" } },
                    new EntityModel() {
                        Name = "@dimensions",
                        Patterns = new List<PatternModel>()
                        {
                            "(@width|@length|@number) (x|by)? (@height|@length|@number)",
                            "(@height|@length|@number) (x|by)? (@width|@length|@number)",
                        }
                    },
                }
            });

            string text = "the box is 9 inches by 7.";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));
            Trace.TraceInformation("\n" + LucyEngine.VizualizeResultsAsHierarchy(text, results));

            var entities = results.Where(e => e.Type == "boxsize").ToList();
            Assert.AreEqual(1, entities.Count);
            var entity = entities.Single().Children.Single();
            Assert.AreEqual("dimensions", entity.Type);
            Assert.AreEqual(2, entity.Children.Count);
            Assert.AreEqual(1, entity.Children.Where(e => e.Type == "number").Count());
            Assert.AreEqual(1, entity.Children.Where(e => e.Type == "length").Count());
        }

        [TestMethod]
        public void FallbackParserTest()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@name", Patterns = new List<PatternModel>(){"name is ___"} },
                }
            });

            Assert.AreEqual(0, engine.EntityPatterns.Count);
            Assert.AreEqual(1, engine.WildcardEntityPatterns.Count);
            var sequence = engine.WildcardEntityPatterns.Single().PatternMatcher as SequencePatternMatcher;
            Assert.IsNotNull(sequence.PatternMatchers.Last() as FallbackPatternMatcher);

            string text = "my name is joe smith";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));
            Trace.TraceInformation("\n" + LucyEngine.VizualizeResultsAsHierarchy(text, results));
        }

        [TestMethod]
        public void WildcardPatternTest()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@name",Patterns = new List<PatternModel>(){"name is ___"} },
                    new EntityModel() { Name = "@conjunction",Patterns = new List<PatternModel>(){"(and|or)"} },
                }
            });

            string text = "my name is joe smith and I am cool";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));
            Trace.TraceInformation("\n" + LucyEngine.VizualizeResultsAsHierarchy(text, results));

            var entities = results.Where(e => e.Type == "name").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("name", entities[0].Type);
            Assert.AreEqual("joe", entities[0].Resolution);
            Assert.AreEqual("name is joe", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void WildcardPatternTest_MultipleTokens()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@name",Patterns = new List<PatternModel>(){"name is (___)+"} },
                    new EntityModel() { Name = "@conjunction",Patterns = new List<PatternModel>(){"(and|or)"} },
                }
            });

            string text = "my name is joe smith and but and I am cool";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));
            Trace.TraceInformation("\n" + LucyEngine.VizualizeResultsAsHierarchy(text, results));

            var entities = results.Where(e => e.Type == "name").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("name", entities[0].Type);
            Assert.AreEqual("joe smith", entities[0].Resolution);
            Assert.AreEqual("name is joe smith", entities[0].Text);
            Assert.AreEqual("name is joe smith", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void MacroTest()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Macros = new Dictionary<string, string>()
                {
                    { "$is","(is|equals)" },
                },
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@boxsize",Patterns = new List<PatternModel>(){ "box $is @dimensions"} },
                    new EntityModel() { Name = "@height", Patterns = new List<PatternModel>() { "(@length) (height|tall)" } },
                    new EntityModel() { Name = "@width", Patterns = new List<PatternModel>() { "(@length) (width|wide)" } },
                    new EntityModel() { Name = "@length", Patterns = new List<PatternModel>() { "@number @units" } },
                    new EntityModel() { Name = "@number", Patterns = new List<PatternModel>() { "(0|1|2|3|4|5|6|7|8|9|10)" } },
                    new EntityModel() { Name = "@units", Patterns = new List<PatternModel>() { "(inches|feet|yards|meters)" } },
                    new EntityModel() {
                        Name = "@dimensions",
                        Patterns = new List<PatternModel>()
                        {
                            "(@width|@length|@number) (x|by)? (@height|@length|@number)",
                            "(@height|@length|@number) (x|by)? (@width|@length|@number)",
                        }
                    },
                }
            });

            string text = "the box is 9 inches by 7.";
            var results = engine.MatchEntities(text, null, true);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));
            Trace.TraceInformation("\n" + LucyEngine.VizualizeResultsAsHierarchy(text, results));

            var entities = results.Where(e => e.Type == "boxsize").ToList();
            Assert.AreEqual(1, entities.Count);
            var entity = entities.Single().Children.Single();
            Assert.AreEqual("dimensions", entity.Type);
            Assert.AreEqual(2, entity.Children.Count);
            Assert.AreEqual(1, entity.Children.Where(e => e.Type == "number").Count());
            Assert.AreEqual(1, entity.Children.Where(e => e.Type == "length").Count());
        }

        [TestMethod]
        public void WildcardOrdinalTests()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Macros = new Dictionary<string, string>()
                {
                    { "$is","(is|equals)" },
                },
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@size", Patterns = new List<PatternModel>() { "(small|medium|large)" } },
                    new EntityModel() {
                        Name = "@drink",
                        Patterns = new List<PatternModel>()
                        {
                            "a (@size)? (___)* (drink|cocktail|beverage)?"
                        }
                    },
                }
            });

            string text = "I would like a clyde mills drink.";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));
            Trace.TraceInformation("\n" + LucyEngine.VizualizeResultsAsHierarchy(text, results));

            var entities = results.Where(e => e.Type == "drink").ToList();
            Assert.AreEqual(1, entities.Count);
            var entity = entities.Single().Children.Single();
            Assert.AreEqual("wildcard", entity.Type);
            Assert.AreEqual("clyde mills", entity.Resolution);
        }

        [TestMethod]
        public void WildcardNamedTests()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Macros = new Dictionary<string, string>()
                {
                    { "$is","(is|equals)" },
                },
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@size", Patterns = new List<PatternModel>() { "(small|medium|large)" } },
                    new EntityModel() {
                        Name = "@drink",
                        Patterns = new List<PatternModel>()
                        {
                            "a (@size)? (foo:___)* (drink|cocktail|beverage)?"
                        }
                    },
                }
            });

            string text = "I would like a clyde mills drink.";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));
            Trace.TraceInformation("\n" + LucyEngine.VizualizeResultsAsHierarchy(text, results));

            var entities = results.Where(e => e.Type == "drink").ToList();
            Assert.AreEqual(1, entities.Count);
            var entity = entities.Single().Children.Single();
            Assert.AreEqual("foo", entity.Type);
            Assert.AreEqual("clyde mills", entity.Resolution);
        }

    }
}
