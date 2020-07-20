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
