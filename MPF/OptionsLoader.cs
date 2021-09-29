using System.Collections.Generic;
using System.Configuration;
using MPF.Core.Data;

namespace MPF
{
    public static class OptionsLoader
    {
        /// <summary>
        /// Load the current set of options from the application configuration
        /// </summary>
        public static Options LoadFromConfig()
        {
            Configuration configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var settings = configFile.AppSettings.Settings;
            var dict = new Dictionary<string, string>();

            foreach (string key in settings.AllKeys)
            {
                dict[key] = settings[key]?.Value ?? string.Empty;
            }

            return new Options(dict);
        }

        /// <summary>
        /// Save the current set of options to the application configuration
        /// </summary>
        public static void SaveToConfig(Options options)
        {
            Configuration configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Loop through all settings in Options and save them, overwriting existing settings
            foreach (var kvp in options)
            {
                configFile.AppSettings.Settings.Remove(kvp.Key);
                configFile.AppSettings.Settings.Add(kvp.Key, kvp.Value);
            }

            configFile.Save(ConfigurationSaveMode.Modified);
        }
    }
}
