using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Lucy
{
    public class IntentModel
    {
        public IntentModel()
        {
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("entities")]
        public List<string> Entities { get; set; }

        [JsonProperty("examples")]
        public List<string> Examples { get; set; } = new List<string>();

        public override string ToString() => $"Intent: {Name}";
    }
}
