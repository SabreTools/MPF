
using System;
using System.IO;
#if NET40
using System.Threading.Tasks;
#endif
using MPF.Frontend;
using SabreTools.RedumpLib.Data;
using SabreTools.RedumpLib.Web;
using LogCompression = MPF.Processors.LogCompression;

namespace MPF.Check.Features
{
    internal abstract class BaseFeature : SabreTools.CommandLine.Feature
    {
        #region Properties

        /// <summary>
        /// User-defined options
        /// </summary>
        public Options Options { get; protected set; }

        /// <summary>
        /// Currently-selected system
        /// </summary>
        public RedumpSystem? System { get; protected set; }

        /// <summary>
        /// Seed submission info from an input file
        /// </summary>
        public SubmissionInfo? Seed { get; protected set; }

        /// <summary>
        /// Path to the device to scan
        /// </summary>
        public string? DevicePath { get; protected set; }

        #endregion

        protected BaseFeature(string name, string[] flags, string description, string? detailed = null)
            : base(name, flags, description, detailed)
        {
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
                LogCompression = LogCompression.DeflateMaximum,
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
            // Validate a system type is provided
            if (System == null)
            {
                Console.Error.WriteLine("A system name needs to be provided");
                return false;
            }

            // Validate a program is provided
            if (Options.InternalProgram == InternalProgram.NONE)
            {
                Console.Error.WriteLine("A program name needs to be provided");
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
                // Get the full file path
                string filepath = Path.GetFullPath(Inputs[i].Trim('"'));

                // Now populate an environment
                Drive? drive = null;
                if (!string.IsNullOrEmpty(DevicePath))
                    drive = Drive.Create(null, DevicePath!);

                var env = new DumpEnvironment(Options,
                    filepath,
                    drive,
                    System,
                    internalProgram: null);
                env.SetProcessor();

                // Finally, attempt to do the output dance
                var result = env.VerifyAndSaveDumpOutput(seedInfo: Seed)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
                Console.WriteLine(result.Message);
            }

            return true;
        }

        /// <summary>
        /// Display help for MPF.Check
        /// </summary>
        /// <param name="error">Error string to prefix the help text with</param>
        public static void DisplayHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("MPF.Check <system> [options] </path/to/output.cue|iso|_logs.zip> ...");
            Console.WriteLine();
            Console.WriteLine("Standalone Options:");
            Console.WriteLine("?, h, help              Show this help text");
            Console.WriteLine("version                 Print the program version");
            Console.WriteLine("lc, listcodes           List supported comment/content site codes");
            Console.WriteLine("lo, listconfig          List current configuration values");
            Console.WriteLine("lm, listmedia           List supported media types");
            Console.WriteLine("ls, listsystems         List supported system types");
            Console.WriteLine("lp, listprograms        List supported dumping program outputs");
            Console.WriteLine("i, interactive          Enable interactive mode");
            Console.WriteLine();

            Console.WriteLine("Check Options:");
            Console.WriteLine("-u, --use <program>            Dumping program output type [REQUIRED]");
            Console.WriteLine("    --load-seed <path>         Load a seed submission JSON for user information");
            Console.WriteLine("    --no-placeholders          Disable placeholder values in submission info");
            Console.WriteLine("    --create-ird               Create IRD from output files (PS3 only)");
            Console.WriteLine("    --no-retrieve              Disable retrieving match information from Redump");
            Console.WriteLine("-c, --credentials <user> <pw>  Redump username and password (incompatible with --no-retrieve) [WILL BE REMOVED]");
            Console.WriteLine("-U, --username <user>          Redump username (incompatible with --no-retrieve)");
            Console.WriteLine("-P, --password <pw>            Redump password (incompatible with --no-retrieve)");
            Console.WriteLine("    --pull-all                 Pull all information from Redump (requires --username and --password)");
            Console.WriteLine("-p, --path <drivepath>         Physical drive path for additional checks");
            Console.WriteLine("-s, --scan                     Enable copy protection scan (requires --path)");
            Console.WriteLine("    --disable-archives         Disable scanning archives (requires --scan)");
            Console.WriteLine("    --enable-debug             Enable debug protection information (requires --scan)");
            Console.WriteLine("    --hide-drive-letters       Hide drive letters from scan output (requires --scan)");
            Console.WriteLine("-x, --suffix                   Enable adding filename suffix");
            Console.WriteLine("-j, --json                     Enable submission JSON output");
            Console.WriteLine("    --include-artifacts        Include artifacts in JSON (requires --json)");
            Console.WriteLine("-z, --zip                      Enable log file compression");
            Console.WriteLine("    --log-compression          Set the log compression type (requires compression enabled)");
            Console.WriteLine("-d, --delete                   Enable unnecessary file deletion");
            Console.WriteLine();
            Console.WriteLine("WARNING: If using a configuration file alongside any of the above options");
            Console.WriteLine("then flag options will act as toggles instead of always enabling.");
            Console.WriteLine("For example, if log compression is enabled in your configuration file, then");
            Console.WriteLine("providing the --zip option would disable compression.");
            Console.WriteLine();
            Console.WriteLine("WARNING: Check will overwrite both any existing submission information files as well");
            Console.WriteLine("as any log archives. Please make backups of those if you need to before running Check.");
            Console.WriteLine();
        }
    }
}
