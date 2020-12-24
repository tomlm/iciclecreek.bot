using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Lucy.PatternMatchers.Matchers;
using Newtonsoft.Json.Linq;

namespace Lucy.PatternMatchers
{
    /// <summary>
    /// Will match any unclaimed tokens
    /// </summary>
    public class RegexPatternMatcher
    {
        public RegexPatternMatcher(string entityType, string pattern)
        {
            this.EntityType = entityType.TrimStart('@');
            this.Pattern = new Regex(pattern, RegexOptions.Compiled);
        }

        public Regex Pattern { get; set; }

        public string EntityType { get; set; }

        public List<LucyEntity> Matches(string text)
        {
            return this.Pattern.Matches(text).Cast<Match>().Select(match =>
            {
                return new LucyEntity()
                {
                    Type = this.EntityType,
                    Text = match.Value,
                    Start = match.Index,
                    End = match.Index + match.Length,
                    Resolution = match.Value,
                    Score = 1.0f
                };
            }).ToList();
        }

        public override string ToString() => $"@{this.EntityType} : {this.Pattern}";
    }
}
