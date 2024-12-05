using System.Collections.Generic;
using System.Text;
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
            get
            {
                return (_inputs[FlagStrings.SpeedLong] as Int8Input)?.Value;
            }
            set
            {
                (_inputs[FlagStrings.SpeedLong] as Int8Input)?.SetValue((sbyte?)value);
            }
        }

        #endregion

        #region Flag Values

        public string? InputValue { get; set; }

        public string? Input1Value { get; set; }

        public string? Input2Value { get; set; }

        public string? OutputValue { get; set; }

        public string? RemoteHostValue { get; set; }

        /// <summary>
        /// Set of all pre-command flags
        /// </summary>
        private readonly Dictionary<string, BooleanInput> _preCommandInputs = new()
        {
            [FlagStrings.DebugLong] = new BooleanInput(FlagStrings.DebugShort, FlagStrings.DebugLong, required: false),
            [FlagStrings.HelpLong] = new BooleanInput([FlagStrings.HelpLong, FlagStrings.HelpShort, FlagStrings.HelpShortAlt], required: false),
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
            [FlagStrings.CommentsLong] = new StringInput(FlagStrings.CommentsLong) { Quotes = true },
            [FlagStrings.CreatorLong] = new StringInput(FlagStrings.CreatorLong) { Quotes = true },
            [FlagStrings.DriveManufacturerLong] = new StringInput(FlagStrings.DriveManufacturerLong) { Quotes = true },
            [FlagStrings.DriveModelLong] = new StringInput(FlagStrings.DriveModelLong) { Quotes = true },
            [FlagStrings.DriveRevisionLong] = new StringInput(FlagStrings.DriveRevisionLong) { Quotes = true },
            [FlagStrings.DriveSerialLong] = new StringInput(FlagStrings.DriveSerialLong) { Quotes = true },
            [FlagStrings.EncodingLong] = new StringInput(FlagStrings.EncodingShort, FlagStrings.EncodingLong) { Quotes = true },
            [FlagStrings.FormatLong] = new StringInput([FlagStrings.FormatLong, FlagStrings.FormatConvertShort, FlagStrings.FormatDumpShort]) { Quotes = true },
            [FlagStrings.GeometryLong] = new StringInput(FlagStrings.GeometryShort, FlagStrings.GeometryLong) { Quotes = true },
            [FlagStrings.ImgBurnLogLong] = new StringInput(FlagStrings.ImgBurnLogShort, FlagStrings.ImgBurnLogLong) { Quotes = true },
            [FlagStrings.MediaBarcodeLong] = new StringInput(FlagStrings.MediaBarcodeLong) { Quotes = true },
            [FlagStrings.MediaManufacturerLong] = new StringInput(FlagStrings.MediaManufacturerLong) { Quotes = true },
            [FlagStrings.MediaModelLong] = new StringInput(FlagStrings.MediaModelLong) { Quotes = true },
            [FlagStrings.MediaPartNumberLong] = new StringInput(FlagStrings.MediaPartNumberLong) { Quotes = true },
            [FlagStrings.MediaSerialLong] = new StringInput(FlagStrings.MediaSerialLong) { Quotes = true },
            [FlagStrings.MediaTitleLong] = new StringInput(FlagStrings.MediaTitleLong) { Quotes = true },
            [FlagStrings.MHDDLogLong] = new StringInput(FlagStrings.MHDDLogShort, FlagStrings.MHDDLogLong) { Quotes = true },
            [FlagStrings.NamespaceLong] = new StringInput(FlagStrings.NamespaceShort, FlagStrings.NamespaceLong) { Quotes = true },
            [FlagStrings.OptionsLong] = new StringInput(FlagStrings.OptionsShort, FlagStrings.OptionsLong) { Quotes = true },
            [FlagStrings.OutputPrefixLong] = new StringInput(FlagStrings.OutputPrefixShort, FlagStrings.OutputPrefixLong) { Quotes = true },
            [FlagStrings.ResumeFileLong] = new StringInput(FlagStrings.ResumeFileShort, FlagStrings.ResumeFileLong) { Quotes = true },
            [FlagStrings.SubchannelLong] = new StringInput(FlagStrings.SubchannelLong) { Quotes = true },
            [FlagStrings.XMLSidecarLong] = new StringInput(FlagStrings.XMLSidecarShort, FlagStrings.XMLSidecarLong) { Quotes = true },
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
                    FlagStrings.FormatLong,
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
                    FlagStrings.FormatLong,
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
            var parameters = new StringBuilder();

            #region Pre-command flags

            // Loop though and append all existing
            foreach (var kvp in _preCommandInputs)
            {
                // If the value doesn't exist
                string formatted = kvp.Value.Format(useEquals: false);
                if (formatted.Length == 0)
                    continue;

                // Append the parameter
                parameters.Append($"{formatted} ");
            }

            #endregion

            BaseCommand ??= CommandStrings.NONE;
            if (!string.IsNullOrEmpty(BaseCommand))
                parameters.Append($"{BaseCommand} ");
            else
                return null;

            // Loop though and append all existing
            foreach (var kvp in _inputs)
            {
                // If the value is unsupported
                if (!IsFlagSupported(kvp.Key))
                    continue;

                // If the value doesn't exist
                string formatted = kvp.Value.Format(useEquals: false);
                if (formatted.Length == 0)
                    continue;

                // Append the parameter
                parameters.Append($"{formatted} ");
            }

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

                    parameters.Append($"\"{InputValue}\" ");
                    break;

                // Input value only (device path)
                case CommandStrings.DevicePrefixLong + " " + CommandStrings.DeviceInfo:
                case CommandStrings.DevicePrefixLong + " " + CommandStrings.DeviceReport:
                case CommandStrings.MediaPrefixLong + " " + CommandStrings.MediaInfo:
                case CommandStrings.MediaPrefixLong + " " + CommandStrings.MediaScan:
                    if (string.IsNullOrEmpty(InputValue))
                        return null;

                    if (InputValue!.Contains(" "))
                        parameters.Append($"\"{InputValue!.TrimEnd('\\')}\" ");
                    else
                        parameters.Append($"{InputValue!.TrimEnd('\\')} ");

                    break;

                // Two input values
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageCompareLong:
                    if (string.IsNullOrEmpty(Input1Value) || string.IsNullOrEmpty(Input2Value))
                        return null;

                    parameters.Append($"\"{Input1Value}\" ");
                    parameters.Append($"\"{Input2Value}\" ");
                    break;

                // Input and Output value (file path)
                case CommandStrings.FilesystemPrefixLong + " " + CommandStrings.FilesystemExtract:
                case CommandStrings.ImagePrefixLong + " " + CommandStrings.ImageConvert:
                    if (string.IsNullOrEmpty(InputValue) || string.IsNullOrEmpty(OutputValue))
                        return null;

                    parameters.Append($"\"{InputValue}\" ");
                    parameters.Append($"\"{OutputValue}\" ");
                    break;

                // Input and Output value (device path)
                case CommandStrings.MediaPrefixLong + " " + CommandStrings.MediaDump:
                    if (string.IsNullOrEmpty(InputValue) || string.IsNullOrEmpty(OutputValue))
                        return null;

                    parameters.Append($"{InputValue!.TrimEnd('\\')} ");
                    parameters.Append($"\"{OutputValue}\" ");
                    break;

                // Remote host value only
                case CommandStrings.DevicePrefixLong + " " + CommandStrings.DeviceList:
                case CommandStrings.Remote:
                    if (string.IsNullOrEmpty(RemoteHostValue))
                        return null;

                    parameters.Append($"\"{RemoteHostValue}\" ");
                    break;
            }

            return parameters.ToString().TrimEnd();
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

            InputValue = null;
            Input1Value = null;
            Input2Value = null;
            OutputValue = null;
            RemoteHostValue = null;

            foreach (var kvp in _preCommandInputs)
                kvp.Value.ClearValue();
            foreach (var kvp in _inputs)
                kvp.Value.ClearValue();
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

                // Match all possible flags
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
            int i = start;
            for (; i < parts.Length; i++)
            {
                // Keep a count of keys to determine if we should break out to filename handling or not
                int keyCount = Keys.Count;

                // Match all possible flags
                foreach (var kvp in _inputs)
                {
                    // If the flag is not supported
                    if (!IsFlagSupported(kvp.Key))
                        continue;

                    // If the value was not a match
                    if (!kvp.Value.Process(parts, ref i))
                        continue;

                    // Set the flag
                    this[kvp.Key] = true;
                }

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

                    case CommandStrings.Configure:
                        family = null;
                        command = CommandStrings.Configure;
                        break;
                    case CommandStrings.Formats:
                        family = null;
                        command = CommandStrings.Formats;
                        break;
                    case CommandStrings.ListEncodings:
                        family = null;
                        command = CommandStrings.ListEncodings;
                        break;
                    case CommandStrings.ListNamespaces:
                        family = null;
                        command = CommandStrings.ListNamespaces;
                        break;
                    case CommandStrings.Remote:
                        family = null;
                        command = CommandStrings.Remote;
                        break;

                    default:
                        family = null;
                        command = null;
                        break;
                }
            }

            // For standalone commands with no second input
            else
            {
                family = null;
                command = splitCommand[0] switch
                {
                    CommandStrings.Configure => CommandStrings.Configure,
                    CommandStrings.Formats => CommandStrings.Formats,
                    CommandStrings.ListEncodings => CommandStrings.ListEncodings,
                    CommandStrings.ListNamespaces => CommandStrings.ListNamespaces,
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
