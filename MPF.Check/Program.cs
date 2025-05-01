using System;
using System.IO;
#if NET40
using System.Threading.Tasks;
#endif
using MPF.Frontend;
using MPF.Frontend.Tools;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;
using SabreTools.RedumpLib.Web;

namespace MPF.Check
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create a default options object
            var options = new Frontend.Options()
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
                RedumpUsername = null,
                RedumpPassword = null,
            };

            // Try processing the standalone arguments
            bool? standaloneProcessed = OptionsLoader.ProcessStandaloneArguments(args);
            if (standaloneProcessed != false)
            {
                if (standaloneProcessed == null)
                    DisplayHelp();
                return;
            }

            // Setup common outputs
            CommandOptions opts;
            MediaType mediaType;
            RedumpSystem? knownSystem;
            int startIndex;

            // Use interactive mode
            if (args.Length > 0 && (args[0] == "-i" || args[0] == "--interactive"))
            {
                startIndex = 1;
                opts = InteractiveMode(options, out mediaType, out knownSystem);
            }

            // Use normal commandline parameters
            else
            {
                // Try processing the common arguments
                bool success = OptionsLoader.ProcessCommonArguments(args, out mediaType, out knownSystem, out var error);
                if (!success)
                {
                    DisplayHelp(error);
                    return;
                }

                // Loop through and process options
                startIndex = 2;
                opts = LoadFromArguments(args, options, ref startIndex);
            }

            if (options.InternalProgram == InternalProgram.NONE)
            {
                DisplayHelp("A program name needs to be provided");
                return;
            }

            // Validate the supplied credentials
            if (!string.IsNullOrEmpty(options.RedumpUsername) && !string.IsNullOrEmpty(options.RedumpPassword))
            {
                bool? validated = RedumpClient.ValidateCredentials(options.RedumpUsername!, options.RedumpPassword!).GetAwaiter().GetResult();
                string message = validated switch
                {
                    true => "Redump username and password accepted!",
                    false => "Redump username and password denied!",
                    null => "An error occurred validating your credentials!",
                };

                if (!string.IsNullOrEmpty(message))
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

                var env = new DumpEnvironment(options, filepath, drive, knownSystem, mediaType, internalProgram: null, parameters: null);

                // Finally, attempt to do the output dance
                var result = env.VerifyAndSaveDumpOutput(seedInfo: opts.Seed)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
                Console.WriteLine(result.Message);
            }
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
            Console.WriteLine("MPF.Check <mediatype> <system> [options] </path/to/output.cue/iso> ...");
            Console.WriteLine();
            Console.WriteLine("Standalone Options:");
            Console.WriteLine("-h, -?                  Show this help text");
            Console.WriteLine("-lc, --listcodes        List supported comment/content site codes");
            Console.WriteLine("-lm, --listmedia        List supported media types");
            Console.WriteLine("-ls, --listsystems      List supported system types");
            Console.WriteLine("-lp, --listprograms     List supported dumping program outputs");
            Console.WriteLine("-i, --interactive       Enable interactive mode");
            Console.WriteLine();

            Console.WriteLine("Check Options:");
            Console.WriteLine("-u, --use <program>            Dumping program output type [REQUIRED]");
            Console.WriteLine("    --load-seed <path>         Load a seed submission JSON for user information");
            Console.WriteLine("    --no-placeholders          Disable placeholder values in submission info");
            Console.WriteLine("    --create-ird               Create IRD from output files (PS3 only)");
            Console.WriteLine("-c, --credentials <user> <pw>  Redump username and password");
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
        }

        /// <summary>
        /// Enable interactive mode for entering information
        /// </summary>
        private static CommandOptions InteractiveMode(Frontend.Options options, out MediaType mediaType, out RedumpSystem? system)
        {
            // Create return values
            var opts = new CommandOptions();
            mediaType = MediaType.NONE;
            system = null;

            // These values require multiple parts to be active
            bool scan = false,
                enableArchives = true,
                enableDebug = false,
                hideDriveLetters = false;

            // Create state values
            string? result = string.Empty;

        root:
            Console.Clear();
            Console.WriteLine("MPF.Check Interactive Mode - Main Menu");
            Console.WriteLine("-------------------------");
            Console.WriteLine();
            Console.WriteLine($"1) Set media type (Currently '{mediaType}')");
            Console.WriteLine($"2) Set system (Currently '{system}')");
            Console.WriteLine($"3) Set dumping program (Currently '{options.InternalProgram}')");
            Console.WriteLine($"4) Set seed path (Currently '{opts.Seed}')");
            Console.WriteLine($"5) Add placeholders (Currently '{options.AddPlaceholders}')");
            Console.WriteLine($"6) Create IRD (Currently '{options.CreateIRDAfterDumping}')");
            Console.WriteLine($"7) Redump credentials (Currently '{options.RedumpUsername}')");
            Console.WriteLine($"8) Pull all information (Currently '{options.PullAllInformation}')");
            Console.WriteLine($"9) Set device path (Currently '{opts.DevicePath}')");
            Console.WriteLine($"A) Scan for protection (Currently '{scan}')");
            Console.WriteLine($"B) Scan archives for protection (Currently '{enableArchives}')");
            Console.WriteLine($"C) Debug protection scan output (Currently '{enableDebug}')");
            Console.WriteLine($"D) Hide drive letters in protection output (Currently '{hideDriveLetters}')");
            Console.WriteLine($"E) Hide filename suffix (Currently '{options.AddFilenameSuffix}')");
            Console.WriteLine($"F) Output submission JSON (Currently '{options.OutputSubmissionJSON}')");
            Console.WriteLine($"G) Include JSON artifacts (Currently '{options.IncludeArtifacts}')");
            Console.WriteLine($"H) Compress logs (Currently '{options.CompressLogFiles}')");
            Console.WriteLine($"I) Delete unnecessary files (Currently '{options.DeleteUnnecessaryFiles}')");
            Console.WriteLine();
            Console.WriteLine($"Q) Exit the program");
            Console.WriteLine($"X) Start checking");
            Console.Write("> ");

            result = Console.ReadLine();
            switch (result)
            {
                case "1":
                    goto mediaType;
                case "2":
                    goto system;
                case "3":
                    goto dumpingProgram;
                case "4":
                    goto seedPath;
                case "5":
                    options.AddPlaceholders = !options.AddPlaceholders;
                    goto root;
                case "6":
                    options.CreateIRDAfterDumping = !options.CreateIRDAfterDumping;
                    goto root;
                case "7":
                    goto redumpCredentials;
                case "8":
                    options.PullAllInformation = !options.PullAllInformation;
                    goto root;
                case "9":
                    goto devicePath;
                case "a":
                case "A":
                    scan = !scan;
                    goto root;
                case "b":
                case "B":
                    enableArchives = !enableArchives;
                    goto root;
                case "c":
                case "C":
                    enableDebug = !enableDebug;
                    goto root;
                case "d":
                case "D":
                    hideDriveLetters = !hideDriveLetters;
                    goto root;
                case "e":
                case "E":
                    options.AddFilenameSuffix = !options.AddFilenameSuffix;
                    goto root;
                case "f":
                case "F":
                    options.OutputSubmissionJSON = !options.OutputSubmissionJSON;
                    goto root;
                case "g":
                case "G":
                    options.IncludeArtifacts = !options.IncludeArtifacts;
                    goto root;
                case "h":
                case "H":
                    options.CompressLogFiles = !options.CompressLogFiles;
                    goto root;
                case "i":
                case "I":
                    options.DeleteUnnecessaryFiles = !options.DeleteUnnecessaryFiles;
                    goto root;

                case "q":
                case "Q":
                    Environment.Exit(0);
                    break;
                case "x":
                case "X":
                    Console.Clear();
                    goto exit;
                case "z":
                case "Z":
                    Console.WriteLine("It is pitch black. You are likely to be eaten by a grue.");
                    goto root;
                default:
                    Console.WriteLine($"Invalid selection: {result}");
                    goto root;
            }

        mediaType:
            Console.WriteLine();
            Console.WriteLine("Input the media type and press Enter:");
            Console.Write("> ");
            result = Console.ReadLine();
            mediaType = OptionsLoader.ToMediaType(result);
            goto root;

        system:
            Console.WriteLine();
            Console.WriteLine("Input the system and press Enter:");
            Console.Write("> ");
            result = Console.ReadLine();
            system = Extensions.ToRedumpSystem(result);
            goto root;

        dumpingProgram:
            Console.WriteLine();
            Console.WriteLine("Input the dumping program and press Enter:");
            Console.Write("> ");
            result = Console.ReadLine();
            options.InternalProgram = result.ToInternalProgram();
            goto root;

        seedPath:
            Console.WriteLine();
            Console.WriteLine("Input the seed path and press Enter:");
            Console.Write("> ");
            result = Console.ReadLine();
            opts.Seed = Builder.CreateFromFile(result);
            goto root;

        redumpCredentials:
            Console.WriteLine();
            Console.WriteLine("Enter your Redumper username and press Enter:");
            Console.Write("> ");
            options.RedumpUsername = Console.ReadLine();

            Console.WriteLine("Enter your Redumper password (hidden) and press Enter:");
            Console.Write("> ");
            options.RedumpPassword = string.Empty;
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                    break;

                options.RedumpPassword += key.KeyChar;
            }

            goto root;

        devicePath:
            Console.WriteLine();
            Console.WriteLine("Input the device path and press Enter:");
            Console.Write("> ");
            opts.DevicePath = Console.ReadLine();
            goto root;

        exit:
            // Now deal with the complex options
            options.ScanForProtection = scan && !string.IsNullOrEmpty(opts.DevicePath);
            options.ScanArchivesForProtection = enableArchives && scan && !string.IsNullOrEmpty(opts.DevicePath);
            options.IncludeDebugProtectionInformation = enableDebug && scan && !string.IsNullOrEmpty(opts.DevicePath);
            options.HideDriveLetters = hideDriveLetters && scan && !string.IsNullOrEmpty(opts.DevicePath);

            return opts;
        }

        /// <summary>
        /// Load the current set of options from application arguments
        /// </summary>
        private static CommandOptions LoadFromArguments(string[] args, Frontend.Options options, ref int startIndex)
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
        private class CommandOptions
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
