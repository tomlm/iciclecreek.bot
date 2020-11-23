using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy
{
    /// <summary>
    /// Entities which are tracked in a MatchContext
    /// </summary>
    public class LucyEntity
    {
        // name of the entity type
        public string Type { get; set; }

        // original text
        public string Text { get; set; }

        // normalized value
        public object Resolution { get; set; }

        public float Score { get; set; }

        // start index
        public int Start { get; set; }

        // index of first char outside of token, length = end-start
        public int End { get; set; }

        /// <summary>
        /// Dependent entities that were consumed to match this entity.
        /// </summary>
        public List<LucyEntity> Children { get; set; } = new List<LucyEntity>();

        public override string ToString() => $"{Type} [{Start},{End}] Resolution:{JsonConvert.SerializeObject(Resolution)}";
    }
}
