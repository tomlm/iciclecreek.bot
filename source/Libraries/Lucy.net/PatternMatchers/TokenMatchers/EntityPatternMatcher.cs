using System.Collections.Generic;
using System.Linq;

namespace Lucy.PatternMatchers
{
    /// <summary>
    /// Will match if there is an existing entity @foo at given start location
    /// </summary>
    public class EntityPatternMatcher : PatternMatcher
    {
        public EntityPatternMatcher(string entityType)
        {
            this.EntityType = entityType.TrimStart('@');
        }

        public string EntityType { get; set; }

        public override MatchResult Matches(MatchContext context, TokenEntity startToken, PatternMatcher nextPatterMatcher)
        {
            var tokenEntity = startToken;

            var matchResult = new MatchResult(false, this, tokenEntity);
            if (tokenEntity != null)
            {
                var entity = context.Entities.FirstOrDefault(le => le.Start == tokenEntity.Start && le.Type == EntityType);
                if (entity != null)
                {
                    // add the matched entity to the children of the currentEntity.
                    context.AddToCurrentEntity(entity);

                    matchResult.Matched = true;
                    matchResult.End = entity.End;
                    matchResult.NextToken = context.GetFirstTokenEntity(entity.End);
                }
            }
            return matchResult;
        }

        public override string ToString() => $"@{EntityType}";

        public override IEnumerable<string> GetEntityReferences()
        {
            yield return this.EntityType;
        }

        public override IEnumerable<string> GenerateExamples(LucyEngine engine)
        {
            foreach (var entityPattern in engine.EntityPatterns.Where(ep => ep.Name == EntityType))
            {
                foreach (var example in entityPattern.PatternMatcher.GenerateExamples(engine))
                {
                    yield return example.Trim();
                }
            }
        }

        private static string[] AgePhrases = { "14 years old", "5 1/2 months", "43 days old" };
        private static string[] BooleanPhrases = { "yes", "no", "yep", "true", "false", "nope" };
        private static string[] CurrencyPhrases = { "$11.13", "43 dollars" };
        private static string[] DateTimePhrases = { "next week", "may 25th", "11 pm", "yesterday", "noon" };
        private static string[] DimensionPhrases = { "15 feet", "19 inches", "72 meters" };
        private static string[] EmailPhrases = { "yosiah@contoso.com", "billybob@contoso.com", "gabepluto@contoso.com" };
        private static string[] GuidPhrases = { "{9AF36FB3-78CF-40D3-A273-A63F6AD82B0A}", "{742FEE95-B647-42C2-A72D-4EB05D783CBA}" };
        private static string[] HashtagPhrases = { "#coolbeans", "#freedom", "#seahawks" };
        private static string[] IpPhrases = { "192.168.0.0", "255.123.231.111", "192.168.254.254" };
        private static string[] MentionPhrases = { "@yosiah", "@billybob", "@gabepluto" };
        private static string[] NumberPhrases = { "1", "two", "three" };
        private static string[] NumberRangePhrases = { "19-44", "twenty to fourty", "one hundred seventeen to one hundred twenty" };
        private static string[] OrdinalPhrases = { "next", "previous", "first", "last" };
        private static string[] PercentagePhrases = { "50%", "nineteen percent", ".3%" };
        private static string[] PhoneNumberPhrases = { "555-123-4567", "123-456-7890", "111-222-3333" };
        private static string[] TemperaturePhrases = { "15 degrees", "451 farenheit", "20 degreees celsius" };
        private static string[] UrlPhrases = { "http://contoso.com/x/y/z", "http://bing.com" };
        private static string[] QuotedTextPhrasess = { "\"This is some text\"", "'a b c d e f'", "\"cows pigs dogs\"" };

        public override string GenerateExample(LucyEngine engine)
        {
            switch (EntityType)
            {
                case "age":
                    return AgePhrases[rnd.Next(AgePhrases.Length)];
                case "boolean":
                    return BooleanPhrases[rnd.Next(BooleanPhrases.Length)];
                case "currency":
                    return CurrencyPhrases[rnd.Next(CurrencyPhrases.Length)];
                case "datetime":
                    return DateTimePhrases[rnd.Next(DateTimePhrases.Length)];
                case "dimension":
                    return DimensionPhrases[rnd.Next(DimensionPhrases.Length)];
                case "email":
                    return EmailPhrases[rnd.Next(EmailPhrases.Length)];
                case "guid":
                    return GuidPhrases[rnd.Next(GuidPhrases.Length)];
                case "hashtag":
                    return HashtagPhrases[rnd.Next(HashtagPhrases.Length)];
                case "ip":
                    return IpPhrases[rnd.Next(IpPhrases.Length)];
                case "mention":
                    return MentionPhrases[rnd.Next(MentionPhrases.Length)];
                case "number":
                    return NumberPhrases[rnd.Next(NumberPhrases.Length)];
                case "numberrange":
                    return NumberRangePhrases[rnd.Next(NumberRangePhrases.Length)];
                case "ordinal":
                    return OrdinalPhrases[rnd.Next(OrdinalPhrases.Length)];
                case "percentage":
                    return PercentagePhrases[rnd.Next(PercentagePhrases.Length)];
                case "phonenumber":
                    return PhoneNumberPhrases[rnd.Next(PhoneNumberPhrases.Length)];
                case "temperature":
                    return TemperaturePhrases[rnd.Next(TemperaturePhrases.Length)];
                case "url":
                    return UrlPhrases[rnd.Next(UrlPhrases.Length)];
                case "quotedtext":
                    return QuotedTextPhrasess[rnd.Next(QuotedTextPhrasess.Length)];

                default:
                    var entityPatterns = engine.EntityPatterns.Where(ep => ep.Name == EntityType).ToList();
                    if (entityPatterns.Any())
                    {

                        var entityPattern = entityPatterns[rnd.Next(entityPatterns.Count)];

                        return entityPattern.PatternMatcher.GenerateExample(engine);
                    }
                    return string.Empty;
            }
        }

    }
}
