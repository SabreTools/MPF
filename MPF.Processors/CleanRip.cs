using System.Collections.Generic;
using System.IO;
using System.Text;
using MPF.Processors.OutputFiles;
using SabreTools.Data.Models.Logiqx;
using SabreTools.Hashing;
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
        public CleanRip(RedumpSystem? system) : base(system) { }

        #region BaseProcessor Implementations

        /// <inheritdoc/>
        public override MediaType? DetermineMediaType(string? outputDirectory, string outputFilename)
        {
            return System switch
            {
                RedumpSystem.NintendoGameCube => MediaType.NintendoGameCubeGameDisc,
                RedumpSystem.NintendoWii => MediaType.NintendoWiiOpticalDisc,
                _ => null,
            };
        }

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, MediaType? mediaType, string basePath, bool redumpCompat)
        {
            // TODO: Determine if there's a CleanRip version anywhere
            info.DumpingInfo.DumpingDate = ProcessingTool.GetFileModifiedDate($"{basePath}-dumpinfo.txt")?.ToString("yyyy-MM-dd HH:mm:ss");

            // Try to merge parts together, if needed
            _ = MergeFileParts(basePath);

            // Get the Datafile information
            var datafile = GenerateCleanripDatafile($"{basePath}.iso", $"{basePath}-dumpinfo.txt");
            info.TracksAndWriteOffsets.ClrMameProData = ProcessingTool.GenerateDatfile(datafile);

            // Get the individual hash data, as per internal
            if (ProcessingTool.GetISOHashValues(datafile, out long size, out var crc32, out var md5, out var sha1))
            {
                info.SizeAndChecksums.Size = size;
                info.SizeAndChecksums.CRC32 = crc32;
                info.SizeAndChecksums.MD5 = md5;
                info.SizeAndChecksums.SHA1 = sha1;

                // Dual-layer discs have the same size and layerbreak
                if (size == 8511160320)
                    info.SizeAndChecksums.Layerbreak = 2084960;
            }

            // Get BCA information, if available
            info.Extras.BCA = GetBCA($"{basePath}.bca");

            // Get internal information
            if (GetGameCubeWiiInformation($"{basePath}-dumpinfo.txt", out Region? region, out var version, out var internalName, out var serial))
            {
                info.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalName] = internalName ?? string.Empty;
                info.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalSerialName] = serial ?? string.Empty;
                if (!redumpCompat)
                {
                    info.VersionAndEditions.Version = version ?? info.VersionAndEditions.Version;
                    info.CommonDiscInfo.Region = region ?? info.CommonDiscInfo.Region;
                }
            }
        }

        /// <inheritdoc/>
        internal override List<OutputFile> GetOutputFiles(MediaType? mediaType, string? outputDirectory, string outputFilename)
        {
            // Remove the extension by default
            outputFilename = Path.GetFileNameWithoutExtension(outputFilename);

            return [
                new($"{outputFilename}.bca", OutputFileFlags.Required
                    | OutputFileFlags.Binary
                    | OutputFileFlags.Zippable,
                    "bca"),
                new([$"{outputFilename}.iso", $"{outputFilename}.part0.iso"], OutputFileFlags.Required),

                new($"{outputFilename}-dumpinfo.txt", OutputFileFlags.Required
                    | OutputFileFlags.Artifact
                    | OutputFileFlags.Zippable,
                    "dumpinfo"),
            ];
        }

        #endregion

        #region Private Extra Methods

        /// <summary>
        /// Merge all file parts based on a base path
        /// </summary>
        /// <param name="basePath">Base path to use for building</param>
        /// <returns>Filename of the merged parts on success, null otherwise</returns>
        /// <remarks>This assumes that all file parts are named like $"{basePath}.part{i}.iso"</remarks>
        internal static string? MergeFileParts(string basePath)
        {
            // If the expected output already exists
            if (File.Exists($"{basePath}.iso"))
                return $"{basePath}.iso";

            // If the first part does not exist
            if (!File.Exists($"{basePath}.part0.iso"))
                return null;

            try
            {
                // Try to build the new output file
                string combined = $"{basePath}.iso";
                using var ofs = File.Open(combined, FileMode.Create, FileAccess.Write, FileShare.None);

                int i = 0;
                do
                {
                    // Get the next file part
                    string part = $"{basePath}.part{i++}.iso";
                    if (!File.Exists(part))
                        break;

                    // Copy the file part to the output
                    using var ifs = File.Open(part, FileMode.Open, FileAccess.Read, FileShare.None);

                    // Write in blocks
                    int read = 0;
                    do
                    {
                        byte[] buffer = new byte[3 * 1024 * 1024];

                        read = ifs.Read(buffer, 0, buffer.Length);
                        if (read == 0)
                            break;

                        ofs.Write(buffer, 0, read);
                        ofs.Flush();
                    } while (read > 0);

                } while (true);

                return combined;
            }
            catch
            {
                // Absorb the exception right now
                return null;
            }
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
                // Absorb the exception
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
                // Absorb the exception right now
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
                // Absorb the exception
                return false;
            }
        }

        #endregion
    }
}
