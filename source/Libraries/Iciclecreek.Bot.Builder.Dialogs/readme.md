
# Overview
This library provides two base clases
* **IcyBot** - An IBot implementation for code first bots without adaptive infrastructure dependencies.
* **IcyDialog** - A base dialog which simplifies the creation of code based recognizer based dialogs 

## IBot
The library provides a default IBot implementation that uses Dependency Injection to get the dialogs.
The root dialog is the first Dialog registered in DI.

There is a helper extension **AddBot()** which registers the bot and ensures that state/memory scopes are registered.
 
```C#
var sp = new ServiceCollection()
    .AddSingleton<IStorage,MemoryStorage>() // or whatever storage you want.
    .AddSingleton<Dialog, TestDialog>()
    .AddSingleton<Dialog, FooDialog>()
    .AddBot()
    .BuildServiceProvider();
```

> NOTE: This bot is not set up to handle skills, lg, etc.  If you want all of that stuff you should use an the Adaptive.Runtime
> This bot is a suitable for simple bots that don't need multi-language declarative support, etc.

## IcyDialog
IcyDialog encapsulates a number of patterns together to make a great base class for creating code-first dialogs.

1. hides **BeginDialog/ContinueDialog** and models the dialog simply as **OnTurnAsync()**
    - dialog Options are autoamtically captured via dc.SaveOptions() and available via dc.GetOptions() on any turn.
2. The default OnTurnAsync() will dispatch to strongly typed virtual methods (like ActivityHandler), but with DialogContext instead of TurnContext:
    - **OnMessageActivityAsync(dc)**
    - **OnEndOfConversationAsync(dc)**
    - **OnMessageReactionActivityAsync(dc)**
    - **OnAdaptiveCardInvoke(dc)** 
    - etc.
     
3. The default **OnMessageActivity** will invoke the Recognizer and route the activity using **OnRecognizedIntentAsync()/OnUnrecognizedIntentAsync()** methods
4. The default **OnRecognizedIntentAsync()** implementation will resolve methods to intent handlers using the following naming pattern:

```C#
protected Task<DialogTurnResult> OnXXXIntent(DialogContext dc, IMessageActivity messageActivity, TopScore topSCore, CancellationToken ct);
``` 
    
Examples:
- **"Greeting"** intent => **OnGreetingIntent**(dc, IMessageActivity, topScore, cancellationToken)
- **"Goodbye"** intent => **OnGoodbyeIntent**(dc, IMessageActivity, topScore, cancellationToken)
- **"None"** or empty intents => **OnUnrecognizedIntent**(dc, IMessageActivity, cancallationToken)

Sample dialog:
```C#
    public class TestDialog : IcyDialog
    {
        public TestDialog()
        {
            // create a recognizer
            this.Recognizer = new LucyRecognizer()
            {
                Intents = new string[] { "Greeting", "HighFive", "Goodbye", "Foo" },
                Model = YamlConvert.DeserializeObject<LucyDocument>(...)
            };
        }

        protected async Task<DialogTurnResult> OnGreetingIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            await dc.SendActivityAsync("Hello");
            return await dc.WaitForInputAsync();
        }

        protected async Task<DialogTurnResult> OnHighFiveIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            await dc.SendActivityAsync("Slap!");
            return await dc.WaitForInputAsync();
        }

        protected async Task<DialogTurnResult> OnFooIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            return await dc.BeginDialog<FooDialog>(1, cancellationToken: cancellationToken);
        }

        protected async Task<DialogTurnResult> OnGoodbyeIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            await dc.SendActivityAsync("Goodbye");
            return await dc.EndDialogAsync();
        }

        protected override async Task<DialogTurnResult> OnEndOfConversationActivityAsync(DialogContext dc, IEndOfConversationActivity endOfConversationActivity, CancellationToken cancellationToken)
        {
            await dc.SendActivityAsync("EndOfConversation");
            return await dc.CancelAllDialogsAsync();
        }

    }
}
```

## Extension Helpers
The library includes some helpful extensions to reduce typing.

* **dc.SaveOptions(options)** and **dc.GetOptions<T>()** - methods for capturing and retrieving the options
* **dc.WaitForInputAsync()** - signal that your dialog is waiting input.
* **dc.SendActivity()** - shortcut for dc.Context.SendActivity()
* **dc.BeginDialog<DialogT>()** - begins a dialog assuming that the name of DialogT is the id of the dialog.
* **dialogSet.Add<DialogT>()** - Add an instance of dialogT to a dialogset 
