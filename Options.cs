using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DICUI
{
    public class Options
    {
        public string defaultOutputPath { get; private set; }
        public string dicPath { get; private set; }
        public string psxtPath { get; private set; }
        public string subdumpPath { get; private set; }

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

            configFile.Save(ConfigurationSaveMode.Modified);
        }

        public void Load()
        {
            //TODO: hardcoded, we should find a better way
            dicPath = ConfigurationManager.AppSettings["dicPath"] ?? @"Programs\DiscImageCreator.exe";
            psxtPath = ConfigurationManager.AppSettings["psxt001zPath"] ?? "psxt001z.exe";
            subdumpPath = ConfigurationManager.AppSettings["subdumpPath"] ?? "subdump.exe";
            defaultOutputPath = ConfigurationManager.AppSettings["defaultOutputPath"] ?? "ISO";
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
    }
}
