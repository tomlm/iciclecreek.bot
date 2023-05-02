using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Xunit;

namespace Iciclecreek.Bot.Builder.Dialogs.Tests
{
    public class ExtensionTests
    {
        [Fact]
        public void CreateReply()
        {
            Activity activity = new Activity(ActivityTypes.Message)
            {
                Id = "1234",
                Text = "test",
                ChannelId = "test",
                Conversation = new ConversationAccount(id: "convo1"),
                From = new ChannelAccount(id: "user1"),
                Recipient = new ChannelAccount(id: "bot1"),
                ServiceUrl = "https://test.com",
                ChannelData = new Dictionary<string, object>()
                {
                    { "foo", "bar" }
                },
                Entities = new List<Entity>()
                {
                    new Entity()
                    {
                        Type = "test",
                        Properties = new Newtonsoft.Json.Linq.JObject()
                        {
                            { "foo", "bar" }
                        }
                    }
                },
                Locale = "en-us",
                ReplyToId = "1234",
                Timestamp = System.DateTimeOffset.UtcNow,
                Value = new Dictionary<string, object>()
                {
                    { "foo", "bar" }
                }
            };
            IActivity act = activity as IActivity;
            var reply1 = activity.CreateReply("test");
            var reply2 = act.CreateReply("test");
            reply1.Timestamp = reply2.Timestamp;
            var json1 = JsonConvert.SerializeObject(reply1);
            var json2 = JsonConvert.SerializeObject(reply2);
            Assert.Equal(json1, json2);
        }
    }
}
