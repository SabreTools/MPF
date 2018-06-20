using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DICUI.Utilities
{
    public static class DumpInformation
    {
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
            switch (type)
            {
                case DiscType.CD:
                case DiscType.GDROM: // TODO: Verify GD-ROM outputs this
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
                case DiscType.DVD:
                case DiscType.HDDVD:
                case DiscType.BluRay:
                case DiscType.GameCubeGameDisc:
                case DiscType.WiiOpticalDisc:
                case DiscType.WiiUOpticalDisc:
                case DiscType.UMD:
                    return File.Exists(combinedBase + ".dat")
                        && File.Exists(combinedBase + "_cmd.txt")
                        && File.Exists(combinedBase + "_disc.txt")
                        && File.Exists(combinedBase + "_drive.txt")
                        && File.Exists(combinedBase + "_mainError.txt")
                        && File.Exists(combinedBase + "_mainInfo.txt")
                        && File.Exists(combinedBase + "_volDesc.txt");
                case DiscType.Floppy:
                    return File.Exists(combinedBase + ".dat")
                        && File.Exists(combinedBase + "_cmd.txt")
                       && File.Exists(combinedBase + "_disc.txt");
                default:
                    // Non-dumping commands will usually produce no output, so this is irrelevant
                    return true;
            }
        }

        /// <summary>
        /// Extract all of the possible information from a given input combination
        /// </summary>
        /// <param name="outputDirectory">Base directory to use</param>
        /// <param name="outputFilename">Base filename to use</param>
        /// <param name="sys">KnownSystem value to check</param>
        /// <param name="type">DiscType value to check</param>
        /// <param name="driveLetter">Drive letter to check</param>
        /// <returns>Dictionary containing mapped output values, null on error</returns>
        /// <remarks>TODO: Make sure that all special formats are accounted for</remarks>
        public static Dictionary<string, string> ExtractOutputInformation(string outputDirectory, string outputFilename, KnownSystem? sys, DiscType? type, char driveLetter)
        {
            // First, sanitized the output filename to strip off any potential extension
            outputFilename = Path.GetFileNameWithoutExtension(outputFilename);

            // First, we want to check that all of the relevant files are there
            if (!FoundAllFiles(outputDirectory, outputFilename, type))
            {
                return null;
            }

            // Create the output dictionary with all user-inputted values by default
            string combinedBase = Path.Combine(outputDirectory, outputFilename);
            Dictionary<string, string> mappings = new Dictionary<string, string>
            {
                { Template.TitleField, Template.RequiredValue },
                { Template.DiscNumberField, Template.OptionalValue },
                { Template.DiscTitleField, Template.OptionalValue },
                { Template.CategoryField, "Games" },
                { Template.RegionField, "World (CHANGE THIS)" },
                { Template.LanguagesField, "Klingon (CHANGE THIS)" },
                { Template.DiscSerialField, Template.RequiredIfExistsValue },
                { Template.BarcodeField, Template.OptionalValue},
                { Template.CommentsField, Template.OptionalValue },
                { Template.ContentsField, Template.OptionalValue },
                { Template.VersionField, Template.RequiredIfExistsValue },
                { Template.EditionField, "Original (VERIFY THIS)" },
                { Template.DATField, GetDatfile(combinedBase + ".dat") },
            };

            // Now we want to do a check by DiscType and extract all required info
            switch (type)
            {
                case DiscType.CD:
                case DiscType.GDROM: // TODO: Verify GD-ROM outputs this
                    mappings[Template.MasteringRingField] = Template.RequiredIfExistsValue;
                    mappings[Template.MasteringSIDField] = Template.RequiredIfExistsValue;
                    mappings[Template.MouldSIDField] = Template.RequiredIfExistsValue;
                    mappings[Template.AdditionalMouldField] = Template.RequiredIfExistsValue;
                    mappings[Template.ToolstampField] = Template.RequiredIfExistsValue;
                    mappings[Template.PVDField] = GetPVD(combinedBase + "_mainInfo.txt");
                    mappings[Template.ErrorCountField] = GetErrorCount(combinedBase + ".img_EdcEcc.txt",
                        combinedBase + "_c2Error.txt",
                        combinedBase + "_mainError.txt").ToString();
                    mappings[Template.CuesheetField] = GetFullFile(combinedBase + ".cue");
                    mappings[Template.WriteOffsetField] = GetWriteOffset(combinedBase + "_disc.txt");

                    // System-specific options
                    switch (sys)
                    {
                        case KnownSystem.AppleMacintosh:
                        case KnownSystem.IBMPCCompatible:
                            mappings[Template.ISBNField] = Template.OptionalValue;
                            mappings[Template.CopyProtectionField] = Template.RequiredIfExistsValue;
                            if (File.Exists(combinedBase + "_subIntention.txt"))
                            {
                                FileInfo fi = new FileInfo(combinedBase + "_subIntention.txt");
                                if (fi.Length > 0)
                                {
                                    mappings[Template.SubIntentionField] = GetFullFile(combinedBase + "_subIntention.txt");
                                }
                            }
                            break;
                        case KnownSystem.SegaSaturn:
                            mappings[Template.SaturnHeaderField] = GetSaturnHeader(GetFirstTrack(outputDirectory, outputFilename)).ToString();
                            if (GetSaturnBuildInfo(mappings[Template.SaturnHeaderField], out string serial, out string version, out string buildDate))
                            {
                                mappings[Template.DiscSerialField] = serial;
                                mappings[Template.VersionField] = version;
                                mappings[Template.SaturnBuildDateField] = buildDate;
                            }
                            break;
                        case KnownSystem.SonyPlayStation:
                            mappings[Template.PlaystationEXEDateField] = GetPlayStationEXEDate(driveLetter);
                            mappings[Template.PlayStationEDCField] = Template.YesNoValue;
                            mappings[Template.PlayStationAntiModchipField] = Template.YesNoValue;
                            mappings[Template.PlayStationLibCryptField] = Template.YesNoValue;
                            break;
                        case KnownSystem.SonyPlayStation2:
                            mappings[Template.PlaystationEXEDateField] = GetPlayStationEXEDate(driveLetter);
                            mappings[Template.VersionField] = GetPlayStation2Version(driveLetter);
                            break;
                    }

                    break;
                case DiscType.DVD:
                case DiscType.HDDVD:
                case DiscType.BluRay:
                    string layerbreak = GetLayerbreak(combinedBase + "_disc.txt");
                    
                    // If we have a single-layer disc
                    if (layerbreak == null)
                    {
                        mappings[Template.MasteringRingField] = Template.RequiredIfExistsValue;
                        mappings[Template.MasteringSIDField] = Template.RequiredIfExistsValue;
                        mappings[Template.MouldSIDField] = Template.RequiredIfExistsValue;
                        mappings[Template.AdditionalMouldField] = Template.RequiredIfExistsValue;
                        mappings[Template.ToolstampField] = Template.RequiredIfExistsValue;
                        mappings[Template.PVDField] = GetPVD(combinedBase + "_mainInfo.txt");
                    }
                    // If we have a dual-layer disc
                    else
                    {
                        mappings["Outer " + Template.MasteringRingField] = Template.RequiredIfExistsValue;
                        mappings["Inner " + Template.MasteringRingField] = Template.RequiredIfExistsValue;
                        mappings["Outer " + Template.MasteringSIDField] = Template.RequiredIfExistsValue;
                        mappings["Inner " + Template.MasteringSIDField] = Template.RequiredIfExistsValue;
                        mappings[Template.MouldSIDField] = Template.RequiredIfExistsValue;
                        mappings[Template.AdditionalMouldField] = Template.RequiredIfExistsValue;
                        mappings["Outer " + Template.ToolstampField] = Template.RequiredIfExistsValue;
                        mappings["Inner " + Template.ToolstampField] = Template.RequiredIfExistsValue;
                        mappings[Template.PVDField] = GetPVD(combinedBase + "_mainInfo.txt");
                        mappings[Template.LayerbreakField] = layerbreak;
                    }

                    // System-specific options
                    switch (sys)
                    {
                        case KnownSystem.AppleMacintosh:
                        case KnownSystem.IBMPCCompatible:
                            mappings[Template.ISBNField] = Template.OptionalValue;
                            mappings[Template.CopyProtectionField] = Template.RequiredIfExistsValue;
                            if (File.Exists(combinedBase + "_subIntention.txt"))
                            {
                                FileInfo fi = new FileInfo(combinedBase + "_subIntention.txt");
                                if (fi.Length > 0)
                                {
                                    mappings[Template.SubIntentionField] = GetFullFile(combinedBase + "_subIntention.txt");
                                }
                            }
                            break;
                        case KnownSystem.MicrosoftXBOX:
                        case KnownSystem.MicrosoftXBOX360XDG2:
                        case KnownSystem.MicrosoftXBOX360XDG3:
                            if (GetXBOXAuxInfo(combinedBase + "_disc.txt", out string dmihash, out string pfihash, out string sshash, out string ss))
                            {
                                mappings[Template.XBOXDMIHash] = dmihash;
                                mappings[Template.XBOXPFIHash] = pfihash;
                                mappings[Template.XBOXSSHash] = sshash;
                                mappings[Template.XBOXSSRanges] = ss;
                            }
                            break;
                        case KnownSystem.SonyPlayStation2:
                            mappings[Template.PlaystationEXEDateField] = GetPlayStationEXEDate(driveLetter);
                            mappings[Template.VersionField] = GetPlayStation2Version(driveLetter);
                            break;
                    }
                    break;
            }

            return mappings;
        }

        /// <summary>
        /// Get the full lines from the input file, if possible
        /// </summary>
        /// <param name="filename">file location</param>
        /// <returns>Full text of the file, null on error</returns>
        private static string GetFullFile(string filename)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(filename))
            {
                return null;
            }

            return string.Join("\n", File.ReadAllLines(filename));
        }

        /// <summary>
        /// Get the proper datfile from the input file, if possible
        /// </summary>
        /// <param name="dat">.dat file location</param>
        /// <returns>Relevant pieces of the datfile, null on error</returns>
        private static string GetDatfile(string dat)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(dat))
            {
                return null;
            }

            using (StreamReader sr = File.OpenText(dat))
            {
                try
                {
                    // Make sure this file is a .dat
                    if (sr.ReadLine() != "<?xml version=\"1.0\" encoding=\"UTF-8\"?>")
                    {
                        return null;
                    }
                    if (sr.ReadLine() != "<!DOCTYPE datafile PUBLIC \"-//Logiqx//DTD ROM Management Datafile//EN\" \"http://www.logiqx.com/Dats/datafile.dtd\">")
                    {
                        return null;
                    }

                    // Fast forward to the rom lines
                    while (!sr.ReadLine().TrimStart().StartsWith("<game")) ;
                    sr.ReadLine(); // <category>Games</category>
                    sr.ReadLine(); // <description>Plextor</description>

                    // Now that we're at the relevant entries, read each line in and concatenate
                    string pvd = "", line = sr.ReadLine().Trim();
                    while (line.StartsWith("<rom"))
                    {
                        pvd += line + "\n";
                        line = sr.ReadLine().Trim();
                    }

                    return pvd.TrimEnd('\n');
                }
                catch
                {
                    // We don't care what the exception is right now
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the detected error count from the input files, if possible
        /// </summary>
        /// <param name="edcecc">.img_EdcEcc.txt file location</param>
        /// <param name="c2Error">_c2Error.txt file location</param>
        /// <param name="mainError">_mainError.txt file location</param>
        /// <returns>Error count if possible, -1 on error</returns>
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
                        && !line.StartsWith("Total errors:"))
                    {
                        line = sr.ReadLine();
                    }

                    // Now that we're at the error line, determine what the value should be
                    if (line.StartsWith("[NO ERROR]"))
                    {
                        return 0;
                    }
                    else if (line.StartsWith("Total errors:"))
                    {
                        return Int64.Parse(line.Remove(0, 14));
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
        /// Get the layerbreak from the input file, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <returns>Layerbreak if possible, null on error</returns>
        private static string GetLayerbreak(string disc)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
            {
                return null;
            }

            using (StreamReader sr = File.OpenText(disc))
            {
                try
                {
                    // Make sure this file is a _disc.txt
                    if (sr.ReadLine() != "========== DiscStructure ==========")
                    {
                        return null;
                    }

                    // Fast forward to the layerbreak
                    while (!sr.ReadLine().Trim().StartsWith("EndDataSector")) ;

                    // Now that we're at the layerbreak line, attempt to get the decimal version
                    return sr.ReadLine().Split(' ')[1];
                }
                catch
                {
                    // We don't care what the exception is right now
                    return null;
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
                    while (!sr.ReadLine().StartsWith("0310")) ;

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

        /// <summary>
        /// Get the EXE date from a PlayStation disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>EXE date in "yyyy-mm-dd" format if possible, null on error</returns>
        private static string GetPlayStationEXEDate(char driveLetter)
        {
            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            if (!Directory.Exists(drivePath))
            {
                return null;
            }

            // If we can't find SYSTEM.CNF, we don't have a PlayStation disc
            string systemCnfPath = Path.Combine(drivePath, "SYSTEM.CNF");
            if (!File.Exists(systemCnfPath))
            {
                return null;
            }

            // Let's try reading SYSTEM.CNF to find the "BOOT" value
            string exeName = null;
            try
            {
                using (StreamReader sr = File.OpenText(systemCnfPath))
                {
                    // Not assuming proper ordering, just in case
                    string line = sr.ReadLine();
                    while (!line.StartsWith("BOOT"))
                    {
                        line = sr.ReadLine();
                    }

                    // Once it finds the "BOOT" line, extract the name
                    exeName = Regex.Match(line, @"BOOT.? = cdrom.?:\\(.*?);.*").Groups[1].Value;
                }
            }
            catch
            {
                // We don't care what the error was
                return null;
            }

            // Now that we have the EXE name, try to get the fileinfo for it
            string exePath = Path.Combine(drivePath, exeName);
            if (!File.Exists(exePath))
            {
                return null;
            }

            FileInfo fi = new FileInfo(exePath);
            return fi.LastWriteTimeUtc.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// Get the version from a PlayStation 2 disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>Game version if possible, null on error</returns>
        private static string GetPlayStation2Version(char driveLetter)
        {
            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            if (!Directory.Exists(drivePath))
            {
                return null;
            }

            // If we can't find SYSTEM.CNF, we don't have a PlayStation disc
            string systemCnfPath = Path.Combine(drivePath, "SYSTEM.CNF");
            if (!File.Exists(systemCnfPath))
            {
                return null;
            }

            // Let's try reading SYSTEM.CNF to find the "VER" value
            try
            {
                using (StreamReader sr = File.OpenText(systemCnfPath))
                {
                    // Not assuming proper ordering, just in case
                    string line = sr.ReadLine();
                    while (!line.StartsWith("VER"))
                    {
                        line = sr.ReadLine();
                    }

                    // Once it finds the "VER" line, extract the version
                    return Regex.Match(line, @"VER = (.*)").Groups[1].Value;
                }
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
        }

        /// <summary>
        /// Get the header from a Saturn disc, if possible
        /// </summary>
        /// <param name="firstTrackPath">Path to the first track to check</param>
        /// <returns>Header as a byte array if possible, null on error</returns>
        private static string GetSaturnHeader(string firstTrackPath)
        {
            // If the file doesn't exist, we can't get the header
            if (!File.Exists(firstTrackPath))
            {
                return null;
            }

            // Try to open the file and read the correct number of bytes
            try
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(firstTrackPath)))
                {
                    br.ReadBytes(0x10);
                    byte[] headerBytes = br.ReadBytes(0x100);

                    // Now format the bytes in a way we like
                    string headerString = "";
                    int ptr = 0;
                    while (ptr < headerBytes.Length)
                    {
                        byte[] sub = new byte[16];
                        Array.Copy(headerBytes, ptr, sub, 0, 16);
                        headerString += ptr.ToString("X").PadLeft(4, '0') + " : "
                            + BitConverter.ToString(sub).Replace("-", " ") + "   "
                            + Encoding.ASCII.GetString(sub) + "\n";
                        ptr += 16;
                    }

                    return headerString.TrimEnd('\n');
                }
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
        }

        /// <summary>
        /// Get the build info from a Saturn disc, if possible
        /// </summary>
        /// <<param name="saturnHeader">String representing a formatter variant of the Saturn header</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private static bool GetSaturnBuildInfo(string saturnHeader, out string serial, out string version, out string date)
        {
            serial = null; version = null; date = null;

            // If the input header is null, we can't do a thing
            if (String.IsNullOrWhiteSpace(saturnHeader))
            {
                return false;
            }

            // Now read it in cutting it into lines for easier parsing
            try
            {
                string[] header = saturnHeader.Split('\n');
                string serialVersionLine = header[2].Substring(57);
                string dateLine = header[3].Substring(57);
                serial = serialVersionLine.Substring(0, 8);
                version = serialVersionLine.Substring(10, 6);
                date = dateLine.Substring(0, 8);
                return true;
            }
            catch
            {
                // We don't care what the error is
                return false;
            }
        }

        /// <summary>
        /// Get the XBOX/360 auxiliary info from the outputted files, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private static bool GetXBOXAuxInfo(string disc, out string dmihash, out string pfihash, out string sshash, out string ss)
        {
            dmihash = null; pfihash = null; sshash = null; ss = null;

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
            {
                return false;
            }

            using (StreamReader sr = File.OpenText(disc))
            {
                try
                {
                    // Make sure this file is a _disc.txt for XBOX
                    if (sr.ReadLine() != "========== Lock state ==========")
                    {
                        return false;
                    }

                    // Fast forward to the Security Sector Ranges
                    while (!sr.ReadLine().Trim().StartsWith("Number of security sector ranges:")) ;

                    // Now that we're at the ranges, read each line in and concatenate
                    // TODO: Make this output like the old method (startlba-endlba)
                    string line = sr.ReadLine();
                    while (!line.Trim().StartsWith("========== Unlock 2 state(wxripper) =========="))
                    {
                        ss += line + "\n";
                        line = sr.ReadLine();
                    }

                    // Fast forward to the aux hashes
                    while (!line.Trim().StartsWith("<rom"))
                    {
                        line = sr.ReadLine();
                    }

                    // Read in the hashes to the proper parts
                    sshash = line.Trim();
                    pfihash = sr.ReadLine().Trim();
                    dmihash = sr.ReadLine().Trim();

                    return true;
                }
                catch
                {
                    // We don't care what the exception is right now
                    return false;
                }
            }
        }

        /// <summary>
        /// Get the write offset from the input file, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <returns>Sample write offset if possible, null on error</returns>
        private static string GetWriteOffset(string disc)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
            {
                return null;
            }

            using (StreamReader sr = File.OpenText(disc))
            {
                try
                {
                    // Make sure this file is a _disc.txt
                    if (sr.ReadLine() != "========== TOC ==========")
                    {
                        return null;
                    }

                    // Fast forward to the offsets
                    while (!sr.ReadLine().Trim().StartsWith("========== Offset")) ;
                    sr.ReadLine(); // Combined Offset
                    sr.ReadLine(); // Drive Offset
                    sr.ReadLine(); // Separator line

                    // Now that we're at the offsets, attempt to get the sample offset
                    return sr.ReadLine().Split(' ').LastOrDefault();
                }
                catch
                {
                    // We don't care what the exception is right now
                    return null;
                }
            }
        }

        /// <summary>
        /// Format the output data in a human readable way, separating each printed line into a new item in the list
        /// </summary>
        /// <param name="info">Information dictionary that should contain normalized values</param>
        /// <param name="sys">KnownSystem value to check</param>
        /// <param name="type">DiscType value to check</param>
        /// <returns>List of strings representing each line of an output file, null on error</returns>
        /// <remarks>TODO: Get full list of customizable stuff for other systems</remarks>
        public static List<string> FormatOutputData(Dictionary<string, string> info, KnownSystem? sys, DiscType? type)
        {
            // Check to see if the inputs are valid
            if (info == null)
            {
                return null;
            }

            try
            {
                List<string> output = new List<string>();

                output.Add(Template.TitleField + ": " + info[Template.TitleField]);
                output.Add(Template.DiscNumberField + ": " + info[Template.DiscNumberField]);
                output.Add(Template.DiscTitleField + ": " + info[Template.DiscTitleField]);
                output.Add(Template.CategoryField + ": " + info[Template.CategoryField]);
                output.Add(Template.RegionField + ": " + info[Template.RegionField]);
                output.Add(Template.LanguagesField + ": " + info[Template.LanguagesField]);
                output.Add(Template.DiscSerialField + ": " + info[Template.DiscSerialField]);
                switch (sys)
                {
                    case KnownSystem.SegaSaturn:
                        output.Add(Template.SaturnBuildDateField + ": " + info[Template.SaturnBuildDateField]);
                        break;
                    case KnownSystem.SonyPlayStation:
                    case KnownSystem.SonyPlayStation2:
                        output.Add(Template.PlaystationEXEDateField + ": " + info[Template.PlaystationEXEDateField]);
                        break;
                }
                output.Add("Ringcode Information:");
                switch (type)
                {
                    case DiscType.CD:
                    case DiscType.GDROM:
                    case DiscType.DVD:
                    case DiscType.HDDVD:
                    case DiscType.BluRay:
                        // If we have a dual-layer disc
                        if (info.ContainsKey(Template.LayerbreakField))
                        {
                            output.Add("\tOuter " + Template.MasteringRingField + ": " + info["Outer " + Template.MasteringRingField]);
                            output.Add("\tInner " + Template.MasteringRingField + ": " + info["Inner " + Template.MasteringRingField]);
                            output.Add("\tOuter " + Template.MasteringSIDField + ": " + info["Outer " + Template.MasteringSIDField]);
                            output.Add("\tInner " + Template.MasteringSIDField + ": " + info["Inner " + Template.MasteringSIDField]);
                            output.Add("\t" + Template.MouldSIDField + ": " + info[Template.MouldSIDField]);
                            output.Add("\t" + Template.AdditionalMouldField + ": " + info[Template.AdditionalMouldField]);
                            output.Add("\tOuter " + Template.ToolstampField + ": " + info["Outer " + Template.ToolstampField]);
                            output.Add("\tInner " + Template.ToolstampField + ": " + info["Inner " + Template.ToolstampField]);
                        }
                        // If we have a single-layer disc
                        else
                        {
                            output.Add("\t" + Template.MasteringRingField + ": " + info[Template.MasteringRingField]);
                            output.Add("\t" + Template.MasteringSIDField + ": " + info[Template.MasteringSIDField]);
                            output.Add("\t" + Template.MouldSIDField + ": " + info[Template.MouldSIDField]);
                            output.Add("\t" + Template.AdditionalMouldField + ": " + info[Template.AdditionalMouldField]);
                            output.Add("\t" + Template.ToolstampField + ": " + info[Template.ToolstampField]);
                        }
                        break;
                }
                output.Add(Template.BarcodeField + ": " + info[Template.BarcodeField]);
                switch (sys)
                {
                    case KnownSystem.AppleMacintosh:
                    case KnownSystem.IBMPCCompatible:
                        output.Add(Template.ISBNField + ": " + info[Template.ISBNField]);
                        break;
                }
                switch (type)
                {
                    case DiscType.CD:
                    case DiscType.GDROM:
                        output.Add(Template.ErrorCountField + ": " + info[Template.ErrorCountField]);
                        break;
                }
                output.Add(Template.CommentsField + ": " + info[Template.CommentsField]);
                output.Add(Template.ContentsField + ": " + info[Template.ContentsField]);
                output.Add(Template.VersionField + ": " + info[Template.VersionField]);
                output.Add(Template.EditionField + ": " + info[Template.EditionField]);
                switch (sys)
                {
                    case KnownSystem.SegaSaturn:
                        output.Add(Template.SaturnHeaderField + ":"); output.Add("");
                        output.AddRange(info[Template.SaturnHeaderField].Split('\n')); output.Add("");
                        break;
                    case KnownSystem.SonyPlayStation:
                        output.Add(Template.PlayStationEDCField + ": " + info[Template.PlayStationEDCField]);
                        output.Add(Template.PlayStationAntiModchipField + ": " + info[Template.PlayStationAntiModchipField]);
                        output.Add(Template.PlayStationLibCryptField + ": " + info[Template.PlayStationLibCryptField]);
                        break;
                }
                switch (type)
                {
                    case DiscType.DVD:
                    case DiscType.BluRay:
                        // If we have a dual-layer disc
                        if (info.ContainsKey(Template.LayerbreakField))
                        {
                            output.Add(Template.LayerbreakField + ": " + info[Template.LayerbreakField]);
                        }
                        break;
                }
                output.Add(Template.PVDField + ":"); output.Add("");
                output.AddRange(info[Template.PVDField].Split('\n'));
                switch (sys)
                {
                    case KnownSystem.AppleMacintosh:
                    case KnownSystem.IBMPCCompatible:
                        output.Add(Template.CopyProtectionField + ": " + info[Template.CopyProtectionField]); output.Add("");

                        if (info.ContainsKey(Template.SubIntentionField))
                        {
                            output.Add(Template.SubIntentionField + ":"); output.Add("");
                            output.AddRange(info[Template.SubIntentionField].Split('\n'));
                        }
                        break;
                    case KnownSystem.MicrosoftXBOX:
                    case KnownSystem.MicrosoftXBOX360XDG2:
                    case KnownSystem.MicrosoftXBOX360XDG3:
                        output.Add(Template.XBOXDMIHash + ": " + info[Template.XBOXDMIHash]);
                        output.Add(Template.XBOXPFIHash + ": " + info[Template.XBOXPFIHash]);
                        output.Add(Template.XBOXSSHash + ": " + info[Template.XBOXSSHash]); output.Add("");
                        output.Add(Template.XBOXSSRanges + ":"); output.Add("");
                        output.AddRange(info[Template.XBOXSSRanges].Split('\n'));
                        break;
                }
                switch (type)
                {
                    case DiscType.CD:
                    case DiscType.GDROM:
                        output.Add(Template.CuesheetField + ":"); output.Add("");
                        output.AddRange(info[Template.CuesheetField].Split('\n')); output.Add("");
                        output.Add(Template.WriteOffsetField + ": " + info[Template.WriteOffsetField]); output.Add("");
                        break;
                }
                output.Add(Template.DATField + ":"); output.Add("");
                output.AddRange(info[Template.DATField].Split('\n'));

                return output;
            }
            catch
            {
                // We don't care what the error is
                return null;
            }
        }

        /// <summary>
        /// Write the data to the output folder
        /// </summary>
        /// <param name="outputDirectory">Base directory to use</param>
        /// <param name="outputFilename">Base filename to use</param>
        /// <param name="lines">Preformatted list of lines to write out to the file</param>
        /// <returns>True on success, false on error</returns>
        public static bool WriteOutputData(string outputDirectory, string outputFilename, List<string> lines)
        {
            // Check to see if the inputs are valid
            if (lines == null)
            {
                return false;
            }

            // Then, sanitized the output filename to strip off any potential extension
            outputFilename = Path.GetFileNameWithoutExtension(outputFilename);

            // Now write out to a generic file
            try
            {
                using (StreamWriter sw = new StreamWriter(File.Open(Path.Combine(outputDirectory, "!submissionInfo.txt"), FileMode.Create, FileAccess.Write)))
                {
                    foreach (string line in lines)
                    {
                        sw.WriteLine(line);
                    }
                }
            }
            catch
            {
                // We don't care what the error is right now
                return false;
            }

            return true;
        }
    }
}