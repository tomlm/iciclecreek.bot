![icon](icon.png)

# QLuceneRecognizer
This library provides a custom recognizers for .qna files using Lucene library.

## Installation
To install into your project run the cli:

```dotnet add package Iciclecreek.Bot.Builder.Dialogs.Recognizers.QLucene```

In your startup code add:

```ComponentRegistration.Add(new QLuceneComponentRegistration());```

To add to your schema for usage in Bot Framework Composer from cli:

```bf dialog:merge -p yourproj.proj -o your.schema```

## Library
This library adds Icicielcreek.QLuceneRecognizer which builds a lucene index from .qna json file and serves as an embedded
recognizer for Bot Framework .qna files.


#### Sample Code
```C#
    var dialog = new AdaptiveDialog()
    {
        Recognizer = new QLuceneRecognizer()
        {
            ResourceId = "foo.en-us.qna"
        }
        ...
    }
```

#### Sample Json

```json
{
    "$kind": "Microsoft.AdaptiveDialog",
    "recognizer": {
        "$kind":"Iciclecrerek.QLuceneRecognizer",
        "resourceId": "foo.en-us.qna"
    },
    "triggers":[
        {
            "$kind":"Microsoft.OnIntent",
            "intent":"QnAMatch",
            "actions":[
                {
                    "$kind":"Microsoft.SendActivity"
                    "activitiy": "${turn.recognized.entities.answer[0]}"
                }
            ]
        }
    ]
}
```

### QLuBuild tool
The QLuBuild tool will process .qna files into the .qna.json files and automatically generate the supporting .dialog files
bound to the output.  It's useful, use it. :)

