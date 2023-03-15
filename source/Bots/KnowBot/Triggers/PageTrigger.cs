using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
// using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Settings;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;

namespace KnowBot.Triggers
{
    /// <summary>
    /// Functions trigger for Bot Framework messages.
    /// </summary>
    public class PageTrigger
    {
        private readonly ILogger<ActivitiesTrigger> _logger;

        public PageTrigger(
            ILogger<ActivitiesTrigger> logger)
        {
            _logger = logger;
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
        [FunctionName("webchat")]
        public async Task<HttpResponseMessage> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route ="/")] HttpRequest req)
        {
            await Task.CompletedTask;

            string html = @"<html>
<head><title>KnowBot!</title>
<style>`            -+

 
body{margin:0}
iframe {
    position: fixed;
    width: 600px;
    height:95vh;
    left: 50%;
    transform: translate(-50%, 0%); 
}
</style>

</head>
<body>
<div class='grid-container'>
  <iframe src='https://webchat.botframework.com/embed/KnowBot2?s=jZHoRVXeAB4.TcsUpOHtTGcoz3ICd2eq6lJw2fMQOK9zUwLHbt7JRlA'  style='min-width: 400px; width: 50%; min-height: 500px;'></iframe>
</div>
</body>
</html>";

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(html);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}

