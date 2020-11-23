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

        public HashSet<LucyEntity> Entities { get; set; } = new HashSet<LucyEntity>(new EntityTokenComparer());

        public IEnumerable<LucyEntity> FindNextEntities(string entityType, int start, int slop = 0)
        {
            return this.Entities.Where(entityToken =>
                entityToken.Type == entityType &&
                entityToken.Start >= start &&
                entityToken.Start <= (start + slop + 1));
        }
    }
}
