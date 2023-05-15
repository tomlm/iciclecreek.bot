using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Iciclecreek.Bot.Builder.Dialogs
{

    /// <summary>
    /// A alternative Bot implementation which hosts a code first dialog without complexity of adaptive infrastructure (lg, skills, declarative)
    /// </summary>
    /// <remarks>
    /// This is a bot uses Dependency Injection to resolve:
    /// * MemoryScopes/User/ConversationState
    /// * PathResolvers
    /// * Dialogs
    /// The first dialog found in the DI container is used as the root dialog.
    /// </remarks>
    public class IcyBot : ComponentDialog, IBot
    {
        private readonly DialogStateManagerConfiguration _dialogStateManagerConfiguration;
        private readonly ConversationState _conversationState;
        private readonly UserState _userState;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveDialogBot"/> class.
        /// </summary>
        /// <param name="conversationState">A dependency injected <see cref="ConversationState"/> implementation.</param>
        /// <param name="userState">A dependency injected <see cref="UserState"/> implementation.</param>
        /// <param name="dialogs">dialogs</param>
        /// <param name="pathResolvers">dependcy injected path resolvers (AddBotRuntime())</param>
        /// <param name="memoryScopes">dependency injected memory scopes (AddBotRuntime())</param>
        /// <param name="serviceProvider">servicePRovider</param>
        /// <param name="logger">An <see cref="ILogger"/> instance.</param>
        public IcyBot(
            ConversationState conversationState,
            UserState userState,
            IEnumerable<Dialog> dialogs = null,
            IEnumerable<IPathResolver> pathResolvers = null,
            IEnumerable<MemoryScope> memoryScopes = null,
            IServiceProvider serviceProvider = null,
            ILogger logger = null)
        {
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            _userState = userState ?? throw new ArgumentNullException(nameof(userState));
            _serviceProvider = serviceProvider;
            _logger = logger ?? NullLogger<IcyBot>.Instance;
            _dialogStateManagerConfiguration = new DialogStateManagerConfiguration();

            var services = new ServiceCollection()
                .AddIcyBot()
                .BuildServiceProvider();

            if (pathResolvers == null || !pathResolvers.Any())
            {
                pathResolvers = services.GetServices<IPathResolver>();
            }

            if (memoryScopes == null || !memoryScopes.Any())
            {
                memoryScopes = services.GetServices<MemoryScope>();
            }

            foreach (var pathResolver in pathResolvers)
            {
                _dialogStateManagerConfiguration.PathResolvers.Add(pathResolver);
            }

            foreach (var memoryScope in memoryScopes)
            {
                _dialogStateManagerConfiguration.MemoryScopes.Add(memoryScope);
            }

            if (dialogs != null)
            {
                foreach (var dialog in dialogs)
                {
                    AddDialog(dialog);
                }
            }
        }

        /// <summary>
        /// Helper to add typed dialog
        /// </summary>
        /// <typeparam name="T">dialog type (Id is the name of the dialog)</typeparam>
        public void AddDialog<T>()
            where T : Dialog
        {
            this.AddDialog(ActivatorUtilities.CreateInstance<T>(_serviceProvider));
        }

        /// <inheritdoc/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"IBot.OnTurn");

            turnContext.TurnState.Add(_serviceProvider);
            turnContext.TurnState.Add(_conversationState);
            turnContext.TurnState.Add(_userState);
            turnContext.TurnState.Add(_dialogStateManagerConfiguration);
            turnContext.TurnState.Set<BotCallbackHandler>(OnTurnAsync);

            // Run the Root Dialog
            await this.RunAsync(turnContext, turnContext.TurnState.Get<ConversationState>().CreateProperty<DialogState>("DialogState"), cancellationToken);

            // Save any updates that have been made
            await Task.WhenAll(_conversationState.SaveChangesAsync(turnContext, false, cancellationToken),
                               _userState.SaveChangesAsync(turnContext, false, cancellationToken));
        }
    }
}
