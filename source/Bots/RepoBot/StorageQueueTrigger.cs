using System;
using System.Threading;
using System.Threading.Tasks;
using Iciclecreek.Bot.Builder.Dialogs.Adaptive.GitHub;
using Microsoft.Azure.WebJobs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace RepoBot
{
    public class StorageQueueTrigger
    {
        private readonly BotFrameworkHttpAdapter _botAdapter;
        private readonly GitHubAdapter _githubAdapter;
        private readonly IBot _bot;
        private readonly string _botId;

        public StorageQueueTrigger(IConfiguration configuration, BotFrameworkHttpAdapter botAdapter, GitHubAdapter githubAdapter, IBot bot)
        {
            this._botId = configuration.GetValue<string>("MicrosoftAppId") ?? throw new ArgumentNullException("MicrosoftAppId");
            this._botAdapter = botAdapter ?? throw new ArgumentNullException(nameof(botAdapter)); ;
            this._githubAdapter = githubAdapter ?? throw new ArgumentNullException(nameof(githubAdapter)); ;
            this._bot = bot ?? throw new ArgumentNullException(nameof(bot));
        }

        [FunctionName("ActivitiesQueueTrigger")]
        public async Task Run([QueueTrigger("activities", Connection = "AzureWebJobsStorage")] string activityJson, ILogger log)
        {
            var activity = JsonConvert.DeserializeObject<Activity>(activityJson);
            log.LogInformation($"ContinueLaterTrigger ChannelId:{activity.ChannelId} ConversationId:{activity.Conversation.Id}");

            var conversationReference = activity.GetConversationReference();
            switch (activity.ChannelId)
            {
                case GitHubAdapter.ChannelId:
                    await this._githubAdapter.ContinueConversationAsync(_botId, activity.GetConversationReference(), this._bot.OnTurnAsync, default(CancellationToken)).ConfigureAwait(false);
                    break;
                default:
                    await this._botAdapter.ContinueConversationAsync(_botId, activity.GetConversationReference(), this._bot.OnTurnAsync, default(CancellationToken)).ConfigureAwait(false);
                    break;
            }
        }
    }
}
