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

namespace Lucy
{
    public class LucyModel
    {
        public LucyModel()
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
        /// Macros
        /// </summary>
        [JsonProperty("macros")]
        public Dictionary<string, string> Macros { get; set; }
    }
}
