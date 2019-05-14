using System;
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
        public DateTime? EXEDateBuildDate { get; set; }

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
    }
}
