![icon](../../../icon.png)

# Javascript Functions for AdaptiveExpression
This library provides the ability to call javascript functions from AdaptiveExpressions
* [AdaptiveExpressions](https://github.com/botbuilder-dotnet/libaries/adaptiveExpressions)

## Installation
To install 
```dotnet add package Iciclecreek.AdaptiveExpressions.Javascript```

## Define functions
Given a javascript with functions in it: 

*Example: myfunctions.js*
```
// Export functions
let exports = {};
exports.add2Numbers = add2Numbers;

function add2Numbers(x , y)
{
    return x + y;
}
```

### (Option 1) Load functions into adaptive expressions.
To Load it, simply read the text file and call RegisterFunction with namespace.
``` 
var javascript = File.ReadAllText("myfunctions.js");
JavasacriptFunctions.RegisterFunction("contoso", javascript);
```

### (Option 2) Load functions with Bot Framework Resource Explorer 
If you call Register with a ResourceExplorer then all files of extension ".function.js" will
be automatically mounted with the file name (minus .function.js) will be used as the namespace.
ResourceExplorer change detection will reload the functions if the file changes.

*In startup*
```
    ResourceExplorer resourceExplorer = new ResourceExplorer(...) ... ;
    JavascriptFunctions.AddJavascriptFunctions(resourceExplorer);
``` 

> NOTE: As of 4.12 you do not need to modify your startup if you are using the new runtime component
> It will automatically be registered in resourceExplorer mode.

## To call a function that has been added
Every top level function in myfunctions.js will be mounted in the given namespace (the default namespace will be filename if you are using resource explorer). To call 
a function you simply use the namespace+function name with args.


*Example Expression calling function registered with **RegisterFunction** and a namespace 'contoso'*

```contoso.Add2Numbers(user.age, 7)```

*Example Expression defined as resource file **myfunctions.js** * 

```myfunctions.Add2Numbers(user.age, 7)```


*Example Expression parsing from C#*

```var expression = Expression.Parse("contoso.Add2Numbers(user.age, 7)");```


# Internal Details
This project uses **Jint** javascript interpreter to execute the javascript.

This engine does not have access to the file system, or to the network.  Each file is loaded into it's own interpreter, so there is 
no shared execution environment between javascript files.
