using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions;
using Iciclecreek.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace BeBot
{
    public static class Extensions
    {
        private static Random _rnd = new Random();

        public static Task ReplyText(this DialogContext dc, params string[] variations)
        {
            var index = _rnd.Next(0, variations.Length);
            var expression = Expression.Parse($"`{variations[index]}`");
            var (result, error) = expression.TryEvaluate<string>(dc.State);
            return dc.SendActivityAsync(result);
        }

        public static Task ReplyData(this DialogContext dc, string text, object data = null)
        {
            var activity = Activity.CreateMessageActivity();
            activity.Text = text;
            activity.Value = data;
            return dc.SendActivityAsync(activity);
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
