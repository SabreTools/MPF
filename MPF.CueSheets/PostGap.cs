using System;
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
        /// <param name="throwOnError">True if errors throw an exception, false otherwise</param>
        public PostGap(string length, bool throwOnError = false)
        {
            // Ignore empty lines
            if (string.IsNullOrWhiteSpace(length))
            {
                if (throwOnError)
                    throw new ArgumentException("Length was null or whitespace");

                return;
            }

            // Ignore lines that don't contain the correct information
            if (length.Length != 8 || length.Count(c => c == ':') != 2)
            {
                if (throwOnError)
                    throw new FormatException($"Length was not in a recognized format: {length}");

                return;
            }

            // Split the line
            string[] splitLength = length.Split(':');
            if (splitLength.Length != 3)
            {
                if (throwOnError)
                    throw new FormatException($"Length was not in a recognized format: {length}");

                return;
            }

            // Parse the lengths
            int[] lengthSegments = new int[3];

            // Minutes
            if (!int.TryParse(splitLength[0], out lengthSegments[0]))
            {
                if (throwOnError)
                    throw new FormatException($"Minutes segment was not a number: {splitLength[0]}");

                return;
            }
            else if (lengthSegments[0] < 0)
            {
                if (throwOnError)
                    throw new IndexOutOfRangeException($"Minutes segment must be 0 or greater: {lengthSegments[0]}");

                return;
            }

            // Seconds
            if (!int.TryParse(splitLength[1], out lengthSegments[1]))
            {
                if (throwOnError)
                    throw new FormatException($"Seconds segment was not a number: {splitLength[1]}");

                return;
            }
            else if (lengthSegments[1] < 0 || lengthSegments[1] > 60)
            {
                if (throwOnError)
                    throw new IndexOutOfRangeException($"Seconds segment must be between 0 and 60: {lengthSegments[1]}");

                return;
            }

            // Frames
            if (!int.TryParse(splitLength[2], out lengthSegments[2]))
            {
                if (throwOnError)
                    throw new FormatException($"Frames segment was not a number: {splitLength[2]}");

                return;
            }
            else if (lengthSegments[2] < 0 || lengthSegments[2] > 75)
            {
                if (throwOnError)
                    throw new IndexOutOfRangeException($"Frames segment must be between 0 and 75: {lengthSegments[2]}");

                return;
            }

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
