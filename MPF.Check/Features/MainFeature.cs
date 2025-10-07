using MPF.Frontend;
using MPF.Frontend.Tools;
using SabreTools.CommandLine.Inputs;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;
using LogCompression = MPF.Processors.LogCompression;

namespace MPF.Check.Features
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

        private const string _createIrdName = "create-ird";
        internal readonly FlagInput CreateIrdInput = new(_createIrdName, "--create-ird", "Create IRD from output files (PS3 only)");

        private const string _deleteName = "delete";
        internal readonly FlagInput DeleteInput = new(_deleteName, ["-d", "--delete"], "Enable unnecessary file deletion");

        private const string _disableArchivesName = "disable-archives";
        internal readonly FlagInput DisableArchivesInput = new(_disableArchivesName, "--disable-archives", "Disable scanning archives (requires --scan)");

        private const string _enableDebugName = "enable-debug";
        internal readonly FlagInput EnableDebugInput = new(_enableDebugName, "--enable-debug", "Enable debug protection information (requires --scan)");

        private const string _hideDriveLettersName = "hide-drive-letters";
        internal readonly FlagInput HideDriveLettersInput = new(_hideDriveLettersName, "--hide-drive-letters", "Hide drive letters from scan output (requires --scan)");

        private const string _includeArtifactsName = "include-artifacts";
        internal readonly FlagInput IncludeArtifactsInput = new(_includeArtifactsName, "--include-artifacts", "Include artifacts in JSON (requires --json)");

        private const string _jsonName = "json";
        internal readonly FlagInput JsonInput = new(_jsonName, ["-j", "--json"], "Enable submission JSON output");

        private const string _loadSeedName = "load-seed";
        internal readonly StringInput LoadSeedInput = new(_loadSeedName, "--load-seed", "Load a seed submission JSON for user information");

        private const string _logCompressionName = "log-compression";
        internal readonly StringInput LogCompressionInput = new(_logCompressionName, "--log-compression", "Set the log compression type (requires --zip)");

        private const string _noPlaceholdersName = "no-placeholders";
        internal readonly FlagInput NoPlaceholdersInput = new(_noPlaceholdersName, "--no-placeholders", "Disable placeholder values in submission info");

        private const string _noRetrieveName = "no-retrieve";
        internal readonly FlagInput NoRetrieveInput = new(_noRetrieveName, "--no-retrieve", "Disable retrieving match information from Redump");

        private const string _pathName = "path";
        internal readonly StringInput PathInput = new(_pathName, ["-p", "--path"], "Physical drive path for additional checks");

        private const string _pullAllName = "pull-all";
        internal readonly FlagInput PullAllInput = new(_pullAllName, "--pull-all", "Pull all information from Redump (requires --credentials)");

        private const string _scanName = "scan";
        internal readonly FlagInput ScanInput = new(_scanName, ["-s", "--scan"], "Enable copy protection scan (requires --path)");

        private const string _suffixName = "suffix";
        internal readonly FlagInput SuffixInput = new(_suffixName, ["-x", "--suffix"], "Enable adding filename suffix");

        private const string _useName = "use";
        internal readonly StringInput UseInput = new(_useName, ["-u", "--use"], "Override configured dumping program name");

        private const string _zipName = "zip";
        internal readonly FlagInput ZipInput = new(_zipName, ["-z", "--zip"], "Enable log file compression");

        #endregion

        public MainFeature()
            : base(DisplayName, _flags, _description)
        {
            Add(UseInput);
            Add(LoadSeedInput);
            Add(NoPlaceholdersInput);
            Add(CreateIrdInput);
            Add(NoRetrieveInput);
            // TODO: Figure out how to work with the credentials input
            Add(PullAllInput);
            Add(PathInput);
            Add(ScanInput);
            Add(DisableArchivesInput);
            Add(EnableDebugInput);
            Add(HideDriveLettersInput);
            Add(SuffixInput);
            Add(JsonInput);
            Add(IncludeArtifactsInput);
            Add(ZipInput);
            Add(LogCompressionInput);
            Add(DeleteInput);
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

            // Read the options from config, if possible
            Options = OptionsLoader.LoadFromConfig();
            if (Options.FirstRun)
            {
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
                    LogCompression = LogCompression.DeflateMaximum,
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
            }

            // The first argument is the system type
            System = args[0].Trim('"').ToRedumpSystem();

            // Loop through the arguments and parse out values
            for (index = 1; index < args.Length; index++)
            {
                // Use specific program
                if (UseInput.ProcessInput(args, ref index))
                    Options.InternalProgram = UseInput.Value.ToInternalProgram();

                // Include seed info file
                else if (LoadSeedInput.ProcessInput(args, ref index))
                    Seed = Builder.CreateFromFile(LoadSeedInput.Value);

                // Disable placeholder values in submission info
                else if (NoPlaceholdersInput.ProcessInput(args, ref index))
                    Options.AddPlaceholders = false;

                // Create IRD from output files (PS3 only)
                else if (CreateIrdInput.ProcessInput(args, ref index))
                    Options.CreateIRDAfterDumping = true;

                // Set the log compression type (requires --zip)
                else if (LogCompressionInput.ProcessInput(args, ref index))
                    Options.LogCompression = LogCompressionInput.Value.ToLogCompression();

                // Retrieve Redump match information
                else if (NoRetrieveInput.ProcessInput(args, ref index))
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
                else if (PullAllInput.ProcessInput(args, ref index))
                    Options.PullAllInformation = true;

                // Use a device path for physical checks
                else if (PathInput.ProcessInput(args, ref index))
                    DevicePath = PathInput.Value;

                // Scan for protection (requires device path)
                else if (ScanInput.ProcessInput(args, ref index))
                    scan = true;

                // Disable scanning archives (requires --scan)
                else if (ScanInput.ProcessInput(args, ref index))
                    enableArchives = false;

                // Enable debug protection information (requires --scan)
                else if (EnableDebugInput.ProcessInput(args, ref index))
                    enableDebug = true;

                // Hide drive letters from scan output (requires --scan)
                else if (HideDriveLettersInput.ProcessInput(args, ref index))
                    hideDriveLetters = true;

                // Add filename suffix
                else if (SuffixInput.ProcessInput(args, ref index))
                    Options.AddFilenameSuffix = true;

                // Output submission JSON
                else if (JsonInput.ProcessInput(args, ref index))
                    Options.OutputSubmissionJSON = true;

                // Include JSON artifacts
                else if (IncludeArtifactsInput.ProcessInput(args, ref index))
                    Options.IncludeArtifacts = true;

                // Compress log and extraneous files
                else if (ZipInput.ProcessInput(args, ref index))
                    Options.CompressLogFiles = true;

                // Delete unnecessary files
                else if (DeleteInput.ProcessInput(args, ref index))
                    Options.DeleteUnnecessaryFiles = true;

                // Default, add to inputs
                else
                    Inputs.Add(args[index]);
            }

            // Now deal with the complex options
            Options.ScanForProtection = scan && !string.IsNullOrEmpty(DevicePath);
            Options.ScanArchivesForProtection = enableArchives && scan && !string.IsNullOrEmpty(DevicePath);
            Options.IncludeDebugProtectionInformation = enableDebug && scan && !string.IsNullOrEmpty(DevicePath);
            Options.HideDriveLetters = hideDriveLetters && scan && !string.IsNullOrEmpty(DevicePath);

            return true;
        }

        /// <inheritdoc/>
        public override bool VerifyInputs() => Inputs.Count > 0;
    }
}
