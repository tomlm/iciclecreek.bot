using Microsoft.Azure.WebJobs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Project1
{
    public class StorageQueueTrigger
    {
        private readonly CloudAdapter _botAdapter;
        private readonly IBot _bot;
        private readonly string _botId;

        public StorageQueueTrigger(IConfiguration configuration, CloudAdapter botAdapter, IBot bot)
        {
            this._botId = configuration.GetValue<string>("MicrosoftAppId") ?? throw new ArgumentNullException("MicrosoftAppId");
            this._botAdapter = botAdapter ?? throw new ArgumentNullException(nameof(botAdapter)); ;
            this._bot = bot ?? throw new ArgumentNullException(nameof(bot));
        }

        [FunctionName("ActivitiesQueueTrigger")]
        public async Task Run([QueueTrigger("activities", Connection = "AzureWebJobsStorage")] string activityJson, ILogger log)
        {
            var activity = JsonConvert.DeserializeObject<Activity>(activityJson);
            log.LogInformation($"ContinueLaterTrigger ChannelId:{activity.ChannelId} ConversationId:{activity.Conversation.Id}");

            // continue on correct adapter for the channelId.
            var conversationReference = activity.GetConversationReference();
            switch (activity.ChannelId)
            {
                default:
                    await this._botAdapter.ContinueConversationAsync(_botId, activity.GetConversationReference(), this._bot.OnTurnAsync, default(CancellationToken)).ConfigureAwait(false);
                    break;
            }
        }
    }
}
