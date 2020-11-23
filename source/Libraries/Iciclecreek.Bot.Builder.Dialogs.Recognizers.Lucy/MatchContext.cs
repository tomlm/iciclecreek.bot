using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lucy.PatternMatchers;

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

        public IEnumerable<LucyEntity> FindNextEntities(string entityType, int start, int slop = 0)
        {
            return this.Entities.Where(entityToken =>
                entityToken.Type == entityType &&
                entityToken.Start >= start &&
                entityToken.Start <= (start + slop + 1));
        }
    }
}
