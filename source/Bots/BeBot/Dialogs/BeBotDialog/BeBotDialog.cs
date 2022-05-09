using Iciclecreek.Bot.Builder.Dialogs;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Lucene.Net.Search;
using Lucy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YamlConverter;

namespace BeBot.Dialogs
{
    public partial class BeBotDialog : IcyDialog
    {
        private readonly IConfiguration _configuration;
        private readonly IndexSearcher _searcher;
        private readonly CloudQueueClient _cloudQueue;

        public BeBotDialog(IConfiguration configuration, CloudQueueClient cloudQueueClient, IndexSearcher searcher)
        {
            var yaml = new StreamReader(typeof(BeBotDialog).Assembly.GetManifestResourceStream($"{typeof(BeBotDialog).FullName}.{typeof(BeBotDialog).Name}.yaml")).ReadToEnd();
            this.Recognizer = new LucyRecognizer()
            {
                ExternalEntityRecognizer = new ChannelMentionEntityRecognizer(),
                Intents = new List<string>() { "Help", "Cancel", "Greeting", "Goodbye", "ChangeAlias", "WhereQuery", "WhoQuery", "SetPlan", "Thanks" },
                Model = YamlConvert.DeserializeObject<LucyDocument>(yaml)
            };

            this._configuration = configuration;
            this._searcher = searcher;
            this._cloudQueue = cloudQueueClient;
        }

        protected async override Task<DialogTurnResult> OnMessageActivityAsync(DialogContext dc, IMessageActivity messageActivity, CancellationToken cancellationToken)
        {
            if (!dc.State.TryGetValue("user.welcomed", out var val))
            {
                await dc.SendReplyText(BeBotDialogText.Help);
                dc.State.SetValue("user.welcomed", true);
            }

            return await base.OnMessageActivityAsync(dc, messageActivity, cancellationToken);
        }

        private static IEnumerable<DateTimexValue> GetDateEntities(RecognizerResult recognizerResult)
            => recognizerResult.GetEntities<DateTimexValue>("$..dates..values");

        private static string GetPlace(RecognizerResult recognizerResult)
        {
            var place = recognizerResult.GetEntities<string>("$..Place").FirstOrDefault();
            return NormalizePlace(place);
        }

        private static string NormalizePlace(string place)
        {
            if (place != null)
            {
                // normalize home and work
                if (place.Contains("home"))
                {
                    place = "Home";
                }

                if (place.Contains("work"))
                {
                    place = "Work";
                }
            }

            return place;
        }

        private static string VisualizeDates(IEnumerable<DateTimexValue> dates)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Dates:");
            sb.AppendLine("```");
            sb.AppendLine(YamlConvert.SerializeObject(dates));
            sb.AppendLine("```");
            return sb.ToString();
        }
    }
}
