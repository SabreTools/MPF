using System;
using System.Collections.Generic;

/// <remarks>
/// Information sourced from http://web.archive.org/web/20070221154246/http://www.goldenhawk.com/download/cdrwin.pdf
/// </remarks>
namespace DICUI.Library.CueSheets
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
    }
}
