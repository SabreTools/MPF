﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using SabreTools.Hashing;
using SabreTools.Models.Logiqx;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;

namespace MPF.Processors
{
    /// <summary>
    /// Represents processing CleanRip outputs
    /// </summary>
    public sealed class CleanRip : BaseProcessor
    {
        /// <inheritdoc/>
        public CleanRip(RedumpSystem? system, MediaType? type) : base(system, type) { }

        #region BaseProcessor Implementations

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, string basePath, bool redumpCompat)
        {
            // Ensure that required sections exist
            info = Builder.EnsureAllSections(info);

            // TODO: Determine if there's a CleanRip version anywhere
            info.DumpingInfo!.DumpingDate = ProcessingTool.GetFileModifiedDate(basePath + "-dumpinfo.txt")?.ToString("yyyy-MM-dd HH:mm:ss");

            // Get the Datafile information
            var datafile = GenerateCleanripDatafile(basePath + ".iso", basePath + "-dumpinfo.txt");
            info.TracksAndWriteOffsets!.ClrMameProData = ProcessingTool.GenerateDatfile(datafile);

            // Get the individual hash data, as per internal
            if (ProcessingTool.GetISOHashValues(datafile, out long size, out var crc32, out var md5, out var sha1))
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
            switch (Type)
            {
                case MediaType.DVD: // Only added here to help users; not strictly correct
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    if (File.Exists(basePath + ".bca"))
                        info.Extras!.BCA = GetBCA(basePath + ".bca");

                    if (GetGameCubeWiiInformation(basePath + "-dumpinfo.txt", out Region? gcRegion, out var gcVersion, out var gcName, out var gcSerial))
                    {
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalName] = gcName ?? string.Empty;
                        info.CommonDiscInfo.CommentsSpecialFields![SiteCode.InternalSerialName] = gcSerial ?? string.Empty;
                        if (!redumpCompat)
                        {
                            info.VersionAndEditions!.Version = gcVersion ?? info.VersionAndEditions.Version;
                            info.CommonDiscInfo.Region = gcRegion ?? info.CommonDiscInfo.Region;
                        }
                    }

                    break;
            }
        }

        /// <inheritdoc/>
        internal override List<OutputFile> GetOutputFiles(string? baseDirectory, string baseFilename)
        {
            switch (Type)
            {
                case MediaType.DVD: // Only added here to help users; not strictly correct
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    return [
                        new($"{baseFilename}.bca", OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "bca"),
                        new($"{baseFilename}.iso", OutputFileFlags.Required),

                        new($"{baseFilename}-dumpinfo.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "dumpinfo"),
                    ];
            }

            return [];
        }

        #endregion

        #region Information Extraction Methods

        /// <summary>
        /// Get a formatted datfile from the cleanrip output, if possible
        /// </summary>
        /// <param name="iso">Path to ISO file</param>
        /// <param name="dumpinfo">Path to discinfo file</param>
        /// <returns></returns>
        internal static Datafile? GenerateCleanripDatafile(string iso, string dumpinfo)
        {
            // Get the size from the ISO, if possible
            long size = !string.IsNullOrEmpty(iso) && File.Exists(iso)
                ? new FileInfo(iso).Length
                : -1;

            // Setup the hashes
            string crc = string.Empty;
            string md5 = string.Empty;
            string sha1 = string.Empty;

            try
            {
                // Make sure this file is a dumpinfo
                if (!string.IsNullOrEmpty(dumpinfo) && File.Exists(dumpinfo))
                {
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

                // If no information could be found
                if (size == -1 && crc == string.Empty && md5 == string.Empty && sha1 == string.Empty)
                    return null;

                return new Datafile
                {
                    Game = [new Game() { Rom = [new Rom { Name = Path.GetFileName(iso), Size = size.ToString(), CRC = crc, MD5 = md5, SHA1 = sha1 }] }]
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
        internal static string? GetBCA(string bcaPath)
        {
            // If the file doesn't exist, we can't get the info
            if (string.IsNullOrEmpty(bcaPath))
                return null;
            if (!File.Exists(bcaPath))
                return null;

            try
            {
                var hex = ProcessingTool.GetFullFile(bcaPath, true);
                if (hex == null)
                    return null;

                // Separate into blocks of 4 hex digits and newlines
                var bca = new StringBuilder();
                for (int i = 0; i < hex.Length; i++)
                {
                    bca.Append(hex[i]);
                    if ((i + 1) % 32 == 0)
                        bca.AppendLine();
                    else if ((i + 1) % 4 == 0)
                        bca.Append(' ');
                }

                return bca.ToString();
            }
            catch
            {
                // We don't care what the error was right now
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
        internal static bool GetGameCubeWiiInformation(string dumpinfo, out Region? region, out string? version, out string? name, out string? serial)
        {
            region = null; version = null; name = null; serial = null;

            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(dumpinfo))
                return false;
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
