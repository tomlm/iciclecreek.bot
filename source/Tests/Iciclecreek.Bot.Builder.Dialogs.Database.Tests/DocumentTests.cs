using Iciclecreek.Bot.Builder.Dialogs.Database.Tests;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Iciclecreek.Bot.Builder.Dialogs.Database.AzureStorage.DocumentDB.Tests
{
    [TestClass]
    public class DocumentTests
    {
        public static ResourceExplorer ResourceExplorer { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ResourceExplorer = new ResourceExplorer()
                .AddFolder(Path.Combine(TestUtils.GetProjectPath(), "Tests", nameof(DocumentTests)), monitorChanges: false);
        }

        [TestMethod]
        public async Task DocumentTests_TestOperations()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string>("cosmosConnectionString", "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==") })
                .Build();

            await TestUtils.RunTestScript(ResourceExplorer, configuration: config);
        }
    }
}
