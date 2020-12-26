using System;
using System.Collections.Generic;
using System.Text;
using Lucy.PatternMatchers;

namespace Lucy
{
    /// <summary>
    /// Class which represents a rule to execute, and the Entity to create if it matches.
    /// </summary>
    public class EntityPattern
    {
        public EntityPattern()
        {

        }

        public EntityPattern(string name, PatternMatcher pattern, IEnumerable<string> ignore = null)
        {
            this.Name = name.TrimStart('@');
            this.PatternMatcher = pattern;
            this.Ignore = new HashSet<string>(ignore ?? new List<string>());
        }

        public EntityPattern(string name, string resolution, PatternMatcher pattern, IEnumerable<string> ignore = null)
        {
            this.Name = name.TrimStart('@');
            this.Resolution = resolution?.Trim();
            this.PatternMatcher = pattern;
            this.Ignore = new HashSet<string>(ignore ?? new List<string>());
        }

        /// <summary>
        /// name of the entity
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Resolution to use for the entity
        /// </summary>
        public string Resolution { get; set; }

        /// <summary>
        /// Pattern to evalute for this name/resolution
        /// </summary>
        public PatternMatcher PatternMatcher { get; set; }

        /// <summary>
        /// Ignore list
        /// </summary>
        public HashSet<string> Ignore { get; set; } = new HashSet<string>();

        public override string ToString() => $"{Name} => {PatternMatcher}";
    }
}
