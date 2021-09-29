using System;
using System.IO;
using System.Linq;

/// <remarks>
/// Information sourced from http://web.archive.org/web/20070221154246/http://www.goldenhawk.com/download/cdrwin.pdf
/// </remarks>
namespace MPF.CueSheets
{
    /// <summary>
    /// Represents a single INDEX in a TRACK
    /// </summary>
    public class CueIndex
    {
        /// <summary>
        /// INDEX number, between 0 and 99
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Starting time of INDEX in minutes
        /// </summary>
        public int Minutes { get; set; }

        /// <summary>
        /// Starting time of INDEX in seconds
        /// </summary>
        /// <remarks>There are 60 seconds in a minute</remarks>
        public int Seconds { get; set; }

        /// <summary>
        /// Starting time of INDEX in frames.
        /// </summary>
        /// <remarks>There are 75 frames per second</remarks>
        public int Frames { get; set; }

        /// <summary>
        /// Create an empty INDEX
        /// </summary>
        public CueIndex()
        {
        }

        /// <summary>
        /// Fill a INDEX from an array of lines
        /// </summary>
        /// <param name="index">Index to set</param>
        /// <param name="startTime">Start time to set</param>
        /// <param name="throwOnError">True if errors throw an exception, false otherwise</param>
        public CueIndex(string index, string startTime, bool throwOnError = false)
        {
            // Set the current fields
            if (!int.TryParse(index, out int parsedIndex))
            {
                if (throwOnError)
                    throw new ArgumentException($"Index was not a number: {index}");

                return;
            }
            else if (parsedIndex < 0 || parsedIndex > 99)
            {
                if (throwOnError)
                    throw new IndexOutOfRangeException($"Index must be between 0 and 99: {parsedIndex}");

                return;
            }

            // Ignore empty lines
            if (string.IsNullOrWhiteSpace(startTime))
            {
                if (throwOnError)
                    throw new ArgumentException("Start time was null or whitespace");

                return;
            }

            // Ignore lines that don't contain the correct information
            if (startTime.Length != 8 || startTime.Count(c => c == ':') != 2)
            {
                if (throwOnError)
                    throw new FormatException($"Start time was not in a recognized format: {startTime}");

                return;
            }

            // Split the line
            string[] splitTime = startTime.Split(':');
            if (splitTime.Length != 3)
            {
                if (throwOnError)
                    throw new FormatException($"Start time was not in a recognized format: {startTime}");

                return;
            }

            // Parse the lengths
            int[] lengthSegments = new int[3];

            // Minutes
            if (!int.TryParse(splitTime[0], out lengthSegments[0]))
            {
                if (throwOnError)
                    throw new FormatException($"Minutes segment was not a number: {splitTime[0]}");

                return;
            }
            else if (lengthSegments[0] < 0)
            {
                if (throwOnError)
                    throw new IndexOutOfRangeException($"Minutes segment must be 0 or greater: {lengthSegments[0]}");

                return;
            }

            // Seconds
            if (!int.TryParse(splitTime[1], out lengthSegments[1]))
            {
                if (throwOnError)
                    throw new FormatException($"Seconds segment was not a number: {splitTime[1]}");

                return;
            }
            else if (lengthSegments[1] < 0 || lengthSegments[1] > 60)
            {
                if (throwOnError)
                    throw new IndexOutOfRangeException($"Seconds segment must be between 0 and 60: {lengthSegments[1]}");

                return;
            }

            // Frames
            if (!int.TryParse(splitTime[2], out lengthSegments[2]))
            {
                if (throwOnError)
                    throw new FormatException($"Frames segment was not a number: {splitTime[2]}");

                return;
            }
            else if (lengthSegments[2] < 0 || lengthSegments[2] > 75)
            {
                if (throwOnError)
                    throw new IndexOutOfRangeException($"Frames segment must be between 0 and 75: {lengthSegments[2]}");

                return;
            }

            // Set the values
            this.Index = parsedIndex;
            this.Minutes = lengthSegments[0];
            this.Seconds = lengthSegments[1];
            this.Frames = lengthSegments[2];
        }

        /// <summary>
        /// Write the INDEX out to a stream
        /// </summary>
        /// <param name="sw">StreamWriter to write to</param>
        public void Write(StreamWriter sw)
        {
            sw.WriteLine($"    INDEX {this.Index:D2} {this.Minutes:D2}:{this.Seconds:D2}:{this.Frames:D2}");
        }
    }
}
