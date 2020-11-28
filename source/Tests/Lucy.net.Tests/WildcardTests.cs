using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lucy.PatternMatchers;
using Lucy.PatternMatchers.Matchers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lucy.Tests
{
    [TestClass]
    public class WildcardMatcherTests
    {
        [TestMethod]
        public void FallbackParserTest()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@name", Patterns = new List<PatternModel>(){"name is (___)+"} },
                }
            });

            Assert.AreEqual(1, engine.WildcardEntityPatterns.Count);
            var sequence = engine.WildcardEntityPatterns.Single().PatternMatcher as SequencePatternMatcher;
            Assert.IsNotNull(sequence.PatternMatchers.Last() as MultiWildcardPatternMatcher);

            string text = "my name is joe smith";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));
            Trace.TraceInformation("\n" + LucyEngine.VizualizeResultsAsHierarchy(text, results));
            var entities = results.Where(e => e.Type == "name").ToList();
            Assert.AreEqual("name", entities[0].Type);
            Assert.AreEqual("joe smith", entities[0].Children.First().Resolution);
            Assert.AreEqual("name is joe smith", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void WildcardSingleTest()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@name",Patterns = new List<PatternModel>(){"name is ___"} },
                }
            });

            string text = "my name is joe smith and I am cool";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));
            Trace.TraceInformation("\n" + LucyEngine.VizualizeResultsAsHierarchy(text, results));

            var entities = results.Where(e => e.Type == "name").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("name", entities[0].Type);
            Assert.AreEqual("joe", entities[0].Children.First().Resolution);
            Assert.AreEqual("name is joe", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void WildcardDoubleInlineTest()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@name",Patterns = new List<PatternModel>(){"name is ___ ___"} },
                    new EntityModel() { Name = "@entity",Patterns = new List<PatternModel>(){"end"} },
                }
            });

            // stop on token
            string text = "my name is joe smith and stuff";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));
            Trace.TraceInformation("\n" + LucyEngine.VizualizeResultsAsHierarchy(text, results));

            var entities = results.Where(e => e.Type == "name").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("name", entities[0].Type);
            Assert.AreEqual("joe smith", entities[0].Children[0].Resolution);
            Assert.AreEqual("name is joe smith", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));

            // stop on entity
            text = "my name is joe smith end token";
            results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));
            Trace.TraceInformation("\n" + LucyEngine.VizualizeResultsAsHierarchy(text, results));

            entities = results.Where(e => e.Type == "name").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("name", entities[0].Type);
            Assert.AreEqual("joe smith", entities[0].Children[0].Resolution);
            Assert.AreEqual("name is joe smith", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void WildcardDoubleInlineOrdinalTest()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@name",Patterns = new List<PatternModel>(){"name is (___)+ @entity"} },
                    new EntityModel() { Name = "@entity",Patterns = new List<PatternModel>(){"end"} },
                }
            });

            // stop on token
            string text = "my name is joe smith and stuff";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));
            Trace.TraceInformation("\n" + LucyEngine.VizualizeResultsAsHierarchy(text, results));

            var entities = results.Where(e => e.Type == "name").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("name", entities[0].Type);
            Assert.AreEqual("joe smith and stuff", entities[0].Children[0].Resolution);
            Assert.AreEqual("name is joe smith and stuff", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));

            // stop on entity
            text = "my name is joe smith end token";
            results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));
            Trace.TraceInformation("\n" + LucyEngine.VizualizeResultsAsHierarchy(text, results));

            entities = results.Where(e => e.Type == "name").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("name", entities[0].Type);
            Assert.AreEqual("joe smith", entities[0].Children[0].Resolution);
            Assert.AreEqual("name is joe smith", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
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
            Assert.AreEqual("joe smith", entities[0].Children.First().Resolution);
            Assert.AreEqual("name is joe smith", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
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
                            "a (@size)? (label:___)* (drink|cocktail|beverage)?"
                        }
                    },
                }
            });

            string text = "I would like a clyde mills cocktail.";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualizeResultsAsSpans(text, results));
            Trace.TraceInformation("\n" + LucyEngine.VizualizeResultsAsHierarchy(text, results));

            var entities = results.Where(e => e.Type == "drink").ToList();
            Assert.AreEqual(1, entities.Count);
            var entity = entities.Single().Children.Single();
            Assert.AreEqual("label", entity.Type);
            Assert.AreEqual("clyde mills", entity.Resolution);
        }

    }
}
