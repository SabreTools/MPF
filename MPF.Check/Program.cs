using System;
using System.IO;
using BinaryObjectScanner;
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
                RedumpUsername = null,
                RedumpPassword = null,
                InternalProgram = InternalProgram.NONE,
                AddFilenameSuffix = false,
                OutputSubmissionJSON = false,
                CompressLogFiles = false,
                DeleteUnnecessaryFiles = false,
            };

            // Try processing the standalone arguments
            bool? standaloneProcessed = OptionsLoader.ProcessStandaloneArguments(args);
            if (standaloneProcessed != false)
            {
                if (standaloneProcessed == null)
                    DisplayHelp();
                return;
            }

            // Try processing the common arguments
            (bool success, MediaType mediaType, RedumpSystem? knownSystem, var error) = OptionsLoader.ProcessCommonArguments(args);
            if (!success)
            {
                DisplayHelp(error);
                return;
            }

            // Loop through and process options
            (CommandOptions opts, int startIndex) = LoadFromArguments(args, options, startIndex: 2);
            if (options.InternalProgram == InternalProgram.NONE)
            {
                DisplayHelp("A program name needs to be provided");
                return;
            }

            // Make new Progress objects
            var resultProgress = new Progress<ResultEventArgs>();
            resultProgress.ProgressChanged += ConsoleLogger.ProgressUpdated;
            var protectionProgress = new Progress<ProtectionProgress>();
            protectionProgress.ProgressChanged += ConsoleLogger.ProgressUpdated;

            // Validate the supplied credentials
#if NETFRAMEWORK
            (bool? _, string? message) = RedumpWebClient.ValidateCredentials(options.RedumpUsername ?? string.Empty, options.RedumpPassword ?? string.Empty);
#else
            (bool? _, string? message) = RedumpHttpClient.ValidateCredentials(options.RedumpUsername ?? string.Empty, options.RedumpPassword ?? string.Empty).ConfigureAwait(false).GetAwaiter().GetResult();
#endif
            if (!string.IsNullOrEmpty(message))
                Console.WriteLine(message);

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
#if NET40
                var resultTask = env.VerifyAndSaveDumpOutput(resultProgress, protectionProgress);
                resultTask.Wait();
                var result = resultTask.Result;
#else
                var result = env.VerifyAndSaveDumpOutput(resultProgress, protectionProgress).ConfigureAwait(false).GetAwaiter().GetResult();
#endif
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
            Console.WriteLine();

            Console.WriteLine("Check Options:");
            Console.WriteLine("-u, --use <program>            Dumping program output type [REQUIRED]");
            Console.WriteLine("-c, --credentials <user> <pw>  Redump username and password");
            Console.WriteLine("-a, --pull-all                 Pull all information from Redump (requires --credentials)");
            Console.WriteLine("-p, --path <drivepath>         Physical drive path for additional checks");
            Console.WriteLine("-s, --scan                     Enable copy protection scan (requires --path)");
            Console.WriteLine("-g, --hide-drive-letters       Hide drive letters from scan output (requires --protect-file)");
            Console.WriteLine("-l, --load-seed <path>         Load a seed submission JSON for user information");
            Console.WriteLine("-x, --suffix                   Enable adding filename suffix");
            Console.WriteLine("-j, --json                     Enable submission JSON output");
            Console.WriteLine("-z, --zip                      Enable log file compression");
            Console.WriteLine("-d, --delete                   Enable unnecessary file deletion");
            Console.WriteLine();
        }

        /// <summary>
        /// Load the current set of options from application arguments
        /// </summary>
        private static (CommandOptions, int) LoadFromArguments(string[] args, Frontend.Options options, int startIndex = 0)
        {
            // Create return values
            var opts = new CommandOptions();

            // These values require multiple parts to be active
            bool scan = false, hideDriveLetters = false;

            // If we have no arguments, just return
            if (args == null || args.Length == 0)
                return (opts, 0);

            // If we have an invalid start index, just return
            if (startIndex < 0 || startIndex >= args.Length)
                return (opts, startIndex);

            // Loop through the arguments and parse out values
            for (; startIndex < args.Length; startIndex++)
            {
                // Use specific program
                if (args[startIndex].StartsWith("-u=") || args[startIndex].StartsWith("--use="))
                {
                    string internalProgram = args[startIndex].Split('=')[1];
                    options.InternalProgram = Frontend.Options.ToInternalProgram(internalProgram);
                }
                else if (args[startIndex] == "-u" || args[startIndex] == "--use")
                {
                    string internalProgram = args[startIndex + 1];
                    options.InternalProgram = Frontend.Options.ToInternalProgram(internalProgram);
                    startIndex++;
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
                else if (args[startIndex].Equals("-a") || args[startIndex].Equals("--pull-all"))
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

                // Hide drive letters from scan output (requires --protect-file)
                else if (args[startIndex].Equals("-g") || args[startIndex].Equals("--hide-drive-letters"))
                {
                    hideDriveLetters = true;
                }

                // Include seed info file
                else if (args[startIndex].StartsWith("-l=") || args[startIndex].StartsWith("--load-seed="))
                {
                    string seedInfo = args[startIndex].Split('=')[1];
                    opts.Seed = Builder.CreateFromFile(seedInfo);
                }
                else if (args[startIndex] == "-l" || args[startIndex] == "--load-seed")
                {
                    string seedInfo = args[startIndex + 1];
                    opts.Seed = Builder.CreateFromFile(seedInfo);
                    startIndex++;
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

                // Compress log and extraneous files
                else if (args[startIndex].Equals("-z") || args[startIndex].Equals("--zip"))
                {
                    options.CompressLogFiles = true;
                }

                // Delete unnecessary files files
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
            options.HideDriveLetters = hideDriveLetters && scan && !string.IsNullOrEmpty(opts.DevicePath);

            return (opts, startIndex);
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
