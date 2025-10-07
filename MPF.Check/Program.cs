using System;
using System.Collections.Generic;
#if NET40
using System.Threading.Tasks;
#endif
using MPF.Check.Features;
using MPF.Frontend.Features;
using SabreTools.CommandLine;
using SabreTools.CommandLine.Features;

namespace MPF.Check
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create the command set
            var mainFeature = new MainFeature();
            var commandSet = CreateCommands(mainFeature);

            // If we have no args, show the help and quit
            if (args == null || args.Length == 0)
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
                    if (!interactive.VerifyInputs())
                    {
                        Console.Error.WriteLine("At least one input is required");
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
                    if (!mainFeature.VerifyInputs())
                    {
                        Console.Error.WriteLine("At least one input is required");
                        BaseFeature.DisplayHelp();
                        return;
                    }
                    if (mainFeature.Execute())
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
            commandSet.Add(mainFeature.UseInput);
            commandSet.Add(mainFeature.LoadSeedInput);
            commandSet.Add(mainFeature.NoPlaceholdersInput);
            commandSet.Add(mainFeature.CreateIrdInput);
            commandSet.Add(mainFeature.NoRetrieveInput);
            // TODO: Figure out how to work with the credentials input
            commandSet.Add(mainFeature.PullAllInput);
            commandSet.Add(mainFeature.PathInput);
            commandSet.Add(mainFeature.ScanInput);
            commandSet.Add(mainFeature.DisableArchivesInput);
            commandSet.Add(mainFeature.EnableDebugInput);
            commandSet.Add(mainFeature.HideDriveLettersInput);
            commandSet.Add(mainFeature.SuffixInput);
            commandSet.Add(mainFeature.JsonInput);
            commandSet.Add(mainFeature.IncludeArtifactsInput);
            commandSet.Add(mainFeature.ZipInput);
            commandSet.Add(mainFeature.LogCompressionInput);
            commandSet.Add(mainFeature.DeleteInput);

            return commandSet;
        }
    }
}
