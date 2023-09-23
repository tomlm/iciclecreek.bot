using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.QLucene;
using Lucene.Net.Store;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Extensions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Tests
{
    [TestClass]
    public class DynamicListRecognizerTests
    {
        public static ResourceExplorer ResourceExplorer { get; set; }

        public static Directory IndexDirectory { get; set; }


        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var parent = Environment.CurrentDirectory;
            while (!string.IsNullOrEmpty(parent))
            {
                if (System.IO.Directory.EnumerateFiles(parent, "*proj").Any())
                {
                    break;
                }
                else
                {
                    parent = System.IO.Path.GetDirectoryName(parent);
                }
            }

            ResourceExplorer = new ResourceExplorer();
            ResourceExplorer.AddFolder(parent, monitorChanges: false);
        }

        [TestMethod]
        public async Task TestDynamicListRecognizer()
        {
            var script = ResourceExplorer.LoadType<TestScript>("DynamicListRecognizer.test.dialog");
            await script.ExecuteAsync(ResourceExplorer);
        }
    }
}
