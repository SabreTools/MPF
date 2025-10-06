using System;
using System.Collections.Generic;
#if NET40
using System.Threading.Tasks;
#endif
using MPF.CLI.Features;
using MPF.Frontend;
using MPF.Frontend.Features;
using MPF.Frontend.Tools;
using SabreTools.CommandLine;
using SabreTools.CommandLine.Features;
using SabreTools.RedumpLib.Data;

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

            // Create the command set
            var mainFeature = new MainFeature();
            var commandSet = CreateCommands(mainFeature);

            // If we have no args, show the help and quit
            if (args == null || args.Length == 0)
            {
                DisplayHelp();
                return;
            }

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
                    interactive.ProcessArgs(args, 0);
                    interactive.Execute();
                    break;

                // Default Behavior
                default:
                    mainFeature.ProcessArgs(args, 0);
                    mainFeature.Execute();
                    break;
            }
        }

        /// <summary>
        /// Create the command set for the program
        /// </summary>
        private static CommandSet CreateCommands(MainFeature mainFeature)
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
            commandSet.Add(mainFeature.UseInput);
            commandSet.Add(mainFeature.MediaTypeInput);
            commandSet.Add(mainFeature.DeviceInput);
            commandSet.Add(mainFeature.MountedInput);
            commandSet.Add(mainFeature.FileInput);
            commandSet.Add(mainFeature.SpeedInput);
            commandSet.Add(mainFeature.CustomInput);

            return commandSet;
        }

        /// <summary>
        /// Display help for MPF.CLI
        /// </summary>
        /// <param name="error">Error string to prefix the help text with</param>
        internal static void DisplayHelp(string? error = null)
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
