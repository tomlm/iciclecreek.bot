using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lucyne.Net.Analysis;
using Newtonsoft.Json.Linq;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy
{
    /// <summary>
    /// Represents a entityModels
    /// </summary>
    public class EntityModel
    {
        public EntityModel()
        {
        }

        /// <summary>
        /// Gets or sets the name of the entity
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the default fuzzy match for text tokens
        /// </summary>
        /// <remarks>
        /// If this is set to true, then fuzzy match will be used for all tokens
        /// in the patterns by default, and ~ modifier will turn OFF fuzzy match.
        /// </remarks>
        public bool FuzzyMatch { get; set; } = false;

        /// <summary>
        /// Gets or sets the internal flag.
        /// </summary>
        /// <remarks>If this is set to true, then entity will not be returned as part of the results.</remarks>
        public bool Internal { get; set; } = false;

        // patterns which define the entity
        public List<PatternModel> Patterns { get; set; }  = new List<PatternModel>();

        public override string ToString() => $"{Name}{(FuzzyMatch ? "~" : "")}";
    }
}
