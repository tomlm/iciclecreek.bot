using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Luce;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                    Trace.TraceInformation("Loading model");
                    var x = yamlDeserializer.Deserialize(new StringReader(this.editor.Document.Text));
                    var json = yamlSerializer.Serialize(x);
                    var model = JsonConvert.DeserializeObject<LuceModel>(json, patternModelConverter);
                    engine = new LuceEngine(model);
                    this.error.Visibility = Visibility.Collapsed;
                    lucyModel = this.editor.Document.Text;
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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.editor.Options.ConvertTabsToSpaces = true;
            this.editor.Options.IndentationSize = 2;
            this.editor.ShowLineNumbers = true;

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
    - [l, large]
    - [xl, extra large, venti]

  - name: '@container'
    patterns:
    - (glass|shot|shotglass|tumbler|mug|pitcher)?

  - name: '@beverageType'
    patterns:
    - [beer, suds]
    - wine
    - [cocktail, highball]
    - whiskey
    - bourbon
    - rye
    - gin
    - vodka

  # @drink entity
  - name: '@drinkorder'
    patterns:
    - like (a|some) (@drinkSize|@container)* (of)? (___)* (@beverageType|beverage|drink)?
";
        }

        private void showInternal_Checked(object sender, RoutedEventArgs e)
        {
        }
    }
}
