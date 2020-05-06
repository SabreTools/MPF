using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
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
            Options = new Options();
        }

        /// <summary>
        /// Move these to some utility class in DICUI application
        /// </summary>
        public void Save()
        {
            Configuration configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            //TODO: reflection is used
            //TODO: is remove needed, doesn't the value get directly overridden
            Array.ForEach(
                Options.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance),
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

            var settings = ConvertToDictionary(configFile);
            Options.Load(settings);
        }

        /// <summary>
        /// Convert the AppSettings to a dictionary
        /// </summary>
        /// <param name="configFile"></param>
        /// <returns></returns>
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

        //TODO: probably should be generic for non-string options
        //TODO: using reflection for Set and Get is orthodox but it works, should be changed to a key,value map probably
        public void Set(string key, string value)
        {
           Options.GetType().GetProperty(key, BindingFlags.Public | BindingFlags.Instance).SetValue(this, value);
        }

        public string Get(string key)
        {
            return Options.GetType().GetProperty(key, BindingFlags.Public | BindingFlags.Instance).GetValue(this) as string;
        }

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
    }
}
