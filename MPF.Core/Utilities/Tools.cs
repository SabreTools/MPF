using System;
using System.IO;
using System.Net;
using System.Reflection;
using MPF.Core.Data;
using Newtonsoft.Json.Linq;
using SabreTools.RedumpLib.Data;

namespace MPF.Core.Utilities
{
    public static class Tools
    {
        #region Byte Arrays

        /// <summary>
        /// Search for a byte array in another array
        /// </summary>
        public static bool Contains(this byte[] stack, byte[] needle, out int position, int start = 0, int end = -1)
        {
            // Initialize the found position to -1
            position = -1;

            // If either array is null or empty, we can't do anything
            if (stack == null || stack.Length == 0 || needle == null || needle.Length == 0)
                return false;

            // If the needle array is larger than the stack array, it can't be contained within
            if (needle.Length > stack.Length)
                return false;

            // If start or end are not set properly, set them to defaults
            if (start < 0)
                start = 0;
            if (end < 0)
                end = stack.Length - needle.Length;

            for (int i = start; i < end; i++)
            {
                if (stack.EqualAt(needle, i))
                {
                    position = i;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// See if a byte array starts with another
        /// </summary>
        public static bool StartsWith(this byte[] stack, byte[] needle)
        {
            return stack.Contains(needle, out int _, start: 0, end: 1);
        }

        /// <summary>
        /// Get if a stack at a certain index is equal to a needle
        /// </summary>
        private static bool EqualAt(this byte[] stack, byte[] needle, int index)
        {
            // If we're too close to the end of the stack, return false
            if (needle.Length >= stack.Length - index)
                return false;

            for (int i = 0; i < needle.Length; i++)
            {
                if (stack[i + index] != needle[i])
                    return false;
            }

            return true;
        }

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
        /// Verify that, given a system and a media type, they are correct
        /// </summary>
        public static Result GetSupportStatus(RedumpSystem? system, MediaType? type)
        {
            // No system chosen, update status
            if (system == null)
                return Result.Failure("Please select a valid system");

            // If we're on an unsupported type, update the status accordingly
            return type switch
            {
                // Fully supported types
                MediaType.BluRay
                    or MediaType.CDROM
                    or MediaType.DVD
                    or MediaType.FloppyDisk
                    or MediaType.HardDisk
                    or MediaType.CompactFlash
                    or MediaType.SDCard
                    or MediaType.FlashDrive
                    or MediaType.HDDVD => Result.Success($"{type.LongName()} ready to dump"),

                // Partially supported types
                MediaType.GDROM
                    or MediaType.NintendoGameCubeGameDisc
                    or MediaType.NintendoWiiOpticalDisc => Result.Success($"{type.LongName()} partially supported for dumping"),

                // Special case for other supported tools
                MediaType.UMD => Result.Failure($"{type.LongName()} supported for submission info parsing"),

                // Specifically unknown type
                MediaType.NONE => Result.Failure($"Please select a valid media type"),

                // Undumpable but recognized types
                _ => Result.Failure($"{type.LongName()} media are not supported for dumping"),
            };
        }

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
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
#endif

            // TODO: Figure out a better way than having this hardcoded...
            string url = "https://api.github.com/repos/SabreTools/MPF/releases/latest";
            var message = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
            message.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:64.0) Gecko/20100101 Firefox/64.0");
            var latestReleaseJsonString = hc.SendAsync(message)?.ConfigureAwait(false).GetAwaiter().GetResult()
                .Content?.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            if (latestReleaseJsonString == null)
                return (null, null);

            var latestReleaseJson = JObject.Parse(latestReleaseJsonString);
            if (latestReleaseJson == null)
                return (null, null);

            var latestTag = latestReleaseJson["tag_name"]?.ToString();
            var releaseUrl = latestReleaseJson["html_url"]?.ToString();

            return (latestTag, releaseUrl);
#endif
        }

        #endregion

        #region PlayStation 3

        /// <summary>
        /// Validates a getkey log to check for presence of valid PS3 key
        /// </summary>
        /// <param name="logPath">Path to getkey log file</param>
        /// <param name="key">Output 16 byte key, null if not valid</param>
        /// <returns>True if path to log file contains valid key, false otherwise</returns>
        public static bool ParseGetKeyLog(string? logPath, out byte[]? key, out byte[]? id, out byte[]? pic)
        {
            key = null;
            id = null;
            pic = null;

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
                key = Tools.HexStringToByteArray(discKeyStr);
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
                id = Tools.HexStringToByteArray(discIDStr);
                if (id == null)
                    return false;

                // Look for PIC in log
                while ((line = sr.ReadLine()) != null && line.Trim().StartsWith("PIC:") == false) ;

                // If end of file reached, no PIC found
                if (line == null)
                    return false;

                // Get PIC from log
                string discPICStr = "";
                for (int i = 0; i < 8; i++)
                    discPICStr += sr.ReadLine();
                if (discPICStr == null)
                    return false;

                // Validate PIC from log
                if (discPICStr.Length != 256)
                    return false;

                // Convert PIC to byte array
                pic = Tools.HexStringToByteArray(discPICStr.Substring(0, 230));
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
    }
}
