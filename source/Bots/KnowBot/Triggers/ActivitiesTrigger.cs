using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
// using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace KnowBot.Triggers
{
    /// <summary>
    /// Functions trigger for Bot Framework messages.
    /// </summary>
    public class ActivitiesTrigger
    {
        private readonly IBot _bot;
        private IBotFrameworkHttpAdapter _adapter;
        private readonly ILogger<ActivitiesTrigger> _logger;

        public ActivitiesTrigger(
            IConfiguration configuration,
            IBotFrameworkHttpAdapter adapter,
            IBot bot,
            ILogger<ActivitiesTrigger> logger)
        {
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            _logger = logger;
            _adapter = adapter;
        }

        /// <summary>
        /// Bot Framework messages trigger handling.
        /// </summary>
        /// <param name="req">
        /// The <see cref="HttpRequest"/>.
        /// </param>
        /// <returns>
        /// The <see cref="IActionResult"/>.
        /// </returns>
        [FunctionName("messages")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "api/messages")] HttpRequest req)
        {
            // Delegate the processing of the HTTP POST to the appropriate adapter.
            // The adapter will invoke the bot.
            // IBotFrameworkHttpAdapter is expected to set the appropriate HttpResponse properties
            // (eg. Status and Body), so there is no need to return an IActionResult that will
            // set different HttpResponse values.
            await _adapter.ProcessAsync(req, req.HttpContext.Response, _bot).ConfigureAwait(false);
            return new EmptyResult();
        }
    }
}

