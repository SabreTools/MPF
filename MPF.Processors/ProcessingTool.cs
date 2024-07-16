using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using SabreTools.Models.Logiqx;
using SabreTools.Models.PIC;
using SabreTools.RedumpLib.Data;

namespace MPF.Processors
{
    /// <summary>
    /// Includes processing-specific utility functionality
    /// </summary>
    public static class ProcessingTool
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

        #region Information Extraction

        /// <summary>
        /// Generate the proper datfile from the input Datafile, if possible
        /// </summary>
        /// <param name="datafile">.dat file location</param>
        /// <returns>Relevant pieces of the datfile, null on error</returns>
        public static string? GenerateDatfile(Datafile? datafile)
        {
            // If we don't have a valid datafile, we can't do anything
            if (datafile?.Game == null || datafile.Game.Length == 0)
                return null;

            var roms = datafile.Game[0].Rom;
            if (roms == null || roms.Length == 0)
                return null;

            // Otherwise, reconstruct the hash data with only the required info
            try
            {
                string datString = string.Empty;
                for (int i = 0; i < roms.Length; i++)
                {
                    var rom = roms[i];
                    datString += $"<rom name=\"{rom.Name}\" size=\"{rom.Size}\" crc=\"{rom.CRC}\" md5=\"{rom.MD5}\" sha1=\"{rom.SHA1}\" />\n";
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
        /// Get the Base64 representation of a string
        /// </summary>
        /// <param name="content">String content to encode</param>
        /// <returns>Base64-encoded contents, if possible</returns>
        public static string? GetBase64(string? content)
        {
            if (string.IsNullOrEmpty(content))
                return null;

            byte[] temp = Encoding.UTF8.GetBytes(content);
            return Convert.ToBase64String(temp);
        }

        /// <summary>
        /// Get Datafile from a standard DAT
        /// </summary>
        /// <param name="dat">Path to the DAT file to parse</param>
        /// <returns>Filled Datafile on success, null on error</returns>
        public static Datafile? GetDatafile(string? dat)
        {
            // If there's no path, we can't read the file
            if (string.IsNullOrEmpty(dat))
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
#if NET40_OR_GREATER || NETCOREAPP
                    DtdProcessing = DtdProcessing.Ignore,
#endif
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
        public static DiscInformation? GetDiscInformation(string pic)
        {
            try
            {
                return SabreTools.Serialization.Deserializers.PIC.DeserializeFile(pic);
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
        public static DateTime? GetFileModifiedDate(string? filename, bool fallback = false)
        {
            if (string.IsNullOrEmpty(filename))
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
        public static string? GetFullFile(string filename, bool binary = false)
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
        public static bool GetISOHashValues(Datafile? datafile, out long size, out string? crc32, out string? md5, out string? sha1)
        {
            size = -1; crc32 = null; md5 = null; sha1 = null;

            if (datafile?.Game == null || datafile.Game.Length == 0)
                return false;

            var roms = datafile.Game[0].Rom;
            if (roms == null || roms.Length == 0)
                return false;

            var rom = roms[0];

            _ = Int64.TryParse(rom.Size, out size);
            crc32 = rom.CRC;
            md5 = rom.MD5;
            sha1 = rom.SHA1;

            return true;
        }

        /// <summary>
        /// Get the split values for ISO-based media
        /// </summary>
        /// <param name="hashData">String representing the combined hash data</param>
        /// <returns>True if extraction was successful, false otherwise</returns>
        public static bool GetISOHashValues(string? hashData, out long size, out string? crc32, out string? md5, out string? sha1)
        {
            size = -1; crc32 = null; md5 = null; sha1 = null;

            if (string.IsNullOrEmpty(hashData))
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
        public static bool GetLayerbreaks(DiscInformation? di, out long? layerbreak1, out long? layerbreak2, out long? layerbreak3)
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

#if NET20 || NET35 || NET40
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
        /// Get the PIC identifier from the first disc information unit, if possible
        /// </summary>
        /// <param name="di">Disc information containing the data</param>
        /// <returns>String representing the PIC identifier, null on error</returns>
        public static string? GetPICIdentifier(DiscInformation? di)
        {
            // If we don't have valid disc information, we can't do anything
            if (di?.Units == null || di.Units.Length <= 1)
                return null;

            // We assume the identifier is consistent across all units
            return di.Units[0]?.Body?.DiscTypeIdentifier;
        }

        #endregion

        #region Category Extraction

        /// <summary>
        /// Determine the category based on the UMDImageCreator string
        /// </summary>
        /// <param name="region">String representing the category</param>
        /// <returns>Category, if possible</returns>
        public static DiscCategory? GetUMDCategory(string? category)
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

        /// <summary>
        /// Determine the region based on the XGD serial character
        /// </summary>
        /// <param name="region">Character denoting the region</param>
        /// <returns>Region, if possible</returns>
        public static Region? GetXGDRegion(char? region)
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

                key = HexStringToByteArray(keyString);
                id = HexStringToByteArray(idString);
                pic = HexStringToByteArray(picString.Substring(0, 230));
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
            byte[]? id = HexStringToByteArray(cleandiscID);

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
            byte[]? key = HexStringToByteArray(cleanHexKey);

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
            byte[]? pic = HexStringToByteArray(cleanPIC.Substring(0, 230));

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

            byte[]? crc32 = HexStringToByteArray(inputCRC32);

            if (crc32 == null || crc32.Length != 4)
                return null;

            return (uint)(0x01000000 * crc32[0] + 0x00010000 * crc32[1] + 0x00000100 * crc32[2] + 0x00000001 * crc32[3]);
        }

        #endregion

        #region Xbox and Xbox 360

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