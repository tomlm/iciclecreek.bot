using System;
using System.Collections.Generic;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Phonetic;
using Lucene.Net.Analysis.Phonetic.Language.Bm;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy
{
    public class LucyModel
    {
        public Dictionary<string, string> Macros { get; set; }

        public List<EntityModel> Entities { get; set; } = new List<EntityModel>();

        public List<IntentModel> Intents { get; set; } = new List<IntentModel>();
    }
}
