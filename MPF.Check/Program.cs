using System;
using System.IO;
using System.Linq;
using BurnOutSharp;
using MPF.Core.Converters;
using MPF.Core.Data;
using MPF.Core.Utilities;
using MPF.Library;
using RedumpLib.Data;
using RedumpLib.Web;

namespace MPF.Check
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Try processing the standalone arguments first
            if (ProcessStandaloneArguments(args))
                return;

            // All other use requires at least 3 arguments
            if (args.Length < 3)
            {
                DisplayHelp("Invalid number of arguments");
                return;
            }

            // Check the MediaType
            var mediaType = EnumConverter.ToMediaType(args[0].Trim('"'));
            if (mediaType == MediaType.NONE)
            {
                DisplayHelp($"{args[0]} is not a recognized media type");
                return;
            }

            // Check the RedumpSystem
            var knownSystem = Extensions.ToRedumpSystem(args[1].Trim('"'));
            if (knownSystem == null)
            {
                DisplayHelp($"{args[1]} is not a recognized system");
                return;
            }

            // Loop through and process options
            (Options options, string path, int startIndex) = OptionsLoader.LoadFromArguments(args, startIndex: 2);

            // Make new Progress objects
            var resultProgress = new Progress<Result>();
            resultProgress.ProgressChanged += ProgressUpdated;
            var protectionProgress = new Progress<ProtectionProgress>();
            protectionProgress.ProgressChanged += ProgressUpdated;

            // Validate the supplied credentials
            ValidateCredentials(options);

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
                    drive = new Drive(null, new DriveInfo(path));

                var env = new DumpEnvironment(options, "", filepath, drive, knownSystem, mediaType, null);

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
        /// List all media types with their short usable names
        /// </summary>
        /// TODO: Move to a common location
        private static void ListMediaTypes()
        {
            Console.WriteLine("Supported Media Types:");
            foreach (var val in Enum.GetValues(typeof(MediaType)))
            {
                if (((MediaType)val) == MediaType.NONE)
                    continue;

                Console.WriteLine($"{((MediaType?)val).ShortName()} - {((MediaType?)val).LongName()}");
            }
        }

        /// <summary>
        /// List all programs with their short usable names
        /// </summary>
        /// TODO: Move to a common location
        private static void ListPrograms()
        {
            Console.WriteLine("Supported Programs:");
            foreach (var val in Enum.GetValues(typeof(InternalProgram)))
            {
                if (((InternalProgram)val) == InternalProgram.NONE)
                    continue;

                Console.WriteLine($"{((InternalProgram?)val).LongName()}");
            }
        }

        /// <summary>
        /// List all systems with their short usable names
        /// </summary>
        /// TODO: Move to a common location
        private static void ListSystems()
        {
            Console.WriteLine("Supported Known Systems:");
            var knownSystems = Enum.GetValues(typeof(RedumpSystem))
                .OfType<RedumpSystem?>()
                .Where(s => s != null && !s.IsMarker() && s.GetCategory() != SystemCategory.NONE)
                .OrderBy(s => s.LongName() ?? string.Empty);

            foreach (var val in knownSystems)
            {
                Console.WriteLine($"{val.ShortName()} - {val.LongName()}");
            }
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
                ListMediaTypes();
                Console.ReadLine();
                return true;
            }
            else if (args[0] == "-lp" || args[0] == "--listprograms")
            {
                ListPrograms();
                Console.ReadLine();
                return true;
            }
            else if (args[0] == "-ls" || args[0] == "--listsystems")
            {
                ListSystems();
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

        /// <summary>
        /// Validate supplied credentials
        /// </summary>
        /// TODO: Move to a common location
        private static void ValidateCredentials(Options options)
        {
            // If options are invalid or we're missing something key, just return
            if (string.IsNullOrWhiteSpace(options?.RedumpUsername) || string.IsNullOrWhiteSpace(options?.RedumpPassword))
                return;

            // Try logging in with the supplied credentials otherwise
            using (RedumpWebClient wc = new RedumpWebClient())
            {
                bool? loggedIn = wc.Login(options.RedumpUsername, options.RedumpPassword);
                if (loggedIn == true)
                    Console.WriteLine("Redump username and password accepted!");
                else if (loggedIn == false)
                    Console.WriteLine("Redump username and password denied!");
                else
                    Console.WriteLine("An error occurred validating your crendentials!");
            }
        }
    }
}
