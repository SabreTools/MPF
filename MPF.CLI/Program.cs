﻿using System;
using System.IO;
#if NET40
using System.Threading.Tasks;
#endif
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
                OptionsLoader.SaveToConfig(options);

                // Display non-error message
                DisplayHelp("First-run detected! Please fill out config.json and run again.");
                return;
            }

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

            // Use interactive mode
            if (args.Length > 0 && (args[0] == "-i" || args[0] == "--interactive"))
            {
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

                // Process any custom parameters
                int startIndex = 2;
                opts = LoadFromArguments(args, options, ref startIndex);
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

            // Ensure we have the values we need
            if (opts.CustomParams == null && (opts.DevicePath == null || opts.FilePath == null))
            {
                DisplayHelp("Both a device path and file path need to be supplied, exiting...");
                return;
            }

            // Normalize the file path
            if (opts.FilePath != null)
                opts.FilePath = Path.GetFullPath(opts.FilePath);

            // Get the speed from the options
            int speed = opts.DriveSpeed ?? FrontendTool.GetDefaultSpeedForMediaType(mediaType, options);

            // Populate an environment
            var drive = Drive.Create(null, opts.DevicePath ?? string.Empty);
            var env = new DumpEnvironment(options,
                opts.FilePath,
                drive,
                knownSystem,
                mediaType,
                options.InternalProgram,
                parameters: null);

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
            var dumpResult = env.Run().GetAwaiter().GetResult();
            Console.WriteLine(dumpResult.Message);
            if (!dumpResult)
                return;

            // If it was not a dumping command
            if (!env.IsDumpingCommand())
            {
                Console.WriteLine("Execution not recognized as dumping command, skipping processing...");
                return;
            }

            // If we have a mounted path, replace the environment
            if (opts.MountedPath != null && Directory.Exists(opts.MountedPath))
            {
                drive = Drive.Create(null, opts.MountedPath);
                env = new DumpEnvironment(options,
                    opts.FilePath,
                    drive,
                    knownSystem,
                    mediaType,
                    internalProgram: null,
                    parameters: null);
            }

            // Finally, attempt to do the output dance
            var verifyResult = env.VerifyAndSaveDumpOutput()
                .ConfigureAwait(false).GetAwaiter().GetResult();
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
            Console.WriteLine("-i, --interactive       Enable interactive mode");
            Console.WriteLine();

            Console.WriteLine("CLI Options:");
            Console.WriteLine("-u, --use <program>            Override default dumping program");
            Console.WriteLine("-d, --device <devicepath>      Physical drive path (Required if no custom parameters set)");
            Console.WriteLine("-m, --mounted <dirpath>        Mounted filesystem path for additional checks");
            Console.WriteLine("-f, --file \"<filepath>\"        Output file path (Required if no custom parameters set)");
            Console.WriteLine("-s, --speed <speed>            Override default dumping speed");
            Console.WriteLine("-c, --custom \"<params>\"        Custom parameters to use");
            Console.WriteLine();

            Console.WriteLine("Custom dumping parameters, if used, will fully replace the default parameters.");
            Console.WriteLine("All dumping parameters need to be supplied if doing this.");
            Console.WriteLine("Otherwise, both a drive path and output file path are required.");
            Console.WriteLine();

            Console.WriteLine("Mounted filesystem path is only recommended on OSes that require block");
            Console.WriteLine("device dumping, usually Linux and macOS.");
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

            // Create state values
            string? result = string.Empty;

        root:
            Console.Clear();
            Console.WriteLine("MPF.CLI Interactive Mode - Main Menu");
            Console.WriteLine("-------------------------");
            Console.WriteLine();
            Console.WriteLine($"1) Set media type (Currently '{mediaType}')");
            Console.WriteLine($"2) Set system (Currently '{system}')");
            Console.WriteLine($"3) Set dumping program (Currently '{options.InternalProgram}')");
            Console.WriteLine($"4) Set device path (Currently '{opts.DevicePath}')");
            Console.WriteLine($"5) Set mounted path (Currently '{opts.MountedPath}')");
            Console.WriteLine($"6) Set file path (Currently '{opts.FilePath}')");
            Console.WriteLine($"7) Set override speed (Currently '{opts.DriveSpeed}')");
            Console.WriteLine($"8) Set custom parameters (Currently '{opts.CustomParams}')");
            Console.WriteLine();
            Console.WriteLine($"Q) Exit the program");
            Console.WriteLine($"X) Start dumping");
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
                    goto devicePath;
                case "5":
                    goto mountedPath;
                case "6":
                    goto filePath;
                case "7":
                    goto overrideSpeed;
                case "8":
                    goto customParams;

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
                    Console.Write("> ");
                    Console.ReadLine();
                    goto root;
                default:
                    Console.WriteLine($"Invalid selection: {result}");
                    Console.ReadLine();
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

        devicePath:
            Console.WriteLine();
            Console.WriteLine("Input the device path and press Enter:");
            Console.Write("> ");
            opts.DevicePath = Console.ReadLine();
            goto root;

        mountedPath:
            Console.WriteLine();
            Console.WriteLine("Input the mounted path and press Enter:");
            Console.Write("> ");
            opts.MountedPath = Console.ReadLine();
            goto root;

        filePath:
            Console.WriteLine();
            Console.WriteLine("Input the file path and press Enter:");
            Console.Write("> ");

            result = Console.ReadLine();
            if (!string.IsNullOrEmpty(result))
                result = Path.GetFullPath(result!);

            opts.FilePath = result;
            goto root;

        overrideSpeed:
            Console.WriteLine();
            Console.WriteLine("Input the override speed and press Enter:");
            Console.Write("> ");

            result = Console.ReadLine();
            if (!int.TryParse(result, out int speed))
                speed = -1;

            opts.DriveSpeed = speed;
            goto root;

        customParams:
            Console.WriteLine();
            Console.WriteLine("Input the custom parameters and press Enter:");
            Console.Write("> ");
            opts.CustomParams = Console.ReadLine();
            goto root;

        exit:
            return opts;
        }

        /// <summary>
        /// Load the current set of options from application arguments
        /// </summary>
        private static CommandOptions LoadFromArguments(string[] args, Frontend.Options options, ref int startIndex)
        {
            // Create return values
            var opts = new CommandOptions();

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

                // Use a mounted path for physical checks
                else if (args[startIndex].StartsWith("-m=") || args[startIndex].StartsWith("--mounted="))
                {
                    opts.MountedPath = args[startIndex].Split('=')[1];
                }
                else if (args[startIndex] == "-m" || args[startIndex] == "--mounted")
                {
                    opts.MountedPath = args[startIndex + 1];
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

            return opts;
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
            /// Path to the mounted filesystem to check
            /// </summary>
            /// <remarks>Should only be used when the device path is not readable</remarks>
            public string? MountedPath { get; set; } = null;

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
