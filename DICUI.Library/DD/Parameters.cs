using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DICUI.Data;
using DICUI.Utilities;
using DICUI.Web;

namespace DICUI.DD
{
    /// <summary>
    /// Represents a generic set of DD parameters
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

        public long? BlockSizeValue { get; set; }

        public long? CountValue { get; set; }

        // fixed, removable, disk, partition
        public string FilterValue { get; set; }

        public string InputFileValue { get; set; }

        public string OutputFileValue { get; set; }

        public long? SeekValue { get; set; }

        public long? SkipValue { get; set; }

        #endregion

        /// <summary>
        /// Populate a Parameters object from a param string
        /// </summary>
        /// <param name="parameters">String possibly representing a set of parameters</param>
        public Parameters(string parameters)
            : base(parameters)
        {
            this.InternalProgram = InternalProgram.DD;
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
        /// Blindly generate a parameter string based on the inputs
        /// </summary>
        /// <returns>Correctly formatted parameter string, null on error</returns>
        public override string GenerateParameters()
        {
            List<string> parameters = new List<string>();

            if (BaseCommand != Command.NONE)
                parameters.Add(Converters.LongName(BaseCommand));

            #region Boolean flags

            // Progress
            if (GetSupportedCommands(Flag.Progress).Contains(BaseCommand))
            {
                if (this[Flag.Progress] == true)
                    parameters.Add($"{this[Flag.Progress]}");
            }

            // Size
            if (GetSupportedCommands(Flag.Size).Contains(BaseCommand))
            {
                if (this[Flag.Size] == true)
                    parameters.Add($"{this[Flag.Size]}");
            }

            #endregion

            #region Int64 flags

            // Block Size
            if (GetSupportedCommands(Flag.BlockSize).Contains(BaseCommand))
            {
                if (this[Flag.BlockSize] == true && BlockSizeValue != null)
                    parameters.Add($"{Converters.LongName(Flag.BlockSize)}={BlockSizeValue}");
            }

            // Count
            if (GetSupportedCommands(Flag.Count).Contains(BaseCommand))
            {
                if (this[Flag.Count] == true && CountValue != null)
                    parameters.Add($"{Converters.LongName(Flag.Count)}={CountValue}");
            }

            // Seek
            if (GetSupportedCommands(Flag.Seek).Contains(BaseCommand))
            {
                if (this[Flag.Seek] == true && SeekValue != null)
                    parameters.Add($"{Converters.LongName(Flag.Seek)}={SeekValue}");
            }

            // Skip
            if (GetSupportedCommands(Flag.Skip).Contains(BaseCommand))
            {
                if (this[Flag.Skip] == true && SkipValue != null)
                    parameters.Add($"{Converters.LongName(Flag.Skip)}={SkipValue}");
            }

            #endregion

            #region String flags

            // Filter
            if (GetSupportedCommands(Flag.Filter).Contains(BaseCommand))
            {
                if (this[Flag.Filter] == true && FilterValue != null)
                    parameters.Add($"{Converters.LongName(Flag.Filter)}={FilterValue}");
            }

            // Input File
            if (GetSupportedCommands(Flag.InputFile).Contains(BaseCommand))
            {
                if (this[Flag.InputFile] == true && InputFileValue != null)
                    parameters.Add($"{Converters.LongName(Flag.InputFile)}={InputFileValue}");
                else
                    return null;
            }

            // Output File
            if (GetSupportedCommands(Flag.OutputFile).Contains(BaseCommand))
            {
                if (this[Flag.OutputFile] == true && OutputFileValue != null)
                    parameters.Add($"{Converters.LongName(Flag.OutputFile)}={OutputFileValue}");
                else
                    return null;
            }

            #endregion

            return string.Empty;
        }

        /// <summary>
        /// Get the input path from the implementation
        /// </summary>
        /// <returns>String representing the path, null on error</returns>
        public override string InputPath() => InputFileValue;

        /// <summary>
        /// Get the output path from the implementation
        /// </summary>
        /// <returns>String representing the path, null on error</returns>
        public override string OutputPath() => OutputFileValue;

        /// <summary>
        /// Get the processing speed from the implementation
        /// </summary>
        /// <returns>int? representing the speed, null on error</returns>
        /// <remarks>DD does not support drive speeds</remarks>
        public override int? GetSpeed() => 1;

        /// <summary>
        /// Set the processing speed int the implementation
        /// </summary>
        /// <param name="speed">int? representing the speed</param>
        /// <remarks>DD does not support drive speeds</remarks>
        public override void SetSpeed(int? speed)
        {
        }

        /// <summary>
        /// Get the MediaType from the current set of parameters
        /// </summary>
        /// <returns>MediaType value if successful, null on error</returns>
        /// <remarks>DD does not know the difference between media types</remarks>
        public override MediaType? GetMediaType() => null;

        /// <summary>
        /// Gets if the current command is considered a dumping command or not
        /// </summary>
        /// <returns>True if it's a dumping command, false otherwise</returns>
        public override bool IsDumpingCommand()
        {
            switch (this.BaseCommand)
            {
                case Command.List:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Reset all special variables to have default values
        /// </summary>
        protected override void ResetValues()
        {
            BaseCommand = Command.NONE;

            _flags = new Dictionary<Flag, bool?>();

            BlockSizeValue = null;
            CountValue = null;
            InputFileValue = null;
            OutputFileValue = null;
            SeekValue = null;
            SkipValue = null;
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
            BaseCommand = Command.NONE;

            this[Flag.InputFile] = true;
            InputFileValue = $"\\\\?\\{driveLetter}:";

            this[Flag.OutputFile] = true;
            OutputFileValue = filename;

            // TODO: Add more common block sizes
            this[Flag.BlockSize] = true;
            switch (type)
            {
                case MediaType.FloppyDisk:
                    BlockSizeValue = 1440 * 1024;
                    break;

                default:
                    BlockSizeValue = 1024 * 1024 * 1024;
                    break;
            }

            this[Flag.Progress] = true;
            this[Flag.Size] = true;
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

            // Determine what the commandline should look like given the first item
            int start = 0;
            BaseCommand = Converters.StringToCommand(parts[0]);
            if (BaseCommand != Command.NONE)
                start = 1;

            // Loop through all auxilary flags, if necessary
            int i = 0;
            for (i = start; i < parts.Count; i++)
            {
                // Flag read-out values
                long? longValue = null;
                string stringValue = null;

                // Keep a count of keys to determine if we should break out to filename handling or not
                int keyCount = Keys.Count();

                #region Boolean flags

                // Progress
                ProcessBooleanParameter(parts, FlagStrings.Progress, Flag.Progress, ref i);

                // Size
                ProcessBooleanParameter(parts, FlagStrings.Size, Flag.Size, ref i);

                #endregion

                #region Int64 flags

                // Block Size
                longValue = ProcessInt64Parameter(parts, FlagStrings.BlockSize, Flag.BlockSize, ref i);
                if (longValue == Int64.MinValue)
                    return false;
                else if (longValue != null)
                    BlockSizeValue = longValue;

                // Count
                longValue = ProcessInt64Parameter(parts, FlagStrings.Count, Flag.Count, ref i);
                if (longValue == Int64.MinValue)
                    return false;
                else if (longValue != null)
                    CountValue = longValue;

                // Seek
                longValue = ProcessInt64Parameter(parts, FlagStrings.Seek, Flag.Seek, ref i);
                if (longValue == Int64.MinValue)
                    return false;
                else if (longValue != null)
                    SeekValue = longValue;

                // Skip
                longValue = ProcessInt64Parameter(parts, FlagStrings.Skip, Flag.Skip, ref i);
                if (longValue == Int64.MinValue)
                    return false;
                else if (longValue != null)
                    SkipValue = longValue;

                #endregion

                #region String flags

                // Filter (fixed, removable, disk, partition)
                stringValue = ProcessStringParameter(parts, FlagStrings.Filter, Flag.Filter, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    FilterValue = stringValue;

                // Input File
                stringValue = ProcessStringParameter(parts, FlagStrings.InputFile, Flag.InputFile, ref i);
                if (string.Equals(stringValue, string.Empty))
                    return false;
                else if (stringValue != null)
                    InputFileValue = stringValue;

                // Output File
                stringValue = ProcessStringParameter(parts, FlagStrings.OutputFile, Flag.OutputFile, ref i);
                if (string.Equals(stringValue, string.Empty))
                    return false;
                else if (stringValue != null)
                    OutputFileValue = stringValue;

                #endregion
            }

            return true;
        }

        /// <summary>
        /// Validate if all required output files exist
        /// </summary>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <param name="system">KnownSystem type representing the media</param>
        /// <param name="type">MediaType type representing the media</param>
        /// <returns></returns>
        public override bool CheckAllOutputFilesExist(string basePath, KnownSystem? system, MediaType? type, IProgress<Result> progress = null)
        {
            // TODO: Figure out what sort of output files are expected... just `.bin`?
            return true;
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
            // TODO: Fill in submission info specifics for DD
            string outputDirectory = Path.GetDirectoryName(basePath);

            switch (type)
            {
                // Determine type-specific differences
            }

            switch (system)
            {
                case KnownSystem.KonamiPython2:
                    if (GetPlayStationExecutableInfo(drive?.Letter, out Region? pythonTwoRegion, out string pythonTwoDate))
                    {
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? pythonTwoRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = pythonTwoDate;
                    }

                    info.VersionAndEditions.Version = GetPlayStation2Version(drive?.Letter) ?? "";
                    break;

                case KnownSystem.SonyPlayStation:
                    if (GetPlayStationExecutableInfo(drive?.Letter, out Region? playstationRegion, out string playstationDate))
                    {
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? playstationRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = playstationDate;
                    }

                    info.CopyProtection.AntiModchip = GetPlayStationAntiModchipDetected(drive?.Letter) ? YesNo.Yes : YesNo.No;
                    break;

                case KnownSystem.SonyPlayStation2:
                    if (GetPlayStationExecutableInfo(drive?.Letter, out Region? playstationTwoRegion, out string playstationTwoDate))
                    {
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? playstationTwoRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = playstationTwoDate;
                    }

                    info.VersionAndEditions.Version = GetPlayStation2Version(drive?.Letter) ?? "";
                    break;

                case KnownSystem.SonyPlayStation4:
                    info.VersionAndEditions.Version = GetPlayStation4Version(drive?.Letter) ?? "";
                    break;
            }
        }

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
                #region Boolean flags

                case Flag.Progress:
                    commands.Add(Command.NONE);
                    break;
                case Flag.Size:
                    commands.Add(Command.NONE);
                    break;

                #endregion

                #region Int64 flags

                case Flag.BlockSize:
                    commands.Add(Command.NONE);
                    break;
                case Flag.Count:
                    commands.Add(Command.NONE);
                    break;
                case Flag.Seek:
                    commands.Add(Command.NONE);
                    break;
                case Flag.Skip:
                    commands.Add(Command.NONE);
                    break;

                #endregion

                #region String flags

                case Flag.Filter:
                    commands.Add(Command.NONE);
                    break;
                case Flag.InputFile:
                    commands.Add(Command.NONE);
                    break;
                case Flag.OutputFile:
                    commands.Add(Command.NONE);
                    break;

                #endregion

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
        /// <param name="flagString">Flag string to check</param>
        /// <param name="flag">Flag value corresponding to the flag</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <returns>True if the parameter was processed successfully or skipped, false otherwise</returns>
        private bool ProcessBooleanParameter(List<string> parts, string flagString, Flag flag, ref int i)
        {
            if (parts == null)
                return false;

            if (parts[i] == flagString)
            {
                if (!GetSupportedCommands(flag).Contains(BaseCommand))
                    return false;

                this[flag] = true;
            }

            return true;
        }

        /// <summary>
        /// Process an Int64 parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="flagString">Flag string to check</param>
        /// <param name="flag">Flag value corresponding to the flag</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <returns>Int64 value if success, Int64.MinValue if skipped, null on error/returns>
        private long? ProcessInt64Parameter(List<string> parts, string flagString, Flag flag, ref int i)
        {
            if (parts == null)
                return null;

            if (parts[i].StartsWith(flagString))
            {
                if (!GetSupportedCommands(flag).Contains(BaseCommand))
                    return null;

                string[] commandParts = parts[i].Split('=');
                if (commandParts.Length != 2)
                    return null;

                string valuePart = commandParts[1];
                long factor = 1;

                // Characters
                if (valuePart.EndsWith("c", StringComparison.Ordinal))
                {
                    factor = 1;
                    valuePart.TrimEnd('c');
                }

                // Words
                else if (valuePart.EndsWith("w", StringComparison.Ordinal))
                {
                    factor = 2;
                    valuePart.TrimEnd('w');
                }

                // Double Words
                else if (valuePart.EndsWith("d", StringComparison.Ordinal))
                {
                    factor = 4;
                    valuePart.TrimEnd('d');
                }

                // Quad Words
                else if (valuePart.EndsWith("q", StringComparison.Ordinal))
                {
                    factor = 8;
                    valuePart.TrimEnd('q');
                }

                // Kilobytes
                else if (valuePart.EndsWith("k", StringComparison.Ordinal))
                {
                    factor = 1024;
                    valuePart.TrimEnd('k');
                }

                // Megabytes
                else if (valuePart.EndsWith("M", StringComparison.Ordinal))
                {
                    factor = 1024 * 1024;
                    valuePart.TrimEnd('M');
                }

                // Gigabytes
                else if (valuePart.EndsWith("G", StringComparison.Ordinal))
                {
                    factor = 1024 * 1024 * 1024;
                    valuePart.TrimEnd('G');
                }

                if (!IsValidInt64(valuePart))
                    return null;

                this[flag] = true;
                return long.Parse(valuePart) * factor;
            }

            return Int64.MinValue;
        }

        /// <summary>
        /// Process a string parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="flagString">Flag string to check</param>
        /// <param name="flag">Flag value corresponding to the flag</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <returns>String value if possible, string.Empty on missing, null on error</returns>
        private string ProcessStringParameter(List<string> parts, string flagString, Flag flag, ref int i)
        {
            if (parts == null)
                return null;

            if (parts[i] == flagString)
            {
                if (!GetSupportedCommands(flag).Contains(BaseCommand))
                    return null;

                string[] commandParts = parts[i].Split('=');
                if (commandParts.Length != 2)
                    return null;

                string valuePart = commandParts[1];

                this[flag] = true;
                return valuePart.Trim('"');
            }

            return string.Empty;
        }
    }
}
