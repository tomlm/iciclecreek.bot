using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LucyBot
{
    public class BotFrameworkHttpTrigger
    {
        private readonly BotFrameworkHttpAdapter _adapter;
        private readonly IBot _bot;

        public BotFrameworkHttpTrigger(BotFrameworkHttpAdapter adapter, IBot bot)
        {
            this._adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            this._bot = bot ?? throw new ArgumentNullException(nameof(bot));
        }

        [FunctionName("messages")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var json = await new StreamReader(req.Body).ReadToEndAsync();
            var activity = JsonConvert.DeserializeObject<Activity>(json);
            log.LogInformation($"Messages endpoint triggered [{activity.Type}]");
            string auth = req.Headers.ContainsKey("Authorization") ? req.Headers["Authorization"].ToString() : null;
            var result = await _adapter.ProcessActivityAsync(auth, activity, _bot.OnTurnAsync, default(CancellationToken));
            return new OkObjectResult(result);
        }
    }
}
