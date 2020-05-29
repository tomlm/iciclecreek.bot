![icon](icon.png)

# ComponentSchema
This command line tool uses reflection to generate .schema files for your custom actions.

## Installation
To install:

```dotnet tool install --global componentschema```


## Usage
```
ComponentSchema - Use reflection to generate .schema files for Bot Framework SDK/Composer

Usage:
ComponentSchema assembly [-o folder] [-registration]
     assembly => path to .dll
     folder => path to folder to generate .schema files
     registration => generate ComponentRegistration.cs file

All dialog classes with a public const string Kind constant will be output as [kind].schema files.
```

Add annotations to your class and properties.

General Attributes:
| Attribute                                      | Description                                                          |
|------------------------------------------------|----------------------------------------------------------------------|
| **[DisplayName(title)]**                       | Define title for property (default is property name)                 |
| **[Description(description)]**                 | define description for property (Default is property name humanized) |
| **[Required]**                                 | Mark that the property is required                                   |
| **[DefaultValue(defaultValue)]**               | Define a default value for the property                              |
| **[MinLength(minLength)]**                     | Set the min length for the property                                  |
| **[MaxLength(maxLength)]**                     | set the max length for the property                                  |
| **[Range(minValue,maxValue)]**                 | Set min, max range for the property                                  |

String attributes:
| Attribute                   | Description                                                       |
|-----------------------------|-------------------------------------------------------------------|
| **[StringLength(min,max)]** | Set min, max length of string (can also use MinLength, MaxLength) |
| **[PhoneNumber]**           | String is a phone number pattern                                  |
| **[EmailAddress]**          | string is a email address pattern                                 |
| **[Url]**                   | string is a url pattern                                           |
| **[RegularExpression()]**   | string is a custom regular expression pattern                     |
| **[DataType]**              | string is a given data type (subset for strings)                  |
| **[EnumDataType]**          | string has well known values for auto-complete                    |


## Output
Each class will have a .schema file generated with name of the value of Kind.
 
## Annotations
In your project add System.Data.Annotations package

```dotnet add package System.Data.Annotations```

This tool will generate .schema files for all public dialogs with a public const Kind.

```csharp
    [Description("This is an example")]
    public class Example : Dialog
    {
        public const string Kind = "Test.Example";

        [Description("The quoted text")]
        [RegularExpression("???-???-????")]
        public string QuotedText { get; set; }

        [Description("The amount")]
        public float Amount { get; set; }

        [Description("This is a string which consumes dates")]
        [DataType(DataType.Date)]
        public string SomeDate { get; set; }

        [Description("This is a phone number")]
        [PhoneNumber]
        public string Phone { get; set; }
        
        ...
    }
```

