using System;
using System.Collections.Generic;
using System.Text;

namespace Lucy.PatternMatchers
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
        /// End of recognized sequence.
        /// </summary>
        public int End { get; set; }

        /// <summary>
        /// Next token to process
        /// </summary>
        public LucyEntity NextToken { get; set; }
    }
}
