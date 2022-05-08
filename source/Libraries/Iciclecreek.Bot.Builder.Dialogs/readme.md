
# Overview
This library provides two classes
* **IcyBot** - An IBot implementation for code first bots without adaptive infrastructure dependencies.
* **IcyDialog** - A dialog which simplifies the creation of code based recognizer based dialogs 

## IBot
The library provides a default IBot implementation that uses dependency injection to get the dialogs.
The root dialog is the first Dialog registered in DI.

There is a service extension **AddIcyBot()** which registers the bot and ensures that state/memory scopes are registered.

The **AddPrompts()** DI method injects prompts as dialog classes (the Bot Framework dialog requires you to pass in id, instead of class name.) 
 
```C#
var sp = new ServiceCollection()
    .AddSingleton<IStorage,MemoryStorage>() // or whatever storage you want.
    .AddDialog<TestDialog>()
    .AddDialog<FooDialog>()
    .AddPrompts()
    .AddIcyBot()
    .BuildServiceProvider();
```

> NOTE 1: This bot is not set up to handle skills, lg, etc.  If you want all of that stuff you
> should use an the Adaptive.Runtime

> NOTE 2: IcyDialog runs fine in any IBot implementation, you do not need to use IcyBot.

## IcyDialog
IcyDialog encapsulates a number of patterns together to make a great base class for creating code-first dialogs.

### Dialog methods
* **OnBeginDialogAsync()** -  called when dialog is started.
* **OnEvaluateAsync()** - called when no dialog action is taken (including WaitForInput(). This gives you a place to inspect your state
and decide to prompt the user for information, regardless of how the state was changed.
* **OnPromptCompletedAsync()**/**OnPromptCanceledAsync** - called when a child dialog completes with a result/canceled.

There is a new dc helper method **dc.PromptAsync<DialogT>(property, options)**

This method is a enhancement over calling BeginDialog, because it models a dialogs purpose to gather information for a 
property in memory.The problem with BeginDialog is you have to track what dialog was invoked when you get a resume dialog to continue execution.

The **OnPromptCompletedAsync()** method is passed the property name and the default behavior for **OnPromptCompletedAsync()** is to set the property
name to the value that is returned.  This gives you nice a clean behavior
* you know what dialog (the one for the property) is completing
* you get out of the box default behavior which is saves the result in the property you pass.
* you have a natural place to acknowledge the value you just received.

```C#
    // when this prompt completes the the property "this.name" = result from the child dialog
    await PromptAsync<TextPrompt>("this.name", new PromptOptions(){ ... });
```
**OnEvaluateAsync()** can then be used to decide what the next prompt is needed for the user.

### Text Methods
There are 2 dc extension methods for managing sending reples.

* **dc.AppendReplyText()** - adds a new line of text to the response.
* **dc.SendReplyText()** - send any reply text which has been accumlated.
They both take variations of adaptive expression string interpolations one of which will be randonly selected and be evaluated.

Example
```C#
   // will pick one of these and add to the replyText as a new line.
   dc.AppendReplyText("Hi!", "Hello ${user.name}!", "Greetings");
   dc.AppendReplyText("Now we need to gather some info!", "I need some facts!", "A couple of questions...");
   await dc.SendReplyText("Here go!", "Let's get started..."); // send queued up text.
```

### Memory methods
You can query for a property changing using **dc.IsStateChanged(path)** method.
```C#
   if (dc.IsStateChanged("dialog.name"))
   {
       // do something...
   }
```

### Entities methods
You can query for the value of an entity using GetEntities and a JsonPath.

```C#
  var age = recognizerResult.GetEntities<int?>("$..number..value").FirstOrDefault();
```


### Activity methods
* **OnTurnAsync()** - The default **OnTurnAsync()** implementation will dispatch to strongly typed virtual methods (like ActivityHandler), but with DialogContext instead of TurnContext:
    - **OnMessageActivityAsync(dc)**
    - **OnEndOfConversationAsync(dc)**
    - **OnMessageReactionActivityAsync(dc)**
    - **OnAdaptiveCardInvoke(dc)** 
    - etc.
     
* The default **OnMessageActivity()** implementation invokes the Recognizer and routes the activity using **OnRecognizedIntentAsync()/OnUnrecognizedIntentAsync()** methods
* The default **OnRecognizedIntentAsync()** implementation will resolve methods to intent handlers using the following naming pattern:

```C#
protected Task<DialogTurnResult> OnXXXIntent(DialogContext dc, IMessageActivity messageActivity, TopScore topSCore, CancellationToken ct);
``` 
    
Examples:
- **"Greeting"** intent => **OnGreetingIntent**(dc, IMessageActivity, topScore, cancellationToken)
- **"Goodbye"** intent => **OnGoodbyeIntent**(dc, IMessageActivity, topScore, cancellationToken)
- **"None"** or empty intents => **OnUnrecognizedIntent**(dc, IMessageActivity, cancallationToken)

Sample dialog:
```C#
    internal class PromptTest : IcyDialog
    {
        public PromptTest()
        {
            this.Recognizer = new LucyRecognizer()
            {
                Intents = new List<string>() { "Greeting", "QueryName" },
                Model = YamlConvert.DeserializeObject<LucyDocument>(
@"
entities:
  - name: Greeting
    patterns:
      - hi

  - name: QueryName
    patterns:
      - what is my name
")
            };
        }



        protected async virtual Task<DialogTurnResult> OnGreetingIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            await dc.SendActivityAsync($"Hi!");
            return await OnEvaluateAsync(dc, cancellationToken);
        }

        protected async virtual Task<DialogTurnResult> OnQueryNameIntent(DialogContext dc, IMessageActivity messageActivity, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            var name = ObjectPath.GetPathValue<String>(dc.State, "this.name");
            if (name == null)
            {
                await dc.SendActivityAsync($"I don't know your name.");
            }
            else
            {
                await dc.SendActivityAsync($"Your name is {name}.");
            }
            return await OnEvaluateAsync(dc, cancellationToken);
        }

        protected async override Task<DialogTurnResult> OnEvaluateAsync(DialogContext dc, CancellationToken ct)
        {
            // if we are missing this.name, prompt for it.
            ObjectPath.TryGetPathValue<String>(dc.State, "this.name", out var name);
            if (String.IsNullOrEmpty(name))
            {
                return await PromptAsync<TextPrompt>(dc, "this.name", new PromptOptions() { Prompt = dc.CreateReply("What is your name?") });
            }

            // if we are missing... prompt for it.
            // ...

            // if we are all done, let's end the dialog...
            // return dc.EndDialogAsync(this);

            return await dc.WaitForInputAsync();
        }

        // hook this to use the name we got from the prompt in a greeting back to the user.
        protected override async Task<DialogTurnResult> OnPromptCompletedAsync(DialogContext dc, string property, object result, CancellationToken cancellationToken = default)
        {
            switch (property)
            {
                case "this.name":
                    await dc.SendActivityAsync($"Nice to meet you {result}!");
                    break;
            }
            return await base.OnPromptCompletedAsync(dc, property, result, cancellationToken); ;
        }

        protected override async Task<DialogTurnResult> OnEndOfConversationActivityAsync(DialogContext dc, IEndOfConversationActivity endOfConversationActivity, CancellationToken cancellationToken)
        {
            return await dc.CancelAllDialogsAsync();
        }
    }
}
```

## Cascading call pattern
IcyDialog splits processes input in cascading calls which will end at the point that a dailog action (BeginDailog/EndDialog/CancelDialog/WaitForInput).

The method pattern processing looks like this:

```
BeginDialog()
    => OnBeginDialog()
        => OnTurnAsync()
            => OnMessageActivity()
                => OnRecognizedIntent()
                    => OnXXXIntent()
                        =>OnEvaluateAsync()
                => OnUnrecognizedIntent()
                    => OnEvaluateAsync()
            => OnTypingActivity()
                => OnEvaluateAsync()
            => ...
ContinueDialog()
    => OnContinueDialog()
        => OnTurnAsync()
            => OnMessageActivity()
                => OnRecognizedIntent()
                    => OnXXXIntent()
                        =>OnEvaluateAsync()
                => OnUnrecognizedIntent()
                    => OnEvaluateAsync()
            => OnTypingActivity()
                => OnEvaluateAsync()
            => ...
ResumeDialog()
    => OnPromptCompletedAsync()
        => OnEvaluateAsync()
    => OnPromptCanceledAsync()
        => OnEvaluteAsync()
```

## Extension Helpers
The library includes some helpful extensions to reduce typing.

* **dc.SaveOptions(options)** and **dc.GetOptions<T>()** - methods for capturing and retrieving the options
* **dc.WaitForInputAsync()** - signal that your dialog is waiting input.
* **dc.SendActivity()** - shortcut for dc.Context.SendActivity()
* **dc.BeginDialog<DialogT>()** - begins a dialog assuming that the name of DialogT is the id of the dialog.
* **dialogSet.Add<DialogT>()** - Add an instance of dialogT to a dialogset
* **dc.IsPathChanged(path)** - well return true if the path has changed this turn.
* **recognizer.GetEntities<T>(JsonPath)** - will return resolved value for entities. 
