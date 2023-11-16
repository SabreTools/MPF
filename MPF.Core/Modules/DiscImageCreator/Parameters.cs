using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MPF.Core.Converters;
using MPF.Core.Data;
using MPF.Core.Utilities;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;

namespace MPF.Core.Modules.DiscImageCreator
{
    /// <summary>
    /// Represents a generic set of DiscImageCreator parameters
    /// </summary>
    public class Parameters : BaseParameters
    {
        #region Generic Dumping Information

        /// <inheritdoc/>
        public override string? InputPath => DrivePath;

        /// <inheritdoc/>
        public override string? OutputPath => Filename;

        /// <inheritdoc/>
        /// <inheritdoc/>
        public override int? Speed
        {
            get { return DriveSpeed; }
            set { DriveSpeed = (sbyte?)value; }
        }

        #endregion

        #region Metadata

        /// <inheritdoc/>
        public override InternalProgram InternalProgram => InternalProgram.DiscImageCreator;

        #endregion

        #region Common Input Values

        /// <summary>
        /// Drive letter or path to pass to DiscImageCreator
        /// </summary>
        public string? DrivePath { get; set; }

        /// <summary>
        /// Drive speed to set, if applicable
        /// </summary>
        public int? DriveSpeed { get; set; }

        /// <summary>
        /// Destination filename for DiscImageCreator output
        /// </summary>
        public string? Filename { get; set; }

        /// <summary>
        /// Optiarc drive output filename for merging
        /// </summary>
        public string? OptiarcFilename { get; set; }

        /// <summary>
        /// Start LBA value for dumping specific sectors
        /// </summary>
        public int? StartLBAValue { get; set; }

        /// <summary>
        /// End LBA value for dumping specific sectors
        /// </summary>
        public int? EndLBAValue { get; set; }

        #endregion

        #region Flag Values

        /// <summary>
        /// Manual offset for Audio CD (default 0)
        /// </summary>
        public int? AddOffsetValue { get; set; }

        /// <summary>
        /// 0xbe opcode value for dumping
        /// Possible values: raw (default), pack
        /// </summary>
        /// TODO: Make this an enum
        public string? BEOpcodeValue { get; set; }

        /// <summary>
        /// C2 reread options for dumping [CD only]
        /// [0] - Reread value (default 4000)
        /// [1] - C2 offset (default: 0)
        /// [2] - 0 reread issue sector (default), 1 reread all
        /// [3] - First LBA to reread (default 0)
        /// [4] - Last LBA to reread (default EOS)
        /// </summary>
        public int?[] C2OpcodeValue { get; set; } = new int?[5];

        /// <summary>
        /// C2 reread options for dumping [DVD/HD-DVD/BD only] (default 10)
        /// </summary>
        public int? DVDRereadValue { get; set; }

        /// <summary>
        /// End LBA for fixing
        /// </summary>
        public int? FixValue { get; set; }

        /// <summary>
        /// Set the force unit access flag value (default 1)
        /// </summary>
        public int? ForceUnitAccessValue { get; set; }

        /// <summary>
        /// Set the multi-sector read flag value (default 50)
        /// </summary>
        public int? MultiSectorReadValue { get; set; }

        /// <summary>
        /// Set the no skip security sector flag value (default 100)
        /// </summary>
        public int? NoSkipSecuritySectorValue { get; set; }

        /// <summary>
        /// Set the pad sector flag value (default 0)
        /// </summary>
        public byte? PadSectorValue { get; set; }

        /// <summary>
        /// Set the reverse End LBA value (required for DVD)
        /// </summary>
        public int? ReverseEndLBAValue { get; set; }

        /// <summary>
        /// Set the reverse Start LBA value (required for DVD)
        /// </summary>
        public int? ReverseStartLBAValue { get; set; }

        /// <summary>
        /// Set scan file timeout value (default 60)
        /// </summary>
        public int? ScanFileProtectValue { get; set; }

        /// <summary>
        /// Beginning and ending sectors to skip for physical protection (both default 0)
        /// </summary>
        public int?[] SkipSectorValue { get; set; } = new int?[2];

        /// <summary>
        /// Set the subchanel read level
        /// Possible values: 0 no next sub, 1 next sub (default), 2 next and next next
        /// </summary>
        public int? SubchannelReadLevelValue { get; set; }

        /// <summary>
        /// Set number of empty bytes to insert at the head of first track for VideoNow (default 0)
        /// </summary>
        public int? VideoNowValue { get; set; }

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

            var missingFiles = new List<string>();
            switch (this.Type)
            {
                case MediaType.CDROM:
                case MediaType.GDROM:
                    if (!File.Exists($"{basePath}.cue"))
                        missingFiles.Add($"{basePath}.cue");
                    if (!File.Exists($"{basePath}.img") && !File.Exists($"{basePath}.imgtmp"))
                        missingFiles.Add($"{basePath}.img");

                    // Audio-only discs don't output these files
                    if (!this.System.IsAudio())
                    {
                        if (!File.Exists($"{basePath}.scm") && !File.Exists($"{basePath}.scmtmp"))
                            missingFiles.Add($"{basePath}.scm");
                    }

                    if (!File.Exists($"{basePath}_logs.zip") || !preCheck)
                    {
                        // GD-ROM and GD-R don't output this for the HD area
                        if (this.Type != MediaType.GDROM)
                        {
                            if (!File.Exists($"{basePath}.ccd"))
                                missingFiles.Add($"{basePath}.ccd");
                        }

                        if (!File.Exists($"{basePath}.dat"))
                            missingFiles.Add($"{basePath}.dat");
                        if (!File.Exists($"{basePath}.sub") && !File.Exists($"{basePath}.subtmp"))
                            missingFiles.Add($"{basePath}.sub");
                        if (!File.Exists($"{basePath}_disc.txt"))
                            missingFiles.Add($"{basePath}_disc.txt");
                        if (!File.Exists($"{basePath}_drive.txt"))
                            missingFiles.Add($"{basePath}_drive.txt");
                        if (!File.Exists($"{basePath}_img.cue"))
                            missingFiles.Add($"{basePath}_img.cue");
                        if (!File.Exists($"{basePath}_mainError.txt"))
                            missingFiles.Add($"{basePath}_mainError.txt");
                        if (!File.Exists($"{basePath}_mainInfo.txt"))
                            missingFiles.Add($"{basePath}_mainInfo.txt");
                        if (!File.Exists($"{basePath}_subError.txt"))
                            missingFiles.Add($"{basePath}_subError.txt");
                        if (!File.Exists($"{basePath}_subInfo.txt"))
                            missingFiles.Add($"{basePath}_subInfo.txt");
                        if (!File.Exists($"{basePath}_subReadable.txt") && !File.Exists($"{basePath}_sub.txt"))
                            missingFiles.Add($"{basePath}_subReadable.txt");
                        if (!File.Exists($"{basePath}_volDesc.txt"))
                            missingFiles.Add($"{basePath}_volDesc.txt");

                        // Audio-only discs don't output these files
                        if (!this.System.IsAudio())
                        {
                            if (!File.Exists($"{basePath}.img_EdcEcc.txt") && !File.Exists($"{basePath}.img_EccEdc.txt"))
                                missingFiles.Add($"{basePath}.img_EdcEcc.txt");
                        }
                    }

                    // Removed or inconsistent files
                    //{
                    //    // Doesn't output on Linux
                    //    if (!File.Exists($"{basePath}.c2"))
                    //        missingFiles.Add($"{basePath}.c2");

                    //    // Doesn't output on Linux
                    //    if (!File.Exists($"{basePath}_c2Error.txt"))
                    //        missingFiles.Add($"{basePath}_c2Error.txt");

                    //    // Replaced by timestamp-named file
                    //    if (!File.Exists($"{basePath}_cmd.txt"))
                    //        missingFiles.Add($"{basePath}_cmd.txt");

                    //    // Not guaranteed output
                    //    if (!File.Exists($"{basePath}_subIntention.txt"))
                    //        missingFiles.Add($"{basePath}_subIntention.txt");

                    //    // Not guaranteed output
                    //    if (File.Exists($"{basePath}_suppl.dat"))
                    //        missingFiles.Add($"{basePath}_suppl.dat");

                    //    // Not guaranteed output (at least PCE)
                    //    if (!File.Exists($"{basePath}.toc"))
                    //        missingFiles.Add($"{basePath}.toc");
                    //}

                    break;

                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    if (!File.Exists($"{basePath}_logs.zip") || !preCheck)
                    {
                        if (!File.Exists($"{basePath}.dat"))
                            missingFiles.Add($"{basePath}.dat");
                        if (!File.Exists($"{basePath}_disc.txt"))
                            missingFiles.Add($"{basePath}_disc.txt");
                        if (!File.Exists($"{basePath}_drive.txt"))
                            missingFiles.Add($"{basePath}_drive.txt");
                        if (!File.Exists($"{basePath}_mainError.txt"))
                            missingFiles.Add($"{basePath}_mainError.txt");
                        if (!File.Exists($"{basePath}_mainInfo.txt"))
                            missingFiles.Add($"{basePath}_mainInfo.txt");
                        if (!File.Exists($"{basePath}_volDesc.txt"))
                            missingFiles.Add($"{basePath}_volDesc.txt");
                    }

                    // Removed or inconsistent files
                    //{
                    //    // Replaced by timestamp-named file
                    //    if (!File.Exists($"{basePath}_cmd.txt"))
                    //        missingFiles.Add($"{basePath}_cmd.txt");

                    //    // Not guaranteed output
                    //    if (File.Exists($"{basePath}_CSSKey.txt"))
                    //        missingFiles.Add($"{basePath}_CSSKey.txt");

                    //    // Only output for some parameters
                    //    if (File.Exists($"{basePath}.raw"))
                    //        missingFiles.Add($"{basePath}.raw");

                    //    // Not guaranteed output
                    //    if (File.Exists($"{basePath}_suppl.dat"))
                    //        missingFiles.Add($"{basePath}_suppl.dat");
                    //}

                    break;

                case MediaType.FloppyDisk:
                case MediaType.HardDisk:
                    // TODO: Determine what outputs come out from a HDD, SD, etc.
                    if (!File.Exists($"{basePath}_logs.zip") || !preCheck)
                    {
                        if (!File.Exists($"{basePath}.dat"))
                            missingFiles.Add($"{basePath}.dat");
                        if (!File.Exists($"{basePath}_disc.txt"))
                            missingFiles.Add($"{basePath}_disc.txt");
                    }

                    // Removed or inconsistent files
                    //{
                    //    // Replaced by timestamp-named file
                    //    if (!File.Exists($"{basePath}_cmd.txt"))
                    //        missingFiles.Add($"{basePath}_cmd.txt");
                    //}

                    break;

                default:
                    missingFiles.Add("Media and system combination not supported for DiscImageCreator");
                    break;
            }

            return (!missingFiles.Any(), missingFiles);
        }

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, Options options, string basePath, Drive? drive, bool includeArtifacts)
        {
            var outputDirectory = Path.GetDirectoryName(basePath);

            // Ensure that required sections exist
            info = Builder.EnsureAllSections(info);

            // Get the dumping program and version
            var (dicCmd, dicVersion) = GetCommandFilePathAndVersion(basePath);
            info.DumpingInfo!.DumpingProgram = $"{EnumConverter.LongName(this.InternalProgram)} {dicVersion ?? "Unknown Version"}";
            info.DumpingInfo.DumpingDate = InfoTool.GetFileModifiedDate(dicCmd)?.ToString("yyyy-MM-dd HH:mm:ss");

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
            var datafile = InfoTool.GetDatafile($"{basePath}.dat");

            // Fill in the hash data
            info.TracksAndWriteOffsets!.ClrMameProData = InfoTool.GenerateDatfile(datafile);

            // Extract info based generically on MediaType
            switch (this.Type)
            {
                case MediaType.CDROM:
                case MediaType.GDROM: // TODO: Verify GD-ROM outputs this
                    info.Extras!.PVD = GetPVD($"{basePath}_mainInfo.txt") ?? "Disc has no PVD";

                    // Audio-only discs will fail if there are any C2 errors, so they would never get here
                    if (this.System.IsAudio())
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

                    info.TracksAndWriteOffsets.Cuesheet = GetFullFile($"{basePath}.cue") ?? string.Empty;
                    //var cueSheet = new CueSheet($"{basePath}.cue"); // TODO: Do something with this

                    // Attempt to get the write offset
                    string cdWriteOffset = GetWriteOffset($"{basePath}_disc.txt") ?? string.Empty;
                    info.CommonDiscInfo.RingWriteOffset = cdWriteOffset;
                    info.TracksAndWriteOffsets.OtherWriteOffsets = cdWriteOffset;

                    // Attempt to get multisession data
                    string cdMultiSessionInfo = GetMultisessionInformation($"{basePath}_disc.txt") ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(cdMultiSessionInfo))
                        info.CommonDiscInfo.CommentsSpecialFields![SiteCode.Multisession] = cdMultiSessionInfo;

                    break;

                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:

                    // Get the individual hash data, as per internal
                    if (InfoTool.GetISOHashValues(datafile, out long size, out var crc32, out var md5, out var sha1))
                    {
                        info.SizeAndChecksums!.Size = size;
                        info.SizeAndChecksums.CRC32 = crc32;
                        info.SizeAndChecksums.MD5 = md5;
                        info.SizeAndChecksums.SHA1 = sha1;
                    }

                    // Deal with the layerbreaks
                    if (this.Type == MediaType.DVD)
                    {
                        string layerbreak = GetLayerbreak($"{basePath}_disc.txt", System.IsXGD()) ?? string.Empty;
                        info.SizeAndChecksums!.Layerbreak = !string.IsNullOrEmpty(layerbreak) ? Int64.Parse(layerbreak) : default;
                    }
                    else if (this.Type == MediaType.BluRay)
                    {
                        var di = InfoTool.GetDiscInformation($"{basePath}_PIC.bin");
                        info.SizeAndChecksums!.PICIdentifier = InfoTool.GetPICIdentifier(di);
                        if (InfoTool.GetLayerbreaks(di, out long? layerbreak1, out long? layerbreak2, out long? layerbreak3))
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
                    if (!options.EnableRedumpCompatibility || System != RedumpSystem.MicrosoftXbox)
                        info.Extras!.PVD = GetPVD($"{basePath}_mainInfo.txt") ?? string.Empty;

                    // Bluray-specific options
                    if (this.Type == MediaType.BluRay)
                    {
                        int trimLength = -1;
                        switch (this.System)
                        {
                            case RedumpSystem.SonyPlayStation3:
                            case RedumpSystem.SonyPlayStation4:
                            case RedumpSystem.SonyPlayStation5:
                                trimLength = 264;
                                break;
                        }

                        info.Extras!.PIC = GetPIC($"{basePath}_PIC.bin", trimLength) ?? string.Empty;
                    }

                    break;
            }

            // Extract info based specifically on RedumpSystem
            switch (this.System)
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
                            info.CopyProtection!.SecuROMData = GetFullFile($"{basePath}_subIntention.txt") ?? string.Empty;
                    }

                    break;

                case RedumpSystem.DVDAudio:
                case RedumpSystem.DVDVideo:
                    info.CopyProtection!.Protection = GetDVDProtection($"{basePath}_CSSKey.txt", $"{basePath}_disc.txt") ?? string.Empty;
                    break;

                case RedumpSystem.KonamiPython2:
                    if (InfoTool.GetPlayStationExecutableInfo(drive?.Name, out var pythonTwoSerial, out Region? pythonTwoRegion, out var pythonTwoDate))
                    {
                        // Ensure internal serial is pulled from local data
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = pythonTwoSerial ?? string.Empty;
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? pythonTwoRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = pythonTwoDate;
                    }

                    info.VersionAndEditions!.Version = InfoTool.GetPlayStation2Version(drive?.Name) ?? string.Empty;
                    break;

                case RedumpSystem.MicrosoftXbox:

                    string xmidString;
                    if (string.IsNullOrWhiteSpace(outputDirectory))
                        xmidString = GetXGD1XMID($"{basePath}_DMI.bin");
                    else
                        xmidString = GetXGD1XMID(Path.Combine(outputDirectory, $"{basePath}_DMI.bin"));

                    var xmid = SabreTools.Serialization.Wrappers.XMID.Create(xmidString);
                    if (xmid != null)
                    {
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.XMID] = xmidString?.TrimEnd('\0') ?? string.Empty;
                        info.CommonDiscInfo.Serial = xmid.Serial ?? string.Empty;
                        if (!options.EnableRedumpCompatibility)
                            info.VersionAndEditions!.Version = xmid.Version ?? string.Empty;

                        info.CommonDiscInfo.Region = InfoTool.GetXGDRegion(xmid.Model.RegionIdentifier);
                    }

                    // If we have the new, external DAT
                    if (File.Exists($"{basePath}_suppl.dat"))
                    {
                        var suppl = InfoTool.GetDatafile($"{basePath}_suppl.dat");
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
                    string xemidString;
                    if (string.IsNullOrWhiteSpace(outputDirectory))
                        xemidString = GetXGD23XeMID($"{basePath}_DMI.bin");
                    else
                        xemidString = GetXGD23XeMID(Path.Combine(outputDirectory, $"{basePath}_DMI.bin"));

                    var xemid = SabreTools.Serialization.Wrappers.XeMID.Create(xemidString);
                    if (xemid != null)
                    {
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.XeMID] = xemidString?.TrimEnd('\0') ?? string.Empty;
                        info.CommonDiscInfo.Serial = xemid.Serial ?? string.Empty;
                        if (!options.EnableRedumpCompatibility)
                            info.VersionAndEditions!.Version = xemid.Version ?? string.Empty;

                        info.CommonDiscInfo.Region = InfoTool.GetXGDRegion(xemid.Model.RegionIdentifier);
                    }

                    // If we have the new, external DAT
                    if (File.Exists($"{basePath}_suppl.dat"))
                    {
                        var suppl = InfoTool.GetDatafile($"{basePath}_suppl.dat");
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
                    if (this.Type == MediaType.CDROM)
                    {
                        info.Extras!.Header = GetSegaHeader($"{basePath}_mainInfo.txt") ?? string.Empty;

                        // Take only the first 16 lines for GD-ROM
                        if (!string.IsNullOrEmpty(info.Extras.Header))
                            info.Extras.Header = string.Join("\n", info.Extras.Header.Split('\n').Take(16));

                        if (GetGDROMBuildInfo(info.Extras.Header, out var gdSerial, out var gdVersion, out var gdDate))
                        {
                            // Ensure internal serial is pulled from local data
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = gdSerial ?? string.Empty;
                            info.VersionAndEditions!.Version = gdVersion ?? string.Empty;
                            info.CommonDiscInfo.EXEDateBuildDate = gdDate ?? string.Empty;
                        }
                    }

                    break;

                case RedumpSystem.SegaMegaCDSegaCD:
                    info.Extras!.Header = GetSegaHeader($"{basePath}_mainInfo.txt") ?? string.Empty;

                    // Take only the last 16 lines for Sega CD
                    if (!string.IsNullOrEmpty(info.Extras.Header))
                        info.Extras.Header = string.Join("\n", info.Extras.Header.Split('\n').Skip(16));

                    if (GetSegaCDBuildInfo(info.Extras.Header, out var scdSerial, out var fixedDate))
                    {
                        // Ensure internal serial is pulled from local data
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = scdSerial ?? string.Empty;
                        info.CommonDiscInfo.EXEDateBuildDate = fixedDate ?? string.Empty;
                    }

                    break;

                case RedumpSystem.SegaChihiro:
                    if (this.Type == MediaType.CDROM)
                    {
                        info.Extras!.Header = GetSegaHeader($"{basePath}_mainInfo.txt") ?? string.Empty;

                        // Take only the first 16 lines for GD-ROM
                        if (!string.IsNullOrEmpty(info.Extras.Header))
                            info.Extras.Header = string.Join("\n", info.Extras.Header.Split('\n').Take(16));

                        if (GetGDROMBuildInfo(info.Extras.Header, out var gdSerial, out var gdVersion, out var gdDate))
                        {
                            // Ensure internal serial is pulled from local data
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = gdSerial ?? string.Empty;
                            info.VersionAndEditions!.Version = gdVersion ?? string.Empty;
                            info.CommonDiscInfo.EXEDateBuildDate = gdDate ?? string.Empty;
                        }
                    }

                    break;

                case RedumpSystem.SegaDreamcast:
                    if (this.Type == MediaType.CDROM)
                    {
                        info.Extras!.Header = GetSegaHeader($"{basePath}_mainInfo.txt") ?? string.Empty;

                        // Take only the first 16 lines for GD-ROM
                        if (!string.IsNullOrEmpty(info.Extras.Header))
                            info.Extras.Header = string.Join("\n", info.Extras.Header.Split('\n').Take(16));

                        if (GetGDROMBuildInfo(info.Extras.Header, out var gdSerial, out var gdVersion, out var gdDate))
                        {
                            // Ensure internal serial is pulled from local data
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = gdSerial ?? string.Empty;
                            info.VersionAndEditions!.Version = gdVersion ?? string.Empty;
                            info.CommonDiscInfo.EXEDateBuildDate = gdDate ?? string.Empty;
                        }
                    }

                    break;

                case RedumpSystem.SegaNaomi:
                    if (this.Type == MediaType.CDROM)
                    {
                        info.Extras!.Header = GetSegaHeader($"{basePath}_mainInfo.txt") ?? string.Empty;

                        // Take only the first 16 lines for GD-ROM
                        if (!string.IsNullOrEmpty(info.Extras.Header))
                            info.Extras.Header = string.Join("\n", info.Extras.Header.Split('\n').Take(16));

                        if (GetGDROMBuildInfo(info.Extras.Header, out var gdSerial, out var gdVersion, out var gdDate))
                        {
                            // Ensure internal serial is pulled from local data
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = gdSerial ?? string.Empty;
                            info.VersionAndEditions!.Version = gdVersion ?? string.Empty;
                            info.CommonDiscInfo.EXEDateBuildDate = gdDate ?? string.Empty;
                        }
                    }

                    break;

                case RedumpSystem.SegaNaomi2:
                    if (this.Type == MediaType.CDROM)
                    {
                        info.Extras!.Header = GetSegaHeader($"{basePath}_mainInfo.txt") ?? string.Empty;

                        // Take only the first 16 lines for GD-ROM
                        if (!string.IsNullOrEmpty(info.Extras.Header))
                            info.Extras.Header = string.Join("\n", info.Extras.Header.Split('\n').Take(16));

                        if (GetGDROMBuildInfo(info.Extras.Header, out var gdSerial, out var gdVersion, out var gdDate))
                        {
                            // Ensure internal serial is pulled from local data
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = gdSerial ?? string.Empty;
                            info.VersionAndEditions!.Version = gdVersion ?? string.Empty;
                            info.CommonDiscInfo.EXEDateBuildDate = gdDate ?? string.Empty;
                        }
                    }

                    break;

                case RedumpSystem.SegaSaturn:
                    info.Extras!.Header = GetSegaHeader($"{basePath}_mainInfo.txt") ?? string.Empty;

                    // Take only the first 16 lines for Saturn
                    if (!string.IsNullOrEmpty(info.Extras.Header))
                        info.Extras.Header = string.Join("\n", info.Extras.Header.Split('\n').Take(16));

                    if (GetSaturnBuildInfo(info.Extras.Header, out var saturnSerial, out var saturnVersion, out var buildDate))
                    {
                        // Ensure internal serial is pulled from local data
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = saturnSerial ?? string.Empty;
                        info.VersionAndEditions!.Version = saturnVersion ?? string.Empty;
                        info.CommonDiscInfo.EXEDateBuildDate = buildDate ?? string.Empty;
                    }

                    break;

                case RedumpSystem.SonyPlayStation:
                    if (InfoTool.GetPlayStationExecutableInfo(drive?.Name, out var playstationSerial, out Region? playstationRegion, out var playstationDate))
                    {
                        // Ensure internal serial is pulled from local data
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = playstationSerial ?? string.Empty;
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? playstationRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = playstationDate;
                    }

                    bool? psEdcStatus = null;
                    if (File.Exists($"{basePath}.img_EdcEcc.txt"))
                        psEdcStatus = GetPlayStationEDCStatus($"{basePath}.img_EdcEcc.txt");
                    else if (File.Exists($"{basePath}.img_EccEdc.txt"))
                        psEdcStatus = GetPlayStationEDCStatus($"{basePath}.img_EccEdc.txt");

                    info.EDC!.EDC = psEdcStatus.ToYesNo();
                    info.CopyProtection!.AntiModchip = GetPlayStationAntiModchipDetected($"{basePath}_disc.txt").ToYesNo();
                    break;

                case RedumpSystem.SonyPlayStation2:
                    if (InfoTool.GetPlayStationExecutableInfo(drive?.Name, out var playstationTwoSerial, out Region? playstationTwoRegion, out var playstationTwoDate))
                    {
                        // Ensure internal serial is pulled from local data
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = playstationTwoSerial ?? string.Empty;
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? playstationTwoRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = playstationTwoDate;
                    }

                    info.VersionAndEditions!.Version = InfoTool.GetPlayStation2Version(drive?.Name) ?? string.Empty;
                    break;

                case RedumpSystem.SonyPlayStation3:
                    info.VersionAndEditions!.Version = InfoTool.GetPlayStation3Version(drive?.Name) ?? string.Empty;
                    info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = InfoTool.GetPlayStation3Serial(drive?.Name) ?? string.Empty;
                    string? firmwareVersion = InfoTool.GetPlayStation3FirmwareVersion(drive?.Name);
                    if (firmwareVersion != null)
                        info.CommonDiscInfo!.ContentsSpecialFields![SiteCode.Patches] = $"PS3 Firmware {firmwareVersion}";
                    break;

                case RedumpSystem.SonyPlayStation4:
                    info.VersionAndEditions!.Version = InfoTool.GetPlayStation4Version(drive?.Name) ?? string.Empty;
                    info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = InfoTool.GetPlayStation4Serial(drive?.Name) ?? string.Empty;
                    break;

                case RedumpSystem.SonyPlayStation5:
                    info.VersionAndEditions!.Version = InfoTool.GetPlayStation5Version(drive?.Name) ?? string.Empty;
                    info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = InfoTool.GetPlayStation5Serial(drive?.Name) ?? string.Empty;
                    break;
            }

            // Fill in any artifacts that exist, Base64-encoded, if we need to
            if (includeArtifacts)
            {
                info.Artifacts ??= [];

                //if (File.Exists($"{basePath}.c2"))
                //    info.Artifacts["c2"] = Convert.ToBase64String(File.ReadAllBytes($"{basePath}.c2")) ?? string.Empty;
                if (File.Exists($"{basePath}_c2Error.txt"))
                    info.Artifacts["c2Error"] = GetBase64(GetFullFile($"{basePath}_c2Error.txt")) ?? string.Empty;
                if (File.Exists($"{basePath}.ccd"))
                    info.Artifacts["ccd"] = GetBase64(GetFullFile($"{basePath}.ccd")) ?? string.Empty;
                if (File.Exists($"{basePath}_cmd.txt")) // TODO: Figure out how to read in the timestamp-named file
                    info.Artifacts["cmd"] = GetBase64(GetFullFile($"{basePath}_cmd.txt")) ?? string.Empty;
                if (File.Exists($"{basePath}_CSSKey.txt"))
                    info.Artifacts["csskey"] = GetBase64(GetFullFile($"{basePath}_CSSKey.txt")) ?? string.Empty;
                if (File.Exists($"{basePath}.cue"))
                    info.Artifacts["cue"] = GetBase64(GetFullFile($"{basePath}.cue")) ?? string.Empty;
                if (File.Exists($"{basePath}.dat"))
                    info.Artifacts["dat"] = GetBase64(GetFullFile($"{basePath}.dat")) ?? string.Empty;
                if (File.Exists($"{basePath}_disc.txt"))
                    info.Artifacts["disc"] = GetBase64(GetFullFile($"{basePath}_disc.txt")) ?? string.Empty;
                //if (File.Exists(Path.Combine(outputDirectory, $"{basePath}_DMI.bin")))
                //    info.Artifacts["dmi"] = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(outputDirectory, $"{basePath}_DMI.bin"))) ?? string.Empty;
                if (File.Exists($"{basePath}_drive.txt"))
                    info.Artifacts["drive"] = GetBase64(GetFullFile($"{basePath}_drive.txt")) ?? string.Empty;
                if (File.Exists($"{basePath}_img.cue"))
                    info.Artifacts["img_cue"] = GetBase64(GetFullFile($"{basePath}_img.cue")) ?? string.Empty;
                if (File.Exists($"{basePath}.img_EdcEcc.txt"))
                    info.Artifacts["img_EdcEcc"] = GetBase64(GetFullFile($"{basePath}.img_EdcEcc.txt")) ?? string.Empty;
                if (File.Exists($"{basePath}.img_EccEdc.txt"))
                    info.Artifacts["img_EdcEcc"] = GetBase64(GetFullFile($"{basePath}.img_EccEdc.txt")) ?? string.Empty;
                if (File.Exists($"{basePath}_mainError.txt"))
                    info.Artifacts["mainError"] = GetBase64(GetFullFile($"{basePath}_mainError.txt")) ?? string.Empty;
                if (File.Exists($"{basePath}_mainInfo.txt"))
                    info.Artifacts["mainInfo"] = GetBase64(GetFullFile($"{basePath}_mainInfo.txt")) ?? string.Empty;
                //if (File.Exists($"{basePath}_PFI.bin"))
                //    info.Artifacts["pfi"] = Convert.ToBase64String(File.ReadAllBytes($"{basePath}_PFI.bin")) ?? string.Empty;
                //if (File.Exists($"{basePath}_PIC.bin"))
                //    info.Artifacts["pic"] = Convert.ToBase64String(File.ReadAllBytes($"{basePath}_PIC.bin")) ?? string.Empty;
                //if (File.Exists($"{basePath}_SS.bin"))
                //    info.Artifacts["ss"] = Convert.ToBase64String(File.ReadAllBytes($"{basePath}_SS.bin")) ?? string.Empty;
                if (File.Exists($"{basePath}.sub"))
                    info.Artifacts["sub"] = Convert.ToBase64String(File.ReadAllBytes($"{basePath}.sub")) ?? string.Empty;
                if (File.Exists($"{basePath}_subError.txt"))
                    info.Artifacts["subError"] = GetBase64(GetFullFile($"{basePath}_subError.txt")) ?? string.Empty;
                if (File.Exists($"{basePath}_subInfo.txt"))
                    info.Artifacts["subInfo"] = GetBase64(GetFullFile($"{basePath}_subInfo.txt")) ?? string.Empty;
                if (File.Exists($"{basePath}_subIntention.txt"))
                    info.Artifacts["subIntention"] = GetBase64(GetFullFile($"{basePath}_subIntention.txt")) ?? string.Empty;
                //if (File.Exists($"{basePath}_sub.txt"))
                //    info.Artifacts["subReadable"] = GetBase64(GetFullFile($"{basePath}_sub.txt")) ?? string.Empty;
                //if (File.Exists($"{basePath}_subReadable.txt"))
                //    info.Artifacts["subReadable"] = GetBase64(GetFullFile($"{basePath}_subReadable.txt")) ?? string.Empty;
                if (File.Exists($"{basePath}_volDesc.txt"))
                    info.Artifacts["volDesc"] = GetBase64(GetFullFile($"{basePath}_volDesc.txt")) ?? string.Empty;
            }
        }

        /// <inheritdoc/>
        public override string? GenerateParameters()
        {
            var parameters = new List<string>();

            BaseCommand ??= CommandStrings.NONE;

            if (!string.IsNullOrWhiteSpace(BaseCommand))
                parameters.Add(BaseCommand);
            else
                return null;

            // Drive Letter
            if (BaseCommand == CommandStrings.Audio
                || BaseCommand == CommandStrings.BluRay
                || BaseCommand == CommandStrings.Close
                || BaseCommand == CommandStrings.CompactDisc
                || BaseCommand == CommandStrings.Data
                || BaseCommand == CommandStrings.DigitalVideoDisc
                || BaseCommand == CommandStrings.Disk
                || BaseCommand == CommandStrings.DriveSpeed
                || BaseCommand == CommandStrings.Eject
                || BaseCommand == CommandStrings.Floppy
                || BaseCommand == CommandStrings.GDROM
                || BaseCommand == CommandStrings.Reset
                || BaseCommand == CommandStrings.SACD
                || BaseCommand == CommandStrings.Start
                || BaseCommand == CommandStrings.Stop
                || BaseCommand == CommandStrings.Swap
                || BaseCommand == CommandStrings.XBOX
                || BaseCommand == CommandStrings.XBOXSwap
                || BaseCommand == CommandStrings.XGD2Swap
                || BaseCommand == CommandStrings.XGD3Swap)
            {
                if (DrivePath != null)
                    parameters.Add(DrivePath);
                else
                    return null;
            }

            // Filename
            if (BaseCommand == CommandStrings.Audio
                || BaseCommand == CommandStrings.BluRay
                || BaseCommand == CommandStrings.CompactDisc
                || BaseCommand == CommandStrings.Data
                || BaseCommand == CommandStrings.DigitalVideoDisc
                || BaseCommand == CommandStrings.Disk
                || BaseCommand == CommandStrings.Floppy
                || BaseCommand == CommandStrings.GDROM
                || BaseCommand == CommandStrings.MDS
                || BaseCommand == CommandStrings.Merge
                || BaseCommand == CommandStrings.SACD
                || BaseCommand == CommandStrings.Swap
                || BaseCommand == CommandStrings.Sub
                || BaseCommand == CommandStrings.Tape
                || BaseCommand == CommandStrings.XBOX
                || BaseCommand == CommandStrings.XBOXSwap
                || BaseCommand == CommandStrings.XGD2Swap
                || BaseCommand == CommandStrings.XGD3Swap)
            {
                if (Filename != null)
                    parameters.Add("\"" + Filename.Trim('"') + "\"");
                else
                    return null;
            }

            // Optiarc Filename
            if (BaseCommand == CommandStrings.Merge)
            {
                if (OptiarcFilename != null)
                    parameters.Add("\"" + OptiarcFilename.Trim('"') + "\"");
                else
                    return null;
            }

            // Drive Speed
            if (BaseCommand == CommandStrings.Audio
                || BaseCommand == CommandStrings.BluRay
                || BaseCommand == CommandStrings.CompactDisc
                || BaseCommand == CommandStrings.Data
                || BaseCommand == CommandStrings.DigitalVideoDisc
                || BaseCommand == CommandStrings.GDROM
                || BaseCommand == CommandStrings.SACD
                || BaseCommand == CommandStrings.Swap
                || BaseCommand == CommandStrings.XBOX
                || BaseCommand == CommandStrings.XBOXSwap
                || BaseCommand == CommandStrings.XGD2Swap
                || BaseCommand == CommandStrings.XGD3Swap)
            {
                if (DriveSpeed != null)
                    parameters.Add(DriveSpeed.ToString() ?? string.Empty);
                else
                    return null;
            }

            // LBA Markers
            if (BaseCommand == CommandStrings.Audio
                || BaseCommand == CommandStrings.Data)
            {
                if (StartLBAValue != null && EndLBAValue != null)
                {
                    parameters.Add(StartLBAValue.ToString() ?? string.Empty);
                    parameters.Add(EndLBAValue.ToString() ?? string.Empty);
                }
                else
                    return null;
            }

            // Add Offset
            if (IsFlagSupported(FlagStrings.AddOffset))
            {
                if (this[FlagStrings.AddOffset] == true)
                {
                    parameters.Add(FlagStrings.AddOffset);
                    if (AddOffsetValue != null)
                        parameters.Add(AddOffsetValue.ToString() ?? string.Empty);
                }
            }

            // AMSF Dumping
            if (IsFlagSupported(FlagStrings.AMSF))
            {
                if (this[FlagStrings.AMSF] == true)
                    parameters.Add(FlagStrings.AMSF);
            }

            // Atari Jaguar CD
            if (IsFlagSupported(FlagStrings.AtariJaguar))
            {
                if (this[FlagStrings.AtariJaguar] == true)
                    parameters.Add(FlagStrings.AtariJaguar);
            }

            // BE Opcode
            if (IsFlagSupported(FlagStrings.BEOpcode))
            {
                if (this[FlagStrings.BEOpcode] == true && this[FlagStrings.D8Opcode] != true)
                {
                    parameters.Add(FlagStrings.BEOpcode);
                    if (BEOpcodeValue != null
                        && (BEOpcodeValue == "raw" || BEOpcodeValue == "pack"))
                        parameters.Add(BEOpcodeValue);
                }
            }

            // C2 Opcode
            if (IsFlagSupported(FlagStrings.C2Opcode))
            {
                if (this[FlagStrings.C2Opcode] == true)
                {
                    parameters.Add(FlagStrings.C2Opcode);
                    if (C2OpcodeValue[0] != null)
                    {
                        if (C2OpcodeValue[0] > 0)
                            parameters.Add(C2OpcodeValue[0].ToString() ?? string.Empty);
                        else
                            return null;
                    }
                    if (C2OpcodeValue[1] != null)
                    {
                        parameters.Add(C2OpcodeValue[1].ToString() ?? string.Empty);
                    }
                    if (C2OpcodeValue[2] != null)
                    {
                        if (C2OpcodeValue[2] == 0)
                        {
                            parameters.Add(C2OpcodeValue[2].ToString() ?? string.Empty);
                        }
                        else if (C2OpcodeValue[2] == 1)
                        {
                            parameters.Add(C2OpcodeValue[2].ToString() ?? string.Empty);
                            if (C2OpcodeValue[3] != null && C2OpcodeValue[4] != null)
                            {
                                if (C2OpcodeValue[3] > 0 && C2OpcodeValue[4] > 0)
                                {
                                    parameters.Add(C2OpcodeValue[3].ToString() ?? string.Empty);
                                    parameters.Add(C2OpcodeValue[4].ToString() ?? string.Empty);
                                }
                                else
                                {
                                    return null;
                                }
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }

            // Copyright Management Information
            if (IsFlagSupported(FlagStrings.CopyrightManagementInformation))
            {
                if (this[FlagStrings.CopyrightManagementInformation] == true)
                    parameters.Add(FlagStrings.CopyrightManagementInformation);
            }

            // D8 Opcode
            if (IsFlagSupported(FlagStrings.D8Opcode))
            {
                if (this[FlagStrings.D8Opcode] == true)
                    parameters.Add(FlagStrings.D8Opcode);
            }

            // DAT Expand
            if (IsFlagSupported(FlagStrings.DatExpand))
            {
                if (this[FlagStrings.DatExpand] == true)
                    parameters.Add(FlagStrings.DatExpand);
            }

            // Disable Beep
            if (IsFlagSupported(FlagStrings.DisableBeep))
            {
                if (this[FlagStrings.DisableBeep] == true)
                    parameters.Add(FlagStrings.DisableBeep);
            }

            // DVD/HD-DVD/BD Reread
            if (IsFlagSupported(FlagStrings.DVDReread))
            {
                if (this[FlagStrings.DVDReread] == true)
                {
                    parameters.Add(FlagStrings.DVDReread);
                    if (DVDRereadValue != null)
                        parameters.Add(DVDRereadValue.ToString() ?? string.Empty);
                }
            }

            // Extract MicroSoftCabFile
            if (IsFlagSupported(FlagStrings.ExtractMicroSoftCabFile))
            {
                if (this[FlagStrings.ExtractMicroSoftCabFile] == true)
                    parameters.Add(FlagStrings.ExtractMicroSoftCabFile);
            }

            // Fix
            if (IsFlagSupported(FlagStrings.Fix))
            {
                if (this[FlagStrings.Fix] == true)
                {
                    parameters.Add(FlagStrings.Fix);
                    if (FixValue != null)
                        parameters.Add(FixValue.ToString() ?? string.Empty);
                    else
                        return null;
                }
            }

            // Force Unit Access
            if (IsFlagSupported(FlagStrings.ForceUnitAccess))
            {
                if (this[FlagStrings.ForceUnitAccess] == true)
                {
                    parameters.Add(FlagStrings.ForceUnitAccess);
                    if (ForceUnitAccessValue != null)
                        parameters.Add(ForceUnitAccessValue.ToString() ?? string.Empty);
                }
            }

            // Multi-Sector Read
            if (IsFlagSupported(FlagStrings.MultiSectorRead))
            {
                if (this[FlagStrings.MultiSectorRead] == true)
                {
                    parameters.Add(FlagStrings.MultiSectorRead);
                    if (MultiSectorReadValue != null)
                        parameters.Add(MultiSectorReadValue.ToString() ?? string.Empty);
                }
            }

            // Not fix SubP
            if (IsFlagSupported(FlagStrings.NoFixSubP))
            {
                if (this[FlagStrings.NoFixSubP] == true)
                    parameters.Add(FlagStrings.NoFixSubP);
            }

            // Not fix SubQ
            if (IsFlagSupported(FlagStrings.NoFixSubQ))
            {
                if (this[FlagStrings.NoFixSubQ] == true)
                    parameters.Add(FlagStrings.NoFixSubQ);
            }

            // Not fix SubQ (PlayStation LibCrypt)
            if (IsFlagSupported(FlagStrings.NoFixSubQLibCrypt))
            {
                if (this[FlagStrings.NoFixSubQLibCrypt] == true)
                    parameters.Add(FlagStrings.NoFixSubQLibCrypt);
            }

            // Not fix SubQ (SecuROM)
            if (IsFlagSupported(FlagStrings.NoFixSubQSecuROM))
            {
                if (this[FlagStrings.NoFixSubQSecuROM] == true)
                    parameters.Add(FlagStrings.NoFixSubQSecuROM);
            }

            // Not fix SubRtoW
            if (IsFlagSupported(FlagStrings.NoFixSubRtoW))
            {
                if (this[FlagStrings.NoFixSubRtoW] == true)
                    parameters.Add(FlagStrings.NoFixSubRtoW);
            }

            // Not skip security sectors
            if (IsFlagSupported(FlagStrings.NoSkipSS))
            {
                if (this[FlagStrings.NoSkipSS] == true)
                {
                    parameters.Add(FlagStrings.NoSkipSS);
                    if (NoSkipSecuritySectorValue != null)
                        parameters.Add(NoSkipSecuritySectorValue.ToString() ?? string.Empty);
                }
            }

            // Pad sectors
            if (IsFlagSupported(FlagStrings.PadSector))
            {
                if (this[FlagStrings.PadSector] == true)
                {
                    parameters.Add(FlagStrings.PadSector);
                    if (PadSectorValue != null)
                        parameters.Add(PadSectorValue.ToString() ?? string.Empty);
                }
            }

            // Range
            if (IsFlagSupported(FlagStrings.Range))
            {
                if (this[FlagStrings.Range] == true)
                    parameters.Add(FlagStrings.Range);
            }

            // Raw read (2064 byte/sector)
            if (IsFlagSupported(FlagStrings.Raw))
            {
                if (this[FlagStrings.Raw] == true)
                    parameters.Add(FlagStrings.Raw);
            }

            // Resume
            if (IsFlagSupported(FlagStrings.Resume))
            {
                if (this[FlagStrings.Resume] == true)
                    parameters.Add(FlagStrings.Resume);
            }

            // Reverse read
            if (IsFlagSupported(FlagStrings.Reverse))
            {
                if (this[FlagStrings.Reverse] == true)
                {
                    parameters.Add(FlagStrings.Reverse);

                    if (BaseCommand == CommandStrings.DigitalVideoDisc)
                    {
                        if (ReverseStartLBAValue == null || ReverseEndLBAValue == null)
                            return null;

                        parameters.Add(ReverseStartLBAValue.ToString() ?? string.Empty);
                        parameters.Add(ReverseEndLBAValue.ToString() ?? string.Empty);
                    }
                }
            }

            // Scan PlayStation anti-mod strings
            if (IsFlagSupported(FlagStrings.ScanAntiMod))
            {
                if (this[FlagStrings.ScanAntiMod] == true)
                    parameters.Add(FlagStrings.ScanAntiMod);
            }

            // Scan file to detect protect
            if (IsFlagSupported(FlagStrings.ScanFileProtect))
            {
                if (this[FlagStrings.ScanFileProtect] == true)
                {
                    parameters.Add(FlagStrings.ScanFileProtect);
                    if (ScanFileProtectValue != null)
                    {
                        if (ScanFileProtectValue > 0)
                            parameters.Add(ScanFileProtectValue.ToString() ?? string.Empty);
                        else
                            return null;
                    }
                }
            }

            // Scan file to detect protect
            if (IsFlagSupported(FlagStrings.ScanSectorProtect))
            {
                if (this[FlagStrings.ScanSectorProtect] == true)
                    parameters.Add(FlagStrings.ScanSectorProtect);
            }

            // Scan 74:00:00 (Saturn)
            if (IsFlagSupported(FlagStrings.SeventyFour))
            {
                if (this[FlagStrings.SeventyFour] == true)
                    parameters.Add(FlagStrings.SeventyFour);
            }

            // Skip sectors
            if (IsFlagSupported(FlagStrings.SkipSector))
            {
                if (this[FlagStrings.SkipSector] == true)
                {
                    parameters.Add(FlagStrings.SkipSector);
                    if (SkipSectorValue[0] != null)
                    {
                        if (SkipSectorValue[0] > 0)
                            parameters.Add(SkipSectorValue[0].ToString() ?? string.Empty);
                        else
                            return null;
                    }
                    if (SkipSectorValue[1] != null)
                    {
                        if (SkipSectorValue[1] == 0)
                            parameters.Add(SkipSectorValue[1].ToString() ?? string.Empty);
                    }
                }
            }

            // Set Subchannel read level
            if (IsFlagSupported(FlagStrings.SubchannelReadLevel))
            {
                if (this[FlagStrings.SubchannelReadLevel] == true)
                {
                    parameters.Add(FlagStrings.SubchannelReadLevel);
                    if (SubchannelReadLevelValue != null)
                    {
                        if (SubchannelReadLevelValue >= 0 && SubchannelReadLevelValue <= 2)
                            parameters.Add(SubchannelReadLevelValue.ToString() ?? string.Empty);
                        else
                            return null;
                    }
                }
            }

            // Use Anchor Volume Descriptor Pointer
            if (IsFlagSupported(FlagStrings.UseAnchorVolumeDescriptorPointer))
            {
                if (this[FlagStrings.UseAnchorVolumeDescriptorPointer] == true)
                    parameters.Add(FlagStrings.UseAnchorVolumeDescriptorPointer);
            }

            // VideoNow
            if (IsFlagSupported(FlagStrings.VideoNow))
            {
                if (this[FlagStrings.VideoNow] == true)
                {
                    parameters.Add(FlagStrings.VideoNow);
                    if (VideoNowValue != null)
                    {
                        if (VideoNowValue >= 0)
                            parameters.Add(VideoNowValue.ToString() ?? string.Empty);
                        else
                            return null;
                    }
                }
            }

            // VideoNow Color
            if (IsFlagSupported(FlagStrings.VideoNowColor))
            {
                if (this[FlagStrings.VideoNowColor] == true)
                    parameters.Add(FlagStrings.VideoNowColor);
            }

            // VideoNowXP
            if (IsFlagSupported(FlagStrings.VideoNowXP))
            {
                if (this[FlagStrings.VideoNowXP] == true)
                    parameters.Add(FlagStrings.VideoNowXP);
            }

            return string.Join(" ", parameters);
        }

        /// <inheritdoc/>
        public override Dictionary<string, List<string>> GetCommandSupport()
        {
            return new Dictionary<string, List<string>>()
            {
                [CommandStrings.Audio] =
                [
                    FlagStrings.BEOpcode,
                    FlagStrings.C2Opcode,
                    FlagStrings.D8Opcode,
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                    FlagStrings.ForceUnitAccess,
                    FlagStrings.NoFixSubP,
                    FlagStrings.NoFixSubQ,
                    FlagStrings.NoFixSubRtoW,
                    FlagStrings.Reverse,
                    FlagStrings.ScanAntiMod,
                    FlagStrings.ScanFileProtect,
                    FlagStrings.ScanSectorProtect,
                    FlagStrings.SkipSector,
                    FlagStrings.SubchannelReadLevel,
                ],

                [CommandStrings.BluRay] =
                [
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                    FlagStrings.DVDReread,
                    FlagStrings.ForceUnitAccess,
                    FlagStrings.UseAnchorVolumeDescriptorPointer,
                ],

                [CommandStrings.Close] = [],

                [CommandStrings.CompactDisc] =
                [
                    FlagStrings.AddOffset,
                    FlagStrings.AMSF,
                    FlagStrings.AtariJaguar,
                    FlagStrings.BEOpcode,
                    FlagStrings.C2Opcode,
                    FlagStrings.D8Opcode,
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                    FlagStrings.ExtractMicroSoftCabFile,
                    FlagStrings.ForceUnitAccess,
                    FlagStrings.MultiSectorRead,
                    FlagStrings.NoFixSubP,
                    FlagStrings.NoFixSubQ,
                    FlagStrings.NoFixSubQLibCrypt,
                    FlagStrings.NoFixSubQSecuROM,
                    FlagStrings.NoFixSubRtoW,
                    FlagStrings.ScanAntiMod,
                    FlagStrings.ScanFileProtect,
                    FlagStrings.ScanSectorProtect,
                    FlagStrings.SeventyFour,
                    FlagStrings.SubchannelReadLevel,
                    FlagStrings.VideoNow,
                    FlagStrings.VideoNowColor,
                    FlagStrings.VideoNowXP,
                ],

                [CommandStrings.Data] =
                [
                    FlagStrings.BEOpcode,
                    FlagStrings.C2Opcode,
                    FlagStrings.D8Opcode,
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                    FlagStrings.ForceUnitAccess,
                    FlagStrings.NoFixSubP,
                    FlagStrings.NoFixSubQ,
                    FlagStrings.NoFixSubRtoW,
                    FlagStrings.Reverse,
                    FlagStrings.ScanAntiMod,
                    FlagStrings.ScanFileProtect,
                    FlagStrings.ScanSectorProtect,
                    FlagStrings.SkipSector,
                    FlagStrings.SubchannelReadLevel,
                ],

                [CommandStrings.DigitalVideoDisc] =
                [
                    FlagStrings.CopyrightManagementInformation,
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                    FlagStrings.DVDReread,
                    FlagStrings.Fix,
                    FlagStrings.ForceUnitAccess,
                    FlagStrings.PadSector,
                    FlagStrings.Range,
                    FlagStrings.Raw,
                    FlagStrings.Resume,
                    FlagStrings.Reverse,
                    FlagStrings.ScanFileProtect,
                    FlagStrings.SkipSector,
                    FlagStrings.UseAnchorVolumeDescriptorPointer,
                ],

                [CommandStrings.Disk] =
                [
                    FlagStrings.DatExpand,
                ],

                [CommandStrings.DriveSpeed] = [],

                [CommandStrings.Eject] = [],

                [CommandStrings.Floppy] =
                [
                    FlagStrings.DatExpand,
                ],

                [CommandStrings.GDROM] =
                [
                    FlagStrings.BEOpcode,
                    FlagStrings.C2Opcode,
                    FlagStrings.D8Opcode,
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                    FlagStrings.ForceUnitAccess,
                    FlagStrings.NoFixSubP,
                    FlagStrings.NoFixSubQ,
                    FlagStrings.NoFixSubRtoW,
                    FlagStrings.SubchannelReadLevel,
                ],

                [CommandStrings.MDS] = [],

                [CommandStrings.Merge] = [],

                [CommandStrings.Reset] = [],

                [CommandStrings.SACD] =
                [
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                ],

                [CommandStrings.Start] = [],

                [CommandStrings.Stop] = [],

                [CommandStrings.Sub] = [],

                [CommandStrings.Swap] =
                [
                    FlagStrings.AddOffset,
                    FlagStrings.BEOpcode,
                    FlagStrings.C2Opcode,
                    FlagStrings.D8Opcode,
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                    FlagStrings.ForceUnitAccess,
                    FlagStrings.NoFixSubP,
                    FlagStrings.NoFixSubQ,
                    FlagStrings.NoFixSubQLibCrypt,
                    FlagStrings.NoFixSubQSecuROM,
                    FlagStrings.NoFixSubRtoW,
                    FlagStrings.ScanAntiMod,
                    FlagStrings.ScanFileProtect,
                    FlagStrings.ScanSectorProtect,
                    FlagStrings.SeventyFour,
                    FlagStrings.SubchannelReadLevel,
                    FlagStrings.VideoNow,
                    FlagStrings.VideoNowColor,
                    FlagStrings.VideoNowXP,
                ],

                [CommandStrings.Tape] = [],

                [CommandStrings.Version] = [],

                [CommandStrings.XBOX] =
                [
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                    FlagStrings.DVDReread,
                    FlagStrings.ForceUnitAccess,
                    FlagStrings.NoSkipSS,
                ],

                [CommandStrings.XBOXSwap] =
                [
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                    FlagStrings.ForceUnitAccess,
                    FlagStrings.NoSkipSS,
                ],

                [CommandStrings.XGD2Swap] =
                [
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                    FlagStrings.ForceUnitAccess,
                    FlagStrings.NoSkipSS,
                ],

                [CommandStrings.XGD3Swap] =
                [
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                    FlagStrings.ForceUnitAccess,
                    FlagStrings.NoSkipSS,
                ],
            };
        }

        /// <inheritdoc/>
        public override string? GetDefaultExtension(MediaType? mediaType) => Converters.Extension(mediaType);

        /// <inheritdoc/>
        public override List<string> GetDeleteableFilePaths(string basePath)
        {
            var deleteableFiles = new List<string>();
            switch (this.Type)
            {
                case MediaType.CDROM:
                case MediaType.GDROM:
                    if (File.Exists($"{basePath}.img"))
                        deleteableFiles.Add($"{basePath}.img");
                    if (File.Exists($"{basePath} (Track 0).img"))
                        deleteableFiles.Add($"{basePath} (Track 0).img");
                    if (File.Exists($"{basePath} (Track 00).img"))
                        deleteableFiles.Add($"{basePath} (Track 00).img");
                    if (File.Exists($"{basePath} (Track 1)(-LBA).img"))
                        deleteableFiles.Add($"{basePath} (Track 1)(-LBA).img");
                    if (File.Exists($"{basePath} (Track 01)(-LBA).img"))
                        deleteableFiles.Add($"{basePath} (Track 01)(-LBA).img");
                    if (File.Exists($"{basePath} (Track AA).img"))
                        deleteableFiles.Add($"{basePath} (Track AA).img");

                    if (File.Exists($"{basePath}.scm"))
                        deleteableFiles.Add($"{basePath}.scm");
                    if (File.Exists($"{basePath} (Track 0).scm"))
                        deleteableFiles.Add($"{basePath} (Track 0).scm");
                    if (File.Exists($"{basePath} (Track 00).scm"))
                        deleteableFiles.Add($"{basePath} (Track 00).scm");
                    if (File.Exists($"{basePath} (Track 1)(-LBA).scm"))
                        deleteableFiles.Add($"{basePath} (Track 1)(-LBA).scm");
                    if (File.Exists($"{basePath} (Track 01)(-LBA).scm"))
                        deleteableFiles.Add($"{basePath} (Track 01)(-LBA).scm");
                    if (File.Exists($"{basePath} (Track AA).scm"))
                        deleteableFiles.Add($"{basePath} (Track AA).scm");

                    break;

                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    if (File.Exists($"{basePath}.raw"))
                        deleteableFiles.Add($"{basePath}.raw");

                    break;
            }

            return deleteableFiles;
        }

        /// <inheritdoc/>
        public override List<string> GetLogFilePaths(string basePath)
        {
            (var cmdPath, _) = GetCommandFilePathAndVersion(basePath);

            var logFiles = new List<string>();
            switch (this.Type)
            {
                case MediaType.CDROM:
                case MediaType.GDROM:
                    if (File.Exists($"{basePath}.c2"))
                        logFiles.Add($"{basePath}.c2");
                    if (File.Exists($"{basePath}_c2Error.txt"))
                        logFiles.Add($"{basePath}_c2Error.txt");
                    if (File.Exists($"{basePath}.ccd"))
                        logFiles.Add($"{basePath}.ccd");
                    if (cmdPath != null && File.Exists(cmdPath))
                        logFiles.Add(cmdPath);
                    if (File.Exists($"{basePath}_cmd.txt"))
                        logFiles.Add($"{basePath}_cmd.txt");
                    if (File.Exists($"{basePath}.dat"))
                        logFiles.Add($"{basePath}.dat");
                    if (File.Exists($"{basePath}.sub"))
                        logFiles.Add($"{basePath}.sub");
                    if (File.Exists($"{basePath} (Track 0).sub"))
                        logFiles.Add($"{basePath} (Track 0).sub");
                    if (File.Exists($"{basePath} (Track 00).sub"))
                        logFiles.Add($"{basePath} (Track 00).sub");
                    if (File.Exists($"{basePath} (Track 1)(-LBA).sub"))
                        logFiles.Add($"{basePath} (Track 1)(-LBA).sub");
                    if (File.Exists($"{basePath} (Track 01)(-LBA).sub"))
                        logFiles.Add($"{basePath} (Track 01)(-LBA).sub");
                    if (File.Exists($"{basePath} (Track AA).sub"))
                        logFiles.Add($"{basePath} (Track AA).sub");
                    if (File.Exists($"{basePath}.subtmp"))
                        logFiles.Add($"{basePath}.subtmp");
                    if (File.Exists($"{basePath}.toc"))
                        logFiles.Add($"{basePath}.toc");
                    if (File.Exists($"{basePath}_disc.txt"))
                        logFiles.Add($"{basePath}_disc.txt");
                    if (File.Exists($"{basePath}_drive.txt"))
                        logFiles.Add($"{basePath}_drive.txt");
                    if (File.Exists($"{basePath}_img.cue"))
                        logFiles.Add($"{basePath}_img.cue");
                    if (File.Exists($"{basePath}.img_EdcEcc.txt"))
                        logFiles.Add($"{basePath}.img_EdcEcc.txt");
                    if (File.Exists($"{basePath}.img_EccEdc.txt"))
                        logFiles.Add($"{basePath}.img_EccEdc.txt");
                    if (File.Exists($"{basePath}_mainError.txt"))
                        logFiles.Add($"{basePath}_mainError.txt");
                    if (File.Exists($"{basePath}_mainInfo.txt"))
                        logFiles.Add($"{basePath}_mainInfo.txt");
                    if (File.Exists($"{basePath}_sub.txt"))
                        logFiles.Add($"{basePath}_sub.txt");
                    if (File.Exists($"{basePath}_subError.txt"))
                        logFiles.Add($"{basePath}_subError.txt");
                    if (File.Exists($"{basePath}_subInfo.txt"))
                        logFiles.Add($"{basePath}_subInfo.txt");
                    if (File.Exists($"{basePath}_subIntention.txt"))
                        logFiles.Add($"{basePath}_subIntention.txt");
                    if (File.Exists($"{basePath}_subReadable.txt"))
                        logFiles.Add($"{basePath}_subReadable.txt");
                    if (File.Exists($"{basePath}_suppl.dat"))
                        logFiles.Add($"{basePath}_suppl.dat");
                    if (File.Exists($"{basePath}_volDesc.txt"))
                        logFiles.Add($"{basePath}_volDesc.txt");

                    break;

                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    if (cmdPath != null && File.Exists(cmdPath))
                        logFiles.Add(cmdPath);
                    if (File.Exists($"{basePath}_cmd.txt"))
                        logFiles.Add($"{basePath}_cmd.txt");
                    if (File.Exists($"{basePath}_CSSKey.txt"))
                        logFiles.Add($"{basePath}_CSSKey.txt");
                    if (File.Exists($"{basePath}.dat"))
                        logFiles.Add($"{basePath}.dat");
                    if (File.Exists($"{basePath}.toc"))
                        logFiles.Add($"{basePath}.toc");
                    if (File.Exists($"{basePath}_disc.txt"))
                        logFiles.Add($"{basePath}_disc.txt");
                    if (File.Exists($"{basePath}_drive.txt"))
                        logFiles.Add($"{basePath}_drive.txt");
                    if (File.Exists($"{basePath}_mainError.txt"))
                        logFiles.Add($"{basePath}_mainError.txt");
                    if (File.Exists($"{basePath}_mainInfo.txt"))
                        logFiles.Add($"{basePath}_mainInfo.txt");
                    if (File.Exists($"{basePath}_suppl.dat"))
                        logFiles.Add($"{basePath}_suppl.dat");
                    if (File.Exists($"{basePath}_volDesc.txt"))
                        logFiles.Add($"{basePath}_volDesc.txt");

                    if (File.Exists($"{basePath}_DMI.bin"))
                        logFiles.Add($"{basePath}_DMI.bin");
                    if (File.Exists($"{basePath}_PFI.bin"))
                        logFiles.Add($"{basePath}_PFI.bin");
                    if (File.Exists($"{basePath}_PIC.bin"))
                        logFiles.Add($"{basePath}_PIC.bin");
                    if (File.Exists($"{basePath}_SS.bin"))
                        logFiles.Add($"{basePath}_SS.bin");

                    break;

                case MediaType.FloppyDisk:
                case MediaType.HardDisk:
                    // TODO: Determine what outputs come out from a HDD, SD, etc.
                    if (cmdPath != null && File.Exists(cmdPath))
                        logFiles.Add(cmdPath);
                    if (File.Exists($"{basePath}_cmd.txt"))
                        logFiles.Add($"{basePath}_cmd.txt");
                    if (File.Exists($"{basePath}.dat"))
                        logFiles.Add($"{basePath}.dat");
                    if (File.Exists($"{basePath}_disc.txt"))
                        logFiles.Add($"{basePath}_disc.txt");

                    break;
            }

            return logFiles;
        }

        /// <inheritdoc/>
        public override MediaType? GetMediaType() => Converters.ToMediaType(BaseCommand);

        /// <inheritdoc/>
        public override bool IsDumpingCommand()
        {
            return BaseCommand switch
            {
                CommandStrings.Audio
                    or CommandStrings.BluRay
                    or CommandStrings.CompactDisc
                    or CommandStrings.Data
                    or CommandStrings.DigitalVideoDisc
                    or CommandStrings.Disk
                    or CommandStrings.Floppy
                    or CommandStrings.GDROM
                    or CommandStrings.SACD
                    or CommandStrings.Swap
                    or CommandStrings.Tape
                    or CommandStrings.XBOX
                    or CommandStrings.XBOXSwap
                    or CommandStrings.XGD2Swap
                    or CommandStrings.XGD3Swap => true,
                _ => false,
            };
        }

        /// <inheritdoc/>
        protected override void ResetValues()
        {
            BaseCommand = CommandStrings.NONE;

            DrivePath = null;
            DriveSpeed = null;

            Filename = null;

            StartLBAValue = null;
            EndLBAValue = null;

            flags = [];

            AddOffsetValue = null;
            BEOpcodeValue = null;
            C2OpcodeValue = new int?[5];
            DVDRereadValue = null;
            FixValue = null;
            ForceUnitAccessValue = null;
            NoSkipSecuritySectorValue = null;
            ScanFileProtectValue = null;
            SubchannelReadLevelValue = null;
            VideoNowValue = null;
        }

        /// <inheritdoc/>
        protected override void SetDefaultParameters(string? drivePath, string filename, int? driveSpeed, Options options)
        {
            SetBaseCommand(this.System, this.Type);

            DrivePath = drivePath;
            DriveSpeed = driveSpeed;
            Filename = filename;

            // First check to see if the combination of system and MediaType is valid
            var validTypes = this.System.MediaTypes();
            if (!validTypes.Contains(this.Type))
                return;

            // Set disable beep flag, if needed
            if (options.DICQuietMode)
                this[FlagStrings.DisableBeep] = true;

            // Set the C2 reread count
            C2OpcodeValue[0] = options.DICRereadCount switch
            {
                -1 => null,
                0 => 20,
                _ => options.DICRereadCount,
            };

            // Set the DVD/HD-DVD/BD reread count
            DVDRereadValue = options.DICDVDRereadCount switch
            {
                -1 => null,
                0 => 10,
                _ => options.DICDVDRereadCount,
            };

            // Now sort based on disc type
            switch (this.Type)
            {
                case MediaType.CDROM:
                    this[FlagStrings.C2Opcode] = true;
                    this[FlagStrings.MultiSectorRead] = options.DICMultiSectorRead;
                    if (options.DICMultiSectorRead)
                        this.MultiSectorReadValue = options.DICMultiSectorReadValue;

                    switch (this.System)
                    {
                        case RedumpSystem.AppleMacintosh:
                        case RedumpSystem.IBMPCcompatible:
                            this[FlagStrings.NoFixSubQSecuROM] = true;
                            this[FlagStrings.ScanFileProtect] = true;
                            this[FlagStrings.ScanSectorProtect] = options.DICParanoidMode;
                            this[FlagStrings.SubchannelReadLevel] = options.DICParanoidMode;
                            if (this[FlagStrings.SubchannelReadLevel] == true)
                                SubchannelReadLevelValue = 2;

                            break;
                        case RedumpSystem.AtariJaguarCDInteractiveMultimediaSystem:
                            this[FlagStrings.AtariJaguar] = true;
                            break;
                        case RedumpSystem.HasbroVideoNow:
                        case RedumpSystem.HasbroVideoNowColor:
                        case RedumpSystem.HasbroVideoNowJr:
                        case RedumpSystem.HasbroVideoNowXP:
                            this[FlagStrings.AddOffset] = true;
                            this.AddOffsetValue = 0; // Value needed for first run and placeholder after
                            break;
                        case RedumpSystem.SonyPlayStation:
                            this[FlagStrings.ScanAntiMod] = true;
                            this[FlagStrings.NoFixSubQLibCrypt] = true;
                            break;
                    }
                    break;
                case MediaType.DVD:
                    this[FlagStrings.CopyrightManagementInformation] = options.DICUseCMIFlag;
                    this[FlagStrings.ScanFileProtect] = options.DICParanoidMode;
                    this[FlagStrings.DVDReread] = true;
                    break;
                case MediaType.GDROM:
                    this[FlagStrings.C2Opcode] = true;
                    break;
                case MediaType.HDDVD:
                    this[FlagStrings.CopyrightManagementInformation] = options.DICUseCMIFlag;
                    this[FlagStrings.DVDReread] = true;
                    break;
                case MediaType.BluRay:
                    this[FlagStrings.DVDReread] = true;
                    break;

                // Special Formats
                case MediaType.NintendoGameCubeGameDisc:
                    this[FlagStrings.Raw] = true;
                    break;
                case MediaType.NintendoWiiOpticalDisc:
                    this[FlagStrings.Raw] = true;
                    break;

                // Non-optical
                case MediaType.FloppyDisk:
                    // Currently no defaults set
                    break;
            }
        }

        /// <inheritdoc/>
        protected override bool ValidateAndSetParameters(string? parameters)
        {
            BaseCommand = CommandStrings.NONE;

            // The string has to be valid by itself first
            if (string.IsNullOrWhiteSpace(parameters))
                return false;

            // Now split the string into parts for easier validation
            // https://stackoverflow.com/questions/14655023/split-a-string-that-has-white-spaces-unless-they-are-enclosed-within-quotes
            parameters = parameters!.Trim();
            List<string> parts = Regex.Matches(parameters, @"[\""].+?[\""]|[^ ]+", RegexOptions.Compiled)
                .Cast<Match>()
                .Select(m => m.Value)
                .ToList();

            // Determine what the commandline should look like given the first item
            BaseCommand = parts[0];

            // Loop through ordered command-specific flags
            int index = -1;
            switch (BaseCommand)
            {
                case CommandStrings.Audio:
                    if (parts.Count < 6)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    if (!IsValidInt32(parts[4]))
                        return false;
                    else
                        StartLBAValue = Int32.Parse(parts[4]);

                    if (!IsValidInt32(parts[5]))
                        return false;
                    else
                        EndLBAValue = Int32.Parse(parts[5]);

                    index = 6;
                    break;

                case CommandStrings.BluRay:
                    if (parts.Count < 4)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    index = 4;
                    break;

                case CommandStrings.Close:
                    if (parts.Count != 2)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    break;

                case CommandStrings.CompactDisc:
                    if (parts.Count < 4)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    index = 4;
                    break;

                case CommandStrings.Data:
                    if (parts.Count < 6)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    if (!IsValidInt32(parts[4]))
                        return false;
                    else
                        StartLBAValue = Int32.Parse(parts[4]);

                    if (!IsValidInt32(parts[5]))
                        return false;
                    else
                        EndLBAValue = Int32.Parse(parts[5]);

                    index = 6;
                    break;

                case CommandStrings.DigitalVideoDisc:
                    if (parts.Count < 4)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!IsValidInt32(parts[3], lowerBound: 0, upperBound: 24)) // Officially 0-16
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    index = 4;
                    break;

                case CommandStrings.Disk:
                    if (parts.Count != 3)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    break;

                case CommandStrings.DriveSpeed:
                    if (parts.Count != 2)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    break;

                case CommandStrings.Eject:
                    if (parts.Count != 2)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    break;

                case CommandStrings.Floppy:
                    if (parts.Count != 3)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    break;

                case CommandStrings.GDROM:
                    if (parts.Count < 4)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    index = 4;
                    break;

                case CommandStrings.MDS:
                    if (parts.Count != 2)
                        return false;

                    if (IsFlagSupported(parts[1]) || !File.Exists(parts[1]))
                        return false;
                    else
                        Filename = parts[1];

                    break;

                case CommandStrings.Merge:
                    if (parts.Count != 3)
                        return false;

                    if (IsFlagSupported(parts[1]) || !File.Exists(parts[1]))
                        return false;
                    else
                        Filename = parts[1];

                    if (IsFlagSupported(parts[2]) || !File.Exists(parts[2]))
                        return false;
                    else
                        OptiarcFilename = parts[2];

                    break;

                case CommandStrings.Reset:
                    if (parts.Count != 2)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    break;

                case CommandStrings.SACD:
                    if (parts.Count < 4)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!IsValidInt32(parts[3], lowerBound: 0, upperBound: 16))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    index = 4;
                    break;

                case CommandStrings.Start:
                    if (parts.Count != 2)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    break;

                case CommandStrings.Stop:
                    if (parts.Count != 2)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    break;

                case CommandStrings.Sub:
                    if (parts.Count != 2)
                        return false;

                    if (IsFlagSupported(parts[1]) || !File.Exists(parts[1]))
                        return false;
                    else
                        Filename = parts[1];

                    break;

                case CommandStrings.Swap:
                    if (parts.Count < 4)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    index = 4;
                    break;

                case CommandStrings.Tape:
                    if (parts.Count != 2)
                        return false;

                    if (IsFlagSupported(parts[1]) || !File.Exists(parts[1]))
                        return false;
                    else
                        Filename = parts[1];

                    break;

                case CommandStrings.Version:
                    if (parts.Count != 1)
                        return false;

                    break;

                case CommandStrings.XBOX:
                    if (parts.Count < 4)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    index = 4;
                    break;

                case CommandStrings.XBOXSwap:
                case CommandStrings.XGD2Swap:
                case CommandStrings.XGD3Swap:
                    if (parts.Count < 4)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    for (int i = 4; i < parts.Count; i++)
                    {
                        if (!Int64.TryParse(parts[i], out long temp))
                            return false;
                    }

                    break;
                default:
                    return false;
            }

            // Loop through all auxiliary flags, if necessary
            if (index > 0)
            {
                for (int i = index; i < parts.Count; i++)
                {
                    // Flag read-out values
                    byte? byteValue = null;
                    int? intValue = null;
                    string? stringValue = null;

                    // Add Offset
                    intValue = ProcessInt32Parameter(parts, FlagStrings.AddOffset, ref i, missingAllowed: true);
                    if (intValue != null && intValue != Int32.MinValue)
                        AddOffsetValue = intValue;

                    // AMSF
                    ProcessFlagParameter(parts, FlagStrings.AMSF, ref i);

                    // Atari Jaguar
                    ProcessFlagParameter(parts, FlagStrings.AtariJaguar, ref i);

                    // BE Opcode
                    stringValue = ProcessStringParameter(parts, FlagStrings.BEOpcode, ref i);
                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        if (string.Equals(stringValue, "raw") || string.Equals(stringValue, "pack"))
                            BEOpcodeValue = stringValue;
                        else
                            i--;
                    }

                    // C2 Opcode
                    if (parts[i] == FlagStrings.C2Opcode && IsFlagSupported(FlagStrings.C2Opcode))
                    {
                        this[FlagStrings.C2Opcode] = true;
                        for (int j = 0; j < 5; j++)
                        {
                            if (!DoesExist(parts, i + 1))
                            {
                                break;
                            }
                            else if (IsFlagSupported(parts[i + 1]))
                            {
                                break;
                            }
                            else if (!IsValidInt32(parts[i + 1], lowerBound: 0))
                            {
                                return false;
                            }
                            else
                            {
                                C2OpcodeValue[j] = Int32.Parse(parts[i + 1]);
                                i++;
                            }
                        }
                    }

                    // Copyright Management Information
                    ProcessFlagParameter(parts, FlagStrings.CopyrightManagementInformation, ref i);

                    // D8 Opcode
                    ProcessFlagParameter(parts, FlagStrings.D8Opcode, ref i);

                    // DAT Expand
                    ProcessFlagParameter(parts, FlagStrings.DatExpand, ref i);

                    // Disable Beep
                    ProcessFlagParameter(parts, FlagStrings.DisableBeep, ref i);

                    // DVD/HD-DVD/BD Reread
                    intValue = ProcessInt32Parameter(parts, FlagStrings.DVDReread, ref i, missingAllowed: true);
                    if (intValue != null && intValue != Int32.MinValue)
                        DVDRereadValue = intValue;

                    // Extract MS-CAB
                    ProcessFlagParameter(parts, FlagStrings.ExtractMicroSoftCabFile, ref i);

                    // Fix
                    intValue = ProcessInt32Parameter(parts, FlagStrings.Fix, ref i);
                    if (intValue != null && intValue != Int32.MinValue)
                        FixValue = intValue;

                    // Force Unit Access
                    intValue = ProcessInt32Parameter(parts, FlagStrings.ForceUnitAccess, ref i, missingAllowed: true);
                    if (intValue != null && intValue != Int32.MinValue && intValue >= 0)
                        ForceUnitAccessValue = intValue;

                    // Multi-Sector Read
                    intValue = ProcessInt32Parameter(parts, FlagStrings.MultiSectorRead, ref i, missingAllowed: true);
                    if (intValue != null && intValue != Int32.MinValue && intValue >= 0)
                        MultiSectorReadValue = intValue;

                    // NoFixSubP
                    ProcessFlagParameter(parts, FlagStrings.NoFixSubP, ref i);

                    // NoFixSubQ
                    ProcessFlagParameter(parts, FlagStrings.NoFixSubQ, ref i);

                    // NoFixSubQLibCrypt
                    ProcessFlagParameter(parts, FlagStrings.NoFixSubQLibCrypt, ref i);

                    // NoFixSubQSecuROM
                    ProcessFlagParameter(parts, FlagStrings.NoFixSubQSecuROM, ref i);

                    // NoFixSubRtoW
                    ProcessFlagParameter(parts, FlagStrings.NoFixSubRtoW, ref i);

                    // NoSkipSS
                    intValue = ProcessInt32Parameter(parts, FlagStrings.NoSkipSS, ref i, missingAllowed: true);
                    if (intValue != null && intValue != Int32.MinValue && intValue >= 0)
                        NoSkipSecuritySectorValue = intValue;

                    // PadSector
                    byteValue = ProcessUInt8Parameter(parts, FlagStrings.PadSector, ref i, missingAllowed: true);
                    if (byteValue != null)
                        PadSectorValue = byteValue;

                    // Raw
                    ProcessFlagParameter(parts, FlagStrings.Raw, ref i);

                    // Resume
                    ProcessFlagParameter(parts, FlagStrings.Resume, ref i);

                    // Reverse
                    if (parts[i] == FlagStrings.Reverse && IsFlagSupported(FlagStrings.Reverse))
                    {
                        // DVD specifically requires StartLBA and EndLBA
                        if (BaseCommand == CommandStrings.DigitalVideoDisc)
                        {
                            if (!DoesExist(parts, i + 1) || !DoesExist(parts, i + 2))
                                return false;
                            else if (!IsValidInt32(parts[i + 1], lowerBound: 0) || !IsValidInt32(parts[i + 2], lowerBound: 0))
                                return false;

                            ReverseStartLBAValue = Int32.Parse(parts[i + 1]);
                            ReverseEndLBAValue = Int32.Parse(parts[i + 2]);
                            i += 2;
                        }

                        this[FlagStrings.Reverse] = true;
                    }

                    // ScanAntiMod
                    ProcessFlagParameter(parts, FlagStrings.ScanAntiMod, ref i);

                    // ScanFileProtect
                    intValue = ProcessInt32Parameter(parts, FlagStrings.ScanFileProtect, ref i, missingAllowed: true);
                    if (intValue != null && intValue != Int32.MinValue && intValue >= 0)
                        ScanFileProtectValue = intValue;

                    // ScanSectorProtect
                    ProcessFlagParameter(parts, FlagStrings.ScanSectorProtect, ref i);

                    // SeventyFour
                    ProcessFlagParameter(parts, FlagStrings.SeventyFour, ref i);

                    // SkipSector
                    if (parts[i] == FlagStrings.SkipSector && IsFlagSupported(FlagStrings.SkipSector))
                    {
                        bool stillValid = true;
                        for (int j = 0; j < 2; j++)
                        {
                            if (!DoesExist(parts, i + 1))
                            {
                                break;
                            }
                            else if (IsFlagSupported(parts[i + 1]))
                            {
                                break;
                            }
                            else if (!IsValidInt32(parts[i + 1], lowerBound: 0))
                            {
                                stillValid = false;
                                break;
                            }
                            else
                            {
                                SkipSectorValue[j] = Int32.Parse(parts[i + 1]);
                                i++;
                            }
                        }

                        if (stillValid)
                            this[FlagStrings.SkipSector] = true;
                    }

                    // SubchannelReadLevel
                    intValue = ProcessInt32Parameter(parts, FlagStrings.SubchannelReadLevel, ref i, missingAllowed: true);
                    if (intValue != null && intValue != Int32.MinValue && intValue >= 0 && intValue <= 2)
                        SubchannelReadLevelValue = intValue;

                    // SeventyFour
                    ProcessFlagParameter(parts, FlagStrings.UseAnchorVolumeDescriptorPointer, ref i);

                    // VideoNow
                    intValue = ProcessInt32Parameter(parts, FlagStrings.VideoNow, ref i, missingAllowed: true);
                    if (intValue != null && intValue != Int32.MinValue && intValue >= 0)
                        VideoNowValue = intValue;

                    // VideoNowColor
                    ProcessFlagParameter(parts, FlagStrings.VideoNowColor, ref i);

                    // VideoNowXP
                    ProcessFlagParameter(parts, FlagStrings.VideoNowXP, ref i);
                }
            }

            return true;
        }

        #endregion

        #region Private Extra Methods

        /// <summary>
        /// Get the command file path and extract the version from it
        /// </summary>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <returns>Tuple of file path and version as strings, both null on error</returns>
        private static (string?, string?) GetCommandFilePathAndVersion(string basePath)
        {
            // If we have an invalid base path, we can do nothing
            if (string.IsNullOrWhiteSpace(basePath))
                return (null, null);

            // Generate the matching regex based on the base path
            string basePathFileName = Path.GetFileName(basePath);
            var cmdFilenameRegex = new Regex(Regex.Escape(basePathFileName) + @"_(\d{8})T\d{6}\.txt");

            // Find the first match for the command file
            var parentDirectory = Path.GetDirectoryName(basePath);
            if (string.IsNullOrWhiteSpace(parentDirectory))
                return (null, null);

            var currentFiles = Directory.GetFiles(parentDirectory);
            var commandPath = currentFiles.FirstOrDefault(f => cmdFilenameRegex.IsMatch(f));
            if (string.IsNullOrWhiteSpace(commandPath))
                return (null, null);

            // Extract the version string
            var match = cmdFilenameRegex.Match(commandPath);
            string version = match.Groups[1].Value;
            return (commandPath, version);
        }

        /// <summary>
        /// Set the DIC command to be used for a given system and media type
        /// </summary>
        /// <param name="system">RedumpSystem value to check</param>
        /// <param name="type">MediaType value to check</param>
        private void SetBaseCommand(RedumpSystem? system, MediaType? type)
        {
            // If we have an invalid combination, we should BaseCommand = null
            if (!system.MediaTypes().Contains(type))
            {
                BaseCommand = null;
                return;
            }

            switch (type)
            {
                case MediaType.CDROM:
                    if (system == RedumpSystem.SuperAudioCD)
                        BaseCommand = CommandStrings.SACD;
                    else
                        BaseCommand = CommandStrings.CompactDisc;
                    return;
                case MediaType.DVD:
                    if (system == RedumpSystem.MicrosoftXbox
                        || system == RedumpSystem.MicrosoftXbox360)
                    {
                        BaseCommand = CommandStrings.XBOX;
                        return;
                    }
                    BaseCommand = CommandStrings.DigitalVideoDisc;
                    return;
                case MediaType.GDROM:
                    BaseCommand = CommandStrings.GDROM;
                    return;
                case MediaType.HDDVD:
                    BaseCommand = CommandStrings.DigitalVideoDisc;
                    return;
                case MediaType.BluRay:
                    BaseCommand = CommandStrings.BluRay;
                    return;
                case MediaType.NintendoGameCubeGameDisc:
                    BaseCommand = CommandStrings.DigitalVideoDisc;
                    return;
                case MediaType.NintendoWiiOpticalDisc:
                    BaseCommand = CommandStrings.DigitalVideoDisc;
                    return;
                case MediaType.FloppyDisk:
                    BaseCommand = CommandStrings.Floppy;
                    return;
                case MediaType.HardDisk:
                    BaseCommand = CommandStrings.Disk;
                    return;
                case MediaType.DataCartridge:
                    BaseCommand = CommandStrings.Tape;
                    return;

                default:
                    BaseCommand = null;
                    return;
            }
        }

        #endregion

        #region Information Extraction Methods

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
                        string identifier = line["DiscType: ".Length..];
                        discTypeOrBookTypeSet.Add(identifier);
                    }
                    else if (line.StartsWith("DiscTypeIdentifier:"))
                    {
                        // DiscTypeIdentifier: <discType>
                        string identifier = line["DiscTypeIdentifier: ".Length..];
                        discTypeOrBookTypeSet.Add(identifier);
                    }
                    else if (line.StartsWith("DiscTypeSpecific:"))
                    {
                        // DiscTypeSpecific: <discType>
                        string identifier = line["DiscTypeSpecific: ".Length..];
                        discTypeOrBookTypeSet.Add(identifier);
                    }
                    else if (line.StartsWith("BookType:"))
                    {
                        // BookType: <discType>
                        string identifier = line["BookType: ".Length..];
                        discTypeOrBookTypeSet.Add(identifier);
                    }

                    line = sr.ReadLine();
                }

                // Create the output string
                if (discTypeOrBookTypeSet.Any())
                    discTypeOrBookType = string.Join(", ", discTypeOrBookTypeSet.OrderBy(s => s));

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
        /// <returns>Formatted string representing the DVD protection, null on error</returns>
        private static string? GetDVDProtection(string cssKey, string disc)
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
                            copyrightProtectionSystemType = line["CopyrightProtectionType: ".Length..];
                        else if (line.StartsWith("RegionManagementInformation"))
                            region = line["RegionManagementInformation: ".Length..];

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
                            decryptedDiscKey = line["DecryptedDiscKey[020]: ".Length..];
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
                                    matchedFilename = matchedFilename[..^2];

                                vobKeys += $"{matchedFilename} Title Key: No Title Key\n";
                            }
                            else
                            {
                                var match = Regex.Match(line, @"^LBA:\s*[0-9]+, Filename: (.*?), EncryptedTitleKey: .*?, DecryptedTitleKey: (.*?)$", RegexOptions.Compiled);
                                string matchedFilename = match.Groups[1].Value;
                                if (matchedFilename.EndsWith(";1"))
                                    matchedFilename = matchedFilename[..^2];

                                vobKeys += $"{matchedFilename} Title Key: {match.Groups[2].Value}\n";
                            }
                        }
                    }
                }
                catch { }
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

                        if (Int64.TryParse(line["Total errors: ".Length..].Trim(), out long te))
                            totalErrors += te;
                    }
                    else if (line.StartsWith("Total warnings"))
                    {
                        totalErrors ??= 0;

                        if (Int64.TryParse(line["Total warnings: ".Length..].Trim(), out long tw))
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
            if (string.IsNullOrWhiteSpace(segaHeader))
                return false;

            // Now read it in cutting it into lines for easier parsing
            try
            {
                string[] header = segaHeader!.Split('\n');
                string versionLine = header[4][58..];
                string dateLine = header[5][58..];
                serial = versionLine[..10].TrimEnd();
                version = versionLine.Substring(10, 6).TrimStart('V', 'v');
                date = dateLine[..8];
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
                        manufacturer = line["VendorId: ".Length..];
                    }
                    else if (string.IsNullOrEmpty(model) && line.StartsWith("ProductId"))
                    {
                        // ProductId: <model>
                        model = line["ProductId: ".Length..];
                    }
                    else if (string.IsNullOrEmpty(firmware) && line.StartsWith("ProductRevisionLevel"))
                    {
                        // ProductRevisionLevel: <firmware>
                        firmware = line["ProductRevisionLevel: ".Length..];
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
                        string[] split = line.Split(' ').Where(s => !string.IsNullOrEmpty(s)).ToArray();
                        return split[1];
                    }

                    // Dual-layer discs have a regular layerbreak
                    else if (!xgd && line.StartsWith("LayerZeroSector"))
                    {
                        // LayerZeroSector: <size> (<hex>)
                        string[] split = line.Split(' ').Where(s => !string.IsNullOrEmpty(s)).ToArray();
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
                if (trackSessionMapping.All(kvp => kvp.Value == "1"))
                    return null;

                // Seek to the multisession data
                line = sr.ReadLine()?.Trim();
                if (line == null)
                    return null;

                if (!line.StartsWith("Lead-out length"))
                    while ((line = sr.ReadLine()?.Trim())?.StartsWith("Lead-out length") == false) ;

                // TODO: Are there any examples of 3+ session discs?

                // Read the first session lead-out
                var firstSessionLeadOutLengthString = line?["Lead-out length of 1st session: ".Length..];
                line = sr.ReadLine()?.Trim();
                if (line == null)
                    return null;

                // Read the second session lead-in, if it exists
                string? secondSessionLeadInLengthString = null;
                while (line?.StartsWith("Lead-in length") == false)
                {
                    secondSessionLeadInLengthString = line?["Lead-in length of 2nd session: ".Length..];
                    line = sr.ReadLine()?.Trim();
                }

                // Read the second session pregap
                var secondSessionPregapLengthString = line?["Pregap length of 1st track of 2nd session: ".Length..];

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
            if (string.IsNullOrWhiteSpace(segaHeader))
                return false;

            // Now read it in cutting it into lines for easier parsing
            try
            {
                string[] header = segaHeader!.Split('\n');
                string serialVersionLine = header[2][58..];
                string dateLine = header[3][58..];
                serial = serialVersionLine[..10].Trim();
                version = serialVersionLine.Substring(10, 6).TrimStart('V', 'v');
                date = dateLine[..8];
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
            if (string.IsNullOrWhiteSpace(segaHeader))
                return false;

            // Now read it in cutting it into lines for easier parsing
            try
            {
                string[] header = segaHeader!.Split('\n');
                string serialVersionLine = header[8][58..];
                string dateLine = header[1][58..];
                serial = serialVersionLine.Substring(3, 8).TrimEnd('-', ' ');
                date = dateLine[8..].Trim();

                // Properly format the date string, if possible
                string[] dateSplit = date.Split('.');

                if (dateSplit.Length == 1)
                    dateSplit = [date[..4], date[4..]];

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
                // Fast forward to the offsets
                using var sr = File.OpenText(disc);
                while (sr.ReadLine()?.Trim()?.StartsWith("========== Offset") == false) ;
                sr.ReadLine(); // Combined Offset
                sr.ReadLine(); // Drive Offset
                sr.ReadLine(); // Separator line

                // Now that we're at the offsets, attempt to get the sample offset
                return sr.ReadLine()?.Split(' ')?.LastOrDefault();
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
            if (suppl?.Games == null)
                return false;

            // Try to extract the hash information
            var roms = suppl.Games[0].Roms;
            if (roms == null || roms.Length == 0)
                return false;

            dmihash = roms.FirstOrDefault(r => r.Name?.EndsWith("DMI.bin") == true)?.Crc?.ToUpperInvariant();
            pfihash = roms.FirstOrDefault(r => r.Name?.EndsWith("PFI.bin") == true)?.Crc?.ToUpperInvariant();
            sshash = roms.FirstOrDefault(r => r.Name?.EndsWith("SS.bin") == true)?.Crc?.ToUpperInvariant();

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

            try
            {
                using var sr = File.OpenText(disc);
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine()?.Trim();
                    if (line == null)
                        break;

                    // Security Sector version
                    if (line.StartsWith("Version of challenge table"))
                    {
                        ssver = line.Split(' ')[4]; // "Version of challenge table: <VER>"
                    }

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
                        if (InfoTool.GetISOHashValues(line, out long _, out var crc32, out _, out _))
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

            try
            {
                using var sr = File.OpenText(disc);
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine()?.Trim();
                    if (line == null)
                        break;

                    // Security Sector version
                    if (line.StartsWith("Version of challenge table"))
                    {
                        ssver = line.Split(' ')[4]; // "Version of challenge table: <VER>"
                    }

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

        /// <summary>
        /// Get the XGD1 Master ID (XMID) information
        /// </summary>
        /// <param name="dmi">DMI.bin file location</param>
        /// <returns>String representation of the XGD1 DMI information, empty string on error</returns>
        private static string GetXGD1XMID(string dmi)
        {
            if (!File.Exists(dmi))
                return string.Empty;

            try
            {
                using var br = new BinaryReader(File.OpenRead(dmi));
                br.BaseStream.Seek(8, SeekOrigin.Begin);
                return new string(br.ReadChars(8));
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Get the XGD2/3 Master ID (XeMID) information
        /// </summary>
        /// <param name="dmi">DMI.bin file location</param>
        /// <returns>String representation of the XGD2/3 DMI information, empty string on error</returns>
        private static string GetXGD23XeMID(string dmi)
        {
            if (!File.Exists(dmi))
                return string.Empty;

            try
            {
                using var br = new BinaryReader(File.OpenRead(dmi));
                br.BaseStream.Seek(64, SeekOrigin.Begin);
                return new string(br.ReadChars(14));
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion
    }
}
