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
            if (!Entities.Contains(entity))
            {
                Entities.Add(entity);
                HashSet<LucyEntity> map;
                if (!positionMap.TryGetValue(entity.Start, out map))
                {
                    map = new HashSet<LucyEntity>();
                    positionMap.Add(entity.Start, map);
                }

                map.Add(entity);
            }
        }

        public LucyEntity FindNextEntityOfType(string entityType, int start)
        {
            while (start < Text.Length)
            {
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
                start++;
            }
            return null;
        }

        public IEnumerable<LucyEntity> FindNextRecognizedEntities(int start)
        {
            while (start < Text.Length)
            {
                if (positionMap.TryGetValue(start, out var entities))
                {
                    foreach (var entity in entities)
                    {
                        yield return entity;
                    }
                }
                else
                {
                    start++;
                }
            }
        }

        public bool IsTokenMatched(LucyEntity textToken)
        {
            if (positionMap.TryGetValue(textToken.Start, out var entities))
            {
                return entities.Any();
            }
            return false;
        }

        public LucyEntity FindNextTextEntity(int start)
        {
            return this.TokenEntities.Where(entityToken => entityToken.Start >= start).FirstOrDefault();
        }
    }
}
