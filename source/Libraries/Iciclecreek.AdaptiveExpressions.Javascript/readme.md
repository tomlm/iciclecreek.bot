# Javascript Functions for AdaptiveExpression
This library provides the ability to call javascript functions from AdaptiveExpressions
* [AdaptiveExpressions](https://github.com/botbuilder-dotnet/libaries/adaptiveExpressions)

## Installation
To install 
```dotnet install Iciclecreek.AdaptiveExpressions.Javascript```

## Define functions
Given a javascript with functions in it: 

*Example: myfunctions.js*
```
function Add2Numbers(x , y)
{
    return x + y;
}
```

## Load functions into adaptive expressions.
To Load it, simply read the text file and call RegisterFunction with namespace.
``` 
var javascript = File.ReadAllText("myfunctions.js");
JavasacriptFunctions.RegisterFunction("contoso", javascript);
```

## Load functions with Bot Framework Resource Explorer
If you call Register with a ResourceExplorer then all files of extension ".function.js" will
be automatically mounted with the file name (minus .function.js) will be used as the namespace.
ResourceExplorer change detection will reload the functions if the file changes.

*In startup*
```
    ResourceExplorer resourceExplorer = new ResourceExplorer(...) ... ;
    JavascriptFunctions.Register(resourceExplorer);
``` 

## To call a function that has been added
Every top level function in myfunctions.js will be mounted in the contoso namespace. Use
an expression you simply call it

As expression
```contoso.Add2Numbers(user.age, 7)``

Example parsing from C#
```var expression = Expression.Parse("contoso.Add2Numbers(user.age, 7)");``

