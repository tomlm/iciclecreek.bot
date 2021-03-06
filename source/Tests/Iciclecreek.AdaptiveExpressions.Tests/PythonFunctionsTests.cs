using AdaptiveExpressions;
using IronPython.Runtime.Types;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Iciclecreek.AdaptiveExpressions.Tests
{
    [TestClass]
    public class PythonFunctionsTests
    {
        public static IConfiguration DefaultConfiguration { get; set; } = new ConfigurationBuilder().AddInMemoryCollection().Build();

        public static string RootFolder { get; set; } = GetProjectPath();

        public static string GetProjectPath()
        {
            var parent = Environment.CurrentDirectory;
            while (!string.IsNullOrEmpty(parent))
            {
                if (Directory.EnumerateFiles(parent, "*proj").Any())
                {
                    break;
                }
                else
                {
                    parent = Path.GetDirectoryName(parent);
                }
            }

            return parent;
        }

        public static ResourceExplorer ResourceExplorer { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ResourceExplorer = new ResourceExplorer();
            ResourceExplorer.LoadProject(Path.Combine(GetProjectPath(), "Iciclecreek.AdaptiveExpressions.Tests.csproj"));
            PythonFunctions.AddPythonFunctions(ResourceExplorer);
        }

        [TestMethod]
        public void CallFunctions()
        {
            var (result, error) = Expression.Parse("pyTest.snakeYears(10)").TryEvaluate(new object());
            Assert.AreEqual(210, Convert.ToInt32(result));

            var memory = new
            {
                user = new { name = "joe", age = 52 }
            };
            (result, error) = Expression.Parse("pyTest.dogYears(user)").TryEvaluate(memory);
            Assert.AreEqual(52 * 7, Convert.ToInt32(result));

            (result, error) = Expression.Parse("pyTest.snakeYears(user.age)").TryEvaluate(memory);
            Assert.AreEqual(52 * 21, Convert.ToInt32(result));
        }

        [TestMethod]
        public void TestReturnObject()
        {
            var (result, error) = Expression.Parse("pyTest.returnObject()").TryEvaluate(new object());
            dynamic r = result;
            Assert.AreEqual(13, (int)r.num);
            Assert.AreEqual("string", (string)r.str);
            Assert.AreEqual(3, (int)r.complex.x);
            Assert.AreEqual("y", (string)r.complex.y);
        }

        [TestMethod]
        public void TestReturnArray()
        {
            var (result, error) = Expression.Parse("pyTest.returnArray()").TryEvaluate(new object());
            dynamic r = result;
            Assert.AreEqual("x", (string)r[0]);
            Assert.AreEqual("y", (string)r[1]);
            Assert.AreEqual("z", (string)r[2]);
        }

        [TestMethod]
        public void TestEvaluate()
        {
            var (result, error) = Expression.Parse("pyTest.tryEval()").TryEvaluate(new object());
            Assert.AreEqual(15 + 35, Convert.ToInt32(result));
        }
    }
}
