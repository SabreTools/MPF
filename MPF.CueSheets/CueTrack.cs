using System;
using System.Collections.Generic;
using System.IO;

/// <remarks>
/// Information sourced from http://web.archive.org/web/20070221154246/http://www.goldenhawk.com/download/cdrwin.pdf
/// </remarks>
namespace MPF.CueSheets
{
    /// <summary>
    /// Track datatype
    /// </summary>
    public enum CueTrackDataType
    {
        /// <summary>
        /// AUDIO, Audio/Music (2352)
        /// </summary>
        AUDIO,

        /// <summary>
        /// CDG, Karaoke CD+G (2448)
        /// </summary>
        CDG,

        /// <summary>
        /// MODE1/2048, CD-ROM Mode1 Data (cooked)
        /// </summary>
        MODE1_2048,

        /// <summary>
        /// MODE1/2352 CD-ROM Mode1 Data (raw)
        /// </summary>
        MODE1_2352,

        /// <summary>
        /// MODE2/2336, CD-ROM XA Mode2 Data
        /// </summary>
        MODE2_2336,

        /// <summary>
        /// MODE2/2352, CD-ROM XA Mode2 Data
        /// </summary>
        MODE2_2352,

        /// <summary>
        /// CDI/2336, CD-I Mode2 Data
        /// </summary>
        CDI_2336,

        /// <summary>
        /// CDI/2352, CD-I Mode2 Data
        /// </summary>
        CDI_2352,
    }

    /// <summary>
    /// Special subcode flags within a track
    /// </summary>
    [Flags]
    public enum CueTrackFlag
    {
        /// <summary>
        /// DCP, Digital copy permitted
        /// </summary>
        DCP = 1 << 0,

        /// <summary>
        /// 4CH, Four channel audio
        /// </summary>
        FourCH = 1 << 1,

        /// <summary>
        /// PRE, Pre-emphasis enabled (audio tracks only)
        /// </summary>
        PRE = 1 << 2,

        /// <summary>
        /// SCMS, Serial Copy Management System (not supported by all recorders)
        /// </summary>
        SCMS = 1 << 3,

        /// <summary>
        /// DATA, set for data files. This flag is set automatically based on the track’s filetype
        /// </summary>
        DATA = 1 << 4,
    }

    /// <summary>
    /// Represents a single TRACK in a FILE
    /// </summary>
    public class CueTrack
    {
        /// <summary>
        /// Track number. The range is 1 to 99.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Track datatype
        /// </summary>
        public CueTrackDataType DataType { get; set; }

        /// <summary>
        /// FLAGS
        /// </summary>
        public CueTrackFlag Flags { get; set; }

        /// <summary>
        /// ISRC
        /// </summary>
        /// <remarks>12 characters in length</remarks>
        public string ISRC { get; set; }

        /// <summary>
        /// PERFORMER
        /// </summary>
        public string Performer { get; set; }

        /// <summary>
        /// SONGWRITER
        /// </summary>
        public string Songwriter { get; set; }

        /// <summary>
        /// TITLE
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// PREGAP
        /// </summary>
        public PreGap PreGap { get; set; }

        /// <summary>
        /// List of INDEX in TRACK
        /// </summary>
        /// <remarks>Must start with 0 or 1 and then sequential</remarks>
        public List<CueIndex> Indices { get; set; }

        /// <summary>
        /// POSTGAP
        /// </summary>
        public PostGap PostGap { get; set; }

        /// <summary>
        /// Create an empty TRACK
        /// </summary>
        public CueTrack()
        {
        }

        /// <summary>
        /// Fill a TRACK from an array of lines
        /// </summary>
        /// <param name="number">Number to set</param>
        /// <param name="dataType">Data type to set</param>
        /// <param name="cueLines">Lines array to pull from</param>
        /// <param name="i">Reference to index in array</param>
        /// <param name="throwOnError">True if errors throw an exception, false otherwise</param>
        public CueTrack(string number, string dataType, string[] cueLines, ref int i, bool throwOnError = false)
        {
            if (cueLines == null)
            {
                if (throwOnError)
                    throw new ArgumentNullException(nameof(cueLines));

                return;
            }
            else if (i < 0 || i > cueLines.Length)
            {
                if (throwOnError)
                    throw new IndexOutOfRangeException();

                return;
            }

            // Set the current fields
            if (!int.TryParse(number, out int parsedNumber))
            {
                if (throwOnError)
                    throw new ArgumentException($"Number was not a number: {number}");

                return;
            }
            else if (parsedNumber < 1 || parsedNumber > 99)
            {
                if (throwOnError)
                    throw new IndexOutOfRangeException($"Index must be between 1 and 99: {parsedNumber}");

                return;
            }

            this.Number = parsedNumber;
            this.DataType = GetDataType(dataType);

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

                    // Read flag information
                    case "FLAGS":
                        if (splitLine.Length < 2)
                        {
                            if (throwOnError)
                                throw new FormatException($"FLAGS line malformed: {line}");

                            continue;
                        }

                        this.Flags = GetFlags(splitLine);
                        break;

                    // Read International Standard Recording Code
                    case "ISRC":
                        if (splitLine.Length < 2)
                        {
                            if (throwOnError)
                                throw new FormatException($"ISRC line malformed: {line}");

                            continue;
                        }

                        this.ISRC = splitLine[1];
                        break;

                    // Read CD-Text enhanced performer
                    case "PERFORMER":
                        if (splitLine.Length < 2)
                        {
                            if (throwOnError)
                                throw new FormatException($"PERFORMER line malformed: {line}");

                            continue;
                        }

                        this.Performer = splitLine[1];
                        break;

                    // Read CD-Text enhanced songwriter
                    case "SONGWRITER":
                        if (splitLine.Length < 2)
                        {
                            if (throwOnError)
                                throw new FormatException($"SONGWRITER line malformed: {line}");

                            continue;
                        }

                        this.Songwriter = splitLine[1];
                        break;

                    // Read CD-Text enhanced title
                    case "TITLE":
                        if (splitLine.Length < 2)
                        {
                            if (throwOnError)
                                throw new FormatException($"TITLE line malformed: {line}");

                            continue;
                        }

                        this.Title = splitLine[1];
                        break;

                    // Read pregap information
                    case "PREGAP":
                        if (splitLine.Length < 2)
                        {
                            if (throwOnError)
                                throw new FormatException($"PREGAP line malformed: {line}");

                            continue;
                        }

                        var pregap = new PreGap(splitLine[1]);
                        if (pregap == default)
                        {
                            if (throwOnError)
                                throw new FormatException($"PREGAP line malformed: {line}");

                            continue;
                        }

                        this.PreGap = pregap;
                        break;

                    // Read index information
                    case "INDEX":
                        if (splitLine.Length < 3)
                        {
                            if (throwOnError)
                                throw new FormatException($"INDEX line malformed: {line}");

                            continue;
                        }

                        if (this.Indices == null)
                            this.Indices = new List<CueIndex>();

                        var index = new CueIndex(splitLine[1], splitLine[2]);
                        if (index == default)
                        {
                            if (throwOnError)
                                throw new FormatException($"INDEX line malformed: {line}");

                            continue;
                        }

                        this.Indices.Add(index);
                        break;

                    // Read postgap information
                    case "POSTGAP":
                        if (splitLine.Length < 2)
                        {
                            if (throwOnError)
                                throw new FormatException($"POSTGAP line malformed: {line}");

                            continue;
                        }

                        var postgap = new PostGap(splitLine[1]);
                        if (postgap == default)
                        {
                            if (throwOnError)
                                throw new FormatException($"POSTGAP line malformed: {line}");

                            continue;
                        }

                        this.PostGap = postgap;
                        break;

                    // Default means return
                    default:
                        i--;
                        return;
                }
            }
        }

        /// <summary>
        /// Write the TRACK out to a stream
        /// </summary>
        /// <param name="sw">StreamWriter to write to</param
        /// <param name="throwOnError">True if errors throw an exception, false otherwise</param>
        public void Write(StreamWriter sw, bool throwOnError = false)
        {
            // If we don't have any indices, it's invalid
            if (this.Indices == null)
            {
                if (throwOnError)
                    throw new ArgumentNullException(nameof(this.Indices));

                return;
            }
            else if (this.Indices.Count == 0)
            {
                if (throwOnError)
                    throw new ArgumentException("No indices provided to write");

                return;
            }

            sw.WriteLine($"  TRACK {this.Number:D2} {FromDataType(this.DataType)}");

            if (this.Flags != 0)
                sw.WriteLine($"    FLAGS {FromFlags(this.Flags)}");

            if (!string.IsNullOrEmpty(this.ISRC))
                sw.WriteLine($"ISRC {this.ISRC}");

            if (!string.IsNullOrEmpty(this.Performer))
                sw.WriteLine($"PERFORMER {this.Performer}");

            if (!string.IsNullOrEmpty(this.Songwriter))
                sw.WriteLine($"SONGWRITER {this.Songwriter}");

            if (!string.IsNullOrEmpty(this.Title))
                sw.WriteLine($"TITLE {this.Title}");

            if (this.PreGap != null)
                this.PreGap.Write(sw);

            foreach (var index in Indices)
            {
                index.Write(sw);
            }

            if (this.PostGap != null)
                this.PostGap.Write(sw);
        }

        /// <summary>
        /// Get the data type from a given string
        /// </summary>
        /// <param name="dataType">String to get value from</param>
        /// <returns>CueTrackDataType, if possible (default AUDIO)</returns>
        private CueTrackDataType GetDataType(string dataType)
        {
            switch (dataType.ToLowerInvariant())
            {
                case "audio":
                    return CueTrackDataType.AUDIO;

                case "cdg":
                    return CueTrackDataType.CDG;

                case "mode1/2048":
                    return CueTrackDataType.MODE1_2048;

                case "mode1/2352":
                    return CueTrackDataType.MODE1_2352;

                case "mode2/2336":
                    return CueTrackDataType.MODE2_2336;

                case "mode2/2352":
                    return CueTrackDataType.MODE2_2352;

                case "cdi/2336":
                    return CueTrackDataType.CDI_2336;

                case "cdi/2352":
                    return CueTrackDataType.CDI_2352;

                default:
                    return CueTrackDataType.AUDIO;
            }
        }

        /// <summary>
        /// Get the string from a given data type
        /// </summary>
        /// <param name="dataType">CueTrackDataType to get value from</param>
        /// <returns>string, if possible</returns>
        private string FromDataType(CueTrackDataType dataType)
        {
            switch (dataType)
            {
                case CueTrackDataType.AUDIO:
                    return "AUDIO";

                case CueTrackDataType.CDG:
                    return "CDG";

                case CueTrackDataType.MODE1_2048:
                    return "MODE1/2048";

                case CueTrackDataType.MODE1_2352:
                    return "MODE1/2352";

                case CueTrackDataType.MODE2_2336:
                    return "MODE2/2336";

                case CueTrackDataType.MODE2_2352:
                    return "MODE2/2352";

                case CueTrackDataType.CDI_2336:
                    return "CDI/2336";

                case CueTrackDataType.CDI_2352:
                    return "CDI/2352";

                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Get the flag value for an array of strings
        /// </summary>
        /// <param name="flagStrings">Possible flags as strings</param>
        /// <returns>CueTrackFlag value representing the strings, if possible</returns>
        private CueTrackFlag GetFlags(string[] flagStrings)
        {
            CueTrackFlag flag = 0;

            foreach (string flagString in flagStrings)
            {
                switch (flagString.ToLowerInvariant())
                {
                    case "flags":
                        // No-op since this is the start of the line
                        break;

                    case "dcp":
                        flag |= CueTrackFlag.DCP;
                        break;

                    case "4ch":
                        flag |= CueTrackFlag.FourCH;
                        break;

                    case "pre":
                        flag |= CueTrackFlag.PRE;
                        break;

                    case "scms":
                        flag |= CueTrackFlag.SCMS;
                        break;

                    case "data":
                        flag |= CueTrackFlag.DATA;
                        break;
                }
            }

            return flag;
        }

        /// <summary>
        /// Get the string value for a set of track flags
        /// </summary>
        /// <param name="flags">CueTrackFlag to get value from</param>
        /// <returns>String value representing the CueTrackFlag, if possible</returns>
        private string FromFlags(CueTrackFlag flags)
        {
            string outputFlagString = string.Empty;

            if (flags.HasFlag(CueTrackFlag.DCP))
                outputFlagString += "DCP ";

            if (flags.HasFlag(CueTrackFlag.FourCH))
                outputFlagString += "4CH ";

            if (flags.HasFlag(CueTrackFlag.PRE))
                outputFlagString += "PRE ";

            if (flags.HasFlag(CueTrackFlag.SCMS))
                outputFlagString += "SCMS ";

            if (flags.HasFlag(CueTrackFlag.DATA))
                outputFlagString += "DATA ";

            return outputFlagString.Trim();
        }
    }
}
