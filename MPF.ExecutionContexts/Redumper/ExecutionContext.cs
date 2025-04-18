using System.Collections.Generic;
using System.IO;
using System.Text;
using MPF.ExecutionContexts.Data;
using SabreTools.RedumpLib.Data;

namespace MPF.ExecutionContexts.Redumper
{
    /// <summary>
    /// Represents a generic set of Redumper parameters
    /// </summary>
    public sealed class ExecutionContext : BaseExecutionContext
    {
        #region Generic Dumping Information

        /// <inheritdoc/>
        public override string? InputPath
            => (_inputs[FlagStrings.Drive] as StringInput)?.Value?.Trim('"');

        /// <inheritdoc/>
        public override string? OutputPath => Path.Combine(
                (_inputs[FlagStrings.ImagePath] as StringInput)?.Value?.Trim('"') ?? string.Empty,
                (_inputs[FlagStrings.ImageName] as StringInput)?.Value?.Trim('"') ?? string.Empty)
            + GetDefaultExtension(MediaType);

        /// <inheritdoc/>
        public override int? Speed
        {
            get
            {
                return (_inputs[FlagStrings.Speed] as Int32Input)?.Value;
            }
            set
            {
                (_inputs[FlagStrings.Speed] as Int32Input)?.SetValue(value);
            }
        }

        #endregion

        #region Flag Values

        /// <summary>
        /// Mode being run
        /// </summary>
        public string? ModeValue { get; set; }

        /// <summary>
        /// Set of all command flags
        /// </summary>
        private readonly Dictionary<string, Input> _inputs = new()
        {
            // General
            [FlagStrings.HelpLong] = new FlagInput(FlagStrings.HelpShort, FlagStrings.HelpLong),
            [FlagStrings.Version] = new FlagInput(FlagStrings.Version),
            [FlagStrings.Verbose] = new FlagInput(FlagStrings.Verbose),
            [FlagStrings.Continue] = new StringInput(FlagStrings.Continue),
            [FlagStrings.AutoEject] = new FlagInput(FlagStrings.AutoEject),
            [FlagStrings.Debug] = new FlagInput(FlagStrings.Debug),
            [FlagStrings.DiscType] = new StringInput(FlagStrings.DiscType),
            [FlagStrings.Drive] = new StringInput(FlagStrings.Drive),
            [FlagStrings.Speed] = new Int32Input(FlagStrings.Speed),
            [FlagStrings.Retries] = new Int32Input(FlagStrings.Retries),
            [FlagStrings.ImagePath] = new StringInput(FlagStrings.ImagePath) { Quotes = true },
            [FlagStrings.ImageName] = new StringInput(FlagStrings.ImageName) { Quotes = true },
            [FlagStrings.Overwrite] = new FlagInput(FlagStrings.Overwrite),

            // Drive Configuration
            [FlagStrings.Overwrite] = new FlagInput(FlagStrings.Overwrite),
            [FlagStrings.DriveType] = new StringInput(FlagStrings.DriveType),
            [FlagStrings.DriveReadOffset] = new Int32Input(FlagStrings.DriveReadOffset),
            [FlagStrings.DriveC2Shift] = new Int32Input(FlagStrings.DriveC2Shift),
            [FlagStrings.DrivePregapStart] = new Int32Input(FlagStrings.DrivePregapStart),
            [FlagStrings.DriveReadMethod] = new StringInput(FlagStrings.DriveReadMethod),
            [FlagStrings.DriveSectorOrder] = new StringInput(FlagStrings.DriveSectorOrder),

            // Drive Specific
            [FlagStrings.PlextorSkipLeadin] = new FlagInput(FlagStrings.PlextorSkipLeadin),
            [FlagStrings.PlextorLeadinRetries] = new Int32Input(FlagStrings.PlextorLeadinRetries),
            [FlagStrings.PlextorLeadinForceStore] = new FlagInput(FlagStrings.PlextorLeadinForceStore),
            [FlagStrings.AsusSkipLeadout] = new FlagInput(FlagStrings.AsusSkipLeadout),
            [FlagStrings.AsusLeadoutRetries] = new Int32Input(FlagStrings.AsusLeadoutRetries),

            // Offset
            [FlagStrings.ForceOffset] = new Int32Input(FlagStrings.ForceOffset),
            [FlagStrings.AudioSilenceThreshold] = new Int32Input(FlagStrings.AudioSilenceThreshold),
            [FlagStrings.CorrectOffsetShift] = new FlagInput(FlagStrings.CorrectOffsetShift),
            [FlagStrings.OffsetShiftRelocate] = new FlagInput(FlagStrings.OffsetShiftRelocate),

            // Split
            [FlagStrings.ForceSplit] = new FlagInput(FlagStrings.ForceSplit),
            [FlagStrings.LeaveUnchanged] = new FlagInput(FlagStrings.LeaveUnchanged),
            [FlagStrings.ForceQTOC] = new FlagInput(FlagStrings.ForceQTOC),
            [FlagStrings.SkipFill] = new UInt8Input(FlagStrings.SkipFill),
            [FlagStrings.ISO9660Trim] = new FlagInput(FlagStrings.ISO9660Trim),

            // Miscellaneous
            [FlagStrings.LBAStart] = new Int32Input(FlagStrings.LBAStart),
            [FlagStrings.LBAEnd] = new Int32Input(FlagStrings.LBAEnd),
            [FlagStrings.RefineSubchannel] = new FlagInput(FlagStrings.RefineSubchannel),
            [FlagStrings.Skip] = new StringInput(FlagStrings.Skip),
            [FlagStrings.DumpWriteOffset] = new Int32Input(FlagStrings.DumpWriteOffset),
            [FlagStrings.DumpReadSize] = new Int32Input(FlagStrings.DumpReadSize),
            [FlagStrings.OverreadLeadout] = new FlagInput(FlagStrings.OverreadLeadout),
            [FlagStrings.ForceUnscrambled] = new FlagInput(FlagStrings.ForceUnscrambled),
            [FlagStrings.ForceRefine] = new FlagInput(FlagStrings.ForceRefine),
            [FlagStrings.LegacySubs] = new FlagInput(FlagStrings.LegacySubs),
            [FlagStrings.DisableCDText] = new FlagInput(FlagStrings.DisableCDText),
            [FlagStrings.SkipSubcodeDesync] = new FlagInput(FlagStrings.SkipSubcodeDesync),
            [FlagStrings.DriveTestSkipPlextorLeadin] = new FlagInput(FlagStrings.DriveTestSkipPlextorLeadin),
            [FlagStrings.DriveTestSkipCacheRead] = new FlagInput(FlagStrings.DriveTestSkipCacheRead),
            //[FlagStrings.Firmware] = new StringInput(FlagStrings.Firmware) { Quotes = true },
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
        /// <remarks>Command support is irrelevant for redumper</remarks>
        public override Dictionary<string, List<string>> GetCommandSupport()
        {
            return new Dictionary<string, List<string>>()
            {
                [CommandStrings.NONE] =
                [
                    // General
                    FlagStrings.HelpLong,
                    FlagStrings.HelpShort,
                    FlagStrings.Version,
                    FlagStrings.Verbose,
                    FlagStrings.Continue,
                    FlagStrings.AutoEject,
                    FlagStrings.Debug,
                    FlagStrings.DiscType,
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
                    FlagStrings.PlextorLeadinRetries,
                    FlagStrings.PlextorLeadinForceStore,
                    FlagStrings.AsusSkipLeadout,
                    FlagStrings.AsusLeadoutRetries,

                    // Offset
                    FlagStrings.ForceOffset,
                    FlagStrings.AudioSilenceThreshold,
                    FlagStrings.CorrectOffsetShift,
                    FlagStrings.OffsetShiftRelocate,

                    // Split
                    FlagStrings.ForceSplit,
                    FlagStrings.LeaveUnchanged,
                    FlagStrings.ForceQTOC,
                    FlagStrings.SkipFill,
                    FlagStrings.ISO9660Trim,

                    // Miscellaneous
                    FlagStrings.LBAStart,
                    FlagStrings.LBAEnd,
                    FlagStrings.RefineSubchannel,
                    FlagStrings.Skip,
                    FlagStrings.DumpWriteOffset,
                    FlagStrings.DumpReadSize,
                    FlagStrings.OverreadLeadout,
                    FlagStrings.ForceUnscrambled,
                    FlagStrings.ForceRefine,
                    FlagStrings.LegacySubs,
                    FlagStrings.DisableCDText,
                    FlagStrings.SkipSubcodeDesync,
                    FlagStrings.DriveTestSkipPlextorLeadin,
                    FlagStrings.DriveTestSkipCacheRead,
                    //FlagStrings.Firmware,
                ],
            };
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Redumper is unique in that the base command can be multiple
        /// modes all listed together. It is also unique in that "all
        /// flags are supported for everything" and it filters out internally
        /// </remarks>
        public override string GenerateParameters()
        {
            var parameters = new StringBuilder();

            // Command Mode
            ModeValue ??= CommandStrings.NONE;
            if (ModeValue != CommandStrings.NONE)
                parameters.Append($"{ModeValue} ");

            // Loop though and append all existing
            foreach (var kvp in _inputs)
            {
                // If the value doesn't exist
                string formatted = kvp.Value.Format(useEquals: true);
                if (formatted.Length == 0)
                    continue;

                // Append the parameter
                parameters.Append($"{formatted} ");
            }

            return parameters.ToString().TrimEnd();
        }

        /// <inheritdoc/>
        public override string? GetDefaultExtension(MediaType? mediaType) => Converters.Extension(mediaType);

        /// <inheritdoc/>
        public override MediaType? GetMediaType() => null;

        /// <inheritdoc/>
        public override bool IsDumpingCommand()
        {
            // `dump` command does not provide hashes so will error out after dump if run via MPF
            return ModeValue == CommandStrings.NONE
                || ModeValue == CommandStrings.Disc;
        }

        /// <inheritdoc/>
        protected override void ResetValues()
        {
            BaseCommand = CommandStrings.NONE;

            flags = [];

            foreach (var kvp in _inputs)
                kvp.Value.ClearValue();
        }

        /// <inheritdoc/>
        protected override void SetDefaultParameters(string? drivePath,
            string filename,
            int? driveSpeed,
            Dictionary<string, string?> options)
        {
            BaseCommand = CommandStrings.NONE;
            switch (MediaType)
            {
                case SabreTools.RedumpLib.Data.MediaType.CDROM:
                case SabreTools.RedumpLib.Data.MediaType.DVD:
                case SabreTools.RedumpLib.Data.MediaType.NintendoGameCubeGameDisc:
                case SabreTools.RedumpLib.Data.MediaType.NintendoWiiOpticalDisc:
                case SabreTools.RedumpLib.Data.MediaType.HDDVD:
                case SabreTools.RedumpLib.Data.MediaType.BluRay:
                case SabreTools.RedumpLib.Data.MediaType.NintendoWiiUOpticalDisc:
                    ModeValue = CommandStrings.Disc;
                    break;
                default:
                    BaseCommand = null;
                    return;
            }

            this[FlagStrings.Drive] = true;
            (_inputs[FlagStrings.Drive] as StringInput)?.SetValue(drivePath ?? string.Empty);
            
            if (driveSpeed != null && driveSpeed > 0)
            {
                this[FlagStrings.Speed] = true;
                (_inputs[FlagStrings.Speed] as Int32Input)?.SetValue(driveSpeed);
            }

            // Set user-defined options
            if (GetBooleanSetting(options, SettingConstants.EnableVerbose, SettingConstants.EnableVerboseDefault))
            {
                this[FlagStrings.Verbose] = true;
                (_inputs[FlagStrings.Verbose] as FlagInput)?.SetValue(true);
            }
            if (GetBooleanSetting(options, SettingConstants.EnableDebug, SettingConstants.EnableDebugDefault))
            {
                this[FlagStrings.Debug] = true;
                (_inputs[FlagStrings.Debug] as FlagInput)?.SetValue(true);
            }

            string? readMethod = GetStringSetting(options, SettingConstants.ReadMethod, SettingConstants.ReadMethodDefault);

            if (!string.IsNullOrEmpty(readMethod) && readMethod != ReadMethod.NONE.ToString())
            {
                this[FlagStrings.DriveReadMethod] = true;
                (_inputs[FlagStrings.DriveReadMethod] as StringInput)?.SetValue(readMethod!);
            }

            string? sectorOrder = GetStringSetting(options, SettingConstants.SectorOrder, SettingConstants.SectorOrderDefault);
            if (!string.IsNullOrEmpty(sectorOrder) && sectorOrder != SectorOrder.NONE.ToString())
            {
                this[FlagStrings.DriveSectorOrder] = true;
                (_inputs[FlagStrings.DriveSectorOrder] as StringInput)?.SetValue(sectorOrder!);
            }

            if (GetBooleanSetting(options, SettingConstants.UseGenericDriveType, SettingConstants.UseGenericDriveTypeDefault))
            {
                this[FlagStrings.DriveType] = true;
                (_inputs[FlagStrings.DriveType] as StringInput)?.SetValue("GENERIC");
            }

            // Set the output paths
            if (!string.IsNullOrEmpty(filename))
            {
                var imagePath = Path.GetDirectoryName(filename);
                if (!string.IsNullOrEmpty(imagePath))
                {
                    this[FlagStrings.ImagePath] = true;
                    (_inputs[FlagStrings.ImagePath] as StringInput)?.SetValue(imagePath!);
                }

                string imageName = Path.GetFileNameWithoutExtension(filename);
                if (!string.IsNullOrEmpty(imageName))
                {
                    this[FlagStrings.ImageName] = true;
                    (_inputs[FlagStrings.ImageName] as StringInput)?.SetValue(imageName!);
                }
            }

            this[FlagStrings.Retries] = true;
            (_inputs[FlagStrings.Retries] as Int32Input)?.SetValue(GetInt32Setting(options, SettingConstants.RereadCount, SettingConstants.RereadCountDefault));

            if (GetBooleanSetting(options, SettingConstants.EnableLeadinRetry, SettingConstants.EnableLeadinRetryDefault))
            {
                this[FlagStrings.PlextorLeadinRetries] = true;
                (_inputs[FlagStrings.PlextorLeadinRetries] as Int32Input)?.SetValue(GetInt32Setting(options, SettingConstants.LeadinRetryCount, SettingConstants.LeadinRetryCountDefault));
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

            // Setup the modes
            ModeValue = null;

            // All modes should be cached separately
            int index = 0;
            for (; index < parts.Length; index++)
            {
                // Flag to see if we have a flag
                bool isFlag = false;

                string part = parts[index];
                switch (part)
                {
                    case CommandStrings.Disc:
                    case CommandStrings.Rings:
                    case CommandStrings.Dump:
                    case CommandStrings.DumpExtra:
                    case CommandStrings.Refine:
                    case CommandStrings.Verify:
                    case CommandStrings.DVDKey:
                    case CommandStrings.Eject:
                    case CommandStrings.DVDIsoKey:
                    case CommandStrings.Protection:
                    case CommandStrings.Split:
                    case CommandStrings.Hash:
                    case CommandStrings.Info:
                    case CommandStrings.Skeleton:
                    case CommandStrings.FlashMT1339:
                    case CommandStrings.Subchannel:
                    case CommandStrings.Debug:
                    case CommandStrings.FixMSF:
                    case CommandStrings.DebugFlip:
                    case CommandStrings.DriveTest:
                        // Only allow one mode per command
                        if (ModeValue != null)
                            continue;
                        
                        ModeValue = part;
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
            for (int i = index; i < parts.Length; i++)
            {
                // Match all possible flags
                foreach (var kvp in _inputs)
                {
                    // If the value was not a match
                    if (!kvp.Value.Process(parts, ref i))
                        continue;

                    // Set the flag
                    this[kvp.Key] = true;
                }
            }

            // If the image name was not set, set it with a default value
            if (string.IsNullOrEmpty((_inputs[FlagStrings.ImageName] as StringInput)?.Value))
                (_inputs[FlagStrings.ImageName] as StringInput)?.SetValue("track");

            return true;
        }

        #endregion
    }
}
