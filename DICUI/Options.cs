using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using DICUI.Data;

namespace DICUI
{
    public class Options
    {
        public string AaruPath { get; private set; }
        public string CreatorPath { get; private set; }
        public string DDPath { get; private set; }

        public string DefaultOutputPath { get; private set; }
        public string SubDumpPath { get; private set; }

        public string InternalProgram { get; set; }

        public int PreferredDumpSpeedCD { get; set; }
        public int PreferredDumpSpeedDVD { get; set; }
        public int PreferredDumpSpeedBD { get; set; }

        public bool QuietMode { get; set; }
        public bool ParanoidMode { get; set; }
        public bool ScanForProtection { get; set; }
        public int RereadAmountForC2 { get; set; }
        public bool AddPlaceholders { get; set; }
        public bool PromptForDiscInformation { get; set; }
        public bool IgnoreFixedDrives { get; set; }
        public bool ResetDriveAfterDump { get; set; }

        public bool SkipMediaTypeDetection { get; set; }
        public bool SkipSystemDetection { get; set; }

        public bool VerboseLogging { get; set; }
        public bool OpenLogWindowAtStartup { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        public void Save()
        {
            Configuration configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            //TODO: reflection is used
            //TODO: is remove needed, doesn't the value get directly overridden
            Array.ForEach(
                GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance),
                p => {
                    configFile.AppSettings.Settings.Remove(p.Name);
                    configFile.AppSettings.Settings.Add(p.Name, Convert.ToString(p.GetValue(this)));
                }
            );

            configFile.Save(ConfigurationSaveMode.Modified);
        }

        public void Load()
        {
            Configuration configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            //TODO: hardcoded, we should find a better way
            this.AaruPath = GetStringSetting(configFile, "AaruPath", "Programs\\Aaru\\Aaru.exe");
            this.CreatorPath = GetStringSetting(configFile, "CreatorPath", "Programs\\Creator\\DiscImageCreator.exe");
            this.DDPath = GetStringSetting(configFile, "DDPath", "Programs\\DD\\dd.exe");
            this.SubDumpPath = GetStringSetting(configFile, "SubDumpPath", "Programs\\Subdump\\subdump.exe");
            this.DefaultOutputPath = GetStringSetting(configFile, "DefaultOutputPath", "ISO");
            this.InternalProgram = GetStringSetting(configFile, "InternalProgram", Data.InternalProgram.DiscImageCreator.ToString());

            this.PreferredDumpSpeedCD = GetInt32Setting(configFile, "PreferredDumpSpeedCD", 72);
            this.PreferredDumpSpeedDVD = GetInt32Setting(configFile, "PreferredDumpSpeedDVD", 24);
            this.PreferredDumpSpeedBD = GetInt32Setting(configFile, "PreferredDumpSpeedBD", 16);

            this.QuietMode = GetBooleanSetting(configFile, "QuietMode", false);
            this.ParanoidMode = GetBooleanSetting(configFile, "ParanoidMode", false);
            this.ScanForProtection = GetBooleanSetting(configFile, "ScanForProtection", true);
            this.SkipMediaTypeDetection = GetBooleanSetting(configFile, "SkipMediaTypeDetection", false);
            this.SkipSystemDetection = GetBooleanSetting(configFile, "SkipSystemDetection", false);
            this.RereadAmountForC2 = GetInt32Setting(configFile, "RereadAmountForC2", 20);
            this.VerboseLogging = GetBooleanSetting(configFile, "VerboseLogging", true);
            this.OpenLogWindowAtStartup = GetBooleanSetting(configFile, "OpenLogWindowAtStartup", true);
            this.AddPlaceholders = GetBooleanSetting(configFile, "AddPlaceholders", true);
            this.PromptForDiscInformation = GetBooleanSetting(configFile, "PromptForDiscInformation", true);
            this.IgnoreFixedDrives = GetBooleanSetting(configFile, "IgnoreFixedDrives", false);
            this.ResetDriveAfterDump = GetBooleanSetting(configFile, "ResetDriveAfterDump", false);

            this.Username = GetStringSetting(configFile, "Username", "");
            this.Password = GetStringSetting(configFile, "Password", "");
        }

        /// <summary>
        /// Get a boolean setting from a configuration
        /// </summary>
        /// <param name="configFile">Current configuration file</param>
        /// <param name="key">Setting key to get a value for</param>
        /// <param name="defaultValue">Default value to return if no value is found</param>
        /// <returns>Setting value if possible, default value otherwise</returns>
        public bool GetBooleanSetting(Configuration configFile, string key, bool defaultValue)
        {
            var settings = configFile.AppSettings.Settings;
            if (settings.AllKeys.Contains(key))
            {
                if (Boolean.TryParse(settings[key].Value, out bool value))
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
        /// Get a boolean setting from a configuration
        /// </summary>
        /// <param name="configFile">Current configuration file</param>
        /// <param name="key">Setting key to get a value for</param>
        /// <param name="defaultValue">Default value to return if no value is found</param>
        /// <returns>Setting value if possible, default value otherwise</returns>
        public int GetInt32Setting(Configuration configFile, string key, int defaultValue)
        {
            var settings = configFile.AppSettings.Settings;
            if (settings.AllKeys.Contains(key))
            {
                if (Int32.TryParse(settings[key].Value, out int value))
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
        /// Get a boolean setting from a configuration
        /// </summary>
        /// <param name="configFile">Current configuration file</param>
        /// <param name="key">Setting key to get a value for</param>
        /// <param name="defaultValue">Default value to return if no value is found</param>
        /// <returns>Setting value if possible, default value otherwise</returns>
        public long GetInt64Setting(Configuration configFile, string key, long defaultValue)
        {
            var settings = configFile.AppSettings.Settings;
            if (settings.AllKeys.Contains(key))
            {
                if (Int64.TryParse(settings[key].Value, out long value))
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
        /// Get a boolean setting from a configuration
        /// </summary>
        /// <param name="configFile">Current configuration file</param>
        /// <param name="key">Setting key to get a value for</param>
        /// <param name="defaultValue">Default value to return if no value is found</param>
        /// <returns>Setting value if possible, default value otherwise</returns>
        public string GetStringSetting(Configuration configFile, string key, string defaultValue)
        {
            var settings = configFile.AppSettings.Settings;
            if (settings.AllKeys.Contains(key))
                return settings[key].Value;
            else
                return defaultValue;
        }

        //TODO: probably should be generic for non-string options
        //TODO: using reflection for Set and Get is orthodox but it works, should be changed to a key,value map probably
        public void Set(string key, string value)
        {
            GetType().GetProperty(key, BindingFlags.Public | BindingFlags.Instance).SetValue(this, value);
        }

        public string Get(string key)
        {
            return GetType().GetProperty(key, BindingFlags.Public | BindingFlags.Instance).GetValue(this) as string;
        }

        public int GetPreferredDumpSpeedForMediaType(MediaType? type)
        {
            switch (type)
            {
                case MediaType.CDROM:
                case MediaType.GDROM:
                    return PreferredDumpSpeedCD;
                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    return PreferredDumpSpeedDVD;
                case MediaType.BluRay:
                    return PreferredDumpSpeedBD;
                default:
                    return 8;
            }
        }
    }
}
