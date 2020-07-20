![icon](icon.png)

# Iciclecreek Bot Framework Components
This repo contains source code for a number of components for Bot Framework related projects.

# Dotnet Tools

## ComponentSchema
CLI which generates Bot Framework SDK/Composer schema files from your code with reflection.
[More Details](source/Tools/ComponentSchema/)


# Nuget Libraries

## Iciclecreek.Bot.Builder.Dialogs.Database.AzureStorage
Library which adds components to Bot Framework and Bot Composer for doing CRUD operations against Azure Storage Tables.
[More Details](source/Libraries/Iciclecreek.Bot.Builder.Dialogs.Database.AzureStorage/)

## Iciclecreek.Bot.Builder.Dialogs.Database.Cosmos
Library which adds components to Bot Framework and Bot Composer for doing CRUD operations against Azure CosmosDB document store.
[More Details](source/Libraries/Iciclecreek.Bot.Builder.Dialogs.Database.Cosmos/)

## Iciclecreek.Bot.Builder.Dialogs.Database.SqlClient
Library which adds components to Bot Framework and Bot Composer for executing SQL statements using Microsoft.Data.SqlClient library.
[More Details](source/Libraries/Iciclecreek.Bot.Builder.Dialogs.Database.SqlClient/)

## Iciclecreek.Bot.Builder.Dialogs.Recognizers
Library which adds components to Bot Framework and Bot Composer for custom recognizers.
* **PersonNameEntityRecognizer** - Recognizes common @givenName @surname, @fullname entities (like "John Smith"=> GivenName:"john" Surname:"Smith", fullname:"John Smith")
* **QuotedTextEntityRecognizer** - Recognizes quoted strings as @QuotedText entities
* **CsvTextEntityRecognizer** - Uses a .CSV file to define tokens to match to entities
[More Details](source/Libraries/Iciclecreek.Bot.Builder.Dialogs.Recognizers/)


## Iciclecreek.AdaptiveExpression.Humanizer
 Library which extends AdaptiveExpressions with [Humanizer.Net](https://humanizr.net/) functions.

 Examples of capabilities:
 
* ``` humanizer.humanize(date)``` => ```"yesterday" ```
* ``` humanizer.humanize(date)``` => ```"in 39 minutes" ```

 [More Details](source/Libraries/Iciclecreek.AdaptiveExpressions.Humanizer/)

## Iciclecreek.AdaptiveExpression.Javascript
Library which extends AdaptiveExpressions with the ability to define custom functions via .js files.

Given a javascript with function:

*Example: contoso.js*
```
function Add2Numbers(x , y)
{
    return x + y;
}
```

You can call it from adaptive expressions
 ``` contoso.Add2Numbers(13, user.age) ``` => 52

[More Details](source/Libraries/Iciclecreek.AdaptiveExpressions.Javascript)

## Iciclecreek.AdaptiveExpression.Python
Library which extends AdaptiveExpressions with the ability to define custom functions via .py files.

Given python with function:

*Example: contoso.py*
```
def Add2Numbers(x , y):
    return x + y
```

You can call it from adaptive expressions
 ``` contoso.Add2Numbers(13, user.age) ``` => 52

[More Details](source/Libraries/Iciclecreek.AdaptiveExpressions.Python)

