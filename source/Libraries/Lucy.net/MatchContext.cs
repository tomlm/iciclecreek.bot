using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis.CharFilters;
using Lucy;
using Lucy.PatternMatchers;
using Lucy.PatternMatchers.Matchers;
using Newtonsoft.Json.Linq;

namespace Lucy
{
    /// <summary>
    /// Data structure used to track entities that have been computed over the text.
    /// </summary>
    public class MatchContext
    {
        // map of start token => set of entities which start at that token.
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
        /// All of the current recognized entities
        /// </summary>
        public HashSet<LucyEntity> NewEntities { get; set; } = new HashSet<LucyEntity>(new EntityTokenComparer());

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
                NewEntities.Add(entity);
            }
        }

        public void ProcessNewEntities()
        {
            foreach (var newEntity in NewEntities)
            {
                Entities.Add(newEntity);
                HashSet<LucyEntity> map;
                if (!positionMap.TryGetValue(newEntity.Start, out map))
                {
                    map = new HashSet<LucyEntity>(new EntityTokenComparer());
                    positionMap.Add(newEntity.Start, map);
                }

                map.Add(newEntity);
            }
            NewEntities.Clear();
        }

        public void AddToCurrentEntity(LucyEntity entity)
        {
            var token = this.GetFirstTokenEntity(entity.Start);
            if (token != null)
            {
                var prevToken = this.GetPreviousTokenEntity(token);
                if (prevToken != null)
                {
                    // merge entities if they are contigious and have the same resolution.
                    var existingEntity = CurrentEntity.Children.Where(e => e.Type == entity.Type && e.End == prevToken.End).SingleOrDefault();
                    if (existingEntity != null && JToken.FromObject(entity.Resolution ?? "").ToString() == JToken.FromObject(existingEntity.Resolution ?? "").ToString())
                    {
                        existingEntity.Start = Math.Min(entity.Start, existingEntity.Start);
                        existingEntity.End = Math.Max(entity.End, existingEntity.End);
                        existingEntity.Text = this.Text.Substring(existingEntity.Start, existingEntity.End - existingEntity.Start);
                        return;
                    }
                }
            }

            CurrentEntity.Children.Add(entity);
        }

        public LucyEntity FindNextEntityOfType(string entityType, LucyEntity tokenEntity)
        {
            if (positionMap.TryGetValue(tokenEntity.Start, out var entities))
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

        public bool IsTokenMatched(LucyEntity tokenEntity)
        {
            if (positionMap.TryGetValue(tokenEntity.Start, out var entities))
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

        public void MergeOverlappingEntities()
        {
            // merge entities which are overlapping.
            var mergedEntities = new HashSet<LucyEntity>(new EntityTokenComparer());

            foreach (var entity in this.Entities)
            {
                if (!this.Entities.Any(alternateEntity =>
                    {
                        if (alternateEntity.Type == entity.Type)
                        {
                            // if alternateEntity is bigger on both ends
                            if (alternateEntity.Start < entity.Start && alternateEntity.End > entity.End)
                            {
                                // there is a better candidate
                                return true;
                            }

                            // if offset overlapping at start or end
                            else if ((alternateEntity.Start <= entity.Start && alternateEntity.End >= entity.Start && alternateEntity.End <= entity.End) ||
                            (alternateEntity.Start >= entity.Start && alternateEntity.Start < entity.End && alternateEntity.End >= entity.End))
                            {
                                var entityLength = entity.End - entity.Start;
                                var alternateLength = alternateEntity.End - alternateEntity.Start;
                                if (entityLength < alternateLength)
                                {
                                    // there is a better candidate
                                    return true;
                                }
                            }
                        }
                        // this one it is not better then entity.
                        return false;
                    }))
                {
                    mergedEntities.Add(entity);
                }
            }
            this.Entities = mergedEntities;
        }
    }
}
