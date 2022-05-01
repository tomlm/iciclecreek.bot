﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BeBot.Dialogs
{
    internal class DateTimexValue
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("timex")]
        public string Timex { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        public IEnumerable<string> Days
        {
            get
            {
                if (Type == "date" || Type == "set")
                {
                    yield return ToDay(Timex);
                }

                if (Type == "daterange")
                {
                    var parts = Timex.Trim('(', ')').Split(",");
                    var start = parts[0];
                    var end = parts[1];
                    if (start.StartsWith("XXXX-WXX") && end.StartsWith("XXXX-WXX"))
                    {
                        int iStart = start.Last() - '0';
                        int iEnd = end.Last()-'0';
                        for (int i = iStart; i <= iEnd; i++)
                        {
                            yield return ToDay($"XXXX-WXX-{i}");
                        }
                    }
                }
            }
        }

        [JsonIgnore]
        public DateTime? Date
        {
            get
            {
                if (DateTime.TryParse(Value, out var dt))
                {
                    return dt;
                }
                return null;
            }
        }


        private string ToDay(string timex)
        {
            switch (timex)
            {
                case "XXXX-WXX-1":
                    return "Monday";
                case "XXXX-WXX-2":
                    return "Tuesday";
                case "XXXX-WXX-3":
                    return "Wednesday";
                case "XXXX-WXX-4":
                    return "Thursday";
                case "XXXX-WXX-5":
                    return "Friday";
                case "XXXX-WXX-6":
                    return "Saturday";
                case "XXXX-WXX-7":
                    return "Sunday";
                default:
                    if (Date != null)
                    {

                    }
                    return null;
            }
        }
    }
}
