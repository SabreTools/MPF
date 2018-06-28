using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DICUI.Data;
using DICUI.Utilities;

namespace DICUI
{
    /// <summary>
    /// Generic success/failure result object, with optional message
    /// </summary>
    public class Result
    {
        private bool success;
        public string Message { get; private set; }

        private Result(bool success, string message)
        {
            this.success = success;
            this.Message = message;
        }

        public static Result Success() => new Result(true, "");
        public static Result Success(string message) => new Result(true, message);
        public static Result Success(string message, params object[] args) => new Result(true, string.Format(message, args));

        public static Result Failure() => new Result(false, "");
        public static Result Failure(string message) => new Result(false, message);
        public static Result Failure(string message, params object[] args) => new Result(false, string.Format(message, args));

        public static implicit operator bool(Result result) => result.success;
    }

    /// <summary>
    /// Represents the state of all settings to be used during dumping
    /// </summary>
    public class DumpEnvironment
    {
        // Tool paths
        public string DICPath;
        public string SubdumpPath;

        // Output paths
        public string OutputDirectory;
        public string OutputFilename;

        // UI information
        public char DriveLetter;
        public KnownSystem? System;
        public MediaType? Type;
        public bool IsFloppy;
        public string DICParameters;

        // External process information
        public Process dicProcess;

        /// <summary>
        /// Checks if the configuration is valid
        /// </summary>
        /// <returns>True if the configuration is valid, false otherwise</returns>
        public bool IsConfigurationValid()
        {
            return !((string.IsNullOrWhiteSpace(DICParameters)
            || !Validators.ValidateParameters(DICParameters)
            || (IsFloppy ^ Type == MediaType.Floppy)));
        }

        /// <summary>
        /// Adjust the current environment if we are given custom parameters
        /// </summary>
        public void AdjustForCustomConfiguration()
        {
            // If we have a custom configuration, we need to extract the best possible information from it
            if (System == KnownSystem.Custom)
            {
                Validators.DetermineFlags(DICParameters, out Type, out System, out string letter, out string path);
                DriveLetter = (String.IsNullOrWhiteSpace(letter) ? new char() : letter[0]);
                OutputDirectory = Path.GetDirectoryName(path);
                OutputFilename = Path.GetFileName(path);
            }
        }

        /// <summary>
        /// Fix the output paths to remove characters that DiscImageCreator can't handle
        /// </summary>
        /// <remarks>
        /// TODO: Investigate why the `&` replacement is needed
        /// </remarks>
        public void FixOutputPaths()
        {
            // Only fix OutputDirectory if it's not blank or null
            if (!String.IsNullOrWhiteSpace(OutputDirectory))
                OutputDirectory = OutputDirectory.Replace('.', '_').Replace('&', '_');

            // Only fix OutputFilename if it's not blank or null
            if (!String.IsNullOrWhiteSpace(OutputFilename))
                OutputFilename = new StringBuilder(OutputFilename.Replace('&', '_')).Replace('.', '_', 0, OutputFilename.LastIndexOf('.')).ToString();
        }
    }

    /// <summary>
    /// Class containing dumping tasks
    /// </summary>
    public class Tasks
    {
        /// <summary>
        /// Validate the current DumpEnvironment
        /// </summary>
        /// <param name="env">DumpEnvirionment containing all required information</param>
        /// <returns>Result instance with the outcome</returns>
        public static Result ValidateEnvironment(DumpEnvironment env)
        {
            // Validate that everything is good
            if (!env.IsConfigurationValid())
                return Result.Failure("Error! Current configuration is not supported!");

            env.AdjustForCustomConfiguration();
            env.FixOutputPaths();

            // Validate that the required program exists
            if (!File.Exists(env.DICPath))
                return Result.Failure("Error! Could not find DiscImageCreator!");

            // If a complete dump already exists
            if (DumpInformation.FoundAllFiles(env.OutputDirectory, env.OutputFilename, env.Type))
            {
                MessageBoxResult result = MessageBox.Show("A complete dump already exists! Are you sure you want to overwrite?", "Overwrite?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.No || result == MessageBoxResult.Cancel || result == MessageBoxResult.None)
                {
                    return Result.Failure("Dumping aborted!");
                }
            }

            return Result.Success();
        }

        /// <summary>
        /// Run DiscImageCreator with the given DumpEnvironment
        /// </summary>
        /// <param name="env">DumpEnvirionment containing all required information</param>
        public static void ExecuteDiskImageCreator(DumpEnvironment env)
        {
            env.dicProcess = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = env.DICPath,
                    Arguments = env.DICParameters,
                },
            };
            env.dicProcess.Start();
            env.dicProcess.WaitForExit();
        }

        /// <summary>
        /// Execute subdump for a (potential) Sega Saturn dump
        /// </summary>
        /// <param name="env">DumpEnvirionment containing all required information</param>
        public static async void ExecuteSubdump(DumpEnvironment env)
        {
            await Task.Run(() =>
            {
                Process childProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = env.SubdumpPath,
                        Arguments = "-i " + env.DriveLetter + ": -f " + Path.Combine(env.OutputDirectory, Path.GetFileNameWithoutExtension(env.OutputFilename) + "_subdump.sub") + "-mode 6 -rereadnum 25 -fix 2",
                    },
                };
                childProcess.Start();
                childProcess.WaitForExit();
            });
        }

        /// <summary>
        /// Run any additional tools given a DumpEnvironment
        /// </summary>
        /// <param name="env">DumpEnvirionment containing all required information</param>
        /// <returns>Result instance with the outcome</returns>
        public static Result ExecuteAdditionalToolsAfterDIC(DumpEnvironment env)
        {
            // Special cases
            switch (env.System)
            {
                case KnownSystem.SegaSaturn:
                    if (!File.Exists(env.SubdumpPath))
                        return Result.Failure("Error! Could not find subdump!");

                    ExecuteSubdump(env);
                    break;
            }

            return Result.Success();
        }

        /// <summary>
        /// Verify that the current environment has a complete dump and create submission info is possible
        /// </summary>
        /// <param name="env">DumpEnvirionment containing all required information</param>
        /// <returns>Result instance with the outcome</returns>
        public static Result VerifyAndSaveDumpOutput(DumpEnvironment env)
        {
            // Check to make sure that the output had all the correct files
            if (!DumpInformation.FoundAllFiles(env.OutputDirectory, env.OutputFilename, env.Type))
                return Result.Failure("Error! Please check output directory as dump may be incomplete!");

            Dictionary<string, string> templateValues = DumpInformation.ExtractOutputInformation(env.OutputDirectory, env.OutputFilename, env.System, env.Type, env.DriveLetter);
            List<string> formattedValues = DumpInformation.FormatOutputData(templateValues, env.System, env.Type);
            bool success = DumpInformation.WriteOutputData(env.OutputDirectory, env.OutputFilename, formattedValues);

            return Result.Success();
        }

        /// <summary>
        /// Eject the disc using DIC
        /// </summary>
        public static async void EjectDisc(DumpEnvironment env)
        {
            // Validate that the required program exists
            if (!File.Exists(env.DICPath))
                return;

            CancelDumping(env);

            if (env.IsFloppy)
                return;

            Process childProcess;
            await Task.Run(() =>
            {
                childProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = env.DICPath,
                        Arguments = DICCommands.Eject + " " + env.DriveLetter,
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

        /// <summary>
        /// This executes the complete dump workflow on a DumpEnvironment
        /// </summary>
        public static async Task<Result> StartDumping(DumpEnvironment env)
        {
            Result result = Tasks.ValidateEnvironment(env);

            // is something is wrong in environment return
            if (!result)
                return result;

            // execute DIC
            await Task.Run(() => Tasks.ExecuteDiskImageCreator(env));

            // execute additional tools
            result = Tasks.ExecuteAdditionalToolsAfterDIC(env);

            // is something is wrong with additional tools report and return
            // TODO: don't return, just keep generating output from DIC
            /*if (!result.Item1)
            {
                lbl_Status.Content = result.Item2;
                btn_StartStop.Content = UIElements.StartDumping;
                return;
            }*/

            // verify dump output and save it
            result = Tasks.VerifyAndSaveDumpOutput(env);

            return result;
        }
    }
}
