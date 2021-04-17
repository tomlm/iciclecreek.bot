using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Iciclecreek.Bot.Builder.Adapters;
using System.Linq;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Microsoft.Bot.Builder.Dialogs.Declarative.Obsolete;

namespace Iciclecreek.Bot.Builder.Adapters.Tests
{
    [TestClass]
    public class EmbeddedResourceProviderTests
    {
        public static ResourceExplorer ResourceExplorer { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ComponentRegistration.Add(new DeclarativeComponentRegistration());
            ComponentRegistration.Add(new DialogsComponentRegistration());
            ComponentRegistration.Add(new AdaptiveComponentRegistration());
            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());
            ComponentRegistration.Add(new DeclarativeComponentRegistrationBridge<LucyBotComponent>());

            ResourceExplorer = new ResourceExplorer()
                .AddFolder(@"..\..\..\TestBot");
            ResourceExplorer.AddResourceType("yaml");
        }

        [TestMethod]
        public async Task EmbeddedResource_Tests()
        {
            var resource = ResourceExplorer.GetResource("foo.en-us.lg");
            Assert.IsNotNull(resource);

            var contents = await resource.ReadTextAsync();
            Assert.IsTrue(contents.Contains("Hi"));

            var lgResources = ResourceExplorer.GetResources("lg").ToList<Resource>();
            Assert.IsTrue(lgResources.Any(r => r.Id == "foo.en-us.lg"));
            Assert.IsTrue(lgResources.Any(r => r.Id == "common.en-us.lg"));

            lgResources = ResourceExplorer.GetResources("dialog").ToList<Resource>();
            Assert.IsTrue(lgResources.Any(r => r.Id == "Test.dialog"));

            lgResources = ResourceExplorer.GetResources("yaml").ToList<Resource>();
            Assert.IsTrue(lgResources.Any(r => r.Id == "foo.en.lucy.yaml"));
        }
    }
}
