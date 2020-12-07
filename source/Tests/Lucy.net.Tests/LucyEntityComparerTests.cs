using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Lucy.Tests
{
    [TestClass]
    public class LucyEntityComparerTests
    {
        [TestMethod]
        public void LucyEntityComparerTests_SimpleEquals()
        {
            LucyEntity one = new LucyEntity()
            {
                Start = 10,
                End = 12,
                Type = "a",
                Text = "a",
                Resolution = "a"
            };
            LucyEntity two = new LucyEntity()
            {
                Start = 10,
                End = 12,
                Type = "a",
                Text = "a",
                Resolution = "a"
            };
            Assert.IsTrue(one.Equals(two));
            Assert.AreEqual(one.GetHashCode(), two.GetHashCode());

            one.Start = 11;
            Assert.IsFalse(one.Equals(two));
            Assert.AreNotEqual(one.GetHashCode(), two.GetHashCode());
            two.Start = 11;
            Assert.IsTrue(one.Equals(two));
            Assert.AreEqual(one.GetHashCode(), two.GetHashCode());

            one.Type = "b";
            Assert.IsFalse(one.Equals(two));
            Assert.AreNotEqual(one.GetHashCode(), two.GetHashCode());
            two.Type = "b";
            Assert.IsTrue(one.Equals(two));
            Assert.AreEqual(one.GetHashCode(), two.GetHashCode());

            one.Resolution = "b";
            Assert.IsFalse(one.Equals(two));
            Assert.AreNotEqual(one.GetHashCode(), two.GetHashCode());
            two.Resolution = "b";
            Assert.IsTrue(one.Equals(two));
            Assert.AreEqual(one.GetHashCode(), two.GetHashCode());

            one.Resolution = null;
            Assert.IsFalse(one.Equals(two));
            Assert.AreNotEqual(one.GetHashCode(), two.GetHashCode());
            two.Resolution = null;
            Assert.IsTrue(one.Equals(two));
            Assert.AreEqual(one.GetHashCode(), two.GetHashCode());

            one.Text = "b";
            Assert.IsFalse(one.Equals(two));
            Assert.AreNotEqual(one.GetHashCode(), two.GetHashCode());
            two.Text = "b";
            Assert.IsTrue(one.Equals(two));
            Assert.AreEqual(one.GetHashCode(), two.GetHashCode());
        }

        [TestMethod]
        public void LucyEntityComparerTests_LucyEntitySet()
        {
            var one = new LucyEntity()
            {
                Start = 13,
                End = 20,
                Type = "b",
                Text = "b"
            };
            var two = new LucyEntity()
            {
                Start = 15,
                End = 20,
                Type = "b",
                Text = "b"
            };
            var set = new LucyEntitySet()
            {
                JObject.FromObject(two).ToObject<LucyEntity>()
            };
            Assert.IsFalse(set.Contains(one));
            Assert.IsTrue(set.Contains(two));
        }

        [TestMethod]
        public void LucyEntityComparerTests_Children()
        {
            LucyEntity one = new LucyEntity()
            {
                Start = 10,
                End = 12,
                Type = "a",
                Text = "a",
                Resolution = "a"
            };
            LucyEntity two = new LucyEntity()
            {
                Start = 10,
                End = 12,
                Type = "a",
                Text = "a",
                Resolution = "a",
                Children = new LucyEntitySet()
                {
                    new LucyEntity()
                    {
                        Start = 15,
                        End = 20,
                        Type ="b",
                        Text = "b"
                    }
                }
            };
            Assert.IsFalse(one.Equals(two));
            Assert.AreNotEqual(one.GetHashCode(), two.GetHashCode());
            one.Children.Add(new LucyEntity()
            {
                Start = 15,
                End = 20,
                Type = "b",
                Text = "b"
            });

            Assert.IsTrue(one.Equals(two));
            Assert.AreEqual(one.GetHashCode(), two.GetHashCode());

            one.Children.Clear();
            one.Children.Add(new LucyEntity()
            {
                Start = 15,
                End = 21,
                Type = "b",
                Text = "b"
            });

            Assert.IsFalse(one.Equals(two));
            Assert.AreNotEqual(one.GetHashCode(), two.GetHashCode());
        }

    }
}
