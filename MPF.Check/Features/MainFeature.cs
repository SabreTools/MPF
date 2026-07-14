using System;
using MPF.Frontend;
using MPF.Frontend.Tools;
using SabreTools.CommandLine.Inputs;
using SabreTools.RedumpLib.Data;
using SabreTools.RedumpLib.Tools;
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

        private const string _archivesName = "archives";
        internal readonly FlagInput ArchivesInput = new(_archivesName, "--archives", "Enable scanning archives (requires --scan)");

        private const string _artifactsName = "artifacts";
        internal readonly FlagInput ArtifactsInput = new(_artifactsName, "--artifacts", "Include artifacts in JSON (requires --json)");

        private const string _attemptCountName = "attempt-count";
        internal readonly Int32Input AttemptCountInput = new(_attemptCountName, "--attempt-count", "Set web client attempt count (must be greater than 0)");

        private const string _createIrdName = "create-ird";
        internal readonly FlagInput CreateIrdInput = new(_createIrdName, "--create-ird", "Create IRD from output files (PS3 only)");

        private const string _deleteName = "delete";
        internal readonly FlagInput DeleteInput = new(_deleteName, ["-d", "--delete"], "Enable unnecessary file deletion");

        private const string _debugProtectionName = "debug-protection";
        internal readonly FlagInput DebugProtectionInput = new(_debugProtectionName, "--debug-protection", "Enable debug protection information (requires --scan)");

        private const string _hideDriveLettersName = "hide-drive-letters";
        internal readonly FlagInput HideDriveLettersInput = new(_hideDriveLettersName, "--hide-drive-letters", "Hide drive letters from scan output (requires --scan)");

        private const string _jsonName = "json";
        internal readonly FlagInput JsonInput = new(_jsonName, ["-j", "--json"], "Enable submission JSON output");

        private const string _loadSeedName = "load-seed";
        internal readonly StringInput LoadSeedInput = new(_loadSeedName, "--load-seed", "Load a seed submission JSON for user information");

        private const string _logCompressionName = "log-compression";
        internal readonly StringInput LogCompressionInput = new(_logCompressionName, "--log-compression", "Set the log compression type (requires compression enabled)");

        private const string _noArchivesName = "no-archives";
        internal readonly FlagInput NoArchivesInput = new(_noArchivesName, "--no-archives", "Disable scanning archives (requires --scan)");

        private const string _noArtifactsName = "no-artifacts";
        internal readonly FlagInput NoArtifactsInput = new(_noArtifactsName, "--no-artifacts", "Omit artifacts in JSON (requires --json)");

        private const string _noCreateIrdName = "no-create-ird";
        internal readonly FlagInput NoCreateIrdInput = new(_noCreateIrdName, "--no-create-ird", "Disable create IRD from output files (PS3 only)");

        private const string _noDebugProtectionName = "no-debug-protection";
        internal readonly FlagInput NoDebugProtectionInput = new(_noDebugProtectionName, "--no-debug-protection", "Disable debug protection information (requires --scan)");

        private const string _noDeleteName = "no-delete";
        internal readonly FlagInput NoDeleteInput = new(_noDeleteName, ["--no-delete"], "Disable unnecessary file deletion");

        private const string _noJsonName = "no-json";
        internal readonly FlagInput NoJsonInput = new(_noJsonName, ["--no-json"], "Disable submission JSON output");

        private const string _noPlaceholdersName = "no-placeholders";
        internal readonly FlagInput NoPlaceholdersInput = new(_noPlaceholdersName, "--no-placeholders", "Disable placeholder values in submission info");

        private const string _noPullAllName = "no-pull-all";
        internal readonly FlagInput NoPullAllInput = new(_noPullAllName, "--no-pull-all", "Do not pull all information from online sources");

        private const string _noRetrieveName = "no-retrieve";
        internal readonly FlagInput NoRetrieveInput = new(_noRetrieveName, "--no-retrieve", "Disable retrieving match information from online sources");

        private const string _noScanName = "no-scan";
        internal readonly FlagInput NoScanInput = new(_noScanName, ["--no-scan"], "Disable copy protection scan");

        private const string _noSuffixName = "no-suffix";
        internal readonly FlagInput NoSuffixInput = new(_noSuffixName, ["--no-suffix"], "Disable adding filename suffix");

        private const string _noZipName = "no-zip";
        internal readonly FlagInput NoZipInput = new(_noZipName, ["--no-zip"], "Disable log file compression");

        private const string _pathName = "path";
        internal readonly StringInput PathInput = new(_pathName, ["-p", "--path"], "Physical drive path for additional checks");

        private const string _placeholdersName = "placeholders";
        internal readonly FlagInput PlaceholdersInput = new(_placeholdersName, "--placeholders", "Enable placeholder values in submission info");

        private const string _pullAllName = "pull-all";
        internal readonly FlagInput PullAllInput = new(_pullAllName, "--pull-all", "Pull all information from online sources");

        private const string _retrieveName = "retrieve";
        internal readonly FlagInput RetrieveInput = new(_retrieveName, "--retrieve", "Enable retrieving match information from online sources");

        private const string _scanName = "scan";
        internal readonly FlagInput ScanInput = new(_scanName, ["-s", "--scan"], "Enable copy protection scan (requires --path)");

        private const string _showDriveLettersName = "show-drive-letters";
        internal readonly FlagInput ShowDriveLettersInput = new(_showDriveLettersName, "--show-drive-letters", "Show drive letters in scan output (requires --scan)");

        private const string _suffixName = "suffix";
        internal readonly FlagInput SuffixInput = new(_suffixName, ["-x", "--suffix"], "Enable adding filename suffix");

        private const string _timeoutSecondsName = "timeout-seconds";
        internal readonly Int32Input TimeoutSecondsInput = new(_timeoutSecondsName, "--timeout-seconds", "Set web client timeout in seconds (must be greater than 0)");

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
            Add(PlaceholdersInput);
            Add(NoPlaceholdersInput);
            Add(CreateIrdInput);
            Add(NoCreateIrdInput);
            Add(RetrieveInput);
            Add(NoRetrieveInput);
            Add(AttemptCountInput);
            Add(TimeoutSecondsInput);
            Add(PullAllInput);
            Add(NoPullAllInput);
            Add(PathInput);
            Add(ScanInput);
            Add(NoScanInput);
            Add(ArchivesInput);
            Add(NoArchivesInput);
            Add(DebugProtectionInput);
            Add(NoDebugProtectionInput);
            Add(ShowDriveLettersInput);
            Add(HideDriveLettersInput);
            Add(SuffixInput);
            Add(NoSuffixInput);
            Add(JsonInput);
            Add(NoJsonInput);
            Add(ArtifactsInput);
            Add(NoArtifactsInput);
            Add(ZipInput);
            Add(NoZipInput);
            Add(LogCompressionInput);
            Add(DeleteInput);
            Add(NoDeleteInput);
        }

        /// <inheritdoc/>
        public override bool ProcessArgs(string[] args, int index)
        {
            // If we have no arguments, just return
            if (args is null || args.Length == 0)
                return true;

            // Read the options from config, if possible
            Options = OptionsLoader.LoadFromConfig(out string? configPath);

            // Log the configuration path
            Console.WriteLine($"Configuration path: {configPath}");
            Console.WriteLine();

            // Determine which options to set
            if (Options.FirstRun)
            {
                Options = new Options();

                // Internal Program
                Options.InternalProgram = InternalProgram.NONE;

                // Protection Scanning Options
                Options.Processing.ProtectionScanning.ScanForProtection = false;
                Options.Processing.ProtectionScanning.ScanArchivesForProtection = true;
                Options.Processing.ProtectionScanning.IncludeDebugProtectionInformation = false;
                Options.Processing.ProtectionScanning.HideDriveLetters = false;

                // Web Login Information
                Options.Processing.Login.PullAllInformation = false;
                Options.Processing.Login.RetrieveMatchInformation = true;
                Options.Processing.Login.AttemptCount = 3;
                Options.Processing.Login.TimeoutSeconds = 30;

                // Media Information
                Options.Processing.MediaInformation.AddPlaceholders = true;

                // Post-Information Options
                Options.Processing.AddFilenameSuffix = false;
                Options.Processing.CreateIRDAfterDumping = false;
                Options.Processing.OutputSubmissionJSON = false;
                Options.Processing.IncludeArtifacts = false;
                Options.Processing.CompressLogFiles = false;
                Options.Processing.LogCompression = LogCompression.DeflateMaximum;
                Options.Processing.DeleteUnnecessaryFiles = false;
            }
            else
            {
                Console.WriteLine("Options will be loaded from found configuration file!");
            }

            // The first argument is the system type
            System = args[0].Trim('"').ToPhysicalSystem();

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
                else if (PlaceholdersInput.ProcessInput(args, ref index))
                    Options.Processing.MediaInformation.AddPlaceholders = true;
                else if (NoPlaceholdersInput.ProcessInput(args, ref index))
                    Options.Processing.MediaInformation.AddPlaceholders = false;

                // Create IRD from output files (PS3 only)
                else if (CreateIrdInput.ProcessInput(args, ref index))
                    Options.Processing.CreateIRDAfterDumping = true;
                else if (NoCreateIrdInput.ProcessInput(args, ref index))
                    Options.Processing.CreateIRDAfterDumping = false;

                // Set the log compression type (requires compression enabled)
                else if (LogCompressionInput.ProcessInput(args, ref index))
                    Options.Processing.LogCompression = LogCompressionInput.Value.ToLogCompression();

                // Retrieve online match information
                else if (RetrieveInput.ProcessInput(args, ref index))
                    Options.Processing.Login.RetrieveMatchInformation = true;
                else if (NoRetrieveInput.ProcessInput(args, ref index))
                    Options.Processing.Login.RetrieveMatchInformation = false;

                // Attempt count
                else if (AttemptCountInput.ProcessInput(args, ref index) && AttemptCountInput.Value > 0)
                    Options.Processing.Login.AttemptCount = AttemptCountInput.Value ?? 3;

                // Timeout seconds
                else if (TimeoutSecondsInput.ProcessInput(args, ref index) && TimeoutSecondsInput.Value > 0)
                    Options.Processing.Login.TimeoutSeconds = TimeoutSecondsInput.Value ?? 30;

                // Pull all information
                else if (PullAllInput.ProcessInput(args, ref index))
                    Options.Processing.Login.PullAllInformation = true;
                else if (NoPullAllInput.ProcessInput(args, ref index))
                    Options.Processing.Login.PullAllInformation = false;

                // Use a device path for physical checks
                else if (PathInput.ProcessInput(args, ref index))
                    DevicePath = PathInput.Value;

                // Scan for protection (requires device path)
                else if (ScanInput.ProcessInput(args, ref index))
                    Options.Processing.ProtectionScanning.ScanForProtection = true;
                else if (NoScanInput.ProcessInput(args, ref index))
                    Options.Processing.ProtectionScanning.ScanForProtection = false;

                // Enable scanning archives (requires --scan)
                else if (ArchivesInput.ProcessInput(args, ref index))
                    Options.Processing.ProtectionScanning.ScanArchivesForProtection = true;
                else if (NoArchivesInput.ProcessInput(args, ref index))
                    Options.Processing.ProtectionScanning.ScanArchivesForProtection = false;

                // Enable debug protection information (requires --scan)
                else if (DebugProtectionInput.ProcessInput(args, ref index))
                    Options.Processing.ProtectionScanning.IncludeDebugProtectionInformation = true;
                else if (NoDebugProtectionInput.ProcessInput(args, ref index))
                    Options.Processing.ProtectionScanning.IncludeDebugProtectionInformation = false;

                // Show drive letters in scan output (requires --scan)
                else if (ShowDriveLettersInput.ProcessInput(args, ref index))
                    Options.Processing.ProtectionScanning.HideDriveLetters = false;
                else if (HideDriveLettersInput.ProcessInput(args, ref index))
                    Options.Processing.ProtectionScanning.HideDriveLetters = true;

                // Add filename suffix
                else if (SuffixInput.ProcessInput(args, ref index))
                    Options.Processing.AddFilenameSuffix = true;
                else if (NoSuffixInput.ProcessInput(args, ref index))
                    Options.Processing.AddFilenameSuffix = false;

                // Output submission JSON
                else if (JsonInput.ProcessInput(args, ref index))
                    Options.Processing.OutputSubmissionJSON = true;
                else if (NoJsonInput.ProcessInput(args, ref index))
                    Options.Processing.OutputSubmissionJSON = false;

                // Include JSON artifacts
                else if (ArtifactsInput.ProcessInput(args, ref index))
                    Options.Processing.IncludeArtifacts = true;
                else if (NoArtifactsInput.ProcessInput(args, ref index))
                    Options.Processing.IncludeArtifacts = false;

                // Compress log and extraneous files
                else if (ZipInput.ProcessInput(args, ref index))
                    Options.Processing.CompressLogFiles = true;
                else if (NoZipInput.ProcessInput(args, ref index))
                    Options.Processing.CompressLogFiles = false;

                // Delete unnecessary files
                else if (DeleteInput.ProcessInput(args, ref index))
                    Options.Processing.DeleteUnnecessaryFiles = true;
                else if (NoDeleteInput.ProcessInput(args, ref index))
                    Options.Processing.DeleteUnnecessaryFiles = false;

                // Default, add to inputs
                else
                    Inputs.Add(args[index]);
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool VerifyInputs() => Inputs.Count > 0;
    }
}
