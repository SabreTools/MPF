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
        public bool AddPlaceholders { get; set; }

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
            Configuration configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            //TODO: hardcoded, we should find a better way
            DICPath = configFile.AppSettings.Settings["DICPath"].Value ?? @"Programs\DiscImageCreator.exe";
            SubDumpPath = configFile.AppSettings.Settings["SubDumpPath"].Value ?? "subdump.exe";
            DefaultOutputPath = configFile.AppSettings.Settings["DefaultOutputPath"].Value ?? "ISO";

            this.PreferredDumpSpeedCD = Int32.TryParse(configFile.AppSettings.Settings["PreferredDumpSpeedCD"].Value, out int maxDumpSpeedCD) ? maxDumpSpeedCD : 72;
            this.PreferredDumpSpeedDVD = Int32.TryParse(configFile.AppSettings.Settings["PreferredDumpSpeedDVD"].Value, out int maxDumpSpeedDVD) ? maxDumpSpeedDVD : 24;
            this.PreferredDumpSpeedBD = Int32.TryParse(configFile.AppSettings.Settings["PreferredDumpSpeedBD"].Value, out int maxDumpSpeedBD) ? maxDumpSpeedBD : 16;

            this.QuietMode = Boolean.TryParse(configFile.AppSettings.Settings["QuietMode"].Value, out bool quietMode) ? quietMode : false;
            this.ParanoidMode = Boolean.TryParse(configFile.AppSettings.Settings["ParanoidMode"].Value, out bool paranoidMode) ? paranoidMode : false;
            this.ScanForProtection = Boolean.TryParse(configFile.AppSettings.Settings["ScanForProtection"].Value, out bool scanForProtection) ? scanForProtection : true;
            this.SkipMediaTypeDetection = Boolean.TryParse(configFile.AppSettings.Settings["SkipMediaTypeDetection"].Value, out bool skipMediaTypeDetection) ? skipMediaTypeDetection : false;
            this.RereadAmountForC2 = Int32.TryParse(configFile.AppSettings.Settings["RereadAmountForC2"].Value, out int rereadAmountForC2) ? rereadAmountForC2 : 20;
            this.VerboseLogging = Boolean.TryParse(configFile.AppSettings.Settings["VerboseLogging"].Value, out bool verboseLogging) ? verboseLogging : true;
            this.OpenLogWindowAtStartup = Boolean.TryParse(configFile.AppSettings.Settings["OpenLogWindowAtStartup"].Value, out bool openLogWindowAtStartup) ? openLogWindowAtStartup : true;
            this.AddPlaceholders = Boolean.TryParse(configFile.AppSettings.Settings["AddPlaceholders"].Value, out bool addPlaceholders) ? addPlaceholders : true;

            this.Username = configFile.AppSettings.Settings["Username"].Value ?? "";
            this.Password = configFile.AppSettings.Settings["Password"].Value ?? "";
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
