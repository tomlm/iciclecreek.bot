using AdaptiveExpressions.Properties;
using Humanizer;
using Microsoft.Bot.Builder.Dialogs;
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
                Console.WriteLine("ComponentSchema - Use reflection to generate .schema files for Bot Framework SDK/Composer");
                Console.WriteLine();
                Console.WriteLine("Usage:");
                Console.WriteLine("ComponentSchema assembly [-o folder] [-registration]");
                Console.WriteLine("     assembly => path to .dll");
                Console.WriteLine("     folder => path to folder to generate .schema files");
                Console.WriteLine("     registration => generate ComponentRegistration.cs file");
                Console.WriteLine();
                Console.WriteLine("All dialog classes with a public const string Kind constant will be output as [kind].schema files.");
                Console.WriteLine();
                Console.WriteLine("Add annotations to your class and properties:");
                Console.WriteLine("     [DisplayName(\"title\")]");
                Console.WriteLine("     [Description(\"description\")]");
                Console.WriteLine("     [Required]");
                Console.WriteLine("     [DefaultValue(defaultValue)]");
                Console.WriteLine("     [StringLength(min,max)]");
                Console.WriteLine("     [MinLength(minLength)]");
                Console.WriteLine("     [MaxLength(maxLength)]");
                Console.WriteLine("     [Range(minValue, maxValue)]");
                Console.WriteLine("NOTE: add nuget package System.Data.Annotations to get attributes for annotations");
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

                            dynamic propDef = new JObject();
                            switch (property.PropertyType.Name)
                            {
                                case "Boolean":
                                case "bool":
                                    propDef.type = "boolean";
                                    break;
                                case "SByte":
                                case "Int16":
                                case "Int32":
                                case "Int64":
                                case "Byte":
                                case "UInt16":
                                case "UInt32":
                                case "UInt64":
                                    propDef.type = "integer";
                                    break;

                                case "float":
                                case "double":
                                    propDef.type = "number";
                                    break;

                                case "String":
                                case "string":
                                    propDef.type = "string";
                                    break;
                                case "ObjectExpression`1":
                                    propDef["$ref"] = "schema:#/definitions/objectExprssion";
                                    break;
                                case "BoolExpression":
                                    propDef["$ref"] = "schema:#/definitions/booleanExpression";
                                    break;
                                case "StringExpression":
                                    propDef["$ref"] = "schema:#/definitions/stringExpression";
                                    break;
                                case "ValueExpression":
                                    propDef["$ref"] = "schema:#/definitions/valueExpression";
                                    break;
                                case "NumberExpresion":
                                    propDef["$ref"] = "schema:#/definitions/numberExpression";
                                    break;
                                case "DateTime":
                                    propDef["$ref"] = "schema:#/definitions/dateTimeExpression";
                                    break;
                                case "IntegerExpression":
                                    propDef["$ref"] = "schema:#/definitions/IntegerExpression";
                                    break;
                                case "ObjectExpression<object>":
                                    propDef["$ref"] = "schema:#/definitions/objectExpression";
                                    break;
                                default:
                                    Console.WriteLine($"Unknown type {property.PropertyType.Name}");
                                    break;
                            }

                            var propName = property.Name;
                            var jsonPropAttr = property.GetCustomAttribute<JsonPropertyAttribute>();
                            if (jsonPropAttr != null)
                            {
                                propName = jsonPropAttr.PropertyName;
                            }

                            propDef.title = GetDisplayName(property, property.Name.Humanize());
                            propDef.description = GetDescription(property, property.Name.Humanize());
                            AddValidations(propDef, property);

                            if (GetRequired(property))
                            {
                                definition.required.Add(propName);
                            }

                            definition.properties[propName] = propDef;
                        }

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

    }

}

