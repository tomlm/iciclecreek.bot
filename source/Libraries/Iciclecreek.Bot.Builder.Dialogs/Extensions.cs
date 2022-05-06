using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json.Linq;

namespace Iciclecreek.Bot.Builder.Dialogs
{
    public static class Extensions
    {
        private const string REPLYTEXT = "turn.IcyReplyText";
        private static DialogTurnResult _waitingResult = new DialogTurnResult(DialogTurnStatus.Waiting);

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
            dc.State.SetValue("this.options", options);
        }

        /// <summary>
        /// Helper to Return signal that dialog is waiting for input from user.
        /// </summary>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static Task<DialogTurnResult> WaitForInputAsync(this DialogContext dc, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_waitingResult);
        }

        public static void CaptureSnapshot(this DialogContext dc)
        {
            if (!dc.State.TryGetValue<JObject>("turn.snapshot", out var snapshot))
            {
                dc.State.SetValue("turn.snapshot", dc.State.GetMemorySnapshot());
            }
        }

        /// <summary>
        /// Compares memory snapshot to current state and returns true if the path is changed
        /// </summary>
        /// <remarks>
        /// IcyDialog will call CaptureSnapshot() for you automatically To capture the snapshot
        /// as early as possible call CaptureSnapshot()
        /// </remarks>
        /// <param name="dc">dc</param>
        /// <param name="path">path to evaluate</param>
        /// <returns>true if path or children are different, false if they are unchanged.</returns>
        public static bool IsStateChanged(this DialogContext dc, string path)
        {
            dc.State.TryGetValue<JToken>($"turn.snapshot.{path}", out var snapshot);
            dc.State.TryGetValue<JToken>(path, out var current);
            return snapshot?.ToString() != current?.ToString();
        }

        /// <summary>
        /// CreateReplyActivity
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="variations">variations of adpative expressions (one will be randomly selected)</param>
        /// <returns>messageactivity</returns>
        public static Activity CreateReplyActivity(this DialogContext dc, params string[] variations)
        {
            if (variations.Length > 0)
            {

                var index = _rnd.Next(0, variations.Length);
                var expression = Expression.Parse($"`{variations[index]}`");
                var (result, error) = expression.TryEvaluate<string>(dc.State);
                return dc.Context.Activity.CreateReply(result);
            }

            return dc.Context.Activity.CreateReply();
        }

        private static Random _rnd = new Random();

        /// <summary>
        /// Append ReplyText to text any previous ReplyText to send to the user.
        /// </summary>
        /// <param name="dc">dc</param>
        /// <param name="variations">Adaptive Exprssion string variations (one will be randomly selected)</param>
        public static void AppendReplyText(this DialogContext dc, params string[] variations)
        {
            if (variations.Length > 0)
            {
                string replyText = dc.State.GetStringValue(REPLYTEXT) ?? string.Empty;

                var index = _rnd.Next(0, variations.Length);
                var expression = Expression.Parse($"`{variations[index]}`");
                var (result, error) = expression.TryEvaluate<string>(dc.State);
                if (!String.IsNullOrWhiteSpace(result))
                {
                    dc.State.SetValue("turn.IcyReplyText", $"{replyText}{result}");
                }
            }
        }

        /// <summary>
        /// GetReplyText
        /// </summary>
        /// <param name="dc"></param>
        /// <returns>current ReplyText</returns>
        public static string GetReplyText(this DialogContext dc)
        {
            return dc.State.GetStringValue(REPLYTEXT) ?? string.Empty;
        }

        /// <summary>
        /// AddReplyText and send
        /// </summary>
        /// <param name="dc">dc</param>
        /// <param name="variations">1..N expression variations to pick randomly between</param>
        /// <returns></returns>
        public static async Task<ResourceResponse> SendReplyText(this DialogContext dc, params string[] variations)
        {
            if (variations.Length > 0)
            {
                dc.AppendReplyText(variations);
            }
            return await dc.SendReplyText(CancellationToken.None);
        }

        /// <summary>
        /// AddReplyText and send
        /// </summary>
        /// <param name="dc">dc</param>
        /// <param name="variations">1..N expression variations to pick randomly between</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<ResourceResponse> SendReplyText(this DialogContext dc, CancellationToken cancellationToken, params string[] variations)
        {
            if (variations != null)
            {
                dc.AppendReplyText(variations);
            }
            return await dc.SendReplyText(cancellationToken);
        }


        /// <summary>
        /// Send any queued up reply text.
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<ResourceResponse> SendReplyText(this DialogContext dc, CancellationToken cancellationToken)
        {
            string replyText = dc.State.GetStringValue(REPLYTEXT) ?? string.Empty;
            if (!String.IsNullOrWhiteSpace(replyText))
            {
                dc.State.RemoveValue(REPLYTEXT);
                return await dc.Context.SendActivityAsync(dc.CreateReplyActivity(replyText.Trim()), cancellationToken);
            }
            return null;
        }

        /// <summary>
        /// Begin dialog using the dialog class name as the dialog id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dc"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<DialogTurnResult> BeginDialogAsync<T>(this DialogContext dc, object options, CancellationToken cancellationToken = default)
        {
            return dc.BeginDialogAsync(typeof(T).Name, options, cancellationToken);
        }


        /// <summary>
        /// Helper to add a DialogT
        /// </summary>
        /// <typeparam name="DialogT"></typeparam>
        /// <param name="dialogSet"></param>
        public static void Add<DialogT>(this DialogSet dialogSet)
            where DialogT : Dialog
        {
            dialogSet.Add(Activator.CreateInstance<DialogT>());
        }

        /// <summary>
        /// GetEntities
        /// </summary>
        /// <typeparam name="T">type for object</typeparam>
        /// <param name="recognizerResult">recognizerResult</param>
        /// <param name="jsonPath">$..dates..values</param>
        /// <returns></returns>
        public static IEnumerable<T> GetEntities<T>(this RecognizerResult recognizerResult, string jsonPath)
        {
            foreach (var token in recognizerResult.Entities.SelectTokens(jsonPath)
                .Where(jt => !jt.Path.Contains("$instance")))
            {
                if (token is JArray)
                {
                    foreach (var entity in token)
                    {
                        yield return entity.ToObject<T>();
                    }
                }
                else
                {
                    yield return token.ToObject<T>();
                }
            }
        }
    }
}
