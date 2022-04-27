using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Iciclecreek.Bot.Builder.Dialogs
{
    public static class Extensions
    {
        private static DialogTurnResult waiting = new DialogTurnResult(DialogTurnStatus.Waiting);

        /// <summary>
        /// Registers IcyBot and UserState, ConversationState, Memory, PathResolvers 
        /// </summary>
        /// <remarks>
        /// You still need to register IStorage, Dialogs
        /// </remarks>
        /// <param name="services"></param>
        public static IServiceCollection AddBot(this IServiceCollection services)
        {
            services.TryAddSingleton<UserState>();
            services.TryAddSingleton<ConversationState>();
            new DialogsBotComponent().ConfigureServices(services, new ConfigurationBuilder().Build());
            services.TryAddSingleton<IBot, IcyBot>();
            return services;
        }

        /// <summary>
        /// Registers bot and UserState, ConversationState, Memory, PathResolvers 
        /// </summary>
        /// <remarks>
        /// You still need to register IStorage
        /// </remarks>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static IServiceCollection AddBot<BotT>(this IServiceCollection services)
            where BotT : IcyBot
        {
            services.TryAddSingleton<UserState>();
            services.TryAddSingleton<ConversationState>();
            new DialogsBotComponent().ConfigureServices(services, new ConfigurationBuilder().Build());
            services.TryAddSingleton<IBot, BotT>();
            return services;
        }

        /// <summary>
        /// Get saved options
        /// </summary>
        /// <typeparam name="T">Options type</typeparam>
        /// <param name="dc">dc</param>
        /// <returns>instance of optionsT</returns>
        public static T GetOptions<T>(this DialogContext dc)
        {
            if (ObjectPath.TryGetPathValue<T>(dc.State, "this.options", out var result))
            {
                return result;
            }
            return default(T);
        }

        /// <summary>
        /// Save options object into "this" memory scope
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="options"></param>
        public static void SaveOptions(this DialogContext dc, object options)
        {
            ObjectPath.SetPathValue(dc.State, "this.options", options);
        }

        /// <summary>
        /// Helper to Return signal that dialog is waiting for input from user.
        /// </summary>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static Task<DialogTurnResult> WaitForInputAsync(this DialogContext dc)
        {
            return Task.FromResult(waiting);
        }

        /// <summary>
        /// Helper to send activity from dc directly
        /// </summary>
        public static Task<ResourceResponse> SendActivityAsync(this DialogContext dc, string textReplyToSend, string speak = null, string inputHint = "acceptingInput", CancellationToken cancellationToken = default(CancellationToken))
            => dc.Context.SendActivityAsync(textReplyToSend, speak, inputHint, cancellationToken);

        /// <summary>
        /// Helper to send activity from dc directly
        /// </summary>
        public static Task<ResourceResponse> SendActivityAsync(this DialogContext dc, IActivity activity, CancellationToken cancellationToken = default(CancellationToken))
            => dc.Context.SendActivityAsync(activity, cancellationToken);

        /// <summary>
        /// Begin dialog using the dialog class name as the dialog id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dc"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<DialogTurnResult> BeginDialog<T>(this DialogContext dc, object options, CancellationToken cancellationToken = default)
        {
            return dc.BeginDialogAsync(typeof(T).Name, options, cancellationToken);
        }

        /// <summary>
        /// Helper to add a DialogT
        /// </summary>
        /// <typeparam name="DialogT"></typeparam>
        /// <param name="dialogSet"></param>
        public static void Add<DialogT>(this DialogSet dialogSet)
            where DialogT: Dialog
        {
            dialogSet.Add(Activator.CreateInstance<DialogT>());
        }
    }
}
