using System;
using System.IO;
using BurnOutSharp;
using DICUI.Data;
using DICUI.Utilities;
using DICUI.Web;

namespace DICUI.Check
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Help options
            if (args.Length == 0 || args[0] == "-h" || args[0] == "-?")
            {
                DisplayHelp();
                return;
            }

            // List options
            if (args[0] == "-lm" || args[0] == "--listmedia")
            {
                ListMediaTypes();
                Console.ReadLine();
                return;
            }
            else if (args[0] == "-lp" || args[0] == "--listprograms")
            {
                ListPrograms();
                Console.ReadLine();
                return;
            }
            else if (args[0] == "-ls" || args[0] == "--listsystems")
            {
                ListKnownSystems();
                Console.ReadLine();
                return;
            }

            // Normal operation check
            if (args.Length < 3)
            {
                DisplayHelp("Invalid number of arguments");
                return;
            }

            // Check the MediaType
            var mediaType = Converters.ToMediaType(args[0].Trim('"'));
            if (mediaType == MediaType.NONE)
            {
                DisplayHelp($"{args[0]} is not a recognized media type");
                return;
            }

            // Check the KnownSystem
            var knownSystem = Converters.ToKnownSystem(args[1].Trim('"'));
            if (knownSystem == KnownSystem.NONE)
            {
                DisplayHelp($"{args[1]} is not a recognized system");
                return;
            }

            // Default values
            string username = null, password = null;
            string internalProgram = "DiscImageCreator";
            string path = string.Empty;
            bool scan = false;

            // Loop through and process options
            int startIndex = 2;
            for (; startIndex < args.Length; startIndex++)
            {
                // Redump login
                if (args[startIndex].StartsWith("-c=") || args[startIndex].StartsWith("--credentials="))
                {
                    string[] credentials = args[startIndex].Split('=')[1].Split(';');
                    username = credentials[0];
                    password = credentials[1];
                }
                else if (args[startIndex] == "-c" || args[startIndex] == "--credentials")
                {
                    username = args[startIndex + 1];
                    password = args[startIndex + 2];
                    startIndex += 2;
                }

                // Use specific program
                else if (args[startIndex].StartsWith("-u=") || args[startIndex].StartsWith("--use="))
                {
                    internalProgram = args[startIndex].Split('=')[1];
                }
                else if (args[startIndex] == "-u" || args[startIndex] == "--use")
                {
                    internalProgram = args[startIndex + 1];
                    startIndex++;
                }

                // Use a device path for physical checks
                else if (args[startIndex].StartsWith("-p=") || args[startIndex].StartsWith("--path="))
                {
                    path = args[startIndex].Split('=')[1];
                }
                else if (args[startIndex] == "-p" || args[startIndex] == "--path")
                {
                    path = args[startIndex + 1];
                    startIndex++;
                }

                // Scan for protection (requires device path)
                else if (args[startIndex].StartsWith("-s") || args[startIndex].StartsWith("--scan"))
                {
                    scan = true;
                }

                // Default, we fall out
                else
                {
                    break;
                }
            }

            // Make new Progress objects
            var resultProgress = new Progress<Result>();
            resultProgress.ProgressChanged += ProgressUpdated;
            var protectionProgress = new Progress<FileProtection>();
            protectionProgress.ProgressChanged += ProgressUpdated;

            // If credentials are invalid, alert the user
            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                using (RedumpWebClient wc = new RedumpWebClient())
                {
                    if (wc.Login(username, password))
                        Console.WriteLine("Redump username and password accepted!");
                    else
                        Console.WriteLine("Redump username and password denied!");
                }
            }

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
                // TODO: Replace this with Dictionary constructor
                var options = new Options
                {
                    InternalProgram = internalProgram,
                    ScanForProtection = scan && !string.IsNullOrWhiteSpace(path),
                    PromptForDiscInformation = false,

                    Username = username,
                    Password = password,
                };

                Drive drive = null;
                if (!string.IsNullOrWhiteSpace(path))
                    drive = new Drive(null, new DriveInfo(path));

                var env = new DumpEnvironment(options, "", filepath, drive, knownSystem, mediaType, null);
                env.FixOutputPaths();

                // Finally, attempt to do the output dance
                var result = env.VerifyAndSaveDumpOutput(resultProgress, protectionProgress).ConfigureAwait(false).GetAwaiter().GetResult();
                Console.WriteLine(result.Message);
            }
        }

        /// <summary>
        /// Display help for DICUI.Check
        /// </summary>
        /// <param name="error">Error string to prefix the help text with</param>
        private static void DisplayHelp(string error = null)
        {
            if (error != null)
                Console.WriteLine(error);

            Console.WriteLine("Usage:");
            Console.WriteLine("DICUI.Check.exe <mediatype> <system> [options] </path/to/output.bin> ...");
            Console.WriteLine();
            Console.WriteLine("Standalone Options:");
            Console.WriteLine("-h, -?                  Show this help text");
            Console.WriteLine("-lm, --listmedia        List supported media types");
            Console.WriteLine("-ls, --listsystems      List supported system types");
            Console.WriteLine("-lp, --listprograms     List supported dumping program outputs");
            Console.WriteLine();
            Console.WriteLine("Check Options:");
            Console.WriteLine("-c, --credentials <user> <pw>  Redump username and password");
            Console.WriteLine("-u, --use <program>            Dumping program output type");
            Console.WriteLine("-p, --path <drivepath>         Physical drive path for additional checks");
            Console.WriteLine("-s, --scan                     Enable copy protection scan (requires --path)");
            Console.WriteLine();
        }

        /// <summary>
        /// List all media types with their short usable names
        /// </summary>
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
        /// List all known systems with their short usable names
        /// </summary>
        private static void ListKnownSystems()
        {
            Console.WriteLine("Supported Known Systems:");
            foreach (var val in Enum.GetValues(typeof(KnownSystem)))
            {
                if (((KnownSystem)val) == KnownSystem.NONE)
                    continue;

                Console.WriteLine($"{((KnownSystem?)val).ShortName()} - {((KnownSystem?)val).LongName()}");
            }
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
        private static void ProgressUpdated(object sender, FileProtection value)
        {
            Console.WriteLine($"{value.Percentage * 100:N2}%: {value.Filename} - {value.Protection}");
        }
    }
}
