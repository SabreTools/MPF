
using System;
using System.IO;
#if NET40
using System.Threading.Tasks;
#endif
using MPF.Frontend;
using MPF.Frontend.Tools;
using SabreTools.RedumpLib.Data;
using SabreTools.RedumpLib.Web;

namespace MPF.CLI.Features
{
    internal abstract class BaseFeature : SabreTools.CommandLine.Feature
    {
        #region Properties

        /// <summary>
        /// Progrma-specific options
        /// </summary>
        public Program.CommandOptions CommandOptions { get; protected set; }

        /// <summary>
        /// User-defined options
        /// </summary>
        public Options Options { get; protected set; }

        /// <summary>
        /// Currently-selected system
        /// </summary>
        public RedumpSystem? System { get; protected set; }

        #endregion

        protected BaseFeature(string name, string[] flags, string description, string? detailed = null)
            : base(name, flags, description, detailed)
        {
            CommandOptions = new Program.CommandOptions();
            Options = new Options()
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
        }

        /// <inheritdoc/>
        public override bool Execute()
        {
            // Validate the supplied credentials
            if (Options.RetrieveMatchInformation
                && !string.IsNullOrEmpty(Options.RedumpUsername)
                && !string.IsNullOrEmpty(Options.RedumpPassword))
            {
                bool? validated = RedumpClient.ValidateCredentials(Options.RedumpUsername!, Options.RedumpPassword!).GetAwaiter().GetResult();
                string message = validated switch
                {
                    true => "Redump username and password accepted!",
                    false => "Redump username and password denied!",
                    null => "An error occurred validating your credentials!",
                };

                Console.WriteLine(message);
            }

            // Validate the internal program
            switch (Options.InternalProgram)
            {
                case InternalProgram.Aaru:
                    if (!File.Exists(Options.AaruPath))
                    {
                        Program.DisplayHelp("A path needs to be supplied in config.json for Aaru, exiting...");
                        return false;
                    }
                    break;

                case InternalProgram.DiscImageCreator:
                    if (!File.Exists(Options.DiscImageCreatorPath))
                    {
                        Program.DisplayHelp("A path needs to be supplied in config.json for DIC, exiting...");
                        return false;
                    }
                    break;

                case InternalProgram.Redumper:
                    if (!File.Exists(Options.RedumperPath))
                    {
                        Program.DisplayHelp("A path needs to be supplied in config.json for Redumper, exiting...");
                        return false;
                    }
                    break;

                default:
                    Program.DisplayHelp($"{Options.InternalProgram} is not a supported dumping program, exiting...");
                    break;
            }

            // Ensure we have the values we need
            if (CommandOptions.CustomParams == null && (CommandOptions.DevicePath == null || CommandOptions.FilePath == null))
            {
                Program.DisplayHelp("Both a device path and file path need to be supplied, exiting...");
                return false;
            }
            if (Options.InternalProgram == InternalProgram.DiscImageCreator
                && CommandOptions.CustomParams == null
                && (CommandOptions.MediaType == null || CommandOptions.MediaType == MediaType.NONE))
            {
                Program.DisplayHelp("Media type is required for DiscImageCreator, exiting...");
                return false;
            }

            // Normalize the file path
            if (CommandOptions.FilePath != null)
                CommandOptions.FilePath = FrontendTool.NormalizeOutputPaths(CommandOptions.FilePath, getFullPath: true);

            // Get the speed from the options
            int speed = CommandOptions.DriveSpeed ?? FrontendTool.GetDefaultSpeedForMediaType(CommandOptions.MediaType, Options);

            // Populate an environment
            var drive = Drive.Create(null, CommandOptions.DevicePath ?? string.Empty);
            var env = new DumpEnvironment(Options,
                CommandOptions.FilePath,
                drive,
                System,
                Options.InternalProgram);
            env.SetExecutionContext(CommandOptions.MediaType, null);
            env.SetProcessor();

            // Process the parameters
            string? paramStr = CommandOptions.CustomParams ?? env.GetFullParameters(CommandOptions.MediaType, speed);
            if (string.IsNullOrEmpty(paramStr))
            {
                Program.DisplayHelp("No valid environment could be created, exiting...");
                return false;
            }

            env.SetExecutionContext(CommandOptions.MediaType, paramStr);

            // Invoke the dumping program
            Console.WriteLine($"Invoking {Options.InternalProgram} using '{paramStr}'");
            var dumpResult = env.Run(CommandOptions.MediaType).GetAwaiter().GetResult();
            Console.WriteLine(dumpResult.Message);
            if (!dumpResult)
                return false;

            // If it was not a dumping command
            if (!env.IsDumpingCommand())
            {
                Console.WriteLine("Execution not recognized as dumping command, skipping processing...");
                return true;
            }

            // If we have a mounted path, replace the environment
            if (CommandOptions.MountedPath != null && Directory.Exists(CommandOptions.MountedPath))
            {
                drive = Drive.Create(null, CommandOptions.MountedPath);
                env = new DumpEnvironment(Options,
                    CommandOptions.FilePath,
                    drive,
                    System,
                    internalProgram: null);
                env.SetExecutionContext(CommandOptions.MediaType, null);
                env.SetProcessor();
            }

            // Finally, attempt to do the output dance
            var verifyResult = env.VerifyAndSaveDumpOutput()
                .ConfigureAwait(false).GetAwaiter().GetResult();
            Console.WriteLine(verifyResult.Message);

            return true;
        }
    }
}
