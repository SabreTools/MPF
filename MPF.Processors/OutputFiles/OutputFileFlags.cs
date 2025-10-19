using System;

namespace MPF.Processors.OutputFiles
{
    /// <summary>
    /// Represents attributes about an <see cref="OutputFile"/>
    /// </summary>
    [Flags]
    public enum OutputFileFlags : ushort
    {
        /// <summary>
        /// Default state
        /// </summary>
        None = 0x0000,

        /// <summary>
        /// File is required to exist
        /// </summary>
        Required = 0x0001,

        /// <summary>
        /// File is included as an artifact
        /// </summary>
        Artifact = 0x0002,

        /// <summary>
        /// File is included as a binary artifact
        /// </summary>
        Binary = 0x0004,

        /// <summary>
        /// File can be deleted after processing
        /// </summary>
        Deleteable = 0x0008,

        /// <summary>
        /// File can be zipped after processing
        /// </summary>
        Zippable = 0x0010,

        /// <summary>
        /// File should be preserved after zipping
        /// </summary>
        Preserve = 0x0020,
    }
}
