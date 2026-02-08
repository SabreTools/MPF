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

            serializer.Serialize(writer, ConvertToDictionary(options), typeof(Dictionary<string, string>));
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
        /// Convert options object to a flat dictionary
        /// </summary>
        private static Dictionary<string, string?> ConvertToDictionary(Options options)
        {
            return new Dictionary<string, string?>
            {
                { "FirstRun", options.FirstRun.ToString()},

                { "AaruPath", options.Dumping.AaruPath },
                { "DiscImageCreatorPath", options.Dumping.DiscImageCreatorPath },
                { "DreamdumpPath", options.Dumping.DreamdumpPath },
                { "RedumperPath", options.Dumping.RedumperPath },
                { "InternalProgram", options.InternalProgram.ToString() },

                { "EnableDarkMode", options.GUI.Theming.EnableDarkMode.ToString() },
                { "EnablePurpMode", options.GUI.Theming.EnablePurpMode.ToString() },
                { "CustomBackgroundColor", options.GUI.Theming.CustomBackgroundColor },
                { "CustomTextColor", options.GUI.Theming.CustomTextColor },
                { "CheckForUpdatesOnStartup", options.CheckForUpdatesOnStartup.ToString() },
                { "CopyUpdateUrlToClipboard", options.GUI.CopyUpdateUrlToClipboard.ToString() },
                { "FastUpdateLabel", options.GUI.FastUpdateLabel.ToString() },
                { "DefaultInterfaceLanguage", options.GUI.DefaultInterfaceLanguage.ShortName() },
                { "DefaultOutputPath", options.Dumping.DefaultOutputPath },
                { "DefaultSystem", options.Dumping.DefaultSystem.ToString() },
                { "ShowDebugViewMenuItem", options.GUI.ShowDebugViewMenuItem.ToString() },

                { "PreferredDumpSpeedCD", options.Dumping.DumpSpeeds.CD.ToString() },
                { "PreferredDumpSpeedDVD", options.Dumping.DumpSpeeds.DVD.ToString() },
                { "PreferredDumpSpeedHDDVD", options.Dumping.DumpSpeeds.HDDVD.ToString() },
                { "PreferredDumpSpeedBD", options.Dumping.DumpSpeeds.Bluray.ToString() },

                { ExecutionContexts.Aaru.SettingConstants.EnableDebug, options.Dumping.Aaru.EnableDebug.ToString() },
                { ExecutionContexts.Aaru.SettingConstants.EnableVerbose, options.Dumping.Aaru.EnableVerbose.ToString() },
                { ExecutionContexts.Aaru.SettingConstants.ForceDumping, options.Dumping.Aaru.ForceDumping.ToString() },
                { ExecutionContexts.Aaru.SettingConstants.RereadCount, options.Dumping.Aaru.RereadCount.ToString() },
                { ExecutionContexts.Aaru.SettingConstants.StripPersonalData, options.Dumping.Aaru.StripPersonalData.ToString() },

                { ExecutionContexts.DiscImageCreator.SettingConstants.MultiSectorRead, options.Dumping.DIC.MultiSectorRead.ToString() },
                { ExecutionContexts.DiscImageCreator.SettingConstants.MultiSectorReadValue, options.Dumping.DIC.MultiSectorReadValue.ToString() },
                { ExecutionContexts.DiscImageCreator.SettingConstants.ParanoidMode, options.Dumping.DIC.ParanoidMode.ToString() },
                { ExecutionContexts.DiscImageCreator.SettingConstants.QuietMode, options.Dumping.DIC.QuietMode.ToString() },
                { ExecutionContexts.DiscImageCreator.SettingConstants.RereadCount, options.Dumping.DIC.RereadCount.ToString() },
                { ExecutionContexts.DiscImageCreator.SettingConstants.DVDRereadCount, options.Dumping.DIC.DVDRereadCount.ToString() },
                { ExecutionContexts.DiscImageCreator.SettingConstants.UseCMIFlag, options.Dumping.DIC.UseCMIFlag.ToString() },

                { "DreamdumpNonRedumpMode", options.Dumping.Dreamdump.NonRedumpMode.ToString() },
                { ExecutionContexts.Dreamdump.SettingConstants.SectorOrder, options.Dumping.Dreamdump.SectorOrder.ToString() },
                { ExecutionContexts.Dreamdump.SettingConstants.RereadCount, options.Dumping.Dreamdump.RereadCount.ToString() },

                { ExecutionContexts.Redumper.SettingConstants.EnableSkeleton, options.Dumping.Redumper.EnableSkeleton.ToString() },
                { ExecutionContexts.Redumper.SettingConstants.EnableVerbose, options.Dumping.Redumper.EnableVerbose.ToString() },
                { ExecutionContexts.Redumper.SettingConstants.LeadinRetryCount, options.Dumping.Redumper.LeadinRetryCount.ToString() },
                { "RedumperNonRedumpMode", options.Dumping.Redumper.NonRedumpMode.ToString() },
                { ExecutionContexts.Redumper.SettingConstants.DriveType, options.Dumping.Redumper.DriveType.ToString() },
                { ExecutionContexts.Redumper.SettingConstants.DrivePregapStart, options.Dumping.Redumper.DrivePregapStart.ToString() },
                { ExecutionContexts.Redumper.SettingConstants.ReadMethod, options.Dumping.Redumper.ReadMethod.ToString() },
                { ExecutionContexts.Redumper.SettingConstants.SectorOrder, options.Dumping.Redumper.SectorOrder.ToString() },
                { ExecutionContexts.Redumper.SettingConstants.RereadCount, options.Dumping.Redumper.RereadCount.ToString() },
                { ExecutionContexts.Redumper.SettingConstants.RefineSectorMode, options.Dumping.Redumper.RefineSectorMode.ToString() },

                { "ScanForProtection", options.Processing.ProtectionScanning.ScanForProtection.ToString() },
                { "AddPlaceholders", options.Processing.MediaInformation.AddPlaceholders.ToString() },
                { "PromptForDiscInformation", options.Processing.MediaInformation.PromptForDiscInformation.ToString() },
                { "PullAllInformation", options.Processing.Login.PullAllInformation.ToString() },
                { "EnableTabsInInputFields", options.Processing.MediaInformation.EnableTabsInInputFields.ToString() },
                { "EnableRedumpCompatibility", options.Processing.MediaInformation.EnableRedumpCompatibility.ToString() },
                { "ShowDiscEjectReminder", options.Processing.ShowDiscEjectReminder.ToString() },
                { "IgnoreFixedDrives", options.GUI.IgnoreFixedDrives.ToString() },
                { "AddFilenameSuffix", options.Processing.AddFilenameSuffix.ToString() },
                { "OutputSubmissionJSON", options.Processing.OutputSubmissionJSON.ToString() },
                { "IncludeArtifacts", options.Processing.IncludeArtifacts.ToString() },
                { "CompressLogFiles", options.Processing.CompressLogFiles.ToString() },
                { "LogCompression", options.Processing.LogCompression.ToString() },
                { "DeleteUnnecessaryFiles", options.Processing.DeleteUnnecessaryFiles.ToString() },
                { "CreateIRDAfterDumping", options.Processing.CreateIRDAfterDumping.ToString() },

                { "SkipSystemDetection", options.GUI.SkipSystemDetection.ToString() },

                { "ScanArchivesForProtection", options.Processing.ProtectionScanning.ScanArchivesForProtection.ToString() },
                { "IncludeDebugProtectionInformation", options.Processing.ProtectionScanning.IncludeDebugProtectionInformation.ToString() },
                { "HideDriveLetters", options.Processing.ProtectionScanning.HideDriveLetters.ToString() },

                { "VerboseLogging", options.VerboseLogging.ToString() },
                { "OpenLogWindowAtStartup", options.GUI.OpenLogWindowAtStartup.ToString() },

                { "RetrieveMatchInformation", options.Processing.Login.RetrieveMatchInformation.ToString() },
                { "RedumpUsername", options.Processing.Login.RedumpUsername },
                { "RedumpPassword", options.Processing.Login.RedumpPassword },
            };
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
