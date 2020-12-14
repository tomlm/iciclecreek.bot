using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Lucy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LucyPad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LucyEngine engine = null;
        private LucyRecognizer recognizer = null;
        private string lucyModel = null;
        private JsonConverter patternModelConverter = new PatternModelConverter();

        private IDeserializer yamlDeserializer = new DeserializerBuilder()
                                                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                                    .Build();
        private ISerializer yamlToJsonSerializer = new SerializerBuilder()
                                                .JsonCompatible()
                                                .Build();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void query_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowResults();
        }

        private void ShowResults()
        {
            try
            {
                if (lucyModel != this.editor.Document.Text)
                {
                    LoadModel();
                }

                var text = this.query?.Text?.Trim() ?? string.Empty;
                if (text.Length > 0)
                {
                    var sw = new System.Diagnostics.Stopwatch();
                    sw.Start();
                    var results = engine.MatchEntities(text, includeInternal: this.showInternal.IsChecked.Value);
                    sw.Stop();

                    this.tabs.SelectedIndex = 2;
                    this.labelBox.Text = $"{sw.ElapsedMilliseconds} ms\n" + String.Join("\n", results.Select(s => LucyEngine.VisualizeEntity(text, s, showSpans: true, showHierarchy: false)));
                    this.entitiesBox.Text = String.Join("\n", results.Select(s => LucyEngine.VisualizeEntity(text, s, showSpans: false, showHierarchy: true)));

                    //var activity = new Activity(ActivityTypes.Message) { Text = text };
                    //var tc = new TurnContext(new TestAdapter(), activity);
                    //var dc = new DialogContext(new DialogSet(), tc, new DialogState());
                    //var recognizerResult = recognizer.RecognizeAsync(dc, activity).Result;
                    //// this.recognizerBox.Text = JsonConvert.SerializeObject(recognizerResult, new JsonSerializerSettings() { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore });
                    //this.recognizerBox.Text = new Serializer().Serialize(JObject.FromObject(recognizerResult).ToObject<ExpandoObject>());
                }
            }
            catch (SemanticErrorException err)
            {
                this.error.Content = err.Message;
                this.error.Visibility = Visibility.Visible;
                this.editor.ScrollToLine(err.Start.Line);
                var line = this.editor.Document.GetLineByNumber(err.Start.Line - 1);
                this.editor.Select(line.Offset, line.Length);
            }
            catch (SyntaxErrorException err)
            {
                this.error.Content = err.Message;
                this.error.Visibility = Visibility.Visible;
                this.editor.ScrollToLine(err.Start.Line);
                var line = this.editor.Document.GetLineByNumber(err.Start.Line - 1);
                this.editor.Select(line.Offset, line.Length);
            }
            catch (Exception err)
            {
                this.error.Content = err.Message;
                this.error.Visibility = Visibility.Visible;
            }
        }

        private void LoadModel()
        {
            // Trace.TraceInformation("Loading model");
            var x = yamlDeserializer.Deserialize(new StringReader(this.editor.Document.Text));
            var json = yamlToJsonSerializer.Serialize(x);
            var model = JsonConvert.DeserializeObject<LucyModel>(json, patternModelConverter);
            engine = new LucyEngine(model, useAllBuiltIns: true);
            recognizer = new LucyRecognizer()
            {
                Model = model,
            };

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 100; i++)
            {
                sb.AppendLine(engine.GenerateExample("desireIntent"));
            }
            this.examplesBox.Text = sb.ToString();

            if (engine.Warnings.Any())
            {
                this.error.Content = String.Join("\n", engine.Warnings);
                this.error.Visibility = Visibility.Visible;
            }
            else
            {
                this.error.Visibility = Visibility.Collapsed;
            }
            lucyModel = this.editor.Document.Text;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.editor.Options.ConvertTabsToSpaces = true;
            this.editor.Options.IndentationSize = 2;
            this.editor.ShowLineNumbers = true;

            this.editor.Text = LoadResource("LucyPad.lucy.yaml");
            LoadModel();
        }

        private void showInternal_Checked(object sender, RoutedEventArgs e)
        {
        }

        private string LoadResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(name))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }

        }

        private void CommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            ShowResults();
        }
    }
}
