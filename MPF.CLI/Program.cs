using System;
using System.Collections.Generic;
using System.IO;
#if NET40
using System.Threading.Tasks;
#endif
using MPF.CLI.Features;
using MPF.Frontend;
using MPF.Frontend.Features;
using MPF.Frontend.Tools;
using SabreTools.CommandLine;
using SabreTools.CommandLine.Features;
using SabreTools.CommandLine.Inputs;
using SabreTools.RedumpLib.Data;
using SabreTools.RedumpLib.Web;

namespace MPF.CLI
{
    public class Program
    {
        #region Inputs

        private const string _customName = "custom";
        private static readonly StringInput _customInput = new(_customName, ["-c", "--custom"], "Custom parameters to use");

        private const string _deviceName = "device";
        private static readonly StringInput _deviceInput = new(_deviceName, ["-d", "--device"], "Physical drive path (Required if no custom parameters set)");

        private const string _fileName = "file";
        private static readonly StringInput _fileInput = new(_fileName, ["-f", "--file"], "Output file path (Required if no custom parameters set)");

        private const string _mediaTypeName = "media-type";
        private static readonly StringInput _mediaTypeInput = new(_mediaTypeName, ["-t", "--mediatype"], "Set media type for dumping (Required for DIC)");

        private const string _mountedName = "mounted";
        private static readonly StringInput _mountedInput = new(_mountedName, ["-m", "--mounted"], "Mounted filesystem path for additional checks");

        private const string _speedName = "speed";
        private static readonly Int32Input _speedInput = new(_speedName, ["-s", "--speed"], "Override default dumping speed");

        private const string _useName = "use";
        private static readonly StringInput _useInput = new(_useName, ["-u", "--use"], "Override configured dumping program name");

        #endregion

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

            // Create the command set
            var commandSet = CreateCommands();

            // If we have no args, show the help and quit
            if (args == null || args.Length == 0)
            {
                DisplayHelp();
                return;
            }

            // Setup common outputs
            CommandOptions opts;
            RedumpSystem? knownSystem;

            // Get the first argument as a feature flag
            string featureName = args[0];

            // Try processing the standalone arguments
            var topLevel = commandSet.GetTopLevel(featureName);
            switch (topLevel)
            {
                // Standalone Options
                case Help: DisplayHelp(); return;
                case VersionFeature version: version.Execute(); return;
                case ListCodesFeature lc: lc.Execute(); return;
                case ListMediaTypesFeature lm: lm.Execute(); return;
                case ListProgramsFeature lp: lp.Execute(); return;
                case ListSystemsFeature ls: ls.Execute(); return;

                // Interactive Mode
                case InteractiveFeature interactive:
                    interactive.Execute();

                    opts = interactive.CommandOptions;
                    options = interactive.Options;
                    knownSystem = interactive.System;
                    break;

                // Default Behavior
                default:
                    // Parse the system from the first argument
                    knownSystem = Extensions.ToRedumpSystem(featureName.Trim('"'));

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

                    // Process any custom parameters
                    int startIndex = 1;
                    opts = LoadFromArguments(args, options, ref startIndex);
                    break;
            }

            // Validate the internal program
            switch (options.InternalProgram)
            {
                case InternalProgram.Aaru:
                    if (!File.Exists(options.AaruPath))
                    {
                        DisplayHelp("A path needs to be supplied in config.json for Aaru, exiting...");
                        return;
                    }
                    break;

                case InternalProgram.DiscImageCreator:
                    if (!File.Exists(options.DiscImageCreatorPath))
                    {
                        DisplayHelp("A path needs to be supplied in config.json for DIC, exiting...");
                        return;
                    }
                    break;

                case InternalProgram.Redumper:
                    if (!File.Exists(options.RedumperPath))
                    {
                        DisplayHelp("A path needs to be supplied in config.json for Redumper, exiting...");
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
            if (options.InternalProgram == InternalProgram.DiscImageCreator
                && opts.CustomParams == null
                && (opts.MediaType == null || opts.MediaType == MediaType.NONE))
            {
                DisplayHelp("Media type is required for DiscImageCreator, exiting...");
                return;
            }

            // Normalize the file path
            if (opts.FilePath != null)
                opts.FilePath = FrontendTool.NormalizeOutputPaths(opts.FilePath, getFullPath: true);

            // Get the speed from the options
            int speed = opts.DriveSpeed ?? FrontendTool.GetDefaultSpeedForMediaType(opts.MediaType, options);

            // Populate an environment
            var drive = Drive.Create(null, opts.DevicePath ?? string.Empty);
            var env = new DumpEnvironment(options,
                opts.FilePath,
                drive,
                knownSystem,
                options.InternalProgram);
            env.SetExecutionContext(opts.MediaType, null);
            env.SetProcessor();

            // Process the parameters
            string? paramStr = opts.CustomParams ?? env.GetFullParameters(opts.MediaType, speed);
            if (string.IsNullOrEmpty(paramStr))
            {
                DisplayHelp("No valid environment could be created, exiting...");
                return;
            }

            env.SetExecutionContext(opts.MediaType, paramStr);

            // Invoke the dumping program
            Console.WriteLine($"Invoking {options.InternalProgram} using '{paramStr}'");
            var dumpResult = env.Run(opts.MediaType).GetAwaiter().GetResult();
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
                    internalProgram: null);
                env.SetExecutionContext(opts.MediaType, null);
                env.SetProcessor();
            }

            // Finally, attempt to do the output dance
            var verifyResult = env.VerifyAndSaveDumpOutput()
                .ConfigureAwait(false).GetAwaiter().GetResult();
            Console.WriteLine(verifyResult.Message);
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
                "Dumping program paths and other settings can be found in the config.json file",
                "generated next to the program by default. Ensure that all settings are to user",
                "preference before running MPF.CLI.",
                string.Empty,

                "Custom dumping parameters, if used, will fully replace the default parameters.",
                "All dumping parameters need to be supplied if doing this.",
                "Otherwise, both a drive path and output file path are required.",
                string.Empty,

                "Mounted filesystem path is only recommended on OSes that require block",
                "device dumping, usually Linux and macOS.",
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

            // CLI Options
            commandSet.Add(_useInput);
            commandSet.Add(_mediaTypeInput);
            commandSet.Add(_deviceInput);
            commandSet.Add(_mountedInput);
            commandSet.Add(_fileInput);
            commandSet.Add(_speedInput);
            commandSet.Add(_customInput);

            return commandSet;
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
            Console.WriteLine("MPF.CLI <system> [options]");
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

            Console.WriteLine("CLI Options:");
            Console.WriteLine("-u, --use <program>            Override configured dumping program name");
            Console.WriteLine("-t, --mediatype <mediatype>    Set media type for dumping (Required for DIC)");
            Console.WriteLine("-d, --device <devicepath>      Physical drive path (Required if no custom parameters set)");
            Console.WriteLine("-m, --mounted <dirpath>        Mounted filesystem path for additional checks");
            Console.WriteLine("-f, --file \"<filepath>\"        Output file path (Required if no custom parameters set)");
            Console.WriteLine("-s, --speed <speed>            Override default dumping speed");
            Console.WriteLine("-c, --custom \"<params>\"        Custom parameters to use");
            Console.WriteLine();

            Console.WriteLine("Dumping program paths and other settings can be found in the config.json file");
            Console.WriteLine("generated next to the program by default. Ensure that all settings are to user");
            Console.WriteLine("preference before running MPF.CLI.");
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
        /// Load the current set of options from application arguments
        /// </summary>
        private static CommandOptions LoadFromArguments(string[] args, Options options, ref int startIndex)
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
                if (_useInput.ProcessInput(args, ref startIndex))
                    options.InternalProgram = _useInput.Value.ToInternalProgram();

                // Set a media type
                else if (_mediaTypeInput.ProcessInput(args, ref startIndex))
                    opts.MediaType = OptionsLoader.ToMediaType(_mediaTypeInput.Value?.Trim('"'));

                // Use a device path
                else if (_deviceInput.ProcessInput(args, ref startIndex))
                    opts.DevicePath = _deviceInput.Value;

                // Use a mounted path for physical checks
                else if (_mountedInput.ProcessInput(args, ref startIndex))
                    opts.MountedPath = _mountedInput.Value;

                // Use a file path
                else if (_fileInput.ProcessInput(args, ref startIndex))
                    opts.FilePath = _fileInput.Value;

                // Set an override speed
                else if (_speedInput.ProcessInput(args, ref startIndex))
                    opts.DriveSpeed = _speedInput.Value;

                // Use a custom parameters
                else if (_customInput.ProcessInput(args, ref startIndex))
                    opts.CustomParams = _customInput.Value;

                // Default, we fall out
                else
                    break;
            }

            return opts;
        }

        /// <summary>
        /// Represents commandline options
        /// </summary>
        internal class CommandOptions
        {
            /// <summary>
            /// Media type to dump
            /// </summary>
            /// <remarks>Required for DIC and if custom parameters not set</remarks>
            public MediaType? MediaType { get; set; } = null;

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
