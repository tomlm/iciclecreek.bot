using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis.CharFilters;
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
        private Dictionary<int, HashSet<LucyEntity>> positionMap = new Dictionary<int, HashSet<LucyEntity>>();

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

        public void AddEntity(LucyEntity entity)
        {
            if (entity != null && !Entities.Contains(entity))
            {
                Entities.Add(entity);
                HashSet<LucyEntity> map;
                if (!positionMap.TryGetValue(entity.Start, out map))
                {
                    map = new HashSet<LucyEntity>(new EntityTokenComparer());
                    positionMap.Add(entity.Start, map);
                }

                map.Add(entity);
            }
        }

        public LucyEntity FindNextEntityOfType(string entityType, LucyEntity tokenEntity)
        {
            var start = tokenEntity.Start;
            if (positionMap.TryGetValue(start, out var entities))
            {
                var result = entities
                    .Where(entityToken => String.Equals(entityToken.Type, entityType, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public bool IsTokenMatched(LucyEntity textToken)
        {
            if (positionMap.TryGetValue(textToken.Start, out var entities))
            {
                return entities.Any();
            }
            return false;
        }

        /// <summary>
        /// Get first token starting from offset
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public LucyEntity GetFirstTokenEntity(int start = 0)
        {
            return this.TokenEntities.Where(entityToken => entityToken.Start >= start).FirstOrDefault();
        }

        /// <summary>
        /// Get next token after passed in token.
        /// </summary>
        /// <param name="textToken"></param>
        /// <returns></returns>
        public LucyEntity GetNextTokenEntity(LucyEntity textToken)
        {
            return this.TokenEntities.Where(entityToken => entityToken.Start > textToken.End).FirstOrDefault();
        }

        /// <summary>
        /// Get previous token to the passed in token.
        /// </summary>
        /// <param name="textToken"></param>
        /// <returns></returns>
        public LucyEntity GetPreviousTokenEntity(LucyEntity textToken)
        {
            return this.TokenEntities
                .OrderByDescending(et => et.End).Where(entityToken => entityToken.End < textToken.Start).FirstOrDefault();
        }
    }
}
