using System;
using System.IO;
using BinaryObjectScanner;
using MPF.Frontend;
using MPF.Frontend.Tools;
using SabreTools.RedumpLib.Data;
using SabreTools.RedumpLib.Web;

namespace MPF.CLI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Load options from the config file
            var options = OptionsLoader.LoadFromConfig();
            if (options.FirstRun)
            {
                // Application paths
                options.AaruPath = "FILL ME IN";
                options.DiscImageCreatorPath = "FILL ME IN";
                options.RedumperPath = "FILL ME IN";
                options.InternalProgram = InternalProgram.NONE;

                // Reset first run
                options.FirstRun = false;
                OptionsLoader.SaveToConfig(options, saveDefault: true);
            }

            // Try processing the standalone arguments
            bool? standaloneProcessed = OptionsLoader.ProcessStandaloneArguments(args);
            if (standaloneProcessed != false)
            {
                if (standaloneProcessed == null)
                    DisplayHelp();
                return;
            }

            // Check for the minimum number of arguments
            if (args.Length < 4)
            {
                DisplayHelp("Not enough arguments have been provided, exiting...");
                return;
            }

            // Try processing the common arguments
            (bool success, MediaType mediaType, RedumpSystem? knownSystem, var error) = OptionsLoader.ProcessCommonArguments(args);
            if (!success)
            {
                DisplayHelp(error);
                return;
            }

            // Validate the internal program
            switch (options.InternalProgram)
            {
                case InternalProgram.Aaru:
                    if (!File.Exists(options.AaruPath))
                    {
                        DisplayHelp("A path needs to be supplied for Aaru, exiting...");
                        return;
                    }
                    break;

                case InternalProgram.DiscImageCreator:
                    if (!File.Exists(options.DiscImageCreatorPath))
                    {
                        DisplayHelp("A path needs to be supplied for DIC, exiting...");
                        return;
                    }
                    break;

                case InternalProgram.Redumper:
                    if (!File.Exists(options.RedumperPath))
                    {
                        DisplayHelp("A path needs to be supplied for Redumper, exiting...");
                        return;
                    }
                    break;

                default:
                    DisplayHelp($"{options.InternalProgram} is not a supported dumping program, exiting...");
                    break;
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

            // Process any custom parameters
            (CommandOptions opts, int startIndex) = LoadFromArguments(args, options, startIndex: 2);

            // Ensure we have the values we need
            if (opts.CustomParams == null && (opts.DevicePath == null || opts.DevicePath == null))
            {
                DisplayHelp("Both a device path and file path need to be supplied, exiting...");
                return;
            }

            // Get the speed from the options
            int speed = opts.DriveSpeed ?? FrontendTool.GetDefaultSpeedForMediaType(mediaType, options);

            // Populate an environment
            var drive = Drive.Create(null, opts.DevicePath ?? string.Empty);
            var env = new DumpEnvironment(options, opts.FilePath, drive, knownSystem, mediaType, options.InternalProgram, parameters: null);

            // Process the parameters
            string? paramStr = opts.CustomParams ?? env.GetFullParameters(speed);
            if (string.IsNullOrEmpty(paramStr))
            {
                DisplayHelp("No valid environment could be created, exiting...");
                return;
            }
            env.SetExecutionContext(paramStr);

            // Invoke the dumping program
            Console.WriteLine($"Invoking {options.InternalProgram} using '{paramStr}'");
#if NET40
            var dumpResult = env.Run(resultProgress);
#else
            var dumpResult = env.Run(resultProgress).GetAwaiter().GetResult();
#endif
            Console.WriteLine(dumpResult.Message);
            if (!dumpResult)
                return;

            // If it was not a dumping command
            if (!env.IsDumpingCommand())
            {
                Console.WriteLine("Execution not recognized as dumping command, skipping processing...");
                return;
            }

            // Finally, attempt to do the output dance
#if NET40
            var verifyTask = env.VerifyAndSaveDumpOutput(resultProgress, protectionProgress);
            verifyTask.Wait();
            var verifyResult = verifyTask.Result;
#else
            var verifyResult = env.VerifyAndSaveDumpOutput(resultProgress, protectionProgress).ConfigureAwait(false).GetAwaiter().GetResult();
#endif
            Console.WriteLine(verifyResult.Message);
        }

        /// <summary>
        /// Display help for MPF.CLI
        /// </summary>
        /// <param name="error">Error string to prefix the help text with</param>
        private static void DisplayHelp(string? error = null)
        {
            if (error != null)
                Console.WriteLine(error);

            Console.WriteLine("Usage:");
            Console.WriteLine("MPF.CLI <mediatype> <system> [options]");
            Console.WriteLine();
            Console.WriteLine("Standalone Options:");
            Console.WriteLine("-h, -?                  Show this help text");
            Console.WriteLine("-lc, --listcodes        List supported comment/content site codes");
            Console.WriteLine("-lm, --listmedia        List supported media types");
            Console.WriteLine("-ls, --listsystems      List supported system types");
            Console.WriteLine("-lp, --listprograms     List supported dumping program outputs");
            Console.WriteLine();

            Console.WriteLine("CLI Options:");
            Console.WriteLine("-u, --use <program>            Override default dumping program");
            Console.WriteLine("-d, --device <devicepath>      Physical drive path (Required if no custom parameters set)");
            Console.WriteLine("-f, --file \"<filepath>\"        Output file path (Required if no custom parameters set)");
            Console.WriteLine("-s, --speed <speed>            Override default dumping speed");
            Console.WriteLine("-c, --custom \"<params>\"        Custom parameters to use");
            Console.WriteLine();

            Console.WriteLine("Custom parameters, if used, will fully replace the default parameters.");
            Console.WriteLine("All parameters need to be supplied if doing this.");
            Console.WriteLine();
        }

        /// <summary>
        /// Load the current set of options from application arguments
        /// </summary>
        private static (CommandOptions, int) LoadFromArguments(string[] args, Frontend.Options options, int startIndex = 0)
        {
            // Create return values
            var opts = new CommandOptions();

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

                // Use a device path
                else if (args[startIndex].StartsWith("-d=") || args[startIndex].StartsWith("--device="))
                {
                    opts.DevicePath = args[startIndex].Split('=')[1].Trim('"');
                }
                else if (args[startIndex] == "-d" || args[startIndex] == "--device")
                {
                    opts.DevicePath = args[startIndex + 1].Trim('"');
                    startIndex++;
                }

                // Use a file path
                else if (args[startIndex].StartsWith("-f=") || args[startIndex].StartsWith("--file="))
                {
                    opts.FilePath = args[startIndex].Split('=')[1].Trim('"');
                }
                else if (args[startIndex] == "-f" || args[startIndex] == "--file")
                {
                    opts.FilePath = args[startIndex + 1].Trim('"');
                    startIndex++;
                }

                // Set an override speed
                else if (args[startIndex].StartsWith("-s=") || args[startIndex].StartsWith("--speed="))
                {
                    if (!int.TryParse(args[startIndex].Split('=')[1].Trim('"'), out int speed))
                        speed = -1;

                    opts.DriveSpeed = speed;
                }
                else if (args[startIndex] == "-s" || args[startIndex] == "--speed")
                {
                    if (!int.TryParse(args[startIndex + 1].Trim('"'), out int speed))
                        speed = -1;

                    opts.DriveSpeed = speed;
                    startIndex++;
                }

                // Use a custom parameters
                else if (args[startIndex].StartsWith("-c=") || args[startIndex].StartsWith("--custom="))
                {
                    opts.CustomParams = args[startIndex].Split('=')[1].Trim('"');
                }
                else if (args[startIndex] == "-c" || args[startIndex] == "--custom")
                {
                    opts.CustomParams = args[startIndex + 1].Trim('"');
                    startIndex++;
                }

                // Default, we fall out
                else
                {
                    break;
                }
            }

            return (opts, startIndex);
        }

        /// <summary>
        /// Represents commandline options
        /// </summary>
        private class CommandOptions
        {
            /// <summary>
            /// Path to the device to dump
            /// </summary>
            /// <remarks>Required if custom parameters are not set</remarks>
            public string? DevicePath { get; set; } = null;

            /// <summary>
            /// Path to the output file
            /// </summary>
            /// <remarks>Required if custom parameters are not set</remarks>
            public string? FilePath { get; set; } = null;

            /// <summary>
            /// Override drive speed
            /// </summary>
            public int? DriveSpeed { get; set; } = null;

            /// <summary>
            /// Custom parameters for dumping
            /// </summary>
            public string? CustomParams { get; set; } = null;
        }
    }
}
