using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Schema = Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Octokit;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Bot.Schema;
using System.Security.Claims;
using System.Security.Principal;
using System.Linq;

namespace Iciclecreek.Bot.Builder.Dialogs.Adaptive.GitHub
{
    /// <summary>
    /// BotAdapter for Github webhook events.
    /// </summary>
    /// <remarks>
    /// This adapter does authentication on Github webhook POST objects and
    /// turns them into EventActivity
    /// {
    ///     "channelId":"github",
    ///      "type":"event",
    ///      "name":"Github",
    ///      "value": payload
    ///      "from":{ user.id and name of user that triggered webhook"}
    ///      "recipient" : { .id and name of the github app }
    ///      "conversation": {.id and name of the repo }
    /// }
    /// There is no direct outbound activity, to respond you make github api calls directly
    /// via turnContext.Services&lt;GitHubClient&gt;().
    /// or via
    ///  ((GithubAdapter)turncontext.Adapter).Client...
    /// </remarks>
    public class GitHubAdapter : BotAdapter
    {
        private IConfiguration config;

        public const string ChannelId = "github";

        public GitHubAdapter(IConfiguration configuration)
        {
            this.config = configuration;
        }

        /// <summary>
        /// handle a webhook call back and 
        /// </summary>
        /// <param name="signature">X-Hub-Signature-256 value.</param>
        /// <param name="body">webhook body.</param>
        /// <param name="callback">bot logic.</param>
        /// <param name="cancellationToken">cancelationtoken.</param>
        /// <returns>invokeResponse.</returns>
        public virtual Task<InvokeResponse> ProcessWebhookPayloadAsync(string signature, string body, BotCallbackHandler callback, CancellationToken cancellationToken = default)
        {
            ClaimsIdentity identity = new ClaimsIdentity();
            var secret = config.GetValue<string>("GitHub:WebhookSecret");
            if (!String.IsNullOrEmpty(secret))
            {
                using (var sha = SHA256.Create())
                {
                    var encoding = new UTF8Encoding();

                    Byte[] keyBytes = encoding.GetBytes(secret);
                    Byte[] textBytes = encoding.GetBytes(body);
                    Byte[] hashBytes;
                    using (HMACSHA256 hash = new HMACSHA256(keyBytes))
                    {
                        hashBytes = hash.ComputeHash(textBytes);
                    }

                    var payloadHash = $"sha256={BitConverter.ToString(hashBytes).Replace("-", "")}";
                    if (String.Compare(payloadHash, signature, ignoreCase:true) != 0)
                    {
                        throw new AuthorizationException(HttpStatusCode.Unauthorized, null);
                    }
                }
            }

            dynamic payload = JsonConvert.DeserializeObject(body);

            var activity = (Schema.Activity)Schema.Activity.CreateEventActivity();
            activity.Name = "GitHub";
            activity.ChannelId = ChannelId;
            activity.Conversation = new Schema.ConversationAccount()
            {
                Id = payload.repository?.id ?? "unknown",
                Name = payload.repository?.name ?? "unknown"
            };
            activity.From = new Schema.ChannelAccount()
            {
                Id = payload.sender?.id ?? payload.organization?.id ?? "unknown",
                Name = payload.sender?.login ?? payload.organization?.login ?? "unknown",
                Role = payload.sender?.type ?? "user"
            };
            activity.Recipient = new Schema.ChannelAccount()
            {
                Id = this.config.GetValue<string>("MicrosoftAppId") ?? "unknown",
                Name = this.config.GetValue<string>("BotId") ?? this.config.GetValue<string>("MicrosoftAppId") ?? "unknown"
            };

            activity.Locale = "en-us";
            activity.Value = payload;

            // System.Diagnostics.Debug.WriteLine(JsonConvert.SerializeObject(activity, new JsonSerializerSettings() { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore }));

            return this.ProcessActivityAsync(identity, activity, callback, cancellationToken);
        }

        public override async Task<InvokeResponse> ProcessActivityAsync(ClaimsIdentity claimsIdentity, Schema.Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            BotAssert.ActivityNotNull(activity);

            using (var context = new TurnContext(this, activity))
            {
                context.TurnState.Add<IIdentity>(BotIdentityKey, claimsIdentity);
                context.TurnState.Add(this.config);
                context.TurnState.Add(callback);

                await RunPipelineAsync(context, callback, cancellationToken).ConfigureAwait(false);

                // Handle ExpectedReplies scenarios where the all the activities have been buffered and sent back at once 
                // in an invoke response.
                if (context.Activity.DeliveryMode == DeliveryModes.ExpectReplies)
                {
                    return new InvokeResponse { Status = (int)HttpStatusCode.OK, Body = new ExpectedReplies(context.BufferedReplyActivities) };
                }

                // Handle Invoke scenarios, which deviate from the request/request model in that
                // the Bot will return a specific body and return code.
                if (activity.Type == ActivityTypes.Invoke)
                {
                    return new InvokeResponse { Status = (int)HttpStatusCode.NotImplemented };
                }

                // For all non-invoke scenarios, the HTTP layers above don't have to mess
                // with the Body and return codes.
                return null;
            }
        }

        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Github API doesn't support DeleteActivity semantics, use ((GithubAdapter)turncontext.Adapter).Client to make API calls.");
        }

        public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Schema.Activity[] activities, CancellationToken cancellationToken)
        {
            List<ResourceResponse> responses = new List<ResourceResponse>();
            foreach (var activity in activities)
            {
                System.Diagnostics.Debug.WriteLine($"SEND [{activity.Type}] {activity.Text}");
                responses.Add(new ResourceResponse());
            }
            return Task.FromResult(responses.ToArray());
        }

        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Schema.Activity activity, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Github API doesn't support UpdateActivity semantics, use ((GithubAdapter)turncontext.Adapter).Client to make API calls.");
        }
    }
}
