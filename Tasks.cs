using DICUI.Data;
using DICUI.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DICUI
{
    using DumpResult = Tuple<bool, string>;

    struct DumpEnvironment
    {
        public string dicPath;
        public string subdumpPath;
        public string psxtPath;

        public string outputDirectory;
        public string outputFilename;

        public char driveLetter;

        public KnownSystem? system;
        public MediaType? type;
        public bool isFloppy;

        public string dicParameters;

        public Process dicProcess;

        public bool IsConfigurationValid()
        {
            return !((string.IsNullOrWhiteSpace(customParameters)
            || !Validators.ValidateParameters(customParameters)
            || (isFloppy ^ type == MediaType.Floppy)));
        }

        public void AdjustForCustomConfiguration()
        {
            // If we have a custom configuration, we need to extract the best possible information from it
            if (system == KnownSystem.Custom)
            {
                Validators.DetermineFlags(customParameters, out type, out system, out string letter, out string path);
                driveLetter = letter[0];
                outputDirectory = Path.GetDirectoryName(path);
                outputFilename = Path.GetFileName(path);
            }
        }

        public void FixOutputPaths()
        {
            // Replace characters where needed
            // TODO: Investigate why the `&` replacement is needed
            outputDirectory = outputDirectory.Replace('.', '_').Replace('&', '_');
            outputFilename = new StringBuilder(outputFilename.Replace('&', '_')).Replace('.', '_', 0, outputFilename.LastIndexOf('.')).ToString();
        }
    }

    class Tasks
    {
        public static DumpResult ValidateEnvironment(DumpEnvironment env)
        {
            // Validate that everything is good
            if (!env.IsConfigurationValid())
                return Tuple.Create(false, "Error! Current configuration is not supported!");

            env.AdjustForCustomConfiguration();
            env.FixOutputPaths();

            // Validate that the required program exits
            if (!File.Exists(env.dicPath))
                return Tuple.Create(false, "Error! Could not find DiscImageCreator!");

            // If a complete dump already exists
            if (DumpInformation.FoundAllFiles(env.outputDirectory, env.outputFilename, env.type))
            {
                MessageBoxResult result = MessageBox.Show("A complete dump already exists! Are you sure you want to overwrite?", "Overwrite?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.No || result == MessageBoxResult.Cancel || result == MessageBoxResult.None)
                {
                    return Tuple.Create(false, "Dumping aborted!");
                }
            }

            return Tuple.Create(true, "");
        }

        public static void ExecuteDiskImageCreator(DumpEnvironment env)
        {
            env.dicProcess = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = env.dicPath,
                    Arguments = env.dicParameters,
                },
            };
            env.dicProcess.Start();
            env.dicProcess.WaitForExit();
        }

        /// <summary>
        /// Execute subdump for a Sega Saturn dump
        /// </summary>
        public static async void ExecuteSubdump(DumpEnvironment env)
        {
            await Task.Run(() =>
            {
                Process childProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = env.subdumpPath,
                        Arguments = "-i " + env.driveLetter + ": -f " + Path.Combine(env.outputDirectory, Path.GetFileNameWithoutExtension(env.outputFilename) + "_subdump.sub") + "-mode 6 -rereadnum 25 -fix 2",
                    },
                };
                childProcess.Start();
                childProcess.WaitForExit();
            });
        }

        /// <summary>
        /// Execute psxt001z for a PSX dump
        /// </summary>
        public static async void ExecutePSXT001Z(DumpEnvironment env)
        {
            // Invoke the program with all 3 configurations
            // TODO: Use these outputs for PSX information
            await Task.Run(() =>
            {
                Process childProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = env.psxtPath,
                        Arguments = "\"" + DumpInformation.GetFirstTrack(env.outputDirectory, env.outputFilename) + "\" > " + "\"" + Path.Combine(env.outputDirectory, "psxt001z.txt"),
                    },
                };
                childProcess.Start();
                childProcess.WaitForExit();

                childProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = env.psxtPath,
                        Arguments = "--libcrypt \"" + Path.Combine(env.outputDirectory, Path.GetFileNameWithoutExtension(env.outputFilename) + ".sub") + "\" > \"" + Path.Combine(env.outputDirectory, "libcrypt.txt"),
                    },
                };
                childProcess.Start();
                childProcess.WaitForExit();

                childProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = env.psxtPath,
                        Arguments = "--libcryptdrvfast " + env.driveLetter + " > " + "\"" + Path.Combine(env.outputDirectory, "libcryptdrv.log"),
                    },
                };
                childProcess.Start();
                childProcess.WaitForExit();
            });
        }

        public static DumpResult ExecuteAdditionalToolsAfterDIC(DumpEnvironment env)
        {
            // Special cases
            switch (env.system)
            {
                case KnownSystem.SegaSaturn:
                    if (!File.Exists(env.subdumpPath))
                        return Tuple.Create(false, "Error! Could not find subdump!");

                    ExecuteSubdump(env);
                    break;
                case KnownSystem.SonyPlayStation:
                    if (!File.Exists(env.psxtPath))
                        return Tuple.Create(false, "Error! Could not find psxt001z!");

                    ExecutePSXT001Z(env);
                    break;
            }

            return Tuple.Create(true, "");
        }

        /// <summary>
        /// Eject the disc using DIC
        /// </summary>
        public static async void EjectDisc(DumpEnvironment env)
        {
            // Validate that the required program exists
            if (!File.Exists(env.dicPath))
               return;

            CancelDumping(env);

            if (env.isFloppy)
                return;

            Process childProcess;
            await Task.Run(() =>
            {
                childProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = env.dicPath,
                        Arguments = DICCommands.Eject + " " + env.driveLetter,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                    },
                };
                childProcess.Start();
                childProcess.WaitForExit();
            });
        }

        /// <summary>
        /// Cancel an in-progress dumping process
        /// </summary>
        public static void CancelDumping(DumpEnvironment env)
        {
            try
            {
                if (env.dicProcess != null && !env.dicProcess.HasExited)
                    env.dicProcess.Kill();
            }
            catch
            { }
        }

        public static DumpResult VerifyAndSaveDumpOutput(DumpEnvironment env)
        {
            // Check to make sure that the output had all the correct files
            if (!DumpInformation.FoundAllFiles(env.outputDirectory, env.outputFilename, env.type))
                return Tuple.Create(false, "Error! Please check output directory as dump may be incomplete!");

            Dictionary<string, string> templateValues = DumpInformation.ExtractOutputInformation(env.outputDirectory, env.outputFilename, env.system, env.type, env.driveLetter);
            List<string> formattedValues = DumpInformation.FormatOutputData(templateValues, env.system, env.type);
            bool success = DumpInformation.WriteOutputData(env.outputDirectory, env.outputFilename, formattedValues);

            return Tuple.Create(true, "");
        }
    }
}
