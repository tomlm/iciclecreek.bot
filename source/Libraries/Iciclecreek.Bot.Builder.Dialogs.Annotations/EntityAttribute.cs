using System;

namespace Iciclecreek.Bot.Builder.Dialogs.Annotations
{
    /// <summary>
    /// Attribute for defining the type of entity to map to this slot, and example uttterances.
    /// </summary>
    /// <remarks>
    /// Examples are used to prime generated dialogs .lu files for entity recgonition.
    /// </remarks>
    /// <example>
    /// [Entity("personName", "Joe", "Frank")]
    /// [Entity("firstName", "Joe", "Frank")]
    /// [Entity("lastName", "Smith", "Lee")]
    /// public string Name {get;set;}
    /// </example>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class EntityAttribute : Attribute
    {
        public EntityAttribute(string entity, params string[] examples)
        {
            this.Entity = entity;
            this.Examples = examples ?? Array.Empty<string>();
        }

        /// <summary>
        /// Gets or sets the name of the Entity.
        /// </summary>
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the examples.
        /// </summary>
        public string[] Examples { get; set; }
    }
}
