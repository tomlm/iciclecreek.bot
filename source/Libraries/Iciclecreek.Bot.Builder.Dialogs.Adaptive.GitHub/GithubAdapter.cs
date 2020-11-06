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
        private string botId;
        private string botName;

        public GitHubAdapter(IConfiguration configuration, string botId, string botName)
        {
            this.botId = botId;
            this.botName = botName;
            this.config = configuration;
            this.Client = new GitHubClient(new ProductHeaderValue(nameof(GitHubAdapter), "1.0"));
        }

        /// <summary>
        /// GitHubClient for making github API calls on behalf of your github app (bot).
        /// </summary>
        public GitHubClient Client { get; set; }

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
            var secret = config.GetValue<string>("github:secret");
            if (secret != null)
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

                    var payloadHash = $"sha256='{BitConverter.ToString(hashBytes).Replace(" -", "")}'";
                    if (payloadHash != signature)
                    {
                        throw new AuthorizationException(HttpStatusCode.Unauthorized, null);
                    }
                }
            }

            dynamic payload = JsonConvert.DeserializeObject(body);

            // payload.signature = String.Join(",", ((IEnumerable<JProperty>)payload.Properties()).Select(p => p.Name).OrderBy(p => p));

            var activity = (Schema.Activity)Schema.Activity.CreateEventActivity();
            activity.ChannelId = "github";
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
                Id = this.botId,
                Name = this.botName
            };

            activity.Value = payload;
            activity.Name = "GitHub";

            return this.ProcessActivityAsync(identity, activity, callback, cancellationToken);
        }

        public override async Task<InvokeResponse> ProcessActivityAsync(ClaimsIdentity claimsIdentity, Schema.Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            BotAssert.ActivityNotNull(activity);

            using (var context = new TurnContext(this, activity))
            {
                context.TurnState.Add<IIdentity>(BotIdentityKey, claimsIdentity);

                // The OAuthScope is also stored on the TurnState to get the correct AppCredentials if fetching a token is required.
                //var scope = SkillValidation.IsSkillClaim(claimsIdentity.Claims) ? JwtTokenValidation.GetAppIdFromClaims(claimsIdentity.Claims) : GetBotFrameworkOAuthScope();
                //context.TurnState.Add(OAuthScopeKey, scope);
                context.TurnState.Add(this.Client);
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

        public object UseState(MemoryStorage memoryStorage)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Github API doesn't support DeleteActivity semantics, use ((GithubAdapter)turncontext.Adapter).Client to make API calls.");
        }

        public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Schema.Activity[] activities, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Github API doesn't support SendActivity semantics, use ((GithubAdapter)turncontext.Adapter).Client to make API calls.");
        }

        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Schema.Activity activity, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Github API doesn't support UpdateActivity semantics, use ((GithubAdapter)turncontext.Adapter).Client to make API calls.");
        }
    }
}
