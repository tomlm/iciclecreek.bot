using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BeBot.Dialogs;
using Iciclecreek.Bot.Builder.Dialogs;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Lucy;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using YamlConverter;

namespace BeBot
{
    public static class BeBotHelp
    {
        private static Lazy<Recognizer> _recognizer = new Lazy<Recognizer>(() =>
        {
            var recognizer = new RecognizerSet();
            recognizer.Recognizers.Add(new ChannelMentionEntityRecognizer());
            // recognizer.Recognizers.Add(new LucyRecognizer() { Model = YamlConvert.DeserializeObject<LucyDocument>(yaml) });
            return recognizer;
        });

        public static Recognizer GetSharedRecognizer()
        {
            return _recognizer.Value;
        }

        public static Task ReplyData(this DialogContext dc, string text, object data = null)
        {
            var activity = Activity.CreateMessageActivity();
            activity.Text = text;
            activity.Value = data;
            return dc.Context.SendActivityAsync(activity);
        }

        public static IEnumerable<DateTimexValue> GetDateEntities(RecognizerResult recognizerResult)
            => recognizerResult.GetEntities<DateTimexValue>("$..Dates..values");

        public static string GetPlace(RecognizerResult recognizerResult)
        {
            var place = recognizerResult.GetEntities<string>("$..Place").FirstOrDefault();
            return NormalizePlace(place);
        }

        public static string NormalizePlace(string place)
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

        public static string VisualizeDates(IEnumerable<DateTimexValue> dates)
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
