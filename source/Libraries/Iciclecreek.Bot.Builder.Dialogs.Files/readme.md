![icon](icon.png)

# File Actions
This library provides custom actions for Bot Framework/Composer to do read/write/delete files in the file system;

## Installation
To install into your project run the cli:

```dotnet add package Iciclecreek.Bot.Builder.Dialogs.Files```

To add to your schema for usage in Bot Framework Composer from cli:

```bf dialog:merge -p yourproj.proj -o your.schema```

## Library
This library adds actions for File System.

### Actions
| Action                                | Description       |
|---------------------------------------|-------------------|
| **Iciclecreek.CreatTextFile** | create a file|
| **Iciclecreek.WriteTextFile** | create a text file|
| **Iciclecreek.Delete** | delete file|

