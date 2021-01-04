# Iciclecreek Bot Templates
This is a package which adds new templates to dotnet tempalte libraries. 

## Installation
```
dotnet new -i Iciclecreek.Bot.Templates
```

## Bot Templates
Templates for creating bots.

### LucyBot Template
Creates a Azure Function bot with Lucy Recognizer configured in the default bot.dialog.

```
dotnet new lucybot --name {botName}
```

## Dialog Item Templates
Item templates for creating dialogs.

### LucyDialog Template
Creates a adaptive dialog configured with LucyRecognizer.

In the Dialogs folder:

```
dotnet new lucydialog --name {dialogname}
```
