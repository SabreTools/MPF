using System.Collections.Generic;
using SabreTools.RedumpLib.Data;

namespace MPF.Core
{
    public class Options
    {
        /// <summary>
        /// All settings in the form of a dictionary
        /// </summary>
        public Dictionary<string, string?> Settings { get; private set; }

        /// <summary>
        /// Indicate if the program is being run with a clean configuration
        /// </summary>
        public bool FirstRun
        {
            get { return GetBooleanSetting(Settings, "FirstRun", true); }
            set { Settings["FirstRun"] = value.ToString(); }
        }

        #region Internal Program

        /// <summary>
        /// Path to Aaru
        /// </summary>
        public string? AaruPath
        {
            get { return GetStringSetting(Settings, "AaruPath", "Programs\\Aaru\\Aaru.exe"); }
            set { Settings["AaruPath"] = value; }
        }

        /// <summary>
        /// Path to DiscImageCreator
        /// </summary>
        public string? DiscImageCreatorPath
        {
            get { return GetStringSetting(Settings, "DiscImageCreatorPath", "Programs\\Creator\\DiscImageCreator.exe"); }
            set { Settings["DiscImageCreatorPath"] = value; }
        }

        /// <summary>
        /// Path to Redumper
        /// </summary>
        public string? RedumperPath
        {
            get { return GetStringSetting(Settings, "RedumperPath", "Programs\\Redumper\\redumper.exe"); }
            set { Settings["RedumperPath"] = value; }
        }

        /// <summary>
        /// Currently selected dumping program
        /// </summary>
        public InternalProgram InternalProgram
        {
            get
            {
                var valueString = GetStringSetting(Settings, "InternalProgram", InternalProgram.Redumper.ToString());
                var valueEnum = EnumExtensions.ToInternalProgram(valueString);
                return valueEnum == InternalProgram.NONE ? InternalProgram.Redumper : valueEnum;
            }
            set
            {
                Settings["InternalProgram"] = value.ToString();
            }
        }

        #endregion

        #region UI Defaults

        /// <summary>
        /// Enable dark mode for UI elements
        /// </summary>
        public bool EnableDarkMode
        {
            get { return GetBooleanSetting(Settings, "EnableDarkMode", false); }
            set { Settings["EnableDarkMode"] = value.ToString(); }
        }

        /// <summary>
        /// Check for updates on startup
        /// </summary>
        public bool CheckForUpdatesOnStartup
        {
            get { return GetBooleanSetting(Settings, "CheckForUpdatesOnStartup", true); }
            set { Settings["CheckForUpdatesOnStartup"] = value.ToString(); }
        }

        /// <summary>
        /// Fast update label - Skips disc checks and updates path only
        /// </summary>
        public bool FastUpdateLabel
        {
            get { return GetBooleanSetting(Settings, "FastUpdateLabel", false); }
            set { Settings["FastUpdateLabel"] = value.ToString(); }
        }

        /// <summary>
        /// Default output path for dumps
        /// </summary>
        public string? DefaultOutputPath
        {
            get { return GetStringSetting(Settings, "DefaultOutputPath", "ISO"); }
            set { Settings["DefaultOutputPath"] = value; }
        }

        /// <summary>
        /// Default system if none can be detected
        /// </summary>
        public RedumpSystem? DefaultSystem
        {
            get
            {
                var valueString = GetStringSetting(Settings, "DefaultSystem", null);
                var valueEnum = Extensions.ToRedumpSystem(valueString ?? string.Empty);
                return valueEnum;
            }
            set
            {
                Settings["DefaultSystem"] = value.LongName();
            }
        }

        /// <summary>
        /// Default output path for dumps
        /// </summary>
        /// <remarks>This is a hidden setting</remarks>
        public bool ShowDebugViewMenuItem
        {
            get { return GetBooleanSetting(Settings, "ShowDebugViewMenuItem", false); }
            set { Settings["ShowDebugViewMenuItem"] = value.ToString(); }
        }

        #endregion

        #region Dumping Speeds

        /// <summary>
        /// Default CD dumping speed
        /// </summary>
        public int PreferredDumpSpeedCD
        {
            get { return GetInt32Setting(Settings, "PreferredDumpSpeedCD", 24); }
            set { Settings["PreferredDumpSpeedCD"] = value.ToString(); }
        }

        /// <summary>
        /// Default DVD dumping speed
        /// </summary>
        public int PreferredDumpSpeedDVD
        {
            get { return GetInt32Setting(Settings, "PreferredDumpSpeedDVD", 16); }
            set { Settings["PreferredDumpSpeedDVD"] = value.ToString(); }
        }

        /// <summary>
        /// Default HD-DVD dumping speed
        /// </summary>
        public int PreferredDumpSpeedHDDVD
        {
            get { return GetInt32Setting(Settings, "PreferredDumpSpeedHDDVD", 8); }
            set { Settings["PreferredDumpSpeedHDDVD"] = value.ToString(); }
        }

        /// <summary>
        /// Default BD dumping speed
        /// </summary>
        public int PreferredDumpSpeedBD
        {
            get { return GetInt32Setting(Settings, "PreferredDumpSpeedBD", 8); }
            set { Settings["PreferredDumpSpeedBD"] = value.ToString(); }
        }

        #endregion

        #region Aaru

        /// <summary>
        /// Enable debug output while dumping by default
        /// </summary>
        public bool AaruEnableDebug
        {
            get { return GetBooleanSetting(Settings, "AaruEnableDebug", false); }
            set { Settings["AaruEnableDebug"] = value.ToString(); }
        }

        /// <summary>
        /// Enable verbose output while dumping by default
        /// </summary>
        public bool AaruEnableVerbose
        {
            get { return GetBooleanSetting(Settings, "AaruEnableVerbose", false); }
            set { Settings["AaruEnableVerbose"] = value.ToString(); }
        }

        /// <summary>
        /// Enable force dumping of media by default
        /// </summary>
        public bool AaruForceDumping
        {
            get { return GetBooleanSetting(Settings, "AaruForceDumping", true); }
            set { Settings["AaruForceDumping"] = value.ToString(); }
        }

        /// <summary>
        /// Default number of sector/subchannel rereads
        /// </summary>
        public int AaruRereadCount
        {
            get { return GetInt32Setting(Settings, "AaruRereadCount", 5); }
            set { Settings["AaruRereadCount"] = value.ToString(); }
        }

        /// <summary>
        /// Strip personal data information from Aaru metadata by default
        /// </summary>
        public bool AaruStripPersonalData
        {
            get { return GetBooleanSetting(Settings, "AaruStripPersonalData", false); }
            set { Settings["AaruStripPersonalData"] = value.ToString(); }
        }

        #endregion

        #region DiscImageCreator

        /// <summary>
        /// Enable multi-sector read flag by default
        /// </summary>
        public bool DICMultiSectorRead
        {
            get { return GetBooleanSetting(Settings, "DICMultiSectorRead", false); }
            set { Settings["DICMultiSectorRead"] = value.ToString(); }
        }

        /// <summary>
        /// Include a default multi-sector read value
        /// </summary>
        public int DICMultiSectorReadValue
        {
            get { return GetInt32Setting(Settings, "DICMultiSectorReadValue", 0); }
            set { Settings["DICMultiSectorReadValue"] = value.ToString(); }
        }

        /// <summary>
        /// Enable overly-secure dumping flags by default
        /// </summary>
        /// <remarks>
        /// Split this into component parts later. Currently does:
        /// - Scan sector protection and set subchannel read level to 2 for CD
        /// - Set scan file protect flag for DVD
        /// </remarks>
        public bool DICParanoidMode
        {
            get { return GetBooleanSetting(Settings, "DICParanoidMode", false); }
            set { Settings["DICParanoidMode"] = value.ToString(); }
        }

        /// <summary>
        /// Enable the Quiet flag by default
        /// </summary>
        public bool DICQuietMode
        {
            get { return GetBooleanSetting(Settings, "DICQuietMode", false); }
            set { Settings["DICQuietMode"] = value.ToString(); }
        }

        /// <summary>
        /// Default number of C2 rereads
        /// </summary>
        public int DICRereadCount
        {
            get { return GetInt32Setting(Settings, "DICRereadCount", 20); }
            set { Settings["DICRereadCount"] = value.ToString(); }
        }

        /// <summary>
        /// Default number of DVD/HD-DVD/BD rereads
        /// </summary>
        public int DICDVDRereadCount
        {
            get { return GetInt32Setting(Settings, "DICDVDRereadCount", 10); }
            set { Settings["DICDVDRereadCount"] = value.ToString(); }
        }

        /// <summary>
        /// Use the CMI flag for supported disc types
        /// </summary>
        public bool DICUseCMIFlag
        {
            get { return GetBooleanSetting(Settings, "DICUseCMIFlag", false); }
            set { Settings["DICUseCMIFlag"] = value.ToString(); }
        }

        #endregion

        #region Redumper

        /// <summary>
        /// Enable debug output while dumping by default
        /// </summary>
        public bool RedumperEnableDebug
        {
            get { return GetBooleanSetting(Settings, "RedumperEnableDebug", false); }
            set { Settings["RedumperEnableDebug"] = value.ToString(); }
        }

        /// <summary>
        /// Enable Redumper custom lead-in retries for Plextor drives
        /// </summary>
        public bool RedumperEnableLeadinRetry
        {
            get { return GetBooleanSetting(Settings, "RedumperEnableLeadinRetry", false); }
            set { Settings["RedumperEnableLeadinRetry"] = value.ToString(); }
        }

        /// <summary>
        /// Enable verbose output while dumping by default
        /// </summary>
        public bool RedumperEnableVerbose
        {
            get { return GetBooleanSetting(Settings, "RedumperEnableVerbose", true); }
            set { Settings["RedumperEnableVerbose"] = value.ToString(); }
        }

        /// <summary>
        /// Default number of redumper Plextor leadin retries
        /// </summary>
        public int RedumperLeadinRetryCount
        {
            get { return GetInt32Setting(Settings, "RedumperLeadinRetryCount", 4); }
            set { Settings["RedumperLeadinRetryCount"] = value.ToString(); }
        }

        /// <summary>
        /// Enable options incompatible with redump submissions
        /// </summary>
        public bool RedumperNonRedumpMode
        {
            get { return GetBooleanSetting(Settings, "RedumperNonRedumpMode", false); }
            set { Settings["RedumperNonRedumpMode"] = value.ToString(); }
        }

        /// <summary>
        /// Enable generic drive type by default with Redumper
        /// </summary>
        public bool RedumperUseGenericDriveType
        {
            get { return GetBooleanSetting(Settings, "RedumperUseGenericDriveType", false); }
            set { Settings["RedumperUseGenericDriveType"] = value.ToString(); }
        }

        /// <summary>
        /// Currently selected default redumper read method
        /// </summary>
        public RedumperReadMethod RedumperReadMethod
        {
            get
            {
                var valueString = GetStringSetting(Settings, "RedumperReadMethod", RedumperReadMethod.NONE.ToString());
                return EnumExtensions.ToRedumperReadMethod(valueString);
            }
            set
            {
                Settings["RedumperReadMethod"] = value.ToString();
            }
        }

        /// <summary>
        /// Currently selected default redumper sector order
        /// </summary>
        public RedumperSectorOrder RedumperSectorOrder
        {
            get
            {
                var valueString = GetStringSetting(Settings, "RedumperSectorOrder", RedumperSectorOrder.NONE.ToString());
                return EnumExtensions.ToRedumperSectorOrder(valueString);
            }
            set
            {
                Settings["RedumperSectorOrder"] = value.ToString();
            }
        }

        /// <summary>
        /// Default number of rereads
        /// </summary>
        public int RedumperRereadCount
        {
            get { return GetInt32Setting(Settings, "RedumperRereadCount", 20); }
            set { Settings["RedumperRereadCount"] = value.ToString(); }
        }

        #endregion

        #region Extra Dumping Options

        /// <summary>
        /// Scan the disc for protection after dumping
        /// </summary>
        public bool ScanForProtection
        {
            get { return GetBooleanSetting(Settings, "ScanForProtection", true); }
            set { Settings["ScanForProtection"] = value.ToString(); }
        }

        /// <summary>
        /// Add placeholder values in the submission info
        /// </summary>
        public bool AddPlaceholders
        {
            get { return GetBooleanSetting(Settings, "AddPlaceholders", true); }
            set { Settings["AddPlaceholders"] = value.ToString(); }
        }

        /// <summary>
        /// Show the disc information window after dumping
        /// </summary>
        public bool PromptForDiscInformation
        {
            get { return GetBooleanSetting(Settings, "PromptForDiscInformation", true); }
            set { Settings["PromptForDiscInformation"] = value.ToString(); }
        }

        /// <summary>
        /// Pull all information from Redump if signed in
        /// </summary>
        public bool PullAllInformation
        {
            get { return GetBooleanSetting(Settings, "PullAllInformation", false); }
            set { Settings["PullAllInformation"] = value.ToString(); }
        }

        /// <summary>
        /// Enable tabs in all input fields
        /// </summary>
        public bool EnableTabsInInputFields
        {
            get { return GetBooleanSetting(Settings, "EnableTabsInInputFields", false); }
            set { Settings["EnableTabsInInputFields"] = value.ToString(); }
        }

        /// <summary>
        /// Limit outputs to Redump-supported values only
        /// </summary>
        public bool EnableRedumpCompatibility
        {
            get { return GetBooleanSetting(Settings, "EnableRedumpCompatibility", true); }
            set { Settings["EnableRedumpCompatibility"] = value.ToString(); }
        }

        /// <summary>
        /// Show disc eject reminder before the disc information window is shown
        /// </summary>
        public bool ShowDiscEjectReminder
        {
            get { return GetBooleanSetting(Settings, "ShowDiscEjectReminder", true); }
            set { Settings["ShowDiscEjectReminder"] = value.ToString(); }
        }

        /// <summary>
        /// Ignore fixed drives when populating the list
        /// </summary>
        public bool IgnoreFixedDrives
        {
            get { return GetBooleanSetting(Settings, "IgnoreFixedDrives", true); }
            set { Settings["IgnoreFixedDrives"] = value.ToString(); }
        }

        /// <summary>
        /// Add the dump filename as a suffix to the auto-generated files
        /// </summary>
        public bool AddFilenameSuffix
        {
            get { return GetBooleanSetting(Settings, "AddFilenameSuffix", false); }
            set { Settings["AddFilenameSuffix"] = value.ToString(); }
        }

        /// <summary>
        /// Output the compressed JSON version of the submission info
        /// </summary>
        public bool OutputSubmissionJSON
        {
            get { return GetBooleanSetting(Settings, "OutputSubmissionJSON", false); }
            set { Settings["OutputSubmissionJSON"] = value.ToString(); }
        }

        /// <summary>
        /// Include log files in serialized JSON data
        /// </summary>
        public bool IncludeArtifacts
        {
            get { return GetBooleanSetting(Settings, "IncludeArtifacts", false); }
            set { Settings["IncludeArtifacts"] = value.ToString(); }
        }

        /// <summary>
        /// Compress output log files to reduce space
        /// </summary>
        public bool CompressLogFiles
        {
            get { return GetBooleanSetting(Settings, "CompressLogFiles", true); }
            set { Settings["CompressLogFiles"] = value.ToString(); }
        }

        /// <summary>
        /// Delete unnecessary files to reduce space
        /// </summary>
        public bool DeleteUnnecessaryFiles
        {
            get { return GetBooleanSetting(Settings, "DeleteUnnecessaryFiles", false); }
            set { Settings["DeleteUnnecessaryFiles"] = value.ToString(); }
        }

        /// <summary>
        /// Create a PS3 IRD file after dumping PS3 BD-ROM discs
        /// </summary>
        public bool CreateIRDAfterDumping
        {
            get { return GetBooleanSetting(Settings, "CreateIRDAfterDumping", false); }
            set { Settings["CreateIRDAfterDumping"] = value.ToString(); }
        }

        #endregion

        #region Skip Options

        /// <summary>
        /// Skip detecting media type on disc scan
        /// </summary>
        public bool SkipMediaTypeDetection
        {
            get { return GetBooleanSetting(Settings, "SkipMediaTypeDetection", false); }
            set { Settings["SkipMediaTypeDetection"] = value.ToString(); }
        }

        /// <summary>
        /// Skip detecting known system on disc scan
        /// </summary>
        public bool SkipSystemDetection
        {
            get { return GetBooleanSetting(Settings, "SkipSystemDetection", false); }
            set { Settings["SkipSystemDetection"] = value.ToString(); }
        }

        #endregion

        #region Protection Scanning Options

        /// <summary>
        /// Scan archive contents during protection scanning
        /// </summary>
        public bool ScanArchivesForProtection
        {
            get { return GetBooleanSetting(Settings, "ScanArchivesForProtection", true); }
            set { Settings["ScanArchivesForProtection"] = value.ToString(); }
        }

        /// <summary>
        /// Scan for executable packers during protection scanning
        /// </summary>
        public bool ScanPackersForProtection
        {
            get { return GetBooleanSetting(Settings, "ScanPackersForProtection", false); }
            set { Settings["ScanPackersForProtection"] = value.ToString(); }
        }

        /// <summary>
        /// Include debug information with scan results
        /// </summary>
        public bool IncludeDebugProtectionInformation
        {
            get { return GetBooleanSetting(Settings, "IncludeDebugProtectionInformation", false); }
            set { Settings["IncludeDebugProtectionInformation"] = value.ToString(); }
        }

        /// <summary>
        /// Remove drive letters from protection scan output
        /// </summary>
        public bool HideDriveLetters
        {
            get { return GetBooleanSetting(Settings, "HideDriveLetters", false); }
            set { Settings["HideDriveLetters"] = value.ToString(); }
        }

        #endregion

        #region Logging Options

        /// <summary>
        /// Enable verbose and debug logs to be written
        /// </summary>
        public bool VerboseLogging
        {
            get { return GetBooleanSetting(Settings, "VerboseLogging", true); }
            set { Settings["VerboseLogging"] = value.ToString(); }
        }

        /// <summary>
        /// Have the log panel expanded by default on startup
        /// </summary>
        public bool OpenLogWindowAtStartup
        {
            get { return GetBooleanSetting(Settings, "OpenLogWindowAtStartup", true); }
            set { Settings["OpenLogWindowAtStartup"] = value.ToString(); }
        }

        #endregion

        #region Redump Login Information

        public string? RedumpUsername
        {
            get { return GetStringSetting(Settings, "RedumpUsername", ""); }
            set { Settings["RedumpUsername"] = value; }
        }

        // TODO: Figure out a way to keep this encrypted in some way, BASE64 to start?
        public string? RedumpPassword
        {
            get
            {
                return GetStringSetting(Settings, "RedumpPassword", "");
            }
            set { Settings["RedumpPassword"] = value; }
        }

        /// <summary>
        /// Determine if a complete set of Redump credentials might exist
        /// </summary>
        public bool HasRedumpLogin { get => !string.IsNullOrEmpty(RedumpUsername) && !string.IsNullOrEmpty(RedumpPassword); }

        #endregion

        /// <summary>
        /// Constructor taking a dictionary for settings
        /// </summary>
        /// <param name="settings"></param>
        public Options(Dictionary<string, string?>? settings = null)
        {
            this.Settings = settings ?? [];
        }

        /// <summary>
        /// Constructor taking an existing Options object
        /// </summary>
        /// <param name="source"></param>
        public Options(Options? source)
        {
            Settings = new Dictionary<string, string?>(source?.Settings ?? []);
        }

        /// <summary>
        /// Accessor for the internal dictionary
        /// </summary>
        public string? this[string key]
        {
            get => this.Settings[key];
            set => this.Settings[key] = value;
        }

        #region Helpers

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
