# DynamicListRecognizer
Dynamic lists are lists which are contextual and so not cannot be built into a model like LUIS Recognizer.  LUIS Recognizer allows you to define an **external recognizer** which is executed first. The results of the external recognizer can be passed to a recognizer like LUIS for it to consume.

This is super useful in situations where you need to recognize something contextual, like the names of people partipating in a conversation.

The DyanmicListRecognizer allows you to specify a list of entities to recgonize, and uses Lucene to do the recognition.

# Usage 

# Add nuget package
Add ```Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucene``` nuget reference to your project.

To add to your schema for usage in Bot Framework Composer from cli:

```shell
bf dialog:merge -p yourproj.proj -o your.schema
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
