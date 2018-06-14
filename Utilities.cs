using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;

namespace DICUI
{
    public static class Utilities
    {
        /// <summary>
        /// Get the string representation of the DiscType enum values
        /// </summary>
        /// <param name="type">DiscType value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string DiscTypeToString(DiscType? type)
        {
            switch (type)
            {
                case DiscType.CD:
                    return "CD-ROM";
                case DiscType.DVD5:
                    return "DVD-5 [Single-Layer]";
                case DiscType.DVD9:
                    return "DVD-9 [Dual-Layer]";
                case DiscType.GDROM:
                    return "GD-ROM";
                case DiscType.HDDVD:
                    return "HD-DVD";
                case DiscType.BD25:
                    return "BluRay-25 [Single-Layer]";
                case DiscType.BD50:
                    return "BluRay-50 [Dual-Layer]";

                case DiscType.GameCubeGameDisc:
                    return "GameCube Game";
                case DiscType.WiiOpticalDisc:
                    return "Wii Optical";
                case DiscType.WiiUOpticalDisc:
                    return "Wii U Optical";
                case DiscType.UMD:
                    return "UMD";

                case DiscType.Floppy:
                    return "Floppy Disk";

                case DiscType.NONE:
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// Get the string representation of the KnownSystem enum values
        /// </summary>
        /// <param name="sys">KnownSystem value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string KnownSystemToString(KnownSystem? sys)
        {
            switch (sys)
            {
                #region Consoles

                case KnownSystem.BandaiPlaydiaQuickInteractiveSystem:
                    return "Bandai Playdia Quick Interactive System";
                case KnownSystem.BandaiApplePippin:
                    return "Bandai / Apple Pippin";
                case KnownSystem.CommodoreAmigaCD32:
                    return "Commodore Amiga CD32";
                case KnownSystem.CommodoreAmigaCDTV:
                    return "Commodore Amiga CDTV";
                case KnownSystem.MattelHyperscan:
                    return "Mattel HyperScan";
                case KnownSystem.MicrosoftXBOX:
                    return "Microsoft XBOX";
                case KnownSystem.MicrosoftXBOX360:
                    return "Microsoft XBOX 360";
                case KnownSystem.MicrosoftXBOXOne:
                    return "Microsoft XBOX One";
                case KnownSystem.NECPCEngineTurboGrafxCD:
                    return "NEC PC-Engine / TurboGrafx CD";
                case KnownSystem.NECPCFX:
                    return "NEC PC-FX / PC-FXGA";
                case KnownSystem.NintendoGameCube:
                    return "Nintendo GameCube";
                case KnownSystem.NintendoWii:
                    return "Nintendo Wii";
                case KnownSystem.NintendoWiiU:
                    return "Nintendo Wii U";
                case KnownSystem.Panasonic3DOInteractiveMultiplayer:
                    return "Panasonic 3DO Interactive Multiplayer";
                case KnownSystem.PhilipsCDi:
                    return "Philips CD-i";
                case KnownSystem.SegaCDMegaCD:
                    return "Sega CD / Mega CD";
                case KnownSystem.SegaDreamcast:
                    return "Sega Dreamcast";
                case KnownSystem.SegaSaturn:
                    return "Sega Saturn";
                case KnownSystem.SNKNeoGeoCD:
                    return "SNK Neo Geo CD";
                case KnownSystem.SonyPlayStation:
                    return "Sony PlayStation";
                case KnownSystem.SonyPlayStation2:
                    return "Sony PlayStation 2";
                case KnownSystem.SonyPlayStation3:
                    return "Sony PlayStation 3";
                case KnownSystem.SonyPlayStation4:
                    return "Sony PlayStation 4";
                case KnownSystem.SonyPlayStationPortable:
                    return "Sony PlayStation Portable";
                case KnownSystem.VMLabsNuon:
                    return "VM Labs NUON";
                case KnownSystem.VTechVFlashVSmilePro:
                    return "VTech V.Flash - V.Smile Pro";
                case KnownSystem.ZAPiTGamesGameWaveFamilyEntertainmentSystem:
                    return "ZAPiT Games Game Wave Family Entertainment System";

                #endregion

                #region Computers

                case KnownSystem.AcornArchimedes:
                    return "Acorn Archimedes";
                case KnownSystem.AppleMacintosh:
                    return "Apple Macintosh";
                case KnownSystem.CommodoreAmigaCD:
                    return "Commodore Amiga CD";
                case KnownSystem.FujitsuFMTowns:
                    return "Fujitsu FM Towns series";
                case KnownSystem.IBMPCCompatible:
                    return "IBM PC Compatible";
                case KnownSystem.NECPC88:
                    return "NEC PC-88";
                case KnownSystem.NECPC98:
                    return "NEC PC-98";
                case KnownSystem.SharpX68000:
                    return "Sharp X68000";

                #endregion

                #region Arcade

                case KnownSystem.NamcoSegaNintendoTriforce:
                    return "Namco / Sega / Nintendo Triforce";
                case KnownSystem.NamcoSystem246:
                    return "Namco System 246";
                case KnownSystem.SegaChihiro:
                    return "Sega Chihiro";
                case KnownSystem.SegaLindbergh:
                    return "Sega Lindbergh";
                case KnownSystem.SegaNaomi:
                    return "Sega Naomi";
                case KnownSystem.SegaNaomi2:
                    return "Sega Naomi 2";
                case KnownSystem.SegaNu:
                    return "Sega Nu";
                case KnownSystem.SegaRingEdge2:
                    return "Sega RingEdge 2";
                case KnownSystem.TABAustriaQuizard:
                    return "TAB-Austria Quizard";
                case KnownSystem.TandyMemorexVisualInformationSystem:
                    return "Tandy / Memorex Visual Information System";

                #endregion

                #region Others

                case KnownSystem.AudioCD:
                    return "Audio CD";
                case KnownSystem.BDVideo:
                    return "BD-Video";
                case KnownSystem.DVDVideo:
                    return "DVD-Video";
                case KnownSystem.EnhancedCD:
                    return "Enhanced CD";
                case KnownSystem.PalmOS:
                    return "PalmOS";
                case KnownSystem.PhilipsCDiDigitalVideo:
                    return "Philips CD-i Digital Video";
                case KnownSystem.PhotoCD:
                    return "Photo CD";
                case KnownSystem.PlayStationGameSharkUpdates:
                    return "PlayStation GameShark Updates";
                case KnownSystem.TaoiKTV:
                    return "Tao iKTV";
                case KnownSystem.TomyKissSite:
                    return "Tomy Kiss-Site";
                case KnownSystem.VideoCD:
                    return "Video CD";

                #endregion

                case KnownSystem.NONE:
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// Get a list of valid DiscTypes for a given system
        /// </summary>
        /// <param name="sys">KnownSystem value to check</param>
        /// <returns>List of DiscTypes</returns>
        public static List<DiscType?> GetValidDiscTypes(KnownSystem? sys)
        {
            List<DiscType?> types = new List<DiscType?>();
            
            switch (sys)
            {
                #region Consoles

                case KnownSystem.BandaiPlaydiaQuickInteractiveSystem:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.BandaiApplePippin:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.CommodoreAmigaCD32:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.CommodoreAmigaCDTV:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.MattelHyperscan:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.MicrosoftXBOX:
                    types.Add(DiscType.CD);
                    types.Add(DiscType.DVD5);
                    break;
                case KnownSystem.MicrosoftXBOX360:
                    types.Add(DiscType.CD);
                    types.Add(DiscType.DVD9);
                    types.Add(DiscType.HDDVD);
                    break;
                case KnownSystem.MicrosoftXBOXOne:
                    types.Add(DiscType.BD25);
                    types.Add(DiscType.BD50);
                    break;
                case KnownSystem.NECPCEngineTurboGrafxCD:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.NECPCFX:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.NintendoGameCube:
                    types.Add(DiscType.GameCubeGameDisc);
                    break;
                case KnownSystem.NintendoWii:
                    types.Add(DiscType.WiiOpticalDisc);
                    break;
                case KnownSystem.NintendoWiiU:
                    types.Add(DiscType.WiiUOpticalDisc);
                    break;
                case KnownSystem.Panasonic3DOInteractiveMultiplayer:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.PhilipsCDi:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.SegaCDMegaCD:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.SegaDreamcast:
                    types.Add(DiscType.GDROM);
                    break;
                case KnownSystem.SegaSaturn:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.SNKNeoGeoCD:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.SonyPlayStation:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.SonyPlayStation2:
                    types.Add(DiscType.CD);
                    types.Add(DiscType.DVD5);
                    types.Add(DiscType.DVD9);
                    break;
                case KnownSystem.SonyPlayStation3:
                    types.Add(DiscType.BD25);
                    types.Add(DiscType.BD50);
                    break;
                case KnownSystem.SonyPlayStation4:
                    types.Add(DiscType.BD25);
                    types.Add(DiscType.BD50);
                    break;
                case KnownSystem.SonyPlayStationPortable:
                    types.Add(DiscType.UMD);
                    break;
                case KnownSystem.VMLabsNuon:
                    types.Add(DiscType.DVD5);
                    types.Add(DiscType.DVD9);
                    break;
                case KnownSystem.VTechVFlashVSmilePro:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.ZAPiTGamesGameWaveFamilyEntertainmentSystem:
                    types.Add(DiscType.DVD5);
                    break;

                #endregion

                #region Computers

                case KnownSystem.AcornArchimedes:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.AppleMacintosh:
                    types.Add(DiscType.CD);
                    types.Add(DiscType.DVD5);
                    types.Add(DiscType.DVD9);
                    types.Add(DiscType.Floppy);
                    break;
                case KnownSystem.CommodoreAmigaCD:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.FujitsuFMTowns:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.IBMPCCompatible:
                    types.Add(DiscType.CD);
                    types.Add(DiscType.DVD5);
                    types.Add(DiscType.DVD9);
                    types.Add(DiscType.Floppy);
                    break;
                case KnownSystem.NECPC88:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.NECPC98:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.SharpX68000:
                    types.Add(DiscType.CD);
                    break;

                #endregion

                #region Arcade

                case KnownSystem.NamcoSegaNintendoTriforce:
                    types.Add(DiscType.GDROM);
                    break;
                case KnownSystem.SegaChihiro:
                    types.Add(DiscType.GDROM);
                    break;
                case KnownSystem.SegaLindbergh:
                    types.Add(DiscType.DVD5);
                    types.Add(DiscType.DVD9);
                    break;
                case KnownSystem.SegaNaomi:
                    types.Add(DiscType.GDROM);
                    break;
                case KnownSystem.SegaNaomi2:
                    types.Add(DiscType.GDROM);
                    break;
                case KnownSystem.SegaNu:
                    types.Add(DiscType.BD25);
                    types.Add(DiscType.BD50);
                    break;
                case KnownSystem.SegaRingEdge2:
                    types.Add(DiscType.DVD5);
                    types.Add(DiscType.DVD9);
                    break;
                case KnownSystem.TABAustriaQuizard:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.TandyMemorexVisualInformationSystem:
                    types.Add(DiscType.CD);
                    break;

                #endregion

                #region Others

                case KnownSystem.AudioCD:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.BDVideo:
                    types.Add(DiscType.BD25);
                    types.Add(DiscType.BD50);
                    break;
                case KnownSystem.DVDVideo:
                    types.Add(DiscType.DVD5);
                    types.Add(DiscType.DVD9);
                    break;
                case KnownSystem.EnhancedCD:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.PalmOS:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.PhilipsCDiDigitalVideo:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.PhotoCD:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.PlayStationGameSharkUpdates:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.TaoiKTV:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.TomyKissSite:
                    types.Add(DiscType.CD);
                    break;
                case KnownSystem.VideoCD:
                    types.Add(DiscType.CD);
                    break;

                #endregion

                case KnownSystem.NONE:
                default:
                    types.Add(DiscType.NONE);
                    break;
            }

            return types;
        }

        /// <summary>
        /// Get the DIC command to be used for a given DiscType
        /// </summary>
        /// <param name="type">DiscType value to check</param>
        /// <returns>String containing the command, null on error</returns>
        public static string GetBaseCommand(DiscType? type)
        {
            switch (type)
            {
                case DiscType.CD:
                    return DICCommands.CompactDiscCommand;
                case DiscType.DVD5:
                case DiscType.DVD9:
                    return DICCommands.DVDCommand;
                case DiscType.GDROM:
                    return DICCommands.GDROMCommand;
                case DiscType.HDDVD:
                    return null;
                case DiscType.BD25:
                case DiscType.BD50:
                    return DICCommands.BDCommand;

                // Special Formats
                case DiscType.GameCubeGameDisc:
                    return DICCommands.DVDCommand;
                case DiscType.WiiOpticalDisc:
                    return null;
                case DiscType.WiiUOpticalDisc:
                    return null;
                case DiscType.UMD:
                    return null;

                // Non-optical
                case DiscType.Floppy:
                    return DICCommands.FloppyCommand;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the DiscType associated with a given base command
        /// </summary>
        /// <param name="baseCommand">String value to check</param>
        /// <returns>DiscType if possible, null on error</returns>
        /// <remarks>This takes the "safe" route by assuming the larger of any given format</remarks>
        public static DiscType? GetDiscType(string baseCommand)
        {
            switch (baseCommand)
            {
                case DICCommands.CompactDiscCommand:
                    return DiscType.CD;
                case DICCommands.GDROMCommand:
                case DICCommands.GDROMSwapCommand:
                    return DiscType.GDROM;
                case DICCommands.DVDCommand:
                    return DiscType.DVD9;
                case DICCommands.BDCommand:
                    return DiscType.BD50;
                case DICCommands.XBOXCommand:
                    return DiscType.DVD5;

                // Non-optical
                case DICCommands.FloppyCommand:
                    return DiscType.Floppy;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the most common known system for a given DiscType
        /// </summary>
        /// <param name="type">DiscType value to check</param>
        /// <returns>KnownSystem if possible, null on error</returns>
        public static KnownSystem? GetKnownSystem(DiscType? type)
        {
            switch (type)
            {
                case DiscType.CD:
                case DiscType.DVD5:
                case DiscType.DVD9:
                case DiscType.Floppy:
                    return KnownSystem.IBMPCCompatible;
                case DiscType.GDROM:
                    return KnownSystem.SegaDreamcast;
                case DiscType.HDDVD:
                    return KnownSystem.MicrosoftXBOX360;
                case DiscType.BD25:
                case DiscType.BD50:
                    return KnownSystem.SonyPlayStation3;

                // Special Formats
                case DiscType.GameCubeGameDisc:
                    return KnownSystem.NintendoGameCube;
                case DiscType.WiiOpticalDisc:
                    return KnownSystem.NintendoWii;
                case DiscType.WiiUOpticalDisc:
                    return KnownSystem.NintendoWiiU;
                case DiscType.UMD:
                    return KnownSystem.SonyPlayStationPortable;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Get list of default parameters for a given system and disc type
        /// </summary>
        /// <param name="sys">KnownSystem value to check</param>
        /// <param name="type">DiscType value to check</param>
        /// <returns>List of strings representing the parameters</returns>
        public static List<string> GetDefaultParameters(KnownSystem? sys, DiscType? type)
        {
            // First check to see if the combination of system and disctype is valid
            List<DiscType?> validTypes = GetValidDiscTypes(sys);
            if (!validTypes.Contains(type))
            {
                return null;
            }

            // Now sort based on disc type
            List<string> parameters = new List<string>();
            switch (type)
            {
                case DiscType.CD:
                    parameters.Add(DICFlags.CDC2OpcodeFlag); parameters.Add("20");

                    switch (sys)
                    {
                        case KnownSystem.AppleMacintosh:
                        case KnownSystem.IBMPCCompatible:
                            parameters.Add(DICFlags.CDNoFixSubQSecuROMFlag);
                            parameters.Add(DICFlags.CDScanFileProtectFlag);
                            parameters.Add(DICFlags.CDScanSectorProtectFlag);
                            break;
                        case KnownSystem.NECPCEngineTurboGrafxCD:
                            parameters.Add(DICFlags.CDMCNFlag);
                            break;
                        case KnownSystem.SonyPlayStation:
                            parameters.Add(DICFlags.CDScanAnitModFlag);
                            break;
                    }
                    break;
                case DiscType.DVD5:
                    // Currently no defaults set
                    break;
                case DiscType.DVD9:
                    // Currently no defaults set
                    break;
                case DiscType.GDROM:
                    parameters.Add(DICFlags.CDC2OpcodeFlag); parameters.Add("20");
                    break;
                case DiscType.HDDVD:
                    break;
                case DiscType.BD25:
                    // Currently no defaults set
                    break;
                case DiscType.BD50:
                    // Currently no defaults set
                    break;

                // Special Formats
                case DiscType.GameCubeGameDisc:
                    parameters.Add(DICFlags.DVDRawFlag);
                    break;
                case DiscType.WiiOpticalDisc:
                    // Currently no defaults set
                    break;
                case DiscType.WiiUOpticalDisc:
                    // Currently no defaults set
                    break;
                case DiscType.UMD:
                    break;

                // Non-optical
                case DiscType.Floppy:
                    // Currently no defaults set
                    break;
            }

            return parameters;
        }

        /// <summary>
        /// Get the default extension for a given disc type
        /// </summary>
        /// <param name="type">DiscType value to check</param>
        /// <returns>Valid extension (with leading '.'), null on error</returns>
        public static string GetDefaultExtension(DiscType? type)
        {
            switch(type)
            {
                case DiscType.CD:
                case DiscType.GDROM:
                    return ".bin";
                case DiscType.DVD5:
                case DiscType.DVD9:
                case DiscType.HDDVD:
                case DiscType.BD25:
                case DiscType.BD50:
                case DiscType.GameCubeGameDisc:
                case DiscType.WiiOpticalDisc:
                case DiscType.UMD:
                    return ".iso";
                case DiscType.WiiUOpticalDisc:
                    return ".wud";
                case DiscType.Floppy:
                    return ".img";
                case DiscType.NONE:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Create a list of systems matched to their respective enums
        /// </summary>
        /// <returns>Systems matched to enums, if possible</returns>
        /// <remarks>
        /// This returns a List of Tuples whose structure is as follows:
        ///		Item 1: Printable name
        ///		Item 2: KnownSystem mapping
        ///		Item 3: DiscType mapping
        ///	If something has a "string, null, null" value, it should be assumed that it is a separator
        /// </remarks>
        public static List<Tuple<string, KnownSystem?, DiscType?>> CreateListOfSystems()
        {
            List<Tuple<string, KnownSystem?, DiscType?>> mapping = new List<Tuple<string, KnownSystem?, DiscType?>>();

            foreach (KnownSystem system in Enum.GetValues(typeof(KnownSystem)))
            {
                // In the special cases of breaks, we want to add the proper mappings for sections
                switch (system)
                {
                    // Consoles section
                    case KnownSystem.BandaiPlaydiaQuickInteractiveSystem:
                        mapping.Add(new Tuple<string, KnownSystem?, DiscType?>("---------- Consoles ----------", null, null));
                        break;

                    // Computers section
                    case KnownSystem.AcornArchimedes:
                        mapping.Add(new Tuple<string, KnownSystem?, DiscType?>("---------- Computers ----------", null, null));
                        break;

                    // Arcade section
                    case KnownSystem.NamcoSegaNintendoTriforce:
                        mapping.Add(new Tuple<string, KnownSystem?, DiscType?>("---------- Arcade ----------", null, null));
                        break;

                    // Other section
                    case KnownSystem.AudioCD:
                        mapping.Add(new Tuple<string, KnownSystem?, DiscType?>("---------- Others ----------", null, null));
                        break;
                }

                // First, get a list of all DiscTypes for a given KnownSystem
                List<DiscType?> types = GetValidDiscTypes(system);

                // If we have a single type, we don't want to postfix the system name with it
                if (types.Count == 1)
                {
                    mapping.Add(new Tuple<string, KnownSystem?, DiscType?>(KnownSystemToString(system), system, types[0]));
                }
                // Otherwise, postfix the system name properly
                else
                {
                    foreach (DiscType type in types)
                    {
                        mapping.Add(new Tuple<string, KnownSystem?, DiscType?>(KnownSystemToString(system) + " (" + DiscTypeToString(type) + ")", system, type));
                    }
                }
            }

            // Add final mapping for "Custom"
            mapping.Add(new Tuple<string, KnownSystem?, DiscType?>("Custom Input", KnownSystem.NONE, DiscType.NONE));

            return mapping;
        }

        /// <summary>
        /// Create a list of active optical drives matched to their volume labels
        /// </summary>
        /// <returns>Active drives, matched to labels, if possible</returns>
        /// <remarks>
        /// https://stackoverflow.com/questions/3060796/how-to-distinguish-between-usb-and-floppy-devices?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa
        /// https://msdn.microsoft.com/en-us/library/aa394173(v=vs.85).aspx
        /// This returns a List of Tuples whose structure is as follows:
        ///		Item 1: Drive letter
        ///		Item 2: Volume label
        ///		Item 3: (True for floppy drive, false otherwise)
        /// </remarks>
        public static List<Tuple<char, string, bool>> CreateListOfDrives()
        {
            // Get the floppy drives
            List<Tuple<char, string, bool>> floppyDrives = new List<Tuple<char, string, bool>>();
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_LogicalDisk");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    uint? mediaType = (uint?)queryObj["MediaType"];
                    if (mediaType != null && ((mediaType > 0 && mediaType < 11) || (mediaType > 12 && mediaType < 22)))
                    {
                        char devId = queryObj["DeviceID"].ToString()[0];
                        floppyDrives.Add(new Tuple<char, string, bool>(devId, "FLOPPY", true));
                    }
                }
            }
            catch
            {
                // No-op
            }

            // Get the optical disc drives
            List<Tuple<char, string, bool>> discDrives = DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.CDRom && d.IsReady)
                .Select(d => new Tuple<char, string, bool>(d.Name[0], d.VolumeLabel, false))
                .ToList();

            // Add the two lists together, order, and return
            floppyDrives.AddRange(discDrives);
            return floppyDrives.OrderBy(i => i.Item1).ToList();
        }

        /// <summary>
        /// Validate that at string would be valid as input to DiscImageCreator
        /// </summary>
        /// <param name="parameters">String representing all parameters</param>
        /// <returns>True if it would be valid, false otherwise</returns>
        public static bool ValidateParameters(string parameters)
        {
            // The string has to be valid by itself first
            if (String.IsNullOrWhiteSpace(parameters))
            {
                return false;
            }

            // Now split the string into parts for easier validation
            // https://stackoverflow.com/questions/14655023/split-a-string-that-has-white-spaces-unless-they-are-enclosed-within-quotes
            parameters = parameters.Trim();
            List<string> parts = Regex.Matches(parameters, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToList();

            // Determine what the commandline should look like given the first item
            int index = -1;
            switch (parts[0])
            {
                case DICCommands.CompactDiscCommand:
                case DICCommands.GDROMCommand:
                case DICCommands.GDROMSwapCommand:
                case DICCommands.DataCommand:
                    if (!IsValidDriveLetter(parts[1]))
                    {
                        return false;
                    }
                    else if (IsFlag(parts[2]))
                    {
                        return false;
                    }
                    else if (!IsValidNumber(parts[3], lowerBound: 0, upperBound: 72))
                    {
                        return false;
                    }

                    if (parts[0] == DICCommands.GDROMSwapCommand)
                    {
                        if (parts.Count > 4)
                        {
                            return false;
                        }
                    }
                    else if (parts[0] == DICCommands.DataCommand || parts[0] == DICCommands.AudioCommand)
                    {
                        if (!IsValidNumber(parts[4]) || !IsValidNumber(parts[5]))
                        {
                            return false;
                        }

                        index = 6;
                    }
                    else
                    {
                        index = 4;
                    }

                    break;
                case DICCommands.DVDCommand:
                    if (!IsValidDriveLetter(parts[1]))
                    {
                        return false;
                    }
                    else if (IsFlag(parts[2]))
                    {
                        return false;
                    }
                    else if (!IsValidNumber(parts[3], lowerBound: 0, upperBound: 72)) // Officially 0-16
                    {
                        return false;
                    }

                    // Loop through all auxilary flags
                    for (int i = 4; i < parts.Count; i++)
                    {
                        switch (parts[i])
                        {
                            case DICFlags.DisableBeepFlag:
                            case DICFlags.DVDCMIFlag:
                            case DICFlags.DVDRawFlag:
                                // No-op, all of these are single flags
                                break;
                            case DICFlags.ForceUnitAccessFlag:
                                // If the next item doesn't exist, it's good
                                if (!DoesNextExist(parts, i))
                                {
                                    break;
                                }
                                // If the next item is a flag, it's good
                                if (IsFlag(parts[i + 1]))
                                {
                                    break;
                                }
                                // If the next item isn't a valid number
                                else if (!IsValidNumber(parts[i + 1], lowerBound: 0))
                                {
                                    return false;
                                }
                                i++;
                                break;
                            default:
                                return false;
                        }
                    }
                    break;
                case DICCommands.BDCommand:
                case DICCommands.XBOXCommand:
                    if (!IsValidDriveLetter(parts[1]))
                    {
                        return false;
                    }
                    else if (IsFlag(parts[2]))
                    {
                        return false;
                    }

                    // Loop through all auxilary flags
                    for (int i = 3; i < parts.Count; i++)
                    {
                        switch (parts[i])
                        {
                            case DICFlags.DisableBeepFlag:
                                // No-op, this is a single flag
                                break;
                            case DICFlags.ForceUnitAccessFlag:
                                // If the next item doesn't exist, it's good
                                if (!DoesNextExist(parts, i))
                                {
                                    break;
                                }
                                // If the next item is a flag, it's good
                                if (IsFlag(parts[i + 1]))
                                {
                                    break;
                                }
                                // If the next item isn't a valid number
                                else if (!IsValidNumber(parts[i + 1], lowerBound: 0))
                                {
                                    return false;
                                }
                                i++;
                                break;
                            default:
                                return false;
                        }
                    }
                    break;
                case DICCommands.FloppyCommand:
                    if (!IsValidDriveLetter(parts[1]))
                    {
                        return false;
                    }
                    else if (IsFlag(parts[2]))
                    {
                        return false;
                    }
                    else if (parts.Count > 3)
                    {
                        return false;
                    }
                    break;
                case DICCommands.StopCommand:
                case DICCommands.StartCommand:
                case DICCommands.EjectCommand:
                case DICCommands.CloseCommand:
                case DICCommands.ResetCommand:
                case DICCommands.DriveSpeedCommand:
                    if (!IsValidDriveLetter(parts[1]))
                    {
                        return false;
                    }
                    else if (parts.Count > 2)
                    {
                        return false;
                    }
                    break;
                case DICCommands.SubCommand:
                case DICCommands.MDSCommand:
                    if (IsFlag(parts[1]))
                    {
                        return false;
                    }
                    else if (parts.Count > 2)
                    {
                        return false;
                    }
                    break;
                default:
                    return false;
            }

            // Loop through all auxilary flags, if necessary
            if (index > 0)
            {
                for (int i = index; i < parts.Count; i++)
                {
                    switch(parts[i])
                    {
                        case DICFlags.DisableBeepFlag:
                            if (parts[0] != DICCommands.CompactDiscCommand
                                && parts[0] != DICCommands.GDROMCommand
                                && parts[0] != DICCommands.DataCommand
                                && parts[0] != DICCommands.AudioCommand)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.CDD8OpcodeFlag:
                            if (parts[0] != DICCommands.CompactDiscCommand
                                && parts[0] != DICCommands.GDROMCommand
                                && parts[0] != DICCommands.DataCommand
                                && parts[0] != DICCommands.AudioCommand)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.CDMCNFlag:
                            if (parts[0] != DICCommands.CompactDiscCommand)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.CDAMSFFlag:
                            if (parts[0] != DICCommands.CompactDiscCommand)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.CDReverseFlag:
                            if (parts[0] != DICCommands.CompactDiscCommand
                                && parts[0] != DICCommands.DataCommand)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.CDMultiSessionFlag:
                            if (parts[0] != DICCommands.CompactDiscCommand)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.CDScanSectorProtectFlag:
                            if (parts[0] != DICCommands.CompactDiscCommand
                                && parts[0] != DICCommands.DataCommand)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.CDScanAnitModFlag:
                            if (parts[0] != DICCommands.CompactDiscCommand)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.CDNoFixSubPFlag:
                            if (parts[0] != DICCommands.CompactDiscCommand
                                && parts[0] != DICCommands.GDROMCommand
                                && parts[0] != DICCommands.DataCommand
                                && parts[0] != DICCommands.AudioCommand)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.CDNoFixSubQFlag:
                            if (parts[0] != DICCommands.CompactDiscCommand
                                && parts[0] != DICCommands.GDROMCommand
                                && parts[0] != DICCommands.DataCommand
                                && parts[0] != DICCommands.AudioCommand)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.CDNoFixSubRtoWFlag:
                            if (parts[0] != DICCommands.CompactDiscCommand
                                && parts[0] != DICCommands.GDROMCommand
                                && parts[0] != DICCommands.DataCommand
                                && parts[0] != DICCommands.AudioCommand)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.CDNoFixSubQLibCryptFlag:
                            if (parts[0] != DICCommands.CompactDiscCommand)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.CDNoFixSubQSecuROMFlag:
                            if (parts[0] != DICCommands.CompactDiscCommand
                                && parts[0] != DICCommands.GDROMCommand
                                && parts[0] != DICCommands.DataCommand
                                && parts[0] != DICCommands.AudioCommand)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.CDScanFileProtectFlag:
                            if (parts[0] != DICCommands.CompactDiscCommand
                                && parts[0] != DICCommands.DataCommand)
                            {
                                return false;
                            }
                        
                            // If the next item doesn't exist, it's good
                            if (!DoesNextExist(parts, i))
                            {
                                break;
                            }
                            // If the next item is a flag, it's good
                            if (IsFlag(parts[i + 1]))
                            {
                                break;
                            }
                            // If the next item isn't a valid number
                            else if (!IsValidNumber(parts[i + 1], lowerBound: 0))
                            {
                                return false;
                            }
                            i++;
                            break;
                        case DICFlags.ForceUnitAccessFlag: // CD, GDROM, Data, Audio
                            if (parts[0] != DICCommands.CompactDiscCommand
                                && parts[0] != DICCommands.GDROMCommand
                                && parts[0] != DICCommands.DataCommand
                                && parts[0] != DICCommands.AudioCommand)
                            {
                                return false;
                            }

                            // If the next item doesn't exist, it's good
                            if (!DoesNextExist(parts, i))
                            {
                                break;
                            }
                            // If the next item is a flag, it's good
                            if (IsFlag(parts[i + 1]))
                            {
                                break;
                            }
                            // If the next item isn't a valid number
                            else if (!IsValidNumber(parts[i + 1], lowerBound: 0))
                            {
                                return false;
                            }
                            i++;
                            break;
                        case DICFlags.CDAddOffsetFlag:
                            if (parts[0] != DICCommands.CompactDiscCommand
                                && parts[0] != DICCommands.AudioCommand)
                            {
                                return false;
                            }

                            // If the next item doesn't exist, it's not good
                            if (parts.Count == i + 1)
                            {
                                return false;
                            }
                            // If the next item isn't a valid number
                            else if (IsValidNumber(parts[i + 1]))
                            {
                                return false;
                            }
                            break;
                        case DICFlags.CDBEOpcodeFlag:
                            if (parts[0] != DICCommands.CompactDiscCommand
                                && parts[0] != DICCommands.GDROMCommand
                                && parts[0] != DICCommands.DataCommand
                                && parts[0] != DICCommands.AudioCommand)
                            {
                                return false;
                            }

                            // If the next item doesn't exist, it's good
                            if (!DoesNextExist(parts, i))
                            {
                                break;
                            }
                            // If the next item is a flag, it's good
                            if (IsFlag(parts[i + 1]))
                            {
                                break;
                            }
                            else if (parts[i + 1] != "raw"
                                && (parts[i + 1] != "pack"))
                            {
                                return false;
                            }
                            i++;
                            break;
                        case DICFlags.CDC2OpcodeFlag:
                            if (parts[0] != DICCommands.CompactDiscCommand
                                && parts[0] != DICCommands.GDROMCommand
                                && parts[0] != DICCommands.DataCommand
                                && parts[0] != DICCommands.AudioCommand)
                            {
                                return false;
                            }

                            for (int j = 1; j < 4; j++)
                            {
                                // If the next item doesn't exist, it's good
                                if (!DoesNextExist(parts, i + j - 1))
                                {
                                    break;
                                }
                                // If the next item is a flag, it's good
                                if (IsFlag(parts[i + j]))
                                {
                                    i += (j - 1);
                                    break;
                                }
                                // If the next item isn't a valid number
                                else if (!IsValidNumber(parts[i + j], lowerBound: 0))
                                {
                                    return false;
                                }
                            }
                            break;
                        case DICFlags.CDSubchannelReadLevelFlag:
                            if (parts[0] != DICCommands.CompactDiscCommand
                                && parts[0] != DICCommands.GDROMCommand
                                && parts[0] != DICCommands.DataCommand
                                && parts[0] != DICCommands.AudioCommand)
                            {
                                return false;
                            }

                            // If the next item doesn't exist, it's good
                            if (!DoesNextExist(parts, i))
                            {
                                break;
                            }
                            // If the next item is a flag, it's good
                            if (IsFlag(parts[i + 1]))
                            {
                                break;
                            }
                            // If the next item isn't a valid number
                            else if (!IsValidNumber(parts[3], lowerBound: 0, upperBound: 2))
                            {
                                return false;
                            }
                            break;
                        default:
                            return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Returns whether a string is a valid drive letter
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <returns>True if it's a valid drive letter, false otherwise</returns>
        private static bool IsValidDriveLetter(string parameter)
        {
            if (!Regex.IsMatch(parameter, @"^[A-Z]:?\\?$"))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns whether a string is a flag (starts with '/')
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <returns>True if it's a flag, false otherwise</returns>
        private static bool IsFlag(string parameter)
        {
            if (parameter.Trim('\"').StartsWith("/"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether or not the next item exists
        /// </summary>
        /// <param name="parameters">List of parameters to check against</param>
        /// <param name="index">Current index</param>
        /// <returns>True if the next item exists, false otherwise</returns>
        private static bool DoesNextExist(List<string> parameters, int index)
        {
            if (index >= parameters.Count - 1)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns whether a string is a valid number
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <param name="lowerBound">Lower bound (>=)</param>
        /// <param name="upperBound">Upper bound (<=)</param>
        /// <returns>True if it's a valid number, false otherwise</returns>
        private static bool IsValidNumber(string parameter, int lowerBound = -1, int upperBound = -1)
        {
            if (!Int32.TryParse(parameter, out int temp))
            {
                return false;
            }
            else if (lowerBound != -1 && temp < lowerBound)
            {
                return false;
            }
            else if (upperBound != -1 && temp > upperBound)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determine the base flags to use for checking a commandline
        /// </summary>
        /// <param name="parameters">Parameters as a string to check</param>
        /// <param name="command">Output string containing the found command</param>
        /// <param name="letter">Output string containing the found drive letter</param>
        /// <param name="path">Output string containing the found path</param>
        /// <returns>False on error (and all outputs set to null), true otherwise</returns>
        public static bool DetermineFlags(string parameters, out string command, out string letter, out string path)
        {
            command = null; letter = null; path = null;

            // The string has to be valid by itself first
            if (String.IsNullOrWhiteSpace(parameters))
            {
                return false;
            }

            // Now split the string into parts for easier validation
            // https://stackoverflow.com/questions/14655023/split-a-string-that-has-white-spaces-unless-they-are-enclosed-within-quotes
            parameters = parameters.Trim();
            List<string> parts = Regex.Matches(parameters, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToList();

            // Determine what the commandline should look like given the first item
            switch (parts[0])
            {
                case DICCommands.CompactDiscCommand:
                case DICCommands.GDROMCommand:
                case DICCommands.GDROMSwapCommand:
                case DICCommands.DataCommand:
                case DICCommands.AudioCommand:
                case DICCommands.DVDCommand:
                case DICCommands.BDCommand:
                case DICCommands.XBOXCommand:
                case DICCommands.FloppyCommand:
                    command = parts[0];

                    if (!IsValidDriveLetter(parts[1]))
                    {
                        return false;
                    }
                    letter = parts[1];

                    if (IsFlag(parts[2]))
                    {
                        return false;
                    }
                    path = parts[2].Trim('\"');

                    break;
                case DICCommands.StopCommand:
                case DICCommands.StartCommand:
                case DICCommands.EjectCommand:
                case DICCommands.CloseCommand:
                case DICCommands.ResetCommand:
                case DICCommands.DriveSpeedCommand:
                    command = parts[0];

                    if (!IsValidDriveLetter(parts[1]))
                    {
                        return false;
                    }
                    letter = parts[1];

                    break;
                case DICCommands.SubCommand:
                case DICCommands.MDSCommand:
                    command = parts[0];

                    if (IsFlag(parts[1]))
                    {
                        return false;
                    }
                    path = parts[1].Trim('\"');

                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}
