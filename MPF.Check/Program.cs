using System;
using System.Collections.Generic;
using System.IO;
#if NET40
using System.Threading.Tasks;
#endif
using MPF.Check.Features;
using MPF.Frontend;
using MPF.Frontend.Features;
using MPF.Frontend.Tools;
using SabreTools.CommandLine;
using SabreTools.CommandLine.Features;
using SabreTools.CommandLine.Inputs;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;
using SabreTools.RedumpLib.Web;

namespace MPF.Check
{
    public class Program
    {
        #region Constants

        private const string _createIrdName = "create-ird";
        private const string _deleteName = "delete";
        private const string _disableArchivesName = "disable-archives";
        private const string _enableDebugName = "enable-debug";
        private const string _hideDriveLettersName = "hide-drive-letters";
        private const string _includeArtifactsName = "include-artifacts";
        private const string _jsonName = "json";
        private const string _loadSeedName = "load-seed";
        private const string _noPlaceholdersName = "no-placeholders";
        private const string _noRetrieveName = "no-retrieve";
        private const string _pathName = "path";
        private const string _pullAllName = "pull-all";
        private const string _scanName = "scan";
        private const string _suffixName = "suffix";
        private const string _useName = "use";
        private const string _zipName = "zip";

        #endregion

        public static void Main(string[] args)
        {
            // Try processing the standalone arguments
            bool? standaloneProcessed = OptionsLoader.ProcessStandaloneArguments(args);
            if (standaloneProcessed != false)
            {
                if (standaloneProcessed == null)
                    DisplayHelp();
                return;
            }

            // Setup common outputs
            Options options;
            CommandOptions opts;
            RedumpSystem? knownSystem;
            int startIndex;

            // Use interactive mode
            if (args.Length > 0 && (args[0] == "i" || args[0] == "interactive"))
            {
                startIndex = 1;
                var interactive = new InteractiveFeature();
                interactive.Execute();

                opts = interactive.CommandOptions;
                options = interactive.Options;
                knownSystem = interactive.System;
            }

            // Use normal commandline parameters
            else
            {
                // Create a default options object
                options = new Options()
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

                // Try processing the common arguments
                bool success = OptionsLoader.ProcessCommonArguments(args, out knownSystem, out var error);
                if (!success)
                {
                    DisplayHelp(error);
                    return;
                }

                // Loop through and process options
                startIndex = 1;
                opts = LoadFromArguments(args, options, ref startIndex);
            }

            if (options.InternalProgram == InternalProgram.NONE)
            {
                DisplayHelp("A program name needs to be provided");
                return;
            }

            // Validate the supplied credentials
            if (options.RetrieveMatchInformation
                && !string.IsNullOrEmpty(options.RedumpUsername)
                && !string.IsNullOrEmpty(options.RedumpPassword))
            {
                bool? validated = RedumpClient.ValidateCredentials(options.RedumpUsername!, options.RedumpPassword!).GetAwaiter().GetResult();
                string message = validated switch
                {
                    true => "Redump username and password accepted!",
                    false => "Redump username and password denied!",
                    null => "An error occurred validating your credentials!",
                };

                Console.WriteLine(message);
            }

            // Loop through all the rest of the args
            for (int i = startIndex; i < args.Length; i++)
            {
                // Check for a file
                if (!File.Exists(args[i].Trim('"')))
                {
                    DisplayHelp($"{args[i].Trim('"')} does not exist");
                    return;
                }

                // Get the full file path
                string filepath = Path.GetFullPath(args[i].Trim('"'));

                // Now populate an environment
                Drive? drive = null;
                if (!string.IsNullOrEmpty(opts.DevicePath))
                    drive = Drive.Create(null, opts.DevicePath!);

                var env = new DumpEnvironment(options,
                    filepath,
                    drive,
                    knownSystem,
                    internalProgram: null);
                env.SetProcessor();

                // Finally, attempt to do the output dance
                var result = env.VerifyAndSaveDumpOutput(seedInfo: opts.Seed)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
                Console.WriteLine(result.Message);
            }
        }

        /// <summary>
        /// Create the command set for the program
        /// </summary>
        private static CommandSet CreateCommands()
        {
            List<string> header = [
                "MPF.CLI [standalone|system] [options] <path> ...",
                string.Empty,
            ];

            List<string> footer = [
                string.Empty,
                "WARNING: Check will overwrite both any existing submission information files as well",
                "as any log archives. Please make backups of those if you need to before running Check.",
                string.Empty,
            ];

            var commandSet = new CommandSet(header, footer);

            // Standalone Options
            commandSet.Add(new Help());
            commandSet.Add(new VersionFeature());
            commandSet.Add(new ListCodesFeature());
            commandSet.Add(new ListMediaTypesFeature());
            commandSet.Add(new ListSystemsFeature());
            commandSet.Add(new ListProgramsFeature());
            commandSet.Add(new InteractiveFeature());

            // Check Options
            commandSet.Add(new StringInput(_useName, ["-u", "--use"], "Override configured dumping program name"));
            commandSet.Add(new StringInput(_loadSeedName, "--load-seed", "Load a seed submission JSON for user information"));
            commandSet.Add(new FlagInput(_noPlaceholdersName, "--no-placeholders", "Disable placeholder values in submission info"));
            commandSet.Add(new FlagInput(_createIrdName, "--create-ird", "Create IRD from output files (PS3 only)"));
            commandSet.Add(new FlagInput(_noRetrieveName, "--no-retrieve", "Disable retrieving match information from Redump"));
            // TODO: Figure out how to work with the credentials input
            commandSet.Add(new FlagInput(_pullAllName, "--pull-all", "Pull all information from Redump (requires --credentials)"));
            commandSet.Add(new StringInput(_pathName, ["-p", "--path"], "Physical drive path for additional checks"));
            commandSet.Add(new FlagInput(_scanName, ["-s", "--scan"], "Enable copy protection scan (requires --path)"));
            commandSet.Add(new FlagInput(_disableArchivesName, "--disable-archives", "Disable scanning archives (requires --scan)"));
            commandSet.Add(new FlagInput(_enableDebugName, "--enable-debug", "Enable debug protection information (requires --scan)"));
            commandSet.Add(new FlagInput(_hideDriveLettersName, "--hide-drive-letters", "Hide drive letters from scan output (requires --scan)"));
            commandSet.Add(new FlagInput(_suffixName, ["-x", "--suffix"], "Enable adding filename suffix"));
            commandSet.Add(new FlagInput(_jsonName, ["-j", "--json"], "Enable submission JSON output"));
            commandSet.Add(new FlagInput(_includeArtifactsName, "--include-artifacts", "Include artifacts in JSON (requires --json)"));
            commandSet.Add(new FlagInput(_zipName, ["-z", "--zip"], "Enable log file compression"));
            commandSet.Add(new FlagInput(_deleteName, ["-d", "--delete"], "Enable unnecessary file deletion"));

            return commandSet;
        }

        /// <summary>
        /// Display help for MPF.Check
        /// </summary>
        /// <param name="error">Error string to prefix the help text with</param>
        private static void DisplayHelp(string? error = null)
        {
            if (error != null)
                Console.WriteLine(error);

            Console.WriteLine("Usage:");
            Console.WriteLine("MPF.Check <system> [options] </path/to/output.cue/iso> ...");
            Console.WriteLine();
            Console.WriteLine("Standalone Options:");
            Console.WriteLine("?, h, help              Show this help text");
            Console.WriteLine("version                 Print the program version");
            Console.WriteLine("lc, listcodes           List supported comment/content site codes");
            Console.WriteLine("lm, listmedia           List supported media types");
            Console.WriteLine("ls, listsystems         List supported system types");
            Console.WriteLine("lp, listprograms        List supported dumping program outputs");
            Console.WriteLine("i, interactive          Enable interactive mode");
            Console.WriteLine();

            Console.WriteLine("Check Options:");
            Console.WriteLine("-u, --use <program>            Dumping program output type [REQUIRED]");
            Console.WriteLine("    --load-seed <path>         Load a seed submission JSON for user information");
            Console.WriteLine("    --no-placeholders          Disable placeholder values in submission info");
            Console.WriteLine("    --create-ird               Create IRD from output files (PS3 only)");
            Console.WriteLine("    --no-retrieve              Disable retrieving match information from Redump");
            Console.WriteLine("-c, --credentials <user> <pw>  Redump username and password (incompatible with --no-retrieve)");
            Console.WriteLine("    --pull-all                 Pull all information from Redump (requires --credentials)");
            Console.WriteLine("-p, --path <drivepath>         Physical drive path for additional checks");
            Console.WriteLine("-s, --scan                     Enable copy protection scan (requires --path)");
            Console.WriteLine("    --disable-archives         Disable scanning archives (requires --scan)");
            Console.WriteLine("    --enable-debug             Enable debug protection information (requires --scan)");
            Console.WriteLine("    --hide-drive-letters       Hide drive letters from scan output (requires --scan)");
            Console.WriteLine("-x, --suffix                   Enable adding filename suffix");
            Console.WriteLine("-j, --json                     Enable submission JSON output");
            Console.WriteLine("    --include-artifacts        Include artifacts in JSON (requires --json)");
            Console.WriteLine("-z, --zip                      Enable log file compression");
            Console.WriteLine("-d, --delete                   Enable unnecessary file deletion");
            Console.WriteLine();
            Console.WriteLine("WARNING: Check will overwrite both any existing submission information files as well");
            Console.WriteLine("as any log archives. Please make backups of those if you need to before running Check.");
            Console.WriteLine();
        }

        /// <summary>
        /// Load the current set of options from application arguments
        /// </summary>
        private static CommandOptions LoadFromArguments(string[] args, Options options, ref int startIndex)
        {
            // Create return values
            var opts = new CommandOptions();

            // These values require multiple parts to be active
            bool scan = false,
                enableArchives = true,
                enableDebug = false,
                hideDriveLetters = false;

            // If we have no arguments, just return
            if (args == null || args.Length == 0)
            {
                startIndex = 0;
                return opts;
            }

            // If we have an invalid start index, just return
            if (startIndex < 0 || startIndex >= args.Length)
                return opts;

            // Loop through the arguments and parse out values
            for (; startIndex < args.Length; startIndex++)
            {
                // Use specific program
                if (args[startIndex].StartsWith("-u=") || args[startIndex].StartsWith("--use="))
                {
                    string internalProgram = args[startIndex].Split('=')[1];
                    options.InternalProgram = internalProgram.ToInternalProgram();
                }
                else if (args[startIndex] == "-u" || args[startIndex] == "--use")
                {
                    string internalProgram = args[startIndex + 1];
                    options.InternalProgram = internalProgram.ToInternalProgram();
                    startIndex++;
                }

                // Include seed info file
                else if (args[startIndex].StartsWith("--load-seed="))
                {
                    string seedInfo = args[startIndex].Split('=')[1];
                    opts.Seed = Builder.CreateFromFile(seedInfo);
                }
                else if (args[startIndex] == "--load-seed")
                {
                    string seedInfo = args[startIndex + 1];
                    opts.Seed = Builder.CreateFromFile(seedInfo);
                    startIndex++;
                }

                // Disable placeholder values in submission info
                else if (args[startIndex].Equals("--no-placeholders"))
                {
                    options.AddPlaceholders = false;
                }

                // Create IRD from output files (PS3 only)
                else if (args[startIndex].Equals("--create-ird"))
                {
                    options.CreateIRDAfterDumping = true;
                }

                // Retrieve Redump match information
                else if (args[startIndex] == "--no-retrieve")
                {
                    options.RetrieveMatchInformation = false;
                }

                // Redump login
                else if (args[startIndex].StartsWith("-c=") || args[startIndex].StartsWith("--credentials="))
                {
                    string[] credentials = args[startIndex].Split('=')[1].Split(';');
                    options.RedumpUsername = credentials[0];
                    options.RedumpPassword = credentials[1];
                }
                else if (args[startIndex] == "-c" || args[startIndex] == "--credentials")
                {
                    options.RedumpUsername = args[startIndex + 1];
                    options.RedumpPassword = args[startIndex + 2];
                    startIndex += 2;
                }

                // Pull all information (requires Redump login)
                else if (args[startIndex].Equals("--pull-all"))
                {
                    options.PullAllInformation = true;
                }

                // Use a device path for physical checks
                else if (args[startIndex].StartsWith("-p=") || args[startIndex].StartsWith("--path="))
                {
                    opts.DevicePath = args[startIndex].Split('=')[1];
                }
                else if (args[startIndex] == "-p" || args[startIndex] == "--path")
                {
                    opts.DevicePath = args[startIndex + 1];
                    startIndex++;
                }

                // Scan for protection (requires device path)
                else if (args[startIndex].Equals("-s") || args[startIndex].Equals("--scan"))
                {
                    scan = true;
                }

                // Disable scanning archives (requires --scan)
                else if (args[startIndex].Equals("--disable-archives"))
                {
                    enableArchives = false;
                }

                // Enable debug protection information (requires --scan)
                else if (args[startIndex].Equals("--enable-debug"))
                {
                    enableDebug = true;
                }

                // Hide drive letters from scan output (requires --scan)
                else if (args[startIndex].Equals("--hide-drive-letters"))
                {
                    hideDriveLetters = true;
                }

                // Add filename suffix
                else if (args[startIndex].Equals("-x") || args[startIndex].Equals("--suffix"))
                {
                    options.AddFilenameSuffix = true;
                }

                // Output submission JSON
                else if (args[startIndex].Equals("-j") || args[startIndex].Equals("--json"))
                {
                    options.OutputSubmissionJSON = true;
                }

                // Include JSON artifacts
                else if (args[startIndex].Equals("--include-artifacts"))
                {
                    options.IncludeArtifacts = true;
                }

                // Compress log and extraneous files
                else if (args[startIndex].Equals("-z") || args[startIndex].Equals("--zip"))
                {
                    options.CompressLogFiles = true;
                }

                // Delete unnecessary files
                else if (args[startIndex].Equals("-d") || args[startIndex].Equals("--delete"))
                {
                    options.DeleteUnnecessaryFiles = true;
                }

                // Default, we fall out
                else
                {
                    break;
                }
            }

            // Now deal with the complex options
            options.ScanForProtection = scan && !string.IsNullOrEmpty(opts.DevicePath);
            options.ScanArchivesForProtection = enableArchives && scan && !string.IsNullOrEmpty(opts.DevicePath);
            options.IncludeDebugProtectionInformation = enableDebug && scan && !string.IsNullOrEmpty(opts.DevicePath);
            options.HideDriveLetters = hideDriveLetters && scan && !string.IsNullOrEmpty(opts.DevicePath);

            return opts;
        }

        /// <summary>
        /// Represents commandline options
        /// </summary>
        internal class CommandOptions
        {
            /// <summary>
            /// Seed submission info from an input file
            /// </summary>
            public SubmissionInfo? Seed { get; set; } = null;

            /// <summary>
            /// Path to the device to scan
            /// </summary>
            public string? DevicePath { get; set; } = null;
        }
    }
}
