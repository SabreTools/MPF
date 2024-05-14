using System.Collections.Generic;
using System.IO;
using System.Linq;
using MPF.Core.Converters;
using MPF.Core.Data;
using SabreTools.Hashing;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;

namespace MPF.Core.Modules.XboxBackupCreator
{
    /// <summary>
    /// Represents a generic set of Xbox Backup Creator parameters
    /// </summary>
    public class Parameters : BaseParameters
    {
        #region Metadata

        /// <inheritdoc/>
        public override InternalProgram InternalProgram => InternalProgram.XboxBackupCreator;

        #endregion

        /// <inheritdoc/>
        public Parameters(string? parameters) : base(parameters) { }

        /// <inheritdoc/>
        public Parameters(RedumpSystem? system, MediaType? type, string? drivePath, string filename, int? driveSpeed, Options options)
            : base(system, type, drivePath, filename, driveSpeed, options)
        {
        }

        #region BaseParameters Implementations

        /// <inheritdoc/>
        public override (bool, List<string>) CheckAllOutputFilesExist(string basePath, bool preCheck)
        {
            var missingFiles = new List<string>();
            switch (this.Type)
            {
                case MediaType.DVD:
                    if (!File.Exists($"{basePath}_logs.zip") || !preCheck)
                    {
                        string baseDir = Path.GetDirectoryName(basePath) + Path.PathSeparator;
                        if (!File.Exists($"{baseDir}Log.txt"))
                        {
                            // Try look for lowercase log.txt
                            if (!File.Exists($"{baseDir}log.txt"))
                                missingFiles.Add($"{baseDir}Log.txt");
                        }
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
        public override void GenerateSubmissionInfo(SubmissionInfo info, Options options, string basePath, Drive? drive, bool includeArtifacts)
        {
            // Ensure that required sections exist
            info = Builder.EnsureAllSections(info);

            // Get base directory
            string baseDir = Path.GetDirectoryName(basePath) + Path.PathSeparator;

            // Get log filename
            string logname = "Log";
            if (!File.Exists($"{baseDir}Log.txt") && File.Exists($"{baseDir}log.txt"))
                logname = "log";

            // If GenerateSubmissionInfo is run, Log.txt existence should already be checked
            if (!File.Exists($"{baseDir}{logname}.txt"))
                return;

            // XBC dump info
            info.DumpingInfo!.DumpingProgram = $"{EnumConverter.LongName(this.InternalProgram)} {GetVersion($"{baseDir}{logname}.txt") ?? "Unknown Version"}";
            info.DumpingInfo.DumpingDate = InfoTool.GetFileModifiedDate($"{baseDir}{logname}.txt")?.ToString("yyyy-MM-dd HH:mm:ss");

            // Extract info based generically on MediaType
            switch (this.Type)
            {
                case MediaType.DVD:

                    // Get PVD from ISO
                    if (GetPVD(basePath + ".iso", out string? pvd))
                        info.Extras!.PVD = pvd;

                    // Get Layerbreak from .dvd file if possible
                    if (GetLayerbreak($"{basePath}.dvd", out long layerbreak))
                        info.SizeAndChecksums!.Layerbreak = layerbreak;

                    // Hash data
                    if (HashTool.GetStandardHashes(basePath + ".iso", out long filesize, out var crc32, out var md5, out var sha1))
                    {
                        // Get the Datafile information
                        var datafile = new Datafile
                        {
                            Games = [new Game { Roms = [new Rom { Name = string.Empty, Size = filesize.ToString(), Crc = crc32, Md5 = md5, Sha1 = sha1, }] }]
                        };

                        // Fill in the hash data
                        info.TracksAndWriteOffsets!.ClrMameProData = InfoTool.GenerateDatfile(datafile);

                        info.SizeAndChecksums!.Size = filesize;
                        info.SizeAndChecksums.CRC32 = crc32;
                        info.SizeAndChecksums.MD5 = md5;
                        info.SizeAndChecksums.SHA1 = sha1;
                    }

                    // TODO: Run ss_sector_range to get repeatable SS hash
                    // TODO: Recreate RawSS.bin using SS-Angle-Fixer from hadzz

                    // DMI/PFI/SS CRC32 hashes
                    if (File.Exists($"{baseDir}DMI.bin"))
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.DMIHash] = HashTool.GetFileHash($"{baseDir}DMI.bin", HashType.CRC32) ?? string.Empty;
                    if (File.Exists($"{baseDir}PFI.bin"))
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.PFIHash] = HashTool.GetFileHash($"{baseDir}PFI.bin", HashType.CRC32) ?? string.Empty;
                    if (File.Exists($"{baseDir}SS.bin"))
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.SSHash] = HashTool.GetFileHash($"{baseDir}SS.bin", HashType.CRC32) ?? string.Empty;

                    break;
            }

            // Fill in any artifacts that exist, Base64-encoded, if we need to
            if (includeArtifacts)
            {
                info.Artifacts ??= [];

                if (File.Exists($"{baseDir}{logname}.txt"))
                    info.Artifacts["log"] = GetBase64(GetFullFile($"{baseDir}{logname}.txt")) ?? string.Empty;
                if (File.Exists($"{basePath}.dvd"))
                    info.Artifacts["dvd"] = GetBase64(GetFullFile($"{basePath}.dvd")) ?? string.Empty;
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
        }

        /// <inheritdoc/>
        public override List<string> GetLogFilePaths(string basePath)
        {
            var logFiles = new List<string>();
            string baseDir = Path.GetDirectoryName(basePath) + Path.PathSeparator;
            switch (this.Type)
            {
                case MediaType.DVD:
                    if (File.Exists($"{basePath}.dvd"))
                        logFiles.Add($"{basePath}.dvd");
                    if (File.Exists($"{baseDir}Log.txt"))
                        logFiles.Add($"{baseDir}Log.txt");
                    if (File.Exists($"{baseDir}log.txt"))
                        logFiles.Add($"{baseDir}log.txt");
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
        /// Get the XBC version if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Version if possible, null on error</returns>
        private static string? GetVersion(string log)
        {
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
        /// Get the Layerbreak value if possible
        /// </summary>
        /// <param name="dvd">DVD file location</param>
        /// <param name="layerbreak">Layerbreak value if found</param>
        /// <returns>True if successful, otherwise false</returns>
        /// <returns></returns>
        private static bool GetLayerbreak(string dvd, out long layerbreak)
        {
            layerbreak = 0;

            if (!File.Exists(dvd))
                return false;

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
        /// Parse Xbox Backup Creator log file
        /// </summary>
        /// <param name="log">Path to log file</param>
        /// <returns>True if Log successfully parsed, false otherwise</returns>
        private bool ParseLog(string log)
        {
            if (!File.Exists(log))
                return false;

            // if (this.System == RedumpSystem.MicrosoftXbox)
        }

        #endregion
    }
}
