using System;
using System.IO;
using SabreTools.RedumpLib.Data;
using AaruConstants = MPF.ExecutionContexts.Aaru.SettingConstants;
using DiscImageCreatorConstants = MPF.ExecutionContexts.DiscImageCreator.SettingConstants;
using DreamdumpConstants = MPF.ExecutionContexts.Dreamdump.SettingConstants;
using RedumperConstants = MPF.ExecutionContexts.Redumper.SettingConstants;

namespace MPF.Frontend
{
    /// <summary>
    /// Options that use nested types for setting arrangement
    /// </summary>
    public class SegmentedOptions
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
        public SegmentedOptions() { }

        /// <summary>
        /// Constructor that converts from an existing Options object
        /// </summary>
        /// <param name="source">Options object to read from</param>
        /// TODO: Remove when Options is no longer relevant
        public SegmentedOptions(Options? source)
        {
            source ??= new Options();

            FirstRun = source.FirstRun;
            CheckForUpdatesOnStartup = source.CheckForUpdatesOnStartup;
            VerboseLogging = source.VerboseLogging;
            InternalProgram = source.InternalProgram;

            GUI.CopyUpdateUrlToClipboard = source.CopyUpdateUrlToClipboard;

            GUI.DefaultInterfaceLanguage = source.DefaultInterfaceLanguage;
            GUI.ShowDebugViewMenuItem = source.ShowDebugViewMenuItem;
            GUI.OpenLogWindowAtStartup = source.OpenLogWindowAtStartup;
            GUI.Theming.EnableDarkMode = source.EnableDarkMode;
            GUI.Theming.EnablePurpMode = source.EnablePurpMode;
            GUI.Theming.CustomBackgroundColor = source.CustomBackgroundColor;
            GUI.Theming.CustomTextColor = source.CustomTextColor;

            GUI.FastUpdateLabel = source.FastUpdateLabel;
            GUI.IgnoreFixedDrives = source.IgnoreFixedDrives;
            GUI.SkipSystemDetection = source.SkipSystemDetection;

            Dumping.AaruPath = source.AaruPath ?? DumpSettings.DefaultAaruPath;
            Dumping.DiscImageCreatorPath = source.DiscImageCreatorPath ?? DumpSettings.DefaultDiscImageCreatorPath;
            // Dumping.DreamdumpPath = source.DreamdumpPath ?? DumpSettings.DefaultDreamdumpPath;
            Dumping.RedumperPath = source.RedumperPath ?? DumpSettings.DefaultRedumperPath;

            Dumping.DefaultOutputPath = source.DefaultOutputPath;
            Dumping.DefaultSystem = source.DefaultSystem;

            Dumping.DumpSpeeds.PreferredCD = source.PreferredDumpSpeedCD;
            Dumping.DumpSpeeds.PreferredDVD = source.PreferredDumpSpeedDVD;
            Dumping.DumpSpeeds.PreferredHDDVD = source.PreferredDumpSpeedHDDVD;
            Dumping.DumpSpeeds.PreferredBD = source.PreferredDumpSpeedBD;

            Dumping.Aaru.EnableDebug = source.AaruEnableDebug;
            Dumping.Aaru.EnableVerbose = source.AaruEnableVerbose;
            Dumping.Aaru.ForceDumping = source.AaruForceDumping;
            Dumping.Aaru.RereadCount = source.AaruRereadCount;
            Dumping.Aaru.StripPersonalData = source.AaruStripPersonalData;

            Dumping.DIC.MultiSectorRead = source.DICMultiSectorRead;
            Dumping.DIC.MultiSectorReadValue = source.DICMultiSectorReadValue;
            Dumping.DIC.ParanoidMode = source.DICParanoidMode;
            Dumping.DIC.QuietMode = source.DICQuietMode;
            Dumping.DIC.RereadCount = source.DICRereadCount;
            Dumping.DIC.DVDRereadCount = source.DICDVDRereadCount;
            Dumping.DIC.UseCMIFlag = source.DICUseCMIFlag;

            // Dumping.Dreamdump.NonRedumpMode = source.DreamdumpNonRedumpMode;
            // Dumping.Dreamdump.SectorOrder = source.DreamdumpSectorOrder;
            // Dumping.Dreamdump.RereadCount = source.DreamdumpRereadCount;

            Dumping.Redumper.EnableSkeleton = source.RedumperEnableSkeleton;
            Dumping.Redumper.EnableVerbose = source.RedumperEnableVerbose;
            Dumping.Redumper.LeadinRetryCount = source.RedumperLeadinRetryCount;
            Dumping.Redumper.NonRedumpMode = source.RedumperNonRedumpMode;
            Dumping.Redumper.DriveType = source.RedumperDriveType;
            Dumping.Redumper.DrivePregapStart = source.RedumperDrivePregapStart;
            Dumping.Redumper.ReadMethod = source.RedumperReadMethod;
            Dumping.Redumper.SectorOrder = source.RedumperSectorOrder;
            Dumping.Redumper.RereadCount = source.RedumperRereadCount;
            Dumping.Redumper.RefineSectorMode = source.RedumperRefineSectorMode;

            Processing.ProtectionScanning.ScanForProtection = source.ScanForProtection;
            Processing.ProtectionScanning.ScanArchivesForProtection = source.ScanArchivesForProtection;
            Processing.ProtectionScanning.IncludeDebugProtectionInformation = source.IncludeDebugProtectionInformation;
            Processing.ProtectionScanning.HideDriveLetters = source.HideDriveLetters;

            Processing.Login.RetrieveMatchInformation = source.RetrieveMatchInformation;
            Processing.Login.RedumpUsername = source.RedumpUsername;
            Processing.Login.RedumpPassword = source.RedumpPassword;

            Processing.MediaInformation.AddPlaceholders = source.AddPlaceholders;
            Processing.MediaInformation.PromptForDiscInformation = source.PromptForDiscInformation;
            Processing.MediaInformation.PullAllInformation = source.PullAllInformation;
            Processing.MediaInformation.EnableTabsInInputFields = source.EnableTabsInInputFields;
            Processing.MediaInformation.EnableRedumpCompatibility = source.EnableRedumpCompatibility;

            Processing.ShowDiscEjectReminder = source.ShowDiscEjectReminder;
            Processing.AddFilenameSuffix = source.AddFilenameSuffix;
            Processing.CreateIRDAfterDumping = source.CreateIRDAfterDumping;
            Processing.OutputSubmissionJSON = source.OutputSubmissionJSON;
            Processing.IncludeArtifacts = source.IncludeArtifacts;
            Processing.CompressLogFiles = source.CompressLogFiles;
            Processing.LogCompression = source.LogCompression;
            Processing.DeleteUnnecessaryFiles = source.DeleteUnnecessaryFiles;
        }

        /// <summary>
        /// Constructor that converts from an existing SegmentedOptions object
        /// </summary>
        /// <param name="source">SegmentedOptions object to read from</param>
        public SegmentedOptions(SegmentedOptions? source)
        {
            source ??= new SegmentedOptions();

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
        /// Convert to an Options object
        /// </summary>
        /// TODO: Remove when Options is no longer relevant
        public Options ConvertToOptions()
        {
            return new Options
            {
                FirstRun = FirstRun,

                AaruPath = Dumping.AaruPath,
                DiscImageCreatorPath = Dumping.DiscImageCreatorPath,
                // DreamdumpPath = Dumping.DreamdumpPath,
                RedumperPath = Dumping.RedumperPath,
                InternalProgram = InternalProgram,

                EnableDarkMode = GUI.Theming.EnableDarkMode,
                EnablePurpMode = GUI.Theming.EnablePurpMode,
                CustomBackgroundColor = GUI.Theming.CustomBackgroundColor,
                CustomTextColor = GUI.Theming.CustomTextColor,
                CheckForUpdatesOnStartup = CheckForUpdatesOnStartup,
                CopyUpdateUrlToClipboard = GUI.CopyUpdateUrlToClipboard,
                FastUpdateLabel = GUI.FastUpdateLabel,
                DefaultInterfaceLanguage = GUI.DefaultInterfaceLanguage,
                DefaultOutputPath = Dumping.DefaultOutputPath,
                DefaultSystem = Dumping.DefaultSystem,
                ShowDebugViewMenuItem = GUI.ShowDebugViewMenuItem,

                PreferredDumpSpeedCD = Dumping.DumpSpeeds.PreferredCD,
                PreferredDumpSpeedDVD = Dumping.DumpSpeeds.PreferredDVD,
                PreferredDumpSpeedHDDVD = Dumping.DumpSpeeds.PreferredHDDVD,
                PreferredDumpSpeedBD = Dumping.DumpSpeeds.PreferredBD,

                AaruEnableDebug = Dumping.Aaru.EnableDebug,
                AaruEnableVerbose = Dumping.Aaru.EnableVerbose,
                AaruForceDumping = Dumping.Aaru.ForceDumping,
                AaruRereadCount = Dumping.Aaru.RereadCount,
                AaruStripPersonalData = Dumping.Aaru.StripPersonalData,

                DICMultiSectorRead = Dumping.DIC.MultiSectorRead,
                DICMultiSectorReadValue = Dumping.DIC.MultiSectorReadValue,
                DICParanoidMode = Dumping.DIC.ParanoidMode,
                DICQuietMode = Dumping.DIC.QuietMode,
                DICRereadCount = Dumping.DIC.RereadCount,
                DICDVDRereadCount = Dumping.DIC.DVDRereadCount,
                DICUseCMIFlag = Dumping.DIC.UseCMIFlag,

                // DreamdumpNonRedumpMode = Dumping.Dreamdump.NonRedumpMode,
                // DreamdumpSectorOrder = Dumping.Dreamdump.SectorOrder,
                // DreamdumpRereadCount = Dumping.Dreamdump.RereadCount,

                RedumperEnableSkeleton = Dumping.Redumper.EnableSkeleton,
                RedumperEnableVerbose = Dumping.Redumper.EnableVerbose,
                RedumperLeadinRetryCount = Dumping.Redumper.LeadinRetryCount,
                RedumperNonRedumpMode = Dumping.Redumper.NonRedumpMode,
                RedumperDriveType = Dumping.Redumper.DriveType,
                RedumperDrivePregapStart = Dumping.Redumper.DrivePregapStart,
                RedumperReadMethod = Dumping.Redumper.ReadMethod,
                RedumperSectorOrder = Dumping.Redumper.SectorOrder,
                RedumperRereadCount = Dumping.Redumper.RereadCount,
                RedumperRefineSectorMode = Dumping.Redumper.RefineSectorMode,

                ScanForProtection = Processing.ProtectionScanning.ScanForProtection,
                AddPlaceholders = Processing.MediaInformation.AddPlaceholders,
                PromptForDiscInformation = Processing.MediaInformation.PromptForDiscInformation,
                PullAllInformation = Processing.MediaInformation.PullAllInformation,
                EnableTabsInInputFields = Processing.MediaInformation.EnableTabsInInputFields,
                EnableRedumpCompatibility = Processing.MediaInformation.EnableRedumpCompatibility,
                ShowDiscEjectReminder = Processing.ShowDiscEjectReminder,
                IgnoreFixedDrives = GUI.IgnoreFixedDrives,
                AddFilenameSuffix = Processing.AddFilenameSuffix,
                OutputSubmissionJSON = Processing.OutputSubmissionJSON,
                IncludeArtifacts = Processing.IncludeArtifacts,
                CompressLogFiles = Processing.CompressLogFiles,
                LogCompression = Processing.LogCompression,
                DeleteUnnecessaryFiles = Processing.DeleteUnnecessaryFiles,
                CreateIRDAfterDumping = Processing.CreateIRDAfterDumping,

                SkipSystemDetection = GUI.SkipSystemDetection,

                ScanArchivesForProtection = Processing.ProtectionScanning.ScanArchivesForProtection,
                IncludeDebugProtectionInformation = Processing.ProtectionScanning.IncludeDebugProtectionInformation,
                HideDriveLetters = Processing.ProtectionScanning.HideDriveLetters,

                VerboseLogging = VerboseLogging,
                OpenLogWindowAtStartup = GUI.OpenLogWindowAtStartup,

                RetrieveMatchInformation = Processing.Login.RetrieveMatchInformation,
                RedumpUsername = Processing.Login.RedumpUsername,
                RedumpPassword = Processing.Login.RedumpPassword,
            };
        }
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
        public Processors.LogCompression LogCompression { get; set; } = Processors.LogCompression.DeflateMaximum;

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
