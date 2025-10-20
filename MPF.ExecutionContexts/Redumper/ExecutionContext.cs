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
                if (value != null && value > 0)
                {
                    this[FlagStrings.Speed] = true;
                    (_inputs[FlagStrings.Speed] as Int32Input)?.SetValue(value);
                }
                else
                {
                    this[FlagStrings.Speed] = false;
                    (_inputs[FlagStrings.Speed] as Int32Input)?.SetValue(null);
                }
            }
        }

        #endregion

        #region Flag Values

        /// <summary>
        /// Set of all command flags
        /// </summary>
        private readonly Dictionary<string, Input> _inputs = new()
        {
            // General
            [FlagStrings.HelpLong] = new FlagInput(FlagStrings.HelpShort, FlagStrings.HelpLong),
            [FlagStrings.Version] = new FlagInput(FlagStrings.Version),
            [FlagStrings.Verbose] = new FlagInput(FlagStrings.Verbose),
            [FlagStrings.ListRecommendedDrives] = new FlagInput(FlagStrings.ListRecommendedDrives),
            [FlagStrings.ListAllDrives] = new FlagInput(FlagStrings.ListAllDrives),
            [FlagStrings.AutoEject] = new FlagInput(FlagStrings.AutoEject),
            [FlagStrings.Skeleton] = new FlagInput(FlagStrings.Skeleton),
            [FlagStrings.Drive] = new StringInput(FlagStrings.Drive),
            [FlagStrings.Speed] = new Int32Input(FlagStrings.Speed),
            [FlagStrings.Retries] = new Int32Input(FlagStrings.Retries),
            [FlagStrings.ImagePath] = new StringInput(FlagStrings.ImagePath) { Quotes = true },
            [FlagStrings.ImageName] = new StringInput(FlagStrings.ImageName) { Quotes = true },
            [FlagStrings.Overwrite] = new FlagInput(FlagStrings.Overwrite),
            [FlagStrings.DiscType] = new StringInput(FlagStrings.DiscType),

            // Drive Configuration
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
            [FlagStrings.KreonPartialSS] = new FlagInput(FlagStrings.KreonPartialSS),
            [FlagStrings.AsusSkipLeadout] = new FlagInput(FlagStrings.AsusSkipLeadout),
            [FlagStrings.AsusLeadoutRetries] = new Int32Input(FlagStrings.AsusLeadoutRetries),
            [FlagStrings.DisableCDText] = new FlagInput(FlagStrings.DisableCDText),

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

            // Drive Test
            [FlagStrings.DriveTestSkipPlextorLeadin] = new FlagInput(FlagStrings.DriveTestSkipPlextorLeadin),
            [FlagStrings.DriveTestSkipCacheRead] = new FlagInput(FlagStrings.DriveTestSkipCacheRead),

            // Miscellaneous
            [FlagStrings.Continue] = new StringInput(FlagStrings.Continue),
            [FlagStrings.LBAStart] = new Int32Input(FlagStrings.LBAStart),
            [FlagStrings.LBAEnd] = new Int32Input(FlagStrings.LBAEnd),
            [FlagStrings.RefineSubchannel] = new FlagInput(FlagStrings.RefineSubchannel),
            [FlagStrings.RefineSectorMode] = new FlagInput(FlagStrings.RefineSectorMode),
            [FlagStrings.Skip] = new StringInput(FlagStrings.Skip),
            [FlagStrings.DumpWriteOffset] = new Int32Input(FlagStrings.DumpWriteOffset),
            [FlagStrings.DumpReadSize] = new Int32Input(FlagStrings.DumpReadSize),
            [FlagStrings.OverreadLeadout] = new FlagInput(FlagStrings.OverreadLeadout),
            [FlagStrings.ForceUnscrambled] = new FlagInput(FlagStrings.ForceUnscrambled),
            [FlagStrings.ForceRefine] = new FlagInput(FlagStrings.ForceRefine),
            //[FlagStrings.Firmware] = new StringInput(FlagStrings.Firmware) { Quotes = true },
            [FlagStrings.SkipSubcodeDesync] = new FlagInput(FlagStrings.SkipSubcodeDesync),
            [FlagStrings.Rings] = new FlagInput(FlagStrings.Rings),

            // Undocumented
            [FlagStrings.Debug] = new FlagInput(FlagStrings.Debug),
            [FlagStrings.LegacySubs] = new FlagInput(FlagStrings.LegacySubs),
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
                    FlagStrings.ListRecommendedDrives,
                    FlagStrings.ListAllDrives,
                    FlagStrings.AutoEject,
                    FlagStrings.Skeleton,
                    FlagStrings.Drive,
                    FlagStrings.Speed,
                    FlagStrings.Retries,
                    FlagStrings.ImagePath,
                    FlagStrings.ImageName,
                    FlagStrings.Overwrite,
                    FlagStrings.DiscType,

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
                    FlagStrings.KreonPartialSS,
                    FlagStrings.AsusSkipLeadout,
                    FlagStrings.AsusLeadoutRetries,
                    FlagStrings.DisableCDText,

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

                    // Drive Test
                    FlagStrings.DriveTestSkipPlextorLeadin,
                    FlagStrings.DriveTestSkipCacheRead,

                    // Miscellaneous
                    FlagStrings.Continue,
                    FlagStrings.LBAStart,
                    FlagStrings.LBAEnd,
                    FlagStrings.RefineSubchannel,
                    FlagStrings.RefineSectorMode,
                    FlagStrings.Skip,
                    FlagStrings.DumpWriteOffset,
                    FlagStrings.DumpReadSize,
                    FlagStrings.OverreadLeadout,
                    FlagStrings.ForceUnscrambled,
                    FlagStrings.ForceRefine,
                    //FlagStrings.Firmware,
                    FlagStrings.SkipSubcodeDesync,
                    FlagStrings.Rings,

                    // Undocumented
                    FlagStrings.Debug,
                    FlagStrings.LegacySubs,
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
            BaseCommand ??= CommandStrings.NONE;
            if (BaseCommand != CommandStrings.NONE)
                parameters.Append($"{BaseCommand} ");

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
            return BaseCommand == CommandStrings.NONE
                || BaseCommand == CommandStrings.Disc;
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
            BaseCommand = CommandStrings.Disc;

            if (drivePath != null)
            {
                this[FlagStrings.Drive] = true;
                (_inputs[FlagStrings.Drive] as StringInput)?.SetValue(drivePath);
            }

            if (driveSpeed != null && driveSpeed > 0)
            {
                this[FlagStrings.Speed] = true;
                (_inputs[FlagStrings.Speed] as Int32Input)?.SetValue(driveSpeed);
            }
            else
            {
                this[FlagStrings.Speed] = false;
                (_inputs[FlagStrings.Speed] as Int32Input)?.SetValue(null);
            }

            // Set user-defined options
            if (GetBooleanSetting(options, SettingConstants.EnableVerbose, SettingConstants.EnableVerboseDefault))
            {
                this[FlagStrings.Verbose] = true;
                (_inputs[FlagStrings.Verbose] as FlagInput)?.SetValue(true);
            }
            if (GetBooleanSetting(options, SettingConstants.EnableSkeleton, SettingConstants.EnableSkeletonDefault))
            {
                switch (System)
                {
                    // Systems known to have significant data outside the ISO9660 filesystem
                    case SabreTools.RedumpLib.Data.RedumpSystem.MicrosoftXbox:
                    case SabreTools.RedumpLib.Data.RedumpSystem.MicrosoftXbox360:
                    case SabreTools.RedumpLib.Data.RedumpSystem.PlaymajiPolymega:
                    // Skeletons from newer BD-based consoles unnecessary
                    case SabreTools.RedumpLib.Data.RedumpSystem.MicrosoftXboxOne:
                    case SabreTools.RedumpLib.Data.RedumpSystem.MicrosoftXboxSeriesXS:
                    case SabreTools.RedumpLib.Data.RedumpSystem.SonyPlayStation3:
                    case SabreTools.RedumpLib.Data.RedumpSystem.SonyPlayStation4:
                    case SabreTools.RedumpLib.Data.RedumpSystem.SonyPlayStation5:
                    case SabreTools.RedumpLib.Data.RedumpSystem.NintendoWiiU:
                        break;

                    default:
                        // Enable skeleton for CD and DVD only, by default
                        switch (MediaType)
                        {
                            case SabreTools.RedumpLib.Data.MediaType.CDROM:
                            case SabreTools.RedumpLib.Data.MediaType.DVD:
                                {
                                    this[FlagStrings.Skeleton] = true;
                                    (_inputs[FlagStrings.Skeleton] as FlagInput)?.SetValue(true);
                                }
                                break;

                            // If the type is unknown, also enable
                            case null:
                                this[FlagStrings.Skeleton] = true;
                                (_inputs[FlagStrings.Skeleton] as FlagInput)?.SetValue(true);
                                break;

                            default:
                                break;
                        }
                        break;
                }
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

            string? driveType = GetStringSetting(options, SettingConstants.DriveType, SettingConstants.DriveTypeDefault);
            if (!string.IsNullOrEmpty(driveType) && driveType != DriveType.NONE.ToString())
            {
                this[FlagStrings.DriveType] = true;
                (_inputs[FlagStrings.DriveType] as StringInput)?.SetValue(driveType!);
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

            int retries = GetInt32Setting(options, SettingConstants.RereadCount, SettingConstants.RereadCountDefault);
            if (retries > 0)
            {
                this[FlagStrings.Retries] = true;
                (_inputs[FlagStrings.Retries] as Int32Input)?.SetValue(retries);
            }

            int leadinRetries = GetInt32Setting(options, SettingConstants.LeadinRetryCount, SettingConstants.LeadinRetryCountDefault);
            if (leadinRetries != SettingConstants.LeadinRetryCountDefault)
            {
                this[FlagStrings.PlextorLeadinRetries] = true;
                (_inputs[FlagStrings.PlextorLeadinRetries] as Int32Input)?.SetValue(leadinRetries);
            }

            if (GetBooleanSetting(options, SettingConstants.RefineSectorMode, SettingConstants.RefineSectorModeDefault))
            {
                this[FlagStrings.RefineSectorMode] = true;
                (_inputs[FlagStrings.RefineSectorMode] as FlagInput)?.SetValue(true);
            }
        }

        /// <inheritdoc/>
        protected override bool ValidateAndSetParameters(string? parameters)
        {
            // The string has to be valid by itself first
            if (string.IsNullOrEmpty(parameters))
                return false;

            // Now split the string into parts for easier validation
            string[] parts = SplitParameterString(parameters!);

            // Setup the modes
            BaseCommand = null;

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
                    case CommandStrings.FlashSD616:
                    case CommandStrings.FlashPlextor:
                    case CommandStrings.Subchannel:
                    case CommandStrings.Debug:
                    case CommandStrings.FixMSF:
                    case CommandStrings.DebugFlip:
                    case CommandStrings.DriveTest:
                        // Only allow one mode per command
                        if (BaseCommand != null)
                            continue;

                        BaseCommand = part;
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
