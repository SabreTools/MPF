using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MPF.Core.Data;
using MPF.Core.Modules;
using MPF.Core.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SabreTools.Models.PIC;
using SabreTools.RedumpLib.Data;
using Formatting = Newtonsoft.Json.Formatting;

#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

namespace MPF.Core
{
    public static class InfoTool
    {
        #region Information Extraction

        /// <summary>
        /// Ensures that all required output files have been created
        /// </summary>
        /// <param name="outputDirectory">Output folder to write to</param>
        /// <param name="outputFilename">Output filename to use as the base path</param>
        /// <param name="parameters">Parameters object representing what to send to the internal program</param>
        /// <param name="preCheck">True if this is a check done before a dump, false if done after</param>
        /// <returns>Tuple of true if all required files exist, false otherwise and a list representing missing files</returns>
        internal static (bool, List<string>) FoundAllFiles(string? outputDirectory, string outputFilename, BaseParameters? parameters, bool preCheck)
        {
            // If there are no parameters set
            if (parameters == null)
                return (false, new List<string>());

            // First, sanitized the output filename to strip off any potential extension
            outputFilename = Path.GetFileNameWithoutExtension(outputFilename);

            // Then get the base path for all checking
            string basePath;
            if (string.IsNullOrWhiteSpace(outputDirectory))
                basePath = outputFilename;
            else
                basePath = Path.Combine(outputDirectory, outputFilename);

            // Finally, let the parameters say if all files exist
            return parameters.CheckAllOutputFilesExist(basePath, preCheck);
        }

        /// <summary>
        /// Generate the proper datfile from the input Datafile, if possible
        /// </summary>
        /// <param name="datafile">.dat file location</param>
        /// <returns>Relevant pieces of the datfile, null on error</returns>
        internal static string? GenerateDatfile(Datafile? datafile)
        {
            // If we don't have a valid datafile, we can't do anything
            if (datafile?.Games == null || datafile.Games.Length == 0)
                return null;

            var roms = datafile.Games[0].Roms;
            if (roms == null || roms.Length == 0)
                return null;

            // Otherwise, reconstruct the hash data with only the required info
            try
            {
                string datString = string.Empty;
                for (int i = 0; i < roms.Length; i++)
                {
                    var rom = roms[i];
                    datString += $"<rom name=\"{rom.Name}\" size=\"{rom.Size}\" crc=\"{rom.Crc}\" md5=\"{rom.Md5}\" sha1=\"{rom.Sha1}\" />\n";
                }

                return datString.TrimEnd('\n');
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get the existence of an anti-modchip string from a PlayStation disc, if possible
        /// </summary>
        /// <param name="drive">Drive object representing the current drive</param>
        /// <returns>Anti-modchip existence if possible, false on error</returns>
#if NET40
        internal static bool GetAntiModchipDetected(Drive drive)
        {
            var protectionTask = Protection.GetPlayStationAntiModchipDetected(drive.Name);
            protectionTask.Wait();
            return protectionTask.Result;
        }
#else
        internal static async Task<bool> GetAntiModchipDetected(Drive drive) =>
            await Protection.GetPlayStationAntiModchipDetected(drive.Name);
#endif

        /// <summary>
        /// Get the current detected copy protection(s), if possible
        /// </summary>
        /// <param name="drive">Drive object representing the current drive</param>
        /// <param name="options">Options object that determines what to scan</param>
        /// <param name="progress">Optional progress callback</param>
        /// <returns>Detected copy protection(s) if possible, null on error</returns>
#if NET40
        internal static (string?, Dictionary<string, List<string>>?) GetCopyProtection(Drive? drive, Data.Options options, IProgress<BinaryObjectScanner.ProtectionProgress>? progress = null)
#else
        internal static async Task<(string?, Dictionary<string, List<string>>?)> GetCopyProtection(Drive? drive, Data.Options options, IProgress<BinaryObjectScanner.ProtectionProgress>? progress = null)
#endif
        {
            if (options.ScanForProtection && drive?.Name != null)
            {
#if NET40
                var protectionTask = Protection.RunProtectionScanOnPath(drive.Name, options, progress);
                protectionTask.Wait();
                (var protection, _) = protectionTask.Result;
#else
                (var protection, _) = await Protection.RunProtectionScanOnPath(drive.Name, options, progress);
#endif
                return (Protection.FormatProtections(protection), protection);
            }

            return ("(CHECK WITH PROTECTIONID)", null);
        }

        /// <summary>
        /// Get Datafile from a standard DAT
        /// </summary>
        /// <param name="dat">Path to the DAT file to parse</param>
        /// <returns>Filled Datafile on success, null on error</returns>
        internal static Datafile? GetDatafile(string? dat)
        {
            // If there's no path, we can't read the file
            if (string.IsNullOrWhiteSpace(dat))
                return null;

            // If the file doesn't exist, we can't read it
            if (!File.Exists(dat))
                return null;

            try
            {
                // Open and read in the XML file
                XmlReader xtr = XmlReader.Create(dat, new XmlReaderSettings
                {
                    CheckCharacters = false,
                    DtdProcessing = DtdProcessing.Ignore,
                    IgnoreComments = true,
                    IgnoreWhitespace = true,
                    ValidationFlags = XmlSchemaValidationFlags.None,
                    ValidationType = ValidationType.None,
                });

                // If the reader is null for some reason, we can't do anything
                if (xtr == null)
                    return null;

                var serializer = new XmlSerializer(typeof(Datafile));
                return serializer.Deserialize(xtr) as Datafile;
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Gets disc information from a PIC file
        /// </summary>
        /// <param name="pic">Path to a PIC.bin file</param>
        /// <returns>Filled DiscInformation on success, null on error</returns>
        /// <remarks>This omits the emergency brake information, if it exists</remarks>
        internal static DiscInformation? GetDiscInformation(string pic)
        {
            try
            {
                return new SabreTools.Serialization.Files.PIC().Deserialize(pic);
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
        }

        /// <summary>
        /// Get the last modified date from a file path, if possible
        /// </summary>
        /// <param name="filename">Path to the input file</param>
        /// <returns>Filled DateTime on success, null on failure</returns>
        internal static DateTime? GetFileModifiedDate(string? filename, bool fallback = false)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return fallback ? (DateTime?)DateTime.UtcNow : null;
            else if (!File.Exists(filename))
                return fallback ? (DateTime?)DateTime.UtcNow : null;

            var fi = new FileInfo(filename);
            return fi.LastWriteTimeUtc;
        }

        /// <summary>
        /// Get the full lines from the input file, if possible
        /// </summary>
        /// <param name="filename">file location</param>
        /// <param name="binary">True if should read as binary, false otherwise (default)</param>
        /// <returns>Full text of the file, null on error</returns>
        internal static string? GetFullFile(string filename, bool binary = false)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(filename))
                return null;

            // If we're reading as binary
            if (binary)
            {
                byte[] bytes = File.ReadAllBytes(filename);
                return BitConverter.ToString(bytes).Replace("-", string.Empty);
            }

            return File.ReadAllText(filename);
        }

        /// <summary>
        /// Get the split values for ISO-based media
        /// </summary>
        /// <param name="datafile">Datafile represenging the hash data</param>
        /// <returns>True if extraction was successful, false otherwise</returns>
        internal static bool GetISOHashValues(Datafile? datafile, out long size, out string? crc32, out string? md5, out string? sha1)
        {
            size = -1; crc32 = null; md5 = null; sha1 = null;

            if (datafile?.Games == null || datafile.Games.Length == 0)
                return false;

            var roms = datafile.Games[0].Roms;
            if (roms == null || roms.Length == 0)
                return false;

            var rom = roms[0];

            _ = Int64.TryParse(rom.Size, out size);
            crc32 = rom.Crc;
            md5 = rom.Md5;
            sha1 = rom.Sha1;

            return true;
        }

        /// <summary>
        /// Get the split values for ISO-based media
        /// </summary>
        /// <param name="hashData">String representing the combined hash data</param>
        /// <returns>True if extraction was successful, false otherwise</returns>
        internal static bool GetISOHashValues(string? hashData, out long size, out string? crc32, out string? md5, out string? sha1)
        {
            size = -1; crc32 = null; md5 = null; sha1 = null;

            if (string.IsNullOrWhiteSpace(hashData))
                return false;

            var hashreg = new Regex(@"<rom name="".*?"" size=""(.*?)"" crc=""(.*?)"" md5=""(.*?)"" sha1=""(.*?)""", RegexOptions.Compiled);
            Match m = hashreg.Match(hashData);
            if (m.Success)
            {
                _ = Int64.TryParse(m.Groups[1].Value, out size);
                crc32 = m.Groups[2].Value;
                md5 = m.Groups[3].Value;
                sha1 = m.Groups[4].Value;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get the layerbreak info associated from the disc information
        /// </summary>
        /// <param name="di">Disc information containing unformatted data</param>
        /// <returns>True if layerbreak info was set, false otherwise</returns>
        internal static bool GetLayerbreaks(DiscInformation? di, out long? layerbreak1, out long? layerbreak2, out long? layerbreak3)
        {
            // Set the default values
            layerbreak1 = null; layerbreak2 = null; layerbreak3 = null;

            // If we don't have valid disc information, we can't do anything
            if (di?.Units == null || di.Units.Length <= 1)
                return false;

            static int ReadFromArrayBigEndian(byte[]? bytes, int offset)
            {
                if (bytes == null)
                    return default;

#if NET40
                byte[] rev = new byte[0x04];
                Array.Copy(bytes, offset, rev, 0, 0x04);
#else
                var span = new ReadOnlySpan<byte>(bytes, offset, 0x04);
                byte[] rev = span.ToArray();
#endif
                Array.Reverse(rev);
                return BitConverter.ToInt32(rev, 0);
            }

            // Layerbreak 1 (2+ layers)
            if (di.Units.Length >= 2)
            {
                long offset = ReadFromArrayBigEndian(di.Units[0]?.Body?.FormatDependentContents, 0x0C);
                long value = ReadFromArrayBigEndian(di.Units[0]?.Body?.FormatDependentContents, 0x10);
                layerbreak1 = value - offset + 2;
            }

            // Layerbreak 2 (3+ layers)
            if (di.Units.Length >= 3)
            {
                long offset = ReadFromArrayBigEndian(di.Units[1]?.Body?.FormatDependentContents, 0x0C);
                long value = ReadFromArrayBigEndian(di.Units[1]?.Body?.FormatDependentContents, 0x10);
                layerbreak2 = layerbreak1 + value - offset + 2;
            }

            // Layerbreak 3 (4 layers)
            if (di.Units.Length >= 4)
            {
                long offset = ReadFromArrayBigEndian(di.Units[2]?.Body?.FormatDependentContents, 0x0C);
                long value = ReadFromArrayBigEndian(di.Units[2]?.Body?.FormatDependentContents, 0x10);
                layerbreak3 = layerbreak2 + value - offset + 2;
            }

            return true;
        }

        /// <summary>
        /// Get if LibCrypt data is detected in the subchannel file, if possible
        /// </summary>
        /// <param name="info">Base submission info to fill in specifics for</param>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <returns>Status of the LibCrypt data, if possible</returns>
        internal static void GetLibCryptDetected(SubmissionInfo info, string basePath)
        {
            info.CopyProtection ??= new CopyProtectionSection();

            bool? psLibCryptStatus = Protection.GetLibCryptDetected(basePath + ".sub");
            if (psLibCryptStatus == true)
            {
                // Guard against false positives
                if (File.Exists(basePath + "_subIntention.txt"))
                {
                    string libCryptData = GetFullFile(basePath + "_subIntention.txt") ?? "";
                    if (string.IsNullOrEmpty(libCryptData))
                    {
                        info.CopyProtection.LibCrypt = YesNo.No;
                    }
                    else
                    {
                        info.CopyProtection.LibCrypt = YesNo.Yes;
                        info.CopyProtection.LibCryptData = libCryptData;
                    }
                }
                else
                {
                    info.CopyProtection.LibCrypt = YesNo.No;
                }
            }
            else if (psLibCryptStatus == false)
            {
                info.CopyProtection.LibCrypt = YesNo.No;
            }
            else
            {
                info.CopyProtection.LibCrypt = YesNo.NULL;
                info.CopyProtection.LibCryptData = "LibCrypt could not be detected because subchannel file is missing";
            }
        }

        /// <summary>
        /// Get the PIC identifier from the first disc information unit, if possible
        /// </summary>
        /// <param name="di">Disc information containing the data</param>
        /// <returns>String representing the PIC identifier, null on error</returns>
        internal static string? GetPICIdentifier(DiscInformation? di)
        {
            // If we don't have valid disc information, we can't do anything
            if (di?.Units == null || di.Units.Length <= 1)
                return null;

            // We assume the identifier is consistent across all units
            return di.Units[0]?.Body?.DiscTypeIdentifier;
        }

        /// <summary>
        /// Get the EXE date from a PlayStation disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <param name="serial">Internal disc serial, if possible</param>
        /// <param name="region">Output region, if possible</param>
        /// <param name="date">Output EXE date in "yyyy-mm-dd" format if possible, null on error</param>
        /// <returns></returns>
        internal static bool GetPlayStationExecutableInfo(char? driveLetter, out string? serial, out Region? region, out string? date)
        {
            serial = null; region = null; date = null;

            // If there's no drive letter, we can't do this part
            if (driveLetter == null)
                return false;

            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            return GetPlayStationExecutableInfo(drivePath, out serial, out region, out date);
        }

        /// <summary>
        /// Get the EXE date from a PlayStation disc, if possible
        /// </summary>
        /// <param name="drivePath">Drive path to use to check</param>
        /// <param name="serial">Internal disc serial, if possible</param>
        /// <param name="region">Output region, if possible</param>
        /// <param name="date">Output EXE date in "yyyy-mm-dd" format if possible, null on error</param>
        /// <returns></returns>
        internal static bool GetPlayStationExecutableInfo(string? drivePath, out string? serial, out Region? region, out string? date)
        {
            serial = null; region = null; date = null;

            // If there's no drive path, we can't do this part
            if (string.IsNullOrWhiteSpace(drivePath))
                return false;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drivePath))
                return false;

            // Get the two paths that we will need to check
            string psxExePath = Path.Combine(drivePath, "PSX.EXE");
            string systemCnfPath = Path.Combine(drivePath, "SYSTEM.CNF");

            // Try both of the common paths that contain information
            string? exeName = null;

            // Read the CNF file as an INI file
            var systemCnf = new IniFile(systemCnfPath);
            string bootValue = string.Empty;

            // PlayStation uses "BOOT" as the key
            if (systemCnf.ContainsKey("BOOT"))
                bootValue = systemCnf["BOOT"];

            // PlayStation 2 uses "BOOT2" as the key
            if (systemCnf.ContainsKey("BOOT2"))
                bootValue = systemCnf["BOOT2"];

            // If we had any boot value, parse it and get the executable name
            if (!string.IsNullOrEmpty(bootValue))
            {
                var match = Regex.Match(bootValue, @"cdrom.?:\\?(.*)", RegexOptions.Compiled);
                if (match.Groups.Count > 1)
                {
                    // EXE name may have a trailing `;` after
                    // EXE name should always be in all caps
                    exeName = match.Groups[1].Value
                        .Split(';')[0]
                        .ToUpperInvariant();

                    // Serial is most of the EXE name normalized
                    serial = exeName
                        .Replace('_', '-')
                        .Replace(".", string.Empty);

                    // Some games may have the EXE in a subfolder
                    serial = Path.GetFileName(serial);
                }
            }

            // If the SYSTEM.CNF value can't be found, try PSX.EXE
            if (string.IsNullOrWhiteSpace(exeName) && File.Exists(psxExePath))
                exeName = "PSX.EXE";

            // If neither can be found, we return false
            if (string.IsNullOrWhiteSpace(exeName))
                return false;

            // Get the region, if possible
            region = GetPlayStationRegion(exeName);

            // Now that we have the EXE name, try to get the fileinfo for it
            string exePath = Path.Combine(drivePath, exeName);
            if (!File.Exists(exePath))
                return false;

            // Fix the Y2K timestamp issue
            var fi = new FileInfo(exePath);
            var dt = new DateTime(fi.LastWriteTimeUtc.Year >= 1900 && fi.LastWriteTimeUtc.Year < 1920 ? 2000 + fi.LastWriteTimeUtc.Year % 100 : fi.LastWriteTimeUtc.Year,
                fi.LastWriteTimeUtc.Month, fi.LastWriteTimeUtc.Day);
            date = dt.ToString("yyyy-MM-dd");

            return true;
        }

        /// <summary>
        /// Get the version from a PlayStation 2 disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>Game version if possible, null on error</returns>
        internal static string? GetPlayStation2Version(char? driveLetter)
        {
            // If there's no drive letter, we can't do this part
            if (driveLetter == null)
                return null;

            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            return (GetPlayStation2Version(drivePath));
        }

        /// <summary>
        /// Get the version from a PlayStation 2 disc, if possible
        /// </summary>
        /// <param name="drivePath">Drive path to use to check</param>
        /// <returns>Game version if possible, null on error</returns>
        internal static string? GetPlayStation2Version(string? drivePath)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrWhiteSpace(drivePath))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drivePath))
                return null;

            // Get the SYSTEM.CNF path to check
            string systemCnfPath = Path.Combine(drivePath, "SYSTEM.CNF");

            // Try to parse the SYSTEM.CNF file
            var systemCnf = new IniFile(systemCnfPath);
            if (systemCnf.ContainsKey("VER"))
                return systemCnf["VER"];

            // If "VER" can't be found, we can't do much
            return null;
        }

        /// <summary>
        /// Get the internal serial from a PlayStation 3 disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>Internal disc serial if possible, null on error</returns>
        internal static string? GetPlayStation3Serial(char? driveLetter)
        {
            // If there's no drive letter, we can't do this part
            if (driveLetter == null)
                return null;

            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            return GetPlayStation3Serial(drivePath);
        }

        /// <summary>
        /// Get the internal serial from a PlayStation 3 disc, if possible
        /// </summary>
        /// <param name="drivePath">Drive path to use to check</param>
        /// <returns>Internal disc serial if possible, null on error</returns>
        internal static string? GetPlayStation3Serial(string? drivePath)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrWhiteSpace(drivePath))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drivePath))
                return null;

            // Attempt to use PS3_DISC.SFB
            string sfbPath = Path.Combine(drivePath, "PS3_DISC.SFB");
            if (File.Exists(sfbPath))
            {
                try
                {
                    using var br = new BinaryReader(File.OpenRead(sfbPath));
                    br.BaseStream.Seek(0x220, SeekOrigin.Begin);
                    return new string(br.ReadChars(0x10)).TrimEnd('\0');
                }
                catch
                {
                    // We don't care what the error was
                    return null;
                }
            }

            // Attempt to use PARAM.SFO
            string sfoPath = Path.Combine(drivePath, "PS3_GAME", "PARAM.SFO");
            if (File.Exists(sfoPath))
            {
                try
                {
                    using var br = new BinaryReader(File.OpenRead(sfoPath));
                    br.BaseStream.Seek(-0x18, SeekOrigin.End);
                    return new string(br.ReadChars(9)).TrimEnd('\0'); ;
                }
                catch
                {
                    // We don't care what the error was
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the version from a PlayStation 3 disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>Game version if possible, null on error</returns>
        internal static string? GetPlayStation3Version(char? driveLetter)
        {
            // If there's no drive letter, we can't do this part
            if (driveLetter == null)
                return null;

            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            return GetPlayStation3Version(drivePath);
        }

        /// <summary>
        /// Get the version from a PlayStation 3 disc, if possible
        /// </summary>
        /// <param name="drivePath">Drive path to use to check</param>
        /// <returns>Game version if possible, null on error</returns>
        internal static string? GetPlayStation3Version(string? drivePath)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrWhiteSpace(drivePath))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drivePath))
                return null;

            // Attempt to use PS3_DISC.SFB
            string sfbPath = Path.Combine(drivePath, "PS3_DISC.SFB");
            if (File.Exists(sfbPath))
            {
                try
                {
                    using var br = new BinaryReader(File.OpenRead(sfbPath));
                    br.BaseStream.Seek(0x230, SeekOrigin.Begin);
                    var discVersion = new string(br.ReadChars(0x10)).TrimEnd('\0');
                    if (!string.IsNullOrWhiteSpace(discVersion))
                        return discVersion;
                }
                catch
                {
                    // We don't care what the error was
                    return null;
                }
            }

            // Attempt to use PARAM.SFO
            string sfoPath = Path.Combine(drivePath, "PS3_GAME", "PARAM.SFO");
            if (File.Exists(sfoPath))
            {
                try
                {
                    using var br = new BinaryReader(File.OpenRead(sfoPath));
                    br.BaseStream.Seek(-0x08, SeekOrigin.End);
                    return new string(br.ReadChars(5)).TrimEnd('\0');
                }
                catch
                {
                    // We don't care what the error was
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the firmware version from a PlayStation 3 disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>Firmware version if possible, null on error</returns>
        internal static string? GetPlayStation3FirmwareVersion(char? driveLetter)
        {
            // If there's no drive letter, we can't do this part
            if (driveLetter == null)
                return null;

            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            return GetPlayStation3FirmwareVersion(drivePath);
        }

        /// <summary>
        /// Get the firmware version from a PlayStation 3 disc, if possible
        /// </summary>
        /// <param name="drivePath">Drive path to use to check</param>
        /// <returns>Firmware version if possible, null on error</returns>
        internal static string? GetPlayStation3FirmwareVersion(string? drivePath)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrWhiteSpace(drivePath))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drivePath))
                return null;

            // Attempt to read from /PS3_UPDATE/PS3UPDAT.PUP
            string pupPath = Path.Combine(drivePath, "PS3_UPDATE", "PS3UPDAT.PUP");
            if (!File.Exists(pupPath))
                return null;

            try
            {
                using var br = new BinaryReader(File.OpenRead(pupPath));
                br.BaseStream.Seek(0x3E, SeekOrigin.Begin);
                byte[] buf = new byte[2];
                br.Read(buf, 0, 2);
                Array.Reverse(buf);
                short location = BitConverter.ToInt16(buf, 0);
                br.BaseStream.Seek(location, SeekOrigin.Begin);
                return new string(br.ReadChars(4));
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
        }

        /// <summary>
        /// Get the internal serial from a PlayStation 4 disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>Internal disc serial if possible, null on error</returns>
        internal static string? GetPlayStation4Serial(char? driveLetter)
        {
            // If there's no drive letter, we can't do this part
            if (driveLetter == null)
                return null;

            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            return GetPlayStation4Serial(drivePath);
        }

        /// <summary>
        /// Get the internal serial from a PlayStation 4 disc, if possible
        /// </summary>
        /// <param name="drivePath">Drive path to use to check</param>
        /// <returns>Internal disc serial if possible, null on error</returns>
        internal static string? GetPlayStation4Serial(string? drivePath)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrWhiteSpace(drivePath))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drivePath))
                return null;

            // If we can't find param.sfo, we don't have a PlayStation 4 disc
            string paramSfoPath = Path.Combine(drivePath, "bd", "param.sfo");
            if (!File.Exists(paramSfoPath))
                return null;

            // Let's try reading param.sfo to find the serial at the end of the file
            try
            {
                using var br = new BinaryReader(File.OpenRead(paramSfoPath));
                br.BaseStream.Seek(-0x14, SeekOrigin.End);
                return new string(br.ReadChars(9));
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
        }

        /// <summary>
        /// Get the version from a PlayStation 4 disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>Game version if possible, null on error</returns>
        internal static string? GetPlayStation4Version(char? driveLetter)
        {
            // If there's no drive letter, we can't do this part
            if (driveLetter == null)
                return null;

            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            return GetPlayStation4Version(drivePath);
        }

        /// <summary>
        /// Get the version from a PlayStation 4 disc, if possible
        /// </summary>
        /// <param name="drivePath">Drive path to use to check</param>
        /// <returns>Game version if possible, null on error</returns>
        internal static string? GetPlayStation4Version(string? drivePath)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrWhiteSpace(drivePath))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drivePath))
                return null;

            // If we can't find param.sfo, we don't have a PlayStation 4 disc
            string paramSfoPath = Path.Combine(drivePath, "bd", "param.sfo");
            if (!File.Exists(paramSfoPath))
                return null;

            // Let's try reading param.sfo to find the version at the end of the file
            try
            {
                using var br = new BinaryReader(File.OpenRead(paramSfoPath));
                br.BaseStream.Seek(-0x08, SeekOrigin.End);
                return new string(br.ReadChars(5));
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
        }

        /// <summary>
        /// Get the internal serial from a PlayStation 5 disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>Internal disc serial if possible, null on error</returns>
        internal static string? GetPlayStation5Serial(char? driveLetter)
        {
            // If there's no drive letter, we can't do this part
            if (driveLetter == null)
                return null;

            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            return GetPlayStation5Serial(drivePath);
        }

        /// <summary>
        /// Get the internal serial from a PlayStation 5 disc, if possible
        /// </summary>
        /// <param name="drivePath">Drive path to use to check</param>
        /// <returns>Internal disc serial if possible, null on error</returns>
        internal static string? GetPlayStation5Serial(string? drivePath)
        {
            // Attempt to get the param.json file
            var json = GetPlayStation5ParamsJsonFromDrive(drivePath);
            if (json == null)
                return null;

            try
            {
                return json["disc"]?[0]?["masterDataId"]?.Value<string>();
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
        }

        /// <summary>
        /// Get the version from a PlayStation 5 disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>Game version if possible, null on error</returns>
        internal static string? GetPlayStation5Version(char? driveLetter)
        {
            // If there's no drive letter, we can't do this part
            if (driveLetter == null)
                return null;

            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            return GetPlayStation5Version(drivePath);
        }

        /// <summary>
        /// Get the version from a PlayStation 5 disc, if possible
        /// </summary>
        /// <param name="drivePath">Drive path to use to check</param>
        /// <returns>Game version if possible, null on error</returns>
        internal static string? GetPlayStation5Version(string? drivePath)
        {
            // Attempt to get the param.json file
            var json = GetPlayStation5ParamsJsonFromDrive(drivePath);
            if (json == null)
                return null;

            try
            {
                return json["masterVersion"]?.Value<string>();
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
        }

        /// <summary>
        /// Get the params.json file from a drive path, if possible
        /// </summary>
        /// <param name="drivePath">Drive path to use to check</param>
        /// <returns>JObject representing the JSON on success, null on error</returns>
        private static JObject? GetPlayStation5ParamsJsonFromDrive(string? drivePath)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrWhiteSpace(drivePath))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drivePath))
                return null;

            // If we can't find param.json, we don't have a PlayStation 5 disc
            string paramJsonPath = Path.Combine(drivePath, "bd", "param.json");
            return GetPlayStation5ParamsJsonFromFile(paramJsonPath);
        }

        /// <summary>
        /// Get the params.json file from a filename, if possible
        /// </summary>
        /// <param name="filename">Filename to check</param>
        /// <returns>JObject representing the JSON on success, null on error</returns>
        private static JObject? GetPlayStation5ParamsJsonFromFile(string? filename)
        {
            // If the file doesn't exist
            if (string.IsNullOrWhiteSpace(filename) || !File.Exists(filename))
                return null;

            // Let's try reading param.json to find the version in the unencrypted JSON
            try
            {
                using var br = new BinaryReader(File.OpenRead(filename));
                br.BaseStream.Seek(0x800, SeekOrigin.Begin);
                byte[] jsonBytes = br.ReadBytes((int)(br.BaseStream.Length - 0x800));
                return JsonConvert.DeserializeObject(Encoding.ASCII.GetString(jsonBytes)) as JObject;
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
        }

        #endregion

        #region Category Extraction

        /// <summary>
        /// Determine the category based on the UMDImageCreator string
        /// </summary>
        /// <param name="region">String representing the category</param>
        /// <returns>Category, if possible</returns>
        internal static DiscCategory? GetUMDCategory(string? category)
        {
            return category switch
            {
                "GAME" => DiscCategory.Games,
                "VIDEO" => DiscCategory.Video,
                "AUDIO" => DiscCategory.Audio,
                _ => null,
            };
        }

        #endregion

        #region Region Extraction

        /// <summary>
        /// Determine the region based on the PlayStation serial code
        /// </summary>
        /// <param name="serial">PlayStation serial code</param>
        /// <returns>Region mapped from name, if possible</returns>
        internal static Region? GetPlayStationRegion(string? serial)
        {
            // If we have a fully invalid serial
            if (string.IsNullOrWhiteSpace(serial))
                return null;

            // Standardized "S" serials
            if (serial!.StartsWith("S"))
            {
                // string publisher = serial[0] + serial[1];
                // char secondRegion = serial[3];
                switch (serial[2])
                {
                    case 'A': return Region.Asia;
                    case 'C': return Region.China;
                    case 'E': return Region.Europe;
                    case 'K': return Region.SouthKorea;
                    case 'U': return Region.UnitedStatesOfAmerica;
                    case 'P':
                        // Region of S_P_ serials may be Japan, Asia, or SouthKorea
                        return serial[3] switch
                        {
                            // Check first two digits of S_PS serial
                            'S' => (Region?)(serial.Substring(5, 2) switch
                            {
                                "46" => Region.SouthKorea,
                                "51" => Region.Asia,
                                "56" => Region.SouthKorea,
                                "55" => Region.Asia,
                                _ => Region.Japan,
                            }),

                            // Check first three digits of S_PM serial
                            'M' => (Region?)(serial.Substring(5, 3) switch
                            {
                                "645" => Region.SouthKorea,
                                "675" => Region.SouthKorea,
                                "885" => Region.SouthKorea,
                                _ => Region.Japan, // Remaining S_PM serials may be Japan or Asia
                            }),
                            _ => (Region?)Region.Japan,
                        };
                }
            }

            // Japan-only special serial
            else if (serial.StartsWith("PAPX"))
                return Region.Japan;

            // Region appears entirely random
            else if (serial.StartsWith("PABX"))
                return null;

            // Region appears entirely random
            else if (serial.StartsWith("PBPX"))
                return null;

            // Japan-only special serial
            else if (serial.StartsWith("PCBX"))
                return Region.Japan;

            // Japan-only special serial
            else if (serial.StartsWith("PCXC"))
                return Region.Japan;

            // Single disc known, Japan
            else if (serial.StartsWith("PDBX"))
                return Region.Japan;

            // Single disc known, Europe
            else if (serial.StartsWith("PEBX"))
                return Region.Europe;

            // Single disc known, USA
            else if (serial.StartsWith("PUBX"))
                return Region.UnitedStatesOfAmerica;

            return null;
        }

        /// <summary>
        /// Determine the region based on the XGD serial character
        /// </summary>
        /// <param name="region">Character denoting the region</param>
        /// <returns>Region, if possible</returns>
        internal static Region? GetXGDRegion(char? region)
        {
            return region switch
            {
                'W' => (Region?)Region.World,
                'A' => (Region?)Region.UnitedStatesOfAmerica,
                'J' => (Region?)Region.JapanAsia,
                'E' => (Region?)Region.Europe,
                'K' => (Region?)Region.USAJapan,
                'L' => (Region?)Region.USAEurope,
                'H' => (Region?)Region.JapanEurope,
                _ => null,
            };
        }

        #endregion

        #region Information Output

        /// <summary>
        /// Compress log files to save space
        /// </summary>
        /// <param name="outputDirectory">Output folder to write to</param>
        /// <param name="filenameSuffix">Output filename to use as the base path</param>
        /// <param name="outputFilename">Output filename to use as the base path</param>
        /// <param name="parameters">Parameters object to use to derive log file paths</param>
        /// <returns>True if the process succeeded, false otherwise</returns>
        public static (bool, string) CompressLogFiles(string? outputDirectory, string? filenameSuffix, string outputFilename, BaseParameters? parameters)
        {
#if NET40
            return (false, "Log compression is not available for .NET Framework 4.0");
#else
            // If there are no parameters
            if (parameters == null)
                return (false, "No parameters provided!");

            // Prepare the necessary paths
            outputFilename = Path.GetFileNameWithoutExtension(outputFilename);
            string combinedBase;
            if (string.IsNullOrWhiteSpace(outputDirectory))
                combinedBase = outputFilename;
            else
                combinedBase = Path.Combine(outputDirectory, outputFilename);

            string archiveName = combinedBase + "_logs.zip";

            // Get the list of log files from the parameters object
            var files = parameters.GetLogFilePaths(combinedBase);

            // Add on generated log files if they exist
            var mpfFiles = GetGeneratedFilePaths(outputDirectory, filenameSuffix);
            files.AddRange(mpfFiles);

            if (!files.Any())
                return (true, "No files to compress!");

            // If the file already exists, we want to delete the old one
            try
            {
                if (File.Exists(archiveName))
                    File.Delete(archiveName);
            }
            catch
            {
                return (false, "Could not delete old archive!");
            }

            // Add the log files to the archive and delete the uncompressed file after
            ZipArchive? zf = null;
            try
            {
                zf = ZipFile.Open(archiveName, ZipArchiveMode.Create);
                foreach (string file in files)
                {
                    if (string.IsNullOrWhiteSpace(outputDirectory))
                    {
                        zf.CreateEntryFromFile(file, file, CompressionLevel.Optimal);
                    }
                    else
                    {
                        string entryName = file[outputDirectory!.Length..].TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

#if NETFRAMEWORK || NETCOREAPP3_1 || NET5_0
                        zf.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
#else
                        zf.CreateEntryFromFile(file, entryName, CompressionLevel.SmallestSize);
#endif
                    }

                    // If the file is MPF-specific, don't delete
                    if (mpfFiles.Contains(file))
                        continue;

                    try
                    {
                        File.Delete(file);
                    }
                    catch { }
                }

                return (true, "Compression complete!");
            }
            catch (Exception ex)
            {
                return (false, $"Compression could not complete: {ex}");
            }
            finally
            {
                zf?.Dispose();
            }
#endif
        }

        /// <summary>
        /// Compress log files to save space
        /// </summary>
        /// <param name="outputDirectory">Output folder to write to</param>
        /// <param name="outputFilename">Output filename to use as the base path</param>
        /// <param name="parameters">Parameters object to use to derive log file paths</param>
        /// <returns>True if the process succeeded, false otherwise</returns>
        public static (bool, string) DeleteUnnecessaryFiles(string? outputDirectory, string outputFilename, BaseParameters? parameters)
        {
            // If there are no parameters
            if (parameters == null)
                return (false, "No parameters provided!");

            // Prepare the necessary paths
            outputFilename = Path.GetFileNameWithoutExtension(outputFilename);
            string combinedBase;
            if (string.IsNullOrWhiteSpace(outputDirectory))
                combinedBase = outputFilename;
            else
                combinedBase = Path.Combine(outputDirectory, outputFilename);

            // Get the list of deleteable files from the parameters object
            var files = parameters.GetDeleteableFilePaths(combinedBase);

            if (!files.Any())
                return (true, "No files to delete!");

            // Attempt to delete all of the files
            try
            {
                foreach (string file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch { }
                }

                return (true, "Deletion complete!");
            }
            catch (Exception ex)
            {
                return (false, $"Deletion could not complete: {ex}");
            }
        }

        // Moved to RedumpLib
        /// <summary>
        /// Format the output data in a human readable way, separating each printed line into a new item in the list
        /// </summary>
        /// <param name="info">Information object that should contain normalized values</param>
        /// <param name="options">Options object representing user-defined options</param>
        /// <returns>List of strings representing each line of an output file, null on error</returns>
        public static (List<string>?, string?) FormatOutputData(SubmissionInfo? info, Data.Options options)
        {
            // Check to see if the inputs are valid
            if (info == null)
                return (null, "Submission information was missing");

            try
            {
                // Sony-printed discs have layers in the opposite order
                var system = info.CommonDiscInfo?.System;
                bool reverseOrder = system.HasReversedRingcodes();

                // Preamble for submission
#pragma warning disable IDE0028
                var output = new List<string>
                {
                    "Users who wish to submit this information to Redump must ensure that all of the fields below are accurate for the exact media they have.",
                    "Please double-check to ensure that there are no fields that need verification, such as the version or copy protection.",
                    "If there are no fields in need of verification or all fields are accurate, this preamble can be removed before submission.",
                    "",
                };

                // Common Disc Info section
                output.Add("Common Disc Info:");
                AddIfExists(output, Template.TitleField, info.CommonDiscInfo?.Title, 1);
                AddIfExists(output, Template.ForeignTitleField, info.CommonDiscInfo?.ForeignTitleNonLatin, 1);
                AddIfExists(output, Template.DiscNumberField, info.CommonDiscInfo?.DiscNumberLetter, 1);
                AddIfExists(output, Template.DiscTitleField, info.CommonDiscInfo?.DiscTitle, 1);
                AddIfExists(output, Template.SystemField, info.CommonDiscInfo?.System.LongName(), 1);
                AddIfExists(output, Template.MediaTypeField, GetFixedMediaType(
                        info.CommonDiscInfo?.Media.ToMediaType(),
                        info.SizeAndChecksums?.PICIdentifier,
                        info.SizeAndChecksums?.Size,
                        info.SizeAndChecksums?.Layerbreak,
                        info.SizeAndChecksums?.Layerbreak2,
                        info.SizeAndChecksums?.Layerbreak3),
                    1);
                AddIfExists(output, Template.CategoryField, info.CommonDiscInfo?.Category.LongName(), 1);
                AddIfExists(output, Template.FullyMatchingIDField, info.FullyMatchedID?.ToString(), 1);
                AddIfExists(output, Template.PartiallyMatchingIDsField, info.PartiallyMatchedIDs, 1);
                AddIfExists(output, Template.RegionField, info.CommonDiscInfo?.Region.LongName() ?? "SPACE! (CHANGE THIS)", 1);
                AddIfExists(output, Template.LanguagesField, (info.CommonDiscInfo?.Languages ?? [null]).Select(l => l.LongName() ?? "SILENCE! (CHANGE THIS)").ToArray(), 1);
                AddIfExists(output, Template.PlaystationLanguageSelectionViaField, (info.CommonDiscInfo?.LanguageSelection ?? []).Select(l => l.LongName()).ToArray(), 1);
                AddIfExists(output, Template.DiscSerialField, info.CommonDiscInfo?.Serial, 1);

                // All ringcode information goes in an indented area
                output.Add(""); output.Add("\tRingcode Information:"); output.Add("");

                // If we have a triple-layer disc
                if (info.SizeAndChecksums?.Layerbreak3 != default && info.SizeAndChecksums?.Layerbreak3 != default(long))
                {
                    AddIfExists(output, (reverseOrder ? "Layer 0 (Outer) " : "Layer 0 (Inner) ") + Template.MasteringRingField, info.CommonDiscInfo?.Layer0MasteringRing, 0);
                    AddIfExists(output, (reverseOrder ? "Layer 0 (Outer) " : "Layer 0 (Inner) ") + Template.MasteringSIDField, info.CommonDiscInfo?.Layer0MasteringSID, 0);
                    AddIfExists(output, (reverseOrder ? "Layer 0 (Outer) " : "Layer 0 (Inner) ") + Template.ToolstampField, info.CommonDiscInfo?.Layer0ToolstampMasteringCode, 0);
                    AddIfExists(output, "Data Side " + Template.MouldSIDField, info.CommonDiscInfo?.Layer0MouldSID, 0);
                    AddIfExists(output, "Data Side " + Template.AdditionalMouldField, info.CommonDiscInfo?.Layer0AdditionalMould, 0);

                    AddIfExists(output, "Layer 1 " + Template.MasteringRingField, info.CommonDiscInfo?.Layer1MasteringRing, 0);
                    AddIfExists(output, "Layer 1 " + Template.MasteringSIDField, info.CommonDiscInfo?.Layer1MasteringSID, 0);
                    AddIfExists(output, "Layer 1 " + Template.ToolstampField, info.CommonDiscInfo?.Layer1ToolstampMasteringCode, 0);
                    AddIfExists(output, "Label Side " + Template.MouldSIDField, info.CommonDiscInfo?.Layer1MouldSID, 0);
                    AddIfExists(output, "Label Side " + Template.AdditionalMouldField, info.CommonDiscInfo?.Layer1AdditionalMould, 0);

                    AddIfExists(output, "Layer 2 " + Template.MasteringRingField, info.CommonDiscInfo?.Layer2MasteringRing, 0);
                    AddIfExists(output, "Layer 2 " + Template.MasteringSIDField, info.CommonDiscInfo?.Layer2MasteringSID, 0);
                    AddIfExists(output, "Layer 2 " + Template.ToolstampField, info.CommonDiscInfo?.Layer2ToolstampMasteringCode, 0);

                    AddIfExists(output, (reverseOrder ? "Layer 3 (Inner) " : "Layer 3 (Outer) ") + Template.MasteringRingField, info.CommonDiscInfo?.Layer3MasteringRing, 0);
                    AddIfExists(output, (reverseOrder ? "Layer 3 (Inner) " : "Layer 3 (Outer) ") + Template.MasteringSIDField, info.CommonDiscInfo?.Layer3MasteringSID, 0);
                    AddIfExists(output, (reverseOrder ? "Layer 3 (Inner) " : "Layer 3 (Outer) ") + Template.ToolstampField, info.CommonDiscInfo?.Layer3ToolstampMasteringCode, 0);
                }
                // If we have a triple-layer disc
                else if (info.SizeAndChecksums?.Layerbreak2 != default && info.SizeAndChecksums?.Layerbreak2 != default(long))
                {
                    AddIfExists(output, (reverseOrder ? "Layer 0 (Outer) " : "Layer 0 (Inner) ") + Template.MasteringRingField, info.CommonDiscInfo?.Layer0MasteringRing, 0);
                    AddIfExists(output, (reverseOrder ? "Layer 0 (Outer) " : "Layer 0 (Inner) ") + Template.MasteringSIDField, info.CommonDiscInfo?.Layer0MasteringSID, 0);
                    AddIfExists(output, (reverseOrder ? "Layer 0 (Outer) " : "Layer 0 (Inner) ") + Template.ToolstampField, info.CommonDiscInfo?.Layer0ToolstampMasteringCode, 0);
                    AddIfExists(output, "Data Side " + Template.MouldSIDField, info.CommonDiscInfo?.Layer0MouldSID, 0);
                    AddIfExists(output, "Data Side " + Template.AdditionalMouldField, info.CommonDiscInfo?.Layer0AdditionalMould, 0);

                    AddIfExists(output, "Layer 1 " + Template.MasteringRingField, info.CommonDiscInfo?.Layer1MasteringRing, 0);
                    AddIfExists(output, "Layer 1 " + Template.MasteringSIDField, info.CommonDiscInfo?.Layer1MasteringSID, 0);
                    AddIfExists(output, "Layer 1 " + Template.ToolstampField, info.CommonDiscInfo?.Layer1ToolstampMasteringCode, 0);
                    AddIfExists(output, "Label Side " + Template.MouldSIDField, info.CommonDiscInfo?.Layer1MouldSID, 0);
                    AddIfExists(output, "Label Side " + Template.AdditionalMouldField, info.CommonDiscInfo?.Layer1AdditionalMould, 0);

                    AddIfExists(output, (reverseOrder ? "Layer 2 (Inner) " : "Layer 2 (Outer) ") + Template.MasteringRingField, info.CommonDiscInfo?.Layer2MasteringRing, 0);
                    AddIfExists(output, (reverseOrder ? "Layer 2 (Inner) " : "Layer 2 (Outer) ") + Template.MasteringSIDField, info.CommonDiscInfo?.Layer2MasteringSID, 0);
                    AddIfExists(output, (reverseOrder ? "Layer 2 (Inner) " : "Layer 2 (Outer) ") + Template.ToolstampField, info.CommonDiscInfo?.Layer2ToolstampMasteringCode, 0);
                }
                // If we have a dual-layer disc
                else if (info.SizeAndChecksums?.Layerbreak != default && info.SizeAndChecksums?.Layerbreak != default(long))
                {
                    AddIfExists(output, (reverseOrder ? "Layer 0 (Outer) " : "Layer 0 (Inner) ") + Template.MasteringRingField, info.CommonDiscInfo?.Layer0MasteringRing, 0);
                    AddIfExists(output, (reverseOrder ? "Layer 0 (Outer) " : "Layer 0 (Inner) ") + Template.MasteringSIDField, info.CommonDiscInfo?.Layer0MasteringSID, 0);
                    AddIfExists(output, (reverseOrder ? "Layer 0 (Outer) " : "Layer 0 (Inner) ") + Template.ToolstampField, info.CommonDiscInfo?.Layer0ToolstampMasteringCode, 0);
                    AddIfExists(output, "Data Side " + Template.MouldSIDField, info.CommonDiscInfo?.Layer0MouldSID, 0);
                    AddIfExists(output, "Data Side " + Template.AdditionalMouldField, info.CommonDiscInfo?.Layer0AdditionalMould, 0);

                    AddIfExists(output, (reverseOrder ? "Layer 1 (Inner) " : "Layer 1 (Outer) ") + Template.MasteringRingField, info.CommonDiscInfo?.Layer1MasteringRing, 0);
                    AddIfExists(output, (reverseOrder ? "Layer 1 (Inner) " : "Layer 1 (Outer) ") + Template.MasteringSIDField, info.CommonDiscInfo?.Layer1MasteringSID, 0);
                    AddIfExists(output, (reverseOrder ? "Layer 1 (Inner) " : "Layer 1 (Outer) ") + Template.ToolstampField, info.CommonDiscInfo?.Layer1ToolstampMasteringCode, 0);
                    AddIfExists(output, "Label Side " + Template.MouldSIDField, info.CommonDiscInfo?.Layer1MouldSID, 0);
                    AddIfExists(output, "Label Side " + Template.AdditionalMouldField, info.CommonDiscInfo?.Layer1AdditionalMould, 0);
                }
                // If we have a single-layer disc
                else
                {
                    AddIfExists(output, "Data Side " + Template.MasteringRingField, info.CommonDiscInfo?.Layer0MasteringRing, 0);
                    AddIfExists(output, "Data Side " + Template.MasteringSIDField, info.CommonDiscInfo?.Layer0MasteringSID, 0);
                    AddIfExists(output, "Data Side " + Template.ToolstampField, info.CommonDiscInfo?.Layer0ToolstampMasteringCode, 0);
                    AddIfExists(output, "Data Side " + Template.MouldSIDField, info.CommonDiscInfo?.Layer0MouldSID, 0);
                    AddIfExists(output, "Data Side " + Template.AdditionalMouldField, info.CommonDiscInfo?.Layer0AdditionalMould, 0);

                    AddIfExists(output, "Label Side " + Template.MasteringRingField, info.CommonDiscInfo?.Layer1MasteringRing, 0);
                    AddIfExists(output, "Label Side " + Template.MasteringSIDField, info.CommonDiscInfo?.Layer1MasteringSID, 0);
                    AddIfExists(output, "Label Side " + Template.ToolstampField, info.CommonDiscInfo?.Layer1ToolstampMasteringCode, 0);
                    AddIfExists(output, "Label Side " + Template.MouldSIDField, info.CommonDiscInfo?.Layer1MouldSID, 0);
                    AddIfExists(output, "Label Side " + Template.AdditionalMouldField, info.CommonDiscInfo?.Layer1AdditionalMould, 0);
                }

                output.Add("");
                AddIfExists(output, Template.BarcodeField, info.CommonDiscInfo?.Barcode, 1);
                AddIfExists(output, Template.EXEDateBuildDate, info.CommonDiscInfo?.EXEDateBuildDate, 1);
                AddIfExists(output, Template.ErrorCountField, info.CommonDiscInfo?.ErrorsCount, 1);
                AddIfExists(output, Template.CommentsField, info.CommonDiscInfo?.Comments?.Trim(), 1);
                AddIfExists(output, Template.ContentsField, info.CommonDiscInfo?.Contents?.Trim(), 1);

                // Version and Editions section
                output.Add(""); output.Add("Version and Editions:");
                AddIfExists(output, Template.VersionField, info.VersionAndEditions?.Version, 1);
                AddIfExists(output, Template.EditionField, info.VersionAndEditions?.OtherEditions, 1);

                // EDC section
                if (info.CommonDiscInfo?.System == RedumpSystem.SonyPlayStation)
                {
                    output.Add(""); output.Add("EDC:");
                    AddIfExists(output, Template.PlayStationEDCField, info.EDC?.EDC.LongName(), 1);
                }

                // Parent/Clone Relationship section
                // output.Add(""); output.Add("Parent/Clone Relationship:");
                // AddIfExists(output, Template.ParentIDField, info.ParentID);
                // AddIfExists(output, Template.RegionalParentField, info.RegionalParent.ToString());

                // Extras section
                if (info.Extras?.PVD != null || info.Extras?.PIC != null || info.Extras?.BCA != null || info.Extras?.SecuritySectorRanges != null)
                {
                    output.Add(""); output.Add("Extras:");
                    AddIfExists(output, Template.PVDField, info.Extras.PVD?.Trim(), 1);
                    AddIfExists(output, Template.PlayStation3WiiDiscKeyField, info.Extras.DiscKey, 1);
                    AddIfExists(output, Template.PlayStation3DiscIDField, info.Extras.DiscID, 1);
                    AddIfExists(output, Template.PICField, info.Extras.PIC, 1);
                    AddIfExists(output, Template.HeaderField, info.Extras.Header, 1);
                    AddIfExists(output, Template.GameCubeWiiBCAField, info.Extras.BCA, 1);
                    AddIfExists(output, Template.XBOXSSRanges, info.Extras.SecuritySectorRanges, 1);
                }

                // Copy Protection section
                if (!string.IsNullOrWhiteSpace(info.CopyProtection?.Protection)
                    || (info.CopyProtection?.AntiModchip != null && info.CopyProtection.AntiModchip != YesNo.NULL)
                    || (info.CopyProtection?.LibCrypt != null && info.CopyProtection.LibCrypt != YesNo.NULL)
                    || !string.IsNullOrWhiteSpace(info.CopyProtection?.LibCryptData)
                    || !string.IsNullOrWhiteSpace(info.CopyProtection?.SecuROMData))
                {
                    output.Add(""); output.Add("Copy Protection:");
                    if (info.CommonDiscInfo?.System == RedumpSystem.SonyPlayStation)
                    {
                        AddIfExists(output, Template.PlayStationAntiModchipField, info.CopyProtection!.AntiModchip.LongName(), 1);
                        AddIfExists(output, Template.PlayStationLibCryptField, info.CopyProtection.LibCrypt.LongName(), 1);
                        AddIfExists(output, Template.SubIntentionField, info.CopyProtection.LibCryptData, 1);
                    }

                    AddIfExists(output, Template.CopyProtectionField, info.CopyProtection!.Protection, 1);
                    AddIfExists(output, Template.SubIntentionField, info.CopyProtection.SecuROMData, 1);
                }

                // Dumpers and Status section
                // output.Add(""); output.Add("Dumpers and Status");
                // AddIfExists(output, Template.StatusField, info.Status.Name());
                // AddIfExists(output, Template.OtherDumpersField, info.OtherDumpers);

                // Tracks and Write Offsets section
                if (!string.IsNullOrWhiteSpace(info.TracksAndWriteOffsets?.ClrMameProData))
                {
                    output.Add(""); output.Add("Tracks and Write Offsets:");
                    AddIfExists(output, Template.DATField, info.TracksAndWriteOffsets!.ClrMameProData + "\n", 1);
                    AddIfExists(output, Template.CuesheetField, info.TracksAndWriteOffsets.Cuesheet, 1);
                    var offset = info.TracksAndWriteOffsets.OtherWriteOffsets;
                    if (Int32.TryParse(offset, out int i))
                        offset = i.ToString("+#;-#;0");

                    AddIfExists(output, Template.WriteOffsetField, offset, 1);
                }
                // Size & Checksum section
                else
                {
                    output.Add(""); output.Add("Size & Checksum:");

                    // Gross hack because of automatic layerbreaks in Redump
                    if (!options.EnableRedumpCompatibility
                        || (info.CommonDiscInfo?.Media.ToMediaType() != MediaType.BluRay
                            && info.CommonDiscInfo?.System.IsXGD() == false))
                    {
                        AddIfExists(output, Template.LayerbreakField, info.SizeAndChecksums?.Layerbreak, 1);
                    }

                    AddIfExists(output, Template.SizeField, info.SizeAndChecksums?.Size.ToString(), 1);
                    AddIfExists(output, Template.CRC32Field, info.SizeAndChecksums?.CRC32, 1);
                    AddIfExists(output, Template.MD5Field, info.SizeAndChecksums?.MD5, 1);
                    AddIfExists(output, Template.SHA1Field, info.SizeAndChecksums?.SHA1, 1);
                }

                // Dumping Info section
                output.Add(""); output.Add("Dumping Info:");
                AddIfExists(output, Template.DumpingProgramField, info.DumpingInfo?.DumpingProgram, 1);
                AddIfExists(output, Template.DumpingDateField, info.DumpingInfo?.DumpingDate, 1);
                AddIfExists(output, Template.DumpingDriveManufacturer, info.DumpingInfo?.Manufacturer, 1);
                AddIfExists(output, Template.DumpingDriveModel, info.DumpingInfo?.Model, 1);
                AddIfExists(output, Template.DumpingDriveFirmware, info.DumpingInfo?.Firmware, 1);
                AddIfExists(output, Template.ReportedDiscType, info.DumpingInfo?.ReportedDiscType, 1);

                // Make sure there aren't any instances of two blank lines in a row
                string? last = null;
                for (int i = 0; i < output.Count;)
                {
                    if (output[i] == last && string.IsNullOrWhiteSpace(last))
                    {
                        output.RemoveAt(i);
                    }
                    else
                    {
                        last = output[i];
                        i++;
                    }
                }

                return (output, "Formatting complete!");
            }
            catch (Exception ex)
            {
                return (null, $"Error formatting submission info: {ex}");
            }
        }

        // Moved to RedumpLib
        /// <summary>
        /// Get the adjusted name of the media based on layers, if applicable
        /// </summary>
        /// <param name="mediaType">MediaType to get the proper name for</param>
        /// <param name="picIdentifier">PIC identifier string (BD only)</param>
        /// <param name="size">Size of the current media</param>
        /// <param name="layerbreak">First layerbreak value, as applicable</param>
        /// <param name="layerbreak2">Second layerbreak value, as applicable</param>
        /// <param name="layerbreak3">Third layerbreak value, as applicable</param>
        /// <returns>String representation of the media, including layer specification</returns>
        /// TODO: Figure out why we have this and NormalizeDiscType as well
        public static string? GetFixedMediaType(MediaType? mediaType, string? picIdentifier, long? size, long? layerbreak, long? layerbreak2, long? layerbreak3)
        {
            switch (mediaType)
            {
                case MediaType.DVD:
                    if (layerbreak != default && layerbreak != default(long))
                        return $"{mediaType.LongName()}-9";
                    else
                        return $"{mediaType.LongName()}-5";

                case MediaType.BluRay:
                    if (layerbreak3 != default && layerbreak3 != default(long))
                        return $"{mediaType.LongName()}-128";
                    else if (layerbreak2 != default && layerbreak2 != default(long))
                        return $"{mediaType.LongName()}-100";
                    else if (layerbreak != default && layerbreak != default(long) && picIdentifier == SabreTools.Models.PIC.Constants.DiscTypeIdentifierROMUltra)
                        return $"{mediaType.LongName()}-66";
                    else if (layerbreak != default && layerbreak != default(long) && size > 53_687_063_712)
                        return $"{mediaType.LongName()}-66";
                    else if (layerbreak != default && layerbreak != default(long))
                        return $"{mediaType.LongName()}-50";
                    else if (picIdentifier == SabreTools.Models.PIC.Constants.DiscTypeIdentifierROMUltra)
                        return $"{mediaType.LongName()}-33";
                    else if (size > 26_843_531_856)
                        return $"{mediaType.LongName()}-33";
                    else
                        return $"{mediaType.LongName()}-25";

                case MediaType.UMD:
                    if (layerbreak != default && layerbreak != default(long))
                        return $"{mediaType.LongName()}-DL";
                    else
                        return $"{mediaType.LongName()}-SL";

                default:
                    return mediaType.LongName();
            }
        }

        // Moved to RedumpLib
        /// <summary>
        /// Process any fields that have to be combined
        /// </summary>
        /// <param name="info">Information object to normalize</param>
        public static void ProcessSpecialFields(SubmissionInfo? info)
        {
            // If there is no submission info
            if (info == null)
                return;

            // Process the comments field
            if (info.CommonDiscInfo?.CommentsSpecialFields != null && info.CommonDiscInfo.CommentsSpecialFields?.Any() == true)
            {
                // If the field is missing, add an empty one to fill in
                if (info.CommonDiscInfo.Comments == null)
                    info.CommonDiscInfo.Comments = string.Empty;

                // Add all special fields before any comments
                info.CommonDiscInfo.Comments = string.Join(
                    "\n", OrderCommentTags(info.CommonDiscInfo.CommentsSpecialFields)
                        .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                        .Select(FormatSiteTag)
                        .Where(s => !string.IsNullOrEmpty(s))
                ) + "\n" + info.CommonDiscInfo.Comments;

                // Normalize newlines
                info.CommonDiscInfo.Comments = info.CommonDiscInfo.Comments.Replace("\r\n", "\n");

                // Trim the comments field
                info.CommonDiscInfo.Comments = info.CommonDiscInfo.Comments.Trim();

                // Wipe out the special fields dictionary
                info.CommonDiscInfo.CommentsSpecialFields = null;
            }

            // Process the contents field
            if (info.CommonDiscInfo?.ContentsSpecialFields != null && info.CommonDiscInfo.ContentsSpecialFields?.Any() == true)
            {
                // If the field is missing, add an empty one to fill in
                if (info.CommonDiscInfo.Contents == null)
                    info.CommonDiscInfo.Contents = string.Empty;

                // Add all special fields before any contents
                info.CommonDiscInfo.Contents = string.Join(
                    "\n", OrderContentTags(info.CommonDiscInfo.ContentsSpecialFields)
                        .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                        .Select(FormatSiteTag)
                        .Where(s => !string.IsNullOrEmpty(s))
                ) + "\n" + info.CommonDiscInfo.Contents;

                // Normalize newlines
                info.CommonDiscInfo.Contents = info.CommonDiscInfo.Contents.Replace("\r\n", "\n");

                // Trim the contents field
                info.CommonDiscInfo.Contents = info.CommonDiscInfo.Contents.Trim();

                // Wipe out the special fields dictionary
                info.CommonDiscInfo.ContentsSpecialFields = null;
            }
        }

        /// <summary>
        /// Write the data to the output folder
        /// </summary>
        /// <param name="outputDirectory">Output folder to write to</param>
        /// <param name="filenameSuffix">Optional suffix to append to the filename</param>
        /// <param name="lines">Preformatted list of lines to write out to the file</param>
        /// <returns>True on success, false on error</returns>
        public static (bool, string) WriteOutputData(string? outputDirectory, string? filenameSuffix, List<string>? lines)
        {
            // Check to see if the inputs are valid
            if (lines == null)
                return (false, "No formatted data found to write!");

            // Now write out to a generic file
            try
            {
                // Get the file path
                var path = string.Empty;
                if (string.IsNullOrWhiteSpace(outputDirectory) && string.IsNullOrWhiteSpace(filenameSuffix))
                    path = "!submissionInfo.txt";
                else if (string.IsNullOrWhiteSpace(outputDirectory) && !string.IsNullOrWhiteSpace(filenameSuffix))
                    path = $"!submissionInfo_{filenameSuffix}.txt";
                else if (!string.IsNullOrWhiteSpace(outputDirectory) && string.IsNullOrWhiteSpace(filenameSuffix))
                    path = Path.Combine(outputDirectory, "!submissionInfo.txt");
                else if (!string.IsNullOrWhiteSpace(outputDirectory) && !string.IsNullOrWhiteSpace(filenameSuffix))
                    path = Path.Combine(outputDirectory, $"!submissionInfo_{filenameSuffix}.txt");

                using var sw = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.Write));
                foreach (string line in lines)
                {
                    sw.WriteLine(line);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Writing could not complete: {ex}");
            }

            return (true, "Writing complete!");
        }

        /// <summary>
        /// Write the data to the output folder
        /// </summary>
        /// <param name="outputDirectory">Output folder to write to</param>
        /// <param name="filenameSuffix">Optional suffix to append to the filename</param>
        /// <param name="info">SubmissionInfo object representing the JSON to write out to the file</param>
        /// <param name="includedArtifacts">True if artifacts were included, false otherwise</param>
        /// <returns>True on success, false on error</returns>
        public static bool WriteOutputData(string? outputDirectory, string? filenameSuffix, SubmissionInfo? info, bool includedArtifacts)
        {
            // Check to see if the input is valid
            if (info == null)
                return false;

            try
            {
                // Serialize the JSON and get it writable
                string json = JsonConvert.SerializeObject(info, Formatting.Indented);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

                // If we included artifacts, write to a GZip-compressed file
                if (includedArtifacts)
                {
                    var path = string.Empty;
                    if (string.IsNullOrWhiteSpace(outputDirectory) && string.IsNullOrWhiteSpace(filenameSuffix))
                        path = "!submissionInfo.json.gz";
                    else if (string.IsNullOrWhiteSpace(outputDirectory) && !string.IsNullOrWhiteSpace(filenameSuffix))
                        path = $"!submissionInfo_{filenameSuffix}.json.gz";
                    else if (!string.IsNullOrWhiteSpace(outputDirectory) && string.IsNullOrWhiteSpace(filenameSuffix))
                        path = Path.Combine(outputDirectory, "!submissionInfo.json.gz");
                    else if (!string.IsNullOrWhiteSpace(outputDirectory) && !string.IsNullOrWhiteSpace(filenameSuffix))
                        path = Path.Combine(outputDirectory, $"!submissionInfo_{filenameSuffix}.json.gz");

                    using var fs = File.Create(path);
                    using var gs = new GZipStream(fs, CompressionMode.Compress);
                    gs.Write(jsonBytes, 0, jsonBytes.Length);
                }

                // Otherwise, write out to a normal JSON
                else
                {
                    var path = string.Empty;
                    if (string.IsNullOrWhiteSpace(outputDirectory) && string.IsNullOrWhiteSpace(filenameSuffix))
                        path = "!submissionInfo.json";
                    else if (string.IsNullOrWhiteSpace(outputDirectory) && !string.IsNullOrWhiteSpace(filenameSuffix))
                        path = $"!submissionInfo_{filenameSuffix}.json";
                    else if (!string.IsNullOrWhiteSpace(outputDirectory) && string.IsNullOrWhiteSpace(filenameSuffix))
                        path = Path.Combine(outputDirectory, "!submissionInfo.json");
                    else if (!string.IsNullOrWhiteSpace(outputDirectory) && !string.IsNullOrWhiteSpace(filenameSuffix))
                        path = Path.Combine(outputDirectory, $"!submissionInfo_{filenameSuffix}.json");

                    using var fs = File.Create(path);
                    fs.Write(jsonBytes, 0, jsonBytes.Length);
                }
            }
            catch
            {
                // We don't care what the error is right now
                return false;
            }

            return true;
        }

        /// <summary>
        /// Write the protection data to the output folder
        /// </summary>
        /// <param name="outputDirectory">Output folder to write to</param>
        /// <param name="filenameSuffix">Optional suffix to append to the filename</param>
        /// <param name="info">SubmissionInfo object containing the protection information</param>
        /// <returns>True on success, false on error</returns>
        public static bool WriteProtectionData(string? outputDirectory, string? filenameSuffix, SubmissionInfo? info)
        {
            // Check to see if the inputs are valid
            if (info?.CopyProtection?.FullProtections == null || !info.CopyProtection.FullProtections.Any())
                return true;

            // Now write out to a generic file
            try
            {
                var path = string.Empty;
                if (string.IsNullOrWhiteSpace(outputDirectory) && string.IsNullOrWhiteSpace(filenameSuffix))
                    path = "!protectionInfo.txt";
                else if (string.IsNullOrWhiteSpace(outputDirectory) && !string.IsNullOrWhiteSpace(filenameSuffix))
                    path = $"!protectionInfo{filenameSuffix}.txt";
                else if (!string.IsNullOrWhiteSpace(outputDirectory) && string.IsNullOrWhiteSpace(filenameSuffix))
                    path = Path.Combine(outputDirectory, "!protectionInfo.txt");
                else if (!string.IsNullOrWhiteSpace(outputDirectory) && !string.IsNullOrWhiteSpace(filenameSuffix))
                    path = Path.Combine(outputDirectory, $"!protectionInfo{filenameSuffix}.txt");

                using var sw = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.Write));
                foreach (var kvp in info.CopyProtection.FullProtections)
                {
                    if (kvp.Value == null)
                        sw.WriteLine($"{kvp.Key}: None");
                    else
                        sw.WriteLine($"{kvp.Key}: {string.Join(", ", kvp.Value)}");
                }
            }
            catch
            {
                // We don't care what the error is right now
                return false;
            }

            return true;
        }

        // Moved to RedumpLib
        /// <summary>
        /// Add the properly formatted key and value, if possible
        /// </summary>
        /// <param name="output">Output list</param>
        /// <param name="key">Name of the output key to write</param>
        /// <param name="value">Name of the output value to write</param>
        /// <param name="indent">Number of tabs to indent the line</param>
        private static void AddIfExists(List<string> output, string key, string? value, int indent)
        {
            // If there's no valid value to write
            if (value == null)
                return;

            string prefix = string.Empty;
            for (int i = 0; i < indent; i++)
                prefix += "\t";

            // Skip fields that need to keep internal whitespace intact
            if (key != "Primary Volume Descriptor (PVD)"
                && key != "Header"
                && key != "Cuesheet")
            {
                // Convert to tabs
                value = value.Replace("<tab>", "\t");
                value = value.Replace("<TAB>", "\t");
                value = value.Replace("   ", "\t");

                // Sanitize whitespace around tabs
                value = Regex.Replace(value, @"\s*\t\s*", "\t", RegexOptions.Compiled);
            }

            // If the value contains a newline
            value = value.Replace("\r\n", "\n");
            if (value.Contains('\n'))
            {
                output.Add(prefix + key + ":"); output.Add("");
                string[] values = value.Split('\n');
                foreach (string val in values)
                    output.Add(val);

                output.Add("");
            }

            // For all regular values
            else
            {
                output.Add(prefix + key + ": " + value);
            }
        }

        // Moved to RedumpLib
        /// <summary>
        /// Add the properly formatted key and value, if possible
        /// </summary>
        /// <param name="output">Output list</param>
        /// <param name="key">Name of the output key to write</param>
        /// <param name="value">Name of the output value to write</param>
        /// <param name="indent">Number of tabs to indent the line</param>
        private static void AddIfExists(List<string> output, string key, string?[]? value, int indent)
        {
            // If there's no valid value to write
            if (value == null || value.Length == 0)
                return;

            AddIfExists(output, key, string.Join(", ", value), indent);
        }

        // Moved to RedumpLib
        /// <summary>
        /// Add the properly formatted key and value, if possible
        /// </summary>
        /// <param name="output">Output list</param>
        /// <param name="key">Name of the output key to write</param>
        /// <param name="value">Name of the output value to write</param>
        /// <param name="indent">Number of tabs to indent the line</param>
        private static void AddIfExists(List<string> output, string key, long? value, int indent)
        {
            // If there's no valid value to write
            if (value == null || value == default(long))
                return;

            string prefix = string.Empty;
            for (int i = 0; i < indent; i++)
                prefix += "\t";

            output.Add(prefix + key + ": " + value);
        }

        // Moved to RedumpLib
        /// <summary>
        /// Add the properly formatted key and value, if possible
        /// </summary>
        /// <param name="output">Output list</param>
        /// <param name="key">Name of the output key to write</param>
        /// <param name="value">Name of the output value to write</param>
        /// <param name="indent">Number of tabs to indent the line</param>
        private static void AddIfExists(List<string> output, string key, List<int>? value, int indent)
        {
            // If there's no valid value to write
            if (value == null || value.Count == 0)
                return;

            AddIfExists(output, key, string.Join(", ", value.Select(o => o.ToString())), indent);
        }

        /// <summary>
        /// Generate a list of all MPF-specific log files generated
        /// </summary>
        /// <param name="outputDirectory">Output folder to write to</param>
        /// <param name="filenameSuffix">Optional suffix to append to the filename</param>
        /// <returns>List of all log file paths, empty otherwise</returns>
        private static List<string> GetGeneratedFilePaths(string? outputDirectory, string? filenameSuffix)
        {
            var files = new List<string>();

            if (string.IsNullOrWhiteSpace(outputDirectory) && string.IsNullOrWhiteSpace(filenameSuffix))
            {
                if (File.Exists("!submissionInfo.txt"))
                    files.Add("!submissionInfo.txt");
                if (File.Exists("!submissionInfo.json"))
                    files.Add("!submissionInfo.json");
                if (File.Exists("!submissionInfo.json.gz"))
                    files.Add("!submissionInfo.json.gz");
                if (File.Exists("!protectionInfo.txt"))
                    files.Add("!protectionInfo.txt");
            }
            else if (string.IsNullOrWhiteSpace(outputDirectory) && !string.IsNullOrWhiteSpace(filenameSuffix))
            {
                if (File.Exists($"!submissionInfo_{filenameSuffix}.txt"))
                    files.Add($"!submissionInfo_{filenameSuffix}.txt");
                if (File.Exists($"!submissionInfo_{filenameSuffix}.json"))
                    files.Add($"!submissionInfo_{filenameSuffix}.json");
                if (File.Exists($"!submissionInfo_{filenameSuffix}.json.gz"))
                    files.Add($"!submissionInfo_{filenameSuffix}.json.gz");
                if (File.Exists($"!protectionInfo_{filenameSuffix}.txt"))
                    files.Add($"!protectionInfo_{filenameSuffix}.txt");
            }
            else if (!string.IsNullOrWhiteSpace(outputDirectory) && string.IsNullOrWhiteSpace(filenameSuffix))
            {
                if (File.Exists(Path.Combine(outputDirectory, "!submissionInfo.txt")))
                    files.Add(Path.Combine(outputDirectory, "!submissionInfo.txt"));
                if (File.Exists(Path.Combine(outputDirectory, "!submissionInfo.json")))
                    files.Add(Path.Combine(outputDirectory, "!submissionInfo.json"));
                if (File.Exists(Path.Combine(outputDirectory, "!submissionInfo.json.gz")))
                    files.Add(Path.Combine(outputDirectory, "!submissionInfo.json.gz"));
                if (File.Exists(Path.Combine(outputDirectory, "!protectionInfo.txt")))
                    files.Add(Path.Combine(outputDirectory, "!protectionInfo.txt"));
            }
            else if (!string.IsNullOrWhiteSpace(outputDirectory) && !string.IsNullOrWhiteSpace(filenameSuffix))
            {
                if (File.Exists(Path.Combine(outputDirectory, $"!submissionInfo_{filenameSuffix}.txt")))
                    files.Add(Path.Combine(outputDirectory, $"!submissionInfo_{filenameSuffix}.txt"));
                if (File.Exists(Path.Combine(outputDirectory, $"!submissionInfo_{filenameSuffix}.json")))
                    files.Add(Path.Combine(outputDirectory, $"!submissionInfo_{filenameSuffix}.json"));
                if (File.Exists(Path.Combine(outputDirectory, $"!submissionInfo_{filenameSuffix}.json.gz")))
                    files.Add(Path.Combine(outputDirectory, $"!submissionInfo_{filenameSuffix}.json.gz"));
                if (File.Exists(Path.Combine(outputDirectory, $"!protectionInfo_{filenameSuffix}.txt")))
                    files.Add(Path.Combine(outputDirectory, $"!protectionInfo_{filenameSuffix}.txt"));
            }

            return files;
        }

        #endregion

        #region Normalization

        /// <summary>
        /// Adjust a disc title so that it will be processed correctly by Redump
        /// </summary>
        /// <param name="title">Existing title to potentially reformat</param>
        /// <param name="languages">Array of languages to use for assuming articles</param>
        /// <returns>The reformatted title</returns>
        public static string NormalizeDiscTitle(string title, Language[] languages)
        {
            // If we have no set languages, then assume English
            if (languages == null || languages.Length == 0)
                languages = [Language.English];

            // Loop through all of the given languages
            foreach (var language in languages)
            {
                // If the new title is different, assume it was normalized and return it
                string newTitle = NormalizeDiscTitle(title, language);
                if (newTitle == title)
                    return newTitle;
            }

            // If we didn't already try English, try it now
            if (!languages.Contains(Language.English))
                return NormalizeDiscTitle(title, Language.English);

            // If all fails, then the title didn't need normalization
            return title;
        }

        /// <summary>
        /// Adjust a disc title so that it will be processed correctly by Redump
        /// </summary>
        /// <param name="title">Existing title to potentially reformat</param>
        /// <param name="language">Language to use for assuming articles</param>
        /// <returns>The reformatted title</returns>
        /// <remarks>
        /// If the language of the title is unknown or if it's multilingual,
        /// pass in Language.English for standardized coverage.
        /// </remarks>
        public static string NormalizeDiscTitle(string title, Language language)
        {
            // If we have an invalid title, just return it as-is
            if (string.IsNullOrWhiteSpace(title))
                return title;

            // Get the title split into parts
            string[] splitTitle = title.Split(' ').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            // If we only have one part, we can't do anything
            if (splitTitle.Length <= 1)
                return title;

            // Determine if we have a definite or indefinite article as the first item
            string firstItem = splitTitle[0];
            switch (firstItem.ToLowerInvariant())
            {
                // Latin script articles
                case "'n"
                    when language is Language.Manx:
                case "a"
                    when language is Language.English
                        || language is Language.Hungarian
                        || language is Language.Portuguese
                        || language is Language.Scots:
                case "a'"
                    when language is Language.English
                        || language is Language.Hungarian
                        || language is Language.Irish
                        || language is Language.Gaelic:     // Scottish Gaelic
                case "al"
                    when language is Language.Breton:
                case "am"
                    when language is Language.Gaelic:       // Scottish Gaelic
                case "an"
                    when language is Language.Breton
                        || language is Language.Cornish
                        || language is Language.English
                        || language is Language.Irish
                        || language is Language.Gaelic:     // Scottish Gaelic
                case "anek"
                    when language is Language.Nepali:
                case "ar"
                    when language is Language.Breton:
                case "az"
                    when language is Language.Hungarian:
                case "ān"
                    when language is Language.Persian:
                case "as"
                    when language is Language.Portuguese:
                case "d'"
                    when language is Language.Luxembourgish:
                case "das"
                    when language is Language.German:
                case "dat"
                    when language is Language.Luxembourgish:
                case "de"
                    when language is Language.Dutch:
                case "déi"
                    when language is Language.Luxembourgish:
                case "dem"
                    when language is Language.German
                        || language is Language.Luxembourgish:
                case "den"
                    when language is Language.Dutch
                        || language is Language.German
                        || language is Language.Luxembourgish:
                case "der"
                    when language is Language.Dutch
                        || language is Language.German
                        || language is Language.Luxembourgish:
                case "des"
                    when language is Language.Dutch
                        || language is Language.French
                        || language is Language.German:
                case "die"
                    when language is Language.Afrikaans
                        || language is Language.German:
                case "e"
                    when language is Language.Papiamento:
                case "een"
                    when language is Language.Dutch:
                case "egy"
                    when language is Language.Hungarian:
                case "ei"
                    when language is Language.Norwegian:
                case "ein"
                    when language is Language.German
                        || language is Language.Norwegian:
                case "eine"
                    when language is Language.German:
                case "einem"
                    when language is Language.German:
                case "einen"
                    when language is Language.German:
                case "einer"
                    when language is Language.German:
                case "eines"
                    when language is Language.German:
                case "eit"
                    when language is Language.Norwegian:
                case "ek"
                    when language is Language.Nepali:
                case "el"
                    when language is Language.Arabic
                        || language is Language.Catalan
                        || language is Language.Spanish:
                case "els"
                    when language is Language.Catalan:
                case "en"
                    when language is Language.Danish
                        || language is Language.Luxembourgish
                        || language is Language.Norwegian
                        || language is Language.Swedish:
                case "eng"
                    when language is Language.Luxembourgish:
                case "engem"
                    when language is Language.Luxembourgish:
                case "enger"
                    when language is Language.Luxembourgish:
                case "es"
                    when language is Language.Catalan:
                case "et"
                    when language is Language.Danish
                        || language is Language.Norwegian:
                case "ett"
                    when language is Language.Swedish:
                case "euta"
                    when language is Language.Nepali:
                case "euti"
                    when language is Language.Nepali:
                case "gli"
                    when language is Language.Italian:
                case "he"
                    when language is Language.Hawaiian
                        || language is Language.Maori:
                case "het"
                    when language is Language.Dutch:
                case "i"
                    when language is Language.Italian
                        || language is Language.Khasi:
                case "il"
                    when language is Language.Italian:
                case "in"
                    when language is Language.Persian:
                case "ka"
                    when language is Language.Hawaiian
                        || language is Language.Khasi:
                case "ke"
                    when language is Language.Hawaiian:
                case "ki"
                    when language is Language.Khasi:
                case "kunai"
                    when language is Language.Nepali:
                case "l'"
                    when language is Language.Catalan
                        || language is Language.French
                        || language is Language.Italian:
                case "la"
                    when language is Language.Catalan
                        || language is Language.Esperanto
                        || language is Language.French
                        || language is Language.Italian
                        || language is Language.Spanish:
                case "las"
                    when language is Language.Spanish:
                case "le"
                    when language is Language.French
                        || language is Language.Interlingua
                        || language is Language.Italian:
                case "les"
                    when language is Language.Catalan
                        || language is Language.French:
                case "lo"
                    when language is Language.Catalan
                        || language is Language.Italian
                        || language is Language.Spanish:
                case "los"
                    when language is Language.Catalan
                        || language is Language.Spanish:
                case "na"
                    when language is Language.Irish
                        || language is Language.Gaelic:     // Scottish Gaelic
                case "nam"
                    when language is Language.Gaelic:       // Scottish Gaelic
                case "nan"
                    when language is Language.Gaelic:       // Scottish Gaelic
                case "nā"
                    when language is Language.Hawaiian:
                case "ngā"
                    when language is Language.Maori:
                case "niște"
                    when language is Language.Romanian:
                case "ny"
                    when language is Language.Manx:
                case "o"
                    when language is Language.Portuguese
                        || language is Language.Romanian:
                case "os"
                    when language is Language.Portuguese:
                case "sa"
                    when language is Language.Catalan:
                case "sang"
                    when language is Language.Malay:
                case "se"
                    when language is Language.Finnish:
                case "ses"
                    when language is Language.Catalan:
                case "si"
                    when language is Language.Malay:
                case "te"
                    when language is Language.Maori:
                case "the"
                    when language is Language.English
                        || language is Language.Scots:
                case "u"
                    when language is Language.Khasi:
                case "ul"
                    when language is Language.Breton:
                case "um"
                    when language is Language.Portuguese:
                case "uma"
                    when language is Language.Portuguese:
                case "umas"
                    when language is Language.Portuguese:
                case "un"
                    when language is Language.Breton
                        || language is Language.Catalan
                        || language is Language.French
                        || language is Language.Interlingua
                        || language is Language.Italian
                        || language is Language.Papiamento
                        || language is Language.Romanian
                        || language is Language.Spanish:
                case "un'"
                    when language is Language.Italian:
                case "una"
                    when language is Language.Catalan
                        || language is Language.Italian:
                case "unas"
                    when language is Language.Spanish:
                case "une"
                    when language is Language.French:
                case "uno"
                    when language is Language.Italian:
                case "unos"
                    when language is Language.Spanish:
                case "uns"
                    when language is Language.Catalan
                        || language is Language.Portuguese:
                case "unei"
                    when language is Language.Romanian:
                case "unes"
                    when language is Language.Catalan:
                case "unor"
                    when language is Language.Romanian:
                case "unui"
                    when language is Language.Romanian:
                case "ur"
                    when language is Language.Breton:
                case "y"
                    when language is Language.Manx
                        || language is Language.Welsh:
                case "ye"
                    when language is Language.Persian:
                case "yek"
                    when language is Language.Persian:
                case "yn"
                    when language is Language.Manx:
                case "yr"
                    when language is Language.Welsh:

                // Non-latin script articles
                case "ο"
                    when language is Language.Greek:
                case "η"
                    when language is Language.Greek:
                case "το"
                    when language is Language.Greek:
                case "οι"
                    when language is Language.Greek:
                case "τα"
                    when language is Language.Greek:
                case "ένας"
                    when language is Language.Greek:
                case "μια"
                    when language is Language.Greek:
                case "ένα"
                    when language is Language.Greek:
                case "еден"
                    when language is Language.Macedonian:
                case "една"
                    when language is Language.Macedonian:
                case "едно"
                    when language is Language.Macedonian:
                case "едни"
                    when language is Language.Macedonian:
                case "एउटा"
                    when language is Language.Nepali:
                case "एउटी"
                    when language is Language.Nepali:
                case "एक"
                    when language is Language.Nepali:
                case "अनेक"
                    when language is Language.Nepali:
                case "कुनै"
                    when language is Language.Nepali:
                case "דער"
                    when language is Language.Yiddish:
                case "די"
                    when language is Language.Yiddish:
                case "דאָס"
                    when language is Language.Yiddish:
                case "דעם"
                    when language is Language.Yiddish:
                case "אַ"
                    when language is Language.Yiddish:
                case "אַן"
                    when language is Language.Yiddish:

                // Seen by Redump, unknown origin
                case "du":
                    break;

                // Otherwise, just return it as-is
                default:
                    return title;
            }

            // Insert the first item if we have a `:` or `-`
            bool itemInserted = false;
            var newTitleBuilder = new StringBuilder();
            for (int i = 1; i < splitTitle.Length; i++)
            {
                string segment = splitTitle[i];
                if (segment.EndsWith(":") || segment.EndsWith("-"))
                {
                    itemInserted = true;
                    newTitleBuilder.Append($"{segment}, {firstItem}");
                }
                else
                {
                    newTitleBuilder.Append($"{segment} ");
                }
            }

            // If we didn't insert the item yet, add it to the end
            string newTitle = newTitleBuilder.ToString().Trim();
            if (!itemInserted)
                newTitle = $"{newTitle}, {firstItem}";

            return newTitle;
        }

        // Moved to RedumpLib
        /// <summary>
        /// Adjust the disc type based on size and layerbreak information
        /// </summary>
        /// <param name="info">Existing SubmissionInfo object to fill</param>
        /// <returns>Corrected disc type, if possible</returns>
        public static void NormalizeDiscType(SubmissionInfo info)
        {
            // If we have nothing valid, do nothing
            if (info?.CommonDiscInfo?.Media == null || info?.SizeAndChecksums == null)
                return;

            switch (info.CommonDiscInfo.Media)
            {
                case DiscType.BD25:
                case DiscType.BD33:
                case DiscType.BD50:
                case DiscType.BD66:
                case DiscType.BD100:
                case DiscType.BD128:
                    if (info.SizeAndChecksums.Layerbreak3 != default)
                        info.CommonDiscInfo.Media = DiscType.BD128;
                    else if (info.SizeAndChecksums.Layerbreak2 != default)
                        info.CommonDiscInfo.Media = DiscType.BD100;
                    else if (info.SizeAndChecksums.Layerbreak != default && info.SizeAndChecksums.PICIdentifier == SabreTools.Models.PIC.Constants.DiscTypeIdentifierROMUltra)
                        info.CommonDiscInfo.Media = DiscType.BD66;
                    else if (info.SizeAndChecksums.Layerbreak != default && info.SizeAndChecksums.Size > 50_050_629_632)
                        info.CommonDiscInfo.Media = DiscType.BD66;
                    else if (info.SizeAndChecksums.Layerbreak != default)
                        info.CommonDiscInfo.Media = DiscType.BD50;
                    else if (info.SizeAndChecksums.PICIdentifier == SabreTools.Models.PIC.Constants.DiscTypeIdentifierROMUltra)
                        info.CommonDiscInfo.Media = DiscType.BD33;
                    else if (info.SizeAndChecksums.Size > 25_025_314_816)
                        info.CommonDiscInfo.Media = DiscType.BD33;
                    else
                        info.CommonDiscInfo.Media = DiscType.BD25;
                    break;

                case DiscType.DVD5:
                case DiscType.DVD9:
                    if (info.SizeAndChecksums.Layerbreak != default)
                        info.CommonDiscInfo.Media = DiscType.DVD9;
                    else
                        info.CommonDiscInfo.Media = DiscType.DVD5;
                    break;

                case DiscType.HDDVDSL:
                case DiscType.HDDVDDL:
                    if (info.SizeAndChecksums.Layerbreak != default)
                        info.CommonDiscInfo.Media = DiscType.HDDVDDL;
                    else
                        info.CommonDiscInfo.Media = DiscType.HDDVDSL;
                    break;

                case DiscType.UMDSL:
                case DiscType.UMDDL:
                    if (info.SizeAndChecksums.Layerbreak != default)
                        info.CommonDiscInfo.Media = DiscType.UMDDL;
                    else
                        info.CommonDiscInfo.Media = DiscType.UMDSL;
                    break;

                // All other disc types are not processed
                default:
                    break;
            }
        }

        /// <summary>
        /// Normalize a split set of paths
        /// </summary>
        /// <param name="path">Path value to normalize</param>
        public static string NormalizeOutputPaths(string? path, bool getFullPath)
        {
            // The easy way
            try
            {
                // If we have an invalid path
                if (string.IsNullOrWhiteSpace(path))
                    return string.Empty;

                // Remove quotes from path
                path = path!.Replace("\"", string.Empty);

                // Try getting the combined path and returning that directly
                string fullPath = getFullPath ? Path.GetFullPath(path) : path;
                var fullDirectory = Path.GetDirectoryName(fullPath);
                string fullFile = Path.GetFileName(fullPath);

                // Remove invalid path characters
                if (fullDirectory != null)
                {
                    foreach (char c in Path.GetInvalidPathChars())
                        fullDirectory = fullDirectory.Replace(c, '_');
                }

                // Remove invalid filename characters
                foreach (char c in Path.GetInvalidFileNameChars())
                    fullFile = fullFile.Replace(c, '_');

                if (string.IsNullOrWhiteSpace(fullDirectory))
                    return fullFile;
                else
                    return Path.Combine(fullDirectory, fullFile);
            }
            catch { }

            return path ?? string.Empty;
        }

        #endregion

        #region Helpers

        // Moved to RedumpLib
        /// <summary>
        /// Format a single site tag to string
        /// </summary>
        /// <param name="kvp">KeyValuePair representing the site tag and value</param>
        /// <returns>String-formatted tag and value</returns>
        private static string FormatSiteTag(KeyValuePair<SiteCode?, string> kvp)
        {
            bool isMultiLine = SubmissionInfoTool.IsMultiLine(kvp.Key);
            string line = $"{kvp.Key.ShortName()}{(isMultiLine ? "\n" : " ")}";

            // Special case for boolean fields
            if (IsBoolean(kvp.Key))
            {
                if (kvp.Value != true.ToString())
                    return string.Empty;

                return line.Trim();
            }

            return $"{line}{kvp.Value}{(isMultiLine ? "\n" : string.Empty)}";
        }

        // Moved to RedumpLib
        /// <summary>
        /// Check if a site code is boolean or not
        /// </summary>
        /// <param name="siteCode">SiteCode to check</param>
        /// <returns>True if the code field is a flag with no value, false otherwise</returns>
        /// <remarks>TODO: This should move to Extensions at some point</remarks>
        private static bool IsBoolean(SiteCode? siteCode)
        {
            return siteCode switch
            {
                SiteCode.PostgapType => true,
                SiteCode.VCD => true,
                _ => false,
            };
        }

        // Moved to RedumpLib
        /// <summary>
        /// Order comment code tags according to Redump requirements
        /// </summary>
        /// <returns>Ordered list of KeyValuePairs representing the tags and values</returns>
        private static List<KeyValuePair<SiteCode?, string>> OrderCommentTags(Dictionary<SiteCode, string> tags)
        {
            var sorted = new List<KeyValuePair<SiteCode?, string>>();

            // If the input is invalid, just return an empty set
            if (tags == null || tags.Count == 0)
                return sorted;

            // Identifying Info
            if (tags.ContainsKey(SiteCode.AlternativeTitle))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.AlternativeTitle, tags[SiteCode.AlternativeTitle]));
            if (tags.ContainsKey(SiteCode.AlternativeForeignTitle))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.AlternativeForeignTitle, tags[SiteCode.AlternativeForeignTitle]));
            if (tags.ContainsKey(SiteCode.InternalName))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.InternalName, tags[SiteCode.InternalName]));
            if (tags.ContainsKey(SiteCode.InternalSerialName))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.InternalSerialName, tags[SiteCode.InternalSerialName]));
            if (tags.ContainsKey(SiteCode.VolumeLabel))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.VolumeLabel, tags[SiteCode.VolumeLabel]));
            if (tags.ContainsKey(SiteCode.Multisession))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.Multisession, tags[SiteCode.Multisession]));
            if (tags.ContainsKey(SiteCode.UniversalHash))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.UniversalHash, tags[SiteCode.UniversalHash]));
            if (tags.ContainsKey(SiteCode.RingNonZeroDataStart))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.RingNonZeroDataStart, tags[SiteCode.RingNonZeroDataStart]));

            if (tags.ContainsKey(SiteCode.XMID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.XMID, tags[SiteCode.XMID]));
            if (tags.ContainsKey(SiteCode.XeMID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.XeMID, tags[SiteCode.XeMID]));
            if (tags.ContainsKey(SiteCode.DMIHash))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.DMIHash, tags[SiteCode.DMIHash]));
            if (tags.ContainsKey(SiteCode.PFIHash))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.PFIHash, tags[SiteCode.PFIHash]));
            if (tags.ContainsKey(SiteCode.SSHash))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.SSHash, tags[SiteCode.SSHash]));
            if (tags.ContainsKey(SiteCode.SSVersion))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.SSVersion, tags[SiteCode.SSVersion]));

            if (tags.ContainsKey(SiteCode.Filename))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.Filename, tags[SiteCode.Filename]));

            if (tags.ContainsKey(SiteCode.BBFCRegistrationNumber))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.BBFCRegistrationNumber, tags[SiteCode.BBFCRegistrationNumber]));
            if (tags.ContainsKey(SiteCode.CDProjektID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.CDProjektID, tags[SiteCode.CDProjektID]));
            if (tags.ContainsKey(SiteCode.DiscHologramID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.DiscHologramID, tags[SiteCode.DiscHologramID]));
            if (tags.ContainsKey(SiteCode.DNASDiscID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.DNASDiscID, tags[SiteCode.DNASDiscID]));
            if (tags.ContainsKey(SiteCode.ISBN))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.ISBN, tags[SiteCode.ISBN]));
            if (tags.ContainsKey(SiteCode.ISSN))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.ISSN, tags[SiteCode.ISSN]));
            if (tags.ContainsKey(SiteCode.PPN))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.PPN, tags[SiteCode.PPN]));
            if (tags.ContainsKey(SiteCode.VFCCode))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.VFCCode, tags[SiteCode.VFCCode]));

            if (tags.ContainsKey(SiteCode.Genre))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.Genre, tags[SiteCode.Genre]));
            if (tags.ContainsKey(SiteCode.Series))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.Series, tags[SiteCode.Series]));
            if (tags.ContainsKey(SiteCode.PostgapType))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.PostgapType, tags[SiteCode.PostgapType]));
            if (tags.ContainsKey(SiteCode.VCD))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.VCD, tags[SiteCode.VCD]));

            // Publisher / Company IDs
            if (tags.ContainsKey(SiteCode.AcclaimID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.AcclaimID, tags[SiteCode.AcclaimID]));
            if (tags.ContainsKey(SiteCode.ActivisionID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.ActivisionID, tags[SiteCode.ActivisionID]));
            if (tags.ContainsKey(SiteCode.BandaiID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.BandaiID, tags[SiteCode.BandaiID]));
            if (tags.ContainsKey(SiteCode.ElectronicArtsID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.ElectronicArtsID, tags[SiteCode.ElectronicArtsID]));
            if (tags.ContainsKey(SiteCode.FoxInteractiveID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.FoxInteractiveID, tags[SiteCode.FoxInteractiveID]));
            if (tags.ContainsKey(SiteCode.GTInteractiveID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.GTInteractiveID, tags[SiteCode.GTInteractiveID]));
            if (tags.ContainsKey(SiteCode.JASRACID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.JASRACID, tags[SiteCode.JASRACID]));
            if (tags.ContainsKey(SiteCode.KingRecordsID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.KingRecordsID, tags[SiteCode.KingRecordsID]));
            if (tags.ContainsKey(SiteCode.KoeiID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.KoeiID, tags[SiteCode.KoeiID]));
            if (tags.ContainsKey(SiteCode.KonamiID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.KonamiID, tags[SiteCode.KonamiID]));
            if (tags.ContainsKey(SiteCode.LucasArtsID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.LucasArtsID, tags[SiteCode.LucasArtsID]));
            if (tags.ContainsKey(SiteCode.MicrosoftID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.MicrosoftID, tags[SiteCode.MicrosoftID]));
            if (tags.ContainsKey(SiteCode.NaganoID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.NaganoID, tags[SiteCode.NaganoID]));
            if (tags.ContainsKey(SiteCode.NamcoID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.NamcoID, tags[SiteCode.NamcoID]));
            if (tags.ContainsKey(SiteCode.NipponIchiSoftwareID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.NipponIchiSoftwareID, tags[SiteCode.NipponIchiSoftwareID]));
            if (tags.ContainsKey(SiteCode.OriginID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.OriginID, tags[SiteCode.OriginID]));
            if (tags.ContainsKey(SiteCode.PonyCanyonID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.PonyCanyonID, tags[SiteCode.PonyCanyonID]));
            if (tags.ContainsKey(SiteCode.SegaID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.SegaID, tags[SiteCode.SegaID]));
            if (tags.ContainsKey(SiteCode.SelenID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.SelenID, tags[SiteCode.SelenID]));
            if (tags.ContainsKey(SiteCode.SierraID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.SierraID, tags[SiteCode.SierraID]));
            if (tags.ContainsKey(SiteCode.TaitoID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.TaitoID, tags[SiteCode.TaitoID]));
            if (tags.ContainsKey(SiteCode.UbisoftID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.UbisoftID, tags[SiteCode.UbisoftID]));
            if (tags.ContainsKey(SiteCode.ValveID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.ValveID, tags[SiteCode.ValveID]));

            return sorted;
        }

        // Moved to RedumpLib
        /// <summary>
        /// Order content code tags according to Redump requirements
        /// </summary>
        /// <returns>Ordered list of KeyValuePairs representing the tags and values</returns>
        private static List<KeyValuePair<SiteCode?, string>> OrderContentTags(Dictionary<SiteCode, string> tags)
        {
            var sorted = new List<KeyValuePair<SiteCode?, string>>();

            // If the input is invalid, just return an empty set
            if (tags == null || tags.Count == 0)
                return sorted;

            // Games
            if (tags.ContainsKey(SiteCode.Games))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.Games, tags[SiteCode.Games]));
            if (tags.ContainsKey(SiteCode.NetYarozeGames))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.NetYarozeGames, tags[SiteCode.NetYarozeGames]));

            // Demos
            if (tags.ContainsKey(SiteCode.PlayableDemos))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.PlayableDemos, tags[SiteCode.PlayableDemos]));
            if (tags.ContainsKey(SiteCode.RollingDemos))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.RollingDemos, tags[SiteCode.RollingDemos]));
            if (tags.ContainsKey(SiteCode.TechDemos))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.TechDemos, tags[SiteCode.TechDemos]));

            // Video
            if (tags.ContainsKey(SiteCode.GameFootage))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.GameFootage, tags[SiteCode.GameFootage]));
            if (tags.ContainsKey(SiteCode.Videos))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.Videos, tags[SiteCode.Videos]));

            // Miscellaneous
            if (tags.ContainsKey(SiteCode.Patches))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.Patches, tags[SiteCode.Patches]));
            if (tags.ContainsKey(SiteCode.Savegames))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.Savegames, tags[SiteCode.Savegames]));
            if (tags.ContainsKey(SiteCode.Extras))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.Extras, tags[SiteCode.Extras]));

            return sorted;
        }

        #endregion
    }
}
