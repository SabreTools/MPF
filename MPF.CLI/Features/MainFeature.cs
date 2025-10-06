
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
        private const string _description = "";

        #endregion

        #region Inputs

        private const string _customName = "custom";
        internal readonly StringInput CustomInput = new(_customName, ["-c", "--custom"], "Custom parameters to use");

        private const string _deviceName = "device";
        internal readonly StringInput DeviceInput = new(_deviceName, ["-d", "--device"], "Physical drive path (Required if no custom parameters set)");

        private const string _fileName = "file";
        internal readonly StringInput FileInput = new(_fileName, ["-f", "--file"], "Output file path (Required if no custom parameters set)");

        private const string _mediaTypeName = "media-type";
        internal readonly StringInput MediaTypeInput = new(_mediaTypeName, ["-t", "--mediatype"], "Set media type for dumping (Required for DIC)");

        private const string _mountedName = "mounted";
        internal readonly StringInput MountedInput = new(_mountedName, ["-m", "--mounted"], "Mounted filesystem path for additional checks");

        private const string _speedName = "speed";
        internal readonly Int32Input SpeedInput = new(_speedName, ["-s", "--speed"], "Override default dumping speed");

        private const string _useName = "use";
        internal readonly StringInput UseInput = new(_useName, ["-u", "--use"], "Override configured dumping program name");

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

            Add(UseInput);
            Add(MediaTypeInput);
            Add(DeviceInput);
            Add(MountedInput);
            Add(FileInput);
            Add(SpeedInput);
            Add(CustomInput);
        }

        /// <inheritdoc/>
        public override bool ProcessArgs(string[] args, int index)
        {
            // If we have no arguments, just return
            if (args == null || args.Length == 0)
                return true;

            // The first argument is the system type
            System = args[0].Trim('"').ToRedumpSystem();

            // Loop through the arguments and parse out values
            for (index = 1; index < args.Length; index++)
            {
                // Use specific program
                if (UseInput.ProcessInput(args, ref index))
                    Options.InternalProgram = UseInput.Value.ToInternalProgram();

                // Set a media type
                else if (MediaTypeInput.ProcessInput(args, ref index))
                    CommandOptions.MediaType = OptionsLoader.ToMediaType(MediaTypeInput.Value?.Trim('"'));

                // Use a device path
                else if (DeviceInput.ProcessInput(args, ref index))
                    CommandOptions.DevicePath = DeviceInput.Value;

                // Use a mounted path for physical checks
                else if (MountedInput.ProcessInput(args, ref index))
                    CommandOptions.MountedPath = MountedInput.Value;

                // Use a file path
                else if (FileInput.ProcessInput(args, ref index))
                    CommandOptions.FilePath = FileInput.Value;

                // Set an override speed
                else if (SpeedInput.ProcessInput(args, ref index))
                    CommandOptions.DriveSpeed = SpeedInput.Value;

                // Use a custom parameters
                else if (CustomInput.ProcessInput(args, ref index))
                    CommandOptions.CustomParams = CustomInput.Value;

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
