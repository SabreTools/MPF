using System;
using System.IO;
using BinaryObjectScanner;
using MPF.Core;
using MPF.Core.Data;
using MPF.Core.Utilities;
using SabreTools.RedumpLib.Data;
using SabreTools.RedumpLib.Web;

namespace MPF.Check
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Try processing the standalone arguments
            bool? standaloneProcessed = OptionsLoader.ProcessStandaloneArguments(args);
            if (standaloneProcessed != false)
            {
                if (standaloneProcessed == null)
                    DisplayHelp();
                return;
            }

            // Try processing the common arguments
            (bool success, MediaType mediaType, RedumpSystem? knownSystem, var error) = OptionsLoader.ProcessCommonArguments(args);
            if (!success)
            {
                DisplayHelp(error);
                return;
            }

            // Loop through and process options
            (var options, var seedInfo, var path, int startIndex) = OptionsLoader.LoadFromArguments(args, startIndex: 2);
            if (options.InternalProgram == InternalProgram.NONE)
            {
                DisplayHelp("A program name needs to be provided");
                return;
            }

            // Make new Progress objects
            var resultProgress = new Progress<Result>();
            resultProgress.ProgressChanged += ConsoleLogger.ProgressUpdated;
            var protectionProgress = new Progress<ProtectionProgress>();
            protectionProgress.ProgressChanged += ConsoleLogger.ProgressUpdated;

            // Validate the supplied credentials
            (bool? _, string? message) = RedumpHttpClient.ValidateCredentials(options.RedumpUsername ?? string.Empty, options.RedumpPassword ?? string.Empty).ConfigureAwait(false).GetAwaiter().GetResult();
            if (!string.IsNullOrWhiteSpace(message))
                Console.WriteLine(message);

            // Loop through all the rest of the args
            for (int i = startIndex; i < args.Length; i++)
            {
                // Check for a file
                if (!File.Exists(args[i].Trim('"')))
                {
                    DisplayHelp($"{args[i].Trim('"')} does not exist");
                    return;
                }

                // Get the full file path
                string filepath = Path.GetFullPath(args[i].Trim('"'));

                // Now populate an environment
                Drive? drive = null;
                if (!string.IsNullOrWhiteSpace(path))
                    drive = Drive.Create(null, path);

                var env = new DumpEnvironment(options, filepath, drive, knownSystem, mediaType, internalProgram: null, parameters: null);

                // Finally, attempt to do the output dance
                var result = env.VerifyAndSaveDumpOutput(resultProgress, protectionProgress).ConfigureAwait(false).GetAwaiter().GetResult();
                Console.WriteLine(result.Message);
            }
        }

        /// <summary>
        /// Display help for MPF.Check
        /// </summary>
        /// <param name="error">Error string to prefix the help text with</param>
        private static void DisplayHelp(string? error = null)
        {
            if (error != null)
                Console.WriteLine(error);

            Console.WriteLine("Usage:");
            Console.WriteLine("MPF.Check.exe <mediatype> <system> [options] </path/to/output.cue/iso> ...");
            Console.WriteLine();
            Console.WriteLine("Standalone Options:");
            Console.WriteLine("-h, -?                  Show this help text");
            Console.WriteLine("-lm, --listmedia        List supported media types");
            Console.WriteLine("-ls, --listsystems      List supported system types");
            Console.WriteLine("-lp, --listprograms     List supported dumping program outputs");
            Console.WriteLine();

            Console.WriteLine("Check Options:");
            var supportedArguments = OptionsLoader.PrintSupportedArguments();
            foreach (string argument in supportedArguments)
            {
                Console.WriteLine(argument);
            }
            Console.WriteLine();
        }
    }
}
