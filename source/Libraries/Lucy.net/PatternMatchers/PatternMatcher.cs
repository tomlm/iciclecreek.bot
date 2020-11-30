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
        /// <summary>
        /// Match and add entity as appropriate.
        /// </summary>
        /// <param name="matchContext">match context.</param>
        /// <param name="tokenEntity">token entity</param>
        /// <returns>matchresult</returns>
        public abstract MatchResult Matches(MatchContext matchContext, LucyEntity tokenEntity);

        public virtual byte MaxTokens { get; protected set; } = byte.MaxValue;

        public virtual bool ContainsWildcard()
        {
            return false;
        }

        public virtual IEnumerable<string> GetEntityReferences()
        {
            yield break;
        }

    }
}
