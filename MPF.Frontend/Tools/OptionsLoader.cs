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
                if (_configPath is not null)
                    return _configPath;

                _configPath = GetConfigurationPath();
                return _configPath;
            }
        }
        private static string? _configPath = null;

        #region Arguments

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

            // If the file does not exist
            if (!File.Exists(ConfigurationPath) || new FileInfo(ConfigurationPath).Length == 0)
                return new Options();

            var serializer = JsonSerializer.Create();
            var stream = File.Open(ConfigurationPath, FileMode.Open, FileAccess.Read, FileShare.None);
            using var reader = new StreamReader(stream);
            var settings = serializer.Deserialize(reader, typeof(Dictionary<string, string?>)) as Dictionary<string, string?>;

            return new Options(settings);
        }

        /// <summary>
        /// Load the current set of options from the application configuration
        /// </summary>
        public static Options LoadFromConfigNative()
        {
            // If no options path can be found
            if (string.IsNullOrEmpty(ConfigurationPath))
                return new Options();

            // If the file does not exist
            if (!File.Exists(ConfigurationPath) || new FileInfo(ConfigurationPath).Length == 0)
                return new Options();

            var serializer = JsonSerializer.Create();
            serializer.DefaultValueHandling = DefaultValueHandling.Ignore;
            serializer.MissingMemberHandling = MissingMemberHandling.Ignore;
            serializer.NullValueHandling = NullValueHandling.Ignore;

            var stream = File.Open(ConfigurationPath, FileMode.Open, FileAccess.Read, FileShare.None);
            using var sr = new StreamReader(stream);
            var reader = new JsonTextReader(sr);

            return serializer.Deserialize<Options>(reader) ?? new Options();
        }

        /// <summary>
        /// Save the current set of options to the application configuration
        /// </summary>
        public static void SaveToConfig(Options options)
        {
            // If no options path can be found
            if (string.IsNullOrEmpty(ConfigurationPath))
                return;

            var serializer = JsonSerializer.Create();
            var stream = File.Open(ConfigurationPath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var sw = new StreamWriter(stream) { AutoFlush = true };
            var writer = new JsonTextWriter(sw) { Formatting = Formatting.Indented };

            serializer.Serialize(writer, options.ConvertToDictionary(), typeof(Dictionary<string, string>));
        }

        /// <summary>
        /// Save the current set of options to the application configuration
        /// </summary>
        public static void SaveToConfigNative(Options options)
        {
            // If no options path can be found
            if (string.IsNullOrEmpty(ConfigurationPath))
                return;

            var serializer = JsonSerializer.Create();
            var stream = File.Open(ConfigurationPath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var sw = new StreamWriter(stream) { AutoFlush = true };
            var writer = new JsonTextWriter(sw) { Formatting = Formatting.Indented };

            serializer.Serialize(writer, options, typeof(Options));
        }

        /// <summary>
        /// Attempt to determine the configuration path
        /// </summary>
        private static string GetConfigurationPath()
        {
            // User configuration
            string homeDir = GetUserConfigurationPath();
            if (File.Exists(Path.Combine(homeDir, ConfigurationFileName)))
                return Path.Combine(homeDir, ConfigurationFileName);

            // Portable configuration
            string runtimeDir = GetRuntimeConfigurationPath();
            if (File.Exists(Path.Combine(runtimeDir, ConfigurationFileName)))
                return Path.Combine(runtimeDir, ConfigurationFileName);

            // Attempt portable configuration
            try
            {
                Directory.CreateDirectory(runtimeDir);
                File.Create(Path.Combine(runtimeDir, ConfigurationFileName)).Dispose();
                return Path.Combine(runtimeDir, ConfigurationFileName);
            }
            catch { }

            // Attempt user configuration
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

        /// <summary>
        /// Get the runtime configuration path
        /// </summary>
        private static string GetRuntimeConfigurationPath()
        {
#if NET20 || NET35 || NET40 || NET452
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#else
            return AppContext.BaseDirectory;
#endif
        }

        /// <summary>
        /// Get the user configuration path
        /// </summary>
        /// <remarks>Typically this is located in the profile or home directory</remarks>
        private static string GetUserConfigurationPath()
        {
#if NET20 || NET35
            string homeDir = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            homeDir = Path.Combine(Path.Combine(homeDir, ".config"), "mpf");
#else
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            homeDir = Path.Combine(homeDir, ".config", "mpf");
#endif
            return homeDir;
        }

        #endregion
    }
}
