namespace RedumpLib.Attributes
{
    /// <summary>
    /// Attribute specifc to Language values
    /// </summary>
    /// <remarks>
    /// Some languages have multiple proper names. Should all be supported?
    /// </remarks>
    public class LanguageAttribute : HumanReadableAttribute
    {
        /// <summary>
        /// ISO 639-1 Code
        /// </summary>
        public string TwoLetterCode { get; set; } = null;

        /// <summary>
        /// ISO 639-2 Code (Standard or Bibliographic)
        /// </summary>
        public string ThreeLetterCode { get; set; } = null;

        /// <summary>
        /// ISO 639-2 Code (Terminology)
        /// </summary>
        public string ThreeLetterCodeAlt { get; set; } = null;
    }
}