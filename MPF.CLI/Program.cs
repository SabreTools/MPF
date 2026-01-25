using System;
using System.Collections.Generic;
#if NET40
using System.Threading.Tasks;
#endif
using MPF.CLI.Features;
using MPF.Frontend.Features;
using MPF.Frontend.Tools;
using SabreTools.CommandLine;
using SabreTools.CommandLine.Features;

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
                // Reset first run
                options.FirstRun = false;
                OptionsLoader.SaveToConfig(options);

                // Display non-error message
                Console.WriteLine("First-run detected! Please verify the generated config.json and run again.");
                return;
            }

            // Create the command set
            var mainFeature = new MainFeature();
            var commandSet = CreateCommands(mainFeature);

            // If we have no args, show the help and quit
            if (args is null || args.Length == 0)
            {
                BaseFeature.DisplayHelp();
                return;
            }

            // Get the first argument as a feature flag
            string featureName = args[0];

            // Try processing the standalone arguments
            var topLevel = commandSet.GetTopLevel(featureName);
            switch (topLevel)
            {
                // Standalone Options
                case Help: BaseFeature.DisplayHelp(); return;
                case VersionFeature version: version.Execute(); return;
                case ListCodesFeature lc: lc.Execute(); return;
                case ListConfigFeature lc: lc.Execute(); return;
                case ListMediaTypesFeature lm: lm.Execute(); return;
                case ListProgramsFeature lp: lp.Execute(); return;
                case ListSystemsFeature ls: ls.Execute(); return;

                // Interactive Mode
                case InteractiveFeature interactive:
                    if (!interactive.ProcessArgs(args, 0))
                    {
                        BaseFeature.DisplayHelp();
                        return;
                    }

                    if (!interactive.Execute())
                    {
                        BaseFeature.DisplayHelp();
                        return;
                    }

                    break;

                // Default Behavior
                default:
                    if (!mainFeature.ProcessArgs(args, 0))
                    {
                        BaseFeature.DisplayHelp();
                        return;
                    }

                    if (!mainFeature.Execute())
                    {
                        BaseFeature.DisplayHelp();
                        return;
                    }

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
            commandSet.Add(new ListConfigFeature());
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
    }
}
