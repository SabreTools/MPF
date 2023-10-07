using System;
using System.IO;
using BurnOutSharp;
using MPF.Core;
using MPF.Core.Converters;
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
            if (ProcessStandaloneArguments(args))
                return;

            // Try processing the common arguments
            (bool success, MediaType mediaType, RedumpSystem? knownSystem) = ProcessCommonArguments(args);
            if (!success)
                return;

            // Loop through and process options
            (Core.Data.Options options, string path, int startIndex) = OptionsLoader.LoadFromArguments(args, startIndex: 2);
            if (options.InternalProgram == InternalProgram.NONE)
            {
                DisplayHelp("A program name needs to be provided");
                return;
            }

            // Make new Progress objects
            var resultProgress = new Progress<Result>();
            resultProgress.ProgressChanged += ProgressUpdated;
            var protectionProgress = new Progress<ProtectionProgress>();
            protectionProgress.ProgressChanged += ProgressUpdated;

            // Validate the supplied credentials
#if NET48
            (bool? _, string message) = RedumpWebClient.ValidateCredentials(options?.RedumpUsername, options?.RedumpPassword);
#else
            (bool? _, string message) = RedumpHttpClient.ValidateCredentials(options?.RedumpUsername, options?.RedumpPassword).ConfigureAwait(false).GetAwaiter().GetResult();
#endif
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
                Drive drive = null;
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
        private static void DisplayHelp(string error = null)
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

        /// <summary>
        /// Process common arguments for all functionality
        /// </summary>
        /// <returns>True if all arguments pass, false otherwise</returns>
        private static (bool, MediaType, RedumpSystem?) ProcessCommonArguments(string[] args)
        {
            // All other use requires at least 3 arguments
            if (args.Length < 3)
            {
                DisplayHelp("Invalid number of arguments");
                return (false, MediaType.NONE, null);
            }

            // Check the MediaType
            var mediaType = EnumConverter.ToMediaType(args[0].Trim('"'));
            if (mediaType == MediaType.NONE)
            {
                DisplayHelp($"{args[0]} is not a recognized media type");
                return (false, MediaType.NONE, null);
            }

            // Check the RedumpSystem
            var knownSystem = Extensions.ToRedumpSystem(args[1].Trim('"'));
            if (knownSystem == null)
            {
                DisplayHelp($"{args[1]} is not a recognized system");
                return (false, MediaType.NONE, null);
            }

            return (true, mediaType, knownSystem);
        }

        /// <summary>
        /// Process any standalone arguments for the program
        /// </summary>
        /// <returns>True if one of the arguments was processed, false otherwise</returns>
        private static bool ProcessStandaloneArguments(string[] args)
        {
            // Help options
            if (args.Length == 0 || args[0] == "-h" || args[0] == "-?")
            {
                DisplayHelp();
                return true;
            }

            // List options
            if (args[0] == "-lm" || args[0] == "--listmedia")
            {
                Console.WriteLine("Supported Media Types:");
                foreach (string mediaType in Extensions.ListMediaTypes())
                {
                    Console.WriteLine(mediaType);
                }
                Console.ReadLine();
                return true;
            }
            else if (args[0] == "-lp" || args[0] == "--listprograms")
            {
                Console.WriteLine("Supported Programs:");
                foreach (string program in EnumExtensions.ListPrograms())
                {
                    Console.WriteLine(program);
                }
                Console.ReadLine();
                return true;
            }
            else if (args[0] == "-ls" || args[0] == "--listsystems")
            {
                Console.WriteLine("Supported Systems:");
                foreach (string system in Extensions.ListSystems())
                {
                    Console.WriteLine(system);
                }
                Console.ReadLine();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Simple process counter to write to console
        /// </summary>
        private static void ProgressUpdated(object sender, Result value)
        {
            Console.WriteLine(value.Message);
        }

        /// <summary>
        /// Simple process counter to write to console
        /// </summary>
        private static void ProgressUpdated(object sender, ProtectionProgress value)
        {
            Console.WriteLine($"{value.Percentage * 100:N2}%: {value.Filename} - {value.Protection}");
        }
    }
}
