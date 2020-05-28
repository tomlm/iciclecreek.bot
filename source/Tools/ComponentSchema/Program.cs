using AdaptiveExpressions.Properties;
using Humanizer;
using Iciclecreek.Bot.Builder.Dialogs.Annotations;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;

namespace Iciclecreek.Bot
{
    class Program
    {
        private static JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("ComponentSchema");
                Console.WriteLine("Use reflection to generate .schema files for Bot Framework SDK/Composer");
                Console.WriteLine();
                Console.WriteLine("Usage:");
                Console.WriteLine("ComponentSchema assembly [-o folder] [-registration]");
                Console.WriteLine("     assembly => path to .dll");
                Console.WriteLine("     folder => path to folder to generate .schema files");
                Console.WriteLine("     registration => generate ComponentRegistration.cs file");
                Console.WriteLine();
                Console.WriteLine("All dialog classes with a `public const string Kind` constant will be output as [kind].schema files.");
                Console.WriteLine();
                Console.WriteLine("NOTE: add nuget packages");
                Console.WriteLine("* System.Data.Annotations - to get basic attributes");
                Console.WriteLine("* Iciclecreek.Bot.Builder.Dialogs.Annotations - to get Entity() attribute");
                Console.WriteLine();
                Console.WriteLine("Add annotations to your class and properties.");
                Console.WriteLine("General Attributes:");
                Console.WriteLine("     [DisplayName(\"title\")]");
                Console.WriteLine("     [Description(\"description\")]");
                Console.WriteLine("     [Entity(entityName, example1, example2, example3, ...)]");
                Console.WriteLine("     [Required]");
                Console.WriteLine("     [DefaultValue(defaultValue)]");
                Console.WriteLine("     [MinLength(minLength)]");
                Console.WriteLine("     [MaxLength(maxLength)]");
                Console.WriteLine("     [Range(minValue, maxValue)]");
                Console.WriteLine();
                Console.WriteLine("String attributes:");
                Console.WriteLine("     [StringLength(min,max)]");
                Console.WriteLine("     [PhoneNumber]");
                Console.WriteLine("     [EmailAddress]");
                Console.WriteLine("     [Url]");
                Console.WriteLine("     [RegularExpression()]");
                Console.WriteLine("     [DataType]");
                Console.WriteLine("     [EnumDataType]");
                return;
            }

            Console.WriteLine($"Generating schema for {args[0]}");

            var generateRegistration = args.Any(a => a.TrimStart('-') == "registration");
            var outputFolder = Environment.CurrentDirectory;
            if (args.Length >= 3 && args[1] == "-o")
            {
                outputFolder = args[2];
            }

            var assembly = Assembly.LoadFrom(args[0]);
            var ns = assembly.GetName().Name;

            Dictionary<string, Type> kinds = new Dictionary<string, Type>();
            foreach (var type in assembly.ExportedTypes)
            {
                // if it derives from dialog
                if (typeof(Dialog).IsAssignableFrom(type))
                {
                    Dictionary<string, List<string>> examples = new Dictionary<string, List<string>>();

                    // if it has a kind property
                    var kindProperty = type.GetField("Kind");
                    string kind;
                    dynamic definition = new JObject();
                    if (kindProperty != null)
                    {
                        kind = (string)kindProperty.GetRawConstantValue();
                        kinds.Add(kind, type);

                        definition["$schema"] = "https://raw.githubusercontent.com/microsoft/botframework-sdk/master/schemas/component/component.schema";
                        definition["$role"] = "implements(Microsoft.IDialog)";
                        definition.title = GetDisplayName(type, type.Name);
                        definition.description = GetDescription(type, type.Name.Humanize());
                        definition.type = "object";
                        definition.required = new JArray();
                        definition.properties = new JObject();

                        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        {
                            if (property.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                            {
                                // ignore
                                continue;
                            }

                            var propName = property.Name;
                            var jsonPropAttr = property.GetCustomAttribute<JsonPropertyAttribute>();
                            if (jsonPropAttr != null)
                            {
                                propName = jsonPropAttr.PropertyName;
                            }

                            dynamic propDef = new JObject();
                            propDef.title = GetDisplayName(property, property.Name.Humanize());
                            propDef.description = GetDescription(property, property.Name.Humanize());

                            switch (property.PropertyType.Name)
                            {
                                case "Boolean":
                                case "Bool":
                                case "bool":
                                    propDef.type = "boolean";
                                    break;

                                case "SByte":
                                case "Byte":
                                case "Int16":
                                case "UInt16":
                                case "Int32":
                                case "UInt32":
                                case "Int64":
                                case "UInt64":
                                    propDef.type = "integer";
                                    break;

                                case "Single":
                                case "Double":
                                case "float":
                                case "double":
                                    propDef.type = "number";
                                    break;

                                case "String":
                                case "string":
                                    propDef.type = "string";
                                    AddStringDataTypes(propDef, property);
                                    break;

                                case "DateTime":
                                    propDef.type = "string";
                                    propDef.format = "date-time";
                                    break;

                                case "TimeSpan":
                                    propDef.type = "string";
                                    propDef.format = "time";
                                    break;

                                case "BoolExpression":
                                    propDef["$ref"] = "schema:#/definitions/booleanExpression";
                                    break;

                                case "StringExpression":
                                    propDef["$ref"] = "schema:#/definitions/stringExpression";
                                    AddStringDataTypes(propDef, property);
                                    break;

                                case "NumberExpresion":
                                    propDef["$ref"] = "schema:#/definitions/numberExpression";
                                    break;

                                case "DateTimeExpression":
                                    propDef["$ref"] = "schema:#/definitions/dateTimeExpression";
                                    break;

                                case "IntegerExpression":
                                    propDef["$ref"] = "schema:#/definitions/IntegerExpression";
                                    break;

                                case "ValueExpression":
                                    propDef["$ref"] = "schema:#/definitions/valueExpression";
                                    break;

                                case "ObjectExpression`1":
                                    propDef["$ref"] = "schema:#/definitions/objectExpression";
                                    break;

                                case "EnumExpression`1":
                                    propDef.oneOf = new JArray();
                                    dynamic options = new JObject();
                                    options.title = propDef.title;
                                    options.description = propDef.description;
                                    options["enum"] = new JArray(property.PropertyType.GetGenericArguments()[0].GetEnumNames());
                                    propDef.oneOf.Add(options);
                                    options = new JObject();
                                    options["$ref"] = "schema:#/definitions/equalsExpression";
                                    propDef.oneOf.Add(options);
                                    break;

                                default:
                                    if (property.PropertyType.IsEnum)
                                    {
                                        propDef.type = "string";
                                        propDef["enum"] = new JArray(property.PropertyType.GetEnumNames());
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Unknown type {property.PropertyType.Name}");
                                    }
                                    break;
                            }

                            AddValidations(propDef, property);

                            AddEntities(propDef, property, examples);

                            if (GetRequired(property))
                            {
                                definition.required.Add(propName);
                            }

                            definition.properties[propName] = propDef;
                        }

                        if (examples.Any())
                        {
                            definition["$examples"] = JObject.FromObject(examples);
                        }

                        Console.WriteLine($"{kind}.schema");

                        var schemaFile = Path.Combine(outputFolder, $"{kind}.schema");
                        Console.WriteLine(schemaFile);
                        File.WriteAllText(schemaFile, JsonConvert.SerializeObject(definition, settings));
                    }
                }

            }

            if (generateRegistration && kinds.Any())
            {
                // Write ComponentRegistration
                var prefix = kinds.First().Key;
                prefix = prefix.Substring(0, prefix.LastIndexOf('.')).Replace(".", "");

                WriteComponentRegistration(outputFolder, ns, prefix, kinds);
            }
        }

        private static void WriteComponentRegistration(string outputFolder, String ns, string prefix, Dictionary<String, Type> kinds)
        {
            var filePath = Path.Combine(outputFolder, $"{prefix}ComponentRegistration.cs");
            Console.WriteLine(filePath);
            using (var stream = File.OpenWrite(filePath))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine("using AdaptiveExpressions.Converters;");
                    writer.WriteLine("using Microsoft.Bot.Builder;");
                    writer.WriteLine("using Microsoft.Bot.Builder.Dialogs.Debugging;");
                    writer.WriteLine("using Microsoft.Bot.Builder.Dialogs.Declarative;");
                    writer.WriteLine("using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;");
                    writer.WriteLine("using Newtonsoft.Json;");
                    writer.WriteLine("using System;");
                    writer.WriteLine("using System.Collections.Generic;");
                    writer.WriteLine("using System.Text;");
                    writer.WriteLine();
                    writer.WriteLine($"namespace {ns}");
                    writer.WriteLine("{");
                    writer.WriteLine($"    public class {prefix}ComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes");
                    writer.WriteLine("    {");
                    writer.WriteLine("        public IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)");
                    writer.WriteLine("        {");
                    foreach (var kv in kinds)
                    {
                        writer.WriteLine($"            yield return new DeclarativeType<{kv.Value.Name}>({kv.Value.Name}.Kind);");
                    }
                    writer.WriteLine("        }");
                    writer.WriteLine("    }");
                    writer.WriteLine("}");
                }
            }
        }

        private static string GetDisplayName(Type type, string def)
        {
            string value = def;
            var attr = type.GetCustomAttribute<DisplayNameAttribute>();
            if (attr != null)
            {
                value = attr.DisplayName;
            }
            return value;
        }


        private static string GetDescription(Type type, string def)
        {
            string value = def;
            var attr = type.GetCustomAttribute<DescriptionAttribute>();
            if (attr != null)
            {
                value = attr.Description;
            }
            return value;
        }

        private static string GetDisplayName(PropertyInfo property, string def)
        {
            string value = def;
            var attr = property.GetCustomAttribute<DisplayNameAttribute>();
            if (attr != null)
            {
                value = attr.DisplayName;
            }
            return value;
        }

        private static string GetDescription(PropertyInfo property, string def)
        {
            string value = def;
            var attr = property.GetCustomAttribute<DescriptionAttribute>();
            if (attr != null)
            {
                value = attr.Description;
            }
            return value;
        }

        private static bool GetRequired(PropertyInfo property)
        {
            bool value = false;
            var attr = property.GetCustomAttribute<RequiredAttribute>();
            if (attr != null)
            {
                value = true;
            }
            return value;
        }

        private static void AddValidations(dynamic propDef, PropertyInfo property)
        {
            var lenAttr = property.GetCustomAttribute<StringLengthAttribute>();
            if (lenAttr != null)
            {
                propDef.minLength = lenAttr.MinimumLength;
                propDef.maxLength = lenAttr.MaximumLength;
            }

            var minLengthAttr = property.GetCustomAttribute<MinLengthAttribute>();
            if (minLengthAttr != null)
            {
                propDef.minLength = minLengthAttr.Length;
            }

            var maxLengthAttr = property.GetCustomAttribute<MaxLengthAttribute>();
            if (maxLengthAttr != null)
            {
                propDef.maxLength = maxLengthAttr.Length;
            }

            var rangeAttr = property.GetCustomAttribute<RangeAttribute>();
            if (rangeAttr != null)
            {
                propDef.minimum = rangeAttr.Minimum;
                propDef.maximum = rangeAttr.Maximum;
            }

            var defaultAttr = property.GetCustomAttribute<DefaultValueAttribute>();
            if (defaultAttr != null)
            {
                propDef["default"] = JToken.FromObject(defaultAttr.Value);
            }
        }

        private static void AddStringDataTypes(dynamic propDef, PropertyInfo property)
        {
            propDef.type = "string";
            if (property.GetCustomAttribute<PhoneAttribute>() != null)
            {
                propDef.format = "phone";
            }
            else if (property.GetCustomAttribute<EmailAddressAttribute>() != null)
            {
                propDef.format = "email";
            }
            else if (property.GetCustomAttribute<UrlAttribute>() != null)
            {
                propDef.format = "uri";
            }
            else if (property.GetCustomAttribute<DataTypeAttribute>() != null)
            {
                var attr = property.GetCustomAttribute<DataTypeAttribute>();
                switch (attr.DataType)
                {
                    case DataType.Url:
                        propDef.format = "uri";
                        break;
                    case DataType.EmailAddress:
                        propDef.format = "email";
                        break;
                    case DataType.PhoneNumber:
                        propDef.format = "phone";
                        break;
                    case DataType.Date:
                        propDef.format = "date";
                        break;
                    case DataType.Time:
                        propDef.format = "time";
                        break;
                    case DataType.DateTime:
                        propDef.format = "date-time";
                        break;
                }
            }
            else if (property.GetCustomAttribute<EnumDataTypeAttribute>() != null)
            {
                var attr = property.GetCustomAttribute<EnumDataTypeAttribute>();
                propDef["enum"] = new JArray(attr.EnumType.GetEnumNames());
            }

            if (property.GetCustomAttribute<RegularExpressionAttribute>() != null)
            {
                propDef.pattern = property.GetCustomAttribute<RegularExpressionAttribute>().Pattern;
            }
        }

        private static void AddEntities(dynamic propDef, PropertyInfo property, Dictionary<string, List<string>> entityExamples)
        {
            var attributes = property.GetCustomAttributes<EntityAttribute>();
            if (attributes.Any())
            {
                var entities = new List<string>();
                foreach (var entityAttr in attributes)
                {
                    var entityName = entityAttr.Entity.TrimStart('@').Trim();
                    entities.Add(entityName);
                    if (entityAttr.Examples != null && entityAttr.Examples.Any())
                    {
                        if (!entityExamples.TryGetValue(entityName, out List<string> examples))
                        {
                            examples = new List<string>();
                            entityExamples[entityName] = examples;
                        }

                        examples.AddRange(entityAttr.Examples);
                    }
                }

                propDef["$entities"] = JArray.FromObject(entities.Distinct().ToList());
            }
        }
    }
}