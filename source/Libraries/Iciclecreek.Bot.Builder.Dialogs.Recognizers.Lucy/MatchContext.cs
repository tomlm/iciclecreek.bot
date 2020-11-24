using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy.PatternMatchers;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy.PatternMatchers.Matchers;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy
{
    /// <summary>
    /// Data structure used to track entities that have been computed over the text.
    /// </summary>
    public class MatchContext
    {
        public MatchContext()
        {
        }

        public string Text { get; set; }

        /// <summary>
        /// All of the current recognized entities
        /// </summary>
        public HashSet<LucyEntity> Entities { get; set; } = new HashSet<LucyEntity>(new EntityTokenComparer());

        /// <summary>
        /// Current entity pattern being evaluated
        /// </summary>
        public EntityPattern EntityPattern { get; set; }

        /// <summary>
        /// This is the entity which is being built up as part of matching.
        /// </summary>
        public LucyEntity CurrentEntity { get; set; }

        public IEnumerable<LucyEntity> FindNextEntities(string entityType, int start)
        {
            // slop is large for text tokens (to take care of embedded spaces)
            // slop is tight for entity matching
            int slop = (entityType == TokenPatternMatcher.ENTITYTYPE || entityType == FuzzyTokenPatternMatcher.ENTITYTYPE) ? 255 : 0;
            return this.Entities.Where(entityToken =>
                String.Equals(entityToken.Type, entityType, StringComparison.OrdinalIgnoreCase) &&
                entityToken.Start >= start && entityToken.Start <= (start + slop + 1));
        }
    }
}
