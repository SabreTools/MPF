/// <remarks>
/// Information sourced from http://web.archive.org/web/20070221154246/http://www.goldenhawk.com/download/cdrwin.pdf
/// </remarks>
namespace DICUI.Library.CueSheets
{
    /// <summary>
    /// Represents pregap information of a track
    /// </summary>
    public class PreGap
    {
        /// <summary>
        /// Length of postgap in minutes
        /// </summary>
        public int Minutes { get; set; }

        /// <summary>
        /// Length of postgap in seconds
        /// </summary>
        /// <remarks>There are 60 seconds in a minute</remarks>
        public int Seconds { get; set; }

        /// <summary>
        /// Length of postgap in frames.
        /// </summary>
        /// <remarks>There are 75 frames per second</remarks>
        public int Frames { get; set; }
    }
}
