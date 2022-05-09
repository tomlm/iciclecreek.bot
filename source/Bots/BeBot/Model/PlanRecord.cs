using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace BeBot.Dialogs
{
    public class PlanRecord 
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("day")]
        public string[] Days { get; set; }
    }
}
