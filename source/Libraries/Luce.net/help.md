# LucE Help
LucE *(pronounced Lucy)* is an Entity Recognition engine which defines a simple yaml syntax for recognizing **entities**.

# Entities
Entites are *named fragments of information*. 

When entities are recognized they have **resolution** which is structural
information from the recognition process.

For example, ```@dimension``` outputs a structure ```{ value:13, units:inches }```

Entities are made up of 1..N token patterns.

# Patterns
A pattern is a sequence of tokens and/or entities.  Patterns look a like regular 
expression but they are not.  A regular expression defines sequences at the 
**character** level, while LucE patterns are expressed at the **token** level.  

LucE uses Lucene language tokenizers to normalize text into tokens with best practices of stemming, punctation, etc. Since your token patterns are matched at the token level
you don't have to worry about punctionation, stemming, etc.

Given a simple pattern ```walk the dog``` you are telling LucE to look for 
a sequence of normalized tokens ```["walk", "the", "dog"]```.

## Token patterns
Matching a set of alternative tokens is the simplest case. To do this is easy:
```yaml
  - name: '@drinkSize'
    patterns: 
    - small
    - medium
    - large
```
or even more simply:
```yaml
  - name: '@drinkSize'
    patterns: [small, medium, large]
```
Given text like ```I would like a large``` will match ``@drinkSize = large``.

## Canonical values
When an entity is recognized, the calling program will get an entity where the 
resolution is the matched text. But the matched text could be mispelled or a 
language variation. 

Programmers like working with a fixed set of enumerated values, aka *canonical values*.

To do that is easy, just change a pattern line to be an array of strings, and if any 
of the tokens is found, the first one in the array will be the resolution.

Example:
```yaml
  - name: '@drinkSize'
    patterns:
    - [s, small, short, tiny]
    - [m, medium, tall]
    - [l, large, big]
    - [xl, extra large, venti, huge, ginormous, enormous]
```
Given text like ```I would like a extra large``` will match ``@drinkSize = xl``.

## Entity references
In a pattern sequence you can refer to an entity by using ```@``` prefix.

 ```@xxx``` in a sequence means
*"if you found an entity called ```xxx``` aligning here in the token sequence, then I would
match that please."*

Example:
```yaml
  - name: '@animal'
    patterns:
    - dog
    - cat
    - snake
  - name: '@myentity'
    patterns:
    - walk the @animal
    - feed the @animal
    - pet the @animal
```
This defines an entity ```@animal``` which will recognize the animal tokens.  ```@myentity``` will 
match ```walk the dog```, because it sees that ```@animal`` aligns.  Because it matched on an 
entity it will capture the entity value for animal.

## Variations
Just like regular expressions you can define variations inline by putting parens around it and using the
pipe operator like ```(token|token|token)```

These are identical definitions:

```yaml
  - name: '@animal1'
    patterns:
    - dog
    - cat
    - snake
  - name: '@animal2'
    patterns:
    - (dog|cat|snake)
```

## Ordinality
Just like in regular expressions you can define ordinality on a variation. 

| postfix | description        |
|---------|--------------------|
| ?       | match zero or one  |
| *       | match zero or more |
| +       | match one or more  |

```yaml
  - name: '@example'
    patterns:
    - (walk|feed|pet) (the)? (dog|cat|snake)+
```
With variations, this one pattern does all of the combinations of action and animal.

## Fuzzy Matching
Matching on tokens is powerful but brittle because people spell poorly, or speech
rec provides a word which sounds right, but is wrong word. When fuzzy matching is used
we will match using techniques such as phonetic matching. 

There are 2 ways to enable fuzzy matching. 
1. You can use the ~ postfix to enable fuzzy matching for a token or group of tokens
2. you can use the ```fuzzyMatch``` property on an entity definition

## Wildcards

# LucE file format

A luce file (**xxx.luce.yaml**) has 4 top level properties:

| name                                  | description                 |
|---------------------------------------|-----------------------------|
| [locale](#locale)                     | the locale for tokenization |
| [entities](#entities)                 | entity definitions          |
| [macros](#macros)                     | macro definitions           |
| [externalEntities](#externalEntities) | external entity definitions |

## **locale** property
The **locale** property defines the language tokenizer to use. 
> The **locale** property is optional.
>
> The default value is 'en'

Example:
```yaml
locale: en
```


## **entities** property
An **entity** definition defines a named set of patterns.

### Entity Defintion 
The **entities** property is made up of 1 or more entity definitions.

An **entity** definition is a object with 3 properties:

| name       | description                                                               |
|------------|---------------------------------------------------------------------------|
| name       | the name of the entity                                                    |
| fuzzyMatch | sets the default fuzzyMatch for all tokens in the patterns of this entity |
| patterns   | is array of pattern strings (see [Patterns](#Patterns) )                  |

> the name does not have to start with '@', but by convention it makes the file easier to read.

Example:
```yaml
entities:
  - name: '@colors'
    patterns: [red, green, blue, yellow, orange, indigo, violet]
    
  - name: '@drinkSizes'
    patterns:
    - small
    - medium
    - large
    - extra large
```
## **macros** property
The **macros** property lets you define a named fragment of text that will be substituted 
into any pattern.
> The **macros** property is optional.
>
> Macro names must start with '$'

```yaml
macros:
  $foo: (x|y|z)
entities:
  - name: '@test'
    patterns:
    - I want $foo to match.
```

## **externalEntities** property
You can reference external entities (*entities which are detected externally and passed in*)
as part of a pattern just like any other entity.

Adding an external entity reference to this property will suppress warnings that you are referencing
an entity which is not defined in the .yaml file.

```yaml
externalEntities: ['@foo','@bar']
```
