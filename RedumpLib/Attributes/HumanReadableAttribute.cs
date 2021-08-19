using System;

namespace RedumpLib.Attributes
{
    /// <summary>
    /// Generic attribute for human readable values
    /// </summary>
    public class HumanReadableAttribute : Attribute
    {
        /// <summary>
        /// Item is marked as obsolete or unusable
        /// </summary>
        public bool Available { get; set; } = true;

        /// <summary>
        /// Human-readable name of the item
        /// </summary>
        public string LongName { get; set; }

        /// <summary>
        /// Internally used name of the item
        /// </summary>
        public string ShortName { get; set; }
    }
}