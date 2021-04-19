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

        #region Convert From String

        /// <summary>
        /// Get the Command enum value for a given string
        /// </summary>
        /// <param name="command">First part of String value to convert</param>
        /// <returns>Command represented by the string, if possible</returns>
        public static Command StringToCommand(string command)
        {
            string[] split = command?.Split(' ');
            if (split == null || split.Length == 0)
                return Command.NONE;
            else if (split.Length == 1)
                return StringToCommand(split[0], null, out bool _);
            else
                return StringToCommand(split[0], split[1], out bool _);
        }

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
