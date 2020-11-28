using System;
using System.Collections.Generic;
using System.Text;
using Lucy.PatternMatchers;
using Newtonsoft.Json;

namespace Lucy
{
    /// <summary>
    /// Entities which are tracked in a MatchContext
    /// </summary>
    public class LucyEntity
    {
        public LucyEntity()
        {
        }

        // name of the entity type
        [JsonProperty("type")]
        public string Type { get; set; }

        // original text
        [JsonProperty("text")]
        public string Text { get; set; }

        // normalized value
        [JsonProperty("resolution")]
        public object Resolution { get; set; }

        [JsonProperty("score")]
        public float Score { get; set; }

        // start index
        [JsonProperty("start")]
        public int Start { get; set; }

        // index of first char outside of token, length = end-start
        [JsonProperty("end")]
        public int End { get; set; }

        /// <summary>
        /// Dependent entities that were consumed to match this entity.
        /// </summary>
        [JsonProperty("children")]
        public HashSet<LucyEntity> Children { get; set; } = new HashSet<LucyEntity>(new EntityTokenComparer());

        public IEnumerable<LucyEntity> GetAllEntities()
        {
            foreach(var child in Children)
            {
                yield return child;
                foreach(var subChild in child.GetAllEntities())
                {
                    yield return subChild;
                }
            }
        }

        public override string ToString()
        {
            if (Resolution == null)
            {
                return $"{Type} [{Start},{End}]";
            }
            return $"{Type} [{Start},{End}] Resolution:{JsonConvert.SerializeObject(Resolution)}";
        }
    }
}
