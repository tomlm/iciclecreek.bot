using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LucyBot
{
    public class BotFrameworkHttpTrigger
    {
        private readonly BotFrameworkHttpAdapter _adapter;
        private readonly IBot _bot;
        private readonly string _internalMessageEndpoint;

        public BotFrameworkHttpTrigger(BotFrameworkHttpAdapter adapter, IBot bot, IConfiguration config)
        {
            this._adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            this._bot = bot ?? throw new ArgumentNullException(nameof(bot));
            var hostname = config.GetValue<string>("WEBSITE_HOSTNAME");
            var protocol = hostname.StartsWith("localhost") ? "http" : "https";
            _internalMessageEndpoint = $"{protocol}://{hostname}/api/internalMessages";
        }

        [FunctionName("messages")]
        public async Task<IActionResult> Messages(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
        ILogger log)
        {
            var json = await new StreamReader(req.Body).ReadToEndAsync();
            var activity = JsonConvert.DeserializeObject<Activity>(json);
            string authHeader = req.Headers.ContainsKey("Authorization") ? req.Headers["Authorization"].ToString() : null;
            if (activity.DeliveryMode == "expectReplies" || activity.Type == ActivityTypes.Invoke)
            {
                log.LogInformation($"Messages endpoint triggered [{activity.Type}]");
                var result = await _adapter.ProcessActivityAsync(authHeader, activity, _bot.OnTurnAsync, default(CancellationToken));
                return new ObjectResult(result.Body) { StatusCode = result.Status };
            }
            else
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                HttpClient client = new HttpClient();
                if (authHeader != null)
                    client.DefaultRequestHeaders.Add("Authorization", authHeader);
                client.PostAsJsonAsync(_internalMessageEndpoint, activity)
                    .ContinueWith(t => log.LogError(t.Exception.Message), TaskContinuationOptions.OnlyOnFaulted);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                return new AcceptedResult();
            }
        }

        [FunctionName("internalMessages")]
        public async Task<IActionResult> InternalMessages(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var json = await new StreamReader(req.Body).ReadToEndAsync();
            var activity = JsonConvert.DeserializeObject<Activity>(json);
            log.LogInformation($"InternalMessages endpoint triggered [{activity.Type}]");
            string authHeader = req.Headers.ContainsKey("Authorization") ? req.Headers["Authorization"].ToString() : null;
            await _adapter.ProcessActivityAsync(authHeader, activity, _bot.OnTurnAsync, default(CancellationToken));
            return new OkResult();
        }
    }
}
