using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BinaryObjectScanner;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SabreTools.IO;
using SabreTools.RedumpLib.Data;
using Formatting = Newtonsoft.Json.Formatting;

#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

namespace MPF.Core.Utilities
{
    public static class InfoTool
    {
        #region Information Extraction

        /// <summary>
        /// Get the current detected copy protection(s), if possible
        /// </summary>
        /// <param name="drive">Drive object representing the current drive</param>
        /// <param name="options">Options object that determines what to scan</param>
        /// <param name="progress">Optional progress callback</param>
        /// <returns>Detected copy protection(s) if possible, null on error</returns>
        public static async Task<(string?, Dictionary<string, List<string>>?)> GetCopyProtection(Drive? drive,
            Core.Options options,
            IProgress<ProtectionProgress>? progress = null)
        {
            if (options.ScanForProtection && drive?.Name != null)
            {
                (var protection, _) = await ProtectionTool.RunProtectionScanOnPath(drive.Name, options, progress);
                return (ProtectionTool.FormatProtections(protection), protection);
            }

            return ("(CHECK WITH PROTECTIONID)", null);
        }

        /// <summary>
        /// Get the EXE name from a PlayStation disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>Executable name on success, null otherwise</returns>
        public static string? GetPlayStationExecutableName(char? driveLetter)
        {
            // If there's no drive letter, we can't get exe name
            if (driveLetter == null)
                return null;

            // Convert drive letter to drive path
            string drivePath = driveLetter + ":\\";
            return GetPlayStationExecutableName(drivePath);
        }

        /// <summary>
        /// Get the EXE name from a PlayStation disc, if possible
        /// </summary>
        /// <param name="drivePath">Drive path to use to check</param>
        /// <returns>Executable name on success, null otherwise</returns>
        public static string? GetPlayStationExecutableName(string? drivePath)
        {
            // If there's no drive path, we can't get exe name
            if (string.IsNullOrEmpty(drivePath))
                return null;

            // If the folder no longer exists, we can't get exe name
            if (!Directory.Exists(drivePath))
                return null;

            // Get the two paths that we will need to check
            string psxExePath = Path.Combine(drivePath, "PSX.EXE");
            string systemCnfPath = Path.Combine(drivePath, "SYSTEM.CNF");

            // Read the CNF file as an INI file
            var systemCnf = new IniFile(systemCnfPath);
            string? bootValue = string.Empty;

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
                    string? serial = match.Groups[1].Value;

                    // Some games may have the EXE in a subfolder
                    serial = Path.GetFileName(serial);

                    return serial;
                }
            }

            // If the SYSTEM.CNF value can't be found, try PSX.EXE
            if (File.Exists(psxExePath))
                return "PSX.EXE";

            // If neither can be found, we return null
            return null;
        }

        /// <summary>
        /// Get the EXE date from a PlayStation disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <param name="serial">Internal disc serial, if possible</param>
        /// <param name="region">Output region, if possible</param>
        /// <param name="date">Output EXE date in "yyyy-mm-dd" format if possible, null on error</param>
        /// <returns>True if information could be determined, false otherwise</returns>
        public static bool GetPlayStationExecutableInfo(char? driveLetter, out string? serial, out Region? region, out string? date)
        {
            serial = null; region = null; date = null;

            // If there's no drive letter, we can't do this part
            if (driveLetter == null)
                return false;

            // Convert drive letter to drive path
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
        /// <returns>True if information could be determined, false otherwise</returns>
        public static bool GetPlayStationExecutableInfo(string? drivePath, out string? serial, out Region? region, out string? date)
        {
            serial = null; region = null; date = null;

            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(drivePath))
                return false;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drivePath))
                return false;

            // Get the executable name
            string? exeName = GetPlayStationExecutableName(drivePath);

            // If no executable found, we can't do this part
            if (exeName == null)
                return false;

            // EXE name may have a trailing `;` after
            // EXE name should always be in all caps
            exeName = exeName
                .Split(';')[0]
                .ToUpperInvariant();

            // Serial is most of the EXE name normalized
            serial = exeName
                .Replace('_', '-')
                .Replace(".", string.Empty);

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
        public static string? GetPlayStation2Version(char? driveLetter)
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
        public static string? GetPlayStation2Version(string? drivePath)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(drivePath))
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
        public static string? GetPlayStation3Serial(char? driveLetter)
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
        public static string? GetPlayStation3Serial(string? drivePath)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(drivePath))
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
#if NET20 || NET35
            string sfoPath = Path.Combine(Path.Combine(drivePath, "PS3_GAME"), "PARAM.SFO");
#else
            string sfoPath = Path.Combine(drivePath, "PS3_GAME", "PARAM.SFO");
#endif
            if (File.Exists(sfoPath))
            {
                try
                {
                    using var br = new BinaryReader(File.OpenRead(sfoPath));
                    br.BaseStream.Seek(-0x18, SeekOrigin.End);
                    return new string(br.ReadChars(9)).TrimEnd('\0').Insert(4, "-");
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
        public static string? GetPlayStation3Version(char? driveLetter)
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
        public static string? GetPlayStation3Version(string? drivePath)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(drivePath))
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
                    if (!string.IsNullOrEmpty(discVersion))
                        return discVersion;
                }
                catch
                {
                    // We don't care what the error was
                    return null;
                }
            }

            // Attempt to use PARAM.SFO
#if NET20 || NET35
            string sfoPath = Path.Combine(Path.Combine(drivePath, "PS3_GAME"), "PARAM.SFO");
#else
            string sfoPath = Path.Combine(drivePath, "PS3_GAME", "PARAM.SFO");
#endif
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
        public static string? GetPlayStation3FirmwareVersion(char? driveLetter)
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
        public static string? GetPlayStation3FirmwareVersion(string? drivePath)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(drivePath))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drivePath))
                return null;

            // Attempt to read from /PS3_UPDATE/PS3UPDAT.PUP
#if NET20 || NET35
            string pupPath = Path.Combine(Path.Combine(drivePath, "PS3_UPDATE"), "PS3UPDAT.PUP");
#else
            string pupPath = Path.Combine(drivePath, "PS3_UPDATE", "PS3UPDAT.PUP");
#endif
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
        public static string? GetPlayStation4Serial(char? driveLetter)
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
        public static string? GetPlayStation4Serial(string? drivePath)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(drivePath))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drivePath))
                return null;

            // If we can't find param.sfo, we don't have a PlayStation 4 disc
#if NET20 || NET35
            string paramSfoPath = Path.Combine(Path.Combine(drivePath, "bd"), "param.sfo");
#else
            string paramSfoPath = Path.Combine(drivePath, "bd", "param.sfo");
#endif
            if (!File.Exists(paramSfoPath))
                return null;

            // Let's try reading param.sfo to find the serial at the end of the file
            try
            {
                using var br = new BinaryReader(File.OpenRead(paramSfoPath));
                br.BaseStream.Seek(-0x14, SeekOrigin.End);
                return new string(br.ReadChars(9)).Insert(4, "-");
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
        public static string? GetPlayStation4Version(char? driveLetter)
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
        public static string? GetPlayStation4Version(string? drivePath)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(drivePath))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drivePath))
                return null;

            // If we can't find param.sfo, we don't have a PlayStation 4 disc
#if NET20 || NET35
            string paramSfoPath = Path.Combine(Path.Combine(drivePath, "bd"), "param.sfo");
#else
            string paramSfoPath = Path.Combine(drivePath, "bd", "param.sfo");
#endif
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
        public static string? GetPlayStation5Serial(char? driveLetter)
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
        public static string? GetPlayStation5Serial(string? drivePath)
        {
            // Attempt to get the param.json file
            var json = GetPlayStation5ParamsJsonFromDrive(drivePath);
            if (json == null)
                return null;

            try
            {
                return json["disc"]?[0]?["masterDataId"]?.Value<string>()?.Insert(4, "-");
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
        public static string? GetPlayStation5Version(char? driveLetter)
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
        public static string? GetPlayStation5Version(string? drivePath)
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
            if (string.IsNullOrEmpty(drivePath))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drivePath))
                return null;

            // If we can't find param.json, we don't have a PlayStation 5 disc
#if NET20 || NET35
            string paramJsonPath = Path.Combine(Path.Combine(drivePath, "bd"), "param.json");
#else
            string paramJsonPath = Path.Combine(drivePath, "bd", "param.json");
#endif
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
            if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
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

        /// <summary>
        /// Get the current system from the drive volume label
        /// </summary>
        /// <returns>The system based on volume label, null if none detected</returns>
        public static RedumpSystem? GetRedumpSystemFromVolumeLabel(string? volumeLabel)
        {
            // If the volume label is empty, we can't do anything
            if (string.IsNullOrEmpty(volumeLabel))
                return null;

            // Audio CD
            if (volumeLabel!.Equals("Audio CD", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.AudioCD;

            // Microsoft Xbox
            if (volumeLabel.Equals("SEP13011042", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.MicrosoftXbox;
            else if (volumeLabel.Equals("SEP13011042072", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.MicrosoftXbox;

            // Microsoft Xbox 360
            if (volumeLabel.Equals("XBOX360", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.MicrosoftXbox360;
            else if (volumeLabel.Equals("XGD2DVD_NTSC", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.MicrosoftXbox360;

            // Microsoft Xbox 360 - Too overly broad even if a lot of discs use this
            //if (volumeLabel.Equals("CD_ROM", StringComparison.OrdinalIgnoreCase))
            //    return RedumpSystem.MicrosoftXbox360; // Also for Xbox One?
            //if (volumeLabel.Equals("DVD_ROM", StringComparison.OrdinalIgnoreCase))
            //    return RedumpSystem.MicrosoftXbox360;

            // Sega Mega-CD / Sega-CD
            if (volumeLabel.Equals("Sega_CD", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.SegaMegaCDSegaCD;

            // Sony PlayStation 3
            if (volumeLabel.Equals("PS3VOLUME", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.SonyPlayStation3;

            // Sony PlayStation 4
            if (volumeLabel.Equals("PS4VOLUME", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.SonyPlayStation4;

            // Sony PlayStation 5
            if (volumeLabel.Equals("PS5VOLUME", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.SonyPlayStation5;

            return null;
        }

        #endregion

        #region Region Extraction

        /// <summary>
        /// Determine the region based on the PlayStation serial code
        /// </summary>
        /// <param name="serial">PlayStation serial code</param>
        /// <returns>Region mapped from name, if possible</returns>
        public static Region? GetPlayStationRegion(string? serial)
        {
            // If we have a fully invalid serial
            if (string.IsNullOrEmpty(serial))
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

        #endregion

        #region Information Output

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
                if (string.IsNullOrEmpty(outputDirectory) && string.IsNullOrEmpty(filenameSuffix))
                    path = "!submissionInfo.txt";
                else if (string.IsNullOrEmpty(outputDirectory) && !string.IsNullOrEmpty(filenameSuffix))
                    path = $"!submissionInfo_{filenameSuffix}.txt";
                else if (!string.IsNullOrEmpty(outputDirectory) && string.IsNullOrEmpty(filenameSuffix))
                    path = Path.Combine(outputDirectory, "!submissionInfo.txt");
                else if (!string.IsNullOrEmpty(outputDirectory) && !string.IsNullOrEmpty(filenameSuffix))
                    path = Path.Combine(outputDirectory, $"!submissionInfo_{filenameSuffix}.txt");

                using var sw = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.Write), Encoding.UTF8);
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

        // MOVE TO REDUMPLIB
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
                    if (string.IsNullOrEmpty(outputDirectory) && string.IsNullOrEmpty(filenameSuffix))
                        path = "!submissionInfo.json.gz";
                    else if (string.IsNullOrEmpty(outputDirectory) && !string.IsNullOrEmpty(filenameSuffix))
                        path = $"!submissionInfo_{filenameSuffix}.json.gz";
                    else if (!string.IsNullOrEmpty(outputDirectory) && string.IsNullOrEmpty(filenameSuffix))
                        path = Path.Combine(outputDirectory, "!submissionInfo.json.gz");
                    else if (!string.IsNullOrEmpty(outputDirectory) && !string.IsNullOrEmpty(filenameSuffix))
                        path = Path.Combine(outputDirectory, $"!submissionInfo_{filenameSuffix}.json.gz");

                    using var fs = File.Create(path);
                    using var gs = new GZipStream(fs, CompressionMode.Compress);
                    gs.Write(jsonBytes, 0, jsonBytes.Length);
                }

                // Otherwise, write out to a normal JSON
                else
                {
                    var path = string.Empty;
                    if (string.IsNullOrEmpty(outputDirectory) && string.IsNullOrEmpty(filenameSuffix))
                        path = "!submissionInfo.json";
                    else if (string.IsNullOrEmpty(outputDirectory) && !string.IsNullOrEmpty(filenameSuffix))
                        path = $"!submissionInfo_{filenameSuffix}.json";
                    else if (!string.IsNullOrEmpty(outputDirectory) && string.IsNullOrEmpty(filenameSuffix))
                        path = Path.Combine(outputDirectory, "!submissionInfo.json");
                    else if (!string.IsNullOrEmpty(outputDirectory) && !string.IsNullOrEmpty(filenameSuffix))
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

        // MOVE TO REDUMPLIB
        /// <summary>
        /// Write the protection data to the output folder
        /// </summary>
        /// <param name="outputDirectory">Output folder to write to</param>
        /// <param name="filenameSuffix">Optional suffix to append to the filename</param>
        /// <param name="info">SubmissionInfo object containing the protection information</param>
        /// <param name="hideDriveLetters">True if drive letters are to be removed from output, false otherwise</param>
        /// <returns>True on success, false on error</returns>
        public static bool WriteProtectionData(string? outputDirectory, string? filenameSuffix, SubmissionInfo? info, bool hideDriveLetters)
        {
            // Check to see if the inputs are valid
            if (info?.CopyProtection?.FullProtections == null || !info.CopyProtection.FullProtections.Any())
                return true;

            // Now write out to a generic file
            try
            {
                var path = string.Empty;
                if (string.IsNullOrEmpty(outputDirectory) && string.IsNullOrEmpty(filenameSuffix))
                    path = "!protectionInfo.txt";
                else if (string.IsNullOrEmpty(outputDirectory) && !string.IsNullOrEmpty(filenameSuffix))
                    path = $"!protectionInfo{filenameSuffix}.txt";
                else if (!string.IsNullOrEmpty(outputDirectory) && string.IsNullOrEmpty(filenameSuffix))
                    path = Path.Combine(outputDirectory, "!protectionInfo.txt");
                else if (!string.IsNullOrEmpty(outputDirectory) && !string.IsNullOrEmpty(filenameSuffix))
                    path = Path.Combine(outputDirectory, $"!protectionInfo{filenameSuffix}.txt");

                using var sw = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.Write), Encoding.UTF8);

                List<string> sortedKeys = [.. info.CopyProtection.FullProtections.Keys.OrderBy(k => k)];
                foreach (string key in sortedKeys)
                {
                    string scanPath = key;
                    if (hideDriveLetters)
                        scanPath = Path.DirectorySeparatorChar + key.Substring((Path.GetPathRoot(key) ?? String.Empty).Length);

                    List<string>? scanResult = info.CopyProtection.FullProtections[key];

                    if (scanResult == null)
                        sw.WriteLine($"{scanPath}: None");
                    else
                        sw.WriteLine($"{scanPath}: {string.Join(", ", [.. scanResult])}");
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
        /// Create an IRD and write it to the specified output directory with optional filename suffix
        /// </summary>
        /// <param name="outputDirectory">Output folder to write to</param>
        /// <param name="filenameSuffix">Optional suffix to append to the filename</param>
        /// <param name="outputFilename">Output filename to use as the base path</param>
        /// <returns>True on success, false on error</returns>
        public static async Task<(bool, string)> WriteIRD(string isoPath, string? discKeyString, string? discIDString, string? picString, long? layerbreak, string? crc32)
        {
            try
            {
                // Output IRD file path
                string irdPath = Path.ChangeExtension(isoPath, ".ird");

                // Parse disc key from submission info (Required)
                byte[]? discKey = Tools.ParseHexKey(discKeyString);
                if (discKey == null)
                    return (false, "Failed to create IRD: No key provided");

                // Parse Disc ID from submission info (Optional)
                byte[]? discID = Tools.ParseDiscID(discIDString);

                // Parse PIC from submission info (Optional)
                byte[]? pic = Tools.ParsePIC(picString);

                // Parse CRC32 strings into ISO hash for Unique ID field (Optional)
                uint? uid = Tools.ParseCRC32(crc32);

                // Ensure layerbreak value is valid (Optional)
                layerbreak = Tools.ParseLayerbreak(layerbreak);

                // Create Redump-style reproducible IRD
#if NET40
                LibIRD.ReIRD ird = await Task.Factory.StartNew(() =>
#else
                LibIRD.ReIRD ird = await Task.Run(() =>
#endif
                    new LibIRD.ReIRD(isoPath, discKey, layerbreak, uid));
                if (pic != null)
                    ird.PIC = pic;
                if (discID != null && ird.DiscID[15] != 0x00)
                    ird.DiscID = discID;

                // Write IRD to file
                ird.Write(irdPath);

                return (true, "IRD created!");
            }
            catch (Exception)
            {
                // We don't care what the error is
                return (false, "Failed to create IRD");
            }
        }

        #endregion

        #region Normalization

        /// <summary>
        /// Adjust a disc title so that it will be processed correctly by Redump
        /// </summary>
        /// <param name="title">Existing title to potentially reformat</param>
        /// <param name="languages">Array of languages to use for assuming articles</param>
        /// <returns>The reformatted title</returns>
        public static string? NormalizeDiscTitle(string? title, Language?[]? languages)
        {
            // If we have no set languages, then assume English
            if (languages == null || languages.Length == 0)
                languages = [Language.English];

            // Loop through all of the given languages
            foreach (var language in languages)
            {
                // If the new title is different, assume it was normalized and return it
                string? newTitle = NormalizeDiscTitle(title, language);
                if (newTitle != title)
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
        public static string? NormalizeDiscTitle(string? title, Language? language)
        {
            // If we have an invalid title, just return it as-is
            if (string.IsNullOrEmpty(title))
                return title;

            // If we have an invalid language, assume Language.English
            if (language == null)
                language = Language.English;

            // Get the title split into parts
            string[] splitTitle = title!.Split(' ').Where(s => !string.IsNullOrEmpty(s)).ToArray();

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
                case "du"
                    when language is Language.French:
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
                if (!itemInserted && segment == ":")
                {
                    itemInserted = true;
                    newTitleBuilder.Append($", {firstItem} :");
                }
                else if (!itemInserted && segment == "-")
                {
                    itemInserted = true;
                    newTitleBuilder.Append($", {firstItem} -");
                }
                else if (!itemInserted && segment.EndsWith(":"))
                {
                    itemInserted = true;
                    newTitleBuilder.Append($" {segment.Substring(0, segment.Length - 1)}, {firstItem}:");
                }
                else if (!itemInserted && segment.EndsWith("-"))
                {
                    itemInserted = true;
                    newTitleBuilder.Append($" {segment.Substring(0, segment.Length - 1)}, {firstItem}-");
                }
                else
                {
                    newTitleBuilder.Append($" {segment}");
                }
            }

            // If we didn't insert the item yet, add it to the end
            string newTitle = newTitleBuilder.ToString().Trim();
            if (!itemInserted)
                newTitle = $"{newTitle}, {firstItem}";

            return newTitle;
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
                if (string.IsNullOrEmpty(path))
                    return string.Empty;

                // Remove quotes and angle brackets from path
                path = path!.Replace("\"", string.Empty);
                path = path!.Replace("<", string.Empty);
                path = path!.Replace(">", string.Empty);

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

                if (string.IsNullOrEmpty(fullDirectory))
                    return fullFile;
                else
                    return Path.Combine(fullDirectory, fullFile);
            }
            catch { }

            return path ?? string.Empty;
        }

        #endregion
    }
}
