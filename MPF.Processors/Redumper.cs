using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SabreTools.Hashing;
using SabreTools.Models.CueSheets;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;

namespace MPF.Processors
{
    /// <summary>
    /// Represents processing Redumper outputs
    /// </summary>
    public sealed class Redumper : BaseProcessor
    {
        /// <inheritdoc/>
        public Redumper(RedumpSystem? system, MediaType? type) : base(system, type) { }

        #region BaseProcessor Implementations

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, string basePath, bool redumpCompat)
        {
            // Ensure that required sections exist
            info = Builder.EnsureAllSections(info);

            // Get the dumping program and version
            info.DumpingInfo!.DumpingProgram ??= string.Empty;
            info.DumpingInfo.DumpingProgram += $" {GetVersion($"{basePath}.log") ?? "Unknown Version"}";
            info.DumpingInfo.DumpingDate = ProcessingTool.GetFileModifiedDate($"{basePath}.log")?.ToString("yyyy-MM-dd HH:mm:ss");

            // Fill in the hardware data
            if (GetHardwareInfo($"{basePath}.log", out var manufacturer, out var model, out var firmware))
            {
                info.DumpingInfo.Manufacturer = manufacturer;
                info.DumpingInfo.Model = model;
                info.DumpingInfo.Firmware = firmware;
            }

            // Fill in the disc type data
            if (GetDiscType($"{basePath}.log", out var discTypeOrBookType))
                info.DumpingInfo.ReportedDiscType = discTypeOrBookType;

            // Fill in the volume labels
            if (GetVolumeLabels($"{basePath}.log", out var volLabels))
                VolumeLabels = volLabels;

            switch (Type)
            {
                case MediaType.CDROM:
                    info.Extras!.PVD = GetPVD($"{basePath}.log") ?? "Disc has no PVD";
                    info.TracksAndWriteOffsets!.ClrMameProData = GetDatfile($"{basePath}.log");
                    info.TracksAndWriteOffsets.Cuesheet = ProcessingTool.GetFullFile($"{basePath}.cue") ?? string.Empty;

                    // Attempt to get the write offset
                    string cdWriteOffset = GetWriteOffset($"{basePath}.log") ?? string.Empty;
                    info.CommonDiscInfo!.RingWriteOffset = cdWriteOffset;
                    info.TracksAndWriteOffsets.OtherWriteOffsets = cdWriteOffset;

                    // Attempt to get the error count
                    if (GetErrorCount($"{basePath}.log", out long redumpErrors, out long c2Errors))
                    {
                        info.CommonDiscInfo.ErrorsCount = (redumpErrors == -1 ? "Error retrieving error count" : redumpErrors.ToString());
                        info.DumpingInfo.C2ErrorsCount = (c2Errors == -1 ? "Error retrieving error count" : c2Errors.ToString());
                    }

                    // Attempt to get multisession data
                    string cdMultiSessionInfo = GetMultisessionInformation($"{basePath}.log") ?? string.Empty;
                    if (!string.IsNullOrEmpty(cdMultiSessionInfo))
                        info.CommonDiscInfo.CommentsSpecialFields![SiteCode.Multisession] = cdMultiSessionInfo;

                    // Attempt to get the universal hash, if it's an audio disc
                    if (System.IsAudio())
                    {
                        string universalHash = GetUniversalHash($"{basePath}.log") ?? string.Empty;
                        info.CommonDiscInfo.CommentsSpecialFields![SiteCode.UniversalHash] = universalHash;
                    }

                    // Attempt to get the non-zero data start, if it's an audio disc
                    if (System.IsAudio())
                    {
                        string ringNonZeroDataStart = GetRingNonZeroDataStart($"{basePath}.log") ?? string.Empty;
                        info.CommonDiscInfo.CommentsSpecialFields![SiteCode.RingNonZeroDataStart] = ringNonZeroDataStart;
                    }

                    break;

                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                    info.Extras!.PVD = GetPVD($"{basePath}.log") ?? "Disc has no PVD";
                    info.TracksAndWriteOffsets!.ClrMameProData = GetDatfile($"{basePath}.log");

                    // Get the individual hash data, as per internal
                    if (ProcessingTool.GetISOHashValues(info.TracksAndWriteOffsets.ClrMameProData, out long size, out var crc32, out var md5, out var sha1))
                    {
                        info.SizeAndChecksums!.Size = size;
                        info.SizeAndChecksums.CRC32 = crc32;
                        info.SizeAndChecksums.MD5 = md5;
                        info.SizeAndChecksums.SHA1 = sha1;
                    }

                    // Deal with the layerbreaks
                    if (GetLayerbreaks($"{basePath}.log", out var layerbreak1, out var layerbreak2, out var layerbreak3))
                    {
                        info.SizeAndChecksums!.Layerbreak = !string.IsNullOrEmpty(layerbreak1) ? Int64.Parse(layerbreak1) : default;
                        info.SizeAndChecksums!.Layerbreak2 = !string.IsNullOrEmpty(layerbreak2) ? Int64.Parse(layerbreak2) : default;
                        info.SizeAndChecksums!.Layerbreak3 = !string.IsNullOrEmpty(layerbreak3) ? Int64.Parse(layerbreak3) : default;
                    }

                    // Bluray-specific options
                    if (Type == MediaType.BluRay)
                    {
                        int trimLength = -1;
                        switch (System)
                        {
                            case RedumpSystem.MicrosoftXboxOne:
                            case RedumpSystem.MicrosoftXboxSeriesXS:
                            case RedumpSystem.SonyPlayStation3:
                            case RedumpSystem.SonyPlayStation4:
                            case RedumpSystem.SonyPlayStation5:
                                if (info.SizeAndChecksums!.Layerbreak3 != default)
                                    trimLength = 520;
                                else if (info.SizeAndChecksums!.Layerbreak2 != default)
                                    trimLength = 392;
                                else
                                    trimLength = 264;
                                break;
                        }

                        info.Extras!.PIC = GetPIC($"{basePath}.physical", trimLength)
                            ?? GetPIC($"{basePath}.0.physical", trimLength)
                            ?? GetPIC($"{basePath}.1.physical", trimLength)
                            ?? string.Empty;

                        var di = ProcessingTool.GetDiscInformation($"{basePath}.physical")
                            ?? ProcessingTool.GetDiscInformation($"{basePath}.0.physical")
                            ?? ProcessingTool.GetDiscInformation($"{basePath}.1.physical");
                        info.SizeAndChecksums!.PICIdentifier = ProcessingTool.GetPICIdentifier(di);
                    }

                    break;
            }

            switch (System)
            {
                case RedumpSystem.AppleMacintosh:
                case RedumpSystem.EnhancedCD:
                case RedumpSystem.IBMPCcompatible:
                case RedumpSystem.RainbowDisc:
                case RedumpSystem.SonyElectronicBook:
                    info.CopyProtection!.SecuROMData = GetSecuROMData($"{basePath}.log") ?? string.Empty;

                    // Needed for some odd copy protections
                    info.CopyProtection!.Protection = GetDVDProtection($"{basePath}.log", false) ?? string.Empty;
                    break;

                case RedumpSystem.DVDAudio:
                case RedumpSystem.DVDVideo:
                    info.CopyProtection!.Protection = GetDVDProtection($"{basePath}.log", true) ?? string.Empty;
                    break;

                case RedumpSystem.KonamiPython2:
                    if (GetPlayStationInfo($"{basePath}.log", out string? kp2EXEDate, out string? kp2Serial, out string? kp2Version))
                    {
                        info.CommonDiscInfo!.EXEDateBuildDate = kp2EXEDate;
                        info.CommonDiscInfo.CommentsSpecialFields![SiteCode.InternalSerialName] = kp2Serial ?? string.Empty;
                        info.VersionAndEditions!.Version = kp2Version ?? string.Empty;
                    }

                    break;

                case RedumpSystem.MicrosoftXbox:
                    string xmidString = ProcessingTool.GetXMID($"{basePath}.manufacturer");
                    var xmid = SabreTools.Serialization.Wrappers.XMID.Create(xmidString);
                    if (xmid != null)
                    {
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.XMID] = xmidString?.TrimEnd('\0') ?? string.Empty;
                        info.CommonDiscInfo.Serial = xmid.Serial ?? string.Empty;
                        if (!redumpCompat)
                            info.VersionAndEditions!.Version = xmid.Version ?? string.Empty;

                        info.CommonDiscInfo.Region = ProcessingTool.GetXGDRegion(xmid.Model.RegionIdentifier);
                    }

                    if (HashTool.GetStandardHashes($"{basePath}.manufacturer", out _, out string? dmi1Crc, out _, out _))
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.DMIHash] = dmi1Crc ?? string.Empty;
                    if (HashTool.GetStandardHashes($"{basePath}.physical", out _, out string? pfi1Crc, out _, out _))
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.PFIHash] = pfi1Crc ?? string.Empty;

                    // TODO: Support SS information when generated
                    break;

                case RedumpSystem.MicrosoftXbox360:
                    string xemidString = ProcessingTool.GetXeMID($"{basePath}.manufacturer");
                    var xemid = SabreTools.Serialization.Wrappers.XeMID.Create(xemidString);
                    if (xemid != null)
                    {
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.XeMID] = xemidString?.TrimEnd('\0') ?? string.Empty;
                        info.CommonDiscInfo.Serial = xemid.Serial ?? string.Empty;
                        if (!redumpCompat)
                            info.VersionAndEditions!.Version = xemid.Version ?? string.Empty;

                        info.CommonDiscInfo.Region = ProcessingTool.GetXGDRegion(xemid.Model.RegionIdentifier);
                    }

                    if (HashTool.GetStandardHashes($"{basePath}.manufacturer", out _, out string? dmi23Crc, out _, out _))
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.DMIHash] = dmi23Crc ?? string.Empty;
                    if (HashTool.GetStandardHashes($"{basePath}.physical", out _, out string? pfi23Crc, out _, out _))
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.PFIHash] = pfi23Crc ?? string.Empty;

                    // TODO: Support SS information when generated
                    break;

                case RedumpSystem.NamcoSegaNintendoTriforce:
                    if (Type == MediaType.CDROM)
                    {
                        info.Extras!.Header = GetGDROMHeader($"{basePath}.log",
                            out string? buildDate,
                            out string? serial,
                            out _,
                            out string? version) ?? string.Empty;
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = serial ?? string.Empty;
                        info.CommonDiscInfo.EXEDateBuildDate = buildDate ?? string.Empty;
                        // TODO: Support region setting from parsed value
                        info.VersionAndEditions!.Version = version ?? string.Empty;
                    }
                    break;

                case RedumpSystem.SegaMegaCDSegaCD:
                    info.Extras!.Header = GetSegaCDHeader($"{basePath}.log", out var scdBuildDate, out var scdSerial, out _) ?? string.Empty;
                    info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = scdSerial ?? string.Empty;
                    info.CommonDiscInfo.EXEDateBuildDate = scdBuildDate ?? string.Empty;
                    // TODO: Support region setting from parsed value
                    break;

                case RedumpSystem.SegaChihiro:
                    if (Type == MediaType.CDROM)
                    {
                        info.Extras!.Header = GetGDROMHeader($"{basePath}.log",
                            out string? buildDate,
                            out string? serial,
                            out _,
                            out string? version) ?? string.Empty;
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = serial ?? string.Empty;
                        info.CommonDiscInfo.EXEDateBuildDate = buildDate ?? string.Empty;
                        // TODO: Support region setting from parsed value
                        info.VersionAndEditions!.Version = version ?? string.Empty;
                    }
                    break;

                case RedumpSystem.SegaDreamcast:
                    if (Type == MediaType.CDROM)
                    {
                        info.Extras!.Header = GetGDROMHeader($"{basePath}.log",
                            out string? buildDate,
                            out string? serial,
                            out _,
                            out string? version) ?? string.Empty;
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = serial ?? string.Empty;
                        info.CommonDiscInfo.EXEDateBuildDate = buildDate ?? string.Empty;
                        // TODO: Support region setting from parsed value
                        info.VersionAndEditions!.Version = version ?? string.Empty;
                    }
                    break;

                case RedumpSystem.SegaNaomi:
                    if (Type == MediaType.CDROM)
                    {
                        info.Extras!.Header = GetGDROMHeader($"{basePath}.log",
                            out string? buildDate,
                            out string? serial,
                            out _,
                            out string? version) ?? string.Empty;
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = serial ?? string.Empty;
                        info.CommonDiscInfo.EXEDateBuildDate = buildDate ?? string.Empty;
                        // TODO: Support region setting from parsed value
                        info.VersionAndEditions!.Version = version ?? string.Empty;
                    }
                    break;

                case RedumpSystem.SegaNaomi2:
                    if (Type == MediaType.CDROM)
                    {
                        info.Extras!.Header = GetGDROMHeader($"{basePath}.log",
                            out string? buildDate,
                            out string? serial,
                            out _,
                            out string? version) ?? string.Empty;
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = serial ?? string.Empty;
                        info.CommonDiscInfo.EXEDateBuildDate = buildDate ?? string.Empty;
                        // TODO: Support region setting from parsed value
                        info.VersionAndEditions!.Version = version ?? string.Empty;
                    }
                    break;

                case RedumpSystem.SegaSaturn:
                    info.Extras!.Header = GetSaturnHeader($"{basePath}.log",
                        out string? saturnBuildDate,
                        out string? saturnSerial,
                        out _,
                        out string? saturnVersion) ?? string.Empty;
                    info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = saturnSerial ?? string.Empty;
                    info.CommonDiscInfo.EXEDateBuildDate = saturnBuildDate ?? string.Empty;
                    // TODO: Support region setting from parsed value
                    info.VersionAndEditions!.Version = saturnVersion ?? string.Empty;
                    break;

                case RedumpSystem.SonyPlayStation:
                    if (GetPlayStationInfo($"{basePath}.log", out string? psxEXEDate, out string? psxSerial, out var _))
                    {
                        info.CommonDiscInfo!.EXEDateBuildDate = psxEXEDate;
                        info.CommonDiscInfo.CommentsSpecialFields![SiteCode.InternalSerialName] = psxSerial ?? string.Empty;
                    }

                    info.CopyProtection!.AntiModchip = GetPlayStationAntiModchipDetected($"{basePath}.log").ToYesNo();
                    info.EDC!.EDC = GetPlayStationEDCStatus($"{basePath}.log").ToYesNo();
                    info.CopyProtection.LibCrypt = GetPlayStationLibCryptStatus($"{basePath}.log").ToYesNo();
                    info.CopyProtection.LibCryptData = GetPlayStationLibCryptData($"{basePath}.log");
                    break;

                case RedumpSystem.SonyPlayStation2:
                    if (GetPlayStationInfo($"{basePath}.log", out string? ps2EXEDate, out string? ps2Serial, out var ps2Version))
                    {
                        info.CommonDiscInfo!.EXEDateBuildDate = ps2EXEDate;
                        info.CommonDiscInfo.CommentsSpecialFields![SiteCode.InternalSerialName] = ps2Serial ?? string.Empty;
                        info.VersionAndEditions!.Version = ps2Version ?? string.Empty;
                    }

                    break;

                case RedumpSystem.SonyPlayStation3:
                    if (GetPlayStationInfo($"{basePath}.log", out var _, out string? ps3Serial, out var ps3Version))
                    {
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = ps3Serial ?? string.Empty;
                        info.VersionAndEditions!.Version = ps3Version ?? string.Empty;
                    }

                    break;

                case RedumpSystem.SonyPlayStation4:
                    if (GetPlayStationInfo($"{basePath}.log", out var _, out string? ps4Serial, out var ps4Version))
                    {
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = ps4Serial ?? string.Empty;
                        info.VersionAndEditions!.Version = ps4Version ?? string.Empty;
                    }

                    break;

                case RedumpSystem.SonyPlayStation5:
                    if (GetPlayStationInfo($"{basePath}.log", out var _, out string? ps5Serial, out var ps5Version))
                    {
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = ps5Serial ?? string.Empty;
                        info.VersionAndEditions!.Version = ps5Version ?? string.Empty;
                    }

                    break;
            }
        }

        /// <inheritdoc/>
        internal override List<OutputFile> GetOutputFiles(string baseFilename)
        {
            switch (Type)
            {
                case MediaType.CDROM:
                    List<OutputFile> cdrom = [
                        new($"{baseFilename}.asus", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "asus"),
                        new($"{baseFilename}.atip", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "atip"),
                        new($"{baseFilename}.cdtext", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "cdtext"),
                        new($"{baseFilename}.cue", OutputFileFlags.Required),
                        new($"{baseFilename}.fulltoc", OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "fulltoc"),
                        new($"{baseFilename}.log", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "log"),
                        new CustomOutputFile($"{baseFilename}.log", OutputFileFlags.Required,
                            DatfileExists),
                        new($"{baseFilename}.pma", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "pma"),
                        new([$"{baseFilename}.scram", $"{baseFilename}.scrap"], OutputFileFlags.Required
                            | OutputFileFlags.Deleteable),
                        new($"{baseFilename}.state", OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "state"),
                        new($"{baseFilename}.subcode", OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "subcode"),
                        new($"{baseFilename}.toc", OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "toc"),
                    ];

                    // Include .hash and .skeleton for all files in cuesheet
                    var cueSheet = SabreTools.Serialization.Deserializers.CueSheet.DeserializeFile($"{baseFilename}.cue");
                    if (cueSheet?.Files != null)
                    {
                        int trackId = 1;
                        foreach (CueFile? file in cueSheet.Files)
                        {
                            string? trackName = Path.GetFileNameWithoutExtension(file?.FileName);
                            if (trackName == null)
                                continue;

                            cdrom.Add(new($"{trackName}.hash", OutputFileFlags.Binary
                                | OutputFileFlags.Zippable,
                                $"hash_{trackId}"));
                            cdrom.Add(new($"{trackName}.skeleton", OutputFileFlags.Binary
                                | OutputFileFlags.Zippable,
                                $"skeleton_{trackId}"));
                            trackId++;
                        }
                    }
                    else
                    {
                        cdrom.Add(new($"{baseFilename}.hash", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "hash"));
                        cdrom.Add(new($"{baseFilename}.skeleton", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "skeleton"));
                    }

                    return cdrom;

                case MediaType.DVD:
                    return [
                        new($"{baseFilename}.asus", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "asus"),
                        new($"{baseFilename}.hash", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "hash"),
                        new($"{baseFilename}.log", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "log"),
                        new CustomOutputFile($"{baseFilename}.log", OutputFileFlags.Required,
                            DatfileExists),
                        new([$"{baseFilename}.manufacturer", $"{baseFilename}.1.manufacturer"], OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "manufacturer_1"),
                        new($"{baseFilename}.2.manufacturer", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "manufacturer_2"),
                        new([$"{baseFilename}.physical", $"{baseFilename}.0.physical"], OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "physical_0"),
                        new($"{baseFilename}.1.physical", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "physical_1"),
                        new($"{baseFilename}.2.physical", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "physical_2"),
                        new($"{baseFilename}.security", System.IsXGD()
                            ? OutputFileFlags.Required | OutputFileFlags.Binary | OutputFileFlags.Zippable
                            : OutputFileFlags.Binary | OutputFileFlags.Zippable,
                            "security"),
                        new($"{baseFilename}.skeleton", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "skeleton"),
                        new($"{baseFilename}.ss", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "ss"),
                        new($"{baseFilename}.ssv1", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "ssv1"),
                        new($"{baseFilename}.ssv2", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "ssv2"),
                        new($"{baseFilename}.state", OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "state"),
                    ];

                case MediaType.HDDVD: // TODO: Confirm that this information outputs
                case MediaType.BluRay:
                    return [
                        new($"{baseFilename}.asus", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "asus"),
                        new($"{baseFilename}.hash", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "hash"),
                        new($"{baseFilename}.log", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "log"),
                        new CustomOutputFile($"{baseFilename}.log", OutputFileFlags.Required,
                            DatfileExists),
                        new([$"{baseFilename}.physical", $"{baseFilename}.0.physical"], OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "physical_0"),
                        new($"{baseFilename}.1.physical", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "physical_1"),
                        new($"{baseFilename}.2.physical", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "physical_2"),
                        new($"{baseFilename}.skeleton", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "skeleton"),
                        new($"{baseFilename}.state", OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "state"),
                    ];
            }

            return [];
        }

        #endregion

        #region Private Extra Methods

        /// <summary>
        /// Get if the datfile exists in the log
        /// </summary>
        /// <param name="log">Log file location</param>
        private static bool DatfileExists(string log)
            => GetDatfile(log) != null;

        #endregion

        #region Information Extraction Methods

        /// <summary>
        /// Get the cuesheet from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Newline-delimited cuesheet if possible, null on error</returns>
        private static string? GetCuesheet(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            try
            {
                // Fast forward to the dat line
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream && sr.ReadLine()?.TrimStart()?.StartsWith("CUE [") == false) ;
                if (sr.EndOfStream)
                    return null;

                // Now that we're at the relevant entries, read each line in and concatenate
                string? cueString = string.Empty, line = sr.ReadLine()?.Trim();
                while (!string.IsNullOrEmpty(line))
                {
                    cueString += line + "\n";
                    line = sr.ReadLine()?.Trim();
                }

                return cueString.TrimEnd('\n');
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get the datfile from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Newline-delimited datfile if possible, null on error</returns>
        private static string? GetDatfile(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            try
            {
                using var sr = File.OpenText(log);
                string? datString = null;

                // Find all occurrences of the hash information 
                while (!sr.EndOfStream)
                {
                    // Fast forward to the dat line
                    while (!sr.EndOfStream && sr.ReadLine()?.TrimStart()?.StartsWith("dat:") == false) ;
                    if (sr.EndOfStream)
                        break;

                    // Now that we're at the relevant entries, read each line in and concatenate
                    datString = string.Empty;
                    var line = sr.ReadLine()?.Trim();
                    while (line?.StartsWith("<rom") == true)
                    {
                        datString += line + "\n";
                        if (sr.EndOfStream)
                            break;

                        line = sr.ReadLine()?.Trim();
                    }
                }

                return datString?.TrimEnd('\n');
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get reported disc type information, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>True if disc type info was set, false otherwise</returns>
        private static bool GetDiscType(string log, out string? discTypeOrBookType)
        {
            // Set the default values
            discTypeOrBookType = null;

            // If the file doesn't exist, we can't get the info
            if (!File.Exists(log))
                return false;

            try
            {
                using var sr = File.OpenText(log);
                var line = sr.ReadLine();
                while (line != null)
                {
                    // Trim the line for later use
                    line = line.Trim();

                    // The profile is listed in a single line
                    if (line.StartsWith("current profile:"))
                    {
                        // current profile: <discType>
                        discTypeOrBookType = line.Substring("current profile: ".Length);
                    }

                    line = sr.ReadLine();
                }

                return true;
            }
            catch
            {
                // We don't care what the exception is right now
                discTypeOrBookType = null;
                return false;
            }
        }

        /// <summary>
        /// Get the DVD protection information, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <param name="includeAlways">Indicates whether region and protection type are always included</param>
        /// <returns>Formatted string representing the DVD protection, null on error</returns>
        private static string? GetDVDProtection(string log, bool includeAlways)
        {
            // If one of the files doesn't exist, we can't get info from them
            if (!File.Exists(log))
                return null;

            // Setup all of the individual pieces
            string? region = null, rceProtection = null, copyrightProtectionSystemType = null, vobKeys = null, decryptedDiscKey = null;
            using (var sr = File.OpenText(log))
            {
                try
                {
                    // Fast forward to the copyright information
                    while (sr.ReadLine()?.Trim().StartsWith("copyright:") == false) ;

                    // Now read until we hit the manufacturing information
                    var line = sr.ReadLine()?.Trim();
                    while (line != null && !sr.EndOfStream)
                    {
                        if (line.StartsWith("protection system type"))
                        {
                            copyrightProtectionSystemType = line.Substring("protection system type: ".Length);
                            if (copyrightProtectionSystemType == "none" || copyrightProtectionSystemType == "<none>")
                                copyrightProtectionSystemType = "No";
                        }
                        else if (line.StartsWith("region management information:"))
                        {
                            region = line.Substring("region management information: ".Length);
                        }
                        else if (line.StartsWith("disc key"))
                        {
                            decryptedDiscKey = line.Substring("disc key: ".Length).Replace(':', ' ');
                        }
                        else if (line.StartsWith("title keys"))
                        {
                            vobKeys = string.Empty;

                            line = sr.ReadLine()?.Trim();
                            while (!string.IsNullOrEmpty(line))
                            {
                                var match = Regex.Match(line, @"^(.*?): (.*?)$", RegexOptions.Compiled);
                                if (match.Success)
                                {
                                    string normalizedKey = match.Groups[2].Value.Replace(':', ' ');
                                    if (normalizedKey == "none" || normalizedKey == "<none>")
                                        normalizedKey = "No Title Key";
                                    else if (normalizedKey == "<error>")
                                        normalizedKey = "Error Retrieving Title Key";

                                    vobKeys += $"{match.Groups[1].Value} Title Key: {match.Groups[2].Value.Replace(':', ' ')}\n";
                                }
                                else
                                {
                                    break;
                                }

                                line = sr.ReadLine()?.Trim();
                            }
                        }
                        else
                        {
                            break;
                        }

                        line = sr.ReadLine()?.Trim();
                    }
                }
                catch { }
            }

            // Filter out if we're not always including information
            if (!includeAlways)
            {
                if (region == "1 2 3 4 5 6 7 8")
                    region = null;
                if (copyrightProtectionSystemType == "No")
                    copyrightProtectionSystemType = null;
            }

            // Now we format everything we can
            string protection = string.Empty;
            if (!string.IsNullOrEmpty(region))
                protection += $"Region: {region}\n";
            if (!string.IsNullOrEmpty(rceProtection))
                protection += $"RCE Protection: {rceProtection}\n";
            if (!string.IsNullOrEmpty(copyrightProtectionSystemType))
                protection += $"Copyright Protection System Type: {copyrightProtectionSystemType}\n";
            if (!string.IsNullOrEmpty(vobKeys))
                protection += vobKeys;
            if (!string.IsNullOrEmpty(decryptedDiscKey))
                protection += $"Decrypted Disc Key: {decryptedDiscKey}\n";

            return protection;
        }

        /// <summary>
        /// Get the detected error counts from the input files, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>True if error counts could be retrieved, false otherwise</returns>
        private static bool GetErrorCount(string log, out long redumpErrors, out long c2Errors)
        {
            // Set the default values for error counts
            redumpErrors = -1; c2Errors = -1;

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return false;

            try
            {
                using var sr = File.OpenText(log);

                // Find the error counts
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine()?.Trim();
                    if (line == null)
                        break;

                    // C2: <error count>
                    if (line.StartsWith("C2:"))
                    {
                        string[] parts = line.Split(' ');
                        if (!long.TryParse(parts[1], out c2Errors))
                            c2Errors = -1;
                    }

                    // REDUMP.ORG errors: <error count>
                    else if (line.StartsWith("REDUMP.ORG errors:"))
                    {
                        string[] parts = line!.Split(' ');
                        if (!long.TryParse(parts[2], out redumpErrors))
                            redumpErrors = -1;
                    }
                }

                // If the Redump error count is -1, then an issue occurred
                return redumpErrors != -1;
            }
            catch
            {
                // We don't care what the exception is right now
                return false;
            }
        }

        /// <summary>
        /// Get the header from a GD-ROM LD area, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Header as a string if possible, null on error</returns>
        private static string? GetGDROMHeader(string log, out string? buildDate, out string? serial, out string? region, out string? version)
        {
            // Set the default values
            buildDate = null; serial = null; region = null; version = null;

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            try
            {
                // Fast forward to the MCD line
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream && sr.ReadLine()?.TrimStart()?.StartsWith("DC [") == false) ;
                if (sr.EndOfStream)
                    return null;

                string? line, headerString = string.Empty;
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine()?.TrimStart();
                    if (line == null)
                        break;

                    if (line.StartsWith("build date:"))
                    {
                        buildDate = line.Substring("build date: ".Length).Trim();
                    }
                    else if (line.StartsWith("serial:"))
                    {
                        serial = line.Substring("serial: ".Length).Trim();
                    }
                    else if (line.StartsWith("region:"))
                    {
                        region = line.Substring("region: ".Length).Trim();
                    }
                    else if (line.StartsWith("regions:"))
                    {
                        region = line.Substring("regions: ".Length).Trim();
                    }
                    else if (line.StartsWith("version:"))
                    {
                        version = line.Substring("version: ".Length).Trim();
                    }
                    else if (line.StartsWith("header:"))
                    {
                        line = sr.ReadLine()?.TrimStart();
                        while (line?.StartsWith("00") == true)
                        {
                            headerString += line + "\n";
                            line = sr.ReadLine()?.Trim();
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                return headerString.TrimEnd('\n');
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get hardware information from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>True if hardware info was set, false otherwise</returns>
        private static bool GetHardwareInfo(string log, out string? manufacturer, out string? model, out string? firmware)
        {
            // Set the default values
            manufacturer = null; model = null; firmware = null;

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return false;

            try
            {
                // Fast forward to the drive information line
                using var sr = File.OpenText(log);
                while (!(sr.ReadLine()?.Trim().StartsWith("drive path:") ?? true)) ;

                // If we find the hardware info line, return each value
                // drive: <vendor_id> - <product_id> (revision level: <product_revision_level>, vendor specific: <vendor_specific>)
                var regex = new Regex(@"drive: (.+) - (.+) \(revision level: (.+), vendor specific: (.+)\)", RegexOptions.Compiled);

                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    var match = regex.Match(line.Trim());
                    if (match.Success)
                    {
                        manufacturer = match.Groups[1].Value;
                        model = match.Groups[2].Value;
                        firmware = match.Groups[3].Value;
                        firmware += match.Groups[4].Value == "<empty>" ? "" : $" ({match.Groups[4].Value})";
                        return true;
                    }
                }

                // We couldn't detect it then
                return false;
            }
            catch
            {
                // We don't care what the exception is right now
                return false;
            }
        }

        /// <summary>
        /// Get the layerbreaks from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>True if any layerbreaks were found, false otherwise</returns>
        private static bool GetLayerbreaks(string log, out string? layerbreak1, out string? layerbreak2, out string? layerbreak3)
        {
            // Set the default values
            layerbreak1 = null; layerbreak2 = null; layerbreak3 = null;

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return false;

            try
            {
                // Find the layerbreak
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine()?.Trim();

                    // If we have a null line, just break
                    if (line == null)
                        break;

                    // Single-layer discs have no layerbreak
                    if (line.Contains("layers count: 1"))
                    {
                        return false;
                    }

                    // Dual-layer discs have a regular layerbreak (old)
                    else if (line.StartsWith("data "))
                    {
                        // data { LBA: <startLBA> .. <endLBA>, length: <length>, hLBA: <startLBA> .. <endLBA> }
                        string[] split = line.Split(' ').Where(s => !string.IsNullOrEmpty(s)).ToArray();
                        layerbreak1 ??= split[7].TrimEnd(',');
                    }

                    // Dual-layer discs have a regular layerbreak (new)
                    else if (line.StartsWith("layer break:"))
                    {
                        // layer break: <layerbreak>
                        layerbreak1 = line.Substring("layer break: ".Length).Trim();
                    }

                    // Multi-layer discs have the layer in the name
                    else if (line.StartsWith("layer break (layer: 0):"))
                    {
                        // layer break (layer: 0): <layerbreak>
                        layerbreak1 = line.Substring("layer break (layer: 0): ".Length).Trim();
                    }
                    else if (line.StartsWith("layer break (layer: 1):"))
                    {
                        // layer break (layer: 1): <layerbreak>
                        layerbreak2 = line.Substring("layer break (layer: 1): ".Length).Trim();
                    }
                    else if (line.StartsWith("layer break (layer: 2):"))
                    {
                        // layer break (layer: 2): <layerbreak>
                        layerbreak3 = line.Substring("layer break (layer: 2): ".Length).Trim();
                    }
                }

                // Return the layerbreak, if possible
                return true;
            }
            catch
            {
                // We don't care what the exception is right now
                return false;
            }
        }

        /// <summary>
        /// Get multisession information from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Formatted multisession information, null on error</returns>
        private static string? GetMultisessionInformation(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            try
            {
                // Fast forward to the multisession lines
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream && sr.ReadLine()?.Trim()?.StartsWith("multisession:") == false) ;
                if (sr.EndOfStream)
                    return null;

                // Now that we're at the relevant lines, find the session info
                string? firstSession = null, secondSession = null;
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine()?.Trim();

                    // If we have a null line, just break
                    if (line == null)
                        break;

                    // Store the first session range
                    if (line.Contains("session 1:"))
                        firstSession = line.Substring("session 1: ".Length).Trim();

                    // Store the secomd session range
                    else if (line.Contains("session 2:"))
                        secondSession = line.Substring("session 2: ".Length).Trim();
                }

                // If either is blank, we don't have multisession
                if (string.IsNullOrEmpty(firstSession) || string.IsNullOrEmpty(secondSession))
                    return null;

                // Create and return the formatted output
                return $"Session 1: {firstSession}\nSession 2: {secondSession}";
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get the existence of an anti-modchip string from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Anti-modchip existence if possible, false on error</returns>
        private static bool? GetPlayStationAntiModchipDetected(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            try
            {
                // Check for the anti-modchip strings
                using var sr = File.OpenText(log);
                var line = sr.ReadLine()?.Trim();
                while (!sr.EndOfStream)
                {
                    if (line == null)
                        return false;

                    if (line.StartsWith("anti-modchip: no"))
                        return false;
                    else if (line.StartsWith("anti-modchip: yes"))
                        return true;

                    line = sr.ReadLine()?.Trim();
                }

                return false;
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get the detected missing EDC count from the input files, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Status of PS1 EDC, if possible</returns>
        private static bool? GetPlayStationEDCStatus(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            try
            {
                // Check for the EDC strings
                using var sr = File.OpenText(log);
                var line = sr.ReadLine()?.Trim();
                while (!sr.EndOfStream)
                {
                    if (line == null)
                        return false;

                    if (line.Contains("EDC: no"))
                        return false;
                    else if (line.Contains("EDC: yes"))
                        return true;

                    line = sr.ReadLine()?.Trim();
                }

                return false;
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get the info from a PlayStation disc, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>True if section found, null on error</returns>
        private static bool GetPlayStationInfo(string log, out string? exeDate, out string? serial, out string? version)
        {
            // Set the default values
            exeDate = null; serial = null; version = null;

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return false;

            try
            {
                // Fast forward to the PS info line
                using var sr = File.OpenText(log);
                string? line;
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine()?.TrimStart();
                    if (line?.StartsWith("PSX [") == true ||
                        line?.StartsWith("PS2 [") == true ||
                        line?.StartsWith("PS3 [") == true ||
                        line?.StartsWith("PS4 [") == true ||
                        line?.StartsWith("PS5 [") == true)
                        break;
                }
                if (sr.EndOfStream)
                    return false;

                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine()?.TrimStart();
                    if (line == null)
                        break;

                    if (line.StartsWith("anti-modchip:"))
                    {
                        // Valid but skip
                    }
                    else if (line.StartsWith("EXE:"))
                    {
                        // Valid but skip
                    }
                    else if (line.StartsWith("EXE date:"))
                    {
                        exeDate = line.Substring("EXE date: ".Length).Trim();
                    }
                    else if (line.StartsWith("libcrypt:"))
                    {
                        // Valid but skip
                    }
                    else if (line.StartsWith("region:"))
                    {
                        // Valid but skip
                    }
                    else if (line.StartsWith("serial:"))
                    {
                        serial = line.Substring("serial: ".Length).Trim();
                    }
                    else if (line.StartsWith("version:"))
                    {
                        version = line.Substring("version: ".Length).Trim();
                    }
                    else
                    {
                        break;
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

        /// <summary>
        /// Get the LibCrypt data from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>PS1 LibCrypt data, if possible</returns>
        private static string? GetPlayStationLibCryptData(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            try
            {
                // Fast forward to the LibCrypt line
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream && sr.ReadLine()?.TrimStart()?.StartsWith("libcrypt:") == false) ;
                if (sr.EndOfStream)
                    return null;

                // Now that we're at the relevant entries, read each line in and concatenate
                string? libCryptString = "", line = sr.ReadLine()?.Trim();
                while (line?.StartsWith("MSF:") == true)
                {
                    libCryptString += line + "\n";
                    line = sr.ReadLine()?.Trim();
                }

                return libCryptString.TrimEnd('\n');
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get the existence of LibCrypt from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Status of PS1 LibCrypt, if possible</returns>
        private static bool? GetPlayStationLibCryptStatus(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            try
            {
                // Check for the libcrypt strings
                using var sr = File.OpenText(log);
                var line = sr.ReadLine()?.Trim();
                while (!sr.EndOfStream)
                {
                    if (line == null)
                        return false;

                    if (line.StartsWith("libcrypt: no"))
                        return false;
                    else if (line.StartsWith("libcrypt: yes"))
                        return true;

                    line = sr.ReadLine()?.Trim();
                }

                return false;
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get the PVD from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Newline-delimited PVD if possible, null on error</returns>
        private static string? GetPVD(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            try
            {
                // Fast forward to the PVD line
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream && sr.ReadLine()?.TrimStart()?.StartsWith("PVD:") == false) ;
                if (sr.EndOfStream)
                    return null;

                // Now that we're at the relevant entries, read each line in and concatenate
                string? pvdString = "", line = sr.ReadLine();
                while (line?.StartsWith("03") == true)
                {
                    pvdString += line + "\n";
                    line = sr.ReadLine();
                }

                return pvdString.TrimEnd('\n');
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get the non-zero data start from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Non-zero dta start if possible, null on error</returns>
        private static string? GetRingNonZeroDataStart(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            try
            {
                // If we find the sample range, return the start value only
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream)
                {
                    string? line = sr.ReadLine()?.TrimStart();
                    if (line?.StartsWith("non-zero data sample range") == true)
                        return line.Substring("non-zero data sample range: [".Length).Trim().Split(' ')[0];
                }

                // We couldn't detect it then
                return null;
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get the build info from a Saturn disc, if possible
        /// </summary>
        /// <<param name="segaHeader">String representing a formatter variant of the Saturn header</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        /// TODO: Remove when Redumper gets native reading support
        private static bool GetSaturnBuildInfo(string? segaHeader, out string? buildDate, out string? serial, out string? version)
        {
            buildDate = null; serial = null; version = null;

            // If the input header is null, we can't do a thing
            if (string.IsNullOrEmpty(segaHeader))
                return false;

            // Now read it in cutting it into lines for easier parsing
            try
            {
                string[] header = segaHeader!.Split('\n');
                string serialVersionLine = header[2].Substring(58);
                string dateLine = header[3].Substring(58);
                serial = serialVersionLine.Substring(0, 10).Trim();
                version = serialVersionLine.Substring(10, 6).TrimStart('V', 'v');
                buildDate = dateLine.Substring(0, 8);
                buildDate = $"{buildDate[0]}{buildDate[1]}{buildDate[2]}{buildDate[3]}-{buildDate[4]}{buildDate[5]}-{buildDate[6]}{buildDate[7]}";
                return true;
            }
            catch
            {
                // We don't care what the error is
                return false;
            }
        }

        /// <summary>
        /// Get the header from a Saturn, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Header as a byte array if possible, null on error</returns>
        private static string? GetSaturnHeader(string log, out string? buildDate, out string? serial, out string? region, out string? version)
        {
            // Set the default values
            buildDate = null; serial = null; region = null; version = null;

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            try
            {
                // Fast forward to the SS line
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream && sr.ReadLine()?.TrimStart()?.StartsWith("SS [") == false) ;
                if (sr.EndOfStream)
                    return null;

                string? line, headerString = "";
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine()?.TrimStart();
                    if (line == null)
                        break;

                    if (line.StartsWith("build date:"))
                    {
                        buildDate = line.Substring("build date: ".Length).Trim();
                    }
                    else if (line.StartsWith("serial:"))
                    {
                        serial = line.Substring("serial: ".Length).Trim();
                    }
                    else if (line.StartsWith("region:"))
                    {
                        region = line.Substring("region: ".Length).Trim();
                    }
                    else if (line.StartsWith("regions:"))
                    {
                        region = line.Substring("regions: ".Length).Trim();
                    }
                    else if (line.StartsWith("version:"))
                    {
                        version = line.Substring("version: ".Length).Trim();
                    }
                    else if (line?.StartsWith("header:") == true)
                    {
                        line = sr.ReadLine()?.TrimStart();
                        while (line?.StartsWith("00") == true)
                        {
                            headerString += line + "\n";
                            line = sr.ReadLine()?.Trim();
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                // Trim the header
                headerString = headerString.TrimEnd('\n');

                // Fallback if any info could not be found
                if (GetSaturnBuildInfo(headerString, out string? buildDateP, out string? serialP, out string? versionP))
                {
                    buildDate ??= buildDateP;
                    serial ??= serialP;
                    version ??= versionP;
                }

                return headerString;
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get the header from a Saturn, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Header as a byte array if possible, null on error</returns>
        private static string? GetSecuROMData(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            try
            {
                // Fast forward to the SecuROM line
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream && sr.ReadLine()?.TrimStart()?.StartsWith("SecuROM [") == false) ;
                if (sr.EndOfStream)
                    return null;

                var lines = new List<string>();
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine()?.TrimStart();

                    // Skip the "version"/"scheme" line
                    if (line?.StartsWith("version:") == true || line?.StartsWith("scheme:") == true)
                        continue;

                    // Only read until while there are MSF lines
                    if (line?.StartsWith("MSF:") != true)
                        break;

                    lines.Add(line);
                }

                return string.Join("\n", [.. lines]).TrimEnd('\n');
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get the header from a Sega CD / Mega CD, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Header as a byte array if possible, null on error</returns>
        private static string? GetSegaCDHeader(string log, out string? buildDate, out string? serial, out string? region)
        {
            // Set the default values
            buildDate = null; serial = null; region = null;

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            try
            {
                // Fast forward to the MCD line
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream && sr.ReadLine()?.TrimStart()?.StartsWith("MCD [") == false) ;
                if (sr.EndOfStream)
                    return null;

                string? line, headerString = string.Empty;
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine()?.TrimStart();
                    if (line == null)
                        break;

                    if (line.StartsWith("build date:"))
                    {
                        buildDate = line.Substring("build date: ".Length).Trim();
                    }
                    else if (line.StartsWith("serial:"))
                    {
                        serial = line.Substring("serial: ".Length).Trim();
                    }
                    else if (line.StartsWith("region:"))
                    {
                        region = line.Substring("region: ".Length).Trim();
                    }
                    else if (line.StartsWith("regions:"))
                    {
                        region = line.Substring("regions: ".Length).Trim();
                    }
                    else if (line.StartsWith("header:"))
                    {
                        line = sr.ReadLine()?.TrimStart();
                        while (line?.StartsWith("01") == true)
                        {
                            headerString += line + "\n";
                            line = sr.ReadLine()?.Trim();
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                return headerString.TrimEnd('\n');
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get the universal hash from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Universal hash if possible, null on error</returns>
        private static string? GetUniversalHash(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            try
            {
                // If we find the universal hash line, return the hash only
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream)
                {
                    string? line = sr.ReadLine()?.TrimStart();
                    if (line?.StartsWith("Universal Hash") == true)
                        return line.Substring("Universal Hash (SHA-1): ".Length).Trim();
                }

                // We couldn't detect it then
                return null;
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get the version. if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Version if possible, null on error</returns>
        private static string? GetVersion(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            // Samples:
            // redumper v2022.10.28 [Oct 28 2022, 05:41:43] (print usage: --help,-h)
            // redumper v2022.12.22 build_87 [Dec 22 2022, 01:56:26]

            try
            {
                // Skip first line (dump date)
                using var sr = File.OpenText(log);
                sr.ReadLine();

                // Get the next non-warning line
                string nextLine = sr.ReadLine()?.Trim() ?? string.Empty;
                if (nextLine.StartsWith("warning:", StringComparison.OrdinalIgnoreCase))
                    nextLine = sr.ReadLine()?.Trim() ?? string.Empty;

                // Generate regex
                // Permissive
                var regex = new Regex(@"^redumper (v.+) \[.+\]", RegexOptions.Compiled);
                // Strict
                //var regex = new Regex(@"^redumper (v\d{4}\.\d{2}\.\d{2}(| build_\d+)) \[.+\]", RegexOptions.Compiled);

                // Extract the version string
                var match = regex.Match(nextLine);
                var version = match.Groups[1].Value;
                return string.IsNullOrEmpty(version) ? null : version;
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get all Volume Identifiers
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Volume labels (by type), or null if none present</returns>
        private static bool GetVolumeLabels(string log, out Dictionary<string, List<string>> volLabels)
        {
            // If the file doesn't exist, can't get the volume labels
            volLabels = [];
            if (!File.Exists(log))
                return false;

            try
            {
                using var sr = File.OpenText(log);
                var line = sr.ReadLine();

                while (line != null)
                {
                    // Trim the line for later use
                    line = line.Trim();

                    // ISO9660 Volume Identifier
                    if (line.StartsWith("volume identifier: "))
                    {
                        string label = line.Substring("volume identifier: ".Length);

                        // Skip if label is blank
                        if (label == null || label.Length <= 0)
                            break;

                        if (volLabels.ContainsKey(label))
                            volLabels[label].Add("ISO");
                        else
                            volLabels[label] = ["ISO"];

                        // Redumper log currently only outputs ISO9660 label, end here
                        break;
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

        /// <summary>
        /// Get the write offset from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Sample write offset if possible, null on error</returns>
        private static string? GetWriteOffset(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            try
            {
                // If we find the disc write offset line, return the offset
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream)
                {
                    string? line = sr.ReadLine()?.TrimStart();
                    if (line?.StartsWith("disc write offset") == true)
                        return line.Substring("disc write offset: ".Length).Trim();
                }

                // We couldn't detect it then
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
