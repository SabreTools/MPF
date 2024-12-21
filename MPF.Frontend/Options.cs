using System.Collections.Generic;
using SabreTools.RedumpLib.Data;
using AaruSettings = MPF.ExecutionContexts.Aaru.SettingConstants;
using DICSettings = MPF.ExecutionContexts.DiscImageCreator.SettingConstants;
using RedumperReadMethod = MPF.ExecutionContexts.Redumper.ReadMethod;
using RedumperSectorOrder = MPF.ExecutionContexts.Redumper.SectorOrder;
using RedumperSettings = MPF.ExecutionContexts.Redumper.SettingConstants;

namespace MPF.Frontend
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
                var valueEnum = ToInternalProgram(valueString);
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
        /// Enable purple mode for UI elements
        /// </summary>
        public bool EnablePurpMode
        {
            get { return GetBooleanSetting(Settings, "EnablePurpMode", false); }
            set { Settings["EnablePurpMode"] = value.ToString(); }
        }

        /// <summary>
        /// Custom color setting
        /// </summary>
        public string? CustomBackgroundColor
        {
            get { return GetStringSetting(Settings, "CustomBackgroundColor", null); }
            set { Settings["CustomBackgroundColor"] = value; }
        }

        /// <summary>
        /// Custom color setting
        /// </summary>
        public string? CustomTextColor
        {
            get { return GetStringSetting(Settings, "CustomTextColor", null); }
            set { Settings["CustomTextColor"] = value; }
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
                var valueString = GetStringSetting(Settings, "DefaultSystem", RedumpSystem.IBMPCcompatible.LongName());
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
            get { return GetBooleanSetting(Settings, AaruSettings.EnableDebug, AaruSettings.EnableDebugDefault); }
            set { Settings[AaruSettings.EnableDebug] = value.ToString(); }
        }

        /// <summary>
        /// Enable verbose output while dumping by default
        /// </summary>
        public bool AaruEnableVerbose
        {
            get { return GetBooleanSetting(Settings, AaruSettings.EnableVerbose, AaruSettings.EnableVerboseDefault); }
            set { Settings[AaruSettings.EnableVerbose] = value.ToString(); }
        }

        /// <summary>
        /// Enable force dumping of media by default
        /// </summary>
        public bool AaruForceDumping
        {
            get { return GetBooleanSetting(Settings, AaruSettings.ForceDumping, AaruSettings.ForceDumpingDefault); }
            set { Settings[AaruSettings.ForceDumping] = value.ToString(); }
        }

        /// <summary>
        /// Default number of sector/subchannel rereads
        /// </summary>
        public int AaruRereadCount
        {
            get { return GetInt32Setting(Settings, AaruSettings.RereadCount, AaruSettings.RereadCountDefault); }
            set { Settings[AaruSettings.RereadCount] = value.ToString(); }
        }

        /// <summary>
        /// Strip personal data information from Aaru metadata by default
        /// </summary>
        public bool AaruStripPersonalData
        {
            get { return GetBooleanSetting(Settings, AaruSettings.StripPersonalData, AaruSettings.StripPersonalDataDefault); }
            set { Settings[AaruSettings.StripPersonalData] = value.ToString(); }
        }

        #endregion

        #region DiscImageCreator

        /// <summary>
        /// Enable multi-sector read flag by default
        /// </summary>
        public bool DICMultiSectorRead
        {
            get { return GetBooleanSetting(Settings, DICSettings.MultiSectorRead, DICSettings.MultiSectorReadDefault); }
            set { Settings[DICSettings.MultiSectorRead] = value.ToString(); }
        }

        /// <summary>
        /// Include a default multi-sector read value
        /// </summary>
        public int DICMultiSectorReadValue
        {
            get { return GetInt32Setting(Settings, DICSettings.MultiSectorReadValue, DICSettings.MultiSectorReadValueDefault); }
            set { Settings[DICSettings.MultiSectorReadValue] = value.ToString(); }
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
            get { return GetBooleanSetting(Settings, DICSettings.ParanoidMode, DICSettings.ParanoidModeDefault); }
            set { Settings[DICSettings.ParanoidMode] = value.ToString(); }
        }

        /// <summary>
        /// Enable the Quiet flag by default
        /// </summary>
        public bool DICQuietMode
        {
            get { return GetBooleanSetting(Settings, DICSettings.QuietMode, DICSettings.QuietModeDefault); }
            set { Settings[DICSettings.QuietMode] = value.ToString(); }
        }

        /// <summary>
        /// Default number of C2 rereads
        /// </summary>
        public int DICRereadCount
        {
            get { return GetInt32Setting(Settings, DICSettings.RereadCount, DICSettings.RereadCountDefault); }
            set { Settings[DICSettings.RereadCount] = value.ToString(); }
        }

        /// <summary>
        /// Default number of DVD/HD-DVD/BD rereads
        /// </summary>
        public int DICDVDRereadCount
        {
            get { return GetInt32Setting(Settings, DICSettings.DVDRereadCount, DICSettings.DVDRereadCountDefault); }
            set { Settings[DICSettings.DVDRereadCount] = value.ToString(); }
        }

        /// <summary>
        /// Use the CMI flag for supported disc types
        /// </summary>
        public bool DICUseCMIFlag
        {
            get { return GetBooleanSetting(Settings, DICSettings.UseCMIFlag, DICSettings.UseCMIFlagDefault); }
            set { Settings[DICSettings.UseCMIFlag] = value.ToString(); }
        }

        #endregion

        #region Redumper

        /// <summary>
        /// Enable debug output while dumping by default
        /// </summary>
        public bool RedumperEnableDebug
        {
            get { return GetBooleanSetting(Settings, RedumperSettings.EnableDebug, RedumperSettings.EnableDebugDefault); }
            set { Settings[RedumperSettings.EnableDebug] = value.ToString(); }
        }

        /// <summary>
        /// Enable Redumper custom lead-in retries for Plextor drives
        /// </summary>
        public bool RedumperEnableLeadinRetry
        {
            get { return GetBooleanSetting(Settings, RedumperSettings.EnableLeadinRetry, RedumperSettings.EnableLeadinRetryDefault); }
            set { Settings[RedumperSettings.EnableLeadinRetry] = value.ToString(); }
        }

        /// <summary>
        /// Enable verbose output while dumping by default
        /// </summary>
        public bool RedumperEnableVerbose
        {
            get { return GetBooleanSetting(Settings, RedumperSettings.EnableVerbose, RedumperSettings.EnableVerboseDefault); }
            set { Settings[RedumperSettings.EnableVerbose] = value.ToString(); }
        }

        /// <summary>
        /// Default number of redumper Plextor leadin retries
        /// </summary>
        public int RedumperLeadinRetryCount
        {
            get { return GetInt32Setting(Settings, RedumperSettings.LeadinRetryCount, RedumperSettings.LeadinRetryCountDefault); }
            set { Settings[RedumperSettings.LeadinRetryCount] = value.ToString(); }
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
            get { return GetBooleanSetting(Settings, RedumperSettings.UseGenericDriveType, RedumperSettings.UseGenericDriveTypeDefault); }
            set { Settings[RedumperSettings.UseGenericDriveType] = value.ToString(); }
        }

        /// <summary>
        /// Currently selected default redumper read method
        /// </summary>
        public RedumperReadMethod RedumperReadMethod
        {
            get
            {
                var valueString = GetStringSetting(Settings, RedumperSettings.ReadMethod, RedumperSettings.ReadMethodDefault);
                return valueString.ToRedumperReadMethod();
            }
            set
            {
                Settings[RedumperSettings.ReadMethod] = value.ToString();
            }
        }

        /// <summary>
        /// Currently selected default redumper sector order
        /// </summary>
        public RedumperSectorOrder RedumperSectorOrder
        {
            get
            {
                var valueString = GetStringSetting(Settings, RedumperSettings.SectorOrder, RedumperSettings.SectorOrderDefault);
                return valueString.ToRedumperSectorOrder();
            }
            set
            {
                Settings[RedumperSettings.SectorOrder] = value.ToString();
            }
        }

        /// <summary>
        /// Default number of rereads
        /// </summary>
        public int RedumperRereadCount
        {
            get { return GetInt32Setting(Settings, RedumperSettings.RereadCount, RedumperSettings.RereadCountDefault); }
            set { Settings[RedumperSettings.RereadCount] = value.ToString(); }
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
            Settings = settings ?? [];
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
            get => Settings[key];
            set => Settings[key] = value;
        }

        #region Helpers

        /// <summary>
        /// Get the InternalProgram enum value for a given string
        /// </summary>
        /// <param name="internalProgram">String value to convert</param>
        /// <returns>InternalProgram represented by the string, if possible</returns>
        public static InternalProgram ToInternalProgram(string? internalProgram)
        {
            return (internalProgram?.ToLowerInvariant()) switch
            {
                // Dumping support
                "aaru"
                    or "chef"
                    or "dichef"
                    or "discimagechef" => InternalProgram.Aaru,
                "creator"
                    or "dic"
                    or "dicreator"
                    or "discimagecreator" => InternalProgram.DiscImageCreator,
                "rd"
                    or "redumper" => InternalProgram.Redumper,

                // Verification support only
                "cleanrip"
                    or "cr" => InternalProgram.CleanRip,
                "ps3cfw"
                    or "ps3"
                    or "getkey"
                    or "managunz"
                    or "multiman" => InternalProgram.PS3CFW,
                "uic"
                    or "umd"
                    or "umdcreator"
                    or "umdimagecreator" => InternalProgram.UmdImageCreator,
                "xbc"
                    or "xbox"
                    or "xbox360"
                    or "xboxcreator"
                    or "xboxbackupcreator" => InternalProgram.XboxBackupCreator,

                _ => InternalProgram.NONE,
            };
        }

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
}
