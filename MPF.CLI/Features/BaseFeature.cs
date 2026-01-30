
using System;
using System.IO;
#if NET40
using System.Threading.Tasks;
#endif
using MPF.Frontend;
using MPF.Frontend.Tools;
using SabreTools.RedumpLib.Data;
using SabreTools.RedumpLib.Web;
using LogCompression = MPF.Processors.LogCompression;

namespace MPF.CLI.Features
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
        /// Media type to dump
        /// </summary>
        /// <remarks>Required for DIC and if custom parameters not set</remarks>
        public MediaType? MediaType { get; protected set; }

        /// <summary>
        /// Path to the device to dump
        /// </summary>
        /// <remarks>Required if custom parameters are not set</remarks>
        public string? DevicePath { get; protected set; }

        /// <summary>
        /// Path to the mounted filesystem to check
        /// </summary>
        /// <remarks>Should only be used when the device path is not readable</remarks>
        public string? MountedPath { get; protected set; }

        /// <summary>
        /// Path to the output file
        /// </summary>
        /// <remarks>Required if custom parameters are not set</remarks>
        public string? FilePath { get; protected set; }

        /// <summary>
        /// Override drive speed
        /// </summary>
        public int? DriveSpeed { get; protected set; }

        /// <summary>
        /// Custom parameters for dumping
        /// </summary>
        public string? CustomParams { get; protected set; }

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
#pragma warning disable IDE0010
            switch (Options.InternalProgram)
            {
                case InternalProgram.Aaru:
                    if (!File.Exists(Options.AaruPath))
                    {
                        Console.Error.WriteLine("A path needs to be supplied in config.json for Aaru, exiting...");
                        return false;
                    }

                    break;

                case InternalProgram.DiscImageCreator:
                    if (!File.Exists(Options.DiscImageCreatorPath))
                    {
                        Console.Error.WriteLine("A path needs to be supplied in config.json for DIC, exiting...");
                        return false;
                    }

                    break;

                // case InternalProgram.Dreamdump:
                //     if (!File.Exists(Options.DreamdumpPath))
                //     {
                //         Console.Error.WriteLine("A path needs to be supplied in config.json for Dreamdump, exiting...");
                //         return false;
                //     }

                //     break;

                case InternalProgram.Redumper:
                    if (!File.Exists(Options.RedumperPath))
                    {
                        Console.Error.WriteLine("A path needs to be supplied in config.json for Redumper, exiting...");
                        return false;
                    }

                    break;

                default:
                    Console.Error.WriteLine($"{Options.InternalProgram} is not a supported dumping program, exiting...");
                    break;
            }
#pragma warning restore IDE0010

            // Ensure we have the values we need
            if (CustomParams is null && DevicePath is null)
            {
                Console.Error.WriteLine("Either custom parameters or a device path need to be provided, exiting...");
                return false;
            }

            if (Options.InternalProgram == InternalProgram.DiscImageCreator
                && CustomParams is null
                && (MediaType is null || MediaType == SabreTools.RedumpLib.Data.MediaType.NONE))
            {
                Console.Error.WriteLine("Media type is required for DiscImageCreator, exiting...");
                return false;
            }

            // If no media type is provided, use a default
            if (CustomParams is null && (MediaType is null || MediaType == SabreTools.RedumpLib.Data.MediaType.NONE))
            {
                // Get reasonable default values based on the current system
                var mediaTypes = System.MediaTypes();
                MediaType = mediaTypes.Count > 0 ? mediaTypes[0] : SabreTools.RedumpLib.Data.MediaType.CDROM;
                if (MediaType == SabreTools.RedumpLib.Data.MediaType.NONE)
                    MediaType = SabreTools.RedumpLib.Data.MediaType.CDROM;

                Console.WriteLine($"No media type was provided, using {MediaType.LongName()}");
            }

            // Normalize the file path
            if (DevicePath is not null && FilePath is null)
            {
                string defaultFileName = $"track_{DateTime.Now:yyyyMMdd-HHmm}";
                FilePath = Path.Combine(defaultFileName, $"{defaultFileName}.bin");
                if (Options.DefaultOutputPath is not null)
                    FilePath = Path.Combine(Options.DefaultOutputPath, FilePath);
            }

            if (FilePath is not null)
                FilePath = FrontendTool.NormalizeOutputPaths(FilePath, getFullPath: true);

            // Get the speed from the options
            int speed = DriveSpeed ?? FrontendTool.GetDefaultSpeedForMediaType(MediaType, Options);

            // Populate an environment
            var drive = Drive.Create(null, DevicePath ?? string.Empty);
            var env = new DumpEnvironment(Options,
                FilePath,
                drive,
                System,
                Options.InternalProgram);
            env.SetExecutionContext(MediaType, null);
            env.SetProcessor();

            // Process the parameters
            string? paramStr = CustomParams ?? env.GetFullParameters(MediaType, speed);
            if (string.IsNullOrEmpty(paramStr))
            {
                Console.Error.WriteLine("No valid environment could be created, exiting...");
                return false;
            }

            env.SetExecutionContext(MediaType, paramStr);

            // Invoke the dumping program
            Console.WriteLine($"Invoking {Options.InternalProgram} using '{paramStr}'");
            var dumpResult = env.Run(MediaType).GetAwaiter().GetResult();
            Console.WriteLine(dumpResult.Message);
            if (!dumpResult)
                return false;

            // If it was not a dumping command
            if (!env.IsDumpingCommand())
            {
                Console.Error.WriteLine();
                Console.WriteLine("Execution not recognized as dumping command, skipping processing...");
                return true;
            }

            // If we have a mounted path, replace the environment
            if (MountedPath is not null && Directory.Exists(MountedPath))
            {
                drive = Drive.Create(null, MountedPath);
                env = new DumpEnvironment(Options,
                    FilePath,
                    drive,
                    System,
                    internalProgram: null);
                env.SetExecutionContext(MediaType, null);
                env.SetProcessor();
            }

            // Finally, attempt to do the output dance
            var verifyResult = env.VerifyAndSaveDumpOutput()
                .ConfigureAwait(false).GetAwaiter().GetResult();
            Console.WriteLine(verifyResult.Message);

            return true;
        }

        /// <summary>
        /// Display help for MPF.CLI
        /// </summary>
        /// <param name="error">Error string to prefix the help text with</param>
        public static void DisplayHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("MPF.CLI <system> [options]");
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

            Console.WriteLine("CLI Options:");
            Console.WriteLine("-u, --use <program>            Override configured dumping program name");
            Console.WriteLine("-t, --mediatype <mediatype>    Set media type for dumping (Required for DIC)");
            Console.WriteLine("-d, --device <devicepath>      Physical drive path (Required if no custom parameters set)");
            Console.WriteLine("-m, --mounted <dirpath>        Mounted filesystem path for additional checks");
            Console.WriteLine("-f, --file \"<filepath>\"        Output file path (Recommended, uses defaults otherwise)");
            Console.WriteLine("-s, --speed <speed>            Override default dumping speed");
            Console.WriteLine("-c, --custom \"<params>\"        Custom parameters to use");
            Console.WriteLine();

            Console.WriteLine("Dumping program paths and other settings can be found in the config.json file");
            Console.WriteLine("generated next to the program by default. Ensure that all settings are to user");
            Console.WriteLine("preference before running MPF.CLI.");
            Console.WriteLine();

            Console.WriteLine("Custom dumping parameters, if used, will fully replace the default parameters.");
            Console.WriteLine("All dumping parameters need to be supplied if doing this.");
            Console.WriteLine("Otherwise, a drive path is required.");
            Console.WriteLine();

            Console.WriteLine("Mounted filesystem path is only recommended on OSes that require block");
            Console.WriteLine("device dumping, usually Linux and macOS.");
            Console.WriteLine();
        }
    }
}
