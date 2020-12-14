//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace Lucy.Tests
//{
//    [TestClass]
//    public class ExamplesTests
//    {
//        [TestMethod]
//        public void GenerateTokenSequence()
//        {
//            var engine = new LucyEngine(new LucyModel()
//            {
//                Entities = new List<EntityModel>()
//                {
//                    new EntityModel() { Name = "@test", Patterns = new List<PatternModel>() { "x y z" } },
//                }
//            });

//            var result = engine.GenerateExamples("test").ToList();
//            Assert.AreEqual(1, result.Count);
//            Assert.AreEqual("x y z", result.First());
//        }

//        [TestMethod]
//        public void GenerateTokenSequenceOneOf()
//        {
//            var engine = new LucyEngine(new LucyModel()
//            {
//                Entities = new List<EntityModel>()
//                {
//                    new EntityModel() { Name = "@test", Patterns = new List<PatternModel>() { "x (p|d|q) z" } },
//                }
//            });

//            var result = engine.GenerateExamples("test").ToList();
//            Assert.AreEqual(3, result.Count);
//            Assert.AreEqual("x p z", result[0]);
//            Assert.AreEqual("x d z", result[1]);
//            Assert.AreEqual("x q z", result[2]);
//        }

//        [TestMethod]
//        public void GenerateTokenSequenceZeroOrOne()
//        {
//            var engine = new LucyEngine(new LucyModel()
//            {
//                Entities = new List<EntityModel>()
//                {
//                    new EntityModel() { Name = "@test", Patterns = new List<PatternModel>() { "x (p|d|q)? z" } },
//                }
//            });

//            var result = engine.GenerateExamples("test").ToList();
//            Assert.AreEqual(4, result.Count);
//            Assert.IsTrue(result.Contains("x z"));
//            Assert.IsTrue(result.Contains("x p z"));
//            Assert.IsTrue(result.Contains("x d z"));
//            Assert.IsTrue(result.Contains("x q z"));
//        }

//        [TestMethod]
//        public void GenerateTokenSequenceZeroOrMore()
//        {
//            var engine = new LucyEngine(new LucyModel()
//            {
//                Entities = new List<EntityModel>()
//                {
//                    new EntityModel() { Name = "@test", Patterns = new List<PatternModel>() { "x (p|d|q)* z" } },
//                }
//            });

//            var result = engine.GenerateExamples("test").ToList();
//            Assert.IsTrue(result.Count > 1);
//            Assert.IsTrue(result.Contains("x z"));
//            Assert.IsTrue(result.Any(r => r.Contains("p") || r.Contains("d") || r.Contains("q")));
//        }

//        [TestMethod]
//        public void GenerateTokenSequenceOneOrMore()
//        {
//            var engine = new LucyEngine(new LucyModel()
//            {
//                Entities = new List<EntityModel>()
//                {
//                    new EntityModel() { Name = "@test", Patterns = new List<PatternModel>() { "x (p|d|q)+ z" } },
//                }
//            });

//            var result = engine.GenerateExamples("test").ToList();
//            Assert.IsTrue(result.Count >= 1);
//            Assert.IsTrue(!result.Contains("x z"));
//            Assert.IsTrue(result.Any(r => r.Contains("p") || r.Contains("d") || r.Contains("q")));
//        }

//        [TestMethod]
//        public void GenerateEntityMatch()
//        {
//            var engine = new LucyEngine(new LucyModel()
//            {
//                Entities = new List<EntityModel>()
//                {
//                    new EntityModel() { Name = "@test", Patterns = new List<PatternModel>() { "(p|d|q)" } },
//                    new EntityModel() { Name = "@test2", Patterns = new List<PatternModel>() { "x @test z" } },
//                }
//            });

//            var result = engine.GenerateExamples("test2").ToList();
//            Assert.AreEqual(3, result.Count);
//            Assert.IsTrue(result.Contains("x p z"));
//            Assert.IsTrue(result.Contains("x d z"));
//            Assert.IsTrue(result.Contains("x q z"));
//        }

//        //[TestMethod]
//        //public void GenerateEntityMatchNested()
//        //{
//        //    var engine = new LucyEngine(new LucyModel()
//        //    {
//        //        Entities = new List<EntityModel>()
//        //        {
//        //            new EntityModel() { Name = "@test", Patterns = new List<PatternModel>() { "(p|d|q)" } },
//        //            new EntityModel() { Name = "@test2", Patterns = new List<PatternModel>() { "x @test z" } },
//        //            new EntityModel() { Name = "@test3", Patterns = new List<PatternModel>() { "foo (@test2)*" } },
//        //        }
//        //    });

//        //    var result = engine.GenerateExample("test3");
//        //    Assert.IsTrue(result == "foo" || result == "foo x p z" || result == "foo x d z" || result == "foo x q z");
//        //}

//        [TestMethod]
//        public void GenerateWildcard()
//        {
//            var engine = new LucyEngine(new LucyModel()
//            {
//                Entities = new List<EntityModel>()
//                {
//                    new EntityModel() { Name = "@test", Patterns = new List<PatternModel>() { "___" } },
//                }
//            });

//            var result = engine.GenerateExamples("test").ToList();
//            Assert.AreEqual(1, result.Count);
//            Assert.IsTrue(result[0].Length > 0);
//        }

//    }
//}
