using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lucene.Net.Analysis;
using Newtonsoft.Json.Linq;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa
{
    /// <summary>
    /// Represents a entityModels
    /// </summary>
    public class EntityModel
    {
        public EntityModel()
        {
        }

        // Name of the entity
        public string Name { get; set; }

        // Default fuzzy match for text tokens
        public bool FuzzyMatch { get; set; } = false;

        // patterns which define the entity
        public List<PatternModel> Patterns { get; set; }  = new List<PatternModel>();
    }
}
