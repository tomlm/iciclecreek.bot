![Lucy Logo](icon256.png)

# Lucy Overview
Lucy is an Entity Recognition engine which defines a simple yaml syntax for recognizing entities in text.

#  Core concept
Lucy's core concept is that:
> **Entities** are cascading patterns of **tokens** and **entities**.

Lucy focuses on **capturing your knowledge** by focusing on **token patterns**.

Token patterns are a natural way of building a model, because it allows you to focus on the 
fragments of information you understand in way that is instantly testable and naturally
builds up to more complex entities.

# Key tenants

## Tenant 1: **Minimize concept count**
Lucy has 2 core concepts:
1. **Name your Entities** - name each entity in your system.
2. **Define their Patterns** - Define entity patterns as simple sequences of tokens and other entities.

## Tenant 2: **Instant feedback**
When authoring you want immediate feedback on the impact
of your changes.  Lucy acheives this by not requiring
any training or even CLI tool to be invoked to 
see the results of your changes.

## Tenant 3:  **Maximize your input**
Traditional ML systems require you to "invent" many
permutations of sentence structure and the laboriously
label them for the system to understand your patterns.

Lucy focuses on a syntax which needs minimal changes to affect your desired outcome and maximizes the reuse.

# Lucy's relationship to ML
The Lucy engine can be run on it's own and work perfectly
fine, but it can also be coupled with existing LU engines
such as LUIS or Orchestrator to great benefit.
* **External Entity Recognizer** - Lucy can be used as a runtime source of external entities to a machine learning system such as LUIS.
* **Generate LU files** - Lucy can be used to generate .LU files which can contain labeled examples and entity references.

# Language support
There are 2 factors controlling language support in Lucy.

## Tokenizers
Lucy uses off the shelf Lucene token analyzers to tokenize text into tokens.
There are currently token analyzers for 29 lanuages:
* *Arabic
Armenian
Basque
Catalan
Chinese
Czech
Danish
Dutch
English
Finnish
French
Galician
German
Greek
Hindi
Hungarian
Chinese
Japanese
Korean
Indonesian
Irish
Italian
Latvian
Norwegian
Persian
Portuguese
Romanian
Russian
Spanish
Swedish
Turkish*


## Microsoft.Text.Recognizers
The Microsoft.Text.Recognizers libraries provide support 
for core entity types:
* *age boolean currency datetime dimension email guid hashtag 
ip mention number numberrange ordinal percentage phonenumber temperature url*

Microsoft.Text.Recognizers support 15 languages
* *Arabic
Bulgarian
Chinese 
German
English
Spanish
French
Hindi
Italian
Japanese
Korean
Dutch
Portuguese
Swedish
Turkish*

> See [Microsoft.Text.Recognizers](https://github.com/Microsoft/Recognizers-Text) for a complete listing of languages and support.

