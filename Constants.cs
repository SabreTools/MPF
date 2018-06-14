namespace DICUI
{
    public static class UIElements
    {
        public const string StartDumping = "Start Dumping";
        public const string StopDumping = "Stop Dumping";
    }

    /// <summary>
    /// Top-level commands for DiscImageCreator
    /// </summary>
    public static class DICCommands
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
        public const string Reset = "reset";
        public const string Start = "start";
        public const string Stop = "stop";
        public const string Sub = "sub";
        public const string Swap = "swap";
        public const string XBOX = "xbox";
    }

    /// <summary>
    /// Dumping flags for DiscImageCreator
    /// </summary>
    public static class DICFlags
    {
        public const string CDAMSFFlag = "/p";
        public const string CDAddOffsetFlag = "/a";
        public const string CDBEOpcodeFlag = "/be";
        public const string CDC2OpcodeFlag = "/c2";
        public const string CDD8OpcodeFlag = "/d8";
        public const string CDMCNFlag = "/m";
        public const string CDMultiSessionFlag = "/ms";
        public const string CDNoFixSubPFlag = "/np";
        public const string CDNoFixSubQFlag = "/nq";
        public const string CDNoFixSubQLibCryptFlag = "/nl";
        public const string CDNoFixSubQSecuROMFlag = "/ns";
        public const string CDNoFixSubRtoWFlag = "/nr";
        public const string CDReverseFlag = "/r";
        public const string CDScanAnitModFlag = "/am";
        public const string CDScanFileProtectFlag = "/sf";
        public const string CDScanSectorProtectFlag = "/ss";
        public const string CDSubchannelReadLevelFlag = "/s";
        public const string DisableBeepFlag = "/q";
        public const string DVDCMIFlag = "/c";
        public const string DVDRawFlag = "/raw";
        public const string ForceUnitAccessFlag = "/f";
        public const string SeventyFourFlag = "/74";
    }

    public static class Template
    {
        // Manual information

        public const string TitleField = "Title";
        public const string DiscNumberField = "Disc Number / Letter";
        public const string DiscTitleField = "Disc Title";
        public const string CategoryField = "Category";
        public const string RegionField = "Region";
        public const string LanguagesField = "Languages";
        public const string DiscSerialField = "Disc Serial";
        public const string BarcodeField = "Barcode";
        public const string ISBNField = "ISBN";
        public const string CommentsField = "Comments";
        public const string ContentsField = "Contents";
        public const string VersionField = "Version";
        public const string EditionField = "Edition/Release";
        public const string CopyProtectionField = "Copy Protection";
        public const string MasteringRingField = "Mastering Ring";
        public const string MasteringSIDField = "Mastering SID Code";
        public const string MouldSIDField = "Mould SID Code";
        public const string AdditionalMouldField = "Additional Mould";
        public const string ToolstampField = "Toolstamp or Mastering Code";

        // Automatic Information

        public const string PVDField = "Primary Volume Descriptor (PVD)";
        public const string DATField = "DAT";
        public const string ErrorCountField = "Error Count";
        public const string CuesheetField = "Cuesheet";
        public const string SubIntentionField = "SubIntention (SecuROM)";
        public const string WriteOffsetField = "WriteOffset";
        public const string LayerbreakField = "Layerbreak";
        public const string PlaystationEXEDateField = "EXE Date"; // TODO: Not automatic yet
        public const string PlayStationEDCField = "EDC"; // TODO: Not automatic yet
        public const string PlayStationAntiModchipField = "Anti-modchip"; // TODO: Not automatic yet
        public const string PlayStationLibCryptField = "LibCrypt"; // TODO: Not automatic yet
        public const string SaturnHeaderField = "Header"; // TODO: Not automatic yet
        public const string SaturnBuildDateField = "Build Date"; // TODO: Not automatic yet

        // Default values

        public const string RequiredValue = "(REQUIRED)";
        public const string RequiredIfExistsValue = "(REQUIRED, IF EXISTS)";
        public const string OptionalValue = "(OPTIONAL)";
        public const string YesNoValue = "Yes/No";
    }
}
