using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;
using Lucy.PatternMatchers.Matchers;

namespace Lucy.PatternMatchers
{
    public class PatternParser
    {
        private Analyzer exactAnalyzer;
        private Analyzer fuzzyAnalyzer;

        public PatternParser(Analyzer exactAnalyzer, Analyzer fuzzyAnalyzer)
        {
            this.exactAnalyzer = exactAnalyzer;
            this.fuzzyAnalyzer = fuzzyAnalyzer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern">pattern to parse</param>
        /// <returns></returns>
        public PatternMatcher Parse(string pattern, bool fuzzyMatch = false)
        {
            var context = new ParseContext(exactAnalyzer, fuzzyAnalyzer, fuzzyMatch);
            return context.Parse(pattern);
        }
    }
}
