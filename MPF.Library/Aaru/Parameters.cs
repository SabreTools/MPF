using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MPF.CueSheets;
using MPF.Data;
using MPF.Utilities;
using Schemas;

namespace MPF.Aaru
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

        /// <summary>
        /// Base command to run
        /// </summary>
        public Command BaseCommand { get; set; }

        /// <inheritdoc/>
        public override InternalProgram InternalProgram => InternalProgram.Aaru;

        #endregion

        /// <summary>
        /// Set of flags to pass to the executable
        /// </summary>
        protected Dictionary<Flag, bool?> _flags = new Dictionary<Flag, bool?>();
        public bool? this[Flag key]
        {
            get
            {
                if (_flags.ContainsKey(key))
                    return _flags[key];

                return null;
            }
            set
            {
                _flags[key] = value;
            }
        }
        protected internal IEnumerable<Flag> Keys => _flags.Keys;

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

        public string ImgBurnLogValue { get; set; }

        public string InputValue { get; set; }

        public string Input1Value { get; set; }

        public string Input2Value { get; set; }

        public long? LengthValue { get; set; }

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
        public Parameters(KnownSystem? system, MediaType? type, char driveLetter, string filename, int? driveSpeed, Options options)
            : base(system, type, driveLetter, filename, driveSpeed, options)
        {
        }

        #region BaseParameters Implementations

        /// <inheritdoc/>
        public override (bool, List<string>) CheckAllOutputFilesExist(string basePath)
        {
            List<string> missingFiles = new List<string>();
            switch (this.Type)
            {
                case MediaType.CDROM:
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

                    break;

                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
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

                    break;

                default:
                    return (false, missingFiles); // TODO: Figure out more formats
            }

            return (!missingFiles.Any(), missingFiles);
        }

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, string basePath, Drive drive)
        {
            // TODO: Fill in submission info specifics for Aaru
            string outputDirectory = Path.GetDirectoryName(basePath);

            // Deserialize the sidecar, if possible
            var sidecar = GenerateSidecar(basePath + ".cicm.xml");

            // Fill in the hash data
            info.TracksAndWriteOffsets.ClrMameProData = GenerateDatfile(sidecar, basePath);

            switch (this.Type)
            {
                case MediaType.CDROM:
                    // TODO: Can this do GD-ROM?
                    info.Extras.PVD = GeneratePVD(sidecar) ?? "Disc has no PVD";

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

                    // Deal with the layerbreak
                    string layerbreak = null;
                    if (this.Type == MediaType.DVD)
                        layerbreak = GetLayerbreak(sidecar) ?? "";
                    else if (this.Type == MediaType.BluRay)
                        layerbreak = info.SizeAndChecksums.Size > 25_025_314_816 ? "25025314816" : null;

                    // If we have a single-layer disc
                    if (string.IsNullOrWhiteSpace(layerbreak))
                    {
                        info.Extras.PVD = GeneratePVD(sidecar) ?? "Disc has no PVD";
                    }
                    // If we have a dual-layer disc
                    else
                    {
                        info.Extras.PVD = GeneratePVD(sidecar) ?? "Disc has no PVD";
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

                case KnownSystem.DVDAudio:
                case KnownSystem.DVDVideo:
                    info.CopyProtection.Protection = GetDVDProtection(sidecar) ?? "";
                    break;

                case KnownSystem.KonamiPython2:
                    if (GetPlayStationExecutableInfo(drive?.Letter, out string pythonTwoSerial, out RedumpRegion? pythonTwoRegion, out string pythonTwoDate))
                    {
                        info.CommonDiscInfo.Comments += $"Internal Disc Serial: {pythonTwoSerial}\n";
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? pythonTwoRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = pythonTwoDate;
                    }

                    info.VersionAndEditions.Version = GetPlayStation2Version(drive?.Letter) ?? "";
                    break;

                case KnownSystem.MicrosoftXBOX:
                    if (GetXgdAuxInfo(sidecar, out string dmihash, out string pfihash, out string sshash, out string ss, out string ssver))
                    {
                        info.CommonDiscInfo.Comments += $"{Template.XBOXDMIHash}: {dmihash ?? ""}\n" +
                            $"{Template.XBOXPFIHash}: {pfihash ?? ""}\n" +
                            $"{Template.XBOXSSHash}: {sshash ?? ""}\n" +
                            $"{Template.XBOXSSVersion}: {ssver ?? ""}\n";
                        info.Extras.SecuritySectorRanges = ss ?? "";
                    }

                    if (GetXboxDMIInfo(sidecar, out string serial, out string version, out RedumpRegion? region))
                    {
                        info.CommonDiscInfo.Serial = serial ?? "";
                        info.VersionAndEditions.Version = version ?? "";
                        info.CommonDiscInfo.Region = region;
                    }

                    break;

                case KnownSystem.MicrosoftXBOX360:
                    if (GetXgdAuxInfo(sidecar, out string dmi360hash, out string pfi360hash, out string ss360hash, out string ss360, out string ssver360))
                    {
                        info.CommonDiscInfo.Comments += $"{Template.XBOXDMIHash}: {dmi360hash ?? ""}\n" +
                            $"{Template.XBOXPFIHash}: {pfi360hash ?? ""}\n" +
                            $"{Template.XBOXSSHash}: {ss360hash ?? ""}\n" +
                            $"{Template.XBOXSSVersion}: {ssver360 ?? ""}\n";
                        info.Extras.SecuritySectorRanges = ss360 ?? "";
                    }

                    if (GetXbox360DMIInfo(sidecar, out string serial360, out string version360, out RedumpRegion? region360))
                    {
                        info.CommonDiscInfo.Serial = serial360 ?? "";
                        info.VersionAndEditions.Version = version360 ?? "";
                        info.CommonDiscInfo.Region = region360;
                    }
                    break;

                case KnownSystem.SonyPlayStation:
                    if (GetPlayStationExecutableInfo(drive?.Letter, out string playstationSerial, out RedumpRegion? playstationRegion, out string playstationDate))
                    {
                        info.CommonDiscInfo.Comments += $"Internal Disc Serial: {playstationSerial}\n";
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? playstationRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = playstationDate;
                    }

                    info.CopyProtection.AntiModchip = GetPlayStationAntiModchipDetected(drive?.Letter) ? YesNo.Yes : YesNo.No;
                    break;

                case KnownSystem.SonyPlayStation2:
                    if (GetPlayStationExecutableInfo(drive?.Letter, out string playstationTwoSerial, out RedumpRegion? playstationTwoRegion, out string playstationTwoDate))
                    {
                        info.CommonDiscInfo.Comments += $"Internal Disc Serial: {playstationTwoSerial}\n";
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? playstationTwoRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = playstationTwoDate;
                    }

                    info.VersionAndEditions.Version = GetPlayStation2Version(drive?.Letter) ?? "";
                    break;

                case KnownSystem.SonyPlayStation4:
                    info.VersionAndEditions.Version = GetPlayStation4Version(drive?.Letter) ?? "";
                    break;

                case KnownSystem.SonyPlayStation5:
                    info.VersionAndEditions.Version = GetPlayStation5Version(drive?.Letter) ?? "";
                    break;
            }

            // Fill in any artifacts that exist, Base64-encoded
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

        /// <inheritdoc/>
        public override string GenerateParameters()
        {
            List<string> parameters = new List<string>();

            #region Pre-command flags

            // Debug
            if (this[Flag.Debug] == true)
                parameters.Add(Converters.LongName(Flag.Debug));

            // Verbose
            if (this[Flag.Verbose] == true)
                parameters.Add(Converters.LongName(Flag.Verbose));

            // Version
            if (this[Flag.Version] == true)
                parameters.Add(Converters.LongName(Flag.Version));

            // Help
            if (this[Flag.Help] == true)
                parameters.Add(Converters.LongName(Flag.Version));

            #endregion

            if (BaseCommand != Command.NONE)
                parameters.Add(Converters.LongName(BaseCommand));
            else
                return null;

            #region Boolean flags

            // Adler-32
            if (GetSupportedCommands(Flag.Adler32).Contains(BaseCommand))
            {
                if (this[Flag.Adler32] == true)
                    parameters.Add(Converters.LongName(Flag.Adler32));
            }

            // Clear
            if (GetSupportedCommands(Flag.Clear).Contains(BaseCommand))
            {
                if (this[Flag.Clear] == true)
                    parameters.Add(Converters.LongName(Flag.Clear));
            }

            // Clear All
            if (GetSupportedCommands(Flag.ClearAll).Contains(BaseCommand))
            {
                if (this[Flag.ClearAll] == true)
                    parameters.Add(Converters.LongName(Flag.ClearAll));
            }

            // CRC16
            if (GetSupportedCommands(Flag.CRC16).Contains(BaseCommand))
            {
                if (this[Flag.CRC16] == true)
                    parameters.Add(Converters.LongName(Flag.CRC16));
            }

            // CRC32
            if (GetSupportedCommands(Flag.CRC32).Contains(BaseCommand))
            {
                if (this[Flag.CRC32] == true)
                    parameters.Add(Converters.LongName(Flag.CRC32));
            }

            // CRC64
            if (GetSupportedCommands(Flag.CRC64).Contains(BaseCommand))
            {
                if (this[Flag.CRC64] == true)
                    parameters.Add(Converters.LongName(Flag.CRC64));
            }

            // Disk Tags
            if (GetSupportedCommands(Flag.DiskTags).Contains(BaseCommand))
            {
                if (this[Flag.DiskTags] == true)
                    parameters.Add(Converters.LongName(Flag.DiskTags));
            }

            // Duplicated Sectors
            if (GetSupportedCommands(Flag.DuplicatedSectors).Contains(BaseCommand))
            {
                if (this[Flag.DuplicatedSectors] == true)
                    parameters.Add(Converters.LongName(Flag.DuplicatedSectors));
            }

            // Eject
            if (GetSupportedCommands(Flag.Eject).Contains(BaseCommand))
            {
                if (this[Flag.Eject] == true)
                    parameters.Add(Converters.LongName(Flag.Eject));
            }

            // Extended Attributes
            if (GetSupportedCommands(Flag.ExtendedAttributes).Contains(BaseCommand))
            {
                if (this[Flag.ExtendedAttributes] == true)
                    parameters.Add(Converters.LongName(Flag.ExtendedAttributes));
            }

            // Filesystems
            if (GetSupportedCommands(Flag.Filesystems).Contains(BaseCommand))
            {
                if (this[Flag.Filesystems] == true)
                    parameters.Add(Converters.LongName(Flag.Filesystems));
            }

            // First Pregap
            if (GetSupportedCommands(Flag.FirstPregap).Contains(BaseCommand))
            {
                if (this[Flag.FirstPregap] == true)
                    parameters.Add(Converters.LongName(Flag.FirstPregap));
            }

            // Fix Offset
            if (GetSupportedCommands(Flag.FixOffset).Contains(BaseCommand))
            {
                if (this[Flag.FixOffset] == true)
                    parameters.Add(Converters.LongName(Flag.FixOffset));
            }

            // Fix Subchannel
            if (GetSupportedCommands(Flag.FixSubchannel).Contains(BaseCommand))
            {
                if (this[Flag.FixSubchannel] == true)
                    parameters.Add(Converters.LongName(Flag.FixSubchannel));
            }

            // Fix Subchannel CRC
            if (GetSupportedCommands(Flag.FixSubchannelCrc).Contains(BaseCommand))
            {
                if (this[Flag.FixSubchannelCrc] == true)
                    parameters.Add(Converters.LongName(Flag.FixSubchannelCrc));
            }

            // Fix Subchannel Position
            if (GetSupportedCommands(Flag.FixSubchannelPosition).Contains(BaseCommand))
            {
                if (this[Flag.FixSubchannelPosition] == true)
                    parameters.Add(Converters.LongName(Flag.FixSubchannelPosition));
            }

            // Fletcher-16
            if (GetSupportedCommands(Flag.Fletcher16).Contains(BaseCommand))
            {
                if (this[Flag.Fletcher16] == true)
                    parameters.Add(Converters.LongName(Flag.Fletcher16));
            }

            // Fletcher-32
            if (GetSupportedCommands(Flag.Fletcher32).Contains(BaseCommand))
            {
                if (this[Flag.Fletcher32] == true)
                    parameters.Add(Converters.LongName(Flag.Fletcher32));
            }

            // Force
            if (GetSupportedCommands(Flag.Force).Contains(BaseCommand))
            {
                if (this[Flag.Force] == true)
                    parameters.Add(Converters.LongName(Flag.Force));
            }

            // Generate Subchannels
            if (GetSupportedCommands(Flag.GenerateSubchannels).Contains(BaseCommand))
            {
                if (this[Flag.GenerateSubchannels] == true)
                    parameters.Add(Converters.LongName(Flag.GenerateSubchannels));
            }

            // Long Format
            if (GetSupportedCommands(Flag.LongFormat).Contains(BaseCommand))
            {
                if (this[Flag.LongFormat] == true)
                    parameters.Add(Converters.LongName(Flag.LongFormat));
            }

            // Long Sectors
            if (GetSupportedCommands(Flag.LongSectors).Contains(BaseCommand))
            {
                if (this[Flag.LongSectors] == true)
                    parameters.Add(Converters.LongName(Flag.LongSectors));
            }

            // MD5
            if (GetSupportedCommands(Flag.MD5).Contains(BaseCommand))
            {
                if (this[Flag.MD5] == true)
                    parameters.Add(Converters.LongName(Flag.MD5));
            }

            // Metadata
            if (GetSupportedCommands(Flag.Metadata).Contains(BaseCommand))
            {
                if (this[Flag.Metadata] == true)
                    parameters.Add(Converters.LongName(Flag.Metadata));
            }

            // Partitions
            if (GetSupportedCommands(Flag.Partitions).Contains(BaseCommand))
            {
                if (this[Flag.Partitions] == true)
                    parameters.Add(Converters.LongName(Flag.Partitions));
            }

            // Persistent
            if (GetSupportedCommands(Flag.Persistent).Contains(BaseCommand))
            {
                if (this[Flag.Persistent] == true)
                    parameters.Add(Converters.LongName(Flag.Persistent));
            }

            // Private
            if (GetSupportedCommands(Flag.Private).Contains(BaseCommand))
            {
                if (this[Flag.Private] == true)
                    parameters.Add(Converters.LongName(Flag.Private));
            }

            // Resume
            if (GetSupportedCommands(Flag.Resume).Contains(BaseCommand))
            {
                if (this[Flag.Resume] == true)
                    parameters.Add(Converters.LongName(Flag.Resume));
            }

            // Retry Subchannel
            if (GetSupportedCommands(Flag.RetrySubchannel).Contains(BaseCommand))
            {
                if (this[Flag.RetrySubchannel] == true)
                    parameters.Add(Converters.LongName(Flag.RetrySubchannel));
            }

            // Sector Tags
            if (GetSupportedCommands(Flag.SectorTags).Contains(BaseCommand))
            {
                if (this[Flag.SectorTags] == true)
                    parameters.Add(Converters.LongName(Flag.SectorTags));
            }

            // Separated Tracks
            if (GetSupportedCommands(Flag.SeparatedTracks).Contains(BaseCommand))
            {
                if (this[Flag.SeparatedTracks] == true)
                    parameters.Add(Converters.LongName(Flag.SeparatedTracks));
            }

            // SHA-1
            if (GetSupportedCommands(Flag.SHA1).Contains(BaseCommand))
            {
                if (this[Flag.SHA1] == true)
                    parameters.Add(Converters.LongName(Flag.SHA1));
            }

            // SHA-256
            if (GetSupportedCommands(Flag.SHA256).Contains(BaseCommand))
            {
                if (this[Flag.SHA256] == true)
                    parameters.Add(Converters.LongName(Flag.SHA256));
            }

            // SHA-384
            if (GetSupportedCommands(Flag.SHA384).Contains(BaseCommand))
            {
                if (this[Flag.SHA384] == true)
                    parameters.Add(Converters.LongName(Flag.SHA384));
            }

            // SHA-512
            if (GetSupportedCommands(Flag.SHA512).Contains(BaseCommand))
            {
                if (this[Flag.SHA512] == true)
                    parameters.Add(Converters.LongName(Flag.SHA512));
            }

            // Skip CD-i Ready Hole
            if (GetSupportedCommands(Flag.SkipCdiReadyHole).Contains(BaseCommand))
            {
                if (this[Flag.SkipCdiReadyHole] == true)
                    parameters.Add(Converters.LongName(Flag.SkipCdiReadyHole));
            }

            // SpamSum
            if (GetSupportedCommands(Flag.SpamSum).Contains(BaseCommand))
            {
                if (this[Flag.SpamSum] == true)
                    parameters.Add(Converters.LongName(Flag.SpamSum));
            }

            // Stop on Error
            if (GetSupportedCommands(Flag.StopOnError).Contains(BaseCommand))
            {
                if (this[Flag.StopOnError] == true)
                    parameters.Add(Converters.LongName(Flag.StopOnError));
            }

            // Tape
            if (GetSupportedCommands(Flag.Tape).Contains(BaseCommand))
            {
                if (this[Flag.Tape] == true)
                    parameters.Add(Converters.LongName(Flag.Tape));
            }

            // Trim
            if (GetSupportedCommands(Flag.Trim).Contains(BaseCommand))
            {
                if (this[Flag.Trim] == true)
                    parameters.Add(Converters.LongName(Flag.Trim));
            }

            // Verify Disc
            if (GetSupportedCommands(Flag.VerifyDisc).Contains(BaseCommand))
            {
                if (this[Flag.VerifyDisc] == true)
                    parameters.Add(Converters.LongName(Flag.VerifyDisc));
            }

            // Verify Sectors
            if (GetSupportedCommands(Flag.VerifySectors).Contains(BaseCommand))
            {
                if (this[Flag.VerifySectors] == true)
                    parameters.Add(Converters.LongName(Flag.VerifySectors));
            }

            // Whole Disc
            if (GetSupportedCommands(Flag.WholeDisc).Contains(BaseCommand))
            {
                if (this[Flag.WholeDisc] == true)
                    parameters.Add(Converters.LongName(Flag.WholeDisc));
            }

            #endregion

            #region Int8 flags

            // Speed
            if (GetSupportedCommands(Flag.Speed).Contains(BaseCommand))
            {
                if (this[Flag.Speed] == true && SpeedValue != null)
                    parameters.Add($"{Converters.LongName(Flag.Speed)} {SpeedValue}");
            }

            #endregion

            #region Int16 flags

            // Retry Passes
            if (GetSupportedCommands(Flag.RetryPasses).Contains(BaseCommand))
            {
                if (this[Flag.RetryPasses] == true && RetryPassesValue != null)
                    parameters.Add($"{Converters.LongName(Flag.RetryPasses)} {RetryPassesValue}");
            }

            // Width
            if (GetSupportedCommands(Flag.Width).Contains(BaseCommand))
            {
                if (this[Flag.Width] == true && WidthValue != null)
                    parameters.Add($"{Converters.LongName(Flag.Width)} {WidthValue}");
            }

            #endregion

            #region Int32 flags

            // Block Size
            if (GetSupportedCommands(Flag.BlockSize).Contains(BaseCommand))
            {
                if (this[Flag.BlockSize] == true && BlockSizeValue != null)
                    parameters.Add($"{Converters.LongName(Flag.BlockSize)} {BlockSizeValue}");
            }

            // Count
            if (GetSupportedCommands(Flag.Count).Contains(BaseCommand))
            {
                if (this[Flag.Count] == true && CountValue != null)
                    parameters.Add($"{Converters.LongName(Flag.Count)} {CountValue}");
            }

            // Media Last Sequence
            if (GetSupportedCommands(Flag.MediaLastSequence).Contains(BaseCommand))
            {
                if (this[Flag.MediaLastSequence] == true && MediaLastSequenceValue != null)
                    parameters.Add($"{Converters.LongName(Flag.MediaLastSequence)} {MediaLastSequenceValue}");
            }

            // Media Sequence
            if (GetSupportedCommands(Flag.MediaSequence).Contains(BaseCommand))
            {
                if (this[Flag.MediaSequence] == true && MediaSequenceValue != null)
                    parameters.Add($"{Converters.LongName(Flag.MediaSequence)} {MediaSequenceValue}");
            }

            // Skip
            if (GetSupportedCommands(Flag.Skip).Contains(BaseCommand))
            {
                if (this[Flag.Skip] == true && SkipValue != null)
                    parameters.Add($"{Converters.LongName(Flag.Skip)} {SkipValue}");
            }

            #endregion

            #region Int64 flags

            // Length
            if (GetSupportedCommands(Flag.Length).Contains(BaseCommand))
            {
                if (this[Flag.Length] == true && LengthValue != null)
                {
                    if (LengthValue >= 0)
                        parameters.Add($"{Converters.LongName(Flag.Length)} {LengthValue}");
                    else if (LengthValue == -1 && BaseCommand == Command.ImageDecode)
                        parameters.Add($"{Converters.LongName(Flag.Length)} all");
                }
            }

            // Start
            if (GetSupportedCommands(Flag.Start).Contains(BaseCommand))
            {
                if (this[Flag.Start] == true && StartValue != null)
                    parameters.Add($"{Converters.LongName(Flag.Start)} {StartValue}");
            }

            #endregion

            #region String flags

            // Comments
            if (GetSupportedCommands(Flag.Comments).Contains(BaseCommand))
            {
                if (this[Flag.Comments] == true && CommentsValue != null)
                    parameters.Add($"{Converters.LongName(Flag.Comments)} \"{CommentsValue}\"");
            }

            // Creator
            if (GetSupportedCommands(Flag.Creator).Contains(BaseCommand))
            {
                if (this[Flag.Creator] == true && CreatorValue != null)
                    parameters.Add($"{Converters.LongName(Flag.Creator)} \"{CreatorValue}\"");
            }

            // Drive Manufacturer
            if (GetSupportedCommands(Flag.DriveManufacturer).Contains(BaseCommand))
            {
                if (this[Flag.DriveManufacturer] == true && DriveManufacturerValue != null)
                    parameters.Add($"{Converters.LongName(Flag.DriveManufacturer)} \"{DriveManufacturerValue}\"");
            }

            // Drive Model
            if (GetSupportedCommands(Flag.DriveModel).Contains(BaseCommand))
            {
                if (this[Flag.DriveModel] == true && DriveModelValue != null)
                    parameters.Add($"{Converters.LongName(Flag.DriveModel)} \"{DriveModelValue}\"");
            }

            // Drive Revision
            if (GetSupportedCommands(Flag.DriveRevision).Contains(BaseCommand))
            {
                if (this[Flag.DriveRevision] == true && DriveRevisionValue != null)
                    parameters.Add($"{Converters.LongName(Flag.DriveRevision)} \"{DriveRevisionValue}\"");
            }

            // Drive Serial
            if (GetSupportedCommands(Flag.DriveSerial).Contains(BaseCommand))
            {
                if (this[Flag.DriveSerial] == true && DriveSerialValue != null)
                    parameters.Add($"{Converters.LongName(Flag.DriveSerial)} \"{DriveSerialValue}\"");
            }

            // Encoding
            if (GetSupportedCommands(Flag.Encoding).Contains(BaseCommand))
            {
                if (this[Flag.Encoding] == true && EncodingValue != null)
                    parameters.Add($"{Converters.LongName(Flag.Encoding)} \"{EncodingValue}\"");
            }

            // Format (Convert)
            if (GetSupportedCommands(Flag.FormatConvert).Contains(BaseCommand))
            {
                if (this[Flag.FormatConvert] == true && FormatConvertValue != null)
                    parameters.Add($"{Converters.LongName(Flag.FormatConvert)} \"{FormatConvertValue}\"");
            }

            // Format (Dump)
            if (GetSupportedCommands(Flag.FormatDump).Contains(BaseCommand))
            {
                if (this[Flag.FormatDump] == true && FormatDumpValue != null)
                    parameters.Add($"{Converters.LongName(Flag.FormatDump)} \"{FormatDumpValue}\"");
            }

            // ImgBurn Log
            if (GetSupportedCommands(Flag.ImgBurnLog).Contains(BaseCommand))
            {
                if (this[Flag.ImgBurnLog] == true && ImgBurnLogValue != null)
                    parameters.Add($"{Converters.LongName(Flag.ImgBurnLog)} \"{ImgBurnLogValue}\"");
            }

            // Media Barcode
            if (GetSupportedCommands(Flag.MediaBarcode).Contains(BaseCommand))
            {
                if (this[Flag.MediaBarcode] == true && MediaBarcodeValue != null)
                    parameters.Add($"{Converters.LongName(Flag.MediaBarcode)} \"{MediaBarcodeValue}\"");
            }

            // Media Manufacturer
            if (GetSupportedCommands(Flag.MediaManufacturer).Contains(BaseCommand))
            {
                if (this[Flag.MediaManufacturer] == true && MediaManufacturerValue != null)
                    parameters.Add($"{Converters.LongName(Flag.MediaManufacturer)} \"{MediaManufacturerValue}\"");
            }

            // Media Model
            if (GetSupportedCommands(Flag.MediaModel).Contains(BaseCommand))
            {
                if (this[Flag.MediaModel] == true && MediaModelValue != null)
                    parameters.Add($"{Converters.LongName(Flag.MediaModel)} \"{MediaModelValue}\"");
            }

            // Media Part Number
            if (GetSupportedCommands(Flag.MediaPartNumber).Contains(BaseCommand))
            {
                if (this[Flag.MediaPartNumber] == true && MediaPartNumberValue != null)
                    parameters.Add($"{Converters.LongName(Flag.MediaPartNumber)} \"{MediaPartNumberValue}\"");
            }

            // Media Serial
            if (GetSupportedCommands(Flag.MediaSerial).Contains(BaseCommand))
            {
                if (this[Flag.MediaSerial] == true && MediaSerialValue != null)
                    parameters.Add($"{Converters.LongName(Flag.MediaSerial)} \"{MediaSerialValue}\"");
            }

            // Media Title
            if (GetSupportedCommands(Flag.MediaTitle).Contains(BaseCommand))
            {
                if (this[Flag.MediaTitle] == true && MediaTitleValue != null)
                    parameters.Add($"{Converters.LongName(Flag.MediaTitle)} \"{MediaTitleValue}\"");
            }

            // MHDD Log
            if (GetSupportedCommands(Flag.MHDDLog).Contains(BaseCommand))
            {
                if (this[Flag.MHDDLog] == true && MHDDLogValue != null)
                    parameters.Add($"{Converters.LongName(Flag.MHDDLog)} \"{MHDDLogValue}\"");
            }

            // Namespace
            if (GetSupportedCommands(Flag.Namespace).Contains(BaseCommand))
            {
                if (this[Flag.Namespace] == true && NamespaceValue != null)
                    parameters.Add($"{Converters.LongName(Flag.Namespace)} \"{NamespaceValue}\"");
            }

            // Options
            if (GetSupportedCommands(Flag.Options).Contains(BaseCommand))
            {
                if (this[Flag.Options] == true && OptionsValue != null)
                    parameters.Add($"{Converters.LongName(Flag.Options)} \"{OptionsValue}\"");
            }

            // Output Prefix
            if (GetSupportedCommands(Flag.OutputPrefix).Contains(BaseCommand))
            {
                if (this[Flag.OutputPrefix] == true && OutputPrefixValue != null)
                    parameters.Add($"{Converters.LongName(Flag.OutputPrefix)} \"{OutputPrefixValue}\"");
            }

            // Resume File
            if (GetSupportedCommands(Flag.ResumeFile).Contains(BaseCommand))
            {
                if (this[Flag.ResumeFile] == true && ResumeFileValue != null)
                    parameters.Add($"{Converters.LongName(Flag.ResumeFile)} \"{ResumeFileValue}\"");
            }

            // Subchannel
            if (GetSupportedCommands(Flag.Subchannel).Contains(BaseCommand))
            {
                if (this[Flag.Subchannel] == true && SubchannelValue != null)
                    parameters.Add($"{Converters.LongName(Flag.Subchannel)} \"{SubchannelValue}\"");
            }

            // XML Sidecar
            if (GetSupportedCommands(Flag.XMLSidecar).Contains(BaseCommand))
            {
                if (this[Flag.XMLSidecar] == true && XMLSidecarValue != null)
                    parameters.Add($"{Converters.LongName(Flag.XMLSidecar)} \"{XMLSidecarValue}\"");
            }

            #endregion

            // Handle filenames based on command, if necessary
            switch (BaseCommand)
            {
                // Input value only
                case Command.DeviceInfo:
                case Command.DeviceReport:
                case Command.FilesystemList:
                case Command.ImageAnalyze:
                case Command.ImageChecksum:
                case Command.ImageCreateSidecar:
                case Command.ImageDecode:
                case Command.ImageEntropy:
                case Command.ImageInfo:
                case Command.ImagePrint:
                case Command.ImageVerify:
                case Command.MediaInfo:
                case Command.MediaScan:
                    if (string.IsNullOrWhiteSpace(InputValue))
                        return null;

                    parameters.Add($"\"{InputValue}\"");
                    break;

                // Two input values
                case Command.ImageCompare:
                    if (string.IsNullOrWhiteSpace(Input1Value) || string.IsNullOrWhiteSpace(Input2Value))
                        return null;

                    parameters.Add($"\"{Input1Value}\"");
                    parameters.Add($"\"{Input2Value}\"");
                    break;

                // Input and Output value
                case Command.FilesystemExtract:
                case Command.ImageConvert:
                case Command.MediaDump:
                    if (string.IsNullOrWhiteSpace(InputValue) || string.IsNullOrWhiteSpace(OutputValue))
                        return null;

                    parameters.Add($"\"{InputValue}\"");
                    parameters.Add($"\"{OutputValue}\"");
                    break;

                // Remote host value only
                case Command.DeviceList:
                case Command.Remote:
                    if (string.IsNullOrWhiteSpace(RemoteHostValue))
                        return null;

                    parameters.Add($"\"{RemoteHostValue}\"");
                    break;
            }

            return string.Join(" ", parameters);
        }

        /// <inheritdoc/>
        public override string GetDefaultExtension(MediaType? mediaType) => Converters.Extension(mediaType);

        /// <inheritdoc/>
        public override bool IsDumpingCommand()
        {
            switch (BaseCommand)
            {
                case Command.MediaDump:
                    return true;

                default:
                    return false;
            }
        }

        /// <inheritdoc/>
        protected override void ResetValues()
        {
            BaseCommand = Command.NONE;

            _flags = new Dictionary<Flag, bool?>();

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
            BaseCommand = Command.MediaDump;

            InputValue = $"\\\\?\\{driveLetter}:";
            OutputValue = filename;

            if (driveSpeed != null)
            {
                this[Flag.Speed] = true;
                SpeedValue = (sbyte?)driveSpeed;
            }

            // First check to see if the combination of system and MediaType is valid
            var validTypes = Validators.GetValidMediaTypes(this.System);
            if (!validTypes.Contains(this.Type))
                return;

            // Set retry count
            if (options.AaruRereadCount > 0)
            {
                this[Flag.RetryPasses] = true;
                RetryPassesValue = (short)options.AaruRereadCount;
            }

            // Set user-defined options
            this[Flag.Debug] = options.AaruEnableDebug;
            this[Flag.Verbose] = options.AaruEnableVerbose;
            this[Flag.Force] = options.AaruForceDumping;
            this[Flag.Private] = options.AaruStripPersonalData;

            // TODO: Look at dump-media formats and the like and see what options there are there to fill in defaults
            // Now sort based on disc type
            switch (this.Type)
            {
                case MediaType.CDROM:
                    this[Flag.FirstPregap] = true;
                    this[Flag.FixOffset] = true;
                    this[Flag.Subchannel] = true;
                    SubchannelValue = "any";
                    break;
                case MediaType.DVD:
                    // Currently no defaults set
                    break;
                case MediaType.GDROM:
                    // Currently no defaults set
                    break;
                case MediaType.HDDVD:
                    // Currently no defaults set
                    break;
                case MediaType.BluRay:
                    // Currently no defaults set
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
                ProcessFlagParameter(parts, FlagStrings.DebugShort, FlagStrings.DebugLong, Flag.Debug, ref start);

                // Verbose
                ProcessFlagParameter(parts, FlagStrings.VerboseShort, FlagStrings.VerboseLong, Flag.Verbose, ref start);

                // Version
                ProcessFlagParameter(parts, null, FlagStrings.VersionLong, Flag.Version, ref start);

                // Help
                ProcessFlagParameter(parts, FlagStrings.HelpShort, FlagStrings.HelpLong, Flag.Help, ref start);
                ProcessFlagParameter(parts, FlagStrings.HelpShortAlt, null, Flag.Help, ref start);

                // If we didn't add any new flags, break out since we might be at command handling
                if (keyCount == Keys.Count())
                    break;
            }

            // Determine what the commandline should look like given the first item
            BaseCommand = Converters.StringToCommand(parts[start], parts.Count > start ? parts[start + 1] : null, out bool useSecond);
            if (BaseCommand == Command.NONE)
                return false;

            // Set the start according to what the full command was
            start += useSecond ? 2 : 1;

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
                ProcessFlagParameter(parts, FlagStrings.Adler32Short, FlagStrings.Adler32Long, Flag.Adler32, ref i);

                // Clear
                ProcessFlagParameter(parts, null, FlagStrings.ClearLong, Flag.Clear, ref i);

                // Clear All
                ProcessFlagParameter(parts, null, FlagStrings.ClearAllLong, Flag.ClearAll, ref i);

                // CRC16
                ProcessFlagParameter(parts, null, FlagStrings.CRC16Long, Flag.CRC16, ref i);

                // CRC32
                ProcessFlagParameter(parts, FlagStrings.CRC32Short, FlagStrings.CRC32Long, Flag.CRC32, ref i);

                // CRC64
                ProcessFlagParameter(parts, null, FlagStrings.CRC64Long, Flag.CRC64, ref i);

                // Disk Tags
                ProcessFlagParameter(parts, FlagStrings.DiskTagsShort, FlagStrings.DiskTagsLong, Flag.DiskTags, ref i);

                // Deduplicated Sectors
                ProcessFlagParameter(parts, FlagStrings.DuplicatedSectorsShort, FlagStrings.DuplicatedSectorsLong, Flag.DuplicatedSectors, ref i);

                // Eject
                ProcessFlagParameter(parts, null, FlagStrings.EjectLong, Flag.Eject, ref i);

                // Extended Attributes
                ProcessFlagParameter(parts, FlagStrings.ExtendedAttributesShort, FlagStrings.ExtendedAttributesLong, Flag.ExtendedAttributes, ref i);

                // Filesystems
                ProcessFlagParameter(parts, FlagStrings.FilesystemsShort, FlagStrings.FilesystemsLong, Flag.Filesystems, ref i);

                // First Pregap
                ProcessFlagParameter(parts, null, FlagStrings.FirstPregapLong, Flag.FirstPregap, ref i);

                // Fix Offset
                ProcessFlagParameter(parts, null, FlagStrings.FixOffsetLong, Flag.FixOffset, ref i);

                // Fix Subchannel
                ProcessFlagParameter(parts, null, FlagStrings.FixSubchannelLong, Flag.FixSubchannel, ref i);

                // Fix Subchannel CRC
                ProcessFlagParameter(parts, null, FlagStrings.FixSubchannelCrcLong, Flag.FixSubchannelCrc, ref i);

                // Fix Subchannel Position
                ProcessFlagParameter(parts, null, FlagStrings.FixSubchannelPositionLong, Flag.FixSubchannelPosition, ref i);

                // Fletcher-16
                ProcessFlagParameter(parts, null, FlagStrings.Fletcher16Long, Flag.Fletcher16, ref i);

                // Fletcher-32
                ProcessFlagParameter(parts, null, FlagStrings.Fletcher32Long, Flag.Fletcher32, ref i);

                // Force
                ProcessFlagParameter(parts, FlagStrings.ForceShort, FlagStrings.ForceLong, Flag.Force, ref i);

                // Generate Subchannels
                ProcessFlagParameter(parts, null, FlagStrings.GenerateSubchannelsLong, Flag.GenerateSubchannels, ref i);

                // Long Format
                ProcessFlagParameter(parts, FlagStrings.LongFormatShort, FlagStrings.LongFormatLong, Flag.LongFormat, ref i);

                // Long Sectors
                ProcessFlagParameter(parts, FlagStrings.LongSectorsShort, FlagStrings.LongSectorsLong, Flag.LongSectors, ref i);

                // MD5
                ProcessFlagParameter(parts, FlagStrings.MD5Short, FlagStrings.MD5Long, Flag.MD5, ref i);

                // Metadata
                ProcessFlagParameter(parts, null, FlagStrings.MetadataLong, Flag.Metadata, ref i);

                // Partitions
                ProcessFlagParameter(parts, FlagStrings.PartitionsShort, FlagStrings.PartitionsLong, Flag.Partitions, ref i);

                // Persistent
                ProcessFlagParameter(parts, null, FlagStrings.PersistentLong, Flag.Persistent, ref i);

                // Private
                ProcessFlagParameter(parts, null, FlagStrings.PrivateLong, Flag.Private, ref i);

                // Resume
                ProcessFlagParameter(parts, FlagStrings.ResumeShort, FlagStrings.ResumeLong, Flag.Resume, ref i);

                // Retry Subchannel
                ProcessFlagParameter(parts, null, FlagStrings.RetrySubchannelLong, Flag.RetrySubchannel, ref i);

                // Sector Tags
                ProcessFlagParameter(parts, FlagStrings.SectorTagsShort, FlagStrings.SectorTagsLong, Flag.SectorTags, ref i);

                // Separated Tracks
                ProcessFlagParameter(parts, FlagStrings.SeparatedTracksShort, FlagStrings.SeparatedTracksLong, Flag.SeparatedTracks, ref i);

                // SHA-1
                ProcessFlagParameter(parts, FlagStrings.SHA1Short, FlagStrings.SHA1Long, Flag.SHA1, ref i);

                // SHA-256
                ProcessFlagParameter(parts, null, FlagStrings.SHA256Long, Flag.SHA256, ref i);

                // SHA-384
                ProcessFlagParameter(parts, null, FlagStrings.SHA384Long, Flag.SHA384, ref i);

                // SHA-512
                ProcessFlagParameter(parts, null, FlagStrings.SHA512Long, Flag.SHA512, ref i);

                // Skip CD-i Ready Hole
                ProcessFlagParameter(parts, null, FlagStrings.SkipCdiReadyHoleLong, Flag.SkipCdiReadyHole, ref i);

                // SpamSum
                ProcessFlagParameter(parts, FlagStrings.SpamSumShort, FlagStrings.SpamSumLong, Flag.SpamSum, ref i);

                // Stop on Error
                ProcessFlagParameter(parts, FlagStrings.StopOnErrorShort, FlagStrings.StopOnErrorLong, Flag.StopOnError, ref i);

                // Tape
                ProcessFlagParameter(parts, FlagStrings.TapeShort, FlagStrings.TapeLong, Flag.Tape, ref i);

                // Trim
                ProcessFlagParameter(parts, null, FlagStrings.TrimLong, Flag.Trim, ref i);

                // Verify Disc
                ProcessFlagParameter(parts, FlagStrings.VerifyDiscShort, FlagStrings.VerifyDiscLong, Flag.VerifyDisc, ref i);

                // Verify Sectors
                ProcessFlagParameter(parts, FlagStrings.VerifySectorsShort, FlagStrings.VerifySectorsLong, Flag.VerifySectors, ref i);

                // Whole Disc
                ProcessFlagParameter(parts, FlagStrings.WholeDiscShort, FlagStrings.WholeDiscLong, Flag.VerifySectors, ref i);

                #endregion

                #region Int8 flags

                // Speed
                byteValue = ProcessInt8Parameter(parts, null, FlagStrings.SpeedLong, Flag.Speed, ref i);
                if (byteValue == null && byteValue != SByte.MinValue)
                    SpeedValue = byteValue;

                #endregion

                #region Int16 flags

                // Retry Passes
                shortValue = ProcessInt16Parameter(parts, FlagStrings.RetryPassesShort, FlagStrings.RetryPassesLong, Flag.RetryPasses, ref i);
                if (shortValue != null && shortValue != Int16.MinValue)
                    RetryPassesValue = shortValue;

                // Width
                shortValue = ProcessInt16Parameter(parts, FlagStrings.WidthShort, FlagStrings.WidthLong, Flag.Width, ref i);
                if (shortValue != null && shortValue != Int16.MinValue)
                    WidthValue = shortValue;

                #endregion

                #region Int32 flags

                // Block Size
                intValue = ProcessInt32Parameter(parts, FlagStrings.BlockSizeShort, FlagStrings.BlockSizeLong, Flag.BlockSize, ref i);
                if (intValue != null && intValue != Int32.MinValue)
                    BlockSizeValue = intValue;

                // Count
                intValue = ProcessInt32Parameter(parts, FlagStrings.CountShort, FlagStrings.CountLong, Flag.Count, ref i);
                if (intValue != null && intValue != Int32.MinValue)
                    CountValue = intValue;

                // Media Last Sequence
                intValue = ProcessInt32Parameter(parts, null, FlagStrings.MediaLastSequenceLong, Flag.MediaLastSequence, ref i);
                if (intValue != null && intValue != Int32.MinValue)
                    MediaLastSequenceValue = intValue;

                // Media Sequence
                intValue = ProcessInt32Parameter(parts, null, FlagStrings.MediaSequenceLong, Flag.MediaSequence, ref i);
                if (intValue != null && intValue != Int32.MinValue)
                    MediaSequenceValue = intValue;

                // Skip
                intValue = ProcessInt32Parameter(parts, FlagStrings.SkipShort, FlagStrings.SkipLong, Flag.Skip, ref i);
                if (intValue != null && intValue != Int32.MinValue)
                    SkipValue = intValue;

                #endregion

                #region Int64 flags

                // Length
                longValue = ProcessInt64Parameter(parts, FlagStrings.LengthShort, FlagStrings.LengthLong, Flag.Length, ref i);
                if (longValue != null && longValue != Int64.MinValue)
                {
                    LengthValue = longValue;
                }
                else
                {
                    stringValue = ProcessStringParameter(parts, FlagStrings.LengthShort, FlagStrings.LengthLong, Flag.Length, ref i);
                    if (string.Equals(stringValue, "all"))
                        LengthValue = -1;
                }

                // Start -- Required value
                longValue = ProcessInt64Parameter(parts, FlagStrings.StartShort, FlagStrings.StartLong, Flag.Start, ref i);
                if (longValue == null)
                    return false;
                else if (longValue != Int64.MinValue)
                    StartValue = longValue;

                #endregion

                #region String flags

                // Comments
                stringValue = ProcessStringParameter(parts, null, FlagStrings.CommentsLong, Flag.Comments, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    CommentsValue = stringValue;

                // Creator
                stringValue = ProcessStringParameter(parts, null, FlagStrings.CreatorLong, Flag.Creator, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    CreatorValue = stringValue;

                // Drive Manufacturer
                stringValue = ProcessStringParameter(parts, null, FlagStrings.DriveManufacturerLong, Flag.DriveManufacturer, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    DriveManufacturerValue = stringValue;

                // Drive Model
                stringValue = ProcessStringParameter(parts, null, FlagStrings.DriveModelLong, Flag.DriveModel, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    DriveModelValue = stringValue;

                // Drive Revision
                stringValue = ProcessStringParameter(parts, null, FlagStrings.DriveRevisionLong, Flag.DriveRevision, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    DriveRevisionValue = stringValue;

                // Drive Serial
                stringValue = ProcessStringParameter(parts, null, FlagStrings.DriveSerialLong, Flag.DriveSerial, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    DriveSerialValue = stringValue;

                // Encoding -- TODO: List of encodings?
                stringValue = ProcessStringParameter(parts, FlagStrings.EncodingShort, FlagStrings.EncodingLong, Flag.Encoding, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    EncodingValue = stringValue;

                // Format (Convert) -- TODO: List of formats?
                stringValue = ProcessStringParameter(parts, FlagStrings.FormatConvertShort, FlagStrings.FormatConvertLong, Flag.FormatConvert, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    FormatConvertValue = stringValue;

                // Format (Dump) -- TODO: List of formats?
                stringValue = ProcessStringParameter(parts, FlagStrings.FormatDumpShort, FlagStrings.FormatDumpLong, Flag.FormatDump, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    FormatDumpValue = stringValue;

                // ImgBurn Log
                stringValue = ProcessStringParameter(parts, FlagStrings.ImgBurnLogShort, FlagStrings.ImgBurnLogLong, Flag.ImgBurnLog, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    ImgBurnLogValue = stringValue;

                // Media Barcode
                stringValue = ProcessStringParameter(parts, null, FlagStrings.MediaBarcodeLong, Flag.MediaBarcode, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    MediaBarcodeValue = stringValue;

                // Media Manufacturer
                stringValue = ProcessStringParameter(parts, null, FlagStrings.MediaManufacturerLong, Flag.MediaManufacturer, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    MediaManufacturerValue = stringValue;

                // Media Model
                stringValue = ProcessStringParameter(parts, null, FlagStrings.MediaModelLong, Flag.MediaModel, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    MediaModelValue = stringValue;

                // Media Part Number
                stringValue = ProcessStringParameter(parts, null, FlagStrings.MediaPartNumberLong, Flag.MediaPartNumber, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    MediaPartNumberValue = stringValue;

                // Media Serial
                stringValue = ProcessStringParameter(parts, null, FlagStrings.MediaSerialLong, Flag.MediaSerial, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    MediaSerialValue = stringValue;

                // Media Title
                stringValue = ProcessStringParameter(parts, null, FlagStrings.MediaTitleLong, Flag.MediaTitle, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    MediaTitleValue = stringValue;

                // MHDD Log
                stringValue = ProcessStringParameter(parts, FlagStrings.MHDDLogShort, FlagStrings.MHDDLogLong, Flag.MHDDLog, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    MHDDLogValue = stringValue;

                // Namespace
                stringValue = ProcessStringParameter(parts, FlagStrings.NamespaceShort, FlagStrings.NamespaceLong, Flag.Namespace, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    NamespaceValue = stringValue;

                // Options -- TODO: Validate options?
                stringValue = ProcessStringParameter(parts, FlagStrings.OptionsShort, FlagStrings.OptionsLong, Flag.Options, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    OptionsValue = stringValue;

                // Output Prefix
                stringValue = ProcessStringParameter(parts, FlagStrings.OutputPrefixShort, FlagStrings.OutputPrefixLong, Flag.OutputPrefix, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    OutputPrefixValue = stringValue;

                // Resume File
                stringValue = ProcessStringParameter(parts, FlagStrings.ResumeFileShort, FlagStrings.ResumeFileLong, Flag.ResumeFile, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    ResumeFileValue = stringValue;

                // Subchannel
                stringValue = ProcessStringParameter(parts, null, FlagStrings.SubchannelLong, Flag.Subchannel, ref i);
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
                stringValue = ProcessStringParameter(parts, FlagStrings.XMLSidecarShort, FlagStrings.XMLSidecarLong, Flag.XMLSidecar, ref i);
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
                case Command.DeviceInfo:
                case Command.DeviceReport:
                case Command.FilesystemList:
                case Command.ImageAnalyze:
                case Command.ImageChecksum:
                case Command.ImageCreateSidecar:
                case Command.ImageDecode:
                case Command.ImageEntropy:
                case Command.ImageInfo:
                case Command.ImagePrint:
                case Command.ImageVerify:
                case Command.MediaInfo:
                case Command.MediaScan:
                    if (!DoesExist(parts, i))
                        return false;

                    InputValue = parts[i].Trim('"');
                    i++;
                    break;

                // Two input values
                case Command.ImageCompare:
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
                case Command.FilesystemExtract:
                case Command.ImageConvert:
                case Command.MediaDump:
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
                case Command.DeviceList:
                case Command.Remote:
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
        /// Get the list of commands that use a given flag
        /// </summary>
        /// <param name="flag">Flag value to get commands for</param>
        /// <returns>List of Commands, if possible</returns>
        private static List<Command> GetSupportedCommands(Flag flag)
        {
            var commands = new List<Command>();
            switch (flag)
            {
                case Flag.Adler32:
                    commands.Add(Command.ImageChecksum);
                    break;
                case Flag.BlockSize:
                    commands.Add(Command.ImageCreateSidecar);
                    break;
                case Flag.Clear:
                    commands.Add(Command.DatabaseUpdate);
                    break;
                case Flag.ClearAll:
                    commands.Add(Command.DatabaseUpdate);
                    break;
                case Flag.Comments:
                    commands.Add(Command.ImageConvert);
                    break;
                case Flag.Count:
                    commands.Add(Command.ImageConvert);
                    break;
                case Flag.CRC16:
                    commands.Add(Command.ImageChecksum);
                    break;
                case Flag.CRC32:
                    commands.Add(Command.ImageChecksum);
                    break;
                case Flag.CRC64:
                    commands.Add(Command.ImageChecksum);
                    break;
                case Flag.Creator:
                    commands.Add(Command.ImageConvert);
                    break;
                case Flag.Debug:
                    commands.Add(Command.NONE);
                    break;
                case Flag.DiskTags:
                    commands.Add(Command.ImageDecode);
                    break;
                case Flag.DriveManufacturer:
                    commands.Add(Command.ImageConvert);
                    break;
                case Flag.DriveModel:
                    commands.Add(Command.ImageConvert);
                    break;
                case Flag.DriveRevision:
                    commands.Add(Command.ImageConvert);
                    break;
                case Flag.DriveSerial:
                    commands.Add(Command.ImageConvert);
                    break;
                case Flag.DuplicatedSectors:
                    commands.Add(Command.ImageEntropy);
                    break;
                case Flag.Eject:
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.Encoding:
                    commands.Add(Command.FilesystemExtract);
                    commands.Add(Command.FilesystemList);
                    commands.Add(Command.ImageAnalyze);
                    commands.Add(Command.ImageCreateSidecar);
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.ExtendedAttributes:
                    commands.Add(Command.FilesystemExtract);
                    break;
                case Flag.Filesystems:
                    commands.Add(Command.ImageAnalyze);
                    break;
                case Flag.FirstPregap:
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.FixOffset:
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.FixSubchannel:
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.FixSubchannelCrc:
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.FixSubchannelPosition:
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.Fletcher16:
                    commands.Add(Command.ImageChecksum);
                    break;
                case Flag.Fletcher32:
                    commands.Add(Command.ImageChecksum);
                    break;
                case Flag.Force:
                    commands.Add(Command.ImageConvert);
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.FormatConvert:
                    commands.Add(Command.ImageConvert);
                    break;
                case Flag.FormatDump:
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.GenerateSubchannels:
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.Help:
                    commands.Add(Command.NONE);
                    break;
                case Flag.ImgBurnLog:
                    commands.Add(Command.MediaScan);
                    break;
                case Flag.Length:
                    commands.Add(Command.ImageDecode);
                    commands.Add(Command.ImagePrint);
                    break;
                case Flag.LongFormat:
                    commands.Add(Command.FilesystemList);
                    break;
                case Flag.LongSectors:
                    commands.Add(Command.ImagePrint);
                    break;
                case Flag.MD5:
                    commands.Add(Command.ImageChecksum);
                    break;
                case Flag.MediaBarcode:
                    commands.Add(Command.ImageConvert);
                    break;
                case Flag.MediaLastSequence:
                    commands.Add(Command.ImageConvert);
                    break;
                case Flag.MediaManufacturer:
                    commands.Add(Command.ImageConvert);
                    break;
                case Flag.MediaModel:
                    commands.Add(Command.ImageConvert);
                    break;
                case Flag.MediaPartNumber:
                    commands.Add(Command.ImageConvert);
                    break;
                case Flag.MediaSequence:
                    commands.Add(Command.ImageConvert);
                    break;
                case Flag.MediaSerial:
                    commands.Add(Command.ImageConvert);
                    break;
                case Flag.MediaTitle:
                    commands.Add(Command.ImageConvert);
                    break;
                case Flag.Metadata:
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.MHDDLog:
                    commands.Add(Command.MediaScan);
                    break;
                case Flag.Namespace:
                    commands.Add(Command.FilesystemExtract);
                    commands.Add(Command.FilesystemList);
                    break;
                case Flag.Options:
                    commands.Add(Command.FilesystemExtract);
                    commands.Add(Command.FilesystemList);
                    commands.Add(Command.ImageConvert);
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.OutputPrefix:
                    commands.Add(Command.DeviceInfo);
                    commands.Add(Command.MediaInfo);
                    break;
                case Flag.Partitions:
                    commands.Add(Command.ImageAnalyze);
                    break;
                case Flag.Persistent:
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.Private:
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.Resume:
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.ResumeFile:
                    commands.Add(Command.ImageConvert);
                    break;
                case Flag.RetryPasses:
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.RetrySubchannel:
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.SectorTags:
                    commands.Add(Command.ImageDecode);
                    break;
                case Flag.SeparatedTracks:
                    commands.Add(Command.ImageChecksum);
                    commands.Add(Command.ImageEntropy);
                    break;
                case Flag.SHA1:
                    commands.Add(Command.ImageChecksum);
                    break;
                case Flag.SHA256:
                    commands.Add(Command.ImageChecksum);
                    break;
                case Flag.SHA384:
                    commands.Add(Command.ImageChecksum);
                    break;
                case Flag.SHA512:
                    commands.Add(Command.ImageChecksum);
                    break;
                case Flag.Skip:
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.SkipCdiReadyHole:
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.SpamSum:
                    commands.Add(Command.ImageChecksum);
                    break;
                case Flag.Speed:
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.Start:
                    commands.Add(Command.ImageDecode);
                    commands.Add(Command.ImagePrint);
                    break;
                case Flag.StopOnError:
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.Subchannel:
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.Tape:
                    commands.Add(Command.ImageCreateSidecar);
                    break;
                case Flag.Trim:
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.Verbose:
                    commands.Add(Command.NONE);
                    break;
                case Flag.VerifyDisc:
                    commands.Add(Command.ImageAnalyze);
                    commands.Add(Command.ImageVerify);
                    break;
                case Flag.VerifySectors:
                    commands.Add(Command.ImageAnalyze);
                    commands.Add(Command.ImageVerify);
                    break;
                case Flag.Version:
                    commands.Add(Command.NONE);
                    break;
                case Flag.WholeDisc:
                    commands.Add(Command.ImageChecksum);
                    commands.Add(Command.ImageEntropy);
                    break;
                case Flag.Width:
                    commands.Add(Command.ImagePrint);
                    break;
                case Flag.XMLSidecar:
                    commands.Add(Command.ImageConvert);
                    commands.Add(Command.MediaDump);
                    break;

                case Flag.NONE:
                default:
                    return commands;
            }

            return commands;
        }

        #endregion

        #region Process Parameter Helpers

        /// <summary>
        /// Process a flag parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="flag">Flag value corresponding to the flag</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <returns>True if the parameter was processed successfully or skipped, false otherwise</returns>
        private bool ProcessFlagParameter(List<string> parts, string shortFlagString, string longFlagString, Flag flag, ref int i)
        {
            if (parts == null)
                return false;

            if (parts[i] == shortFlagString || parts[i] == longFlagString)
            {
                if (!GetSupportedCommands(flag).Contains(BaseCommand))
                    return false;

                this[flag] = true;
            }

            return true;
        }

        /// <summary>
        /// Process a boolean parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="flag">Flag value corresponding to the flag</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <returns>True if the parameter was processed successfully or skipped, false otherwise</returns>
        private bool ProcessBooleanParameter(List<string> parts, string shortFlagString, string longFlagString, Flag flag, ref int i)
        {
            if (parts == null)
                return false;

            if (parts[i] == shortFlagString || parts[i] == longFlagString)
            {
                if (!GetSupportedCommands(flag).Contains(BaseCommand))
                    return false;
                else if (!DoesExist(parts, i + 1))
                    return false;
                else if (!IsValidBool(parts[i + 1]))
                    return false;

                this[flag] = bool.Parse(parts[i + 1]);
                i++;
            }

            return true;
        }

        /// <summary>
        /// Process an sbyte parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="flag">Flag value corresponding to the flag</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <returns>SByte value if success, Int16.MinValue if skipped, null on error/returns>
        private sbyte? ProcessInt8Parameter(List<string> parts, string shortFlagString, string longFlagString, Flag flag, ref int i)
        {
            if (parts == null)
                return null;

            if (parts[i] == shortFlagString || parts[i] == longFlagString)
            {
                if (!GetSupportedCommands(flag).Contains(BaseCommand))
                    return null;
                else if (!DoesExist(parts, i + 1))
                    return null;
                else if (!IsValidInt8(parts[i + 1]))
                    return null;

                this[flag] = true;
                i++;
                return sbyte.Parse(parts[i]);
            }

            return SByte.MinValue;
        }

        /// <summary>
        /// Process an Int16 parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="flag">Flag value corresponding to the flag</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <returns>Int16 value if success, Int16.MinValue if skipped, null on error/returns>
        private short? ProcessInt16Parameter(List<string> parts, string shortFlagString, string longFlagString, Flag flag, ref int i)
        {
            if (parts == null)
                return null;

            if (parts[i] == shortFlagString || parts[i] == longFlagString)
            {
                if (!GetSupportedCommands(flag).Contains(BaseCommand))
                    return null;
                else if (!DoesExist(parts, i + 1))
                    return null;
                else if (!IsValidInt16(parts[i + 1]))
                    return null;

                this[flag] = true;
                i++;
                return short.Parse(parts[i]);
            }

            return Int16.MinValue;
        }

        /// <summary>
        /// Process an Int32 parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="flag">Flag value corresponding to the flag</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <returns>Int32 value if success, Int32.MinValue if skipped, null on error/returns>
        private int? ProcessInt32Parameter(List<string> parts, string shortFlagString, string longFlagString, Flag flag, ref int i)
        {
            if (parts == null)
                return null;

            if (parts[i] == shortFlagString || parts[i] == longFlagString)
            {
                if (!GetSupportedCommands(flag).Contains(BaseCommand))
                    return null;
                else if (!DoesExist(parts, i + 1))
                    return null;
                else if (!IsValidInt32(parts[i + 1]))
                    return null;

                this[flag] = true;
                i++;
                return int.Parse(parts[i]);
            }

            return Int32.MinValue;
        }

        /// <summary>
        /// Process an Int64 parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="flag">Flag value corresponding to the flag</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <returns>Int64 value if success, Int64.MinValue if skipped, null on error/returns>
        private long? ProcessInt64Parameter(List<string> parts, string shortFlagString, string longFlagString, Flag flag, ref int i)
        {
            if (parts == null)
                return null;

            if (parts[i] == shortFlagString || parts[i] == longFlagString)
            {
                if (!GetSupportedCommands(flag).Contains(BaseCommand))
                    return null;
                else if (!DoesExist(parts, i + 1))
                    return null;
                else if (!IsValidInt64(parts[i + 1]))
                    return null;

                this[flag] = true;
                i++;
                return long.Parse(parts[i]);
            }

            return Int64.MinValue;
        }

        /// <summary>
        /// Process a string parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="flag">Flag value corresponding to the flag</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <returns>String value if possible, string.Empty on missing, null on error</returns>
        private string ProcessStringParameter(List<string> parts, string shortFlagString, string longFlagString, Flag flag, ref int i)
        {
            if (parts == null)
                return null;

            if (parts[i] == shortFlagString || parts[i] == longFlagString)
            {
                if (!GetSupportedCommands(flag).Contains(BaseCommand))
                    return null;
                else if (!DoesExist(parts, i + 1))
                    return null;
                else if (string.IsNullOrWhiteSpace(parts[i + 1]))
                    return null;

                this[flag] = true;
                i++;
                return parts[i].Trim('"');
            }

            return string.Empty;
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
            CueSheet cueSheet = new CueSheet();
            cueSheet.Performer = string.Join(", ", cicmSidecar.Performer ?? new string[0]);
            cueSheet.Files = new List<CueFile>();

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
                    CueTrack cueTrack = new CueTrack();
                    cueTrack.Number = (int)(track.Sequence?.TrackNumber ?? 0);
                    cueTrack.DataType = ConvertToDataType(track.TrackType1, track.BytesPerSector);
                    cueTrack.Flags = ConvertToTrackFlag(track.Flags);
                    cueTrack.ISRC = track.ISRC;

                    // Create cue file entry
                    CueFile cueFile = new CueFile();
                    cueFile.FileName = GenerateTrackName(basePath, (int)totalTracks, cueTrack.Number, opticalDisc.DiscType);
                    cueFile.FileType = CueFileType.BINARY;
                    cueFile.Tracks = new List<CueTrack>();

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
        private static bool GetXboxDMIInfo(CICMMetadataType cicmSidecar, out string serial, out string version, out RedumpRegion? region)
        {
            serial = null; version = null; region = RedumpRegion.World;

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
        private static bool GetXbox360DMIInfo(CICMMetadataType cicmSidecar, out string serial, out string version, out RedumpRegion? region)
        {
            serial = null; version = null; region = RedumpRegion.World;

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
