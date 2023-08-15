using System.Collections.Generic;
using System.Configuration;
using MPF.Core.Converters;
using MPF.Core.Data;

namespace MPF.Core.Utilities
{
    public static class OptionsLoader
    {
        #region Arguments

        /// <summary>
        /// Load the current set of options from application arguments
        /// </summary>
        public static (Options, string, int) LoadFromArguments(string[] args, int startIndex = 0)
        {
            // Create the output values with defaults
            var options = new Options()
            {
                RedumpUsername = null,
                RedumpPassword = null,
                InternalProgram = InternalProgram.NONE,
                OutputSubmissionJSON = false,
                CompressLogFiles = false,
            };

            string parsedPath = null;

            // These values require multiple parts to be active
            bool scan = false, protectFile = false;

            // If we have no arguments, just return
            if (args == null || args.Length == 0)
                return (options, null, 0);

            // If we have an invalid start index, just return
            if (startIndex < 0 || startIndex >= args.Length)
                return (options, null, startIndex);

            // Loop through the arguments and parse out values
            for (; startIndex < args.Length; startIndex++)
            {
                // Redump login
                if (args[startIndex].StartsWith("-c=") || args[startIndex].StartsWith("--credentials="))
                {
                    string[] credentials = args[startIndex].Split('=')[1].Split(';');
                    options.RedumpUsername = credentials[0];
                    options.RedumpPassword = credentials[1];
                }
                else if (args[startIndex] == "-c" || args[startIndex] == "--credentials")
                {
                    options.RedumpUsername = args[startIndex + 1];
                    options.RedumpPassword = args[startIndex + 2];
                    startIndex += 2;
                }

                // Use specific program
                else if (args[startIndex].StartsWith("-u=") || args[startIndex].StartsWith("--use="))
                {
                    string internalProgram = args[startIndex].Split('=')[1];
                    options.InternalProgram = EnumConverter.ToInternalProgram(internalProgram);
                }
                else if (args[startIndex] == "-u" || args[startIndex] == "--use")
                {
                    string internalProgram = args[startIndex + 1];
                    options.InternalProgram = EnumConverter.ToInternalProgram(internalProgram);
                    startIndex++;
                }

                // Use a device path for physical checks
                else if (args[startIndex].StartsWith("-p=") || args[startIndex].StartsWith("--path="))
                {
                    parsedPath = args[startIndex].Split('=')[1];
                }
                else if (args[startIndex] == "-p" || args[startIndex] == "--path")
                {
                    parsedPath = args[startIndex + 1];
                    startIndex++;
                }

                // Scan for protection (requires device path)
                else if (args[startIndex].Equals("-s") || args[startIndex].Equals("--scan"))
                {
                    scan = true;
                }

                // Output protection to separate file (requires scan for protection)
                else if (args[startIndex].Equals("-f") || args[startIndex].Equals("--protect-file"))
                {
                    protectFile = true;
                }

                // Output submission JSON
                else if (args[startIndex].Equals("-j") || args[startIndex].Equals("--json"))
                {
                    options.OutputSubmissionJSON = true;
                }

                // Compress log and extraneous files
                else if (args[startIndex].Equals("-z") || args[startIndex].Equals("--zip"))
                {
                    options.CompressLogFiles = true;
                }

                // Default, we fall out
                else
                {
                    break;
                }
            }

            // Now deal with the complex options
            options.ScanForProtection = scan && !string.IsNullOrWhiteSpace(parsedPath);
            options.OutputSeparateProtectionFile = scan && protectFile && !string.IsNullOrWhiteSpace(parsedPath);

            return (options, parsedPath, startIndex);
        }

        /// <summary>
        /// Return a list of supported arguments and descriptions
        /// </summary>
        public static List<string> PrintSupportedArguments()
        {
            var supportedArguments = new List<string>();

            supportedArguments.Add("-u, --use <program>            Dumping program output type [REQUIRED]");
            supportedArguments.Add("-c, --credentials <user> <pw>  Redump username and password");
            supportedArguments.Add("-p, --path <drivepath>         Physical drive path for additional checks");
            supportedArguments.Add("-s, --scan                     Enable copy protection scan (requires --path)");
            supportedArguments.Add("-f, --protect-file             Output protection to separate file (requires --scan)");
            supportedArguments.Add("-j, --json                     Enable submission JSON output");
            supportedArguments.Add("-z, --zip                      Enable log file compression");

            return supportedArguments;
        }

        #endregion

        #region Configuration

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

        #endregion
    }
}
