using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;

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
        public float Score { get; set; } = 1.0f;

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
        public LucyEntitySet Children { get; set; } = new LucyEntitySet();

        public bool Contains(LucyEntity entity)
        {
            if (this == entity)
            {
                return true;
            }

            if (Children.Contains(entity))
            {
                return true;
            }

            return Children.Any(e => e.Contains(entity));
        }

        public IEnumerable<LucyEntity> GetAllEntities()
        {
            foreach (var child in Children)
            {
                yield return child;
                foreach (var subChild in child.GetAllEntities())
                {
                    yield return subChild;
                }
            }
        }

        public override string ToString()
        {
            if (Resolution == null)
            {
                return $"@{Type} [{Start},{End}] {String.Join(",", this.Children.Select(c => '@'+c.Type))}";
            }
            var json = JsonConvert.SerializeObject(Resolution);
            return $"@{Type} [{Start},{End}] '{Text}' Resolution:{json}";
        }

        public override bool Equals(object obj)
        {
            LucyEntity entity2 = obj as LucyEntity;

            if (entity2 == null)
                return false;

            if (this.Start != entity2.Start)
                return false;
            if (this.End != entity2.End)
                return false;
            if (!String.Equals(this.Type, entity2.Type, StringComparison.OrdinalIgnoreCase))
                return false;
            if (!String.Equals(this.Text, entity2.Text, StringComparison.OrdinalIgnoreCase))
                return false;
            if ((this.Resolution != null && entity2.Resolution == null) ||
                     (entity2.Resolution == null && entity2.Resolution != null) ||
                     (this.Resolution?.GetType().Name != entity2.Resolution?.GetType().Name))
                return false;
            if (this.Resolution?.ToString() != entity2.Resolution?.ToString())
                return false;
            if (this.Children.Count != entity2.Children.Count)
                return false;

            foreach (var child in this.Children)
            {
                if (entity2.Children.Contains(child) == false)
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hCode = (this.Type.GetHashCode() * 17) ^ this.Start ^ this.End;
            if (this.Text != null)
                hCode ^= (this.Text.GetHashCode() * 18);
            if (this.Resolution != null)
                hCode ^= (this.Resolution.GetHashCode() * 19);

            if (this.Children != null)
            {
                foreach (var child in this.Children)
                {
                    hCode ^= child.GetHashCode();
                }
            }

            return hCode;
        }
    }
}
