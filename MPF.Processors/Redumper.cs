using System;
using System.Collections.Generic;
using System.IO;
#if NET452_OR_GREATER || NETCOREAPP
using System.IO.Compression;
#endif
using System.Text;
using System.Text.RegularExpressions;
using SabreTools.Hashing;
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
        public Redumper(RedumpSystem? system) : base(system) { }

        #region BaseProcessor Implementations

        /// <inheritdoc/>
        public override MediaType? DetermineMediaType(string? outputDirectory, string outputFilename)
        {
            // If the filename is invalid
            if (string.IsNullOrEmpty(outputFilename))
                return null;

            // Reassemble the base path
            string basePath = Path.GetFileNameWithoutExtension(outputFilename);
            if (!string.IsNullOrEmpty(outputDirectory))
                basePath = Path.Combine(outputDirectory, basePath);

            // Use the log first, if it exists
            if (GetDiscType($"{basePath}.log", out MediaType? mediaType))
                return mediaType;

            // Use the profile for older Redumper versions
            if (GetDiscProfile($"{basePath}.log", out string? discProfile))
                return GetDiscTypeFromProfile(discProfile);

            // The type could not be determined
            return null;
        }

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, MediaType? mediaType, string basePath, bool redumpCompat)
        {
            // Ensure that required sections exist
            info = Builder.EnsureAllSections(info);

            // Get the dumping program and version
            info.DumpingInfo!.DumpingProgram ??= string.Empty;
            info.DumpingInfo.DumpingProgram += $" {GetVersion($"{basePath}.log") ?? "Unknown Version"}";
            info.DumpingInfo.DumpingParameters = GetParameters($"{basePath}.log") ?? "Unknown Parameters";
            info.DumpingInfo.DumpingDate = ProcessingTool.GetFileModifiedDate($"{basePath}.log")?.ToString("yyyy-MM-dd HH:mm:ss");

            // Fill in the hardware data
            if (GetHardwareInfo($"{basePath}.log", out var manufacturer, out var model, out var firmware))
            {
                info.DumpingInfo.Manufacturer = manufacturer;
                info.DumpingInfo.Model = model;
                info.DumpingInfo.Firmware = firmware;
            }

            // Fill in the disc type data
            if (GetDiscProfile($"{basePath}.log", out var discTypeOrBookType))
                info.DumpingInfo.ReportedDiscType = discTypeOrBookType;

            // Get the PVD, if it exists
            info.Extras!.PVD = GetPVD($"{basePath}.log") ?? "Disc has no PVD";

            // Get the Datafile information
            info.TracksAndWriteOffsets!.ClrMameProData = GetDatfile($"{basePath}.log");

            // Get the write offset, if it exists
            string? writeOffset = GetWriteOffset($"{basePath}.log");
            info.CommonDiscInfo!.RingWriteOffset = writeOffset;
            info.TracksAndWriteOffsets.OtherWriteOffsets = writeOffset;

            // Attempt to get multisession data
            string? multiSessionInfo = GetMultisessionInformation($"{basePath}.log");
            if (!string.IsNullOrEmpty(multiSessionInfo))
                info.CommonDiscInfo.CommentsSpecialFields![SiteCode.Multisession] = multiSessionInfo!;

            // Fill in the volume labels (ignore for Xbox/Xbox360)
            if (System != RedumpSystem.MicrosoftXbox && System != RedumpSystem.MicrosoftXbox360)
            {
                if (GetVolumeLabels($"{basePath}.log", out var volLabels))
                    VolumeLabels = volLabels;
            }

            // Extract info based generically on MediaType
            switch (mediaType)
            {
                case MediaType.CDROM:
                    info.TracksAndWriteOffsets.Cuesheet = ProcessingTool.GetFullFile($"{basePath}.cue") ?? string.Empty;

                    // Attempt to get the error count
                    if (GetErrorCount($"{basePath}.log", out long redumpErrors, out long c2Errors))
                    {
                        info.CommonDiscInfo.ErrorsCount = (redumpErrors == -1 ? "Error retrieving error count" : redumpErrors.ToString());
                        info.DumpingInfo.C2ErrorsCount = (c2Errors == -1 ? "Error retrieving error count" : c2Errors.ToString());
                    }

                    // Attempt to get extra metadata if it's an audio disc
                    if (IsAudio(info.TracksAndWriteOffsets.Cuesheet))
                    {
                        string universalHash = GetUniversalHash($"{basePath}.log") ?? string.Empty;
                        info.CommonDiscInfo.CommentsSpecialFields![SiteCode.UniversalHash] = universalHash;

                        string ringNonZeroDataStart = GetRingNonZeroDataStart($"{basePath}.log") ?? string.Empty;
                        info.CommonDiscInfo.CommentsSpecialFields![SiteCode.RingNonZeroDataStart] = ringNonZeroDataStart!;

                        string ringPerfectAudioOffset = GetRingPerfectAudioOffset($"{basePath}.log") ?? string.Empty;
                        info.CommonDiscInfo.CommentsSpecialFields![SiteCode.RingPerfectAudioOffset] = ringPerfectAudioOffset!;
                    }

                    break;

                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                case MediaType.NintendoWiiUOpticalDisc:
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
                        info.SizeAndChecksums!.Layerbreak = !string.IsNullOrEmpty(layerbreak1) ? long.Parse(layerbreak1) : default;
                        info.SizeAndChecksums!.Layerbreak2 = !string.IsNullOrEmpty(layerbreak2) ? long.Parse(layerbreak2) : default;
                        info.SizeAndChecksums!.Layerbreak3 = !string.IsNullOrEmpty(layerbreak3) ? long.Parse(layerbreak3) : default;
                    }

                    // Attempt to get the error count
                    long scsiErrors = GetSCSIErrorCount($"{basePath}.log");
                    info.CommonDiscInfo!.ErrorsCount = (scsiErrors == -1 ? "Error retrieving error count" : scsiErrors.ToString());

                    // Bluray-specific options
                    if (mediaType == MediaType.BluRay || mediaType == MediaType.NintendoWiiUOpticalDisc)
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

            // Extract info based specifically on RedumpSystem
            switch (System)
            {
                case RedumpSystem.AppleMacintosh:
                case RedumpSystem.EnhancedCD:
                case RedumpSystem.IBMPCcompatible:
                case RedumpSystem.RainbowDisc:
                case RedumpSystem.SonyElectronicBook:
                    info.CopyProtection!.SecuROMData = GetSecuROMData($"{basePath}.log", out SecuROMScheme secuROMScheme) ?? string.Empty;
                    if (secuROMScheme == SecuROMScheme.Unknown)
                        info.CommonDiscInfo!.Comments = "Warning: Incorrect SecuROM sector count" + Environment.NewLine;

                    // Needed for some odd copy protections
                    info.CopyProtection!.Protection += GetDVDProtection($"{basePath}.log", false) ?? string.Empty;
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
                case RedumpSystem.MicrosoftXbox360:
                    // If .dmi / .pfi / .ss don't already exist, create them
                    if (!File.Exists($"{basePath}.dmi"))
                        RemoveHeaderAndTrim($"{basePath}.manufacturer", $"{basePath}.dmi");
                    if (!File.Exists($"{basePath}.pfi"))
                        RemoveHeaderAndTrim($"{basePath}.physical", $"{basePath}.pfi");
                    if (!File.Exists($"{basePath}.ss"))
                        ProcessingTool.CleanSS($"{basePath}.security", $"{basePath}.ss");

                    string xmidString = ProcessingTool.GetXMID($"{basePath}.dmi").Trim('\0');
                    if (!string.IsNullOrEmpty(xmidString))
                    {
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.XMID] = xmidString;
                        var xmid = SabreTools.Serialization.Wrappers.XMID.Create(xmidString);
                        info.CommonDiscInfo.Serial = xmid?.Serial ?? string.Empty;
                        if (!redumpCompat)
                        {
                            info.VersionAndEditions!.Version = xmid?.Version ?? string.Empty;
                            info.CommonDiscInfo.Region = ProcessingTool.GetXGDRegion(xmid?.Model.RegionIdentifier);
                        }
                    }

                    string xemidString = ProcessingTool.GetXeMID($"{basePath}.dmi").Trim('\0');
                    if (!string.IsNullOrEmpty(xemidString))
                    {
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.XeMID] = xemidString;
                        var xemid = SabreTools.Serialization.Wrappers.XeMID.Create(xemidString);
                        info.CommonDiscInfo.Serial = xemid?.Serial ?? string.Empty;
                        if (!redumpCompat)
                        {
                            info.VersionAndEditions!.Version = xemid?.Version ?? string.Empty;
                            info.CommonDiscInfo.Region = ProcessingTool.GetXGDRegion(xemid?.Model.RegionIdentifier);
                        }
                    }

                    string? dmiCrc = HashTool.GetFileHash($"{basePath}.dmi", HashType.CRC32);
                    if (dmiCrc != null)
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.DMIHash] = dmiCrc.ToUpperInvariant();
                    string? pfiCrc = HashTool.GetFileHash($"{basePath}.pfi", HashType.CRC32);
                    if (pfiCrc != null)
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.PFIHash] = pfiCrc.ToUpperInvariant();
                    if (ProcessingTool.IsValidSS($"{basePath}.ss") && !ProcessingTool.IsValidPartialSS($"{basePath}.ss"))
                    {
                        string? ssCrc = HashTool.GetFileHash($"{basePath}.ss", HashType.CRC32);
                        if (ssCrc != null)
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.SSHash] = ssCrc.ToUpperInvariant();
                    }

                    string? ssRanges = ProcessingTool.GetSSRanges($"{basePath}.ss");
                    if (!string.IsNullOrEmpty(ssRanges))
                        info.Extras!.SecuritySectorRanges = ssRanges;

                    break;

                case RedumpSystem.NamcoSegaNintendoTriforce:
                    if (mediaType == MediaType.CDROM)
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
                    if (mediaType == MediaType.CDROM)
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
                    if (mediaType == MediaType.CDROM)
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
                    if (mediaType == MediaType.CDROM)
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
                    if (mediaType == MediaType.CDROM)
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

                    string? ps2Protection = GetPlayStation2Protection($"{basePath}.log");
                    if (ps2Protection != null)
                        info.CommonDiscInfo!.Comments = $"<b>Protection</b>: {ps2Protection}" + Environment.NewLine;

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
        internal override List<OutputFile> GetOutputFiles(MediaType? mediaType, string? outputDirectory, string outputFilename)
        {
            // Remove the extension by default
            outputFilename = Path.GetFileNameWithoutExtension(outputFilename);

            // Assemble the base path
            string basePath = Path.GetFileNameWithoutExtension(outputFilename);
            if (!string.IsNullOrEmpty(outputDirectory))
                basePath = Path.Combine(outputDirectory, basePath);

            switch (mediaType)
            {
                case MediaType.CDROM:
                case MediaType.GDROM:
                    List<OutputFile> cdrom = [
                        // .asus is obsolete: newer redumper produces .cache instead
                        new($"{outputFilename}.asus", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "asus"),
                        new($"{outputFilename}.atip", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "atip"),
                        new($"{outputFilename}.cache", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "cache"),
                        new($"{outputFilename}.cdtext", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "cdtext"),
                        new($"{outputFilename}.cue", OutputFileFlags.Required),
                        new($"{outputFilename}.flip", OutputFileFlags.None),
                        new($"{outputFilename}.fulltoc", OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "fulltoc"),
                        new($"{outputFilename}.log", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "log"),
                        new CustomOutputFile([$"{outputFilename}.dat", $"{outputFilename}.log"], OutputFileFlags.Required,
                            DatfileExists),
                        new($"{outputFilename}.pma", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "pma"),
                        new([$"{outputFilename}.scram", $"{outputFilename}.scrap"], OutputFileFlags.Required
                            | OutputFileFlags.Deleteable),
                        new($"{outputFilename}.state", OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "state"),
                        new($"{outputFilename}.subcode", OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "subcode"),
                        new($"{outputFilename}.toc", OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "toc"),
                    ];

                    // Include .hash and .skeleton for all files in cuesheet
                    try
                    {
                        // Read the entire cuesheet
                        string[] cueLines = File.ReadAllLines($"{basePath}.cue");

                        // Track number, assuming 1-based
                        uint trackNumber = 1;
                        foreach (string cueLine in cueLines)
                        {
                            // Skip all non-FILE lines
                            if (!cueLine.StartsWith("FILE"))
                                continue;

                            // Extract the information
                            var match = Regex.Match(cueLine, @"FILE ""(.*?)"" BINARY");
                            if (!match.Success || match.Groups.Count == 0)
                                continue;

                            // Get the track name from the matches
                            string trackName = match.Groups[1].Value;
                            trackName = Path.GetFileNameWithoutExtension(trackName);

                            // Add the artifacts
                            cdrom.Add(new($"{trackName}.hash", OutputFileFlags.Binary
                                | OutputFileFlags.Zippable,
                                $"hash_{trackNumber}"));
                            cdrom.Add(new($"{trackName}.skeleton", OutputFileFlags.Binary
                                | OutputFileFlags.Zippable,
                                $"skeleton_{trackNumber}"));
                            trackNumber++;
                        }
                    }
                    catch
                    {
                        cdrom.Add(new($"{outputFilename}.hash", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "hash"));
                        cdrom.Add(new($"{outputFilename}.skeleton", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "skeleton"));
                    }

                    return cdrom;

                case MediaType.DVD:
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    return [
                        // .asus is obsolete: newer redumper produces .cache instead
                        new($"{outputFilename}.asus", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "asus"),
                        new($"{outputFilename}.cache", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "cache"),
                        new($"{outputFilename}.dmi", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "dmi"),
                        new($"{outputFilename}.hash", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "hash"),
                        new($"{outputFilename}.log", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "log"),
                        new CustomOutputFile([$"{outputFilename}.dat", $"{outputFilename}.log"], OutputFileFlags.Required,
                            DatfileExists),
                        new([$"{outputFilename}.manufacturer", $"{outputFilename}.0.manufacturer"], OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "manufacturer_0"),
                        new($"{outputFilename}.1.manufacturer", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "manufacturer_1"),
                        new($"{outputFilename}.pfi", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "pfi"),
                        new([$"{outputFilename}.physical", $"{outputFilename}.0.physical"], OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "physical_0"),
                        new($"{outputFilename}.1.physical", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "physical_1"),
                        new($"{outputFilename}.security", System.IsXGD() && !IsManufacturerEmpty($"{basePath}.manufacturer")
                            ? OutputFileFlags.Required | OutputFileFlags.Binary | OutputFileFlags.Zippable
                            : OutputFileFlags.Binary | OutputFileFlags.Zippable,
                            "security"),
                        new($"{outputFilename}.skeleton", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "skeleton"),
                        new($"{outputFilename}.ss", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "ss"),
                        new($"{outputFilename}.ssv1", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "ssv1"),
                        new($"{outputFilename}.ssv2", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "ssv2"),
                        new($"{outputFilename}.state", OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "state"),
                    ];

                case MediaType.HDDVD: // TODO: Confirm that this information outputs
                case MediaType.BluRay:
                case MediaType.NintendoWiiUOpticalDisc:
                    return [
                        new($"{outputFilename}.asus", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "asus"),
                        new($"{outputFilename}.hash", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "hash"),
                        new($"{outputFilename}.log", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "log"),
                        new CustomOutputFile([$"{outputFilename}.dat", $"{outputFilename}.log"], OutputFileFlags.Required,
                            DatfileExists),
                        new([$"{outputFilename}.physical", $"{outputFilename}.0.physical"], OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "physical_0"),
                        new($"{outputFilename}.1.physical", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "physical_1"),
                        new($"{outputFilename}.2.physical", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "physical_2"),
                        new($"{outputFilename}.3.physical", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "physical_3"),
                        new($"{outputFilename}.skeleton", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "skeleton"),
                        new($"{outputFilename}.state", OutputFileFlags.Required
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
        {
            // Uncompressed outputs
            if (GetDatfile(log) != null)
                return true;

            // Check for the log file
            string outputFilename = Path.GetFileName(log);
            string? outputDirectory = Path.GetDirectoryName(log);
            string basePath = Path.GetFileNameWithoutExtension(outputFilename);
            if (!string.IsNullOrEmpty(outputDirectory))
                basePath = Path.Combine(outputDirectory, basePath);

#if NET20 || NET35 || NET40
            // Assume the zipfile has the file in it
            return File.Exists($"{basePath}_logs.zip");
#else
            // If the zipfile doesn't exist
            if (!File.Exists($"{basePath}_logs.zip"))
                return false;

            try
            {
                // Try to open the archive
                using ZipArchive archive = ZipFile.OpenRead($"{basePath}_logs.zip");

                // Get the log entry and check it, if possible
                var entry = archive.GetEntry(outputFilename);
                if (entry == null)
                    return false;

                using var sr = new StreamReader(entry.Open());
                return GetDatfile(sr) != null;
            }
            catch
            {
                return false;
            }
#endif
        }

        /// <summary>
        /// Copies a file with the header removed and filesize trimmed
        /// </summary>
        /// <param name="inputFilename">Filename of file to copy from</param>
        /// <param name="outputFilename">Filename of file to copy to</param>
        /// <param name="headerLength">Length of header to remove</param>
        /// <param name="headerLength">Length of file to trim to</param>
        private static bool RemoveHeaderAndTrim(string inputFilename, string outputFilename, int headerLength = 4, int trimLength = 2048)
        {
            // If the file doesn't exist, we can't copy
            if (!File.Exists(inputFilename))
                return false;

            // If the output file already exists, don't overwrite
            if (File.Exists(outputFilename))
                return false;

            try
            {
                using var inputStream = new FileStream(inputFilename, FileMode.Open, FileAccess.Read);

                // If the header length is not valid, don't copy
                if (headerLength < 1 || headerLength >= inputStream.Length)
                    return false;

                using var outputStream = new FileStream(outputFilename, FileMode.Create, FileAccess.Write);

                // Skip the header
                inputStream.Seek(headerLength, SeekOrigin.Begin);

                byte[] buffer = new byte[trimLength];
                int count = inputStream.Read(buffer, 0, buffer.Length);
                outputStream.Write(buffer, 0, count);

                return true;
            }
            catch
            {
                // We don't care what the exception is right now
                return false;
            }
        }

        /// <summary>
        /// Checks whether a .manufacturer file is empty or not
        /// True if standard DVD (empty DMI), False if error or XGD with security sectors 
        /// </summary>
        /// <param name="inputFilename">Filename of .manufacturer file to check</param>
        private static bool IsManufacturerEmpty(string inputFilename)
        {
            // If the file doesn't exist, we can't copy
            if (!File.Exists(inputFilename))
                return false;

            try
            {
                using var inputStream = new FileStream(inputFilename, FileMode.Open, FileAccess.Read);

                // If the manufacturer file is not the correct size, return false
                if (inputStream.Length != 2052)
                    return false;

                byte[] buffer = new byte[2052];
                int bytesRead = inputStream.Read(buffer, 0, buffer.Length);

                // Return false if any value is non-zero, skip SCSI header (4 bytes)
                for (int i = 4; i < bytesRead; i++)
                {
                    if (buffer[i] != 0x00)
                        return false;
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

        #region Information Extraction Methods

        /// <summary>
        /// Get the cuesheet from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Newline-delimited cuesheet if possible, null on error</returns>
        internal static string? GetCuesheet(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
            if (!File.Exists(log))
                return null;

            try
            {
                // Fast forward to the cuesheet line
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream && sr.ReadLine()?.TrimStart()?.StartsWith("CUE [") == false) ;
                if (sr.EndOfStream)
                    return null;

                // Now that we're at the relevant entries, read each line in and concatenate
                var sb = new StringBuilder();
                string? line = sr.ReadLine()?.Trim();
                while (!string.IsNullOrEmpty(line))
                {
                    // TODO: Figure out how to use NormalizeShiftJIS here
                    sb.AppendLine(line);
                    line = sr.ReadLine()?.Trim();
                }

                return sb.ToString().TrimEnd('\n');
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
        internal static string? GetDatfile(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            try
            {
                using var sr = File.OpenText(log);
                return GetDatfile(sr);
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
        /// <param name="sr">StreamReader representing the input file</param>
        /// <returns>Newline-delimited datfile if possible, null on error</returns>
        internal static string? GetDatfile(StreamReader sr)
        {
            try
            {
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
        /// Get reported disc profile information, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>True if disc profile information was set, false otherwise</returns>
        internal static bool GetDiscProfile(string log, out string? discProfile)
        {
            // Set the default values
            discProfile = null;

            // If the file doesn't exist, we can't get the info
            if (string.IsNullOrEmpty(log))
                return false;
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
                        discProfile = line.Substring("current profile: ".Length);
                    }

                    line = sr.ReadLine();
                }

                return true;
            }
            catch
            {
                // We don't care what the exception is right now
                discProfile = null;
                return false;
            }
        }

        /// <summary>
        /// Get reported disc type, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>True if disc type was set, false otherwise</returns>
        internal static bool GetDiscType(string log, out MediaType? discType)
        {
            // Set the default values
            discType = null;

            // If the file doesn't exist, we can't get the info
            if (string.IsNullOrEmpty(log))
                return false;
            if (!File.Exists(log))
                return false;

            try
            {
                using var sr = File.OpenText(log);
                var line = sr.ReadLine()?.TrimEnd();
                while (line != null)
                {
                    // If the line isn't the non-embedded disc type, skip
                    if (!line.StartsWith("disc type:"))
                    {
                        line = sr.ReadLine()?.TrimEnd();
                        continue;
                    }

                    // disc type: <discType>
                    string discTypeStr = line.Substring("disc type: ".Length);

                    // Set the media type based on the string
                    discType = discTypeStr switch
                    {
                        "CD" => MediaType.CDROM,
                        "DVD" => MediaType.DVD,
                        "BLURAY" => MediaType.BluRay,
                        "BLURAY-R" => MediaType.BluRay,
                        "HD-DVD" => MediaType.HDDVD,
                        _ => null,
                    };
                    return discType != null;
                }

                return false;
            }
            catch
            {
                // We don't care what the exception is right now
                discType = null;
                return false;
            }
        }

        /// <summary>
        /// Convert the profile to a media type, if possible
        /// </summary>
        /// <param name="profile">Profile string to check</param>
        /// <returns>Media type on success, null otherwise</returns>
        internal static MediaType? GetDiscTypeFromProfile(string? profile)
        {
            return profile switch
            {
                "reserved" => null,
                "non removable disk" => null,
                "removable disk" => null,
                "MO erasable" => null,
                "MO write once" => null,
                "AS MO" => null,

                "CD-ROM" => MediaType.CDROM,
                "CD-R" => MediaType.CDROM,
                "CD-RW" => MediaType.CDROM,

                "DVD-ROM" => MediaType.DVD,
                "DVD-R" => MediaType.DVD,
                "DVD-RAM" => MediaType.DVD,
                "DVD-RW RO" => MediaType.DVD,
                "DVD-RW" => MediaType.DVD,
                "DVD-R DL" => MediaType.DVD,
                "DVD-R DL LJR" => MediaType.DVD,
                "DVD+RW" => MediaType.DVD,
                "DVD+R" => MediaType.DVD,

                "DDCD-ROM" => MediaType.CDROM,
                "DDCD-R" => MediaType.CDROM,
                "DDCD-RW" => MediaType.CDROM,

                "DVD+RW DL" => MediaType.DVD,
                "DVD+R DL" => MediaType.DVD,

                "BD-ROM" => MediaType.BluRay,
                "BD-R" => MediaType.BluRay,
                "BD-R RRM" => MediaType.BluRay,
                "BD-RW" => MediaType.BluRay,

                "HD DVD-ROM" => MediaType.HDDVD,
                "HD DVD-R" => MediaType.HDDVD,
                "HD DVD-RAM" => MediaType.HDDVD,
                "HD DVD-RW" => MediaType.HDDVD,
                "HD DVD-R DL" => MediaType.HDDVD,
                "HD DVD-RW DL" => MediaType.HDDVD,

                "NON STANDARD" => null,

                _ => null,
            };
        }

        /// <summary>
        /// Get the DVD protection information, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <param name="includeAlways">Indicates whether region and protection type are always included</param>
        /// <returns>Formatted string representing the DVD protection, null on error</returns>
        internal static string? GetDVDProtection(string log, bool includeAlways)
        {
            // If one of the files doesn't exist, we can't get info from them
            if (string.IsNullOrEmpty(log))
                return null;
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

                                    vobKeys += $"{match.Groups[1].Value} Title Key: {normalizedKey}\n";
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
        internal static bool GetErrorCount(string log, out long redumpErrors, out long c2Errors)
        {
            redumpErrors = -1; c2Errors = -1;

            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return false;
            if (!File.Exists(log))
                return false;

            try
            {
                redumpErrors = 0; c2Errors = 0;
                using var sr = File.OpenText(log);

                // Find the error counts
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine()?.Trim();
                    if (line == null)
                        break;

                    // C2: <error count>
                    // C2: <error count> samples
                    if (line.StartsWith("C2:"))
                    {
                        // Ensure there are the correct number of parts
                        string[] parts = line.Split(' ');
                        if (parts.Length < 2)
                        {
                            c2Errors = -1;
                            break;
                        }

                        // If there is a parsing error, return
                        if (!long.TryParse(parts[1], out long c2TrackErrors))
                        {
                            c2Errors = -1;
                            break;
                        }

                        // Standard error counts always add sectors
                        if (parts.Length == 2)
                        {
                            c2Errors += c2TrackErrors;
                        }
                        // Correction counts are ignored for now
                        else if (parts.Length > 2)
                        {
                            // No-op
                        }
                    }

                    // REDUMP.ORG errors: <error count>
                    else if (line.StartsWith("REDUMP.ORG errors:"))
                    {
                        // Ensure there are the correct number of parts
                        string[] parts = line!.Split(' ');
                        if (parts.Length < 3)
                        {
                            redumpErrors = -1;
                            break;
                        }

                        // If there is a parsing error, return
                        if (!long.TryParse(parts[2], out long redumpTrackErrors))
                        {
                            redumpErrors = -1;
                            break;
                        }

                        // Always add Redump errors
                        redumpErrors += redumpTrackErrors;
                    }

                    // Reset C2 errors when a media errors section is found
                    else if (line.StartsWith("media errors:"))
                    {
                        c2Errors = 0;
                    }

                    // Reset Redump errors when an INFO block is found
                    else if (line.StartsWith("*** INFO"))
                    {
                        redumpErrors = 0;
                    }
                }

                // If either error count is -1, then an issue occurred
                return c2Errors != -1 && redumpErrors != -1;
            }
            catch
            {
                // We don't care what the exception is right now
                redumpErrors = -1; c2Errors = -1;
                return false;
            }
        }

        /// <summary>
        /// Get the header from a GD-ROM LD area, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Header as a string if possible, null on error</returns>
        internal static string? GetGDROMHeader(string log, out string? buildDate, out string? serial, out string? region, out string? version)
        {
            // Set the default values
            buildDate = null; serial = null; region = null; version = null;

            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
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
                    else if (line.StartsWith("version:"))
                    {
                        version = line.Substring("version: ".Length).Trim();
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
                        while (line?.StartsWith("00") == true)
                        {
                            headerString += line + "\n";
                            line = sr.ReadLine()?.TrimStart();
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
        internal static bool GetHardwareInfo(string log, out string? manufacturer, out string? model, out string? firmware)
        {
            // Set the default values
            manufacturer = null; model = null; firmware = null;

            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return false;
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
        internal static bool GetLayerbreaks(string log, out string? layerbreak1, out string? layerbreak2, out string? layerbreak3)
        {
            // Set the default values
            layerbreak1 = null; layerbreak2 = null; layerbreak3 = null;

            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return false;
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
                        string[] split = Array.FindAll(line.Split(' '), s => !string.IsNullOrEmpty(s));
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
        internal static string? GetMultisessionInformation(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
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
        internal static bool? GetPlayStationAntiModchipDetected(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
            if (!File.Exists(log))
                return null;

            try
            {
                // Check for the anti-modchip strings
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine()?.Trim();

                    // If we have a null line, just break
                    if (line == null)
                        break;

                    if (line.StartsWith("anti-modchip: no"))
                        return false;
                    else if (line.StartsWith("anti-modchip: yes"))
                        return true;
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
        internal static bool? GetPlayStationEDCStatus(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
            if (!File.Exists(log))
                return null;

            try
            {
                // Check for the EDC strings
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine()?.Trim();

                    // If we have a null line, just break
                    if (line == null)
                        break;

                    if (line.Contains("EDC: no"))
                        return false;
                    else if (line.Contains("EDC: yes"))
                        return true;
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
        internal static bool GetPlayStationInfo(string log, out string? exeDate, out string? serial, out string? version)
        {
            // Set the default values
            exeDate = null; serial = null; version = null;

            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return false;
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
                    else if (line.StartsWith("libcrypt:") || line.StartsWith("MSF:"))
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
        internal static string? GetPlayStationLibCryptData(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
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
        internal static bool? GetPlayStationLibCryptStatus(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
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
        /// Get PlayStation 2 protection info from the log file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>PlayStation 2 protection if possible, null otherwise</returns>
        internal static string? GetPlayStation2Protection(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
            if (!File.Exists(log))
                return null;

            try
            {
                // Check for the protection strings
                using var sr = File.OpenText(log);
                var line = sr.ReadLine()?.Trim();
                while (!sr.EndOfStream)
                {
                    if (line == null)
                        return null;

                    if (line.StartsWith("protection: PS2"))
                        return line.Substring("protection: ".Length).Trim();

                    line = sr.ReadLine()?.Trim();
                }

                return null;
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
        internal static string? GetPVD(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
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
                string? pvdString = string.Empty, line = sr.ReadLine();
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
        internal static string? GetRingNonZeroDataStart(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
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
        /// Get the perfect audio offset from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Non-zero dta start if possible, null on error</returns>
        internal static string? GetRingPerfectAudioOffset(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
            if (!File.Exists(log))
                return null;

            try
            {
                // If we have a perfect audio offset, return
                bool perfectAudioOffsetApplied = false;
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream)
                {
                    string? line = sr.ReadLine()?.TrimStart();
                    if (line?.StartsWith("Perfect Audio Offset applied") == true)
                    {
                        perfectAudioOffsetApplied = true;
                    }
                    else if (line?.StartsWith("disc write offset: +0") == true)
                    {
                        if (perfectAudioOffsetApplied)
                            return "+0";
                        else
                            return null;
                    }
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
        internal static bool GetSaturnBuildInfo(string? segaHeader, out string? buildDate, out string? serial, out string? version)
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
        internal static string? GetSaturnHeader(string log, out string? buildDate, out string? serial, out string? region, out string? version)
        {
            // Set the default values
            buildDate = null; serial = null; region = null; version = null;

            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
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
                    else if (line.StartsWith("version:"))
                    {
                        version = line.Substring("version: ".Length).Trim();
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
                    else if (line?.StartsWith("header:") == true)
                    {
                        line = sr.ReadLine()?.TrimStart();
                        while (line?.StartsWith("00") == true)
                        {
                            headerString += line + "\n";
                            line = sr.ReadLine()?.TrimStart();
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
        /// Get the SCSI error count from the input files, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>SCSI error count on success, -1 on error</returns>
        /// TODO: Remove when Redumper adds this to normal errors
        internal static long GetSCSIErrorCount(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return -1;
            if (!File.Exists(log))
                return -1;

            try
            {
                long errorCount = 0;
                using var sr = File.OpenText(log);

                // Find the error counts
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine()?.Trim();
                    if (line == null)
                        break;

                    // SCSI: <error count>
                    // SCSI: <error count> samples
                    if (line.StartsWith("SCSI: "))
                    {
                        // Ensure there are the correct number of parts
                        string[] parts = line.Split(' ');
                        if (parts.Length < 2)
                        {
                            errorCount = -1;
                            break;
                        }

                        // If there is a parsing error, return
                        if (!long.TryParse(parts[1], out long scsiErrors))
                        {
                            errorCount = -1;
                            break;
                        }

                        // Standard error counts always add sectors
                        if (parts.Length == 2)
                        {
                            errorCount += scsiErrors;
                        }
                        // Correction counts are ignored for now
                        else if (parts.Length > 2)
                        {
                            // No-op
                        }
                    }

                    // Reset SCSI errors when a media errors section is found
                    else if (line.StartsWith("media errors:"))
                    {
                        errorCount = 0;
                    }
                }

                // Return error count, default -1 if no SCSI error count found
                return errorCount;
            }
            catch
            {
                // We don't care what the exception is right now
                return -1;
            }
        }

        /// <summary>
        /// Get the SecuROM data from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <param name="secuROMScheme">SecuROM scheme found</param>
        /// <returns>SecuROM data, if possible</returns>
        internal static string? GetSecuROMData(string log, out SecuROMScheme secuROMScheme)
        {
            secuROMScheme = SecuROMScheme.None;

            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
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

                // Return the securom scheme if correct sector count
                secuROMScheme = lines.Count switch
                {
                    0 => SecuROMScheme.None,
                    216 => SecuROMScheme.PreV3,
                    90 => SecuROMScheme.V3,
                    99 => SecuROMScheme.V4,
                    11 => SecuROMScheme.V4Plus,
                    _ => SecuROMScheme.Unknown,
                };

                return string.Join("\n", [.. lines]).TrimEnd('\n');
            }
            catch
            {
                // We don't care what the exception is right now
                secuROMScheme = SecuROMScheme.None;
                return null;
            }
        }

        /// <summary>
        /// Get the header from a Sega CD / Mega CD, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Header as a byte array if possible, null on error</returns>
        internal static string? GetSegaCDHeader(string log, out string? buildDate, out string? serial, out string? region)
        {
            // Set the default values
            buildDate = null; serial = null; region = null;

            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
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
        internal static string? GetUniversalHash(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
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
        internal static string? GetVersion(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
            if (!File.Exists(log))
                return null;

            // Samples:
            // redumper v2022.10.28 [Oct 28 2022, 05:41:43] (print usage: --help,-h)
            // redumper v2022.12.22 build_87 [Dec 22 2022, 01:56:26]
            // redumper v2025.03.29 build_481

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
                var regex = new Regex(@"^redumper (v.+)", RegexOptions.Compiled);
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
        /// Get the dumping parameters, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Dumping parameters if possible, null on error</returns>
        internal static string? GetParameters(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
            if (!File.Exists(log))
                return null;

            // Samples:
            // arguments: cd --verbose --drive=F:\ --speed=24 --retries=200
            // arguments: bd --image-path=ISO\PS3_VOLUME --image-name=PS3_VOLUME

            try
            {
                // If we find the arguments line, return the arguments
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream)
                {
                    string? line = sr.ReadLine()?.TrimStart();
                    if (line?.StartsWith("arguments: ") == true)
                        return line.Substring("arguments: ".Length).Trim();
                }

                // We couldn't detect any arguments
                return null;
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
        internal static bool GetVolumeLabels(string log, out Dictionary<string, List<string>> volLabels)
        {
            // If the file doesn't exist, can't get the volume labels
            volLabels = [];
            if (string.IsNullOrEmpty(log))
                return false;
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
        internal static string? GetWriteOffset(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
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
