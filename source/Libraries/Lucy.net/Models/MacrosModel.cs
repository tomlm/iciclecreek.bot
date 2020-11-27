using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lucene.Net.Analysis;
using Newtonsoft.Json.Linq;

namespace Lucy
{
    /// <summary>
    /// Represents a Value
    /// </summary>
    public class MacrosModel
    {
        public MacrosModel()
        {
        }

        /// <summary>
        /// name (must start with $)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Aliases 
        /// </summary>
        public string Value { get; set; }

        public override string ToString() => $"{Name} = {Value}";
    }
}
