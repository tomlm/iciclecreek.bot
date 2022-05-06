using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace BeBot
{
    public static class Extensions
    {
        public static Task ReplyData(this DialogContext dc, string text, object data = null)
        {
            var activity = Activity.CreateMessageActivity();
            activity.Text = text;
            activity.Value = data;
            return dc.Context.SendActivityAsync(activity);
        }
        public static JToken RemoveFields(this JToken token, params string[] fields)
        {
            JContainer container = token as JContainer;
            if (container == null) return token;

            List<JToken> removeList = new List<JToken>();
            foreach (JToken el in container.Children())
            {
                JProperty p = el as JProperty;
                if (p != null && fields.Contains(p.Name))
                {
                    removeList.Add(el);
                }
                el.RemoveFields(fields);
            }

            foreach (JToken el in removeList)
            {
                el.Remove();
            }

            return token;
        }
    }
}
