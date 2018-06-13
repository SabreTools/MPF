using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                    return "GameCube";
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
                    types.Add(DiscType.DVD5); // TODO: Confirm
                    types.Add(DiscType.DVD9); // TODO: Confirm
                    break;
                case KnownSystem.NintendoWiiU:
                    types.Add(DiscType.DVD5); // TODO: Confirm
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
                    types.Add(DiscType.DVD5); // TODO: Confirm
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
                    types.Add(DiscType.DVD5); // TODO: Confirm
                    break;
                case KnownSystem.SegaNaomi:
                    types.Add(DiscType.GDROM);
                    break;
                case KnownSystem.SegaNaomi2:
                    types.Add(DiscType.GDROM);
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
                    return DICCommands.GDROMCommand; // TODO: Constants.GDROMSwapCommand?
                case DiscType.HDDVD:
                    return null;
                case DiscType.BD25:
                case DiscType.BD50:
                    return DICCommands.BDCommand;

                // Special Formats
                case DiscType.GameCubeGameDisc:
                    return DICCommands.DVDCommand;
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
                    parameters.Add(DICCommands.CDC2OpcodeFlag); parameters.Add("20");

                    switch (sys)
                    {
                        case KnownSystem.AppleMacintosh:
                        case KnownSystem.IBMPCCompatible:
                            parameters.Add(DICCommands.CDNoFixSubQSecuROMFlag);
                            parameters.Add(DICCommands.CDScanFileProtectFlag);
                            parameters.Add(DICCommands.CDScanSectorProtectFlag);
                            break;
                        case KnownSystem.NECPCEngineTurboGrafxCD:
                            parameters.Add(DICCommands.CDMCNFlag);
                            break;
                        case KnownSystem.SonyPlayStation:
                            parameters.Add(DICCommands.CDScanAnitModFlag);
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
                    parameters.Add(DICCommands.CDC2OpcodeFlag); parameters.Add("20");
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
                    parameters.Add(DICCommands.DVDRawFlag);
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
                case DiscType.UMD:
                    return ".iso";
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
        /// This returns a List of Tuples whose structure is as follows:
        ///		Item 1: Drive letter
        ///		Item 2: Volume label
        /// </remarks>
        public static List<Tuple<char, string>> CreateListOfDrives()
        {
            // TODO: Floppy drives show up as DriveType.Removable, but so do USB drives, 
            return DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.CDRom && d.IsReady)
                .Select(d => new Tuple<char, string>(d.Name[0], d.VolumeLabel))
                .ToList();
        }

        /// <summary>
        /// Validate that at string would be valid as input to DiscImageCreator
        /// </summary>
        /// <param name="parameters">String representing all parameters</param>
        /// <returns>True if it would be valid, false otherwise</returns>
        /// <remarks>TODO: Refactor this to make it cleaner</remarks>
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
            switch (parts[0])
            {
                case DICCommands.CompactDiscCommand:
                    if (!Regex.IsMatch(parts[1], @"[A-Z]:?\\?"))
                    {
                        return false;
                    }
                    else if (parts[2].Trim('\"').StartsWith("/"))
                    {
                        return false;
                    }
                    else if (!Int32.TryParse(parts[3], out int cdspeed))
                    {
                        return false;
                    }
                    else if (cdspeed < 0 || cdspeed > 72)
                    {
                        return false;
                    }

                    // Loop through all auxilary flags
                    for (int i = 4; i < parts.Count; i++)
                    {
                        switch (parts[i])
                        {
                            case DICCommands.DisableBeepFlag:
                            case DICCommands.CDD8OpcodeFlag:
                            case DICCommands.CDMCNFlag:
                            case DICCommands.CDAMSFFlag:
                            case DICCommands.CDReverseFlag:
                            case DICCommands.CDMultiSessionFlag:
                            case DICCommands.CDScanSectorProtectFlag:
                            case DICCommands.CDScanAnitModFlag:
                            case DICCommands.CDNoFixSubPFlag:
                            case DICCommands.CDNoFixSubQFlag:
                            case DICCommands.CDNoFixSubRtoWFlag:
                            case DICCommands.CDNoFixSubQLibCryptFlag:
                            case DICCommands.CDNoFixSubQSecuROMFlag:
                                // No-op, all of these are single flags
                                break;
                            case DICCommands.CDScanFileProtectFlag:
                                // If the next item is a flag, it's good
                                if (parts[i + 1].StartsWith("/"))
                                {
                                    break;
                                }
                                // If the next item isn't a valid number
                                else if (!Int32.TryParse(parts[i + 1], out int sfp1))
                                {
                                    return false;
                                }
                                else if (sfp1 < 0)
                                {
                                    return false;
                                }
                                i++;
                                break;
                            case DICCommands.ForceUnitAccessFlag:
                                // If the next item is a flag, it's good
                                if (parts[i + 1].StartsWith("/"))
                                {
                                    break;
                                }
                                // If the next item isn't a valid number
                                else if (!Int32.TryParse(parts[i + 1], out int fua1))
                                {
                                    return false;
                                }
                                else if (fua1 < 0)
                                {
                                    return false;
                                }
                                i++;
                                break;
                            case DICCommands.CDAddOffsetFlag:
                                // If the next item isn't a valid number
                                if (!Int32.TryParse(parts[i + 1], out int af1))
                                {
                                    return false;
                                }
                                break;
                            case DICCommands.CDBEOpcodeFlag:
                                // If the next item is a flag, it's good
                                if (parts[i + 1].StartsWith("/"))
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
                            case DICCommands.CDC2OpcodeFlag:
                                for (int j = 1; j < 4; j++)
                                {
                                    // If the next item is a flag, it's good
                                    if (parts[i + j].StartsWith("/"))
                                    {
                                        i += (j - 1);
                                        break;
                                    }
                                    // If the next item isn't a valid number
                                    else if (!Int32.TryParse(parts[i + j], out int c2))
                                    {
                                        return false;
                                    }
                                    else if (c2 < 0)
                                    {
                                        return false;
                                    }
                                }
                                break;
                            case DICCommands.CDSubchannelReadLevelFlag:
                                // If the next item is a flag, it's good
                                if (parts[i + 1].StartsWith("/"))
                                {
                                    break;
                                }
                                // If the next item isn't a valid number
                                else if (!Int32.TryParse(parts[i + 1], out int sub))
                                {
                                    return false;
                                }
                                else if (sub < 0 || sub > 2)
                                {
                                    return false;
                                }
                                break;
                            default:
                                return false;
                        }
                    }
                    break;
                case DICCommands.GDROMCommand:
                    if (!Regex.IsMatch(parts[1], @"[A-Z]:?\\?"))
                    {
                        return false;
                    }
                    else if (parts[2].Trim('\"').StartsWith("/"))
                    {
                        return false;
                    }
                    else if (!Int32.TryParse(parts[3], out int cdspeed))
                    {
                        return false;
                    }
                    else if (cdspeed < 0 || cdspeed > 72)
                    {
                        return false;
                    }

                    // Loop through all auxilary flags
                    for (int i = 4; i < parts.Count; i++)
                    {
                        switch (parts[i])
                        {
                            case DICCommands.DisableBeepFlag:
                            case DICCommands.CDD8OpcodeFlag:
                            case DICCommands.CDNoFixSubPFlag:
                            case DICCommands.CDNoFixSubQFlag:
                            case DICCommands.CDNoFixSubRtoWFlag:
                            case DICCommands.CDNoFixSubQSecuROMFlag:
                                // No-op, all of these are single flags
                                break;
                            case DICCommands.ForceUnitAccessFlag:
                                // If the next item is a flag, it's good
                                if (parts[i + 1].StartsWith("/"))
                                {
                                    break;
                                }
                                // If the next item isn't a valid number
                                else if (!Int32.TryParse(parts[i + 1], out int fua1))
                                {
                                    return false;
                                }
                                else if (fua1 < 0)
                                {
                                    return false;
                                }
                                i++;
                                break;
                            case DICCommands.CDBEOpcodeFlag:
                                // If the next item is a flag, it's good
                                if (parts[i + 1].StartsWith("/"))
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
                            case DICCommands.CDC2OpcodeFlag:
                                for (int j = 1; j < 4; j++)
                                {
                                    // If the next item is a flag, it's good
                                    if (parts[i + j].StartsWith("/"))
                                    {
                                        i += (j - 1);
                                        break;
                                    }
                                    // If the next item isn't a valid number
                                    else if (!Int32.TryParse(parts[i + j], out int c2))
                                    {
                                        return false;
                                    }
                                    else if (c2 < 0)
                                    {
                                        return false;
                                    }
                                }
                                break;
                            case DICCommands.CDSubchannelReadLevelFlag:
                                // If the next item is a flag, it's good
                                if (parts[i + 1].StartsWith("/"))
                                {
                                    break;
                                }
                                // If the next item isn't a valid number
                                else if (!Int32.TryParse(parts[i + 1], out int sub))
                                {
                                    return false;
                                }
                                else if (sub < 0 || sub > 2)
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
                case DICCommands.DataCommand:
                    if (!Regex.IsMatch(parts[1], @"[A-Z]:?\\?"))
                    {
                        return false;
                    }
                    else if (parts[2].Trim('\"').StartsWith("/"))
                    {
                        return false;
                    }
                    else if (!Int32.TryParse(parts[3], out int cdspeed))
                    {
                        return false;
                    }
                    else if (cdspeed < 0 || cdspeed > 72)
                    {
                        return false;
                    }
                    else if (!Int32.TryParse(parts[4], out int startlba)
                        || !Int32.TryParse(parts[5], out int endlba))
                    {
                        return false;
                    }

                    // Loop through all auxilary flags
                    for (int i = 6; i < parts.Count; i++)
                    {
                        switch (parts[i])
                        {
                            case DICCommands.DisableBeepFlag:
                            case DICCommands.CDD8OpcodeFlag:
                            case DICCommands.CDReverseFlag:
                            case DICCommands.CDScanSectorProtectFlag:
                            case DICCommands.CDNoFixSubPFlag:
                            case DICCommands.CDNoFixSubQFlag:
                            case DICCommands.CDNoFixSubRtoWFlag:
                            case DICCommands.CDNoFixSubQSecuROMFlag:
                                // No-op, all of these are single flags
                                break;
                            case DICCommands.ForceUnitAccessFlag:
                                // If the next item is a flag, it's good
                                if (parts[i + 1].StartsWith("/"))
                                {
                                    break;
                                }
                                // If the next item isn't a valid number
                                else if (!Int32.TryParse(parts[i + 1], out int fua1))
                                {
                                    return false;
                                }
                                else if (fua1 < 0)
                                {
                                    return false;
                                }
                                i++;
                                break;
                            case DICCommands.CDScanFileProtectFlag:
                                // If the next item is a flag, it's good
                                if (parts[i + 1].StartsWith("/"))
                                {
                                    break;
                                }
                                // If the next item isn't a valid number
                                else if (!Int32.TryParse(parts[i + 1], out int sfp1))
                                {
                                    return false;
                                }
                                else if (sfp1 < 0)
                                {
                                    return false;
                                }
                                i++;
                                break;
                            case DICCommands.CDBEOpcodeFlag:
                                // If the next item is a flag, it's good
                                if (parts[i + 1].StartsWith("/"))
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
                            case DICCommands.CDC2OpcodeFlag:
                                for (int j = 1; j < 4; j++)
                                {
                                    // If the next item is a flag, it's good
                                    if (parts[i + j].StartsWith("/"))
                                    {
                                        i += (j - 1);
                                        break;
                                    }
                                    // If the next item isn't a valid number
                                    else if (!Int32.TryParse(parts[i + j], out int c2))
                                    {
                                        return false;
                                    }
                                    else if (c2 < 0)
                                    {
                                        return false;
                                    }
                                }
                                break;
                            case DICCommands.CDSubchannelReadLevelFlag:
                                // If the next item is a flag, it's good
                                if (parts[i + 1].StartsWith("/"))
                                {
                                    break;
                                }
                                // If the next item isn't a valid number
                                else if (!Int32.TryParse(parts[i + 1], out int sub))
                                {
                                    return false;
                                }
                                else if (sub < 0 || sub > 2)
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
                case DICCommands.AudioCommand:
                    if (!Regex.IsMatch(parts[1], @"[A-Z]:?\\?"))
                    {
                        return false;
                    }
                    else if (parts[2].Trim('\"').StartsWith("/"))
                    {
                        return false;
                    }
                    else if (!Int32.TryParse(parts[3], out int cdspeed))
                    {
                        return false;
                    }
                    else if (cdspeed < 0 || cdspeed > 72)
                    {
                        return false;
                    }
                    else if (!Int32.TryParse(parts[4], out int startlba)
                        || !Int32.TryParse(parts[5], out int endlba))
                    {
                        return false;
                    }

                    // Loop through all auxilary flags
                    for (int i = 6; i < parts.Count; i++)
                    {
                        switch (parts[i])
                        {
                            case DICCommands.DisableBeepFlag:
                            case DICCommands.CDD8OpcodeFlag:
                            case DICCommands.CDNoFixSubPFlag:
                            case DICCommands.CDNoFixSubQFlag:
                            case DICCommands.CDNoFixSubRtoWFlag:
                            case DICCommands.CDNoFixSubQSecuROMFlag:
                                // No-op, all of these are single flags
                                break;
                            case DICCommands.ForceUnitAccessFlag:
                                // If the next item is a flag, it's good
                                if (parts[i + 1].StartsWith("/"))
                                {
                                    break;
                                }
                                // If the next item isn't a valid number
                                else if (!Int32.TryParse(parts[i + 1], out int fua1))
                                {
                                    return false;
                                }
                                else if (fua1 < 0)
                                {
                                    return false;
                                }
                                i++;
                                break;
                            case DICCommands.CDAddOffsetFlag:
                                // If the next item isn't a valid number
                                if (!Int32.TryParse(parts[i + 1], out int af1))
                                {
                                    return false;
                                }
                                break;
                            case DICCommands.CDBEOpcodeFlag:
                                // If the next item is a flag, it's good
                                if (parts[i + 1].StartsWith("/"))
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
                            case DICCommands.CDC2OpcodeFlag:
                                for (int j = 1; j < 4; j++)
                                {
                                    // If the next item is a flag, it's good
                                    if (parts[i + j].StartsWith("/"))
                                    {
                                        i += (j - 1);
                                        break;
                                    }
                                    // If the next item isn't a valid number
                                    else if (!Int32.TryParse(parts[i + j], out int c2))
                                    {
                                        return false;
                                    }
                                    else if (c2 < 0)
                                    {
                                        return false;
                                    }
                                }
                                break;
                            case DICCommands.CDSubchannelReadLevelFlag:
                                // If the next item is a flag, it's good
                                if (parts[i + 1].StartsWith("/"))
                                {
                                    break;
                                }
                                // If the next item isn't a valid number
                                else if (!Int32.TryParse(parts[i + 1], out int sub))
                                {
                                    return false;
                                }
                                else if (sub < 0 || sub > 2)
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
                case DICCommands.DVDCommand:
                    if (!Regex.IsMatch(parts[1], @"[A-Z]:?\\?"))
                    {
                        return false;
                    }
                    else if (parts[2].Trim('\"').StartsWith("/"))
                    {
                        return false;
                    }
                    else if (!Int32.TryParse(parts[3], out int dvdspeed))
                    {
                        return false;
                    }
                    else if (dvdspeed < 0 || dvdspeed > 72) // Officialy, 0-16
                    {
                        return false;
                    }

                    // Loop through all auxilary flags
                    for (int i = 4; i < parts.Count; i++)
                    {
                        switch (parts[i])
                        {
                            case DICCommands.DisableBeepFlag:
                            case DICCommands.DVDCMIFlag:
                            case DICCommands.DVDRawFlag:
                                // No-op, all of these are single flags
                                break;
                            case DICCommands.ForceUnitAccessFlag:
                                // If the next item is a flag, it's good
                                if (parts[i + 1].StartsWith("/"))
                                {
                                    break;
                                }
                                // If the next item isn't a valid number
                                else if (!Int32.TryParse(parts[i + 1], out int fua1))
                                {
                                    return false;
                                }
                                else if (fua1 < 0)
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
                    if (!Regex.IsMatch(parts[1], @"[A-Z]:?\\?"))
                    {
                        return false;
                    }
                    else if (parts[2].Trim('\"').StartsWith("/"))
                    {
                        return false;
                    }

                    // Loop through all auxilary flags
                    for (int i = 3; i < parts.Count; i++)
                    {
                        switch (parts[i])
                        {
                            case DICCommands.DisableBeepFlag:
                                // No-op, this is a single flag
                                break;
                            case DICCommands.ForceUnitAccessFlag:
                                // If the next item is a flag, it's good
                                if (parts[i + 1].StartsWith("/"))
                                {
                                    break;
                                }
                                // If the next item isn't a valid number
                                else if (!Int32.TryParse(parts[i + 1], out int fua1))
                                {
                                    return false;
                                }
                                else if (fua1 < 0)
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
                    if (!Regex.IsMatch(parts[1], @"[A-Z]:?\\?"))
                    {
                        return false;
                    }
                    else if (parts[2].Trim('\"').StartsWith("/"))
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
                    if (!Regex.IsMatch(parts[1], @"[A-Z]:?\\?"))
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
                    if (parts[2].Trim('\"').StartsWith("/"))
                    {
                        return false;
                    }
                    break;
                case DICCommands.GDROMSwapCommand: // TODO: How to validate this?
                default:
                    return false;
            }

            return true;
        }
    }
}
