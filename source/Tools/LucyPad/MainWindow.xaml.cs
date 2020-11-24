using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private string lucyModel = null;
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
                if (lucyModel != this.editor.Document.Text)
                {
                    lucyModel = this.editor.Document.Text;

                    var x = yamlDeserializer.Deserialize(new StringReader(lucyModel));
                    var json = yamlSerializer.Serialize(x);
                    var model = JsonConvert.DeserializeObject<LucyModel>(json, patternModelConverter);
                    engine = new LucyEngine(model);
                }

                var text = this.query?.Text?.Trim() ?? string.Empty;
                if (text.Length > 0)
                {
                    var results = engine.MatchEntities(text);
                    this.entitiesBox.Text = LucyEngine.VisualizeResultsAsSpans(text, results);
                    this.entitiesBox.Text += "\n\n====================================\n"+LucyEngine.VizualizeResultsAsHierarchy(text, results);
                }
            }
            catch
            {
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.editor.Options.ConvertTabsToSpaces = true;
            this.editor.Options.IndentationSize = 2;

            this.editor.Text = @"# macros expand when used in a pattern
macros:
    $example: (cold|frozen|chilly)

entities:
  # @colors entity
  - name: '@colors'
    patterns: [red, green, blue, yellow, purple, white, orange]

  # @drinkSize entity
  - name: '@drinkSize'
    patterns:
    # NOTE:if patterns are an array the first value is the canonical value
    - [s, small, short]
    - [m, medium, tall]
    - [l, large, tall]
    - [xl, extra large, venti]
";
        }
    }
}
