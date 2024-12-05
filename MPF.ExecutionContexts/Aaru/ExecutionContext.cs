using System;
using System.Collections.Generic;
using MPF.ExecutionContexts.Data;
using SabreTools.RedumpLib.Data;

namespace MPF.ExecutionContexts.Aaru
{
    /// <summary>
    /// Represents a generic set of Aaru parameters
    /// </summary>
    public sealed class ExecutionContext : BaseExecutionContext
    {
        #region Generic Dumping Information

        /// <inheritdoc/>
        public override string? InputPath => InputValue?.Trim('"');

        /// <inheritdoc/>
        public override string? OutputPath => OutputValue?.Trim('"');

        /// <inheritdoc/>
        public override int? Speed
        {
            get { return SpeedValue; }
            set
            {
                SpeedValue = (sbyte?)value;
                (_inputs[FlagStrings.SpeedLong] as Int8Input)?.SetValue((sbyte?)value);
            }
        }

        #endregion

        #region Flag Values

        public int? BlockSizeValue { get; set; }

        public string? CommentsValue { get; set; }

        public string? CreatorValue { get; set; }

        public int? CountValue { get; set; }

        public string? DriveManufacturerValue { get; set; }

        public string? DriveModelValue { get; set; }

        public string? DriveRevisionValue { get; set; }

        public string? DriveSerialValue { get; set; }

        public string? EncodingValue { get; set; }

        public string? FormatConvertValue { get; set; }

        public string? FormatDumpValue { get; set; }

        public string? GeometryValue { get; set; }

        public string? ImgBurnLogValue { get; set; }

        public string? InputValue { get; set; }

        public string? Input1Value { get; set; }

        public string? Input2Value { get; set; }

        public long? LengthValue { get; set; }

        public int? MaxBlocksValue { get; set; }

        public string? MediaBarcodeValue { get; set; }

        public int? MediaLastSequenceValue { get; set; }

        public string? MediaManufacturerValue { get; set; }

        public string? MediaModelValue { get; set; }

        public string? MediaPartNumberValue { get; set; }

        public int? MediaSequenceValue { get; set; }

        public string? MediaSerialValue { get; set; }

        public string? MediaTitleValue { get; set; }

        public string? MHDDLogValue { get; set; }

        public string? NamespaceValue { get; set; }

        public string? OptionsValue { get; set; }

        public string? OutputValue { get; set; }

        public string? OutputPrefixValue { get; set; }

        public string? RemoteHostValue { get; set; }

        public string? ResumeFileValue { get; set; }

        public short? RetryPassesValue { get; set; }

        public int? SkipValue { get; set; }

        public sbyte? SpeedValue { get; set; }

        public long? StartValue { get; set; }

        public string? SubchannelValue { get; set; }

        public short? WidthValue { get; set; }

        public string? XMLSidecarValue { get; set; }

        /// <summary>
        /// Set of all pre-command flags
        /// </summary>
        private readonly Dictionary<string, BooleanInput> _preCommandInputs = new()
        {
            [FlagStrings.DebugLong] = new BooleanInput(FlagStrings.DebugShort, FlagStrings.DebugLong, required: false),
            [FlagStrings.HelpLong] = new BooleanInput(FlagStrings.HelpShort, FlagStrings.HelpLong, required: false),
            [FlagStrings.HelpShortAlt] = new BooleanInput(FlagStrings.HelpShortAlt, FlagStrings.HelpLong, required: false),
            [FlagStrings.VerboseLong] = new BooleanInput(FlagStrings.VerboseShort, FlagStrings.VerboseLong, required: false),
            [FlagStrings.VersionLong] = new BooleanInput(FlagStrings.VersionLong, required: false),
        };

        /// <summary>
        /// Set of all command flags
        /// </summary>
        private readonly Dictionary<string, Input> _inputs = new()
        {
            // Boolean flags
            [FlagStrings.Adler32Long] = new BooleanInput(FlagStrings.Adler32Short, FlagStrings.Adler32Long, required: false),
            [FlagStrings.ClearLong] = new BooleanInput(FlagStrings.ClearLong, required: false),
            [FlagStrings.ClearAllLong] = new BooleanInput(FlagStrings.ClearAllLong, required: false),
            [FlagStrings.CRC16Long] = new BooleanInput(FlagStrings.CRC16Long, required: false),
            [FlagStrings.CRC32Long] = new BooleanInput(FlagStrings.CRC32Short, FlagStrings.CRC32Long, required: false),
            [FlagStrings.CRC64Long] = new BooleanInput(FlagStrings.CRC64Long, required: false),
            [FlagStrings.DiskTagsLong] = new BooleanInput(FlagStrings.DiskTagsShort, FlagStrings.DiskTagsLong, required: false),
            [FlagStrings.DuplicatedSectorsLong] = new BooleanInput(FlagStrings.DuplicatedSectorsShort, FlagStrings.DuplicatedSectorsLong, required: false),
            [FlagStrings.EjectLong] = new BooleanInput(FlagStrings.EjectLong, required: false),
            [FlagStrings.ExtendedAttributesLong] = new BooleanInput(FlagStrings.ExtendedAttributesShort, FlagStrings.ExtendedAttributesLong, required: false),
            [FlagStrings.FilesystemsLong] = new BooleanInput(FlagStrings.FilesystemsShort, FlagStrings.FilesystemsLong, required: false),
            [FlagStrings.FirstPregapLong] = new BooleanInput(FlagStrings.FirstPregapLong, required: false),
            [FlagStrings.FixOffsetLong] = new BooleanInput(FlagStrings.FixOffsetLong, required: false),
            [FlagStrings.FixSubchannelLong] = new BooleanInput(FlagStrings.FixSubchannelLong, required: false),
            [FlagStrings.FixSubchannelCrcLong] = new BooleanInput(FlagStrings.FixSubchannelCrcLong, required: false),
            [FlagStrings.FixSubchannelPositionLong] = new BooleanInput(FlagStrings.FixSubchannelPositionLong, required: false),
            [FlagStrings.Fletcher16Long] = new BooleanInput(FlagStrings.Fletcher16Long, required: false),
            [FlagStrings.Fletcher32Long] = new BooleanInput(FlagStrings.Fletcher32Long, required: false),
            [FlagStrings.ForceLong] = new BooleanInput(FlagStrings.ForceShort, FlagStrings.ForceLong, required: false),
            [FlagStrings.GenerateSubchannelsLong] = new BooleanInput(FlagStrings.GenerateSubchannelsLong, required: false),
            [FlagStrings.LongFormatLong] = new BooleanInput(FlagStrings.LongFormatShort, FlagStrings.LongFormatLong, required: false),
            [FlagStrings.LongSectorsLong] = new BooleanInput(FlagStrings.LongSectorsShort, FlagStrings.LongSectorsLong, required: false),
            [FlagStrings.MD5Long] = new BooleanInput(FlagStrings.MD5Short, FlagStrings.MD5Long, required: false),
            [FlagStrings.MetadataLong] = new BooleanInput(FlagStrings.MetadataLong, required: false),
            [FlagStrings.PartitionsLong] = new BooleanInput(FlagStrings.PartitionsShort, FlagStrings.PartitionsLong, required: false),
            [FlagStrings.PauseLong] = new BooleanInput(FlagStrings.PauseLong, required: false),
            [FlagStrings.PersistentLong] = new BooleanInput(FlagStrings.PersistentLong, required: false),
            [FlagStrings.PrivateLong] = new BooleanInput(FlagStrings.PrivateLong, required: false),
            [FlagStrings.ResumeLong] = new BooleanInput(FlagStrings.ResumeShort, FlagStrings.ResumeLong, required: false),
            [FlagStrings.RetrySubchannelLong] = new BooleanInput(FlagStrings.RetrySubchannelLong, required: false),
            [FlagStrings.SectorTagsLong] = new BooleanInput(FlagStrings.SectorTagsShort, FlagStrings.SectorTagsLong, required: false),
            [FlagStrings.SeparatedTracksLong] = new BooleanInput(FlagStrings.SeparatedTracksShort, FlagStrings.SeparatedTracksLong, required: false),
            [FlagStrings.SHA1Long] = new BooleanInput(FlagStrings.SHA1Short, FlagStrings.SHA1Long, required: false),
            [FlagStrings.SHA256Long] = new BooleanInput(FlagStrings.SHA256Long, required: false),
            [FlagStrings.SHA384Long] = new BooleanInput(FlagStrings.SHA384Long, required: false),
            [FlagStrings.SHA512Long] = new BooleanInput(FlagStrings.SHA512Long, required: false),
            [FlagStrings.SkipCdiReadyHoleLong] = new BooleanInput(FlagStrings.SkipCdiReadyHoleLong, required: false),
            [FlagStrings.SpamSumLong] = new BooleanInput(FlagStrings.SpamSumShort, FlagStrings.SpamSumLong, required: false),
            [FlagStrings.StopOnErrorLong] = new BooleanInput(FlagStrings.StopOnErrorShort, FlagStrings.StopOnErrorLong, required: false),
            [FlagStrings.StoreEncryptedLong] = new BooleanInput(FlagStrings.StoreEncryptedLong, required: false),
            [FlagStrings.TapeLong] = new BooleanInput(FlagStrings.TapeShort, FlagStrings.TapeLong, required: false),
            [FlagStrings.TitleKeysLong] = new BooleanInput(FlagStrings.TitleKeysLong, required: false),
            [FlagStrings.TrapDiscLong] = new BooleanInput(FlagStrings.TrapDiscShort, FlagStrings.TrapDiscLong, required: false),
            [FlagStrings.TrimLong] = new BooleanInput(FlagStrings.TrimLong, required: false),
            [FlagStrings.UseBufferedReadsLong] = new BooleanInput(FlagStrings.UseBufferedReadsLong, required: false),
            [FlagStrings.VerifyDiscLong] = new BooleanInput(FlagStrings.VerifyDiscShort, FlagStrings.VerifyDiscLong, required: false),
            [FlagStrings.VerifySectorsLong] = new BooleanInput(FlagStrings.VerifySectorsShort, FlagStrings.VerifySectorsLong, required: false),
            [FlagStrings.WholeDiscLong] = new BooleanInput(FlagStrings.WholeDiscShort, FlagStrings.WholeDiscLong, required: false),

            // Int8 flags
            [FlagStrings.SpeedLong] = new Int8Input(FlagStrings.SpeedLong),

            // Int16 flags
            [FlagStrings.RetryPassesLong] = new Int16Input(FlagStrings.RetryPassesShort, FlagStrings.RetryPassesLong),
            [FlagStrings.WidthLong] = new Int16Input(FlagStrings.WidthShort, FlagStrings.WidthLong),

            // Int32 flags
            [FlagStrings.BlockSizeLong] = new Int32Input(FlagStrings.BlockSizeShort, FlagStrings.BlockSizeLong),
            [FlagStrings.CountLong] = new Int32Input(FlagStrings.CountShort, FlagStrings.CountLong),
            [FlagStrings.MaxBlocksLong] = new Int32Input(FlagStrings.MaxBlocksLong),
            [FlagStrings.MediaLastSequenceLong] = new Int32Input(FlagStrings.MediaLastSequenceLong),
            [FlagStrings.MediaSequenceLong] = new Int32Input(FlagStrings.MediaSequenceLong),
            [FlagStrings.SkipLong] = new Int32Input(FlagStrings.SkipShort, FlagStrings.SkipLong),

            // Int64 flags
            //[FlagStrings.LengthLong] = new Int64Input(FlagStrings.LengthShort, FlagStrings.LengthLong),
            [FlagStrings.LengthLong] = new StringInput(FlagStrings.LengthShort, FlagStrings.LengthLong),
            [FlagStrings.StartLong] = new Int64Input(FlagStrings.StartShort, FlagStrings.StartLong),

            // String flags
            [FlagStrings.CommentsLong] = new StringInput(FlagStrings.CommentsLong),
            [FlagStrings.CreatorLong] = new StringInput(FlagStrings.CreatorLong),
            [FlagStrings.DriveManufacturerLong] = new StringInput(FlagStrings.DriveManufacturerLong),
            [FlagStrings.DriveModelLong] = new StringInput(FlagStrings.DriveModelLong),
            [FlagStrings.DriveRevisionLong] = new StringInput(FlagStrings.DriveRevisionLong),
            [FlagStrings.DriveSerialLong] = new StringInput(FlagStrings.DriveSerialLong),
            [FlagStrings.EncodingLong] = new StringInput(FlagStrings.EncodingShort, FlagStrings.EncodingLong),
            [FlagStrings.FormatConvertLong] = new StringInput(FlagStrings.FormatConvertShort, FlagStrings.FormatConvertLong),
            [FlagStrings.FormatDumpLong] = new StringInput(FlagStrings.FormatDumpShort, FlagStrings.FormatDumpLong),
            [FlagStrings.GeometryLong] = new StringInput(FlagStrings.GeometryShort, FlagStrings.GeometryLong),
            [FlagStrings.ImgBurnLogLong] = new StringInput(FlagStrings.ImgBurnLogShort, FlagStrings.ImgBurnLogLong),
            [FlagStrings.MediaBarcodeLong] = new StringInput(FlagStrings.MediaBarcodeLong),
            [FlagStrings.MediaManufacturerLong] = new StringInput(FlagStrings.MediaManufacturerLong),
            [FlagStrings.MediaModelLong] = new StringInput(FlagStrings.MediaModelLong),
            [FlagStrings.MediaPartNumberLong] = new StringInput(FlagStrings.MediaPartNumberLong),
            [FlagStrings.MediaSerialLong] = new StringInput(FlagStrings.MediaSerialLong),
            [FlagStrings.MediaTitleLong] = new StringInput(FlagStrings.MediaTitleLong),
            [FlagStrings.MHDDLogLong] = new StringInput(FlagStrings.MHDDLogShort, FlagStrings.MHDDLogLong),
            [FlagStrings.NamespaceLong] = new StringInput(FlagStrings.NamespaceShort, FlagStrings.NamespaceLong),
            [FlagStrings.OptionsLong] = new StringInput(FlagStrings.OptionsShort, FlagStrings.OptionsLong),
            [FlagStrings.OutputPrefixLong] = new StringInput(FlagStrings.OutputPrefixShort, FlagStrings.OutputPrefixLong),
            [FlagStrings.ResumeFileLong] = new StringInput(FlagStrings.ResumeFileShort, FlagStrings.ResumeFileLong),
            [FlagStrings.SubchannelLong] = new StringInput(FlagStrings.SubchannelLong),
            [FlagStrings.XMLSidecarLong] = new StringInput(FlagStrings.XMLSidecarShort, FlagStrings.XMLSidecarLong),
        };

        #endregion

        /// <inheritdoc/>
        public ExecutionContext(string? parameters) : base(parameters) { }

        /// <inheritdoc/>
        public ExecutionContext(RedumpSystem? system,
            MediaType? type,
            string? drivePath,
            string filename,
            int? driveSpeed,
            Dictionary<string, string?> options)
            : base(system, type, drivePath, filename, driveSpeed, options)
        {
        }

        #region BaseExecutionContext Implementations

        /// <inheritdoc/>
        public override Dictionary<string, List<string>> GetCommandSupport()
        {
            return new Dictionary<string, List<string>>()
            {
                #region Archive Family

                [CommandStrings.ArchivePrefixLong + " " + CommandStrings.ArchiveInfo] = [],

                #endregion

                #region Database Family

                [CommandStrings.DatabasePrefixLong + " " + CommandStrings.DatabaseStats] = [],

                [CommandStrings.DatabasePrefixLong + " " + CommandStrings.DatabaseUpdate] =
                [
                    FlagStrings.ClearLong,
                    FlagStrings.ClearAllLong,
                ],

                #endregion

                #region Device Family

                [CommandStrings.DevicePrefixLong + " " + CommandStrings.DeviceInfo] =
                [
                    FlagStrings.OutputPrefixLong,
                ],

                [CommandStrings.DevicePrefixLong + " " + CommandStrings.DeviceList] = [],

                [CommandStrings.DevicePrefixLong + " " + CommandStrings.DeviceReport] =
                [
                    FlagStrings.TrapDiscLong,
                ],

                #endregion

                #region Filesystem Family

                [CommandStrings.FilesystemPrefixLong + " " + CommandStrings.FilesystemExtract] =
                [
                    FlagStrings.EncodingLong,
                    FlagStrings.EncodingShort,
                    FlagStrings.ExtendedAttributesLong,
                    FlagStrings.ExtendedAttributesShort,
                    FlagStrings.NamespaceLong,
                    FlagStrings.NamespaceShort,
                    FlagStrings.OptionsLong,
                    FlagStrings.OptionsShort,
                ],

                [CommandStrings.FilesystemPrefixLong + " " + CommandStrings.FilesystemInfo] =
                [
                    FlagStrings.EncodingLong,
                    FlagStrings.EncodingShort,
                    FlagStrings.ExtendedAttributesLong,
                    FlagStrings.ExtendedAttributesShort,
                    FlagStrings.NamespaceLong,
                    FlagStrings.NamespaceShort,
                    FlagStrings.OptionsLong,
                    FlagStrings.OptionsShort,
                ],

                [CommandStrings.FilesystemPrefixLong + " " + CommandStrings.FilesystemListLong] =
                [
                    FlagStrings.EncodingLong,
                    FlagStrings.EncodingShort,
                    FlagStrings.FilesystemsLong,
                    FlagStrings.FilesystemsShort,
                    FlagStrings.LongFormatLong,
                    FlagStrings.LongFormatShort,
                    FlagStrings.PartitionsLong,
                    FlagStrings.PartitionsShort,
                ],

                [CommandStrings.FilesystemPrefixLong + " " + CommandStrings.FilesystemOptions] = [],

                #endregion

                #region Image Family

                [CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageChecksumLong] =
                [
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
                ],

                [CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageCompareLong] = [],

                [CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageConvert] =
                [
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
                ],

                [CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageCreateSidecar] =
                [
                    FlagStrings.BlockSizeLong,
                    FlagStrings.BlockSizeShort,
                    FlagStrings.EncodingLong,
                    FlagStrings.EncodingShort,
                    FlagStrings.TapeLong,
                    FlagStrings.TapeShort,
                ],

                [CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageDecode] =
                [
                    FlagStrings.DiskTagsLong,
                    FlagStrings.DiskTagsShort,
                    FlagStrings.LengthLong,
                    FlagStrings.LengthShort,
                    FlagStrings.SectorTagsLong,
                    FlagStrings.SectorTagsShort,
                    FlagStrings.StartLong,
                    FlagStrings.StartShort,
                ],

                [CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageEntropy] =
                [
                    FlagStrings.DuplicatedSectorsLong,
                    FlagStrings.DuplicatedSectorsShort,
                    FlagStrings.SeparatedTracksLong,
                    FlagStrings.SeparatedTracksShort,
                    FlagStrings.WholeDiscLong,
                    FlagStrings.WholeDiscShort,
                ],

                [CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageInfo] = [],

                [CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageOptions] = [],

                [CommandStrings.ImagePrefixLong + " " + CommandStrings.ImagePrint] =
                [
                    FlagStrings.LengthLong,
                    FlagStrings.LengthShort,
                    FlagStrings.LongSectorsLong,
                    FlagStrings.LongSectorsShort,
                    FlagStrings.StartLong,
                    FlagStrings.StartShort,
                    FlagStrings.WidthLong,
                    FlagStrings.WidthShort,
                ],

                [CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageVerify] =
                [
                    FlagStrings.VerifyDiscLong,
                    FlagStrings.VerifyDiscShort,
                    FlagStrings.VerifySectorsLong,
                    FlagStrings.VerifySectorsShort,
                ],

                #endregion

                #region Media Family

                [CommandStrings.MediaPrefixLong + " " + CommandStrings.MediaDump] =
                [
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
                ],

                [CommandStrings.MediaPrefixLong + " " + CommandStrings.MediaInfo] =
                [
                    FlagStrings.OutputPrefixLong,
                    FlagStrings.OutputPrefixShort,
                ],

                [CommandStrings.MediaPrefixLong + " " + CommandStrings.MediaScan] =
                [
                    FlagStrings.ImgBurnLogLong,
                    FlagStrings.ImgBurnLogShort,
                    FlagStrings.MHDDLogLong,
                    FlagStrings.MHDDLogShort,
                    FlagStrings.UseBufferedReadsLong,
                ],

                #endregion

                #region Standalone Commands

                [CommandStrings.NONE] =
                [
                    FlagStrings.DebugLong,
                    FlagStrings.DebugShort,
                    FlagStrings.HelpLong,
                    FlagStrings.HelpShort,
                    FlagStrings.HelpShortAlt,
                    FlagStrings.VerboseLong,
                    FlagStrings.VerboseShort,
                    FlagStrings.VersionLong,
                ],

                [CommandStrings.Configure] = [],

                [CommandStrings.Formats] = [],

                [CommandStrings.ListEncodings] = [],

                [CommandStrings.ListNamespaces] = [],

                [CommandStrings.Remote] = [],

                #endregion
            };
        }

        /// <inheritdoc/>
        public override string? GenerateParameters()
        {
            var parameters = new List<string>();

            #region Pre-command flags

            foreach (var kvp in _preCommandInputs)
            {
                // If the value doesn't exist
                string formatted = kvp.Value.Format(useEquals: false);
                if (formatted.Length == 0)
                    continue;

                // Append the parameter
                parameters.Add(formatted);
            }

            #endregion

            BaseCommand ??= CommandStrings.NONE;
            if (!string.IsNullOrEmpty(BaseCommand))
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
                // Input value only (file path)
                case CommandStrings.ArchivePrefixLong + " " + CommandStrings.ArchiveInfo:
                case CommandStrings.FilesystemPrefixLong + " " + CommandStrings.FilesystemInfo:
                case CommandStrings.FilesystemPrefixLong + " " + CommandStrings.FilesystemListLong:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageChecksumLong:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageCreateSidecar:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageDecode:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageEntropy:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageInfo:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImagePrint:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageVerify:
                    if (string.IsNullOrEmpty(InputValue))
                        return null;

                    parameters.Add($"\"{InputValue}\"");
                    break;

                // Input value only (device path)
                case CommandStrings.DevicePrefixLong + " " + CommandStrings.DeviceInfo:
                case CommandStrings.DevicePrefixLong + " " + CommandStrings.DeviceReport:
                case CommandStrings.MediaPrefixLong + " " + CommandStrings.MediaInfo:
                case CommandStrings.MediaPrefixLong + " " + CommandStrings.MediaScan:
                    if (string.IsNullOrEmpty(InputValue))
                        return null;

                    if (InputValue!.Contains(" "))
                        parameters.Add($"\"{InputValue!.TrimEnd('\\')}\"");
                    else
                        parameters.Add(InputValue!.TrimEnd('\\'));

                    break;

                // Two input values
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageCompareLong:
                    if (string.IsNullOrEmpty(Input1Value) || string.IsNullOrEmpty(Input2Value))
                        return null;

                    parameters.Add($"\"{Input1Value}\"");
                    parameters.Add($"\"{Input2Value}\"");
                    break;

                // Input and Output value (file path)
                case CommandStrings.FilesystemPrefixLong + " " + CommandStrings.FilesystemExtract:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageConvert:
                    if (string.IsNullOrEmpty(InputValue) || string.IsNullOrEmpty(OutputValue))
                        return null;

                    parameters.Add($"\"{InputValue}\"");
                    parameters.Add($"\"{OutputValue}\"");
                    break;

                // Input and Output value (device path)
                case CommandStrings.MediaPrefixLong + " " + CommandStrings.MediaDump:
                    if (string.IsNullOrEmpty(InputValue) || string.IsNullOrEmpty(OutputValue))
                        return null;

                    parameters.Add(InputValue!.TrimEnd('\\'));
                    parameters.Add($"\"{OutputValue}\"");
                    break;

                // Remote host value only
                case CommandStrings.DevicePrefixLong + " " + CommandStrings.DeviceList:
                case CommandStrings.Remote:
                    if (string.IsNullOrEmpty(RemoteHostValue))
                        return null;

                    parameters.Add($"\"{RemoteHostValue}\"");
                    break;
            }

            return string.Join(" ", [.. parameters]);
        }

        /// <inheritdoc/>
        public override string GetDefaultExtension(MediaType? mediaType)
            => Converters.Extension(mediaType);

        /// <inheritdoc/>
        public override MediaType? GetMediaType() => null;

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

            flags = [];

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
        protected override void SetDefaultParameters(string? drivePath,
            string filename,
            int? driveSpeed,
            Dictionary<string, string?> options)
        {
            BaseCommand = $"{CommandStrings.MediaPrefixLong} {CommandStrings.MediaDump}";

            InputValue = drivePath;
            OutputValue = filename;

            if (driveSpeed != null)
            {
                this[FlagStrings.SpeedLong] = true;
                SpeedValue = (sbyte?)driveSpeed;
                (_inputs[FlagStrings.SpeedLong] as Int8Input)?.SetValue((sbyte)driveSpeed);
            }

            // First check to see if the combination of system and MediaType is valid
            var validTypes = RedumpSystem.MediaTypes();
            if (!validTypes.Contains(MediaType))
                return;

            // Set retry count
            int rereadCount = GetInt32Setting(options, SettingConstants.RereadCount, SettingConstants.RereadCountDefault);
            if (rereadCount > 0)
            {
                this[FlagStrings.RetryPassesLong] = true;
                RetryPassesValue = (short)rereadCount;
                (_inputs[FlagStrings.RetryPassesLong] as Int16Input)?.SetValue((short)rereadCount);
            }

            // Set user-defined options
            if (GetBooleanSetting(options, SettingConstants.EnableDebug, SettingConstants.EnableDebugDefault))
            {
                this[FlagStrings.DebugLong] = true;
                _preCommandInputs[FlagStrings.DebugLong].SetValue(true);
            }
            if (GetBooleanSetting(options, SettingConstants.EnableVerbose, SettingConstants.EnableVerboseDefault))
            {
                this[FlagStrings.VerboseLong] = true;
                _preCommandInputs[FlagStrings.VerboseLong].SetValue(true);
            }
            if (GetBooleanSetting(options, SettingConstants.ForceDumping, SettingConstants.ForceDumpingDefault))
            {
                this[FlagStrings.ForceLong] = true;
                (_inputs[FlagStrings.ForceLong] as BooleanInput)?.SetValue(true);
            }
            if (GetBooleanSetting(options, SettingConstants.StripPersonalData, SettingConstants.StripPersonalDataDefault))
            {
                this[FlagStrings.PrivateLong] = true;
                (_inputs[FlagStrings.PrivateLong] as BooleanInput)?.SetValue(true);
            }

            // TODO: Look at dump-media formats and the like and see what options there are there to fill in defaults
            // Now sort based on disc type
            switch (MediaType)
            {
                case SabreTools.RedumpLib.Data.MediaType.CDROM:
                    // Currently no defaults set
                    break;
                case SabreTools.RedumpLib.Data.MediaType.DVD:
                    this[FlagStrings.StoreEncryptedLong] = true; // TODO: Make this configurable
                    (_inputs[FlagStrings.StoreEncryptedLong] as BooleanInput)?.SetValue(true);
                    this[FlagStrings.TitleKeysLong] = false; // TODO: Make this configurable
                    (_inputs[FlagStrings.TitleKeysLong] as BooleanInput)?.SetValue(true);
                    this[FlagStrings.TrimLong] = true; // TODO: Make this configurable
                    (_inputs[FlagStrings.TrimLong] as BooleanInput)?.SetValue(true);
                    break;
                case SabreTools.RedumpLib.Data.MediaType.GDROM:
                    // Currently no defaults set
                    break;
                case SabreTools.RedumpLib.Data.MediaType.HDDVD:
                    this[FlagStrings.StoreEncryptedLong] = true; // TODO: Make this configurable
                    (_inputs[FlagStrings.StoreEncryptedLong] as BooleanInput)?.SetValue(true);
                    this[FlagStrings.TitleKeysLong] = false; // TODO: Make this configurable
                    (_inputs[FlagStrings.TitleKeysLong] as BooleanInput)?.SetValue(true);
                    this[FlagStrings.TrimLong] = true; // TODO: Make this configurable
                    (_inputs[FlagStrings.TrimLong] as BooleanInput)?.SetValue(true);
                    break;
                case SabreTools.RedumpLib.Data.MediaType.BluRay:
                    this[FlagStrings.StoreEncryptedLong] = true; // TODO: Make this configurable
                    (_inputs[FlagStrings.StoreEncryptedLong] as BooleanInput)?.SetValue(true);
                    this[FlagStrings.TitleKeysLong] = false; // TODO: Make this configurable
                    (_inputs[FlagStrings.TitleKeysLong] as BooleanInput)?.SetValue(true);
                    this[FlagStrings.TrimLong] = true; // TODO: Make this configurable
                    (_inputs[FlagStrings.TrimLong] as BooleanInput)?.SetValue(true);
                    break;

                // Special Formats
                case SabreTools.RedumpLib.Data.MediaType.NintendoGameCubeGameDisc:
                    // Currently no defaults set
                    break;
                case SabreTools.RedumpLib.Data.MediaType.NintendoWiiOpticalDisc:
                    // Currently no defaults set
                    break;

                // Non-optical
                case SabreTools.RedumpLib.Data.MediaType.FloppyDisk:
                    // Currently no defaults set
                    break;
            }
        }

        /// <inheritdoc/>
        protected override bool ValidateAndSetParameters(string? parameters)
        {
            BaseCommand = CommandStrings.NONE;

            // The string has to be valid by itself first
            if (string.IsNullOrEmpty(parameters))
                return false;

            // Now split the string into parts for easier validation
            string[] parts = SplitParameterString(parameters!);

            // Search for pre-command flags first
            int start = 0;
            for (; start < parts.Length; start++)
            {
                // Keep a count of keys to determine if we should break out to command handling or not
                int keyCount = Keys.Count;

                foreach (var kvp in _preCommandInputs)
                {
                    // If the value was not a match
                    if (!kvp.Value.Process(parts, ref start))
                        continue;

                    // Set the flag
                    this[kvp.Key] = true;
                    break;
                }

                // If we didn't add any new flags, break out since we might be at command handling
                if (keyCount == Keys.Count)
                    break;
            }

            // If the required starting index doesn't exist, we can't do anything
            if (!DoesExist(parts, start))
                return false;

            // Determine what the commandline should look like given the first item
            BaseCommand = NormalizeCommand(parts, ref start);
            if (string.IsNullOrEmpty(BaseCommand))
                return false;

            // Set the start position
            start++;

            // Loop through all auxilary flags, if necessary
            int i = 0;
            for (i = start; i < parts.Length; i++)
            {
                // Flag read-out values
                sbyte? byteValue;
                short? shortValue;
                int? intValue;
                long? longValue;
                string? stringValue;

                // Keep a count of keys to determine if we should break out to filename handling or not
                int keyCount = Keys.Count;

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
                if (byteValue != null && byteValue != SByte.MinValue)
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
                if (longValue != null && longValue != long.MinValue)
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
                else if (longValue != long.MinValue)
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
                if (keyCount == Keys.Count)
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
            if (i != parts.Length)
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
        private static string? NormalizeCommand(string[] parts, ref int start)
        {
            // Invalid start means invalid command
            if (start < 0 || start >= parts.Length)
                return null;

            string partOne = parts[start];
            string partTwo = string.Empty;
            if (start + 1 < parts.Length)
                partTwo = parts[start + 1];

            var normalized = NormalizeCommand($"{partOne} {partTwo}".Trim());

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
        private static string? NormalizeCommand(string baseCommand)
        {
            // If the base command is inavlid, just return nulls
            if (string.IsNullOrEmpty(baseCommand))
                return null;

            // Split the command otherwise
            string[] splitCommand = baseCommand.Split(' ');
            string? family, command;

            // For commands with a family
            if (splitCommand.Length > 1)
            {
                // Handle the family first
                switch (splitCommand[0])
                {
                    case CommandStrings.ArchivePrefixShort:
                    case CommandStrings.ArchivePrefixLong:
                        family = CommandStrings.ArchivePrefixLong;
                        command = splitCommand[1] switch
                        {
                            CommandStrings.ArchiveInfo => CommandStrings.ArchiveInfo,
                            _ => null,
                        };

                        break;
                    case CommandStrings.DatabasePrefixShort:
                    case CommandStrings.DatabasePrefixLong:
                        family = CommandStrings.DatabasePrefixLong;
                        command = splitCommand[1] switch
                        {
                            CommandStrings.DatabaseStats => CommandStrings.DatabaseStats,
                            CommandStrings.DatabaseUpdate => CommandStrings.DatabaseUpdate,
                            _ => null,
                        };

                        break;

                    case CommandStrings.DevicePrefixShort:
                    case CommandStrings.DevicePrefixLong:
                        family = CommandStrings.DevicePrefixLong;
                        command = splitCommand[1] switch
                        {
                            CommandStrings.DeviceInfo => CommandStrings.DeviceInfo,
                            CommandStrings.DeviceList => CommandStrings.DeviceList,
                            CommandStrings.DeviceReport => CommandStrings.DeviceReport,
                            _ => null,
                        };

                        break;

                    case CommandStrings.FilesystemPrefixShort:
                    case CommandStrings.FilesystemPrefixShortAlt:
                    case CommandStrings.FilesystemPrefixLong:
                        family = CommandStrings.FilesystemPrefixLong;
                        command = splitCommand[1] switch
                        {
                            CommandStrings.FilesystemExtract => CommandStrings.FilesystemExtract,
                            CommandStrings.FilesystemInfo => CommandStrings.FilesystemInfo,
                            CommandStrings.FilesystemListShort => CommandStrings.FilesystemListLong,
                            CommandStrings.FilesystemListLong => CommandStrings.FilesystemListLong,
                            CommandStrings.FilesystemOptions => CommandStrings.FilesystemOptions,
                            _ => null,
                        };

                        break;

                    case CommandStrings.ImagePrefixShort:
                    case CommandStrings.ImagePrefixLong:
                        family = CommandStrings.ImagePrefixLong;
                        command = splitCommand[1] switch
                        {
                            CommandStrings.ImageChecksumShort => CommandStrings.ImageChecksumLong,
                            CommandStrings.ImageChecksumLong => CommandStrings.ImageChecksumLong,
                            CommandStrings.ImageCompareShort => CommandStrings.ImageCompareLong,
                            CommandStrings.ImageCompareLong => CommandStrings.ImageCompareLong,
                            CommandStrings.ImageConvert => CommandStrings.ImageConvert,
                            CommandStrings.ImageCreateSidecar => CommandStrings.ImageCreateSidecar,
                            CommandStrings.ImageDecode => CommandStrings.ImageDecode,
                            CommandStrings.ImageEntropy => CommandStrings.ImageEntropy,
                            CommandStrings.ImageInfo => CommandStrings.ImageInfo,
                            CommandStrings.ImageOptions => CommandStrings.ImageOptions,
                            CommandStrings.ImagePrint => CommandStrings.ImagePrint,
                            CommandStrings.ImageVerify => CommandStrings.ImageVerify,
                            _ => null,
                        };

                        break;

                    case CommandStrings.MediaPrefixShort:
                    case CommandStrings.MediaPrefixLong:
                        family = CommandStrings.MediaPrefixLong;
                        command = splitCommand[1] switch
                        {
                            CommandStrings.MediaDump => CommandStrings.MediaDump,
                            CommandStrings.MediaInfo => CommandStrings.MediaInfo,
                            CommandStrings.MediaScan => CommandStrings.MediaScan,
                            _ => null,
                        };

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
                command = splitCommand[0] switch
                {
                    CommandStrings.Configure => CommandStrings.Configure,
                    CommandStrings.Formats => CommandStrings.Formats,
                    CommandStrings.ListEncodings => CommandStrings.ListEncodings,
                    CommandStrings.ListNamespaces => CommandStrings.ListNamespaces,
                    CommandStrings.Remote => CommandStrings.Remote,
                    _ => null,
                };
            }

            // If the command itself is invalid, then return null
            if (string.IsNullOrEmpty(command))
                return null;

            // Combine the result
            if (!string.IsNullOrEmpty(family))
                return $"{family} {command}";
            else
                return command;
        }

        #endregion
    }
}
