using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DICUI.Data;

namespace DICUI.Utilities
{
    /// <summary>
    /// Represents a generic set of DiscImageChef parameters
    /// </summary>
    public class ChefParameters
    {
        /// <summary>
        /// Base DiscImageChef command to run
        /// </summary>
        public ChefCommand Command { get; set; }

        /// <summary>
        /// Set of flags to pass to DiscImageCreator
        /// </summary>
        private Dictionary<ChefFlag, bool?> _flags = new Dictionary<ChefFlag, bool?>();
        public bool? this[ChefFlag key]
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
        internal IEnumerable<ChefFlag> Keys => _flags.Keys;

        #region DiscImageChef Flag Values

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
        /// Populate a ChefParameters object from a param string
        /// </summary>
        /// <param name="parameters">String possibly representing a set of parameters</param>
        public ChefParameters(string parameters)
        {
            // If any parameters are not valid, wipe out everything
            if (!ValidateAndSetParameters(parameters))
            {
                Command = ChefCommand.NONE;

                _flags = new Dictionary<ChefFlag, bool?>();

                // TODO: Recreate this list later
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
                ResumeFileValue = null;
                RetryPassesValue = null;
                SkipValue = null;
                SpeedValue = null;
                StartValue = null;
                SubchannelValue = null;
                WidthValue = null;
                XMLSidecarValue = null;
            }
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
        /// <param name="retryPassesCount">User-defined retry passes count</param>
        public ChefParameters(KnownSystem? system, MediaType? type, char driveLetter, string filename, int? driveSpeed, bool paranoid, int retryPassesCount)
        {
            Command = ChefCommand.MediaDump;

            InputValue = $"\\\\?\\{driveLetter.ToString()}:";
            OutputValue = filename;

            this[ChefFlag.Force] = true;

            if (driveSpeed != null)
            {
                this[ChefFlag.Speed] = true;
                SpeedValue = (sbyte?)driveSpeed;
            }

            SetDefaultParameters(system, type, paranoid, retryPassesCount);
        }
        
        /// <summary>
        /// Blindly generate a parameter string based on the inputs
        /// </summary>
        /// <returns>Correctly formatted parameter string, null on error</returns>
        public string GenerateParameters()
        {
            List<string> parameters = new List<string>();

            // TODO: Fix this again :(
            if (Command != ChefCommand.NONE)
                parameters.Add(Command.LongName());
            else
                return null;

            #region Boolean flags

            // Adler-32
            if (GetSupportedCommands(ChefFlag.Adler32).Contains(Command))
            {
                if (this[ChefFlag.Adler32] != null)
                    parameters.Add($"{ChefFlag.Adler32.LongName()} {this[ChefFlag.Adler32]}");
            }

            // Clear
            if (GetSupportedCommands(ChefFlag.Clear).Contains(Command))
            {
                if (this[ChefFlag.Clear] != null)
                    parameters.Add($"{ChefFlag.Clear.LongName()} {this[ChefFlag.Clear]}");
            }

            // Clear All
            if (GetSupportedCommands(ChefFlag.ClearAll).Contains(Command))
            {
                if (this[ChefFlag.ClearAll] != null)
                    parameters.Add($"{ChefFlag.ClearAll.LongName()} {this[ChefFlag.ClearAll]}");
            }

            // CRC16
            if (GetSupportedCommands(ChefFlag.CRC16).Contains(Command))
            {
                if (this[ChefFlag.CRC16] != null)
                    parameters.Add($"{ChefFlag.CRC16.LongName()} {this[ChefFlag.CRC16]}");
            }

            // CRC32
            if (GetSupportedCommands(ChefFlag.CRC32).Contains(Command))
            {
                if (this[ChefFlag.CRC32] != null)
                    parameters.Add($"{ChefFlag.CRC32.LongName()} {this[ChefFlag.CRC32]}");
            }

            // CRC64
            if (GetSupportedCommands(ChefFlag.CRC64).Contains(Command))
            {
                if (this[ChefFlag.CRC64] != null)
                    parameters.Add($"{ChefFlag.CRC64.LongName()} {this[ChefFlag.CRC64]}");
            }

            // Debug
            if (GetSupportedCommands(ChefFlag.Debug).Contains(Command))
            {
                if (this[ChefFlag.Debug] != null)
                    parameters.Add($"{ChefFlag.Debug.LongName()} {this[ChefFlag.Debug]}");
            }

            // Disk Tags
            if (GetSupportedCommands(ChefFlag.DiskTags).Contains(Command))
            {
                if (this[ChefFlag.DiskTags] != null)
                    parameters.Add($"{ChefFlag.DiskTags.LongName()} {this[ChefFlag.DiskTags]}");
            }

            // Duplicated Sectors
            if (GetSupportedCommands(ChefFlag.DuplicatedSectors).Contains(Command))
            {
                if (this[ChefFlag.DuplicatedSectors] != null)
                    parameters.Add($"{ChefFlag.DuplicatedSectors.LongName()} {this[ChefFlag.DuplicatedSectors]}");
            }

            // Extended Attributes
            if (GetSupportedCommands(ChefFlag.ExtendedAttributes).Contains(Command))
            {
                if (this[ChefFlag.ExtendedAttributes] != null)
                    parameters.Add($"{ChefFlag.ExtendedAttributes.LongName()} {this[ChefFlag.ExtendedAttributes]}");
            }

            // Filesystems
            if (GetSupportedCommands(ChefFlag.Filesystems).Contains(Command))
            {
                if (this[ChefFlag.Filesystems] != null)
                    parameters.Add($"{ChefFlag.Filesystems.LongName()} {this[ChefFlag.Filesystems]}");
            }

            // First Pregap
            if (GetSupportedCommands(ChefFlag.FirstPregap).Contains(Command))
            {
                if (this[ChefFlag.FirstPregap] != null)
                    parameters.Add($"{ChefFlag.FirstPregap.LongName()} {this[ChefFlag.FirstPregap]}");
            }

            // Fix Offset
            if (GetSupportedCommands(ChefFlag.FixOffset).Contains(Command))
            {
                if (this[ChefFlag.FixOffset] != null)
                    parameters.Add($"{ChefFlag.FixOffset.LongName()} {this[ChefFlag.FixOffset]}");
            }

            // Fletcher-16
            if (GetSupportedCommands(ChefFlag.Fletcher16).Contains(Command))
            {
                if (this[ChefFlag.Fletcher16] != null)
                    parameters.Add($"{ChefFlag.Fletcher16.LongName()} {this[ChefFlag.Fletcher16]}");
            }

            // Fletcher-32
            if (GetSupportedCommands(ChefFlag.Fletcher32).Contains(Command))
            {
                if (this[ChefFlag.Fletcher32] != null)
                    parameters.Add($"{ChefFlag.Fletcher32.LongName()} {this[ChefFlag.Fletcher32]}");
            }

            // Force
            if (GetSupportedCommands(ChefFlag.Force).Contains(Command))
            {
                if (this[ChefFlag.Force] != null)
                    parameters.Add($"{ChefFlag.Force.LongName()} {this[ChefFlag.Force]}");
            }

            // Long Format
            if (GetSupportedCommands(ChefFlag.LongFormat).Contains(Command))
            {
                if (this[ChefFlag.LongFormat] != null)
                    parameters.Add($"{ChefFlag.LongFormat.LongName()} {this[ChefFlag.LongFormat]}");
            }

            // Long Sectors
            if (GetSupportedCommands(ChefFlag.LongSectors).Contains(Command))
            {
                if (this[ChefFlag.LongSectors] != null)
                    parameters.Add($"{ChefFlag.LongSectors.LongName()} {this[ChefFlag.LongSectors]}");
            }

            // MD5
            if (GetSupportedCommands(ChefFlag.MD5).Contains(Command))
            {
                if (this[ChefFlag.MD5] != null)
                    parameters.Add($"{ChefFlag.MD5.LongName()} {this[ChefFlag.MD5]}");
            }

            // Metadata
            if (GetSupportedCommands(ChefFlag.Metadata).Contains(Command))
            {
                if (this[ChefFlag.Metadata] != null)
                    parameters.Add($"{ChefFlag.Metadata.LongName()} {this[ChefFlag.Metadata]}");
            }

            // Partitions
            if (GetSupportedCommands(ChefFlag.Partitions).Contains(Command))
            {
                if (this[ChefFlag.Partitions] != null)
                    parameters.Add($"{ChefFlag.Partitions.LongName()} {this[ChefFlag.Partitions]}");
            }

            // Persistent
            if (GetSupportedCommands(ChefFlag.Persistent).Contains(Command))
            {
                if (this[ChefFlag.Persistent] != null)
                    parameters.Add($"{ChefFlag.Persistent.LongName()} {this[ChefFlag.Persistent]}");
            }

            // Resume
            if (GetSupportedCommands(ChefFlag.Resume).Contains(Command))
            {
                if (this[ChefFlag.Resume] != null)
                    parameters.Add($"{ChefFlag.Resume.LongName()} {this[ChefFlag.Resume]}");
            }

            // Sector Tags
            if (GetSupportedCommands(ChefFlag.SectorTags).Contains(Command))
            {
                if (this[ChefFlag.SectorTags] != null)
                    parameters.Add($"{ChefFlag.SectorTags.LongName()} {this[ChefFlag.SectorTags]}");
            }

            // Separated Tracks
            if (GetSupportedCommands(ChefFlag.SeparatedTracks).Contains(Command))
            {
                if (this[ChefFlag.SeparatedTracks] != null)
                    parameters.Add($"{ChefFlag.SeparatedTracks.LongName()} {this[ChefFlag.SeparatedTracks]}");
            }

            // SHA-1
            if (GetSupportedCommands(ChefFlag.SHA1).Contains(Command))
            {
                if (this[ChefFlag.SHA1] != null)
                    parameters.Add($"{ChefFlag.SHA1.LongName()} {this[ChefFlag.SHA1]}");
            }

            // SHA-256
            if (GetSupportedCommands(ChefFlag.SHA256).Contains(Command))
            {
                if (this[ChefFlag.SHA256] != null)
                    parameters.Add($"{ChefFlag.SHA256.LongName()} {this[ChefFlag.SHA256]}");
            }

            // SHA-384
            if (GetSupportedCommands(ChefFlag.SHA384).Contains(Command))
            {
                if (this[ChefFlag.SHA384] != null)
                    parameters.Add($"{ChefFlag.SHA384.LongName()} {this[ChefFlag.SHA384]}");
            }

            // SHA-512
            if (GetSupportedCommands(ChefFlag.SHA512).Contains(Command))
            {
                if (this[ChefFlag.SHA512] != null)
                    parameters.Add($"{ChefFlag.SHA512.LongName()} {this[ChefFlag.SHA512]}");
            }

            // SpamSum
            if (GetSupportedCommands(ChefFlag.SpamSum).Contains(Command))
            {
                if (this[ChefFlag.SpamSum] != null)
                    parameters.Add($"{ChefFlag.SpamSum.LongName()} {this[ChefFlag.SpamSum]}");
            }

            // Stop on Error
            if (GetSupportedCommands(ChefFlag.StopOnError).Contains(Command))
            {
                if (this[ChefFlag.StopOnError] != null)
                    parameters.Add($"{ChefFlag.StopOnError.LongName()} {this[ChefFlag.StopOnError]}");
            }

            // Tape
            if (GetSupportedCommands(ChefFlag.Tape).Contains(Command))
            {
                if (this[ChefFlag.Tape] != null)
                    parameters.Add($"{ChefFlag.Tape.LongName()} {this[ChefFlag.Tape]}");
            }

            // Trim
            if (GetSupportedCommands(ChefFlag.Trim).Contains(Command))
            {
                if (this[ChefFlag.Trim] != null)
                    parameters.Add($"{ChefFlag.Trim.LongName()} {this[ChefFlag.Trim]}");
            }

            // Verbose
            if (GetSupportedCommands(ChefFlag.Verbose).Contains(Command))
            {
                if (this[ChefFlag.Verbose] != null)
                    parameters.Add($"{ChefFlag.Verbose.LongName()} {this[ChefFlag.Verbose]}");
            }

            // Verify Disc
            if (GetSupportedCommands(ChefFlag.VerifyDisc).Contains(Command))
            {
                if (this[ChefFlag.VerifyDisc] != null)
                    parameters.Add($"{ChefFlag.VerifyDisc.LongName()} {this[ChefFlag.VerifyDisc]}");
            }

            // Verify Sectors
            if (GetSupportedCommands(ChefFlag.VerifySectors).Contains(Command))
            {
                if (this[ChefFlag.VerifySectors] != null)
                    parameters.Add($"{ChefFlag.VerifySectors.LongName()} {this[ChefFlag.VerifySectors]}");
            }

            // Whole Disc
            if (GetSupportedCommands(ChefFlag.WholeDisc).Contains(Command))
            {
                if (this[ChefFlag.WholeDisc] != null)
                    parameters.Add($"{ChefFlag.WholeDisc.LongName()} {this[ChefFlag.WholeDisc]}");
            }

            #endregion

            #region Int8 flags

            // Speed
            if (GetSupportedCommands(ChefFlag.Speed).Contains(Command))
            {
                if (this[ChefFlag.Speed] == true && SpeedValue != null)
                    parameters.Add($"{ChefFlag.Speed.LongName()} {SpeedValue}");
            }

            #endregion

            #region Int16 flags

            // Retry Passes
            if (GetSupportedCommands(ChefFlag.RetryPasses).Contains(Command))
            {
                if (this[ChefFlag.RetryPasses] == true && RetryPassesValue != null)
                    parameters.Add($"{ChefFlag.RetryPasses.LongName()} {RetryPassesValue}");
            }

            // Width
            if (GetSupportedCommands(ChefFlag.Width).Contains(Command))
            {
                if (this[ChefFlag.Width] == true && WidthValue != null)
                    parameters.Add($"{ChefFlag.Width.LongName()} {WidthValue}");
            }

            #endregion

            #region Int32 flags

            // Block Size
            if (GetSupportedCommands(ChefFlag.BlockSize).Contains(Command))
            {
                if (this[ChefFlag.BlockSize] == true && BlockSizeValue != null)
                    parameters.Add($"{ChefFlag.BlockSize.LongName()} {BlockSizeValue}");
            }

            // Count
            if (GetSupportedCommands(ChefFlag.Count).Contains(Command))
            {
                if (this[ChefFlag.Count] == true && CountValue != null)
                    parameters.Add($"{ChefFlag.Count.LongName()} {CountValue}");
            }

            // Media Last Sequence
            if (GetSupportedCommands(ChefFlag.MediaLastSequence).Contains(Command))
            {
                if (this[ChefFlag.MediaLastSequence] == true && MediaLastSequenceValue != null)
                    parameters.Add($"{ChefFlag.MediaLastSequence.LongName()} {MediaLastSequenceValue}");
            }

            // Media Sequence
            if (GetSupportedCommands(ChefFlag.MediaSequence).Contains(Command))
            {
                if (this[ChefFlag.MediaSequence] == true && MediaSequenceValue != null)
                    parameters.Add($"{ChefFlag.MediaSequence.LongName()} {MediaSequenceValue}");
            }

            // Skip
            if (GetSupportedCommands(ChefFlag.Skip).Contains(Command))
            {
                if (this[ChefFlag.Skip] == true && SkipValue != null)
                    parameters.Add($"{ChefFlag.Skip.LongName()} {SkipValue}");
            }

            #endregion

            #region Int64 flags

            // Length -- TODO: Support "all" case
            if (GetSupportedCommands(ChefFlag.Length).Contains(Command))
            {
                if (this[ChefFlag.Length] == true && LengthValue != null)
                    parameters.Add($"{ChefFlag.Length.LongName()} {LengthValue}");
            }

            // Start
            if (GetSupportedCommands(ChefFlag.Start).Contains(Command))
            {
                if (this[ChefFlag.Start] == true && StartValue != null)
                    parameters.Add($"{ChefFlag.Start.LongName()} {StartValue}");
            }

            #endregion

            #region String flags

            // Comments
            if (GetSupportedCommands(ChefFlag.Comments).Contains(Command))
            {
                if (this[ChefFlag.Comments] == true && CommentsValue != null)
                    parameters.Add($"{ChefFlag.Comments.LongName()} \"{CommentsValue}\"");
            }

            // Creator
            if (GetSupportedCommands(ChefFlag.Creator).Contains(Command))
            {
                if (this[ChefFlag.Creator] == true && CreatorValue != null)
                    parameters.Add($"{ChefFlag.Creator.LongName()} \"{CreatorValue}\"");
            }

            // Drive Manufacturer
            if (GetSupportedCommands(ChefFlag.DriveManufacturer).Contains(Command))
            {
                if (this[ChefFlag.DriveManufacturer] == true && DriveManufacturerValue != null)
                    parameters.Add($"{ChefFlag.DriveManufacturer.LongName()} \"{DriveManufacturerValue}\"");
            }

            // Drive Model
            if (GetSupportedCommands(ChefFlag.DriveModel).Contains(Command))
            {
                if (this[ChefFlag.DriveModel] == true && DriveModelValue != null)
                    parameters.Add($"{ChefFlag.DriveModel.LongName()} \"{DriveModelValue}\"");
            }

            // Drive Revision
            if (GetSupportedCommands(ChefFlag.DriveRevision).Contains(Command))
            {
                if (this[ChefFlag.DriveRevision] == true && DriveRevisionValue != null)
                    parameters.Add($"{ChefFlag.DriveRevision.LongName()} \"{DriveRevisionValue}\"");
            }

            // Drive Serial
            if (GetSupportedCommands(ChefFlag.DriveSerial).Contains(Command))
            {
                if (this[ChefFlag.DriveSerial] == true && DriveSerialValue != null)
                    parameters.Add($"{ChefFlag.DriveSerial.LongName()} \"{DriveSerialValue}\"");
            }

            // Encoding
            if (GetSupportedCommands(ChefFlag.Encoding).Contains(Command))
            {
                if (this[ChefFlag.Encoding] == true && EncodingValue != null)
                    parameters.Add($"{ChefFlag.Encoding.LongName()} \"{EncodingValue}\"");
            }

            // Format (Convert)
            if (GetSupportedCommands(ChefFlag.FormatConvert).Contains(Command))
            {
                if (this[ChefFlag.FormatConvert] == true && FormatConvertValue != null)
                    parameters.Add($"{ChefFlag.FormatConvert.LongName()} \"{FormatConvertValue}\"");
            }

            // Format (Dump)
            if (GetSupportedCommands(ChefFlag.FormatDump).Contains(Command))
            {
                if (this[ChefFlag.FormatDump] == true && FormatDumpValue != null)
                    parameters.Add($"{ChefFlag.FormatDump.LongName()} \"{FormatDumpValue}\"");
            }

            // ImgBurn Log
            if (GetSupportedCommands(ChefFlag.ImgBurnLog).Contains(Command))
            {
                if (this[ChefFlag.ImgBurnLog] == true && ImgBurnLogValue != null)
                    parameters.Add($"{ChefFlag.ImgBurnLog.LongName()} \"{ImgBurnLogValue}\"");
            }

            // Media Barcode
            if (GetSupportedCommands(ChefFlag.MediaBarcode).Contains(Command))
            {
                if (this[ChefFlag.MediaBarcode] == true && MediaBarcodeValue != null)
                    parameters.Add($"{ChefFlag.MediaBarcode.LongName()} \"{MediaBarcodeValue}\"");
            }

            // Media Manufacturer
            if (GetSupportedCommands(ChefFlag.MediaManufacturer).Contains(Command))
            {
                if (this[ChefFlag.MediaManufacturer] == true && MediaManufacturerValue != null)
                    parameters.Add($"{ChefFlag.MediaManufacturer.LongName()} \"{MediaManufacturerValue}\"");
            }

            // Media Model
            if (GetSupportedCommands(ChefFlag.MediaModel).Contains(Command))
            {
                if (this[ChefFlag.MediaModel] == true && MediaModelValue != null)
                    parameters.Add($"{ChefFlag.MediaModel.LongName()} \"{MediaModelValue}\"");
            }

            // Media Part Number
            if (GetSupportedCommands(ChefFlag.MediaPartNumber).Contains(Command))
            {
                if (this[ChefFlag.MediaPartNumber] == true && MediaPartNumberValue != null)
                    parameters.Add($"{ChefFlag.MediaPartNumber.LongName()} \"{MediaPartNumberValue}\"");
            }

            // Media Serial
            if (GetSupportedCommands(ChefFlag.MediaSerial).Contains(Command))
            {
                if (this[ChefFlag.MediaSerial] == true && MediaSerialValue != null)
                    parameters.Add($"{ChefFlag.MediaSerial.LongName()} \"{MediaSerialValue}\"");
            }

            // Media Title
            if (GetSupportedCommands(ChefFlag.MediaTitle).Contains(Command))
            {
                if (this[ChefFlag.MediaTitle] == true && MediaTitleValue != null)
                    parameters.Add($"{ChefFlag.MediaTitle.LongName()} \"{MediaTitleValue}\"");
            }

            // MHDD Log
            if (GetSupportedCommands(ChefFlag.MHDDLog).Contains(Command))
            {
                if (this[ChefFlag.MHDDLog] == true && MHDDLogValue != null)
                    parameters.Add($"{ChefFlag.MHDDLog.LongName()} \"{MHDDLogValue}\"");
            }

            // Namespace
            if (GetSupportedCommands(ChefFlag.Namespace).Contains(Command))
            {
                if (this[ChefFlag.Namespace] == true && NamespaceValue != null)
                    parameters.Add($"{ChefFlag.Namespace.LongName()} \"{NamespaceValue}\"");
            }

            // Options
            if (GetSupportedCommands(ChefFlag.Options).Contains(Command))
            {
                if (this[ChefFlag.Options] == true && OptionsValue != null)
                    parameters.Add($"{ChefFlag.Options.LongName()} \"{OptionsValue}\"");
            }

            // Output Prefix
            if (GetSupportedCommands(ChefFlag.OutputPrefix).Contains(Command))
            {
                if (this[ChefFlag.OutputPrefix] == true && OutputPrefixValue != null)
                    parameters.Add($"{ChefFlag.OutputPrefix.LongName()} \"{OutputPrefixValue}\"");
            }

            // Resume File
            if (GetSupportedCommands(ChefFlag.ResumeFile).Contains(Command))
            {
                if (this[ChefFlag.ResumeFile] == true && ResumeFileValue != null)
                    parameters.Add($"{ChefFlag.ResumeFile.LongName()} \"{ResumeFileValue}\"");
            }

            // Subchannel
            if (GetSupportedCommands(ChefFlag.Subchannel).Contains(Command))
            {
                if (this[ChefFlag.Subchannel] == true && SubchannelValue != null)
                    parameters.Add($"{ChefFlag.Subchannel.LongName()} \"{SubchannelValue}\"");
            }

            // XML Sidecar
            if (GetSupportedCommands(ChefFlag.XMLSidecar).Contains(Command))
            {
                if (this[ChefFlag.XMLSidecar] == true && XMLSidecarValue != null)
                    parameters.Add($"{ChefFlag.XMLSidecar.LongName()} \"{XMLSidecarValue}\"");
            }

            #endregion

            // Handle filenames based on command, if necessary
            switch (Command)
            {
                // Input value only
                case ChefCommand.DeviceInfo:
                case ChefCommand.DeviceReport:
                case ChefCommand.FilesystemList:
                case ChefCommand.ImageAnalyze:
                case ChefCommand.ImageChecksum:
                case ChefCommand.ImageCreateSidecar:
                case ChefCommand.ImageDecode:
                case ChefCommand.ImageEntropy:
                case ChefCommand.ImageInfo:
                case ChefCommand.ImagePrint:
                case ChefCommand.ImageVerify:
                case ChefCommand.MediaInfo:
                case ChefCommand.MediaScan:
                    if (string.IsNullOrWhiteSpace(InputValue))
                        return null;

                    parameters.Add($"\"{InputValue}\"");
                    break;

                // Two input values
                case ChefCommand.ImageCompare:
                    if (string.IsNullOrWhiteSpace(Input1Value) || string.IsNullOrWhiteSpace(Input2Value))
                        return null;

                    parameters.Add($"\"{Input1Value}\"");
                    parameters.Add($"\"{Input2Value}\"");
                    break;

                // Input and Output value
                case ChefCommand.FilesystemExtract:
                case ChefCommand.ImageConvert:
                case ChefCommand.MediaDump:
                    if (string.IsNullOrWhiteSpace(InputValue) || string.IsNullOrWhiteSpace(OutputValue))
                        return null;

                    parameters.Add($"\"{InputValue}\"");
                    parameters.Add($"\"{OutputValue}\"");
                    break;

                // Remote host value only
                case ChefCommand.DeviceList:
                case ChefCommand.Remote:
                    if (string.IsNullOrWhiteSpace(RemoteHostValue))
                        return null;

                    parameters.Add($"\"{RemoteHostValue}\"");
                    break;
            }

            return string.Join(" ", parameters);
        }

        /// <summary>
        /// Returns if the current Parameter object is valid
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return GenerateParameters() != null;
        }

        /// <summary>
        /// Scan a possible parameter string and populate whatever possible
        /// </summary>
        /// <param name="parameters">String possibly representing parameters</param>
        /// <returns></returns>
        private bool ValidateAndSetParameters(string parameters)
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

            // Determine what the commandline should look like given the first item
            Command = Converters.StringToChefCommand(parts[0], parts.Count > 1 ? parts[1] : null, out bool useSecond);
            if (Command == ChefCommand.NONE)
                return false;

            // Set the start according to what the full command was
            int start = useSecond ? 2 : 1;

            // Keep a count of keys to determine if we should break out to filename handling or not
            int keyCount = Keys.Count();

            // Loop through all auxilary flags, if necessary
            // TODO: Should an invalid flag mean instant failure? There are some flags that share a short form and this could cause issues
            int i = 0;
            for (i = start; i < parts.Count; i++)
            {
                sbyte? byteValue = null;
                short? shortValue = null;
                int? intValue = null;
                long? longValue = null;
                string stringValue = null;

                #region Boolean flags

                // Adler-32
                if (!ProcessBooleanParameter(parts, ChefFlagStrings.Adler32Short, ChefFlagStrings.Adler32Long, ChefFlag.Adler32, ref i))
                    return false;

                // Clear
                if (!ProcessBooleanParameter(parts, null, ChefFlagStrings.ClearLong, ChefFlag.Clear, ref i))
                    return false;

                // Clear All
                if (!ProcessBooleanParameter(parts, null, ChefFlagStrings.ClearAllLong, ChefFlag.ClearAll, ref i))
                    return false;

                // CRC16
                else if (!ProcessBooleanParameter(parts, null, ChefFlagStrings.CRC16Long, ChefFlag.CRC16, ref i))
                    return false;

                // CRC32
                else if (!ProcessBooleanParameter(parts, ChefFlagStrings.CRC32Short, ChefFlagStrings.CRC32Long, ChefFlag.CRC32, ref i))
                    return false;

                // CRC64
                else if (!ProcessBooleanParameter(parts, null, ChefFlagStrings.CRC64Long, ChefFlag.CRC64, ref i))
                    return false;

                // Debug
                else if (!ProcessBooleanParameter(parts, ChefFlagStrings.DebugShort, ChefFlagStrings.DebugLong, ChefFlag.Debug, ref i))
                    return false;

                // Disk Tags
                else if (!ProcessBooleanParameter(parts, ChefFlagStrings.DiskTagsShort, ChefFlagStrings.DiskTagsLong, ChefFlag.DiskTags, ref i))
                    return false;

                // Deduplicated Sectors
                else if (!ProcessBooleanParameter(parts, ChefFlagStrings.DuplicatedSectorsShort, ChefFlagStrings.DuplicatedSectorsLong, ChefFlag.DuplicatedSectors, ref i))
                    return false;

                // Extended Attributes
                else if (!ProcessBooleanParameter(parts, ChefFlagStrings.ExtendedAttributesShort, ChefFlagStrings.ExtendedAttributesLong, ChefFlag.ExtendedAttributes, ref i))
                    return false;

                // Filesystems
                else if (!ProcessBooleanParameter(parts, ChefFlagStrings.FilesystemsShort, ChefFlagStrings.FilesystemsLong, ChefFlag.Filesystems, ref i))
                    return false;

                // First Pregap
                else if (!ProcessBooleanParameter(parts, null, ChefFlagStrings.FirstPregapLong, ChefFlag.FirstPregap, ref i))
                    return false;

                // Fix Offset
                else if (!ProcessBooleanParameter(parts, null, ChefFlagStrings.FixOffsetLong, ChefFlag.FixOffset, ref i))
                    return false;

                // Fletcher-16
                else if (!ProcessBooleanParameter(parts, null, ChefFlagStrings.Fletcher16Long, ChefFlag.Fletcher16, ref i))
                    return false;

                // Fletcher-32
                else if (!ProcessBooleanParameter(parts, null, ChefFlagStrings.Fletcher32Long, ChefFlag.Fletcher32, ref i))
                    return false;

                // Force
                else if (!ProcessBooleanParameter(parts, ChefFlagStrings.ForceShort, ChefFlagStrings.ForceLong, ChefFlag.Force, ref i))
                    return false;

                // Long Format
                else if (!ProcessBooleanParameter(parts, ChefFlagStrings.LongFormatShort, ChefFlagStrings.LongFormatLong, ChefFlag.LongFormat, ref i))
                    return false;

                // Long Sectors
                else if (!ProcessBooleanParameter(parts, ChefFlagStrings.LongSectorsShort, ChefFlagStrings.LongSectorsLong, ChefFlag.LongSectors, ref i))
                    return false;

                // MD5
                else if (!ProcessBooleanParameter(parts, ChefFlagStrings.MD5Short, ChefFlagStrings.MD5Long, ChefFlag.MD5, ref i))
                    return false;

                // Metadata
                else if (!ProcessBooleanParameter(parts, null, ChefFlagStrings.MetadataLong, ChefFlag.Metadata, ref i))
                    return false;

                // Partitions
                else if (!ProcessBooleanParameter(parts, ChefFlagStrings.PartitionsShort, ChefFlagStrings.PartitionsLong, ChefFlag.Partitions, ref i))
                    return false;

                // Persistent
                else if (!ProcessBooleanParameter(parts, null, ChefFlagStrings.PersistentLong, ChefFlag.Persistent, ref i))
                    return false;

                // Resume
                else if (!ProcessBooleanParameter(parts, ChefFlagStrings.ResumeShort, ChefFlagStrings.ResumeLong, ChefFlag.Resume, ref i))
                    return false;

                // Sector Tags
                else if (!ProcessBooleanParameter(parts, ChefFlagStrings.SectorTagsShort, ChefFlagStrings.SectorTagsLong, ChefFlag.SectorTags, ref i))
                    return false;

                // Separated Tracks
                else if (!ProcessBooleanParameter(parts, ChefFlagStrings.SeparatedTracksShort, ChefFlagStrings.SeparatedTracksLong, ChefFlag.SeparatedTracks, ref i))
                    return false;

                // SHA-1
                else if (!ProcessBooleanParameter(parts, ChefFlagStrings.SHA1Short, ChefFlagStrings.SHA1Long, ChefFlag.SHA1, ref i))
                    return false;

                // SHA-256
                else if (!ProcessBooleanParameter(parts, null, ChefFlagStrings.SHA256Long, ChefFlag.SHA256, ref i))
                    return false;

                // SHA-384
                else if (!ProcessBooleanParameter(parts, null, ChefFlagStrings.SHA384Long, ChefFlag.SHA384, ref i))
                    return false;

                // SHA-512
                else if (!ProcessBooleanParameter(parts, null, ChefFlagStrings.SHA512Long, ChefFlag.SHA512, ref i))
                    return false;

                // SpamSum
                else if (!ProcessBooleanParameter(parts, ChefFlagStrings.SpamSumShort, ChefFlagStrings.SpamSumLong, ChefFlag.SpamSum, ref i))
                    return false;

                // Stop on Error
                else if (!ProcessBooleanParameter(parts, ChefFlagStrings.StopOnErrorShort, ChefFlagStrings.StopOnErrorLong, ChefFlag.StopOnError, ref i))
                    return false;

                // Tape
                else if (!ProcessBooleanParameter(parts, ChefFlagStrings.TapeShort, ChefFlagStrings.TapeLong, ChefFlag.Tape, ref i))
                    return false;

                // Trim
                else if (!ProcessBooleanParameter(parts, null, ChefFlagStrings.TrimLong, ChefFlag.Trim, ref i))
                    return false;

                // Verbose
                else if (!ProcessBooleanParameter(parts, ChefFlagStrings.VerboseShort, ChefFlagStrings.VerboseLong, ChefFlag.Verbose, ref i))
                    return false;

                // Verify Disc
                else if (!ProcessBooleanParameter(parts, ChefFlagStrings.VerifyDiscShort, ChefFlagStrings.VerifyDiscLong, ChefFlag.VerifyDisc, ref i))
                    return false;

                // Verify Sectors
                else if (!ProcessBooleanParameter(parts, ChefFlagStrings.VerifySectorsShort, ChefFlagStrings.VerifySectorsLong, ChefFlag.VerifySectors, ref i))
                    return false;

                // Whole Disc
                else if (!ProcessBooleanParameter(parts, ChefFlagStrings.WholeDiscShort, ChefFlagStrings.WholeDiscLong, ChefFlag.VerifySectors, ref i))
                    return false;

                #endregion

                #region Int8 flags

                // Speed
                byteValue = ProcessInt8Parameter(parts, null, ChefFlagStrings.SpeedLong, ChefFlag.Speed, ref i);
                if (byteValue == null)
                    return false;
                else if (byteValue != SByte.MinValue)
                    SpeedValue = byteValue;

                #endregion

                #region Int16 flags

                // Retry Passes
                shortValue = ProcessInt16Parameter(parts, ChefFlagStrings.RetryPassesShort, ChefFlagStrings.RetryPassesLong, ChefFlag.RetryPasses, ref i);
                if (shortValue == null)
                    return false;
                else if (shortValue != Int16.MinValue)
                    RetryPassesValue = shortValue;

                // Width
                shortValue = ProcessInt16Parameter(parts, ChefFlagStrings.WidthShort, ChefFlagStrings.WidthLong, ChefFlag.Width, ref i);
                if (shortValue == null)
                    return false;
                else if (shortValue != Int16.MinValue)
                    WidthValue = shortValue;

                #endregion

                #region Int32 flags

                // Block Size
                intValue = ProcessInt32Parameter(parts, ChefFlagStrings.BlockSizeShort, ChefFlagStrings.BlockSizeLong, ChefFlag.BlockSize, ref i);
                if (intValue == null)
                    return false;
                else if (intValue != Int32.MinValue)
                    BlockSizeValue = intValue;

                // Count
                intValue = ProcessInt32Parameter(parts, ChefFlagStrings.CountShort, ChefFlagStrings.CountLong, ChefFlag.Count, ref i);
                if (intValue == null)
                    return false;
                else if (intValue != Int32.MinValue)
                    CountValue = intValue;

                // Media Last Sequence
                intValue = ProcessInt32Parameter(parts, null, ChefFlagStrings.MediaLastSequenceLong, ChefFlag.MediaLastSequence, ref i);
                if (intValue == null)
                    return false;
                else if (intValue != Int32.MinValue)
                    MediaLastSequenceValue = intValue;

                // Media Sequence
                intValue = ProcessInt32Parameter(parts, null, ChefFlagStrings.MediaSequenceLong, ChefFlag.MediaSequence, ref i);
                if (intValue == null)
                    return false;
                else if (intValue != Int32.MinValue)
                    MediaSequenceValue = intValue;

                // Skip
                intValue = ProcessInt32Parameter(parts, ChefFlagStrings.SkipShort, ChefFlagStrings.SkipLong, ChefFlag.Skip, ref i);
                if (intValue == null)
                    return false;
                else if (intValue != Int32.MinValue)
                    SkipValue = intValue;

                #endregion

                #region Int64 flags

                // Length -- Can also be "all"
                longValue = ProcessInt64Parameter(parts, ChefFlagStrings.LengthShort, ChefFlagStrings.LengthLong, ChefFlag.Length, ref i);
                if (longValue == null)
                    return false;
                else if (longValue != Int64.MinValue)
                    LengthValue = longValue;

                // Start
                longValue = ProcessInt64Parameter(parts, ChefFlagStrings.StartShort, ChefFlagStrings.StartLong, ChefFlag.Start, ref i);
                if (longValue == null)
                    return false;
                else if (longValue != Int64.MinValue)
                    StartValue = longValue;

                #endregion

                #region String flags

                // Comments
                stringValue = ProcessStringParameter(parts, null, ChefFlagStrings.CommentsLong, ChefFlag.Comments, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    CommentsValue = stringValue;

                // Creator
                stringValue = ProcessStringParameter(parts, null, ChefFlagStrings.CreatorLong, ChefFlag.Creator, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    CreatorValue = stringValue;

                // Drive Manufacturer
                stringValue = ProcessStringParameter(parts, null, ChefFlagStrings.DriveManufacturerLong, ChefFlag.DriveManufacturer, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    DriveManufacturerValue = stringValue;

                // Drive Model
                stringValue = ProcessStringParameter(parts, null, ChefFlagStrings.DriveModelLong, ChefFlag.DriveModel, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    DriveModelValue = stringValue;

                // Drive Revision
                stringValue = ProcessStringParameter(parts, null, ChefFlagStrings.DriveRevisionLong, ChefFlag.DriveRevision, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    DriveRevisionValue = stringValue;

                // Drive Serial
                stringValue = ProcessStringParameter(parts, null, ChefFlagStrings.DriveSerialLong, ChefFlag.DriveSerial, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    DriveSerialValue = stringValue;

                // Encoding -- TODO: List of encodings?
                stringValue = ProcessStringParameter(parts, ChefFlagStrings.EncodingShort, ChefFlagStrings.EncodingLong, ChefFlag.Encoding, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    EncodingValue = stringValue;

                // Format (Convert) -- TODO: List of formats?
                stringValue = ProcessStringParameter(parts, ChefFlagStrings.FormatConvertShort, ChefFlagStrings.FormatConvertLong, ChefFlag.FormatConvert, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    FormatConvertValue = stringValue;

                // Format (Dump) -- TODO: List of formats?
                stringValue = ProcessStringParameter(parts, ChefFlagStrings.FormatDumpShort, ChefFlagStrings.FormatDumpLong, ChefFlag.FormatDump, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    FormatDumpValue = stringValue;

                // ImgBurn Log
                stringValue = ProcessStringParameter(parts, ChefFlagStrings.ImgBurnLogShort, ChefFlagStrings.ImgBurnLogLong, ChefFlag.ImgBurnLog, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    ImgBurnLogValue = stringValue;

                // Media Barcode
                stringValue = ProcessStringParameter(parts, null, ChefFlagStrings.MediaBarcodeLong, ChefFlag.MediaBarcode, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    MediaBarcodeValue = stringValue;

                // Media Manufacturer
                stringValue = ProcessStringParameter(parts, null, ChefFlagStrings.MediaManufacturerLong, ChefFlag.MediaManufacturer, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    MediaManufacturerValue = stringValue;

                // Media Model
                stringValue = ProcessStringParameter(parts, null, ChefFlagStrings.MediaModelLong, ChefFlag.MediaModel, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    MediaModelValue = stringValue;

                // Media Part Number
                stringValue = ProcessStringParameter(parts, null, ChefFlagStrings.MediaPartNumberLong, ChefFlag.MediaPartNumber, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    MediaPartNumberValue = stringValue;

                // Media Serial
                stringValue = ProcessStringParameter(parts, null, ChefFlagStrings.MediaSerialLong, ChefFlag.MediaSerial, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    MediaSerialValue = stringValue;

                // Media Title
                stringValue = ProcessStringParameter(parts, null, ChefFlagStrings.MediaTitleLong, ChefFlag.MediaTitle, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    MediaTitleValue = stringValue;

                // MHDD Log
                stringValue = ProcessStringParameter(parts, ChefFlagStrings.MHDDLogShort, ChefFlagStrings.MHDDLogLong, ChefFlag.MHDDLog, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    MHDDLogValue = stringValue;

                // Namespace
                stringValue = ProcessStringParameter(parts, ChefFlagStrings.NamespaceShort, ChefFlagStrings.NamespaceLong, ChefFlag.Namespace, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    NamespaceValue = stringValue;

                // Options -- TODO: Validate options?
                stringValue = ProcessStringParameter(parts, ChefFlagStrings.OptionsShort, ChefFlagStrings.OptionsLong, ChefFlag.Options, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    OptionsValue = stringValue;

                // Output Prefix
                stringValue = ProcessStringParameter(parts, ChefFlagStrings.OutputPrefixShort, ChefFlagStrings.OutputPrefixLong, ChefFlag.OutputPrefix, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    OutputPrefixValue = stringValue;

                // Resume File
                stringValue = ProcessStringParameter(parts, ChefFlagStrings.ResumeFileShort, ChefFlagStrings.ResumeFileLong, ChefFlag.ResumeFile, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    ResumeFileValue = stringValue;

                // Subchannel
                stringValue = ProcessStringParameter(parts, null, ChefFlagStrings.SubchannelLong, ChefFlag.Subchannel, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    SubchannelValue = stringValue;

                // XML Sidecar
                stringValue = ProcessStringParameter(parts, ChefFlagStrings.XMLSidecarShort, ChefFlagStrings.XMLSidecarLong, ChefFlag.XMLSidecar, ref i);
                if (stringValue == null)
                    return false;
                else if (stringValue != string.Empty)
                    XMLSidecarValue = stringValue;

                #endregion

                // If we didn't add any new flags, break out since we might be at filename handling
                if (keyCount == Keys.Count())
                    break;
            }

            // Handle filenames based on command, if necessary
            switch (Command)
            {
                // Input value only
                case ChefCommand.DeviceInfo:
                case ChefCommand.DeviceReport:
                case ChefCommand.FilesystemList:
                case ChefCommand.ImageAnalyze:
                case ChefCommand.ImageChecksum:
                case ChefCommand.ImageCreateSidecar:
                case ChefCommand.ImageDecode:
                case ChefCommand.ImageEntropy:
                case ChefCommand.ImageInfo:
                case ChefCommand.ImagePrint:
                case ChefCommand.ImageVerify:
                case ChefCommand.MediaInfo:
                case ChefCommand.MediaScan:
                    if (!DoesExist(parts, i))
                        return false;

                    InputValue = parts[i];
                    break;

                // Two input values
                case ChefCommand.ImageCompare:
                    if (!DoesExist(parts, i))
                        return false;

                    Input1Value = parts[i];
                    i++;

                    if (!DoesExist(parts, i))
                        return false;

                    Input2Value = parts[i];
                    break;

                // Input and Output value
                case ChefCommand.FilesystemExtract:
                case ChefCommand.ImageConvert:
                case ChefCommand.MediaDump:
                    if (!DoesExist(parts, i))
                        return false;

                    InputValue = parts[i];
                    i++;

                    if (!DoesExist(parts, i))
                        return false;

                    OutputValue = parts[i];
                    break;

                // Remote host value only
                case ChefCommand.DeviceList:
                case ChefCommand.Remote:
                    if (!DoesExist(parts, i))
                        return false;

                    RemoteHostValue = parts[i];
                    break;
            }

            // If we didn't reach the end for some reason, it failed
            if (i != parts.Count)
                return false;

            return true;
        }

        /// <summary>
        /// Returns whether or not the selected item exists
        /// </summary>
        /// <param name="parameters">List of parameters to check against</param>
        /// <param name="index">Current index</param>
        /// <returns>True if the next item exists, false otherwise</returns>
        private bool DoesExist(List<string> parameters, int index)
        {
            if (index >= parameters.Count)
                return false;

            return true;
        }

        /// <summary>
        /// Get the list of commands that use a given flag
        /// </summary>
        /// <param name="flag">Flag value to get commands for</param>
        /// <returns>List of ChefCommands, if possible</returns>
        private List<ChefCommand> GetSupportedCommands(ChefFlag flag)
        {
            var commands = new List<ChefCommand>();
            switch (flag)
            {
                case ChefFlag.Adler32:
                    commands.Add(ChefCommand.ImageChecksum);
                    break;
                case ChefFlag.BlockSize:
                    commands.Add(ChefCommand.ImageCreateSidecar);
                    break;
                case ChefFlag.Clear:
                    commands.Add(ChefCommand.DatabaseUpdate);
                    break;
                case ChefFlag.ClearAll:
                    commands.Add(ChefCommand.DatabaseUpdate);
                    break;
                case ChefFlag.Comments:
                    commands.Add(ChefCommand.ImageConvert);
                    break;
                case ChefFlag.Count:
                    commands.Add(ChefCommand.ImageConvert);
                    break;
                case ChefFlag.CRC16:
                    commands.Add(ChefCommand.ImageChecksum);
                    break;
                case ChefFlag.CRC32:
                    commands.Add(ChefCommand.ImageChecksum);
                    break;
                case ChefFlag.CRC64:
                    commands.Add(ChefCommand.ImageChecksum);
                    break;
                case ChefFlag.Creator:
                    commands.Add(ChefCommand.ImageConvert);
                    break;
                case ChefFlag.Debug:
                    commands.Add(ChefCommand.DatabaseStats);
                    commands.Add(ChefCommand.DatabaseUpdate);
                    commands.Add(ChefCommand.DeviceInfo);
                    commands.Add(ChefCommand.DeviceList);
                    commands.Add(ChefCommand.DeviceReport);
                    commands.Add(ChefCommand.FilesystemExtract);
                    commands.Add(ChefCommand.FilesystemList);
                    commands.Add(ChefCommand.FilesystemOptions);
                    commands.Add(ChefCommand.ImageAnalyze);
                    commands.Add(ChefCommand.ImageChecksum);
                    commands.Add(ChefCommand.ImageCompare);
                    commands.Add(ChefCommand.ImageConvert);
                    commands.Add(ChefCommand.ImageCreateSidecar);
                    commands.Add(ChefCommand.ImageDecode);
                    commands.Add(ChefCommand.ImageEntropy);
                    commands.Add(ChefCommand.ImageInfo);
                    commands.Add(ChefCommand.ImageOptions);
                    commands.Add(ChefCommand.ImagePrint);
                    commands.Add(ChefCommand.ImageVerify);
                    commands.Add(ChefCommand.MediaDump);
                    commands.Add(ChefCommand.MediaInfo);
                    commands.Add(ChefCommand.MediaScan);
                    commands.Add(ChefCommand.Configure);
                    commands.Add(ChefCommand.Formats);
                    commands.Add(ChefCommand.ListEncodings);
                    commands.Add(ChefCommand.ListNamespaces);
                    commands.Add(ChefCommand.Remote);
                    break;
                case ChefFlag.DiskTags:
                    commands.Add(ChefCommand.ImageDecode);
                    break;
                case ChefFlag.DriveManufacturer:
                    commands.Add(ChefCommand.ImageConvert);
                    break;
                case ChefFlag.DriveModel:
                    commands.Add(ChefCommand.ImageConvert);
                    break;
                case ChefFlag.DriveRevision:
                    commands.Add(ChefCommand.ImageConvert);
                    break;
                case ChefFlag.DriveSerial:
                    commands.Add(ChefCommand.ImageConvert);
                    break;
                case ChefFlag.DuplicatedSectors:
                    commands.Add(ChefCommand.ImageEntropy);
                    break;
                case ChefFlag.Encoding:
                    commands.Add(ChefCommand.FilesystemExtract);
                    commands.Add(ChefCommand.FilesystemList);
                    commands.Add(ChefCommand.ImageAnalyze);
                    commands.Add(ChefCommand.ImageCreateSidecar);
                    commands.Add(ChefCommand.MediaDump);
                    break;
                case ChefFlag.ExtendedAttributes:
                    commands.Add(ChefCommand.FilesystemExtract);
                    break;
                case ChefFlag.Filesystems:
                    commands.Add(ChefCommand.ImageAnalyze);
                    break;
                case ChefFlag.FirstPregap:
                    commands.Add(ChefCommand.MediaDump);
                    break;
                case ChefFlag.FixOffset:
                    commands.Add(ChefCommand.MediaDump);
                    break;
                case ChefFlag.Fletcher16:
                    commands.Add(ChefCommand.ImageChecksum);
                    break;
                case ChefFlag.Fletcher32:
                    commands.Add(ChefCommand.ImageChecksum);
                    break;
                case ChefFlag.Force:
                    commands.Add(ChefCommand.ImageConvert);
                    commands.Add(ChefCommand.MediaDump);
                    break;
                case ChefFlag.FormatConvert:
                    commands.Add(ChefCommand.ImageConvert);
                    break;
                case ChefFlag.FormatDump:
                    commands.Add(ChefCommand.MediaDump);
                    break;
                case ChefFlag.ImgBurnLog:
                    commands.Add(ChefCommand.MediaScan);
                    break;
                case ChefFlag.Length:
                    commands.Add(ChefCommand.ImageDecode);
                    commands.Add(ChefCommand.ImagePrint);
                    break;
                case ChefFlag.LongFormat:
                    commands.Add(ChefCommand.FilesystemList);
                    break;
                case ChefFlag.LongSectors:
                    commands.Add(ChefCommand.ImagePrint);
                    break;
                case ChefFlag.MD5:
                    commands.Add(ChefCommand.ImageChecksum);
                    break;
                case ChefFlag.MediaBarcode:
                    commands.Add(ChefCommand.ImageConvert);
                    break;
                case ChefFlag.MediaLastSequence:
                    commands.Add(ChefCommand.ImageConvert);
                    break;
                case ChefFlag.MediaManufacturer:
                    commands.Add(ChefCommand.ImageConvert);
                    break;
                case ChefFlag.MediaModel:
                    commands.Add(ChefCommand.ImageConvert);
                    break;
                case ChefFlag.MediaPartNumber:
                    commands.Add(ChefCommand.ImageConvert);
                    break;
                case ChefFlag.MediaSequence:
                    commands.Add(ChefCommand.ImageConvert);
                    break;
                case ChefFlag.MediaSerial:
                    commands.Add(ChefCommand.ImageConvert);
                    break;
                case ChefFlag.MediaTitle:
                    commands.Add(ChefCommand.ImageConvert);
                    break;
                case ChefFlag.Metadata:
                    commands.Add(ChefCommand.MediaDump);
                    break;
                case ChefFlag.MHDDLog:
                    commands.Add(ChefCommand.MediaScan);
                    break;
                case ChefFlag.Namespace:
                    commands.Add(ChefCommand.FilesystemExtract);
                    commands.Add(ChefCommand.FilesystemList);
                    break;
                case ChefFlag.Options:
                    commands.Add(ChefCommand.FilesystemExtract);
                    commands.Add(ChefCommand.FilesystemList);
                    commands.Add(ChefCommand.ImageConvert);
                    commands.Add(ChefCommand.MediaDump);
                    break;
                case ChefFlag.OutputPrefix:
                    commands.Add(ChefCommand.DeviceInfo);
                    commands.Add(ChefCommand.MediaInfo);
                    break;
                case ChefFlag.Partitions:
                    commands.Add(ChefCommand.ImageAnalyze);
                    break;
                case ChefFlag.Persistent:
                    commands.Add(ChefCommand.MediaDump);
                    break;
                case ChefFlag.Resume:
                    commands.Add(ChefCommand.MediaDump);
                    break;
                case ChefFlag.ResumeFile:
                    commands.Add(ChefCommand.ImageConvert);
                    break;
                case ChefFlag.RetryPasses:
                    commands.Add(ChefCommand.MediaDump);
                    break;
                case ChefFlag.SectorTags:
                    commands.Add(ChefCommand.ImageDecode);
                    break;
                case ChefFlag.SeparatedTracks:
                    commands.Add(ChefCommand.ImageChecksum);
                    commands.Add(ChefCommand.ImageEntropy);
                    break;
                case ChefFlag.SHA1:
                    commands.Add(ChefCommand.ImageChecksum);
                    break;
                case ChefFlag.SHA256:
                    commands.Add(ChefCommand.ImageChecksum);
                    break;
                case ChefFlag.SHA384:
                    commands.Add(ChefCommand.ImageChecksum);
                    break;
                case ChefFlag.SHA512:
                    commands.Add(ChefCommand.ImageChecksum);
                    break;
                case ChefFlag.Skip:
                    commands.Add(ChefCommand.MediaDump);
                    break;
                case ChefFlag.SpamSum:
                    commands.Add(ChefCommand.ImageChecksum);
                    break;
                case ChefFlag.Speed:
                    commands.Add(ChefCommand.MediaDump);
                    break;
                case ChefFlag.Start:
                    commands.Add(ChefCommand.ImageDecode);
                    commands.Add(ChefCommand.ImagePrint);
                    break;
                case ChefFlag.StopOnError:
                    commands.Add(ChefCommand.MediaDump);
                    break;
                case ChefFlag.Subchannel:
                    commands.Add(ChefCommand.MediaDump);
                    break;
                case ChefFlag.Tape:
                    commands.Add(ChefCommand.ImageCreateSidecar);
                    break;
                case ChefFlag.Trim:
                    commands.Add(ChefCommand.MediaDump);
                    break;
                case ChefFlag.Verbose:
                    commands.Add(ChefCommand.DatabaseStats);
                    commands.Add(ChefCommand.DatabaseUpdate);
                    commands.Add(ChefCommand.DeviceInfo);
                    commands.Add(ChefCommand.DeviceList);
                    commands.Add(ChefCommand.DeviceReport);
                    commands.Add(ChefCommand.FilesystemExtract);
                    commands.Add(ChefCommand.FilesystemList);
                    commands.Add(ChefCommand.FilesystemOptions);
                    commands.Add(ChefCommand.ImageAnalyze);
                    commands.Add(ChefCommand.ImageChecksum);
                    commands.Add(ChefCommand.ImageCompare);
                    commands.Add(ChefCommand.ImageConvert);
                    commands.Add(ChefCommand.ImageCreateSidecar);
                    commands.Add(ChefCommand.ImageDecode);
                    commands.Add(ChefCommand.ImageEntropy);
                    commands.Add(ChefCommand.ImageInfo);
                    commands.Add(ChefCommand.ImageOptions);
                    commands.Add(ChefCommand.ImagePrint);
                    commands.Add(ChefCommand.ImageVerify);
                    commands.Add(ChefCommand.MediaDump);
                    commands.Add(ChefCommand.MediaInfo);
                    commands.Add(ChefCommand.MediaScan);
                    commands.Add(ChefCommand.Configure);
                    commands.Add(ChefCommand.Formats);
                    commands.Add(ChefCommand.ListEncodings);
                    commands.Add(ChefCommand.ListNamespaces);
                    commands.Add(ChefCommand.Remote);
                    break;
                case ChefFlag.VerifyDisc:
                    commands.Add(ChefCommand.ImageAnalyze);
                    commands.Add(ChefCommand.ImageVerify);
                    break;
                case ChefFlag.VerifySectors:
                    commands.Add(ChefCommand.ImageAnalyze);
                    commands.Add(ChefCommand.ImageVerify);
                    break;
                case ChefFlag.WholeDisc:
                    commands.Add(ChefCommand.ImageChecksum);
                    commands.Add(ChefCommand.ImageEntropy);
                    break;
                case ChefFlag.Width:
                    commands.Add(ChefCommand.ImagePrint);
                    break;
                case ChefFlag.XMLSidecar:
                    commands.Add(ChefCommand.ImageConvert);
                    commands.Add(ChefCommand.MediaDump);
                    break;

                case ChefFlag.NONE:
                default:
                    return commands;
            }

            return commands;
        }

        /// <summary>
        /// Returns whether a string is a valid bool
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <returns>True if it's a valid bool, false otherwise</returns>
        private bool IsValidBool(string parameter)
        {
            return bool.TryParse(parameter, out bool temp);
        }

        /// <summary>
        /// Returns whether a string is a valid byte
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <param name="lowerBound">Lower bound (>=)</param>
        /// <param name="upperBound">Upper bound (<=)</param>
        /// <returns>True if it's a valid byte, false otherwise</returns>
        private bool IsValidInt8(string parameter, sbyte lowerBound = -1, sbyte upperBound = -1)
        {
            if (!sbyte.TryParse(parameter, out sbyte temp))
                return false;
            else if (lowerBound != -1 && temp < lowerBound)
                return false;
            else if (upperBound != -1 && temp > upperBound)
                return false;

            return true;
        }

        /// <summary>
        /// Returns whether a string is a valid Int16
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <param name="lowerBound">Lower bound (>=)</param>
        /// <param name="upperBound">Upper bound (<=)</param>
        /// <returns>True if it's a valid Int16, false otherwise</returns>
        private bool IsValidInt16(string parameter, short lowerBound = -1, short upperBound = -1)
        {
            if (!short.TryParse(parameter, out short temp))
                return false;
            else if (lowerBound != -1 && temp < lowerBound)
                return false;
            else if (upperBound != -1 && temp > upperBound)
                return false;

            return true;
        }

        /// <summary>
        /// Returns whether a string is a valid Int32
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <param name="lowerBound">Lower bound (>=)</param>
        /// <param name="upperBound">Upper bound (<=)</param>
        /// <returns>True if it's a valid Int32, false otherwise</returns>
        private bool IsValidInt32(string parameter, int lowerBound = -1, int upperBound = -1)
        {
            if (!int.TryParse(parameter, out int temp))
                return false;
            else if (lowerBound != -1 && temp < lowerBound)
                return false;
            else if (upperBound != -1 && temp > upperBound)
                return false;

            return true;
        }

        /// <summary>
        /// Returns whether a string is a valid Int64
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <param name="lowerBound">Lower bound (>=)</param>
        /// <param name="upperBound">Upper bound (<=)</param>
        /// <returns>True if it's a valid Int64, false otherwise</returns>
        private bool IsValidInt64(string parameter, long lowerBound = -1, long upperBound = -1)
        {
            if (!long.TryParse(parameter, out long temp))
                return false;
            else if (lowerBound != -1 && temp < lowerBound)
                return false;
            else if (upperBound != -1 && temp > upperBound)
                return false;

            return true;
        }

        /// <summary>
        /// Process a boolean parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="flag">ChefFlag value corresponding to the flag</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <returns>True if the parameter was processed successfully or skipped, false otherwise</returns>
        private bool ProcessBooleanParameter(List<string> parts, string shortFlagString, string longFlagString, ChefFlag flag, ref int i)
        {
            if (parts == null)
                return false;

            if (parts[i] == shortFlagString || parts[i] == longFlagString)
            {
                if (!GetSupportedCommands(flag).Contains(Command))
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
        /// <param name="flag">ChefFlag value corresponding to the flag</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <returns>SByte value if success, Int16.MinValue if skipped, null on error/returns>
        private sbyte? ProcessInt8Parameter(List<string> parts, string shortFlagString, string longFlagString, ChefFlag flag, ref int i)
        {
            if (parts == null)
                return null;

            if (parts[i] == shortFlagString || parts[i] == longFlagString)
            {
                if (!GetSupportedCommands(flag).Contains(Command))
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
        /// <param name="flag">ChefFlag value corresponding to the flag</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <returns>Int16 value if success, Int16.MinValue if skipped, null on error/returns>
        private short? ProcessInt16Parameter(List<string> parts, string shortFlagString, string longFlagString, ChefFlag flag, ref int i)
        {
            if (parts == null)
                return null;

            if (parts[i] == shortFlagString || parts[i] == longFlagString)
            {
                if (!GetSupportedCommands(flag).Contains(Command))
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
        /// <param name="flag">ChefFlag value corresponding to the flag</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <returns>Int32 value if success, Int32.MinValue if skipped, null on error/returns>
        private int? ProcessInt32Parameter(List<string> parts, string shortFlagString, string longFlagString, ChefFlag flag, ref int i)
        {
            if (parts == null)
                return null;

            if (parts[i] == shortFlagString || parts[i] == longFlagString)
            {
                if (!GetSupportedCommands(flag).Contains(Command))
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
        /// <param name="flag">ChefFlag value corresponding to the flag</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <returns>Int64 value if success, Int64.MinValue if skipped, null on error/returns>
        private long? ProcessInt64Parameter(List<string> parts, string shortFlagString, string longFlagString, ChefFlag flag, ref int i)
        {
            if (parts == null)
                return null;

            if (parts[i] == shortFlagString || parts[i] == longFlagString)
            {
                if (!GetSupportedCommands(flag).Contains(Command))
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
        /// <param name="flag">ChefFlag value corresponding to the flag</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <returns>String value if possible, string.Empty on missing, null on error</returns>
        private string ProcessStringParameter(List<string> parts, string shortFlagString, string longFlagString, ChefFlag flag, ref int i)
        {
            if (parts == null)
                return null;

            if (parts[i] == shortFlagString || parts[i] == longFlagString)
            {
                if (!GetSupportedCommands(flag).Contains(Command))
                    return null;
                else if (!DoesExist(parts, i + 1))
                    return null;
                else if (string.IsNullOrWhiteSpace(parts[i + 1]))
                    return null;

                this[flag] = true;
                i++;
                return parts[i];
            }

            return string.Empty;
        }

        /// <summary>
        /// Set default parameters for a given system and media type
        /// </summary>
        /// <param name="system">KnownSystem value to check</param>
        /// <param name="type">MediaType value to check</param>
        /// <param name="paranoid">Enable paranoid mode (safer dumping)</param>
        /// <param name="retryPassesCount">User-defined reread count</param>
        private void SetDefaultParameters(KnownSystem? system, MediaType? type, bool paranoid, int retryPassesCount)
        {
            // First check to see if the combination of system and MediaType is valid
            var validTypes = Validators.GetValidMediaTypes(system);
            if (!validTypes.Contains(type))
                return;

            // Set retry count
            if (retryPassesCount > 0)
            {
                this[ChefFlag.RetryPasses] = true;
                RetryPassesValue = (short)retryPassesCount;
            }

            // Paranoia in DiscImageChef means more output
            if (paranoid)
            {
                this[ChefFlag.Debug] = true;
                this[ChefFlag.Verbose] = true;
            }
            
            // TODO: Look at dump-media formats and the like and see what options there are there to fill in defaults
            // Now sort based on disc type
            switch (type)
            {
                case MediaType.CDROM:
                    // TODO: Re-add when implemented
                    //this[ChefFlag.Options] = true;
                    //OptionsValue = $"{ChefOptionStrings.CDRWinCuesheetSeparate}=true";

                    this[ChefFlag.FirstPregap] = true;
                    this[ChefFlag.Subchannel] = true;
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
    }
}
