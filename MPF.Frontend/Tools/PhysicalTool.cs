using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MPF.Processors;
using SabreTools.IO;
using SabreTools.RedumpLib.Data;

namespace MPF.Frontend.Tools
{
    public static class PhysicalTool
    {
        #region Information Extraction

        /// <summary>
        /// Get the EXE name from a PlayStation disc, if possible
        /// </summary>
        /// <param name="drive">Drive to extract information from</param>
        /// <returns>Executable name on success, null otherwise</returns>
        public static string? GetPlayStationExecutableName(Drive? drive)
        {
            // If there's no drive path, we can't get exe name
            if (string.IsNullOrEmpty(drive?.Name))
                return null;

            // If the folder no longer exists, we can't get exe name
            if (!Directory.Exists(drive!.Name))
                return null;

            // Get the two paths that we will need to check
            string psxExePath = Path.Combine(drive.Name, "PSX.EXE");
            string systemCnfPath = Path.Combine(drive.Name, "SYSTEM.CNF");

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
        /// <param name="drive">Drive to extract information from</param>
        /// <param name="serial">Internal disc serial, if possible</param>
        /// <param name="region">Output region, if possible</param>
        /// <param name="date">Output EXE date in "yyyy-mm-dd" format if possible, null on error</param>
        /// <returns>True if information could be determined, false otherwise</returns>
        public static bool GetPlayStationExecutableInfo(Drive? drive, out string? serial, out Region? region, out string? date)
        {
            serial = null; region = null; date = null;

            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(drive?.Name))
                return false;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drive!.Name))
                return false;

            // Get the executable name
            string? exeName = GetPlayStationExecutableName(drive);

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
            region = ProcessingTool.GetPlayStationRegion(exeName);

            // Now that we have the EXE name, try to get the fileinfo for it
            string exePath = Path.Combine(drive.Name, exeName);
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
        /// <param name="drive">Drive to extract information from</param>
        /// <returns>Game version if possible, null on error</returns>
        public static string? GetPlayStation2Version(Drive? drive)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(drive?.Name))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drive!.Name))
                return null;

            // Get the SYSTEM.CNF path to check
            string systemCnfPath = Path.Combine(drive.Name, "SYSTEM.CNF");

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
        /// <param name="drive">Drive to extract information from</param>
        /// <returns>Internal disc serial if possible, null on error</returns>
        public static string? GetPlayStation3Serial(Drive? drive)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(drive?.Name))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drive!.Name))
                return null;

            // Attempt to use PS3_DISC.SFB
            string sfbPath = Path.Combine(drive.Name, "PS3_DISC.SFB");
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
            string sfoPath = Path.Combine(Path.Combine(drive.Name, "PS3_GAME"), "PARAM.SFO");
#else
            string sfoPath = Path.Combine(drive.Name, "PS3_GAME", "PARAM.SFO");
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
        /// <param name="drive">Drive to extract information from</param>
        /// <returns>Game version if possible, null on error</returns>
        public static string? GetPlayStation3Version(Drive? drive)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(drive?.Name))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drive!.Name))
                return null;

            // Attempt to use PS3_DISC.SFB
            string sfbPath = Path.Combine(drive.Name, "PS3_DISC.SFB");
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
            string sfoPath = Path.Combine(Path.Combine(drive.Name, "PS3_GAME"), "PARAM.SFO");
#else
            string sfoPath = Path.Combine(drive.Name, "PS3_GAME", "PARAM.SFO");
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
        /// <param name="drive">Drive to extract information from</param>
        /// <returns>Firmware version if possible, null on error</returns>
        public static string? GetPlayStation3FirmwareVersion(Drive? drive)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(drive?.Name))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drive!.Name))
                return null;

            // Attempt to read from /PS3_UPDATE/PS3UPDAT.PUP
#if NET20 || NET35
            string pupPath = Path.Combine(Path.Combine(drive.Name, "PS3_UPDATE"), "PS3UPDAT.PUP");
#else
            string pupPath = Path.Combine(drive.Name, "PS3_UPDATE", "PS3UPDAT.PUP");
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
        /// <param name="drive">Drive to extract information from</param>
        /// <returns>Internal disc serial if possible, null on error</returns>
        public static string? GetPlayStation4Serial(Drive? drive)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(drive?.Name))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drive!.Name))
                return null;

            // If we can't find param.sfo, we don't have a PlayStation 4 disc
#if NET20 || NET35
            string paramSfoPath = Path.Combine(Path.Combine(drive.Name, "bd"), "param.sfo");
#else
            string paramSfoPath = Path.Combine(drive.Name, "bd", "param.sfo");
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
        /// <param name="drive">Drive to extract information from</param>
        /// <returns>Game version if possible, null on error</returns>
        public static string? GetPlayStation4Version(Drive? drive)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(drive?.Name))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drive!.Name))
                return null;

            // If we can't find param.sfo, we don't have a PlayStation 4 disc
#if NET20 || NET35
            string paramSfoPath = Path.Combine(Path.Combine(drive.Name, "bd"), "param.sfo");
#else
            string paramSfoPath = Path.Combine(drive.Name, "bd", "param.sfo");
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
        /// <param name="drive">Drive to extract information from</param>
        /// <returns>Internal disc serial if possible, null on error</returns>
        public static string? GetPlayStation5Serial(Drive? drive)
        {
            // Attempt to get the param.json file
            var json = GetPlayStation5ParamsJsonFromDrive(drive);
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

        // <summary>
        /// Get the version from a PlayStation 5 disc, if possible
        /// </summary>
        /// <param name="drive">Drive to extract information from</param>
        /// <returns>Game version if possible, null on error</returns>
        public static string? GetPlayStation5Version(Drive? drive)
        {
            // Attempt to get the param.json file
            var json = GetPlayStation5ParamsJsonFromDrive(drive);
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
        /// <param name="drive">Drive to extract information from</param>
        /// <returns>JObject representing the JSON on success, null on error</returns>
        private static JObject? GetPlayStation5ParamsJsonFromDrive(Drive? drive)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(drive?.Name))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drive!.Name))
                return null;

            // If we can't find param.json, we don't have a PlayStation 5 disc
#if NET20 || NET35
            string paramJsonPath = Path.Combine(Path.Combine(drive.Name, "bd"), "param.json");
#else
            string paramJsonPath = Path.Combine(drive.Name, "bd", "param.json");
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

        #endregion
    }
}