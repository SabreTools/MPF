using DICUI.Data;
using System;
using System.Configuration;
using System.Reflection;

namespace DICUI
{
    public class Options
    {
        public string defaultOutputPath { get; private set; }
        public string dicPath { get; private set; }
        public string subdumpPath { get; private set; }

        public int maxDumpSpeedCD { get; set; }
        public int maxDumpSpeedDVD { get; set; }

        public void Save()
        {
            Configuration configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            //TODO: reflection is used
            Array.ForEach(
                GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance), 
                p => {
                    configFile.AppSettings.Settings.Remove(p.Name);
                    configFile.AppSettings.Settings.Add(p.Name, p.GetValue(this) as string);
                }
            );

            //TODO: is remove needed, doesn't the value get directly overridden?
            configFile.AppSettings.Settings.Remove("maxDumpSpeedCD");
            configFile.AppSettings.Settings.Add("maxDumpSpeedCD", Convert.ToString(maxDumpSpeedCD));

            configFile.AppSettings.Settings.Remove("maxDumpSpeedDVD");
            configFile.AppSettings.Settings.Add("maxDumpSpeedDVD", Convert.ToString(maxDumpSpeedDVD));

            configFile.Save(ConfigurationSaveMode.Modified);
        }

        public void Load()
        {
            //TODO: hardcoded, we should find a better way
            dicPath = ConfigurationManager.AppSettings["dicPath"] ?? @"Programs\DiscImageCreator.exe";
            subdumpPath = ConfigurationManager.AppSettings["subdumpPath"] ?? "subdump.exe";
            defaultOutputPath = ConfigurationManager.AppSettings["defaultOutputPath"] ?? "ISO";

            this.maxDumpSpeedCD = Int32.TryParse(ConfigurationManager.AppSettings["maxDumpSpeedCD"], out int maxDumpSpeedCD) ? maxDumpSpeedCD : 72;
            this.maxDumpSpeedDVD = Int32.TryParse(ConfigurationManager.AppSettings["maxDumpSpeedDVD"], out int maxDumpSpeedDVD) ? maxDumpSpeedDVD : 72;
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
                case MediaType.CD: return maxDumpSpeedCD;
                case MediaType.DVD: return maxDumpSpeedDVD;
                default: return 1; //TODO: think what we want here
            }
        }
    }
}
