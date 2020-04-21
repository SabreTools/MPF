using DICUI.Data;

namespace DICUI.DD
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
            // DD has a single, unified output format by default
            return ".bin";
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
                case Command.List:
                    return CommandStrings.List;

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
                case Flag.Progress:
                    return FlagStrings.Progress;
                case Flag.Size:
                    return FlagStrings.Size;

                // Int64 flags
                case Flag.BlockSize:
                    return FlagStrings.BlockSize;
                case Flag.Count:
                    return FlagStrings.Count;
                case Flag.Seek:
                    return FlagStrings.Seek;
                case Flag.Skip:
                    return FlagStrings.Skip;

                // String flags
                case Flag.Filter:
                    return FlagStrings.Filter;
                case Flag.InputFile:
                    return FlagStrings.InputFile;
                case Flag.OutputFile:
                    return FlagStrings.OutputFile;

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
        /// <param name="command">String value to convert</param>
        /// <returns>Command represented by the string(s), if possible</returns>
        public static Command StringToCommand(string command)
        {
            switch (command)
            {
                case CommandStrings.List:
                    return Command.List;

                default:
                    return Command.NONE;
            }
        }

        #endregion
    }
}