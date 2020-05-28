![icon](icon.png)

# Overview
This library provides custom attributes for annotating your Dialog class with metadata that allows the 
[ComponentSchema](https://github.com/tomlm/iciclecreek.bot/tree/master/source/Tools/ComponentSchema) tool to automatically 
generate .schema files from your classes.

## Installation
To install into your project run the cli:

```shell
dotnet add package Iciclecreek.Bot.Builder.Dialogs.Annotations
```

## Usage
This library provides the EntityAttribute that can be applied to properties to desribe entities that can be mapped to the property.
(aka Slot Filling)

The first arg is the @entity to slot fill.

The rest of the args are optional example utterances which are used to prime .lu recognition of that entity type.  The attribute can be applied multiple times to enable you to describe that multiple entity types can be mapped to this property.

```csharp
[Entity("firstName", "Joe","Frank")]
[Entity("lastName", "Smith","Lee")]
[Entity("name", "Joe Smith","Frank Lee")]
public string Name {get;set;}
```



