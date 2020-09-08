using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CShellNet;
using Medallion.Shell;
using Newtonsoft.Json.Linq;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.QLucene;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;

namespace QLuBuild
{
    partial class Program
    {
        static async Task Main(string[] args)
        {
            await new Script().Main(args);
        }
    }

    class Script : CShell
    {
        public async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("QLuBuild folder");
                return;
            }

            var folder = args[0];

            Console.WriteLine("Building qna.json files...");
            var multiLanguageRecognizers = new Dictionary<string, JObject>();
            foreach (var file in Directory.EnumerateFiles(folder, "*.qna", SearchOption.AllDirectories))
            {
                // bf qnamaker:convert --in=MSMeeting-Facts.en-us.qna --out=MSMeeting-Facts.en-us.qna.json --force
                var jsonPath = $"{file}.json";
                await Cmd($"bf qnamaker:convert --in={file} --out={jsonPath} --force").Execute();

                var dir = Path.GetDirectoryName(file);
                var rootName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file));
                var language = Path.GetExtension(Path.GetFileNameWithoutExtension(file)).Trim('.');
                if (String.IsNullOrEmpty(language))
                {
                    language = "en-us";
                }
                var multiRecognizerPath = Path.Combine(dir, $"{rootName}.qna.dialog");
                JObject recognizers;
                if (!multiLanguageRecognizers.TryGetValue(multiRecognizerPath, out recognizers))
                {
                    recognizers = new JObject();
                    multiLanguageRecognizers.Add(multiRecognizerPath, recognizers);
                }
                var languageRecognizerPath = Path.Combine(dir, $"{file}.dialog");
                recognizers[language] = Path.GetFileNameWithoutExtension(languageRecognizerPath);

                dynamic recognizer = new JObject();
                recognizer["$kind"] = QLuceneRecognizer.Kind;
                recognizer.id = Path.GetFileName(file);
                recognizer.resourceId = Path.GetFileName(jsonPath);
                Console.WriteLine(languageRecognizerPath);
                File.WriteAllText(languageRecognizerPath, JsonConvert.SerializeObject(recognizer, Formatting.Indented));
            }

            foreach (var kv in multiLanguageRecognizers)
            {
                dynamic multiRecognizer = new JObject();
                multiRecognizer["$kind"] = MultiLanguageRecognizer.Kind;
                multiRecognizer.recognizers = kv.Value;
                Console.WriteLine(kv.Key);
                File.WriteAllText(kv.Key, JsonConvert.SerializeObject(multiRecognizer, Formatting.Indented));
            }

            Console.WriteLine("Done");
        }
    }
}
