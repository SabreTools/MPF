using System;
using System.Collections;
using System.Collections.Generic;

namespace MPF.Data
{
    public class Options : IDictionary<string, string>
    {
        private Dictionary<string, string> _settings;

        #region Internal Program

        /// <summary>
        /// Path to Aaru
        /// </summary>
        public string AaruPath
        {
            get { return GetStringSetting(_settings, "AaruPath", "Programs\\Aaru\\Aaru.exe"); }
            set { _settings["AaruPath"] = value; }
        }

        /// <summary>
        /// Path to DiscImageCreator
        /// </summary>
        public string DiscImageCreatorPath
        {
            get { return GetStringSetting(_settings, "DiscImageCreatorPath", "Programs\\Creator\\DiscImageCreator.exe"); }
            set { _settings["DiscImageCreatorPath"] = value; }
        }

        /// <summary>
        /// Path to dd for Windows
        /// </summary>
        public string DDPath
        {
            get { return GetStringSetting(_settings, "DDPath", "Programs\\DD\\dd.exe"); }
            set { _settings["DDPath"] = value; }
        }

        /// <summary>
        /// Currently selected dumping program
        /// </summary>
        public string InternalProgram
        {
            get { return GetStringSetting(_settings, "InternalProgram", Data.InternalProgram.DiscImageCreator.ToString()); }
            set { _settings["InternalProgram"] = value; }
        }

        #endregion

        #region UI Defaults

        /// <summary>
        /// Default output path for dumps
        /// </summary>
        public string DefaultOutputPath
        {
            get { return GetStringSetting(_settings, "DefaultOutputPath", "ISO"); }
            set { _settings["DefaultOutputPath"] = value; }
        }

        /// <summary>
        /// Default system if none can be detected
        /// </summary>
        public string DefaultSystem
        {
            get { return GetStringSetting(_settings, "DefaultSystem", KnownSystem.NONE.ToString()); }
            set { _settings["DefaultSystem"] = value; }
        }

        #endregion

        #region Dumping Speeds

        /// <summary>
        /// Default CD dumping speed
        /// </summary>
        public int PreferredDumpSpeedCD
        {
            get { return GetInt32Setting(_settings, "PreferredDumpSpeedCD", 72); }
            set { _settings["PreferredDumpSpeedCD"] = value.ToString(); }
        }

        /// <summary>
        /// Default DVD dumping speed
        /// </summary>
        public int PreferredDumpSpeedDVD
        {
            get { return GetInt32Setting(_settings, "PreferredDumpSpeedDVD", 24); }
            set { _settings["PreferredDumpSpeedDVD"] = value.ToString(); }
        }

        /// <summary>
        /// Default BD dumping speed
        /// </summary>
        public int PreferredDumpSpeedBD
        {
            get { return GetInt32Setting(_settings, "PreferredDumpSpeedBD", 16); }
            set { _settings["PreferredDumpSpeedBD"] = value.ToString(); }
        }

        #endregion

        #region Aaru

        /// <summary>
        /// Enable debug output while dumping by default
        /// </summary>
        public bool AaruEnableDebug
        {
            get { return GetBooleanSetting(_settings, "AaruEnableDebug", false); }
            set { _settings["AaruEnableDebug"] = value.ToString(); }
        }

        /// <summary>
        /// Enable verbose output while dumping by default
        /// </summary>
        public bool AaruEnableVerbose
        {
            get { return GetBooleanSetting(_settings, "AaruEnableVerbose", false); }
            set { _settings["AaruEnableVerbose"] = value.ToString(); }
        }

        /// <summary>
        /// Default number of sector/subchannel rereads
        /// </summary>
        public int AaruRereadCount
        {
            get { return GetInt32Setting(_settings, "AaruRereadCount", 5); }
            set { _settings["AaruRereadCount"] = value.ToString(); }
        }

        /// <summary>
        /// Strip personal data information from Aaru metadata by default
        /// </summary>
        public bool AaruStripPersonalData
        {
            get { return GetBooleanSetting(_settings, "AaruStripPersonalData", false); }
            set { _settings["AaruStripPersonalData"] = value.ToString(); }
        }

        #endregion

        #region DiscImageCreator

        /// <summary>
        /// Enable overly-secure dumping flags by default
        /// </summary>
        /// <remarks>
        /// Split this into component parts later. Currently does:
        /// - Scan sector protection and set subchannel read level to 2 for CD
        /// - Set CMI and scan file protect flags for DVD
        /// - Set CMI flag for HD-DVD
        /// </remarks>
        public bool DICParanoidMode
        {
            get { return GetBooleanSetting(_settings, "DICParanoidMode", false); }
            set { _settings["DICParanoidMode"] = value.ToString(); }
        }

        /// <summary>
        /// Enable the Quiet flag by default
        /// </summary>
        public bool DICQuietMode
        {
            get { return GetBooleanSetting(_settings, "DICQuietMode", false); }
            set { _settings["DICQuietMode"] = value.ToString(); }
        }

        /// <summary>
        /// Default number of C2 rereads
        /// </summary>
        public int DICRereadCount
        {
            get { return GetInt32Setting(_settings, "DICRereadCount", 20); }
            set { _settings["DICRereadCount"] = value.ToString(); }
        }

        /// <summary>
        /// Reset drive after dumping (useful for older drives)
        /// </summary>
        public bool DICResetDriveAfterDump
        {
            get { return GetBooleanSetting(_settings, "DICResetDriveAfterDump", false); }
            set { _settings["DICResetDriveAfterDump"] = value.ToString(); }
        }

        #endregion

        #region Extra Dumping Options

        /// <summary>
        /// Scan the disc for protection after dumping
        /// </summary>
        public bool ScanForProtection
        {
            get { return GetBooleanSetting(_settings, "ScanForProtection", true); }
            set { _settings["ScanForProtection"] = value.ToString(); }
        }

        /// <summary>
        /// Add placeholder values in the submission info
        /// </summary>
        public bool AddPlaceholders
        {
            get { return GetBooleanSetting(_settings, "AddPlaceholders", true); }
            set { _settings["AddPlaceholders"] = value.ToString(); }
        }

        /// <summary>
        /// Show the disc information window after dumping
        /// </summary>
        public bool PromptForDiscInformation
        {
            get { return GetBooleanSetting(_settings, "PromptForDiscInformation", true); }
            set { _settings["PromptForDiscInformation"] = value.ToString(); }
        }

        /// <summary>
        /// Eject the disc after dumping
        /// </summary>
        public bool EjectAfterDump
        {
            get { return GetBooleanSetting(_settings, "EjectAfterDump", true); }
            set { _settings["EjectAfterDump"] = value.ToString(); }
        }

        /// <summary>
        /// Ignore fixed drives when populating the list
        /// </summary>
        public bool IgnoreFixedDrives
        {
            get { return GetBooleanSetting(_settings, "IgnoreFixedDrives", true); }
            set { _settings["IgnoreFixedDrives"] = value.ToString(); }
        }

        #endregion

        #region Skip Options

        /// <summary>
        /// Skip detecting media type on disc scan
        /// </summary>
        public bool SkipMediaTypeDetection
        {
            get { return GetBooleanSetting(_settings, "SkipMediaTypeDetection", false); }
            set { _settings["SkipMediaTypeDetection"] = value.ToString(); }
        }

        /// <summary>
        /// Skip detecting known system on disc scan
        /// </summary>
        public bool SkipSystemDetection
        {
            get { return GetBooleanSetting(_settings, "SkipSystemDetection", false); }
            set { _settings["SkipSystemDetection"] = value.ToString(); }
        }

        #endregion

        #region Logging Options

        public bool VerboseLogging
        {
            get { return GetBooleanSetting(_settings, "VerboseLogging", true); }
            set { _settings["VerboseLogging"] = value.ToString(); }
        }

        public bool OpenLogWindowAtStartup
        {
            get { return GetBooleanSetting(_settings, "OpenLogWindowAtStartup", true); }
            set { _settings["OpenLogWindowAtStartup"] = value.ToString(); }
        }

        #endregion

        #region Redump Login Information

        public string RedumpUsername
        {
            get { return GetStringSetting(_settings, "RedumpUsername", ""); }
            set { _settings["RedumpUsername"] = value; }
        }

        // TODO: Figure out a way to keep this encrypted in some way, BASE64 to start?
        public string RedumpPassword
        {
            get { return GetStringSetting(_settings, "RedumpPassword", ""); }
            set { _settings["RedumpPassword"] = value; }
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
        public Options(Dictionary<string, string> settings = null)
        {
            this._settings = settings ?? new Dictionary<string, string>();
        }

        #region Helpers

        /// <summary>
        /// Get a Boolean setting from a settings, dictionary
        /// </summary>
        /// <param name="settings">Dictionary representing the settings</param>
        /// <param name="key">Setting key to get a value for</param>
        /// <param name="defaultValue">Default value to return if no value is found</param>
        /// <returns>Setting value if possible, default value otherwise</returns>
        private bool GetBooleanSetting(Dictionary<string, string> settings, string key, bool defaultValue)
        {
            if (settings.ContainsKey(key))
            {
                if (Boolean.TryParse(settings[key], out bool value))
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
        private int GetInt32Setting(Dictionary<string, string> settings, string key, int defaultValue)
        {
            if (settings.ContainsKey(key))
            {
                if (Int32.TryParse(settings[key], out int value))
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
        private string GetStringSetting(Dictionary<string, string> settings, string key, string defaultValue)
        {
            if (settings.ContainsKey(key))
                return settings[key];
            else
                return defaultValue;
        }

        #endregion

        #region IDictionary implementations

        public ICollection<string> Keys => _settings.Keys;

        public ICollection<string> Values => _settings.Values;

        public int Count => _settings.Count;

        public bool IsReadOnly => ((IDictionary<string, string>)_settings).IsReadOnly;

        public string this[string key]
        {
            get { return (_settings.ContainsKey(key) ? _settings[key] : null); }
            set { _settings[key] = value; }
        }

        public bool ContainsKey(string key) => _settings.ContainsKey(key);

        public void Add(string key, string value) => _settings.Add(key, value);

        public bool Remove(string key) => _settings.Remove(key);

        public bool TryGetValue(string key, out string value) => _settings.TryGetValue(key, out value);

        public void Add(KeyValuePair<string, string> item) => _settings.Add(item.Key, item.Value);

        public void Clear() => _settings.Clear();

        public bool Contains(KeyValuePair<string, string> item) => ((IDictionary<string, string>)_settings).Contains(item);

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) => ((IDictionary<string, string>)_settings).CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<string, string> item) => ((IDictionary<string, string>)_settings).Remove(item);

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _settings.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _settings.GetEnumerator();

        #endregion
    }
}
