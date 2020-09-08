using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CShellNet;
using Newtonsoft.Json.Linq;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.QLucene;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using System.Security.Cryptography;
using System.Text;

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
            Echo = false;

            var folder = args[0];

            var multiLanguageRecognizers = new Dictionary<string, JObject>();
            foreach (var file in Directory.EnumerateFiles(folder, "*.qna", SearchOption.AllDirectories))
            {
                Console.Write($"{file}...");
                // bf qnamaker:convert --in=MSMeeting-Facts.en-us.qna --out=MSMeeting-Facts.en-us.qna.json --force
                var jsonPath = $"{file}.json";

                // convert qna => json file.
                var source = File.ReadAllText(file);
                var hash = ComputeSHA256Hash(source);
                dynamic contents = null;

                // figure out if the source file is different then the one which was used to create the .json file
                if (File.Exists(jsonPath))
                {
                    contents = JsonConvert.DeserializeObject(File.ReadAllText(jsonPath));
                    if ((string)contents.hash == hash)
                    {
                        Console.WriteLine($"(no change)");
                        // we can skip to next file.
                        continue;
                    }
                }

                await Cmd($"bf qnamaker:convert --in={file} --out={jsonPath} --force").Execute(false);

                contents = JsonConvert.DeserializeObject(File.ReadAllText(jsonPath));
                contents.hash = hash;
                File.WriteAllText(jsonPath, JsonConvert.SerializeObject(contents, Formatting.Indented));

                // Write file.{lang}.qna.dialog
                var dir = Path.GetDirectoryName(file);
                var rootName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file));
                var language = Path.GetExtension(Path.GetFileNameWithoutExtension(file)).Trim('.');
                if (String.IsNullOrEmpty(language))
                {
                    language = "en-us";
                }

                var languageRecognizerPath = $"{file}.dialog";
                dynamic recognizer = new JObject();
                recognizer["$kind"] = QLuceneRecognizer.Kind;
                recognizer.id = Path.GetFileName(file);
                recognizer.resourceId = Path.GetFileName(jsonPath);
                File.WriteAllText(languageRecognizerPath, JsonConvert.SerializeObject(recognizer, Formatting.Indented));
                Console.WriteLine($"done");

                // update multi-language recognzier bookeeping.
                var multiRecognizerPath = Path.Combine(dir, $"{rootName}.qna.dialog");
                JObject recognizers;
                if (!multiLanguageRecognizers.TryGetValue(multiRecognizerPath, out recognizers))
                {
                    recognizers = new JObject();
                    multiLanguageRecognizers.Add(multiRecognizerPath, recognizers);
                }
                recognizers[language] = Path.GetFileNameWithoutExtension(languageRecognizerPath);
            }

            // Write multilanguage recognizer dialog file.qna.dialog
            foreach (var kv in multiLanguageRecognizers)
            {
                dynamic multiRecognizer = new JObject();
                multiRecognizer["$kind"] = MultiLanguageRecognizer.Kind;
                multiRecognizer.recognizers = kv.Value;
                Console.WriteLine(kv.Key);
                File.WriteAllText(kv.Key, JsonConvert.SerializeObject(multiRecognizer, Formatting.Indented));
            }
        }

        public static string ComputeSHA256Hash(string text)
        {
            using (var sha256 = new SHA256Managed())
            {
                return BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "");
            }
        }
    }
}
