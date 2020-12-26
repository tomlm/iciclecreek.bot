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
    public class PatternMatcherTests
    {
        [TestMethod]
        public void CreatesTextTokens()
        {
            var engine = new LucyEngine(new LucyModel());

            string text = "this is a test";
            var results = engine.MatchEntities(text, includeInternal: true);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            var entities = results.Where(e => e.Type == TokenPatternMatcher.ENTITYTYPE).ToList();
            Assert.AreEqual(4, entities.Count);
            Assert.AreEqual("this", entities[0].Text);
            Assert.AreEqual("is", entities[1].Text);
            Assert.AreEqual("a", entities[2].Text);
            Assert.AreEqual("test", entities[3].Text);
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
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

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
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

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
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

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
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

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
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            var entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("test", entities[0].Type);
            Assert.AreEqual("a test", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));

            text = "this is a nottest notdog notfrog";
            results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

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
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            var entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("test", entities[0].Type);
            Assert.AreEqual("a test dog", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void PatternParser_OneOrMoreModifierQuanittyTests()
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
                            "a (dog|cat|test)+1"
                        }
                    }
                }
            });

            string text = "this is a test dog frog";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            var entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("test", entities[0].Type);
            Assert.AreEqual("a test", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
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
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

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
        public void PatternParser_ZeroOrMoreModifierQuantityTests()
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
                            "a (dog|cat|test)3*"
                        }
                    }
                }
            });

            var text = "this is a dog dog dog test test test";
            var results = engine.MatchEntities(text, null);

            var entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("test", entities[0].Type);
            Assert.AreEqual("a dog dog dog", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
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
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            var entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("test", entities[0].Type);
            Assert.AreEqual("a", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));

            text = "this is a test dog frog";
            results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

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
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            var entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(2, entities.Count);
            Assert.AreEqual("test", entities[0].Type);
            Assert.AreEqual("DSM", entities[0].Resolution);
            Assert.AreEqual("dez moiynes", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));

            Assert.AreEqual("test", entities[1].Type);
            Assert.AreEqual("SEA", entities[1].Resolution);
            Assert.AreEqual("seattle", text.Substring(entities[1].Start, entities[1].End - entities[1].Start));
        }

        [TestMethod]
        public void CascadingPatternTests()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@boxsize",Patterns = new List<PatternModel>(){ "box is @twoDimensional" } },
                    new EntityModel() { Name = "@height", Patterns = new List<PatternModel>() { "(@dimension|@number) (height|tall)" } },
                    new EntityModel() { Name = "@width", Patterns = new List<PatternModel>() { "(@dimension|@number) (width|wide)" } },
                    new EntityModel() {
                        Name = "@twoDimensional",
                        Patterns = new List<PatternModel>()
                        {
                            "(@width|@dimension|@number) (x|by)? (@height|@dimension|@number)",
                            "(@height|@dimension|@number) (x|by)? (@width|@dimension|@number)",
                        }
                    },
                }
            });

            string text = "the box is 9 inches by 7.";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            var entities = results.Where(e => e.Type == "boxsize").ToList();
            Assert.AreEqual(1, entities.Count);
            var entity = entities.Single().Children.Single();
            Assert.AreEqual("twoDimensional", entity.Type);
            Assert.AreEqual(2, entity.Children.Count);
            Assert.AreEqual(1, entity.Children.Where(e => e.Type == "number").Count());
            Assert.AreEqual(1, entity.Children.Where(e => e.Type == "dimension").Count());
        }

        [TestMethod]
        public void NestedPatternTests()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@test",Patterns = new List<PatternModel>(){ "x ((p|d|q)|cool)" } },
                }
            });

            string text = "x z";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            var entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(0, entities.Count);

            text = "x q ";
            results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);
        }

        [TestMethod]
        public void NestedPatternOrdinalityTests()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@test", Patterns = new List<PatternModel>(){ "(x y)+" } },
                }
            });

            string text = "x y x y";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));
            var entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);

            text = "x x z y y";
            results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));
            entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(0, entities.Count);
        }

        [TestMethod]
        public void NestedPatternOrdinalityTests2()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@test", Patterns = new List<PatternModel>(){ "((x) (y))+" } },
                }
            });

            string text = "x y x y";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));
            var entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);

            text = "x x z y y";
            results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));
            entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(0, entities.Count);
        }


        [TestMethod]
        public void NestedPatternOrdinalityTests3()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@test", Patterns = new List<PatternModel>(){ "((x)* (y)+)+" } },
                }
            });

            string text = "x x";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));
            var entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(0, entities.Count);

            text = "y y";
            results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));
            entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);

            text = "x x z y y";
            results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));
            entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);

            text = "x x y y";
            results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));
            entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);
        }

        [TestMethod]
        public void MacroTest()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Macros = new Dictionary<string, string>()
                {
                    { "$test","(is|equals)" },
                },
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@name", Patterns = new List<PatternModel>() { "name $test ___" } },
                    new EntityModel() { Name = "@boxsize",Patterns = new List<PatternModel>(){ "box $test @twoDimensional" } },
                    new EntityModel() { Name = "@height", Patterns = new List<PatternModel>() { "(@dimension|@number) (height|tall)" } },
                    new EntityModel() { Name = "@width", Patterns = new List<PatternModel>() { "(@dimension|@number) (width|wide)" } },
                    new EntityModel() {
                        Name = "@twoDimensional",
                        Patterns = new List<PatternModel>()
                        {
                            "(@width|@dimension|@number) (x|by)? (@height|@dimension|@number)",
                            "(@height|@dimension|@number) (x|by)? (@width|@dimension|@number)",
                        }
                    },
                }
            });

            string text = "the box is 9 inches by 7.";
            var results = engine.MatchEntities(text);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            var entities = results.Where(e => e.Type == "boxsize").ToList();
            Assert.AreEqual(1, entities.Count);
            var entity = entities.Single().Children.Single();
            Assert.AreEqual("twoDimensional", entity.Type);
            Assert.AreEqual(2, entity.Children.Count);
            Assert.AreEqual(1, entity.Children.Where(e => e.Type == "number").Count());
            Assert.AreEqual(1, entity.Children.Where(e => e.Type == "dimension").Count());
        }

        [TestMethod]
        public void TokenPatternMatcherWithEntityTests()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@test1", Patterns = new List<PatternModel>() { "test1" } },
                    new EntityModel() { Name = "@test2", Patterns = new List<PatternModel>() { "test2" } },
                    new EntityModel() { Name = "@test3", Patterns = new List<PatternModel>() {"@test1 @test2" } },
                }
            });

            string text = "nomatch test1 test2 nomatch";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            var entities = results.Where(e => e.Type == "test3").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("test1 test2", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }


        [TestMethod]
        public void PatternParser_MergeContigious()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@cold", Patterns = new List<PatternModel>(){ new string[] { "cold", "ice", "freezing", "frigid"} } },
                    new EntityModel() { Name = "@test", Patterns = new List<PatternModel>(){ "I want (@cold)* beer" } }
                }
            });

            string text = "I want ice cold beer";
            var results = engine.MatchEntities(text);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            var entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("test", entities[0].Type);
            Assert.AreEqual("I want ice cold beer", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
            Assert.AreEqual("ice cold", entities[0].Children.First().Text);
        }

        [TestMethod]
        public void RegexPatternMatcher()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@test", Patterns = new List<PatternModel>(){ "/N[\\dA-Z]{5}/" } }
                }
            });

            string text = "I fly N185LM and used to fly N87405.";
            var results = engine.MatchEntities(text);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            var entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(2, entities.Count);
            Assert.AreEqual("test", entities[0].Type);
            Assert.AreEqual("N185LM", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
            Assert.AreEqual("N87405", text.Substring(entities[1].Start, entities[1].End - entities[1].Start));
        }

        [TestMethod]
        public void IgnoreTokenTests()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Locale = "en",
                Entities = new List<EntityModel>()
                {
                    new EntityModel()
                    {
                        Name = "@test",
                        Ignore = new List<string>() { "is" },
                        Patterns = new List<PatternModel>() { new PatternModel("name (value:___)") }
                    }
                }
            });

            string text = "my name is joe";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));

            var entities = results.Where(e => e.Type == "test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("test", entities[0].Type);
            Assert.AreEqual("name is joe", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
            Assert.AreEqual("joe", entities[0].Children.First().Resolution);
        }

    }
}
