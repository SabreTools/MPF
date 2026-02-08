using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SabreTools.RedumpLib.Data;
using AaruDumpSettings = MPF.ExecutionContexts.Aaru.DumpSettings;
using DiscImageCreatorDumpSettings = MPF.ExecutionContexts.DiscImageCreator.DumpSettings;
using DreamdumpDumpSettings = MPF.ExecutionContexts.Dreamdump.DumpSettings;
using LogCompression = MPF.Processors.LogCompression;
using RedumperDumpSettings = MPF.ExecutionContexts.Redumper.DumpSettings;

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

        #region Constructors

        /// <summary>
        /// Empty constructor for serialization
        /// </summary>
        public Options() { }

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
            GUI.OpenLogWindowAtStartup = source.GUI.OpenLogWindowAtStartup;

            GUI.DefaultInterfaceLanguage = source.GUI.DefaultInterfaceLanguage;
            GUI.ShowDebugViewMenuItem = source.GUI.ShowDebugViewMenuItem;
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
            Dumping.DumpSpeeds.CD = source.Dumping.DumpSpeeds.CD;
            Dumping.DumpSpeeds.DVD = source.Dumping.DumpSpeeds.DVD;
            Dumping.DumpSpeeds.HDDVD = source.Dumping.DumpSpeeds.HDDVD;
            Dumping.DumpSpeeds.Bluray = source.Dumping.DumpSpeeds.Bluray;

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
            Processing.ProtectionScanning.HideDriveLetters = source.Processing.ProtectionScanning.HideDriveLetters;
            Processing.ProtectionScanning.IncludeDebugProtectionInformation = source.Processing.ProtectionScanning.IncludeDebugProtectionInformation;

            Processing.Login.PullAllInformation = source.Processing.Login.PullAllInformation;
            Processing.Login.RedumpUsername = source.Processing.Login.RedumpUsername;
            Processing.Login.RedumpPassword = source.Processing.Login.RedumpPassword;
            Processing.Login.RetrieveMatchInformation = source.Processing.Login.RetrieveMatchInformation;

            Processing.MediaInformation.AddPlaceholders = source.Processing.MediaInformation.AddPlaceholders;
            Processing.MediaInformation.EnableRedumpCompatibility = source.Processing.MediaInformation.EnableRedumpCompatibility;
            Processing.MediaInformation.EnableTabsInInputFields = source.Processing.MediaInformation.EnableTabsInInputFields;
            Processing.MediaInformation.PromptForDiscInformation = source.Processing.MediaInformation.PromptForDiscInformation;

            Processing.AddFilenameSuffix = source.Processing.AddFilenameSuffix;
            Processing.CompressLogFiles = source.Processing.CompressLogFiles;
            Processing.CreateIRDAfterDumping = source.Processing.CreateIRDAfterDumping;
            Processing.DeleteUnnecessaryFiles = source.Processing.DeleteUnnecessaryFiles;
            Processing.IncludeArtifacts = source.Processing.IncludeArtifacts;
            Processing.LogCompression = source.Processing.LogCompression;
            Processing.OutputSubmissionJSON = source.Processing.OutputSubmissionJSON;
            Processing.ShowDiscEjectReminder = source.Processing.ShowDiscEjectReminder;
        }

        #endregion

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
    /// Context-specific settings that can be used by caller
    /// </summary>
    public class DumpSettings
    {
        #region Default Paths

        /// <summary>
        /// Default Aaru path
        /// </summary>
        [JsonIgnore]
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
        [JsonIgnore]
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
        [JsonIgnore]
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
        [JsonIgnore]
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
        public int CD { get; set; } = 24;

        /// <summary>
        /// Default DVD dumping speed
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int DVD { get; set; } = 16;

        /// <summary>
        /// Default HD-DVD dumping speed
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int HDDVD { get; set; } = 8;

        /// <summary>
        /// Default BD dumping speed
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int Bluray { get; set; } = 8;
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

        /// <summary>
        /// Have the log panel expanded by default on startup
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool OpenLogWindowAtStartup { get; set; } = true;

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
        /// Limit outputs to Redump-supported values only
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool EnableRedumpCompatibility { get; set; } = true;

        /// <summary>
        /// Enable tabs in all input fields
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool EnableTabsInInputFields { get; set; } = true;

        /// <summary>
        /// Show the media information window after dumping
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool PromptForDiscInformation { get; set; } = true;
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

        #region Post-Information

        /// <summary>
        /// Add the dump filename as a suffix to the auto-generated files
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool AddFilenameSuffix { get; set; } = false;

        /// <summary>
        /// Compress output log files to reduce space
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool CompressLogFiles { get; set; } = true;

        /// <summary>
        /// Create a PS3 IRD file after dumping PS3 BD-ROM discs
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool CreateIRDAfterDumping { get; set; } = false;

        /// <summary>
        /// Delete unnecessary files to reduce space
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool DeleteUnnecessaryFiles { get; set; } = false;

        /// <summary>
        /// Include log files in serialized JSON data
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool IncludeArtifacts { get; set; } = false;

        /// <summary>
        /// Compression type used during log compression
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public LogCompression LogCompression { get; set; } = LogCompression.DeflateMaximum;

        /// <summary>
        /// Output the compressed JSON version of the submission info
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool OutputSubmissionJSON { get; set; } = false;

        /// <summary>
        /// Show disc eject reminder before the media information window is shown
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool ShowDiscEjectReminder { get; set; } = true;

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
        /// Remove drive letters from protection scan output
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool HideDriveLetters { get; set; } = false;

        /// <summary>
        /// Include debug information with scan results
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool IncludeDebugProtectionInformation { get; set; } = false;
    }

    /// <summary>
    /// Settings related to site logins
    /// </summary>
    public class SiteLoginSettings
    {
        /// <summary>
        /// Pull all information from Redump if signed in
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool PullAllInformation { get; set; } = false;

        /// <summary>
        /// Username for Redump, requires <see cref="RedumpPassword"/>
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public string? RedumpUsername { get; set; } = string.Empty;

        /// <summary>
        /// Password for Redump, requires <see cref="RedumpUsername"/>
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        // TODO: Figure out a way to keep this encrypted in some way, BASE64 to start?
        public string? RedumpPassword { get; set; } = string.Empty;

        /// <summary>
        /// Enable retrieving match information from Redump
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool RetrieveMatchInformation { get; set; } = true;
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

    #endregion
}
