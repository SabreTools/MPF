﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MPF.Core.Converters;
using MPF.Core.Data;
using SabreTools.Hashing;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;

namespace MPF.Core.Modules.CleanRip
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
                    missingFiles.Add("Media and system combination not supported for CleanRip");
                    break;
            }

            return (!missingFiles.Any(), missingFiles);
        }

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, Options options, string basePath, Drive? drive, bool includeArtifacts)
        {
            // Ensure that required sections exist
            info = Builder.EnsureAllSections(info);

            // TODO: Determine if there's a CleanRip version anywhere
            info.DumpingInfo!.DumpingProgram = EnumConverter.LongName(this.InternalProgram);
            info.DumpingInfo.DumpingDate = InfoTool.GetFileModifiedDate(basePath + "-dumpinfo.txt")?.ToString("yyyy-MM-dd HH:mm:ss");

            // Get the Datafile information
            var datafile = GenerateCleanripDatafile(basePath + ".iso", basePath + "-dumpinfo.txt");
            info.TracksAndWriteOffsets!.ClrMameProData = InfoTool.GenerateDatfile(datafile);

            // Get the individual hash data, as per internal
            if (InfoTool.GetISOHashValues(datafile, out long size, out var crc32, out var md5, out var sha1))
            {
                info.SizeAndChecksums!.Size = size;
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
                        info.Extras!.BCA = GetBCA(basePath + ".bca");

                    if (GetGameCubeWiiInformation(basePath + "-dumpinfo.txt", out Region? gcRegion, out var gcVersion, out var gcName, out var gcSerial))
                    {
                        info.CommonDiscInfo!.Region = gcRegion ?? info.CommonDiscInfo.Region;
                        info.VersionAndEditions!.Version = gcVersion ?? info.VersionAndEditions.Version;
                        info.CommonDiscInfo.CommentsSpecialFields![SiteCode.InternalName] = gcName ?? string.Empty;
                        info.CommonDiscInfo.CommentsSpecialFields![SiteCode.InternalSerialName] = gcSerial ?? string.Empty;
                    }

                    break;
            }

            // Fill in any artifacts that exist, Base64-encoded, if we need to
            if (includeArtifacts)
            {
                info.Artifacts ??= [];

                if (File.Exists(basePath + ".bca"))
                    info.Artifacts["bca"] = GetBase64(GetFullFile(basePath + ".bca", binary: true)) ?? string.Empty;
                if (File.Exists(basePath + "-dumpinfo.txt"))
                    info.Artifacts["dumpinfo"] = GetBase64(GetFullFile(basePath + "-dumpinfo.txt")) ?? string.Empty;
            }
        }

        /// <inheritdoc/>
        public override List<string> GetLogFilePaths(string basePath)
        {
            var logFiles = new List<string>();
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
        /// Get a formatted datfile from the cleanrip output, if possible
        /// </summary>
        /// <param name="iso">Path to ISO file</param>
        /// <param name="dumpinfo">Path to discinfo file</param>
        /// <returns></returns>
        private static Datafile? GenerateCleanripDatafile(string iso, string dumpinfo)
        {
            // If the files don't exist, we can't get info from it
            if (!File.Exists(iso) || !File.Exists(dumpinfo))
                return null;

            long size = new FileInfo(iso).Length;
            string crc = string.Empty;
            string md5 = string.Empty;
            string sha1 = string.Empty;

            try
            {
                // Make sure this file is a dumpinfo
                using var sr = File.OpenText(dumpinfo);
                if (sr.ReadLine()?.Contains("--File Generated by CleanRip") != true)
                    return null;

                // Read all lines and gather dat information
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine()?.Trim();
                    if (string.IsNullOrEmpty(line))
                        continue;
                    else if (line!.StartsWith("CRC32"))
                        crc = line.Substring(7).ToLowerInvariant();
                    else if (line.StartsWith("MD5"))
                        md5 = line.Substring(5);
                    else if (line.StartsWith("SHA-1"))
                        sha1 = line.Substring(7);
                }

                // Ensure all checksums were found in log
                if (crc == string.Empty || md5 == string.Empty || sha1 == string.Empty)
                {
                    if (HashTool.GetStandardHashes(iso, out long isoSize, out string? isoCRC, out string? isoMD5, out string? isoSHA1))
                    {
                        crc = isoCRC ?? crc;
                        md5 = isoMD5 ?? md5;
                        sha1 = isoSHA1 ?? sha1;
                    }
                }

                return new Datafile
                {
                    Games =
                    [
                        new()
                        {
                            Roms =
                            [
                                new Rom { Name = Path.GetFileName(iso), Size = size.ToString(), Crc = crc, Md5 = md5, Sha1 = sha1 },
                            ]
                        }
                    ]
                };
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get the hex contents of the BCA file
        /// </summary>
        /// <param name="bcaPath">Path to the BCA file associated with the dump</param>
        /// <returns>BCA data as a hex string if possible, null on error</returns>
        /// <remarks>https://stackoverflow.com/questions/9932096/add-separator-to-string-at-every-n-characters</remarks>
        private static string? GetBCA(string bcaPath)
        {
            // If the file doesn't exist, we can't get the info
            if (!File.Exists(bcaPath))
                return null;

            try
            {
                var hex = GetFullFile(bcaPath, true);
                if (hex == null)
                    return null;

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
        private static string? GetCleanripDatfile(string iso, string dumpinfo)
        {
            // If the files don't exist, we can't get info from it
            if (!File.Exists(iso) || !File.Exists(dumpinfo))
                return null;

            long size = new FileInfo(iso).Length;
            string crc = string.Empty;
            string md5 = string.Empty;
            string sha1 = string.Empty;

            try
            {
                // Make sure this file is a dumpinfo
                using var sr = File.OpenText(dumpinfo);
                if (sr.ReadLine()?.Contains("--File Generated by CleanRip") != true)
                    return null;

                // Read all lines and gather dat information
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine()?.Trim();
                    if (string.IsNullOrEmpty(line))
                        continue;
                    else if (line!.StartsWith("CRC32"))
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

        /// <summary>
        /// Get the extracted GC and Wii version
        /// </summary>
        /// <param name="dumpinfo">Path to discinfo file</param>
        /// <param name="region">Output region, if possible</param>
        /// <param name="version">Output internal version of the game</param>
        /// <param name="name">Output internal name of the game</param>
        /// <param name="serial">Output internal serial of the game</param>
        /// <returns></returns>
        private static bool GetGameCubeWiiInformation(string dumpinfo, out Region? region, out string? version, out string? name, out string? serial)
        {
            region = null; version = null; name = null; serial = null;

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(dumpinfo))
                return false;

            try
            {
                // Make sure this file is a dumpinfo
                using var sr = File.OpenText(dumpinfo);
                if (sr.ReadLine()?.Contains("--File Generated by CleanRip") != true)
                    return false;

                // Read all lines and gather dat information
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine()?.Trim();
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }
                    else if (line!.StartsWith("Version"))
                    {
                        version = line.Substring("Version: ".Length);
                    }
                    else if (line.StartsWith("Internal Name"))
                    {
                        name = line.Substring("Internal Name: ".Length);
                    }
                    else if (line.StartsWith("Filename"))
                    {
                        serial = line.Substring("Filename: ".Length);
                        if (serial.EndsWith("-disc2"))
                            serial = serial.Replace("-disc2", string.Empty);

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
                                region = Region.UnitedStatesOfAmerica;
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
                                region = Region.SouthKorea;
                                break;
                            case 'L':
                                region = Region.Europe; // Japanese import to Europe
                                break;
                            case 'M':
                                region = Region.Europe; // American import to Europe
                                break;
                            case 'N':
                                region = Region.UnitedStatesOfAmerica; // Japanese import to USA
                                break;
                            case 'P':
                                region = Region.Europe;
                                break;
                            case 'R':
                                region = Region.RussianFederation;
                                break;
                            case 'S':
                                region = Region.Spain;
                                break;
                            case 'Q':
                                region = Region.SouthKorea; // Korea with Japanese language
                                break;
                            case 'T':
                                region = Region.SouthKorea; // Korea with English language
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

        #endregion
    }
}
