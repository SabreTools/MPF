using MPF.Data;

namespace MPF.Aaru
{
    public static class Converters
    {
        #region Cross-enumeration conversions

        /// <summary>
        /// Get the default extension for a given disc type
        /// </summary>
        /// <param name="type">MediaType value to check</param>
        /// <returns>Valid extension (with leading '.'), null on error</returns>
        public static string Extension(MediaType? type)
        {
            // Aaru has a single, unified output format by default
            return ".aaruf";
        }

        #endregion

        #region Convert to Long Name

        /// <summary>
        /// Get the string representation of the Command enum values
        /// </summary>
        /// <param name="command">Command value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string LongName(Command command)
        {
            switch (command)
            {
                // Database Family
                case Command.DatabaseStats:
                    return $"{CommandStrings.DatabasePrefixLong} {CommandStrings.DatabaseStats}";
                case Command.DatabaseUpdate:
                    return $"{CommandStrings.DatabasePrefixLong} {CommandStrings.DatabaseUpdate}";

                // Device Family
                case Command.DeviceInfo:
                    return $"{CommandStrings.DevicePrefixLong} {CommandStrings.DeviceInfo}";
                case Command.DeviceList:
                    return $"{CommandStrings.DevicePrefixLong} {CommandStrings.DeviceList}";
                case Command.DeviceReport:
                    return $"{CommandStrings.DevicePrefixLong} {CommandStrings.DeviceReport}";

                // Filesystem Family
                case Command.FilesystemExtract:
                    return $"{CommandStrings.FilesystemPrefixLong} {CommandStrings.FilesystemExtract}";
                case Command.FilesystemList:
                    return $"{CommandStrings.FilesystemPrefixLong} {CommandStrings.FilesystemListLong}";
                case Command.FilesystemOptions:
                    return $"{CommandStrings.FilesystemPrefixLong} {CommandStrings.FilesystemOptions}";

                // Image Family
                case Command.ImageAnalyze:
                    return $"{CommandStrings.ImagePrefixLong} {CommandStrings.ImageAnalyze}";
                case Command.ImageChecksum:
                    return $"{CommandStrings.ImagePrefixLong} {CommandStrings.ImageChecksumLong}";
                case Command.ImageCompare:
                    return $"{CommandStrings.ImagePrefixLong} {CommandStrings.ImageCompareLong}";
                case Command.ImageConvert:
                    return $"{CommandStrings.ImagePrefixLong} {CommandStrings.ImageConvert}";
                case Command.ImageCreateSidecar:
                    return $"{CommandStrings.ImagePrefixLong} {CommandStrings.ImageCreateSidecar}";
                case Command.ImageDecode:
                    return $"{CommandStrings.ImagePrefixLong} {CommandStrings.ImageDecode}";
                case Command.ImageEntropy:
                    return $"{CommandStrings.ImagePrefixLong} {CommandStrings.ImageEntropy}";
                case Command.ImageInfo:
                    return $"{CommandStrings.ImagePrefixLong} {CommandStrings.ImageInfo}";
                case Command.ImageOptions:
                    return $"{CommandStrings.ImagePrefixLong} {CommandStrings.ImageOptions}";
                case Command.ImagePrint:
                    return $"{CommandStrings.ImagePrefixLong} {CommandStrings.ImagePrint}";
                case Command.ImageVerify:
                    return $"{CommandStrings.ImagePrefixLong} {CommandStrings.ImageVerify}";

                // Media Family
                case Command.MediaDump:
                    return $"{CommandStrings.MediaPrefixLong} {CommandStrings.MediaDump}";
                case Command.MediaInfo:
                    return $"{CommandStrings.MediaPrefixLong} {CommandStrings.MediaInfo}";
                case Command.MediaScan:
                    return $"{CommandStrings.MediaPrefixLong} {CommandStrings.MediaScan}";

                // Standalone Commands
                case Command.Configure:
                    return CommandStrings.Configure;
                case Command.Formats:
                    return CommandStrings.Formats;
                case Command.ListEncodings:
                    return CommandStrings.ListEncodings;
                case Command.ListNamespaces:
                    return CommandStrings.ListNamespaces;
                case Command.Remote:
                    return CommandStrings.Remote;

                case Command.NONE:
                default:
                    return "";
            }
        }

        /// <summary>
        /// Get the string representation of the Flag enum values
        /// </summary>
        /// <param name="command">Flag value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string LongName(Flag flag)
        {
            switch (flag)
            {
                // Boolean flags
                case Flag.Adler32:
                    return FlagStrings.Adler32Long;
                case Flag.Clear:
                    return FlagStrings.ClearLong;
                case Flag.ClearAll:
                    return FlagStrings.ClearAllLong;
                case Flag.CRC16:
                    return FlagStrings.CRC16Long;
                case Flag.CRC32:
                    return FlagStrings.CRC32Long;
                case Flag.CRC64:
                    return FlagStrings.CRC64Long;
                case Flag.Debug:
                    return FlagStrings.DebugLong;
                case Flag.DiskTags:
                    return FlagStrings.DiskTagsLong;
                case Flag.DuplicatedSectors:
                    return FlagStrings.DuplicatedSectorsLong;
                case Flag.Eject:
                    return FlagStrings.EjectLong;
                case Flag.ExtendedAttributes:
                    return FlagStrings.ExtendedAttributesLong;
                case Flag.Filesystems:
                    return FlagStrings.FilesystemsLong;
                case Flag.FirstPregap:
                    return FlagStrings.FirstPregapLong;
                case Flag.FixOffset:
                    return FlagStrings.FixOffsetLong;
                case Flag.FixSubchannel:
                    return FlagStrings.FixSubchannelLong;
                case Flag.FixSubchannelCrc:
                    return FlagStrings.FixSubchannelCrcLong;
                case Flag.FixSubchannelPosition:
                    return FlagStrings.FixSubchannelPositionLong;
                case Flag.Fletcher16:
                    return FlagStrings.Fletcher16Long;
                case Flag.Fletcher32:
                    return FlagStrings.Fletcher32Long;
                case Flag.Force:
                    return FlagStrings.ForceLong;
                case Flag.GenerateSubchannels:
                    return FlagStrings.GenerateSubchannelsLong;
                case Flag.Help:
                    return FlagStrings.HelpLong;
                case Flag.LongFormat:
                    return FlagStrings.LongFormatLong;
                case Flag.LongSectors:
                    return FlagStrings.LongSectorsLong;
                case Flag.MD5:
                    return FlagStrings.MD5Long;
                case Flag.Metadata:
                    return FlagStrings.MetadataLong;
                case Flag.Partitions:
                    return FlagStrings.PartitionsLong;
                case Flag.Persistent:
                    return FlagStrings.PersistentLong;
                case Flag.Private:
                    return FlagStrings.PrivateLong;
                case Flag.Resume:
                    return FlagStrings.ResumeLong;
                case Flag.RetrySubchannel:
                    return FlagStrings.RetrySubchannelLong;
                case Flag.SectorTags:
                    return FlagStrings.SectorTagsLong;
                case Flag.SeparatedTracks:
                    return FlagStrings.SeparatedTracksLong;
                case Flag.SHA1:
                    return FlagStrings.SHA1Long;
                case Flag.SHA256:
                    return FlagStrings.SHA256Long;
                case Flag.SHA384:
                    return FlagStrings.SHA384Long;
                case Flag.SHA512:
                    return FlagStrings.SHA512Long;
                case Flag.SkipCdiReadyHole:
                    return FlagStrings.SkipCdiReadyHoleLong;
                case Flag.SpamSum:
                    return FlagStrings.SpamSumLong;
                case Flag.StopOnError:
                    return FlagStrings.StopOnErrorLong;
                case Flag.Tape:
                    return FlagStrings.TapeLong;
                case Flag.Trim:
                    return FlagStrings.TrimLong;
                case Flag.Verbose:
                    return FlagStrings.VerboseLong;
                case Flag.VerifyDisc:
                    return FlagStrings.VerifyDiscLong;
                case Flag.VerifySectors:
                    return FlagStrings.VerifySectorsLong;
                case Flag.Version:
                    return FlagStrings.VersionLong;
                case Flag.WholeDisc:
                    return FlagStrings.WholeDiscLong;

                // Int8 flags
                case Flag.Speed:
                    return FlagStrings.SpeedLong;

                // Int16 flags
                case Flag.RetryPasses:
                    return FlagStrings.RetryPassesLong;
                case Flag.Width:
                    return FlagStrings.WidthLong;

                // Int32 flags
                case Flag.BlockSize:
                    return FlagStrings.BlockSizeLong;
                case Flag.Count:
                    return FlagStrings.CountLong;
                case Flag.MediaLastSequence:
                    return FlagStrings.MediaLastSequenceLong;
                case Flag.MediaSequence:
                    return FlagStrings.MediaSequenceLong;
                case Flag.Skip:
                    return FlagStrings.SkipLong;

                // Int64 flags
                case Flag.Length:
                    return FlagStrings.LengthLong;
                case Flag.Start:
                    return FlagStrings.StartLong;

                // String flags
                case Flag.Comments:
                    return FlagStrings.CommentsLong;
                case Flag.Creator:
                    return FlagStrings.CreatorLong;
                case Flag.DriveManufacturer:
                    return FlagStrings.DriveManufacturerLong;
                case Flag.DriveModel:
                    return FlagStrings.DriveModelLong;
                case Flag.DriveRevision:
                    return FlagStrings.DriveRevisionLong;
                case Flag.DriveSerial:
                    return FlagStrings.DriveSerialLong;
                case Flag.Encoding:
                    return FlagStrings.EncodingLong;
                case Flag.FormatConvert:
                    return FlagStrings.FormatConvertLong;
                case Flag.FormatDump:
                    return FlagStrings.FormatDumpLong;
                case Flag.ImgBurnLog:
                    return FlagStrings.ImgBurnLogLong;
                case Flag.MediaBarcode:
                    return FlagStrings.MediaBarcodeLong;
                case Flag.MediaManufacturer:
                    return FlagStrings.MediaManufacturerLong;
                case Flag.MediaModel:
                    return FlagStrings.MediaModelLong;
                case Flag.MediaPartNumber:
                    return FlagStrings.MediaPartNumberLong;
                case Flag.MediaSerial:
                    return FlagStrings.MediaSerialLong;
                case Flag.MediaTitle:
                    return FlagStrings.MediaTitleLong;
                case Flag.MHDDLog:
                    return FlagStrings.MHDDLogLong;
                case Flag.Namespace:
                    return FlagStrings.NamespaceLong;
                case Flag.Options:
                    return FlagStrings.OptionsLong;
                case Flag.OutputPrefix:
                    return FlagStrings.OutputPrefixLong;
                case Flag.ResumeFile:
                    return FlagStrings.ResumeFileLong;
                case Flag.Subchannel:
                    return FlagStrings.SubchannelLong;
                case Flag.XMLSidecar:
                    return FlagStrings.XMLSidecarLong;

                case Flag.NONE:
                default:
                    return "";
            }
        }

        #endregion

        #region Convert From String

        /// <summary>
        /// Get the Command enum value for a given string
        /// </summary>
        /// <param name="commandOne">First part of String value to convert</param>
        /// <param name="commandTwo">Second part of String value to convert</param>
        /// <param name="useSecond">Output bool if the second command was used</param>
        /// <returns>Command represented by the string(s), if possible</returns>
        public static Command StringToCommand(string commandOne, string commandTwo, out bool useSecond)
        {
            useSecond = false;
            switch (commandOne)
            {
                // Database Family
                case CommandStrings.DatabasePrefixShort:
                case CommandStrings.DatabasePrefixLong:
                    useSecond = true;
                    switch (commandTwo)
                    {
                        case CommandStrings.DatabaseStats:
                            return Command.DatabaseStats;
                        case CommandStrings.DatabaseUpdate:
                            return Command.DatabaseUpdate;
                    }

                    break;

                // Device Family
                case CommandStrings.DevicePrefixShort:
                case CommandStrings.DevicePrefixLong:
                    useSecond = true;
                    switch (commandTwo)
                    {
                        case CommandStrings.DeviceInfo:
                            return Command.DeviceInfo;
                        case CommandStrings.DeviceList:
                            return Command.DeviceList;
                        case CommandStrings.DeviceReport:
                            return Command.DeviceReport;
                    }

                    break;

                // Filesystem Family
                case CommandStrings.FilesystemPrefixShort:
                case CommandStrings.FilesystemPrefixShortAlt:
                case CommandStrings.FilesystemPrefixLong:
                    useSecond = true;
                    switch (commandTwo)
                    {
                        case CommandStrings.FilesystemExtract:
                            return Command.FilesystemExtract;
                        case CommandStrings.FilesystemListShort:
                        case CommandStrings.FilesystemListLong:
                            return Command.FilesystemList;
                        case CommandStrings.DatabaseStats:
                            return Command.FilesystemOptions;
                    }

                    break;

                // Image Family
                case CommandStrings.ImagePrefixShort:
                case CommandStrings.ImagePrefixLong:
                    useSecond = true;
                    switch (commandTwo)
                    {
                        case CommandStrings.ImageAnalyze:
                            return Command.ImageAnalyze;
                        case CommandStrings.ImageChecksumShort:
                        case CommandStrings.ImageChecksumLong:
                            return Command.ImageChecksum;
                        case CommandStrings.ImageCompareShort:
                        case CommandStrings.ImageCompareLong:
                            return Command.ImageCompare;
                        case CommandStrings.ImageConvert:
                            return Command.ImageConvert;
                        case CommandStrings.ImageCreateSidecar:
                            return Command.ImageCreateSidecar;
                        case CommandStrings.ImageDecode:
                            return Command.ImageDecode;
                        case CommandStrings.ImageEntropy:
                            return Command.ImageEntropy;
                        case CommandStrings.ImageInfo:
                            return Command.ImageInfo;
                        case CommandStrings.ImageOptions:
                            return Command.ImageOptions;
                        case CommandStrings.ImagePrint:
                            return Command.ImagePrint;
                        case CommandStrings.ImageVerify:
                            return Command.ImageVerify;
                    }

                    break;

                // Media Family
                case CommandStrings.MediaPrefixShort:
                case CommandStrings.MediaPrefixLong:
                    useSecond = true;
                    switch (commandTwo)
                    {
                        case CommandStrings.MediaDump:
                            return Command.MediaDump;
                        case CommandStrings.MediaInfo:
                            return Command.MediaInfo;
                        case CommandStrings.MediaScan:
                            return Command.MediaScan;
                    }

                    break;

                // Standalone Commands
                case CommandStrings.Configure:
                    return Command.Configure;
                case CommandStrings.Formats:
                    return Command.Formats;
                case CommandStrings.ListEncodings:
                    return Command.ListEncodings;
                case CommandStrings.ListNamespaces:
                    return Command.ListNamespaces;
                case CommandStrings.Remote:
                    return Command.Remote;
            }

            return Command.NONE;
        }

        #endregion
    }
}
