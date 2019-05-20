using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DICUI.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace DICUI.Data
{
    public class SubmissionInfo
    {
        #region Common disc info

        // TODO: Name not defined
        [JsonProperty(PropertyName = "d_system", Required = Required.AllowNull)]
        [JsonConverter(typeof(StringEnumConverter))]
        public KnownSystem? System { get; set; }

        // TODO: Name not defined
        [JsonProperty(PropertyName = "d_media", Required = Required.AllowNull)]
        [JsonConverter(typeof(StringEnumConverter))]
        public MediaType? Media { get; set; }

        [JsonProperty(PropertyName = "d_title", Required = Required.AllowNull)]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "d_title_foreign", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ForeignTitleNonLatin { get; set; }

        [JsonProperty(PropertyName = "d_number", NullValueHandling = NullValueHandling.Ignore)]
        public string DiscNumberLetter { get; set; }

        [JsonProperty(PropertyName = "d_label", NullValueHandling = NullValueHandling.Ignore)]
        public string DiscTitle { get; set; }

        [JsonProperty(PropertyName = "d_category", Required = Required.AllowNull)]
        public Category? Category { get; set; }

        [JsonProperty(PropertyName = "d_region", Required = Required.AllowNull)]
        [JsonConverter(typeof(StringEnumConverter))]
        public Region? Region { get; set; }

        [JsonProperty(PropertyName = "d_languages", Required = Required.AllowNull)]
        public Language?[] Languages { get; set; }

        // "Bios settings", "Language selector", "Options menu"
        [JsonProperty(PropertyName = "d_languages_selection", NullValueHandling = NullValueHandling.Ignore)]
        public string[] LanguageSelection { get; set; }

        [JsonProperty(PropertyName = "d_serial", NullValueHandling = NullValueHandling.Ignore)]
        public string Serial { get; set; }

        [JsonProperty(PropertyName = "d_ring", NullValueHandling = NullValueHandling.Ignore)]
        public string Ring { get; }

        [JsonProperty(PropertyName = "d_ring_0_id", NullValueHandling = NullValueHandling.Ignore)]
        public string RingId { get; }

        [JsonProperty(PropertyName = "d_ring_0_ma1", Required = Required.AllowNull)]
        public string MasteringRingFirstLayerDataSide { get; set; }

        [JsonProperty(PropertyName = "d_ring_0_ma1_sid", NullValueHandling = NullValueHandling.Ignore)]
        public string MasteringSIDCodeFirstLayerDataSide { get; set; }

        [JsonProperty(PropertyName = "d_ring_0_ts1", NullValueHandling = NullValueHandling.Ignore)]
        public string ToolstampMasteringCodeFirstLayerDataSide { get; set; }

        [JsonProperty(PropertyName = "d_ring_0_mo1_sid", NullValueHandling = NullValueHandling.Ignore)]
        public string MouldSIDCodeFirstLayerDataSide { get; set; }

        [JsonProperty(PropertyName = "dr_ring_0_mo1", NullValueHandling = NullValueHandling.Ignore)]
        public string AdditionalMouldFirstLayerDataSide { get; set; }

        [JsonProperty(PropertyName = "d_ring_0_ma2", NullValueHandling = NullValueHandling.Ignore)]
        public string MasteringRingSecondLayerLabelSide { get; set; }

        [JsonProperty(PropertyName = "d_ring_0_ma2_sid", NullValueHandling = NullValueHandling.Ignore)]
        public string MasteringSIDCodeSecondLayerLabelSide { get; set; }

        [JsonProperty(PropertyName = "d_ring_0_ts2", NullValueHandling = NullValueHandling.Ignore)]
        public string ToolstampMasteringCodeSecondLayerLabelSide { get; set; }

        [JsonProperty(PropertyName = "d_ring_0_mo2_sid", NullValueHandling = NullValueHandling.Ignore)]
        public string MouldSIDCodeSecondLayerLabelSide { get; set; }

        [JsonProperty(PropertyName = "dr_ring_0_mo2", NullValueHandling = NullValueHandling.Ignore)]
        public string AdditionalMouldSecondLayerLabelSide { get; set; }

        [JsonProperty(PropertyName = "d_ring_0_offsets", NullValueHandling = NullValueHandling.Ignore)]
        public string RingOffsetsHidden { get { return "1"; } }

        [JsonProperty(PropertyName = "d_ring_0_0_id", NullValueHandling = NullValueHandling.Ignore)]
        public string RingZeroId { get; }

        [JsonProperty(PropertyName = "d_ring_0_0_density", NullValueHandling = NullValueHandling.Ignore)]
        public string RingZeroDensity { get; }

        [JsonProperty(PropertyName = "d_ring_0_0_value", NullValueHandling = NullValueHandling.Ignore)]
        public string RingWriteOffset { get; set; }

        [JsonProperty(PropertyName = "d_ring_count", NullValueHandling = NullValueHandling.Ignore)]
        public string RingCount { get { return "1"; } }

        [JsonProperty(PropertyName = "d_barcode", NullValueHandling = NullValueHandling.Ignore)]
        public string Barcode { get; set; }

        [JsonProperty(PropertyName = "d_date", NullValueHandling = NullValueHandling.Ignore)]
        public string EXEDateBuildDate { get; set; }

        [JsonProperty(PropertyName = "d_errors", NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorsCount { get; set; }

        [JsonProperty(PropertyName = "d_comments", NullValueHandling = NullValueHandling.Ignore)]
        public string Comments { get; set; }

        [JsonProperty(PropertyName = "d_contents", NullValueHandling = NullValueHandling.Ignore)]
        public string Contents { get; set; }

        #endregion

        #region Version and Editions

        [JsonProperty(PropertyName = "d_version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "d_version_datfile", NullValueHandling = NullValueHandling.Ignore)]
        public string VersionDatfile { get; set; }

        [JsonProperty(PropertyName = "d_editions", NullValueHandling = NullValueHandling.Ignore)]
        public string[] CommonEditions { get; set; }

        [JsonProperty(PropertyName = "d_editions_text", NullValueHandling = NullValueHandling.Ignore)]
        public string OtherEditions { get; set; }

        #endregion

        #region EDC (PSX-only)

        [JsonProperty(PropertyName = "d_edc", NullValueHandling = NullValueHandling.Ignore)]
        public YesNo EDC { get; set; }

        #endregion

        #region Parent/Clone relationship

        [JsonProperty(PropertyName = "d_parent_id", NullValueHandling = NullValueHandling.Ignore)]
        public string ParentID { get; set; }

        [JsonProperty(PropertyName = "d_is_regional_parent", NullValueHandling = NullValueHandling.Ignore)]
        public bool RegionalParent { get; set; }

        #endregion

        #region Extras

        [JsonProperty(PropertyName = "d_pvd", NullValueHandling = NullValueHandling.Ignore)]
        public string PVD { get; set; }

        [JsonProperty(PropertyName = "d_d1_key", NullValueHandling = NullValueHandling.Ignore)]
        public string DiscKey { get; set; }

        [JsonProperty(PropertyName = "d_d2_key", NullValueHandling = NullValueHandling.Ignore)]
        public string DiscID { get; set; }

        [JsonProperty(PropertyName = "d_pic_data", NullValueHandling = NullValueHandling.Ignore)]
        public string PIC { get; set; }

        [JsonProperty(PropertyName = "d_header", NullValueHandling = NullValueHandling.Ignore)]
        public string Header { get; set; }

        [JsonProperty(PropertyName = "d_bca", NullValueHandling = NullValueHandling.Ignore)]
        public string BCA { get; set; }

        [JsonProperty(PropertyName = "d_ssranges", NullValueHandling = NullValueHandling.Ignore)]
        public string SecuritySectorRanges { get; set; }

        #endregion

        #region Copy protection

        [JsonProperty(PropertyName = "d_protection_a", NullValueHandling = NullValueHandling.Ignore)]
        public YesNo AntiModchip { get; set; }

        [JsonProperty(PropertyName = "d_protection_1", NullValueHandling = NullValueHandling.Ignore)]
        public YesNo LibCrypt { get; set; }

        [JsonProperty(PropertyName = "d_libcrypt", NullValueHandling = NullValueHandling.Ignore)]
        public string LibCryptData { get; set; }

        [JsonProperty(PropertyName = "d_protection", NullValueHandling = NullValueHandling.Ignore)]
        public string Protection { get; set; }

        [JsonProperty(PropertyName = "d_securom", NullValueHandling = NullValueHandling.Ignore)]
        public string SecuROMData { get; set; }

        #endregion

        #region Dumpers and Status (Moderator only)

        [JsonProperty(PropertyName = "d_status", NullValueHandling = NullValueHandling.Ignore)]
        public DumpStatus Status { get; set; }

        [JsonProperty(PropertyName = "d_dumpers", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Dumpers { get; set; }

        [JsonProperty(PropertyName = "d_dumpers_text", NullValueHandling = NullValueHandling.Ignore)]
        public string OtherDumpers { get; set; }

        #endregion

        #region Tracks and write offsets (CD,GD-baed)

        [JsonProperty(PropertyName = "d_tracks", NullValueHandling = NullValueHandling.Ignore)]
        public string ClrMameProData { get; set; }

        [JsonProperty(PropertyName = "d_cue", NullValueHandling = NullValueHandling.Ignore)]
        public string Cuesheet { get; set; }

        [JsonProperty(PropertyName = "d_offset", NullValueHandling = NullValueHandling.Ignore)]
        public int[] CommonWriteOffsets { get; set; }

        [JsonProperty(PropertyName = "d_offset_text", NullValueHandling = NullValueHandling.Ignore)]
        public string OtherWriteOffsets { get; set; }

        #endregion

        #region Size & Checksum (DVD,BD,UMD-based)

        [JsonProperty(PropertyName = "d_layerbreak", NullValueHandling = NullValueHandling.Ignore)]
        public long Layerbreak { get; set; }

        [JsonProperty(PropertyName = "d_size", NullValueHandling = NullValueHandling.Ignore)]
        public long Size { get; set; }

        [JsonProperty(PropertyName = "d_crc32", NullValueHandling = NullValueHandling.Ignore)]
        public string CRC32 { get; set; }

        [JsonProperty(PropertyName = "d_md5", NullValueHandling = NullValueHandling.Ignore)]
        public string MD5 { get; set; }

        [JsonProperty(PropertyName = "d_sha1", NullValueHandling = NullValueHandling.Ignore)]
        public string SHA1 { get; set; }

        #endregion

        #region Nonstandard Information

        [JsonIgnore]
        public List<int> MatchedIDs { get; set; }

        [JsonIgnore]
        public DateTime? Added { get; set; }

        [JsonIgnore]
        public DateTime? LastModified { get; set; }

        #endregion

        #region Regexes

        private Regex addedRegex = new Regex(@"<tr><th>Added</th><td>(.*?)</td></tr>");
        private Regex barcodeRegex = new Regex(@"<tr><th>Barcode</th></tr><tr><td>(.*?)</td></tr>");
        private Regex bcaRegex = new Regex(@"<h3>BCA .*?/></h3></td><td .*?></td></tr>"
            + "<tr><th>Row</th><th>Contents</th><th>ASCII</th></tr>"
            + "<tr><td>(?<row1number>.*?)</td><td>(?<row1contents>.*?)</td><td>(?<row1ascii>.*?)</td></tr>"
            + "<tr><td>(?<row2number>.*?)</td><td>(?<row2contents>.*?)</td><td>(?<row2ascii>.*?)</td></tr>"
            + "<tr><td>(?<row3number>.*?)</td><td>(?<row3contents>.*?)</td><td>(?<row3ascii>.*?)</td></tr>"
            + "<tr><td>(?<row4number>.*?)</td><td>(?<row4contents>.*?)</td><td>(?<row4ascii>.*?)</td></tr>");
        private Regex categoryRegex = new Regex(@"<tr><th>Category</th><td>(.*?)</td></tr>");
        private Regex commentsRegex = new Regex(@"<tr><th>Comments</th></tr><tr><td>(.*?)</td></tr>");
        private Regex contentsRegex = new Regex(@"<tr><th>Contents</th></tr><tr .*?><td>(.*?)</td></tr>");
        private Regex discNumberLetterRegex = new Regex(@"\((.*?)\)");
        private Regex dumpersRegex = new Regex(@"<a href=""/discs/dumper/(.*?)/"">");
        private Regex editionRegex = new Regex(@"<tr><th>Edition</th><td>(.*?)</td></tr>");
        private Regex errorCountRegex = new Regex(@"<tr><th>Errors count</th><td>(.*?)</td></tr>");
        private Regex foreignTitleRegex = new Regex(@"<h2>(.*?)</h2>");
        private Regex languagesRegex = new Regex(@"<img src=""/images/languages/(.*?)\.png"" alt="".*?"" title="".*?"" />\s*");
        private Regex lastModifiedRegex = new Regex(@"<tr><th>Last modified</th><td>(.*?)</td></tr>");
        private Regex mediaRegex = new Regex(@"<tr><th>Media</th><td>(.*?)</td></tr>");
        private Regex pvdRegex = new Regex(@"<h3>Primary Volume Descriptor (PVD) <img .*?/></h3></td><td .*?></td></tr>"
            + @"<tr><th>Record / Entry</th><th>Contents</th><th>Date</th><th>Time</th><th>GMT</th></tr>"
            + @"<tr><td>Creation</td><td>(?<creationbytes>.*?)</td><td>(?<creationdate>.*?)</td><td>(?<creationtime>.*?)</td><td>(?<creationtimezone>.*?)</td></tr>"
            + @"<tr><td>Modification</td><td>(?<modificationbytes>.*?)</td><td>(?<modificationdate>.*?)</td><td>(?<modificationtime>.*?)</td><td>(?<modificationtimezone>.*?)</td></tr>"
            + @"<tr><td>Expiration</td><td>(?<expirationbytes>.*?)</td><td>(?<expirationdate>.*?)</td><td>(?<expirationtime>.*?)</td><td>(?<expirationtimezone>.*?)</td></tr>"
            + @"<tr><td>Effective</td><td>(?<effectivebytes>.*?)</td><td>(?<effectivedate>.*?)</td><td>(?<effectivetime>.*?)</td><td>(?<effectivetimezone>.*?)</td></tr>");
        private Regex regionRegex = new Regex(@"<tr><th>Region</th><td><a href=""/discs/region/(.*?)/"">");
        private Regex ringCodeDoubleRegex = new Regex(@""); // Varies based on available fields, like Addtional Mould
        private Regex ringCodeSingleRegex = new Regex(@""); // Varies based on available fields, like Addtional Mould
        private Regex serialRegex = new Regex(@"<tr><th>Serial</th><td>(.*?)</td></tr>");
        private Regex systemRegex = new Regex(@"<tr><th>System</th><td><a href=""/discs/system/(.*?)/"">");
        private Regex titleRegex = new Regex(@"<h1>(.*?)</h1>");
        private Regex trackRegex = new Regex(@"<tr><td>(?<number>.*?)</td><td>(?<type>.*?)</td><td>(?<pregap>.*?)</td><td>(?<length>.*?)</td><td>(?<sectors>.*?)</td><td>(?<size>.*?)</td><td>(?<crc32>.*?)</td><td>(?<md5>.*?)</td><td>(?<sha1>.*?)</td></tr>");
        private Regex trackCountRegex = new Regex(@"<tr><th>Number of tracks</th><td>(.*?)</td></tr>");
        private Regex versionRegex = new Regex(@"<tr><th>Version</th><td>(.*?)</td></tr>");
        private Regex writeOffsetRegex = new Regex(@"<tr><th>Write offset</th><td>(.*?)</td></tr>");

        #endregion

        /// <summary>
        /// Fill in information from a Redump disc page
        /// </summary>
        /// <param name="discData">String representation of the disc page</param>
        public void FillFromDiscPage(string discData)
        {
            // Title, Disc Number/Letter, Disc Title
            var match = titleRegex.Match(discData);
            if (match.Success)
            {
                string title = match.Groups[1].Value;

                // If we have parenthesis, title is everything before the first one
                int firstParenLocation = title.IndexOf(" (");
                if (firstParenLocation >= 0)
                {
                    this.Title = title.Substring(0, firstParenLocation);
                    var subMatches = discNumberLetterRegex.Match(title);
                    for (int i = 1; i < subMatches.Groups.Count; i++)
                    {
                        string subMatch = subMatches.Groups[i].Value;

                        // Disc number or letter
                        if (subMatch.StartsWith("Disc"))
                            this.DiscNumberLetter = subMatch.Remove("Disc ".Length);

                        // Disc title
                        else
                            this.DiscTitle = subMatch;
                    }
                }
                // Otherwise, leave the title as-is
                else
                {
                    this.Title = title;
                }
            }

            // Foreign Title
            match = foreignTitleRegex.Match(discData);
            if (match.Success)
                this.ForeignTitleNonLatin = match.Groups[1].Value;
            else
                this.ForeignTitleNonLatin = null;

            // Category
            match = categoryRegex.Match(discData);
            if (match.Success)
                this.Category = Converters.StringToCategory(match.Groups[1].Value);
            else
                this.Category = Data.Category.Games;

            // Region
            match = regionRegex.Match(discData);
            if (match.Success)
                this.Region = Converters.StringToRegion(match.Groups[1].Value);

            // Languages
            var matches = languagesRegex.Matches(discData);
            if (matches.Count > 0)
            {
                List<Language?> tempLanguages = new List<Language?>();
                foreach (Match submatch in matches)
                    tempLanguages.Add(Converters.StringToLanguage(submatch.Groups[1].Value));

                this.Languages = tempLanguages.ToArray();
            }

            // Serial
            match = serialRegex.Match(discData);
            if (match.Success)
                this.Serial = match.Groups[1].Value;

            // Error count
            match = errorCountRegex.Match(discData);
            if (match.Success)
            {
                // If the error counts don't match, then use the one from the disc page
                if (!string.IsNullOrEmpty(this.ErrorsCount) && match.Groups[1].Value != this.ErrorsCount)
                    this.ErrorsCount = match.Groups[1].Value;
            }

            // Version
            match = versionRegex.Match(discData);
            if (match.Success)
                this.Version = match.Groups[1].Value;

            // Edition
            match = editionRegex.Match(discData);
            if (match.Success)
                this.OtherEditions = match.Groups[1].Value;

            // Dumpers
            matches = dumpersRegex.Matches(discData);
            if (matches.Count > 0)
            {
                // Start with any currently listed dumpers
                List<string> tempDumpers = new List<string>();
                if (this.Dumpers.Length > 0)
                {
                    foreach (string dumper in this.Dumpers)
                        tempDumpers.Add(dumper);
                }

                foreach (Match submatch in matches)
                    tempDumpers.Add(submatch.Groups[1].Value);

                this.Dumpers = tempDumpers.ToArray();
            }

            // Barcode
            match = barcodeRegex.Match(discData);
            if (match.Success)
                this.Barcode = match.Groups[1].Value;

            // Comments
            match = commentsRegex.Match(discData);
            if (match.Success)
            {
                this.Comments = match.Groups[1].Value
                    .Replace("<br />", "\n")
                    .Replace("<b>ISBN</b>", "[T:ISBN]") + "\n";
            }

            // Contents
            match = contentsRegex.Match(discData);
            if (match.Success)
            {
                this.Contents = match.Groups[1].Value
                       .Replace("<br />", "\n")
                       .Replace("</div>", "");
                this.Contents = Regex.Replace(this.Contents, @"<div .*?>", "");
            }

            // Added
            match = addedRegex.Match(discData);
            if (match.Success)
                this.Added = DateTime.Parse(match.Groups[1].Value);

            // Last Modified
            match = lastModifiedRegex.Match(discData);
            if (match.Success)
                this.LastModified = DateTime.Parse(match.Groups[1].Value);
        }
    }
}
