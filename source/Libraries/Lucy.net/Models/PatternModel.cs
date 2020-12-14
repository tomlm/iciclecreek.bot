using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lucene.Net.Analysis;
using Newtonsoft.Json.Linq;

namespace Lucy
{
    /// <summary>
    /// Represents a pattern which is a string, or array of strings
    /// </summary>
    public class PatternModel : IEnumerable<string>
    {
        private List<string> patterns = new List<string>();

        public PatternModel()
        {
        }

        public PatternModel(string patternDefinition)
        {
            this.patterns.Add(patternDefinition.Trim());
        }

        public PatternModel(string[] patternDefinitions)
        {
            if (patternDefinitions!= null && patternDefinitions.Any())
            {
                this.patterns.AddRange(patternDefinitions.Select(pattern => pattern.Trim()));
            }
        }

        public IEnumerator<string> GetEnumerator() => this.patterns.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.patterns).GetEnumerator();

        public static implicit operator PatternModel(string patternDefinition) => new PatternModel(patternDefinition);
        public static implicit operator PatternModel(JValue patternDefinition) => new PatternModel((string)patternDefinition);

        public static implicit operator PatternModel(string[] patternDefinitions) => new PatternModel(patternDefinitions);
        public static implicit operator PatternModel(JArray patternDefinitions) => new PatternModel(patternDefinitions.ToObject<string[]>());

        public override string ToString() => $"[{this.patterns.FirstOrDefault()}, ...]";
    }
}
