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
    public class MergeTests
    {
        [TestMethod]
        public void MergeNestedChildren()
        {
            var engine = new LucyEngine(new LucyModel()
            {
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@PizzaOrder", Patterns = new List<PatternModel>(){ "(@Topping)* (@AddToppings)* " } },
                    new EntityModel() { Name = "@AddToppings", Patterns = new List<PatternModel>(){ "(@ToppingQuantifier)* (@Topping)+" } },
                    new EntityModel() { Name = "@ToppingQuantifier", Patterns = new List<PatternModel>(){ "extra" } },
                    new EntityModel() { Name = "@Topping", Patterns = new List<PatternModel>(){ "Cheese" } },
                }
            });

            string text = "extra cheese";
            var results = engine.MatchEntities(text, null);
            Trace.TraceInformation("\n" + LucyEngine.VisualEntities(text, results));
            Assert.AreEqual(1, results[0].Children.Count());
        }
    }
}
