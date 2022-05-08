using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Iciclecreek.Bot.Builder.Dialogs
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Registers IcyBot and UserState, ConversationState, Memory, PathResolvers 
        /// </summary>
        /// <remarks>
        /// You still need to register IStorage, Dialogs
        /// </remarks>
        /// <param name="services"></param>
        public static IServiceCollection AddIcyBot(this IServiceCollection services)
        {
            services.TryAddSingleton<UserState>();
            services.TryAddSingleton<ConversationState>();
            new DialogsBotComponent().ConfigureServices(services, new ConfigurationBuilder().Build());
            services.AddPrompts();
            services.TryAddSingleton<IBot, IcyBot>();
            return services;
        }

        /// <summary>
        /// Add default prompts (Attachment, choice, DateTime, Number, Text) so you can invoke them by type instead of id
        /// </summary>
        /// <remarks>
        /// dc.BeginDialog&lt;TextPrompt&gr;(new PrompOptions() {...} );
        /// </remarks>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddPrompts(this IServiceCollection services)
        {
            services.TryAddDialog<IcyAttachmentPrompt>();
            services.TryAddDialog<IcyChoicePrompt>();
            services.TryAddDialog<IcyDateTimePrompt>();
            services.TryAddDialog<IcyNumberPrompt<int>>();
            services.TryAddDialog<IcyNumberPrompt<float>>();
            services.TryAddDialog<IcyTextPrompt>();
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
        /// Add dialog to dependency injection
        /// </summary>
        /// <typeparam name="DialogT"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDialog<DialogT>(this IServiceCollection services)
            where DialogT : Dialog
        {
            services.AddSingleton<Dialog, DialogT>();
            return services;
        }

        /// <summary>
        /// Add dialog to dependency injection
        /// </summary>
        /// <typeparam name="DialogT"></typeparam>
        /// <param name="services"></param>a
        /// <returns></returns>
        public static IServiceCollection TryAddDialog<DialogT>(this IServiceCollection services)
            where DialogT : Dialog
        {
            services.AddSingleton<Dialog, DialogT>();
            return services;
        }

        /// <summary>
        /// Add dialog to dependency injection
        /// </summary>
        /// <typeparam name="DialogT"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDialog<DialogT>(this IServiceCollection services, Func<IServiceProvider, DialogT> implementationFactory)
            where DialogT : Dialog
        {
            services.AddSingleton<Dialog, DialogT>(implementationFactory);
            return services;
        }

        /// <summary>
        /// Add dialog to dependency injection
        /// </summary>
        /// <typeparam name="DialogT"></typeparam>
        /// <param name="services"></param>a
        /// <returns></returns>
        public static IServiceCollection TryAddDialog<DialogT>(this IServiceCollection services, Func<IServiceProvider, DialogT> implementationFactory)
            where DialogT : Dialog
        {
            services.AddSingleton<Dialog, DialogT>(implementationFactory);
            return services;
        }


    }
}
