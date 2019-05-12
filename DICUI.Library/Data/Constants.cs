namespace DICUI.Data
{
    /// <summary>
    /// Top-level commands for DiscImageCreator
    /// </summary>
    public static class DICCommandStrings
    {
        public const string Audio = "audio";
        public const string BluRay = "bd";
        public const string Close = "close";
        public const string CompactDisc = "cd";
        public const string Data = "data";
        public const string DigitalVideoDisc = "dvd";
        public const string DriveSpeed = "ls";
        public const string Eject = "eject";
        public const string Floppy = "fd";
        public const string GDROM = "gd";
        public const string MDS = "mds";
        public const string Merge = "merge";
        public const string Reset = "reset";
        public const string Start = "start";
        public const string Stop = "stop";
        public const string Sub = "sub";
        public const string Swap = "swap";
        public const string XBOX = "xbox";
        public const string XBOXSwap = "xboxswap";
        public const string XGD2Swap = "xgd2swap";
        public const string XGD3Swap = "xgd3swap";
    }

    /// <summary>
    /// Dumping flags for DiscImageCreator
    /// </summary>
    public static class DICFlagStrings
    {
        public const string AddOffset = "/a";
        public const string AMSF = "/p";
        public const string BEOpcode = "/be";
        public const string C2Opcode = "/c2";
        public const string CopyrightManagementInformation = "/c";
        public const string D8Opcode = "/d8";
        public const string DisableBeep = "/q";
        public const string ForceUnitAccess = "/f";
        public const string MCN = "/m";
        public const string MultiSession = "/ms";
        public const string NoFixSubP = "/np";
        public const string NoFixSubQ = "/nq";
        public const string NoFixSubQLibCrypt = "/nl";
        public const string NoFixSubQSecuROM = "/ns";
        public const string NoFixSubRtoW = "/nr";
        public const string Raw = "/raw";
        public const string Reverse = "/r";
        public const string ScanAntiMod = "/am";
        public const string ScanFileProtect = "/sf";
        public const string ScanSectorProtect = "/ss";
        public const string SeventyFour = "/74";
        public const string SkipSector = "/sk";
        public const string SubchannelReadLevel = "/s";
        public const string VideoNow = "/vn";
    }

    /// <summary>
    /// Internal names of fields on the New Disc page
    /// </summary>
    public static class NewDiscPageStrings
    {
        // Common disc info

        public const string System = ""; // Undefined, use shortnames
        public const string Media = ""; // Undefined, use shortnames
        public const string Title = "d_title";
        public const string ForeignTitleNonLatin = "d_title_foreign";
        public const string DiscNumberLetter = "d_number";
        public const string DiscTitle = "d_label";
        public const string Category = "d_category"; // Category enum
        public const string Region = "d_region"; // Single region only
        public const string Languages = "d_languages[]"; // Multiple languages possible
        public const string LanguageSelectionVia = "d_languages_selection[]"; // PS2; "Bios settings", "Language selector", "Options menu"; Muliple choices possible
        public const string Serial = "d_serial";
        public const string Ring = "d_ring";
        public const string RingId = "d_ring_0_id"; // Hidden
        public const string MasteringRingFirstLayerDataSide = "d_ring_0_ma1";
        public const string MasteringSIDCodeFirstLayerDataSide = "d_ring_0_ma1_sid";
        public const string ToolstampMasteringCodeFirstLayerDataSide = "d_ring_0_ts1";
        public const string MouldSIDCodeFirstLayerDataSide = "d_ring_0_mo1_sid";
        public const string AdditionalMouldFirstLayerDataSide = "dr_ring_0_mo1";
        public const string MasteringRingSecondLayerLabelSide = "d_ring_0_ma2";
        public const string MasteringSIDCodeSecondLayerLabelSide = "d_ring_0_ma2_sid";
        public const string ToolstampMasteringCodeSecondLayerLabelSide = "d_ring_0_ts2";
        public const string MouldSIDCodeSecondLayerLabelSide = "d_ring_0_mo2_sid";
        public const string AdditionalMouldSecondLayerLabelSide = "dr_ring_0_mo2";
        public const string RingOffsetsHidden = "d_ring_0_offsets"; // Hidden, value '1'
        public const string RingZeroId = "d_ring_0_0_id"; // Hidden
        public const string RingZeroDensity = "d_ring_0_0_density"; // Hidden
        public const string RingWriteOffset = "d_ring_0_0_value";
        public const string RingCount = "d_ring_count"; // Hidden, value '1'
        public const string Barcode = "d_barcode";
        public const string EXEDateBuildDate = "d_date"; // PSX, PS2, MCD, SS, DC, PC-98, ACD, CD32, CDTV, NGCD, QIS, FMT, HS, ITE, KEA, KFB, KS573, KSGV, KT, CHIHIRO, NAOMI, NAOMI2, STV, TRIFORCE, NAVI21
        public const string ErrorsCount = "d_errors";
        public const string Comments = "d_comments";
        public const string Contents = "d_contents";

        // Version and editions

        public const string Version = "d_version";
        public const string VersionDatfile = "d_version_datfile";
        public const string CommonEditions = "d_editions[]"; // Multiple editions possible
        public const string OtherEditions = "d_editions_text";

        // EDC (PSX)
        public const string EDC = "d_edc"; // 1 = No, 2 = Yes

        // Parent/Clone relationship
        public const string ParentID = "d_parent_id";
        public const string RegionalParent = "d_is_regional_parent"; // Single checkbox

        // Extras
        public const string PVD = "d_pvd"; // CD,DVD-based (ISO9660 FS only)
        public const string DiscKey = "d_d1_key"; // PS3 BD-based, WIIU
        public const string DiscID = "d_d2_key"; // PS3 BD-based
        public const string PIC = "d_pic_data"; // BD-based
        public const string Header = "d_header"; // MCD, SS, GD-based
        public const string BCA = "d_bca"; // GC, WII
        public const string SecuritySectorRanges = "d_ssranges"; // XBOX DVD-based with SS, XBOX360 DVD-based with SS

        // Copy protection
        public const string AntiModchip = "d_protection_a"; // PSX; RedumpYesNo enum
        public const string LibCrypt = "d_protection_1"; // PSX; RedumpYesNo enum
        public const string LibCryptData = "d_libcrypt"; // PSX
        public const string Protection = "d_protection"; // PC, MAC, DVD-VIDEO, BD-VIDEO, GAMEWAVE
        public const string SecuROMData = "d_securom"; // PC

        // Dumpers and status (moderator only)
        public const string Status = "d_status"; // DumpStatus enum
        public const string Dumpers = "d_dumpers[]"; // Multiple dumpers possible (by dumper ID)
        public const string OtherDumpers = "d_dumpers_text";

        // Tracks and write offsets (CD,GD-based)
        public const string ClrMameProData = "d_tracks";
        public const string Cuesheet = "d_cue";
        public const string CommonWriteOffsets = "d_offset[]"; // Multiple offsets possible
        public const string OtherWriteOffsets = "d_offset_text";

        // Size & checksums (DVD,BD,UMD-based)
        public const string Layerbreak = "d_layerbreak"; // DVD-9, BD-50, UMD-DL only
        public const string Size = "d_size";
        public const string CRC32 = "d_crc32";
        public const string MD5 = "d_md5";
        public const string SHA1 = "d_sha1";
    }

    /// <summary>
    /// Template field values for submission info
    /// </summary>
    public static class Template
    {
        // Manual information

        public const string TitleField = "Title";
        public const string ForeignTitleField = "Foreign Title (Non-latin)";
        public const string DiscNumberField = "Disc Number / Letter";
        public const string DiscTitleField = "Disc Title";
        public const string SystemField = "System";
        public const string MediaTypeField = "Media Type";
        public const string CategoryField = "Category";
        public const string RegionField = "Region";
        public const string LanguagesField = "Languages";
        public const string PlaystationLanguageSelectionViaField = "Language Selection Via";
        public const string DiscSerialField = "Disc Serial";
        public const string BarcodeField = "Barcode";
        public const string ISBNField = "ISBN";
        public const string CommentsField = "Comments";
        public const string ContentsField = "Contents";
        public const string VersionField = "Version";
        public const string EditionField = "Edition/Release";
        public const string PlayStation3WiiDiscKeyField = "Disc Key";
        public const string PlayStation3DiscIDField = "Disc ID";
        public const string GameCubeWiiBCAField = "BCA";
        public const string CopyProtectionField = "Copy Protection";
        public const string MasteringRingField = "Mastering Code (laser branded/etched)";
        public const string MasteringSIDField = "Mastering SID Code";
        public const string MouldSIDField = "Mould SID Code";
        public const string AdditionalMouldField = "Additional Mould";
        public const string ToolstampField = "Toolstamp or Mastering Code (engraved/stamped)";

        // Automatic Information

        public const string PVDField = "Primary Volume Descriptor (PVD)";
        public const string DATField = "DAT";
        public const string ErrorCountField = "Error Count";
        public const string CuesheetField = "Cuesheet";
        public const string SubIntentionField = "SubIntention Data (SecuROM/LibCrypt)";
        public const string WriteOffsetField = "Write Offset";
        public const string LayerbreakField = "Layerbreak";
        public const string EXEDateBuildDate = "EXE/Build Date";
        public const string HeaderField = "Header";
        public const string PICField = "Permanent Information & Control (PIC)";
        public const string PlayStationEDCField = "EDC";
        public const string PlayStationAntiModchipField = "Anti-modchip";
        public const string PlayStationLibCryptField = "LibCrypt";
        public const string XBOXDMIHash = "DMI.bin Hashes";
        public const string XBOXPFIHash = "PFI.bin Hashes";
        public const string XBOXSSHash = "SS.bin Hashes";
        public const string XBOXSSRanges = "Security Sector Ranges";
        public const string XBOXSSVersion = "Security Sector Version";

        // Default values

        public const string RequiredValue = "(REQUIRED)";
        public const string RequiredIfExistsValue = "(REQUIRED, IF EXISTS)";
        public const string OptionalValue = "(OPTIONAL)";
        public const string YesNoValue = "Yes/No";
        public const string DiscNotDetected = "Disc Not Detected";
    }
}
