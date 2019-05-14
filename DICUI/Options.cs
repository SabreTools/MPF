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

        public int PreferredDumpSpeedCD { get; set; }
        public int PreferredDumpSpeedDVD { get; set; }
        public int PreferredDumpSpeedBD { get; set; }

        public bool QuietMode { get; set; }
        public bool ParanoidMode { get; set; }
        public bool ScanForProtection { get; set; }
        public int RereadAmountForC2 { get; set; }

        public bool SkipMediaTypeDetection { get; set; }

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
            //TODO: hardcoded, we should find a better way
            DICPath = ConfigurationManager.AppSettings["DICPath"] ?? @"Programs\DiscImageCreator.exe";
            SubDumpPath = ConfigurationManager.AppSettings["SubDumpPath"] ?? "subdump.exe";
            DefaultOutputPath = ConfigurationManager.AppSettings["DefaultOutputPath"] ?? "ISO";

            this.PreferredDumpSpeedCD = Int32.TryParse(ConfigurationManager.AppSettings["preferredDumpSpeedCD"], out int maxDumpSpeedCD) ? maxDumpSpeedCD : 72;
            this.PreferredDumpSpeedDVD = Int32.TryParse(ConfigurationManager.AppSettings["preferredDumpSpeedDVD"], out int maxDumpSpeedDVD) ? maxDumpSpeedDVD : 24;
            this.PreferredDumpSpeedBD = Int32.TryParse(ConfigurationManager.AppSettings["preferredDumpSpeedBD"], out int maxDumpSpeedBD) ? maxDumpSpeedBD : 16;

            this.QuietMode = Boolean.TryParse(ConfigurationManager.AppSettings["QuietMode"], out bool quietMode) ? quietMode : false;
            this.ParanoidMode = Boolean.TryParse(ConfigurationManager.AppSettings["ParanoidMode"], out bool paranoidMode) ? paranoidMode : false;
            this.ScanForProtection = Boolean.TryParse(ConfigurationManager.AppSettings["ScanForProtection"], out bool scanForProtection) ? scanForProtection : true;
            this.SkipMediaTypeDetection = Boolean.TryParse(ConfigurationManager.AppSettings["SkipMediaTypeDetection"], out bool skipMediaTypeDetection) ? skipMediaTypeDetection : false;
            this.RereadAmountForC2 = Int32.TryParse(ConfigurationManager.AppSettings["RereadAmountForC2"], out int rereadAmountForC2) ? rereadAmountForC2 : 20;
            this.VerboseLogging = Boolean.TryParse(ConfigurationManager.AppSettings["VerboseLogging"], out bool verboseLogging) ? verboseLogging : true;
            this.OpenLogWindowAtStartup = Boolean.TryParse(ConfigurationManager.AppSettings["OpenLogWindowAtStartup"], out bool openLogWindowAtStartup) ? openLogWindowAtStartup : true;

            this.Username = ConfigurationManager.AppSettings["username"] ?? "";
            this.Password = ConfigurationManager.AppSettings["password"] ?? "";
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
