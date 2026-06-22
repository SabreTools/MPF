using System.Collections.Generic;
using System.IO;
using MPF.Processors.OutputFiles;
using SabreTools.Data.Models.Logiqx;
using SabreTools.RedumpLib.Data;

namespace MPF.Processors
{
    /// <summary>
    /// Represents processing PlayStation 3 Custom Firmware outputs
    /// </summary>
    public sealed class PS3CFW : BaseProcessor
    {
        /// <inheritdoc/>
        public PS3CFW(PhysicalSystem? system) : base(system) { }

        #region BaseProcessor Implementations

        /// <inheritdoc/>
        public override PhysicalMediaType? DeterminePhysicalMediaType(string? outputDirectory, string outputFilename)
            => PhysicalMediaType.BluRay;

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, PhysicalMediaType? mediaType, string basePath, bool redumpCompat)
        {
            // Try to determine the ISO file
            string isoPath = string.Empty;
            if (File.Exists($"{basePath}.iso"))
                isoPath = $"{basePath}.iso";
            else if (File.Exists($"{basePath}.ISO"))
                isoPath = $"{basePath}.ISO";

            // TODO: Determine if there's a CFW version anywhere
            info.DumpingInfo.DumpingDate = ProcessingTool.GetFileModifiedDate(isoPath)?.ToString("yyyy-MM-dd HH:mm:ss");

            // Get the Datafile information
            Datafile? datafile = GenerateDatafile(isoPath);
            info.DumpMetadata.Dat = ProcessingTool.GenerateDatfile(datafile);

            // Get the PVD from the ISO
            if (GetPVD(isoPath, out string? pvd))
                info.DumpMetadata.PVD = pvd;

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
            info.DumpMetadata.PICIdentifier = ProcessingTool.GetPICIdentifier(di);
            if (ProcessingTool.GetLayerbreaks(di, out long? layerbreak1, out long? layerbreak2, out long? layerbreak3))
            {
                long size = datafile?.Game?[0]?.Rom?[0]?.Size ?? 0;

                if (layerbreak1 is not null && layerbreak1 * 2048 < size)
                    info.DiscIdentifiers.Layerbreak = layerbreak1.Value;

                if (layerbreak2 is not null && layerbreak2 * 2048 < size)
                    info.DiscIdentifiers.Layerbreak2 = layerbreak2.Value;

                if (layerbreak3 is not null && layerbreak3 * 2048 < size)
                    info.DiscIdentifiers.Layerbreak3 = layerbreak3.Value;
            }

            // Set the trim length based on the layer count
            int trimLength;
            if (info.DiscIdentifiers.Layerbreak3 != default)
                trimLength = 520;
            else if (info.DiscIdentifiers.Layerbreak2 != default)
                trimLength = 392;
            else
                trimLength = 264;

            // TODO: Put info about abnormal PIC info beyond 132 bytes in comments?
            info.DumpMetadata.PIC = GetPIC(picPath, trimLength);

            // Parse Disc Key, Disc ID, and PIC from the getkey.log file
            if (ProcessingTool.ParseGetKeyLog(getKeyPath, out string? key, out string? id, out string? pic))
            {
                if (key is not null)
                    info.DiscIdentifiers.DiscKey = key.ToUpperInvariant();
                if (id is not null)
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                    info.DiscIdentifiers.DiscID = $"{id.ToUpperInvariant()[..24]}XXXXXXXX";
#else
                    info.DiscIdentifiers.DiscID = id.ToUpperInvariant().Substring(0, 24) + "XXXXXXXX";
#endif
                if (string.IsNullOrEmpty(info.DumpMetadata.PIC) && !string.IsNullOrEmpty(pic))
                    info.DumpMetadata.PIC = SplitString(pic, 32);
            }
        }

        /// <inheritdoc/>
        internal override List<OutputFile> GetOutputFiles(PhysicalMediaType? mediaType, string? outputDirectory, string outputFilename)
        {
            // Remove the extension by default
            outputFilename = Path.GetFileNameWithoutExtension(outputFilename);

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

        #endregion
    }
}
