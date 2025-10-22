using System.Collections.Generic;
using System.IO;
using MPF.Processors.OutputFiles;
using SabreTools.Data.Models.Logiqx;
using SabreTools.Hashing;
using SabreTools.IO.Extensions;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;

namespace MPF.Processors
{
    /// <summary>
    /// Represents processing Xbox Backup Creator outputs
    /// </summary>
    public sealed class XboxBackupCreator : BaseProcessor
    {
        /// <inheritdoc/>
        public XboxBackupCreator(RedumpSystem? system) : base(system) { }

        #region BaseProcessor Implementations

        /// <inheritdoc/>
        public override MediaType? DetermineMediaType(string? outputDirectory, string outputFilename)
            => MediaType.DVD;

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, MediaType? mediaType, string basePath, bool redumpCompat)
        {
            // Get output directory
            string outputDirectory = Path.GetDirectoryName(basePath) ?? string.Empty;

            // Get log filename
            string? logPath = GetLogName(outputDirectory);
            if (string.IsNullOrEmpty(logPath))
                return;

            // XBC dump info
            info.DumpingInfo.DumpingProgram ??= string.Empty;
            info.DumpingInfo.DumpingProgram += $" {GetVersion(logPath) ?? "Unknown Version"}";
            info.DumpingInfo.DumpingDate = ProcessingTool.GetFileModifiedDate(logPath)?.ToString("yyyy-MM-dd HH:mm:ss");
            info.DumpingInfo.Model = GetDrive(logPath) ?? "Unknown Drive";

            // Get the Datafile information
            Datafile? datafile = GenerateDatafile($"{basePath}.iso");
            info.TracksAndWriteOffsets!.ClrMameProData = ProcessingTool.GenerateDatfile(datafile);

            // Get the individual hash data, as per internal
            if (ProcessingTool.GetISOHashValues(datafile, out long size, out var crc32, out var md5, out var sha1))
            {
                info.SizeAndChecksums!.Size = size;
                info.SizeAndChecksums.CRC32 = crc32;
                info.SizeAndChecksums.MD5 = md5;
                info.SizeAndChecksums.SHA1 = sha1;
            }

            // Get Layerbreak from .dvd file if possible
            if (GetLayerbreak($"{basePath}.dvd", out long layerbreak))
                info.SizeAndChecksums!.Layerbreak = layerbreak;

            // Look for read errors
            if (GetReadErrors(logPath, out long readErrors))
                info.CommonDiscInfo.ErrorsCount = readErrors == -1 ? "Error retrieving error count" : readErrors.ToString();

            switch (System)
            {
                case RedumpSystem.MicrosoftXbox:
                    string xmidString = ProcessingTool.GetXMID(Path.Combine(outputDirectory, "DMI.bin"));
                    var xmid = SabreTools.Serialization.Wrappers.XMID.Create(xmidString);
                    if (xmid != null)
                    {
                        info.CommonDiscInfo.CommentsSpecialFields![SiteCode.XMID] = xmidString?.TrimEnd('\0') ?? string.Empty;
                        info.CommonDiscInfo.Serial = xmid.Serial ?? string.Empty;
                        if (!redumpCompat)
                        {
                            info.VersionAndEditions!.Version = xmid.Version ?? string.Empty;
                            info.CommonDiscInfo.Region = ProcessingTool.GetXGDRegion(xmid.Model.RegionIdentifier);
                        }
                    }

                    break;

                case RedumpSystem.MicrosoftXbox360:

                    // Get PVD from ISO
                    if (GetPVD($"{basePath}.iso", out string? pvd))
                        info.Extras!.PVD = pvd;

                    // Parse Media ID
                    //string? mediaID = GetMediaID(logPath);

                    // Parse DMI.bin
                    string xemidString = ProcessingTool.GetXeMID(Path.Combine(outputDirectory, "DMI.bin"));
                    var xemid = SabreTools.Serialization.Wrappers.XeMID.Create(xemidString);
                    if (xemid != null)
                    {
                        info.CommonDiscInfo.CommentsSpecialFields![SiteCode.XeMID] = xemidString?.TrimEnd('\0') ?? string.Empty;
                        info.CommonDiscInfo.Serial = xemid.Serial ?? string.Empty;
                        if (!redumpCompat)
                            info.VersionAndEditions!.Version = xemid.Version ?? string.Empty;

                        info.CommonDiscInfo.Region = ProcessingTool.GetXGDRegion(xemid.Model.RegionIdentifier);
                    }

                    break;
            }

            // Get the output file paths
            string dmiPath = Path.Combine(outputDirectory, "DMI.bin");
            string pfiPath = Path.Combine(outputDirectory, "PFI.bin");
            string ssPath = Path.Combine(outputDirectory, "SS.bin");

            // Deal with SS.bin
            if (File.Exists(ssPath))
            {
                // Save security sector ranges
                string? ranges = ProcessingTool.GetSSRanges(ssPath);
                if (!string.IsNullOrEmpty(ranges))
                    info.Extras!.SecuritySectorRanges = ranges;

                // Recreate RawSS.bin
                RecreateSS(logPath!, ssPath, Path.Combine(outputDirectory, "RawSS.bin"));

                // Run ss_sector_range to get repeatable SS hash
                ProcessingTool.CleanSS(ssPath, ssPath);
            }

            // DMI/PFI/SS CRC32 hashes
            if (File.Exists(dmiPath))
                info.CommonDiscInfo.CommentsSpecialFields![SiteCode.DMIHash] = HashTool.GetFileHash(dmiPath, HashType.CRC32)?.ToUpperInvariant() ?? string.Empty;
            if (File.Exists(pfiPath))
                info.CommonDiscInfo.CommentsSpecialFields![SiteCode.PFIHash] = HashTool.GetFileHash(pfiPath, HashType.CRC32)?.ToUpperInvariant() ?? string.Empty;
            if (File.Exists(ssPath))
                info.CommonDiscInfo.CommentsSpecialFields![SiteCode.SSHash] = HashTool.GetFileHash(ssPath, HashType.CRC32)?.ToUpperInvariant() ?? string.Empty;
        }

        /// <inheritdoc/>
        internal override List<OutputFile> GetOutputFiles(MediaType? mediaType, string? outputDirectory, string outputFilename)
        {
            // Remove the extension by default
            outputFilename = Path.GetFileNameWithoutExtension(outputFilename);

            return [
                new($"{outputFilename}.dvd", OutputFileFlags.Artifact
                    | OutputFileFlags.Zippable,
                    "dvd"),
                new($"{outputFilename}.iso", OutputFileFlags.Required),

                new("DMI.bin", OutputFileFlags.Required
                    | OutputFileFlags.Binary
                    | OutputFileFlags.Zippable,
                    "dmi"),
                new RegexOutputFile("[lL]og\\.txt", OutputFileFlags.Required
                    | OutputFileFlags.Artifact
                    | OutputFileFlags.Zippable,
                    "log"),
                new("PFI.bin", OutputFileFlags.Required
                    | OutputFileFlags.Binary
                    | OutputFileFlags.Zippable,
                    "pfi"),
                new("RawSS.bin", OutputFileFlags.Binary
                    | OutputFileFlags.Zippable,
                    "raw_ss"),
                new("SS.bin", OutputFileFlags.Required
                    | OutputFileFlags.Binary
                    | OutputFileFlags.Zippable,
                    "ss"),
            ];
        }

        #endregion

        #region Information Extraction Methods

        /// <summary>
        /// Determines the file path of the XBC log
        /// </summary>
        /// <param name="outputDirectory">Base directory to search in</param>
        /// <returns>Log path if found, null otherwise</returns>
        internal static string? GetLogName(string outputDirectory)
        {
            // If the directory name is invalid
            if (outputDirectory.Length == 0)
                return null;

            // If the directory doesn't exist
            if (!Directory.Exists(outputDirectory))
                return null;

            // Check the known paths first
            if (IsSuccessfulLog(Path.Combine(outputDirectory, "Log.txt")))
                return Path.Combine(outputDirectory, "Log.txt");
            else if (IsSuccessfulLog(Path.Combine(outputDirectory, "log.txt")))
                return Path.Combine(outputDirectory, "log.txt");

            // Search for a renamed log file (assume there is only one)
            string[] files = Directory.GetFiles(outputDirectory, "*.txt", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                if (IsSuccessfulLog(file))
                    return file;
            }

            return null;
        }

        /// <summary>
        /// Checks if Log file has a successful read in it
        /// </summary>
        /// <param name="log">Path to log file</param>
        /// <returns>True if successful log found, false otherwise</returns>
        private static bool IsSuccessfulLog(string log)
        {
            // If the log path is invalid
            if (string.IsNullOrEmpty(log))
                return false;
            if (!File.Exists(log))
                return false;

            // Successful Example:
            //    Read completed in 00:50:23
            // Failed Example:
            //    Read failed

            try
            {
                // If Version is not found, not a valid log file
                if (string.IsNullOrEmpty(GetVersion(log)))
                    return false;

                // Look for "   Read completed in " in log file
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream)
                {
                    string? line = sr.ReadLine();
                    if (line?.StartsWith("   Read completed in ") == true)
                    {
                        return true;
                    }
                }

                // Required lines were not found
                return false;
            }
            catch
            {
                // Absorb the exception
                return false;
            }
        }

        /// <summary>
        /// Get the XBC version if possible
        /// </summary>
        /// <param name="log">Path to XBC log file</param>
        /// <returns>Version if possible, null on error</returns>
        internal static string? GetVersion(string? log)
        {
            // If the log path is invalid
            if (string.IsNullOrEmpty(log))
                return null;
            if (!File.Exists(log))
                return null;

            // Sample:
            // ====================================================================
            // Xbox Backup Creator v2.9 Build:0425 By Redline99
            //

            try
            {
                // Assume version is appended after first mention of Xbox Backup Creator
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream)
                {
                    string? line = sr.ReadLine()?.Trim();
                    if (line?.StartsWith("Xbox Backup Creator ") == true)
                        return line.Substring("Xbox Backup Creator ".Length).Trim();
                }

                // Required lines were not found
                return null;
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Get the drive model from the log
        /// </summary>
        /// <param name="log">Path to XBC log file</param>
        /// <returns>Drive model if found, null otherwise</returns>
        internal static string? GetDrive(string? log)
        {
            // If the log path is invalid
            if (string.IsNullOrEmpty(log))
                return null;
            if (!File.Exists(log))
                return null;

            // Example:
            // ========================================
            // < --Security Sector Details -->
            // Source Drive: SH-D162D
            // ----------------------------------------

            try
            {
                // Parse drive model from log file
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream)
                {
                    string? line = sr.ReadLine()?.Trim();
                    if (line?.StartsWith("Source Drive: ") == true)
                    {
                        return line.Substring("Source Drive: ".Length).Trim();
                    }
                }

                // Required lines were not found
                return null;
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Get the Layerbreak value if possible
        /// </summary>
        /// <param name="dvd">Path to layerbreak file</param>
        /// <param name="layerbreak">Layerbreak value if found, -1 otherwise</param>
        /// <returns>True if successful, otherwise false</returns>
        /// <returns></returns>
        internal static bool GetLayerbreak(string? dvd, out long layerbreak)
        {
            layerbreak = -1;

            if (string.IsNullOrEmpty(dvd))
                return false;

            if (!File.Exists(dvd))
                return false;

            // Example:
            // LayerBreak=1913776
            // track.iso

            try
            {
                // Parse Layerbreak value from DVD file
                using var sr = File.OpenText(dvd);
                while (!sr.EndOfStream)
                {
                    string? line = sr.ReadLine()?.Trim();
                    if (line?.StartsWith("LayerBreak=") == true)
                    {
                        return long.TryParse(line.Substring("LayerBreak=".Length).Trim(), out layerbreak);
                    }
                }

                // Required lines were not found
                return false;
            }
            catch
            {
                // Absorb the exception
                return false;
            }
        }

        /// <summary>
        /// Get the read error count if possible
        /// </summary>
        /// <param name="log">Path to XBC log file</param>
        /// <param name="readErrors">Read error count if found, -1 otherwise</param>
        /// <returns>True if sucessful, otherwise false</returns>
        internal bool GetReadErrors(string? log, out long readErrors)
        {
            readErrors = -1;

            if (string.IsNullOrEmpty(log))
                return false;

            if (!File.Exists(log))
                return false;

            // TODO: Logic when more than one dump is in the logs

            // Example: (replace [E] with drive letter)
            // Creating SplitVid backup image [E]
            // ...
            //     Reading Game Partition
            // Setting read speed to 1x
            //     Unrecovered read error at Partition LBA: 0

            // Example: (replace track with base filename)
            // Creating Layer Break File
            //     LayerBreak file saved as: "track.dvd"
            //     A total of 1 sectors were zeroed out.

            // Example: (for Original Xbox)
            // A total of 65,536 sectors were zeroed out.
            // A total of 31 sectors with read errors were recovered.

            try
            {
                // Parse Layerbreak value from DVD file
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream)
                {
                    string? line = sr.ReadLine()?.Trim();
                    if (line?.StartsWith("Creating Layer Break File") == true)
                    {
                        // Read error count is two lines below
                        line = sr.ReadLine()?.Trim();
                        line = sr.ReadLine()?.Trim();
                        if (line?.StartsWith("A total of ") == true && line?.EndsWith(" sectors were zeroed out.") == true)
                        {
                            string? errorCount = line.Substring("A total of ".Length, line.Length - 36).Replace(",", "").Trim();
                            bool success = long.TryParse(errorCount, out readErrors);

                            // Original Xbox should have 65536 read errors when dumping with XBC
                            if (System == RedumpSystem.MicrosoftXbox)
                            {
                                if (readErrors == 65536)
                                    readErrors = 0;
                                else if (readErrors > 65536)
                                    readErrors -= 65536;
                            }

                            return success;
                        }
                    }
                }

                // Required lines were not found
                return false;
            }
            catch
            {
                // Absorb the exception
                return false;
            }
        }

        /// <summary>
        /// Get Xbox360 Media ID from XBC log file
        /// </summary>
        /// <param name="log">Path to XBC log file</param>
        /// <returns>Media ID if Log successfully parsed, null otherwise</returns>
        internal string? GetMediaID(string? log)
        {
            if (string.IsNullOrEmpty(log))
                return null;
            if (!File.Exists(log))
                return null;

            if (System == RedumpSystem.MicrosoftXbox)
                return null;

            // Example:
            // ----------------------------------------
            // Media ID
            // A76B9983D170EFF8749A892BC-8B62A812
            // ----------------------------------------

            try
            {
                // Parse Layerbreak value from DVD file
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream)
                {
                    string? line = sr.ReadLine()?.Trim();
                    if (line?.StartsWith("Media ID") == true)
                    {
                        line = sr.ReadLine()?.Trim();
                        return line?.Substring(26).Trim();
                    }
                }

                // Required lines were not found
                return null;
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Recreate an SS.bin file from XBC log and write it to a file
        /// </summary>
        /// <param name="log">Path to XBC log</param>
        /// <param name="cleanSS">Path to the clean SS file to read from</param>
        /// <param name="rawSS">Path to the raw SS file to write to</param>
        /// <returns>True if successful, false otherwise</returns>
        private static bool RecreateSS(string log, string cleanSS, string rawSS)
        {
            if (!File.Exists(log) || !File.Exists(cleanSS))
                return false;

            byte[] ss = File.ReadAllBytes(cleanSS);
            if (ss.Length != 2048)
                return false;

            if (!RecreateSS(log!, ss))
                return false;

            File.WriteAllBytes(rawSS, ss);
            return true;
        }

        /// <summary>
        /// Recreate an SS.bin byte array from an XBC log.
        /// With help from https://github.com/hadzz/SS-Angle-Fixer/
        /// </summary>
        /// <param name="log">Path to XBC log</param>
        /// <param name="ss">Byte array of SS sector</param>
        /// <returns>True if successful, false otherwise</returns>
        private static bool RecreateSS(string log, byte[] ss)
        {
            // Log file must exist
            if (!File.Exists(log))
                return false;

            // SS must be complete sector
            if (ss.Length != 2048)
                return false;

            // Ignore XGD1 discs
            if (!ProcessingTool.GetXGDType(ss, out int xgdType))
                return false;
            if (xgdType == 0)
                return false;

            // Don't recreate an already raw SS
            // (but do save to file, so return true)
            if (!ProcessingTool.IsCleanSS(ss))
                return true;

            // Example replay table:
            /*
            ----------------------------------------
            RT CID MOD DATA          Drive Response
            -- --  --  ------------- -------------------
            01 14  00  033100 0340FF B7D8C32A B703590100
            03 BE  00  244530 24552F F4B9B528 BE46360500
            01 97  00  DBBAD0 DBCACF DD7787F4 484977ED00
            03 45  00  FCAF00 FCBEFF FB7A7773 AAB662FC00
            05 6B  00  033100 033E7F 0A31252A 0200000200
            07 46  00  244530 2452AF F8E77EBC 5B00005B00
            05 36  00  DBBAD0 DBC84F F5DFA735 B50000B500
            07 A1  00  FCAF00 FCBC7F 6B749DBF 0E01000E01
            E0 50  00  42F4E1 00B6F7 00000000 0000000000
            --------------------------------------------
            */

            try
            {
                // Parse Replay Table from log
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream)
                {
                    string? line = sr.ReadLine()?.Trim();
                    if (line?.StartsWith("RT CID MOD DATA          Drive Response") == true)
                    {
                        // Ignore next line
                        line = sr.ReadLine()?.Trim();
                        if (sr.EndOfStream)
                            return false;

                        byte[][] responses = new byte[4][];

                        // Parse the nine rows from replay table
                        for (int i = 0; i < 9; i++)
                        {
                            line = sr.ReadLine()?.Trim();
                            // Validate line
                            if (sr.EndOfStream || string.IsNullOrEmpty(line) || line!.Length < 44)
                                return false;

                            // Save useful angle responses
                            if (i >= 4 && i <= 7)
                            {
                                byte[]? angles = line!.Substring(34, 10).FromHexString();
                                if (angles == null || angles.Length != 5)
                                    return false;
                                responses[i - 4] = angles!;
                            }
                        }

                        int rtOffset = 0x204;
                        if (xgdType == 3)
                            rtOffset = 0x24;

                        // Replace angles
                        for (int i = 0; i < 4; i++)
                        {
                            int offset = rtOffset + (9 * (i + 4));
                            for (int j = 0; j < 5; j++)
                            {
                                // Ignore the middle byte
                                if (j == 2)
                                    continue;

                                ss[offset + j] = responses[i][j];
                            }
                        }

                        return true;
                    }
                }

                // Required lines were not found
                return false;
            }
            catch
            {
                // Absorb the exception
                return false;
            }
        }

        #endregion
    }
}
