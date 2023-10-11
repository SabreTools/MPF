using System.Collections.Generic;
using MPF.Core.Converters;
using SabreTools.RedumpLib.Data;

namespace MPF.Core.Data
{
#if NET48
    public class Options
#else
    public class Options
#endif
    {
        /// <summary>
        /// All settings in the form of a dictionary
        /// </summary>
#if NET48
        public Dictionary<string, string> Settings { get; private set; }
#else
        public Dictionary<string, string?> Settings { get; private set; }
#endif

        #region Internal Program

        /// <summary>
        /// Path to Aaru
        /// </summary>
#if NET48
        public string AaruPath
#else
        public string? AaruPath
#endif
        {
            get { return GetStringSetting(Settings, "AaruPath", "Programs\\Aaru\\Aaru.exe"); }
            set { Settings["AaruPath"] = value; }
        }

        /// <summary>
        /// Path to DiscImageCreator
        /// </summary>
#if NET48
        public string DiscImageCreatorPath
#else
        public string? DiscImageCreatorPath
#endif
        {
            get { return GetStringSetting(Settings, "DiscImageCreatorPath", "Programs\\Creator\\DiscImageCreator.exe"); }
            set { Settings["DiscImageCreatorPath"] = value; }
        }

        /// <summary>
        /// Path to Redumper
        /// </summary>
#if NET48
        public string RedumperPath
#else
        public string? RedumperPath
#endif
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
                var valueString = GetStringSetting(Settings, "InternalProgram", InternalProgram.DiscImageCreator.ToString());
                var valueEnum = EnumConverter.ToInternalProgram(valueString);
                return valueEnum == InternalProgram.NONE ? InternalProgram.DiscImageCreator : valueEnum;
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
#if NET48
        public string DefaultOutputPath
#else
        public string? DefaultOutputPath
#endif
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
        /// Reset drive after dumping (useful for older drives)
        /// </summary>
        public bool DICResetDriveAfterDump
        {
            get { return GetBooleanSetting(Settings, "DICResetDriveAfterDump", false); }
            set { Settings["DICResetDriveAfterDump"] = value.ToString(); }
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
        /// Enable verbose output while dumping by default
        /// </summary>
        public bool RedumperEnableVerbose
        {
            get { return GetBooleanSetting(Settings, "RedumperEnableVerbose", false); }
            set { Settings["RedumperEnableVerbose"] = value.ToString(); }
        }

        /// <summary>
        /// Enable BE reading by default with Redumper
        /// </summary>
        public bool RedumperUseBEReading
        {
            get { return GetBooleanSetting(Settings, "RedumperUseBEReading", false); }
            set { Settings["RedumperUseBEReading"] = value.ToString(); }
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
        /// Output all found protections to a separate file in the directory
        /// </summary>
        public bool OutputSeparateProtectionFile
        {
            get { return GetBooleanSetting(Settings, "OutputSeparateProtectionFile", true); }
            set { Settings["OutputSeparateProtectionFile"] = value.ToString(); }
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
        /// Eject the disc after dumping
        /// </summary>
        public bool EjectAfterDump
        {
            get { return GetBooleanSetting(Settings, "EjectAfterDump", false); }
            set { Settings["EjectAfterDump"] = value.ToString(); }
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
        /// Show dumping tools in their own window instead of in the log
        /// </summary>
        public bool ToolsInSeparateWindow
        {
            get { return GetBooleanSetting(Settings, "ToolsInSeparateWindow", true); }
            set { Settings["ToolsInSeparateWindow"] = value.ToString(); }
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

#if NET48
        public string RedumpUsername
#else
        public string? RedumpUsername
#endif
        {
            get { return GetStringSetting(Settings, "RedumpUsername", ""); }
            set { Settings["RedumpUsername"] = value; }
        }

        // TODO: Figure out a way to keep this encrypted in some way, BASE64 to start?
#if NET48
        public string RedumpPassword
#else
        public string? RedumpPassword
#endif
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
        public bool HasRedumpLogin { get => !string.IsNullOrWhiteSpace(RedumpUsername) && !string.IsNullOrWhiteSpace(RedumpPassword); }

        #endregion

        /// <summary>
        /// Constructor taking a dictionary for settings
        /// </summary>
        /// <param name="settings"></param>
#if NET48
        public Options(Dictionary<string, string> settings = null)
#else
        public Options(Dictionary<string, string?>? settings = null)
#endif
        {
#if NET48
            this.Settings = settings ?? new Dictionary<string, string>();
#else
            this.Settings = settings ?? new Dictionary<string, string?>();
#endif
        }

        /// <summary>
        /// Constructor taking an existing Options object
        /// </summary>
        /// <param name="source"></param>
#if NET48
        public Options(Options source)
#else
        public Options(Options? source)
#endif
        {
#if NET48
            Settings = new Dictionary<string, string>(source?.Settings ?? new Dictionary<string, string>());
#else
            Settings = new Dictionary<string, string?>(source?.Settings ?? new Dictionary<string, string?>());
#endif
        }

        /// <summary>
        /// Accessor for the internal dictionary
        /// </summary>
#if NET48
        public string this[string key]
#else
        public string? this[string key]
#endif
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
#if NET48
        private bool GetBooleanSetting(Dictionary<string, string> settings, string key, bool defaultValue)
#else
        private bool GetBooleanSetting(Dictionary<string, string?> settings, string key, bool defaultValue)
#endif
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
#if NET48
        private int GetInt32Setting(Dictionary<string, string> settings, string key, int defaultValue)
#else
        private int GetInt32Setting(Dictionary<string, string?> settings, string key, int defaultValue)
#endif
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
#if NET48
        private string GetStringSetting(Dictionary<string, string> settings, string key, string defaultValue)
#else
        private string? GetStringSetting(Dictionary<string, string?> settings, string key, string? defaultValue)
#endif
        {
            if (settings.ContainsKey(key))
                return settings[key];
            else
                return defaultValue;
        }

        #endregion
    }
}
