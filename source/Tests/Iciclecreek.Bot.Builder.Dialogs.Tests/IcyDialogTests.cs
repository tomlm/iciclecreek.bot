using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Iciclecreek.Bot.Builder.Dialogs.Tests
{
    public class MyBot : IcyBot
    {
        public MyBot(ConversationState conversationState, UserState userState, IEnumerable<Dialog> dialogs, IEnumerable<IPathResolver> pathResolvers = null, IEnumerable<MemoryScope> scopes = null, IServiceProvider serviceProvider = null, ILogger logger = null)
            : base(conversationState, userState, dialogs, pathResolvers, scopes, serviceProvider, logger)
        {
            this.AddDialog<TestDialog>();
        }
    }

    public class IcyDialogTests
    {
        [Fact]
        public async Task MyNestedDialogTest()
        {
            var sp = new ServiceCollection()
                .AddSingleton<IStorage>(new MemoryStorage())
                .AddDialog<TestDialog>()
                .AddIcyBot()
                .BuildServiceProvider();

            await TestBot(sp);
        }

        [Fact]
        public async Task MyBotTest()
        {
            var sp = new ServiceCollection()
                .AddSingleton<IStorage>(new MemoryStorage())
                .AddBot<MyBot>()
                .BuildServiceProvider();

            await TestBot(sp);
        }

        [Fact]
        public async Task IcyBotTest()
        {
            var sp = new ServiceCollection()
                .AddSingleton<IStorage>(new MemoryStorage())
                .AddDialog<TestDialog>()
                .AddDialog<FooDialog>()
                .AddIcyBot()
                .BuildServiceProvider();

            await TestBot(sp);
        }

        [Fact]
        public async Task TestPathChanged()
        {
            var sp = new ServiceCollection()
                .AddSingleton<IStorage>(new MemoryStorage())
                .AddDialog<PathChangedDialog>()
                .AddPrompts()
                .AddIcyBot()
                .BuildServiceProvider();

            await new TestFlow(new TestAdapter(), sp.GetService<IBot>())
                .Send("hi")
                    .AssertReply("Hi!\n\nWhat is your name and age?")
                .Send("people call me Tom")
                    .AssertReply("Your name is Tom.\n\nWhat is your age?")
                .Send("i am 19 years old")
                    .AssertReply("Your age is 19.")
                .StartTestAsync();
        }

        [Fact]
        public async Task TestPathChanged2()
        {
            var sp = new ServiceCollection()
                .AddSingleton<IStorage>(new MemoryStorage())
                .AddDialog<PathChangedDialog>()
                .AddPrompts()
                .AddIcyBot()
                .BuildServiceProvider();

            await new TestFlow(new TestAdapter(), sp.GetService<IBot>())
                .Send("hi")
                    .AssertReply("Hi!\n\nWhat is your name and age?")
                .Send("My name is Tom and I'm 19.")
                    .AssertReply("Your name is Tom.\n\nYour age is 19.")
                .Send("hi")
                    .AssertReply("Hi!")
                .StartTestAsync();
        }

        [Fact]
        public async Task TestAnswer()
        {
            var sp = new ServiceCollection()
                .AddSingleton<IStorage>(new MemoryStorage())
                .AddDialog<AnswerTestDialog>()
                .AddPrompts()
                .AddIcyBot()
                .BuildServiceProvider();

            await new TestFlow(new TestAdapter(), sp.GetService<IBot>())
                .Send("hi")
                    .AssertReply("Hi!\n\nWhat is your name?")
                .Send("My name is Tom")
                    .AssertReply("Hello Tom!")
                .Send("change my name")
                    .AssertReply("What is your name?")
                .Send("Bob")
                    .AssertReply("Hello Bob!")
                .StartTestAsync();
        }

        [Fact]
        public async Task TestCallDialog()
        {
            var sp = new ServiceCollection()
                .AddSingleton<IStorage>(new MemoryStorage())
                .AddDialog<PromptTest>()
                .AddPrompts()
                .AddIcyBot()
                .BuildServiceProvider();

            await new TestFlow(new TestAdapter(), sp.GetService<IBot>())
                .Send("hi")
                    .AssertReply("Hi!")
                    .AssertReply("What is your name?")
                .Send("Tom")
                    .AssertReply("Nice to meet you Tom!")
                .Send("what is my name")
                    .AssertReply("Your name is Tom.")
                .StartTestAsync();
        }


        private static async Task TestBot(ServiceProvider sp) =>
            await new TestFlow(new TestAdapter(), sp.GetService<IBot>())
                        // ---- TestDialog Active
                        .Send("hi")
                            // TestDialog intent
                            .AssertReply("Hello")
                        .Send("bye")
                            // TestDialog intent
                            .AssertReply("Goodbye")
                        .Send("Foo")
                            // TestDialog -> BeginDialog FooDialog
                            .AssertReply("Foo 1")
                        // --- FooDialog Active
                        .Send("yo")
                            // FooDialog intent
                            .AssertReply("Yo 1")
                        .Send("end")
                            // FooDialog intent-> End
                            .AssertReply("End 1")
                        // --- TestDialog Active
                        .Send("hi")
                            // TestDialog intent
                            .AssertReply("Hello")
                        .Send("xxxx")
                            // TestDialog Unknown intent
                            .AssertReply("I'm sorry, I didn't understand that.")
                        .Send("bye")
                            // TestDialog intent
                            .AssertReply("Goodbye")
                        // TestDialog activity handling.
                        .Send(Activity.CreateEndOfConversationActivity())
                            .AssertReply("EndOfConversation")
                        .StartTestAsync();
    }
}
