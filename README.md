# Iciclecreek Bot Framework Components
This repo contains source code for a number of components for Bot Framework related projects.

# Libraries

## Iciclecreek.AdaptiveExpression.Humanizer
 Library which extends AdaptiveExpressions with [Humanizer.Net](https://humanizr.net/) functions.

 Examples of capabilities:
 
* ``` humanizer.humanize(date) ``` => "yesterday" ```
* ``` humanizer.humanize(date) ``` => "in 39 minutes" ```

 [More Details](/tomlm/iciclecreek.bot/libraries/Iciclecreek.AdaptiveExpressions.Humanizer/readme.md) 

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

[More Details](/tomlm/iciclecreek.bot/source/libraries/Iciclecreek.AdaptiveExpressions.Javascript/readme.md) 

