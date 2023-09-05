using SabreTools.RedumpLib.Data;

namespace MPF.Core.Data
{
    /// <summary>
    /// Contains information specific to an XGD disc
    /// </summary>
    /// <remarks>
    /// XGD1 XMID Format Information:
    ///
    /// AABBBCCD
    /// - AA        => The two-ASCII-character publisher identifier (see GetPublisher for details)
    /// - BBB       => Game ID
    /// - CC        => Version number
    /// - D         => Region identifier (see GetRegion for details)
    ///
    /// XGD2/3 XeMID Format Information:
    ///
    /// AABCCCDDEFFGHH(IIIIIIII)
    /// - AA        => The two-ASCII-character publisher identifier (see GetPublisher for details)
    /// - B         => Platform identifier; 2 indicates Xbox 360.
    /// - CCC       => Game ID
    /// - DD        => SKU number (unique per SKU of a title)
    /// - E         => Region identifier (see GetRegion for details)
    /// - FF        => Base version; usually starts at 01 (can be 1 or 2 characters)
    /// - G         => Media type identifier (see GetMediaSubtype for details)
    /// - HH        => Disc number stored in [disc number][total discs] format
    /// - IIIIIIII  => 8-hex-digit certification submission identifier; usually on test discs only
    /// </remarks>
    public class XgdInfo
    {
        #region Fields

        /// <summary>
        /// Indicates whether the information in this object is fully instantiated or not
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        /// Raw XMID/XeMID string that all other information is derived from
        /// </summary>
        public string XMID { get; private set; }

        /// <summary>
        /// 2-character publisher identifier
        /// </summary>
        public string PublisherIdentifier { get; private set; }

        /// <summary>
        /// Platform disc is made for, 2 indicates Xbox 360
        /// </summary>
        public char? PlatformIdentifier { get; private set; }

        /// <summary>
        /// Game ID
        /// </summary>
        public string GameID { get; private set; }

        /// <summary>
        /// For XGD1:   Internal version number
        /// For XGD2/3: Title-specific SKU
        /// </summary>
        public string SKU { get; private set; }

        /// <summary>
        /// Region identifier character
        /// </summary>
        public char RegionIdentifier { get; private set; }

        /// <summary>
        /// Base version of executables, usually starts at 01
        /// </summary>
        /// <remarks>
        /// TODO: Check if this is always 2 characters for XGD2/3
        /// </remarks>
        public string BaseVersion { get; private set; }

        /// <summary>
        /// Media subtype identifier
        /// </summary>
        public char MediaSubtypeIdentifier { get; private set; }

        /// <summary>
        /// Disc number stored in [disc number][total discs] format
        /// </summary>
        public string DiscNumberIdentifier { get; private set; }

        /// <summary>
        /// 8-hex-digit certification submission identifier; usually on test discs only
        /// </summary>
        public string CertificationSubmissionIdentifier { get; private set; }

        #endregion

        #region Auto-Generated Information

        /// <summary>
        /// Human-readable name derived from the publisher identifier
        /// </summary>
        public string PublisherName => GetPublisher(this.PublisherIdentifier);

        /// <summary>
        /// Internally represented region
        /// </summary>
        public Region? InternalRegion => GetRegion(this.RegionIdentifier);

        /// <summary>
        /// Human-readable subtype derived from the media identifier
        /// </summary>
        public string MediaSubtype => GetMediaSubtype(this.MediaSubtypeIdentifier);

        #endregion

        /// <summary>
        /// Populate a set of XGD information from a Master ID (XMID/XeMID) string
        /// </summary>
        /// <param name="xmid">XMID/XeMID string representing the DMI information</param>
        /// <param name="validate">True if value validation should be performed, false otherwise</param>
        public XgdInfo(string xmid, bool validate = false)
        {
            this.Initialized = false;
            if (string.IsNullOrWhiteSpace(xmid))
                return;

            this.XMID = xmid.TrimEnd('\0');
            if (string.IsNullOrWhiteSpace(this.XMID))
                return;

            // XGD1 information is 8 characters
            if (this.XMID.Length == 8)
                this.Initialized = ParseXGD1XMID(this.XMID, validate);

            // XGD2/3 information is semi-variable length
            else if (this.XMID.Length == 13 || this.XMID.Length == 14 || this.XMID.Length == 21 || this.XMID.Length == 22)
                this.Initialized = ParseXGD23XeMID(this.XMID, validate);
        }

        /// <summary>
        /// Get the human-readable serial string
        /// </summary>
        /// <returns>Formatted serial string, null on error</returns>
        public string GetSerial()
        {
            if (!this.Initialized)
                return null;

            try
            {
                // XGD1 doesn't use PlatformIdentifier
                if (this.PlatformIdentifier == null)
                    return $"{this.PublisherIdentifier}-{this.GameID}";

                // XGD2/3 uses a specific identifier
                else if (this.PlatformIdentifier == '2')
                    return $"{this.PublisherIdentifier}-{this.PlatformIdentifier}{this.GameID}";

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get the human-readable version string
        /// </summary>
        /// <returns>Formatted version string, null on error</returns>
        /// <remarks>This may differ for XGD2/3 in the future</remarks>
        public string GetVersion()
        {
            if (!this.Initialized)
                return null;

            try
            {
                // XGD1 doesn't use PlatformIdentifier
                if (this.PlatformIdentifier == null)
                    return $"1.{this.SKU}";

                // XGD2/3 uses a specific identifier
                else if (this.PlatformIdentifier == '2')
                    return $"1.{this.SKU}";

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Parse an XGD1 XMID string
        /// </summary>
        /// <param name="xmid">XMID string to attempt to parse</param>
        /// <param name="validate">True if value validation should be performed, false otherwise</param>
        /// <returns>True if the XMID could be parsed, false otherwise</returns>
        private bool ParseXGD1XMID(string xmid, bool validate)
        {
            if (xmid == null || xmid.Length != 8)
                return false;

            this.PublisherIdentifier = xmid.Substring(0, 2);
            if (validate && string.IsNullOrEmpty(this.PublisherName))
                return false;

            this.GameID = xmid.Substring(2, 3);
            this.SKU = xmid.Substring(5, 2);
            this.RegionIdentifier = xmid[7];
            if (validate && this.InternalRegion == null)
                return false;

            return true;
        }

        /// <summary>
        /// Parse an XGD2/3 XeMID string
        /// </summary>
        /// <param name="xemid">XeMID string to attempt to parse</param>
        /// <param name="validate">True if value validation should be performed, false otherwise</param>
        /// <returns>True if the XeMID could be parsed, false otherwise</returns>
        private bool ParseXGD23XeMID(string xemid, bool validate)
        {
            if (xemid == null
                || (xemid.Length != 13 && xemid.Length != 14
                    && xemid.Length != 21 && xemid.Length != 22))
                return false;

            this.PublisherIdentifier = xemid.Substring(0, 2);
            if (validate && string.IsNullOrEmpty(this.PublisherName))
                return false;

            this.PlatformIdentifier = xemid[2];
            if (validate && this.PlatformIdentifier != '2')
                return false;

            this.GameID = xemid.Substring(3, 3);
            this.SKU = xemid.Substring(6, 2);
            this.RegionIdentifier = xemid[8];
            if (validate && this.InternalRegion == null)
                return false;

            if (xemid.Length == 13 || xemid.Length == 21)
            {
                this.BaseVersion = xemid.Substring(9, 1);
                this.MediaSubtypeIdentifier = xemid[10];
                if (validate && string.IsNullOrEmpty(this.MediaSubtype))
                    return false;

                this.DiscNumberIdentifier = xemid.Substring(11, 2);
            }
            else if (xemid.Length == 14 || xemid.Length == 22)
            {
                this.BaseVersion = xemid.Substring(9, 2);
                this.MediaSubtypeIdentifier = xemid[11];
                if (validate && string.IsNullOrEmpty(this.MediaSubtype))
                    return false;

                this.DiscNumberIdentifier = xemid.Substring(12, 2);
            }

            if (xemid.Length == 21)
                this.CertificationSubmissionIdentifier = xemid.Substring(13);
            else if (xemid.Length == 22)
                this.CertificationSubmissionIdentifier = xemid.Substring(14);

            return true;
        }

        #region Helpers

        /// <summary>
        /// Determine the XGD type based on the XGD2/3 media type identifier character
        /// </summary>
        /// <param name="mediaTypeIdentifier">Character denoting the media type</param>
        /// <returns>Media subtype as a string, if possible</returns>
        private static string GetMediaSubtype(char mediaTypeIdentifier)
        {
            switch (mediaTypeIdentifier)
            {
                case 'F': return "XGD3";
                case 'X': return "XGD2";
                case 'Z': return "Games on Demand / Marketplace Demo";
                default: return null;
            }
        }

        /// <summary>
        /// Get the full name of the publisher from the 2-character identifier
        /// </summary>
        /// <param name="publisherIdentifier">Case-sensitive 2-character identifier</param>
        /// <returns>Publisher name, if possible</returns>
        /// <see cref="https://xboxdevwiki.net/Xbe#Title_ID"/>
        private static string GetPublisher(string publisherIdentifier)
        {
            switch (publisherIdentifier)
            {
                case "AC": return "Acclaim Entertainment";
                case "AH": return "ARUSH Entertainment";
                case "AQ": return "Aqua System";
                case "AS": return "ASK";
                case "AT": return "Atlus";
                case "AV": return "Activision";
                case "AY": return "Aspyr Media";
                case "BA": return "Bandai";
                case "BL": return "Black Box";
                case "BM": return "BAM! Entertainment";
                case "BR": return "Broccoli Co.";
                case "BS": return "Bethesda Softworks";
                case "BU": return "Bunkasha Co.";
                case "BV": return "Buena Vista Games";
                case "BW": return "BBC Multimedia";
                case "BZ": return "Blizzard";
                case "CC": return "Capcom";
                case "CK": return "Kemco Corporation"; // TODO: Confirm
                case "CM": return "Codemasters";
                case "CV": return "Crave Entertainment";
                case "DC": return "DreamCatcher Interactive";
                case "DX": return "Davilex";
                case "EA": return "Electronic Arts (EA)";
                case "EC": return "Encore inc";
                case "EL": return "Enlight Software";
                case "EM": return "Empire Interactive";
                case "ES": return "Eidos Interactive";
                case "FI": return "Fox Interactive";
                case "FS": return "From Software";
                case "GE": return "Genki Co.";
                case "GV": return "Groove Games";
                case "HE": return "Tru Blu (Entertainment division of Home Entertainment Suppliers)";
                case "HP": return "Hip games";
                case "HU": return "Hudson Soft";
                case "HW": return "Highwaystar";
                case "IA": return "Mad Catz Interactive";
                case "IF": return "Idea Factory";
                case "IG": return "Infogrames";
                case "IL": return "Interlex Corporation";
                case "IM": return "Imagine Media";
                case "IO": return "Ignition Entertainment";
                case "IP": return "Interplay Entertainment";
                case "IX": return "InXile Entertainment"; // TODO: Confirm
                case "JA": return "Jaleco";
                case "JW": return "JoWooD";
                case "KB": return "Kemco"; // TODO: Confirm
                case "KI": return "Kids Station Inc."; // TODO: Confirm
                case "KN": return "Konami";
                case "KO": return "KOEI";
                case "KU": return "Kobi and / or GAE (formerly Global A Entertainment)"; // TODO: Confirm
                case "LA": return "LucasArts";
                case "LS": return "Black Bean Games (publishing arm of Leader S.p.A.)";
                case "MD": return "Metro3D";
                case "ME": return "Medix";
                case "MI": return "Microïds";
                case "MJ": return "Majesco Entertainment";
                case "MM": return "Myelin Media";
                case "MP": return "MediaQuest"; // TODO: Confirm
                case "MS": return "Microsoft Game Studios";
                case "MW": return "Midway Games";
                case "MX": return "Empire Interactive"; // TODO: Confirm
                case "NK": return "NewKidCo";
                case "NL": return "NovaLogic";
                case "NM": return "Namco";
                case "OX": return "Oxygen Interactive";
                case "PC": return "Playlogic Entertainment";
                case "PL": return "Phantagram Co., Ltd.";
                case "RA": return "Rage";
                case "SA": return "Sammy";
                case "SC": return "SCi Games";
                case "SE": return "SEGA";
                case "SN": return "SNK";
                case "SS": return "Simon & Schuster";
                case "SU": return "Success Corporation";
                case "SW": return "Swing! Deutschland";
                case "TA": return "Takara";
                case "TC": return "Tecmo";
                case "TD": return "The 3DO Company (or just 3DO)";
                case "TK": return "Takuyo";
                case "TM": return "TDK Mediactive";
                case "TQ": return "THQ";
                case "TS": return "Titus Interactive";
                case "TT": return "Take-Two Interactive Software";
                case "US": return "Ubisoft";
                case "VC": return "Victor Interactive Software";
                case "VN": return "Vivendi Universal (just took Interplays publishing rights)"; // TODO: Confirm
                case "VU": return "Vivendi Universal Games";
                case "VV": return "Vivendi Universal Games"; // TODO: Confirm
                case "WE": return "Wanadoo Edition";
                case "WR": return "Warner Bros. Interactive Entertainment"; // TODO: Confirm
                case "XI": return "XPEC Entertainment and Idea Factory";
                case "XK": return "Xbox kiosk disk?"; // TODO: Confirm
                case "XL": return "Xbox special bundled or live demo disk?"; // TODO: Confirm
                case "XM": return "Evolved Games"; // TODO: Confirm
                case "XP": return "XPEC Entertainment";
                case "XR": return "Panorama";
                case "YB": return "YBM Sisa (South-Korea)";
                case "ZD": return "Zushi Games (formerly Zoo Digital Publishing)";
                default: return null;
            }
        }

        /// <summary>
        /// Determine the region based on the XGD serial character
        /// </summary>
        /// <param name="region">Character denoting the region</param>
        /// <returns>Region, if possible</returns>
        private static Region? GetRegion(char region)
        {
            switch (region)
            {
                case 'W': return Region.World;
                case 'A': return Region.UnitedStatesOfAmerica;
                case 'J': return Region.JapanAsia;
                case 'E': return Region.Europe;
                case 'K': return Region.USAJapan;
                case 'L': return Region.USAEurope;
                case 'H': return Region.JapanEurope;
                default: return null;
            }
        }

        #endregion
    }
}
