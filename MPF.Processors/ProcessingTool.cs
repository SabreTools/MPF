using System;
using System.IO;
#if NET35_OR_GREATER || NETCOREAPP
using System.Linq;
#endif
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using SabreTools.Hashing;
using SabreTools.IO.Extensions;
using SabreTools.Data.Models.Logiqx;
using SabreTools.Data.Models.PIC;
using SabreTools.RedumpLib.Data;

namespace MPF.Processors
{
    /// <summary>
    /// Includes processing-specific utility functionality
    /// </summary>
    public static class ProcessingTool
    {
        #region Constants

        /// <summary>
        /// Shift-JIS encoding for detection and conversion
        /// </summary>
        private static readonly Encoding ShiftJIS = Encoding.GetEncoding(932);

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
                // Absorb the exception
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
            // If the file doesn't exist, we can't read it
            if (string.IsNullOrEmpty(dat))
                return null;
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
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Gets disc information from a PIC file
        /// </summary>
        /// <param name="pic">Path to a PIC.bin file</param>
        /// <returns>Filled DiscInformation on success, null on error</returns>
        /// <remarks>This omits the emergency brake information, if it exists</remarks>
        public static DiscInformation? GetDiscInformation(string? pic)
        {
            try
            {
                return new SabreTools.Serialization.Readers.PIC().Deserialize(pic);
            }
            catch
            {
                // Absorb the exception
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
                return fallback ? DateTime.UtcNow : null;
            if (!File.Exists(filename))
                return fallback ? DateTime.UtcNow : null;

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
            if (string.IsNullOrEmpty(filename))
                return null;
            if (!File.Exists(filename))
                return null;

            // Read the entire file as bytes
            byte[] bytes = File.ReadAllBytes(filename);

            // If we're reading as binary
            if (binary)
                return BitConverter.ToString(bytes).Replace("-", string.Empty);

            // If we're reading as text
            return NormalizeShiftJIS(bytes);
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

            _ = long.TryParse(rom.Size, out size);
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
                _ = long.TryParse(m.Groups[1].Value, out size);
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

            // Wrap big-endian reading
            static int ReadFromArrayBigEndian(byte[]? bytes, int offset)
            {
                if (bytes == null)
                    return default;

                return bytes.ReadInt32BigEndian(ref offset);
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
            if (di?.Units == null || di.Units.Length < 1)
                return null;

            // Assume the identifier is consistent across all units
            return di.Units[0]?.Body?.DiscTypeIdentifier;
        }

        /// <summary>
        /// Normalize a byte array that may contain Shift-JIS characters
        /// </summary>
        /// <param name="contents">String as a byte array to normalize</param>
        /// <returns>Normalized version of a string</returns>
        public static string NormalizeShiftJIS(byte[]? contents)
        {
            // Invalid arrays are passed as-is
            if (contents == null || contents.Length == 0)
                return string.Empty;

#if NET462_OR_GREATER || NETCOREAPP
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif

            // If the line contains Shift-JIS characters
            if (BytesContainsShiftJIS(contents))
                return ShiftJIS.GetString(contents);

            return Encoding.UTF8.GetString(contents);
        }

        /// <summary>
        /// Determine if a byte array contains Shift-JIS encoded characters
        /// </summary>
        /// <param name="line">Byte array to check for Shift-JIS encoding</param>
        /// <returns>True if the byte array contains Shift-JIS characters, false otherwise</returns>
        /// <see href="https://www.lemoda.net/c/detect-shift-jis/"/>
        internal static bool BytesContainsShiftJIS(byte[] bytes)
        {
            // Invalid arrays do not count
            if (bytes == null || bytes.Length == 0)
                return false;

            // Loop through and check each pair of bytes
            for (int i = 0; i < bytes.Length - 1; i++)
            {
                byte first = bytes[i];
                byte second = bytes[i + 1];

                if ((first >= 0x81 && first <= 0x84) ||
                    (first >= 0x87 && first <= 0x9F))
                {
                    if (second >= 0x40 && second <= 0x9E)
                        return true;
                    else if (second >= 0x9F && second <= 0xFC)
                        return true;
                }
                else if (first >= 0xE0 && first <= 0xEF)
                {
                    if (second >= 0x40 && second <= 0x9E)
                        return true;
                    else if (second >= 0x9F && second <= 0xFC)
                        return true;
                }
            }

            return false;
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
            return category?.ToLowerInvariant() switch
            {
                "(game)" => DiscCategory.Games,
                "(video)" => DiscCategory.Video,
                "(audio)" => DiscCategory.Audio,
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

        #region PlayStation specific tools

        /// <summary>
        /// Get if LibCrypt data is detected in the subchannel file, if possible
        /// </summary>
        /// <param name="subPath">Path to the subchannel file</param>
        /// <returns>True if LibCrypt was detected, false otherwise</returns>
        public static bool DetectLibCrypt(string? subPath)
        {
            if (string.IsNullOrEmpty(subPath))
                return false;

            // Create conversion delegates
            byte _btoi(byte b) => (byte)(b / 16 * 10 + b % 16);
            byte _itob(byte i) => (byte)(i / 10 * 16 + i % 10);

            try
            {
                if (!File.Exists(subPath))
                    return false;

                // Open the subfile
                var subFile = File.OpenRead(subPath);

                // Check the size
                long size = subFile.Length;
                if (size % 96 != 0)
                    return false;

                // Setup loop variables
                byte[] expected = new byte[12];
                uint time = 0;

                // Process sector data
                for (uint sector = 150; sector < ((size / 96) + 150); sector++)
                {
                    // Read the sector header
                    subFile.Seek(12, SeekOrigin.Current);
                    byte[] actual = subFile.ReadBytes(12);

                    // Skip the rest of the data for the sector
                    subFile.Seek(72, SeekOrigin.Current);

                    // New track
                    if ((_btoi(actual[1]) == (_btoi(expected[1]) + 1)) && (actual[2] == 0 || actual[2] == 1))
                    {
                        Array.Copy(actual, expected, 6);
                        time = (uint)((_btoi((byte)(actual[3] * 60)) + _btoi(actual[4])) * 75) + _btoi(actual[5]);
                    }

                    // New index
                    else if (_btoi(actual[2]) == (_btoi(expected[2]) + 1) && actual[1] == expected[1])
                    {
                        Array.Copy(actual, 2, expected, 2, 4);
                        time = (uint)((_btoi((byte)(actual[3] * 60)) + _btoi(actual[4])) * 75) + _btoi(actual[5]);
                    }

                    // MSF1 [3-5]
                    else
                    {
                        if (expected[2] == 0)
                            time--;
                        else
                            time++;

                        expected[3] = _itob((byte)(time / 60 / 75));
                        expected[4] = _itob((byte)((time / 75) % 60));
                        expected[5] = _itob((byte)(time % 75));
                    }

                    // Force skip [6]
                    actual[6] = expected[6] = 0;

                    // MSF2 [7-9]
                    expected[7] = _itob((byte)(sector / 60 / 75));
                    expected[8] = _itob((byte)((sector / 75) % 60));
                    expected[9] = _itob((byte)(sector % 75));

                    // CRC-16 [10-11] -- TODO: Ensure byte order is correct
                    var crcWrapper = new HashWrapper(HashType.CRC16);
                    crcWrapper.Process(expected, 0, 10);
                    byte[] crc = crcWrapper.CurrentHashBytes!;
                    expected[10] = crc[0];
                    expected[11] = crc[1];

                    // If a subchannel mismatch is detected
                    if (!actual.EqualsExactly(expected))
                        return true;
                }
            }
            catch
            {
                // Absorb the exception
                return false;
            }

            return false;
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
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                string discKeyStr = line["disc_key = ".Length..];
#else
                string discKeyStr = line.Substring("disc_key = ".Length);
#endif

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
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                string discIDStr = line["disc_id = ".Length..];
#else
                string discIDStr = line.Substring("disc_id = ".Length);
#endif

                // Validate Disc ID from log
                if (discIDStr.Length != 32)
                    return false;

                // Replace X's in Disc ID with 00000001
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                discIDStr = $"{discIDStr[..24]}00000001";
#else
                discIDStr = discIDStr.Substring(0, 24) + "00000001";
#endif

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
                // Absorb the exception
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

                key = keyString.FromHexString();
                id = idString.FromHexString();
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                pic = picString[..230].FromHexString();
#else
                pic = picString.Substring(0, 230).FromHexString();
#endif
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
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
            cleandiscID = $"{cleandiscID[..24]}00000001";
#else
            cleandiscID = cleandiscID.Substring(0, 24) + "00000001";
#endif

            // Convert to byte array, null if invalid hex string
            return cleandiscID.FromHexString();
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
            return cleanHexKey.FromHexString();
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

            string cleanPIC = inputPIC!.Trim().Replace("\n", string.Empty).Replace("\r", string.Empty);

            if (cleanPIC.Length < 230)
                return null;

            // Convert to byte array, null if invalid hex string
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
            return cleanPIC[..230].FromHexString();
#else
            return cleanPIC.Substring(0, 230).FromHexString();
#endif
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

            byte[]? crc32 = inputCRC32.FromHexString();
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
        public static string GetXMID(string dmi)
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
        public static string GetXeMID(string dmi)
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

            if (string.IsNullOrEmpty(ss))
                return false;

            if (!File.Exists(ss))
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
        /// Determine if a given SS.bin is valid (Known XGD type and SSv2 if XGD3)
        /// </summary>
        /// <param name="ssPath">Path to the SS file to check</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidSS(string ssPath)
        {
            if (!File.Exists(ssPath))
                return false;

            byte[] ss = File.ReadAllBytes(ssPath);
            if (ss.Length != 2048)
                return false;

            return IsValidSS(ss);
        }

        /// <summary>
        /// Determine if a given SS is valid (2048 bytes, known XGD type, and SSv2 if XGD3)
        /// </summary>
        /// <param name="ss">Byte array of SS sector</param>
        /// <returns>True if SS is valid, false otherwise</returns>
        public static bool IsValidSS(byte[] ss)
        {
            // Check 1 sector long
            if (ss.Length != 2048)
                return false;

            // Must be a valid XGD type
            if (!GetXGDType(ss, out int xgdType))
                return false;

            // Only continue to check SSv2 for XGD3
            if (xgdType != 3)
                return true;

            // Determine if XGD3 SS.bin is SSv1 (Original Kreon) or SSv2 (0800 / Repaired Kreon)
#if NET20
            var checkArr = new byte[72];
            Array.Copy(ss, 32, checkArr, 0, 72);
            return Array.Exists(checkArr, x => x != 0);
#else
            return ss.Skip(32).Take(72).Any(x => x != 0);
#endif
        }

        /// <summary>
        /// Determine if a given SS.bin is valid but contains zeroed challenge responses
        /// </summary>
        /// <param name="ssPath">Path to the SS file to check</param>
        /// <returns>True if valid but partial SS.bin, false otherwise</returns>
        public static bool IsValidPartialSS(string ssPath)
        {
            if (!File.Exists(ssPath))
                return false;

            byte[] ss = File.ReadAllBytes(ssPath);
            if (ss.Length != 2048)
                return false;

            return IsValidPartialSS(ss);
        }

        /// <summary>
        /// Determine if a given SS is valid but contains zeroed challenge responses
        /// </summary>
        /// <param name="ss">Byte array of SS sector</param>
        /// <returns>True if SS is a valid but partial SS, false otherwise</returns>
        public static bool IsValidPartialSS(byte[] ss)
        {
            // Check 1 sector long
            if (ss.Length != 2048)
                return false;

            // Must be a valid XGD type
            if (!GetXGDType(ss, out int xgdType))
                return false;

            // Determine challenge table offset, XGD1 is never partial
            int ccrt_offset = 0;
            if (xgdType == 1)
                return false;
            else if (xgdType == 2)
                ccrt_offset = 0x200;
            else if (xgdType == 3)
                ccrt_offset = 0x20;

            int[] entry_offsets = [0, 9, 18, 27, 36, 45, 54, 63];
            int[] entry_lengths = [8, 8, 8, 8, 4, 4, 4, 4];
            for (int i = 0; i < entry_offsets.Length; i++)
            {
                bool emptyResponse = true;
                for (int b = 0; b < entry_lengths[i]; b++)
                {
                    if (ss[ccrt_offset + entry_offsets[i] + b] != 0x00)
                    {
                        emptyResponse = false;
                        break;
                    }
                }
                if (emptyResponse)
                    return true;
            }

            return false;
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

#if NET20
            var checkArr = new byte[72];
            Array.Copy(ss, 32, checkArr, 0, 72);
            if (xgdType == 3 && Array.Exists(checkArr, x => x != 0))
#else
            if (xgdType == 3 && ss.Skip(32).Take(72).Any(x => x != 0))
#endif
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

            // Must be a valid SS file
            if (!IsValidSS(ss))
                return false;

            // Determine XGD type
            if (!GetXGDType(ss, out int xgdType))
                return false;

            // Determine if XGD3 SS.bin is SSv1 (Original Kreon) or SSv2 (0800 / Repaired Kreon)
#if NET20
            var checkArr = new byte[72];
            Array.Copy(ss, 32, checkArr, 0, 72);
            bool ssv2 = Array.Exists(checkArr, x => x != 0);
#else
            bool ssv2 = ss.Skip(32).Take(72).Any(x => x != 0);
#endif

            // Do not produce an SS hash for bad SS (SSv1 XGD3 / Unrepaired Kreon SS)
            if (xgdType == 3 && !ssv2)
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
                else if (xgdType == 2 && startPSN >= (1913760 + 0x030000))
                {
                    // Layer 1 of XGD2
                    startLBA[i] = (1913760 + 0x030000) * 2 - (startPSN ^ 0xFFFFFF) - 0x030000 - 1;
                    endLBA[i] = (1913760 + 0x030000) * 2 - (endPSN ^ 0xFFFFFF) - 0x030000 - 1;
                }
                else if (xgdType == 3 && startPSN >= (2133520 + 0x030000))
                {
                    // Layer 1 of XGD3
                    startLBA[i] = (2133520 + 0x030000) * 2 - (startPSN ^ 0xFFFFFF) - 0x030000 - 1;
                    endLBA[i] = (2133520 + 0x030000) * 2 - (endPSN ^ 0xFFFFFF) - 0x030000 - 1;
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
