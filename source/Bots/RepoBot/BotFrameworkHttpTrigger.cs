using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Octokit;
using System;
using System.Net;
using System.Threading.Tasks;

namespace RepoBot
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
            log.LogInformation($"Messages endpoint triggered.");
            await _adapter.ProcessAsync(req, req.HttpContext.Response, _bot);
            return new StatusCodeResult(req.HttpContext.Response.StatusCode);
        }
    }
}
