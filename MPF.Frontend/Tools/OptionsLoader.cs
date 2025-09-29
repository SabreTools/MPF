using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using SabreTools.RedumpLib.Data;

namespace MPF.Frontend.Tools
{
    public static class OptionsLoader
    {
        /// <summary>
        /// Configuration file name
        /// </summary>
        private const string ConfigurationFileName = "config.json";

        /// <summary>
        /// Full path to the configuration file used by the program
        /// </summary>
        private static string ConfigurationPath
        {
            get
            {
                if (_configPath != null)
                    return _configPath;

                _configPath = GetConfigurationPath();
                return _configPath;
            }
        }
        private static string? _configPath = null;

        #region Arguments

        /// <summary>
        /// Process any standalone arguments for the program
        /// </summary>
        /// <returns>True if one of the arguments was processed, false otherwise</returns>
        public static bool? ProcessStandaloneArguments(string[] args)
        {
            // Help options
            if (args.Length == 0 || args[0] == "-h" || args[0] == "-?" || args[0] == "--help")
                return null;

            if (args[0] == "--version")
            {
                Console.WriteLine(FrontendTool.GetCurrentVersion() ?? "Unknown version");
                return true;
            }

            // List options
            if (args[0] == "-lc" || args[0] == "--listcodes")
            {
                Console.WriteLine("Supported Site Codes:");
                foreach (string siteCode in Extensions.ListSiteCodes())
                {
                    Console.WriteLine(siteCode);
                }

                return true;
            }
            else if (args[0] == "-lm" || args[0] == "--listmedia")
            {
                Console.WriteLine("Supported Media Types:");
                foreach (string mediaType in Extensions.ListMediaTypes())
                {
                    Console.WriteLine(mediaType);
                }

                return true;
            }
            else if (args[0] == "-lp" || args[0] == "--listprograms")
            {
                Console.WriteLine("Supported Programs:");
                foreach (string program in ListPrograms())
                {
                    Console.WriteLine(program);
                }

                return true;
            }
            else if (args[0] == "-ls" || args[0] == "--listsystems")
            {
                Console.WriteLine("Supported Systems:");
                foreach (string system in Extensions.ListSystems())
                {
                    Console.WriteLine(system);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Process common arguments for all functionality
        /// </summary>
        /// <returns>True if all arguments pass, false otherwise</returns>
        public static bool ProcessCommonArguments(string[] args, out RedumpSystem? system, out string? message)
        {
            // All other use requires at least 3 arguments
            if (args.Length < 2)
            {
                system = null;
                message = "Invalid number of arguments";
                return false;
            }

            // Check the RedumpSystem
            system = Extensions.ToRedumpSystem(args[0].Trim('"'));
            if (system == null)
            {
                message = $"{args[0]} is not a recognized system";
                return false;
            }

            message = null;
            return true;
        }

        /// <summary>
        /// Get the MediaType enum value for a given string
        /// </summary>
        /// <param name="type">String value to convert</param>
        /// <returns>MediaType represented by the string, if possible</returns>
        public static MediaType ToMediaType(string? type)
        {
            return (type?.ToLowerInvariant()) switch
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

                programs.Add($"{((InternalProgram?)val).ShortName()} - {((InternalProgram?)val).LongName()}");
            }

            return programs;
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Load the current set of options from the application configuration
        /// </summary>
        public static Options LoadFromConfig()
        {
            // If no options path can be found
            if (string.IsNullOrEmpty(ConfigurationPath))
                return new Options();

            // Ensure the file exists
            if (!File.Exists(ConfigurationPath) || new FileInfo(ConfigurationPath).Length == 0)
            {
                File.Create(ConfigurationPath).Dispose();
                return new Options();
            }

            var serializer = JsonSerializer.Create();
            var stream = File.Open(ConfigurationPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var reader = new StreamReader(stream);
            var settings = serializer.Deserialize(reader, typeof(Dictionary<string, string?>)) as Dictionary<string, string?>;
            reader.Dispose();
            return new Options(settings);
        }

        /// <summary>
        /// Save the current set of options to the application configuration
        /// </summary>
        public static void SaveToConfig(Options options)
        {
            // If no options path can be found
            if (string.IsNullOrEmpty(ConfigurationPath))
                return;

            // Ensure default values are included
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

            // Handle a very strange edge case
            if (!File.Exists(ConfigurationPath))
                File.Create(ConfigurationPath).Dispose();

            var serializer = JsonSerializer.Create();
            var sw = new StreamWriter(ConfigurationPath) { AutoFlush = true };
            var writer = new JsonTextWriter(sw) { Formatting = Formatting.Indented };
            serializer.Serialize(writer, options.Settings, typeof(Dictionary<string, string>));
        }

        /// <summary>
        /// Attempt to determine the configuration path
        /// </summary>
        private static string GetConfigurationPath()
        {
            // User home directory
#if NET20 || NET35
            string homeDir = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            homeDir = Path.Combine(Path.Combine(homeDir, ".config"), "mpf");
#else
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            homeDir = Path.Combine(homeDir, ".config", "mpf");
#endif
            if (File.Exists(Path.Combine(homeDir, ConfigurationFileName)))
                return Path.Combine(homeDir, ConfigurationFileName);

            // Local folder
#if NET20 || NET35 || NET40 || NET452
            string runtimeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#else
            string runtimeDir = AppContext.BaseDirectory;
#endif
            if (File.Exists(Path.Combine(runtimeDir, ConfigurationFileName)))
                return Path.Combine(runtimeDir, ConfigurationFileName);

            // Attempt to use local folder
            try
            {
                Directory.CreateDirectory(runtimeDir);
                File.Create(Path.Combine(runtimeDir, ConfigurationFileName)).Dispose();
                return Path.Combine(runtimeDir, ConfigurationFileName);
            }
            catch { }

            // Attempt to use home directory
            try
            {
                Directory.CreateDirectory(homeDir);
                File.Create(Path.Combine(homeDir, ConfigurationFileName)).Dispose();
                return Path.Combine(homeDir, ConfigurationFileName);
            }
            catch { }

            // This should not happen
            return string.Empty;
        }

        #endregion
    }
}
