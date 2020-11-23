using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa.PatternMatchers;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa.PatternMatchers.Matchers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Phonetic;
using Lucene.Net.Analysis.Phonetic.Language.Bm;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa.Tests
{
    [TestClass]
    public class PatternMatcherTests
    {
        private static CharArraySet CreateStopWords(string[] stopWords = null)
        {
            return CharArraySet.UnmodifiableSet(new CharArraySet(LuceneVersion.LUCENE_48, stopWords ?? Array.Empty<string>(), false));
        }

        public static Lazy<Analyzer> exactAnalyzer = new Lazy<Analyzer>(() => new Lucene.Net.Analysis.Standard.StandardAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48, CreateStopWords()));

        public static Lazy<Analyzer> fuzzyAnalyzer = new Lazy<Analyzer>(() =>
            Analyzer.NewAnonymous((field, textReader) =>
            {
                Tokenizer tokenizer = new StandardTokenizer(LuceneVersion.LUCENE_48, textReader);
                // TokenStream stream = new DoubleMetaphoneFilter(tokenizer, 6, false);
                var factory = new BeiderMorseFilterFactory(new Dictionary<string, string>()
                {
                    { "nameType", NameType.GENERIC.ToString()},
                    { "ruleType", RuleType.APPROX.ToString() },
                    { "languageSet", "auto"}
                });
                TokenStream stream = factory.Create(tokenizer);
                return new TokenStreamComponents(tokenizer, stream);
            })
        );

        [TestMethod]
        public void CreatesTextTokens()
        {
            var engine = new LupaEngine(exactAnalyzer.Value, fuzzyAnalyzer.Value)
            {
                IncludeInternalEntites = true
            };

            string text = "this is a test";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LupaEngine.FormatResults(text, results));

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

            entities = results.Where(e => e.Type == WildcardPatternMatcher.ENTITYTYPE).ToList();
            Assert.AreEqual(4, entities.Count);
            Assert.AreEqual("this", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
            Assert.AreEqual("is", text.Substring(entities[1].Start, entities[1].End - entities[1].Start));
            Assert.AreEqual("a", text.Substring(entities[2].Start, entities[2].End - entities[2].Start));
            Assert.AreEqual("test", text.Substring(entities[3].Start, entities[3].End - entities[3].Start));
        }

        [TestMethod]
        public void TokenPatternMatcherTests()
        {
            var engine = new LupaEngine(new LupaModel()
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
            }, exactAnalyzer.Value, fuzzyAnalyzer.Value);

            string text = "this is a test";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LupaEngine.FormatResults(text, results));

            var entities = results.Where(e => e.Type == "@test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("@test", entities[0].Type);
            Assert.AreEqual("test", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void FuzzyTokenPatternMatcherTests()
        {
            var engine = new LupaEngine(new LupaModel()
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
            },
            exactAnalyzer.Value, fuzzyAnalyzer.Value);

            string text = "this is a tesst";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n"+LupaEngine.FormatResults(text, results));

            var entities = results.Where(e => e.Type == "@test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("@test", entities[0].Type);
            Assert.AreEqual("tesst", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void PatternParser_FuzzyModifierTests()
        {
            var engine = new LupaEngine(new LupaModel()
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
            },
            exactAnalyzer.Value, fuzzyAnalyzer.Value);

            string text = "this is a tesst";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LupaEngine.FormatResults(text, results));

            var entities = results.Where(e => e.Type == "@test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("@test", entities[0].Type);
            Assert.AreEqual("tesst", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void PatternParser_OneOfModifierTests()
        {
            var engine = new LupaEngine(new LupaModel()
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
            },
            exactAnalyzer.Value, fuzzyAnalyzer.Value);

            string text = "this is a test dog frog";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LupaEngine.FormatResults(text, results));

            var entities = results.Where(e => e.Type == "@test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("@test", entities[0].Type);
            Assert.AreEqual("a test", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));

            text = "this is a nottest notdog notfrog";
            results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LupaEngine.FormatResults(text, results));

            entities = results.Where(e => e.Type == "@test").ToList();
            Assert.AreEqual(0, entities.Count);
        }

        [TestMethod]
        public void PatternParser_OneOrMoreModifierTests()
        {
            var engine = new LupaEngine(new LupaModel()
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
            },
            exactAnalyzer.Value, fuzzyAnalyzer.Value);

            string text = "this is a test dog frog";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LupaEngine.FormatResults(text, results));

            var entities = results.Where(e => e.Type == "@test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("@test", entities[0].Type);
            Assert.AreEqual("a test dog", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void PatternParser_ZeroOrMoreModifierTests()
        {
            var engine = new LupaEngine(new LupaModel()
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
            },
            exactAnalyzer.Value, fuzzyAnalyzer.Value);

            string text = "this is a frog frog frog frog";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LupaEngine.FormatResults(text, results));

            var entities = results.Where(e => e.Type == "@test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("@test", entities[0].Type);
            Assert.AreEqual("a", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));

            text = "this is a test dog frog";
            results = engine.MatchEntities(text, null);

            entities = results.Where(e => e.Type == "@test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("@test", entities[0].Type);
            Assert.AreEqual("a test dog", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void PatternParser_ZeroOrOneModifierTests()
        {
            var engine = new LupaEngine(new LupaModel()
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
            },
            exactAnalyzer.Value, fuzzyAnalyzer.Value);

            string text = "this is a frog frog frog frog";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n"+LupaEngine.FormatResults(text, results));

            var entities = results.Where(e => e.Type == "@test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("@test", entities[0].Type);
            Assert.AreEqual("a", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));

            text = "this is a test dog frog";
            results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LupaEngine.FormatResults(text, results));

            entities = results.Where(e => e.Type == "@test").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("@test", entities[0].Type);
            Assert.AreEqual("a test", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }

        [TestMethod]
        public void CanonicalValuesTest()
        {
            var engine = new LupaEngine(new LupaModel()
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
            }, exactAnalyzer.Value, fuzzyAnalyzer.Value);

            string text = "flight from seattle to dez moiynes";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LupaEngine.FormatResults(text, results));

            var entities = results.Where(e => e.Type == "@test").ToList();
            Assert.AreEqual(2, entities.Count);
            Assert.AreEqual("@test", entities[0].Type);
            Assert.AreEqual("SEA", entities[0].Resolution);
            Assert.AreEqual("seattle", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));

            Assert.AreEqual("@test", entities[1].Type);
            Assert.AreEqual("DSM", entities[1].Resolution);
            Assert.AreEqual("dez moiynes", text.Substring(entities[1].Start, entities[1].End - entities[1].Start));
        }

        [TestMethod]
        public void WildcardPatternTest()
        {
            var engine = new LupaEngine(new LupaModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel()
                    {
                        Name = "@name",
                        Patterns = new List<PatternModel>()
                        {
                            "name is ___"
                        }
                    }
                }
            }, exactAnalyzer.Value, fuzzyAnalyzer.Value);

            string text = "my name is joe smith";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LupaEngine.FormatResults(text, results));

            var entities = results.Where(e => e.Type == "@name").ToList();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("@name", entities[0].Type);
            Assert.AreEqual("joe", entities[0].Text);
            Assert.AreEqual("joe", text.Substring(entities[0].Start, entities[0].End - entities[0].Start));
        }
    }
}
