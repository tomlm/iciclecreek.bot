using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace BeBot.Dialogs
{
    public class WorkDoc 
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("day")]
        public string Day { get; set; }
    }
}
