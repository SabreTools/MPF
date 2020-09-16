using System.Collections.Generic;
using System.IO;

/// <remarks>
/// Information sourced from http://web.archive.org/web/20070221154246/http://www.goldenhawk.com/download/cdrwin.pdf
/// </remarks>
namespace DICUI.CueSheets
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
        public string FileName { get; set; }

        /// <summary>
        /// filetype
        /// </summary>
        public CueFileType FileType { get; set; }

        /// <summary>
        /// List of TRACK in FILE
        /// </summary>
        public List<CueTrack> Tracks { get; set; }

        /// <summary>
        /// Create an empty FILE
        /// </summary>
        public CueFile()
        {
        }

        /// <summary>
        /// Fill a FILE from a stream reader
        /// </summary>
        /// <param name="fileName">File name to set</param>
        /// <param name="fileType">File type to set</param>
        /// <param name="cueLines">Lines array to pull from</param>
        /// <param name="i">Reference to index in array</param>
        public CueFile(string fileName, string fileType, string[] cueLines, ref int i)
        {
            if (cueLines == null || i < 0 || i > cueLines.Length)
                return; // TODO: Make this throw an exception

            // Set the current fields
            this.FileName = fileName.Trim('"');
            this.FileType = GetFileType(fileType);

            // Increment to start
            i++;

            for (; i < cueLines.Length; i++)
            {
                string line = cueLines[i].Trim();
                string[] splitLine = line.Split(' ');

                // If we have an empty line, we skip
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                switch (splitLine[0])
                {
                    // Read comments
                    case "REM":
                        // We ignore all comments for now
                        break;

                    // Read track information
                    case "TRACK":
                        if (splitLine.Length < 3)
                            continue; // TODO: Make this throw an exception

                        if (this.Tracks == null)
                            this.Tracks = new List<CueTrack>();

                        var track = new CueTrack(splitLine[1], splitLine[2], cueLines, ref i);
                        if (track == default)
                            continue; // TODO: Make this throw an exception

                        this.Tracks.Add(track);
                        break;

                    // Default means return
                    default:
                        i--;
                        return;
                }
            }
        }

        /// <summary>
        /// Get the file type from a given string
        /// </summary>
        /// <param name="fileType">String to get value from</param>
        /// <returns>CueFileType, if possible (default BINARY)</returns>
        private CueFileType GetFileType(string fileType)
        {
            switch (fileType.ToLowerInvariant())
            {
                case "binary":
                    return CueFileType.BINARY;

                case "motorola":
                    return CueFileType.MOTOROLA;

                case "aiff":
                    return CueFileType.AIFF;

                case "wave":
                    return CueFileType.WAVE;

                case "mp3":
                    return CueFileType.MP3;

                default:
                    return CueFileType.BINARY;
            }
        }
    }
}
