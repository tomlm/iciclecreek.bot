using System;
using System.Collections.Generic;
using System.Text;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa.PatternMatchers;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa
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
            this.Name = name;
            this.PatternMatcher = pattern;
        }

        public EntityPattern(string name, string resolution, PatternMatcher pattern)
        {
            this.Name = name;
            this.Resolution = resolution;
            this.PatternMatcher = pattern;
        }

        public string Name { get; set; }

        public string Resolution { get; set; }

        public PatternMatcher PatternMatcher { get; set; }
    }
}
