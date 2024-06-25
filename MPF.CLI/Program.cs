using System;
using System.IO;
using System.Linq;
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

            // Get the explicit output options
            string path = args[2].Trim('"');
            string filepath = args[3].Trim('"');

            // Get the speed from the options
            int speed = mediaType switch
            {
                // CD dump speed
                MediaType.CDROM => options.PreferredDumpSpeedCD,
                MediaType.GDROM => options.PreferredDumpSpeedCD,

                // DVD dump speed
                MediaType.DVD => options.PreferredDumpSpeedDVD,
                MediaType.NintendoGameCubeGameDisc => options.PreferredDumpSpeedDVD,
                MediaType.NintendoWiiOpticalDisc => options.PreferredDumpSpeedDVD,

                // HD-DVD dump speed
                MediaType.HDDVD => options.PreferredDumpSpeedHDDVD,

                // BD dump speed
                MediaType.BluRay => options.PreferredDumpSpeedBD,

                // Default
                _ => options.PreferredDumpSpeedCD,
            };

            // Now populate an environment
            var drive = Drive.Create(null, path);
            var env = new DumpEnvironment(options, filepath, drive, knownSystem, mediaType, options.InternalProgram, parameters: null);
            string? paramStr = env.GetFullParameters(speed);

            // Process custom parameters
            if (args.Length > 4)
                paramStr = string.Join(" ", args.Skip(4).ToArray());
            
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
            Console.WriteLine("MPF.CLI <mediatype> <system> <drivepath> </path/to/output.cue/iso> [custom-params]");
            Console.WriteLine();
            Console.WriteLine("Standalone Options:");
            Console.WriteLine("-h, -?                  Show this help text");
            Console.WriteLine("-lc, --listcodes        List supported comment/content site codes");
            Console.WriteLine("-lm, --listmedia        List supported media types");
            Console.WriteLine("-ls, --listsystems      List supported system types");
            Console.WriteLine("-lp, --listprograms     List supported dumping program outputs");
            Console.WriteLine();
            Console.WriteLine("Custom parameters, if supplied, will fully replace the default parameters for the dumping");
            Console.WriteLine("program defined in the configuration. All parameters need to be supplied if doing this.");
            Console.WriteLine();
        }
    }
}
