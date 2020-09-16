using System;
using System.Collections.Generic;
using System.IO;

/// <remarks>
/// Information sourced from http://web.archive.org/web/20070221154246/http://www.goldenhawk.com/download/cdrwin.pdf
/// </remarks>
namespace DICUI.CueSheets
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
        /// Fill a TRACK from a stream reader
        /// </summary>
        /// <param name="number">Number to set</param>
        /// <param name="dataType">Data type to set</param>
        /// <param name="sr">StreamReader to pull from</param>
        public CueTrack(string number, string dataType, StreamReader sr)
        {
            if (sr == null)
                return;

            // Set the current fields
            if (!int.TryParse(number, out int parsedNumber))
                return; // TODO: Make this throw an exception
            else if (parsedNumber < 1 || parsedNumber > 99)
                return; // TODO: Make this throw an exception

            this.Number = parsedNumber;
            this.DataType = GetDataType(dataType);

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine().Trim();
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
                            continue; // TODO: Make this throw an exception

                        this.Flags = GetFlags(splitLine);
                        break;

                    // Read International Standard Recording Code
                    case "ISRC":
                        if (splitLine.Length < 2)
                            continue; // TODO: Make this throw an exception

                        this.ISRC = splitLine[1];
                        break;

                    // Read CD-Text enhanced performer
                    case "PERFORMER":
                        if (splitLine.Length < 2)
                            continue; // TODO: Make this throw an exception

                        this.Performer = splitLine[1];
                        break;

                    // Read CD-Text enhanced songwriter
                    case "SONGWRITER":
                        if (splitLine.Length < 2)
                            continue; // TODO: Make this throw an exception

                        this.Songwriter = splitLine[1];
                        break;

                    // Read CD-Text enhanced title
                    case "TITLE":
                        if (splitLine.Length < 2)
                            continue; // TODO: Make this throw an exception

                        this.Title = splitLine[1];
                        break;

                    // Read pregap information
                    case "PREGAP":
                        if (splitLine.Length < 2)
                            continue; // TODO: Make this throw an exception

                        var pregap = new PreGap(splitLine[1]);
                        if (pregap == default)
                            continue; // TODO: Make this throw an exception

                        this.PreGap = pregap;
                        break;

                    // Read index information
                    case "INDEX":
                        if (splitLine.Length < 3)
                            continue; // TODO: Make this throw an exception

                        if (this.Indices == null)
                            this.Indices = new List<CueIndex>();

                        var index = new CueIndex(splitLine[1], splitLine[2]);
                        if (index == default)
                            continue; // TODO: Make this throw an exception

                        this.Indices.Add(index);
                        break;

                    // Read postgap information
                    case "POSTGAP":
                        if (splitLine.Length < 2)
                            continue; // TODO: Make this throw an exception

                        var postgap = new PostGap(splitLine[1]);
                        if (postgap == default)
                            continue; // TODO: Make this throw an exception

                        this.PostGap = postgap;
                        break;

                    // Default means return
                    default:
                        return;
                }
            }
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
        /// Get the flag value for an array of strings
        /// </summary>
        /// <param name="flagStrings">Possible flags as strings</param>
        /// <returns>CueTrackFlag value representing the strings, if possible</returns>
        private CueTrackFlag GetFlags(string[] flagStrings)
        {
            CueTrackFlag flag = 0;

            foreach (string flagString in flagStrings)
            {
                // TODO: Make default throw an exception
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
    }
}
