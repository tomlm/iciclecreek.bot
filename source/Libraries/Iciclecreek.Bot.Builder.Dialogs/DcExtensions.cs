using System;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Iciclecreek.Bot.Builder.Dialogs
{
    public static class DcExtensions
    {
        private static DialogTurnResult _waitingResult = new DialogTurnResult(DialogTurnStatus.Waiting);

        internal const string REPLYTEXT_PATH = "turn.icy.ReplyText";
        internal const string PROPERTY_KEY = "this.icy.property";
        internal const string OPTIONS_PATH = "this.icy.options";
        internal const string SNAPSHOT_PATH = "turn.icy.snapshot";
        internal const string LASTQUESTION_PATH = "this.icy.lastquestion";

        /// <summary>
        /// Get saved options
        /// </summary>
        /// <typeparam name="T">Options type</typeparam>
        /// <param name="dc">dc</param>
        /// <returns>instance of optionsT</returns>
        public static T GetOptions<T>(this DialogContext dc)
        {
            if (ObjectPath.TryGetPathValue<T>(dc.State, OPTIONS_PATH, out var result))
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
            dc.State.SetValue(OPTIONS_PATH, options);
        }

        /// <summary>
        /// Cancel the current dialog only and return cancelled dialogturnstatus.
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="result">result to return</param>
        /// <param name="cancellationToken"></param>
        /// <returns>DialogTurnStatus.Canceled</returns>
        public async static Task<DialogTurnResult> CancelDialogAsync(this DialogContext dc, object result = null, CancellationToken cancellationToken = default)
        {
            if (result is CancellationToken)
            {
                throw new ArgumentException($"{dc.ActiveDialog.Id}.EndDialogAsync() You can't pass a cancellation token as the result of a dialog when calling EndDialog.");
            }

            // End the active dialog
            if (result is CancellationToken)
            {
                throw new ArgumentException($"{nameof(result)} cannot be a cancellation token");
            }

            var instance = dc.ActiveDialog;
            if (instance != null)
            {
                // Lookup dialog
                var dialog = dc.Dialogs.Find(instance.Id);
                if (dialog != null)
                {
                    // Notify dialog of end
                    await dialog.EndDialogAsync(dc.Context, instance, DialogReason.CancelCalled, cancellationToken).ConfigureAwait(false);
                }

                // Pop dialog off stack
                dc.Stack.RemoveAt(0);
            }

            // Resume parent dialog
            if (dc.ActiveDialog != null)
            {
                // Lookup dialog
                var dialog = dc.FindDialog(dc.ActiveDialog.Id);
                if (dialog == null)
                {
                    throw new InvalidOperationException($"DialogContext.EndDialogAsync(): Can't resume previous dialog. A dialog with an id of '{dc.ActiveDialog.Id}' wasn't found.");
                }

                // Return result to previous dialog
                return await dialog.ResumeDialogAsync(dc, DialogReason.CancelCalled, result: result, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return new DialogTurnResult(DialogTurnStatus.Complete, result);
            }
        }

        /// <summary>
        /// RouteDialogAsync() - route activity to dialog to be processed.
        /// </summary>
        /// <remarks>
        /// if you use a recognizer (IcyDialog or AdaptiveDialog) it will set a flag signalling that the activity is processed
        /// When BeginDialog() is called the called dialog by default will not process the input again.
        ///
        /// An example of this is the outer dialog has a list of numbered options.  The user picks option "1". You don't want
        /// the called dialog to process "1" as input.  BeginDialog() behaves this way.
        /// 
        /// But if the outer dialog is routing the activity it has a model which indicates which dialog to route the activity to be
        /// processed.  The RouteDialogAsync() method is just like BeginDialog only it clears the recognizer flag so that target
        /// dialog will attempt to interpret the activity.
        /// </remarks>
        /// <typeparam name="DialogT">dialog to beginDialog on</typeparam>
        /// <param name="dc"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<DialogTurnResult> RouteDialogAsync<DialogT>(this DialogContext dc, object options, CancellationToken cancellationToken)
            where DialogT : Dialog
        {
            // signal that activity has not been recognized and the IcyDialog/AdaptiveDialog/... 
            dc.State.SetValue(TurnPath.ActivityProcessed, false);
            return dc.BeginDialogAsync<DialogT>(options, cancellationToken);
        }

        /// <summary>
        /// PromptAsync() - Begins a dialog and stores the result from the dialog in property path
        /// </summary>
        /// <typeparam name="DialogT"></typeparam>
        /// <param name="dc">dc</param>
        /// <param name="property">name of the property for the dialog result (Ex: "this.name")</param>
        /// <param name="options">options for the dialog</param>
        /// <param name="cancellationToken">ct</param>
        /// <returns>dtr</returns>
        public static Task<DialogTurnResult> PromptAsync<DialogT>(this DialogContext dc, string property, object options, CancellationToken cancellationToken = default)
            where DialogT : Dialog
        {
            return PromptAsync(dc, typeof(DialogT).Name, property, options, cancellationToken);
        }

        /// <summary>
        /// PromptAsync() - Begins a dialog and stores the result from the dialog in property path
        /// </summary>
        /// <param name="dc">dc</param>
        /// <param name="dialogId">dialogId</param>
        /// <param name="property">name of the property for the dialog result (Ex: "this.name")</param>
        /// <param name="options">options for the dialog</param>
        /// <param name="cancellationToken">ct</param>
        /// <returns>dtr</returns>
        public static async Task<DialogTurnResult> PromptAsync(this DialogContext dc, string dialogId, string property, object options, CancellationToken cancellationToken = default)
        {
            dc.State.SetValue(PROPERTY_KEY, property ?? dialogId);
            await dc.SendReplyText(cancellationToken);
            return await dc.BeginDialogAsync(dialogId, options, cancellationToken);
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

        /// <summary>
        /// End the conversation (and all dialogs) 
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async static Task<DialogTurnResult> EndConversationAsync(this DialogContext dc, CancellationToken cancellationToken = default)
        {
            var activity = dc.Context.Activity.CreateReply();
            activity.Type = ActivityTypes.EndOfConversation;
            await dc.Context.SendActivityAsync(activity);
            return await dc.CancelAllDialogsAsync(cancellationToken);
        }

        public static void CaptureSnapshot(this DialogContext dc)
        {
            if (!dc.State.TryGetValue<JObject>(SNAPSHOT_PATH, out var snapshot))
            {
                dc.State.SetValue(SNAPSHOT_PATH, dc.State.GetMemorySnapshot());
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
            dc.State.TryGetValue<JToken>($"{SNAPSHOT_PATH}.{path}", out var snapshot);
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
                string replyText = dc.State.GetStringValue(REPLYTEXT_PATH) ?? string.Empty;

                var index = _rnd.Next(0, variations.Length);
                var expression = Expression.Parse($"`{variations[index]}`");
                var (result, error) = expression.TryEvaluate<string>(dc.State);
                if (!String.IsNullOrWhiteSpace(result))
                {
                    dc.State.SetValue(REPLYTEXT_PATH, $"{replyText}{result}");
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
            return dc.State.GetStringValue(REPLYTEXT_PATH) ?? string.Empty;
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
            string replyText = dc.State.GetStringValue(REPLYTEXT_PATH) ?? string.Empty;
            if (!String.IsNullOrWhiteSpace(replyText))
            {
                dc.State.RemoveValue(REPLYTEXT_PATH);
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
        /// Ask a question, remembering the question and waiting for input.
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="label">label to use for the question.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="variations">variations of the question.</param>
        /// <returns></returns>
        public static Task<DialogTurnResult> AskQuestionAsync(this DialogContext dc, string label, params string[] variations)
        {
            return dc.AskQuestionAsync(label, CancellationToken.None, variations);
        }

        /// <summary>
        /// Ask a question, remembering the question and waiting for input.
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="label">label to use for the question.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="variations">variations of the question.</param>
        /// <returns></returns>
        public static async Task<DialogTurnResult> AskQuestionAsync(this DialogContext dc, string label, CancellationToken cancellationToken, params string[] variations)
        {
            dc.State.SetValue(LASTQUESTION_PATH, label);
            await dc.SendReplyText(cancellationToken, variations);
            return await dc.WaitForInputAsync(cancellationToken);
        }

        /// <summary>
        /// Get the label of the last question asked.
        /// </summary>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static string GetLastQuestion(this DialogContext dc)
        {
            return dc.State.GetValue<string>(LASTQUESTION_PATH);
        }

        /// <summary>
        /// Clear out the last question asked.
        /// </summary>
        /// <param name="dc"></param>
        public static void ClearQuestion(this DialogContext dc)
        {
            dc.State.RemoveValue(LASTQUESTION_PATH);
        }

        /// <summary>
        /// Get the last result returned to ResumeDialog
        /// </summary>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static T GetLastResult<T>(this DialogContext dc)
        {
            return dc.State.GetValue<T>(TurnPath.LastResult);
        }

    }
}
