using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using psxt001z;
using SabreTools.Models.Logiqx;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;

/*
If there are no external programs, such as error checking, etc., DIC outputs
a slightly different set of files. This reduced set needs to be documented in
order for special use cases, such as self-built versions of DIC or removed
helper programs, can be detected to the best of our ability. Below is the list
of files that are generated in that case:

    .bin
    .c2
    .ccd
    .cue
    .img/.imgtmp
    .scm/.scmtmp
    .sub/.subtmp
    _cmd.txt (formerly)
    _img.cue

This list needs to be translated into the minimum viable set of information
such that things like error checking can be passed back as a flag, or some
similar method.

Here are some notes about the various output files and what they represent:
- bin           - Final split output disc image (CD/GD only)
- c2            - Represents each byte per sector as one bit; 0 means no error, 1 means error
- c2Error       - Human-readable version of `c2`; only errors are printed
- ccd           - CloneCD control file referencing the `img` file
- cmd           - Represents the commandline that was run
- cue           - CDRWIN cuesheet referencing the `bin` file(s)
- dat           - Logiqx datfile referencing the `bin` file(s)
- disc          - Disc metadata and information
- drive         - Drive metadata and information
- img           - CloneCD output disc image (CD/GD only)
- img.cue       - CDRWIN cuesheet referencing the `img` file
- img_EdcEcc    - ECC check output as run on the `img` file
- iso           - Final output disc image (DVD/BD only)
- mainError     - Read, drive, or system errors
- mainInfo      - ISOBuster-formatted sector information
- scm           - Scrambled disc image
- sub           - Binary subchannel data as read from the disc
- subError      - Subchannel read errors
- subInfo       - Subchannel informational messages
- subIntention  - Subchannel intentional error information
- subReadable   - Human-readable version of `sub`
- toc           - Binary representation of the table of contents
- volDesc       - Volume descriptor information
*/
namespace MPF.Processors
{
    /// <summary>
    /// Represents processing DiscImageCreator outputs
    /// </summary>
    public sealed class DiscImageCreator : BaseProcessor
    {
        /// <inheritdoc/>
        public DiscImageCreator(RedumpSystem? system, MediaType? type) : base(system, type) { }

        #region BaseProcessor Implementations

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, string basePath, bool redumpCompat)
        {
            var outputDirectory = Path.GetDirectoryName(basePath);

            // Ensure that required sections exist
            info = Builder.EnsureAllSections(info);

            // Get the dumping program and version
            var dicVersion = GetCommandFilePathAndVersion(basePath, out var dicCmd);
            info.DumpingInfo!.DumpingProgram ??= string.Empty;
            info.DumpingInfo.DumpingProgram += $" {dicVersion ?? "Unknown Version"}";
            info.DumpingInfo.DumpingDate = ProcessingTool.GetFileModifiedDate(dicCmd)?.ToString("yyyy-MM-dd HH:mm:ss");

            // Fill in the hardware data
            if (GetHardwareInfo($"{basePath}_drive.txt", out var manufacturer, out var model, out var firmware))
            {
                info.DumpingInfo.Manufacturer = manufacturer;
                info.DumpingInfo.Model = model;
                info.DumpingInfo.Firmware = firmware;
            }

            // Fill in the disc type data
            if (GetDiscType($"{basePath}_disc.txt", out var discTypeOrBookType))
                info.DumpingInfo.ReportedDiscType = discTypeOrBookType;

            // Get the Datafile information
            var datafile = ProcessingTool.GetDatafile($"{basePath}.dat");

            // Fill in the hash data
            info.TracksAndWriteOffsets!.ClrMameProData = ProcessingTool.GenerateDatfile(datafile);

            // Fill in the volume labels
            if (GetVolumeLabels($"{basePath}_volDesc.txt", out var volLabels))
                VolumeLabels = volLabels;

            // Extract info based generically on MediaType
            switch (Type)
            {
                case MediaType.CDROM:
                case MediaType.GDROM: // TODO: Verify GD-ROM outputs this
                    info.Extras!.PVD = GetPVD($"{basePath}_mainInfo.txt") ?? "Disc has no PVD";

                    // Audio-only discs will fail if there are any C2 errors, so they would never get here
                    if (System.IsAudio())
                    {
                        info.CommonDiscInfo!.ErrorsCount = "0";
                    }
                    else
                    {
                        long errorCount = -1;
                        if (File.Exists($"{basePath}.img_EdcEcc.txt"))
                            errorCount = GetErrorCount($"{basePath}.img_EdcEcc.txt");
                        else if (File.Exists($"{basePath}.img_EccEdc.txt"))
                            errorCount = GetErrorCount($"{basePath}.img_EccEdc.txt");

                        info.CommonDiscInfo!.ErrorsCount = (errorCount == -1 ? "Error retrieving error count" : errorCount.ToString());
                    }

                    info.TracksAndWriteOffsets.Cuesheet = ProcessingTool.GetFullFile($"{basePath}.cue") ?? string.Empty;
                    //var cueSheet = new CueSheet($"{basePath}.cue"); // TODO: Do something with this

                    // Attempt to get the write offset
                    string cdWriteOffset = GetWriteOffset($"{basePath}_disc.txt") ?? string.Empty;
                    info.CommonDiscInfo.RingWriteOffset = cdWriteOffset;
                    info.TracksAndWriteOffsets.OtherWriteOffsets = cdWriteOffset;

                    // Attempt to get multisession data
                    string cdMultiSessionInfo = GetMultisessionInformation($"{basePath}_disc.txt") ?? string.Empty;
                    if (!string.IsNullOrEmpty(cdMultiSessionInfo))
                        info.CommonDiscInfo.CommentsSpecialFields![SiteCode.Multisession] = cdMultiSessionInfo;

                    break;

                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:

                    // Get the individual hash data, as per internal
                    if (ProcessingTool.GetISOHashValues(datafile, out long size, out var crc32, out var md5, out var sha1))
                    {
                        info.SizeAndChecksums!.Size = size;
                        info.SizeAndChecksums.CRC32 = crc32;
                        info.SizeAndChecksums.MD5 = md5;
                        info.SizeAndChecksums.SHA1 = sha1;
                    }

                    // Deal with the layerbreaks
                    if (Type == MediaType.DVD)
                    {
                        string layerbreak = GetLayerbreak($"{basePath}_disc.txt", System.IsXGD()) ?? string.Empty;
                        info.SizeAndChecksums!.Layerbreak = !string.IsNullOrEmpty(layerbreak) ? Int64.Parse(layerbreak) : default;
                    }
                    else if (Type == MediaType.BluRay)
                    {
                        var di = ProcessingTool.GetDiscInformation($"{basePath}_PIC.bin");
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
                    }

                    // Read the PVD
                    if (!redumpCompat || System != RedumpSystem.MicrosoftXbox)
                        info.Extras!.PVD = GetPVD($"{basePath}_mainInfo.txt") ?? string.Empty;

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

                        info.Extras!.PIC = GetPIC($"{basePath}_PIC.bin", trimLength) ?? string.Empty;
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
                    if (File.Exists($"{basePath}_subIntention.txt"))
                    {
                        var fi = new FileInfo($"{basePath}_subIntention.txt");
                        if (fi.Length > 0)
                            info.CopyProtection!.SecuROMData = ProcessingTool.GetFullFile($"{basePath}_subIntention.txt") ?? string.Empty;
                    }

                    // Needed for some odd copy protections
                    info.CopyProtection!.Protection = GetDVDProtection($"{basePath}_CSSKey.txt", $"{basePath}_disc.txt", false) ?? string.Empty;

                    break;

                case RedumpSystem.DVDAudio:
                case RedumpSystem.DVDVideo:
                    info.CopyProtection!.Protection = GetDVDProtection($"{basePath}_CSSKey.txt", $"{basePath}_disc.txt", true) ?? string.Empty;
                    break;

                case RedumpSystem.MicrosoftXbox:
                    string xmidString = ProcessingTool.GetXMID($"{basePath}_DMI.bin");
                    var xmid = SabreTools.Serialization.Wrappers.XMID.Create(xmidString);
                    if (xmid != null)
                    {
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.XMID] = xmidString?.TrimEnd('\0') ?? string.Empty;
                        info.CommonDiscInfo.Serial = xmid.Serial ?? string.Empty;
                        if (!redumpCompat)
                            info.VersionAndEditions!.Version = xmid.Version ?? string.Empty;

                        info.CommonDiscInfo.Region = ProcessingTool.GetXGDRegion(xmid.Model.RegionIdentifier);
                    }

                    // If we have the new, external DAT
                    if (File.Exists($"{basePath}_suppl.dat"))
                    {
                        var suppl = ProcessingTool.GetDatafile($"{basePath}_suppl.dat");
                        if (GetXGDAuxHashInfo(suppl, out var xgd1DMIHash, out var xgd1PFIHash, out var xgd1SSHash))
                        {
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.DMIHash] = xgd1DMIHash ?? string.Empty;
                            info.CommonDiscInfo.CommentsSpecialFields[SiteCode.PFIHash] = xgd1PFIHash ?? string.Empty;
                            info.CommonDiscInfo.CommentsSpecialFields[SiteCode.SSHash] = xgd1SSHash ?? string.Empty;
                        }

                        if (GetXGDAuxSSInfo($"{basePath}_disc.txt", out var xgd1SS, out var xgd1SSVer))
                        {
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.SSVersion] = xgd1SSVer ?? string.Empty;
                            info.Extras!.SecuritySectorRanges = xgd1SS ?? string.Empty;
                        }
                    }
                    else
                    {
                        if (GetXGDAuxInfo($"{basePath}_disc.txt", out var xgd1DMIHash, out var xgd1PFIHash, out var xgd1SSHash, out var xgd1SS, out var xgd1SSVer))
                        {
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.DMIHash] = xgd1DMIHash ?? string.Empty;
                            info.CommonDiscInfo.CommentsSpecialFields[SiteCode.PFIHash] = xgd1PFIHash ?? string.Empty;
                            info.CommonDiscInfo.CommentsSpecialFields[SiteCode.SSHash] = xgd1SSHash ?? string.Empty;
                            info.CommonDiscInfo.CommentsSpecialFields[SiteCode.SSVersion] = xgd1SSVer ?? string.Empty;
                            info.Extras!.SecuritySectorRanges = xgd1SS ?? string.Empty;
                        }
                    }

                    break;

                case RedumpSystem.MicrosoftXbox360:
                    string xemidString = ProcessingTool.GetXeMID($"{basePath}_DMI.bin");
                    var xemid = SabreTools.Serialization.Wrappers.XeMID.Create(xemidString);
                    if (xemid != null)
                    {
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.XeMID] = xemidString?.TrimEnd('\0') ?? string.Empty;
                        info.CommonDiscInfo.Serial = xemid.Serial ?? string.Empty;
                        if (!redumpCompat)
                            info.VersionAndEditions!.Version = xemid.Version ?? string.Empty;

                        info.CommonDiscInfo.Region = ProcessingTool.GetXGDRegion(xemid.Model.RegionIdentifier);
                    }

                    // If we have the new, external DAT
                    if (File.Exists($"{basePath}_suppl.dat"))
                    {
                        var suppl = ProcessingTool.GetDatafile($"{basePath}_suppl.dat");
                        if (GetXGDAuxHashInfo(suppl, out var xgd23DMIHash, out var xgd23PFIHash, out var xgd23SSHash))
                        {
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.DMIHash] = xgd23DMIHash ?? string.Empty;
                            info.CommonDiscInfo.CommentsSpecialFields[SiteCode.PFIHash] = xgd23PFIHash ?? string.Empty;
                            info.CommonDiscInfo.CommentsSpecialFields[SiteCode.SSHash] = xgd23SSHash ?? string.Empty;
                        }

                        if (GetXGDAuxSSInfo($"{basePath}_disc.txt", out var xgd23SS, out var xgd23SSVer))
                        {
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.SSVersion] = xgd23SSVer ?? string.Empty;
                            info.Extras!.SecuritySectorRanges = xgd23SS ?? string.Empty;
                        }
                    }
                    else
                    {
                        if (GetXGDAuxInfo($"{basePath}_disc.txt", out var xgd23DMIHash, out var xgd23PFIHash, out var xgd23SSHash, out var xgd23SS, out var xgd23SSVer))
                        {
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.DMIHash] = xgd23DMIHash ?? string.Empty;
                            info.CommonDiscInfo.CommentsSpecialFields[SiteCode.PFIHash] = xgd23PFIHash ?? string.Empty;
                            info.CommonDiscInfo.CommentsSpecialFields[SiteCode.SSHash] = xgd23SSHash ?? string.Empty;
                            info.CommonDiscInfo.CommentsSpecialFields[SiteCode.SSVersion] = xgd23SSVer ?? string.Empty;
                            info.Extras!.SecuritySectorRanges = xgd23SS ?? string.Empty;
                        }
                    }

                    break;

                case RedumpSystem.NamcoSegaNintendoTriforce:
                    if (Type == MediaType.CDROM)
                    {
                        info.Extras!.Header = GetSegaHeader($"{basePath}_mainInfo.txt") ?? string.Empty;

                        // Take only the first 16 lines for GD-ROM
                        if (!string.IsNullOrEmpty(info.Extras.Header))
                            info.Extras.Header = string.Join("\n", info.Extras.Header.Split('\n'), 0, 16);

                        if (GetGDROMBuildInfo(info.Extras.Header,
                            out var serial,
                            out var version,
                            out var date))
                        {
                            // Ensure internal serial is pulled from local data
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = serial ?? string.Empty;
                            info.VersionAndEditions!.Version = version ?? string.Empty;
                            info.CommonDiscInfo.EXEDateBuildDate = date ?? string.Empty;
                        }
                    }

                    break;

                case RedumpSystem.SegaMegaCDSegaCD:
                    info.Extras!.Header = GetSegaHeader($"{basePath}_mainInfo.txt") ?? string.Empty;

                    // Take only the last 16 lines for Sega CD
                    if (!string.IsNullOrEmpty(info.Extras.Header))
                        info.Extras.Header = string.Join("\n", info.Extras.Header.Split('\n'), 0, 16);

                    if (GetSegaCDBuildInfo(info.Extras.Header, out var scdSerial, out var fixedDate))
                    {
                        // Ensure internal serial is pulled from local data
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = scdSerial ?? string.Empty;
                        info.CommonDiscInfo.EXEDateBuildDate = fixedDate ?? string.Empty;
                    }

                    break;

                case RedumpSystem.SegaChihiro:
                    if (Type == MediaType.CDROM)
                    {
                        info.Extras!.Header = GetSegaHeader($"{basePath}_mainInfo.txt") ?? string.Empty;

                        // Take only the first 16 lines for GD-ROM
                        if (!string.IsNullOrEmpty(info.Extras.Header))
                            info.Extras.Header = string.Join("\n", info.Extras.Header.Split('\n'), 0, 16);

                        if (GetGDROMBuildInfo(info.Extras.Header,
                            out var serial,
                            out var version,
                            out var date))
                        {
                            // Ensure internal serial is pulled from local data
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = serial ?? string.Empty;
                            info.VersionAndEditions!.Version = version ?? string.Empty;
                            info.CommonDiscInfo.EXEDateBuildDate = date ?? string.Empty;
                        }
                    }

                    break;

                case RedumpSystem.SegaDreamcast:
                    if (Type == MediaType.CDROM)
                    {
                        info.Extras!.Header = GetSegaHeader($"{basePath}_mainInfo.txt") ?? string.Empty;

                        // Take only the first 16 lines for GD-ROM
                        if (!string.IsNullOrEmpty(info.Extras.Header))
                            info.Extras.Header = string.Join("\n", info.Extras.Header.Split('\n'), 0, 16);

                        if (GetGDROMBuildInfo(info.Extras.Header,
                            out var serial,
                            out var version,
                            out var date))
                        {
                            // Ensure internal serial is pulled from local data
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = serial ?? string.Empty;
                            info.VersionAndEditions!.Version = version ?? string.Empty;
                            info.CommonDiscInfo.EXEDateBuildDate = date ?? string.Empty;
                        }
                    }

                    break;

                case RedumpSystem.SegaNaomi:
                    if (Type == MediaType.CDROM)
                    {
                        info.Extras!.Header = GetSegaHeader($"{basePath}_mainInfo.txt") ?? string.Empty;

                        // Take only the first 16 lines for GD-ROM
                        if (!string.IsNullOrEmpty(info.Extras.Header))
                            info.Extras.Header = string.Join("\n", info.Extras.Header.Split('\n'), 0, 16);

                        if (GetGDROMBuildInfo(info.Extras.Header,
                            out var serial,
                            out var version,
                            out var date))
                        {
                            // Ensure internal serial is pulled from local data
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = serial ?? string.Empty;
                            info.VersionAndEditions!.Version = version ?? string.Empty;
                            info.CommonDiscInfo.EXEDateBuildDate = date ?? string.Empty;
                        }
                    }

                    break;

                case RedumpSystem.SegaNaomi2:
                    if (Type == MediaType.CDROM)
                    {
                        info.Extras!.Header = GetSegaHeader($"{basePath}_mainInfo.txt") ?? string.Empty;

                        // Take only the first 16 lines for GD-ROM
                        if (!string.IsNullOrEmpty(info.Extras.Header))
                            info.Extras.Header = string.Join("\n", info.Extras.Header.Split('\n'), 0, 16);

                        if (GetGDROMBuildInfo(info.Extras.Header,
                            out var serial,
                            out var version,
                            out var date))
                        {
                            // Ensure internal serial is pulled from local data
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = serial ?? string.Empty;
                            info.VersionAndEditions!.Version = version ?? string.Empty;
                            info.CommonDiscInfo.EXEDateBuildDate = date ?? string.Empty;
                        }
                    }

                    break;

                case RedumpSystem.SegaSaturn:
                    info.Extras!.Header = GetSegaHeader($"{basePath}_mainInfo.txt") ?? string.Empty;

                    // Take only the first 16 lines for Saturn
                    if (!string.IsNullOrEmpty(info.Extras.Header))
                        info.Extras.Header = string.Join("\n", info.Extras.Header.Split('\n'), 0, 16);

                    if (GetSaturnBuildInfo(info.Extras.Header, out var saturnSerial, out var saturnVersion, out var buildDate))
                    {
                        // Ensure internal serial is pulled from local data
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = saturnSerial ?? string.Empty;
                        info.VersionAndEditions!.Version = saturnVersion ?? string.Empty;
                        info.CommonDiscInfo.EXEDateBuildDate = buildDate ?? string.Empty;
                    }

                    break;

                case RedumpSystem.SonyPlayStation:
                    bool? psEdcStatus = null;
                    if (File.Exists($"{basePath}.img_EdcEcc.txt"))
                        psEdcStatus = GetPlayStationEDCStatus($"{basePath}.img_EdcEcc.txt");
                    else if (File.Exists($"{basePath}.img_EccEdc.txt"))
                        psEdcStatus = GetPlayStationEDCStatus($"{basePath}.img_EccEdc.txt");

                    info.EDC!.EDC = psEdcStatus.ToYesNo();
                    info.CopyProtection!.AntiModchip = GetPlayStationAntiModchipDetected($"{basePath}_disc.txt").ToYesNo();
                    GetLibCryptDetected(basePath, out YesNo libCryptDetected, out string? libCryptData);
                    info.CopyProtection.LibCrypt = libCryptDetected;
                    info.CopyProtection.LibCryptData = libCryptData;
                    break;

                case RedumpSystem.SonyPlayStation3:
                    if (GetPlayStation3Info($"{basePath}_disc.txt", out string? ps3Serial, out string? ps3Version, out string? ps3FirmwareVersion))
                    {
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = ps3Serial ?? string.Empty;
                        info.VersionAndEditions!.Version = ps3Version ?? string.Empty;
                        if (ps3FirmwareVersion != null)
                            info.CommonDiscInfo!.ContentsSpecialFields![SiteCode.Patches] = $"PS3 Firmware {ps3FirmwareVersion}";
                    }

                    break;
            }
        }

        /// <inheritdoc/>
        internal override List<OutputFile> GetOutputFiles(string? baseDirectory, string baseFilename)
        {
            switch (Type)
            {
                case MediaType.CDROM:
                    return [
                        new($"{baseFilename}.c2", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "c2"), // Doesn't output on Linux
                        new($"{baseFilename}.ccd", OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "ccd"),
                        new($"{baseFilename}.cue", OutputFileFlags.Required),
                        new($"{baseFilename}.dat", OutputFileFlags.Required
                            | OutputFileFlags.Zippable),
                        new($"{baseFilename}.img", OutputFileFlags.Required
                            | OutputFileFlags.Deleteable),
                        new([$"{baseFilename}.img_EdcEcc.txt", $"{baseFilename}.img_EccEdc.txt"], System.IsAudio()
                            ? OutputFileFlags.Artifact | OutputFileFlags.Zippable
                            : OutputFileFlags.Required | OutputFileFlags.Artifact | OutputFileFlags.Zippable,
                            "img_edcecc"),
                        new([$"{baseFilename}.scm", $"{baseFilename}.scmtmp"], System.IsAudio()
                            ? OutputFileFlags.Deleteable
                            : OutputFileFlags.Required | OutputFileFlags.Deleteable),
                        new([$"{baseFilename}.sub", $"{baseFilename}.subtmp"], OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "sub"),
                        new($"{baseFilename}.toc", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "toc"),

                        new($"{baseFilename}_c2Error.txt", OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "c2_error"), // Doesn't output on Linux
                        new RegexOutputFile(Regex.Escape(baseFilename) + @"_(\d{8})T\d{6}\.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "cmd"),
                        new($"{baseFilename}_cmd.txt", OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "cmd"),
                        new($"{baseFilename}_disc.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "disc"),
                        new($"{baseFilename}_drive.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "drive"),
                        new($"{baseFilename}_img.cue", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "img_cue"),
                        new($"{baseFilename}_mainError.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "main_error"),
                        new($"{baseFilename}_mainInfo.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "main_info"),
                        new([$"{baseFilename}_sub.txt", $"{baseFilename}_subReadable.txt"], OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "sub_readable"),
                        new($"{baseFilename}_subError.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "sub_error"),
                        new($"{baseFilename}_subInfo.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "sub_info"),
                        new($"{baseFilename}_subIntention.txt", OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "sub_intention"),
                        new($"{baseFilename}_suppl.dat", OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "suppl_dat"),
                        new($"{baseFilename}_volDesc.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "vol_desc"),

                        new([$"{baseFilename} (Track 0).sub", $"{baseFilename} (Track 00).sub"], OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "track00_sub"),
                        new([$"{baseFilename} (Track 1)(-LBA).sub", $"{baseFilename} (Track 01)(-LBA).sub"], OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "track1_lba_sub"),
                        new([$"{baseFilename} (Track AA).sub", $"{baseFilename} (Lead-out)(Track AA).sub"], OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "track_aa_sub"),
                    ];

                // TODO: Confirm GD-ROM HD area outputs
                case MediaType.GDROM:
                    return [
                        new($"{baseFilename}.dat", OutputFileFlags.Required
                            | OutputFileFlags.Zippable),
                        new($"{baseFilename}.toc", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "toc"),

                        new RegexOutputFile(Regex.Escape(baseFilename) + @"_(\d{8})T\d{6}\.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "cmd"),
                        new($"{baseFilename}_cmd.txt", OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "cmd"),
                        new($"{baseFilename}_disc.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "disc"),
                        new($"{baseFilename}_drive.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
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
                        new($"{baseFilename}_suppl.dat", OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "suppl_dat"),
                        new($"{baseFilename}_volDesc.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "vol_desc"),
                    ];

                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    return [
                        new($"{baseFilename}.dat", OutputFileFlags.Required
                            | OutputFileFlags.Zippable),
                        new($"{baseFilename}.raw", OutputFileFlags.None),
                        new($"{baseFilename}.toc", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "toc"),

                        new RegexOutputFile(Regex.Escape(baseFilename) + @"_(\d{8})T\d{6}\.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "cmd"),
                        new($"{baseFilename}_cmd.txt", OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "cmd"),
                        new($"{baseFilename}_CSSKey.txt", OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "css_key"),
                        new($"{baseFilename}_disc.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "disc"),
                        new($"{baseFilename}_drive.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
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
                        new($"{baseFilename}_suppl.dat", OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "suppl_dat"),
                        new($"{baseFilename}_volDesc.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "vol_desc"),

                        // TODO: Figure out when these are required
                        new($"{baseFilename}_DMI.bin", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "dmi"),
                        new($"{baseFilename}_PFI.bin", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "pfi"),
                        new($"{baseFilename}_PIC.bin", OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "pic"),
                        new($"{baseFilename}_SS.bin", System.IsXGD()
                            ? OutputFileFlags.Required | OutputFileFlags.Binary | OutputFileFlags.Zippable
                            : OutputFileFlags.Binary | OutputFileFlags.Zippable,
                            "ss"),
                    ];

                case MediaType.FloppyDisk:
                case MediaType.HardDisk:
                    // TODO: Determine what outputs come out from a HDD, SD, etc.
                    return [
                        new($"{baseFilename}.dat", OutputFileFlags.Required
                            | OutputFileFlags.Zippable),

                        new RegexOutputFile(Regex.Escape(baseFilename) + @"_(\d{8})T\d{6}\.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "cmd"),
                        new($"{baseFilename}_cmd.txt", OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "cmd"),
                        new($"{baseFilename}_disc.txt", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "disc"),
                    ];
            }

            return [];
        }

        #endregion

        #region Private Extra Methods

        /// <summary>
        /// Get the command file path and extract the version from it
        /// </summary>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <returns>The version as a string, both null on error</returns>
        private static string? GetCommandFilePathAndVersion(string basePath, out string? commandPath)
        {
            // If we have an invalid base path, we can do nothing
            commandPath = null;
            if (string.IsNullOrEmpty(basePath))
                return null;

            // Generate the matching regex based on the base path
            string baseFilename = Path.GetFileName(basePath);
            var cmdFilenameRegex = new Regex(Regex.Escape(baseFilename) + @"_(\d{8})T\d{6}\.txt");

            // Find the first match for the command file
            var parentDirectory = Path.GetDirectoryName(basePath);
            if (string.IsNullOrEmpty(parentDirectory))
                return null;

            var currentFiles = Directory.GetFiles(parentDirectory);
            commandPath = Array.Find(currentFiles, f => cmdFilenameRegex.IsMatch(f));
            if (string.IsNullOrEmpty(value: commandPath))
                return null;

            // Extract the version string
            var match = cmdFilenameRegex.Match(commandPath);
            return match.Groups[1].Value;
        }

        #endregion

        #region Information Extraction Methods

        /// <summary>
        /// Get the PSX/PS2/KP2 EXE Date from the log, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <param name="serial">Internal serial</param>
        /// <param name="psx">True if PSX disc, false otherwise</param>
        /// <returns>EXE date if possible, null otherwise</returns>
        public static string? GetPlayStationEXEDate(string log, string? exeName, bool psx = false)
        {
            // If the file doesn't exist, we can't get the info
            if (!File.Exists(log))
                return null;

            // If the EXE name is not valid, we can't get the info
            if (string.IsNullOrEmpty(exeName))
                return null;

            try
            {
                string? exeDate = null;
                using var sr = File.OpenText(log);
                var line = sr.ReadLine();
                while (line != null)
                {
                    // Trim the line for later use
                    line = line.Trim();

                    // The exe date is listed in a single line, File Identifier: ABCD_123.45;1
                    if (line.Length >= "File Identifier: ".Length + 11 &&
                        line.StartsWith("File Identifier:") &&
                        line.Substring("File Identifier: ".Length) == exeName)
                    {
                        // Account for Y2K date problem
                        if (exeDate != null && exeDate.Substring(0, 2) == "19")
                        {
                            string decade = exeDate!.Substring(2, 1);

                            // Does only PSX need to account for 1920s-60s?
                            if (decade == "0" || decade == "1" ||
                                psx && (decade == "2" || decade == "3" || decade == "4" || decade == "5" || decade == "6"))
                                exeDate = $"20{exeDate.Substring(2)}";
                        }

                        // Currently stored date is the EXE date, return it
                        return exeDate;
                    }

                    // The exe datetime is listed in a single line
                    if (line.Length >= "Recording Date and Time: ".Length + 10 &&
                        line.StartsWith("Recording Date and Time:"))
                    {
                        // exe date: ISO datetime (yyyy-MM-ddT.....)
                        exeDate = line.Substring("Recording Date and Time: ".Length, 10);
                    }

                    line = sr.ReadLine();
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
        /// Get reported disc type information, if possible
        /// </summary>
        /// <param name="drive">_disc.txt file location</param>
        /// <returns>True if disc type info was set, false otherwise</returns>
        private static bool GetDiscType(string drive, out string? discTypeOrBookType)
        {
            // Set the default values
            discTypeOrBookType = null;

            // If the file doesn't exist, we can't get the info
            if (!File.Exists(drive))
                return false;

            try
            {
                // Create a hashset to contain all of the found values
                var discTypeOrBookTypeSet = new HashSet<string>();

                using var sr = File.OpenText(drive);
                var line = sr.ReadLine();
                while (line != null)
                {
                    // Trim the line for later use
                    line = line.Trim();

                    // Concatenate all found values for each possible line type
                    if (line.StartsWith("DiscType:"))
                    {
                        // DiscType: <discType>
                        string identifier = line.Substring("DiscType: ".Length);
                        discTypeOrBookTypeSet.Add(identifier);
                    }
                    else if (line.StartsWith("DiscTypeIdentifier:"))
                    {
                        // DiscTypeIdentifier: <discType>
                        string identifier = line.Substring("DiscTypeIdentifier: ".Length);
                        discTypeOrBookTypeSet.Add(identifier);
                    }
                    else if (line.StartsWith("DiscTypeSpecific:"))
                    {
                        // DiscTypeSpecific: <discType>
                        string identifier = line.Substring("DiscTypeSpecific: ".Length);
                        discTypeOrBookTypeSet.Add(identifier);
                    }
                    else if (line.StartsWith("BookType:"))
                    {
                        // BookType: <discType>
                        string identifier = line.Substring("BookType: ".Length);
                        discTypeOrBookTypeSet.Add(identifier);
                    }

                    line = sr.ReadLine();
                }

                // Create the output string
                if (discTypeOrBookTypeSet.Count > 0)
                    discTypeOrBookType = string.Join(", ", [.. discTypeOrBookTypeSet]);

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
        /// <param name="cssKey">_CSSKey.txt file location</param>
        /// <param name="disc">_disc.txt file location</param>
        /// <param name="includeAlways">Indicates whether region and protection type are always included</param>
        /// <returns>Formatted string representing the DVD protection, null on error</returns>
        private static string? GetDVDProtection(string cssKey, string disc, bool includeAlways)
        {
            // If one of the files doesn't exist, we can't get info from them
            if (!File.Exists(disc))
                return null;

            // Setup all of the individual pieces
            string? region = null, rceProtection = null, copyrightProtectionSystemType = null, vobKeys = null, decryptedDiscKey = null;

            // Get everything from _disc.txt first
            using (var sr = File.OpenText(disc))
            {
                try
                {
                    // Fast forward to the copyright information
                    while (sr.ReadLine()?.Trim()?.StartsWith("========== CopyrightInformation ==========") == false) ;

                    // Now read until we hit the manufacturing information
                    var line = sr.ReadLine()?.Trim();
                    while (line?.StartsWith("========== ManufacturingInformation ==========") == false)
                    {
                        if (line == null)
                            break;

                        if (line.StartsWith("CopyrightProtectionType"))
                            copyrightProtectionSystemType = line.Substring("CopyrightProtectionType: ".Length);
                        else if (line.StartsWith("RegionManagementInformation"))
                            region = line.Substring("RegionManagementInformation: ".Length);

                        line = sr.ReadLine()?.Trim();
                    }
                }
                catch { }
            }

            // Get everything from _CSSKey.txt next, if it exists
            if (File.Exists(cssKey))
            {
                try
                {
                    // Read until the end
                    using var sr = File.OpenText(cssKey);
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine()?.Trim();
                        if (line == null)
                            break;

                        if (line.StartsWith("DecryptedDiscKey"))
                        {
                            decryptedDiscKey = line.Substring("DecryptedDiscKey[020]: ".Length);
                        }
                        else if (line.StartsWith("LBA:"))
                        {
                            // Set the key string if necessary
                            vobKeys ??= string.Empty;

                            // No keys
                            if (line.Contains("No TitleKey"))
                            {
                                var match = Regex.Match(line, @"^LBA:\s*[0-9]+, Filename: (.*?), No TitleKey$", RegexOptions.Compiled);
                                string matchedFilename = match.Groups[1].Value;
                                if (matchedFilename.EndsWith(";1"))
                                    matchedFilename = matchedFilename.Substring(0, matchedFilename.Length - 2);

                                vobKeys += $"{matchedFilename} Title Key: No Title Key\n";
                            }
                            else
                            {
                                var match = Regex.Match(line, @"^LBA:\s*[0-9]+, Filename: (.*?), EncryptedTitleKey: .*?, DecryptedTitleKey: (.*?)$", RegexOptions.Compiled);
                                string matchedFilename = match.Groups[1].Value;
                                if (matchedFilename.EndsWith(";1"))
                                    matchedFilename = matchedFilename.Substring(0, matchedFilename.Length - 2);

                                vobKeys += $"{matchedFilename} Title Key: {match.Groups[2].Value}\n";
                            }
                        }
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
        /// Get the detected error count from the input files, if possible
        /// </summary>
        /// <param name="edcecc">.img_EdcEcc.txt/.img_EccEdc.txt file location</param>
        /// <returns>Error count if possible, -1 on error</returns>
        private static long GetErrorCount(string edcecc)
        {
            // TODO: Better usage of _mainInfo and _c2Error for uncorrectable errors

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(edcecc))
                return -1;

            // Get a total error count for after
            long? totalErrors = null;

            // First line of defense is the EdcEcc error file
            try
            {
                // Read in the error count whenever we find it
                using var sr = File.OpenText(edcecc);
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine()?.Trim();
                    if (line == null)
                        break;

                    if (line.StartsWith("[NO ERROR]"))
                    {
                        totalErrors = 0;
                        break;
                    }
                    else if (line.StartsWith("Total errors"))
                    {
                        totalErrors ??= 0;

                        if (Int64.TryParse(line.Substring("Total errors: ".Length).Trim(), out long te))
                            totalErrors += te;
                    }
                    else if (line.StartsWith("Total warnings"))
                    {
                        totalErrors ??= 0;

                        if (Int64.TryParse(line.Substring("Total warnings: ".Length).Trim(), out long tw))
                            totalErrors += tw;
                    }
                }

                // If we haven't found anything, return -1
                return totalErrors ?? -1;
            }
            catch
            {
                // We don't care what the exception is right now
                return Int64.MaxValue;
            }
        }

        /// <summary>
        /// Get the build info from a GD-ROM LD area, if possible
        /// </summary>
        /// <<param name="segaHeader">String representing a formatter variant of the GD-ROM header</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private static bool GetGDROMBuildInfo(string? segaHeader, out string? serial, out string? version, out string? date)
        {
            serial = null; version = null; date = null;

            // If the input header is null, we can't do a thing
            if (string.IsNullOrEmpty(segaHeader))
                return false;

            // Now read it in cutting it into lines for easier parsing
            try
            {
                string[] header = segaHeader!.Split('\n');
                string versionLine = header[4].Substring(58);
                string dateLine = header[5].Substring(58);
                serial = versionLine.Substring(0, 10).TrimEnd();
                version = versionLine.Substring(10, 6).TrimStart('V', 'v');
                date = dateLine.Substring(0, 8);
                return true;
            }
            catch
            {
                // We don't care what the error is
                return false;
            }
        }

        /// <summary>
        /// Get hardware information from the input file, if possible
        /// </summary>
        /// <param name="drive">_drive.txt file location</param>
        /// <returns>True if hardware info was set, false otherwise</returns>
        private static bool GetHardwareInfo(string drive, out string? manufacturer, out string? model, out string? firmware)
        {
            // Set the default values
            manufacturer = null; model = null; firmware = null;

            // If the file doesn't exist, we can't get the info
            if (!File.Exists(drive))
                return false;

            try
            {
                using var sr = File.OpenText(drive);
                var line = sr.ReadLine();
                while (line != null)
                {
                    // Trim the line for later use
                    line = line.Trim();

                    // Only take the first instance of each value
                    if (string.IsNullOrEmpty(manufacturer) && line.StartsWith("VendorId"))
                    {
                        // VendorId: <manufacturer>
                        manufacturer = line.Substring("VendorId: ".Length);
                    }
                    else if (string.IsNullOrEmpty(model) && line.StartsWith("ProductId"))
                    {
                        // ProductId: <model>
                        model = line.Substring("ProductId: ".Length);
                    }
                    else if (string.IsNullOrEmpty(firmware) && line.StartsWith("ProductRevisionLevel"))
                    {
                        // ProductRevisionLevel: <firmware>
                        firmware = line.Substring("ProductRevisionLevel: ".Length);
                    }

                    line = sr.ReadLine();
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
        /// Get the layerbreak from the input file, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <param name="xgd">True if XGD layerbreak info should be used, false otherwise</param>
        /// <returns>Layerbreak if possible, null on error</returns>
        private static string? GetLayerbreak(string disc, bool xgd)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
                return null;

            try
            {
                using var sr = File.OpenText(disc);
                var line = sr.ReadLine();
                while (line != null)
                {
                    // Trim the line for later use
                    line = line.Trim();

                    // Single-layer discs have no layerbreak
                    if (line.Contains("NumberOfLayers: Single Layer"))
                    {
                        return null;
                    }

                    // Xbox discs have a special layerbreaks
                    else if (xgd && line.StartsWith("LayerBreak"))
                    {
                        // LayerBreak: <size> (L0 Video: <size>, L0 Middle: <size>, L0 Game: <size>)
                        string[] split = Array.FindAll(line.Split(' '), s => !string.IsNullOrEmpty(s));
                        return split[1];
                    }

                    // Dual-layer discs have a regular layerbreak
                    else if (!xgd && line.StartsWith("LayerZeroSector"))
                    {
                        // LayerZeroSector: <size> (<hex>)
                        string[] split = Array.FindAll(line.Split(' '), s => !string.IsNullOrEmpty(s));
                        return split[1];
                    }

                    line = sr.ReadLine();
                }

                // If we get to the end, there's an issue
                return null;
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get if LibCrypt data is detected in the subchannel file, if possible
        /// </summary>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <returns>Status of the LibCrypt data, if possible</returns>
        private static void GetLibCryptDetected(string basePath, out YesNo detected, out string? data)
        {
            string subPath = basePath + ".sub";
            if (!File.Exists(subPath))
            {
                detected = YesNo.NULL;
                data = "LibCrypt could not be detected because subchannel file is missing";
                return;
            }

            if (!LibCrypt.DetectLibCrypt([subPath]))
            {
                detected = YesNo.No;
                data = null;
                return;
            }

            // Guard against false positives
            if (File.Exists(basePath + "_subIntention.txt"))
            {
                string libCryptData = ProcessingTool.GetFullFile(basePath + "_subIntention.txt") ?? "";
                if (string.IsNullOrEmpty(libCryptData))
                {
                    detected = YesNo.No;
                    data = null;
                }
                else
                {
                    detected = YesNo.Yes;
                    data = libCryptData;
                }
            }
            else
            {
                detected = YesNo.No;
                data = null;
            }
        }

        /// <summary>
        /// Get multisession information from the input file, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <returns>Formatted multisession information, null on error</returns>
        private static string? GetMultisessionInformation(string disc)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
                return null;

            try
            {
                // Seek to the TOC data
                using var sr = File.OpenText(disc);
                var line = sr.ReadLine();
                if (line == null)
                    return null;

                if (!line.StartsWith("========== TOC"))
                    while ((line = sr.ReadLine())?.StartsWith("========== TOC") == false) ;
                if (line == null)
                    return null;

                // Create the required regex
                var trackLengthRegex = new Regex(@"^\s*.*?Track\s*([0-9]{1,2}), LBA\s*[0-9]{1,8} - \s*[0-9]{1,8}, Length\s*([0-9]{1,8})$", RegexOptions.Compiled);

                // Read in the track length data
                var trackLengthMapping = new Dictionary<string, string>();
                while ((line = sr.ReadLine())?.Contains("Track") == true)
                {
                    var match = trackLengthRegex.Match(line);
                    trackLengthMapping[match.Groups[1].Value] = match.Groups[2].Value;
                }

                if (line == null)
                    return null;

                // Seek to the FULL TOC data
                line = sr.ReadLine();
                if (line == null)
                    return null;

                if (!line.StartsWith("========== FULL TOC"))
                    while ((line = sr.ReadLine())?.StartsWith("========== FULL TOC") == false) ;
                if (line == null)
                    return null;

                // Create the required regex
                var trackSessionRegex = new Regex(@"^\s*Session\s*([0-9]{1,2}),.*?,\s*Track\s*([0-9]{1,2}).*?$", RegexOptions.Compiled);

                // Read in the track session data
                var trackSessionMapping = new Dictionary<string, string>();
                while ((line = sr.ReadLine())?.StartsWith("========== OpCode") == false)
                {
                    if (line == null)
                        return null;

                    var match = trackSessionRegex.Match(line);
                    if (!match.Success)
                        continue;

                    trackSessionMapping[match.Groups[2].Value] = match.Groups[1].Value;
                }

                // If we have all Session 1, we can just skip out
                if (trackSessionMapping.Values.All(v => v == "1"))
                    return null;

                // Seek to the multisession data
                line = sr.ReadLine()?.Trim();
                if (line == null)
                    return null;

                if (!line.StartsWith("Lead-out length"))
                    while ((line = sr.ReadLine()?.Trim())?.StartsWith("Lead-out length") == false) ;

                // TODO: Are there any examples of 3+ session discs?

                // Read the first session lead-out
                var firstSessionLeadOutLengthString = line?.Substring("Lead-out length of 1st session: ".Length);
                line = sr.ReadLine()?.Trim();
                if (line == null)
                    return null;

                // Read the second session lead-in, if it exists
                string? secondSessionLeadInLengthString = null;
                while (line?.StartsWith("Lead-in length") == false)
                {
                    secondSessionLeadInLengthString = line?.Substring("Lead-in length of 2nd session: ".Length);
                    line = sr.ReadLine()?.Trim();
                }

                // Read the second session pregap
                var secondSessionPregapLengthString = line?.Substring("Pregap length of 1st track of 2nd session: ".Length);

                // Calculate the session gap total
                if (!int.TryParse(firstSessionLeadOutLengthString, out int firstSessionLeadOutLength))
                    firstSessionLeadOutLength = 0;
                if (!int.TryParse(secondSessionLeadInLengthString, out int secondSessionLeadInLength))
                    secondSessionLeadInLength = 0;
                if (!int.TryParse(secondSessionPregapLengthString, out int secondSessionPregapLength))
                    secondSessionPregapLength = 0;
                int sessionGapTotal = firstSessionLeadOutLength + secondSessionLeadInLength + secondSessionPregapLength;

                // Calculate first session length and total length
                int firstSessionLength = 0, totalLength = 0;
                foreach (var lengthMapping in trackLengthMapping)
                {
                    if (!int.TryParse(lengthMapping.Value, out int trackLength))
                        trackLength = 0;

                    if (trackSessionMapping.TryGetValue(lengthMapping.Key, out var session))
                        firstSessionLength += session == "1" ? trackLength : 0;

                    totalLength += trackLength;
                }

                // Adjust the session gap in a consistent way
                if (firstSessionLength - sessionGapTotal < 0)
                    sessionGapTotal = firstSessionLeadOutLength + secondSessionLeadInLength;

                // Create and return the formatted output
                string multisessionData =
                    $"Session 1: 0-{firstSessionLength - sessionGapTotal - 1}\n"
                    + $"Session 2: {firstSessionLength}-{totalLength - 1}";

                return multisessionData;
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
        /// <param name="disc">_disc.txt file location</param>
        /// <returns>Anti-modchip existence if possible, false on error</returns>
        private static bool? GetPlayStationAntiModchipDetected(string disc)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
                return null;

            try
            {
                // Check for either antimod string
                using var sr = File.OpenText(disc);
                var line = sr.ReadLine()?.Trim();
                if (line == null)
                    return null;

                while (!sr.EndOfStream)
                {
                    if (line == null)
                        return false;

                    if (line.StartsWith("Detected anti-mod string"))
                        return true;
                    else if (line.StartsWith("No anti-mod string"))
                        return false;

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
        /// Get the info from a PlayStation 3 disc, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <returns>True if section found, null on error</returns>
        private static bool GetPlayStation3Info(string disc, out string? serial, out string? version, out string? firmwareVersion)
        {
            // Set the default values
            serial = null; version = null; firmwareVersion = null;

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
                return false;

            try
            {
                using var sr = File.OpenText(disc);
                var line = sr.ReadLine()?.Trim();
                if (line == null)
                    return false;

                string section = string.Empty;
                while (!sr.EndOfStream)
                {
                    if (line == null)
                        return false;

                    // Determine the section we are in
                    if (line.EndsWith("PS3_DISC.SFB =========="))
                    {
                        section = "SFB";
                    }
                    else if (line.EndsWith("PARAM.SFO =========="))
                    {
                        section = "SFO";
                    }
                    else if (line.EndsWith("PS3UPDAT.PUP =========="))
                    {
                        section = "UPDATE";
                    }

                    // If we're in a section already
                    else if (!string.IsNullOrEmpty(section))
                    {
                        // Firmware Version
                        if (line.StartsWith("version") && section == "UPDATE")
                        {
                            firmwareVersion = line.Substring("version: ".Length);
                        }

                        // Internal Serial
                        else if (line.StartsWith("TITLE_ID"))
                        {
                            // Always use the SFB if it exists
                            if (section == "SFB")
                                serial = line.Substring("TITLE_ID: ".Length);
                            else if (section == "SFO" && string.IsNullOrEmpty(version))
                                serial = line.Substring("TITLE_ID: ".Length).Insert(4, "-");
                        }

                        // Version
                        else if (line.StartsWith("VERSION"))
                        {
                            // Always use the SFB if it exists
                            if (section == "SFB")
                                version = line.Substring("VERSION: ".Length);
                            else if (section == "SFO" && string.IsNullOrEmpty(version))
                                version = line.Substring("VERSION: ".Length);
                        }

                        // Set the section, if needed
                        if (line.EndsWith("PS3_DISC.SFB =========="))
                            section = "SFB";
                        else if (line.EndsWith("PARAM.SFO =========="))
                            section = "SFO";
                        else if (line.EndsWith("PS3UPDAT.PUP =========="))
                            section = "UPDATE";
                        else if (line.StartsWith("=========="))
                            section = string.Empty;
                    }

                    // Everything else resets the section
                    else
                    {
                        section = string.Empty;
                    }

                    line = sr.ReadLine()?.Trim();
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
        /// Get the detected missing EDC count from the input files, if possible
        /// </summary>
        /// <param name="edcecc">.img_EdcEcc.txt file location</param>
        /// <returns>Status of PS1 EDC, if possible</returns>
        private static bool? GetPlayStationEDCStatus(string edcecc)
        {
            // If one of the files doesn't exist, we can't get info from them
            if (!File.Exists(edcecc))
                return null;

            // First line of defense is the EdcEcc error file
            int modeTwoNoEdc = 0;
            int modeTwoFormTwo = 0;
            try
            {
                using var sr = File.OpenText(edcecc);
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line == null)
                        break;

                    if (line.Contains("mode 2 form 2"))
                        modeTwoFormTwo++;
                    else if (line.Contains("mode 2 no edc"))
                        modeTwoNoEdc++;
                }

                // This shouldn't happen
                if (modeTwoNoEdc == 0 && modeTwoFormTwo == 0)
                    return null;

                // EDC exists
                else if (modeTwoNoEdc == 0 && modeTwoFormTwo != 0)
                    return true;

                // EDC doesn't exist
                else if (modeTwoNoEdc != 0 && modeTwoFormTwo == 0)
                    return false;

                // This shouldn't happen
                else if (modeTwoNoEdc != 0 && modeTwoFormTwo != 0)
                    return null;

                // No idea how it would fall through
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
        /// <param name="mainInfo">_mainInfo.txt file location</param>
        /// <returns>Newline-delimited PVD if possible, null on error</returns>
        private static string? GetPVD(string mainInfo)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(mainInfo))
                return null;

            try
            {
                // If we're in a new mainInfo, the location of the header changed
                using var sr = File.OpenText(mainInfo);
                var line = sr.ReadLine();
                if (line == null)
                    return null;

                if (line.StartsWith("========== OpCode")
                    || line.StartsWith("========== TOC (Binary)")
                    || line.StartsWith("========== FULL TOC (Binary)"))
                {
                    // Seek to unscrambled data
                    while ((line = sr.ReadLine())?.StartsWith("========== Check Volume Descriptor ==========") == false) ;

                    // Read the next line so the search goes properly
                    line = sr.ReadLine();
                }

                if (line == null)
                    return null;

                // Make sure we're in the area
                if (line.StartsWith("========== LBA") == false)
                    while ((line = sr.ReadLine())?.StartsWith("========== LBA") == false) ;
                if (line == null)
                    return null;

                // If we have a Sega disc, skip sector 0
                if (line.StartsWith("========== LBA[000000, 0000000]: Main Channel =========="))
                    while ((line = sr.ReadLine())?.StartsWith("========== LBA") == false) ;
                if (line == null)
                    return null;

                // If we have a PlayStation disc, skip sector 4
                if (line.StartsWith("========== LBA[000004, 0x00004]: Main Channel =========="))
                    while ((line = sr.ReadLine())?.StartsWith("========== LBA") == false) ;
                if (line == null)
                    return null;

                // We assume the first non-LBA0/4 sector listed is the proper one
                // Fast forward to the PVD
                while ((line = sr.ReadLine())?.StartsWith("0310") == false) ;

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
        /// Get the build info from a Saturn disc, if possible
        /// </summary>
        /// <<param name="segaHeader">String representing a formatter variant of the Saturn header</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private static bool GetSaturnBuildInfo(string? segaHeader, out string? serial, out string? version, out string? date)
        {
            serial = null; version = null; date = null;

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
                date = dateLine.Substring(0, 8);
                date = $"{date[0]}{date[1]}{date[2]}{date[3]}-{date[4]}{date[5]}-{date[6]}{date[7]}";
                return true;
            }
            catch
            {
                // We don't care what the error is
                return false;
            }
        }

        /// <summary>
        /// Get the build info from a Sega CD disc, if possible
        /// </summary>
        /// <<param name="segaHeader">String representing a formatter variant of the  Sega CD header</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        /// <remarks>Note that this works for MOST headers, except ones where the copyright stretches > 1 line</remarks>
        private static bool GetSegaCDBuildInfo(string? segaHeader, out string? serial, out string? date)
        {
            serial = null; date = null;

            // If the input header is null, we can't do a thing
            if (string.IsNullOrEmpty(segaHeader))
                return false;

            // Now read it in cutting it into lines for easier parsing
            try
            {
                string[] header = segaHeader!.Split('\n');
                string serialVersionLine = header[8].Substring(58);
                string dateLine = header[1].Substring(58);
                serial = serialVersionLine.Substring(3, 8).TrimEnd('-', ' ');
                date = dateLine.Substring(8).Trim();

                // Properly format the date string, if possible
                string[] dateSplit = date.Split('.');

                if (dateSplit.Length == 1)
                    dateSplit = [date.Substring(0, 4), date.Substring(4)];

                string month = dateSplit[1];
                dateSplit[1] = month switch
                {
                    "JAN" => "01",
                    "FEB" => "02",
                    "MAR" => "03",
                    "APR" => "04",
                    "MAY" => "05",
                    "JUN" => "06",
                    "JUL" => "07",
                    "AUG" => "08",
                    "SEP" => "09",
                    "OCT" => "10",
                    "NOV" => "11",
                    "DEC" => "12",
                    _ => "00",
                };

                date = string.Join("-", dateSplit);

                return true;
            }
            catch
            {
                // We don't care what the error is
                return false;
            }
        }

        /// <summary>
        /// Get the header from a Sega CD / Mega CD, Saturn, or Dreamcast Low-Density region, if possible
        /// </summary>
        /// <param name="mainInfo">_mainInfo.txt file location</param>
        /// <returns>Header as a byte array if possible, null on error</returns>
        private static string? GetSegaHeader(string mainInfo)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(mainInfo))
                return null;

            try
            {
                // If we're in a new mainInfo, the location of the header changed
                using var sr = File.OpenText(mainInfo);
                var line = sr.ReadLine();
                if (line == null)
                    return null;

                if (line.StartsWith("========== OpCode")
                    || line.StartsWith("========== TOC (Binary)")
                    || line.StartsWith("========== FULL TOC (Binary)"))
                {
                    // Seek to unscrambled data
                    while ((line = sr.ReadLine())?.Contains("Check MCN and/or ISRC") == false) ;
                    if (line == null)
                        return null;

                    // Read the next line so the search goes properly
                    line = sr.ReadLine();
                }

                if (line == null)
                    return null;

                // Make sure we're in the area
                if (!line.StartsWith("========== LBA"))
                    while ((line = sr.ReadLine())?.StartsWith("========== LBA") == false) ;
                if (line == null)
                    return null;

                // Make sure we're in the right sector
                if (!line.StartsWith("========== LBA[000000, 0000000]: Main Channel =========="))
                    while ((line = sr.ReadLine())?.StartsWith("========== LBA[000000, 0000000]: Main Channel ==========") == false) ;
                if (line == null)
                    return null;

                // Fast forward to the header
                while ((line = sr.ReadLine())?.Trim()?.StartsWith("+0 +1 +2 +3 +4 +5 +6 +7  +8 +9 +A +B +C +D +E +F") == false) ;
                if (line == null)
                    return null;

                // Now that we're at the Header, read each line in and concatenate
                string header = "";
                for (int i = 0; i < 32; i++)
                    header += sr.ReadLine() + "\n"; // 0000-01F0

                return header;
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
        /// <param name="volDesc">_volDesc.txt file location</param>
        /// <returns>Volume labels (by type), or null if none present</returns>
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

        /// <summary>
        /// Get the write offset from the input file, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <returns>Sample write offset if possible, null on error</returns>
        private static string? GetWriteOffset(string disc)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
                return null;

            try
            {
                // Get a list for all found offsets
                var offsets = new List<string>();

                // Loop over all possible offsets
                using var sr = File.OpenText(disc);

                while (!sr.EndOfStream)
                {
                    // Fast forward to the offsets
                    while (sr.ReadLine()?.Trim()?.StartsWith("========== Offset") == false) ;
                    if (sr.EndOfStream)
                        break;

                    sr.ReadLine(); // Combined Offset
                    sr.ReadLine(); // Drive Offset
                    sr.ReadLine(); // Separator line

                    // Now that we're at the offsets, attempt to get the sample offset
                    var offsetLine = sr.ReadLine()?.Split(' ');
                    string offset = offsetLine != null ? offsetLine[offsetLine.Length - 1] : string.Empty;
                    offsets.Add(offset);
                }

                // Deduplicate the offsets
                offsets = offsets
                    .FindAll(s => !string.IsNullOrEmpty(s))
                    .Distinct()
                    .ToList();

                // Now that we're at the offsets, attempt to get the sample offset
                return string.Join("; ", [.. offsets]);
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get the XGD auxiliary hash info from the outputted files, if possible
        /// </summary>
        /// <param name="suppl">Datafile representing the supplementary hashes</param>
        /// <param name="dmihash">Extracted DMI.bin CRC32 hash (upper-cased)</param>
        /// <param name="pfihash">Extracted PFI.bin CRC32 hash (upper-cased)</param>
        /// <param name="sshash">Extracted SS.bin CRC32 hash (upper-cased)</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        /// <remarks>Currently only the CRC32 values are returned for each, this may change in the future</remarks>
        private static bool GetXGDAuxHashInfo(Datafile? suppl, out string? dmihash, out string? pfihash, out string? sshash)
        {
            // Assign values to all outputs first
            dmihash = null; pfihash = null; sshash = null;

            // If we don't have a valid datafile, we can't do anything
            if (suppl?.Game == null || suppl.Game.Length == 0)
                return false;

            // Try to extract the hash information
            var roms = suppl.Game[0].Rom;
            if (roms == null || roms.Length == 0)
                return false;

            dmihash = Array.Find(roms, r => r.Name?.EndsWith("DMI.bin") == true)?.CRC?.ToUpperInvariant();
            pfihash = Array.Find(roms, r => r.Name?.EndsWith("PFI.bin") == true)?.CRC?.ToUpperInvariant();
            sshash = Array.Find(roms, r => r.Name?.EndsWith("SS.bin") == true)?.CRC?.ToUpperInvariant();

            return true;
        }

        /// <summary>
        /// Get the XGD auxiliary info from the outputted files, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <param name="dmihash">Extracted DMI.bin CRC32 hash (upper-cased)</param>
        /// <param name="pfihash">Extracted PFI.bin CRC32 hash (upper-cased)</param>
        /// <param name="sshash">Extracted SS.bin CRC32 hash (upper-cased)</param>
        /// <param name="ss">Extracted security sector data</param>
        /// <param name="ssver">Extracted security sector version</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private static bool GetXGDAuxInfo(string disc, out string? dmihash, out string? pfihash, out string? sshash, out string? ss, out string? ssver)
        {
            dmihash = null; pfihash = null; sshash = null; ss = null; ssver = null;

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
                return false;

            // This flag is needed because recent versions of DIC include security data twice
            bool foundSecuritySectors = false;

            // SS version for all Kreon DIC dumps is v1
            ssver = "01";

            try
            {
                using var sr = File.OpenText(disc);
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine()?.Trim();
                    if (line == null)
                        break;

                    // XGD version (1 = Xbox, 2 = Xbox360)
                    /*
                    if (line.StartsWith("Version of challenge table"))
                    {
                        xgdver = line.Split(' ')[4]; // "Version of challenge table: <VER>"
                    }
                    */

                    // Security Sector ranges
                    else if (line.StartsWith("Number of security sector ranges:") && !foundSecuritySectors)
                    {
                        // Set the flag so we don't read duplicate data
                        foundSecuritySectors = true;

                        var layerRegex = new Regex(@"Layer [01].*, startLBA-endLBA:\s*(\d+)-\s*(\d+)", RegexOptions.Compiled);

                        line = sr.ReadLine()?.Trim();
                        if (line == null)
                            break;

                        while (!line.StartsWith("========== TotalLength ==========")
                            && !line.StartsWith("========== Unlock 2 state(wxripper) =========="))
                        {
                            // If we have a recognized line format, parse it
                            if (line.StartsWith("Layer "))
                            {
                                var match = layerRegex.Match(line);
                                ss += $"{match.Groups[1]}-{match.Groups[2]}\n";
                            }

                            line = sr.ReadLine()?.Trim();
                            if (line == null)
                                break;
                        }

                        if (line == null)
                            break;
                    }

                    // Special File Hashes
                    else if (line.StartsWith("<rom"))
                    {
                        if (ProcessingTool.GetISOHashValues(line, out long _, out var crc32, out _, out _))
                        {
                            if (line.Contains("SS.bin"))
                                sshash = crc32?.ToUpperInvariant();
                            else if (line.Contains("PFI.bin"))
                                pfihash = crc32?.ToUpperInvariant();
                            else if (line.Contains("DMI.bin"))
                                dmihash = crc32?.ToUpperInvariant();
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

        /// <summary>
        /// Get the XGD auxiliary security sector info from the outputted files, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <param name="ss">Extracted security sector data</param>
        /// <param name="ssver">Extracted security sector version</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private static bool GetXGDAuxSSInfo(string disc, out string? ss, out string? ssver)
        {
            ss = null; ssver = null;

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
                return false;

            // This flag is needed because recent versions of DIC include security data twice
            bool foundSecuritySectors = false;

            // SS version for all Kreon DIC dumps is v1
            ssver = "01";

            try
            {
                using var sr = File.OpenText(disc);
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine()?.Trim();
                    if (line == null)
                        break;

                    // XGD version (1 = Xbox, 2 = Xbox360)
                    /*
                    if (line.StartsWith("Version of challenge table"))
                    {
                        xgdver = line.Split(' ')[4]; // "Version of challenge table: <VER>"
                    }
                    */

                    // Security Sector ranges
                    else if (line.StartsWith("Number of security sector ranges:") && !foundSecuritySectors)
                    {
                        // Set the flag so we don't read duplicate data
                        foundSecuritySectors = true;

                        var layerRegex = new Regex(@"Layer [01].*, startLBA-endLBA:\s*(\d+)-\s*(\d+)", RegexOptions.Compiled);

                        line = sr.ReadLine()?.Trim();
                        if (line == null)
                            break;

                        while (!line.StartsWith("========== TotalLength ==========")
                            && !line.StartsWith("========== Unlock 2 state(wxripper) =========="))
                        {
                            // If we have a recognized line format, parse it
                            if (line.StartsWith("Layer "))
                            {
                                var match = layerRegex.Match(line);
                                ss += $"{match.Groups[1]}-{match.Groups[2]}\n";
                            }

                            line = sr.ReadLine()?.Trim();
                            if (line == null)
                                break;
                        }

                        if (line == null)
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

        #endregion
    }
}
