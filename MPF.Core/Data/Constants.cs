using System.Collections.Generic;
using System.Linq;
using SabreTools.RedumpLib.Data;

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
        public static readonly byte[] SaturnSectorZeroStart = [0x53, 0x45, 0x47, 0x41, 0x20, 0x53, 0x45, 0x47, 0x41, 0x53, 0x41, 0x54, 0x55, 0x52, 0x4E, 0x20];

        // Lists of known drive speed ranges
#if NET40
        public static IList<int> CD { get; } = new List<int> { 1, 2, 3, 4, 6, 8, 12, 16, 20, 24, 32, 40, 44, 48, 52, 56, 72 };
        public static IList<int> DVD { get; } = CD.Where(s => s <= 24).ToList();
        public static IList<int> HDDVD { get; } = CD.Where(s => s <= 24).ToList();
        public static IList<int> BD { get; } = CD.Where(s => s <= 16).ToList();
        public static IList<int> Unknown { get; } = new List<int> { 1 };
#else
        public static IReadOnlyList<int> CD { get; } = new List<int> { 1, 2, 3, 4, 6, 8, 12, 16, 20, 24, 32, 40, 44, 48, 52, 56, 72 };
        public static IReadOnlyList<int> DVD { get; } = CD.Where(s => s <= 24).ToList();
        public static IReadOnlyList<int> HDDVD { get; } = CD.Where(s => s <= 24).ToList();
        public static IReadOnlyList<int> BD { get; } = CD.Where(s => s <= 16).ToList();
        public static IReadOnlyList<int> Unknown { get; } = new List<int> { 1 };
#endif

        /// <summary>
        /// Get list of all drive speeds for a given MediaType
        /// </summary>
        /// <param name="type">MediaType? that represents the current item</param>
        /// <returns>Read-only list of drive speeds</returns>
#if NET40
        public static IList<int> GetSpeedsForMediaType(MediaType? type)
#else
        public static IReadOnlyList<int> GetSpeedsForMediaType(MediaType? type)
#endif
        {
            return type switch
            {
                MediaType.CDROM
                    or MediaType.GDROM => CD,
                MediaType.DVD
                    or MediaType.NintendoGameCubeGameDisc
                    or MediaType.NintendoWiiOpticalDisc => DVD,
                MediaType.HDDVD => HDDVD,
                MediaType.BluRay => BD,
                _ => Unknown,
            };
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

        public const string DumpingProgramField = "Dumping Program";
        public const string DumpingDateField = "Date";
        public const string DumpingDriveManufacturer = "Manufacturer";
        public const string DumpingDriveModel = "Model";
        public const string DumpingDriveFirmware = "Firmware";
        public const string ReportedDiscType = "Reported Disc Type";
        public const string PVDField = "Primary Volume Descriptor (PVD)";
        public const string DATField = "DAT";
        public const string SizeField = "Size";
        public const string CRC32Field = "CRC32";
        public const string MD5Field = "MD5";
        public const string SHA1Field = "SHA1";
        public const string FullyMatchingIDField = "Fully Matching ID";
        public const string PartiallyMatchingIDsField = "Partially Matching IDs";
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
        public const string XBOXSSRanges = "Security Sector Ranges";

        // Default values

        public const string RequiredValue = "(REQUIRED)";
        public const string RequiredIfExistsValue = "(REQUIRED, IF EXISTS)";
        public const string OptionalValue = "(OPTIONAL)";
        public const string DiscNotDetected = "Disc Not Detected";
    }
}
