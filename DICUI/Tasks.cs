using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using DICUI.Data;
using DICUI.External;
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
    /// Class containing dumping tasks
    /// </summary>
    public class Tasks
    {
        /// <summary>
        /// Get disc speed using DIC
        /// </summary>
        /// <returns>Drive speed if possible, -1 on error</returns>
        public static async Task<int> GetDiscSpeed(DumpEnvironment env)
        {
            // Validate that the required program exists
            if (!File.Exists(env.DICPath))
                return -1;

            // Validate that the drive is set up
            if (env.Drive == null)
                return -1;

            // Validate we're not trying to get the speed for a floppy disk
            if (env.IsFloppy)
                return -1;

            // Get the drive speed directly
            //int speed = Validators.GetDriveSpeed((char)selected?.Key);
            //int speed = Validators.GetDriveSpeedEx((char)selected?.Key, _currentMediaType);

            // Get the drive speed from DIC, if possible
            Process childProcess;
            string output = await Task.Run(() =>
            {
                childProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = env.DICPath,
                        Arguments = DICCommands.DriveSpeed + " " + env.Drive.Letter,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                    },
                };
                childProcess.Start();
                childProcess.WaitForExit();
                return childProcess.StandardOutput.ReadToEnd();
            });

            // If we get that the firmware is out of date, tell the user
            if (output.Contains("[ERROR] This drive isn't latest firmware. Please update."))
            {
                MessageBox.Show($"DiscImageCreator has reported that drive {env.Drive.Letter} is not updated to the most recent firmware. Please update the firmware for your drive and try again.", "Outdated Firmware", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }
            // Otherwise, if we find the maximum read speed as reported
            else if (output.Contains("ReadSpeedMaximum:"))
            {
                int index = output.IndexOf("ReadSpeedMaximum:");
                string readspeed = Regex.Match(output.Substring(index), @"ReadSpeedMaximum: [0-9]+KB/sec \(([0-9]*)x\)").Groups[1].Value;
                if (!Int32.TryParse(readspeed, out int speed) || speed <= 0)
                {
                    return -1;
                }

                return speed;
            }

            return -1;
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

            // Validate we're not trying to eject a floppy disk
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
                        Arguments = DICCommands.Eject + " " + env.Drive.Letter,
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
            Result result = ValidateEnvironment(env);

            // is something is wrong in environment return
            if (!result)
                return result;

            // execute DIC
            await Task.Run(() => ExecuteDiskImageCreator(env));

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

        /// <summary>
        /// Validate the current DumpEnvironment
        /// </summary>
        /// <param name="env">DumpEnvirionment containing all required information</param>
        /// <returns>Result instance with the outcome</returns>
        private static Result ValidateEnvironment(DumpEnvironment env)
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
            if (env.FoundAllFiles())
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
        private static void ExecuteDiskImageCreator(DumpEnvironment env)
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
        /// Run any additional tools given a DumpEnvironment
        /// </summary>
        /// <param name="env">DumpEnvirionment containing all required information</param>
        /// <returns>Result instance with the outcome</returns>
        private static Result ExecuteAdditionalToolsAfterDIC(DumpEnvironment env)
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
        /// Execute subdump for a (potential) Sega Saturn dump
        /// </summary>
        /// <param name="env">DumpEnvirionment containing all required information</param>
        private static async void ExecuteSubdump(DumpEnvironment env)
        {
            await Task.Run(() =>
            {
                Process childProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = env.SubdumpPath,
                        Arguments = "-i " + env.Drive.Letter + ": -f " + Path.Combine(env.OutputDirectory, Path.GetFileNameWithoutExtension(env.OutputFilename) + "_subdump.sub") + "-mode 6 -rereadnum 25 -fix 2",
                    },
                };
                childProcess.Start();
                childProcess.WaitForExit();
            });
        }

        /// <summary>
        /// Run protection scan on a given dump environment
        /// </summary>
        /// <param name="env">DumpEnvirionment containing all required information</param>
        /// <returns>Copy protection detected in the envirionment, if any</returns>
        public static async Task<string> RunProtectionScan(string path)
        {
            var found = await Task.Run(() =>
            {
                return ProtectionFind.Scan(path);
            });

            if (found == null)
                return "None found";

            return string.Join("\n", found.Select(kvp => kvp.Key + ": " + kvp.Value).ToArray());
        }

        /// <summary>
        /// Verify that the current environment has a complete dump and create submission info is possible
        /// </summary>
        /// <param name="env">DumpEnvirionment containing all required information</param>
        /// <returns>Result instance with the outcome</returns>
        private static Result VerifyAndSaveDumpOutput(DumpEnvironment env)
        {
            // Check to make sure that the output had all the correct files
            if (!env.FoundAllFiles())
                return Result.Failure("Error! Please check output directory as dump may be incomplete!");

            Dictionary<string, string> templateValues = env.ExtractOutputInformation();
            List<string> formattedValues = env.FormatOutputData(templateValues);
            bool success = env.WriteOutputData(formattedValues);

            return Result.Success();
        }
    }
}
