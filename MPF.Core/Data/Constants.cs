using System.Collections.Generic;
using System.Linq;
using RedumpLib.Data;

namespace MPF.Core.Data
{
    /// <summary>
    /// Constant values for UI
    /// </summary>
    public static class Interface
    {
        // Button values
        public const string StartDumping = "Start Dumping";
        public const string StopDumping = "Stop Dumping";

        // Byte arrays for signatures
        public static readonly byte[] SaturnSectorZeroStart = new byte[] { 0x53, 0x45, 0x47, 0x41, 0x20, 0x53, 0x45, 0x47, 0x41, 0x53, 0x41, 0x54, 0x55, 0x52, 0x4E, 0x20 };

        // Private lists of known drive speed ranges
        private static IReadOnlyList<int> CD { get; } = new List<int> { 1, 2, 3, 4, 6, 8, 12, 16, 20, 24, 32, 40, 44, 48, 52, 56, 72 };
        private static IReadOnlyList<int> DVD { get; } = CD.Where(s => s <= 24).ToList();
        private static IReadOnlyList<int> BD { get; } = CD.Where(s => s <= 16).ToList();
        private static IReadOnlyList<int> Unknown { get; } = new List<int> { 1 };

        /// <summary>
        /// Get list of all drive speeds for a given MediaType
        /// </summary>
        /// <param name="type">MediaType? that represents the current item</param>
        /// <returns>Read-only list of drive speeds</returns>
        public static IReadOnlyList<int> GetSpeedsForMediaType(MediaType? type)
        {
            switch (type)
            {
                case MediaType.CDROM:
                case MediaType.GDROM:
                    return CD;
                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    return DVD;
                case MediaType.BluRay:
                    return BD;
                default:
                    return Unknown;
            }
        }
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
        public const string SizeField = "Size";
        public const string CRC32Field = "CRC32";
        public const string MD5Field = "MD5";
        public const string SHA1Field = "SHA1";
        public const string MatchingIDsField = "Matching IDs";
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
        public const string XBOXDMIHash = "DMI";
        public const string XBOXPFIHash = "PFI";
        public const string XBOXSSHash = "SS";
        public const string XBOXSSRanges = "Security Sector Ranges";
        public const string XBOXSSVersion = "Security Sector Version";
        public const string XBOXXeMID = "XeMID";

        // Redump Comment Field Special Tags

        // TODO: Make this into a proper enum with LongName and ShortName

        public const string AcclaimIDCommentField = "[T:ACC]";
        public const string AcclaimIDCommentString = "<b>Acclaim ID</b>:";
        public const string ActivisionIDCommentField = "[T:ACT]";
        public const string ActivisionIDCommentString = "<b>Activision ID</b>:";
        public const string AlternativeTitleCommentField = "[T:ALT]";
        public const string AlternativeTitleCommentString = "<b>Alternative Title</b>:";
        public const string AlternativeForeignTitleCommentField = "[T:ALTF]";
        public const string AlternativeForeignTitleCommentString = "<b>Alternative Foreign Title</b>:";
        public const string BandaiIDCommentField = "[T:BID]";
        public const string BandaiIDCommentString = "<b>Bandai ID</b>:";
        public const string BBFCRegistrationNumberCommentField = "[T:BBFC]";
        public const string BBFCRegistrationNumberCommentString = "<b>BBFC Reg. No.</b>:";
        public const string DNASDiscIDCommentField = "[T:DNAS]";
        public const string DNASDiscIDCommentString = "<b>DNAS Disc ID</b>:";
        public const string ElectronicArtsIDCommentField = "[T:EAID]";
        public const string ElectronicArtsIDCommentString = "<b>Electronic Arts ID</b>:";
        public const string ExtrasCommentField = "[T:X]";
        public const string ExtrasCommentString = "<b>Extras</b>:";
        public const string FoxInteractiveIDCommentField = "[T:FIID]";
        public const string FoxInteractiveIDCommentString = "<b>Fox Interactive ID</b>:";
        public const string GameFootageCommentField = "[T:GF]";
        public const string GameFootageCommentString = "<b>Game Footage</b>:";
        public const string GenreCommentField = "[T:G]";
        public const string GenreCommentString = "<b>Genre</b>:";
        public const string GTInteractiveIDCommentField = "[T:GTID]";
        public const string GTInteractiveIDCommentString = "<b>GT Interactive ID</b>:";
        public const string InternalSerialNameCommentField = "[T:ISN]";
        public const string InternalSerialNameCommentString = "<b>Internal Serial Name</b>:";
        public const string ISBNCommentField = "[T:ISBN]";
        public const string ISBNCommentString = "<b>ISBN</b>:";
        public const string ISSNCommentField = "[T:ISSN]";
        public const string ISSNCommentString = "<b>ISSN</b>:";
        public const string JASRACIDCommentField = "[T:JID]";
        public const string JASRACIDCommentString = "<b>JASRAC ID</b>:";
        public const string KingRecordsIDCommentField = "[T:KIRZ]";
        public const string KingRecordsIDCommentString = "<b>King Records ID</b>:";
        public const string KoeiIDCommentField = "[T:KOEI]";
        public const string KoeiIDCommentString = "<b>Koei ID</b>:";
        public const string KonamiIDCommentField = "[T:KID]";
        public const string KonamiIDCommentString = "<b>Konami ID</b>:";
        public const string LucasArtsIDCommentField = "[T:LAID]";
        public const string LucasArtsIDCommentString = "<b>Lucas Arts ID</b>:";
        public const string NaganoIDCommentField = "[T:NGID]";
        public const string NaganoIDCommentString = "<b>Nagano ID</b>:";
        public const string NamcoIDCommentField = "[T:NID]";
        public const string NamcoIDCommentString = "<b>Namco ID</b>:";
        public const string NetYarozeGamesCommentField = "[T:NYG]";
        public const string NetYarozeGamesCommentSring = "Net Yaroze Games</b>:";
        public const string NipponIchiSoftwareIDCommentField = "[T:NPS]";
        public const string NipponIchiSoftwareIDCommentString = "<b>Nippon Ichi Software ID</b>:";
        public const string OriginIDCommentField = "[T:OID]";
        public const string OriginIDCommentString = "<b>Origin ID</b>:";
        public const string PatchesCommentField = "[T:P]";
        public const string PatchesCommentString = "<b>Patches</b>:";
        public const string PlayableDemosCommentField = "[T:PD]";
        public const string PlayableDemosCommentString = "<b>Playable Demos</b>:";
        public const string PonyCanyonIDCommentField = "[T:PCID]";
        public const string PonyCanyonIDCommentString = "<b>Pony Canyon ID</b>:";
        public const string PostgapTypeCommentField = "[T:PT2]";
        public const string PostgapTypeCommentString = "<b>Postgap type</b>: Form 2";
        public const string PPNCommentField = "[T:PPN]";
        public const string PPNCommentString = "<b>PPN</b>:";
        public const string RollingDemosCommentField = "[T:RD]";
        public const string RollingDemosCommentString = "<b>Rolling Demos</b>:";
        public const string SavegamesCommentField = "[T:SG]";
        public const string SavegamesCommentString = "<b>Savegames</b>:";
        public const string SegaIDCommentField = "[T:SID]";
        public const string SegaIDCommentString = "<b>Sega ID</b>:";
        public const string SelenIDCommentField = "[T:SNID]";
        public const string SelenIDCommentString = "<b>Selen ID</b>:";
        public const string SeriesCommentField = "[T:S]";
        public const string SeriesCommentString = "<b>Series</b>:";
        public const string TaitoIDCommentField = "[T:TID]";
        public const string TaitoIDCommentString = "<b>Taito ID</b>:";
        public const string TechDemosCommentField = "[T:TD]";
        public const string TechDemosCommentString = "<b>Tech Demos</b>:";
        public const string UbisoftIDCommentField = "[T:UID]";
        public const string UbisoftIDCommentString = "<b>Ubisoft ID</b>:";
        public const string ValveIDCommentField = "[T:VID]";
        public const string ValveIDCommentString = "<b>Valve ID</b>:";
        public const string VFCCodeCommentField = "[T:VFC]";
        public const string VGCCodeCommentString = "<b>VFC code</b>:";
        public const string VideosCommentField = "[T:V]";
        public const string VideosCommentString = "<b>Videos</b>:";
        public const string VolumeLabelCommentField = "[T:VOL]";
        public const string VolumeLabelCommentString = "<b>Volume Label</b>:";
        public const string VCDCommentField = "[T:VCD]";
        public const string VCDCommentString = "<b>V-CD</b>";

        // Default values

        public const string RequiredValue = "(REQUIRED)";
        public const string RequiredIfExistsValue = "(REQUIRED, IF EXISTS)";
        public const string OptionalValue = "(OPTIONAL)";
        public const string DiscNotDetected = "Disc Not Detected";
    }
}
