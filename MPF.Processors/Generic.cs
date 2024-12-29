using System;
using System.Collections.Generic;
using System.IO;
using SabreTools.Hashing;
using SabreTools.Models.Logiqx;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;

namespace MPF.Processors
{
    /// <summary>
    /// Represents processing generic outputs
    /// </summary>
    public sealed class Generic : BaseProcessor
    {
        /// <inheritdoc/>
        public Generic(RedumpSystem? system, MediaType? type) : base(system, type) { }

        #region BaseProcessor Implementations

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, string basePath, bool redumpCompat)
        {
            // Ensure that required sections exist
            info = Builder.EnsureAllSections(info);

            // Get the dumping program and version
            info.DumpingInfo!.DumpingProgram = "UNKNOWN PROGRAM";
            info.DumpingInfo.DumpingDate = GetDumpingDate(basePath);

            // Get the Datafile information
            var datafile = GetDatafile(basePath);
            if (datafile != null)
                info.TracksAndWriteOffsets!.ClrMameProData = ProcessingTool.GenerateDatfile(datafile);

            // Extract info based generically on MediaType
            switch (Type)
            {
                case MediaType.CDROM:
                case MediaType.GDROM:
                    info.TracksAndWriteOffsets!.Cuesheet = ProcessingTool.GetFullFile($"{basePath}.cue") ?? string.Empty;
                    break;

                case MediaType.DVD:
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                case MediaType.NintendoWiiUOpticalDisc:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                case MediaType.UMD:
                    var firstRom = datafile?.Game?[0]?.Rom?[0];
                    if (firstRom != null)
                    {
                        info.SizeAndChecksums!.Size = long.Parse(firstRom.Size ?? "0");
                        info.SizeAndChecksums.CRC32 = firstRom.CRC;
                        info.SizeAndChecksums.MD5 = firstRom.MD5;
                        info.SizeAndChecksums.SHA1 = firstRom.SHA1;
                    }

                    break;
            }
        }

        /// <inheritdoc/>
        internal override List<OutputFile> GetOutputFiles(string? outputDirectory, string outputFilename)
        {
            return [];
        }

        #endregion

        #region Information Extraction Methods

        /// <summary>
        /// Attempt to get the dumping date from a base path
        /// </summary>
        private static string? GetDumpingDate(string basePath)
        {
            DateTime? fileModifiedDate;
            if (File.Exists($"{basePath}.iso"))
                fileModifiedDate = ProcessingTool.GetFileModifiedDate($"{basePath}.iso");
            else if (File.Exists($"{basePath}.cue"))
                fileModifiedDate = ProcessingTool.GetFileModifiedDate($"{basePath}.cue");
            else if (File.Exists($"{basePath}.bin"))
                fileModifiedDate = ProcessingTool.GetFileModifiedDate($"{basePath}.bin");
            else if (File.Exists($"{basePath} (Track 1).bin"))
                fileModifiedDate = ProcessingTool.GetFileModifiedDate($"{basePath} (Track 1).bin");
            else if (File.Exists($"{basePath} (Track 01).bin"))
                fileModifiedDate = ProcessingTool.GetFileModifiedDate($"{basePath} (Track 01).bin");
            else if (File.Exists($"{basePath}.ccd"))
                fileModifiedDate = ProcessingTool.GetFileModifiedDate($"{basePath}.ccd");
            else if (File.Exists($"{basePath}.img"))
                fileModifiedDate = ProcessingTool.GetFileModifiedDate($"{basePath}.img");
            else if (File.Exists($"{basePath}.mds"))
                fileModifiedDate = ProcessingTool.GetFileModifiedDate($"{basePath}.mds");
            else if (File.Exists($"{basePath}.mdf"))
                fileModifiedDate = ProcessingTool.GetFileModifiedDate($"{basePath}.mdf");
            else if (File.Exists($"{basePath}.ima"))
                fileModifiedDate = ProcessingTool.GetFileModifiedDate($"{basePath}.ima");
            else
                return null;

            return fileModifiedDate?.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Attempt to generate a datafile from a base path
        /// </summary>
        private static Datafile? GetDatafile(string basePath)
        {
            // Single-file outputs
            if (File.Exists($"{basePath}.bin"))
            {
                var rom = GetRom($"{basePath}.bin");
                if (rom != null)
                    return GetDatafile(basePath, rom);
            }
            else if (File.Exists($"{basePath}.ima"))
            {
                var rom = GetRom($"{basePath}.ima");
                if (rom != null)
                    return GetDatafile(basePath, rom);
            }
            else if (File.Exists($"{basePath}.img"))
            {
                var rom = GetRom($"{basePath}.img");
                if (rom != null)
                    return GetDatafile(basePath, rom);
            }
            else if (File.Exists($"{basePath}.iso"))
            {
                var rom = GetRom($"{basePath}.iso");
                if (rom != null)
                    return GetDatafile(basePath, rom);
            }
            else if (File.Exists($"{basePath}.mdf"))
            {
                var rom = GetRom($"{basePath}.mdf");
                if (rom != null)
                    return GetDatafile(basePath, rom);
            }

            // Multi-file outputs
            else if (File.Exists($"{basePath} (Track 1).bin"))
            {
                var roms = new List<Rom>();

                // Try Track 0
                var track0rom = GetRom($"{basePath} (Track 0).bin");
                if (track0rom != null)
                    roms.Add(track0rom);

                // Hash all found tracks
                for (int i = 1; i < 10; i++)
                {
                    // Get the rom for the track
                    var rom = GetRom($"{basePath} (Track {i}).bin");
                    if (rom == null)
                        break;

                    // Add the track to the output
                    roms.Add(rom);
                }

                // Try Track A
                var trackArom = GetRom($"{basePath} (Track A).bin");
                if (trackArom != null)
                    roms.Add(trackArom);

                // Create and return if there are any tracks
                if (roms.Count > 0)
                    return GetDatafile(basePath, roms);
            }
            else if (File.Exists($"{basePath} (Track 01).bin"))
            {
                var roms = new List<Rom>();

                // Try Track 00
                var track00rom = GetRom($"{basePath} (Track 00).bin");
                if (track00rom != null)
                    roms.Add(track00rom);

                // Hash all found tracks
                for (int i = 1; i < 100; i++)
                {
                    // Get the rom for the track
                    var rom = GetRom($"{basePath} (Track {i:D2}).bin");
                    if (rom == null)
                        break;

                    // Add the track to the output
                    roms.Add(rom);
                }

                // Try Track AA
                var trackAArom = GetRom($"{basePath} (Track AA).bin");
                if (trackAArom != null)
                    roms.Add(trackAArom);

                // Create and return if there are any tracks
                if (roms.Count > 0)
                    return GetDatafile(basePath, roms);
            }

            // Unrecognized file input
            return null;
        }

        /// <summary>
        /// Generate a Datafile from a base path and a Rom
        /// </summary>
        private static Datafile? GetDatafile(string basePath, Rom rom)
            => GetDatafile(basePath, [rom]);

        /// <summary>
        /// Generate a Datafile from a base path and a list of Roms
        /// </summary>
        private static Datafile? GetDatafile(string basePath, List<Rom> roms)
        {
            // Return null if no Roms exist
            if (roms.Count == 0)
                return null;

            // Format the new Datafile
            return new Datafile
            {
                Game =
                [
                    new Game
                    {
                        Name = Path.GetFileName(basePath),
                        Rom = [.. roms]
                    }
                ]
            };
        }

        /// <summary>
        /// Get a Rom object for a single file
        /// </summary>
        private static Rom? GetRom(string file)
        {
            // Skip missing or invalid files
            if (!File.Exists(file))
                return null;
            if (!HashTool.GetStandardHashes(file, out long filesize, out var crc32, out var md5, out var sha1))
                return null;

            // Format the data into a rom object
            return new Rom { Name = Path.GetFileName(file), Size = filesize.ToString(), CRC = crc32, MD5 = md5, SHA1 = sha1 };
        }

        #endregion
    }
}
