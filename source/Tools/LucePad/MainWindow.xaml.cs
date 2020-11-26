using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Luce;
using Microsoft.MarkedNet;
using Newtonsoft.Json;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LucePad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LuceEngine engine = null;
        private string luceModel = null;
        private JsonConverter patternModelConverter = new PatternModelConverter();

        private IDeserializer yamlDeserializer = new DeserializerBuilder()
                                                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                                    .Build();
        private ISerializer yamlSerializer = new SerializerBuilder()
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
            try
            {
                if (luceModel != this.editor.Document.Text)
                {
                    LoadModel();
                }

                var text = this.query?.Text?.Trim() ?? string.Empty;
                if (text.Length > 0)
                {
                    var results = engine.MatchEntities(text, includeInternal: this.showInternal.IsChecked.Value);

                    this.labelBox.Text = LuceEngine.VisualizeResultsAsSpans(text, results);
                    this.entitiesBox.Text = LuceEngine.VizualizeResultsAsHierarchy(text, results);
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
            Trace.TraceInformation("Loading model");
            var x = yamlDeserializer.Deserialize(new StringReader(this.editor.Document.Text));
            var json = yamlSerializer.Serialize(x);
            var model = JsonConvert.DeserializeObject<LuceModel>(json, patternModelConverter);
            engine = new LuceEngine(model);
            engine.UseAllBuiltEntities();
            if (engine.Warnings.Any())
            {
                this.error.Content = String.Join("\n", engine.Warnings);
                this.error.Visibility = Visibility.Visible;
            }
            else
            {
                this.error.Visibility = Visibility.Collapsed;
            }
            luceModel = this.editor.Document.Text;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.editor.Options.ConvertTabsToSpaces = true;
            this.editor.Options.IndentationSize = 2;
            this.editor.ShowLineNumbers = true;

            this.editor.Text = LoadResource("LucePad.luce.yaml");
            LoadModel();

            //this.overviewViewer.NavigateToString(LoadMarkdown("LucePad.overview.md"));
            //this.helpViewer.NavigateToString(LoadMarkdown("LucePad.help.md"));
        }

        private void showInternal_Checked(object sender, RoutedEventArgs e)
        {
        }

        private string LoadMarkdown(string name)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<html>");
            sb.AppendLine("<body style=\"font-family:Calibri\">");
            sb.AppendLine(new Marked().Parse(LoadResource(name)));
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            return sb.ToString();
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
    }
}
