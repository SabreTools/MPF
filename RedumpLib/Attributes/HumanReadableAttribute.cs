using System;

namespace RedumpLib.Attributes
{
    /// <summary>
    /// Generic attribute for human readable values
    /// </summary>
    public class HumanReadableAttribute : Attribute
    {
        public string LongName { get; set; }

        public string ShortName { get; set; }
    }
}