using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MPF.Data;
using RedumpLib.Data;

namespace MPF.DD
{
    /// <summary>
    /// Represents a generic set of DD parameters
    /// </summary>
    public class Parameters : BaseParameters
    {
        #region Generic Dumping Information

        /// <inheritdoc/>
        public override string InputPath => InputFileValue?.TrimStart('\\', '?');

        /// <inheritdoc/>
        public override string OutputPath => OutputFileValue;

        /// <inheritdoc/>
        /// <inheritdoc/>
        public override int? Speed
        {
            get { return 1; }
            set { }
        }

        #endregion

        #region Metadata

        /// <inheritdoc/>
        public override InternalProgram InternalProgram => InternalProgram.DD;

        #endregion

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

        /// <inheritdoc/>
        public Parameters(string parameters) : base(parameters) { }

        /// <inheritdoc/>
        public Parameters(KnownSystem? system, MediaType? type, char driveLetter, string filename, int? driveSpeed, Options options)
            : base(system, type, driveLetter, filename, driveSpeed, options)
        {
        }

        #region BaseParameters Implementations

        /// <inheritdoc/>
        public override (bool, List<string>) CheckAllOutputFilesExist(string basePath, bool preCheck)
        {
            // TODO: Figure out what sort of output files are expected... just `.bin`?
            return (true, new List<string>());
        }

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, string basePath, Drive drive, bool includeArtifacts)
        {
            // TODO: Fill in submission info specifics for DD
            string outputDirectory = Path.GetDirectoryName(basePath);

            switch (this.Type)
            {
                // Determine type-specific differences
            }

            switch (this.System)
            {
                case KnownSystem.KonamiPython2:
                    if (GetPlayStationExecutableInfo(drive?.Letter, out string pythonTwoSerial, out Region? pythonTwoRegion, out string pythonTwoDate))
                    {
                        info.CommonDiscInfo.Comments += $"Internal Disc Serial: {pythonTwoSerial}\n";
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? pythonTwoRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = pythonTwoDate;
                    }

                    info.VersionAndEditions.Version = GetPlayStation2Version(drive?.Letter) ?? "";
                    break;

                case KnownSystem.SonyPlayStation:
                    if (GetPlayStationExecutableInfo(drive?.Letter, out string playstationSerial, out Region? playstationRegion, out string playstationDate))
                    {
                        info.CommonDiscInfo.Comments += $"Internal Disc Serial: {playstationSerial}\n";
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? playstationRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = playstationDate;
                    }

                    info.CopyProtection.AntiModchip = GetPlayStationAntiModchipDetected(drive?.Letter) ? YesNo.Yes : YesNo.No;
                    break;

                case KnownSystem.SonyPlayStation2:
                    if (GetPlayStationExecutableInfo(drive?.Letter, out string playstationTwoSerial, out Region? playstationTwoRegion, out string playstationTwoDate))
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
        }

        /// <inheritdoc/>
        public override string GenerateParameters()
        {
            List<string> parameters = new List<string>();

            if (BaseCommand == null)
                BaseCommand = CommandStrings.NONE;

            if (!string.IsNullOrEmpty(BaseCommand))
                parameters.Add(BaseCommand);

            #region Boolean flags

            // Progress
            if (IsFlagSupported(FlagStrings.Progress))
            {
                if (this[FlagStrings.Progress] == true)
                    parameters.Add($"{FlagStrings.Progress}");
            }

            // Size
            if (IsFlagSupported(FlagStrings.Size))
            {
                if (this[FlagStrings.Size] == true)
                    parameters.Add($"{FlagStrings.Size}");
            }

            #endregion

            #region Int64 flags

            // Block Size
            if (IsFlagSupported(FlagStrings.BlockSize))
            {
                if (this[FlagStrings.BlockSize] == true && BlockSizeValue != null)
                    parameters.Add($"{FlagStrings.BlockSize}={BlockSizeValue}");
            }

            // Count
            if (IsFlagSupported(FlagStrings.Count))
            {
                if (this[FlagStrings.Count] == true && CountValue != null)
                    parameters.Add($"{FlagStrings.Count}={CountValue}");
            }

            // Seek
            if (IsFlagSupported(FlagStrings.Seek))
            {
                if (this[FlagStrings.Seek] == true && SeekValue != null)
                    parameters.Add($"{FlagStrings.Seek}={SeekValue}");
            }

            // Skip
            if (IsFlagSupported(FlagStrings.Skip))
            {
                if (this[FlagStrings.Skip] == true && SkipValue != null)
                    parameters.Add($"{FlagStrings.Skip}={SkipValue}");
            }

            #endregion

            #region String flags

            // Filter
            if (IsFlagSupported(FlagStrings.Filter))
            {
                if (this[FlagStrings.Filter] == true && FilterValue != null)
                    parameters.Add($"{FlagStrings.Filter}={FilterValue}");
            }

            // Input File
            if (IsFlagSupported(FlagStrings.InputFile))
            {
                if (this[FlagStrings.InputFile] == true && InputFileValue != null)
                    parameters.Add($"{FlagStrings.InputFile}=\"{InputFileValue}\"");
                else
                    return null;
            }

            // Output File
            if (IsFlagSupported(FlagStrings.OutputFile))
            {
                if (this[FlagStrings.OutputFile] == true && OutputFileValue != null)
                    parameters.Add($"{FlagStrings.OutputFile}=\"{OutputFileValue}\"");
                else
                    return null;
            }

            #endregion

            return string.Join(" ", parameters);
        }

        /// <inheritdoc/>
        public override Dictionary<string, List<string>> GetCommandSupport()
        {
            return new Dictionary<string, List<string>>()
            {
                [CommandStrings.NONE] = new List<string>()
                {
                    FlagStrings.BlockSize,
                    FlagStrings.Count,
                    FlagStrings.Filter,
                    FlagStrings.InputFile,
                    FlagStrings.OutputFile,
                    FlagStrings.Progress,
                    FlagStrings.Seek,
                    FlagStrings.Size,
                    FlagStrings.Skip,
                },

                [CommandStrings.List] = new List<string>()
                {
                },
            };
        }

        /// <inheritdoc/>
        public override string GetDefaultExtension(MediaType? mediaType) => Converters.Extension(mediaType);

        /// <inheritdoc/>
        public override bool IsDumpingCommand()
        {
            switch (this.BaseCommand)
            {
                case CommandStrings.List:
                    return false;
                default:
                    return true;
            }
        }

        /// <inheritdoc/>
        protected override void ResetValues()
        {
            BaseCommand = CommandStrings.NONE;

            flags = new Dictionary<string, bool?>();

            BlockSizeValue = null;
            CountValue = null;
            InputFileValue = null;
            OutputFileValue = null;
            SeekValue = null;
            SkipValue = null;
        }

        /// <inheritdoc/>
        protected override void SetDefaultParameters(char driveLetter, string filename, int? driveSpeed, Options options)
        {
            BaseCommand = CommandStrings.NONE;

            this[FlagStrings.InputFile] = true;
            InputFileValue = $"\\\\?\\{driveLetter}:";

            this[FlagStrings.OutputFile] = true;
            OutputFileValue = filename;

            // TODO: Add more common block sizes
            this[FlagStrings.BlockSize] = true;
            switch (this.Type)
            {
                case MediaType.FloppyDisk:
                    BlockSizeValue = 1440 * 1024;
                    break;

                default:
                    BlockSizeValue = 1024 * 1024 * 1024;
                    break;
            }

            this[FlagStrings.Progress] = true;
            this[FlagStrings.Size] = true;
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

            // Determine what the commandline should look like given the first item
            int start = 0;
            if (parts[0] == CommandStrings.List)
            {
                BaseCommand = parts[0];
                start = 1;
            }

            // Loop through all auxilary flags, if necessary
            for (int i = start; i < parts.Count; i++)
            {
                // Flag read-out values
                long? longValue = null;
                string stringValue = null;

                // Keep a count of keys to determine if we should break out to filename handling or not
                int keyCount = Keys.Count();

                #region Boolean flags

                // Progress
                ProcessFlagParameter(parts, FlagStrings.Progress, ref i);

                // Size
                ProcessFlagParameter(parts, FlagStrings.Size, ref i);

                #endregion

                #region Int64 flags

                // Block Size
                longValue = ProcessInt64Parameter(parts, FlagStrings.BlockSize, ref i);
                if (longValue != null)
                    BlockSizeValue = longValue;

                // Count
                longValue = ProcessInt64Parameter(parts, FlagStrings.Count, ref i);
                if (longValue != null)
                    CountValue = longValue;

                // Seek
                longValue = ProcessInt64Parameter(parts, FlagStrings.Seek, ref i);
                if (longValue != null)
                    SeekValue = longValue;

                // Skip
                longValue = ProcessInt64Parameter(parts, FlagStrings.Skip, ref i);
                if (longValue != null)
                    SkipValue = longValue;

                #endregion

                #region String flags

                // Filter (fixed, removable, disk, partition)
                stringValue = ProcessStringParameter(parts, FlagStrings.Filter, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    FilterValue = stringValue;

                // Input File
                stringValue = ProcessStringParameter(parts, FlagStrings.InputFile, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    InputFileValue = stringValue;

                // Output File
                stringValue = ProcessStringParameter(parts, FlagStrings.OutputFile, ref i);
                if (!string.IsNullOrEmpty(stringValue))
                    OutputFileValue = stringValue;

                #endregion
            }

            return true;
        }

        #endregion
    }
}
