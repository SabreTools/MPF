using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SabreTools.IO;
using SabreTools.RedumpLib.Data;

namespace MPF.Frontend.Tools
{
    public static class PhysicalTool
    {
        #region Generic

        /// <summary>
        /// Get the last modified date for a file from a physical disc, if possible
        /// </summary>
        /// <param name="drive">Drive to extract information from</param>
        /// <param name="filePath">Relative file path</param>
        /// <returns>Output last modified date in "yyyy-mm-dd" format if possible, null on error</returns>
        public static string? GetFileDate(Drive? drive, string? filePath, bool fixTwoDigitYear = false)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(drive?.Name))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drive!.Name))
                return null;

            // If the executable name is invalid, we can't do this part
            if (string.IsNullOrEmpty(filePath))
                return null;

            // Now that we have the EXE name, try to get the fileinfo for it
            string exePath = Path.Combine(drive.Name, filePath);
            if (!File.Exists(exePath))
                return null;

            try
            {
                // Get the last modified time
                var fi = new FileInfo(exePath);
                var lastModified = fi.LastWriteTimeUtc;
                int year = lastModified.Year;
                int month = lastModified.Month;
                int day = lastModified.Day;

                // Fix the Y2K timestamp issue, if required
                if (fixTwoDigitYear)
                    year = year >= 1900 && year < 1920 ? 2000 + year % 100 : year;

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
        /// Get the first numBytes bytes from a disc drive
        /// </summary>
        /// <param name="drive">Drive to get sector from</param>
        /// <param name="numBytes">Number of bytes to read from drive, maximum of one sector (2048 bytes)</param>
        /// <returns>Byte array of first sector of data, null on error</returns>
        public static byte[]? GetFirstBytes(Drive? drive, int numBytes)
        {
            if (drive == null || drive.Letter == null || drive.Letter == '\0')
                return null;

            // Must read between 1 and 2048 bytes
            if (numBytes < 1)
                return null;
            else if (numBytes > 2048)
                numBytes = 2048;

            string drivePath = $"\\\\.\\{drive.Letter}:";
            var firstSector = new byte[numBytes];
            try
            {
                // Open the drive as a raw device
                using var driveStream = new FileStream(drivePath, FileMode.Open, FileAccess.Read);

                // Read the first sector
                int bytesRead = driveStream.Read(firstSector, 0, numBytes);
                if (bytesRead < numBytes)
                    return null;
            }
            catch
            {
                // Absorb the exception
                return null;
            }

            return firstSector;
        }

        #endregion

        #region BD-Video

        /// <summary>
        /// Get if the Bus Encryption Enabled (BEE) flag is set in a path
        /// </summary>
        /// <param name="drive">Drive to extract information from</param>
        /// <returns>Bus encryption enabled status if possible, false otherwise</returns>
        public static bool GetBusEncryptionEnabled(Drive? drive)
        {
            // If there's no drive path, we can't get BEE flag
            if (string.IsNullOrEmpty(drive?.Name))
                return false;

            // If the folder no longer exists, we can't get exe name
            if (!Directory.Exists(drive!.Name))
                return false;

            // Get the two possible file paths
#if NET20 || NET35
            string content000 = Path.Combine(Path.Combine(drive.Name, "AACS"), "Content000.cer");
            string content001 = Path.Combine(Path.Combine(drive.Name, "AACS"), "Content001.cer");
#else
            string content000 = Path.Combine(drive.Name, "AACS", "Content000.cer");
            string content001 = Path.Combine(drive.Name, "AACS", "Content001.cer");
#endif

            try
            {
                // Check the required files
                if (File.Exists(content000) && new FileInfo(content000).Length > 1)
                {
                    using var fs = File.OpenRead(content000);
                    _ = fs.ReadByte(); // Skip the first byte
                    return fs.ReadByte() > 127;
                }
                else if (File.Exists(content001) && new FileInfo(content001).Length > 1)
                {
                    using var fs = File.OpenRead(content001);
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
        
        // TODO: This could technically be PC or Mac, unsure what to call it
        #region Computer
        
        // TODO: All 3 steam methods contain a lot of duplicated code. Is it possible to merge them?
        /// <summary>
        /// Get info for discs containing Steam2 (sis/sim/sid) depots
        /// </summary>
        /// <param name="drive">Drive to extract information from</param>
        /// <returns>Steam2 information on success, null otherwise</returns>
        public static string? GetSteam2Info(Drive? drive)
        {
            // If there's no drive path, we can't get exe name
            if (string.IsNullOrEmpty(drive?.Name))
                return null;

            // If the folder no longer exists, we can't get any information
            if (!Directory.Exists(drive!.Name))
                return null;

            try
            {
                string? steamInfo = "";
                var steamDepotIdList = new List<string>();

                // TODO: is this case-sensitive?
                string[] sisPaths = Directory.GetFiles(drive.Name, "*.sis");

                foreach (string sis in sisPaths)
                {
                    if (!File.Exists(sis))
                        continue;
                    
                    string filename = Path.GetFileName(sis);

                    // Skips steam3 sku sis files
                    // TODO: is this always the correct assumption?
                    if (filename.ToLower() == "sku.sis")
                        continue;
                    
                    long sisSize = new FileInfo(sis).Length;
                    
                    // Arbitrary filesize
                    // TODO: check all sis sizes to make sure this is good
                    if (sisSize < 1000000)
                        continue;

                    // Read the sku sis file
                    using var fileStream = new FileStream(sis, FileMode.Open, FileAccess.Read);
                    var skuSisDeserializer = new SabreTools.Serialization.Readers.SkuSis();
                    var skuSis = skuSisDeserializer.Deserialize(fileStream);

                    if (skuSis != null && skuSis.VDFObject != null)
                    {
                        JToken? upper = null;
                    
                        //TODO: use ST.Serialization constants?
                        if (skuSis.VDFObject.TryGetValue("SKU", out var steam2Value))
                            upper = steam2Value;
                    
                        if (upper == null)
                            continue;

                        if (upper["depots"] == null)
                            continue;

                        // TODO: why do I need to use conditional access still
                        var depotArr = upper["depots"]?.ToObject<Dictionary<string, string>>();
                    
                        if (depotArr == null)
                            continue;
                    
                        steamDepotIdList.AddRange(depotArr.Values.ToList());
                    }
                }
                
                var sortedArray = steamDepotIdList.Select(long.Parse).ToArray();
                
                // TODO: do I need to get the environment newline here
                // TODO: compatibility with pre dotnet 4.0
                steamInfo = string.Join("\n", sortedArray);

                if (steamInfo == "")
                    return null;
                else
                    return steamInfo;
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }
        
                /// <summary>
        /// Get info for discs containing Steam2 (sis/csm/csd) depots
        /// </summary>
        /// <param name="drive">Drive to extract information from</param>
        /// <returns>Steam3 information on success, null otherwise</returns>
        public static string? GetSteam3Info(Drive? drive)
        {
            // If there's no drive path, we can't get exe name
            if (string.IsNullOrEmpty(drive?.Name))
                return null;

            // If the folder no longer exists, we can't get any information
            if (!Directory.Exists(drive!.Name))
                return null;

            try
            {
                string? steamInfo = "";
                var steamDepotIdDict = new SortedDictionary<long, long>();

                // TODO: is this case-sensitive?
                string[] sisPaths = Directory.GetFiles(drive.Name, "*.sis");

                foreach (string sis in sisPaths)
                {
                    if (!File.Exists(sis))
                        continue;
                    
                    string filename = Path.GetFileName(sis);

                    // Skips steam2 sku sis files
                    // TODO: is this always the correct assumption?
                    if (filename.ToLower() != "sku.sis")
                        continue;
                    
                    long sisSize = new FileInfo(sis).Length;
                    
                    // Arbitrary filesize
                    // TODO: check all sis sizes to make sure this is good
                    if (sisSize < 1000000)
                        continue;

                    // Read the sku sis file
                    using var fileStream = new FileStream(sis, FileMode.Open, FileAccess.Read);
                    var skuSisDeserializer = new SabreTools.Serialization.Readers.SkuSis();
                    var skuSis = skuSisDeserializer.Deserialize(fileStream);

                    if (skuSis != null && skuSis.VDFObject != null)
                    {
                        JToken? upper = null;
                    
                        //TODO: use ST.Serialization constants?
                        if (skuSis.VDFObject.TryGetValue("sku", out var steam3Value))
                            upper = steam3Value;
                    
                        if (upper == null)
                            continue;

                        if (upper["manifests"] == null)
                            continue;

                        // TODO: why do I need to use conditional access still
                        // TODO: i dont think parsing them directly to long, long works. Fix this later, or rectify the
                        // TODO: others if it actually does
                        var depotArr = upper["manifests"]?.ToObject<Dictionary<long, long>>();
                    
                        if (depotArr == null)
                            continue;

                        foreach (var depot in depotArr)
                            steamDepotIdDict.Add(depot.Key, depot.Value);
                    }
                }

                // TODO: do I need to get the environment newline here
                foreach (var depot in steamDepotIdDict)
                {
                    steamInfo += $"{depot.Key} ({depot.Value})\n";
                }

                if (steamInfo == "")
                    return null;
                else
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
        /// <param name="drive">Drive to extract information from</param>
        /// <returns>Steam2 information on success, null otherwise</returns>
        public static string? GetSteamAppInfo(Drive? drive)
        {
            // If there's no drive path, we can't get exe name
            if (string.IsNullOrEmpty(drive?.Name))
                return null;

            // If the folder no longer exists, we can't get any information
            if (!Directory.Exists(drive!.Name))
                return null;

            try
            {
                string? steamInfo = "";
                var steamAppIdList = new List<string>();

                // TODO: is this case-sensitive?
                string[] sisPaths = Directory.GetFiles(drive.Name, "*.sis");

                // Looping needed in case i.e. this is a coverdisc with multiple steam game installers on it.
                foreach (string sis in sisPaths)
                {
                    if (!File.Exists(sis))
                        continue;
                    
                    string filename = Path.GetFileName(sis);

                    // Skips steam3 sku sis files
                    // TODO: is this always the correct assumption?
                    if (filename.ToLower() == "sku.sis")
                        continue;
                    
                    long sisSize = new FileInfo(sis).Length;
                    
                    // Arbitrary filesize
                    // TODO: check all sis sizes to make sure this is good
                    if (sisSize < 1000000)
                        continue;

                    // Read the sku sis file
                    using var fileStream = new FileStream(sis, FileMode.Open, FileAccess.Read);
                    var skuSisDeserializer = new SabreTools.Serialization.Readers.SkuSis();
                    var skuSis = skuSisDeserializer.Deserialize(fileStream);

                    if (skuSis != null && skuSis.VDFObject != null)
                    {
                        JToken? upper = null;
                        
                        //TODO: use ST.Serialization constants?
                        if (skuSis.VDFObject.TryGetValue("SKU", out var steam2Value))
                            upper = steam2Value;
                        else if (skuSis.VDFObject.TryGetValue("sku", out var steam3Value))
                            upper = steam3Value;
                        
                        if (upper == null)
                            continue;

                        if (upper["apps"] == null)
                            continue;

                        // TODO: why do I need to use conditional access still
                        var appArr = upper["apps"]?.ToObject<Dictionary<string, string>>();
                        
                        if (appArr == null)
                            continue;
                        
                        steamAppIdList.AddRange(appArr.Values.ToList());
                    }
                }

                var sortedArray = steamAppIdList.Select(long.Parse).ToArray();
                // TODO: compatibility with pre dotnet 4.0
                steamInfo = string.Join(", ", sortedArray);

                if (steamInfo == "")
                    return null;
                else
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

            try
            {
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
            if (File.Exists(psxExePath))
                return "PSX.EXE";

            // If neither can be found, we return null
            return null;
        }

        /// <summary>
        /// Get the serial from a PlayStation disc, if possible
        /// </summary>
        /// <param name="drive">Drive to extract information from</param>
        /// <returns>Serial on success, null otherwise</returns>
        public static string? GetPlayStationSerial(Drive? drive)
        {
            // Try to get the executable name
            string? exeName = GetPlayStationExecutableName(drive);
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

            try
            {
                // Try to parse the SYSTEM.CNF file
                var systemCnf = new IniFile(systemCnfPath);
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
                    // Absorb the exception
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
                    // Absorb the exception
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
                    // Absorb the exception
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
                    // Absorb the exception
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
                // Absorb the exception
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
                // Absorb the exception
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
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Get the app.pkg info from a PlayStation 4 disc, if possible
        /// </summary>
        /// <param name="drive">Drive to extract information from</param>
        /// <returns>PKG info if possible, null on error</returns>
        public static string? GetPlayStation4PkgInfo(Drive? drive)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(drive?.Name))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drive!.Name))
                return null;

            // Try parse the app.pkg (multiple if they exist)
            try
            {
                string? pkgInfo = "";

                string[] appDirs = Directory.GetDirectories(Path.Combine(drive.Name, "app"), "?????????", SearchOption.TopDirectoryOnly);

                foreach (string dir in appDirs)
                {
                    string appPkgPath = Path.Combine(dir, "app.pkg");
                    if (!File.Exists(appPkgPath))
                        continue;

                    long appPkgSize = new FileInfo(appPkgPath).Length;
                    if (appPkgSize < 4096)
                        continue;

                    // Read the app.pkg header
                    using var fileStream = new FileStream(appPkgPath, FileMode.Open, FileAccess.Read);
                    var appPkgHeaderDeserializer = new SabreTools.Serialization.Readers.AppPkgHeader();
                    var appPkgHeader = appPkgHeaderDeserializer.Deserialize(fileStream);

                    if (appPkgHeader != null)
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
                // Absorb the exception
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
                // Absorb the exception
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
            if (string.IsNullOrEmpty(filename))
                return null;
            if (!File.Exists(filename))
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
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Get the app.pkg info from a PlayStation 5 disc, if possible
        /// </summary>
        /// <param name="drive">Drive to extract information from</param>
        /// <returns>PKG info if possible, null on error</returns>
        public static string? GetPlayStation5PkgInfo(Drive? drive)
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(drive?.Name))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(drive!.Name))
                return null;

            // Try parse the app_sc.pkg (multiple if they exist)
            try
            {
                string? pkgInfo = "";

                string[] appDirs = Directory.GetDirectories(Path.Combine(drive.Name, "app"), "?????????", SearchOption.TopDirectoryOnly);

                foreach (string dir in appDirs)
                {
                    string appPkgPath = Path.Combine(dir, "app_sc.pkg");
                    if (!File.Exists(appPkgPath))
                        continue;

                    long appPkgSize = new FileInfo(appPkgPath).Length;
                    if (appPkgSize < 4096)
                        continue;

                    // Read the app_sc.pkg header
                    using var fileStream = new FileStream(appPkgPath, FileMode.Open, FileAccess.Read);
                    var appPkgHeaderDeserializer = new SabreTools.Serialization.Readers.AppPkgHeader();
                    var appPkgHeader = appPkgHeaderDeserializer.Deserialize(fileStream);

                    if (appPkgHeader != null)
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
        /// <param name="drive">Drive to extract information from</param>
        /// <returns>Filenames if possible, null on error</returns>
        public static string? GetXboxFilenames(Drive? drive)
        {
            // If there's no drive path, can't do anything
            if (string.IsNullOrEmpty(drive?.Name))
                return null;

            // If the folder no longer exists, can't do anything
            if (!Directory.Exists(drive!.Name))
                return null;

            // Get the MSXC directory path
            string msxc = Path.Combine(drive.Name, "MSXC");
            if (!Directory.Exists(msxc))
                return null;

            try
            {
                var files = Directory.GetFiles(msxc, "*", SearchOption.TopDirectoryOnly);
                var filenames = Array.ConvertAll(files, Path.GetFileName);
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
        /// <param name="drive">Drive to extract information from</param>
        /// <returns>Title ID(s) if possible, null on error</returns>
        public static string? GetXboxTitleID(Drive? drive)
        {
            // If there's no drive path, can't do anything
            if (string.IsNullOrEmpty(drive?.Name))
                return null;

            // If the folder no longer exists, can't do anything
            if (!Directory.Exists(drive!.Name))
                return null;

            // Get the catalog.js path
#if NET20 || NET35
            string catalogjs = Path.Combine(drive.Name, Path.Combine("MSXC", Path.Combine("Metadata", "catalog.js")));
#else
            string catalogjs = Path.Combine(drive.Name, "MSXC", "Metadata", "catalog.js");
#endif
            // Check catalog.js exists
            if (!File.Exists(catalogjs))
                return null;

            // Deserialize catalog.js and extract Title ID(s)
            try
            {
                var catalog = new SabreTools.Serialization.Readers.Catalog().Deserialize(catalogjs);
                if (catalog == null)
                    return null;
                if (!string.IsNullOrEmpty(catalog.TitleID))
                    return catalog.TitleID;
                if (catalog.Packages == null)
                    return null;

                List<string> titleIDs = [];
                foreach (var package in catalog.Packages)
                {
                    if (package?.TitleID != null)
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
        /// <param name="drive">Drive to detect system from</param>
        /// <returns>Detected RedumpSystem if detected, null otherwise</returns>
        public static RedumpSystem? DetectSegaSystem(Drive? drive)
        {
            if (drive == null)
                return null;

            byte[]? firstSector = GetFirstBytes(drive, 0x10);
            if (firstSector == null || firstSector.Length < 0x10)
                return null;

            string systemType = Encoding.ASCII.GetString(firstSector, 0x00, 0x10);

            if (systemType.Equals("SEGA SEGASATURN ", StringComparison.Ordinal))
                return RedumpSystem.SegaSaturn;
            else if (systemType.Equals("SEGA SEGAKATANA ", StringComparison.Ordinal))
                return RedumpSystem.SegaDreamcast;
            else if (systemType.Equals("SEGADISCSYSTEM  ", StringComparison.Ordinal))
                return RedumpSystem.SegaMegaCDSegaCD;
            else if (systemType.Equals("SEGA MEGA DRIVE ", StringComparison.Ordinal))
                return RedumpSystem.SegaMegaCDSegaCD;
            else if (systemType.Equals("SEGA GENESIS    ", StringComparison.Ordinal))
                return RedumpSystem.SegaMegaCDSegaCD;

            return null;
        }

        #endregion

        #region Other

        /// <summary>
        /// Detect a 3DO disc based on the CD ROM header
        /// </summary>
        /// <param name="drive">Drive to detect 3DO disc from</param>
        /// <returns>RedumpSystem.Panasonic3DOInteractiveMultiplayer if detected, null otherwise</returns>
        public static RedumpSystem? Detect3DOSystem(Drive? drive)
        {
            if (drive == null)
                return null;

            byte[]? firstSector = GetFirstBytes(drive, 0xC0);
            if (firstSector == null || firstSector.Length < 0xC0)
                return null;

            string systemType = Encoding.ASCII.GetString(firstSector, 0xB0, 0x10);

            if (systemType.Equals("iamaduckiamaduck", StringComparison.Ordinal))
                return RedumpSystem.Panasonic3DOInteractiveMultiplayer;

            return null;
        }

        #endregion

    }
}
