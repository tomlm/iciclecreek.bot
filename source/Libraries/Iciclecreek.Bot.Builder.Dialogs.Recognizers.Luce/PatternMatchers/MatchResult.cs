using System;
using System.Collections.Generic;
using System.Text;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Luce.PatternMatchers
{
    public class MatchResult
    {
        /// <summary>
        /// True if the matcher was successful
        /// </summary>
        public bool Matched { get; set; }

        /// <summary>
        /// Set to true to have this matcher run again on the next token
        /// </summary>
        public bool Repeat { get; set; } = false;

        /// <summary>
        /// The starting point for sequence
        /// </summary>
        public int NextStart { get; set; }
    }
}
