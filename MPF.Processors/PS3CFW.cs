using System.Collections.Generic;
using System.IO;
using SabreTools.Hashing;
using SabreTools.Models.Logiqx;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;

namespace MPF.Processors
{
    /// <summary>
    /// Represents processing PlayStation 3 Custom Firmware outputs
    /// </summary>
    public sealed class PS3CFW : BaseProcessor
    {
        /// <inheritdoc/>
        public PS3CFW(RedumpSystem? system, MediaType? type) : base(system, type) { }

        #region BaseProcessor Implementations

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, string basePath, bool redumpCompat)
        {
            // Ensure that required sections exist
            info = Builder.EnsureAllSections(info);

            // Try to determine the ISO file
            string isoPath = string.Empty;
            if (File.Exists($"{basePath}.iso"))
                isoPath = $"{basePath}.iso";
            else if (File.Exists($"{basePath}.ISO"))
                isoPath = $"{basePath}.ISO";

            // TODO: Determine if there's a CFW version anywhere
            info.DumpingInfo!.DumpingDate = ProcessingTool.GetFileModifiedDate(isoPath)?.ToString("yyyy-MM-dd HH:mm:ss");

            // Get the Datafile information
            Datafile? datafile = GeneratePS3CFWDatafile(isoPath);

            // Fill in the hash data
            info.TracksAndWriteOffsets!.ClrMameProData = ProcessingTool.GenerateDatfile(datafile);

            // Get the individual hash data, as per internal
            if (ProcessingTool.GetISOHashValues(datafile, out long size, out var crc32, out var md5, out var sha1))
            {
                info.SizeAndChecksums!.Size = size;
                info.SizeAndChecksums.CRC32 = crc32;
                info.SizeAndChecksums.MD5 = md5;
                info.SizeAndChecksums.SHA1 = sha1;
            }

            // Get the PVD from the ISO
            if (GetPVD(isoPath, out string? pvd))
                info.Extras!.PVD = pvd;

            // Get the base directory
            string baseDir = Path.GetDirectoryName(basePath) ?? string.Empty;

            // Try to determine the name of the GetKey file
            string getKeyPath = string.Empty;
            if (File.Exists($"{basePath}.getkey.log"))
            {
                getKeyPath = $"{basePath}.getkey.log";
            }
            else if (File.Exists(Path.Combine(baseDir, "getkey.log")))
            {
                getKeyPath = Path.Combine(baseDir, "getkey.log");
            }
            else
            {
                string[] getKeyFiles = Directory.GetFiles(baseDir, "*.getkey.log");
                if (getKeyFiles.Length > 0)
                    getKeyPath = getKeyFiles[0];
            }

            // Get dumping date from GetKey log date
            if (!string.IsNullOrEmpty(getKeyPath))
                info.DumpingInfo.DumpingDate = ProcessingTool.GetFileModifiedDate(getKeyPath)?.ToString("yyyy-MM-dd HH:mm:ss");

            // Try to determine the name of the PIC
            string? picPath = null;
            if (File.Exists($"{basePath}.disc.pic"))
            {
                picPath = $"{basePath}.disc.pic";
            }
            else if (File.Exists(Path.Combine(baseDir, "disc.pic")))
            {
                picPath = Path.Combine(baseDir, "disc.pic");
            }
            else
            {
                string[] discPicFiles = Directory.GetFiles(baseDir, "*.disc.pic");
                if (discPicFiles.Length > 0)
                    picPath = discPicFiles[0];
            }

            // Get the layerbreak information from the PIC
            var di = ProcessingTool.GetDiscInformation(picPath);
            info.SizeAndChecksums!.PICIdentifier = ProcessingTool.GetPICIdentifier(di);
            if (ProcessingTool.GetLayerbreaks(di, out long? layerbreak1, out long? layerbreak2, out long? layerbreak3))
            {
                if (layerbreak1 != null && layerbreak1 * 2048 < info.SizeAndChecksums.Size)
                    info.SizeAndChecksums.Layerbreak = layerbreak1.Value;

                if (layerbreak2 != null && layerbreak2 * 2048 < info.SizeAndChecksums.Size)
                    info.SizeAndChecksums.Layerbreak2 = layerbreak2.Value;

                if (layerbreak3 != null && layerbreak3 * 2048 < info.SizeAndChecksums.Size)
                    info.SizeAndChecksums.Layerbreak3 = layerbreak3.Value;
            }

            // Set the trim length based on the layer count
            int trimLength;
            if (info.SizeAndChecksums!.Layerbreak3 != default)
                trimLength = 520;
            else if (info.SizeAndChecksums!.Layerbreak2 != default)
                trimLength = 392;
            else
                trimLength = 264;

            // TODO: Put info about abnormal PIC info beyond 132 bytes in comments?
            info.Extras!.PIC = GetPIC(picPath, trimLength);

            // Parse Disc Key, Disc ID, and PIC from the getkey.log file
            if (ProcessingTool.ParseGetKeyLog(getKeyPath, out string? key, out string? id, out string? pic))
            {
                if (key != null)
                    info.Extras!.DiscKey = key.ToUpperInvariant();
                if (id != null)
                    info.Extras!.DiscID = id.ToUpperInvariant().Substring(0, 24) + "XXXXXXXX";
                if (string.IsNullOrEmpty(info.Extras!.PIC) && !string.IsNullOrEmpty(pic))
                    info.Extras.PIC = SplitString(pic, 32);
            }
        }

        /// <inheritdoc/>
        internal override List<OutputFile> GetOutputFiles(string? outputDirectory, string outputFilename)
        {
            // Remove the extension by default
            outputFilename = Path.GetFileNameWithoutExtension(outputFilename);

            switch (Type)
            {
                case MediaType.BluRay:
                    return [
                        new([$"{outputFilename}.iso", $"{outputFilename}.ISO"], OutputFileFlags.Required),
                        new([$"{outputFilename}.cue", $"{outputFilename}.CUE"], OutputFileFlags.Zippable),
                        new RegexOutputFile($"getkey\\.log$", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "getkey_log"),
                        new RegexOutputFile($"disc\\.pic$", OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "disc_pic"),
                    ];
            }

            return [];
        }

        #endregion

        #region Information Extraction Methods

        /// <summary>
        /// Get a formatted datfile from the PS3 CFW output, if possible
        /// </summary>
        /// <param name="iso">Path to ISO file</param>
        /// <returns></returns>
        internal static Datafile? GeneratePS3CFWDatafile(string iso)
        {
            // If the ISO file doesn't exist
            if (string.IsNullOrEmpty(iso))
                return null;
            if (!File.Exists(iso))
                return null;

            try
            {
                if (HashTool.GetStandardHashes(iso, out long size, out string? crc, out string? md5, out string? sha1))
                {
                    return new Datafile
                    {
                        Game = [new Game { Rom = [new Rom { Name = Path.GetFileName(iso), Size = size.ToString(), CRC = crc, MD5 = md5, SHA1 = sha1 }] }]
                    };
                }
                return null;
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        #endregion
    }
}
