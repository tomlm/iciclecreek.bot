using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data.Common;
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

        [Ignore]
        [TestMethod]
        public async Task SqlClientTests_TestExecute()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string>("connectionString", "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==") })
                .Build();
            await TestUtils.RunTestScript(ResourceExplorer, configuration: config);
        }
    }
}
