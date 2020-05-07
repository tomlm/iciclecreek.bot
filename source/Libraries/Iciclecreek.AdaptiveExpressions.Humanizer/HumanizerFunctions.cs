using AdaptiveExpressions;
using Antlr4.Runtime.Misc;
using Humanizer;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;

namespace Iciclecreek.Bot.Expressions.Humanizer
{
    public static class HumanizerFunctions
    {
        public static void Register()
        {
            // apply case functions
            Expression.Functions.Add($"humanizer.allCaps", (args) => CasingExtensions.ApplyCase(args[0], LetterCasing.AllCaps));
            Expression.Functions.Add($"humanizer.lowerCase", (args) => CasingExtensions.ApplyCase(args[0], LetterCasing.LowerCase));
            Expression.Functions.Add($"humanizer.sentence", (args) => CasingExtensions.ApplyCase(args[0], LetterCasing.Sentence));
            Expression.Functions.Add($"humanizer.title", (args) => CasingExtensions.ApplyCase(args[0], LetterCasing.Title));

            // humanize collections
            Expression.Functions.Add($"humanizer.humanizeList", (args) =>
            {
                CollectionHumanizeExtensions.Humanize<dynamic>(args[0], args[1]);
                return string.Empty;
            });

            // DateTime extensions
            Expression.Functions.Add($"humanizer.datetime", (args) =>
            {
                return DateHumanizeExtensions.Humanize(ParseDate(args[0]), ParseDate(args.Skip(1).FirstOrDefault()));
            });

            Expression.Functions.Add($"humanizer.datetimeOrdinal", (args) =>
            {
                if (args.Count == 2)
                {
                    return DateToOrdinalWordsExtensions.ToOrdinalWords(ParseDate(args[0]), ParseEnum<GrammaticalCase>(args.Skip(1).FirstOrDefault()));
                }
                return DateToOrdinalWordsExtensions.ToOrdinalWords(ParseDate(args[0]));
            });

            // bytesize
            Expression.Functions.Add($"humanizer.bits", (args) => ByteSizeExtensions.Bits(ParseInt((object)args[0])).ToString());
            Expression.Functions.Add($"humanizer.bytes", (args) => ByteSizeExtensions.Bytes(ParseNumber((object)args[0])).ToString());
            Expression.Functions.Add($"humanizer.kilobytes", (args) => ByteSizeExtensions.Kilobytes(ParseNumber((object)args[0])).ToString());
            Expression.Functions.Add($"humanizer.megabytes", (args) => ByteSizeExtensions.Megabytes(ParseNumber((object)args[0])).ToString());
            Expression.Functions.Add($"humanizer.gigabytes", (args) => ByteSizeExtensions.Gigabytes(ParseNumber((object)args[0])).ToString());
            Expression.Functions.Add($"humanizer.terabytes", (args) => ByteSizeExtensions.Terabytes(ParseNumber((object)args[0])).ToString());

            // headings (float => North) "north" => float
            Expression.Functions.Add($"humanizer.degrees2heading", (args) =>
            {
                if (args.Count >= 2)
                    return HeadingExtensions.ToHeading(ParseNumber((object)args[0]), ParseEnum<HeadingStyle>(args.Skip(1).FirstOrDefault()));
                return HeadingExtensions.ToHeading(ParseNumber((object)args[0]));
            });
            Expression.Functions.Add($"humanizer.heading2degrees", (args) => HeadingExtensions.FromAbbreviatedHeading((string)args[0].ToString()));

            // inflector
            Expression.Functions.Add($"humanizer.pluralize", (args) => InflectorExtensions.Pluralize(((object)args[0]).ToString(), ParseBool(args.Skip(1).FirstOrDefault())));
            Expression.Functions.Add($"humanizer.camelize", (args) => InflectorExtensions.Camelize(((object)args[0]).ToString()));
            Expression.Functions.Add($"humanizer.dasherize", (args) => InflectorExtensions.Dasherize(((object)args[0]).ToString()));
            Expression.Functions.Add($"humanizer.hyphenate", (args) => InflectorExtensions.Hyphenate(((object)args[0]).ToString()));
            Expression.Functions.Add($"humanizer.kebaberize", (args) => InflectorExtensions.Kebaberize(((object)args[0]).ToString()));
            Expression.Functions.Add($"humanizer.pascalize", (args) => InflectorExtensions.Pascalize(((object)args[0]).ToString()));
            Expression.Functions.Add($"humanizer.singularize", (args) => InflectorExtensions.Singularize(((object)args[0]).ToString(), ParseBool(args.Skip(1).FirstOrDefault(), true), ParseBool(args.Skip(2).FirstOrDefault())));
            Expression.Functions.Add($"humanizer.titleize", (args) => InflectorExtensions.Titleize(((object)args[0]).ToString()));
        }

        internal static T ParseEnum<T>(dynamic arg)
        {
            if (arg is T result)
                return result;

            if (arg is string str)
                return (T)Enum.Parse(typeof(T), str);

            throw new ArgumentNullException($"Not parsable as {typeof(T).FullName}");
        }

        internal static T Parse<T>(dynamic arg, T def = default)
        {
            if (arg == null)
            {
                return def;
            }

            if (arg is T result)
                return result;

            if (arg is JToken jt)
                return jt.ToObject<T>();

            return JObject.FromObject(arg).ToObject<T>();
        }

        internal static DateTime? ParseDate(dynamic arg)
        {
            if (arg is null)
            {
                return null;
            }

            if (arg is DateTime result)
                return result;

            if (arg is string str)
            {
                DateTime.TryParse(str, out result);
                return result;
            }

            return JObject.FromObject(arg).ToObject<DateTime>();
        }

        internal static double ParseNumber(dynamic arg)
        {
            return Convert.ToDouble(arg);
        }

        internal static Int64 ParseInt(dynamic arg)
        {
            return Convert.ToInt64(arg);
        }

        internal static bool ParseBool(dynamic arg, bool def=false)
        {
            if (arg == null)
            {
                return def;
            }

            return Convert.ToBoolean(arg);
        }

    }
}
