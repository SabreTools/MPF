using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DICUI
{
    // TODO: Separate into different utility classes based on functionality
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
                    return "cd";
                case DiscType.DVD5:
                    return "dvd";
                case DiscType.DVD9:
                    return "dvd";
                case DiscType.GDROM:
                    return "gd"; // TODO: "swap"?
                case DiscType.HDDVD:
                    return null;
                case DiscType.BD25:
                    return "bd";
                case DiscType.BD50:
                    return "bd";

                // Special Formats
                case DiscType.GameCubeGameDisc:
                    return "dvd";
                case DiscType.UMD:
                    return null;

                // Non-optical
                case DiscType.Floppy:
                    return "fd";

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
                    parameters.Add("/c2 20");

                    switch (sys)
                    {
                        case KnownSystem.AppleMacintosh:
                        case KnownSystem.IBMPCCompatible:
                            parameters.Add("/ns");
                            parameters.Add("/sf");
                            parameters.Add("/ss");
                            break;
                        case KnownSystem.NECPCEngineTurboGrafxCD:
                            parameters.Add("/m");
                            break;
                        case KnownSystem.SonyPlayStation:
                            parameters.Add("/am");
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
                    parameters.Add("/c2 20");
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
                    parameters.Add("/raw");
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
        /// Attempts to find the first track of a dumped disc based on the inputs
        /// </summary>
        /// <param name="outputDirectory">Base directory to use</param>
        /// <param name="outputFilename">Base filename to use</param>
        /// <returns>Proper path to first track, null on error</returns>
        /// <remarks>
        /// By default, this assumes that the outputFilename doesn't contain a proper path, and just a name.
        /// This can lead to a situation where the outputFilename contains a path, but only the filename gets
        /// used in the processing and can lead to a "false null" return
        /// </remarks>
        public static string GetFirstTrack(string outputDirectory, string outputFilename)
        {
            // First, sanitized the output filename to strip off any potential extension
            outputFilename = Path.GetFileNameWithoutExtension(outputFilename);

            // Go through all standard output naming schemes
            string combinedBase = Path.Combine(outputDirectory, outputFilename);
            if (File.Exists(combinedBase + ".bin"))
            {
                return combinedBase + ".bin";
            }
            if (File.Exists(combinedBase + " (Track 1).bin"))
            {
                return combinedBase + " (Track 1).bin";
            }
            if (File.Exists(combinedBase + " (Track 01).bin"))
            {
                return combinedBase + " (Track 01).bin";
            }
            if (File.Exists(combinedBase + ".iso"))
            {
                return Path.Combine(combinedBase + ".iso");
            }

            return null;
        }

        /// <summary>
        /// Ensures that all required output files have been created
        /// </summary>
        /// <param name="outputDirectory">Base directory to use</param>
        /// <param name="outputFilename">Base filename to use</param>
        /// <param name="type">DiscType value to check</param>
        /// <returns></returns>
        public static bool FoundAllFiles(string outputDirectory, string outputFilename, DiscType? type)
        {
            // First, sanitized the output filename to strip off any potential extension
            outputFilename = Path.GetFileNameWithoutExtension(outputFilename);

            // Now ensure that all required files exist
            string combinedBase = Path.Combine(outputDirectory, outputFilename);
            switch(type)
            {
                case DiscType.CD:
                case DiscType.GDROM: // TODO: Verify
                    return File.Exists(combinedBase + ".c2")
                        && File.Exists(combinedBase + ".ccd")
                        && File.Exists(combinedBase + ".cue")
                        && File.Exists(combinedBase + ".dat")
                        && File.Exists(combinedBase + ".img")
                        && File.Exists(combinedBase + ".img_EdcEcc.txt")
                        && File.Exists(combinedBase + ".scm")
                        && File.Exists(combinedBase + ".sub")
                        && File.Exists(combinedBase + "_c2Error.txt")
                        && File.Exists(combinedBase + "_cmd.txt")
                        && File.Exists(combinedBase + "_disc.txt")
                        && File.Exists(combinedBase + "_drive.txt")
                        && File.Exists(combinedBase + "_img.cue")
                        && File.Exists(combinedBase + "_mainError.txt")
                        && File.Exists(combinedBase + "_mainInfo.txt")
                        && File.Exists(combinedBase + "_subError.txt")
                        && File.Exists(combinedBase + "_subInfo.txt")
                        && File.Exists(combinedBase + "_subIntention.txt")
                        && File.Exists(combinedBase + "_subReadable.txt")
                        && File.Exists(combinedBase + "_volDesc.txt");
                case DiscType.DVD5:
                case DiscType.DVD9:
                case DiscType.HDDVD:
                case DiscType.BD25:
                case DiscType.BD50:
                case DiscType.GameCubeGameDisc:
                case DiscType.UMD:
                    return File.Exists(combinedBase + ".dat")
                        && File.Exists(combinedBase + "_cmd.txt")
                        && File.Exists(combinedBase + "_disc.txt")
                        && File.Exists(combinedBase + "_drive.txt")
                        && File.Exists(combinedBase + "_mainError.txt")
                        && File.Exists(combinedBase + "_mainInfo.txt")
                        && File.Exists(combinedBase + "_volDesc.txt");
                case DiscType.Floppy:
                default:
                    return false;
            }
        }

        /// <summary>
        /// Extract all of the possible information from a given input combination
        /// </summary>
        /// <param name="outputDirectory">Base directory to use</param>
        /// <param name="outputFilename">Base filename to use</param>
        /// <param name="sys">KnownSystem value to check</param>
        /// <param name="type">DiscType value to check</param>
        /// <returns>Dictionary containing mapped output values, null on error</returns>
        public static Dictionary<string, string> ExtractOutputInformation(string outputDirectory, string outputFilename, KnownSystem? sys, DiscType? type)
        {
            // First, we want to check that all of the relevant files are there
            if (!FoundAllFiles(outputDirectory, outputFilename, type))
            {
                return null;
            }

            // Create the output dictionary with all user-inputted values by default
            Dictionary<string, string> mappings = new Dictionary<string, string>
            {
                { "Title", "(REQUIRED)" },
                { "Disc Number / Letter", "(OPTIONAL)" },
                { "Disc Title", "(OPTIONAL)" },
                { "Category", "Games" },
                { "Region", "World (CHANGE THIS)" },
                { "Languages", "Klingon (CHANGE THIS)" },
                { "Disc Serial", "(OPTIONAL)" },
                { "Mastering Ring", "" },
                { "Mastering SID Code", "" },
                { "Mould SID Code", "" },
                { "Additional Mould", "" },
                { "Toolstamp or Mastering Code", "" },
                { "Barcode", "" },
                { "ISBN", "" },
                { "Comments", "(OPTIONAL)" },
                { "Contents", "(OPTIONAL)" },
                { "Version", "" },
                { "Edition/Release", "Original (VERIFY THIS)" },
                { "Primary Volume Descriptor (PVD)", "" },
                { "Copy Protection", "(REQUIRED, IF EXISTS)" },
                { "DAT", "" },
            };

            // Now we want to do a check by DiscType and extract all required info
            string combinedBase = Path.Combine(outputDirectory, outputFilename);
            switch (type)
            {
                case DiscType.CD:
                case DiscType.GDROM: // TODO: Verify
                    mappings["Cuesheet"] = "";
                    mappings["Write Offset"] = "";

                    mappings["Primary Volume Descriptor (PVD)"] = GetPVD(combinedBase + "_mainInfo.txt");
                    mappings["Error Count"] = GetErrorCount(combinedBase + ".img_EdcEcc.txt",
                        combinedBase + "_c2Error.txt",
                        combinedBase + "_mainError.txt").ToString();
                    break;
                case DiscType.DVD5:
                case DiscType.HDDVD:
                case DiscType.BD25:
                case DiscType.GameCubeGameDisc:
                case DiscType.UMD:
                    mappings["Primary Volume Descriptor (PVD)"] = GetPVD(combinedBase + "_mainInfo.txt");
                    break;
                case DiscType.DVD9:
                case DiscType.BD50:
                    mappings["Primary Volume Descriptor (PVD)"] = GetPVD(combinedBase + "_mainInfo.txt");
                    mappings["Layerbreak"] = "(REQUIRED)";
                    break;
                case DiscType.Floppy:
                default:
                    // No-op
                    break;
            }

            return mappings;
        }

        /// <summary>
        /// Get the detected error count from the input files, if possible
        /// </summary>
        /// <param name="edcecc">.img_EdcEcc.txt file location</param>
        /// <param name="c2Error">_c2Error.txt file location</param>
        /// <param name="mainError">_mainError.txt file location</param>
        /// <returns>Error count if possible, -1 on error</returns>
        /// <remarks>TODO: Ensure all possible error states are taken care of</remarks>
        private static long GetErrorCount(string edcecc, string c2Error, string mainError)
        {
            // If one of the files doesn't exist, we can't get info from them
            if (!File.Exists(edcecc) || !File.Exists(c2Error) || !File.Exists(mainError))
            {
                return -1;
            }

            // First off, if the mainError file has any contents, we have an uncorrectable error
            if (new FileInfo(mainError).Length > 0)
            {
                return -1;
            }

            // First line of defense is the EdcEcc error file
            using (StreamReader sr = File.OpenText(edcecc))
            {
                try
                {
                    // Fast forward to the PVD
                    string line = sr.ReadLine();
                    while (!line.StartsWith("[NO ERROR]")
                        && !line.StartsWith("[WARNING]")
                        && !line.StartsWith("[ERROR]"))
                    {
                        line = sr.ReadLine();
                    }

                    // Now that we're at the error line, determine what the value should be
                    if (line.StartsWith("[NO ERROR]"))
                    {
                        return 0;
                    }
                    else if (line.StartsWith("[WARNING]"))
                    {
                        // Not sure how to handle these properly
                        return -1;
                    }
                    else if (line.StartsWith("[ERROR] Number of sector(s) where user data doesn't match the expected ECC/EDC:"))
                    {
                        return Int64.Parse(line.Remove(0, 80));
                    }

                    return -1;
                }
                catch
                {
                    // We don't care what the exception is right now
                    return -1;
                }
            }
        }

        /// <summary>
        /// Get the PVD from the input file, if possible
        /// </summary>
        /// <param name="mainInfo">_mainInfo.txt file location</param>
        /// <returns>Newline-deliminated PVD if possible, null on error</returns>
        private static string GetPVD(string mainInfo)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(mainInfo))
            {
                return null;
            }

            using (StreamReader sr = File.OpenText(mainInfo))
            {
                try
                {
                    // Make sure this file is a _mainInfo.txt
                    if (sr.ReadLine() != "========== LBA[000016, 0x00010]: Main Channel ==========")
                    {
                        return null;
                    }

                    // Fast forward to the PVD
                    while (!sr.ReadLine().StartsWith("0310"));

                    // Now that we're at the PVD, read each line in and concatenate
                    string pvd = sr.ReadLine() + "\n"; // 0320
                    pvd += sr.ReadLine() + "\n"; // 0330
                    pvd += sr.ReadLine() + "\n"; // 0340
                    pvd += sr.ReadLine() + "\n"; // 0350
                    pvd += sr.ReadLine() + "\n"; // 0360
                    pvd += sr.ReadLine() + "\n"; // 0370

                    return pvd;
                }
                catch
                {
                    // We don't care what the exception is right now
                    return null;
                }
            }
        }
    }
}
