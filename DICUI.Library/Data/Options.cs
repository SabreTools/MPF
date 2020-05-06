using System;
using System.Collections.Generic;

namespace DICUI.Data
{
    public class Options
    {
        #region Internal Program

        public string AaruPath { get; set; }
        public string CreatorPath { get; set; }
        public string DDPath { get; set; }
        public string InternalProgram { get; set; }

        #endregion

        #region Extra Paths

        public string DefaultOutputPath { get; set; }
        public string SubDumpPath { get; set; }

        #endregion

        #region Dumping Speeds

        public int PreferredDumpSpeedCD { get; set; }
        public int PreferredDumpSpeedDVD { get; set; }
        public int PreferredDumpSpeedBD { get; set; }

        #endregion

        #region Extra Dumping Options

        public bool QuietMode { get; set; }
        public bool ParanoidMode { get; set; }
        public bool ScanForProtection { get; set; }
        public int RereadAmountForC2 { get; set; }
        public bool AddPlaceholders { get; set; }
        public bool PromptForDiscInformation { get; set; }
        public bool IgnoreFixedDrives { get; set; }
        public bool ResetDriveAfterDump { get; set; }

        #endregion

        #region Skip Options

        public bool SkipMediaTypeDetection { get; set; }
        public bool SkipSystemDetection { get; set; }

        #endregion

        #region Logging Options

        public bool VerboseLogging { get; set; }
        public bool OpenLogWindowAtStartup { get; set; }

        #endregion

        #region Redump Login Information

        public string Username { get; set; }
        public string Password { get; set; }

        #endregion

        /// <summary>
        /// Load settings from a Dictionary<string, string></string>
        /// </summary>
        /// <param name="settings">Settings dictionary to pull from</param>
        public void Load(Dictionary<string, string> settings)
        {
            // Internal Program
            this.AaruPath = GetStringSetting(settings, "AaruPath", "Programs\\Aaru\\Aaru.exe");
            this.CreatorPath = GetStringSetting(settings, "CreatorPath", "Programs\\Creator\\DiscImageCreator.exe");
            this.DDPath = GetStringSetting(settings, "DDPath", "Programs\\DD\\dd.exe");
            this.InternalProgram = GetStringSetting(settings, "InternalProgram", Data.InternalProgram.DiscImageCreator.ToString());

            // Extra Paths
            this.DefaultOutputPath = GetStringSetting(settings, "DefaultOutputPath", "ISO");
            this.SubDumpPath = GetStringSetting(settings, "SubDumpPath", "Programs\\Subdump\\subdump.exe");

            // Dumping Speeds
            this.PreferredDumpSpeedCD = GetInt32Setting(settings, "PreferredDumpSpeedCD", 72);
            this.PreferredDumpSpeedDVD = GetInt32Setting(settings, "PreferredDumpSpeedDVD", 24);
            this.PreferredDumpSpeedBD = GetInt32Setting(settings, "PreferredDumpSpeedBD", 16);

            // Extra Dumping Options
            this.QuietMode = GetBooleanSetting(settings, "QuietMode", false);
            this.ParanoidMode = GetBooleanSetting(settings, "ParanoidMode", false);
            this.ScanForProtection = GetBooleanSetting(settings, "ScanForProtection", true);
            this.RereadAmountForC2 = GetInt32Setting(settings, "RereadAmountForC2", 20);
            this.AddPlaceholders = GetBooleanSetting(settings, "AddPlaceholders", true);
            this.PromptForDiscInformation = GetBooleanSetting(settings, "PromptForDiscInformation", true);
            this.IgnoreFixedDrives = GetBooleanSetting(settings, "IgnoreFixedDrives", false);
            this.ResetDriveAfterDump = GetBooleanSetting(settings, "ResetDriveAfterDump", false);

            // Skip Options
            this.SkipMediaTypeDetection = GetBooleanSetting(settings, "SkipMediaTypeDetection", false);
            this.SkipSystemDetection = GetBooleanSetting(settings, "SkipSystemDetection", false);

            // Logging Options
            this.VerboseLogging = GetBooleanSetting(settings, "VerboseLogging", true);
            this.OpenLogWindowAtStartup = GetBooleanSetting(settings, "OpenLogWindowAtStartup", true);

            // Redump Login Information
            this.Username = GetStringSetting(settings, "Username", "");
            this.Password = GetStringSetting(settings, "Password", "");
        }

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
    }
}
