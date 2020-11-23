using System;
using System.Collections.Generic;
using System.Text;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa.PatternMatchers
{
    public class MatchResult
    {
        /// <summary>
        /// True if the matcher was successful
        /// </summary>
        public bool Matched { get; set; }

        /// <summary>
        /// The starting point for sequence
        /// </summary>
        public int NextStart { get; set; }
    }
}
