using System;
using System.Collections.Generic;
using Lucyne.Net.Analysis;
using Lucyne.Net.Analysis.Core;
using Lucyne.Net.Analysis.Phonetic;
using Lucyne.Net.Analysis.Phonetic.Language.Bm;
using Lucyne.Net.Analysis.Standard;
using Lucyne.Net.Analysis.Util;
using Lucyne.Net.Util;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy
{
    public class LucyModel
    {
        public LucyModel()
        { }

        public Dictionary<string, string> Macros { get; set; }

        public List<EntityModel> Entities { get; set; } = new List<EntityModel>();

        public List<IntentModel> Intents { get; set; } = new List<IntentModel>();
    }
}
