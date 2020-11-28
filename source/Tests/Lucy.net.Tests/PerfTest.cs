using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lucy.Tests
{
    [TestClass]
    public class PerfTests
    {
        [TestMethod]
        public void RunPerfTests()
        {
            StringBuilder sb = new StringBuilder();
            var model = new LucyModel()
            {
                Macros = new Dictionary<string, string>()
                {
                    { "$name", "(name|nom de plum|handle)" },
                    { "$is", "(is|equals)?" }
                },
                Entities = new List<EntityModel>()
                {
                    new EntityModel() { Name = "@name", Patterns = new List<PatternModel>() { "$name $is ___" } },
                    new EntityModel() { Name = "@boxsize",Patterns = new List<PatternModel>(){ "box $is @twoDimensions" } },
                    new EntityModel() { Name = "@height", Patterns = new List<PatternModel>() { "(@dimension|@number) (height|tall)" } },
                    new EntityModel() { Name = "@width", Patterns = new List<PatternModel>() { "(@dimension|@number) (width|wide)" } },
                    new EntityModel() {
                        Name = "@twoDimensions",
                        Patterns = new List<PatternModel>()
                        {
                            "(@width|@dimension|@number) (x|by)? (@height|@dimension|@number)",
                            "(@height|@dimension|@number) (x|by)? (@width|@dimension|@number)",
                        }
                    },
                }
            };

            Stopwatch sw = new Stopwatch();
            sw.Start();
            var engine = new LucyEngine(model);
            sw.Stop();
            sb.AppendLine($"loading: {sw.Elapsed}");
            sw.Reset();

            string text = "the box is 9 inches by 7.";

            sw.Restart();
            var results = engine.MatchEntities(text);
            sw.Stop();
            sb.AppendLine($"single cold match: {sw.Elapsed}");
            sw.Reset();

            sw.Restart();
            results = engine.MatchEntities(text, null);
            sw.Stop();
            sb.AppendLine($"single warm match: {sw.Elapsed}");
            sw.Reset();

            sw.Restart();
            var count = 1000;
            for (int i = 0; i < count; i++)
            {
                results = engine.MatchEntities(text);
            }
            sw.Stop();
            sb.AppendLine($"{count} utterances: {sw.Elapsed} {(float)sw.ElapsedMilliseconds/count} ms per match ");
            sw.Reset();
            Trace.TraceInformation(sb.ToString());
            //File.WriteAllText(@"c:\scratch\timing.txt", sb.ToString());
        }
    }
}
