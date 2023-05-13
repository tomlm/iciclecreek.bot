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
            services.AddSingleton<IcyBot>();
            services.AddSingleton<IBot, IcyBot>();
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
            services.AddDialog<IcyAttachmentPrompt>();
            services.AddDialog<IcyChoicePrompt>();
            services.AddDialog<IcyDateTimePrompt>();
            services.AddDialog<IcyNumberPrompt<int>>();
            services.AddDialog<IcyNumberPrompt<float>>();
            services.AddDialog<IcyTextPrompt>();
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
            services.AddSingleton<IBot, BotT>();
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
            services.AddSingleton<DialogT>();
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
            services.TryAddSingleton<DialogT>();
            services.TryAddSingleton<Dialog, DialogT>();
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
            services.AddSingleton<DialogT>(implementationFactory);
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
            services.TryAddSingleton<DialogT>(implementationFactory);
            services.AddSingleton<Dialog, DialogT>(implementationFactory);
            return services;
        }
    }
}
