using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa;
using Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa.PatternMatchers;

namespace Iciclecreek.Bot.Builder.Dialogs.Recognizers.Lupa
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

        public HashSet<LupaEntity> Entities { get; set; } = new HashSet<LupaEntity>(new EntityTokenComparer());

        public IEnumerable<LupaEntity> FindNextEntities(string entityType, int start, int slop = 10)
        {
            return this.Entities.Where(entityToken =>
                entityToken.Type == entityType &&
                entityToken.Start >= start &&
                entityToken.Start <= (start + slop));
        }
    }
}
