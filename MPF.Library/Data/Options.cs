using System;
using System.Collections;
using System.Collections.Generic;

namespace MPF.Data
{
    public class Options : IDictionary<string, string>
    {
        private Dictionary<string, string> _settings;

        #region Internal Program

        public string AaruPath
        {
            get { return GetStringSetting(_settings, "AaruPath", "Programs\\Aaru\\Aaru.exe"); }
            set { _settings["AaruPath"] = value; }
        }

        public string CreatorPath
        {
            get { return GetStringSetting(_settings, "CreatorPath", "Programs\\Creator\\DiscImageCreator.exe"); }
            set { _settings["CreatorPath"] = value; }
        }

        public string DDPath
        {
            get { return GetStringSetting(_settings, "DDPath", "Programs\\DD\\dd.exe"); }
            set { _settings["DDPath"] = value; }
        }

        public string InternalProgram
        {
            get { return GetStringSetting(_settings, "InternalProgram", Data.InternalProgram.DiscImageCreator.ToString()); }
            set { _settings["InternalProgram"] = value; }
        }

        #endregion

        #region Extra Paths

        public string DefaultOutputPath
        {
            get { return GetStringSetting(_settings, "DefaultOutputPath", "ISO"); }
            set { _settings["DefaultOutputPath"] = value; }
        }

        #endregion

        #region Dumping Speeds

        public int PreferredDumpSpeedCD
        {
            get { return GetInt32Setting(_settings, "PreferredDumpSpeedCD", 72); }
            set { _settings["PreferredDumpSpeedCD"] = value.ToString(); }
        }

        public int PreferredDumpSpeedDVD
        {
            get { return GetInt32Setting(_settings, "PreferredDumpSpeedDVD", 24); }
            set { _settings["PreferredDumpSpeedDVD"] = value.ToString(); }
        }

        public int PreferredDumpSpeedBD
        {
            get { return GetInt32Setting(_settings, "PreferredDumpSpeedBD", 16); }
            set { _settings["PreferredDumpSpeedBD"] = value.ToString(); }
        }

        #endregion

        #region Extra Dumping Options

        public bool QuietMode
        {
            get { return GetBooleanSetting(_settings, "QuietMode", false); }
            set { _settings["QuietMode"] = value.ToString(); }
        }

        public bool ParanoidMode
        {
            get { return GetBooleanSetting(_settings, "ParanoidMode", false); }
            set { _settings["ParanoidMode"] = value.ToString(); }
        }

        public bool ScanForProtection
        {
            get { return GetBooleanSetting(_settings, "ScanForProtection", true); }
            set { _settings["ScanForProtection"] = value.ToString(); }
        }

        public int RereadAmountForC2
        {
            get { return GetInt32Setting(_settings, "RereadAmountForC2", 20); }
            set { _settings["RereadAmountForC2"] = value.ToString(); }
        }

        public bool AddPlaceholders
        {
            get { return GetBooleanSetting(_settings, "AddPlaceholders", true); }
            set { _settings["AddPlaceholders"] = value.ToString(); }
        }

        public bool PromptForDiscInformation
        {
            get { return GetBooleanSetting(_settings, "PromptForDiscInformation", true); }
            set { _settings["PromptForDiscInformation"] = value.ToString(); }
        }

        public bool IgnoreFixedDrives
        {
            get { return GetBooleanSetting(_settings, "IgnoreFixedDrives", true); }
            set { _settings["IgnoreFixedDrives"] = value.ToString(); }
        }

        public bool ResetDriveAfterDump
        {
            get { return GetBooleanSetting(_settings, "ResetDriveAfterDump", false); }
            set { _settings["ResetDriveAfterDump"] = value.ToString(); }
        }

        #endregion

        #region Skip Options

        public bool SkipMediaTypeDetection
        {
            get { return GetBooleanSetting(_settings, "SkipMediaTypeDetection", false); }
            set { _settings["SkipMediaTypeDetection"] = value.ToString(); }
        }

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

        public string Username
        {
            get { return GetStringSetting(_settings, "Username", ""); }
            set { _settings["Username"] = value; }
        }

        // TODO: Figure out a way to keep this encrypted in some way, BASE64 to start?
        public string Password
        {
            get { return GetStringSetting(_settings, "Password", ""); }
            set { _settings["Password"] = value; }
        }

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
