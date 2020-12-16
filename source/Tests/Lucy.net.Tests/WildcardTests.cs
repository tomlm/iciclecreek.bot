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
                    new EntityModel() { Name = "@name", Patterns = new List<PatternModel>(){"name is (value:___)+"} },
                }
            });

            Assert.AreEqual(1, engine.WildcardEntityPatterns.Count);
            var sequence = engine.WildcardEntityPatterns.Single().PatternMatcher as SequencePatternMatcher;

            string text = "my name is joe smith";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));
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
                    new EntityModel() { Name = "@name",Patterns = new List<PatternModel>(){"name is (value:___)"} },
                }
            });

            string text = "my name is joe smith and I am cool";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            var entities = results.Where(e => e.Type == "name").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("name", entities[0].Type);
            Assert.AreEqual("joe", entities[0].Children.First().Resolution);
            Assert.AreEqual("name is joe", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void WildcardSinglePrefixTest()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@beer", Patterns = new List<PatternModel>(){"(value:___) beer"} },
                }
            });

            string text = "like coors beer";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            var entities = results.Where(e => e.Type == "beer").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("beer", entities[0].Type);
            Assert.AreEqual("coors", entities[0].Children.First().Resolution);
            Assert.AreEqual("coors beer", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void WildcardSingleMultiPrefixTest()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@desire", Patterns = new List<PatternModel>(){"like"} },
                    new EntityModel() { Name = "@beer", Patterns = new List<PatternModel>(){"(value:___)+2 beer"} },
                }
            });

            string text = "like coors beer";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            var entities = results.Where(e => e.Type == "beer").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("beer", entities[0].Type);
            Assert.AreEqual("coors", entities[0].Children.First().Resolution);
            Assert.AreEqual("coors beer", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void WildcardDoubleInlineTest()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@name",Patterns = new List<PatternModel>(){"name is (value:___) (value:___)"} },
                    new EntityModel() { Name = "@entity",Patterns = new List<PatternModel>(){"end"} },
                }
            });

            // stop on token
            string text = "my name is joe smith and stuff";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            var entities = results.Where(e => e.Type == "name").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("name", entities[0].Type);
            Assert.AreEqual("joe smith", entities[0].Children.First().Resolution);
            Assert.AreEqual("name is joe smith", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));

            // stop on entity
            text = "my name is joe smith end token";
            results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            entities = results.Where(e => e.Type == "name").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("name", entities[0].Type);
            Assert.AreEqual("joe smith", entities[0].Children.First().Resolution);
            Assert.AreEqual("name is joe smith", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void WildcardDoubleInlineOrdinalTest()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@name",Patterns = new List<PatternModel>(){"name is (value:___)+"} },
                    new EntityModel() { Name = "@entity",Patterns = new List<PatternModel>(){"end"} },
                }
            });

            // stop on token
            string text = "my name is joe smith and stuff";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            var entities = results.Where(e => e.Type == "name").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("name", entities[0].Type);
            Assert.AreEqual("joe smith and stuff", entities[0].Children.First().Resolution);
            Assert.AreEqual("name is joe smith and stuff", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));

            // stop on entity
            text = "my name is joe smith end token";
            results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            entities = results.Where(e => e.Type == "name").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("name", entities[0].Type);
            Assert.AreEqual("joe smith", entities[0].Children.First().Resolution);
            Assert.AreEqual("name is joe smith", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void WildcardPatternTest_StopsOnAnyEntity()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@name",Patterns = new List<PatternModel>(){"name is (value:___)+"} },
                    new EntityModel() { Name = "@conjunction",Patterns = new List<PatternModel>(){"(and|or)"} },
                }
            });

            string text = "my name is joe smith and I am cool";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

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
                            "a (@size)? (value:___)* (drink|cocktail|beverage)?"
                        }
                    },
                }
            });

            string text = "like a clyde mills drink.";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            var entities = results.Where(e => e.Type == "drink").ToList();
            Assert.AreEqual(1, entities.Count);
            var entity = entities.Single().Children.Single();
            Assert.AreEqual("value", entity.Type);
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
                    new EntityModel() { Name = "@size", Patterns = new List<PatternModel>() { "small" } },
                    new EntityModel() {
                        Name = "@drink",
                        Patterns = new List<PatternModel>()
                        {
                            "a (@size)? (label:___)* cocktail"
                        }
                    },
                }
            });

            string text = "a clyde mills cocktail.";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            var entities = results.Where(e => e.Type == "drink").ToList();
            Assert.AreEqual(1, entities.Count);
            var entity = entities.Single().Children.Single();
            Assert.AreEqual("label", entity.Type);
            Assert.AreEqual("clyde mills", entity.Resolution);
        }

        [TestMethod]
        public void WildcardMatcherWithEntityTests()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@test1", Patterns = new List<PatternModel>() { "test1" } },
                    new EntityModel() { Name = "@test2", Patterns = new List<PatternModel>() { "test2" } },
                    new EntityModel() { Name = "@test3", Patterns = new List<PatternModel>() {"start (@test1|@test2|label:___)* end" } },
                }
            });

            string text = "start test1 cool end";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            var entities = results.Where(e => e.Type == "test3").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("start test1 cool end", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void WildcardMatcherWithOptionalTokenTests()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@test1", Patterns = new List<PatternModel>() { "test1" } },
                    new EntityModel() { Name = "@test2", Patterns = new List<PatternModel>() { "test2" } },
                    new EntityModel() { Name = "@test3", Patterns = new List<PatternModel>() { "start (@test1|@test2|label:___)* end (label:___)* " } },
                }
            });

            string text = "start test1 cool end more stuff";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            var entities = results.Where(e => e.Type == "test3").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("test1", entities.First().Children.First().Text);
            Assert.AreEqual("cool", entities.First().Children.Skip(1).First().Text);
            Assert.AreEqual("more stuff", entities.First().Children.Skip(2).First().Text);
        }
    }
}
