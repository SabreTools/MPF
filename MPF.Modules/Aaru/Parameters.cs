using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MPF.Core.Converters;
using MPF.Core.Data;
using MPF.CueSheets;
using RedumpLib.Data;
using Schemas;

namespace MPF.Modules.Aaru
{
    /// <summary>
    /// Represents a generic set of Aaru parameters
    /// </summary>
    public class Parameters : BaseParameters
    {
        #region Generic Dumping Information

        /// <inheritdoc/>
        public override string InputPath => InputValue;

        /// <inheritdoc/>
        public override string OutputPath => OutputValue;

        /// <inheritdoc/>
        public override int? Speed
        {
            get { return SpeedValue; }
            set { SpeedValue = (sbyte?)value; }
        }

        #endregion

        #region Metadata

        /// <inheritdoc/>
        public override InternalProgram InternalProgram => InternalProgram.Aaru;

        #endregion

        #region Flag Values

        public int? BlockSizeValue { get; set; }

        public string CommentsValue { get; set; }

        public string CreatorValue { get; set; }

        public int? CountValue { get; set; }

        public string DriveManufacturerValue { get; set; }

        public string DriveModelValue { get; set; }

        public string DriveRevisionValue { get; set; }

        public string DriveSerialValue { get; set; }

        public string EncodingValue { get; set; }

        public string FormatConvertValue { get; set; }

        public string FormatDumpValue { get; set; }

        public string GeometryValue { get; set; }

        public string ImgBurnLogValue { get; set; }

        public string InputValue { get; set; }

        public string Input1Value { get; set; }

        public string Input2Value { get; set; }

        public long? LengthValue { get; set; }

        public int? MaxBlocksValue { get; set; }

        public string MediaBarcodeValue { get; set; }

        public int? MediaLastSequenceValue { get; set; }

        public string MediaManufacturerValue { get; set; }

        public string MediaModelValue { get; set; }

        public string MediaPartNumberValue { get; set; }

        public int? MediaSequenceValue { get; set; }

        public string MediaSerialValue { get; set; }

        public string MediaTitleValue { get; set; }

        public string MHDDLogValue { get; set; }

        public string NamespaceValue { get; set; }

        public string OptionsValue { get; set; }

        public string OutputValue { get; set; }

        public string OutputPrefixValue { get; set; }

        public string RemoteHostValue { get; set; }

        public string ResumeFileValue { get; set; }

        public short? RetryPassesValue { get; set; }

        public int? SkipValue { get; set; }

        public sbyte? SpeedValue { get; set; }

        public long? StartValue { get; set; }

        public string SubchannelValue { get; set; }

        public short? WidthValue { get; set; }

        public string XMLSidecarValue { get; set; }

        #endregion

        /// <inheritdoc/>
        public Parameters(string parameters) : base(parameters) { }

        /// <inheritdoc/>
        public Parameters(RedumpSystem? system, MediaType? type, char driveLetter, string filename, int? driveSpeed, Options options)
            : base(system, type, driveLetter, filename, driveSpeed, options)
        {
        }

        #region BaseParameters Implementations

        /// <inheritdoc/>
        public override (bool, List<string>) CheckAllOutputFilesExist(string basePath, bool preCheck)
        {
            List<string> missingFiles = new List<string>();
            switch (this.Type)
            {
                case MediaType.CDROM:
                    if (!File.Exists($"{basePath}_logs.zip") || !preCheck)
                    {
                        if (!File.Exists($"{basePath}.cicm.xml"))
                            missingFiles.Add($"{basePath}.cicm.xml");
                        if (!File.Exists($"{basePath}.ibg"))
                            missingFiles.Add($"{basePath}.ibg");
                        if (!File.Exists($"{basePath}.log"))
                            missingFiles.Add($"{basePath}.log");
                        if (!File.Exists($"{basePath}.mhddlog.bin"))
                            missingFiles.Add($"{basePath}.mhddlog.bin");
                        if (!File.Exists($"{basePath}.resume.xml"))
                            missingFiles.Add($"{basePath}.resume.xml");
                        if (!File.Exists($"{basePath}.sub.log"))
                            missingFiles.Add($"{basePath}.sub.log");
                    }

                    break;

                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                    if (!File.Exists($"{basePath}_logs.zip") || !preCheck)
                    {
                        if (!File.Exists($"{basePath}.cicm.xml"))
                            missingFiles.Add($"{basePath}.cicm.xml");
                        if (!File.Exists($"{basePath}.ibg"))
                            missingFiles.Add($"{basePath}.ibg");
                        if (!File.Exists($"{basePath}.log"))
                            missingFiles.Add($"{basePath}.log");
                        if (!File.Exists($"{basePath}.mhddlog.bin"))
                            missingFiles.Add($"{basePath}.mhddlog.bin");
                        if (!File.Exists($"{basePath}.resume.xml"))
                            missingFiles.Add($"{basePath}.resume.xml");
                    }

                    break;

                default:
                    missingFiles.Add("Media and system combination not supported for Aaru");
                    break;
            }

            return (!missingFiles.Any(), missingFiles);
        }

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, Options options, string basePath, Drive drive, bool includeArtifacts)
        {
            // TODO: Fill in submission info specifics for Aaru
            string outputDirectory = Path.GetDirectoryName(basePath);

            // TODO: Determine if there's an Aaru version anywhere
            info.DumpingInfo.DumpingProgram = EnumConverter.LongName(this.InternalProgram);

            // Deserialize the sidecar, if possible
            var sidecar = GenerateSidecar(basePath + ".cicm.xml");

            // Fill in the hardware data
            if (GetHardwareInfo(sidecar, out string manufacturer, out string model, out string firmware))
            {
                info.DumpingInfo.Manufacturer = manufacturer;
                info.DumpingInfo.Model = model;
                info.DumpingInfo.Firmware = firmware;
            }

            // Fill in the disc type data
            if (GetDiscType(sidecar, out string discType, out string discSubType))
            {
                string fullDiscType = string.Empty;
                if (!string.IsNullOrWhiteSpace(discType) && !string.IsNullOrWhiteSpace(discSubType))
                    fullDiscType = $"{discType} ({discSubType})";
                else if (!string.IsNullOrWhiteSpace(discType) && string.IsNullOrWhiteSpace(discSubType))
                    fullDiscType = discType;
                else if (string.IsNullOrWhiteSpace(discType) && !string.IsNullOrWhiteSpace(discSubType))
                    fullDiscType = discSubType;

                info.DumpingInfo.ReportedDiscType = fullDiscType;
            }

            // Fill in the hash data
            info.TracksAndWriteOffsets.ClrMameProData = GenerateDatfile(sidecar, basePath);

            switch (this.Type)
            {
                // TODO: Can this do GD-ROM?
                case MediaType.CDROM:

                    // TODO: Re-enable once PVD generation / finding is fixed
                    // Generate / obtain the PVD
                    //info.Extras.PVD = GeneratePVD(sidecar) ?? "Disc has no PVD";

                    long errorCount = -1;
                    if (File.Exists(basePath + ".resume.xml"))
                        errorCount = GetErrorCount(basePath + ".resume.xml");

                    info.CommonDiscInfo.ErrorsCount = (errorCount == -1 ? "Error retrieving error count" : errorCount.ToString());

                    info.TracksAndWriteOffsets.Cuesheet = GenerateCuesheet(sidecar, basePath) ?? "";

                    string cdWriteOffset = GetWriteOffset(sidecar) ?? "";
                    info.CommonDiscInfo.RingWriteOffset = cdWriteOffset;
                    info.TracksAndWriteOffsets.OtherWriteOffsets = cdWriteOffset;
                    break;

                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                    // Get the individual hash data, as per internal
                    if (GetISOHashValues(info.TracksAndWriteOffsets.ClrMameProData, out long size, out string crc32, out string md5, out string sha1))
                    {
                        info.SizeAndChecksums.Size = size;
                        info.SizeAndChecksums.CRC32 = crc32;
                        info.SizeAndChecksums.MD5 = md5;
                        info.SizeAndChecksums.SHA1 = sha1;
                    }

                    // TODO: Re-enable once PVD generation / finding is fixed
                    // Generate / obtain the PVD
                    //info.Extras.PVD = GeneratePVD(sidecar) ?? "Disc has no PVD";

                    // Deal with the layerbreak
                    string layerbreak = null;
                    if (this.Type == MediaType.DVD)
                        layerbreak = GetLayerbreak(sidecar) ?? "";
                    else if (this.Type == MediaType.BluRay)
                        layerbreak = info.SizeAndChecksums.Size > 25_025_314_816 ? "25025314816" : null;

                    // If we have a single-layer disc
                    if (string.IsNullOrWhiteSpace(layerbreak))
                    {
                        // Currently no-op
                    }
                    // If we have a dual-layer disc
                    else
                    {
                        info.SizeAndChecksums.Layerbreak = Int64.Parse(layerbreak);
                    }

                    // TODO: Investigate XGD disc outputs
                    // TODO: Investigate BD specifics like PIC

                    break;
            }

            switch (this.System)
            {
                // TODO: Can we get SecuROM data?
                // TODO: Can we get SS version/ranges?
                // TODO: Can we get DMI info?
                // TODO: Can we get Sega Header info?
                // TODO: Can we get PS1 EDC status?
                // TODO: Can we get PS1 LibCrypt status?

                case RedumpSystem.DVDAudio:
                case RedumpSystem.DVDVideo:
                    info.CopyProtection.Protection = GetDVDProtection(sidecar) ?? "";
                    break;

                case RedumpSystem.KonamiPython2:
                    if (GetPlayStationExecutableInfo(drive?.Letter, out string pythonTwoSerial, out Region? pythonTwoRegion, out string pythonTwoDate))
                    {
                        // Ensure internal serial is pulled from local data
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalSerialName] = pythonTwoSerial ?? string.Empty;
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? pythonTwoRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = pythonTwoDate;
                    }

                    info.VersionAndEditions.Version = GetPlayStation2Version(drive?.Letter) ?? "";
                    break;

                case RedumpSystem.MicrosoftXbox:
                    if (GetXgdAuxInfo(sidecar, out string xgd1DMIHash, out string xgd1PFIHash, out string xgd1SSHash, out string ss, out string xgd1SSVer))
                    {
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.DMIHash] = xgd1DMIHash;
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.PFIHash] = xgd1PFIHash;
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.SSHash] = xgd1SSHash;
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.SSVersion] = xgd1SSVer;
                        info.Extras.SecuritySectorRanges = ss ?? "";
                    }

                    if (GetXboxDMIInfo(sidecar, out string serial, out string version, out Region? region))
                    {
                        info.CommonDiscInfo.Serial = serial ?? "";
                        info.VersionAndEditions.Version = version ?? "";
                        info.CommonDiscInfo.Region = region;
                    }

                    break;

                case RedumpSystem.MicrosoftXbox360:
                    if (GetXgdAuxInfo(sidecar, out string xgd23DMIHash, out string xgd23PFIHash, out string xgd23SSHash, out string ss360, out string xgd23SSVer))
                    {
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.DMIHash] = xgd23DMIHash;
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.PFIHash] = xgd23PFIHash;
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.SSHash] = xgd23SSHash;
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.SSVersion] = xgd23SSVer;
                        info.Extras.SecuritySectorRanges = ss360 ?? "";
                    }

                    if (GetXbox360DMIInfo(sidecar, out string serial360, out string version360, out Region? region360))
                    {
                        info.CommonDiscInfo.Serial = serial360 ?? "";
                        info.VersionAndEditions.Version = version360 ?? "";
                        info.CommonDiscInfo.Region = region360;
                    }
                    break;

                case RedumpSystem.SonyPlayStation:
                    if (GetPlayStationExecutableInfo(drive?.Letter, out string playstationSerial, out Region? playstationRegion, out string playstationDate))
                    {
                        // Ensure internal serial is pulled from local data
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalSerialName] = playstationSerial ?? string.Empty;
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? playstationRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = playstationDate;
                    }

                    break;

                case RedumpSystem.SonyPlayStation2:
                    if (GetPlayStationExecutableInfo(drive?.Letter, out string playstationTwoSerial, out Region? playstationTwoRegion, out string playstationTwoDate))
                    {
                        // Ensure internal serial is pulled from local data
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalSerialName] = playstationTwoSerial ?? string.Empty;
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? playstationTwoRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = playstationTwoDate;
                    }

                    info.VersionAndEditions.Version = GetPlayStation2Version(drive?.Letter) ?? "";
                    break;

                case RedumpSystem.SonyPlayStation4:
                    info.VersionAndEditions.Version = GetPlayStation4Version(drive?.Letter) ?? "";
                    break;

                case RedumpSystem.SonyPlayStation5:
                    info.VersionAndEditions.Version = GetPlayStation5Version(drive?.Letter) ?? "";
                    break;
            }

            // Fill in any artifacts that exist, Base64-encoded, if we need to
            if (includeArtifacts)
            {
                if (File.Exists(basePath + ".cicm.xml"))
                    info.Artifacts["cicm"] = GetBase64(GetFullFile(basePath + ".cicm.xml"));
                if (File.Exists(basePath + ".ibg"))
                    info.Artifacts["ibg"] = Convert.ToBase64String(File.ReadAllBytes(basePath + ".ibg"));
                if (File.Exists(basePath + ".log"))
                    info.Artifacts["log"] = GetBase64(GetFullFile(basePath + ".log"));
                if (File.Exists(basePath + ".mhddlog.bin"))
                    info.Artifacts["mhddlog_bin"] = Convert.ToBase64String(File.ReadAllBytes(basePath + ".mhddlog.bin"));
                if (File.Exists(basePath + ".resume.xml"))
                    info.Artifacts["resume"] = GetBase64(GetFullFile(basePath + ".resume.xml"));
                if (File.Exists(basePath + ".sub.log"))
                    info.Artifacts["sub_log"] = GetBase64(GetFullFile(basePath + ".sub.log"));
            }
        }

        /// <inheritdoc/>
        public override string GenerateParameters()
        {
            List<string> parameters = new List<string>();

            #region Pre-command flags

            // Debug
            if (this[FlagStrings.DebugLong] != null)
                parameters.Add($"{FlagStrings.DebugLong} {this[FlagStrings.DebugLong]}");

            // Pause
            if (this[FlagStrings.PauseLong] != null)
                parameters.Add($"{FlagStrings.PauseLong} {this[FlagStrings.PauseLong]}");

            // Verbose
            if (this[FlagStrings.VerboseLong] != null)
                parameters.Add($"{FlagStrings.VerboseLong} {this[FlagStrings.VerboseLong]}");

            // Version
            if (this[FlagStrings.VersionLong] != null)
                parameters.Add($"{FlagStrings.VersionLong} {this[FlagStrings.VersionLong]}");

            // Help
            if (this[FlagStrings.HelpLong] != null)
                parameters.Add($"{FlagStrings.HelpLong} {this[FlagStrings.HelpLong]}");

            #endregion

            if (BaseCommand == null)
                BaseCommand = CommandStrings.NONE;

            if (!string.IsNullOrWhiteSpace(BaseCommand))
                parameters.Add(BaseCommand);
            else
                return null;

            #region Boolean flags

            // Adler-32
            if (IsFlagSupported(FlagStrings.Adler32Long))
            {
                if (this[FlagStrings.Adler32Long] != null)
                    parameters.Add($"{FlagStrings.Adler32Long} {this[FlagStrings.Adler32Long]}");
            }

            // Clear
            if (IsFlagSupported(FlagStrings.ClearLong))
            {
                if (this[FlagStrings.ClearLong] != null)
                    parameters.Add($"{FlagStrings.ClearLong} {this[FlagStrings.ClearLong]}");
            }

            // Clear All
            if (IsFlagSupported(FlagStrings.ClearAllLong))
            {
                if (this[FlagStrings.ClearAllLong] != null)
                    parameters.Add($"{FlagStrings.ClearAllLong} {this[FlagStrings.ClearAllLong]}");
            }

            // CRC16
            if (IsFlagSupported(FlagStrings.CRC16Long))
            {
                if (this[FlagStrings.CRC16Long] != null)
                    parameters.Add($"{FlagStrings.CRC16Long} {this[FlagStrings.CRC16Long]}");
            }

            // CRC32
            if (IsFlagSupported(FlagStrings.CRC32Long))
            {
                if (this[FlagStrings.CRC32Long] != null)
                    parameters.Add($"{FlagStrings.CRC32Long} {this[FlagStrings.CRC32Long]}");
            }

            // CRC64
            if (IsFlagSupported(FlagStrings.CRC64Long))
            {
                if (this[FlagStrings.CRC64Long] != null)
                    parameters.Add($"{FlagStrings.CRC64Long} {this[FlagStrings.CRC64Long]}");
            }

            // Disk Tags
            if (IsFlagSupported(FlagStrings.DiskTagsLong))
            {
                if (this[FlagStrings.DiskTagsLong] != null)
                    parameters.Add($"{FlagStrings.DiskTagsLong} {this[FlagStrings.DiskTagsLong]}");
            }

            // Duplicated Sectors
            if (IsFlagSupported(FlagStrings.DuplicatedSectorsLong))
            {
                if (this[FlagStrings.DuplicatedSectorsLong] != null)
                    parameters.Add($"{FlagStrings.DuplicatedSectorsLong} {this[FlagStrings.DuplicatedSectorsLong]}");
            }

            // Eject
            if (IsFlagSupported(FlagStrings.EjectLong))
            {
                if (this[FlagStrings.EjectLong] != null)
                    parameters.Add($"{FlagStrings.EjectLong} {this[FlagStrings.EjectLong]}");
            }

            // Extended Attributes
            if (IsFlagSupported(FlagStrings.ExtendedAttributesLong))
            {
                if (this[FlagStrings.ExtendedAttributesLong] != null)
                    parameters.Add($"{FlagStrings.ExtendedAttributesLong} {this[FlagStrings.ExtendedAttributesLong]}");
            }

            // Filesystems
            if (IsFlagSupported(FlagStrings.FilesystemsLong))
            {
                if (this[FlagStrings.FilesystemsLong] != null)
                    parameters.Add($"{FlagStrings.FilesystemsLong} {this[FlagStrings.FilesystemsLong]}");
            }

            // First Pregap
            if (IsFlagSupported(FlagStrings.FirstPregapLong))
            {
                if (this[FlagStrings.FirstPregapLong] != null)
                    parameters.Add($"{FlagStrings.FirstPregapLong} {this[FlagStrings.FirstPregapLong]}");
            }

            // Fix Offset
            if (IsFlagSupported(FlagStrings.FixOffsetLong))
            {
                if (this[FlagStrings.FixOffsetLong] != null)
                    parameters.Add($"{FlagStrings.FixOffsetLong} {this[FlagStrings.FixOffsetLong]}");
            }

            // Fix Subchannel
            if (IsFlagSupported(FlagStrings.FixSubchannelLong))
            {
                if (this[FlagStrings.FixSubchannelLong] != null)
                    parameters.Add($"{FlagStrings.FixSubchannelLong} {this[FlagStrings.FixSubchannelLong]}");
            }

            // Fix Subchannel CRC
            if (IsFlagSupported(FlagStrings.FixSubchannelCrcLong))
            {
                if (this[FlagStrings.FixSubchannelCrcLong] != null)
                    parameters.Add($"{FlagStrings.FixSubchannelCrcLong} {this[FlagStrings.FixSubchannelCrcLong]}");
            }

            // Fix Subchannel Position
            if (IsFlagSupported(FlagStrings.FixSubchannelPositionLong))
            {
                if (this[FlagStrings.FixSubchannelPositionLong] != null)
                    parameters.Add($"{FlagStrings.FixSubchannelPositionLong} {this[FlagStrings.FixSubchannelPositionLong]}");
            }

            // Fletcher-16
            if (IsFlagSupported(FlagStrings.Fletcher16Long))
            {
                if (this[FlagStrings.Fletcher16Long] != null)
                    parameters.Add($"{FlagStrings.Fletcher16Long} {this[FlagStrings.Fletcher16Long]}");
            }

            // Fletcher-32
            if (IsFlagSupported(FlagStrings.Fletcher32Long))
            {
                if (this[FlagStrings.Fletcher32Long] != null)
                    parameters.Add($"{FlagStrings.Fletcher32Long} {this[FlagStrings.Fletcher32Long]}");
            }

            // Force
            if (IsFlagSupported(FlagStrings.ForceLong))
            {
                if (this[FlagStrings.ForceLong] != null)
                    parameters.Add($"{FlagStrings.ForceLong} {this[FlagStrings.ForceLong]}");
            }

            // Generate Subchannels
            if (IsFlagSupported(FlagStrings.GenerateSubchannelsLong))
            {
                if (this[FlagStrings.GenerateSubchannelsLong] != null)
                    parameters.Add($"{FlagStrings.GenerateSubchannelsLong} {this[FlagStrings.GenerateSubchannelsLong]}");
            }

            // Long Format
            if (IsFlagSupported(FlagStrings.LongFormatLong))
            {
                if (this[FlagStrings.LongFormatLong] != null)
                    parameters.Add($"{FlagStrings.LongFormatLong} {this[FlagStrings.LongFormatLong]}");
            }

            // Long Sectors
            if (IsFlagSupported(FlagStrings.LongSectorsLong))
            {
                if (this[FlagStrings.LongSectorsLong] != null)
                    parameters.Add($"{FlagStrings.LongSectorsLong} {this[FlagStrings.LongSectorsLong]}");
            }

            // MD5
            if (IsFlagSupported(FlagStrings.MD5Long))
            {
                if (this[FlagStrings.MD5Long] != null)
                    parameters.Add($"{FlagStrings.MD5Long} {this[FlagStrings.MD5Long]}");
            }

            // Metadata
            if (IsFlagSupported(FlagStrings.MetadataLong))
            {
                if (this[FlagStrings.MetadataLong] != null)
                    parameters.Add($"{FlagStrings.MetadataLong} {this[FlagStrings.MetadataLong]}");
            }

            // Partitions
            if (IsFlagSupported(FlagStrings.PartitionsLong))
            {
                if (this[FlagStrings.PartitionsLong] != null)
                    parameters.Add($"{FlagStrings.PartitionsLong} {this[FlagStrings.PartitionsLong]}");
            }

            // Persistent
            if (IsFlagSupported(FlagStrings.PersistentLong))
            {
                if (this[FlagStrings.PersistentLong] != null)
                    parameters.Add($"{FlagStrings.PersistentLong} {this[FlagStrings.PersistentLong]}");
            }

            // Private
            if (IsFlagSupported(FlagStrings.PrivateLong))
            {
                if (this[FlagStrings.PrivateLong] != null)
                    parameters.Add($"{FlagStrings.PrivateLong} {this[FlagStrings.PrivateLong]}");
            }

            // Resume
            if (IsFlagSupported(FlagStrings.ResumeLong))
            {
                if (this[FlagStrings.ResumeLong] != null)
                    parameters.Add($"{FlagStrings.ResumeLong} {this[FlagStrings.ResumeLong]}");
            }

            // Retry Subchannel
            if (IsFlagSupported(FlagStrings.RetrySubchannelLong))
            {
                if (this[FlagStrings.RetrySubchannelLong] != null)
                    parameters.Add($"{FlagStrings.RetrySubchannelLong} {this[FlagStrings.RetrySubchannelLong]}");
            }

            // Sector Tags
            if (IsFlagSupported(FlagStrings.SectorTagsLong))
            {
                if (this[FlagStrings.SectorTagsLong] != null)
                    parameters.Add($"{FlagStrings.SectorTagsLong} {this[FlagStrings.SectorTagsLong]}");
            }

            // Separated Tracks
            if (IsFlagSupported(FlagStrings.SeparatedTracksLong))
            {
                if (this[FlagStrings.SeparatedTracksLong] != null)
                    parameters.Add($"{FlagStrings.SeparatedTracksLong} {this[FlagStrings.SeparatedTracksLong]}");
            }

            // SHA-1
            if (IsFlagSupported(FlagStrings.SHA1Long))
            {
                if (this[FlagStrings.SHA1Long] != null)
                    parameters.Add($"{FlagStrings.SHA1Long} {this[FlagStrings.SHA1Long]}");
            }

            // SHA-256
            if (IsFlagSupported(FlagStrings.SHA256Long))
            {
                if (this[FlagStrings.SHA256Long] != null)
                    parameters.Add($"{FlagStrings.SHA256Long} {this[FlagStrings.SHA256Long]}");
            }

            // SHA-384
            if (IsFlagSupported(FlagStrings.SHA384Long))
            {
                if (this[FlagStrings.SHA384Long] != null)
                    parameters.Add($"{FlagStrings.SHA384Long} {this[FlagStrings.SHA384Long]}");
            }

            // SHA-512
            if (IsFlagSupported(FlagStrings.SHA512Long))
            {
                if (this[FlagStrings.SHA512Long] != null)
                    parameters.Add($"{FlagStrings.SHA512Long} {this[FlagStrings.SHA512Long]}");
            }

            // Skip CD-i Ready Hole
            if (IsFlagSupported(FlagStrings.SkipCdiReadyHoleLong))
            {
                if (this[FlagStrings.SkipCdiReadyHoleLong] != null)
                    parameters.Add($"{FlagStrings.SkipCdiReadyHoleLong} {this[FlagStrings.SkipCdiReadyHoleLong]}");
            }

            // SpamSum
            if (IsFlagSupported(FlagStrings.SpamSumLong))
            {
                if (this[FlagStrings.SpamSumLong] != null)
                    parameters.Add($"{FlagStrings.SpamSumLong} {this[FlagStrings.SpamSumLong]}");
            }

            // Stop on Error
            if (IsFlagSupported(FlagStrings.StopOnErrorLong))
            {
                if (this[FlagStrings.StopOnErrorLong] != null)
                    parameters.Add($"{FlagStrings.StopOnErrorLong} {this[FlagStrings.StopOnErrorLong]}");
            }

            // Stop on Error
            if (IsFlagSupported(FlagStrings.StoreEncryptedLong))
            {
                if (this[FlagStrings.StoreEncryptedLong] != null)
                    parameters.Add($"{FlagStrings.StoreEncryptedLong} {this[FlagStrings.StoreEncryptedLong]}");
            }

            // Tape
            if (IsFlagSupported(FlagStrings.TapeLong))
            {
                if (this[FlagStrings.TapeLong] != null)
                    parameters.Add($"{FlagStrings.TapeLong} {this[FlagStrings.TapeLong]}");
            }

            // Title Keys
            if (IsFlagSupported(FlagStrings.TitleKeysLong))
            {
                if (this[FlagStrings.TitleKeysLong] != null)
                    parameters.Add($"{FlagStrings.TitleKeysLong} {this[FlagStrings.TitleKeysLong]}");
            }

            // Trap Disc
            if (IsFlagSupported(FlagStrings.TrapDiscLong))
            {
                if (this[FlagStrings.TrapDiscLong] != null)
                    parameters.Add($"{FlagStrings.TrapDiscLong} {this[FlagStrings.TrapDiscLong]}");
            }

            // Trim
            if (IsFlagSupported(FlagStrings.TrimLong))
            {
                if (this[FlagStrings.TrimLong] != null)
                    parameters.Add($"{FlagStrings.TrimLong} {this[FlagStrings.TrimLong]}");
            }

            // Use Buffered Reads
            if (IsFlagSupported(FlagStrings.UseBufferedReadsLong))
            {
                if (this[FlagStrings.UseBufferedReadsLong] != null)
                    parameters.Add($"{FlagStrings.UseBufferedReadsLong} {this[FlagStrings.UseBufferedReadsLong]}");
            }

            // Verify Disc
            if (IsFlagSupported(FlagStrings.VerifyDiscLong))
            {
                if (this[FlagStrings.VerifyDiscLong] != null)
                    parameters.Add($"{FlagStrings.VerifyDiscLong} {this[FlagStrings.VerifyDiscLong]}");
            }

            // Verify Sectors
            if (IsFlagSupported(FlagStrings.VerifySectorsLong))
            {
                if (this[FlagStrings.VerifySectorsLong] != null)
                    parameters.Add($"{FlagStrings.VerifySectorsLong} {this[FlagStrings.VerifySectorsLong]}");
            }

            // Whole Disc
            if (IsFlagSupported(FlagStrings.WholeDiscLong))
            {
                if (this[FlagStrings.WholeDiscLong] != null)
                    parameters.Add($"{FlagStrings.WholeDiscLong} {this[FlagStrings.WholeDiscLong]}");
            }

            #endregion

            #region Int8 flags

            // Speed
            if (IsFlagSupported(FlagStrings.SpeedLong))
            {
                if (this[FlagStrings.SpeedLong] == true && SpeedValue != null)
                    parameters.Add($"{FlagStrings.SpeedLong} {SpeedValue}");
            }

            #endregion

            #region Int16 flags

            // Retry Passes
            if (IsFlagSupported(FlagStrings.RetryPassesLong))
            {
                if (this[FlagStrings.RetryPassesLong] == true && RetryPassesValue != null)
                    parameters.Add($"{FlagStrings.RetryPassesLong} {RetryPassesValue}");
            }

            // Width
            if (IsFlagSupported(FlagStrings.WidthLong))
            {
                if (this[FlagStrings.WidthLong] == true && WidthValue != null)
                    parameters.Add($"{FlagStrings.WidthLong} {WidthValue}");
            }

            #endregion

            #region Int32 flags

            // Block Size
            if (IsFlagSupported(FlagStrings.BlockSizeLong))
            {
                if (this[FlagStrings.BlockSizeLong] == true && BlockSizeValue != null)
                    parameters.Add($"{FlagStrings.BlockSizeLong} {BlockSizeValue}");
            }

            // Count
            if (IsFlagSupported(FlagStrings.CountLong))
            {
                if (this[FlagStrings.CountLong] == true && CountValue != null)
                    parameters.Add($"{FlagStrings.CountLong} {CountValue}");
            }

            // Max Blocks
            if (IsFlagSupported(FlagStrings.MaxBlocksLong))
            {
                if (this[FlagStrings.MaxBlocksLong] == true && MaxBlocksValue != null)
                    parameters.Add($"{FlagStrings.MaxBlocksLong} {MaxBlocksValue}");
            }

            // Media Last Sequence
            if (IsFlagSupported(FlagStrings.MediaLastSequenceLong))
            {
                if (this[FlagStrings.MediaLastSequenceLong] == true && MediaLastSequenceValue != null)
                    parameters.Add($"{FlagStrings.MediaLastSequenceLong} {MediaLastSequenceValue}");
            }

            // Media Sequence
            if (IsFlagSupported(FlagStrings.MediaSequenceLong))
            {
                if (this[FlagStrings.MediaSequenceLong] == true && MediaSequenceValue != null)
                    parameters.Add($"{FlagStrings.MediaSequenceLong} {MediaSequenceValue}");
            }

            // Skip
            if (IsFlagSupported(FlagStrings.SkipLong))
            {
                if (this[FlagStrings.SkipLong] == true && SkipValue != null)
                    parameters.Add($"{FlagStrings.SkipLong} {SkipValue}");
            }

            #endregion

            #region Int64 flags

            // Length
            if (IsFlagSupported(FlagStrings.LengthLong))
            {
                if (this[FlagStrings.LengthLong] == true && LengthValue != null)
                {
                    if (LengthValue >= 0)
                        parameters.Add($"{FlagStrings.LengthLong} {LengthValue}");
                    else if (LengthValue == -1 && BaseCommand == CommandStrings.ImageDecode)
                        parameters.Add($"{FlagStrings.LengthLong} all");
                }
            }

            // Start
            if (IsFlagSupported(FlagStrings.StartLong))
            {
                if (this[FlagStrings.StartLong] == true && StartValue != null)
                    parameters.Add($"{FlagStrings.StartLong} {StartValue}");
            }

            #endregion

            #region String flags

            // Comments
            if (IsFlagSupported(FlagStrings.CommentsLong))
            {
                if (this[FlagStrings.CommentsLong] == true && CommentsValue != null)
                    parameters.Add($"{FlagStrings.CommentsLong} \"{CommentsValue}\"");
            }

            // Creator
            if (IsFlagSupported(FlagStrings.CreatorLong))
            {
                if (this[FlagStrings.CreatorLong] == true && CreatorValue != null)
                    parameters.Add($"{FlagStrings.CreatorLong} \"{CreatorValue}\"");
            }

            // Drive Manufacturer
            if (IsFlagSupported(FlagStrings.DriveManufacturerLong))
            {
                if (this[FlagStrings.DriveManufacturerLong] == true && DriveManufacturerValue != null)
                    parameters.Add($"{FlagStrings.DriveManufacturerLong} \"{DriveManufacturerValue}\"");
            }

            // Drive Model
            if (IsFlagSupported(FlagStrings.DriveModelLong))
            {
                if (this[FlagStrings.DriveModelLong] == true && DriveModelValue != null)
                    parameters.Add($"{FlagStrings.DriveModelLong} \"{DriveModelValue}\"");
            }

            // Drive Revision
            if (IsFlagSupported(FlagStrings.DriveRevisionLong))
            {
                if (this[FlagStrings.DriveRevisionLong] == true && DriveRevisionValue != null)
                    parameters.Add($"{FlagStrings.DriveRevisionLong} \"{DriveRevisionValue}\"");
            }

            // Drive Serial
            if (IsFlagSupported(FlagStrings.DriveSerialLong))
            {
                if (this[FlagStrings.DriveSerialLong] == true && DriveSerialValue != null)
                    parameters.Add($"{FlagStrings.DriveSerialLong} \"{DriveSerialValue}\"");
            }

            // Encoding
            if (IsFlagSupported(FlagStrings.EncodingLong))
            {
                if (this[FlagStrings.EncodingLong] == true && EncodingValue != null)
                    parameters.Add($"{FlagStrings.EncodingLong} \"{EncodingValue}\"");
            }

            // Format (Convert)
            if (IsFlagSupported(FlagStrings.FormatConvertLong))
            {
                if (this[FlagStrings.FormatConvertLong] == true && FormatConvertValue != null)
                    parameters.Add($"{FlagStrings.FormatConvertLong} \"{FormatConvertValue}\"");
            }

            // Format (Dump)
            if (IsFlagSupported(FlagStrings.FormatDumpLong))
            {
                if (this[FlagStrings.FormatDumpLong] == true && FormatDumpValue != null)
                    parameters.Add($"{FlagStrings.FormatDumpLong} \"{FormatDumpValue}\"");
            }

            // Geometry
            if (IsFlagSupported(FlagStrings.GeometryLong))
            {
                if (this[FlagStrings.GeometryLong] == true && GeometryValue != null)
                    parameters.Add($"{FlagStrings.GeometryLong} \"{GeometryValue}\"");
            }

            // ImgBurn Log
            if (IsFlagSupported(FlagStrings.ImgBurnLogLong))
            {
                if (this[FlagStrings.ImgBurnLogLong] == true && ImgBurnLogValue != null)
                    parameters.Add($"{FlagStrings.ImgBurnLogLong} \"{ImgBurnLogValue}\"");
            }

            // Media Barcode
            if (IsFlagSupported(FlagStrings.MediaBarcodeLong))
            {
                if (this[FlagStrings.MediaBarcodeLong] == true && MediaBarcodeValue != null)
                    parameters.Add($"{FlagStrings.MediaBarcodeLong} \"{MediaBarcodeValue}\"");
            }

            // Media Manufacturer
            if (IsFlagSupported(FlagStrings.MediaManufacturerLong))
            {
                if (this[FlagStrings.MediaManufacturerLong] == true && MediaManufacturerValue != null)
                    parameters.Add($"{FlagStrings.MediaManufacturerLong} \"{MediaManufacturerValue}\"");
            }

            // Media Model
            if (IsFlagSupported(FlagStrings.MediaModelLong))
            {
                if (this[FlagStrings.MediaModelLong] == true && MediaModelValue != null)
                    parameters.Add($"{FlagStrings.MediaModelLong} \"{MediaModelValue}\"");
            }

            // Media Part Number
            if (IsFlagSupported(FlagStrings.MediaPartNumberLong))
            {
                if (this[FlagStrings.MediaPartNumberLong] == true && MediaPartNumberValue != null)
                    parameters.Add($"{FlagStrings.MediaPartNumberLong} \"{MediaPartNumberValue}\"");
            }

            // Media Serial
            if (IsFlagSupported(FlagStrings.MediaSerialLong))
            {
                if (this[FlagStrings.MediaSerialLong] == true && MediaSerialValue != null)
                    parameters.Add($"{FlagStrings.MediaSerialLong} \"{MediaSerialValue}\"");
            }

            // Media Title
            if (IsFlagSupported(FlagStrings.MediaTitleLong))
            {
                if (this[FlagStrings.MediaTitleLong] == true && MediaTitleValue != null)
                    parameters.Add($"{FlagStrings.MediaTitleLong} \"{MediaTitleValue}\"");
            }

            // MHDD Log
            if (IsFlagSupported(FlagStrings.MHDDLogLong))
            {
                if (this[FlagStrings.MHDDLogLong] == true && MHDDLogValue != null)
                    parameters.Add($"{FlagStrings.MHDDLogLong} \"{MHDDLogValue}\"");
            }

            // Namespace
            if (IsFlagSupported(FlagStrings.NamespaceLong))
            {
                if (this[FlagStrings.NamespaceLong] == true && NamespaceValue != null)
                    parameters.Add($"{FlagStrings.NamespaceLong} \"{NamespaceValue}\"");
            }

            // Options
            if (IsFlagSupported(FlagStrings.OptionsLong))
            {
                if (this[FlagStrings.OptionsLong] == true && OptionsValue != null)
                    parameters.Add($"{FlagStrings.OptionsLong} \"{OptionsValue}\"");
            }

            // Output Prefix
            if (IsFlagSupported(FlagStrings.OutputPrefixLong))
            {
                if (this[FlagStrings.OutputPrefixLong] == true && OutputPrefixValue != null)
                    parameters.Add($"{FlagStrings.OutputPrefixLong} \"{OutputPrefixValue}\"");
            }

            // Resume File
            if (IsFlagSupported(FlagStrings.ResumeFileLong))
            {
                if (this[FlagStrings.ResumeFileLong] == true && ResumeFileValue != null)
                    parameters.Add($"{FlagStrings.ResumeFileLong} \"{ResumeFileValue}\"");
            }

            // Subchannel
            if (IsFlagSupported(FlagStrings.SubchannelLong))
            {
                if (this[FlagStrings.SubchannelLong] == true && SubchannelValue != null)
                    parameters.Add($"{FlagStrings.SubchannelLong} \"{SubchannelValue}\"");
            }

            // XML Sidecar
            if (IsFlagSupported(FlagStrings.XMLSidecarLong))
            {
                if (this[FlagStrings.XMLSidecarLong] == true && XMLSidecarValue != null)
                    parameters.Add($"{FlagStrings.XMLSidecarLong} \"{XMLSidecarValue}\"");
            }

            #endregion

            // Handle filenames based on command, if necessary
            switch (BaseCommand)
            {
                // Input value only
                case CommandStrings.ArchivePrefixLong + " " + CommandStrings.ArchiveInfo:
                case CommandStrings.DevicePrefixLong + " " + CommandStrings.DeviceInfo:
                case CommandStrings.DevicePrefixLong + " " + CommandStrings.DeviceReport:
                case CommandStrings.FilesystemPrefixLong + " " + CommandStrings.FilesystemInfo:
                case CommandStrings.FilesystemPrefixLong + " " + CommandStrings.FilesystemListLong:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageChecksumLong:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageCreateSidecar:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageDecode:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageEntropy:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageInfo:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImagePrint:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageVerify:
                case CommandStrings.MediaPrefixLong + " " + CommandStrings.MediaInfo:
                case CommandStrings.MediaPrefixLong + " " + CommandStrings.MediaScan:
                    if (string.IsNullOrWhiteSpace(InputValue))
                        return null;

                    parameters.Add($"\"{InputValue}\"");
                    break;

                // Two input values
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageCompareLong:
                    if (string.IsNullOrWhiteSpace(Input1Value) || string.IsNullOrWhiteSpace(Input2Value))
                        return null;

                    parameters.Add($"\"{Input1Value}\"");
                    parameters.Add($"\"{Input2Value}\"");
                    break;

                // Input and Output value
                case CommandStrings.FilesystemPrefixLong + " " + CommandStrings.FilesystemExtract:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageConvert:
                case CommandStrings.MediaPrefixLong + " " + CommandStrings.MediaDump:
                    if (string.IsNullOrWhiteSpace(InputValue) || string.IsNullOrWhiteSpace(OutputValue))
                        return null;

                    parameters.Add($"\"{InputValue}\"");
                    parameters.Add($"\"{OutputValue}\"");
                    break;

                // Remote host value only
                case CommandStrings.DevicePrefixLong + " " + CommandStrings.DeviceList:
                case CommandStrings.Remote:
                    if (string.IsNullOrWhiteSpace(RemoteHostValue))
                        return null;

                    parameters.Add($"\"{RemoteHostValue}\"");
                    break;
            }

            return string.Join(" ", parameters);
        }

        /// <inheritdoc/>
        public override Dictionary<string, List<string>> GetCommandSupport()
        {
            return new Dictionary<string, List<string>>()
            {
                #region Archive Family

                [CommandStrings.ArchivePrefixLong + " " + CommandStrings.ArchiveInfo] = new List<string>()
                {
                },

                #endregion

                #region Database Family

                [CommandStrings.DatabasePrefixLong + " " + CommandStrings.DatabaseStats] = new List<string>()
                {
                },

                [CommandStrings.DatabasePrefixLong + " " + CommandStrings.DatabaseUpdate] = new List<string>()
                {
                    FlagStrings.ClearLong,
                    FlagStrings.ClearAllLong,
                },

                #endregion

                #region Device Family

                [CommandStrings.DevicePrefixLong + " " + CommandStrings.DeviceInfo] = new List<string>()
                {
                    FlagStrings.OutputPrefixLong,
                },

                [CommandStrings.DevicePrefixLong + " " + CommandStrings.DeviceList] = new List<string>()
                {
                },

                [CommandStrings.DevicePrefixLong + " " + CommandStrings.DeviceReport] = new List<string>()
                {
                    FlagStrings.TrapDiscLong,
                },

                #endregion

                #region Filesystem Family

                [CommandStrings.FilesystemPrefixLong + " " + CommandStrings.FilesystemExtract] = new List<string>()
                {
                    FlagStrings.EncodingLong,
                    FlagStrings.EncodingShort,
                    FlagStrings.ExtendedAttributesLong,
                    FlagStrings.ExtendedAttributesShort,
                    FlagStrings.NamespaceLong,
                    FlagStrings.NamespaceShort,
                    FlagStrings.OptionsLong,
                    FlagStrings.OptionsShort,
                },

                [CommandStrings.FilesystemPrefixLong + " " + CommandStrings.FilesystemInfo] = new List<string>()
                {
                    FlagStrings.EncodingLong,
                    FlagStrings.EncodingShort,
                    FlagStrings.ExtendedAttributesLong,
                    FlagStrings.ExtendedAttributesShort,
                    FlagStrings.NamespaceLong,
                    FlagStrings.NamespaceShort,
                    FlagStrings.OptionsLong,
                    FlagStrings.OptionsShort,
                },

                [CommandStrings.FilesystemPrefixLong + " " + CommandStrings.FilesystemListLong] = new List<string>()
                {
                    FlagStrings.EncodingLong,
                    FlagStrings.EncodingShort,
                    FlagStrings.FilesystemsLong,
                    FlagStrings.FilesystemsShort,
                    FlagStrings.LongFormatLong,
                    FlagStrings.LongFormatShort,
                    FlagStrings.PartitionsLong,
                    FlagStrings.PartitionsShort,
                },

                [CommandStrings.FilesystemPrefixLong + " " + CommandStrings.FilesystemOptions] = new List<string>()
                {
                },

                #endregion

                #region Image Family

                [CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageChecksumLong] = new List<string>()
                {
                    FlagStrings.Adler32Long,
                    FlagStrings.Adler32Short,
                    FlagStrings.CRC16Long,
                    FlagStrings.CRC32Long,
                    FlagStrings.CRC32Short,
                    FlagStrings.CRC64Long,
                    FlagStrings.Fletcher16Long,
                    FlagStrings.Fletcher32Long,
                    FlagStrings.MD5Long,
                    FlagStrings.MD5Short,
                    FlagStrings.SeparatedTracksLong,
                    FlagStrings.SeparatedTracksShort,
                    FlagStrings.SHA1Long,
                    FlagStrings.SHA1Short,
                    FlagStrings.SHA256Long,
                    FlagStrings.SHA384Long,
                    FlagStrings.SHA512Long,
                    FlagStrings.SpamSumLong,
                    FlagStrings.SpamSumShort,
                    FlagStrings.WholeDiscLong,
                    FlagStrings.WholeDiscShort,
                },

                [CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageCompareLong] = new List<string>()
                {
                },

                [CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageConvert] = new List<string>()
                {
                    FlagStrings.CommentsLong,
                    FlagStrings.CountLong,
                    FlagStrings.CountShort,
                    FlagStrings.CreatorLong,
                    FlagStrings.DriveManufacturerLong,
                    FlagStrings.DriveModelLong,
                    FlagStrings.DriveRevisionLong,
                    FlagStrings.DriveSerialLong,
                    FlagStrings.FixSubchannelLong,
                    FlagStrings.FixSubchannelCrcLong,
                    FlagStrings.FixSubchannelPositionLong,
                    FlagStrings.ForceLong,
                    FlagStrings.ForceShort,
                    FlagStrings.FormatConvertLong,
                    FlagStrings.FormatConvertShort,
                    FlagStrings.GenerateSubchannelsLong,
                    FlagStrings.GeometryLong,
                    FlagStrings.GeometryShort,
                    FlagStrings.MediaBarcodeLong,
                    FlagStrings.MediaLastSequenceLong,
                    FlagStrings.MediaManufacturerLong,
                    FlagStrings.MediaModelLong,
                    FlagStrings.MediaPartNumberLong,
                    FlagStrings.MediaSequenceLong,
                    FlagStrings.MediaSerialLong,
                    FlagStrings.MediaTitleLong,
                    FlagStrings.OptionsLong,
                    FlagStrings.OptionsShort,
                    FlagStrings.ResumeFileLong,
                    FlagStrings.ResumeFileShort,
                    FlagStrings.XMLSidecarLong,
                    FlagStrings.XMLSidecarShort,
                },

                [CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageCreateSidecar] = new List<string>()
                {
                    FlagStrings.BlockSizeLong,
                    FlagStrings.BlockSizeShort,
                    FlagStrings.EncodingLong,
                    FlagStrings.EncodingShort,
                    FlagStrings.TapeLong,
                    FlagStrings.TapeShort,
                },

                [CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageDecode] = new List<string>()
                {
                    FlagStrings.DiskTagsLong,
                    FlagStrings.DiskTagsShort,
                    FlagStrings.LengthLong,
                    FlagStrings.LengthShort,
                    FlagStrings.SectorTagsLong,
                    FlagStrings.SectorTagsShort,
                    FlagStrings.StartLong,
                    FlagStrings.StartShort,
                },

                [CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageEntropy] = new List<string>()
                {
                    FlagStrings.DuplicatedSectorsLong,
                    FlagStrings.DuplicatedSectorsShort,
                    FlagStrings.SeparatedTracksLong,
                    FlagStrings.SeparatedTracksShort,
                    FlagStrings.WholeDiscLong,
                    FlagStrings.WholeDiscShort,
                },

                [CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageInfo] = new List<string>()
                {
                },

                [CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageOptions] = new List<string>()
                {
                },

                [CommandStrings.ImagePrefixLong + " " + CommandStrings.ImagePrint] = new List<string>()
                {
                    FlagStrings.LengthLong,
                    FlagStrings.LengthShort,
                    FlagStrings.LongSectorsLong,
                    FlagStrings.LongSectorsShort,
                    FlagStrings.StartLong,
                    FlagStrings.StartShort,
                    FlagStrings.WidthLong,
                    FlagStrings.WidthShort,
                },

                [CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageVerify] = new List<string>()
                {
                    FlagStrings.VerifyDiscLong,
                    FlagStrings.VerifyDiscShort,
                    FlagStrings.VerifySectorsLong,
                    FlagStrings.VerifySectorsShort,
                },

                #endregion

                #region Media Family

                [CommandStrings.MediaPrefixLong + " " + CommandStrings.MediaDump] = new List<string>()
                {
                    FlagStrings.EjectLong,
                    FlagStrings.EncodingLong,
                    FlagStrings.EncodingShort,
                    FlagStrings.FirstPregapLong,
                    FlagStrings.FixOffsetLong,
                    FlagStrings.FixSubchannelLong,
                    FlagStrings.FixSubchannelCrcLong,
                    FlagStrings.FixSubchannelPositionLong,
                    FlagStrings.ForceLong,
                    FlagStrings.ForceShort,
                    FlagStrings.FormatConvertLong,
                    FlagStrings.FormatConvertShort,
                    FlagStrings.GenerateSubchannelsLong,
                    FlagStrings.MaxBlocksLong,
                    FlagStrings.MetadataLong,
                    FlagStrings.OptionsLong,
                    FlagStrings.OptionsShort,
                    FlagStrings.PersistentLong,
                    FlagStrings.PrivateLong,
                    FlagStrings.ResumeLong,
                    FlagStrings.ResumeShort,
                    FlagStrings.RetryPassesLong,
                    FlagStrings.RetryPassesShort,
                    FlagStrings.RetrySubchannelLong,
                    FlagStrings.SkipLong,
                    FlagStrings.SkipShort,
                    FlagStrings.SkipCdiReadyHoleLong,
                    FlagStrings.SpeedLong,
                    FlagStrings.StopOnErrorLong,
                    FlagStrings.StopOnErrorShort,
                    FlagStrings.StoreEncryptedLong,
                    FlagStrings.SubchannelLong,
                    FlagStrings.TitleKeysLong,
                    FlagStrings.TrimLong,
                    FlagStrings.UseBufferedReadsLong,
                    FlagStrings.XMLSidecarLong,
                    FlagStrings.XMLSidecarShort,
                },

                [CommandStrings.MediaPrefixLong + " " + CommandStrings.MediaInfo] = new List<string>()
                {
                    FlagStrings.OutputPrefixLong,
                    FlagStrings.OutputPrefixShort,
                },

                [CommandStrings.MediaPrefixLong + " " + CommandStrings.MediaScan] = new List<string>()
                {
                    FlagStrings.ImgBurnLogLong,
                    FlagStrings.ImgBurnLogShort,
                    FlagStrings.MHDDLogLong,
                    FlagStrings.MHDDLogShort,
                    FlagStrings.UseBufferedReadsLong,
                },

                #endregion

                #region Standalone Commands

                [CommandStrings.NONE] = new List<string>()
                {
                    FlagStrings.DebugLong,
                    FlagStrings.DebugShort,
                    FlagStrings.HelpLong,
                    FlagStrings.HelpShort,
                    FlagStrings.HelpShortAlt,
                    FlagStrings.VerboseLong,
                    FlagStrings.VerboseShort,
                    FlagStrings.VersionLong,
                },

                [CommandStrings.Configure] = new List<string>()
                {
                },

                [CommandStrings.Formats] = new List<string>()
                {
                },

                [CommandStrings.ListEncodings] = new List<string>()
                {
                },

                [CommandStrings.ListNamespaces] = new List<string>()
                {
                },

                [CommandStrings.Remote] = new List<string>()
                {
                },

                #endregion
            };
        }

        /// <inheritdoc/>
        public override string GetDefaultExtension(MediaType? mediaType) => Converters.Extension(mediaType);

        /// <inheritdoc/>
        public override List<string> GetLogFilePaths(string basePath)
        {
            List<string> logFiles = new List<string>();
            switch (this.Type)
            {
                case MediaType.CDROM:
                    if (File.Exists($"{basePath}.cicm.xml"))
                        logFiles.Add($"{basePath}.cicm.xml");
                    if (File.Exists($"{basePath}.ibg"))
                        logFiles.Add($"{basePath}.ibg");
                    if (File.Exists($"{basePath}.log"))
                        logFiles.Add($"{basePath}.log");
                    if (File.Exists($"{basePath}.mhddlog.bin"))
                        logFiles.Add($"{basePath}.mhddlog.bin");
                    if (File.Exists($"{basePath}.resume.xml"))
                        logFiles.Add($"{basePath}.resume.xml");
                    if (File.Exists($"{basePath}.sub.log"))
                        logFiles.Add($"{basePath}.sub.log");

                    break;

                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                    if (File.Exists($"{basePath}.cicm.xml"))
                        logFiles.Add($"{basePath}.cicm.xml");
                    if (File.Exists($"{basePath}.ibg"))
                        logFiles.Add($"{basePath}.ibg");
                    if (File.Exists($"{basePath}.log"))
                        logFiles.Add($"{basePath}.log");
                    if (File.Exists($"{basePath}.mhddlog.bin"))
                        logFiles.Add($"{basePath}.mhddlog.bin");
                    if (File.Exists($"{basePath}.resume.xml"))
                        logFiles.Add($"{basePath}.resume.xml");

                    break;
            }

            return logFiles;
        }

        /// <inheritdoc/>
        public override bool IsDumpingCommand()
        {
            if (BaseCommand == $"{CommandStrings.MediaPrefixLong} {CommandStrings.MediaDump}")
                return true;

            return false;
        }

        /// <inheritdoc/>
        protected override void ResetValues()
        {
            BaseCommand = CommandStrings.NONE;

            flags = new Dictionary<string, bool?>();

            BlockSizeValue = null;
            CommentsValue = null;
            CreatorValue = null;
            CountValue = null;
            DriveManufacturerValue = null;
            DriveModelValue = null;
            DriveRevisionValue = null;
            DriveSerialValue = null;
            EncodingValue = null;
            FormatConvertValue = null;
            FormatDumpValue = null;
            ImgBurnLogValue = null;
            InputValue = null;
            Input1Value = null;
            Input2Value = null;
            LengthValue = null;
            MediaBarcodeValue = null;
            MediaLastSequenceValue = null;
            MediaManufacturerValue = null;
            MediaModelValue = null;
            MediaPartNumberValue = null;
            MediaSequenceValue = null;
            MediaSerialValue = null;
            MediaTitleValue = null;
            MHDDLogValue = null;
            NamespaceValue = null;
            OptionsValue = null;
            OutputValue = null;
            OutputPrefixValue = null;
            RemoteHostValue = null;
            ResumeFileValue = null;
            RetryPassesValue = null;
            SkipValue = null;
            SpeedValue = null;
            StartValue = null;
            SubchannelValue = null;
            WidthValue = null;
            XMLSidecarValue = null;
        }

        /// <inheritdoc/>
        protected override void SetDefaultParameters(char driveLetter, string filename, int? driveSpeed, Options options)
        {
            BaseCommand = $"{CommandStrings.MediaPrefixLong} {CommandStrings.MediaDump}";

            InputValue = $"{driveLetter}:";
            OutputValue = filename;

            if (driveSpeed != null)
            {
                this[FlagStrings.SpeedLong] = true;
                SpeedValue = (sbyte?)driveSpeed;
            }

            // First check to see if the combination of system and MediaType is valid
            var validTypes = this.System.MediaTypes();
            if (!validTypes.Contains(this.Type))
                return;

            // Set retry count
            if (options.AaruRereadCount > 0)
            {
                this[FlagStrings.RetryPassesLong] = true;
                RetryPassesValue = (short)options.AaruRereadCount;
            }

            // Set user-defined options
            if (options.AaruEnableDebug)
                this[FlagStrings.DebugLong] = options.AaruEnableDebug;
            if (options.AaruEnableVerbose)
                this[FlagStrings.VerboseLong] = options.AaruEnableVerbose;
            if (options.AaruForceDumping)
                this[FlagStrings.ForceLong] = options.AaruForceDumping;
            if (options.AaruStripPersonalData)
                this[FlagStrings.PrivateLong] = options.AaruStripPersonalData;

            // TODO: Look at dump-media formats and the like and see what options there are there to fill in defaults
            // Now sort based on disc type
            switch (this.Type)
            {
                case MediaType.CDROM:
                    // Currently no defaults set
                    break;
                case MediaType.DVD:
                    this[FlagStrings.StoreEncryptedLong] = true; // TODO: Make this configurable
                    this[FlagStrings.TitleKeysLong] = false; // TODO: Make this configurable
                    this[FlagStrings.TrimLong] = true; // TODO: Make this configurable
                    break;
                case MediaType.GDROM:
                    // Currently no defaults set
                    break;
                case MediaType.HDDVD:
                    this[FlagStrings.StoreEncryptedLong] = true; // TODO: Make this configurable
                    this[FlagStrings.TitleKeysLong] = false; // TODO: Make this configurable
                    this[FlagStrings.TrimLong] = true; // TODO: Make this configurable
                    break;
                case MediaType.BluRay:
                    this[FlagStrings.StoreEncryptedLong] = true; // TODO: Make this configurable
                    this[FlagStrings.TitleKeysLong] = false; // TODO: Make this configurable
                    this[FlagStrings.TrimLong] = true; // TODO: Make this configurable
                    break;

                // Special Formats
                case MediaType.NintendoGameCubeGameDisc:
                    // Currently no defaults set
                    break;
                case MediaType.NintendoWiiOpticalDisc:
                    // Currently no defaults set
                    break;

                // Non-optical
                case MediaType.FloppyDisk:
                    // Currently no defaults set
                    break;
            }
        }

        /// <inheritdoc/>
        protected override bool ValidateAndSetParameters(string parameters)
        {
            BaseCommand = CommandStrings.NONE;

            // The string has to be valid by itself first
            if (string.IsNullOrWhiteSpace(parameters))
                return false;

            // Now split the string into parts for easier validation
            // https://stackoverflow.com/questions/14655023/split-a-string-that-has-white-spaces-unless-they-are-enclosed-within-quotes
            parameters = parameters.Trim();
            List<string> parts = Regex.Matches(parameters, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToList();

            // Search for pre-command flags first
            int start = 0;
            for (start = 0; start < parts.Count; start++)
            {
                // Keep a count of keys to determine if we should break out to command handling or not
                int keyCount = Keys.Count();

                // Debug
                ProcessBooleanParameter(parts, FlagStrings.DebugShort, FlagStrings.DebugLong, ref start, true);

                // Pause
                ProcessBooleanParameter(parts, null, FlagStrings.PauseLong, ref start, true);

                // Verbose
                ProcessBooleanParameter(parts, FlagStrings.VerboseShort, FlagStrings.VerboseLong, ref start, true);

                // Version
                ProcessBooleanParameter(parts, null, FlagStrings.VersionLong, ref start, true);

                // Help
                ProcessBooleanParameter(parts, FlagStrings.HelpShort, FlagStrings.HelpLong, ref start, true);
                ProcessBooleanParameter(parts, FlagStrings.HelpShortAlt, FlagStrings.HelpLong, ref start, true);

                // If we didn't add any new flags, break out since we might be at command handling
                if (keyCount == Keys.Count())
                    break;
            }

            // If the required starting index doesn't exist, we can't do anything
            if (!DoesExist(parts, start))
                return false;

            // Determine what the commandline should look like given the first item
            BaseCommand = NormalizeCommand(parts, ref start);
            if (string.IsNullOrWhiteSpace(BaseCommand))
                return false;

            // Set the start position
            start++;

            // Loop through all auxilary flags, if necessary
            int i = 0;
            for (i = start; i < parts.Count; i++)
            {
                // Flag read-out values
                sbyte? byteValue = null;
                short? shortValue = null;
                int? intValue = null;
                long? longValue = null;
                string stringValue = null;

                // Keep a count of keys to determine if we should break out to filename handling or not
                int keyCount = Keys.Count();

                #region Boolean flags

                // Adler-32
                ProcessBooleanParameter(parts, FlagStrings.Adler32Short, FlagStrings.Adler32Long, ref i, true);

                // Clear
                ProcessBooleanParameter(parts, null, FlagStrings.ClearLong, ref i, true);

                // Clear All
                ProcessBooleanParameter(parts, null, FlagStrings.ClearAllLong, ref i, true);

                // CRC16
                ProcessBooleanParameter(parts, null, FlagStrings.CRC16Long, ref i, true);

                // CRC32
                ProcessBooleanParameter(parts, FlagStrings.CRC32Short, FlagStrings.CRC32Long, ref i, true);

                // CRC64
                ProcessBooleanParameter(parts, null, FlagStrings.CRC64Long, ref i, true);

                // Disk Tags
                ProcessBooleanParameter(parts, FlagStrings.DiskTagsShort, FlagStrings.DiskTagsLong, ref i, true);

                // Deduplicated Sectors
                ProcessBooleanParameter(parts, FlagStrings.DuplicatedSectorsShort, FlagStrings.DuplicatedSectorsLong, ref i, true);

                // Eject
                ProcessBooleanParameter(parts, null, FlagStrings.EjectLong, ref i, true);

                // Extended Attributes
                ProcessBooleanParameter(parts, FlagStrings.ExtendedAttributesShort, FlagStrings.ExtendedAttributesLong, ref i, true);

                // Filesystems
                ProcessBooleanParameter(parts, FlagStrings.FilesystemsShort, FlagStrings.FilesystemsLong, ref i, true);

                // First Pregap
                ProcessBooleanParameter(parts, null, FlagStrings.FirstPregapLong, ref i, true);

                // Fix Offset
                ProcessBooleanParameter(parts, null, FlagStrings.FixOffsetLong, ref i, true);

                // Fix Subchannel
                ProcessBooleanParameter(parts, null, FlagStrings.FixSubchannelLong, ref i, true);

                // Fix Subchannel CRC
                ProcessBooleanParameter(parts, null, FlagStrings.FixSubchannelCrcLong, ref i, true);

                // Fix Subchannel Position
                ProcessBooleanParameter(parts, null, FlagStrings.FixSubchannelPositionLong, ref i, true);

                // Fletcher-16
                ProcessBooleanParameter(parts, null, FlagStrings.Fletcher16Long, ref i, true);

                // Fletcher-32
                ProcessBooleanParameter(parts, null, FlagStrings.Fletcher32Long, ref i, true);

                // Force
                ProcessBooleanParameter(parts, FlagStrings.ForceShort, FlagStrings.ForceLong, ref i, true);

                // Generate Subchannels
                ProcessBooleanParameter(parts, null, FlagStrings.GenerateSubchannelsLong, ref i, true);

                // Long Format
                ProcessBooleanParameter(parts, FlagStrings.LongFormatShort, FlagStrings.LongFormatLong, ref i, true);

                // Long Sectors
                ProcessBooleanParameter(parts, FlagStrings.LongSectorsShort, FlagStrings.LongSectorsLong, ref i, true);

                // MD5
                ProcessBooleanParameter(parts, FlagStrings.MD5Short, FlagStrings.MD5Long, ref i, true);

                // Metadata
                ProcessBooleanParameter(parts, null, FlagStrings.MetadataLong, ref i, true);

                // Partitions
                ProcessBooleanParameter(parts, FlagStrings.PartitionsShort, FlagStrings.PartitionsLong, ref i, true);

                // Persistent
                ProcessBooleanParameter(parts, null, FlagStrings.PersistentLong, ref i, true);

                // Private
                ProcessBooleanParameter(parts, null, FlagStrings.PrivateLong, ref i, true);

                // Resume
                ProcessBooleanParameter(parts, FlagStrings.ResumeShort, FlagStrings.ResumeLong, ref i, true);

                // Retry Subchannel
                ProcessBooleanParameter(parts, null, FlagStrings.RetrySubchannelLong, ref i, true);

                // Sector Tags
                ProcessBooleanParameter(parts, FlagStrings.SectorTagsShort, FlagStrings.SectorTagsLong, ref i, true);

                // Separated Tracks
                ProcessBooleanParameter(parts, FlagStrings.SeparatedTracksShort, FlagStrings.SeparatedTracksLong, ref i, true);

                // SHA-1
                ProcessBooleanParameter(parts, FlagStrings.SHA1Short, FlagStrings.SHA1Long, ref i, true);

                // SHA-256
                ProcessBooleanParameter(parts, null, FlagStrings.SHA256Long, ref i, true);

                // SHA-384
                ProcessBooleanParameter(parts, null, FlagStrings.SHA384Long, ref i, true);

                // SHA-512
                ProcessBooleanParameter(parts, null, FlagStrings.SHA512Long, ref i, true);

                // Skip CD-i Ready Hole
                ProcessBooleanParameter(parts, null, FlagStrings.SkipCdiReadyHoleLong, ref i, true);

                // SpamSum
                ProcessBooleanParameter(parts, FlagStrings.SpamSumShort, FlagStrings.SpamSumLong, ref i, true);

                // Stop on Error
                ProcessBooleanParameter(parts, FlagStrings.StopOnErrorShort, FlagStrings.StopOnErrorLong, ref i, true);

                // Store Encrypted
                ProcessBooleanParameter(parts, null, FlagStrings.StoreEncryptedLong, ref i, true);

                // Tape
                ProcessBooleanParameter(parts, FlagStrings.TapeShort, FlagStrings.TapeLong, ref i, true);

                // Title Keys
                ProcessBooleanParameter(parts, null, FlagStrings.TitleKeysLong, ref i, true);

                // Trap Disc
                ProcessBooleanParameter(parts, FlagStrings.TrapDiscShort, FlagStrings.TrapDiscLong, ref i, true);

                // Trim
                ProcessBooleanParameter(parts, null, FlagStrings.TrimLong, ref i, true);

                // Use Buffered Reads
                ProcessBooleanParameter(parts, null, FlagStrings.UseBufferedReadsLong, ref i, true);

                // Verify Disc
                ProcessBooleanParameter(parts, FlagStrings.VerifyDiscShort, FlagStrings.VerifyDiscLong, ref i, true);

                // Verify Sectors
                ProcessBooleanParameter(parts, FlagStrings.VerifySectorsShort, FlagStrings.VerifySectorsLong, ref i, true);

                // Whole Disc
                ProcessBooleanParameter(parts, FlagStrings.WholeDiscShort, FlagStrings.WholeDiscLong, ref i, true);

                #endregion

                #region Int8 flags

                // Speed
                byteValue = ProcessInt8Parameter(parts, null, FlagStrings.SpeedLong, ref i);
                if (byteValue == null && byteValue != SByte.MinValue)
                    SpeedValue = byteValue;

                #endregion

                #region Int16 flags

                // Retry Passes
                shortValue = ProcessInt16Parameter(parts, FlagStrings.RetryPassesShort, FlagStrings.RetryPassesLong, ref i);
                if (shortValue != null && shortValue != Int16.MinValue)
                    RetryPassesValue = shortValue;

                // Width
                shortValue = ProcessInt16Parameter(parts, FlagStrings.WidthShort, FlagStrings.WidthLong, ref i);
                if (shortValue != null && shortValue != Int16.MinValue)
                    WidthValue = shortValue;

                #endregion

                #region Int32 flags

                // Block Size
                intValue = ProcessInt32Parameter(parts, FlagStrings.BlockSizeShort, FlagStrings.BlockSizeLong, ref i);
                if (intValue != null && intValue != Int32.MinValue)
                    BlockSizeValue = intValue;

                // Count
                intValue = ProcessInt32Parameter(parts, FlagStrings.CountShort, FlagStrings.CountLong, ref i);
                if (intValue != null && intValue != Int32.MinValue)
                    CountValue = intValue;

                // MaxBlocks
                intValue = ProcessInt32Parameter(parts, null, FlagStrings.MaxBlocksLong, ref i);
                if (intValue != null && intValue != Int32.MinValue)
                    MaxBlocksValue = intValue;

                // Media Last Sequence
                intValue = ProcessInt32Parameter(parts, null, FlagStrings.MediaLastSequenceLong, ref i);
                if (intValue != null && intValue != Int32.MinValue)
                    MediaLastSequenceValue = intValue;

                // Media Sequence
                intValue = ProcessInt32Parameter(parts, null, FlagStrings.MediaSequenceLong, ref i);
                if (intValue != null && intValue != Int32.MinValue)
                    MediaSequenceValue = intValue;

                // Skip
                intValue = ProcessInt32Parameter(parts, FlagStrings.SkipShort, FlagStrings.SkipLong, ref i);
                if (intValue != null && intValue != Int32.MinValue)
                    SkipValue = intValue;

                #endregion

                #region Int64 flags

                // Length
                longValue = ProcessInt64Parameter(parts, FlagStrings.LengthShort, FlagStrings.LengthLong, ref i);
                if (longValue != null && longValue != Int64.MinValue)
                {
                    LengthValue = longValue;
                }
                else
                {
                    stringValue = ProcessStringParameter(parts, FlagStrings.LengthShort, FlagStrings.LengthLong, ref i);
                    if (string.Equals(stringValue, "all"))
                        LengthValue = -1;
                }

                // Start -- Required value
                longValue = ProcessInt64Parameter(parts, FlagStrings.StartShort, FlagStrings.StartLong, ref i);
                if (longValue == null)
                    return false;
                else if (longValue != Int64.MinValue)
                    StartValue = longValue;

                #endregion

                #region String flags

                // Comments
                stringValue = ProcessStringParameter(parts, null, FlagStrings.CommentsLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    CommentsValue = stringValue;

                // Creator
                stringValue = ProcessStringParameter(parts, null, FlagStrings.CreatorLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    CreatorValue = stringValue;

                // Drive Manufacturer
                stringValue = ProcessStringParameter(parts, null, FlagStrings.DriveManufacturerLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    DriveManufacturerValue = stringValue;

                // Drive Model
                stringValue = ProcessStringParameter(parts, null, FlagStrings.DriveModelLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    DriveModelValue = stringValue;

                // Drive Revision
                stringValue = ProcessStringParameter(parts, null, FlagStrings.DriveRevisionLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    DriveRevisionValue = stringValue;

                // Drive Serial
                stringValue = ProcessStringParameter(parts, null, FlagStrings.DriveSerialLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    DriveSerialValue = stringValue;

                // Encoding -- TODO: List of encodings?
                stringValue = ProcessStringParameter(parts, FlagStrings.EncodingShort, FlagStrings.EncodingLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    EncodingValue = stringValue;

                // Format (Convert) -- TODO: List of formats?
                stringValue = ProcessStringParameter(parts, FlagStrings.FormatConvertShort, FlagStrings.FormatConvertLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    FormatConvertValue = stringValue;

                // Format (Dump) -- TODO: List of formats?
                stringValue = ProcessStringParameter(parts, FlagStrings.FormatDumpShort, FlagStrings.FormatDumpLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    FormatDumpValue = stringValue;

                // Geometry
                stringValue = ProcessStringParameter(parts, FlagStrings.GeometryShort, FlagStrings.GeometryLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    GeometryValue = stringValue;

                // ImgBurn Log
                stringValue = ProcessStringParameter(parts, FlagStrings.ImgBurnLogShort, FlagStrings.ImgBurnLogLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    ImgBurnLogValue = stringValue;

                // Media Barcode
                stringValue = ProcessStringParameter(parts, null, FlagStrings.MediaBarcodeLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    MediaBarcodeValue = stringValue;

                // Media Manufacturer
                stringValue = ProcessStringParameter(parts, null, FlagStrings.MediaManufacturerLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    MediaManufacturerValue = stringValue;

                // Media Model
                stringValue = ProcessStringParameter(parts, null, FlagStrings.MediaModelLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    MediaModelValue = stringValue;

                // Media Part Number
                stringValue = ProcessStringParameter(parts, null, FlagStrings.MediaPartNumberLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    MediaPartNumberValue = stringValue;

                // Media Serial
                stringValue = ProcessStringParameter(parts, null, FlagStrings.MediaSerialLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    MediaSerialValue = stringValue;

                // Media Title
                stringValue = ProcessStringParameter(parts, null, FlagStrings.MediaTitleLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    MediaTitleValue = stringValue;

                // MHDD Log
                stringValue = ProcessStringParameter(parts, FlagStrings.MHDDLogShort, FlagStrings.MHDDLogLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    MHDDLogValue = stringValue;

                // Namespace
                stringValue = ProcessStringParameter(parts, FlagStrings.NamespaceShort, FlagStrings.NamespaceLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    NamespaceValue = stringValue;

                // Options -- TODO: Validate options?
                stringValue = ProcessStringParameter(parts, FlagStrings.OptionsShort, FlagStrings.OptionsLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    OptionsValue = stringValue;

                // Output Prefix
                stringValue = ProcessStringParameter(parts, FlagStrings.OutputPrefixShort, FlagStrings.OutputPrefixLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    OutputPrefixValue = stringValue;

                // Resume File
                stringValue = ProcessStringParameter(parts, FlagStrings.ResumeFileShort, FlagStrings.ResumeFileLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    ResumeFileValue = stringValue;

                // Subchannel
                stringValue = ProcessStringParameter(parts, null, FlagStrings.SubchannelLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                {
                    if (string.Equals(stringValue, "any")
                        || string.Equals(stringValue, "rw")
                        || string.Equals(stringValue, "rw-or-pq")
                        || string.Equals(stringValue, "pq")
                        || string.Equals(stringValue, "none")
                        )
                    {
                        SubchannelValue = stringValue;
                    }
                    else
                    {
                        SubchannelValue = "any";
                    }
                }

                // XML Sidecar
                stringValue = ProcessStringParameter(parts, FlagStrings.XMLSidecarShort, FlagStrings.XMLSidecarLong, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    XMLSidecarValue = stringValue;

                #endregion

                // If we didn't add any new flags, break out since we might be at filename handling
                if (keyCount == Keys.Count())
                    break;
            }

            // Handle filenames based on command, if necessary
            switch (BaseCommand)
            {
                // Input value only
                case CommandStrings.ArchivePrefixLong + " " + CommandStrings.ArchiveInfo:
                case CommandStrings.DevicePrefixLong + " " + CommandStrings.DeviceInfo:
                case CommandStrings.DevicePrefixLong + " " + CommandStrings.DeviceReport:
                case CommandStrings.FilesystemPrefixLong + " " + CommandStrings.FilesystemInfo:
                case CommandStrings.FilesystemPrefixLong + " " + CommandStrings.FilesystemListLong:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageChecksumLong:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageCreateSidecar:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageDecode:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageEntropy:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageInfo:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImagePrint:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageVerify:
                case CommandStrings.MediaPrefixLong + " " + CommandStrings.MediaInfo:
                case CommandStrings.MediaPrefixLong + " " + CommandStrings.MediaScan:
                    if (!DoesExist(parts, i))
                        return false;

                    InputValue = parts[i].Trim('"');
                    i++;
                    break;

                // Two input values
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageCompareLong:
                    if (!DoesExist(parts, i))
                        return false;

                    Input1Value = parts[i].Trim('"');
                    i++;

                    if (!DoesExist(parts, i))
                        return false;

                    Input2Value = parts[i].Trim('"');
                    i++;
                    break;

                // Input and Output value
                case CommandStrings.FilesystemPrefixLong + " " + CommandStrings.FilesystemExtract:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageConvert:
                case CommandStrings.MediaPrefixLong + " " + CommandStrings.MediaDump:
                    if (!DoesExist(parts, i))
                        return false;

                    InputValue = parts[i].Trim('"');
                    i++;

                    if (!DoesExist(parts, i))
                        return false;

                    OutputValue = parts[i].Trim('"');
                    i++;
                    break;

                // Remote host value only
                case CommandStrings.DevicePrefixLong + " " + CommandStrings.DeviceList:
                case CommandStrings.Remote:
                    if (!DoesExist(parts, i))
                        return false;

                    RemoteHostValue = parts[i].Trim('"');
                    i++;
                    break;
            }

            // If we didn't reach the end for some reason, it failed
            if (i != parts.Count)
                return false;

            return true;
        }

        #endregion

        #region Private Extra Methods

        /// <summary>
        /// Normalize a command string to use long form values for easier lookup
        /// </summary>
        /// <param name="baseCommand">Command string to normalize</param>
        /// <returns>Normalized command</returns>
        private string NormalizeCommand(List<string> parts, ref int start)
        {
            // Invalid start means invalid command
            if (start < 0 || start >= parts.Count)
                return null;

            string partOne = parts[start];
            string partTwo = string.Empty;
            if (start + 1 < parts.Count)
                partTwo = parts[start + 1];

            string normalized = NormalizeCommand($"{partOne} {partTwo}".Trim());

            // Null normalization means invalid command
            if (normalized == null)
                return null;

            // Determine if start should be incremented
            if (normalized.Split(' ').Length > 1)
                start++;

            return normalized;
        }

        /// <summary>
        /// Normalize a command string to use long form values for easier lookup
        /// </summary>
        /// <param name="baseCommand">Command string to normalize</param>
        /// <returns>Normalized command</returns>
        private string NormalizeCommand(string baseCommand)
        {
            // If the base command is inavlid, just return nulls
            if (string.IsNullOrWhiteSpace(baseCommand))
                return null;

            // Split the command otherwise
            string[] splitCommand = baseCommand.Split(' ');
            string family, command;


            // For commands with a family
            if (splitCommand.Length > 1)
            {
                // Handle the family first
                switch (splitCommand[0])
                {
                    case CommandStrings.ArchivePrefixShort:
                    case CommandStrings.ArchivePrefixLong:
                        family = CommandStrings.ArchivePrefixLong;
                        switch (splitCommand[1])
                        {
                            case CommandStrings.ArchiveInfo:
                                command = CommandStrings.ArchiveInfo;
                                break;
                            default:
                                command = null;
                                break;
                        }

                        break;
                    case CommandStrings.DatabasePrefixShort:
                    case CommandStrings.DatabasePrefixLong:
                        family = CommandStrings.DatabasePrefixLong;
                        switch (splitCommand[1])
                        {
                            case CommandStrings.DatabaseStats:
                                command = CommandStrings.DatabaseStats;
                                break;
                            case CommandStrings.DatabaseUpdate:
                                command = CommandStrings.DatabaseUpdate;
                                break;
                            default:
                                command = null;
                                break;
                        }

                        break;

                    case CommandStrings.DevicePrefixShort:
                    case CommandStrings.DevicePrefixLong:
                        family = CommandStrings.DevicePrefixLong;
                        switch (splitCommand[1])
                        {
                            case CommandStrings.DeviceInfo:
                                command = CommandStrings.DeviceInfo;
                                break;
                            case CommandStrings.DeviceList:
                                command = CommandStrings.DeviceList;
                                break;
                            case CommandStrings.DeviceReport:
                                command = CommandStrings.DeviceReport;
                                break;
                            default:
                                command = null;
                                break;
                        }

                        break;

                    case CommandStrings.FilesystemPrefixShort:
                    case CommandStrings.FilesystemPrefixShortAlt:
                    case CommandStrings.FilesystemPrefixLong:
                        family = CommandStrings.FilesystemPrefixLong;
                        switch (splitCommand[1])
                        {
                            case CommandStrings.FilesystemExtract:
                                command = CommandStrings.FilesystemExtract;
                                break;
                            case CommandStrings.FilesystemInfo:
                                command = CommandStrings.FilesystemInfo;
                                break;
                            case CommandStrings.FilesystemListShort:
                            case CommandStrings.FilesystemListLong:
                                command = CommandStrings.FilesystemListLong;
                                break;
                            case CommandStrings.FilesystemOptions:
                                command = CommandStrings.FilesystemOptions;
                                break;
                            default:
                                command = null;
                                break;
                        }

                        break;

                    case CommandStrings.ImagePrefixShort:
                    case CommandStrings.ImagePrefixLong:
                        family = CommandStrings.ImagePrefixLong;
                        switch (splitCommand[1])
                        {
                            case CommandStrings.ImageChecksumShort:
                            case CommandStrings.ImageChecksumLong:
                                command = CommandStrings.ImageChecksumLong;
                                break;
                            case CommandStrings.ImageCompareShort:
                            case CommandStrings.ImageCompareLong:
                                command = CommandStrings.ImageChecksumLong;
                                break;
                            case CommandStrings.ImageConvert:
                                command = CommandStrings.ImageConvert;
                                break;
                            case CommandStrings.ImageCreateSidecar:
                                command = CommandStrings.ImageCreateSidecar;
                                break;
                            case CommandStrings.ImageDecode:
                                command = CommandStrings.ImageDecode;
                                break;
                            case CommandStrings.ImageEntropy:
                                command = CommandStrings.ImageEntropy;
                                break;
                            case CommandStrings.ImageInfo:
                                command = CommandStrings.ImageInfo;
                                break;
                            case CommandStrings.ImageOptions:
                                command = CommandStrings.ImageOptions;
                                break;
                            case CommandStrings.ImagePrint:
                                command = CommandStrings.ImagePrint;
                                break;
                            case CommandStrings.ImageVerify:
                                command = CommandStrings.ImageVerify;
                                break;
                            default:
                                command = null;
                                break;
                        }

                        break;

                    case CommandStrings.MediaPrefixShort:
                    case CommandStrings.MediaPrefixLong:
                        family = CommandStrings.MediaPrefixLong;
                        switch (splitCommand[1])
                        {
                            case CommandStrings.MediaDump:
                                command = CommandStrings.MediaDump;
                                break;
                            case CommandStrings.MediaInfo:
                                command = CommandStrings.MediaInfo;
                                break;
                            case CommandStrings.MediaScan:
                                command = CommandStrings.MediaScan;
                                break;
                            default:
                                command = null;
                                break;
                        }

                        break;

                    default:
                        family = null;
                        command = null;
                        break;
                }
            }

            // For standalone commands
            else
            {
                family = null;
                switch (splitCommand[0])
                {
                    case CommandStrings.Configure:
                        command = CommandStrings.Configure;
                        break;
                    case CommandStrings.Formats:
                        command = CommandStrings.Formats;
                        break;
                    case CommandStrings.ListEncodings:
                        command = CommandStrings.ListEncodings;
                        break;
                    case CommandStrings.ListNamespaces:
                        command = CommandStrings.ListNamespaces;
                        break;
                    case CommandStrings.Remote:
                        command = CommandStrings.Remote;
                        break;
                    default:
                        command = null;
                        break;
                }
            }

            // If the command itself is invalid, then return null
            if (string.IsNullOrWhiteSpace(command))
                return null;

            // Combine the result
            if (!string.IsNullOrWhiteSpace(family))
                return $"{family} {command}";
            else
                return command;
        }

        #endregion

        #region Information Extraction Methods

        /// <summary>
        /// Convert the TrackTypeTrackType value to a CueTrackDataType
        /// </summary>
        /// <param name="trackType">TrackTypeTrackType to convert</param>
        /// <param name="bytesPerSector">Sector size to help with specific subtypes</param>
        /// <returns>CueTrackDataType representing the input data</returns>
        private CueTrackDataType ConvertToDataType(TrackTypeTrackType trackType, uint bytesPerSector)
        {
            switch (trackType)
            {
                case TrackTypeTrackType.audio:
                    return CueTrackDataType.AUDIO;

                case TrackTypeTrackType.mode1:
                    if (bytesPerSector == 2048)
                        return CueTrackDataType.MODE1_2048;
                    else
                        return CueTrackDataType.MODE1_2352;

                case TrackTypeTrackType.mode2:
                case TrackTypeTrackType.m2f1:
                case TrackTypeTrackType.m2f2:
                    if (bytesPerSector == 2336)
                        return CueTrackDataType.MODE2_2336;
                    else
                        return CueTrackDataType.MODE2_2352;

                default:
                    return CueTrackDataType.MODE1_2352;
            }
        }

        /// <summary>
        /// Convert the TrackFlagsType value to a CueTrackFlag
        /// </summary>
        /// <param name="trackFlagsType">TrackFlagsType containing flag data</param>
        /// <returns>CueTrackFlag representing the flags</returns>
        private CueTrackFlag ConvertToTrackFlag(TrackFlagsType trackFlagsType)
        {
            if (trackFlagsType == null)
                return 0;

            CueTrackFlag flag = 0;

            if (trackFlagsType.CopyPermitted)
                flag |= CueTrackFlag.DCP;

            if (trackFlagsType.Quadraphonic)
                flag |= CueTrackFlag.FourCH;

            if (trackFlagsType.PreEmphasis)
                flag |= CueTrackFlag.PRE;

            return flag;
        }

        /// <summary>
        /// Generate a cuesheet string based on CICM sidecar data
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <param name="basePath">Base path for determining file names</param>
        /// <returns>String containing the cuesheet, null on error</returns>
        private string GenerateCuesheet(CICMMetadataType cicmSidecar, string basePath)
        {
            // If the object is null, we can't get information from it
            if (cicmSidecar == null)
                return null;

            // Required variables
            uint totalTracks = 0;
            CueSheet cueSheet = new CueSheet
            {
                Performer = string.Join(", ", cicmSidecar.Performer ?? new string[0]),
                Files = new List<CueFile>(),
            };

            // Only care about OpticalDisc types
            if (cicmSidecar.OpticalDisc == null || cicmSidecar.OpticalDisc.Length == 0)
                return null;

            // Loop through each OpticalDisc in the metadata
            foreach (OpticalDiscType opticalDisc in cicmSidecar.OpticalDisc)
            {
                // Only capture the first total track count
                if (opticalDisc.Tracks != null && opticalDisc.Tracks.Length > 0)
                    totalTracks = opticalDisc.Tracks[0];

                // If there are no tracks, we can't get a cuesheet
                if (opticalDisc.Track == null || opticalDisc.Track.Length == 0)
                    continue;

                // Get cuesheet-level information
                cueSheet.Catalog = opticalDisc.MediaCatalogueNumber;

                // Loop through each track
                foreach (TrackType track in opticalDisc.Track)
                {
                    // Create cue track entry
                    CueTrack cueTrack = new CueTrack
                    {
                        Number = (int)(track.Sequence?.TrackNumber ?? 0),
                        DataType = ConvertToDataType(track.TrackType1, track.BytesPerSector),
                        Flags = ConvertToTrackFlag(track.Flags),
                        ISRC = track.ISRC,
                    };

                    // Create cue file entry
                    CueFile cueFile = new CueFile
                    {
                        FileName = GenerateTrackName(basePath, (int)totalTracks, cueTrack.Number, opticalDisc.DiscType),
                        FileType = CueFileType.BINARY,
                        Tracks = new List<CueTrack>(),
                    };

                    // Add index data
                    if (track.Indexes != null && track.Indexes.Length > 0)
                    {
                        cueTrack.Indices = new List<CueIndex>();

                        // Loop through each index
                        foreach (TrackIndexType trackIndex in track.Indexes)
                        {
                            // Get timestamp from frame count
                            int absoluteLength = Math.Abs(trackIndex.Value);
                            int frames = absoluteLength % 75;
                            int seconds = (absoluteLength / 75) % 60;
                            int minutes = (absoluteLength / 75 / 60);
                            string timeString = $"{minutes:D2}:{seconds:D2}:{frames:D2}";

                            // Pregap information
                            if (trackIndex.Value < 0)
                                cueTrack.PreGap = new PreGap(timeString);

                            // Individual indexes
                            else
                                cueTrack.Indices.Add(new CueIndex(trackIndex.index.ToString(), timeString));
                        }
                    }
                    else
                    {
                        // Default if index data missing from sidecar
                        cueTrack.Indices = new List<CueIndex>()
                        {
                            new CueIndex("01", "00:00:00"),
                        };
                    }

                    // Add the track to the file
                    cueFile.Tracks.Add(cueTrack);

                    // Add the file to the cuesheet
                    cueSheet.Files.Add(cueFile);
                }
            }

            // If we have a cuesheet to write out, do so
            if (cueSheet != null && cueSheet != default)
            {
                MemoryStream ms = new MemoryStream();
                cueSheet.Write(ms);
                ms.Position = 0;
                using (var sr = new StreamReader(ms))
                {
                    return sr.ReadToEnd();
                }
            }

            return null;
        }

        /// <summary>
        /// Generate a CMP XML datfile string based on CICM sidecar data
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <param name="basePath">Base path for determining file names</param>
        /// <returns>String containing the datfile, null on error</returns>
        private static string GenerateDatfile(CICMMetadataType cicmSidecar, string basePath)
        {
            // If the object is null, we can't get information from it
            if (cicmSidecar == null)
                return null;

            // Required variables
            string datfile = string.Empty;

            // Process OpticalDisc, if possible
            if (cicmSidecar.OpticalDisc != null && cicmSidecar.OpticalDisc.Length > 0)
            {
                // Loop through each OpticalDisc in the metadata
                foreach (OpticalDiscType opticalDisc in cicmSidecar.OpticalDisc)
                {
                    // Only capture the first total track count
                    uint totalTracks = 0;
                    if (opticalDisc.Tracks != null && opticalDisc.Tracks.Length > 0)
                        totalTracks = opticalDisc.Tracks[0];

                    // If there are no tracks, we can't get a datfile
                    if (opticalDisc.Track == null || opticalDisc.Track.Length == 0)
                        continue;

                    // Loop through each track
                    foreach (TrackType track in opticalDisc.Track)
                    {
                        uint trackNumber = track.Sequence?.TrackNumber ?? 0;
                        ulong size = track.Size;
                        string crc32 = string.Empty;
                        string md5 = string.Empty;
                        string sha1 = string.Empty;

                        // If we don't have any checksums, we can't get a DAT for this track
                        if (track.Checksums == null || track.Checksums.Length == 0)
                            continue;

                        // Extract only relevant checksums
                        foreach (ChecksumType checksum in track.Checksums)
                        {
                            switch (checksum.type)
                            {
                                case ChecksumTypeType.crc32:
                                    crc32 = checksum.Value;
                                    break;
                                case ChecksumTypeType.md5:
                                    md5 = checksum.Value;
                                    break;
                                case ChecksumTypeType.sha1:
                                    sha1 = checksum.Value;
                                    break;
                            }
                        }

                        // Build the track datfile data and append
                        string trackName = GenerateTrackName(basePath, (int)totalTracks, (int)trackNumber, opticalDisc.DiscType);
                        datfile += $"<rom name=\"{trackName}\" size=\"{size}\" crc=\"{crc32}\" md5=\"{md5}\" sha1=\"{sha1}\" />\n";
                    }
                }
            }

            // Process BlockMedia, if possible
            if (cicmSidecar.BlockMedia != null && cicmSidecar.BlockMedia.Length > 0)
            {
                // Loop through each BlockMedia in the metadata
                foreach (BlockMediaType blockMedia in cicmSidecar.BlockMedia)
                {
                    ulong size = blockMedia.Size;
                    string crc32 = string.Empty;
                    string md5 = string.Empty;
                    string sha1 = string.Empty;

                    // If we don't have any checksums, we can't get a DAT for this track
                    if (blockMedia.Checksums == null || blockMedia.Checksums.Length == 0)
                        continue;

                    // Extract only relevant checksums
                    foreach (ChecksumType checksum in blockMedia.Checksums)
                    {
                        switch (checksum.type)
                        {
                            case ChecksumTypeType.crc32:
                                crc32 = checksum.Value;
                                break;
                            case ChecksumTypeType.md5:
                                md5 = checksum.Value;
                                break;
                            case ChecksumTypeType.sha1:
                                sha1 = checksum.Value;
                                break;
                        }
                    }

                    // Build the track datfile data and append
                    string trackName = $"{basePath}.bin";
                    datfile += $"<rom name=\"{trackName}\" size=\"{size}\" crc=\"{crc32}\" md5=\"{md5}\" sha1=\"{sha1}\" />\n";
                }
            }

            return datfile;
        }

        /// <summary>
        /// Generate a track name based on current path and tracks
        /// </summary>
        /// <param name="basePath">Base path for determining file names</param>
        /// <param name="totalTracks">Total number of tracks in the media</param>
        /// <param name="trackNumber">Current track index</param>
        /// <param name="discType">Current disc type, used for determining extension</param>
        /// <returns>Formatted string representing the track name according to Redump standards</returns>
        private static string GenerateTrackName(string basePath, int totalTracks, int trackNumber, string discType)
        {
            string extension = "bin";
            if (discType.Contains("BD") || discType.Contains("DVD"))
                extension = "iso";

            string trackName = Path.GetFileNameWithoutExtension(basePath);
            if (totalTracks == 1)
                trackName = $"{trackName}.{extension}";
            else if (totalTracks > 1 && totalTracks < 10)
                trackName = $"{trackName} (Track {trackNumber}).{extension}";
            else
                trackName = $"{trackName} (Track {trackNumber:D2}).{extension}";

            return trackName;
        }

        /// <summary>
        /// Generate a Redump-compatible PVD block based on CICM sidecar file
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <returns>String containing the PVD, null on error</returns>
        private static string GeneratePVD(CICMMetadataType cicmSidecar)
        {
            // If the object is null, we can't get information from it
            if (cicmSidecar == null)
                return null;

            // Process OpticalDisc, if possible
            if (cicmSidecar.OpticalDisc != null || cicmSidecar.OpticalDisc.Length > 0)
            {
                // Loop through each OpticalDisc in the metadata
                foreach (OpticalDiscType opticalDisc in cicmSidecar.OpticalDisc)
                {
                    byte[] pvdData = GeneratePVDData(opticalDisc);

                    // If we got a null value, we skip this disc
                    if (pvdData == null)
                        continue;

                    // Build each row in consecutive order
                    string pvd = string.Empty;
                    pvd += GenerateSectorOutputLine("0320", new ReadOnlySpan<byte>(pvdData, 0, 16));
                    pvd += GenerateSectorOutputLine("0330", new ReadOnlySpan<byte>(pvdData, 16, 16));
                    pvd += GenerateSectorOutputLine("0340", new ReadOnlySpan<byte>(pvdData, 32, 16));
                    pvd += GenerateSectorOutputLine("0350", new ReadOnlySpan<byte>(pvdData, 48, 16));
                    pvd += GenerateSectorOutputLine("0360", new ReadOnlySpan<byte>(pvdData, 64, 16));
                    pvd += GenerateSectorOutputLine("0370", new ReadOnlySpan<byte>(pvdData, 80, 16));

                    return pvd;
                }
            }

            return null;
        }

        /// <summary>
        /// Generate the byte array representing the current PVD information
        /// </summary>
        /// <param name="opticalDisc">OpticalDisc type from CICM Sidecar data</param>
        /// <returns>Byte array representing the PVD, null on error</returns>
        private static byte[] GeneratePVDData(OpticalDiscType opticalDisc)
        {
            // Required variables
            DateTime creation = DateTime.MinValue;
            DateTime modification = DateTime.MinValue;
            DateTime expiration = DateTime.MinValue;
            DateTime effective = DateTime.MinValue;

            // If there are no tracks, we can't get a PVD
            if (opticalDisc.Track == null || opticalDisc.Track.Length == 0)
                return null;

            // Take the first track only
            TrackType track = opticalDisc.Track[0];

            // If there are no partitions, we can't get a PVD
            if (track.FileSystemInformation == null || track.FileSystemInformation.Length == 0)
                return null;

            // Loop through each Partition
            foreach (PartitionType partition in track.FileSystemInformation)
            {
                // If the partition has no file systems, we can't get a PVD
                if (partition.FileSystems == null || partition.FileSystems.Length == 0)
                    continue;

                // Loop through each FileSystem until we find a PVD
                foreach (FileSystemType fileSystem in partition.FileSystems)
                {
                    // If we don't have a PVD-able filesystem, we can't get a PVD
                    if (!fileSystem.CreationDateSpecified
                        && !fileSystem.ModificationDateSpecified
                        && !fileSystem.ExpirationDateSpecified
                        && !fileSystem.EffectiveDateSpecified)
                    {
                        continue;
                    }

                    // Creation Date
                    if (fileSystem.CreationDateSpecified)
                        creation = fileSystem.CreationDate;

                    // Modification Date
                    if (fileSystem.ModificationDateSpecified)
                        modification = fileSystem.ModificationDate;

                    // Expiration Date
                    if (fileSystem.ExpirationDateSpecified)
                        expiration = fileSystem.ExpirationDate;

                    // Effective Date
                    if (fileSystem.EffectiveDateSpecified)
                        effective = fileSystem.EffectiveDate;

                    break;
                }

                // If we found a Partition with PVD data, we break
                if (creation != DateTime.MinValue
                    || modification != DateTime.MinValue
                    || expiration != DateTime.MinValue
                    || effective != DateTime.MinValue)
                {
                    break;
                }
            }

            // If we found no partitions, we return null
            if (creation == DateTime.MinValue
                && modification == DateTime.MinValue
                && expiration == DateTime.MinValue
                && effective == DateTime.MinValue)
            {
                return null;
            }

            // Now generate the byte array data
            List<byte> pvdData = new List<byte>();
            pvdData.AddRange(new string(' ', 13).ToCharArray().Select(c => (byte)c));
            pvdData.AddRange(GeneratePVDDateTimeBytes(creation));
            pvdData.AddRange(GeneratePVDDateTimeBytes(modification));
            pvdData.AddRange(GeneratePVDDateTimeBytes(expiration));
            pvdData.AddRange(GeneratePVDDateTimeBytes(effective));
            pvdData.Add(0x01);
            pvdData.AddRange(new string((char)0, 14).ToCharArray().Select(c => (byte)c));

            // Return the filled array
            return pvdData.ToArray();
        }

        /// <summary>
        /// Generate the required bytes from a DateTime object
        /// </summary>
        /// <param name="dateTime">DateTime to get representation of</param>
        /// <returns>Byte array representing the DateTime</returns>
        private static byte[] GeneratePVDDateTimeBytes(DateTime dateTime)
        {
            string emptyTime = "0000000000000000";
            string dateTimeString = emptyTime;
            byte timeZoneNumber = 0;

            // If we don't have default values, set the proper string
            if (dateTime != DateTime.MinValue)
            {
                dateTimeString = dateTime.ToString("yyyyMMddHHmmssff");

                // Get timezone offset (0 == GMT, up and down in 15-minute increments)
                string timeZoneString;
                try
                {
                    timeZoneString = dateTime.ToString("zzz");
                }
                catch
                {
                    timeZoneString = "00:00";
                }

                // Format is hh:mm
                string[] splitTimeZoneString = timeZoneString.Split(':');
                if (int.TryParse(splitTimeZoneString[0], out int hours))
                    timeZoneNumber += (byte)(hours * 4);
                if (int.TryParse(splitTimeZoneString[1], out int minutes))
                    timeZoneNumber += (byte)(minutes / 15);
            }

            // Get and return the byte array
            List<byte> dateTimeList = dateTimeString.ToCharArray().Select(c => (byte)c).ToList();
            dateTimeList.Add(timeZoneNumber);
            return dateTimeList.ToArray();
        }

        /// <summary>
        /// Generate a single 16-byte sector line from a byte array
        /// </summary>
        /// <param name="row">Row ID for outputting</param>
        /// <param name="bytes">Byte span representing the data to write</param>
        /// <returns>Formatted string representing the sector line</returns>
        private static string GenerateSectorOutputLine(string row, ReadOnlySpan<byte> bytes)
        {
            // If the data isn't correct, return null
            if (bytes == null || bytes.Length != 16)
                return null;

            string pvdLine = $"{row} : ";
            pvdLine += BitConverter.ToString(bytes.Slice(0, 8).ToArray()).Replace("-", " ");
            pvdLine += "  ";
            pvdLine += BitConverter.ToString(bytes.Slice(8, 8).ToArray().ToArray()).Replace("-", " ");
            pvdLine += "   ";
            pvdLine += Encoding.ASCII.GetString(bytes.ToArray()).Replace((char)0, '.').Replace('?', '.');
            pvdLine += "\n";

            return pvdLine;
        }

        /// <summary>
        /// Read the CICM Sidecar as an object
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <returns>Object containing the data, null on error</returns>
        private static CICMMetadataType GenerateSidecar(string cicmSidecar)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(cicmSidecar))
                return null;

            // Open and read in the XML file
            XmlReader xtr = XmlReader.Create(cicmSidecar, new XmlReaderSettings
            {
                CheckCharacters = false,
                DtdProcessing = DtdProcessing.Ignore,
                IgnoreComments = true,
                IgnoreWhitespace = true,
                ValidationFlags = XmlSchemaValidationFlags.None,
                ValidationType = ValidationType.None,
            });

            // If the reader is null for some reason, we can't do anything
            if (xtr == null)
                return null;

            XmlSerializer serializer = new XmlSerializer(typeof(CICMMetadataType));
            CICMMetadataType obj = serializer.Deserialize(xtr) as CICMMetadataType;

            return obj;
        }

        /// <summary>
        /// Get reported disc type information, if possible
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <returns>True if disc type info was set, false otherwise</returns>
        private static bool GetDiscType(CICMMetadataType cicmSidecar, out string discType, out string discSubType)
        {
            // Set the default values
            discType = null; discSubType = null;

            // If the object is null, we can't get information from it
            if (cicmSidecar == null)
                return false;

            // Only care about OpticalDisc types
            if (cicmSidecar.OpticalDisc == null || cicmSidecar.OpticalDisc.Length == 0)
                return false;

            // Find and return the hardware info, if possible
            foreach (OpticalDiscType opticalDisc in cicmSidecar.OpticalDisc)
            {
                // Store the first instance of each value
                if (string.IsNullOrEmpty(discType) && !string.IsNullOrEmpty(opticalDisc.DiscType))
                    discType = opticalDisc.DiscType;
                if (string.IsNullOrEmpty(discSubType) && !string.IsNullOrEmpty(opticalDisc.DiscSubType))
                    discSubType = opticalDisc.DiscSubType;
            }

            return !string.IsNullOrEmpty(discType) || !string.IsNullOrEmpty(discSubType);
        }

        /// <summary>
        /// Get the DVD protection information, if possible
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <returns>Formatted string representing the DVD protection, null on error</returns>
        private static string GetDVDProtection(CICMMetadataType cicmSidecar)
        {
            // If the object is null, we can't get information from it
            if (cicmSidecar == null)
                return null;

            // Only care about OpticalDisc types
            if (cicmSidecar.OpticalDisc == null || cicmSidecar.OpticalDisc.Length == 0)
                return null;

            // Get an output for the copyright protection
            string copyrightProtectionSystemType = string.Empty;

            // Loop through each OpticalDisc in the metadata
            foreach (OpticalDiscType opticalDisc in cicmSidecar.OpticalDisc)
            {
                if (!string.IsNullOrWhiteSpace(opticalDisc.CopyProtection))
                    copyrightProtectionSystemType += $", {opticalDisc.CopyProtection}";
            }

            // Trim the values
            copyrightProtectionSystemType = copyrightProtectionSystemType.TrimStart(',').Trim();

            // TODO: Note- Most of the below values are not currently captured by Aaru.
            // At the time of writing, there are open issues to capture more of this
            // information and store it in the output. For now, only the copyright
            // protection system can be retrieved.

            // Now we format everything we can
            string protection = string.Empty;
            //if (!string.IsNullOrEmpty(region))
            //    protection += $"Region: {region}\n";
            //if (!string.IsNullOrEmpty(rceProtection))
            //    protection += $"RCE Protection: {rceProtection}\n";
            if (!string.IsNullOrEmpty(copyrightProtectionSystemType))
                protection += $"Copyright Protection System Type: {copyrightProtectionSystemType}\n";
            //if (!string.IsNullOrEmpty(vobKeys))
            //    protection += vobKeys;
            //if (!string.IsNullOrEmpty(decryptedDiscKey))
            //    protection += $"Decrypted Disc Key: {decryptedDiscKey}\n";

            return protection;
        }

        /// <summary>
        /// Get the detected error count from the input files, if possible
        /// </summary>
        /// <param name="resume">.resume.xml file location</param>
        /// <returns>Error count if possible, -1 on error</returns>
        private static long GetErrorCount(string resume)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(resume))
                return -1;

            // Get a total error count for after
            long? totalErrors = null;

            // Parse the resume XML file
            using (StreamReader sr = File.OpenText(resume))
            {
                try
                {
                    // Read in the error count whenever we find it
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine().Trim();

                        // Initialize on seeing the open tag
                        if (line.StartsWith("<BadBlocks>"))
                            totalErrors = 0;
                        else if (line.StartsWith("</BadBlocks>"))
                            return totalErrors ?? -1;
                        else if (line.StartsWith("<Block>") && totalErrors != null)
                            totalErrors++;
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
        }

        /// <summary>
        /// Get hardware information, if possible
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <returns>True if hardware info was set, false otherwise</returns>
        private static bool GetHardwareInfo(CICMMetadataType cicmSidecar, out string manufacturer, out string model, out string firmware)
        {
            // Set the default values
            manufacturer = null; model = null; firmware = null;

            // If the object is null, we can't get information from it
            if (cicmSidecar == null)
                return false;

            // Only care about OpticalDisc types
            if (cicmSidecar.OpticalDisc == null || cicmSidecar.OpticalDisc.Length == 0)
                return false;

            // Find and return the hardware info, if possible
            foreach (OpticalDiscType opticalDisc in cicmSidecar.OpticalDisc)
            {
                // If there's no hardware information, skip
                if (opticalDisc.DumpHardwareArray == null || !opticalDisc.DumpHardwareArray.Any())
                    continue;

                foreach (DumpHardwareType hardware in opticalDisc.DumpHardwareArray)
                {
                    // If the hardware information is invalid, skip
                    if (hardware == null)
                        continue;

                    // Store the first instance of each value
                    if (string.IsNullOrEmpty(manufacturer) && !string.IsNullOrEmpty(hardware.Manufacturer))
                        manufacturer = hardware.Manufacturer;
                    if (string.IsNullOrEmpty(model) && !string.IsNullOrEmpty(hardware.Model))
                        model = hardware.Model;
                    if (string.IsNullOrEmpty(firmware) && !string.IsNullOrEmpty(hardware.Firmware))
                        firmware = hardware.Firmware;
                }
            }

            return !string.IsNullOrEmpty(manufacturer) || !string.IsNullOrEmpty(model) || !string.IsNullOrEmpty(firmware);
        }

        /// <summary>
        /// Get the layerbreak from the input file, if possible
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <returns>Layerbreak if possible, null on error</returns>
        private static string GetLayerbreak(CICMMetadataType cicmSidecar)
        {
            // If the object is null, we can't get information from it
            if (cicmSidecar == null)
                return null;

            // Only care about OpticalDisc types
            if (cicmSidecar.OpticalDisc == null || cicmSidecar.OpticalDisc.Length == 0)
                return null;

            // Setup the layerbreak
            string layerbreak = null;

            // Find and return the layerbreak, if possible
            foreach (OpticalDiscType opticalDisc in cicmSidecar.OpticalDisc)
            {
                // If there's no layer information, skip
                if (opticalDisc.Layers == null)
                    continue;

                // TODO: Determine how to find the layerbreak from the CICM or other outputs
            }

            return layerbreak;
        }

        /// <summary>
        /// Get the write offset from the CICM Sidecar file, if possible
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <returns>Sample write offset if possible, null on error</returns>
        private static string GetWriteOffset(CICMMetadataType cicmSidecar)
        {
            // If the object is null, we can't get information from it
            if (cicmSidecar == null)
                return null;

            // Only care about OpticalDisc types
            if (cicmSidecar.OpticalDisc == null || cicmSidecar.OpticalDisc.Length == 0)
                return null;

            // Loop through each OpticalDisc in the metadata
            foreach (OpticalDiscType opticalDisc in cicmSidecar.OpticalDisc)
            {
                // If the disc doesn't have an offset specified, we skip it;
                if (!opticalDisc.OffsetSpecified)
                    continue;

                return opticalDisc.Offset.ToString();
            }

            return null;
        }

        /// <summary>
        /// Get the XGD auxiliary info from the CICM Sidecar file, if possible
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private static bool GetXgdAuxInfo(CICMMetadataType cicmSidecar, out string dmihash, out string pfihash, out string sshash, out string ss, out string ssver)
        {
            dmihash = null; pfihash = null; sshash = null; ss = null; ssver = null;

            // If the object is null, we can't get information from it
            if (cicmSidecar == null)
                return false;

            // Only care about OpticalDisc types
            if (cicmSidecar.OpticalDisc == null || cicmSidecar.OpticalDisc.Length == 0)
                return false;

            // Loop through each OpticalDisc in the metadata
            foreach (OpticalDiscType opticalDisc in cicmSidecar.OpticalDisc)
            {
                // If the Xbox type isn't set, we can't extract information
                if (opticalDisc.Xbox == null)
                    continue;

                // Get the Xbox information
                XboxType xbox = opticalDisc.Xbox;

                // DMI
                if (xbox.DMI != null)
                {
                    DumpType dmi = xbox.DMI;
                    if (dmi.Checksums != null && dmi.Checksums.Length != 0)
                    {
                        foreach (ChecksumType checksum in dmi.Checksums)
                        {
                            // We only care about the CRC32
                            if (checksum.type == ChecksumTypeType.crc32)
                            {
                                dmihash = checksum.Value;
                                break;
                            }
                        }
                    }
                }

                // PFI
                if (xbox.PFI != null)
                {
                    DumpType pfi = xbox.PFI;
                    if (pfi.Checksums != null && pfi.Checksums.Length != 0)
                    {
                        foreach (ChecksumType checksum in pfi.Checksums)
                        {
                            // We only care about the CRC32
                            if (checksum.type == ChecksumTypeType.crc32)
                            {
                                pfihash = checksum.Value;
                                break;
                            }
                        }
                    }
                }

                // SS
                if (xbox.SecuritySectors != null && xbox.SecuritySectors.Length > 0)
                {
                    foreach (XboxSecuritySectorsType securitySector in xbox.SecuritySectors)
                    {
                        DumpType security = securitySector.SecuritySectors;
                        if (security.Checksums != null && security.Checksums.Length != 0)
                        {
                            foreach (ChecksumType checksum in security.Checksums)
                            {
                                // We only care about the CRC32
                                if (checksum.type == ChecksumTypeType.crc32)
                                {
                                    // TODO: Validate correctness for all 3 fields
                                    ss = security.Image;
                                    ssver = securitySector.RequestVersion.ToString();
                                    sshash = checksum.Value;
                                    break;
                                }
                            }
                        }

                        // If we got a hash, we can break
                        if (sshash != null)
                            break;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Get the Xbox serial info from the CICM Sidecar file, if possible
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private static bool GetXboxDMIInfo(CICMMetadataType cicmSidecar, out string serial, out string version, out Region? region)
        {
            serial = null; version = null; region = Region.World;

            // If the object is null, we can't get information from it
            if (cicmSidecar == null)
                return false;

            // Only care about OpticalDisc types
            if (cicmSidecar.OpticalDisc == null || cicmSidecar.OpticalDisc.Length == 0)
                return false;

            // Loop through each OpticalDisc in the metadata
            foreach (OpticalDiscType opticalDisc in cicmSidecar.OpticalDisc)
            {
                // If the Xbox type isn't set, we can't extract information
                if (opticalDisc.Xbox == null)
                    continue;

                // Get the Xbox information
                XboxType xbox = opticalDisc.Xbox;

                // DMI
                if (xbox.DMI != null)
                {
                    DumpType dmi = xbox.DMI;
                    string image = dmi.Image;

                    // TODO: Figure out if `image` is the right thing here
                    // TODO: Figure out how to extract info from `image`
                    //br.BaseStream.Seek(8, SeekOrigin.Begin);
                    //char[] str = br.ReadChars(8);

                    //serial = $"{str[0]}{str[1]}-{str[2]}{str[3]}{str[4]}";
                    //version = $"1.{str[5]}{str[6]}";
                    //region = GetXgdRegion(str[7]);
                    //return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the Xbox 360 serial info from the CICM Sidecar file, if possible
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private static bool GetXbox360DMIInfo(CICMMetadataType cicmSidecar, out string serial, out string version, out Region? region)
        {
            serial = null; version = null; region = Region.World;

            // If the object is null, we can't get information from it
            if (cicmSidecar == null)
                return false;

            // Only care about OpticalDisc types
            if (cicmSidecar.OpticalDisc == null || cicmSidecar.OpticalDisc.Length == 0)
                return false;

            // Loop through each OpticalDisc in the metadata
            foreach (OpticalDiscType opticalDisc in cicmSidecar.OpticalDisc)
            {
                // If the Xbox type isn't set, we can't extract information
                if (opticalDisc.Xbox == null)
                    continue;

                // Get the Xbox information
                XboxType xbox = opticalDisc.Xbox;

                // DMI
                if (xbox.DMI != null)
                {
                    DumpType dmi = xbox.DMI;
                    string image = dmi.Image;

                    // TODO: Figure out if `image` is the right thing here
                    // TODO: Figure out how to extract info from `image`
                    //br.BaseStream.Seek(64, SeekOrigin.Begin);
                    //char[] str = br.ReadChars(14);

                    //serial = $"{str[0]}{str[1]}-{str[2]}{str[3]}{str[4]}{str[5]}";
                    //version = $"1.{str[6]}{str[7]}";
                    //region = GetXgdRegion(str[8]);
                    // str[9], str[10], str[11] - unknown purpose
                    // str[12], str[13] - disc <12> of <13>
                    //return true;
                }
            }

            return false;
        }

        #endregion
    }
}
