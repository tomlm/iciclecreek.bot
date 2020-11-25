using System;
using System.Collections.Generic;
using System.Text;
using Luce.PatternMatchers;

namespace Luce
{
    /// <summary>
    /// Class which represents a rule to execute, and the Entity to create if it matches.
    /// </summary>
    public class EntityPattern
    {
        public EntityPattern()
        {

        }

        public EntityPattern(string name, PatternMatcher pattern)
        {
            this.Name = name.TrimStart('@');
            this.PatternMatcher = pattern;
        }

        public EntityPattern(string name, string resolution, PatternMatcher pattern)
        {
            this.Name = name.TrimStart('@');
            this.Resolution = resolution?.Trim('(', ')', '~').Trim();
            this.PatternMatcher = pattern;
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

        public override string ToString() => $"{Name} => {PatternMatcher}";
    }
}
