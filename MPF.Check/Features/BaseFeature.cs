
using System;
using System.IO;
using System.Threading;
#if NET40
using System.Threading.Tasks;
#endif
using MPF.Frontend;
using SabreTools.RedumpLib.Data;
using LogCompression = MPF.Processors.LogCompression;

namespace MPF.Check.Features
{
    internal abstract class BaseFeature : SabreTools.CommandLine.Feature
    {
        #region Properties

        /// <summary>
        /// User-defined options
        /// </summary>
        public Options Options { get; protected set; }

        /// <summary>
        /// Currently-selected system
        /// </summary>
        public PhysicalSystem? System { get; protected set; }

        /// <summary>
        /// Seed submission info from an input file
        /// </summary>
        public SubmissionInfo? Seed { get; protected set; }

        /// <summary>
        /// Path to the device to scan
        /// </summary>
        public string? DevicePath { get; protected set; }

        /// <summary>
        /// Cancellation token source for cancelling a scan
        /// </summary>
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        #endregion

        protected BaseFeature(string name, string[] flags, string description, string? detailed = null)
            : base(name, flags, description, detailed)
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

            // Post-Information Options
            Options.Processing.AddFilenameSuffix = false;
            Options.Processing.CreateIRDAfterDumping = false;
            Options.Processing.OutputSubmissionJSON = false;
            Options.Processing.CompressLogFiles = false;
            Options.Processing.LogCompression = LogCompression.DeflateMaximum;
            Options.Processing.DeleteUnnecessaryFiles = false;
        }

        /// <inheritdoc/>
        public override bool Execute()
        {
            // Register handler for console cancellation
            Console.CancelKeyPress += CancelScan;

            // Validate a system type is provided
            if (System is null)
            {
                Console.Error.WriteLine("A system name needs to be provided");
                return false;
            }

            // Log the system being used, in case it came from config
            Console.WriteLine($"Using system: {System.Name}");

            // Validate a program is provided
            if (Options.InternalProgram == InternalProgram.NONE)
            {
                Console.Error.WriteLine("A program name needs to be provided");
                return false;
            }

            // Loop through all the rest of the args
            for (int i = 0; i < Inputs.Count; i++)
            {
                // Get the full file path
                string filepath = Path.GetFullPath(Inputs[i].Trim('"'));
                Console.WriteLine($"Processing input path: {filepath}");

                // Now populate an environment
                Drive? drive = null;
                if (!string.IsNullOrEmpty(DevicePath))
                {
                    drive = Drive.Create(null, DevicePath!);

                    // TODO: This should populate the correct thing by default
                    drive?.MountedPath = DevicePath;
                }

                var env = new DumpEnvironment(Options,
                    filepath,
                    drive,
                    System,
                    internalProgram: null);
                env.SetProcessor();

                // Finally, attempt to do the output dance
                var result = env.VerifyAndSaveDumpOutput(
                    processUserInfo: ProcessUserInfo,
                    seedInfo: Seed,
                    cancellationToken: _cancellationTokenSource.Token)
                        .ConfigureAwait(false).GetAwaiter().GetResult();
                Console.WriteLine(result.Message);
            }

            return true;
        }

        /// <summary>
        /// Display help for MPF.Check
        /// </summary>
        /// <param name="error">Error string to prefix the help text with</param>
        public static void DisplayHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("MPF.Check <system> [options] </path/to/output.cue|iso|_logs.zip> ...");
            Console.WriteLine();
            Console.WriteLine("Standalone Options:");
            Console.WriteLine("?, h, help              Show this help text");
            Console.WriteLine("man                     Write a roff-formatted man page for this program to standard output");
            Console.WriteLine("version                 Print the program version");
            Console.WriteLine("lc, listcodes           List supported comment/content site codes");
            Console.WriteLine("lo, listconfig          List current configuration values");
            Console.WriteLine("lpm, listpath           List supported path macros for configuration and input");
            Console.WriteLine("lm, listmedia           List supported media types");
            Console.WriteLine("lp, listprograms        List supported dumping program outputs");
            Console.WriteLine("ls, listsystems         List supported system types");
            Console.WriteLine("i, interactive          Enable interactive mode");
            Console.WriteLine();

            Console.WriteLine("Check Options:");
            Console.WriteLine("-u, --use <program>            Override configured dumping program name [REQUIRED]");
            Console.WriteLine("    --load-seed <path>         Load a seed submission JSON for user information");
            Console.WriteLine("    --create-ird               Create IRD from output files (PS3 only)");
            Console.WriteLine("    --no-create-ird            Disable create IRD from output files (PS3 only)");
            Console.WriteLine("    --retrieve                 Enable retrieving match information from online sources");
            Console.WriteLine("    --no-retrieve              Disable retrieving match information from online sources");
            Console.WriteLine("    --attempt-count            Set web client attempt count (must be greater than 0)");
            Console.WriteLine("    --timeout-seconds          Set web client timeout in seconds (must be greater than 0)");
            Console.WriteLine("    --pull-all                 Pull all information from online sources");
            Console.WriteLine("    --no-pull-all              Do not pull all information from online sources");
            Console.WriteLine("-p, --path <drivepath>         Mounted drive path for additional checks");
            Console.WriteLine("-s, --scan                     Enable copy protection scan (requires --path)");
            Console.WriteLine("    --no-scan                  Disable copy protection scan");
            Console.WriteLine("    --archives                 Enable scanning archives (requires --scan)");
            Console.WriteLine("    --no-archives              Disable scanning archives (requires --scan)");
            Console.WriteLine("    --debug-protection         Enable debug protection information (requires --scan)");
            Console.WriteLine("    --no-debug-protection      Disable debug protection information (requires --scan)");
            Console.WriteLine("    --show-drive-letters       Show drive letters in scan output (requires --scan)");
            Console.WriteLine("    --hide-drive-letters       Hide drive letters from scan output (requires --scan)");
            Console.WriteLine("-x, --suffix                   Enable adding filename suffix");
            Console.WriteLine("    --no-suffix                Disable adding filename suffix");
            Console.WriteLine("-j, --json                     Enable submission JSON output");
            Console.WriteLine("    --no-json                  Disable submission JSON output");
            Console.WriteLine("-z, --zip                      Enable log file compression");
            Console.WriteLine("    --no-zip                   Disable log file compression");
            Console.WriteLine("    --log-compression          Set the log compression type (requires compression enabled)");
            Console.WriteLine("-d, --delete                   Enable unnecessary file deletion");
            Console.WriteLine("    --no-delete                Disable unnecessary file deletion");
            Console.WriteLine();
            Console.WriteLine("WARNING: Check will overwrite both any existing submission information files as well");
            Console.WriteLine("as any log archives. Please make backups of those if you need to before running Check.");
            Console.WriteLine();
        }

        /// <summary>
        /// Determines how user information is processed, if at all
        /// </summary>
        /// <param name="options">Options set that may impact processing</params>
        /// <param name="info">Submission info that may be overwritten</param>
        /// <returns>True for successful updating, false or null otherwise</returns>
        public bool? ProcessUserInfo(Options? options, ref SubmissionInfo? submissionInfo)
        {
            // TODO: Somehow use the GUI strings here instead of hardcoding English
            if (options?.Processing?.ShowDiscEjectReminder == true)
                Console.WriteLine("It is now safe to eject the disc");

            // TODO: Implement some sort of CLI-based processing of user information
            return null;
        }

        /// <summary>
        /// Cancel an existing scan by capturing a cancel key press
        /// </summary>
        private void CancelScan(object? sender, ConsoleCancelEventArgs e)
            => _cancellationTokenSource.Cancel();
    }
}
