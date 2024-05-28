using System.Collections.Generic;
using System.IO;
using System.Linq;
using MPF.Core;
using MPF.Core.Utilities;
using SabreTools.Hashing;
using SabreTools.Models.Logiqx;
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
        public XboxBackupCreator(RedumpSystem? system, MediaType? type) : base(system, type) { }

        #region BaseProcessor Implementations

        /// <inheritdoc/>
        public override (bool, List<string>) CheckAllOutputFilesExist(string basePath, bool preCheck)
        {
            var missingFiles = new List<string>();
            switch (Type)
            {
                case MediaType.DVD:
                    if (!File.Exists($"{basePath}_logs.zip") || !preCheck)
                    {
                        string baseDir = Path.GetDirectoryName(basePath) + Path.DirectorySeparatorChar;
                        string? logPath = GetLogName(baseDir);
                        if (string.IsNullOrEmpty(logPath))
                            missingFiles.Add($"{baseDir}Log.txt");
                        if (!File.Exists($"{baseDir}DMI.bin"))
                            missingFiles.Add($"{baseDir}DMI.bin");
                        if (!File.Exists($"{baseDir}PFI.bin"))
                            missingFiles.Add($"{baseDir}PFI.bin");
                        if (!File.Exists($"{baseDir}SS.bin"))
                            missingFiles.Add($"{baseDir}SS.bin");

                        // Not required from XBC
                        //if (!File.Exists($"{basePath}.dvd"))
                        //    missingFiles.Add($"{basePath}.dvd");
                    }

                    break;

                default:
                    missingFiles.Add("Media and system combination not supported for XboxBackupCreator");
                    break;
            }

            return (!missingFiles.Any(), missingFiles);
        }

        /// <inheritdoc/>
        public override void GenerateArtifacts(SubmissionInfo info, string basePath)
        {
            info.Artifacts ??= [];

            string baseDir = Path.GetDirectoryName(basePath) + Path.DirectorySeparatorChar;
            string? logPath = GetLogName(baseDir);

            if (File.Exists(logPath))
                info.Artifacts["log"] = ProcessingTool.GetBase64(ProcessingTool.GetFullFile(logPath!)) ?? string.Empty;
            if (File.Exists($"{basePath}.dvd"))
                info.Artifacts["dvd"] = ProcessingTool.GetBase64(ProcessingTool.GetFullFile($"{basePath}.dvd")) ?? string.Empty;
            //if (File.Exists($"{baseDir}DMI.bin"))
            //    info.Artifacts["dmi"] = Convert.ToBase64String(File.ReadAllBytes($"{baseDir}DMI.bin")) ?? string.Empty;
            // TODO: Include PFI artifact only if the hash doesn't match known PFI hashes
            //if (File.Exists($"{baseDir}PFI.bin"))
            //    info.Artifacts["pfi"] = Convert.ToBase64String(File.ReadAllBytes($"{baseDir}PFI.bin")) ?? string.Empty;
            //if (File.Exists($"{baseDir}SS.bin"))
            //    info.Artifacts["ss"] = Convert.ToBase64String(File.ReadAllBytes($"{baseDir}SS.bin")) ?? string.Empty;
            //if (File.Exists($"{baseDir}RawSS.bin"))
            //    info.Artifacts["rawss"] = Convert.ToBase64String(File.ReadAllBytes($"{baseDir}RawSS.bin")) ?? string.Empty;
        }

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, string basePath, Drive? drive, bool redumpCompat)
        {
            // Ensure that required sections exist
            info = Builder.EnsureAllSections(info);

            // Get base directory
            string baseDir = Path.GetDirectoryName(basePath) + Path.DirectorySeparatorChar;

            // Get log filename
            string? logPath = GetLogName(baseDir);
            if (string.IsNullOrEmpty(logPath))
                return;

            // XBC dump info
            info.DumpingInfo!.DumpingProgram = $"{EnumExtensions.LongName(InternalProgram.XboxBackupCreator)} {GetVersion(logPath) ?? "Unknown Version"}";
            info.DumpingInfo.DumpingDate = ProcessingTool.GetFileModifiedDate(logPath)?.ToString("yyyy-MM-dd HH:mm:ss");
            info.DumpingInfo.Model = GetDrive(logPath) ?? "Unknown Drive";

            // Look for read errors
            if (GetReadErrors(logPath, out long readErrors))
                info.CommonDiscInfo!.ErrorsCount = readErrors == -1 ? "Error retrieving error count" : readErrors.ToString();

            // Extract info based generically on MediaType
            switch (Type)
            {
                case MediaType.DVD:

                    // Get Layerbreak from .dvd file if possible
                    if (GetLayerbreak($"{basePath}.dvd", out long layerbreak))
                        info.SizeAndChecksums!.Layerbreak = layerbreak;

                    // Hash data
                    if (HashTool.GetStandardHashes(basePath + ".iso", out long filesize, out var crc32, out var md5, out var sha1))
                    {
                        // Get the Datafile information
                        var datafile = new Datafile
                        {
                            Game = [new Game { Rom = [new Rom { Name = string.Empty, Size = filesize.ToString(), CRC = crc32, MD5 = md5, SHA1 = sha1 }] }]
                        };

                        // Fill in the hash data
                        info.TracksAndWriteOffsets!.ClrMameProData = ProcessingTool.GenerateDatfile(datafile);

                        info.SizeAndChecksums!.Size = filesize;
                        info.SizeAndChecksums.CRC32 = crc32;
                        info.SizeAndChecksums.MD5 = md5;
                        info.SizeAndChecksums.SHA1 = sha1;
                    }

                    switch (System)
                    {
                        case RedumpSystem.MicrosoftXbox:

                            // Parse DMI.bin
                            string xmidString = Tools.GetXGD1XMID($"{baseDir}DMI.bin");
                            var xmid = SabreTools.Serialization.Wrappers.XMID.Create(xmidString);
                            if (xmid != null)
                            {
                                info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.XMID] = xmidString?.TrimEnd('\0') ?? string.Empty;
                                info.CommonDiscInfo.Serial = xmid.Serial ?? string.Empty;
                                if (!redumpCompat)
                                    info.VersionAndEditions!.Version = xmid.Version ?? string.Empty;

                                info.CommonDiscInfo.Region = ProcessingTool.GetXGDRegion(xmid.Model.RegionIdentifier);
                            }

                            break;

                        case RedumpSystem.MicrosoftXbox360:

                            // Get PVD from ISO
                            if (GetPVD(basePath + ".iso", out string? pvd))
                                info.Extras!.PVD = pvd;

                            // Parse Media ID
                            //string? mediaID = GetMediaID(logPath);

                            // Parse DMI.bin
                            string xemidString = Tools.GetXGD23XeMID($"{baseDir}DMI.bin");
                            var xemid = SabreTools.Serialization.Wrappers.XeMID.Create(xemidString);
                            if (xemid != null)
                            {
                                info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.XeMID] = xemidString?.TrimEnd('\0') ?? string.Empty;
                                info.CommonDiscInfo.Serial = xemid.Serial ?? string.Empty;
                                if (!redumpCompat)
                                    info.VersionAndEditions!.Version = xemid.Version ?? string.Empty;

                                info.CommonDiscInfo.Region = ProcessingTool.GetXGDRegion(xemid.Model.RegionIdentifier);
                            }

                            break;
                    }

                    // Deal with SS.bin
                    if (File.Exists($"{baseDir}SS.bin"))
                    {
                        // Save security sector ranges
                        string? ranges = Tools.GetSSRanges($"{baseDir}SS.bin");
                        if (!string.IsNullOrEmpty(ranges))
                            info.Extras!.SecuritySectorRanges = ranges;

                        // TODO: Determine SS version?
                        //info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.SSVersion] = 

                        // Recreate RawSS.bin
                        RecreateSS(logPath!, $"{baseDir}SS.bin", $"{baseDir}RawSS.bin");

                        // Run ss_sector_range to get repeatable SS hash
                        Tools.CleanSS($"{baseDir}SS.bin", $"{baseDir}SS.bin");
                    }

                    // DMI/PFI/SS CRC32 hashes
                    if (File.Exists($"{baseDir}DMI.bin"))
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.DMIHash] = HashTool.GetFileHash($"{baseDir}DMI.bin", HashType.CRC32)?.ToUpperInvariant() ?? string.Empty;
                    if (File.Exists($"{baseDir}PFI.bin"))
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.PFIHash] = HashTool.GetFileHash($"{baseDir}PFI.bin", HashType.CRC32)?.ToUpperInvariant() ?? string.Empty;
                    if (File.Exists($"{baseDir}SS.bin"))
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.SSHash] = HashTool.GetFileHash($"{baseDir}SS.bin", HashType.CRC32)?.ToUpperInvariant() ?? string.Empty;

                    break;
            }
        }

        /// <inheritdoc/>
        public override List<string> GetLogFilePaths(string basePath)
        {
            var logFiles = new List<string>();
            string baseDir = Path.GetDirectoryName(basePath) + Path.DirectorySeparatorChar;
            switch (Type)
            {
                case MediaType.DVD:
                    string? logPath = GetLogName(baseDir);
                    if (!string.IsNullOrEmpty(logPath))
                        logFiles.Add(logPath!);
                    if (File.Exists($"{basePath}.dvd"))
                        logFiles.Add($"{basePath}.dvd");
                    if (File.Exists($"{baseDir}DMI.bin"))
                        logFiles.Add($"{baseDir}DMI.bin");
                    if (File.Exists($"{baseDir}PFI.bin"))
                        logFiles.Add($"{baseDir}PFI.bin");
                    if (File.Exists($"{baseDir}SS.bin"))
                        logFiles.Add($"{baseDir}SS.bin");
                    if (File.Exists($"{baseDir}RawSS.bin"))
                        logFiles.Add($"{baseDir}RawSS.bin");

                    break;
            }

            return logFiles;
        }

        #endregion

        #region Information Extraction Methods

        /// <summary>
        /// Determines the file path of the XBC log
        /// </summary>
        /// <param name="baseDir">Base directory to search in</param>
        /// <returns>Log path if found, null otherwise</returns>
        private static string? GetLogName(string baseDir)
        {
            if (IsSuccessfulLog($"{baseDir}Log.txt"))
                return $"{baseDir}Log.txt";

            // Search for a renamed log file (assume there is only one)
            string[] files = Directory.GetFiles(baseDir, "*.txt", SearchOption.TopDirectoryOnly);
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

                // We couldn't find a successful dump
                return false;
            }
            catch
            {
                // We don't care what the exception is right now
                return false;
            }
        }

        /// <summary>
        /// Get the XBC version if possible
        /// </summary>
        /// <param name="log">Path to XBC log file</param>
        /// <returns>Version if possible, null on error</returns>
        private static string? GetVersion(string? log)
        {
            if (string.IsNullOrEmpty(log) || !File.Exists(log))
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

                // We couldn't detect the version
                return null;
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get the drive model from the log
        /// </summary>
        /// <param name="log">Path to XBC log file</param>
        /// <returns>Drive model if found, null otherwise</returns>
        private static string? GetDrive(string? log)
        {
            if (string.IsNullOrEmpty(log) || !File.Exists(log))
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

                // We couldn't detect the drive model
                return null;
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get the Layerbreak value if possible
        /// </summary>
        /// <param name="dvd">Path to layerbreak file</param>
        /// <param name="layerbreak">Layerbreak value if found</param>
        /// <returns>True if successful, otherwise false</returns>
        /// <returns></returns>
        private static bool GetLayerbreak(string? dvd, out long layerbreak)
        {
            layerbreak = 0;

            if (string.IsNullOrEmpty(dvd) || !File.Exists(dvd))
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

                // We couldn't detect the Layerbreak
                return false;
            }
            catch
            {
                // We don't care what the exception is right now
                return false;
            }
        }

        /// <summary>
        /// Get the read error count if possible
        /// </summary>
        /// <param name="log">Path to XBC log file</param>
        /// <param name="readErrors">Read error count if found, -1 otherwise</param>
        /// <returns>True if sucessful, otherwise false</returns>
        private bool GetReadErrors(string? log, out long readErrors)
        {
            readErrors = -1;

            if (string.IsNullOrEmpty(log) || !File.Exists(log))
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

                // We couldn't detect the read error count
                return false;
            }
            catch
            {
                // We don't care what the exception is right now
                return false;
            }
        }

        /// <summary>
        /// Get Xbox360 Media ID from XBC log file
        /// </summary>
        /// <param name="log">Path to XBC log file</param>
        /// <returns>Media ID if Log successfully parsed, null otherwise</returns>
        private string? GetMediaID(string? log)
        {
            if (string.IsNullOrEmpty(log) || !File.Exists(log))
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
                        return line?.Substring(25).Trim();
                    }
                }

                // We couldn't detect the Layerbreak
                return null;
            }
            catch
            {
                // We don't care what the exception is right now
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
            if (!Tools.GetXGDType(ss, out int xgdType))
                return false;
            if (xgdType == 0)
                return false;

            // Don't recreate an already raw SS
            // (but do save to file, so return true)
            if (!Tools.IsCleanSS(ss))
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
                                byte[]? angles = Tools.HexStringToByteArray(line!.Substring(34, 10));
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

                // We couldn't detect the replay table
                return false;
            }
            catch
            {
                // We don't care what the exception is right now
                return false;
            }
        }

        #endregion
    }
}
