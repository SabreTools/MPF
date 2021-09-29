using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MPF.Core.Data;
using RedumpLib.Data;

namespace MPF.Modules.CleanRip
{
    /// <summary>
    /// Represents a generic set of CleanRip parameters
    /// </summary>
    public class Parameters : BaseParameters
    {
        #region Metadata

        /// <inheritdoc/>
        public override InternalProgram InternalProgram => InternalProgram.CleanRip;

        #endregion

        /// <inheritdoc/>
        public Parameters(string parameters) : base(parameters) { }

        /// <inheritdoc/>
        public Parameters(RedumpSystem? system, MediaType? type, char driveLetter, string filename, int? driveSpeed, Options options)
            : base(system, type, driveLetter, filename, driveSpeed, options)
        {
        }

        #region BaseParameters Implementations

        /// <inheritdoc/>
        public override (bool, List<string>) CheckAllOutputFilesExist(string basePath, bool preCheck)
        {
            List<string> missingFiles = new List<string>();
            switch (this.Type)
            {
                case MediaType.DVD: // Only added here to help users; not strictly correct
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    if (!File.Exists($"{basePath}_logs.zip") || !preCheck)
                    {
                        if (!File.Exists($"{basePath}-dumpinfo.txt"))
                            missingFiles.Add($"{basePath}-dumpinfo.txt");
                        if (!File.Exists($"{basePath}.bca"))
                            missingFiles.Add($"{basePath}.bca");
                    }

                    break;

                default:
                    return (false, missingFiles);
            }

            return (!missingFiles.Any(), missingFiles);
        }

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, string basePath, Drive drive, bool includeArtifacts)
        {
            info.TracksAndWriteOffsets.ClrMameProData = GetCleanripDatfile(basePath + ".iso", basePath + "-dumpinfo.txt");
            
            // Get the individual hash data, as per internal
            if (GetISOHashValues(info.TracksAndWriteOffsets.ClrMameProData, out long size, out string crc32, out string md5, out string sha1))
            {
                info.SizeAndChecksums.Size = size;
                info.SizeAndChecksums.CRC32 = crc32;
                info.SizeAndChecksums.MD5 = md5;
                info.SizeAndChecksums.SHA1 = sha1;

                // Dual-layer discs have the same size and layerbreak
                if (size == 8511160320)
                    info.SizeAndChecksums.Layerbreak = 2084960;
            }            

            // Extract info based generically on MediaType
            switch (this.Type)
            {
                case MediaType.DVD: // Only added here to help users; not strictly correct
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    if (File.Exists(basePath + ".bca"))
                        info.Extras.BCA = GetBCA(basePath + ".bca");

                    if (GetGameCubeWiiInformation(basePath + "-dumpinfo.txt", out Region? gcRegion, out string gcVersion))
                    {
                        info.CommonDiscInfo.Region = gcRegion ?? info.CommonDiscInfo.Region;
                        info.VersionAndEditions.Version = gcVersion ?? info.VersionAndEditions.Version;
                    }

                    break;
            }

            // Fill in any artifacts that exist, Base64-encoded, if we need to
            if (includeArtifacts)
            {
                if (File.Exists(basePath + ".bca"))
                    info.Artifacts["bca"] = GetBase64(GetFullFile(basePath + ".bca", binary: true));
                if (File.Exists(basePath + "-dumpinfo.txt"))
                    info.Artifacts["dumpinfo"] = GetBase64(GetFullFile(basePath + "-dumpinfo.txt"));
            }
        }

        /// <inheritdoc/>
        public override List<string> GetLogFilePaths(string basePath)
        {
            List<string> logFiles = new List<string>();
            switch (this.Type)
            {
                case MediaType.DVD: // Only added here to help users; not strictly correct
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    if (File.Exists($"{basePath}-dumpinfo.txt"))
                        logFiles.Add($"{basePath}-dumpinfo.txt");
                    if (File.Exists($"{basePath}.bca"))
                        logFiles.Add($"{basePath}.bca");

                    break;
            }

            return logFiles;
        }

        #endregion

        #region Information Extraction Methods

        /// <summary>
        /// Get the hex contents of the BCA file
        /// </summary>
        /// <param name="bcaPath">Path to the BCA file associated with the dump</param>
        /// <returns>BCA data as a hex string if possible, null on error</returns>
        /// <remarks>https://stackoverflow.com/questions/9932096/add-separator-to-string-at-every-n-characters</remarks>
        private static string GetBCA(string bcaPath)
        {
            // If the file doesn't exist, we can't get the info
            if (!File.Exists(bcaPath))
                return null;

            try
            {
                string hex = GetFullFile(bcaPath, true);
                return Regex.Replace(hex, ".{32}", "$0\n");
            }
            catch
            {
                // We don't care what the error was right now
                return null;
            }
        }

        /// <summary>
        /// Get a formatted datfile from the cleanrip output, if possible
        /// </summary>
        /// <param name="iso">Path to ISO file</param>
        /// <param name="dumpinfo">Path to discinfo file</param>
        /// <returns></returns>
        private static string GetCleanripDatfile(string iso, string dumpinfo)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(dumpinfo))
                return null;

            using (StreamReader sr = File.OpenText(dumpinfo))
            {
                long size = new FileInfo(iso).Length;
                string crc = string.Empty;
                string md5 = string.Empty;
                string sha1 = string.Empty;

                try
                {
                    // Make sure this file is a dumpinfo
                    if (!sr.ReadLine().Contains("--File Generated by CleanRip"))
                        return null;

                    // Read all lines and gather dat information
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine().Trim();
                        if (line.StartsWith("CRC32"))
                            crc = line.Substring(7).ToLowerInvariant();
                        else if (line.StartsWith("MD5"))
                            md5 = line.Substring(5);
                        else if (line.StartsWith("SHA-1"))
                            sha1 = line.Substring(7);
                    }

                    return $"<rom name=\"{Path.GetFileName(iso)}\" size=\"{size}\" crc=\"{crc}\" md5=\"{md5}\" sha1=\"{sha1}\" />";
                }
                catch
                {
                    // We don't care what the exception is right now
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the extracted GC and Wii version
        /// </summary>
        /// <param name="dumpinfo">Path to discinfo file</param>
        /// <param name="region">Output region, if possible</param>
        /// <param name="version">Output internal version of the game</param>
        /// <returns></returns>
        private static bool GetGameCubeWiiInformation(string dumpinfo, out Region? region, out string version)
        {
            region = null; version = null;

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(dumpinfo))
                return false;

            using (StreamReader sr = File.OpenText(dumpinfo))
            {
                try
                {
                    // Make sure this file is a dumpinfo
                    if (!sr.ReadLine().Contains("--File Generated by CleanRip"))
                        return false;

                    // Read all lines and gather dat information
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine().Trim();
                        if (line.StartsWith("Version"))
                        {
                            version = line.Substring(9);
                        }
                        else if (line.StartsWith("Filename"))
                        {
                            string serial = line.Substring(10);

                            // char gameType = serial[0];
                            // string gameid = serial[1] + serial[2];
                            // string version = serial[4] + serial[5]

                            switch (serial[3])
                            {
                                case 'A':
                                    region = Region.World;
                                    break;
                                case 'D':
                                    region = Region.Germany;
                                    break;
                                case 'E':
                                    region = Region.USA;
                                    break;
                                case 'F':
                                    region = Region.France;
                                    break;
                                case 'I':
                                    region = Region.Italy;
                                    break;
                                case 'J':
                                    region = Region.Japan;
                                    break;
                                case 'K':
                                    region = Region.Korea;
                                    break;
                                case 'L':
                                    region = Region.Europe; // Japanese import to Europe
                                    break;
                                case 'M':
                                    region = Region.Europe; // American import to Europe
                                    break;
                                case 'N':
                                    region = Region.USA; // Japanese import to USA
                                    break;
                                case 'P':
                                    region = Region.Europe;
                                    break;
                                case 'R':
                                    region = Region.Russia;
                                    break;
                                case 'S':
                                    region = Region.Spain;
                                    break;
                                case 'Q':
                                    region = Region.Korea; // Korea with Japanese language
                                    break;
                                case 'T':
                                    region = Region.Korea; // Korea with English language
                                    break;
                                case 'X':
                                    region = null; // Not a real region code
                                    break;
                            }
                        }
                    }

                    return true;
                }
                catch
                {
                    // We don't care what the exception is right now
                    return false;
                }
            }
        }

        #endregion
    }
}
