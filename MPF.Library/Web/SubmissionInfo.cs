using System;
using System.Collections.Generic;
using MPF.Data;
using MPF.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MPF.Web
{
    public class SubmissionInfo
    {
        /// <summary>
        /// Version of the current schema
        /// </summary>
        [JsonProperty(PropertyName = "schema_version", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int SchemaVersion { get; set; } = 1;

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

        [JsonProperty(PropertyName = "artifacts", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, string> Artifacts { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Common disc info section of New Disc Form
    /// </summary>
    public class CommonDiscInfoSection
    {
        // Name not defined by Redump
        [JsonProperty(PropertyName = "d_system", Required = Required.AllowNull)]
        [JsonConverter(typeof(KnownSystemConverter))]
        public KnownSystem? System { get; set; }

        // Name not defined by Redump
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
        public DiscCategory? Category { get; set; }

        [JsonProperty(PropertyName = "d_region", Required = Required.AllowNull)]
        [JsonConverter(typeof(RegionConverter))]
        public Region? Region { get; set; }

        [JsonProperty(PropertyName = "d_languages", Required = Required.AllowNull)]
        [JsonConverter(typeof(LanguagesConverter))]
        public Language?[] Languages { get; set; }

        [JsonProperty(PropertyName = "d_languages_selection", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(LanguageSelectionConverter))]
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

        [JsonProperty(PropertyName = "d_ring_0_ma3", Required = Required.AllowNull)]
        public string MasteringRingThirdLayer { get; set; }

        [JsonProperty(PropertyName = "d_ring_0_ma3_sid", NullValueHandling = NullValueHandling.Ignore)]
        public string MasteringSIDCodeThirdLayer { get; set; }

        [JsonProperty(PropertyName = "d_ring_0_ts3", NullValueHandling = NullValueHandling.Ignore)]
        public string ToolstampMasteringCodeThirdLayer { get; set; }

        [JsonProperty(PropertyName = "d_ring_0_ma4", Required = Required.AllowNull)]
        public string MasteringRingFourthLayer { get; set; }

        [JsonProperty(PropertyName = "d_ring_0_ma4_sid", NullValueHandling = NullValueHandling.Ignore)]
        public string MasteringSIDCodeFourthLayer { get; set; }

        [JsonProperty(PropertyName = "d_ring_0_ts4", NullValueHandling = NullValueHandling.Ignore)]
        public string ToolstampMasteringCodeFourthLayer { get; set; }

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
    /// Size &amp; checksums section of New Disc form (DVD/BD/UMD-based)
    /// </summary>
    public class SizeAndChecksumsSection
    {
        [JsonProperty(PropertyName = "d_layerbreak", NullValueHandling = NullValueHandling.Ignore)]
        public long Layerbreak { get; set; }

        [JsonProperty(PropertyName = "d_layerbreak_2", NullValueHandling = NullValueHandling.Ignore)]
        public long Layerbreak2 { get; set; }

        [JsonProperty(PropertyName = "d_layerbreak_3", NullValueHandling = NullValueHandling.Ignore)]
        public long Layerbreak3 { get; set; }

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
