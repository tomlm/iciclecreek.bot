using Iciclecreek.Bot.Builder.Dialogs.Database.Tests;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace Iciclecreek.Bot.Builder.Dialogs.Database.Table.Tests
{
    [TestClass]
    public class TableTests
    {
        public static ResourceExplorer ResourceExplorer { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ResourceExplorer = new ResourceExplorer()
                .AddFolder(Path.Combine(TestUtils.GetProjectPath(), "Tests", nameof(TableTests)), monitorChanges: false);
        }

        [TestMethod]
        public async Task TableTests_TestOperations()
        {
            await TestUtils.RunTestScript(ResourceExplorer);
        }
    }
}
