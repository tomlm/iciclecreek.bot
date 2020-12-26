using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lucy
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

        /// <summary>
        /// The entities to use to make this entity.
        /// </summary>
        [JsonProperty("entities")]
        public List<string> Entities{ get; set; } = new List<string>();

        /// <summary>
        /// Example utterances for this entity.
        /// </summary>
        [JsonProperty("examples")]
        public List<string> Examples { get; set; } = new List<string>();

        // patterns which define the entity
        [JsonProperty("patterns")]
        public List<PatternModel> Patterns { get; set; }  = new List<PatternModel>();

        /// <summary>
        /// Ignore tokens.
        /// </summary>
        [JsonProperty("ignore")]
        public List<string> Ignore{ get; set; } = new List<string>();

        public override string ToString() => $"{Name}{(FuzzyMatch ? "~" : "")}";
    }
}
