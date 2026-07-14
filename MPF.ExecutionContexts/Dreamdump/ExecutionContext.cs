using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MPF.ExecutionContexts.Data;
using SabreTools.RedumpLib.Data;

namespace MPF.ExecutionContexts.Dreamdump
{
    /// <summary>
    /// Represents a generic set of Dreamdump parameters
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
            + GetDefaultExtension(PhysicalMediaType);

        /// <inheritdoc/>
        public override int? Speed
        {
            get
            {
                return (_inputs[FlagStrings.Speed] as Int32Input)?.Value;
            }
            set
            {
                if (value is not null && value > 0)
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
            [FlagStrings.Drive] = new StringInput(FlagStrings.Drive),
            [FlagStrings.SectorOrder] = new StringInput(FlagStrings.SectorOrder),
            [FlagStrings.Cutoff] = new Int32Input(FlagStrings.Cutoff),
            [FlagStrings.ReadOffset] = new Int16Input(FlagStrings.ReadOffset),
            [FlagStrings.Speed] = new UInt16Input(FlagStrings.Speed),
            [FlagStrings.ImageName] = new StringInput(FlagStrings.ImageName) { Quotes = true },
            [FlagStrings.ImagePath] = new StringInput(FlagStrings.ImagePath) { Quotes = true },
            [FlagStrings.ForceQTOC] = new FlagInput(FlagStrings.ForceQTOC),
            [FlagStrings.Train] = new FlagInput(FlagStrings.Train),
            [FlagStrings.ForceSectorOrder] = new FlagInput(FlagStrings.ForceSectorOrder),
            [FlagStrings.ReadAtOnce] = new UInt8Input(FlagStrings.ReadAtOnce),
            [FlagStrings.Retries] = new UInt8Input(FlagStrings.Retries),
        };

        #endregion

        /// <inheritdoc/>
        public ExecutionContext(string? parameters) : base(parameters) { }

        /// <inheritdoc/>
        public ExecutionContext(PhysicalSystem? system,
            PhysicalMediaType? type,
            string? drivePath,
            string filename,
            int? driveSpeed,
            BaseDumpSettings baseDumpSettings)
            : base(system, type, drivePath, filename, driveSpeed, baseDumpSettings)
        {
        }

        #region BaseExecutionContext Implementations

        /// <inheritdoc/>
        /// <remarks>Command support is irrelevant for Dreamdump</remarks>
        public override Dictionary<string, List<string>> GetCommandSupport()
        {
            return new Dictionary<string, List<string>>()
            {
                [CommandStrings.NONE] =
                [
                    FlagStrings.Drive,
                    FlagStrings.SectorOrder,
                    FlagStrings.Cutoff,
                    FlagStrings.ReadOffset,
                    FlagStrings.Speed,
                    FlagStrings.ImageName,
                    FlagStrings.ImagePath,
                    FlagStrings.ForceQTOC,
                    FlagStrings.Train,
                    FlagStrings.ForceSectorOrder,
                    FlagStrings.ReadAtOnce,
                    FlagStrings.Retries,
                ],
            };
        }

        /// <inheritdoc/>
        public override string GenerateParameters()
        {
            var parameters = new StringBuilder();

            // Command Mode
            BaseCommand ??= CommandStrings.Disc;
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
        public override string? GetDefaultExtension(PhysicalMediaType? mediaType) => ".bin";

        /// <inheritdoc/>
        public override PhysicalMediaType? GetPhysicalMediaType() => SabreTools.RedumpLib.Data.PhysicalMediaType.GDROM;

        /// <inheritdoc/>
        public override bool IsDumpingCommand() => true;

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
            BaseDumpSettings baseDumpSettings)
        {
            // Get the base settings, if possible
            var dumpSettings = baseDumpSettings as DumpSettings ?? new DumpSettings();

            BaseCommand = CommandStrings.Disc;

            if (drivePath is not null)
            {
                this[FlagStrings.Drive] = true;
                (_inputs[FlagStrings.Drive] as StringInput)?.SetValue(drivePath);
            }

            if (driveSpeed is not null && driveSpeed > 0)
            {
                this[FlagStrings.Speed] = true;
                (_inputs[FlagStrings.Speed] as UInt16Input)?.SetValue((ushort)driveSpeed);
            }
            else
            {
                this[FlagStrings.Speed] = false;
                (_inputs[FlagStrings.Speed] as UInt16Input)?.SetValue(null);
            }

            // Set user-defined options
            if (dumpSettings.SectorOrder != SectorOrder.NONE)
            {
                this[FlagStrings.SectorOrder] = true;
                (_inputs[FlagStrings.SectorOrder] as StringInput)?.SetValue(dumpSettings.SectorOrder.ToString());
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

            if (dumpSettings.RereadCount > 0)
            {
                this[FlagStrings.Retries] = true;
                (_inputs[FlagStrings.Retries] as UInt8Input)?.SetValue((byte)dumpSettings.RereadCount);
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
                    case CommandStrings.Split:
                        // Only allow one mode per command
                        if (BaseCommand is not null)
                            continue;

                        BaseCommand = part;
                        break;

                    // Default is either a flag or an invalid mode
                    default:
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                        if (part.StartsWith('-'))
#else
                        if (part.StartsWith("-"))
#endif
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
                (_inputs[FlagStrings.ImageName] as StringInput)?.SetValue($"track_{DateTime.Now:yyyyMMdd-HHmm}");

            return true;
        }

        #endregion
    }
}
