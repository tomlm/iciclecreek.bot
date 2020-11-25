using System.Collections.Generic;
using Newtonsoft.Json;

namespace Luce
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
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the default fuzzy match for text tokens
        /// </summary>
        /// <remarks>
        /// If this is set to true, then fuzzy match will be used for all tokens
        /// in the patterns by default, and ~ modifier will turn OFF fuzzy match.
        /// </remarks>
        [JsonProperty("fuzzyMatch")]
        public bool FuzzyMatch { get; set; } = false;

        // patterns which define the entity
        public List<PatternModel> Patterns { get; set; }  = new List<PatternModel>();

        public override string ToString() => $"{Name}{(FuzzyMatch ? "~" : "")}";
    }
}
