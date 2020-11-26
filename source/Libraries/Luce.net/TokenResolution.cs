using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Luce
{
    public class TokenResolution
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("fuzzyToken")]
        public HashSet<string> FuzzyTokens { get; set; } = new HashSet<string>();
    }
}
