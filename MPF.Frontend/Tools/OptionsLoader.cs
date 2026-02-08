using System;
using System.Collections.Generic;
using System.IO;
#if NET20 || NET35 || NET40 || NET452
using System.Reflection;
#endif
using Newtonsoft.Json;
using SabreTools.RedumpLib.Data;
using AaruConstants = MPF.ExecutionContexts.Aaru.SettingConstants;
using DiscImageCreatorConstants = MPF.ExecutionContexts.DiscImageCreator.SettingConstants;
using DreamdumpConstants = MPF.ExecutionContexts.Dreamdump.SettingConstants;
using LogCompression = MPF.Processors.LogCompression;
using RedumperConstants = MPF.ExecutionContexts.Redumper.SettingConstants;

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

            return ConvertFromDictionary(settings);
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

        #region Options Helpers

        /// <summary>
        /// Convert a flat dictionary to an options object
        /// </summary>
        private static Options ConvertFromDictionary(Dictionary<string, string?>? source)
        {
            source ??= [];

            var options = new Options();

            options.FirstRun = GetBooleanSetting(source, "FirstRun", true);
            options.CheckForUpdatesOnStartup = GetBooleanSetting(source, "CheckForUpdatesOnStartup", true);
            options.VerboseLogging = GetBooleanSetting(source, "VerboseLogging", true);
            var valueString = GetStringSetting(source, "InternalProgram", InternalProgram.Redumper.ToString());
            var tempInternalProgram = valueString.ToInternalProgram();
            options.InternalProgram = tempInternalProgram == InternalProgram.NONE ? InternalProgram.Redumper : tempInternalProgram;

            options.GUI.CopyUpdateUrlToClipboard = GetBooleanSetting(source, "CopyUpdateUrlToClipboard", true);
            options.GUI.OpenLogWindowAtStartup = GetBooleanSetting(source, "OpenLogWindowAtStartup", true);

            valueString = GetStringSetting(source, "DefaultInterfaceLanguage", InterfaceLanguage.AutoDetect.ShortName());
            options.GUI.DefaultInterfaceLanguage = valueString.ToInterfaceLanguage();
            options.GUI.ShowDebugViewMenuItem = GetBooleanSetting(source, "ShowDebugViewMenuItem", false);
            options.GUI.Theming.EnableDarkMode = GetBooleanSetting(source, "EnableDarkMode", false);
            options.GUI.Theming.EnablePurpMode = GetBooleanSetting(source, "EnablePurpMode", false);
            options.GUI.Theming.CustomBackgroundColor = GetStringSetting(source, "CustomBackgroundColor", null);
            options.GUI.Theming.CustomTextColor = GetStringSetting(source, "CustomTextColor", null);

            options.GUI.FastUpdateLabel = GetBooleanSetting(source, "FastUpdateLabel", false);
            options.GUI.IgnoreFixedDrives = GetBooleanSetting(source, "IgnoreFixedDrives", true);
            options.GUI.SkipSystemDetection = GetBooleanSetting(source, "SkipSystemDetection", false);

            options.Dumping.AaruPath = GetStringSetting(source, "AaruPath", DumpSettings.DefaultAaruPath) ?? DumpSettings.DefaultAaruPath;
            options.Dumping.DiscImageCreatorPath = GetStringSetting(source, "DiscImageCreatorPath", DumpSettings.DefaultDiscImageCreatorPath) ?? DumpSettings.DefaultDiscImageCreatorPath;
            options.Dumping.DreamdumpPath = GetStringSetting(source, "DreamdumpPath", DumpSettings.DefaultDreamdumpPath) ?? DumpSettings.DefaultDreamdumpPath;
            options.Dumping.RedumperPath = GetStringSetting(source, "RedumperPath", DumpSettings.DefaultRedumperPath) ?? DumpSettings.DefaultRedumperPath;

            options.Dumping.DefaultOutputPath = GetStringSetting(source, "DefaultOutputPath", "ISO");
            valueString = GetStringSetting(source, "DefaultSystem", RedumpSystem.IBMPCcompatible.ToString());
            options.Dumping.DefaultSystem = (valueString ?? string.Empty).ToRedumpSystem();
            options.Dumping.DumpSpeeds.CD = GetInt32Setting(source, "PreferredDumpSpeedCD", 24);
            options.Dumping.DumpSpeeds.DVD = GetInt32Setting(source, "PreferredDumpSpeedDVD", 16);
            options.Dumping.DumpSpeeds.HDDVD = GetInt32Setting(source, "PreferredDumpSpeedHDDVD", 8);
            options.Dumping.DumpSpeeds.Bluray = GetInt32Setting(source, "PreferredDumpSpeedBD", 8);

            options.Dumping.Aaru.EnableDebug = GetBooleanSetting(source, AaruConstants.EnableDebug, AaruConstants.EnableDebugDefault);
            options.Dumping.Aaru.EnableVerbose = GetBooleanSetting(source, AaruConstants.EnableVerbose, AaruConstants.EnableVerboseDefault);
            options.Dumping.Aaru.ForceDumping = GetBooleanSetting(source, AaruConstants.ForceDumping, AaruConstants.ForceDumpingDefault);
            options.Dumping.Aaru.RereadCount = GetInt32Setting(source, AaruConstants.RereadCount, AaruConstants.RereadCountDefault);
            options.Dumping.Aaru.StripPersonalData = GetBooleanSetting(source, AaruConstants.StripPersonalData, AaruConstants.StripPersonalDataDefault);

            options.Dumping.DIC.MultiSectorRead = GetBooleanSetting(source, DiscImageCreatorConstants.MultiSectorRead, DiscImageCreatorConstants.MultiSectorReadDefault);
            options.Dumping.DIC.MultiSectorReadValue = GetInt32Setting(source, DiscImageCreatorConstants.MultiSectorReadValue, DiscImageCreatorConstants.MultiSectorReadValueDefault);
            options.Dumping.DIC.ParanoidMode = GetBooleanSetting(source, DiscImageCreatorConstants.ParanoidMode, DiscImageCreatorConstants.ParanoidModeDefault);
            options.Dumping.DIC.QuietMode = GetBooleanSetting(source, DiscImageCreatorConstants.QuietMode, DiscImageCreatorConstants.QuietModeDefault);
            options.Dumping.DIC.RereadCount = GetInt32Setting(source, DiscImageCreatorConstants.RereadCount, DiscImageCreatorConstants.RereadCountDefault);
            options.Dumping.DIC.DVDRereadCount = GetInt32Setting(source, DiscImageCreatorConstants.DVDRereadCount, DiscImageCreatorConstants.DVDRereadCountDefault);
            options.Dumping.DIC.UseCMIFlag = GetBooleanSetting(source, DiscImageCreatorConstants.UseCMIFlag, DiscImageCreatorConstants.UseCMIFlagDefault);

            options.Dumping.Dreamdump.NonRedumpMode = GetBooleanSetting(source, "DreamdumpNonRedumpMode", false);
            valueString = GetStringSetting(source, DreamdumpConstants.SectorOrder, DreamdumpConstants.SectorOrderDefault.ToString());
            options.Dumping.Dreamdump.SectorOrder = valueString.ToDreamdumpSectorOrder();
            options.Dumping.Dreamdump.RereadCount = GetInt32Setting(source, DreamdumpConstants.RereadCount, DreamdumpConstants.RereadCountDefault);

            options.Dumping.Redumper.EnableSkeleton = GetBooleanSetting(source, RedumperConstants.EnableSkeleton, RedumperConstants.EnableSkeletonDefault);
            options.Dumping.Redumper.EnableVerbose = GetBooleanSetting(source, RedumperConstants.EnableVerbose, RedumperConstants.EnableVerboseDefault);
            options.Dumping.Redumper.LeadinRetryCount = GetInt32Setting(source, RedumperConstants.LeadinRetryCount, RedumperConstants.LeadinRetryCountDefault);
            options.Dumping.Redumper.NonRedumpMode = GetBooleanSetting(source, "RedumperNonRedumpMode", false);
            valueString = GetStringSetting(source, RedumperConstants.DriveType, RedumperConstants.DriveTypeDefault.ToString());
            options.Dumping.Redumper.DriveType = valueString.ToRedumperDriveType();
            options.Dumping.Redumper.DrivePregapStart = GetInt32Setting(source, RedumperConstants.DrivePregapStart, RedumperConstants.DrivePregapStartDefault);
            valueString = GetStringSetting(source, RedumperConstants.ReadMethod, RedumperConstants.ReadMethodDefault.ToString());
            options.Dumping.Redumper.ReadMethod = valueString.ToRedumperReadMethod();
            valueString = GetStringSetting(source, RedumperConstants.SectorOrder, RedumperConstants.SectorOrderDefault.ToString());
            options.Dumping.Redumper.SectorOrder = valueString.ToRedumperSectorOrder();
            options.Dumping.Redumper.RereadCount = GetInt32Setting(source, RedumperConstants.RereadCount, RedumperConstants.RereadCountDefault);
            options.Dumping.Redumper.RefineSectorMode = GetBooleanSetting(source, RedumperConstants.RefineSectorMode, RedumperConstants.RefineSectorModeDefault);

            options.Processing.ProtectionScanning.ScanForProtection = GetBooleanSetting(source, "ScanForProtection", true);
            options.Processing.ProtectionScanning.ScanArchivesForProtection = GetBooleanSetting(source, "ScanArchivesForProtection", true);
            options.Processing.ProtectionScanning.HideDriveLetters = GetBooleanSetting(source, "HideDriveLetters", false);
            options.Processing.ProtectionScanning.IncludeDebugProtectionInformation = GetBooleanSetting(source, "IncludeDebugProtectionInformation", false);

            options.Processing.Login.PullAllInformation = GetBooleanSetting(source, "PullAllInformation", false);
            options.Processing.Login.RedumpUsername = GetStringSetting(source, "RedumpUsername", string.Empty);
            options.Processing.Login.RedumpPassword = GetStringSetting(source, "RedumpPassword", string.Empty);
            options.Processing.Login.RetrieveMatchInformation = GetBooleanSetting(source, "RetrieveMatchInformation", true);

            options.Processing.MediaInformation.AddPlaceholders = GetBooleanSetting(source, "AddPlaceholders", true);
            options.Processing.MediaInformation.EnableRedumpCompatibility = GetBooleanSetting(source, "EnableRedumpCompatibility", true);
            options.Processing.MediaInformation.EnableTabsInInputFields = GetBooleanSetting(source, "EnableTabsInInputFields", true);
            options.Processing.MediaInformation.PromptForDiscInformation = GetBooleanSetting(source, "PromptForDiscInformation", true);

            options.Processing.AddFilenameSuffix = GetBooleanSetting(source, "AddFilenameSuffix", false);
            options.Processing.CompressLogFiles = GetBooleanSetting(source, "CompressLogFiles", true);
            options.Processing.CreateIRDAfterDumping = GetBooleanSetting(source, "CreateIRDAfterDumping", false);
            options.Processing.DeleteUnnecessaryFiles = GetBooleanSetting(source, "DeleteUnnecessaryFiles", false);
            options.Processing.IncludeArtifacts = GetBooleanSetting(source, "IncludeArtifacts", false);
            valueString = GetStringSetting(source, "LogCompression", LogCompression.DeflateMaximum.ToString());
            options.Processing.LogCompression = valueString.ToLogCompression();
            options.Processing.OutputSubmissionJSON = GetBooleanSetting(source, "OutputSubmissionJSON", false);
            options.Processing.ShowDiscEjectReminder = GetBooleanSetting(source, "ShowDiscEjectReminder", true);

            return options;
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

                { AaruConstants.EnableDebug, options.Dumping.Aaru.EnableDebug.ToString() },
                { AaruConstants.EnableVerbose, options.Dumping.Aaru.EnableVerbose.ToString() },
                { AaruConstants.ForceDumping, options.Dumping.Aaru.ForceDumping.ToString() },
                { AaruConstants.RereadCount, options.Dumping.Aaru.RereadCount.ToString() },
                { AaruConstants.StripPersonalData, options.Dumping.Aaru.StripPersonalData.ToString() },

                { DiscImageCreatorConstants.MultiSectorRead, options.Dumping.DIC.MultiSectorRead.ToString() },
                { DiscImageCreatorConstants.MultiSectorReadValue, options.Dumping.DIC.MultiSectorReadValue.ToString() },
                { DiscImageCreatorConstants.ParanoidMode, options.Dumping.DIC.ParanoidMode.ToString() },
                { DiscImageCreatorConstants.QuietMode, options.Dumping.DIC.QuietMode.ToString() },
                { DiscImageCreatorConstants.RereadCount, options.Dumping.DIC.RereadCount.ToString() },
                { DiscImageCreatorConstants.DVDRereadCount, options.Dumping.DIC.DVDRereadCount.ToString() },
                { DiscImageCreatorConstants.UseCMIFlag, options.Dumping.DIC.UseCMIFlag.ToString() },

                { "DreamdumpNonRedumpMode", options.Dumping.Dreamdump.NonRedumpMode.ToString() },
                { DreamdumpConstants.SectorOrder, options.Dumping.Dreamdump.SectorOrder.ToString() },
                { DreamdumpConstants.RereadCount, options.Dumping.Dreamdump.RereadCount.ToString() },

                { RedumperConstants.EnableSkeleton, options.Dumping.Redumper.EnableSkeleton.ToString() },
                { RedumperConstants.EnableVerbose, options.Dumping.Redumper.EnableVerbose.ToString() },
                { RedumperConstants.LeadinRetryCount, options.Dumping.Redumper.LeadinRetryCount.ToString() },
                { "RedumperNonRedumpMode", options.Dumping.Redumper.NonRedumpMode.ToString() },
                { RedumperConstants.DriveType, options.Dumping.Redumper.DriveType.ToString() },
                { RedumperConstants.DrivePregapStart, options.Dumping.Redumper.DrivePregapStart.ToString() },
                { RedumperConstants.ReadMethod, options.Dumping.Redumper.ReadMethod.ToString() },
                { RedumperConstants.SectorOrder, options.Dumping.Redumper.SectorOrder.ToString() },
                { RedumperConstants.RereadCount, options.Dumping.Redumper.RereadCount.ToString() },
                { RedumperConstants.RefineSectorMode, options.Dumping.Redumper.RefineSectorMode.ToString() },

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
        /// Get a Boolean setting from a settings, dictionary
        /// </summary>
        /// <param name="settings">Dictionary representing the settings</param>
        /// <param name="key">Setting key to get a value for</param>
        /// <param name="defaultValue">Default value to return if no value is found</param>
        /// <returns>Setting value if possible, default value otherwise</returns>
        private static bool GetBooleanSetting(Dictionary<string, string?> settings, string key, bool defaultValue)
        {
            if (settings.ContainsKey(key))
            {
                if (bool.TryParse(settings[key], out bool value))
                    return value;
                else
                    return defaultValue;
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Get an Int32 setting from a settings, dictionary
        /// </summary>
        /// <param name="settings">Dictionary representing the settings</param>
        /// <param name="key">Setting key to get a value for</param>
        /// <param name="defaultValue">Default value to return if no value is found</param>
        /// <returns>Setting value if possible, default value otherwise</returns>
        private static int GetInt32Setting(Dictionary<string, string?> settings, string key, int defaultValue)
        {
            if (settings.ContainsKey(key))
            {
                if (int.TryParse(settings[key], out int value))
                    return value;
                else
                    return defaultValue;
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Get a String setting from a settings, dictionary
        /// </summary>
        /// <param name="settings">Dictionary representing the settings</param>
        /// <param name="key">Setting key to get a value for</param>
        /// <param name="defaultValue">Default value to return if no value is found</param>
        /// <returns>Setting value if possible, default value otherwise</returns>
        private static string? GetStringSetting(Dictionary<string, string?> settings, string key, string? defaultValue)
        {
            if (settings.ContainsKey(key))
                return settings[key];
            else
                return defaultValue;
        }

        #endregion
    }
}
