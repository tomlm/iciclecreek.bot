# Bot Component Library
A Bot Component Library adds additional assets to a bot project.

## Creating Custom Actions
To create a custom action you simply create a dialog and register it's kind => type with the system.

## Creating dialog
To be a custom action it MUST have:
* derive from Dialog
* define kind constant with a namespace prefixed value.

To be declarative it must have
* properties which can be bound to JSON.
* It must have parameterless constructor.

Best practices are:
* It should have a default parameterless constructor which captures caller information for debugging purposes.
* It should have JsonProperty attributes to serialize properties camelcase.
* It should define properties using Adaptive Expression Properties (StringExpression, BoolExpression, etc)

Example: 
```CSharp
    public class CustomAction : Dialog
    {
        [JsonProperty("$kind")]
        public const string Kind = "Contoso.CustomAction";

        [JsonConstructor]
        public CustomAction([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        [JsonProperty("name")]
        public StringExpression Name {get;set;}
```

## .schema file
If you want your custom action to be declarative and to light up in composer correctly then each action needs a .schema file
The .Schema file describes the shape of the JSON and drives the UI to create a default editing experience for your object.

The nameing convention is that the value of $kind is the name of the file, so Contoso.CustomAction.schema contains the declartive definition
of the class bound to $kind = "Contoso.CustomAction".

## ComponentRegistration class
The final step is to create a ComponentRegistration class, which registers your objects into the kind table, and provides
custom converters for converting templated objects.  This gets registered in the startup.cs of the host project to expose
the functionality which is in the library.

```csharp
ComponentRegistration.Add(new ContosoComponentRegistration());
```

## bf dialog:merge
When you add a new component library to a bot project, you need to run the **bf dialog:merge** tool to merge in the new component's
.schema files with the rest of the .schema files. The merged file is the file that the Composer UI uses for editing dialog that 
have your component in them.


## .csproj file
.schema files need to be included in the package the right way, make sure this is in your .csproj

```xml
    <PropertyGroup>
        <ContentTargetFolders>content</ContentTargetFolders>
    </PropertyGroup>

    <ItemGroup>
        <Content Include="**/*.dialog" />
        <Content Include="**/*.lg" />
        <Content Include="**/*.lu" />
        <Content Include="**/*.schema" />
        <Content Include="**/*.qna" />
    </ItemGroup>
```
