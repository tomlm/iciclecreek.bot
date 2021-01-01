# LucyRecognizer
[Lucy](https://github.com/tomlm/lucy) is an Entity Recognition engine which defines a simple syntax for recognizing entities in text.  
See 
* [Lucy Documentation ](https://github.com/tomlm/lucy/help.md) 
* [LucyPad2](https://lucypad2.azurewebsites.net) - online editor for working with Lucy models.

# Usage 

# Add nuget package
Add ```Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy``` nuget reference to your project.

# Register
Register the classes in startup.cs
```
            ComponentRegistration.Add(new LucyComponentRegistration());
```

# Defining a model 
This package adds a new recognizer kind **Iciclecreek.LucyRecognizer**.  

Declaratively in .dialog files you can define the Lucy model in 3 ways:

## Define as JSON inline to the .dialog file
```json
"recognizer": {
    "$kind": "Iciclecreek.LucyRecognizer",
    "model": {
        "locale": "en",
        "entities": [
            {
                "name": "colors",
                "patterns": [
                    [ "red", "rojo" ],
                    "green",
                    "blue",
                    "yellow",
                    "purple",
                    "white",
                    "orange"
                ]
            },
            ...
        ]
    }
},
```

## Storing model in a seperate a resource 
Or you can put the model in a .json resource such as **example.json** 

```json
"recognizer": {
    "$kind": "Iciclecreek.LucyRecognizer",
    "resourceId": "example.json"
},
```

Or in a .yaml resource such as **example.yaml**. 
```json
"recognizer": {
    "$kind": "Iciclecreek.LucyRecognizer",
    "resourceId": "example.yaml"
},
```

## Intents
The default is that if any entities are found then an intent of "Matched" is returned.

You can promote any named entity to an intent by adding it to the the **intents** array.

```json
"recognizer": {
    "$kind": "Iciclecreek.LucyRecognizer",
    "resourceId": "example.yaml",
    "intents": [ "drinkOrder",... ] 
},
```
