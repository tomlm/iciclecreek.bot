![icon](../../../icon.png)

# Python Functions for AdaptiveExpression
This library provides the ability to call python functions from AdaptiveExpressions
* [AdaptiveExpressions](https://github.com/botbuilder-dotnet/libaries/adaptiveExpressions)

## Installation
To install 
```dotnet add package Iciclecreek.AdaptiveExpressions.Python```

## Define functions
Given a python with functions in it: 

*Example: myfunctions.py*
```
def add2Numbers(x , y):
    return x + y;
```

### (Option 1) Load functions into adaptive expressions.
To Load it, simply read the text file and call RegisterFunction with namespace.
``` 
var python = File.ReadAllText("myfunctions.py");
PythonFunctions.RegisterFunction("contoso", python);
```

### (Option 2) Load functions with Bot Framework Resource Explorer
If you call Register with a ResourceExplorer then all files of extension ".function.py" will
be automatically mounted with the file name (minus .function.py) will be used as the namespace.
ResourceExplorer change detection will reload the functions if the file changes.

> NOTE: As of 4.12 with new Runtime Integration you do NOT need to modify startup, it automatically
> registers .py files 

*In startup*
```
    ResourceExplorer resourceExplorer = new ResourceExplorer(...) ... ;
    PythonFunctions.AddPythonFunctions(resourceExplorer);
``` 

## To call a function that has been added
Every top level function in myfunctions.py will be mounted in the given namespace. To call 
a function you simply use the namespace+function name with args.


*Example Expression calling function added via AddPythonFunctions*

```contoso.Add2Numbers(user.age, 7)```

*Example Expression calling function defined in a myfunctions.py*

```myfunctions.Add2Numbers(user.age, 7)```


*Example Expression parsing from C#*

```var expression = Expression.Parse("contoso.Add2Numbers(user.age, 7)");```


# Internal Details
This project uses **IronPython** to execute the python 2.7.
