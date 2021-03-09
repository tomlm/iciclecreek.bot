# DynamicListRecognizer
Dynamic lists are lists which are contextual and so not cannot be built into a model like LUIS Recognizer.  LUIS Recognizer allows you to define an **external recognizer** which is executed first. The results of the external recognizer can be passed to a recognizer like LUIS for it to consume.

This is super useful in situations where you need to recognize something contextual, like the names of people partipating in a conversation.

The DyanmicListRecognizer allows you to specify a list of entities to recgonize, and uses Lucene to do the recognition.

# Usage 

# Add nuget package
Add ```Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucene``` nuget reference to your project.

# Register
> NOTE: as of 4.12 if you are using the new runtime integration you do not need to modify startup.cs, it is autoregistered.

Register the classes in startup.cs
```
            ComponentRegistration.Add(new LucyComponentRegistration());
```

## Define a dynamic list recognizer as input to a luis recognizer
```json
{
    "$kind": "Microsoft.LuisRecognizer",
    ...
    "externalEntityRecognizer": {
        "$kind": "Iciclecreek.DynamicListRecognizer",
        "dynamicList": {
        ...
        },
        "fuzzyMatch":false
    }
}
```
See https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cognitiveservices.language.luis.runtime.models.dynamiclist.-ctor?view=azure-dotnet for shape of dynamiclist
