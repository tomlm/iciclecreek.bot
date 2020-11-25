using System;
using System.Collections.Generic;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Phonetic;
using Lucene.Net.Analysis.Phonetic.Language.Bm;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;
using Newtonsoft.Json;

namespace Luce
{
    public class LuceModel
    {
        public LuceModel()
        { }

        [JsonProperty("externalEntities")]
        public List<string> ExternalEntities { get; set; } = new List<string>();

        [JsonProperty("entities")]
        public List<EntityModel> Entities { get; set; } = new List<EntityModel>();

        [JsonProperty("intents")]
        public List<IntentModel> Intents { get; set; } = new List<IntentModel>();

        [JsonProperty("macros")]
        public Dictionary<string, string> Macros { get; set; }
    }
}
