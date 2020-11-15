using Iciclecreek.Bot.Builder.Dialogs.Adaptive.GitHub;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RepoBot
{
    public class GitHubTrigger
    {
        private readonly GitHubAdapter _adapter;
        private readonly IBot _bot;

        public GitHubTrigger(GitHubAdapter adapter, IBot bot)
        {
            this._adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            this._bot = bot ?? throw new ArgumentNullException(nameof(bot));
        }

        [FunctionName("github")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"GitHubWebHook endpoint triggered.");
            var body = await req.ReadAsStringAsync();
            var signature = req.Headers["X-Hub-Signature-256"].FirstOrDefault();
            var response = await _adapter.ProcessWebhookPayloadAsync(signature, body, _bot.OnTurnAsync, default(CancellationToken)).ConfigureAwait(false);
            return new OkResult();
        }
    }
}
