using System.IO;
using System.Linq;

/// <remarks>
/// Information sourced from http://web.archive.org/web/20070221154246/http://www.goldenhawk.com/download/cdrwin.pdf
/// </remarks>
namespace MPF.CueSheets
{
    /// <summary>
    /// Represents POSTGAP information of a track
    /// </summary>
    public class PostGap
    {
        /// <summary>
        /// Length of POSTGAP in minutes
        /// </summary>
        public int Minutes { get; set; }

        /// <summary>
        /// Length of POSTGAP in seconds
        /// </summary>
        /// <remarks>There are 60 seconds in a minute</remarks>
        public int Seconds { get; set; }

        /// <summary>
        /// Length of POSTGAP in frames.
        /// </summary>
        /// <remarks>There are 75 frames per second</remarks>
        public int Frames { get; set; }

        /// Create an empty POSTGAP
        /// </summary>
        public PostGap()
        {
        }

        /// <summary>
        /// Create a POSTGAP from a mm:ss:ff length
        /// </summary>
        /// <param name="length">String to get length information from</param>
        public PostGap(string length)
        {
            // Ignore empty lines
            if (string.IsNullOrWhiteSpace(length))
                return; // TODO: Make this throw an exception

            // Ignore lines that don't contain the correct information
            if (length.Length != 8 || length.Count(c => c == ':') != 2)
                return; // TODO: Make this throw an exception

            // Split the line
            string[] splitLength = length.Split(':');
            if (splitLength.Length != 3)
                return; // TODO: Make this throw an exception

            // Parse the lengths
            int[] lengthSegments = new int[3];

            // Minutes
            if (!int.TryParse(splitLength[0], out lengthSegments[0]))
                return; // TODO: Make this throw an exception
            else if (lengthSegments[0] < 0)
                return; // TODO: Make this throw an exception

            // Seconds
            if (!int.TryParse(splitLength[1], out lengthSegments[1]))
                return; // TODO: Make this throw an exception
            else if (lengthSegments[1] < 0 || lengthSegments[1] > 60)
                return; // TODO: Make this throw an exception

            // Frames
            if (!int.TryParse(splitLength[2], out lengthSegments[2]))
                return; // TODO: Make this throw an exception
            else if (lengthSegments[2] < 0 || lengthSegments[2] > 75)
                return; // TODO: Make this throw an exception

            // Set the values
            this.Minutes = lengthSegments[0];
            this.Seconds = lengthSegments[1];
            this.Frames = lengthSegments[2];
        }

        /// <summary>
        /// Write the POSTGAP out to a stream
        /// </summary>
        /// <param name="sw">StreamWriter to write to</param>
        public void Write(StreamWriter sw)
        {
            sw.WriteLine($"    POSTGAP {this.Minutes:D2}:{this.Seconds:D2}:{this.Frames:D2}");
        }
    }
}
