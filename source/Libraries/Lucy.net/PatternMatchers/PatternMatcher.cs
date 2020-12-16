using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Lucy.PatternMatchers.Matchers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;

namespace Lucy.PatternMatchers
{
    public abstract partial class PatternMatcher
    {
        protected static Random rnd = new Random();

        /// <summary>
        /// Match and add entity as appropriate.
        /// </summary>
        /// <param name="matchContext">match context.</param>
        /// <param name="tokenEntity">token entity</param>
        /// <returns>matchresult</returns>
        public abstract MatchResult Matches(MatchContext matchContext, TokenEntity tokenEntity, PatternMatcher nextPatternMatcher);

        public virtual bool ContainsWildcard()
        {
            return false;
        }

        public virtual IEnumerable<string> GetEntityReferences()
        {
            yield break;
        }

        public abstract IEnumerable<string> GenerateExamples(LucyEngine engine);

        public abstract string GenerateExample(LucyEngine engine);
    }
}
