using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Iciclecreek.Bot.Builder.Adapters
{
    public class ConsoleAdapter : BotAdapter
    {
        private string conversationId;
        private bool endConversation = false;
        private ConsoleColor initialForegroundColor;

        public ConsoleAdapter()
            : base()
        {
            conversationId = Guid.NewGuid().ToString("n");
            initialForegroundColor = Console.ForegroundColor;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            this.OnTurnError = async (turnContext, exception) => Console.Error.WriteLine(exception.Message);
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        }

        /// <summary>
        /// Color to use for user input.
        /// </summary>
        public ConsoleColor UserColor { get; set; } = Console.ForegroundColor;

        /// <summary>
        /// Color to use for bot output.
        /// </summary>
        public ConsoleColor BotColor { get; set; } = ConsoleColor.Cyan;

        /// <summary>
        /// Process
        /// </summary>
        /// <param name="args"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async Task StartConversation(BotCallbackHandler callback, string text = null)
        {
            var activity = CreateUserActivity(ActivityTypes.ConversationUpdate);
            activity.MembersAdded = new List<ChannelAccount>()
            {
                activity.From,
                activity.Recipient
            };
            activity.MembersRemoved = new List<ChannelAccount>();

            await ProcessActivity(callback, activity).ConfigureAwait(false);

            if (!String.IsNullOrWhiteSpace(text))
            {
                activity = CreateUserActivity(ActivityTypes.Message, text);
                await ProcessActivity(callback, activity).ConfigureAwait(false);
            }

            while (!endConversation)
            {
                var msg = Console.ReadLine();
                if (!String.IsNullOrWhiteSpace(msg))
                {
                    activity = CreateUserActivity(ActivityTypes.Message);
                    activity.Text = msg;

                    await ProcessActivity(callback, activity).ConfigureAwait(false);
                }
            }

            Console.ForegroundColor = initialForegroundColor;
        }

        public async Task ProcessActivity(BotCallbackHandler callback, Activity activity)
        {
            Console.ForegroundColor = BotColor;
            using (var context = new TurnContext(this, activity))
            {
                await this.RunPipelineAsync(context, callback, default(CancellationToken)).ConfigureAwait(false);
            }
            Console.ForegroundColor = UserColor;
        }

        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext context, Activity[] activities, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (activities == null)
            {
                throw new ArgumentNullException(nameof(activities));
            }

            if (activities.Length == 0)
            {
                throw new ArgumentException("Expecting one or more activities, but the array was empty.", nameof(activities));
            }

            var responses = new ResourceResponse[activities.Length];

            for (var index = 0; index < activities.Length; index++)
            {
                var activity = activities[index];

                switch (activity.Type)
                {
                    case ActivityTypes.EndOfConversation:
                        this.endConversation = true;
                        break;

                    case ActivityTypes.Message:
                        {
                            IMessageActivity message = activity.AsMessageActivity();

                            // A message exchange between user and bot can contain media attachments
                            // (e.g., image, video, audio, file).  In this particular example, we are unable
                            // to create Attachments to messages, but this illustrates processing.
                            if (message.Attachments != null && message.Attachments.Any())
                            {
                                var attachment = message.Attachments.Count == 1 ? "1 attachment" : $"{message.Attachments.Count()} attachments";
                                Console.WriteLine($"\r{message.Text} with {attachment} ");
                            }
                            else
                            {
                                Console.WriteLine($"\r{message.Text}");
                            }
                        }

                        break;

                    case ActivityTypesEx.Delay:
                        {
                            // The Activity Schema doesn't have a delay type build in, so it's simulated
                            // here in the Bot. This matches the behavior in the Node connector.
                            int delayMs = (int)((Activity)activity).Value;
                            await Task.Delay(delayMs).ConfigureAwait(false);
                        }

                        break;

                    case ActivityTypes.Trace:
                        // Do not send trace activities 
                        break;

                    case ActivityTypes.Typing:
                        Console.Write("... ");
                        break;

                    default:
                        Console.WriteLine("Bot: activity type: {0}", activity.Type);
                        break;
                }

                responses[index] = new ResourceResponse(activity.Id);
            }

            return responses;
        }

        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Activity CreateUserActivity(string type, string text = null)
        {
            return new Activity()
            {
                Type = type,
                Text = text,
                ChannelId = "console",
                Locale = CultureInfo.CurrentUICulture.Name,
                From = new ChannelAccount(id: Environment.UserDomainName, name: Environment.UserName),
                Recipient = new ChannelAccount(id: Path.GetFileName(Environment.CommandLine), name: Path.GetFileName(Environment.CommandLine)),
                Conversation = new ConversationAccount(id: conversationId),
                Timestamp = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
            };
        }
    }
}
