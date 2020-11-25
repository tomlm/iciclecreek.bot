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

        /// <summary>
        /// The locale for this model (default:en)
        /// </summary>
        [JsonProperty("locale")]
        public string Locale { get; set; } = "en";

        /// <summary>
        /// The names of any external entities that may be passed in.
        /// </summary>
        [JsonProperty("externalEntities")]
        public List<string> ExternalEntities { get; set; } = new List<string>();

        /// <summary>
        /// Entity definitions
        /// </summary>
        [JsonProperty("entities")]
        public List<EntityModel> Entities { get; set; } = new List<EntityModel>();

        /// <summary>
        /// Intent definitions.
        /// </summary>
        [JsonProperty("intents")]
        public List<IntentModel> Intents { get; set; } = new List<IntentModel>();

        /// <summary>
        /// Macros
        /// </summary>
        [JsonProperty("macros")]
        public Dictionary<string, string> Macros { get; set; }
    }
}
