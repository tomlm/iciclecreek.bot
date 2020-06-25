![icon](icon.png)

# Comos Actions
This library provides custom recognizers for Bot Framework/Composer.

## Installation
To install into your project run the cli:

```dotnet add package Iciclecreek.Bot.Builder.Dialogs.Recognizers```

In your startup code add:

```ComponentRegistration.Add(new RecognizerComponentRegistration());```

To add to your schema for usage in Bot Framework Composer from cli:

```bf dialog:merge -p yourproj.proj -o your.schema```

## Library
This library adds an entity recognizer to recognize quoted strings as a @QuotedText entity across 64 languages:
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
 

## Sample Code
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

## Sample Json

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
