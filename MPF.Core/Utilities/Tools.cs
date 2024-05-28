using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SabreTools.RedumpLib.Data;

namespace MPF.Core.Utilities
{
    public static class Tools
    {
        #region Byte Arrays

        /// <summary>
        /// Converts a hex string into a byte array
        /// </summary>
        /// <param name="hex">Hex string</param>
        /// <returns>Converted byte array, or null if invalid hex string</returns>
        public static byte[]? HexStringToByteArray(string? hexString)
        {
            // Valid hex string must be an even number of characters
            if (string.IsNullOrEmpty(hexString) || hexString!.Length % 2 == 1)
                return null;

            // Convert ASCII to byte via lookup table
            int[] hexLookup = [0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F];
            byte[] byteArray = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
            {
                // Convert next two chars to ASCII value relative to '0'
                int a = Char.ToUpper(hexString[i]) - '0';
                int b = Char.ToUpper(hexString[i + 1]) - '0';

                // Ensure hex string only has '0' through '9' and 'A' through 'F' (case insensitive)
                if ((a < 0 || b < 0 || a > 22 || b > 22) || (a > 10 && a < 17) || (b > 10 && b < 17))
                    return null;
                byteArray[i / 2] = (byte)(hexLookup[a] << 4 | hexLookup[b]);
            }

            return byteArray;
        }

        #endregion

        #region Support

        /// <summary>
        /// Returns false if a given InternalProgram does not support a given MediaType
        /// </summary>
        public static bool ProgramSupportsMedia(InternalProgram program, MediaType? type)
        {
            // If the media type is not set, return false
            if (type == null || type == MediaType.NONE)
                return false;

            return (program) switch
            {
                // Aaru
                InternalProgram.Aaru when type == MediaType.BluRay => true,
                InternalProgram.Aaru when type == MediaType.CDROM => true,
                InternalProgram.Aaru when type == MediaType.CompactFlash => true,
                InternalProgram.Aaru when type == MediaType.DVD => true,
                InternalProgram.Aaru when type == MediaType.GDROM => true,
                InternalProgram.Aaru when type == MediaType.FlashDrive => true,
                InternalProgram.Aaru when type == MediaType.FloppyDisk => true,
                InternalProgram.Aaru when type == MediaType.HardDisk => true,
                InternalProgram.Aaru when type == MediaType.HDDVD => true,
                InternalProgram.Aaru when type == MediaType.NintendoGameCubeGameDisc => true,
                InternalProgram.Aaru when type == MediaType.NintendoWiiOpticalDisc => true,
                InternalProgram.Aaru when type == MediaType.SDCard => true,

                // DiscImageCreator
                InternalProgram.DiscImageCreator when type == MediaType.BluRay => true,
                InternalProgram.DiscImageCreator when type == MediaType.CDROM => true,
                InternalProgram.DiscImageCreator when type == MediaType.CompactFlash => true,
                InternalProgram.DiscImageCreator when type == MediaType.DVD => true,
                InternalProgram.DiscImageCreator when type == MediaType.GDROM => true,
                InternalProgram.DiscImageCreator when type == MediaType.FlashDrive => true,
                InternalProgram.DiscImageCreator when type == MediaType.FloppyDisk => true,
                InternalProgram.DiscImageCreator when type == MediaType.HardDisk => true,
                InternalProgram.DiscImageCreator when type == MediaType.HDDVD => true,
                InternalProgram.DiscImageCreator when type == MediaType.NintendoGameCubeGameDisc => true,
                InternalProgram.DiscImageCreator when type == MediaType.NintendoWiiOpticalDisc => true,
                InternalProgram.DiscImageCreator when type == MediaType.SDCard => true,

                // Redumper
                InternalProgram.Redumper when type == MediaType.BluRay => true,
                InternalProgram.Redumper when type == MediaType.CDROM => true,
                InternalProgram.Redumper when type == MediaType.DVD => true,
                InternalProgram.Redumper when type == MediaType.GDROM => true,
                InternalProgram.Redumper when type == MediaType.HDDVD => true,

                // Default
                _ => false,
            };
        }

        #endregion

        #region Versioning

        /// <summary>
        /// Check for a new MPF version
        /// </summary>
        /// <returns>
        /// Bool representing if the values are different.
        /// String representing the message to display the the user.
        /// String representing the new release URL.
        /// </returns>
        public static (bool different, string message, string? url) CheckForNewVersion()
        {
            try
            {
                // Get current assembly version
                var assemblyVersion = Assembly.GetEntryAssembly()?.GetName()?.Version;
                if (assemblyVersion == null)
                    return (false, "Assembly version could not be determined", null);

                string version = $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";

                // Get the latest tag from GitHub
                var (tag, url) = GetRemoteVersionAndUrl();
                bool different = version != tag && tag != null;

                string message = $"Local version: {version}"
                    + $"{Environment.NewLine}Remote version: {tag}"
                    + (different
                        ? $"{Environment.NewLine}The update URL has been added copied to your clipboard"
                        : $"{Environment.NewLine}You have the newest version!");

                return (different, message, url);
            }
            catch (Exception ex)
            {
                return (false, ex.ToString(), null);
            }
        }

        /// <summary>
        /// Get the current informational version formatted as a string
        /// </summary>
        public static string? GetCurrentVersion()
        {
            try
            {
                var assembly = Assembly.GetEntryAssembly();
                if (assembly == null)
                    return null;

                var assemblyVersion = Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
                return assemblyVersion?.InformationalVersion;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        /// <summary>
        /// Get the latest version of MPF from GitHub and the release URL
        /// </summary>
        private static (string? tag, string? url) GetRemoteVersionAndUrl()
        {
#if NET20 || NET35 || NET40
            // Not supported in .NET Frameworks 2.0, 3.5, or 4.0
            return (null, null);
#else
            using var hc = new System.Net.Http.HttpClient();
#if NET452
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
#endif

            // TODO: Figure out a better way than having this hardcoded...
            string url = "https://api.github.com/repos/SabreTools/MPF/releases/latest";
            var message = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
            message.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:64.0) Gecko/20100101 Firefox/64.0");
            var latestReleaseJsonString = hc.SendAsync(message)?.ConfigureAwait(false).GetAwaiter().GetResult()
                .Content?.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            if (latestReleaseJsonString == null)
                return (null, null);

            var latestReleaseJson = Newtonsoft.Json.Linq.JObject.Parse(latestReleaseJsonString);
            if (latestReleaseJson == null)
                return (null, null);

            var latestTag = latestReleaseJson["tag_name"]?.ToString();
            var releaseUrl = latestReleaseJson["html_url"]?.ToString();

            return (latestTag, releaseUrl);
#endif
        }

        #endregion

        #region PlayStation 3 specific tools

        /// <summary>
        /// Validates a getkey log to check for presence of valid PS3 key
        /// </summary>
        /// <param name="logPath">Path to getkey log file</param>
        /// <param name="key">Output key string, null if not valid</param>
        /// <param name="id">Output disc ID string, null if not valid</param>
        /// <param name="pic">Output PIC string, null if not valid</param>
        /// <returns>True if path to log file contains valid key, false otherwise</returns>
        public static bool ParseGetKeyLog(string? logPath, out string? key, out string? id, out string? pic)
        {
            key = id = pic = null;

            if (string.IsNullOrEmpty(logPath))
                return false;

            try
            {
                if (!File.Exists(logPath))
                    return false;

                // Protect from attempting to read from really long files
                FileInfo logFile = new(logPath);
                if (logFile.Length > 65536)
                    return false;

                // Read from .getkey.log file
                using StreamReader sr = File.OpenText(logPath);

                // Determine whether GetKey was successful
                string? line;
                while ((line = sr.ReadLine()) != null && line.Trim().StartsWith("get_dec_key succeeded!") == false) ;
                if (line == null)
                    return false;

                // Look for Disc Key in log
                while ((line = sr.ReadLine()) != null && line.Trim().StartsWith("disc_key = ") == false) ;

                // If end of file reached, no key found
                if (line == null)
                    return false;

                // Get Disc Key from log
                string discKeyStr = line.Substring("disc_key = ".Length);

                // Validate Disc Key from log
                if (discKeyStr.Length != 32)
                    return false;

                // Convert Disc Key to byte array
                key = discKeyStr;
                if (key == null)
                    return false;

                // Read Disc ID
                while ((line = sr.ReadLine()) != null && line.Trim().StartsWith("disc_id = ") == false) ;

                // If end of file reached, no ID found
                if (line == null)
                    return false;

                // Get Disc ID from log
                string discIDStr = line.Substring("disc_id = ".Length);

                // Validate Disc ID from log
                if (discIDStr.Length != 32)
                    return false;

                // Replace X's in Disc ID with 00000001
                discIDStr = discIDStr.Substring(0, 24) + "00000001";

                // Convert Disc ID to byte array
                id = discIDStr;
                if (id == null)
                    return false;

                // Look for PIC in log
                while ((line = sr.ReadLine()) != null && line.Trim().StartsWith("PIC:") == false) ;

                // If end of file reached, no PIC found
                if (line == null)
                    return false;

                // Get PIC from log
                string discPICStr = "";
                for (int i = 0; i < 9; i++)
                    discPICStr += sr.ReadLine();
                if (discPICStr == null)
                    return false;

                // Validate PIC from log
                if (discPICStr.Length != 264)
                    return false;

                // Convert PIC to byte array
                pic = discPICStr;
                if (pic == null)
                    return false;

                // Double check for warnings in .getkey.log
                while ((line = sr.ReadLine()) != null)
                {
                    string t = line.Trim();
                    if (t.StartsWith("WARNING"))
                        return false;
                    else if (t.StartsWith("SUCCESS"))
                        return true;
                }
            }
            catch
            {
                // We are not concerned with the error
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates a getkey log to check for presence of valid PS3 key
        /// </summary>
        /// <param name="logPath">Path to getkey log file</param>
        /// <param name="key">Output 16 byte disc key, null if not valid</param>
        /// <param name="id">Output 16 byte disc ID, null if not valid</param>
        /// <param name="pic">Output 230 byte PIC, null if not valid</param>
        /// <returns>True if path to log file contains valid key, false otherwise</returns>
        public static bool ParseGetKeyLog(string? logPath, out byte[]? key, out byte[]? id, out byte[]? pic)
        {
            key = id = pic = null;
            if (ParseGetKeyLog(logPath, out string? keyString, out string? idString, out string? picString))
            {
                if (string.IsNullOrEmpty(keyString) || string.IsNullOrEmpty(idString) || string.IsNullOrEmpty(picString) || picString!.Length < 230)
                    return false;

                key = Tools.HexStringToByteArray(keyString);
                id = Tools.HexStringToByteArray(idString);
                pic = Tools.HexStringToByteArray(picString.Substring(0, 230));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Validates a hexadecimal disc ID
        /// </summary>
        /// <param name="discID">String representing hexadecimal disc ID</param>
        /// <returns>True if string is a valid disc ID, false otherwise</returns>
        public static byte[]? ParseDiscID(string? discID)
        {
            if (string.IsNullOrEmpty(discID))
                return null;

            string cleandiscID = discID!.Trim().Replace("\n", string.Empty);

            if (discID!.Length != 32)
                return null;

            // Censor last 4 bytes by replacing with 0x00000001
            cleandiscID = cleandiscID.Substring(0, 24) + "00000001";

            // Convert to byte array, null if invalid hex string
            byte[]? id = Tools.HexStringToByteArray(cleandiscID);

            return id;
        }

        /// <summary>
        /// Validates a key file to check for presence of valid PS3 key
        /// </summary>
        /// <param name="keyPath">Path to key file</param>
        /// <returns>Output 16 byte key, null if not valid</returns>
        public static byte[]? ParseKeyFile(string? keyPath)
        {
            if (string.IsNullOrEmpty(keyPath))
                return null;

            // Try read from key file
            try
            {
                if (!File.Exists(keyPath))
                    return null;

                // Key file must be exactly 16 bytes long
                FileInfo keyFile = new(keyPath);
                if (keyFile.Length != 16)
                    return null;
                byte[] key = new byte[16];

                // Read 16 bytes from Key file
                using FileStream fs = new(keyPath, FileMode.Open, FileAccess.Read);
                using BinaryReader reader = new(fs);
                int numBytes = reader.Read(key, 0, 16);
                if (numBytes != 16)
                    return null;

                return key;
            }
            catch
            {
                // Not concerned with error
                return null;
            }
        }

        /// <summary>
        /// Validates a hexadecimal key
        /// </summary>
        /// <param name="hexKey">String representing hexadecimal key</param>
        /// <returns>Output 16 byte key, null if not valid</returns>
        public static byte[]? ParseHexKey(string? hexKey)
        {
            if (string.IsNullOrEmpty(hexKey))
                return null;

            string cleanHexKey = hexKey!.Trim().Replace("\n", string.Empty);

            if (cleanHexKey.Length != 32)
                return null;

            // Convert to byte array, null if invalid hex string
            byte[]? key = Tools.HexStringToByteArray(cleanHexKey);

            return key;
        }

        /// <summary>
        /// Validates a PIC file path
        /// </summary>
        /// <param name="picPath">Path to PIC file</param>
        /// <returns>Output PIC byte array, null if not valid</returns>
        public static byte[]? ParsePICFile(string? picPath)
        {
            if (string.IsNullOrEmpty(picPath))
                return null;

            // Try read from PIC file
            try
            {
                if (!File.Exists(picPath))
                    return null;

                // PIC file must be at least 115 bytes long
                FileInfo picFile = new(picPath);
                if (picFile.Length < 115)
                    return null;
                byte[] pic = new byte[115];

                // Read 115 bytes from PIC file
                using FileStream fs = new(picPath, FileMode.Open, FileAccess.Read);
                using BinaryReader reader = new(fs);
                int numBytes = reader.Read(pic, 0, 115);
                if (numBytes != 115)
                    return null;

                // Validate that a PIC was read by checking first 6 bytes
                if (pic[0] != 0x10 ||
                    pic[1] != 0x02 ||
                    pic[2] != 0x00 ||
                    pic[3] != 0x00 ||
                    pic[4] != 0x44 ||
                    pic[5] != 0x49)
                    return null;

                return pic;
            }
            catch
            {
                // Not concerned with error
                return null;
            }
        }

        /// <summary>
        /// Validates a PIC
        /// </summary>
        /// <param name="inputPIC">String representing PIC</param>
        /// <returns>Output PIC byte array, null if not valid</returns>
        public static byte[]? ParsePIC(string? inputPIC)
        {
            if (string.IsNullOrEmpty(inputPIC))
                return null;

            string cleanPIC = inputPIC!.Trim().Replace("\n", string.Empty);

            if (cleanPIC.Length < 230)
                return null;

            // Convert to byte array, null if invalid hex string
            byte[]? pic = Tools.HexStringToByteArray(cleanPIC.Substring(0, 230));

            return pic;
        }

        /// <summary>
        /// Validates a string representing a layerbreak value (in sectors)
        /// </summary>
        /// <param name="inputLayerbreak">String representing layerbreak value</param>
        /// <param name="layerbreak">Output layerbreak value, null if not valid</param>
        /// <returns>True if layerbreak is valid, false otherwise</returns>
        public static long? ParseLayerbreak(string? inputLayerbreak)
        {
            if (string.IsNullOrEmpty(inputLayerbreak))
                return null;

            if (!long.TryParse(inputLayerbreak, out long layerbreak))
                return null;

            return ParseLayerbreak(layerbreak);
        }

        /// <summary>
        /// Validates a layerbreak value (in sectors)
        /// </summary>
        /// <param name="inputLayerbreak">Number representing layerbreak value</param>
        /// <param name="layerbreak">Output layerbreak value, null if not valid</param>
        /// <returns>True if layerbreak is valid, false otherwise</returns>
        public static long? ParseLayerbreak(long? layerbreak)
        {
            // Check that layerbreak is positive number and smaller than largest disc size (in sectors)
            if (layerbreak <= 0 || layerbreak > 24438784)
                return null;

            return layerbreak;
        }

        /// <summary>
        /// Converts a CRC32 hash hex string into uint32 representation
        /// </summary>
        /// <param name="inputLayerbreak">Hex string representing CRC32 hash</param>
        /// <param name="layerbreak">Output CRC32 value, null if not valid</param>
        /// <returns>True if CRC32 hash string is valid, false otherwise</returns>
        public static uint? ParseCRC32(string? inputCRC32)
        {
            if (string.IsNullOrEmpty(inputCRC32))
                return null;

            byte[]? crc32 = Tools.HexStringToByteArray(inputCRC32);

            if (crc32 == null || crc32.Length != 4)
                return null;

            return (uint)(0x01000000 * crc32[0] + 0x00010000 * crc32[1] + 0x00000100 * crc32[2] + 0x00000001 * crc32[3]);
        }

        #endregion

        #region Xbox/Xbox360 specific tools

        /// <summary>
        /// Get the XGD1 Master ID (XMID) information
        /// </summary>
        /// <param name="dmi">DMI.bin file location</param>
        /// <returns>String representation of the XGD1 DMI information, empty string on error</returns>
        public static string GetXGD1XMID(string dmi)
        {
            if (!File.Exists(dmi))
                return string.Empty;

            try
            {
                using var br = new BinaryReader(File.OpenRead(dmi));
                br.BaseStream.Seek(8, SeekOrigin.Begin);
                return new string(br.ReadChars(8));
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Get the XGD2/3 Master ID (XeMID) information
        /// </summary>
        /// <param name="dmi">DMI.bin file location</param>
        /// <returns>String representation of the XGD2/3 DMI information, empty string on error</returns>
        public static string GetXGD23XeMID(string dmi)
        {
            if (!File.Exists(dmi))
                return string.Empty;

            try
            {
                using var br = new BinaryReader(File.OpenRead(dmi));
                br.BaseStream.Seek(64, SeekOrigin.Begin);
                return new string(br.ReadChars(14));
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Get XGD type from SS.bin file
        /// </summary>
        /// <param name="ss"></param>
        /// <param name="xgdType"></param>
        /// <returns></returns>
        public static bool GetXGDType(string? ss, out int xgdType)
        {
            xgdType = 0;

            if (string.IsNullOrEmpty(ss) || !File.Exists(ss))
                return false;

            using FileStream fs = File.OpenRead(ss);
            byte[] buf = new byte[3];
            int numBytes = fs.Read(buf, 13, 16);

            if (numBytes != 3)
                return false;

            return GetXGDType(buf, out xgdType);
        }

        /// <summary>
        /// Get XGD type from SS.bin sector
        /// </summary>
        /// <param name="ss">Byte array of SS.bin sector</param>
        /// <param name="xgdType">XGD type</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool GetXGDType(byte[] ss, out int xgdType)
        {
            xgdType = 0;

            // Concatenate the last three values
            long lastThree = (((ss[13] << 8) | ss[14]) << 8) | ss[15];

            // Return XGD type based on value
            switch (lastThree)
            {
                case 0x2033AF:
                    xgdType = 1;
                    return true;
                case 0x20339F:
                    xgdType = 2;
                    return true;
                case 0x238E0F:
                    xgdType = 3;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determine if a given SS has already been cleaned
        /// </summary>
        /// <param name="ss">Byte array of SS sector</param>
        /// <returns>True if SS is clean, false otherwise</returns>
        public static bool IsCleanSS(byte[] ss)
        {
            if (ss.Length != 2048)
                return false;

            if (!GetXGDType(ss, out int xgdType))
                return false;

            if (xgdType == 3 && ss.Skip(32).Take(72).All(x => x == 0))
            {
                // Check for a cleaned SSv2

                int rtOffset = 0x24;

                if (ss[rtOffset + 36] != 0x01)
                    return false;
                if (ss[rtOffset + 37] != 0x00)
                    return false;
                if (ss[rtOffset + 39] != 0x01)
                    return false;
                if (ss[rtOffset + 40] != 0x00)
                    return false;
                if (ss[rtOffset + 45] != 0x5B)
                    return false;
                if (ss[rtOffset + 46] != 0x00)
                    return false;
                if (ss[rtOffset + 48] != 0x5B)
                    return false;
                if (ss[rtOffset + 49] != 0x00)
                    return false;
                if (ss[rtOffset + 54] != 0xB5)
                    return false;
                if (ss[rtOffset + 55] != 0x00)
                    return false;
                if (ss[rtOffset + 57] != 0xB5)
                    return false;
                if (ss[rtOffset + 58] != 0x00)
                    return false;
                if (ss[rtOffset + 63] != 0x0F)
                    return false;
                if (ss[rtOffset + 64] != 0x01)
                    return false;
                if (ss[rtOffset + 66] != 0x0F)
                    return false;
                if (ss[rtOffset + 67] != 0x01)
                    return false;
            }
            else
            {
                // Check for a cleaned SSv1

                int rtOffset = 0x204;

                if (ss[rtOffset + 36] != 0x01)
                    return false;
                if (ss[rtOffset + 37] != 0x00)
                    return false;
                if (xgdType == 2 && ss[rtOffset + 39] != 0x00)
                    return false;
                if (xgdType == 2 && ss[rtOffset + 40] != 0x00)
                    return false;
                if (ss[rtOffset + 45] != 0x5B)
                    return false;
                if (ss[rtOffset + 46] != 0x00)
                    return false;
                if (xgdType == 2 && ss[rtOffset + 48] != 0x00)
                    return false;
                if (xgdType == 2 && ss[rtOffset + 49] != 0x00)
                    return false;
                if (ss[rtOffset + 54] != 0xB5)
                    return false;
                if (ss[rtOffset + 55] != 0x00)
                    return false;
                if (xgdType == 2 && ss[rtOffset + 57] != 0x00)
                    return false;
                if (xgdType == 2 && ss[rtOffset + 58] != 0x00)
                    return false;
                if (ss[rtOffset + 63] != 0x0F)
                    return false;
                if (ss[rtOffset + 64] != 0x01)
                    return false;
                if (xgdType == 2 && ss[rtOffset + 66] != 0x00)
                    return false;
                if (xgdType == 2 && ss[rtOffset + 67] != 0x00)
                    return false;
            }

            // All angles are as expected, it is clean
            return true;
        }

        /// <summary>
        /// Clean a rawSS.bin file and write it to a file
        /// </summary>
        /// <param name="rawSS">Path to the raw SS file to read from</param>
        /// <param name="cleanSS">Path to the clean SS file to write to</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool CleanSS(string rawSS, string cleanSS)
        {
            if (!File.Exists(rawSS))
                return false;

            byte[] ss = File.ReadAllBytes(rawSS);
            if (ss.Length != 2048)
                return false;

            if (!CleanSS(ss))
                return false;

            File.WriteAllBytes(cleanSS, ss);
            return true;
        }

        /// <summary>
        /// Fix a SS sector to its predictable clean form.
        /// With help from ss_sector_range
        /// </summary>
        /// <param name="ss">Byte array of raw SS sector</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool CleanSS(byte[] ss)
        {
            // Must be entire sector
            if (ss.Length != 2048)
                return false;

            // Determine XGD type
            if (!GetXGDType(ss, out int xgdType))
                return false;

            switch (xgdType)
            {
                case 1:
                    // Leave Original Xbox SS.bin unchanged
                    return true;

                case 2:
                    // Fix standard SSv1 ss.bin
                    ss[552] = 1;   // 0x01
                    ss[553] = 0;   // 0x00
                    ss[555] = 0;   // 0x00
                    ss[556] = 0;   // 0x00

                    ss[561] = 91;  // 0x5B
                    ss[562] = 0;   // 0x00
                    ss[564] = 0;   // 0x00
                    ss[565] = 0;   // 0x00

                    ss[570] = 181; // 0xB5
                    ss[571] = 0;   // 0x00
                    ss[573] = 0;   // 0x00
                    ss[574] = 0;   // 0x00

                    ss[579] = 15;  // 0x0F
                    ss[580] = 1;   // 0x01
                    ss[582] = 0;   // 0x00
                    ss[583] = 0;   // 0x00
                    return true;

                case 3:
                    // Determine if XGD3 SS.bin is SSv1 (Kreon) or SSv2 (0800)
                    bool ssv2 = ss.Skip(32).Take(72).All(x => x == 0);

                    if (ssv2)
                    {
                        ss[72] = 1;   // 0x01
                        ss[73] = 0;   // 0x00
                        ss[75] = 1;   // 0x01
                        ss[76] = 0;   // 0x00

                        ss[81] = 91;  // 0x5B
                        ss[82] = 0;   // 0x00
                        ss[84] = 91;  // 0x5B
                        ss[85] = 0;   // 0x00

                        ss[90] = 181; // 0xB5
                        ss[91] = 0;   // 0x00
                        ss[93] = 181; // 0xB5
                        ss[94] = 0;   // 0x00

                        ss[99] = 15;  // 0x0F
                        ss[100] = 1;   // 0x01
                        ss[102] = 15;  // 0x0F
                        ss[103] = 1;   // 0x01
                    }
                    else
                    {
                        ss[552] = 1;   // 0x01
                        ss[553] = 0;   // 0x00

                        ss[561] = 91;  // 0x5B
                        ss[562] = 0;   // 0x00

                        ss[570] = 181; // 0xB5
                        ss[571] = 0;   // 0x00

                        ss[579] = 15;  // 0x0F
                        ss[580] = 1;   // 0x01
                    }

                    return true;

                default:
                    // Unknown XGD type
                    return false;
            }
        }

        /// <summary>
        /// Get Security Sector ranges from SS.bin
        /// </summary>
        /// <param name="ssBin">Path to SS.bin file</param>
        /// <returns>Sector ranges if found, null otherwise</returns>
        public static string? GetSSRanges(string ssBin)
        {
            if (!File.Exists(ssBin))
                return null;

            byte[] ss = File.ReadAllBytes(ssBin);
            if (ss.Length != 2048)
                return null;

            return GetSSRanges(ss);
        }

        /// <summary>
        /// Get Security Sector ranges from SS sector.
        /// With help from ss_sector_range
        /// </summary>
        /// <param name="ss">Byte array of SS sector</param>
        /// <returns>Sector ranges if found, null otherwise</returns>
        public static string? GetSSRanges(byte[] ss)
        {
            if (ss.Length != 2048)
                return null;

            if (!GetXGDType(ss, out int xgdType))
                return null;

            //uint numRanges = ss[1632];
            uint numRanges;
            if (xgdType == 1)
                numRanges = 16;
            else
                numRanges = 4;


            uint[] startLBA = new uint[numRanges];
            uint[] endLBA = new uint[numRanges];
            for (uint i = 0; i < numRanges; i++)
            {
                // Determine range Physical Sector Number
                uint startPSN = (uint)((((ss[i * 9 + 1636] << 8) | ss[i * 9 + 1637]) << 8) | ss[i * 9 + 1638]);
                uint endPSN = (uint)((((ss[i * 9 + 1639] << 8) | ss[i * 9 + 1640]) << 8) | ss[i * 9 + 1641]);
                
                // Determine range Logical Sector Number
                if (xgdType == 1 && startPSN >= (1913776 + 0x030000))
                {
                    // Layer 1 of XGD1
                    startLBA[i] = (1913776 + 0x030000) * 2 - (startPSN ^ 0xFFFFFF) - 0x030000 - 1;
                    endLBA[i] = (1913776 + 0x030000) * 2 - (endPSN ^ 0xFFFFFF) - 0x030000 - 1;
                }
                else if (xgdType > 1 && startPSN >= (1913760 + 0x030000))
                {
                    // Layer 1 of XGD2 or XGD3
                    startLBA[i] = (1913760 + 0x030000) * 2 - (startPSN ^ 0xFFFFFF) - 0x030000 - 1;
                    endLBA[i] = (1913760 + 0x030000) * 2 - (endPSN ^ 0xFFFFFF) - 0x030000 - 1;
                }
                else
                {
                    // Layer 0
                    startLBA[i] = startPSN - 0x030000;
                    endLBA[i] = endPSN - 0x030000;
                }
            }

            // Sort ranges for XGD1
            if (xgdType == 1)
                Array.Sort(startLBA, endLBA);

            // Represent ranges as string
            string? ranges = null;
            if (xgdType == 1)
            {
                for (int i = 0; i < 16; i++)
                {
                    ranges += $"{startLBA[i]}-{endLBA[i]}";
                    if (i != numRanges - 1)
                        ranges += "\r\n";
                }
            }
            else
            {
                ranges = $"{startLBA[0]}-{endLBA[0]}\r\n{startLBA[3]}-{endLBA[3]}";
            }

            return ranges;
        }

        #endregion
    }
}
