using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;

namespace MPF.Frontend.Tools
{
    public static class OptionsLoader
    {
        private const string ConfigurationPath = "config.json";

        #region Arguments

        /// <summary>
        /// Process any standalone arguments for the program
        /// </summary>
        /// <returns>True if one of the arguments was processed, false otherwise</returns>
        public static bool? ProcessStandaloneArguments(string[] args)
        {
            // Help options
            if (args.Length == 0 || args[0] == "-h" || args[0] == "-?")
                return false;

            // List options
            if (args[0] == "-lc" || args[0] == "--listcodes")
            {
                Console.WriteLine("Supported Site Codes:");
                foreach (string siteCode in Extensions.ListSiteCodes())
                {
                    Console.WriteLine(siteCode);
                }
                Console.ReadLine();
                return true;
            }
            else if (args[0] == "-lm" || args[0] == "--listmedia")
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
                foreach (string program in ListPrograms())
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
        /// Process common arguments for all functionality
        /// </summary>
        /// <returns>True if all arguments pass, false otherwise</returns>
        public static (bool, MediaType, RedumpSystem?, string?) ProcessCommonArguments(string[] args)
        {
            // All other use requires at least 3 arguments
            if (args.Length < 3)
                return (false, MediaType.NONE, null, "Invalid number of arguments");

            // Check the MediaType
            var mediaType = ToMediaType(args[0].Trim('"'));
            if (mediaType == MediaType.NONE)
                return (false, MediaType.NONE, null, $"{args[0]} is not a recognized media type");

            // Check the RedumpSystem
            var knownSystem = Extensions.ToRedumpSystem(args[1].Trim('"'));
            if (knownSystem == null)
                return (false, MediaType.NONE, null, $"{args[1]} is not a recognized system");

            return (true, mediaType, knownSystem, null);
        }

        /// <summary>
        /// Load the current set of options from application arguments
        /// </summary>
        public static (Options, SubmissionInfo?, string?, int) LoadFromArguments(string[] args, int startIndex = 0)
        {
            // Create the output values with defaults
            var options = new Options()
            {
                RedumpUsername = null,
                RedumpPassword = null,
                InternalProgram = InternalProgram.NONE,
                AddFilenameSuffix = false,
                OutputSubmissionJSON = false,
                CompressLogFiles = false,
                DeleteUnnecessaryFiles = false,
            };

            // Create the submission info to return, if necessary
            SubmissionInfo? info = null;
            string? parsedPath = null;

            // These values require multiple parts to be active
            bool scan = false, hideDriveLetters = false;

            // If we have no arguments, just return
            if (args == null || args.Length == 0)
                return (options, null, null, 0);

            // If we have an invalid start index, just return
            if (startIndex < 0 || startIndex >= args.Length)
                return (options, null, null, startIndex);

            // Loop through the arguments and parse out values
            for (; startIndex < args.Length; startIndex++)
            {
                // Use specific program
                if (args[startIndex].StartsWith("-u=") || args[startIndex].StartsWith("--use="))
                {
                    string internalProgram = args[startIndex].Split('=')[1];
                    options.InternalProgram = Options.ToInternalProgram(internalProgram);
                }
                else if (args[startIndex] == "-u" || args[startIndex] == "--use")
                {
                    string internalProgram = args[startIndex + 1];
                    options.InternalProgram = Options.ToInternalProgram(internalProgram);
                    startIndex++;
                }

                // Redump login
                else if (args[startIndex].StartsWith("-c=") || args[startIndex].StartsWith("--credentials="))
                {
                    string[] credentials = args[startIndex].Split('=')[1].Split(';');
                    options.RedumpUsername = credentials[0];
                    options.RedumpPassword = credentials[1];
                }
                else if (args[startIndex] == "-c" || args[startIndex] == "--credentials")
                {
                    options.RedumpUsername = args[startIndex + 1];
                    options.RedumpPassword = args[startIndex + 2];
                    startIndex += 2;
                }

                // Pull all information (requires Redump login)
                else if (args[startIndex].Equals("-a") || args[startIndex].Equals("--pull-all"))
                {
                    options.PullAllInformation = true;
                }

                // Use a device path for physical checks
                else if (args[startIndex].StartsWith("-p=") || args[startIndex].StartsWith("--path="))
                {
                    parsedPath = args[startIndex].Split('=')[1];
                }
                else if (args[startIndex] == "-p" || args[startIndex] == "--path")
                {
                    parsedPath = args[startIndex + 1];
                    startIndex++;
                }

                // Scan for protection (requires device path)
                else if (args[startIndex].Equals("-s") || args[startIndex].Equals("--scan"))
                {
                    scan = true;
                }

                // Hide drive letters from scan output (requires --protect-file)
                else if (args[startIndex].Equals("-g") || args[startIndex].Equals("--hide-drive-letters"))
                {
                    hideDriveLetters = true;
                }

                // Include seed info file
                else if (args[startIndex].StartsWith("-l=") || args[startIndex].StartsWith("--load-seed="))
                {
                    string seedInfo = args[startIndex].Split('=')[1];
                    info = Builder.CreateFromFile(seedInfo);
                }
                else if (args[startIndex] == "-l" || args[startIndex] == "--load-seed")
                {
                    string seedInfo = args[startIndex + 1];
                    info = Builder.CreateFromFile(seedInfo);
                    startIndex++;
                }

                // Add filename suffix
                else if (args[startIndex].Equals("-x") || args[startIndex].Equals("--suffix"))
                {
                    options.AddFilenameSuffix = true;
                }

                // Output submission JSON
                else if (args[startIndex].Equals("-j") || args[startIndex].Equals("--json"))
                {
                    options.OutputSubmissionJSON = true;
                }

                // Compress log and extraneous files
                else if (args[startIndex].Equals("-z") || args[startIndex].Equals("--zip"))
                {
                    options.CompressLogFiles = true;
                }

                // Delete unnecessary files files
                else if (args[startIndex].Equals("-d") || args[startIndex].Equals("--delete"))
                {
                    options.DeleteUnnecessaryFiles = true;
                }

                // Default, we fall out
                else
                {
                    break;
                }
            }

            // Now deal with the complex options
            options.ScanForProtection = scan && !string.IsNullOrEmpty(parsedPath);
            options.HideDriveLetters = hideDriveLetters && scan && !string.IsNullOrEmpty(parsedPath);

            return (options, info, parsedPath, startIndex);
        }

        /// <summary>
        /// Return a list of supported arguments and descriptions
        /// </summary>
        public static List<string> PrintSupportedArguments()
        {
            var supportedArguments = new List<string>
            {
                "-u, --use <program>            Dumping program output type [REQUIRED]",
                "-c, --credentials <user> <pw>  Redump username and password",
                "-a, --pull-all                 Pull all information from Redump (requires --credentials)",
                "-p, --path <drivepath>         Physical drive path for additional checks",
                "-s, --scan                     Enable copy protection scan (requires --path)",
                "-g, --hide-drive-letters       Hide drive letters from scan output (requires --protect-file)",
                "-l, --load-seed <path>         Load a seed submission JSON for user information",
                "-x, --suffix                   Enable adding filename suffix",
                "-j, --json                     Enable submission JSON output",
                "-z, --zip                      Enable log file compression",
                "-d, --delete                   Enable unnecessary file deletion",
            };

            return supportedArguments;
        }

        /// <summary>
        /// List all programs with their short usable names
        /// </summary>
        private static List<string> ListPrograms()
        {
            var programs = new List<string>();

            foreach (var val in Enum.GetValues(typeof(InternalProgram)))
            {
                if (((InternalProgram)val!) == InternalProgram.NONE)
                    continue;

                programs.Add($"{((InternalProgram?)val).LongName()}");
            }

            return programs;
        }

        /// <summary>
        /// Get the MediaType enum value for a given string
        /// </summary>
        /// <param name="type">String value to convert</param>
        /// <returns>MediaType represented by the string, if possible</returns>
        private static MediaType ToMediaType(string type)
        {
            return (type.ToLowerInvariant()) switch
            {
                #region Punched Media

                "aperture"
                    or "aperturecard"
                    or "aperture card" => MediaType.ApertureCard,
                "jacquardloom"
                    or "jacquardloomcard"
                    or "jacquard loom card" => MediaType.JacquardLoomCard,
                "magneticstripe"
                    or "magneticstripecard"
                    or "magnetic stripe card" => MediaType.MagneticStripeCard,
                "opticalphone"
                    or "opticalphonecard"
                    or "optical phonecard" => MediaType.OpticalPhonecard,
                "punchcard"
                    or "punchedcard"
                    or "punched card" => MediaType.PunchedCard,
                "punchtape"
                    or "punchedtape"
                    or "punched tape" => MediaType.PunchedTape,

                #endregion

                #region Tape

                "openreel"
                    or "openreeltape"
                    or "open reel tape" => MediaType.OpenReel,
                "datacart"
                    or "datacartridge"
                    or "datatapecartridge"
                    or "data tape cartridge" => MediaType.DataCartridge,
                "cassette"
                    or "cassettetape"
                    or "cassette tape" => MediaType.Cassette,

                #endregion

                #region Disc / Disc

                "bd"
                    or "bdrom"
                    or "bd-rom"
                    or "bluray" => MediaType.BluRay,
                "cd"
                    or "cdrom"
                    or "cd-rom" => MediaType.CDROM,
                "dvd"
                    or "dvd5"
                    or "dvd-5"
                    or "dvd9"
                    or "dvd-9"
                    or "dvdrom"
                    or "dvd-rom" => MediaType.DVD,
                "fd"
                    or "floppy"
                    or "floppydisk"
                    or "floppy disk"
                    or "floppy diskette" => MediaType.FloppyDisk,
                "floptical" => MediaType.Floptical,
                "gd"
                    or "gdrom"
                    or "gd-rom" => MediaType.GDROM,
                "hddvd"
                    or "hd-dvd"
                    or "hddvdrom"
                    or "hd-dvd-rom" => MediaType.HDDVD,
                "hdd"
                    or "harddisk"
                    or "hard disk" => MediaType.HardDisk,
                "bernoullidisk"
                    or "iomegabernoullidisk"
                    or "bernoulli disk"
                    or "iomega bernoulli disk" => MediaType.IomegaBernoulliDisk,
                "jaz"
                    or "iomegajaz"
                    or "iomega jaz" => MediaType.IomegaJaz,
                "zip"
                    or "zipdisk"
                    or "iomegazip"
                    or "iomega zip" => MediaType.IomegaZip,
                "ldrom"
                    or "lvrom"
                    or "ld-rom"
                    or "lv-rom"
                    or "laserdisc"
                    or "laservision"
                    or "ld-rom / lv-rom" => MediaType.LaserDisc,
                "64dd"
                    or "n64dd"
                    or "64dddisk"
                    or "n64dddisk"
                    or "64dd disk"
                    or "n64dd disk" => MediaType.Nintendo64DD,
                "fds"
                    or "famicom"
                    or "nfds"
                    or "nintendofamicom"
                    or "famicomdisksystem"
                    or "famicom disk system"
                    or "famicom disk system disk" => MediaType.NintendoFamicomDiskSystem,
                "gc"
                    or "gamecube"
                    or "nintendogamecube"
                    or "nintendo gamecube"
                    or "gamecube disc"
                    or "gamecube game disc" => MediaType.NintendoGameCubeGameDisc,
                "wii"
                    or "nintendowii"
                    or "nintendo wii"
                    or "nintendo wii disc"
                    or "wii optical disc" => MediaType.NintendoWiiOpticalDisc,
                "wiiu"
                    or "nintendowiiu"
                    or "nintendo wiiu"
                    or "nintendo wiiu disc"
                    or "wiiu optical disc"
                    or "wii u optical disc" => MediaType.NintendoWiiUOpticalDisc,
                "umd" => MediaType.UMD,

                #endregion

                // Unsorted Formats
                "cartridge" => MediaType.Cartridge,
                "ced"
                    or "rcaced"
                    or "rca ced"
                    or "videodisc"
                    or "rca videodisc" => MediaType.CED,

                _ => MediaType.NONE,
            };
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Load the current set of options from the application configuration
        /// </summary>
        public static Options LoadFromConfig()
        {
            if (!File.Exists(ConfigurationPath))
            {
                _ = File.Create(ConfigurationPath);
                return new Options();
            }

            var serializer = JsonSerializer.Create();
            var stream = File.Open(ConfigurationPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var reader = new StreamReader(stream);
            var settings = serializer.Deserialize(reader, typeof(Dictionary<string, string?>)) as Dictionary<string, string?>;
            return new Options(settings);
        }

        /// <summary>
        /// Save the current set of options to the application configuration
        /// </summary>
        public static void SaveToConfig(Options options, bool saveDefault = false)
        {
            // If default values should be saved as well
            if (saveDefault)
            {
                PropertyInfo[] properties = typeof(Options).GetProperties();
                foreach (var property in properties)
                {
                    // Skip dictionary properties
                    if (property.Name == "Item")
                        continue;

                    // Skip non-option properties
                    if (property.Name == "Settings" || property.Name == "HasRedumpLogin")
                        continue;

                    var val = property.GetValue(options, null);
                    property.SetValue(options, val, null);
                }
            }

            var serializer = JsonSerializer.Create();
            var sw = new StreamWriter(ConfigurationPath) { AutoFlush = true };
            var writer = new JsonTextWriter(sw) { Formatting = Formatting.Indented };
            serializer.Serialize(writer, options.Settings, typeof(Dictionary<string, string>));
        }

        #endregion
    }
}
