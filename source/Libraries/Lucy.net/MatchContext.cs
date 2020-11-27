using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucy;
using Lucy.PatternMatchers;
using Lucy.PatternMatchers.Matchers;

namespace Lucy
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
        /// All of the token entities (Aka text and fuzzy text entities)
        /// </summary>
        public HashSet<LucyEntity> TokenEntities { get; set; } = new HashSet<LucyEntity>(new EntityTokenComparer());

        /// <summary>
        /// Current entity pattern being evaluated
        /// </summary>
        public EntityPattern EntityPattern { get; set; }

        /// <summary>
        /// This is the entity which is being built up as part of matching.
        /// </summary>
        public LucyEntity CurrentEntity { get; set; }

        public IEnumerable<LucyEntity> FindNextEntityOfType(string entityType, int start)
        {
            return this.Entities.Where(entityToken =>
                String.Equals(entityToken.Type, entityType, StringComparison.OrdinalIgnoreCase) &&
                entityToken.Start >= start && entityToken.Start <= (start + 1));
        }

        public IEnumerable<LucyEntity> FindNextRecognizedEntities(int start)
        {
            return this.Entities.Where(entityToken =>
                entityToken.Start >= start && entityToken.Start <= (start + 1));
        }

        public LucyEntity FindNextTextEntity(int start)
        {
            return this.TokenEntities.Where(entityToken => entityToken.Start >= start).FirstOrDefault();
        }
    }
}
