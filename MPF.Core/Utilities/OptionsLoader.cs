using System;
using System.Collections.Generic;
using System.IO;
using MPF.Core.Converters;
using MPF.Core.Data;
using Newtonsoft.Json;
using SabreTools.RedumpLib.Data;

namespace MPF.Core.Utilities
{
    public static class OptionsLoader
    {
        private const string ConfigurationPath = "config.json";

        #region Arguments

        /// <summary>
        /// Process any standalone arguments for the program
        /// </summary>
        /// <returns>True if one of the arguments was processed, false otherwise</returns>
        public static bool? ProcessStandaloneArguments(string[] args)
        {
            // Help options
            if (args.Length == 0 || args[0] == "-h" || args[0] == "-?")
                return false;

            // List options
            if (args[0] == "-lm" || args[0] == "--listmedia")
            {
                Console.WriteLine("Supported Media Types:");
                foreach (string mediaType in Extensions.ListMediaTypes())
                {
                    Console.WriteLine(mediaType);
                }
                Console.ReadLine();
                return true;
            }
            else if (args[0] == "-lp" || args[0] == "--listprograms")
            {
                Console.WriteLine("Supported Programs:");
                foreach (string program in EnumExtensions.ListPrograms())
                {
                    Console.WriteLine(program);
                }
                Console.ReadLine();
                return true;
            }
            else if (args[0] == "-ls" || args[0] == "--listsystems")
            {
                Console.WriteLine("Supported Systems:");
                foreach (string system in Extensions.ListSystems())
                {
                    Console.WriteLine(system);
                }
                Console.ReadLine();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Process common arguments for all functionality
        /// </summary>
        /// <returns>True if all arguments pass, false otherwise</returns>
#if NET48
        public static (bool, MediaType, RedumpSystem?, string) ProcessCommonArguments(string[] args)
#else
        public static (bool, MediaType, RedumpSystem?, string?) ProcessCommonArguments(string[] args)
#endif
        {
            // All other use requires at least 3 arguments
            if (args.Length < 3)
                return (false, MediaType.NONE, null, "Invalid number of arguments");

            // Check the MediaType
            var mediaType = EnumConverter.ToMediaType(args[0].Trim('"'));
            if (mediaType == MediaType.NONE)
                return (false, MediaType.NONE, null, $"{args[0]} is not a recognized media type");

            // Check the RedumpSystem
            var knownSystem = Extensions.ToRedumpSystem(args[1].Trim('"'));
            if (knownSystem == null)
                return (false, MediaType.NONE, null, $"{args[1]} is not a recognized system");

            return (true, mediaType, knownSystem, null);
        }

        /// <summary>
        /// Load the current set of options from application arguments
        /// </summary>
#if NET48
        public static (Options, SubmissionInfo, string, int) LoadFromArguments(string[] args, int startIndex = 0)
#else
        public static (Options, SubmissionInfo?, string?, int) LoadFromArguments(string[] args, int startIndex = 0)
#endif
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

            // Create the submission info to return, if necessary
#if NET48
            SubmissionInfo info = null;
            string parsedPath = null;
#else
            SubmissionInfo? info = null;
            string? parsedPath = null;
#endif

            // These values require multiple parts to be active
            bool scan = false, protectFile = false;

            // If we have no arguments, just return
            if (args == null || args.Length == 0)
                return (options, null, null, 0);

            // If we have an invalid start index, just return
            if (startIndex < 0 || startIndex >= args.Length)
                return (options, null, null, startIndex);

            // Loop through the arguments and parse out values
            for (; startIndex < args.Length; startIndex++)
            {
                // Use specific program
                if (args[startIndex].StartsWith("-u=") || args[startIndex].StartsWith("--use="))
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

                // Redump login
                else if (args[startIndex].StartsWith("-c=") || args[startIndex].StartsWith("--credentials="))
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

                // Pull all information (requires Redump login)
                else if (args[startIndex].Equals("-a") || args[startIndex].Equals("--pull-all"))
                {
                    options.PullAllInformation = true;
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

                // Include seed info file
                if (args[startIndex].StartsWith("-l=") || args[startIndex].StartsWith("--load-seed="))
                {
                    string seedInfo = args[startIndex].Split('=')[1];
                    info = InfoTool.CreateFromFile(seedInfo);
                }
                else if (args[startIndex] == "-l" || args[startIndex] == "--load-seed")
                {
                    string seedInfo = args[startIndex + 1];
                    info = InfoTool.CreateFromFile(seedInfo);
                    startIndex++;
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

            return (options, info, parsedPath, startIndex);
        }

        /// <summary>
        /// Return a list of supported arguments and descriptions
        /// </summary>
        public static List<string> PrintSupportedArguments()
        {
            var supportedArguments = new List<string>
            {
                "-u, --use <program>            Dumping program output type [REQUIRED]",
                "-c, --credentials <user> <pw>  Redump username and password",
                "-a, --pull-all                 Pull all information from Redump (requires --credentials)",
                "-p, --path <drivepath>         Physical drive path for additional checks",
                "-s, --scan                     Enable copy protection scan (requires --path)",
                "-f, --protect-file             Output protection to separate file (requires --scan)",
                "-l, --load-seed <path>         Load a seed submission JSON for user information",
                "-j, --json                     Enable submission JSON output",
                "-z, --zip                      Enable log file compression"
            };

            return supportedArguments;
        }

#endregion

        #region Configuration

        /// <summary>
        /// Load the current set of options from the application configuration
        /// </summary>
        public static Options LoadFromConfig()
        {
            if (!File.Exists(ConfigurationPath))
            {
                _ = File.Create(ConfigurationPath);
                return new Options();
            }

            var serializer = JsonSerializer.Create();
            var reader = new StreamReader(ConfigurationPath);
#if NET48
            var settings = serializer.Deserialize(reader, typeof(Dictionary<string, string>)) as Dictionary<string, string>;
#else
            var settings = serializer.Deserialize(reader, typeof(Dictionary<string, string?>)) as Dictionary<string, string?>;
#endif
            return new Options(settings);
        }

        /// <summary>
        /// Save the current set of options to the application configuration
        /// </summary>
        public static void SaveToConfig(Options options)
        {
            var serializer = JsonSerializer.Create();
            var sw = new StreamWriter(ConfigurationPath) { AutoFlush = true };
            var writer = new JsonTextWriter(sw) { Formatting = Formatting.Indented };
            serializer.Serialize(writer, options.Settings, typeof(Dictionary<string, string>));
        }

        #endregion
    }
}
