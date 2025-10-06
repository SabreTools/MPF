
using MPF.Frontend;
using MPF.Frontend.Tools;
using SabreTools.CommandLine.Inputs;
using SabreTools.RedumpLib.Data;

namespace MPF.CLI.Features
{
    internal sealed class MainFeature : BaseFeature
    {
        #region Feature Definition

        public const string DisplayName = "main";

        /// <remarks>Flags are unused</remarks>
        private static readonly string[] _flags = [];

        /// <remarks>Description is unused</remarks>
        internal const string _description = "";

        #endregion

        #region Inputs

        private const string _customName = "custom";
        private static readonly StringInput _customInput = new(_customName, ["-c", "--custom"], "Custom parameters to use");

        private const string _deviceName = "device";
        private static readonly StringInput _deviceInput = new(_deviceName, ["-d", "--device"], "Physical drive path (Required if no custom parameters set)");

        private const string _fileName = "file";
        private static readonly StringInput _fileInput = new(_fileName, ["-f", "--file"], "Output file path (Required if no custom parameters set)");

        private const string _mediaTypeName = "media-type";
        private static readonly StringInput _mediaTypeInput = new(_mediaTypeName, ["-t", "--mediatype"], "Set media type for dumping (Required for DIC)");

        private const string _mountedName = "mounted";
        private static readonly StringInput _mountedInput = new(_mountedName, ["-m", "--mounted"], "Mounted filesystem path for additional checks");

        private const string _speedName = "speed";
        private static readonly Int32Input _speedInput = new(_speedName, ["-s", "--speed"], "Override default dumping speed");

        private const string _useName = "use";
        private static readonly StringInput _useInput = new(_useName, ["-u", "--use"], "Override configured dumping program name");

        #endregion

        public MainFeature()
            : base(DisplayName, _flags, _description)
        {
            CommandOptions = new Program.CommandOptions();
            Options = new Options()
            {
                // Internal Program
                InternalProgram = InternalProgram.NONE,

                // Extra Dumping Options
                ScanForProtection = false,
                AddPlaceholders = true,
                PullAllInformation = false,
                AddFilenameSuffix = false,
                OutputSubmissionJSON = false,
                IncludeArtifacts = false,
                CompressLogFiles = false,
                DeleteUnnecessaryFiles = false,
                CreateIRDAfterDumping = false,

                // Protection Scanning Options
                ScanArchivesForProtection = true,
                IncludeDebugProtectionInformation = false,
                HideDriveLetters = false,

                // Redump Login Information
                RetrieveMatchInformation = true,
                RedumpUsername = null,
                RedumpPassword = null,
            };

            Add(_useInput);
            Add(_mediaTypeInput);
            Add(_deviceInput);
            Add(_mountedInput);
            Add(_fileInput);
            Add(_speedInput);
            Add(_customInput);
        }

        /// <inheritdoc/>
        public override bool ProcessArgs(string[] args, int index)
        {
            // If we have no arguments, just return
            if (args == null || args.Length == 0)
                return true;

            // The first argument is the system type
            System = Extensions.ToRedumpSystem(args[0].Trim('"'));

            // Loop through the arguments and parse out values
            for (index = 1; index < args.Length; index++)
            {
                // Use specific program
                if (_useInput.ProcessInput(args, ref index))
                    Options.InternalProgram = _useInput.Value.ToInternalProgram();

                // Set a media type
                else if (_mediaTypeInput.ProcessInput(args, ref index))
                    CommandOptions.MediaType = OptionsLoader.ToMediaType(_mediaTypeInput.Value?.Trim('"'));

                // Use a device path
                else if (_deviceInput.ProcessInput(args, ref index))
                    CommandOptions.DevicePath = _deviceInput.Value;

                // Use a mounted path for physical checks
                else if (_mountedInput.ProcessInput(args, ref index))
                    CommandOptions.MountedPath = _mountedInput.Value;

                // Use a file path
                else if (_fileInput.ProcessInput(args, ref index))
                    CommandOptions.FilePath = _fileInput.Value;

                // Set an override speed
                else if (_speedInput.ProcessInput(args, ref index))
                    CommandOptions.DriveSpeed = _speedInput.Value;

                // Use a custom parameters
                else if (_customInput.ProcessInput(args, ref index))
                    CommandOptions.CustomParams = _customInput.Value;

                // Default, add to inputs
                else
                    Inputs.Add(args[index]);
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool VerifyInputs() => true;
    }
}
