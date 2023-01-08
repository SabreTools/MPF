﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MPF.Core.Converters;
using MPF.Core.Data;
using RedumpLib.Data;

namespace MPF.Modules.Redumper
{
    /// <summary>
    /// Represents a generic set of Redumper parameters
    /// </summary>
    /// <remarks>
    /// TODO: Redumper natively supports multiple, chained commands but MPF
    /// doesn't have an easy way of dealing with that right now. Maybe have
    /// BaseCommand be a space-deliminated list of the commands?
    /// TODO: With chained commands, how do we tell what parameters are
    /// actually supported? Do we go one by one and if it doesn't match
    /// any, then it's incorrect?
    /// </remarks>
    public class Parameters : BaseParameters
    {
        #region Generic Dumping Information

        /// <inheritdoc/>
        public override string InputPath => DriveValue;

        /// <inheritdoc/>
        public override string OutputPath => Path.Combine(ImagePathValue ?? string.Empty, ImageNameValue ?? string.Empty);

        /// <inheritdoc/>
        public override int? Speed => SpeedValue;

        #endregion

        #region Metadata

        /// <inheritdoc/>
        public override InternalProgram InternalProgram => InternalProgram.Redumper;

        #endregion

        #region Flag Values

        /// <summary>
        /// List of all modes being run
        /// </summary>
        public List<string> ModeValues { get; set; }

        #region General

        /// <summary>
        /// Drive to use, first available drive with disc, if not provided
        /// </summary>
        public string DriveValue { get; set; }

        /// <summary>
        /// Drive read speed, optimal drive speed will be used if not provided
        /// </summary>
        public int? SpeedValue { get; set; }

        /// <summary>
        /// Number of sector retries in case of SCSI/C2 error (default: 0)
        /// </summary>
        public int? RetriesValue { get; set; }

        /// <summary>
        /// Dump files base directory
        /// </summary>
        public string ImagePathValue { get; set; }

        /// <summary>
        /// Dump files prefix, autogenerated in dump mode, if not provided
        /// </summary>
        public string ImageNameValue { get; set; }

        #endregion

        #region Drive Configuration

        /// <summary>
        /// Override drive type, possible values: GENERIC, PLEXTOR, LG_ASUS
        /// </summary>
        public string DriveTypeValue { get; set; }

        /// <summary>
        /// Override drive read offset
        /// </summary>
        public int? DriveReadOffsetValue { get; set; }

        /// <summary>
        /// Override drive C2 shift
        /// </summary>
        public int? DriveC2ShiftValue { get; set; }

        /// <summary>
        /// Override drive pre-gap start LBA
        /// </summary>
        public int? DrivePregapStartValue { get; set; }

        /// <summary>
        /// Override drive read method, possible values: BE, D8, BE_CDDA
        /// </summary>
        public string DriveReadMethodValue { get; set; }

        /// <summary>
        /// Override drive sector order, possible values: DATA_C2_SUB, DATA_SUB_C2
        /// </summary>
        public string DriveSectorOrderValue { get; set; }

        #endregion

        #region Offset

        /// <summary>
        /// Override offset autodetection and use supplied value
        /// </summary>
        public int? ForceOffsetValue { get; set; }

        /// <summary>
        /// Maximum absolute sample value to treat it as silence (default: 32)
        /// </summary>
        public int? AudioSilenceThresholdValue { get; set; }

        #endregion

        #region Split

        /// <summary>
        /// Fill byte value for skipped sectors (default: 0x55)
        /// </summary>
        public byte? SkipFillValue { get; set; }

        #endregion

        #region Miscellaneous

        /// <summary>
        /// LBA to start dumping from
        /// </summary>
        public int? LBAStartValue { get; set; }

        /// <summary>
        /// LBA to stop dumping at (everything before the value), useful for discs with fake TOC
        /// </summary>
        public int? LBAEndValue { get; set; }

        /// <summary>
        /// LBA ranges of sectors to skip
        /// </summary>
        public string SkipValue { get; set; }

        #endregion

        #endregion

        /// <inheritdoc/>
        public Parameters(string parameters) : base(parameters) { }

        /// <inheritdoc/>
        public Parameters(RedumpSystem? system, MediaType? type, char driveLetter, string filename, int? driveSpeed, Options options)
            : base(system, type, driveLetter, filename, driveSpeed, options)
        {
        }

        #region BaseParameters Implementations

        /// <inheritdoc/>
        public override (bool, List<string>) CheckAllOutputFilesExist(string basePath, bool preCheck)
        {
            List<string> missingFiles = new List<string>();

            switch (this.Type)
            {
                case MediaType.CDROM:
                    if (!File.Exists($"{basePath}.fulltoc"))
                        missingFiles.Add($"{basePath}.fulltoc");
                    if (!File.Exists($"{basePath}.log"))
                        missingFiles.Add($"{basePath}.log");
                    if (!File.Exists($"{basePath}.scram") && !File.Exists($"{basePath}.scrap"))
                        missingFiles.Add($"{basePath}.scram");
                    if (!File.Exists($"{basePath}.state"))
                        missingFiles.Add($"{basePath}.state");
                    if (!File.Exists($"{basePath}.subcode"))
                        missingFiles.Add($"{basePath}.subcode");
                    if (!File.Exists($"{basePath}.toc"))
                        missingFiles.Add($"{basePath}.toc");

                    break;
            }
            
            return (true, new List<string>());
        }

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, Options options, string basePath, Drive drive, bool includeArtifacts)
        {
            // Get the dumping program and version
            info.DumpingInfo.DumpingProgram = $"{EnumConverter.LongName(this.InternalProgram)} {GetVersion($"{basePath}.log") ?? "Unknown Version"}";

            switch (this.Type)
            {
                case MediaType.CDROM:
                    info.Extras.PVD = GetPVD($"{basePath}.log") ?? "Disc has no PVD"; ;
                    info.TracksAndWriteOffsets.ClrMameProData = GetDatfile($"{basePath}.log");
                    info.TracksAndWriteOffsets.Cuesheet = GetFullFile($"{basePath}.cue") ?? "";

                    string cdWriteOffset = GetWriteOffset($"{basePath}.log") ?? "";
                    info.CommonDiscInfo.RingWriteOffset = cdWriteOffset;
                    info.TracksAndWriteOffsets.OtherWriteOffsets = cdWriteOffset;

                    long errorCount = GetErrorCount($"{basePath}.log");
                    info.CommonDiscInfo.ErrorsCount = (errorCount == -1 ? "Error retrieving error count" : errorCount.ToString());
                    break;
            }

            switch (this.System)
            {
                case RedumpSystem.KonamiPython2:
                    if (GetPlayStationExecutableInfo(drive?.Letter, out string pythonTwoSerial, out Region? pythonTwoRegion, out string pythonTwoDate))
                    {
                        // Ensure internal serial is pulled from local data
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalSerialName] = pythonTwoSerial ?? string.Empty;
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? pythonTwoRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = pythonTwoDate;
                    }

                    info.VersionAndEditions.Version = GetPlayStation2Version(drive?.Letter) ?? "";
                    break;

                case RedumpSystem.SonyPlayStation:
                    if (GetPlayStationExecutableInfo(drive?.Letter, out string playstationSerial, out Region? playstationRegion, out string playstationDate))
                    {
                        // Ensure internal serial is pulled from local data
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalSerialName] = playstationSerial ?? string.Empty;
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? playstationRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = playstationDate;
                    }

                    break;

                case RedumpSystem.SonyPlayStation2:
                    if (GetPlayStationExecutableInfo(drive?.Letter, out string playstationTwoSerial, out Region? playstationTwoRegion, out string playstationTwoDate))
                    {
                        // Ensure internal serial is pulled from local data
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalSerialName] = playstationTwoSerial ?? string.Empty;
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? playstationTwoRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = playstationTwoDate;
                    }

                    info.VersionAndEditions.Version = GetPlayStation2Version(drive?.Letter) ?? "";
                    break;

                case RedumpSystem.SonyPlayStation3:
                    info.VersionAndEditions.Version = GetPlayStation3Version(drive?.Letter) ?? "";
                    info.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalSerialName] = GetPlayStation3Serial(drive?.Letter) ?? "";
                    break;

                case RedumpSystem.SonyPlayStation4:
                    info.VersionAndEditions.Version = GetPlayStation4Version(drive?.Letter) ?? "";
                    info.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalSerialName] = GetPlayStation4Serial(drive?.Letter) ?? "";
                    break;

                case RedumpSystem.SonyPlayStation5:
                    info.VersionAndEditions.Version = GetPlayStation5Version(drive?.Letter) ?? "";
                    info.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalSerialName] = GetPlayStation5Serial(drive?.Letter) ?? "";
                    break;
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Redumper is unique in that the base command can be multiple
        /// modes all listed together. It is also unique in that "all
        /// flags are supported for everything" and it filters out internally
        /// </remarks>
        public override string GenerateParameters()
        {
            List<string> parameters = new List<string>();

            if (ModeValues == null)
                ModeValues = new List<string> { CommandStrings.NONE };

            // Modes
            parameters.AddRange(ModeValues);

            #region General

            // Help
            if (this[FlagStrings.HelpLong] == true)
                parameters.Add(FlagStrings.HelpLong);

            // Verbose
            if (this[FlagStrings.Verbose] == true)
                parameters.Add(FlagStrings.Verbose);

            // Drive
            if (this[FlagStrings.Drive] == true)
            {
                if (DriveValue != null)
                    parameters.Add($"{FlagStrings.Drive}={DriveValue}");
            }

            // Speed
            if (this[FlagStrings.Speed] == true)
            {
                if (SpeedValue != null)
                    parameters.Add($"{FlagStrings.Speed}={SpeedValue}");
            }

            // Retries
            if (this[FlagStrings.Retries] == true)
            {
                if (RetriesValue != null)
                    parameters.Add($"{FlagStrings.Retries}={RetriesValue}");
            }

            // Image Path
            if (this[FlagStrings.ImagePath] == true)
            {
                if (ImagePathValue != null)
                    parameters.Add($"{FlagStrings.ImagePath}={ImagePathValue}");
            }

            // Image Name
            if (this[FlagStrings.ImageName] == true)
            {
                if (ImageNameValue != null)
                    parameters.Add($"{FlagStrings.ImageName}={ImageNameValue}");
            }

            // Overwrite
            if (this[FlagStrings.Overwrite] == true)
                parameters.Add(FlagStrings.Overwrite);

            #endregion

            #region Drive Configuration

            // Drive Type
            if (this[FlagStrings.DriveType] == true)
            {
                if (DriveTypeValue != null)
                    parameters.Add($"{FlagStrings.DriveType}={DriveTypeValue}");
            }

            // Drive Read Offset
            if (this[FlagStrings.DriveReadOffset] == true)
            {
                if (DriveReadOffsetValue != null)
                    parameters.Add($"{FlagStrings.DriveReadOffset}={DriveReadOffsetValue}");
            }

            // Drive C2 Shift
            if (this[FlagStrings.DriveC2Shift] == true)
            {
                if (DriveC2ShiftValue != null)
                    parameters.Add($"{FlagStrings.DriveC2Shift}={DriveC2ShiftValue}");
            }

            // Drive Pregap Start
            if (this[FlagStrings.DrivePregapStart] == true)
            {
                if (DrivePregapStartValue != null)
                    parameters.Add($"{FlagStrings.DrivePregapStart}={DrivePregapStartValue}");
            }

            // Drive Read Method
            if (this[FlagStrings.DriveReadMethod] == true)
            {
                if (DriveReadMethodValue != null)
                    parameters.Add($"{FlagStrings.DriveReadMethod}={DriveReadMethodValue}");
            }

            // Drive Sector Order
            if (this[FlagStrings.DriveSectorOrder] == true)
            {
                if (DriveSectorOrderValue != null)
                    parameters.Add($"{FlagStrings.DriveSectorOrder}={DriveSectorOrderValue}");
            }

            #endregion

            #region Drive Specific

            // Plextor Skip Leadin
            if (this[FlagStrings.PlextorSkipLeadin] == true)
                parameters.Add(FlagStrings.PlextorSkipLeadin);

            // Asus Skip Leadout
            if (this[FlagStrings.AsusSkipLeadout] == true)
                parameters.Add(FlagStrings.AsusSkipLeadout);

            #endregion

            #region Offset

            // Force Offset
            if (this[FlagStrings.ForceOffset] == true)
            {
                if (ForceOffsetValue != null)
                    parameters.Add($"{FlagStrings.ForceOffset}={ForceOffsetValue}");
            }

            // Audio Silence Threshold
            if (this[FlagStrings.AudioSilenceThreshold] == true)
            {
                if (AudioSilenceThresholdValue != null)
                    parameters.Add($"{FlagStrings.AudioSilenceThreshold}={AudioSilenceThresholdValue}");
            }

            // Correct Offset Shift
            if (this[FlagStrings.CorrectOffsetShift] == true)
                parameters.Add(FlagStrings.CorrectOffsetShift);

            #endregion

            #region Split

            // Force Split
            if (this[FlagStrings.ForceSplit] == true)
                parameters.Add(FlagStrings.ForceSplit);

            // Leave Unchanged
            if (this[FlagStrings.LeaveUnchanged] == true)
                parameters.Add(FlagStrings.LeaveUnchanged);

            // Force QTOC
            if (this[FlagStrings.ForceQTOC] == true)
                parameters.Add(FlagStrings.ForceQTOC);

            // Skip Fill
            if (this[FlagStrings.SkipFill] == true)
            {
                if (SkipFillValue != null)
                    parameters.Add($"{FlagStrings.SkipFill}={SkipFillValue:x}");
            }

            // ISO9660 Trim
            if (this[FlagStrings.ISO9660Trim] == true)
                parameters.Add(FlagStrings.ISO9660Trim);

            // CD-i Ready Normalize
            if (this[FlagStrings.CDiReadyNormalize] == true)
                parameters.Add(FlagStrings.CDiReadyNormalize);

            #endregion

            #region Miscellaneous

            // LBA Start
            if (this[FlagStrings.LBAStart] == true)
            {
                if (LBAStartValue != null)
                    parameters.Add($"{FlagStrings.LBAStart}={LBAStartValue}");
            }

            // LBA End
            if (this[FlagStrings.LBAEnd] == true)
            {
                if (LBAEndValue != null)
                    parameters.Add($"{FlagStrings.LBAEnd}={LBAEndValue}");
            }

            // Refine Subchannel
            if (this[FlagStrings.RefineSubchannel] == true)
                parameters.Add(FlagStrings.RefineSubchannel);

            // Skip
            if (this[FlagStrings.Skip] == true)
            {
                if (!string.IsNullOrWhiteSpace(SkipValue))
                    parameters.Add($"{FlagStrings.Skip}={SkipValue}");
            }

            #endregion

            return string.Join(" ", parameters);
        }

        /// <inheritdoc/>
        /// <remarks>Command support is irrelevant for redumper</remarks>
        public override Dictionary<string, List<string>> GetCommandSupport()
        {
            return new Dictionary<string, List<string>>()
            {
                [CommandStrings.NONE] = new List<string>
                {
                    // General
                    FlagStrings.HelpLong,
                    FlagStrings.HelpShort,
                    FlagStrings.Verbose,
                    FlagStrings.Drive,
                    FlagStrings.Speed,
                    FlagStrings.Retries,
                    FlagStrings.ImagePath,
                    FlagStrings.ImageName,
                    FlagStrings.Overwrite,

                    // Drive Configuration
                    FlagStrings.DriveType,
                    FlagStrings.DriveReadOffset,
                    FlagStrings.DriveC2Shift,
                    FlagStrings.DrivePregapStart,
                    FlagStrings.DriveReadMethod,
                    FlagStrings.DriveSectorOrder,

                    // Drive Specific
                    FlagStrings.PlextorSkipLeadin,
                    FlagStrings.AsusSkipLeadout,

                    // Offset
                    FlagStrings.ForceOffset,
                    FlagStrings.AudioSilenceThreshold,
                    FlagStrings.CorrectOffsetShift,

                    // Split
                    FlagStrings.ForceSplit,
                    FlagStrings.LeaveUnchanged,
                    FlagStrings.ForceQTOC,
                    FlagStrings.SkipFill,
                    FlagStrings.ISO9660Trim,
                    FlagStrings.CDiReadyNormalize,

                    // Miscellaneous
                    FlagStrings.LBAStart,
                    FlagStrings.LBAEnd,
                    FlagStrings.RefineSubchannel,
                    FlagStrings.Skip,
                },
            };
        }

        /// <inheritdoc/>
        public override string GetDefaultExtension(MediaType? mediaType) => Converters.Extension(mediaType);

        /// <inheritdoc/>
        public override List<string> GetLogFilePaths(string basePath)
        {
            List<string> logFiles = new List<string>();

            switch (this.Type)
            {
                case MediaType.CDROM:
                    if (File.Exists($"{basePath}.cdtext"))
                        logFiles.Add($"{basePath}.cdtext");
                    if (File.Exists($"{basePath}.fulltoc"))
                        logFiles.Add($"{basePath}.fulltoc");
                    if (File.Exists($"{basePath}.log"))
                        logFiles.Add($"{basePath}.log");
                    // if (File.Exists($"{basePath}.scram"))
                    //     logFiles.Add($"{basePath}.scram");
                    // if (File.Exists($"{basePath}.scrap"))
                    //     logFiles.Add($"{basePath}.scrap");
                    if (File.Exists($"{basePath}.state"))
                        logFiles.Add($"{basePath}.state");
                    if (File.Exists($"{basePath}.subcode"))
                        logFiles.Add($"{basePath}.subcode");
                    if (File.Exists($"{basePath}.toc"))
                        logFiles.Add($"{basePath}.toc");
                    break;
            }

            return logFiles;
        }

        /// <inheritdoc/>
        public override bool IsDumpingCommand()
        {
            return this.BaseCommand == CommandStrings.NONE
                || this.BaseCommand.Contains(CommandStrings.CD)
                || this.BaseCommand.Contains(CommandStrings.Dump);
        }

        /// <inheritdoc/>
        protected override void ResetValues()
        {
            BaseCommand = CommandStrings.NONE;

            flags = new Dictionary<string, bool?>();

            // General
            DriveValue = null;
            SpeedValue = null;
            RetriesValue = null;
            ImagePathValue = null;
            ImageNameValue = null;

            // Drive Configuration
            DriveTypeValue = null;
            DriveReadOffsetValue = null;
            DriveC2ShiftValue = null;
            DrivePregapStartValue = null;
            DriveReadMethodValue = null;
            DriveSectorOrderValue = null;

            // Offset
            ForceOffsetValue = null;
            AudioSilenceThresholdValue = null;

            // Split
            SkipFillValue = null;

            // Miscellaneous
            LBAStartValue = null;
            LBAEndValue = null;
            SkipValue = null;
        }

        /// <inheritdoc/>
        protected override void SetDefaultParameters(char driveLetter, string filename, int? driveSpeed, Options options)
        {
            // If we don't have a CD, we can't dump using redumper
            if (this.Type != MediaType.CDROM)
                return;

            BaseCommand = CommandStrings.NONE;
            ModeValues = new List<string> { CommandStrings.CD };

            DriveValue = driveLetter.ToString();
            SpeedValue = driveSpeed;

            // Set the output paths
            if (!string.IsNullOrWhiteSpace(filename))
            {
                string imagePath = Path.GetDirectoryName(filename);
                if (!string.IsNullOrWhiteSpace(imagePath))
                    ImagePathValue = imagePath;

                string imageName = Path.GetFileName(filename);
                if (!string.IsNullOrWhiteSpace(imageName))
                    ImageNameValue = imageName;
            }

            this[FlagStrings.Retries] = true;
            RetriesValue = options.RedumperRereadCount;
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

            // Setup the modes
            ModeValues = new List<string>();

            // All modes should be cached separately
            int index = 0;
            for (; index < parts.Count; index++)
            {
                // Flag to see if we have a flag
                bool isFlag = false;

                string part = parts[index];
                switch (part)
                {
                    case CommandStrings.CD:
                    case CommandStrings.Dump:
                    case CommandStrings.Protection:
                    case CommandStrings.Refine:
                    case CommandStrings.Split:
                    case CommandStrings.Info:
                        // case CommandStrings.Rings:
                        ModeValues.Add(part);
                        break;

                    // Default is either a flag or an invalid mode
                    default:
                        if (part.StartsWith("-"))
                        {
                            isFlag = true;
                            break;
                        }
                        else
                        {
                            return false;
                        }
                }

                // If we had a flag, break out
                if (isFlag)
                    break;
            }

            // Loop through all auxiliary flags, if necessary
            for (int i = index; i < parts.Count; i++)
            {
                #region General

                // Help
                ProcessFlagParameter(parts, FlagStrings.HelpShort, FlagStrings.HelpLong, ref i);

                // Verbose
                ProcessFlagParameter(parts, FlagStrings.Verbose, ref i);

                // Drive
                DriveValue = ProcessStringParameter(parts, FlagStrings.Drive, ref i);

                // Speed
                SpeedValue = ProcessInt32Parameter(parts, FlagStrings.Speed, ref i);

                // Retries
                RetriesValue = ProcessInt32Parameter(parts, FlagStrings.Retries, ref i);

                // Image Path
                ImagePathValue = ProcessStringParameter(parts, FlagStrings.ImagePath, ref i);

                // Image Name
                ImageNameValue = ProcessStringParameter(parts, FlagStrings.ImageName, ref i);

                // Overwrite
                ProcessFlagParameter(parts, FlagStrings.Overwrite, ref i);

                #endregion

                #region Drive Configuration

                // Drive Type
                DriveTypeValue = ProcessStringParameter(parts, FlagStrings.DriveType, ref i);

                // Drive Read Offset
                DriveReadOffsetValue = ProcessInt32Parameter(parts, FlagStrings.DriveReadOffset, ref i);

                // Drive C2 Shift
                DriveC2ShiftValue = ProcessInt32Parameter(parts, FlagStrings.DriveC2Shift, ref i);

                // Drive Pregap Start
                DrivePregapStartValue = ProcessInt32Parameter(parts, FlagStrings.DrivePregapStart, ref i);

                // Drive Read Method
                DriveReadMethodValue = ProcessStringParameter(parts, FlagStrings.DriveReadMethod, ref i);

                // Drive Sector Order
                DriveSectorOrderValue = ProcessStringParameter(parts, FlagStrings.DriveSectorOrder, ref i);

                #endregion

                #region Drive Specific

                // Plextor Skip Leadin
                ProcessFlagParameter(parts, FlagStrings.PlextorSkipLeadin, ref i);

                // Asus Skip Leadout
                ProcessFlagParameter(parts, FlagStrings.AsusSkipLeadout, ref i);

                #endregion

                #region Offset

                // Force Offset
                ForceOffsetValue = ProcessInt32Parameter(parts, FlagStrings.ForceOffset, ref i);

                // Audio Silence Threshold
                AudioSilenceThresholdValue = ProcessInt32Parameter(parts, FlagStrings.AudioSilenceThreshold, ref i);

                // Correct Offset Shift
                ProcessFlagParameter(parts, FlagStrings.CorrectOffsetShift, ref i);

                #endregion

                #region Split

                // Force Split
                ProcessFlagParameter(parts, FlagStrings.ForceSplit, ref i);

                // Leave Unchanged
                ProcessFlagParameter(parts, FlagStrings.LeaveUnchanged, ref i);

                // Force QTOC
                ProcessFlagParameter(parts, FlagStrings.ForceQTOC, ref i);

                // Skip Fill
                SkipFillValue = ProcessUInt8Parameter(parts, FlagStrings.SkipFill, ref i);

                // ISO9660 Trim
                ProcessFlagParameter(parts, FlagStrings.ISO9660Trim, ref i);

                // CD-i Ready Normalize
                ProcessFlagParameter(parts, FlagStrings.CDiReadyNormalize, ref i);

                #endregion

                #region Miscellaneous

                // LBA Start
                LBAStartValue = ProcessInt32Parameter(parts, FlagStrings.LBAStart, ref i);

                // LBA End
                LBAEndValue = ProcessInt32Parameter(parts, FlagStrings.LBAEnd, ref i);

                // Refine Subchannel
                ProcessFlagParameter(parts, FlagStrings.RefineSubchannel, ref i);

                // Skip
                SkipValue = ProcessStringParameter(parts, FlagStrings.Skip, ref i);

                #endregion
            }

            // If the image name was not set, set it with a default value
            if (string.IsNullOrWhiteSpace(this.ImageNameValue))
                this.ImagePathValue = "track.bin";

            return true;
        }

        #endregion

        #region Information Extraction Methods

        /// <summary>
        /// Get the cuesheet from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Newline-delimited cuesheet if possible, null on error</returns>
        private static string GetCuesheet(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            using (StreamReader sr = File.OpenText(log))
            {
                try
                {
                    // Fast forward to the dat line
                    while (!sr.EndOfStream && !sr.ReadLine().TrimStart().StartsWith("CUE ["));
                    if (sr.EndOfStream)
                        return null;

                    // Now that we're at the relevant entries, read each line in and concatenate
                    string cueString = "", line = sr.ReadLine().Trim();
                    while (!string.IsNullOrWhiteSpace(line))
                    {
                        cueString += line + "\n";
                        line = sr.ReadLine().Trim();
                    }

                    return cueString.TrimEnd('\n');
                }
                catch
                {
                    // We don't care what the exception is right now
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the datfile from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Newline-delimited datfile if possible, null on error</returns>
        private static string GetDatfile(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            using (StreamReader sr = File.OpenText(log))
            {
                try
                {
                    // Fast forward to the dat line
                    while (!sr.EndOfStream && !sr.ReadLine().TrimStart().StartsWith("dat:"));
                    if (sr.EndOfStream)
                        return null;

                    // Now that we're at the relevant entries, read each line in and concatenate
                    string datString = "", line = sr.ReadLine().Trim();
                    while (line.StartsWith("<rom"))
                    {
                        datString += line + "\n";
                        line = sr.ReadLine().Trim();
                    }

                    return datString.TrimEnd('\n');
                }
                catch
                {
                    // We don't care what the exception is right now
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the detected error count from the input files, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Error count if possible, -1 on error</returns>
        public static long GetErrorCount(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(path: log))
                return -1;

            // First line of defense is the EdcEcc error file
            using (StreamReader sr = File.OpenText(log))
            {
                try
                {
                    // Fast forward to the errors lines
                    while (!sr.EndOfStream && !sr.ReadLine().Trim().StartsWith("data errors"));
                    if (sr.EndOfStream)
                        return 0;

                    // Now that we're at the relevant entries, read each line in and concatenate
                    long errorCount = 0;
                    while (!sr.EndOfStream)
                    {
                        // Skip forward to the "redump" line
                        string line;
                        while (!(line = sr.ReadLine().Trim()).StartsWith("redump"));

                        // redump: <error count>
                        string[] parts = line.Split(' ');
                        if (long.TryParse(parts[1], out long redump))
                            errorCount += redump;
                        else
                            return -1;

                        if (!sr.EndOfStream)
                            sr.ReadLine(); // Empty line

                        if (!sr.EndOfStream && !sr.ReadLine().Trim().StartsWith("data errors"))
                            break;
                    }

                    return errorCount;
                }
                catch
                {
                    // We don't care what the exception is right now
                    return -1;
                }
            }
        }

        /// <summary>
        /// Get the PVD from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Newline-delimited PVD if possible, null on error</returns>
        private static string GetPVD(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            using (StreamReader sr = File.OpenText(log))
            {
                try
                {
                    // Fast forward to the dat line
                    while (!sr.EndOfStream && !sr.ReadLine().TrimStart().StartsWith("PVD:"));
                    if (sr.EndOfStream)
                        return null;

                    // Now that we're at the relevant entries, read each line in and concatenate
                    string pvdString = "", line = sr.ReadLine().Trim();
                    while (line.StartsWith("03"))
                    {
                        pvdString += line + "\n";
                        line = sr.ReadLine().Trim();
                    }

                    return pvdString.TrimEnd('\n');
                }
                catch
                {
                    // We don't care what the exception is right now
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the write offset from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Sample write offset if possible, null on error</returns>
        private static string GetWriteOffset(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            using (StreamReader sr = File.OpenText(log))
            {
                try
                {
                    // Fast forward to the offset lines
                    while (!sr.EndOfStream && !sr.ReadLine().TrimStart().StartsWith("detecting offset"));
                    if (sr.EndOfStream)
                        return null;

                    // We skip during detection and get the write offset
                    string line;
                    while (!sr.EndOfStream && !(line = sr.ReadLine().TrimStart()).StartsWith("detection complete"))
                    {
                        if (line.StartsWith("disc write offset:"))
                            return line.Substring("disc write offset: ".Length).Trim();
                    }

                    // We couldn't detect it then
                    return null;
                }
                catch
                {
                    // We don't care what the exception is right now
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the version. if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Version if possible, null on error</returns>
        private static string GetVersion(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            // Samples:
            // redumper v2022.10.28 [Oct 28 2022, 05:41:43] (print usage: --help,-h)
            // redumper v2022.12.22 build_87 [Dec 22 2022, 01:56:26]

            using (StreamReader sr = File.OpenText(log))
            {
                try
                {
                    // Skip first line (dump date)
                    sr.ReadLine();

                    // Generate regex
                    // Permissive
                    var regex = new Regex(@"^redumper (v.+) \[.+\]");
                    // Strict
                    //var regex = new Regex(@"^redumper (v\d{4}\.\d{2}\.\d{2}(| build_\d+)) \[.+\]");

                    // Extract the version string
                    var match = regex.Match(sr.ReadLine().Trim());
                    var version = match.Groups[1].Value;
                    return string.IsNullOrWhiteSpace(version) ? null : version;
                }
                catch
                {
                    // We don't care what the exception is right now
                    return null;
                }
            }
        }

        #endregion
    }
}
