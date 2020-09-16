/// <remarks>
/// Information sourced from http://web.archive.org/web/20070221154246/http://www.goldenhawk.com/download/cdrwin.pdf
/// </remarks>
namespace DICUI.Library.CueSheets
{
    /// <summary>
    /// Represents a single INDEX in a TRACK
    /// </summary>
    public class CueIndex
    {
        /// <summary>
        /// number, between 0 and 99
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Starting time of index in minutes
        /// </summary>
        public int Minutes { get; set; }

        /// <summary>
        /// Starting time of index in seconds
        /// </summary>
        /// <remarks>There are 60 seconds in a minute</remarks>
        public int Seconds { get; set; }

        /// <summary>
        /// Starting time of index in frames.
        /// </summary>
        /// <remarks>There are 75 frames per second</remarks>
        public int Frames { get; set; }
    }
}
