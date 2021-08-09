using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Iciclecreek.Bot.Builder.Dialogs.Database.Tests
{
    [TestClass]
    public class ExecuteSqlTests
    {
        public static ResourceExplorer ResourceExplorer { get; set; }

        public TestContext TestContext { get; set; }


        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ResourceExplorer = new ResourceExplorer()
                .AddFolder(Path.Combine(TestUtils.GetProjectPath(), "Tests", nameof(ExecuteSqlTests)), monitorChanges: false);
        }

        [TestMethod]
        public async Task SqlClientTests_TestExecute()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string>("connectionString", @"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=SSPI;") })
                .Build();
            await TestUtils.RunTestScript(ResourceExplorer, configuration: config);
        }
    }
}
