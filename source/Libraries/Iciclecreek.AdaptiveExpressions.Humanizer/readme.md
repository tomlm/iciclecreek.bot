![icon](../../../icon.png)

# Humanizer Functions for AdaptiveExpression
This library provides humanizer functions to AdaptiveExpressions
* [AdaptiveExpressions](https://github.com/botbuilder-dotnet/libaries/adaptiveExpressions)
* [Humanizer](https://humanizr.net/) library which provides language libraries for converting standard data types into
"human" output. 

## Installation
To install 
```dotnet install Iciclecreek.AdaptiveExpressions.Humanizer```

In your startup code call:
``` HumanizerFunctions.Register() ```

## Library
This library creates expression functions which are bound to humanizer functions. 

### Text Capitalization
| Function            | Description                     | Example                               | Output           |
|---------------------|---------------------------------|---------------------------------------|------------------|
| **allCaps(text)**   | returns text all caps           | humanizer.allCaps("This is a test")   | "THIS IS A TEST" |
| **lowerCase(text)** | returns text all lower          | humanizer.lowerCase("This is a test") | "this is a test" |
| **sentence(text)**  | returns text as sentence cap    | humanizer.sentence("this is a test")  | "This is a test" |
| **title(text)**     | returns text with each word cap | humanizer.sentence("this is a test")  | "This Is A Test" |

### Humanize
The HumanizeXX functions turn complex structures like Dates and TimeSpans into human phrases

| Function                                                     | Description                        | Example                                              | Output                |
|--------------------------------------------------------------|------------------------------------|------------------------------------------------------|-----------------------|
| **humanizeDateTime(date,  ?culture)**                        | humanize a date relative to a now  | humanizer.humanizeDateTime(date)                     | "yesterday"           |
| **humanizeDateTime(date, relDate, ?culture)**                | humanize a date relative to a date | humanizer.humanizeDateTime(date, timeInPast)         | "an hour before"      |
| **humanizeTimeSpan(span, ?culture)**                         | humanize timespan                  | humanizer.humanizeTimeSpan(span)                     | "a couple of minutes" |
| **humanizeTimeSpan(span, GrammaticalCase,?culture)**         | humanize timespan                  | humanizer.humanizeTimeSpan(span, GrammaticalCase)    | a couple of minutes   |
| **dateTimeToOrdinalWords(date, ?culture)**                   | turn a date time into words        | humanizer.dateTimeToOrdinalWords(date)               | "May 5th, 2020"       |
| **dateTimeToOrdinalWords(date, ?GrammaticalCase, ?culture)** | turn a date time into words        | humanizer.dateTimeToOrdinalWords(date, 'Accusitive') | "May 5th, 2020"       |

### TimeSpan
Functions for creating TimeSpans from numbers.

| Function                 | Description                                  | Example                    | Output                        |
|--------------------------|----------------------------------------------|----------------------------|-------------------------------|
| **weeks(number)**        | creates timespan from number of weeks        | humanizer.weeks(13)        | TimeSpan.FromWeeks(13)        |
| **days(number)**         | creates timespan from number of days         | humanizer.days(13)         | TimeSpan.FromDays(13)         |
| **hours(number)**        | creates timespan from number of hours        | humanizer.hours(13)        | TimeSpan.FromHours(13)        |
| **minutes(number)**      | creates timespan from number of minutes      | humanizer.minutes(13)      | TimeSpan.FromMinutes(13)      |
| **seconds(number)**      | creates timespan from number of seconds      | humanizer.seconds(13)      | TimeSpan.FromSeconds(13)      |
| **milliseconds(number)** | creates timespan from number of milliseconds | humanizer.milliseconds(13) | TimeSpan.FromMilliseconds(13) |

### Bytes
Functions for converting intepreting number as binary units and converting to text.

| Function              | Description                          | Example                  | Output    |
|-----------------------|--------------------------------------|--------------------------|-----------|
| **bits(number)**      | converts number as bits to text      | humanizer.bits(32)       | "32 bits" |
| **bytes(number)**     | converts number as bytes to text     | humanizer.bytes(32)      | "32 B"    |
| **kilobytes(number)** | converts number as kilboytes to text | humanizer.kilobytes(65)  | "65 Kb"   |
| **megabytes(number)** | converts number as megabytes to text | humanizer.megabytes(1.3) | "1.3 Mb"  |
| **gigabytes(number)** | converts number as gigabytes to text | humanizer.gigabytes(1.3) | "1.3 Gb"  |
| **terabytes(number)** | converts number as terbytes to text  | humanizer.terabytes(1.3) | "1.3 Gb"  |

### Headings
Converts degrees to direction headings and back

| Function                                            | Description                                         | Example                               | Output            |
|-----------------------------------------------------|-----------------------------------------------------|---------------------------------------|-------------------|
| **degress2heading(number, ?culture)**               | converts number as degrees to heading text          | humanizer.degrees2heading(10)         | "NE"              |
| **degress2heading(number, headingstyle, ?culture)** | converts number as degrees to heading text as style | humanizer.degrees2heading(10, 'Full') | "north-northeast" |
| **heading2degrees(heading())**                      | gets degrees from heading text                      | humanizer.heading2degrees('ne')       | 45                |

### Text transforms
Text transformation functions.

| Function                                   | Description                                                         | Example                                             | Output           |
|--------------------------------------------|---------------------------------------------------------------------|-----------------------------------------------------|------------------|
| **pluralize(text, ?isKnownToBeSinglar)**   | turns singular word into plural                                     | humanizer.pluraize('cat')                           | "cats"           |
| **singularlize(text, ?isKnownToBePlural)** | turns plural word into singular                                     | humanizer.pluraize('cats')                          | "cat"            |
| **camelize(text)**                         | transform text into camelcase                                       | humanizer.camelize('this is a test')                | "thisIsATest"    |
| **dashorize(text)**                        | transform text to replace underscore with dash                      | humanizer.dashorize('this_is_a_test')               | "this-is-a-test" |
| **hyphenate(text)**                        | transform text to replace underscore with dash                      | humanizer.hyphenate('this_is_a_test')               | "this-is-a-test" |
| **kebaberize(text)**                       | transform text to replace whitespace with dash                      | humanizer.kebaberize('this is a test')              | "this-is-a-test" |
| **pascalize(text)**                        | transform text to replace whitespace with Uppercase word boundaries | humanizer.pascalize('this is a test')               | "ThisIsATest"    |
| **titleize(text)**                         | transform text to upercase words on word boundary                   | humanizer.titleize('this is a test')                | "This Is A Test" |
| **truncate(text, chars, truncation)**      | transform text to truncate at number of chars, appending trunction  | humanizer.truncate('this is a test', 3, '...')      | "thi..."         |
| **truncateWords(text, words, truncation)** | transform text to truncate at number of words, appending trunction  | humanizer.truncateWords('this is a test', 2, '...') | "this is a..."   |


### Number functions
for working with numbers, units and text.

| Function                                           | Description                                             | Example                                       | Output                      |
|----------------------------------------------------|---------------------------------------------------------|-----------------------------------------------|-----------------------------|
| **number2metric(text)**                            | convert number to metric text                           | humanizer.number2metric(1300)                 | '1.3k'                      |
| **metric2number(text, ?hasSpace, ?decimals)**      | interpret text as metric                                | humanizer.metric2number('1.3k')               | 1300                        |
| **number2words(number, ?culture)**                 | convert number to words                                 | humanizer.number2words(1500)                  | 'one thousand five hundred' |
| **number2words(number, ?gender, ?culture)**        | convert number to words                                 | humanizer.number2words(1500, 'Masculine')     | 'one thousand five hundred' |
| **number2ordinalWords(number, ?culture)**          | convert number to ordinal words (first, second)         | humanizer.number2ordinalWords(1)              | 'first'                     |
| **number2ordinalWords(number, ?gender, ?culture)** | convert number to ordinal words (first, second)         | humanizer.number2ordinalWords(1, 'Masculine') | 'first'                     |
| **number2ordinal(number)**                         | convert number to ordinal (1st, 2nd) words              | humanizer.number2ordinal(1)                   | '1st'                       |
| **number2ordinal(number, ?gender)**                | convert number to ordinal (1st, 2nd) words              | humanizer.number2ordinal(1, 'Masculine')      | '1st'                       |
| **fromRoman(text)**                                | convert text  to number                                 | humanizer.fromRoman('IV')                     | 4                           |
| **toRoman(number)**                                | convert number to roman numeral text                    | humanizer.toRoman(4)                          | 'IV'                        |
| **toQuantity(name, quantity, ?style)**             | convert name and quantity to grammatically correct name | humanizer.toQuantity('cats',0)                | 'no cats'                   |
| **toQuantity(name, quantity, ?style)**             | convert name and quantity to grammatically correct name | humanizer.toQuantity('cats',1, 'Numeric')     | '1 cat'                     |
| **toQuantity(name, quantity, ?style)**             | convert name and quantity to grammatically correct name | humanizer.toQuantity('cats',2, 'Words')       | 'Two cats'                  |

### Dotnet format functions
Functions which map to dotnet standard format strings.

| Function                     | Description     | Example                                     | Output       |
|------------------------------|-----------------|---------------------------------------------|--------------|
| **numberToString(format,?culture)**   | format number   | dotnet.numberToString(1.3, "C")             | "$1.30"      |
| **intToString(format,?culture)**      | format integer  | dotnet.numberToString(1, "D2")              | "1"          |
| **dateTimeToString(format,?culture)** | format dateTime | dotnet.dateTimeToString(date, "YYYY-MM-DD") | "2020-05-25" |
| **timeSpanToString(format,?culture)** | format dateTime | dotnet.timeSpanToString(span, "N")          | "00:43:00"   |

> NOTE: these functions are not part of humanizer, and are dotnet only, so they are in dotnet namespace.

## Language Support
By default the **Thread.CurrentThread.CurrentThread.CurrentUICulture** property will be used for locale sensitive conversations. 
Any method which has  a **?culture** argument can also accept the culture as a string like **'fr'**

Humanizer has broad open source language support:

| Code       | Language                     |
|------------|------------------------------|
| af         | Afrikaans                    |
| ar         | Arabic                       |
| bg         | Bulgarian                    |
| bn-BD      | Bangla (Bangladesh)          |
| cs         | Czech                        |
| da         | Danish                       |
| de         | German                       |
| el         | Greek                        |
| es         | Spanish                      |
| fa         | Persian                      |
| fi-FI      | Finnish (Finland)            |
| fr         | French                       |
| fr-BE      | French (Belgium)             |
| he         | Hebrew                       |
| hr         | Croatian                     |
| hu         | Hungarian                    |
| hy         | Armenian                     |
| id         | Indonesian                   |
| it         | Italian                      |
| ko-KR      | Korean (Korea)               |
| lv         | Latvian                      |
| ms-MY      | Malay (Malaysia)             |
| mt         | Maltese                      |
| nb         | Norwegian Bokmål             |
| nb-NO      | Norwegian Bokmål (Norway)    |
| nl         | Dutch                        |
| pl         | Polish                       |
| pt         | Portuguese                   |
| ro         | Romanian                     |
| ru         | Russian                      |
| sk         | Slovak                       |
| sl         | Slovenian                    |
| sr         | Serbian                      |
| sr-Latn    | Serbian (Latin)              |
| sv         | Swedish                      |
| th-TH      | Thai (Thailand)              |
| tr         | Turkish                      |
| uk         | Ukrainian                    |
| uz-Cyrl-UZ | Uzbek (Cyrillic, Uzbekistan) |
| uz-Latn-UZ | Uzbek (Latin, Uzbekistan)    |
| vi         | Vietnamese                   |
| zh-CN      | Chinese (Simplified, China)  |
| zh-Hans    | Chinese (Simplified)         |
| zh-Hant    | Chinese (Traditional)        |

