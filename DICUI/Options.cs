using System;
using System.Configuration;
using System.Reflection;
using DICUI.Data;

namespace DICUI
{
    public class Options
    {
        public string DefaultOutputPath { get; private set; }
        public string DICPath { get; private set; }
        public string SubDumpPath { get; private set; }

        public int preferredDumpSpeedCD { get; set; }
        public int preferredDumpSpeedDVD { get; set; }

        public bool QuietMode { get; set; }
        public bool ParanoidMode { get; set; }
        public int RereadAmountForC2 { get; set; }

        public bool SkipMediaTypeDetection { get; set; }

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
            //TODO: hardcoded, we should find a better way
            DICPath = ConfigurationManager.AppSettings["DICPath"] ?? @"Programs\DiscImageCreator.exe";
            SubDumpPath = ConfigurationManager.AppSettings["SubDumpPath"] ?? "subdump.exe";
            DefaultOutputPath = ConfigurationManager.AppSettings["DefaultOutputPath"] ?? "ISO";

            this.preferredDumpSpeedCD = Int32.TryParse(ConfigurationManager.AppSettings["preferredDumpSpeedCD"], out int maxDumpSpeedCD) ? maxDumpSpeedCD : 72;
            this.preferredDumpSpeedDVD = Int32.TryParse(ConfigurationManager.AppSettings["preferredDumpSpeedDVD"], out int maxDumpSpeedDVD) ? maxDumpSpeedDVD : 72;

            this.QuietMode = Boolean.TryParse(ConfigurationManager.AppSettings["QuietMode"], out bool quietMode) ? quietMode : false;
            this.ParanoidMode = Boolean.TryParse(ConfigurationManager.AppSettings["ParanoidMode"], out bool paranoidMode) ? paranoidMode : false;
            this.SkipMediaTypeDetection = Boolean.TryParse(ConfigurationManager.AppSettings["SkipMediaTypeDetection"], out bool skipMediaTypeDetection) ? skipMediaTypeDetection : false;
            this.RereadAmountForC2 = Int32.TryParse(ConfigurationManager.AppSettings["RereadAmountForC2"], out int rereadAmountForC2) ? rereadAmountForC2 : 20;
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
                case MediaType.CD: return preferredDumpSpeedCD;
                case MediaType.DVD: return preferredDumpSpeedDVD;
                default: return 8;
            }
        }
    }
}
