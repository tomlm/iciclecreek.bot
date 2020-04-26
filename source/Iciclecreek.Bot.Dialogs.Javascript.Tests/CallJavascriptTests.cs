using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Iciclecreek.Bot.Dialogs.Javascript.Tests
{
    [TestClass]
    public class CallJavascriptTests
    {
        public static IConfiguration DefaultConfiguration { get; set; } = new ConfigurationBuilder().AddInMemoryCollection().Build();

        public static string RootFolder { get; set; } = GetProjectPath();

        public static IEnumerable<object[]> GetTestScripts(string relativeFolder)
        {
            string testFolder = Path.GetFullPath(Path.Combine(RootFolder, PathUtils.NormalizePath(relativeFolder)));
            return Directory.EnumerateFiles(testFolder, "*.test.dialog", SearchOption.AllDirectories).Select(s => new object[] { Path.GetFileName(s) }).ToArray();
        }

        public static async Task RunTestScript(ResourceExplorer resourceExplorer, string resourceId = null, [CallerMemberName] string testName = null)
        {
            var script = resourceExplorer.LoadType<TestScript>(resourceId ?? $"{testName}.test.dialog");
            script.Description = script.Description ?? resourceId;

            await script.ExecuteAsync(testName: testName, resourceExplorer: resourceExplorer).ConfigureAwait(false);
        }

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
            ResourceExplorer = new ResourceExplorer()
                .AddFolder(Path.Combine(GetProjectPath(), "Tests", nameof(CallJavascriptTests)), monitorChanges: false);

            ResourceExplorer.RegisterType<CallJavascript>(CallJavascript.Kind);
        }

        [TestMethod]
        public async Task CallJavascript_inline()
        {
            await RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task CallJavascript_custom()
        {
            await RunTestScript(ResourceExplorer);
        }

    }
}
