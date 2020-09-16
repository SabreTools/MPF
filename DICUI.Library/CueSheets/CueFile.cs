using System;
using System.Collections.Generic;

/// <remarks>
/// Information sourced from http://web.archive.org/web/20070221154246/http://www.goldenhawk.com/download/cdrwin.pdf
/// </remarks>
namespace DICUI.Library.CueSheets
{
    /// <summary>
    /// The audio or data file’s filetype
    /// </summary>
    public enum CueFileType
    {
        /// <summary>
        /// Intel binary file (least significant byte first). Use for data files.
        /// </summary>
        BINARY,

        /// <summary>
        /// Motorola binary file (most significant byte first). Use for data files.
        /// </summary>
        MOTOROLA,

        /// <summary>
        /// Audio AIFF file (44.1KHz 16-bit stereo)
        /// </summary>
        AIFF,

        /// <summary>
        /// Audio WAVE file (44.1KHz 16-bit stereo)
        /// </summary>
        WAVE,

        /// <summary>
        /// Audio MP3 file (44.1KHz 16-bit stereo)
        /// </summary>
        MP3,
    }

    /// <summary>
    /// Represents a single FILE in a cuesheet
    /// </summary>
    public class CueFile
    {
        /// <summary>
        /// filename
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// filetype
        /// </summary>
        public CueFileType FileType { get; set; }

        /// <summary>
        /// List of TRACK in FILE
        /// </summary>
        public List<CueTrack> Tracks { get; set; }
    }
}
