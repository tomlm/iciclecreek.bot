![icon](icon.png)

# Recognizers
This library provides custom recognizers for Bot Framework/Composer.

## Installation
To install into your project run the cli:

```dotnet add package Iciclecreek.Bot.Builder.Dialogs.Recognizers```

In your startup code add:

```ComponentRegistration.Add(new RecognizerComponentRegistration());```

To add to your schema for usage in Bot Framework Composer from cli:

```bf dialog:merge -p yourproj.proj -o your.schema```

## Library
This library adds recognizers:
* **PersonNameEntityRecognizer** - Recognizes common @givenName @surname, @fullname entities (like "John Smith"=> GivenName:"john" Surname:"Smith", fullname:"John Smith")
* **QuotedTextEntityRecognizer** - Recognizes quoted strings as @QuotedText entities
* **CsvTextEntityRecognizer** - Uses a .CSV file to define tokens to match to entities
* **ThresholdRecognizer** - Applies a threshold to intents, any intents which have scores which are within the threshold will trigger an OnChooseIntent event.


### PersonNameEntityRecognizer
Recognizes common @givenName @surname, @fullname entities (like "John Smith"=> GivenName:"john" Surname:"Smith", fullname:"John Smith"). You can optionally point it to a
URL to extend matched given names and surnames.

#### Sample Code
```C#
    var dialog = new AdaptiveDialog()
    {
        Recognizer = new RegexRecognizer()
        {
            Entities = new EntityRecognizerSet()
            {
                new PersonNamEntityRecognizer()
            }
        }
        ...
    }
```

#### Sample Json

```json
{
    "$kind": "Microsoft.AdaptiveDialog",
    "recognizer": {
        "$kind":"Microsoft.RegexRecognizer",
        "entities": [
            {
                "$kind":"Iciclecreek.PersonNameEntityRecognizer"
            }
        ]
    }
}
```
### QuotedTextEntityRecognizer
Use the QuotedTextEntityRecognizer to recognize quoted strings as a @QuotedText entity across 64 languages:
* Afrikaans
* Albanian
* Amharic
* Arabic
* Armenian
* Azerbaijani
* Basque
* Belarusian
* Bosnian
* Bulgarian
* Catalan
* Chinese
* Croatian
* Czech
* Danish
* Dutch
* English
* Esperanto
* Estonian
* Filipino
* Finnish
* French
* Galician
* Georgian
* German
* Greek
* Hebrew
* Hindi
* Hungarian
* Icelandic
* Indonesian
* Interlingua
* Irish
* Italian
* Japanese
* Kazakh
* Khmer
* Korean
* Lao
* Latvian
* Lithuanian
* Macedonian
* Maltese
* New Tai Lue
* Norwegian
* Occitan
* Pashto
* Persian
* Polish
* Romanian
* Russian
* Serbian
* Scottish Gaelic
* Slovak
* Slovene
* Sorbian
* Spanish
* Swedish
* Tai Le
* Tamil
* Tibetan
* Tigrinya
* Thai
* Turkish
* Ukrainian
* Uyghur
* Uzbek
* Vietnamese
* Welsh
 

#### Sample Code
```C#
    var dialog = new AdaptiveDialog()
    {
        Recognizer = new RegexRecognizer()
        {
            Entities = new EntityRecognizerSet()
            {
                new QuotedTextEntityRecognizer()
            }
        }
        ...
    }
```

#### Sample Json

```json
{
    "$kind": "Microsoft.AdaptiveDialog",
    "recognizer": {
        "$kind":"Microsoft.RegexRecognizer",
        "entities": [
            {
                "$kind":"Iciclecreek.QuotedTextEntityRecognizer"
            }
        ]
    }
}
```


### CsvEntityRecognizer
Use the CsvEntityRecognizer to easily define custom entities to recognize from a csv file.

The CSV file should be in the format:

**TOKEN,TYPE,VALUE1,..,VALUEN**

The value columns are property paths which will be used to set modify the entity with the data in the value column.

Example:
```
TOKEN,TYPE,genus.class,genus.latin
alligator,animal,reptile,alligator mississippiensis
squirrel,animal,mammal,squirrus maximus
...



#### Sample Code
```C#
    var dialog = new AdaptiveDialog()
    {
        Recognizer = new RegexRecognizer()
        {
            Entities = new EntityRecognizerSet()
            {
                new CsvEntityRecognizer()
                {
                    Url = "http://contoso.com/animals.csv"
                }
            }
        }
        ...
    }
```

#### Sample Json

```json
{
    "$kind": "Microsoft.AdaptiveDialog",
    "recognizer": {
        "$kind":"Microsoft.RegexRecognizer",
        "entities": [
            {
                "$kind":"Iciclecreek.CsvEntityRecognizer",
                "Url": "http://contoso.com/animals.csv"
            }
        ]
    }
}
```

### ThresholdRecognizer
Applies a threshold to intents, any intents which have scores which are within the threshold will trigger an OnChooseIntent event.

#### Sample Code
```C#
    var dialog = new AdaptiveDialog()
    {
        Recognizer = new ThresholdRecognizer()
        {
            Threshold = 1.0f,
            Recognizer = ...
        }
        ...
    }
```

#### Sample Json

```json
{
    "$kind": "Microsoft.AdaptiveDialog",
    "recognizer": {
        "$kind":"Iciclecreek.ThresholdRecognizer",
        "threshold": 0.2,
        "recognizer": {
            ...
        }
    }
}
```

#### Sample code for handling OnChooseIntent

```C#
    new OnChooseIntent()
    {
        Intents = new List<string> { "foo","bar" },
        Actions = new List<Dialog>()
        {
            new SetProperty()
            {
                Property = "dialog.candidates",
                Value = $"=turn.recognized.candidates"
            },
            new ChoiceInput()
            {
                    Choices = new ChoiceSet()
                    {
                        new Choice("foo"),
                        new Choice("bar"),
                    },
                    Prompt = new ActivityTemplate("Which intent?")
            },
            new EmitEvent()
            {
                EventName = AdaptiveEvents.RecognizedIntent,
                EventValue = "=first(where(dialog.candidates, c, c.intent == turn.lastResult)).result"
            },
            new DeleteProperty()
            {  
                Property = "dialog.candidates"
            }
        }
    }
```


#### Sample code for generically handling OnChooseIntent
If you don't have the Intents constraint, then you can programmatically ask for the intents ( or map the intents to human language) like this:

```C#
new ChoiceInput()
{
     Choices = "=select(dialog.candidates, result, result.intent)",
     Prompt = new ActivityTemplate("Which intent?")
},
```

