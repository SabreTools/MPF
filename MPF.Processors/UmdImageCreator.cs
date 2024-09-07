﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SabreTools.Hashing;
using SabreTools.Models.Logiqx;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;

namespace MPF.Processors
{
    /// <summary>
    /// Represents processing UmdImageCreator outputs
    /// </summary>
    public sealed class UmdImageCreator : BaseProcessor
    {
        /// <inheritdoc/>
        public UmdImageCreator(RedumpSystem? system, MediaType? type) : base(system, type) { }

        #region BaseProcessor Implementations

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, string basePath, bool redumpCompat)
        {
            // Ensure that required sections exist
            info = Builder.EnsureAllSections(info);

            // TODO: Determine if there's a UMDImageCreator version anywhere
            info.DumpingInfo!.DumpingDate = ProcessingTool.GetFileModifiedDate(basePath + "_disc.txt")?.ToString("yyyy-MM-dd HH:mm:ss");

            // Fill in the volume labels
            if (GetVolumeLabels($"{basePath}_volDesc.txt", out var volLabels))
                VolumeLabels = volLabels;

            // Extract info based generically on MediaType
            switch (Type)
            {
                case MediaType.UMD:
                    info.Extras!.PVD = GetPVD(basePath + "_mainInfo.txt") ?? string.Empty;

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

                    if (GetUMDAuxInfo(basePath + "_disc.txt",
                        out var title,
                        out DiscCategory? category,
                        out string? serial,
                        out var version,
                        out var layer,
                        out long size))
                    {
                        info.CommonDiscInfo!.Title = title ?? string.Empty;
                        info.CommonDiscInfo.Category = category ?? DiscCategory.Games;
                        info.CommonDiscInfo.CommentsSpecialFields![SiteCode.InternalSerialName] = serial ?? string.Empty;
                        info.VersionAndEditions!.Version = version ?? string.Empty;
                        info.SizeAndChecksums!.Size = size;

                        if (!string.IsNullOrEmpty(layer))
                            info.SizeAndChecksums.Layerbreak = Int64.Parse(layer ?? "-1");
                    }

                    break;
            }
        }

        /// <inheritdoc/>
        internal override List<OutputFile> GetOutputFiles(string baseFilename)
        {
            switch (Type)
            {
                case MediaType.UMD:
                    return [
                        new($"{baseFilename}.iso", OutputFileFlags.Required),

                        new($"{baseFilename}_disc.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "disc"),
                        new($"{baseFilename}_drive.txt", OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "drive"),
                        new($"{baseFilename}_mainError.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "main_error"),
                        new($"{baseFilename}_mainInfo.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "main_info"),
                        new($"{baseFilename}_PFI.bin", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "pfi"),
                        new($"{baseFilename}_volDesc.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "vol_desc"),
                    ];
            }

            return [];
        }

        #endregion

        #region Information Extraction Methods

        /// <summary>
        /// Get the PVD from the input file, if possible
        /// </summary>
        /// <param name="mainInfo">_mainInfo.txt file location</param>
        /// <returns>Newline-deliminated PVD if possible, null on error</returns>
        private static string? GetPVD(string mainInfo)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(mainInfo))
                return null;

            try
            {
                // Make sure we're in the right sector
                using var sr = File.OpenText(mainInfo);
                while (sr.ReadLine()?.StartsWith("========== LBA[000016, 0x0000010]: Main Channel ==========") == false) ;

                // Fast forward to the PVD
                while (sr.ReadLine()?.StartsWith("0310") == false) ;

                // Now that we're at the PVD, read each line in and concatenate
                string pvd = "";
                for (int i = 0; i < 6; i++)
                    pvd += sr.ReadLine() + "\n"; // 320-370

                return pvd;
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get the UMD auxiliary info from the outputted files, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private static bool GetUMDAuxInfo(string disc,
            out string? title,
            out DiscCategory? category,
            out string? serial,
            out string? version,
            out string? layer,
            out long size)
        {
            title = null; serial = null; version = null; layer = null;
            category = null;
            size = -1;

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
                return false;

            try
            {
                // Loop through everything to get the first instance of each required field
                using var sr = File.OpenText(disc);
                while (!sr.EndOfStream)
                {
                    string? line = sr.ReadLine()?.Trim();
                    if (line == null)
                        break;

                    if (line.StartsWith("TITLE") && title == null)
                        title = line.Split(' ')[1];
                    else if (line.StartsWith("DISC_ID") && version == null)
                        serial = line.Split(' ')[1];
                    else if (line.StartsWith("DISC_VERSION") && version == null)
                        version = line.Split(' ')[1];
                    else if (line.StartsWith("pspUmdTypes"))
                        category = ProcessingTool.GetUMDCategory(line.Split(' ')[1]);
                    else if (line.StartsWith("L0 length"))
                        layer = line.Split(' ')[2];
                    else if (line.StartsWith("FileSize:"))
                        size = Int64.Parse(line.Split(' ')[1]);
                }

                // If we have a serial, format it
                if (!string.IsNullOrEmpty(serial) && serial!.Length > 4)
                    serial = serial.Substring(0, 4) + "-" + serial.Substring(4);

                // If the L0 length is the size of the full disc, there's no layerbreak
                if (Int64.TryParse(layer, out long umdlayerValue) && umdlayerValue * 2048 == size)
                    layer = null;

                return true;
            }
            catch
            {
                // We don't care what the exception is right now
                return false;
            }
        }

        /// <summary>
        /// Get all Volume Identifiers
        /// </summary>
        /// <param name="volDesc">_volDesc.txt file location</param>
        /// <returns>Volume labels (by type), or null if none present</returns>
        /// <remarks>This is a copy of the code from DiscImageCreator and has extrandous checks</remarks>
        private static bool GetVolumeLabels(string volDesc, out Dictionary<string, List<string>> volLabels)
        {
            // If the file doesn't exist, can't get the volume labels
            volLabels = [];
            if (!File.Exists(volDesc))
                return false;

            try
            {
                using var sr = File.OpenText(volDesc);
                var line = sr.ReadLine();

                string volType = "UNKNOWN";
                string label;
                while (line != null)
                {
                    // Trim the line for later use
                    line = line.Trim();

                    // ISO9660 and extensions section
                    if (line.StartsWith("Volume Descriptor Type: "))
                    {
                        Int32.TryParse(line.Substring("Volume Descriptor Type: ".Length), out int volTypeInt);
                        volType = volTypeInt switch
                        {
                            // 0 => "Boot Record" // Should not not contain a Volume Identifier
                            1 => "ISO", // ISO9660
                            2 => "Joliet",
                            // 3 => "Volume Partition Descriptor" // Should not not contain a Volume Identifier
                            // 255 => "???" // Should not not contain a Volume Identifier
                            _ => "UNKNOWN" // Should not contain a Volume Identifier
                        };
                    }
                    // UDF section
                    else if (line.StartsWith("Primary Volume Descriptor Number:"))
                    {
                        volType = "UDF";
                    }
                    // Identifier
                    else if (line.StartsWith("Volume Identifier: "))
                    {
                        label = line.Substring("Volume Identifier: ".Length);

                        // Remove leading non-printable character (unsure why DIC outputs this)
                        if (Convert.ToUInt32(label[0]) == 0x7F || Convert.ToUInt32(label[0]) < 0x20)
                            label = label.Substring(1);

                        // Skip if label is blank
                        if (label == null || label.Length <= 0)
                        {
                            volType = "UNKNOWN";
                            line = sr.ReadLine();
                            continue;
                        }

                        if (volLabels.ContainsKey(label))
                            volLabels[label].Add(volType);
                        else
                            volLabels.Add(label, [volType]);

                        // Reset volume type
                        volType = "UNKNOWN";
                    }

                    line = sr.ReadLine();
                }

                // Return true if a volume label was found
                return volLabels.Count > 0;
            }
            catch
            {
                // We don't care what the exception is right now
                volLabels = [];
                return false;
            }
        }

        #endregion
    }
}
