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
using System.Linq;
using Lucene.Net.Store;
using System.Diagnostics;

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
                Console.WriteLine("QLuBuild folder [--prebuild]");
                Console.WriteLine("--prebuild - prebuild cached catalog in {qnaFile}.catalog folder.");
                return;
            }
            Echo = false;

            var folder = args[0];
            var prebuild = args.Where(a => a == "--prebuild").Any();

            var multiLanguageRecognizers = new Dictionary<string, JObject>();
            foreach (var file in System.IO.Directory.EnumerateFiles(folder, "*.qna", SearchOption.AllDirectories))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Console.Write($"Parsing {file}...");
                var jsonPath = $"{file}.json";

                // convert qna => json file.
                var source = File.ReadAllText(file);
                var hash = ComputeSHA256Hash(source);
                dynamic contents = null;

                // figure out if the source file is different then the one which was used to create the .json file
                if (File.Exists(jsonPath))
                {
                    try
                    {
                        contents = JsonConvert.DeserializeObject(File.ReadAllText(jsonPath));
                        if ((string)contents.hash == hash)
                        {
                            Console.WriteLine($"(no change)");
                            // we can skip to next file.
                            continue;
                        }
                    }
                    catch (Exception err)
                    {
                    }
                }

                if (File.ReadAllText(file).Length == 0)
                {
                    File.WriteAllText(file, "\n\n\n");
                }

                await Cmd($"bf qnamaker:convert --in={file} --out={jsonPath} --force").Execute(false);

                if (File.Exists(jsonPath))
                {
                    var json = File.ReadAllText(jsonPath);
                    contents = JsonConvert.DeserializeObject(json);
                    contents.hash = hash;
                    File.WriteAllText(jsonPath, JsonConvert.SerializeObject(contents, Formatting.Indented));
                    sw.Stop();
                    Console.WriteLine(sw.Elapsed);

                    if (prebuild)
                    {
                        // build cached catalog
                        sw.Restart();
                        var catalogPath = $"{file}.catalog";
                        Console.Write($"Creating {catalogPath}...");
                        var catalogDirInfo = new DirectoryInfo(catalogPath);
                        if (catalogDirInfo.Exists)
                        {
                            foreach (var catalogFile in catalogDirInfo.EnumerateFiles())
                            {
                                catalogFile.Delete();
                            }
                        }
                        else
                        {
                            catalogDirInfo.Create();
                        }

                        QLuceneEngine.CreateCatalog(json, FSDirectory.Open(catalogPath));
                        File.Delete(jsonPath);
                        File.Delete(Path.Combine(Path.GetDirectoryName(jsonPath), "alterations_" + Path.GetFileName(jsonPath)));
                        sw.Stop();
                        Console.WriteLine(sw.Elapsed);
                    }
                }

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
                recognizer.knowledgeBase = Path.GetFileName(file);
                File.WriteAllText(languageRecognizerPath, JsonConvert.SerializeObject(recognizer, Formatting.Indented));
                Console.WriteLine(languageRecognizerPath);

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
