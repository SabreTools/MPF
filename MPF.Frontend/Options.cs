using System;
using System.Collections.Generic;
using System.IO;
using SabreTools.RedumpLib.Data;
using AaruConstants = MPF.ExecutionContexts.Aaru.SettingConstants;
using DiscImageCreatorConstants = MPF.ExecutionContexts.DiscImageCreator.SettingConstants;
using DreamdumpConstants = MPF.ExecutionContexts.Dreamdump.SettingConstants;
using LogCompression = MPF.Processors.LogCompression;
using RedumperConstants = MPF.ExecutionContexts.Redumper.SettingConstants;

namespace MPF.Frontend
{
    /// <summary>
    /// Options that use nested types for setting arrangement
    /// </summary>
    public class Options
    {
        #region Properties

        /// <summary>
        /// Internal structure version
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// Indicate if the program is being run with a clean configuration
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool FirstRun { get; set; } = true;

        /// <summary>
        /// Check for updates on startup
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool CheckForUpdatesOnStartup { get; set; } = true;

        /// <summary>
        /// Enable verbose and debug logs to be written
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool VerboseLogging { get; set; } = true;

        /// <summary>
        /// Currently selected dumping program
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public InternalProgram InternalProgram { get; set; } = InternalProgram.Redumper;

        /// <summary>
        /// GUI settings
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public GuiSettings GUI { get; set; } = new GuiSettings();

        /// <summary>
        /// Dumping settings
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public DumpSettings Dumping { get; set; } = new DumpSettings();

        /// <summary>
        /// Processing settings
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public ProcessingSettings Processing { get; set; } = new ProcessingSettings();

        #endregion

        #region Passthrough Properties

        /// <summary>
        /// All settings in the form of a dictionary
        /// </summary>
        /// TODO: Remove when Options is no longer relevant
        public Dictionary<string, string?> Settings
        {
            get { return ConvertToDictionary(); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Empty constructor for serialization
        /// </summary>
        public Options() { }

        /// <summary>
        /// Constructor taking a dictionary for settings
        /// </summary>
        /// <param name="source">Dictionary representing settings</param>
        /// TODO: Remove when Options is no longer relevant
        public Options(Dictionary<string, string?>? source = null)
        {
            source ??= [];

            FirstRun = GetBooleanSetting(source, "FirstRun", true);
            CheckForUpdatesOnStartup = GetBooleanSetting(source, "CheckForUpdatesOnStartup", true);
            VerboseLogging = GetBooleanSetting(source, "VerboseLogging", true);
            var valueString = GetStringSetting(source, "InternalProgram", InternalProgram.Redumper.ToString());
            var tempInternalProgram = valueString.ToInternalProgram();
            InternalProgram = tempInternalProgram == InternalProgram.NONE ? InternalProgram.Redumper : tempInternalProgram;

            GUI.CopyUpdateUrlToClipboard = GetBooleanSetting(source, "CopyUpdateUrlToClipboard", true);

            valueString = GetStringSetting(source, "DefaultInterfaceLanguage", InterfaceLanguage.AutoDetect.ShortName());
            GUI.DefaultInterfaceLanguage = valueString.ToInterfaceLanguage();
            GUI.ShowDebugViewMenuItem = GetBooleanSetting(source, "ShowDebugViewMenuItem", false);
            GUI.OpenLogWindowAtStartup = GetBooleanSetting(source, "OpenLogWindowAtStartup", true);
            GUI.Theming.EnableDarkMode = GetBooleanSetting(source, "EnableDarkMode", false);
            GUI.Theming.EnablePurpMode = GetBooleanSetting(source, "EnablePurpMode", false);
            GUI.Theming.CustomBackgroundColor = GetStringSetting(source, "CustomBackgroundColor", null);
            GUI.Theming.CustomTextColor = GetStringSetting(source, "CustomTextColor", null);

            GUI.FastUpdateLabel = GetBooleanSetting(source, "FastUpdateLabel", false);
            GUI.IgnoreFixedDrives = GetBooleanSetting(source, "IgnoreFixedDrives", true);
            GUI.SkipSystemDetection = GetBooleanSetting(source, "SkipSystemDetection", false);

            Dumping.AaruPath = GetStringSetting(source, "AaruPath", DumpSettings.DefaultAaruPath) ?? DumpSettings.DefaultAaruPath;
            Dumping.DiscImageCreatorPath = GetStringSetting(source, "DiscImageCreatorPath", DumpSettings.DefaultDiscImageCreatorPath) ?? DumpSettings.DefaultDiscImageCreatorPath;
            Dumping.DreamdumpPath = GetStringSetting(source, "DreamdumpPath", DumpSettings.DefaultDreamdumpPath) ?? DumpSettings.DefaultDreamdumpPath;
            Dumping.RedumperPath = GetStringSetting(source, "RedumperPath", DumpSettings.DefaultRedumperPath) ?? DumpSettings.DefaultRedumperPath;

            Dumping.DefaultOutputPath = GetStringSetting(source, "DefaultOutputPath", "ISO");
            valueString = GetStringSetting(source, "DefaultSystem", RedumpSystem.IBMPCcompatible.ToString());
            Dumping.DefaultSystem = (valueString ?? string.Empty).ToRedumpSystem();

            Dumping.DumpSpeeds.PreferredCD = GetInt32Setting(source, "PreferredDumpSpeedCD", 24);
            Dumping.DumpSpeeds.PreferredDVD = GetInt32Setting(source, "PreferredDumpSpeedDVD", 16);
            Dumping.DumpSpeeds.PreferredHDDVD = GetInt32Setting(source, "PreferredDumpSpeedHDDVD", 8);
            Dumping.DumpSpeeds.PreferredBD = GetInt32Setting(source, "PreferredDumpSpeedBD", 8);

            Dumping.Aaru.EnableDebug = GetBooleanSetting(source, AaruConstants.EnableDebug, AaruConstants.EnableDebugDefault);
            Dumping.Aaru.EnableVerbose = GetBooleanSetting(source, AaruConstants.EnableVerbose, AaruConstants.EnableVerboseDefault);
            Dumping.Aaru.ForceDumping = GetBooleanSetting(source, AaruConstants.ForceDumping, AaruConstants.ForceDumpingDefault);
            Dumping.Aaru.RereadCount = GetInt32Setting(source, AaruConstants.RereadCount, AaruConstants.RereadCountDefault);
            Dumping.Aaru.StripPersonalData = GetBooleanSetting(source, AaruConstants.StripPersonalData, AaruConstants.StripPersonalDataDefault);

            Dumping.DIC.MultiSectorRead = GetBooleanSetting(source, DiscImageCreatorConstants.MultiSectorRead, DiscImageCreatorConstants.MultiSectorReadDefault);
            Dumping.DIC.MultiSectorReadValue = GetInt32Setting(source, DiscImageCreatorConstants.MultiSectorReadValue, DiscImageCreatorConstants.MultiSectorReadValueDefault);
            Dumping.DIC.ParanoidMode = GetBooleanSetting(source, DiscImageCreatorConstants.ParanoidMode, DiscImageCreatorConstants.ParanoidModeDefault);
            Dumping.DIC.QuietMode = GetBooleanSetting(source, DiscImageCreatorConstants.QuietMode, DiscImageCreatorConstants.QuietModeDefault);
            Dumping.DIC.RereadCount = GetInt32Setting(source, DiscImageCreatorConstants.RereadCount, DiscImageCreatorConstants.RereadCountDefault);
            Dumping.DIC.DVDRereadCount = GetInt32Setting(source, DiscImageCreatorConstants.DVDRereadCount, DiscImageCreatorConstants.DVDRereadCountDefault);
            Dumping.DIC.UseCMIFlag = GetBooleanSetting(source, DiscImageCreatorConstants.UseCMIFlag, DiscImageCreatorConstants.UseCMIFlagDefault);

            Dumping.Dreamdump.NonRedumpMode = GetBooleanSetting(source, "DreamdumpNonRedumpMode", false);
            valueString = GetStringSetting(source, DreamdumpConstants.SectorOrder, DreamdumpConstants.SectorOrderDefault.ToString());
            Dumping.Dreamdump.SectorOrder = valueString.ToDreamdumpSectorOrder();
            Dumping.Dreamdump.RereadCount = GetInt32Setting(source, DreamdumpConstants.RereadCount, DreamdumpConstants.RereadCountDefault);

            Dumping.Redumper.EnableSkeleton = GetBooleanSetting(source, RedumperConstants.EnableSkeleton, RedumperConstants.EnableSkeletonDefault);
            Dumping.Redumper.EnableVerbose = GetBooleanSetting(source, RedumperConstants.EnableVerbose, RedumperConstants.EnableVerboseDefault);
            Dumping.Redumper.LeadinRetryCount = GetInt32Setting(source, RedumperConstants.LeadinRetryCount, RedumperConstants.LeadinRetryCountDefault);
            Dumping.Redumper.NonRedumpMode = GetBooleanSetting(source, "RedumperNonRedumpMode", false);
            valueString = GetStringSetting(source, RedumperConstants.DriveType, RedumperConstants.DriveTypeDefault.ToString());
            Dumping.Redumper.DriveType = valueString.ToRedumperDriveType();
            Dumping.Redumper.DrivePregapStart = GetInt32Setting(source, RedumperConstants.DrivePregapStart, RedumperConstants.DrivePregapStartDefault);
            valueString = GetStringSetting(source, RedumperConstants.ReadMethod, RedumperConstants.ReadMethodDefault.ToString());
            Dumping.Redumper.ReadMethod = valueString.ToRedumperReadMethod();
            valueString = GetStringSetting(source, RedumperConstants.SectorOrder, RedumperConstants.SectorOrderDefault.ToString());
            Dumping.Redumper.SectorOrder = valueString.ToRedumperSectorOrder();
            Dumping.Redumper.RereadCount = GetInt32Setting(source, RedumperConstants.RereadCount, RedumperConstants.RereadCountDefault);
            Dumping.Redumper.RefineSectorMode = GetBooleanSetting(source, RedumperConstants.RefineSectorMode, RedumperConstants.RefineSectorModeDefault);

            Processing.ProtectionScanning.ScanForProtection = GetBooleanSetting(source, "ScanForProtection", true);
            Processing.ProtectionScanning.ScanArchivesForProtection = GetBooleanSetting(source, "ScanArchivesForProtection", true);
            Processing.ProtectionScanning.IncludeDebugProtectionInformation = GetBooleanSetting(source, "IncludeDebugProtectionInformation", false);
            Processing.ProtectionScanning.HideDriveLetters = GetBooleanSetting(source, "HideDriveLetters", false);

            Processing.Login.RetrieveMatchInformation = GetBooleanSetting(source, "RetrieveMatchInformation", true);
            Processing.Login.RedumpUsername = GetStringSetting(source, "RedumpUsername", string.Empty);
            Processing.Login.RedumpPassword = GetStringSetting(source, "RedumpPassword", string.Empty);

            Processing.MediaInformation.AddPlaceholders = GetBooleanSetting(source, "AddPlaceholders", true);
            Processing.MediaInformation.PromptForDiscInformation = GetBooleanSetting(source, "PromptForDiscInformation", true);
            Processing.MediaInformation.PullAllInformation = GetBooleanSetting(source, "PullAllInformation", false);
            Processing.MediaInformation.EnableTabsInInputFields = GetBooleanSetting(source, "EnableTabsInInputFields", true);
            Processing.MediaInformation.EnableRedumpCompatibility = GetBooleanSetting(source, "EnableRedumpCompatibility", true);

            Processing.ShowDiscEjectReminder = GetBooleanSetting(source, "ShowDiscEjectReminder", true);
            Processing.AddFilenameSuffix = GetBooleanSetting(source, "AddFilenameSuffix", false);
            Processing.CreateIRDAfterDumping = GetBooleanSetting(source, "CreateIRDAfterDumping", false);
            Processing.OutputSubmissionJSON = GetBooleanSetting(source, "OutputSubmissionJSON", false);
            Processing.IncludeArtifacts = GetBooleanSetting(source, "IncludeArtifacts", false);
            Processing.CompressLogFiles = GetBooleanSetting(source, "CompressLogFiles", true);
            valueString = GetStringSetting(source, "LogCompression", LogCompression.DeflateMaximum.ToString());
            Processing.LogCompression = valueString.ToLogCompression();
            Processing.DeleteUnnecessaryFiles = GetBooleanSetting(source, "DeleteUnnecessaryFiles", false);
        }

        /// <summary>
        /// Constructor that converts from an existing Options object
        /// </summary>
        /// <param name="source">Options object to read from</param>
        public Options(Options? source)
        {
            source ??= new Options();

            FirstRun = source.FirstRun;
            CheckForUpdatesOnStartup = source.CheckForUpdatesOnStartup;
            VerboseLogging = source.VerboseLogging;
            InternalProgram = source.InternalProgram;

            GUI.CopyUpdateUrlToClipboard = source.GUI.CopyUpdateUrlToClipboard;

            GUI.DefaultInterfaceLanguage = source.GUI.DefaultInterfaceLanguage;
            GUI.ShowDebugViewMenuItem = source.GUI.ShowDebugViewMenuItem;
            GUI.OpenLogWindowAtStartup = source.GUI.OpenLogWindowAtStartup;
            GUI.Theming.EnableDarkMode = source.GUI.Theming.EnableDarkMode;
            GUI.Theming.EnablePurpMode = source.GUI.Theming.EnablePurpMode;
            GUI.Theming.CustomBackgroundColor = source.GUI.Theming.CustomBackgroundColor;
            GUI.Theming.CustomTextColor = source.GUI.Theming.CustomTextColor;

            GUI.FastUpdateLabel = source.GUI.FastUpdateLabel;
            GUI.IgnoreFixedDrives = source.GUI.IgnoreFixedDrives;
            GUI.SkipSystemDetection = source.GUI.SkipSystemDetection;

            Dumping.AaruPath = source.Dumping.AaruPath;
            Dumping.DiscImageCreatorPath = source.Dumping.DiscImageCreatorPath;
            Dumping.DreamdumpPath = source.Dumping.DreamdumpPath;
            Dumping.RedumperPath = source.Dumping.RedumperPath;

            Dumping.DefaultOutputPath = source.Dumping.DefaultOutputPath;
            Dumping.DefaultSystem = source.Dumping.DefaultSystem;

            Dumping.DumpSpeeds.PreferredCD = source.Dumping.DumpSpeeds.PreferredCD;
            Dumping.DumpSpeeds.PreferredDVD = source.Dumping.DumpSpeeds.PreferredDVD;
            Dumping.DumpSpeeds.PreferredHDDVD = source.Dumping.DumpSpeeds.PreferredHDDVD;
            Dumping.DumpSpeeds.PreferredBD = source.Dumping.DumpSpeeds.PreferredBD;

            Dumping.Aaru.EnableDebug = source.Dumping.Aaru.EnableDebug;
            Dumping.Aaru.EnableVerbose = source.Dumping.Aaru.EnableVerbose;
            Dumping.Aaru.ForceDumping = source.Dumping.Aaru.ForceDumping;
            Dumping.Aaru.RereadCount = source.Dumping.Aaru.RereadCount;
            Dumping.Aaru.StripPersonalData = source.Dumping.Aaru.StripPersonalData;

            Dumping.DIC.MultiSectorRead = source.Dumping.DIC.MultiSectorRead;
            Dumping.DIC.MultiSectorReadValue = source.Dumping.DIC.MultiSectorReadValue;
            Dumping.DIC.ParanoidMode = source.Dumping.DIC.ParanoidMode;
            Dumping.DIC.QuietMode = source.Dumping.DIC.QuietMode;
            Dumping.DIC.RereadCount = source.Dumping.DIC.RereadCount;
            Dumping.DIC.DVDRereadCount = source.Dumping.DIC.DVDRereadCount;
            Dumping.DIC.UseCMIFlag = source.Dumping.DIC.UseCMIFlag;

            Dumping.Dreamdump.NonRedumpMode = source.Dumping.Dreamdump.NonRedumpMode;
            Dumping.Dreamdump.SectorOrder = source.Dumping.Dreamdump.SectorOrder;
            Dumping.Dreamdump.RereadCount = source.Dumping.Dreamdump.RereadCount;

            Dumping.Redumper.EnableSkeleton = source.Dumping.Redumper.EnableSkeleton;
            Dumping.Redumper.EnableVerbose = source.Dumping.Redumper.EnableVerbose;
            Dumping.Redumper.LeadinRetryCount = source.Dumping.Redumper.LeadinRetryCount;
            Dumping.Redumper.NonRedumpMode = source.Dumping.Redumper.NonRedumpMode;
            Dumping.Redumper.DriveType = source.Dumping.Redumper.DriveType;
            Dumping.Redumper.DrivePregapStart = source.Dumping.Redumper.DrivePregapStart;
            Dumping.Redumper.ReadMethod = source.Dumping.Redumper.ReadMethod;
            Dumping.Redumper.SectorOrder = source.Dumping.Redumper.SectorOrder;
            Dumping.Redumper.RereadCount = source.Dumping.Redumper.RereadCount;
            Dumping.Redumper.RefineSectorMode = source.Dumping.Redumper.RefineSectorMode;

            Processing.ProtectionScanning.ScanForProtection = source.Processing.ProtectionScanning.ScanForProtection;
            Processing.ProtectionScanning.ScanArchivesForProtection = source.Processing.ProtectionScanning.ScanArchivesForProtection;
            Processing.ProtectionScanning.IncludeDebugProtectionInformation = source.Processing.ProtectionScanning.IncludeDebugProtectionInformation;
            Processing.ProtectionScanning.HideDriveLetters = source.Processing.ProtectionScanning.HideDriveLetters;

            Processing.Login.RetrieveMatchInformation = source.Processing.Login.RetrieveMatchInformation;
            Processing.Login.RedumpUsername = source.Processing.Login.RedumpUsername;
            Processing.Login.RedumpPassword = source.Processing.Login.RedumpPassword;

            Processing.MediaInformation.AddPlaceholders = source.Processing.MediaInformation.AddPlaceholders;
            Processing.MediaInformation.PromptForDiscInformation = source.Processing.MediaInformation.PromptForDiscInformation;
            Processing.MediaInformation.PullAllInformation = source.Processing.MediaInformation.PullAllInformation;
            Processing.MediaInformation.EnableTabsInInputFields = source.Processing.MediaInformation.EnableTabsInInputFields;
            Processing.MediaInformation.EnableRedumpCompatibility = source.Processing.MediaInformation.EnableRedumpCompatibility;

            Processing.ShowDiscEjectReminder = source.Processing.ShowDiscEjectReminder;
            Processing.AddFilenameSuffix = source.Processing.AddFilenameSuffix;
            Processing.CreateIRDAfterDumping = source.Processing.CreateIRDAfterDumping;
            Processing.OutputSubmissionJSON = source.Processing.OutputSubmissionJSON;
            Processing.IncludeArtifacts = source.Processing.IncludeArtifacts;
            Processing.CompressLogFiles = source.Processing.CompressLogFiles;
            Processing.LogCompression = source.Processing.LogCompression;
            Processing.DeleteUnnecessaryFiles = source.Processing.DeleteUnnecessaryFiles;
        }

        #endregion

        /// <summary>
        /// Convert to a dictionary
        /// </summary>
        internal Dictionary<string, string?> ConvertToDictionary()
        {
            return new Dictionary<string, string?>
            {
                { "FirstRun", FirstRun.ToString()},

                { "AaruPath", Dumping.AaruPath },
                { "DiscImageCreatorPath", Dumping.DiscImageCreatorPath },
                { "DreamdumpPath", Dumping.DreamdumpPath },
                { "RedumperPath", Dumping.RedumperPath },
                { "InternalProgram", InternalProgram.ToString() },

                { "EnableDarkMode", GUI.Theming.EnableDarkMode.ToString() },
                { "EnablePurpMode", GUI.Theming.EnablePurpMode.ToString() },
                { "CustomBackgroundColor", GUI.Theming.CustomBackgroundColor },
                { "CustomTextColor", GUI.Theming.CustomTextColor },
                { "CheckForUpdatesOnStartup", CheckForUpdatesOnStartup.ToString() },
                { "CopyUpdateUrlToClipboard", GUI.CopyUpdateUrlToClipboard.ToString() },
                { "FastUpdateLabel", GUI.FastUpdateLabel.ToString() },
                { "DefaultInterfaceLanguage", GUI.DefaultInterfaceLanguage.ShortName() },
                { "DefaultOutputPath", Dumping.DefaultOutputPath },
                { "DefaultSystem", Dumping.DefaultSystem.ToString() },
                { "ShowDebugViewMenuItem", GUI.ShowDebugViewMenuItem.ToString() },

                { "PreferredDumpSpeedCD", Dumping.DumpSpeeds.PreferredCD.ToString() },
                { "PreferredDumpSpeedDVD", Dumping.DumpSpeeds.PreferredDVD.ToString() },
                { "PreferredDumpSpeedHDDVD", Dumping.DumpSpeeds.PreferredHDDVD.ToString() },
                { "PreferredDumpSpeedBD", Dumping.DumpSpeeds.PreferredBD.ToString() },

                { AaruConstants.EnableDebug, Dumping.Aaru.EnableDebug.ToString() },
                { AaruConstants.EnableVerbose, Dumping.Aaru.EnableVerbose.ToString() },
                { AaruConstants.ForceDumping, Dumping.Aaru.ForceDumping.ToString() },
                { AaruConstants.RereadCount, Dumping.Aaru.RereadCount.ToString() },
                { AaruConstants.StripPersonalData, Dumping.Aaru.StripPersonalData.ToString() },

                { DiscImageCreatorConstants.MultiSectorRead, Dumping.DIC.MultiSectorRead.ToString() },
                { DiscImageCreatorConstants.MultiSectorReadValue, Dumping.DIC.MultiSectorReadValue.ToString() },
                { DiscImageCreatorConstants.ParanoidMode, Dumping.DIC.ParanoidMode.ToString() },
                { DiscImageCreatorConstants.QuietMode, Dumping.DIC.QuietMode.ToString() },
                { DiscImageCreatorConstants.RereadCount, Dumping.DIC.RereadCount.ToString() },
                { DiscImageCreatorConstants.DVDRereadCount, Dumping.DIC.DVDRereadCount.ToString() },
                { DiscImageCreatorConstants.UseCMIFlag, Dumping.DIC.UseCMIFlag.ToString() },

                { "DreamdumpNonRedumpMode", Dumping.Dreamdump.NonRedumpMode.ToString() },
                { DreamdumpConstants.SectorOrder, Dumping.Dreamdump.SectorOrder.ToString() },
                { DreamdumpConstants.RereadCount, Dumping.Dreamdump.RereadCount.ToString() },

                { RedumperConstants.EnableSkeleton, Dumping.Redumper.EnableSkeleton.ToString() },
                { RedumperConstants.EnableVerbose, Dumping.Redumper.EnableVerbose.ToString() },
                { RedumperConstants.LeadinRetryCount, Dumping.Redumper.LeadinRetryCount.ToString() },
                { "RedumperNonRedumpMode", Dumping.Redumper.NonRedumpMode.ToString() },
                { RedumperConstants.DriveType, Dumping.Redumper.DriveType.ToString() },
                { RedumperConstants.DrivePregapStart, Dumping.Redumper.DrivePregapStart.ToString() },
                { RedumperConstants.ReadMethod, Dumping.Redumper.ReadMethod.ToString() },
                { RedumperConstants.SectorOrder, Dumping.Redumper.SectorOrder.ToString() },
                { RedumperConstants.RereadCount, Dumping.Redumper.RereadCount.ToString() },
                { RedumperConstants.RefineSectorMode, Dumping.Redumper.RefineSectorMode.ToString() },

                { "ScanForProtection", Processing.ProtectionScanning.ScanForProtection.ToString() },
                { "AddPlaceholders", Processing.MediaInformation.AddPlaceholders.ToString() },
                { "PromptForDiscInformation", Processing.MediaInformation.PromptForDiscInformation.ToString() },
                { "PullAllInformation", Processing.MediaInformation.PullAllInformation.ToString() },
                { "EnableTabsInInputFields", Processing.MediaInformation.EnableTabsInInputFields.ToString() },
                { "EnableRedumpCompatibility", Processing.MediaInformation.EnableRedumpCompatibility.ToString() },
                { "ShowDiscEjectReminder", Processing.ShowDiscEjectReminder.ToString() },
                { "IgnoreFixedDrives", GUI.IgnoreFixedDrives.ToString() },
                { "AddFilenameSuffix", Processing.AddFilenameSuffix.ToString() },
                { "OutputSubmissionJSON", Processing.OutputSubmissionJSON.ToString() },
                { "IncludeArtifacts", Processing.IncludeArtifacts.ToString() },
                { "CompressLogFiles", Processing.CompressLogFiles.ToString() },
                { "LogCompression", Processing.LogCompression.ToString() },
                { "DeleteUnnecessaryFiles", Processing.DeleteUnnecessaryFiles.ToString() },
                { "CreateIRDAfterDumping", Processing.CreateIRDAfterDumping.ToString() },

                { "SkipSystemDetection", GUI.SkipSystemDetection.ToString() },

                { "ScanArchivesForProtection", Processing.ProtectionScanning.ScanArchivesForProtection.ToString() },
                { "IncludeDebugProtectionInformation", Processing.ProtectionScanning.IncludeDebugProtectionInformation.ToString() },
                { "HideDriveLetters", Processing.ProtectionScanning.HideDriveLetters.ToString() },

                { "VerboseLogging", VerboseLogging.ToString() },
                { "OpenLogWindowAtStartup", GUI.OpenLogWindowAtStartup.ToString() },

                { "RetrieveMatchInformation", Processing.Login.RetrieveMatchInformation.ToString() },
                { "RedumpUsername", Processing.Login.RedumpUsername },
                { "RedumpPassword", Processing.Login.RedumpPassword },
            };
        }

        #region Helpers

        /// <summary>
        /// Get a Boolean setting from a settings, dictionary
        /// </summary>
        /// <param name="settings">Dictionary representing the settings</param>
        /// <param name="key">Setting key to get a value for</param>
        /// <param name="defaultValue">Default value to return if no value is found</param>
        /// <returns>Setting value if possible, default value otherwise</returns>
        internal static bool GetBooleanSetting(Dictionary<string, string?> settings, string key, bool defaultValue)
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
        internal static int GetInt32Setting(Dictionary<string, string?> settings, string key, int defaultValue)
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
        internal static string? GetStringSetting(Dictionary<string, string?> settings, string key, string? defaultValue)
        {
            if (settings.ContainsKey(key))
                return settings[key];
            else
                return defaultValue;
        }

        #endregion
    }

    #region Nested Option Types

    /// <summary>
    /// Settings related to the dumping step
    /// </summary>
    public class DumpSettings
    {
        #region Default Paths

        /// <summary>
        /// Default Aaru path
        /// </summary>
        internal static string DefaultAaruPath
        {
            get
            {
#pragma warning disable IDE0072
                string executableName = Environment.OSVersion.Platform switch
                {
                    PlatformID.Unix => "aaru",
                    PlatformID.MacOSX => "aaru",
                    _ => "aaru.exe"
                };
#pragma warning restore IDE0072

#if NET20 || NET35
                return Path.Combine("Programs", Path.Combine("Aaru", executableName));
#else
                return Path.Combine("Programs", "Aaru", executableName);
#endif
            }
        }

        /// <summary>
        /// Default DiscImageCreator path
        /// </summary>
        internal static string DefaultDiscImageCreatorPath
        {
            get
            {
#pragma warning disable IDE0072
                string executableName = Environment.OSVersion.Platform switch
                {
                    PlatformID.Unix => "DiscImageCreator.out",
                    PlatformID.MacOSX => "DiscImageCreator",
                    _ => "DiscImageCreator.exe"
                };
#pragma warning restore IDE0072

#if NET20 || NET35
                return Path.Combine("Programs", Path.Combine("Creator", executableName));
#else
                return Path.Combine("Programs", "Creator", executableName);
#endif
            }
        }

        /// <summary>
        /// Default Dreamdump path
        /// </summary>
        internal static string DefaultDreamdumpPath
        {
            get
            {
#pragma warning disable IDE0072
                string executableName = Environment.OSVersion.Platform switch
                {
                    PlatformID.Unix => "dreamdump",
                    PlatformID.MacOSX => "dreamdump",
                    _ => "dreamdump.exe"
                };
#pragma warning restore IDE0072

#if NET20 || NET35
                return Path.Combine("Programs", Path.Combine("Dreamdump", executableName));
#else
                return Path.Combine("Programs", "Dreamdump", executableName);
#endif
            }
        }

        /// <summary>
        /// Default Redumper path
        /// </summary>
        internal static string DefaultRedumperPath
        {
            get
            {
#pragma warning disable IDE0072
                string executableName = Environment.OSVersion.Platform switch
                {
                    PlatformID.Unix => "redumper",
                    PlatformID.MacOSX => "redumper",
                    _ => "redumper.exe"
                };
#pragma warning restore IDE0072

#if NET20 || NET35
                return Path.Combine("Programs", Path.Combine("Redumper", executableName));
#else
                return Path.Combine("Programs", "Redumper", executableName);
#endif
            }
        }

        #endregion

        #region Internal Program

        /// <summary>
        /// Path to Aaru
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public string AaruPath
        {
            get;
            set
            {
                field = value ?? DefaultAaruPath;
            }
        } = DefaultAaruPath;

        /// <summary>
        /// Path to DiscImageCreator
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public string DiscImageCreatorPath
        {
            get;
            set
            {
                field = value ?? DefaultDiscImageCreatorPath;
            }
        } = DefaultDiscImageCreatorPath;

        /// <summary>
        /// Path to Dreamdump
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public string DreamdumpPath
        {
            get;
            set
            {
                field = value ?? DefaultDreamdumpPath;
            }
        } = DefaultDreamdumpPath;

        /// <summary>
        /// Path to Redumper
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public string RedumperPath
        {
            get;
            set
            {
                field = value ?? DefaultRedumperPath;
            }
        } = DefaultRedumperPath;

        #endregion

        #region Default Values

        /// <summary>
        /// Default output path for dumps
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public string? DefaultOutputPath
        {
            get;
            set
            {
                field = value ?? "ISO";
            }
        } = "ISO";

        /// <summary>
        /// Default system if none can be detected
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public RedumpSystem? DefaultSystem { get; set; } = RedumpSystem.IBMPCcompatible;

        #endregion

        #region Dumping Speeds

        /// <summary>
        /// Default preferred dumping speeds per media type
        /// </summary>
        public DumpSpeeds DumpSpeeds { get; set; } = new DumpSpeeds();

        #endregion

        #region Per-Program

        /// <summary>
        /// Aaru-specific dump settings
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public AaruDumpSettings Aaru { get; set; } = new AaruDumpSettings();

        /// <summary>
        /// DiscImageCreator-specific dump settings
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public DiscImageCreatorDumpSettings DIC { get; set; } = new DiscImageCreatorDumpSettings();

        /// <summary>
        /// Dreamdump-specific dump settings
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public DreamdumpDumpSettings Dreamdump { get; set; } = new DreamdumpDumpSettings();

        /// <summary>
        /// Redumper-specific dump settings
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public RedumperDumpSettings Redumper { get; set; } = new RedumperDumpSettings();

        #endregion
    }

    /// <summary>
    /// Settings related to dumping speeds
    /// </summary>
    public class DumpSpeeds
    {
        /// <summary>
        /// Default CD dumping speed
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int PreferredCD { get; set; } = 24;

        /// <summary>
        /// Default DVD dumping speed
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int PreferredDVD { get; set; } = 16;

        /// <summary>
        /// Default HD-DVD dumping speed
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int PreferredHDDVD { get; set; } = 8;

        /// <summary>
        /// Default BD dumping speed
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int PreferredBD { get; set; } = 8;
    }

    /// <summary>
    /// Settings related to the GUI
    /// </summary>
    public class GuiSettings
    {
        #region Startup

        /// <summary>
        /// Try to copy the update URL to the clipboard if one is found
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool CopyUpdateUrlToClipboard { get; set; } = true;

        #endregion

        #region Interface

        /// <summary>
        /// Default interface language to launch MPF into
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public InterfaceLanguage DefaultInterfaceLanguage { get; set; } = InterfaceLanguage.AutoDetect;

        /// <summary>
        /// Show the debug menu item
        /// </summary>
        /// <remarks>Version 1 and greater; This is a hidden setting</remarks>
        public bool ShowDebugViewMenuItem { get; set; } = false;

        /// <summary>
        /// Have the log panel expanded by default on startup
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool OpenLogWindowAtStartup { get; set; } = true;

        /// <summary>
        /// Theme settings
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public ThemeSettings Theming { get; set; } = new ThemeSettings();

        #endregion

        #region Input

        /// <summary>
        /// Fast update label. Skips disc checks and updates path only
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool FastUpdateLabel { get; set; } = false;

        /// <summary>
        /// Ignore fixed drives when populating the list
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool IgnoreFixedDrives { get; set; } = true;

        /// <summary>
        /// Skip detecting known system on disc scan
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool SkipSystemDetection { get; set; } = false;

        #endregion
    }

    /// <summary>
    /// Settings related to the media information input
    /// </summary>
    public class MediaInformationSettings
    {
        /// <summary>
        /// Add placeholder values in the submission info
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool AddPlaceholders { get; set; } = true;

        /// <summary>
        /// Show the media information window after dumping
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool PromptForDiscInformation { get; set; } = true;

        /// <summary>
        /// Pull all information from Redump if signed in
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool PullAllInformation { get; set; } = false;

        /// <summary>
        /// Enable tabs in all input fields
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool EnableTabsInInputFields { get; set; } = true;

        /// <summary>
        /// Limit outputs to Redump-supported values only
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool EnableRedumpCompatibility { get; set; } = true;
    }

    /// <summary>
    /// Settings related to the processing step
    /// </summary>
    public class ProcessingSettings
    {
        #region Protection Scanning

        /// <summary>
        /// Protection scanning settings
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public ProtectionScanningSettings ProtectionScanning { get; set; } = new ProtectionScanningSettings();

        #endregion

        #region Site Login

        /// <summary>
        /// Site login information for retrieval
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public SiteLoginSettings Login { get; set; } = new SiteLoginSettings();

        #endregion

        #region User Input

        /// <summary>
        /// Media information input settings
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public MediaInformationSettings MediaInformation { get; set; } = new MediaInformationSettings();

        #endregion

        #region Output

        /// <summary>
        /// Show disc eject reminder before the media information window is shown
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool ShowDiscEjectReminder { get; set; } = true;

        /// <summary>
        /// Add the dump filename as a suffix to the auto-generated files
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool AddFilenameSuffix { get; set; } = false;

        /// <summary>
        /// Create a PS3 IRD file after dumping PS3 BD-ROM discs
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool CreateIRDAfterDumping { get; set; } = false;

        /// <summary>
        /// Output the compressed JSON version of the submission info
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool OutputSubmissionJSON { get; set; } = false;

        /// <summary>
        /// Include log files in serialized JSON data
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool IncludeArtifacts { get; set; } = false;

        /// <summary>
        /// Compress output log files to reduce space
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool CompressLogFiles { get; set; } = true;

        /// <summary>
        /// Compression type used during log compression
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public LogCompression LogCompression { get; set; } = LogCompression.DeflateMaximum;

        /// <summary>
        /// Delete unnecessary files to reduce space
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool DeleteUnnecessaryFiles { get; set; } = false;

        #endregion
    }

    /// <summary>
    /// Settings related to protection scanning
    /// </summary>
    public class ProtectionScanningSettings
    {
        /// <summary>
        /// Scan the disc for protection after dumping
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool ScanForProtection { get; set; } = true;

        /// <summary>
        /// Scan archive contents during protection scanning
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool ScanArchivesForProtection { get; set; } = true;

        /// <summary>
        /// Include debug information with scan results
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool IncludeDebugProtectionInformation { get; set; } = false;

        /// <summary>
        /// Remove drive letters from protection scan output
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool HideDriveLetters { get; set; } = false;
    }

    /// <summary>
    /// Settings related to site logins
    /// </summary>
    public class SiteLoginSettings
    {
        /// <summary>
        /// Enable retrieving match information from Redump
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool RetrieveMatchInformation { get; set; } = true;

        /// <remarks>Version 1 and greater</remarks>
        public string? RedumpUsername { get; set; } = string.Empty;

        /// <remarks>Version 1 and greater</remarks>
        // TODO: Figure out a way to keep this encrypted in some way, BASE64 to start?
        public string? RedumpPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Settings related to themes
    /// </summary>
    public class ThemeSettings
    {
        /// <summary>
        /// Enable dark mode for UI elements
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool EnableDarkMode { get; set; } = false;

        /// <summary>
        /// Enable purple mode for UI elements
        /// </summary>
        /// <remarks>Version 1 and greater; This is a hidden setting</remarks>
        public bool EnablePurpMode { get; set; } = false;

        /// <summary>
        /// Custom color setting
        /// </summary>
        /// <remarks>Version 1 and greater; This is a hidden setting</remarks>
        public string? CustomBackgroundColor { get; set; } = null;

        /// <summary>
        /// Custom color setting
        /// </summary>
        /// <remarks>Version 1 and greater; This is a hidden setting</remarks>
        public string? CustomTextColor { get; set; } = null;
    }

    #region Per-Program Dump Settings

    /// <summary>
    /// Settings related to the dumping step (Aaru)
    /// </summary>
    public class AaruDumpSettings
    {
        /// <summary>
        /// Enable debug output while dumping by default
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool EnableDebug { get; set; } = AaruConstants.EnableDebugDefault;

        /// <summary>
        /// Enable verbose output while dumping by default
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool EnableVerbose { get; set; } = AaruConstants.EnableVerboseDefault;

        /// <summary>
        /// Enable force dumping of media by default
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool ForceDumping { get; set; } = AaruConstants.ForceDumpingDefault;

        /// <summary>
        /// Default number of sector/subchannel rereads
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int RereadCount { get; set; } = AaruConstants.RereadCountDefault;

        /// <summary>
        /// Strip personal data information from Aaru metadata by default
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool StripPersonalData { get; set; } = AaruConstants.StripPersonalDataDefault;
    }

    /// <summary>
    /// Settings related to the dumping step (DIC)
    /// </summary>
    public class DiscImageCreatorDumpSettings
    {
        /// <summary>
        /// Enable multi-sector read flag by default
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool MultiSectorRead { get; set; } = DiscImageCreatorConstants.MultiSectorReadDefault;

        /// <summary>
        /// Include a default multi-sector read value
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int MultiSectorReadValue { get; set; } = DiscImageCreatorConstants.MultiSectorReadValueDefault;

        /// <summary>
        /// Enable overly-secure dumping flags by default
        /// </summary>
        /// <remarks>
        /// Version 1 and greater
        /// Split this into component parts later. Currently does:
        /// - Scan sector protection and set subchannel read level to 2 for CD
        /// - Set scan file protect flag for DVD
        /// </remarks>
        public bool ParanoidMode { get; set; } = DiscImageCreatorConstants.ParanoidModeDefault;

        /// <summary>
        /// Enable the Quiet flag by default
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool QuietMode { get; set; } = DiscImageCreatorConstants.QuietModeDefault;

        /// <summary>
        /// Default number of C2 rereads
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int RereadCount { get; set; } = DiscImageCreatorConstants.RereadCountDefault;

        /// <summary>
        /// Default number of DVD/HD-DVD/BD rereads
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int DVDRereadCount { get; set; } = DiscImageCreatorConstants.DVDRereadCountDefault;

        /// <summary>
        /// Use the CMI flag for supported disc types
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool UseCMIFlag { get; set; } = DiscImageCreatorConstants.UseCMIFlagDefault;
    }

    /// <summary>
    /// Settings related to the dumping step (Dreamdump)
    /// </summary>
    public class DreamdumpDumpSettings
    {
        /// <summary>
        /// Enable options incompatible with redump submissions
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool NonRedumpMode { get; set; } = false;

        /// <summary>
        /// Currently selected default Dreamdump sector order
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public ExecutionContexts.Dreamdump.SectorOrder SectorOrder { get; set; } = DreamdumpConstants.SectorOrderDefault;

        /// <summary>
        /// Default number of rereads
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int RereadCount { get; set; } = DreamdumpConstants.RereadCountDefault;
    }

    /// <summary>
    /// Settings related to the dumping step (Redumper)
    /// </summary>
    public class RedumperDumpSettings
    {
        /// <summary>
        /// Enable skeleton output while dumping by default
        /// </summary>
        /// <remarks>Version 1 and greater; This is a hidden setting</remarks>
        public bool EnableSkeleton { get; set; } = RedumperConstants.EnableSkeletonDefault;

        /// <summary>
        /// Enable verbose output while dumping by default
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool EnableVerbose { get; set; } = RedumperConstants.EnableVerboseDefault;

        /// <summary>
        /// Default number of redumper Plextor leadin retries
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int LeadinRetryCount { get; set; } = RedumperConstants.LeadinRetryCountDefault;

        /// <summary>
        /// Enable options incompatible with redump submissions
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool NonRedumpMode { get; set; } = false;

        /// <summary>
        /// Currently selected default redumper drive type
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public ExecutionContexts.Redumper.DriveType DriveType { get; set; } = RedumperConstants.DriveTypeDefault;

        /// <summary>
        /// Currently selected default redumper drive pregap start sector
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int DrivePregapStart { get; set; } = RedumperConstants.DrivePregapStartDefault;

        /// <summary>
        /// Currently selected default redumper read method
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public ExecutionContexts.Redumper.ReadMethod ReadMethod { get; set; } = RedumperConstants.ReadMethodDefault;

        /// <summary>
        /// Currently selected default redumper sector order
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public ExecutionContexts.Redumper.SectorOrder SectorOrder { get; set; } = RedumperConstants.SectorOrderDefault;

        /// <summary>
        /// Default number of rereads
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int RereadCount { get; set; } = RedumperConstants.RereadCountDefault;

        /// <summary>
        /// Enable the refine sector mode flag by default
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool RefineSectorMode { get; set; } = RedumperConstants.RefineSectorModeDefault;
    }

    #endregion

    #endregion
}
