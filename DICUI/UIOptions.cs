using System.Collections.Generic;
using System.Configuration;
using DICUI.Data;

namespace DICUI
{
    public class UIOptions
    {
        public Options Options { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public UIOptions()
        {
            Load();
        }

        /// <summary>
        /// Save a configuration from internal Options
        /// </summary>
        public void Save()
        {
            Configuration configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Loop through all settings in Options and save them, overwriting existing settings
            foreach (var kvp in Options)
            {
                configFile.AppSettings.Settings.Remove(kvp.Key);
                configFile.AppSettings.Settings.Add(kvp.Key, kvp.Value);
            }

            configFile.Save(ConfigurationSaveMode.Modified);
        }

        /// <summary>
        /// Load a configuration into internal Options
        /// </summary>
        private void Load()
        {
            Configuration configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var settings = ConvertToDictionary(configFile);
            Options = new Options(settings);
        }

        /// <summary>
        /// Get a setting value from the base Options
        /// </summary>
        /// <param name="key">Key to retrieve the value for</param>
        /// <returns>String value if possible, null otherwise</returns>
        public string Get(string key)
        {
            return Options[key];
        }

        /// <summary>
        /// Set a setting value in the base Options
        /// </summary>
        /// <param name="key">Key to set the value for</param>
        /// <param name="value">Value to set</param>
        public void Set(string key, string value)
        {
            Options[key] = value;
        }

        /// <summary>
        /// Get the preferred dumping speed given a media type
        /// </summary>
        /// <param name="type">MediaType representing the current selection</param>
        /// <returns>Int value representing the dump speed</returns>
        public int GetPreferredDumpSpeedForMediaType(MediaType? type)
        {
            switch (type)
            {
                case MediaType.CDROM:
                case MediaType.GDROM:
                    return this.Options.PreferredDumpSpeedCD;
                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    return this.Options.PreferredDumpSpeedDVD;
                case MediaType.BluRay:
                    return this.Options.PreferredDumpSpeedBD;
                default:
                    return this.Options.PreferredDumpSpeedCD;
            }
        }

        /// <summary>
        /// Convert the configuration app settings to a dictionary
        /// </summary>
        /// <param name="configFile">Configuration to load from</param>
        /// <returns>Dictionary with all values from app settings</returns>
        private Dictionary<string, string> ConvertToDictionary(Configuration configFile)
        {
            var settings = configFile.AppSettings.Settings;
            var dict = new Dictionary<string, string>();

            foreach (string key in settings.AllKeys)
            {
                dict[key] = settings[key]?.Value ?? string.Empty;
            }

            return dict;
        }
    }
}
