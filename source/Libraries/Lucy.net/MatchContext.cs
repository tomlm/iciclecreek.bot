using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private Dictionary<int, LucyEntitySet> tokenMap = new Dictionary<int, LucyEntitySet>();

        public MatchContext()
        {
        }

        public string Text { get; set; }

        /// <summary>
        /// All of the current recognized entities
        /// </summary>
        public LucyEntitySet Entities { get; set; } = new LucyEntitySet();

        /// <summary>
        /// New entities for the current pass through the token space.
        /// </summary>
        public LucyEntitySet NewEntities { get; set; } = new LucyEntitySet();

        /// <summary>
        /// All of the token entities (Aka text and fuzzy text entities)
        /// </summary>
        public LucyEntitySet TokenEntities { get; set; } = new LucyEntitySet();

        /// <summary>
        /// Current entity pattern being evaluated
        /// </summary>
        public EntityPattern EntityPattern { get; set; }

        /// <summary>
        /// This is the entity which is being built up as part of matching.
        /// </summary>
        public LucyEntity CurrentEntity { get; set; }

        public void AddTokenEntity(LucyEntity entity)
        {
            TokenEntities.Add(entity);

            LucyEntitySet map;
            if (!tokenMap.TryGetValue(entity.Start, out map))
            {
                map = new LucyEntitySet();
                tokenMap.Add(entity.Start, map);
            }

            map.Add(entity);
        }

        public void AddNewEntity(LucyEntity entity)
        {
            if (entity != null)
            {
                NewEntities.Add(entity);
            }
        }

        /// <summary>
        /// take all of the new entities and merge them into the main entity pool
        /// </summary>
        public void ProcessNewEntities()
        {
            foreach (var newEntity in NewEntities)
            {
                Entities.Add(newEntity);
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
                        var mergedEntity = MergeEntities(entity, existingEntity);
                        CurrentEntity.Children.Remove(existingEntity);
                        CurrentEntity.Children.Add(mergedEntity);
                        return;
                    }
                }
            }
            CurrentEntity.Children.Add(entity);

            this.MergeEntities(CurrentEntity.Children);

            CurrentEntity.End = CurrentEntity.Children.Max(e => e.End);
            CurrentEntity.Score = CurrentEntity.GetAllEntities().Count() + ((float)(CurrentEntity.End - CurrentEntity.Start) / this.Text.Length);
        }

        public bool IsTokenMatched(LucyEntity tokenEntity)
        {
            return this.Entities.Any(e => e.Start == tokenEntity.Start);
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
        /// Get last token 
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public LucyEntity GetLastTokenEntity()
        {
            return this.TokenEntities.OrderByDescending(e => e.End).First();
        }

        /// <summary>
        /// Get next token after passed in token.
        /// </summary>
        /// <param name="textToken"></param>
        /// <returns></returns>
        public LucyEntity GetNextTokenEntity(LucyEntity textToken)
        {
            if (textToken == null)
            {
                return null;
            }

            return this.TokenEntities.Where(entityToken => entityToken.Start > textToken.End).FirstOrDefault();
        }

        /// <summary>
        /// Get previous token to the passed in token.
        /// </summary>
        /// <param name="textToken"></param>
        /// <returns></returns>
        public LucyEntity GetPreviousTokenEntity(LucyEntity textToken)
        {
            if (textToken == null)
            {
                return GetLastTokenEntity();
            }

            return this.TokenEntities
                .OrderByDescending(et => et.End).Where(entityToken => entityToken.End < textToken.Start).FirstOrDefault();
        }

        /// <summary>
        /// merge any overlapping entities of the same type.
        /// </summary>
        /// <param name="entities"></param>
        public void MergeEntities(LucyEntitySet entities)
        {
            if (entities.Count > 0)
            {

                // merge entities which are overlapping.
                var mergedEntities = new LucyEntitySet(entities);

                foreach (var group in entities.GroupBy(e => e.Type))
                {
                    foreach (var entity in group)
                    {
                        var tokenStart = this.GetFirstTokenEntity(entity.Start);
                        var tokenNext = this.GetFirstTokenEntity(entity.End);

                        // look to see if there are alternative is contigious, has same type and resolution.
                        foreach (var alternateEntity in group.Where(e => e != entity))
                        {
                            // if alternateEntity is bigger on both ends
                            if (alternateEntity.Start < entity.Start && alternateEntity.End > entity.End)
                            {
                                // merge them
                                mergedEntities.Remove(entity);
                                mergedEntities.Remove(alternateEntity);
                                mergedEntities.Add(MergeEntities(entity, alternateEntity));
                            }

                            // if offset overlapping at start or end
                            else if ((alternateEntity.Start <= entity.Start && alternateEntity.End >= entity.Start && alternateEntity.End <= entity.End) ||
                                (alternateEntity.Start >= entity.Start && alternateEntity.Start < entity.End && alternateEntity.End >= entity.End))
                            {
                                // merge them
                                mergedEntities.Remove(entity);
                                mergedEntities.Remove(alternateEntity);
                                mergedEntities.Add(MergeEntities(entity, alternateEntity));
                            }
                            else if (entity.Resolution?.ToString() == alternateEntity.Resolution?.ToString())
                            {
                                // if entity is next to alternateEntity
                                var altTokenStart = this.GetFirstTokenEntity(alternateEntity.Start);
                                var altTokenNext = this.GetFirstTokenEntity(alternateEntity.End);
                                if (tokenNext == altTokenStart || altTokenNext == tokenStart)
                                {
                                    mergedEntities.Remove(entity);
                                    mergedEntities.Remove(alternateEntity);
                                    mergedEntities.Add(MergeEntities(entity, alternateEntity));
                                }
                            }
                        }
                    }
                }

                entities.Clear();
                foreach (var entity in mergedEntities)
                {
                    entities.Add(entity);
                }
            }
        }

        /// <summary>
        /// Pick between overlapping entities of different types.
        /// </summary>
        /// <param name="entities"></param>
        public void ResolveEntities(LucyEntitySet entities)
        {
            if (entities.Count > 0)
            {
                var workingEntities = new LucyEntitySet(entities);

                // merge entities which are overlapping.
                foreach (var entity in workingEntities)
                {
                    foreach (var alternateEntity in workingEntities.Where(e => e.Type != entity.Type))
                    {
                        if (entity.Contains(alternateEntity))
                        {
                            entities.Remove(alternateEntity);
                        }
                        else if (alternateEntity.Contains(entity))
                        {
                            entities.Remove(entity);
                        }
                        // if alternateEntity is bigger on both ends
                        else if ((alternateEntity.Start < entity.Start && alternateEntity.End > entity.End) ||
                            // or overlaps on start
                            (alternateEntity.Start <= entity.Start && alternateEntity.End >= entity.Start && alternateEntity.End <= entity.End) ||
                            // or overlaps on the end
                            (alternateEntity.Start >= entity.Start && alternateEntity.Start < entity.End && alternateEntity.End >= entity.End))
                        {
                            // then we need to pick which one to keep.
                            if (entity.Score < alternateEntity.Score)
                            {
                                entities.Remove(entity);
                            }
                            else if (entity.Score > alternateEntity.Score)
                            {
                                entities.Remove(alternateEntity);
                            }
                            else if ((entity.End - entity.Start) > (alternateEntity.End - alternateEntity.Start))
                            {
                                entities.Remove(entity);
                            }
                            else if ((entity.End - entity.Start) < (alternateEntity.End - alternateEntity.Start))
                            {
                                entities.Remove(alternateEntity);
                            }
                            else
                            {
                                // Trace.WriteLine($"Identical scores for different entities:\n{entity}\n{alternateEntity}");
                            }
                        }
                    }
                }
            }
        }


        private LucyEntity MergeEntities(LucyEntity entity, LucyEntity alternateEntity)
        {
            var mergedEntity = new LucyEntity()
            {
                Type = entity.Type,
                Start = Math.Min(entity.Start, alternateEntity.Start),
                End = Math.Max(entity.End, alternateEntity.End),
            };
            mergedEntity.Text = this.Text.Substring(mergedEntity.Start, mergedEntity.End - mergedEntity.Start);
            if (entity.Resolution != null && alternateEntity.Resolution == null)
            {
                mergedEntity.Resolution = entity.Resolution;
            }
            else if (entity.Resolution == null && alternateEntity.Resolution != null)
            {
                mergedEntity.Resolution = alternateEntity.Resolution;
            }
            else if (entity.Resolution == alternateEntity.Resolution)
            {
                mergedEntity.Resolution = entity.Resolution;
            }
            else
            {
                string resolutionText1 = entity.Resolution?.ToString();
                string resolutionTest2 = entity.Resolution?.ToString();
                if (resolutionText1.Length > resolutionTest2.Length)
                {
                    mergedEntity.Resolution = resolutionText1;
                }
                else
                {
                    mergedEntity.Resolution = resolutionTest2;
                }
            }

            if (entity.Children != null)
            {
                foreach (var child in entity.Children)
                {
                    mergedEntity.Children.Add(child);
                }
            }
            if (alternateEntity.Children != null)
            {
                foreach (var child in alternateEntity.Children)
                {
                    mergedEntity.Children.Add(child);
                }
            }

            if (mergedEntity.Children.Any())
            {
                // mergedEntity.Resolution = String.Empty;
                this.MergeEntities(mergedEntity.Children);
                // this.ResolveEntities(mergedEntity.Children);
            }

            mergedEntity.Score = mergedEntity.GetAllEntities().Count() + ((float)(mergedEntity.End - mergedEntity.Start) / this.Text.Length);
            return mergedEntity;
        }
    }
}
