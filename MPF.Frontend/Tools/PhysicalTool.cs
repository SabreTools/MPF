using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MPF.Frontend.FileSystems;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SabreTools.Hashing;
using SabreTools.RedumpLib.Data;
using SabreTools.Text.INI;

namespace MPF.Frontend.Tools
{
    public static class PhysicalTool
    {
        #region Generic

        /// <summary>
        /// Get the last modified date for a file from a physical disc, if possible
        /// </summary>
        /// <param name="files">Files of the disc to extract information from</param>
        /// <param name="filePath">Relative file path</param>
        /// <returns>Output last modified date in "yyyy-mm-dd" format if possible, null on error</returns>
        public static string? GetFileDate(DiscFileSystem? files, string? filePath, bool fixTwoDigitYear = false)
        {
            // If there are no disc files, we can't do this part
            if (files is null)
                return null;

            // If the executable name is invalid, we can't do this part
            if (string.IsNullOrEmpty(filePath))
                return null;

            try
            {
                // Get the last modified time
                var lastWriteTimeUtc = files.GetLastWriteTimeUtc(filePath!);
                if (lastWriteTimeUtc is null)
                    return null;

                var lastModified = lastWriteTimeUtc.Value;
                int year = lastModified.Year;
                int month = lastModified.Month;
                int day = lastModified.Day;

                // Fix the Y2K timestamp issue, if required
                if (fixTwoDigitYear)
                    year = year >= 1900 && year < 1920 ? 2000 + (year % 100) : year;

                // Format and return the string
                var dt = new DateTime(year, month, day);
                return dt.ToString("yyyy-MM-dd");
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Get the first numBytes bytes from a disc
        /// </summary>
        /// <param name="files">Files of the disc to get the first sector from</param>
        /// <param name="numBytes">Number of bytes to read from the disc, maximum of one sector (2048 bytes)</param>
        /// <returns>Byte array of first sector of data, null on error</returns>
        public static byte[]? GetFirstBytes(DiscFileSystem? files, int numBytes)
            => files?.ReadFirstBytes(numBytes);

        #endregion

        #region BD-Video

        /// <summary>
        /// Get Blu-ray protection information
        /// </summary>
        /// <param name="files">Files of the disc to extract information from</param>
        /// <returns>Formatted string representing the protection on success, null otherwise</returns>
        public static string? GetBluRayProtection(DiscFileSystem? files)
        {
            // If there are no disc files, we can't get information
            if (files is null)
                return null;

            // Generate unit key file hash
            string? unitKeyFileHash = null;
            using (var unitKeyFile = files.OpenFile("AACS/Unit_Key_RO.inf "))
            {
                if (unitKeyFile is not null)
                    unitKeyFileHash = HashTool.GetStreamHash(unitKeyFile, HashType.SHA1)?.ToUpperInvariant();
            }

            // Generate the output strings
            var sb = new StringBuilder();

            sb.AppendLine($"Media Key: (OPTIONAL)");
            sb.AppendLine($"Volume ID: (OPTIONAL)");
            sb.AppendLine($"Volume Unique Key: (OPTIONAL)");
            sb.AppendLine($"Unit Key File Hash (DiscID): {unitKeyFileHash ?? "(OPTIONAL)"}");

            return sb.ToString();
        }

        /// <summary>
        /// Get if the Bus Encryption Enabled (BEE) flag is set in a path
        /// </summary>
        /// <param name="files">Files of the disc to extract information from</param>
        /// <returns>Bus encryption enabled status if possible, false otherwise</returns>
        public static bool GetBusEncryptionEnabled(DiscFileSystem? files)
        {
            // If there are no disc files, we can't get BEE flag
            if (files is null)
                return false;

            // Get the two possible file paths
            string content000 = "AACS/Content000.cer";
            string content001 = "AACS/Content001.cer";

            try
            {
                // Check the required files
                if (files.GetFileLength(content000) > 1)
                {
                    using var fs = files.OpenFile(content000)!;
                    _ = fs.ReadByte(); // Skip the first byte
                    return fs.ReadByte() > 127;
                }
                else if (files.GetFileLength(content001) > 1)
                {
                    using var fs = files.OpenFile(content001)!;
                    _ = fs.ReadByte(); // Skip the first byte
                    return fs.ReadByte() > 127;
                }

                // False if neither file fits the criteria
                return false;
            }
            catch
            {
                // Absorb the exception
                return false;
            }
        }

        #endregion

        #region Computer

        /// <summary>
        /// Get info for discs containing Steam2 (sis/sim/sid) depots
        /// </summary>
        /// <param name="files">Files of the disc to extract information from</param>
        /// <returns>Steam2 information on success, null otherwise</returns>
        public static string? GetSteamSimSidInfo(DiscFileSystem? files)
        {
            // If there are no disc files, we can't get any sis files
            if (files is null)
                return null;

            try
            {
                // ? needed due to note in https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.getfiles
                string[] sisPaths = files.GetFiles(string.Empty, "?*.sis", recursive: true);

                var steamDepotIdList = new List<long>();
                foreach (string sis in sisPaths)
                {
                    // Arbitrary filesize cap, a disc would need over 100x the normal amount of depots to go over this
                    long sisSize = files.GetFileLength(sis);
                    if (sisSize < 0 || sisSize > 10_000)
                        continue;

                    // Read the sku sis file
                    using var fileStream = files.OpenFile(sis);
                    var skuSisDeserializer = new SabreTools.Serialization.Readers.SkuSis();
                    var skuSis = skuSisDeserializer.Deserialize(fileStream);
                    if (skuSis?.Sku is null)
                        continue;

                    // Skips csm/csd sku sis files
                    if (skuSis.Sku.Manifests is not null)
                        continue;

                    // There should always be depots
                    if (skuSis.Sku.Depots is null)
                        continue;

                    steamDepotIdList.AddRange([.. skuSis.Sku.Depots.Values]);
                }

                steamDepotIdList.Sort();
                string steamInfo = string.Join("\n", [.. steamDepotIdList.ConvertAll(l => l.ToString()).Distinct()]);
                if (string.IsNullOrEmpty(steamInfo))
                    return null;

                return steamInfo;
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Get info for discs containing Steam3 (sis/csm/csd) depots
        /// </summary>
        /// <param name="files">Files of the disc to extract information from</param>
        /// <returns>Steam Csm/Csd information on success, null otherwise</returns>
        public static string? GetSteamCsmCsdInfo(DiscFileSystem? files)
        {
            // If there are no disc files, we can't get exe name
            if (files is null)
                return null;

            try
            {
                // ? needed due to note in https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.getfiles
                string[] sisPaths = files.GetFiles(string.Empty, "?*.sis", recursive: true);

                var steamDepotIdDict = new SortedDictionary<long, long>();
                foreach (string sis in sisPaths)
                {
                    // Arbitrary filesize cap, a disc would need over 100x the normal amount of depots to go over this.
                    long sisSize = files.GetFileLength(sis);
                    if (sisSize < 0 || sisSize > 10_000)
                        continue;

                    // Read the sku sis file
                    using var fileStream = files.OpenFile(sis);
                    var skuSisDeserializer = new SabreTools.Serialization.Readers.SkuSis();
                    var skuSis = skuSisDeserializer.Deserialize(fileStream);
                    if (skuSis?.Sku is null)
                        continue;

                    // Skips steam2 sku sis files, Steam3 csm/csd sis files always have manifests
                    if (skuSis.Sku.Manifests is null)
                        continue;

                    foreach (var depot in skuSis.Sku.Manifests)
                    {
#if NETFRAMEWORK
                        // Always fine in practice
                        steamDepotIdDict.Add(depot.Key, depot.Value);
#else
                        // Mainly here to allow easier bulk testing
                        steamDepotIdDict.TryAdd(depot.Key, depot.Value);
#endif
                    }
                }

                string steamInfo = string.Empty;
                foreach (var depot in steamDepotIdDict)
                {
                    steamInfo += $"{depot.Key} ({depot.Value})\n";
                }

                if (string.IsNullOrEmpty(steamInfo))
                    return null;

                return steamInfo;
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Get info for discs containing Steam2 (sis/sim/sid) depots
        /// </summary>
        /// <param name="files">Files of the disc to extract information from</param>
        /// <returns>Steam2 information on success, null otherwise</returns>
        public static string? GetSteamAppInfo(DiscFileSystem? files)
        {
            // If there are no disc files, we can't get exe name
            if (files is null)
                return null;

            try
            {
                // ? needed due to note in note in https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.getfiles
                string[] sisPaths = files.GetFiles(string.Empty, "?*.sis", recursive: true);

                // Looping needed in case i.e. this is a coverdisc with multiple steam game installers on it.
                var steamAppIdList = new List<long>();
                foreach (string sis in sisPaths)
                {
                    // Arbitrary filesize cap, a disc would need over 100x the normal amount of depots to go over this.
                    long sisSize = files.GetFileLength(sis);
                    if (sisSize < 0 || sisSize > 10_000)
                        continue;

                    // Read the sku sis file
                    using var fileStream = files.OpenFile(sis);
                    var skuSisDeserializer = new SabreTools.Serialization.Readers.SkuSis();
                    var skuSis = skuSisDeserializer.Deserialize(fileStream);
                    if (skuSis?.Sku is null)
                        continue;

                    // There should always be apps
                    if (skuSis.Sku.Apps is null)
                        continue;

                    steamAppIdList.AddRange([.. skuSis.Sku.Apps.Values]);
                }

                steamAppIdList.Sort();
                string steamInfo = string.Join(", ", [.. steamAppIdList.ConvertAll(l => l.ToString()).Distinct()]);
                if (string.IsNullOrEmpty(steamInfo))
                    return null;

                return steamInfo;
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        #endregion

        #region PlayStation

        /// <summary>
        /// Get the EXE name from a PlayStation disc, if possible
        /// </summary>
        /// <param name="files">Files of the disc to extract information from</param>
        /// <returns>Executable name on success, null otherwise</returns>
        public static string? GetPlayStationExecutableName(DiscFileSystem? files)
        {
            // If there are no disc files, we can't get exe name
            if (files is null)
                return null;

            try
            {
                // Read the CNF file as an INI file
                using var systemCnfFile = files.OpenFile("SYSTEM.CNF") ?? throw new FileNotFoundException();
                var systemCnf = new IniFile(systemCnfFile);
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
                        // Some games may have the EXE in a subfolder
                        string? serial = match.Groups[1].Value;
                        return Path.GetFileName(serial);
                    }
                }
            }
            catch
            {
                // Absorb the exception, assume SYSTEM.CNF doesn't exist
            }

            // If the SYSTEM.CNF value can't be found, try PSX.EXE
            if (files.FileExists("PSX.EXE"))
                return "PSX.EXE";

            // If neither can be found, we return null
            return null;
        }

        /// <summary>
        /// Get the serial from a PlayStation disc, if possible
        /// </summary>
        /// <param name="files">Files of the disc to extract information from</param>
        /// <returns>Serial on success, null otherwise</returns>
        public static string? GetPlayStationSerial(DiscFileSystem? files)
        {
            // Try to get the executable name
            string? exeName = GetPlayStationExecutableName(files);
            if (string.IsNullOrEmpty(exeName))
                return null;

            // Handle generic PSX.EXE
            if (exeName == "PSX.EXE")
                return null;

            // EXE name may have a trailing `;` after
            // EXE name should always be in all caps
            exeName = exeName!
                .Split(';')[0]
                .ToUpperInvariant();

            // Serial is most of the EXE name normalized
            return exeName
                .Replace('_', '-')
                .Replace(".", string.Empty);
        }

        /// <summary>
        /// Get the version from a PlayStation 2 disc, if possible
        /// </summary>
        /// <param name="files">Files of the disc to extract information from</param>
        /// <returns>Game version if possible, null on error</returns>
        public static string? GetPlayStation2Version(DiscFileSystem? files)
        {
            // If there are no disc files, we can't do this part
            if (files is null)
                return null;

            try
            {
                // Try to parse the SYSTEM.CNF file
                using var systemCnfFile = files.OpenFile("SYSTEM.CNF");
                if (systemCnfFile is null)
                    return null;

                var systemCnf = new IniFile(systemCnfFile);
                if (systemCnf.ContainsKey("VER"))
                    return systemCnf["VER"];

                // If "VER" can't be found, we can't do much
                return null;
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Get the internal serial from a PlayStation 3 disc, if possible
        /// </summary>
        /// <param name="files">Files of the disc to extract information from</param>
        /// <returns>Internal disc serial if possible, null on error</returns>
        public static string? GetPlayStation3Serial(DiscFileSystem? files)
        {
            // If there are no disc files, we can't do this part
            if (files is null)
                return null;

            // Attempt to use PS3_DISC.SFB
            if (files.FileExists("PS3_DISC.SFB"))
            {
                try
                {
                    using var br = new BinaryReader(files.OpenFile("PS3_DISC.SFB")!);
                    br.BaseStream.Seek(0x220, SeekOrigin.Begin);
                    return new string(br.ReadChars(0x10)).TrimEnd('\0');
                }
                catch
                {
                    // Absorb the exception
                    return null;
                }
            }

            // Attempt to use PARAM.SFO
            string sfoPath = "PS3_GAME/PARAM.SFO";
            if (files.FileExists(sfoPath))
            {
                try
                {
                    using var br = new BinaryReader(files.OpenFile(sfoPath)!);
                    br.BaseStream.Seek(-0x18, SeekOrigin.End);
                    return new string(br.ReadChars(9)).TrimEnd('\0').Insert(4, "-");
                }
                catch
                {
                    // Absorb the exception
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the version from a PlayStation 3 disc, if possible
        /// </summary>
        /// <param name="files">Files of the disc to extract information from</param>
        /// <returns>Game version if possible, null on error</returns>
        public static string? GetPlayStation3Version(DiscFileSystem? files)
        {
            // If there are no disc files, we can't do this part
            if (files is null)
                return null;

            // Attempt to use PS3_DISC.SFB
            if (files.FileExists("PS3_DISC.SFB"))
            {
                try
                {
                    using var br = new BinaryReader(files.OpenFile("PS3_DISC.SFB")!);
                    br.BaseStream.Seek(0x230, SeekOrigin.Begin);
                    var discVersion = new string(br.ReadChars(0x10)).TrimEnd('\0');
                    if (!string.IsNullOrEmpty(discVersion))
                        return discVersion;
                }
                catch
                {
                    // Absorb the exception
                    return null;
                }
            }

            // Attempt to use PARAM.SFO
            string sfoPath = "PS3_GAME/PARAM.SFO";
            if (files.FileExists(sfoPath))
            {
                try
                {
                    using var br = new BinaryReader(files.OpenFile(sfoPath)!);
                    br.BaseStream.Seek(-0x08, SeekOrigin.End);
                    return new string(br.ReadChars(5)).TrimEnd('\0');
                }
                catch
                {
                    // Absorb the exception
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the firmware version from a PlayStation 3 disc, if possible
        /// </summary>
        /// <param name="files">Files of the disc to extract information from</param>
        /// <returns>Firmware version if possible, null on error</returns>
        public static string? GetPlayStation3FirmwareVersion(DiscFileSystem? files)
        {
            // If there are no disc files, we can't do this part
            if (files is null)
                return null;

            // Attempt to read from /PS3_UPDATE/PS3UPDAT.PUP
            string pupPath = "PS3_UPDATE/PS3UPDAT.PUP";
            if (!files.FileExists(pupPath))
                return null;

            try
            {
                using var br = new BinaryReader(files.OpenFile(pupPath)!);
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
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Get the internal serial from a PlayStation 4 disc, if possible
        /// </summary>
        /// <param name="files">Files of the disc to extract information from</param>
        /// <returns>Internal disc serial if possible, null on error</returns>
        public static string? GetPlayStation4Serial(DiscFileSystem? files)
        {
            // If there are no disc files, we can't do this part
            if (files is null)
                return null;

            // If we can't find param.sfo, we don't have a PlayStation 4 disc
            string paramSfoPath = "bd/param.sfo";
            if (!files.FileExists(paramSfoPath))
                return null;

            // Let's try reading param.sfo to find the serial at the end of the file
            try
            {
                using var br = new BinaryReader(files.OpenFile(paramSfoPath)!);
                br.BaseStream.Seek(-0x14, SeekOrigin.End);
                return new string(br.ReadChars(9)).Insert(4, "-");
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Get the version from a PlayStation 4 disc, if possible
        /// </summary>
        /// <param name="files">Files of the disc to extract information from</param>
        /// <returns>Game version if possible, null on error</returns>
        public static string? GetPlayStation4Version(DiscFileSystem? files)
        {
            // If there are no disc files, we can't do this part
            if (files is null)
                return null;

            // If we can't find param.sfo, we don't have a PlayStation 4 disc
            string paramSfoPath = "bd/param.sfo";
            if (!files.FileExists(paramSfoPath))
                return null;

            // Let's try reading param.sfo to find the version at the end of the file
            try
            {
                using var br = new BinaryReader(files.OpenFile(paramSfoPath)!);
                br.BaseStream.Seek(-0x08, SeekOrigin.End);
                return new string(br.ReadChars(5));
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Get the app.pkg info from a PlayStation 4 disc, if possible
        /// </summary>
        /// <param name="files">Files of the disc to extract information from</param>
        /// <returns>PKG info if possible, null on error</returns>
        public static string? GetPlayStation4PkgInfo(DiscFileSystem? files)
        {
            // If there are no disc files, we can't do this part
            if (files is null)
                return null;

            // Try parse the app.pkg (multiple if they exist)
            try
            {
                string? pkgInfo = "";

                string[] appDirs = files.GetDirectories("app", "?????????");

                foreach (string dir in appDirs)
                {
                    string appPkgPath = $"{dir}/app.pkg";
                    long appPkgSize = files.GetFileLength(appPkgPath);
                    if (appPkgSize < 4096)
                        continue;

                    // Read the app.pkg header
                    using var fileStream = files.OpenFile(appPkgPath);
                    var appPkgHeaderDeserializer = new SabreTools.Serialization.Readers.AppPkgHeader();
                    var appPkgHeader = appPkgHeaderDeserializer.Deserialize(fileStream);

                    if (appPkgHeader is not null)
                        pkgInfo += $"{appPkgHeader.ContentID}{Environment.NewLine}";
                }

                if (pkgInfo == "")
                    return null;
                else
                    return pkgInfo;
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Get the internal serial from a PlayStation 5 disc, if possible
        /// </summary>
        /// <param name="files">Files of the disc to extract information from</param>
        /// <returns>Internal disc serial if possible, null on error</returns>
        public static string? GetPlayStation5Serial(DiscFileSystem? files)
        {
            // Attempt to get the param.json file
            var json = GetPlayStation5ParamsJson(files);
            if (json is null)
                return null;

            try
            {
                return json["disc"]?[0]?["masterDataId"]?.Value<string>()?.Insert(4, "-");
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        // <summary>
        /// Get the version from a PlayStation 5 disc, if possible
        /// </summary>
        /// <param name="files">Files of the disc to extract information from</param>
        /// <returns>Game version if possible, null on error</returns>
        public static string? GetPlayStation5Version(DiscFileSystem? files)
        {
            // Attempt to get the param.json file
            var json = GetPlayStation5ParamsJson(files);
            if (json is null)
                return null;

            try
            {
                return json["masterVersion"]?.Value<string>();
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Get the params.json file from a disc, if possible
        /// </summary>
        /// <param name="files">Files of the disc to extract information from</param>
        /// <returns>JObject representing the JSON on success, null on error</returns>
        private static JObject? GetPlayStation5ParamsJson(DiscFileSystem? files)
        {
            // If there are no disc files, we can't do this part
            if (files is null)
                return null;

            // If we can't find param.json, we don't have a PlayStation 5 disc
            string paramJsonPath = "bd/param.json";
            if (!files.FileExists(paramJsonPath))
                return null;

            // Let's try reading param.json to find the version in the unencrypted JSON
            try
            {
                using var br = new BinaryReader(files.OpenFile(paramJsonPath)!);
                br.BaseStream.Seek(0x800, SeekOrigin.Begin);
                byte[] jsonBytes = br.ReadBytes((int)(br.BaseStream.Length - 0x800));
                return JsonConvert.DeserializeObject(Encoding.ASCII.GetString(jsonBytes)) as JObject;
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Get the app.pkg info from a PlayStation 5 disc, if possible
        /// </summary>
        /// <param name="files">Files of the disc to extract information from</param>
        /// <returns>PKG info if possible, null on error</returns>
        public static string? GetPlayStation5PkgInfo(DiscFileSystem? files)
        {
            // If there are no disc files, we can't do this part
            if (files is null)
                return null;

            // Try parse the app_sc.pkg (multiple if they exist)
            try
            {
                string? pkgInfo = "";

                string[] appDirs = files.GetDirectories("app", "?????????");

                foreach (string dir in appDirs)
                {
                    string appPkgPath = $"{dir}/app_sc.pkg";
                    long appPkgSize = files.GetFileLength(appPkgPath);
                    if (appPkgSize < 4096)
                        continue;

                    // Read the app_sc.pkg header
                    using var fileStream = files.OpenFile(appPkgPath);
                    var appPkgHeaderDeserializer = new SabreTools.Serialization.Readers.AppPkgHeader();
                    var appPkgHeader = appPkgHeaderDeserializer.Deserialize(fileStream);

                    if (appPkgHeader is not null)
                        pkgInfo += $"{appPkgHeader.ContentID}{Environment.NewLine}";
                }

                if (pkgInfo == "")
                    return null;
                else
                    return pkgInfo;
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        #endregion

        #region Xbox

        /// <summary>
        /// Get all filenames for Xbox One and Xbox Series X
        /// </summary>
        /// <param name="files">Files of the disc to extract information from</param>
        /// <returns>Filenames if possible, null on error</returns>
        public static string? GetXboxFilenames(DiscFileSystem? files)
        {
            // If there are no disc files, can't do anything
            if (files is null)
                return null;

            // Get the MSXC directory path
            string msxc = "MSXC";
            if (!files.DirectoryExists(msxc))
                return null;

            try
            {
                var msxcFiles = files.GetFiles(msxc, "*", recursive: false);
                var filenames = Array.ConvertAll(msxcFiles, Path.GetFileName);
                return string.Join("\n", filenames);
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Get Title ID(s) for Xbox One and Xbox Series X
        /// </summary>
        /// <param name="files">Files of the disc to extract information from</param>
        /// <returns>Title ID(s) if possible, null on error</returns>
        public static string? GetXboxTitleID(DiscFileSystem? files)
        {
            // If there are no disc files, can't do anything
            if (files is null)
                return null;

            // Get the catalog.js path
            string catalogjs = "MSXC/Metadata/catalog.js";

            // Check catalog.js exists
            if (!files.FileExists(catalogjs))
                return null;

            // Deserialize catalog.js and extract Title ID(s)
            try
            {
                using var catalogStream = files.OpenFile(catalogjs);
                var catalog = new SabreTools.Serialization.Readers.Catalog().Deserialize(catalogStream);
                if (catalog is null)
                    return null;
                if (!string.IsNullOrEmpty(catalog.TitleID))
                    return catalog.TitleID;
                if (catalog.Packages is null)
                    return null;

                List<string> titleIDs = [];
                foreach (var package in catalog.Packages)
                {
                    if (package?.TitleID is not null)
                        titleIDs.Add(package.TitleID);
                }

                return string.Join(", ", [.. titleIDs]);
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        #endregion

        #region Sega

        /// <summary>
        /// Detect the Sega system based on the CD ROM header
        /// </summary>
        /// <param name="files">Files of the disc to detect system from</param>
        /// <returns>Detected PhysicalSystem if detected, null otherwise</returns>
        public static PhysicalSystem? DetectSegaSystem(DiscFileSystem? files)
        {
            byte[]? firstSector = GetFirstBytes(files, 0x10);
            if (firstSector is null || firstSector.Length < 0x10)
                return null;

            string systemType = Encoding.ASCII.GetString(firstSector, 0x00, 0x10);

            if (systemType.Equals("SEGA SEGASATURN ", StringComparison.Ordinal))
                return PhysicalSystem.SegaSaturn;
            else if (systemType.Equals("SEGA SEGAKATANA ", StringComparison.Ordinal))
                return PhysicalSystem.SegaDreamcast;
            else if (systemType.Equals("SEGADISCSYSTEM  ", StringComparison.Ordinal))
                return PhysicalSystem.SegaMegaCDSegaCD;
            else if (systemType.Equals("SEGA MEGA DRIVE ", StringComparison.Ordinal))
                return PhysicalSystem.SegaMegaCDSegaCD;
            else if (systemType.Equals("SEGA GENESIS    ", StringComparison.Ordinal))
                return PhysicalSystem.SegaMegaCDSegaCD;

            return null;
        }

        #endregion

        #region Other

        /// <summary>
        /// Detect a 3DO disc based on the CD ROM header
        /// </summary>
        /// <param name="files">Files of the disc to detect 3DO disc from</param>
        /// <returns>PhysicalSystem.Panasonic3DOInteractiveMultiplayer if detected, null otherwise</returns>
        public static PhysicalSystem? Detect3DOSystem(DiscFileSystem? files)
        {
            byte[]? firstSector = GetFirstBytes(files, 0xC0);
            if (firstSector is null || firstSector.Length < 0xC0)
                return null;

            string systemType = Encoding.ASCII.GetString(firstSector, 0xB0, 0x10);

            if (systemType.Equals("iamaduckiamaduck", StringComparison.Ordinal))
                return PhysicalSystem.Panasonic3DOInteractiveMultiplayer;

            return null;
        }

        #endregion

    }
}
