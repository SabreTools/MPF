using MPF.Frontend;
using SabreTools.CommandLine.Inputs;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;

namespace MPF.Check.Features
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

        internal const string _createIrdName = "create-ird";
        private readonly FlagInput _createIrdInput = new(_createIrdName, "--create-ird", "Create IRD from output files (PS3 only)");

        internal const string _deleteName = "delete";
        private readonly FlagInput _deleteInput = new(_deleteName, ["-d", "--delete"], "Enable unnecessary file deletion");

        internal const string _disableArchivesName = "disable-archives";
        private readonly FlagInput _disableArchivesInput = new(_disableArchivesName, "--disable-archives", "Disable scanning archives (requires --scan)");

        internal const string _enableDebugName = "enable-debug";
        private readonly FlagInput _enableDebugInput = new(_enableDebugName, "--enable-debug", "Enable debug protection information (requires --scan)");

        internal const string _hideDriveLettersName = "hide-drive-letters";
        private readonly FlagInput _hideDriveLettersInput = new(_hideDriveLettersName, "--hide-drive-letters", "Hide drive letters from scan output (requires --scan)");

        internal const string _includeArtifactsName = "include-artifacts";
        private readonly FlagInput _includeArtifactsInput = new(_includeArtifactsName, "--include-artifacts", "Include artifacts in JSON (requires --json)");

        internal const string _jsonName = "json";
        private readonly FlagInput _jsonInput = new(_jsonName, ["-j", "--json"], "Enable submission JSON output");

        internal const string _loadSeedName = "load-seed";
        private readonly StringInput _loadSeedInput = new(_loadSeedName, "--load-seed", "Load a seed submission JSON for user information");

        internal const string _noPlaceholdersName = "no-placeholders";
        private readonly FlagInput _noPlaceholdersInput = new(_noPlaceholdersName, "--no-placeholders", "Disable placeholder values in submission info");

        internal const string _noRetrieveName = "no-retrieve";
        private readonly FlagInput _noRetrieveInput = new(_noRetrieveName, "--no-retrieve", "Disable retrieving match information from Redump");

        internal const string _pathName = "path";
        private readonly StringInput _pathInput = new(_pathName, ["-p", "--path"], "Physical drive path for additional checks");

        internal const string _pullAllName = "pull-all";
        private readonly FlagInput _pullAllInput = new(_pullAllName, "--pull-all", "Pull all information from Redump (requires --credentials)");

        internal const string _scanName = "scan";
        private readonly FlagInput _scanInput = new(_scanName, ["-s", "--scan"], "Enable copy protection scan (requires --path)");

        internal const string _suffixName = "suffix";
        private readonly FlagInput _suffixInput = new(_suffixName, ["-x", "--suffix"], "Enable adding filename suffix");

        internal const string _useName = "use";
        private readonly StringInput _useInput = new(_useName, ["-u", "--use"], "Override configured dumping program name");

        internal const string _zipName = "zip";
        private readonly FlagInput _zipInput = new(_zipName, ["-z", "--zip"], "Enable log file compression");

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
            Add(_loadSeedInput);
            Add(_noPlaceholdersInput);
            Add(_createIrdInput);
            Add(_noRetrieveInput);
            // TODO: Figure out how to work with the credentials input
            Add(_pullAllInput);
            Add(_pathInput);
            Add(_scanInput);
            Add(_disableArchivesInput);
            Add(_enableDebugInput);
            Add(_hideDriveLettersInput);
            Add(_suffixInput);
            Add(_jsonInput);
            Add(_includeArtifactsInput);
            Add(_zipInput);
            Add(_deleteInput);
        }

        /// <inheritdoc/>
        public override bool ProcessArgs(string[] args, int index)
        {
            // These values require multiple parts to be active
            bool scan = false,
                enableArchives = true,
                enableDebug = false,
                hideDriveLetters = false;

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

                // Include seed info file
                else if (_loadSeedInput.ProcessInput(args, ref index))
                    CommandOptions.Seed = Builder.CreateFromFile(_loadSeedInput.Value);

                // Disable placeholder values in submission info
                else if (_noPlaceholdersInput.ProcessInput(args, ref index))
                    Options.AddPlaceholders = false;

                // Create IRD from output files (PS3 only)
                else if (_createIrdInput.ProcessInput(args, ref index))
                    Options.CreateIRDAfterDumping = true;

                // Retrieve Redump match information
                else if (_noRetrieveInput.ProcessInput(args, ref index))
                    Options.RetrieveMatchInformation = false;

                // Redump login
                else if (args[index].StartsWith("-c=") || args[index].StartsWith("--credentials="))
                {
                    string[] credentials = args[index].Split('=')[1].Split(';');
                    Options.RedumpUsername = credentials[0];
                    Options.RedumpPassword = credentials[1];
                }
                else if (args[index] == "-c" || args[index] == "--credentials")
                {
                    Options.RedumpUsername = args[index + 1];
                    Options.RedumpPassword = args[index + 2];
                    index += 2;
                }

                // Pull all information (requires Redump login)
                else if (_pullAllInput.ProcessInput(args, ref index))
                    Options.PullAllInformation = true;

                // Use a device path for physical checks
                else if (_pathInput.ProcessInput(args, ref index))
                    CommandOptions.DevicePath = _pathInput.Value;

                // Scan for protection (requires device path)
                else if (_scanInput.ProcessInput(args, ref index))
                    scan = true;

                // Disable scanning archives (requires --scan)
                else if (_scanInput.ProcessInput(args, ref index))
                    enableArchives = false;

                // Enable debug protection information (requires --scan)
                else if (_enableDebugInput.ProcessInput(args, ref index))
                    enableDebug = true;

                // Hide drive letters from scan output (requires --scan)
                else if (_hideDriveLettersInput.ProcessInput(args, ref index))
                    hideDriveLetters = true;

                // Add filename suffix
                else if (_suffixInput.ProcessInput(args, ref index))
                    Options.AddFilenameSuffix = true;

                // Output submission JSON
                else if (_jsonInput.ProcessInput(args, ref index))
                    Options.OutputSubmissionJSON = true;

                // Include JSON artifacts
                else if (_includeArtifactsInput.ProcessInput(args, ref index))
                    Options.IncludeArtifacts = true;

                // Compress log and extraneous files
                else if (_zipInput.ProcessInput(args, ref index))
                    Options.CompressLogFiles = true;

                // Delete unnecessary files
                else if (_deleteInput.ProcessInput(args, ref index))
                    Options.DeleteUnnecessaryFiles = true;

                // Default, add to inputs
                else
                    Inputs.Add(args[index]);
            }

            // Now deal with the complex options
            Options.ScanForProtection = scan && !string.IsNullOrEmpty(CommandOptions.DevicePath);
            Options.ScanArchivesForProtection = enableArchives && scan && !string.IsNullOrEmpty(CommandOptions.DevicePath);
            Options.IncludeDebugProtectionInformation = enableDebug && scan && !string.IsNullOrEmpty(CommandOptions.DevicePath);
            Options.HideDriveLetters = hideDriveLetters && scan && !string.IsNullOrEmpty(CommandOptions.DevicePath);

            return true;
        }

        /// <inheritdoc/>
        public override bool VerifyInputs() => Inputs.Count > 0;
    }
}
