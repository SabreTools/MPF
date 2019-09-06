using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net;
using DICUI.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace DICUI.Data
{
    public class SubmissionInfo
    {
        /// <summary>
        /// List of matched Redump IDs
        /// </summary>
        [JsonIgnore]
        public List<int> MatchedIDs { get; set; }

        /// <summary>
        /// DateTime of when the disc was added
        /// </summary>
        [JsonIgnore]
        public DateTime? Added { get; set; }

        /// <summary>
        /// DateTime of when the disc was last modified
        /// </summary>
        [JsonIgnore]
        public DateTime? LastModified { get; set; }

        [JsonProperty(PropertyName = "common_disc_info", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public CommonDiscInfoSection CommonDiscInfo { get; set; } = new CommonDiscInfoSection();

        [JsonProperty(PropertyName = "versions_and_editions", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public VersionAndEditionsSection VersionAndEditions { get; set; } = new VersionAndEditionsSection();

        [JsonProperty(PropertyName = "edc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public EDCSection EDC { get; set; } = new EDCSection();

        [JsonProperty(PropertyName = "parent_clone_relationship", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ParentCloneRelationshipSection ParentCloneRelationship { get; set; } = new ParentCloneRelationshipSection();

        [JsonProperty(PropertyName = "extras", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ExtrasSection Extras { get; set; } = new ExtrasSection();

        [JsonProperty(PropertyName = "copy_protection", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public CopyProtectionSection CopyProtection { get; set; } = new CopyProtectionSection();

        [JsonProperty(PropertyName = "dumpers_and_status", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DumpersAndStatusSection DumpersAndStatus { get; set; } = new DumpersAndStatusSection();

        [JsonProperty(PropertyName = "tracks_and_write_offsets", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TracksAndWriteOffsetsSection TracksAndWriteOffsets { get; set; } = new TracksAndWriteOffsetsSection();

        [JsonProperty(PropertyName = "size_and_checksums", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public SizeAndChecksumsSection SizeAndChecksums { get; set; } = new SizeAndChecksumsSection();

        #region Regexes

        private readonly Regex addedRegex = new Regex(@"<tr><th>Added</th><td>(.*?)</td></tr>");
        private readonly Regex barcodeRegex = new Regex(@"<tr><th>Barcode</th></tr><tr><td>(.*?)</td></tr>");
        private readonly Regex bcaRegex = new Regex(@"<h3>BCA .*?/></h3></td><td .*?></td></tr>"
            + "<tr><th>Row</th><th>Contents</th><th>ASCII</th></tr>"
            + "<tr><td>(?<row1number>.*?)</td><td>(?<row1contents>.*?)</td><td>(?<row1ascii>.*?)</td></tr>"
            + "<tr><td>(?<row2number>.*?)</td><td>(?<row2contents>.*?)</td><td>(?<row2ascii>.*?)</td></tr>"
            + "<tr><td>(?<row3number>.*?)</td><td>(?<row3contents>.*?)</td><td>(?<row3ascii>.*?)</td></tr>"
            + "<tr><td>(?<row4number>.*?)</td><td>(?<row4contents>.*?)</td><td>(?<row4ascii>.*?)</td></tr>");
        private readonly Regex categoryRegex = new Regex(@"<tr><th>Category</th><td>(.*?)</td></tr>");
        private readonly Regex commentsRegex = new Regex(@"<tr><th>Comments</th></tr><tr><td>(.*?)</td></tr>");
        private readonly Regex contentsRegex = new Regex(@"<tr><th>Contents</th></tr><tr .*?><td>(.*?)</td></tr>");
        private readonly Regex discNumberLetterRegex = new Regex(@"\((.*?)\)");
        private readonly Regex dumpersRegex = new Regex(@"<a href=""/discs/dumper/(.*?)/"">");
        private readonly Regex editionRegex = new Regex(@"<tr><th>Edition</th><td>(.*?)</td></tr>");
        private readonly Regex errorCountRegex = new Regex(@"<tr><th>Errors count</th><td>(.*?)</td></tr>");
        private readonly Regex foreignTitleRegex = new Regex(@"<h2>(.*?)</h2>");
        private readonly Regex fullMatchRegex = new Regex(@"<td class=""static"">full match ids: (.*?)</td>");
        private readonly Regex languagesRegex = new Regex(@"<img src=""/images/languages/(.*?)\.png"" alt="".*?"" title="".*?"" />\s*");
        private readonly Regex lastModifiedRegex = new Regex(@"<tr><th>Last modified</th><td>(.*?)</td></tr>");
        private readonly Regex mediaRegex = new Regex(@"<tr><th>Media</th><td>(.*?)</td></tr>");
        private readonly Regex partialMatchRegex = new Regex(@"<td class=""static"">partial match ids: (.*?)</td>");
        private readonly Regex pvdRegex = new Regex(@"<h3>Primary Volume Descriptor (PVD) <img .*?/></h3></td><td .*?></td></tr>"
            + @"<tr><th>Record / Entry</th><th>Contents</th><th>Date</th><th>Time</th><th>GMT</th></tr>"
            + @"<tr><td>Creation</td><td>(?<creationbytes>.*?)</td><td>(?<creationdate>.*?)</td><td>(?<creationtime>.*?)</td><td>(?<creationtimezone>.*?)</td></tr>"
            + @"<tr><td>Modification</td><td>(?<modificationbytes>.*?)</td><td>(?<modificationdate>.*?)</td><td>(?<modificationtime>.*?)</td><td>(?<modificationtimezone>.*?)</td></tr>"
            + @"<tr><td>Expiration</td><td>(?<expirationbytes>.*?)</td><td>(?<expirationdate>.*?)</td><td>(?<expirationtime>.*?)</td><td>(?<expirationtimezone>.*?)</td></tr>"
            + @"<tr><td>Effective</td><td>(?<effectivebytes>.*?)</td><td>(?<effectivedate>.*?)</td><td>(?<effectivetime>.*?)</td><td>(?<effectivetimezone>.*?)</td></tr>");
        private readonly Regex regionRegex = new Regex(@"<tr><th>Region</th><td><a href=""/discs/region/(.*?)/"">");
        private readonly Regex ringCodeDoubleRegex = new Regex(@""); // Varies based on available fields, like Addtional Mould
        private readonly Regex ringCodeSingleRegex = new Regex(@""); // Varies based on available fields, like Addtional Mould
        private readonly Regex serialRegex = new Regex(@"<tr><th>Serial</th><td>(.*?)</td></tr>");
        private readonly Regex systemRegex = new Regex(@"<tr><th>System</th><td><a href=""/discs/system/(.*?)/"">");
        private readonly Regex titleRegex = new Regex(@"<h1>(.*?)</h1>");
        private readonly Regex trackRegex = new Regex(@"<tr><td>(?<number>.*?)</td><td>(?<type>.*?)</td><td>(?<pregap>.*?)</td><td>(?<length>.*?)</td><td>(?<sectors>.*?)</td><td>(?<size>.*?)</td><td>(?<crc32>.*?)</td><td>(?<md5>.*?)</td><td>(?<sha1>.*?)</td></tr>");
        private readonly Regex trackCountRegex = new Regex(@"<tr><th>Number of tracks</th><td>(.*?)</td></tr>");
        private readonly Regex versionRegex = new Regex(@"<tr><th>Version</th><td>(.*?)</td></tr>");
        private readonly Regex writeOffsetRegex = new Regex(@"<tr><th>Write offset</th><td>(.*?)</td></tr>");

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
                string title = WebUtility.HtmlDecode(match.Groups[1].Value);

                // If we have parenthesis, title is everything before the first one
                int firstParenLocation = title.IndexOf(" (");
                if (firstParenLocation >= 0)
                {
                    this.CommonDiscInfo.Title = title.Substring(0, firstParenLocation);
                    var subMatches = discNumberLetterRegex.Match(title);
                    for (int i = 1; i < subMatches.Groups.Count; i++)
                    {
                        string subMatch = subMatches.Groups[i].Value;

                        // Disc number or letter
                        if (subMatch.StartsWith("Disc"))
                            this.CommonDiscInfo.DiscNumberLetter = subMatch.Remove(0, "Disc ".Length);

                        // Disc title
                        else
                            this.CommonDiscInfo.DiscTitle = subMatch;
                    }
                }
                // Otherwise, leave the title as-is
                else
                {
                    this.CommonDiscInfo.Title = title;
                }
            }

            // Foreign Title
            match = foreignTitleRegex.Match(discData);
            if (match.Success)
                this.CommonDiscInfo.ForeignTitleNonLatin = WebUtility.HtmlDecode(match.Groups[1].Value);
            else
                this.CommonDiscInfo.ForeignTitleNonLatin = null;

            // Category
            match = categoryRegex.Match(discData);
            if (match.Success)
                this.CommonDiscInfo.Category = Converters.StringToCategory(match.Groups[1].Value);
            else
                this.CommonDiscInfo.Category = Data.Category.Games;

            // Region
            match = regionRegex.Match(discData);
            if (match.Success)
                this.CommonDiscInfo.Region = Converters.StringToRegion(match.Groups[1].Value);

            // Languages
            var matches = languagesRegex.Matches(discData);
            if (matches.Count > 0)
            {
                List<Language?> tempLanguages = new List<Language?>();
                foreach (Match submatch in matches)
                    tempLanguages.Add(Converters.StringToLanguage(submatch.Groups[1].Value));

                this.CommonDiscInfo.Languages = tempLanguages.ToArray();
            }

            // Serial
            match = serialRegex.Match(discData);
            if (match.Success)
                this.CommonDiscInfo.Serial = WebUtility.HtmlDecode(match.Groups[1].Value);

            // Error count
            match = errorCountRegex.Match(discData);
            if (match.Success)
            {
                // If the error counts don't match, then use the one from the disc page
                if (!string.IsNullOrEmpty(this.CommonDiscInfo.ErrorsCount) && match.Groups[1].Value != this.CommonDiscInfo.ErrorsCount)
                    this.CommonDiscInfo.ErrorsCount = match.Groups[1].Value;
            }

            // Version
            match = versionRegex.Match(discData);
            if (match.Success)
                this.VersionAndEditions.Version = WebUtility.HtmlDecode(match.Groups[1].Value);

            // Edition
            match = editionRegex.Match(discData);
            if (match.Success)
                this.VersionAndEditions.OtherEditions = WebUtility.HtmlDecode(match.Groups[1].Value);

            // Dumpers
            matches = dumpersRegex.Matches(discData);
            if (matches.Count > 0)
            {
                // Start with any currently listed dumpers
                List<string> tempDumpers = new List<string>();
                if (this.DumpersAndStatus.Dumpers.Length > 0)
                {
                    foreach (string dumper in this.DumpersAndStatus.Dumpers)
                        tempDumpers.Add(dumper);
                }

                foreach (Match submatch in matches)
                    tempDumpers.Add(WebUtility.HtmlDecode(submatch.Groups[1].Value));

                this.DumpersAndStatus.Dumpers = tempDumpers.ToArray();
            }

            // Barcode
            match = barcodeRegex.Match(discData);
            if (match.Success)
                this.CommonDiscInfo.Barcode = WebUtility.HtmlDecode(match.Groups[1].Value);

            // Comments
            match = commentsRegex.Match(discData);
            if (match.Success)
            {
                this.CommonDiscInfo.Comments = WebUtility.HtmlDecode(match.Groups[1].Value)
                    .Replace("<br />", "\n")
                    .Replace("<b>ISBN</b>", "[T:ISBN]") + "\n";
            }

            // Contents
            match = contentsRegex.Match(discData);
            if (match.Success)
            {
                this.CommonDiscInfo.Contents = WebUtility.HtmlDecode(match.Groups[1].Value)
                       .Replace("<br />", "\n")
                       .Replace("</div>", "");
                this.CommonDiscInfo.Contents = Regex.Replace(this.CommonDiscInfo.Contents, @"<div .*?>", "");
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

    /// <summary>
    /// Common disc info section of New Disc Form
    /// </summary>
    public class CommonDiscInfoSection
    {
        // TODO: Name not defined
        [JsonProperty(PropertyName = "d_system", Required = Required.AllowNull)]
        [JsonConverter(typeof(KnownSystemConverter))]
        public KnownSystem? System { get; set; }

        // TODO: Name not defined
        // TODO: Have this convert to a new `RedumpMedia?` if possible, for submission
        [JsonProperty(PropertyName = "d_media", Required = Required.AllowNull)]
        [JsonConverter(typeof(MediaTypeConverter))]
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
        [JsonConverter(typeof(RegionConverter))]
        public Region? Region { get; set; }

        [JsonProperty(PropertyName = "d_languages", Required = Required.AllowNull)]
        [JsonConverter(typeof(LanguagesConverter))]
        public Language?[] Languages { get; set; }

        // TODO: Ensure names from new disc form are used here
        [JsonProperty(PropertyName = "d_languages_selection", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public LanguageSelection?[] LanguageSelection { get; set; }

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
    }

    /// <summary>
    /// Version and editions section of New Disc form
    /// </summary>
    public class VersionAndEditionsSection
    {
        [JsonProperty(PropertyName = "d_version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "d_version_datfile", NullValueHandling = NullValueHandling.Ignore)]
        public string VersionDatfile { get; set; }

        [JsonProperty(PropertyName = "d_editions", NullValueHandling = NullValueHandling.Ignore)]
        public string[] CommonEditions { get; set; }

        [JsonProperty(PropertyName = "d_editions_text", NullValueHandling = NullValueHandling.Ignore)]
        public string OtherEditions { get; set; }
    }

    /// <summary>
    /// EDC section of New Disc form (PSX only)
    /// </summary>
    public class EDCSection
    {
        [JsonProperty(PropertyName = "d_edc", NullValueHandling = NullValueHandling.Ignore)]
        public YesNo EDC { get; set; }
    }

    /// <summary>
    /// Parent/Clone relationship section of New Disc form
    /// </summary>
    public class ParentCloneRelationshipSection
    {
        [JsonProperty(PropertyName = "d_parent_id", NullValueHandling = NullValueHandling.Ignore)]
        public string ParentID { get; set; }

        [JsonProperty(PropertyName = "d_is_regional_parent", NullValueHandling = NullValueHandling.Ignore)]
        public bool RegionalParent { get; set; }
    }

    /// <summary>
    /// Extras section of New Disc form
    /// </summary>
    public class ExtrasSection
    {
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
    }

    /// <summary>
    /// Copy protection section of New Disc form
    /// </summary>
    public class CopyProtectionSection
    {
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
    }

    /// <summary>
    /// Dumpers and status section of New Disc form (Moderator only)
    /// </summary>
    public class DumpersAndStatusSection
    {
        [JsonProperty(PropertyName = "d_status", NullValueHandling = NullValueHandling.Ignore)]
        public DumpStatus Status { get; set; }

        [JsonProperty(PropertyName = "d_dumpers", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Dumpers { get; set; }

        [JsonProperty(PropertyName = "d_dumpers_text", NullValueHandling = NullValueHandling.Ignore)]
        public string OtherDumpers { get; set; }
    }

    /// <summary>
    /// Tracks and write offsets section of New Disc form (CD/GD-based)
    /// </summary>
    public class TracksAndWriteOffsetsSection
    {
        [JsonProperty(PropertyName = "d_tracks", NullValueHandling = NullValueHandling.Ignore)]
        public string ClrMameProData { get; set; }

        [JsonProperty(PropertyName = "d_cue", NullValueHandling = NullValueHandling.Ignore)]
        public string Cuesheet { get; set; }

        [JsonProperty(PropertyName = "d_offset", NullValueHandling = NullValueHandling.Ignore)]
        public int[] CommonWriteOffsets { get; set; }

        [JsonProperty(PropertyName = "d_offset_text", NullValueHandling = NullValueHandling.Ignore)]
        public string OtherWriteOffsets { get; set; }
    }

    /// <summary>
    /// Size & checksums section of New Disc form (DVD/BD/UMD-based)
    /// </summary>
    public class SizeAndChecksumsSection
    {
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
    }
}
