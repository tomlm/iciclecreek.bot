using System;

namespace Iciclecreek.Bot.Builder.Dialogs.Annotations
{
    /// <summary>
    /// Attribute for defining the examples of values for a property.
    /// </summary>
    /// <example>
    /// [Examples("Joe", "Frank")]
    /// public string Name {get;set;}
    /// </example>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ExamplesAttribute : Attribute
    {
        public ExamplesAttribute(params object[] examples)
        {
            this.Examples = examples ?? Array.Empty<object>();
        }

        /// <summary>
        /// Gets or sets the examples.
        /// </summary>
        public object[] Examples { get; set; }
    }
}
