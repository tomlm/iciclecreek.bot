using AdaptiveExpressions;
using Antlr4.Runtime.Misc;
using Humanizer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Iciclecreek.Bot.Expressions.Humanizer
{
    public static class HumanizerFunctions
    {
        public static string Locale { get; set; } = "turn.activity.locale";

        public static void Register()
        {
            // case functions
            Expression.Functions.Add($"humanizer.allCaps", (args) => CasingExtensions.ApplyCase(args[0], LetterCasing.AllCaps));
            Expression.Functions.Add($"humanizer.lowerCase", (args) => CasingExtensions.ApplyCase(args[0], LetterCasing.LowerCase));
            Expression.Functions.Add($"humanizer.sentence", (args) => CasingExtensions.ApplyCase(args[0], LetterCasing.Sentence));
            Expression.Functions.Add($"humanizer.title", (args) => CasingExtensions.ApplyCase(args[0], LetterCasing.Title));

            // humanize collections
            Expression.Functions.Add($"humanizer.humanizeList", (args) =>
            {
                CollectionHumanizeExtensions.Humanize<object>(args[0], args[1]);
                return string.Empty;
            });

            // DateTime functions
            Expression.Functions.Add($"humanizer.humanizeDateTime", (args) =>
            {
                var culture = ParseCulture(args);
                var referenceDate = ParseDate((object)args.Skip(1).FirstOrDefault());

                if (referenceDate.HasValue)
                    return DateHumanizeExtensions.Humanize(ParseDate(args[0]), referenceDate.Value, culture);

                return DateHumanizeExtensions.Humanize(ParseDate(args[0]), culture: culture);
            });

            Expression.Functions.Add($"humanizer.dateTimeToOrdinalWords", (args) =>
            {
                if (args.Count == 2)
                {
                    return DateToOrdinalWordsExtensions.ToOrdinalWords(ParseDate(args[0]), ParseEnum<GrammaticalCase>(args.Skip(1).FirstOrDefault()));
                }
                return DateToOrdinalWordsExtensions.ToOrdinalWords(ParseDate(args[0]));
            });

            // timespan functions
            Expression.Functions.Add($"humanizer.humanizeTimeSpan", (args) =>
            {
                var culture = ParseCulture(args);
                var precision = (int)ParseInt(args.Skip(1).FirstOrDefault(), 1);
                return TimeSpanHumanizeExtensions.Humanize(timeSpan: ParseTimeSpan(args[0]), precision: precision, culture: culture);
            });
            Expression.Functions.Add($"humanizer.weeks", (args) => NumberToTimeSpanExtensions.Weeks(ParseNumber((object)args[0])));
            Expression.Functions.Add($"humanizer.days", (args) => NumberToTimeSpanExtensions.Days(ParseNumber((object)args[0])));
            Expression.Functions.Add($"humanizer.hours", (args) => NumberToTimeSpanExtensions.Hours(ParseNumber((object)args[0])));
            Expression.Functions.Add($"humanizer.minutes", (args) => NumberToTimeSpanExtensions.Minutes(ParseNumber((object)args[0])));
            Expression.Functions.Add($"humanizer.seconds", (args) => NumberToTimeSpanExtensions.Seconds(ParseNumber((object)args[0])));
            Expression.Functions.Add($"humanizer.milliseconds", (args) => NumberToTimeSpanExtensions.Milliseconds(ParseNumber((object)args[0])));

            // bytesize functions
            Expression.Functions.Add($"humanizer.bits", (args) => ByteSizeExtensions.Bits(ParseInt((object)args[0])).ToString());
            Expression.Functions.Add($"humanizer.bytes", (args) => ByteSizeExtensions.Bytes(ParseNumber((object)args[0])).ToString());
            Expression.Functions.Add($"humanizer.kilobytes", (args) => ByteSizeExtensions.Kilobytes(ParseNumber((object)args[0])).ToString());
            Expression.Functions.Add($"humanizer.megabytes", (args) => ByteSizeExtensions.Megabytes(ParseNumber((object)args[0])).ToString());
            Expression.Functions.Add($"humanizer.gigabytes", (args) => ByteSizeExtensions.Gigabytes(ParseNumber((object)args[0])).ToString());
            Expression.Functions.Add($"humanizer.terabytes", (args) => ByteSizeExtensions.Terabytes(ParseNumber((object)args[0])).ToString());

            // headings (float => North) "north" => float
            Expression.Functions.Add($"humanizer.degrees2heading", (args) =>
            {
                var culture = ParseCulture(args);

                if (TryParseEnum<HeadingStyle>((object)args.Skip(1).FirstOrDefault(), out var style))
                    return HeadingExtensions.ToHeading(ParseNumber((object)args[0]), style, culture: culture);
                return HeadingExtensions.ToHeading(ParseNumber((object)args[0]), culture: culture);
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

            // metric
            Expression.Functions.Add($"humanizer.metric2number", (args) => MetricNumeralExtensions.FromMetric(((object)args[0]).ToString()));
            Expression.Functions.Add($"humanizer.number2metric", (args) =>
            {
                if (args.Count == 4)
                    return MetricNumeralExtensions.ToMetric(ParseNumber(args[0]), hasSpace: ParseBool(args[2]), useSymbol: ParseBool(args[3]), decimals: (Int32)ParseInt(args[1]));
                else if (args.Count == 3)
                    return MetricNumeralExtensions.ToMetric(ParseNumber(args[0]), hasSpace: ParseBool(args[2]), decimals: (Int32)ParseInt(args[1]));
                else if (args.Count == 2)
                    return MetricNumeralExtensions.ToMetric(ParseNumber(args[0]), decimals: (Int32)ParseInt(args[1]));

                return MetricNumeralExtensions.ToMetric(ParseNumber(args[0]));
            });

            // numberToWords functions
            Expression.Functions.Add($"humanizer.number2words", (args) =>
            {
                var culture = ParseCulture(args);
                if (TryParseEnum<GrammaticalGender>((object)args.Skip(1).FirstOrDefault(), out var enm))
                    return NumberToWordsExtension.ToWords(ParseInt(args[0]), enm, culture: culture);
                return NumberToWordsExtension.ToWords(ParseInt(args[0]), culture: culture);
            });

            Expression.Functions.Add($"humanizer.number2ordinal", (args) =>
            {
                var culture = ParseCulture(args);

                if (TryParseEnum<GrammaticalGender>((object)args.Skip(1).FirstOrDefault(), out var gender))
                    return NumberToWordsExtension.ToOrdinalWords((Int32)ParseInt(args[0]), gender, culture: culture);

                return NumberToWordsExtension.ToOrdinalWords((Int32)ParseInt(args[0]), culture: culture);
            });

            Expression.Functions.Add($"humanizer.ordinalize", (args) =>
            {
                var culture = ParseCulture(args);

                if (TryParseEnum<GrammaticalGender>((object)args.Skip(1).FirstOrDefault(), out var gender))
                    return OrdinalizeExtensions.Ordinalize((Int32)ParseInt(args[0]), gender, culture: culture);

                return OrdinalizeExtensions.Ordinalize((Int32)ParseInt(args[0]), culture: culture);
            });

            // roman functions
            Expression.Functions.Add($"humanizer.fromRoman", (args) => RomanNumeralExtensions.FromRoman(((object)args[0]).ToString()));
            Expression.Functions.Add($"humanizer.toRoman", (args) => RomanNumeralExtensions.ToRoman((Int32)ParseInt(args[0])));

            // toQuantity functions
            Expression.Functions.Add($"humanizer.toQuantity", (args) =>
            {
                if (args.Count == 3)
                {
                    string arg3 = ((object)args[2]).ToString();
                    if (Enum.TryParse<ShowQuantityAs>(arg3, out var showQuanityAs))
                        return ToQuantityExtensions.ToQuantity(((object)args[0]).ToString(), ParseInt(args[1]), showQuanityAs);

                    // use arg[2] as format string
                    return ToQuantityExtensions.ToQuantity(((object)args[0]).ToString(), ParseInt(args[1]), format: arg3);
                }

                if (args.Count == 3)
                    return ToQuantityExtensions.ToQuantity(((object)args[0]).ToString(), ParseInt(args[1]), ParseEnum<ShowQuantityAs>(args[2]));

                return ToQuantityExtensions.ToQuantity(((object)args[0]).ToString(), ParseInt(args[1]));
            });

            // truncate functions
            Expression.Functions.Add($"humanizer.truncate", (args) =>
            {
                if (args.Count == 3)
                    return TruncateExtensions.Truncate(((object)args[0]).ToString(), (int)ParseInt(args[1]), ((object)args[2]).ToString());
                return TruncateExtensions.Truncate(((object)args[0]).ToString(), (int)ParseInt(args[1]));
            });

            Expression.Functions.Add($"humanizer.truncateWords", (args) =>
            {
                if (args.Count == 3)
                    return TruncateExtensions.Truncate(((object)args[0]).ToString(), (int)ParseInt(args[1]), ((object)args[2]).ToString(), truncator: Truncator.FixedNumberOfWords);
                return TruncateExtensions.Truncate(((object)args[0]).ToString(), (int)ParseInt(args[1]), truncator: Truncator.FixedNumberOfWords);
            });

            // tupelize functions
            Expression.Functions.Add($"humanizer.tupleize", (args) => TupleizeExtensions.Tupleize((int)ParseInt(args[0])));

            // dotnet format functions
            Expression.Functions.Add($"dotnet.numberToString", (args) =>
            {
                var culture = ParseCulture(args);
                var number = ParseNumber((object)args[0]);
                var format = ((object)args[1]).ToString();
                return number.ToString(format, culture);
            });
            Expression.Functions.Add($"dotnet.intToString", (args) =>
            {
                var culture = ParseCulture(args);
                var integer = ParseInt((object)args[0]);
                var format = ((object)args[1]).ToString();
                return integer.ToString(format, culture);
            });
            Expression.Functions.Add($"dotnet.dateTimeToString", (args) =>
            {
                var culture = ParseCulture(args);
                var dateTime = ParseDate((object)args[0]);
                var format = ((object)args[1]).ToString();
                return dateTime.Value.ToString(format, culture);
            });
            Expression.Functions.Add($"dotnet.timeSpanToString", (args) =>
            {
                var culture = ParseCulture(args);
                var timeSpan = ParseTimeSpan((object)args[0]);
                var format = ((object)args[1]).ToString();
                return timeSpan.Value.ToString(format, culture);
            });
        }

        internal static bool TryParseEnum<T>(object arg, out T result)
            where T : struct
        {
            result = default(T);

            if (arg == null)
            {
                return false;
            }

            if (arg is T r)
            {
                result = r;
                return true;
            }

            return Enum.TryParse<T>(arg.ToString(), out result);
        }

        internal static T ParseEnum<T>(object arg)
        {
            if (arg is T result)
                return result;

            return (T)Enum.Parse(typeof(T), ((object)arg).ToString());
        }

        internal static T Parse<T>(object arg, T def = default)
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

        internal static DateTime? ParseDate(object arg)
        {
            if (arg is null)
            {
                return null;
            }

            if (arg is DateTime result)
                return result;

            if (arg is string str)
            {
                if (DateTime.TryParse(str, out result))
                    return result;
                return null;
            }

            return JObject.FromObject(arg).ToObject<DateTime>();
        }

        internal static TimeSpan? ParseTimeSpan(object arg)
        {
            if (arg is null)
            {
                return null;
            }

            if (arg is TimeSpan result)
                return result;

            if (arg is string str)
            {
                TimeSpan.TryParse(str, out result);
                return result;
            }

            return JObject.FromObject(arg).ToObject<TimeSpan>();
        }

        internal static double ParseNumber(object arg)
        {
            return Convert.ToDouble(arg);
        }

        internal static Int64 ParseInt(object arg, int def = 0)
        {
            if (arg == null)
            {
                return def;
            }

            try
            {
                return Convert.ToInt64(arg);
            }
            catch
            {
                return def;
            }
        }

        internal static bool ParseBool(object arg, bool def = false)
        {
            if (arg == null)
            {
                return def;
            }

            try
            {
                return Convert.ToBoolean(arg);
            }
            catch
            {
                return def;
            }
        }

        internal static CultureInfo ParseCulture(IEnumerable<object> args)
        {
            foreach (var arg in args.Reverse())
            {
                string culture = arg.ToString();
                try
                {
                    return CultureInfo.GetCultureInfo(culture);
                }
                catch
                {
                    return Thread.CurrentThread.CurrentCulture;
                }
            }
            return Thread.CurrentThread.CurrentCulture;
        }
    }
}
