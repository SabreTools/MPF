using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using DICUI.Data;
using DICUI.Utilities;

namespace DICUI.Aaru
{
    /// <summary>
    /// Represents a generic set of Aaru parameters
    /// </summary>
    public class Parameters : BaseParameters
    {
        /// <summary>
        /// Base command to run
        /// </summary>
        public Command BaseCommand { get; set; }

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

        /// <summary>
        /// Populate a Parameters object from a param string
        /// </summary>
        /// <param name="parameters">String possibly representing a set of parameters</param>
        public Parameters(string parameters)
            : base(parameters)
        {
            this.InternalProgram = InternalProgram.Aaru;
        }

        /// <summary>
        /// Generate parameters based on a set of known inputs
        /// </summary>
        /// <param name="system">KnownSystem value to use</param>
        /// <param name="type">MediaType value to use</param>
        /// <param name="driveLetter">Drive letter to use</param>
        /// <param name="filename">Filename to use</param>
        /// <param name="driveSpeed">Drive speed to use</param>
        /// <param name="paranoid">Enable paranoid mode (safer dumping)</param>
        /// <param name="quietMode">Enable quiet mode (no beeps)</param>
        /// <param name="retryCount">User-defined reread count</param>
        public Parameters(KnownSystem? system, MediaType? type, char driveLetter, string filename, int? driveSpeed, bool paranoid, bool quietMode, int retryCount)
            : base(system, type, driveLetter, filename, driveSpeed, paranoid, quietMode, retryCount)
        {
        }

        /// <summary>
        /// Reset all special variables to have default values
        /// </summary>
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

        /// <summary>
        /// Set default parameters for a given system and media type
        /// </summary>
        /// <param name="system">KnownSystem value to use</param>
        /// <param name="type">MediaType value to use</param>
        /// <param name="driveLetter">Drive letter to use</param>
        /// <param name="filename">Filename to use</param>
        /// <param name="driveSpeed">Drive speed to use</param>
        /// <param name="paranoid">Enable paranoid mode (safer dumping)</param>
        /// <param name="retryCount">User-defined reread count</param>
        protected override void SetDefaultParameters(
            KnownSystem? system,
            MediaType? type,
            char driveLetter,
            string filename,
            int? driveSpeed,
            bool paranoid,
            int retryCount)
        {
            BaseCommand = Command.MediaDump;

            InputValue = $"\\\\?\\{driveLetter}:";
            OutputValue = filename;

            this[Flag.Force] = true;

            if (driveSpeed != null)
            {
                this[Flag.Speed] = true;
                SpeedValue = (sbyte?)driveSpeed;
            }

            // First check to see if the combination of system and MediaType is valid
            var validTypes = Utilities.Validators.GetValidMediaTypes(system);
            if (!validTypes.Contains(type))
                return;

            // Set retry count
            if (retryCount > 0)
            {
                this[Flag.RetryPasses] = true;
                RetryPassesValue = (short)retryCount;
            }

            // Paranoia in Aaru means more output
            if (paranoid)
            {
                this[Flag.Debug] = true;
                this[Flag.Verbose] = true;
            }

            // TODO: Look at dump-media formats and the like and see what options there are there to fill in defaults
            // Now sort based on disc type
            switch (type)
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

        /// <summary>
        /// Blindly generate a parameter string based on the inputs
        /// </summary>
        /// <returns>Correctly formatted parameter string, null on error</returns>
        public override string GenerateParameters()
        {
            List<string> parameters = new List<string>();

            #region Pre-command flags

            // Debug
            if (this[Flag.Debug] != null)
                parameters.Add($"{Converters.LongName(Flag.Debug)} {this[Flag.Debug]}");

            // Verbose
            if (this[Flag.Verbose] != null)
                parameters.Add($"{Converters.LongName(Flag.Verbose)} {this[Flag.Verbose]}");

            // Version
            if (this[Flag.Version] != null)
                parameters.Add($"{Converters.LongName(Flag.Version)} {this[Flag.Version]}");

            #endregion

            if (BaseCommand != Command.NONE)
                parameters.Add(Converters.LongName(BaseCommand));
            else
                return null;

            #region Boolean flags

            // Adler-32
            if (GetSupportedCommands(Flag.Adler32).Contains(BaseCommand))
            {
                if (this[Flag.Adler32] != null)
                    parameters.Add($"{Converters.LongName(Flag.Adler32)} {this[Flag.Adler32]}");
            }

            // Clear
            if (GetSupportedCommands(Flag.Clear).Contains(BaseCommand))
            {
                if (this[Flag.Clear] != null)
                    parameters.Add($"{Converters.LongName(Flag.Clear)} {this[Flag.Clear]}");
            }

            // Clear All
            if (GetSupportedCommands(Flag.ClearAll).Contains(BaseCommand))
            {
                if (this[Flag.ClearAll] != null)
                    parameters.Add($"{Converters.LongName(Flag.ClearAll)} {this[Flag.ClearAll]}");
            }

            // CRC16
            if (GetSupportedCommands(Flag.CRC16).Contains(BaseCommand))
            {
                if (this[Flag.CRC16] != null)
                    parameters.Add($"{Converters.LongName(Flag.CRC16)} {this[Flag.CRC16]}");
            }

            // CRC32
            if (GetSupportedCommands(Flag.CRC32).Contains(BaseCommand))
            {
                if (this[Flag.CRC32] != null)
                    parameters.Add($"{Converters.LongName(Flag.CRC32)} {this[Flag.CRC32]}");
            }

            // CRC64
            if (GetSupportedCommands(Flag.CRC64).Contains(BaseCommand))
            {
                if (this[Flag.CRC64] != null)
                    parameters.Add($"{Converters.LongName(Flag.CRC64)} {this[Flag.CRC64]}");
            }

            // Disk Tags
            if (GetSupportedCommands(Flag.DiskTags).Contains(BaseCommand))
            {
                if (this[Flag.DiskTags] != null)
                    parameters.Add($"{Converters.LongName(Flag.DiskTags)} {this[Flag.DiskTags]}");
            }

            // Duplicated Sectors
            if (GetSupportedCommands(Flag.DuplicatedSectors).Contains(BaseCommand))
            {
                if (this[Flag.DuplicatedSectors] != null)
                    parameters.Add($"{Converters.LongName(Flag.DuplicatedSectors)} {this[Flag.DuplicatedSectors]}");
            }

            // Extended Attributes
            if (GetSupportedCommands(Flag.ExtendedAttributes).Contains(BaseCommand))
            {
                if (this[Flag.ExtendedAttributes] != null)
                    parameters.Add($"{Converters.LongName(Flag.ExtendedAttributes)} {this[Flag.ExtendedAttributes]}");
            }

            // Filesystems
            if (GetSupportedCommands(Flag.Filesystems).Contains(BaseCommand))
            {
                if (this[Flag.Filesystems] != null)
                    parameters.Add($"{Converters.LongName(Flag.Filesystems)} {this[Flag.Filesystems]}");
            }

            // First Pregap
            if (GetSupportedCommands(Flag.FirstPregap).Contains(BaseCommand))
            {
                if (this[Flag.FirstPregap] != null)
                    parameters.Add($"{Converters.LongName(Flag.FirstPregap)} {this[Flag.FirstPregap]}");
            }

            // Fix Offset
            if (GetSupportedCommands(Flag.FixOffset).Contains(BaseCommand))
            {
                if (this[Flag.FixOffset] != null)
                    parameters.Add($"{Converters.LongName(Flag.FixOffset)} {this[Flag.FixOffset]}");
            }

            // Fletcher-16
            if (GetSupportedCommands(Flag.Fletcher16).Contains(BaseCommand))
            {
                if (this[Flag.Fletcher16] != null)
                    parameters.Add($"{Converters.LongName(Flag.Fletcher16)} {this[Flag.Fletcher16]}");
            }

            // Fletcher-32
            if (GetSupportedCommands(Flag.Fletcher32).Contains(BaseCommand))
            {
                if (this[Flag.Fletcher32] != null)
                    parameters.Add($"{Converters.LongName(Flag.Fletcher32)} {this[Flag.Fletcher32]}");
            }

            // Force
            if (GetSupportedCommands(Flag.Force).Contains(BaseCommand))
            {
                if (this[Flag.Force] != null)
                    parameters.Add($"{Converters.LongName(Flag.Force)} {this[Flag.Force]}");
            }

            // Long Format
            if (GetSupportedCommands(Flag.LongFormat).Contains(BaseCommand))
            {
                if (this[Flag.LongFormat] != null)
                    parameters.Add($"{Converters.LongName(Flag.LongFormat)} {this[Flag.LongFormat]}");
            }

            // Long Sectors
            if (GetSupportedCommands(Flag.LongSectors).Contains(BaseCommand))
            {
                if (this[Flag.LongSectors] != null)
                    parameters.Add($"{Converters.LongName(Flag.LongSectors)} {this[Flag.LongSectors]}");
            }

            // MD5
            if (GetSupportedCommands(Flag.MD5).Contains(BaseCommand))
            {
                if (this[Flag.MD5] != null)
                    parameters.Add($"{Converters.LongName(Flag.MD5)} {this[Flag.MD5]}");
            }

            // Metadata
            if (GetSupportedCommands(Flag.Metadata).Contains(BaseCommand))
            {
                if (this[Flag.Metadata] != null)
                    parameters.Add($"{Converters.LongName(Flag.Metadata)} {this[Flag.Metadata]}");
            }

            // Partitions
            if (GetSupportedCommands(Flag.Partitions).Contains(BaseCommand))
            {
                if (this[Flag.Partitions] != null)
                    parameters.Add($"{Converters.LongName(Flag.Partitions)} {this[Flag.Partitions]}");
            }

            // Persistent
            if (GetSupportedCommands(Flag.Persistent).Contains(BaseCommand))
            {
                if (this[Flag.Persistent] != null)
                    parameters.Add($"{Converters.LongName(Flag.Persistent)} {this[Flag.Persistent]}");
            }

            // Resume
            if (GetSupportedCommands(Flag.Resume).Contains(BaseCommand))
            {
                if (this[Flag.Resume] != null)
                    parameters.Add($"{Converters.LongName(Flag.Resume)} {this[Flag.Resume]}");
            }

            // Sector Tags
            if (GetSupportedCommands(Flag.SectorTags).Contains(BaseCommand))
            {
                if (this[Flag.SectorTags] != null)
                    parameters.Add($"{Converters.LongName(Flag.SectorTags)} {this[Flag.SectorTags]}");
            }

            // Separated Tracks
            if (GetSupportedCommands(Flag.SeparatedTracks).Contains(BaseCommand))
            {
                if (this[Flag.SeparatedTracks] != null)
                    parameters.Add($"{Converters.LongName(Flag.SeparatedTracks)} {this[Flag.SeparatedTracks]}");
            }

            // SHA-1
            if (GetSupportedCommands(Flag.SHA1).Contains(BaseCommand))
            {
                if (this[Flag.SHA1] != null)
                    parameters.Add($"{Converters.LongName(Flag.SHA1)} {this[Flag.SHA1]}");
            }

            // SHA-256
            if (GetSupportedCommands(Flag.SHA256).Contains(BaseCommand))
            {
                if (this[Flag.SHA256] != null)
                    parameters.Add($"{Converters.LongName(Flag.SHA256)} {this[Flag.SHA256]}");
            }

            // SHA-384
            if (GetSupportedCommands(Flag.SHA384).Contains(BaseCommand))
            {
                if (this[Flag.SHA384] != null)
                    parameters.Add($"{Converters.LongName(Flag.SHA384)} {this[Flag.SHA384]}");
            }

            // SHA-512
            if (GetSupportedCommands(Flag.SHA512).Contains(BaseCommand))
            {
                if (this[Flag.SHA512] != null)
                    parameters.Add($"{Converters.LongName(Flag.SHA512)} {this[Flag.SHA512]}");
            }

            // SpamSum
            if (GetSupportedCommands(Flag.SpamSum).Contains(BaseCommand))
            {
                if (this[Flag.SpamSum] != null)
                    parameters.Add($"{Converters.LongName(Flag.SpamSum)} {this[Flag.SpamSum]}");
            }

            // Stop on Error
            if (GetSupportedCommands(Flag.StopOnError).Contains(BaseCommand))
            {
                if (this[Flag.StopOnError] != null)
                    parameters.Add($"{Converters.LongName(Flag.StopOnError)} {this[Flag.StopOnError]}");
            }

            // Tape
            if (GetSupportedCommands(Flag.Tape).Contains(BaseCommand))
            {
                if (this[Flag.Tape] != null)
                    parameters.Add($"{Converters.LongName(Flag.Tape)} {this[Flag.Tape]}");
            }

            // Trim
            if (GetSupportedCommands(Flag.Trim).Contains(BaseCommand))
            {
                if (this[Flag.Trim] != null)
                    parameters.Add($"{Converters.LongName(Flag.Trim)} {this[Flag.Trim]}");
            }

            // Verify Disc
            if (GetSupportedCommands(Flag.VerifyDisc).Contains(BaseCommand))
            {
                if (this[Flag.VerifyDisc] != null)
                    parameters.Add($"{Converters.LongName(Flag.VerifyDisc)} {this[Flag.VerifyDisc]}");
            }

            // Verify Sectors
            if (GetSupportedCommands(Flag.VerifySectors).Contains(BaseCommand))
            {
                if (this[Flag.VerifySectors] != null)
                    parameters.Add($"{Converters.LongName(Flag.VerifySectors)} {this[Flag.VerifySectors]}");
            }

            // Whole Disc
            if (GetSupportedCommands(Flag.WholeDisc).Contains(BaseCommand))
            {
                if (this[Flag.WholeDisc] != null)
                    parameters.Add($"{Converters.LongName(Flag.WholeDisc)} {this[Flag.WholeDisc]}");
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

            // Length -- TODO: Support "all" case
            if (GetSupportedCommands(Flag.Length).Contains(BaseCommand))
            {
                if (this[Flag.Length] == true && LengthValue != null)
                    parameters.Add($"{Converters.LongName(Flag.Length)} {LengthValue}");
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

        /// <summary>
        /// Get the input path from the implementation
        /// </summary>
        /// <returns>String representing the path, null on error</returns>
        public override string InputPath() => InputValue;

        /// <summary>
        /// Get the output path from the implementation
        /// </summary>
        /// <returns>String representing the path, null on error</returns>
        public override string OutputPath() => OutputValue;

        /// <summary>
        /// Get the processing speed from the implementation
        /// </summary>
        /// <returns>int? representing the speed, null on error</returns>
        public override int? GetSpeed() => SpeedValue;

        /// <summary>
        /// Set the processing speed int the implementation
        /// </summary>
        /// <param name="speed">int? representing the speed</param>
        public override void SetSpeed(int? speed) => SpeedValue = (sbyte?)speed;

        /// <summary>
        /// Get the MediaType from the current set of parameters
        /// </summary>
        /// <returns>MediaType value if successful, null on error</returns>
        public override MediaType? GetMediaType() => null;

        /// <summary>
        /// Gets if the current command is considered a dumping command or not
        /// </summary>
        /// <returns>True if it's a dumping command, false otherwise</returns>
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

        /// <summary>
        /// Scan a possible parameter string and populate whatever possible
        /// </summary>
        /// <param name="parameters">String possibly representing parameters</param>
        /// <returns></returns>
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
                ProcessBooleanParameter(parts, FlagStrings.DebugShort, FlagStrings.DebugLong, Flag.Debug, ref start);

                // Verbose
                ProcessBooleanParameter(parts, FlagStrings.VerboseShort, FlagStrings.VerboseLong, Flag.Verbose, ref start);

                // Verbose
                ProcessBooleanParameter(parts, null, FlagStrings.VersionLong, Flag.Version, ref start);

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
                ProcessBooleanParameter(parts, FlagStrings.Adler32Short, FlagStrings.Adler32Long, Flag.Adler32, ref i);

                // Clear
                ProcessBooleanParameter(parts, null, FlagStrings.ClearLong, Flag.Clear, ref i);

                // Clear All
                ProcessBooleanParameter(parts, null, FlagStrings.ClearAllLong, Flag.ClearAll, ref i);

                // CRC16
                ProcessBooleanParameter(parts, null, FlagStrings.CRC16Long, Flag.CRC16, ref i);

                // CRC32
                ProcessBooleanParameter(parts, FlagStrings.CRC32Short, FlagStrings.CRC32Long, Flag.CRC32, ref i);

                // CRC64
                ProcessBooleanParameter(parts, null, FlagStrings.CRC64Long, Flag.CRC64, ref i);

                // Disk Tags
                ProcessBooleanParameter(parts, FlagStrings.DiskTagsShort, FlagStrings.DiskTagsLong, Flag.DiskTags, ref i);

                // Deduplicated Sectors
                ProcessBooleanParameter(parts, FlagStrings.DuplicatedSectorsShort, FlagStrings.DuplicatedSectorsLong, Flag.DuplicatedSectors, ref i);

                // Extended Attributes
                ProcessBooleanParameter(parts, FlagStrings.ExtendedAttributesShort, FlagStrings.ExtendedAttributesLong, Flag.ExtendedAttributes, ref i);

                // Filesystems
                ProcessBooleanParameter(parts, FlagStrings.FilesystemsShort, FlagStrings.FilesystemsLong, Flag.Filesystems, ref i);

                // First Pregap
                ProcessBooleanParameter(parts, null, FlagStrings.FirstPregapLong, Flag.FirstPregap, ref i);

                // Fix Offset
                ProcessBooleanParameter(parts, null, FlagStrings.FixOffsetLong, Flag.FixOffset, ref i);

                // Fletcher-16
                ProcessBooleanParameter(parts, null, FlagStrings.Fletcher16Long, Flag.Fletcher16, ref i);

                // Fletcher-32
                ProcessBooleanParameter(parts, null, FlagStrings.Fletcher32Long, Flag.Fletcher32, ref i);

                // Force
                ProcessBooleanParameter(parts, FlagStrings.ForceShort, FlagStrings.ForceLong, Flag.Force, ref i);

                // Long Format
                ProcessBooleanParameter(parts, FlagStrings.LongFormatShort, FlagStrings.LongFormatLong, Flag.LongFormat, ref i);

                // Long Sectors
                ProcessBooleanParameter(parts, FlagStrings.LongSectorsShort, FlagStrings.LongSectorsLong, Flag.LongSectors, ref i);

                // MD5
                ProcessBooleanParameter(parts, FlagStrings.MD5Short, FlagStrings.MD5Long, Flag.MD5, ref i);

                // Metadata
                ProcessBooleanParameter(parts, null, FlagStrings.MetadataLong, Flag.Metadata, ref i);

                // Partitions
                ProcessBooleanParameter(parts, FlagStrings.PartitionsShort, FlagStrings.PartitionsLong, Flag.Partitions, ref i);

                // Persistent
                ProcessBooleanParameter(parts, null, FlagStrings.PersistentLong, Flag.Persistent, ref i);

                // Resume
                ProcessBooleanParameter(parts, FlagStrings.ResumeShort, FlagStrings.ResumeLong, Flag.Resume, ref i);

                // Sector Tags
                ProcessBooleanParameter(parts, FlagStrings.SectorTagsShort, FlagStrings.SectorTagsLong, Flag.SectorTags, ref i);

                // Separated Tracks
                ProcessBooleanParameter(parts, FlagStrings.SeparatedTracksShort, FlagStrings.SeparatedTracksLong, Flag.SeparatedTracks, ref i);

                // SHA-1
                ProcessBooleanParameter(parts, FlagStrings.SHA1Short, FlagStrings.SHA1Long, Flag.SHA1, ref i);

                // SHA-256
                ProcessBooleanParameter(parts, null, FlagStrings.SHA256Long, Flag.SHA256, ref i);

                // SHA-384
                ProcessBooleanParameter(parts, null, FlagStrings.SHA384Long, Flag.SHA384, ref i);

                // SHA-512
                ProcessBooleanParameter(parts, null, FlagStrings.SHA512Long, Flag.SHA512, ref i);

                // SpamSum
                ProcessBooleanParameter(parts, FlagStrings.SpamSumShort, FlagStrings.SpamSumLong, Flag.SpamSum, ref i);

                // Stop on Error
                ProcessBooleanParameter(parts, FlagStrings.StopOnErrorShort, FlagStrings.StopOnErrorLong, Flag.StopOnError, ref i);

                // Tape
                ProcessBooleanParameter(parts, FlagStrings.TapeShort, FlagStrings.TapeLong, Flag.Tape, ref i);

                // Trim
                ProcessBooleanParameter(parts, null, FlagStrings.TrimLong, Flag.Trim, ref i);

                // Verify Disc
                ProcessBooleanParameter(parts, FlagStrings.VerifyDiscShort, FlagStrings.VerifyDiscLong, Flag.VerifyDisc, ref i);

                // Verify Sectors
                ProcessBooleanParameter(parts, FlagStrings.VerifySectorsShort, FlagStrings.VerifySectorsLong, Flag.VerifySectors, ref i);

                // Whole Disc
                ProcessBooleanParameter(parts, FlagStrings.WholeDiscShort, FlagStrings.WholeDiscLong, Flag.VerifySectors, ref i);

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

                // Length -- Can also be "all"
                longValue = ProcessInt64Parameter(parts, FlagStrings.LengthShort, FlagStrings.LengthLong, Flag.Length, ref i);
                if (longValue == null && longValue != Int64.MinValue)
                    LengthValue = longValue;

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
                    SubchannelValue = stringValue;

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

        /// <summary>
        /// Validate if all required output files exist
        /// </summary>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <param name="system">KnownSystem type representing the media</param>
        /// <param name="type">MediaType type representing the media</param>
        /// <returns></returns>
        public override bool CheckAllOutputFilesExist(string basePath, KnownSystem? system, MediaType? type)
        {
            return File.Exists(basePath + ".cicm.xml")
                && File.Exists(basePath + ".aif")
                && File.Exists(basePath + ".ibg")
                && File.Exists(basePath + ".log")
                && File.Exists(basePath + ".mhddlog.bin")
                && File.Exists(basePath + ".resume.xml");
        }

        /// <summary>
        /// Generate a SubmissionInfo for the output files
        /// </summary>
        /// <param name="info">Base submission info to fill in specifics for</param>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <param name="system">KnownSystem type representing the media</param>
        /// <param name="type">MediaType type representing the media</param>
        /// <param name="drive">Drive representing the disc to get information from</param>
        /// <returns></returns>
        public override void GenerateSubmissionInfo(SubmissionInfo info, string basePath, KnownSystem? system, MediaType? type, Drive drive)
        {
            // TODO: Fill in submission info specifics for Aaru
            string outputDirectory = Path.GetDirectoryName(basePath);

            // Fill in the hash data
            info.TracksAndWriteOffsets.ClrMameProData = GenerateDatfile(basePath + ".cicm.xml");

            switch (type)
            {
                case MediaType.CDROM:
                case MediaType.GDROM: // TODO: Verify GD-ROM outputs this
                    info.TracksAndWriteOffsets.Cuesheet = GenerateCuesheet(basePath + ".aif", basePath + ".cicm.xml") ?? "";
                    break;
            }

            switch (system)
            {

            }
        }

        /// <summary>
        /// Get the list of commands that use a given flag
        /// </summary>
        /// <param name="flag">Flag value to get commands for</param>
        /// <returns>List of Commands, if possible</returns>
        private List<Command> GetSupportedCommands(Flag flag)
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
                case Flag.Resume:
                    commands.Add(Command.MediaDump);
                    break;
                case Flag.ResumeFile:
                    commands.Add(Command.ImageConvert);
                    break;
                case Flag.RetryPasses:
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

        #region Information Extraction Methods

        /// <summary>
        /// Generate a cuesheet string based on CICM sidecar data
        /// </summary>
        /// <param name="aif">AIF output image</param>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <returns>String containing the cuesheet, null on error</returns>
        private string GenerateCuesheet(string aif, string cicmSidecar)
        {
            // Note that this path only generates a cuesheet against a single BIN, not split

            // If the files don't exist, we can't get info from them
            if (!File.Exists(aif) || !File.Exists(cicmSidecar))
                return null;

            // If this is being run in Check, we can't run Aaru
            if (string.IsNullOrEmpty(ExecutablePath))
                return null;

            var convertParams = new Parameters(null)
            {
                BaseCommand = Command.ImageConvert,

                InputValue = aif,
                OutputValue = aif + ".cue",
            };

            convertParams[Flag.XMLSidecar] = true;
            convertParams.XMLSidecarValue = cicmSidecar;

            // TODO: How do we get around this generation???
            ExecuteInternalProgram(convertParams).ConfigureAwait(false).GetAwaiter().GetResult();

            File.Delete(aif + ".bin");
            return GetFullFile(aif + ".cue");
        }

        /// <summary>
        /// Generate a CMP XML datfile string based on CICM sidecar data
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <returns>String containing the datfile, null on error</returns>
        private string GenerateDatfile(string cicmSidecar)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(cicmSidecar))
                return null;

            // Required variables
            int totalTracks = 0;
            string datfile = string.Empty;

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

            // Only care about CICM sidecar files
            xtr.MoveToContent();
            if (xtr.Name != "CICMMetadata")
                return null;

            // Only care about OpticalDisc types
            // TODO: Aaru - Support floppy images
            xtr = xtr.ReadSubtree();
            xtr.MoveToContent();
            xtr.Read();
            if (xtr.Name != "OpticalDisc")
                return null;

            // Get track count and all tracks now
            xtr = xtr.ReadSubtree();
            xtr.MoveToContent();
            xtr.Read();
            while (!xtr.EOF)
            {
                // We only want elements
                if (xtr.NodeType != XmlNodeType.Element)
                {
                    xtr.Read();
                    continue;
                }

                switch (xtr.Name)
                {
                    // Total track count
                    case "Tracks":
                        totalTracks = xtr.ReadElementContentAsInt();
                        break;

                    // Individual track data
                    case "Track":
                        XmlReader trackReader = xtr.ReadSubtree();
                        if (trackReader == null)
                            return null;

                        int trackNumber = -1;
                        long size = -1;
                        string crc32 = string.Empty;
                        string md5 = string.Empty;
                        string sha1 = string.Empty;

                        trackReader.MoveToContent();
                        trackReader.Read();
                        while (!trackReader.EOF)
                        {
                            // We only want elements
                            if (trackReader.NodeType != XmlNodeType.Element)
                            {
                                trackReader.Read();
                                continue;
                            }

                            switch (trackReader.Name)
                            {
                                // Track size
                                case "Size":
                                    size = trackReader.ReadElementContentAsLong();
                                    break;

                                // Track number
                                case "Sequence":
                                    XmlReader sequenceReader = trackReader.ReadSubtree();
                                    if (sequenceReader == null)
                                        return null;

                                    sequenceReader.MoveToContent();
                                    sequenceReader.Read();
                                    while (!sequenceReader.EOF)
                                    {
                                        // We only want elements
                                        if (sequenceReader.NodeType != XmlNodeType.Element)
                                        {
                                            sequenceReader.Read();
                                            continue;
                                        }

                                        switch (sequenceReader.Name)
                                        {
                                            case "TrackNumber":
                                                trackNumber = sequenceReader.ReadElementContentAsInt();
                                                break;

                                            default:
                                                trackReader.Skip();
                                                break;
                                        }
                                    }

                                    // Skip the sequence now that we've processed it
                                    trackReader.Skip();

                                    break;

                                // Checksums
                                case "Checksums":
                                    XmlReader checksumReader = trackReader.ReadSubtree();
                                    if (checksumReader == null)
                                        return null;

                                    checksumReader.MoveToContent();
                                    checksumReader.Read();
                                    while (!checksumReader.EOF)
                                    {
                                        // We only want elements
                                        if (checksumReader.NodeType != XmlNodeType.Element)
                                        {
                                            checksumReader.Read();
                                            continue;
                                        }

                                        switch (checksumReader.Name)
                                        {
                                            case "Checksum":
                                                string checksumType = checksumReader.GetAttribute("type");
                                                string checksumValue = checksumReader.ReadElementContentAsString();
                                                switch (checksumType)
                                                {
                                                    case "crc32":
                                                        crc32 = checksumValue;
                                                        break;
                                                    case "md5":
                                                        md5 = checksumValue;
                                                        break;
                                                    case "sha1":
                                                        sha1 = checksumValue;
                                                        break;
                                                }

                                                break;

                                            default:
                                                checksumReader.Skip();
                                                break;
                                        }
                                    }

                                    // Skip the checksums now that we've processed it
                                    trackReader.Skip();

                                    break;

                                default:
                                    trackReader.Skip();
                                    break;
                            }
                        }

                        // Build the track datfile data and append
                        string trackName = Path.GetFileName(cicmSidecar).Replace(".cicm.xml", string.Empty);
                        if (totalTracks == 1)
                        {
                            datfile += $"<rom name=\"{trackName}.bin\" size=\"{size}\" crc=\"{crc32}\" md5=\"{md5}\" sha1=\"{sha1}\" />\n";
                        }
                        else if (totalTracks > 1 && totalTracks < 10)
                        {
                            datfile += $"<rom name=\"{trackName} (Track {trackNumber}).bin\" size=\"{size}\" crc=\"{crc32}\" md5=\"{md5}\" sha1=\"{sha1}\" />\n";
                        }
                        else
                        {
                            datfile += $"<rom name=\"{trackName} (Track {trackNumber:2}).bin\" size=\"{size}\" crc=\"{crc32}\" md5=\"{md5}\" sha1=\"{sha1}\" />\n";
                        }

                        // Skip the track now that we've processed it
                        xtr.Skip();

                        break;

                    default:
                        xtr.Skip();
                        break;
                }
            }

            return datfile;
        }

        #endregion
    }
}
