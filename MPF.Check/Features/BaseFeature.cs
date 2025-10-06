
using System;
using System.IO;
#if NET40
using System.Threading.Tasks;
#endif
using MPF.Frontend;
using SabreTools.RedumpLib.Data;
using SabreTools.RedumpLib.Web;

namespace MPF.Check.Features
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
            if (Options.InternalProgram == InternalProgram.NONE)
            {
                Program.DisplayHelp("A program name needs to be provided");
                return false;
            }

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

            // Loop through all the rest of the args
            for (int i = 0; i < Inputs.Count; i++)
            {
                // Check for a file
                if (!File.Exists(Inputs[i].Trim('"')))
                {
                    Program.DisplayHelp($"{Inputs[i].Trim('"')} does not exist");
                    return false;
                }

                // Get the full file path
                string filepath = Path.GetFullPath(Inputs[i].Trim('"'));

                // Now populate an environment
                Drive? drive = null;
                if (!string.IsNullOrEmpty(CommandOptions.DevicePath))
                    drive = Drive.Create(null, CommandOptions.DevicePath!);

                var env = new DumpEnvironment(Options,
                    filepath,
                    drive,
                    System,
                    internalProgram: null);
                env.SetProcessor();

                // Finally, attempt to do the output dance
                var result = env.VerifyAndSaveDumpOutput(seedInfo: CommandOptions.Seed)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
                Console.WriteLine(result.Message);
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool VerifyInputs() => Inputs.Count > 0;
    }
}
