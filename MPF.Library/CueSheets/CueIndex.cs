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
        public CueIndex(string index, string startTime)
        {
            // Set the current fields
            if (!int.TryParse(index, out int parsedIndex))
                return; // TODO: Make this throw an exception
            else if (parsedIndex < 0 || parsedIndex > 99)
                return; // TODO: Make this throw an exception

            // Ignore empty lines
            if (string.IsNullOrWhiteSpace(startTime))
                return; // TODO: Make this throw an exception

            // Ignore lines that don't contain the correct information
            if (startTime.Length != 8 || startTime.Count(c => c == ':') != 2)
                return; // TODO: Make this throw an exception

            // Split the line
            string[] splitTime = startTime.Split(':');
            if (splitTime.Length != 3)
                return; // TODO: Make this throw an exception

            // Parse the lengths
            int[] lengthSegments = new int[3];

            // Minutes
            if (!int.TryParse(splitTime[0], out lengthSegments[0]))
                return; // TODO: Make this throw an exception
            else if (lengthSegments[0] < 0)
                return; // TODO: Make this throw an exception

            // Seconds
            if (!int.TryParse(splitTime[1], out lengthSegments[1]))
                return; // TODO: Make this throw an exception
            else if (lengthSegments[1] < 0 || lengthSegments[1] > 60)
                return; // TODO: Make this throw an exception

            // Frames
            if (!int.TryParse(splitTime[2], out lengthSegments[2]))
                return; // TODO: Make this throw an exception
            else if (lengthSegments[2] < 0 || lengthSegments[2] > 75)
                return; // TODO: Make this throw an exception

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
