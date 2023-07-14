using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using BurnOutSharp;
using MPF.Core.Data;
using MPF.Core.Utilities;
using MPF.Modules;
using RedumpLib.Data;

namespace MPF.Library
{
    /// <summary>
    /// Represents the state of all settings to be used during dumping
    /// </summary>
    public class DumpEnvironment
    {
        #region Output paths

        /// <summary>
        /// Base output file path to write files to
        /// </summary>
        public string OutputPath { get; private set; }

        #endregion

        #region UI information

        /// <summary>
        /// Drive object representing the current drive
        /// </summary>
        public Drive Drive { get; private set; }

        /// <summary>
        /// Currently selected system
        /// </summary>
        public RedumpSystem? System { get; private set; }

        /// <summary>
        /// Currently selected media type
        /// </summary>
        public MediaType? Type { get; private set; }

        /// <summary>
        /// Currently selected dumping program
        /// </summary>
        public InternalProgram InternalProgram { get; private set; }

        /// <summary>
        /// Options object representing user-defined options
        /// </summary>
        public Core.Data.Options Options { get; private set; }

        /// <summary>
        /// Parameters object representing what to send to the internal program
        /// </summary>
        public BaseParameters Parameters { get; private set; }

        #endregion
        
        #region Event Handlers

        /// <summary>
        /// Generic way of reporting a message
        /// </summary>
        public EventHandler<string> ReportStatus;

        /// <summary>
        /// Queue of items that need to be logged
        /// </summary>
        private ProcessingQueue<string> outputQueue;

        /// <summary>
        /// Event handler for data returned from a process
        /// </summary>
        private void OutputToLog(object proc, string args) => outputQueue.Enqueue(args);

        /// <summary>
        /// Process the outputs in the queue
        /// </summary>
        private void ProcessOutputs(string nextOutput) => ReportStatus.Invoke(this, nextOutput);

        #endregion

        /// <summary>
        /// Constructor for a full DumpEnvironment object from user information
        /// </summary>
        /// <param name="options"></param>
        /// <param name="outputPath"></param>
        /// <param name="drive"></param>
        /// <param name="system"></param>
        /// <param name="type"></param>
        /// <param name="internalProgram"></param>
        /// <param name="parameters"></param>
        public DumpEnvironment(Core.Data.Options options,
            string outputPath,
            Drive drive,
            RedumpSystem? system,
            MediaType? type,
            InternalProgram? internalProgram,
            string parameters)
        {
            // Set options object
            this.Options = options;

            // Output paths
            this.OutputPath = InfoTool.NormalizeOutputPaths(outputPath);

            // UI information
            this.Drive = drive;
            this.System = system ?? options.DefaultSystem;
            this.Type = type ?? MediaType.NONE;
            this.InternalProgram = internalProgram ?? options.InternalProgram;
            
            // Dumping program
            SetParameters(parameters);
        }

        #region Public Functionality

        /// <summary>
        /// Adjust output paths if we're using DiscImageCreator
        /// </summary>
        public void AdjustPathsForDiscImageCreator()
        {
            // Only DiscImageCreator has issues with paths
            if (this.Parameters.InternalProgram != InternalProgram.DiscImageCreator)
                return;

            try
            {
                // Normalize the output path
                string outputPath = InfoTool.NormalizeOutputPaths(this.OutputPath);

                // Replace all instances in the output directory
                string outputDirectory = Path.GetDirectoryName(outputPath);
                outputDirectory = outputDirectory.Replace(".", "_");

                // Replace all instances in the output filename
                string outputFilename = Path.GetFileNameWithoutExtension(outputPath);
                outputFilename = outputFilename.Replace(".", "_");

                // Get the extension for recreating the path
                string outputExtension = Path.GetExtension(outputPath).TrimStart('.');

                // Rebuild the output path
                if (!string.IsNullOrWhiteSpace(outputExtension))
                    this.OutputPath = Path.Combine(outputDirectory, $"{outputFilename}.{outputExtension}");
                else
                    this.OutputPath = Path.Combine(outputDirectory, outputFilename);

                // Assign the path to the filename as well for dumping
                ((Modules.DiscImageCreator.Parameters)this.Parameters).Filename = this.OutputPath;
            }
            catch { }
        }

        /// <summary>
        /// Set the parameters object based on the internal program and parameters string
        /// </summary>
        /// <param name="parameters">String representation of the parameters</param>
        public void SetParameters(string parameters)
        {
            switch (this.InternalProgram)
            {
                // Dumping support
                case InternalProgram.Aaru:
                    this.Parameters = new Modules.Aaru.Parameters(parameters) { ExecutablePath = Options.AaruPath };
                    break;

                case InternalProgram.DD:
                    this.Parameters = new Modules.DD.Parameters(parameters) { ExecutablePath = Options.DDPath };
                    break;

                case InternalProgram.DiscImageCreator:
                    this.Parameters = new Modules.DiscImageCreator.Parameters(parameters) { ExecutablePath = Options.DiscImageCreatorPath };
                    break;

                case InternalProgram.Redumper:
                    this.Parameters = new Modules.Redumper.Parameters(parameters) { ExecutablePath = Options.RedumperPath };
                    break;

                // Verification support only
                case InternalProgram.CleanRip:
                    this.Parameters = new Modules.CleanRip.Parameters(parameters) { ExecutablePath = null };
                    break;

                case InternalProgram.DCDumper:
                    this.Parameters = null; // TODO: Create correct parameter type when supported
                    break;

                case InternalProgram.UmdImageCreator:
                    this.Parameters = new Modules.UmdImageCreator.Parameters(parameters) { ExecutablePath = null };
                    break;

                // This should never happen, but it needs a fallback
                default:
                    this.Parameters = new Modules.DiscImageCreator.Parameters(parameters) { ExecutablePath = Options.DiscImageCreatorPath };
                    break;
            }

            // Set system and type
            this.Parameters.System = this.System;
            this.Parameters.Type = this.Type;
        }

        /// <summary>
        /// Get the full parameter string for either DiscImageCreator or Aaru
        /// </summary>
        /// <param name="driveSpeed">Nullable int representing the drive speed</param>
        /// <returns>String representing the params, null on error</returns>
        public string GetFullParameters(int? driveSpeed)
        {
            // Populate with the correct params for inputs (if we're not on the default option)
            if (System != null && Type != MediaType.NONE)
            {
                // If drive letter is invalid, skip this
                if (Drive == null)
                    return null;

                // Set the proper parameters
                switch (this.InternalProgram)
                {
                    case InternalProgram.Aaru:
                        Parameters = new Modules.Aaru.Parameters(System, Type, Drive.Letter, this.OutputPath, driveSpeed, Options);
                        break;

                    case InternalProgram.DD:
                        Parameters = new Modules.DD.Parameters(System, Type, Drive.Letter, this.OutputPath, driveSpeed, Options);
                        break;

                    case InternalProgram.DiscImageCreator:
                        Parameters = new Modules.DiscImageCreator.Parameters(System, Type, Drive.Letter, this.OutputPath, driveSpeed, Options);
                        break;

                    case InternalProgram.Redumper:
                        Parameters = new Modules.Redumper.Parameters(System, Type, Drive.Letter, this.OutputPath, driveSpeed, Options);
                        break;

                    // This should never happen, but it needs a fallback
                    default:
                        Parameters = new Modules.DiscImageCreator.Parameters(System, Type, Drive.Letter, this.OutputPath, driveSpeed, Options);
                        break;
                }

                // Generate and return the param string
                return Parameters.GenerateParameters();
            }

            return null;
        }

        #endregion

        #region Dumping

        /// <summary>
        /// Cancel an in-progress dumping process
        /// </summary>
        public void CancelDumping() => Parameters.KillInternalProgram();

        /// <summary>
        /// Eject the disc using DiscImageCreator
        /// </summary>
        public async Task<string> EjectDisc() =>
            await RunStandaloneDiscImageCreatorCommand(Modules.DiscImageCreator.CommandStrings.Eject);

        /// <summary>
        /// Reset the current drive using DiscImageCreator
        /// </summary>
        public async Task<string> ResetDrive() =>
            await RunStandaloneDiscImageCreatorCommand(Modules.DiscImageCreator.CommandStrings.Reset);

        /// <summary>
        /// Execute the initial invocation of the dumping programs
        /// </summary>
        /// <param name="progress">Optional result progress callback</param>
        public async Task<Result> Run(IProgress<Result> progress = null)
        {
            // Check that we have the basics for dumping
            Result result = IsValidForDump();
            if (!result)
                return result;

            // Invoke output processing, if needed
            if (!Options.ToolsInSeparateWindow)
            {
                outputQueue = new ProcessingQueue<string>(ProcessOutputs);
                Parameters.ReportStatus += OutputToLog;
            }

            // Execute internal tool
            progress?.Report(Result.Success($"Executing {this.InternalProgram}... {(Options.ToolsInSeparateWindow ? "please wait!" : "see log for output!")}"));
            Directory.CreateDirectory(Path.GetDirectoryName(this.OutputPath));
            await Task.Run(() => Parameters.ExecuteInternalProgram(Options.ToolsInSeparateWindow));
            progress?.Report(Result.Success($"{this.InternalProgram} has finished!"));

            // Execute additional tools
            progress?.Report(Result.Success("Running any additional tools... see log for output!"));
            result = await Task.Run(() => ExecuteAdditionalTools());
            progress?.Report(result);

            // Remove event handler if needed
            if (!Options.ToolsInSeparateWindow)
            {
                outputQueue.Dispose();
                Parameters.ReportStatus -= OutputToLog;
            }

            return result;
        }

        /// <summary>
        /// Verify that the current environment has a complete dump and create submission info is possible
        /// </summary>
        /// <param name="resultProgress">Optional result progress callback</param>
        /// <param name="protectionProgress">Optional protection progress callback</param>
        /// <param name="processUserInfo">Optional user prompt to deal with submission information</param>
        /// <returns>Result instance with the outcome</returns>
        public async Task<Result> VerifyAndSaveDumpOutput(
            IProgress<Result> resultProgress = null,
            IProgress<ProtectionProgress> protectionProgress = null,
            Func<SubmissionInfo, (bool?, SubmissionInfo)> processUserInfo = null)
        {
            resultProgress?.Report(Result.Success("Gathering submission information... please wait!"));

            // Get the output directory and filename separately
            string outputDirectory = Path.GetDirectoryName(this.OutputPath);
            string outputFilename = Path.GetFileName(this.OutputPath);

            // Check to make sure that the output had all the correct files
            (bool foundFiles, List<string> missingFiles) = InfoTool.FoundAllFiles(outputDirectory, outputFilename, this.Parameters, false);
            if (!foundFiles)
            {
                resultProgress?.Report(Result.Failure($"There were files missing from the output:\n{string.Join("\n", missingFiles)}"));
                return Result.Failure("Error! Please check output directory as dump may be incomplete!");
            }

            // Extract the information from the output files
            resultProgress?.Report(Result.Success("Extracting output information from output files..."));
            SubmissionInfo submissionInfo = await InfoTool.ExtractOutputInformation(
                this.OutputPath,
                this.Drive,
                this.System,
                this.Type,
                this.Options,
                this.Parameters,
                resultProgress,
                protectionProgress);
            resultProgress?.Report(Result.Success("Extracting information complete!"));

            // Eject the disc automatically if configured to
            if (Options.EjectAfterDump == true)
            {
                resultProgress?.Report(Result.Success($"Ejecting disc in drive {Drive.Letter}"));
                await EjectDisc();
            }

            // Reset the drive automatically if configured to
            if (this.InternalProgram == InternalProgram.DiscImageCreator && Options.DICResetDriveAfterDump)
            {
                resultProgress?.Report(Result.Success($"Resetting drive {Drive.Letter}"));
                await ResetDrive();
            }

            // Get user-modifiable information if confugured to
            if (Options.PromptForDiscInformation && processUserInfo != null)
            {
                resultProgress?.Report(Result.Success("Waiting for additional disc information..."));

                bool? filledInfo;
                (filledInfo, submissionInfo) = processUserInfo(submissionInfo);

                if (filledInfo == true)
                    resultProgress?.Report(Result.Success("Additional disc information added!"));
                else
                    resultProgress?.Report(Result.Success("Disc information skipped!"));
            }

            // Process special fields for site codes
            resultProgress?.Report(Result.Success("Processing site codes..."));
            InfoTool.ProcessSpecialFields(submissionInfo);
            resultProgress?.Report(Result.Success("Processing complete!"));

            // Format the information for the text output
            resultProgress?.Report(Result.Success("Formatting information..."));
            (List<string> formattedValues, string formatResult) = InfoTool.FormatOutputData(submissionInfo, this.Options);
            if (formattedValues == null)
                resultProgress?.Report(Result.Success(formatResult));
            else
                resultProgress?.Report(Result.Failure(formatResult));

            // Write the text output
            resultProgress?.Report(Result.Success("Writing information to !submissionInfo.txt..."));
            (bool txtSuccess, string txtResult) = InfoTool.WriteOutputData(outputDirectory, formattedValues);
            if (txtSuccess)
                resultProgress?.Report(Result.Success(txtResult));
            else
                resultProgress?.Report(Result.Failure(txtResult));

            // Write the copy protection output
            if (Options.ScanForProtection && Options.OutputSeparateProtectionFile)
            {
                resultProgress?.Report(Result.Success("Writing protection to !protectionInfo.txt..."));
                bool scanSuccess = InfoTool.WriteProtectionData(outputDirectory, submissionInfo);
                if (scanSuccess)
                    resultProgress?.Report(Result.Success("Writing complete!"));
                else
                    resultProgress?.Report(Result.Failure("Writing could not complete!"));
            }

            // Write the JSON output, if required
            if (Options.OutputSubmissionJSON)
            {
                resultProgress?.Report(Result.Success($"Writing information to !submissionInfo.json{(Options.IncludeArtifacts ? ".gz" : string.Empty)}..."));
                bool jsonSuccess = InfoTool.WriteOutputData(outputDirectory, submissionInfo, Options.IncludeArtifacts);
                if (jsonSuccess)
                    resultProgress?.Report(Result.Success("Writing complete!"));
                else
                    resultProgress?.Report(Result.Failure("Writing could not complete!"));
            }

            // Compress the logs, if required
            if (Options.CompressLogFiles)
            {
                resultProgress?.Report(Result.Success("Compressing log files..."));
                (bool compressSuccess, string compressResult) = InfoTool.CompressLogFiles(outputDirectory, outputFilename, this.Parameters);
                if (compressSuccess)
                    resultProgress?.Report(Result.Success(compressResult));
                else
                    resultProgress?.Report(Result.Failure(compressResult));
            }

            resultProgress?.Report(Result.Success("Submission information process complete!"));
            return Result.Success();
        }

        /// <summary>
        /// Checks if the parameters are valid
        /// </summary>
        /// <returns>True if the configuration is valid, false otherwise</returns>
        internal bool ParametersValid()
        {
            bool parametersValid = Parameters.IsValid();
            bool floppyValid = !(Drive.InternalDriveType == InternalDriveType.Floppy ^ Type == MediaType.FloppyDisk);

            // TODO: HardDisk being in the Removable category is a hack, fix this later
            bool removableDiskValid = !((Drive.InternalDriveType == InternalDriveType.Removable || Drive.InternalDriveType == InternalDriveType.HardDisk)
                ^ (Type == MediaType.CompactFlash || Type == MediaType.SDCard || Type == MediaType.FlashDrive || Type == MediaType.HardDisk));

            return parametersValid && floppyValid && removableDiskValid;
        }

        /// <summary>
        /// Run any additional tools given a DumpEnvironment
        /// </summary>
        /// <returns>Result instance with the outcome</returns>
        private Result ExecuteAdditionalTools() => Result.Success("No external tools needed!");

        /// <summary>
        /// Run internal program async with an input set of parameters
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>Standard output from commandline window</returns>
        private async Task<string> ExecuteInternalProgram(BaseParameters parameters)
        {
            Process childProcess;
            string output = await Task.Run(() =>
            {
                childProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = parameters.ExecutablePath,
                        Arguments = parameters.GenerateParameters(),
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                    },
                };
                childProcess.Start();
                childProcess.WaitForExit(1000);

                // Just in case, we want to push a button 5 times to clear any errors
                for (int i = 0; i < 5; i++)
                    childProcess.StandardInput.WriteLine("Y");

                string stdout = childProcess.StandardOutput.ReadToEnd();
                childProcess.Dispose();
                return stdout;
            });

            return output;
        }

        /// <summary>
        /// Validate the current environment is ready for a dump
        /// </summary>
        /// <returns>Result instance with the outcome</returns>
        private Result IsValidForDump()
        {
            // Validate that everything is good
            if (!ParametersValid())
                return Result.Failure("Error! Current configuration is not supported!");

            // Fix the output paths, just in case
            this.OutputPath = InfoTool.NormalizeOutputPaths(this.OutputPath);

            // Validate that the output path isn't on the dumping drive
            if (this.OutputPath[0] == Drive.Letter)
                return Result.Failure("Error! Cannot output to same drive that is being dumped!");

            // Validate that the required program exists
            if (!File.Exists(Parameters.ExecutablePath))
                return Result.Failure($"Error! {Parameters.ExecutablePath} does not exist!");

            // Validate that the dumping drive doesn't contain the executable
            string fullExecutablePath = Path.GetFullPath(Parameters.ExecutablePath);
            if (fullExecutablePath[0] == Drive.Letter)
                return Result.Failure("Error! Cannot dump same drive that executable resides on!");

            // Validate that the current configuration is supported
            return Tools.GetSupportStatus(System, Type);
        }

        /// <summary>
        /// Validate that DIscImageCreator is able to be found
        /// </summary>
        /// <returns>True if DiscImageCreator is found properly, false otherwise</returns>
        private bool RequiredProgramsExist()
        {
            // Validate that the path is configured
            if (string.IsNullOrWhiteSpace(Options.DiscImageCreatorPath))
                return false;

            // Validate that the required program exists
            if (!File.Exists(Options.DiscImageCreatorPath))
                return false;

            return true;
        }

        /// <summary>
        /// Run a standalone DiscImageCreator command
        /// </summary>
        /// <param name="command">Command string to run</param>
        /// <returns>The output of the command on success, null on error</returns>
        private async Task<string> RunStandaloneDiscImageCreatorCommand(string command)
        {
            // Validate that DiscImageCreator is all set
            if (!RequiredProgramsExist())
                return null;

            // Validate we're not trying to eject a non-optical
            if (Drive.InternalDriveType != InternalDriveType.Optical)
                return null;

            CancelDumping();

            var parameters = new Modules.DiscImageCreator.Parameters(string.Empty)
            {
                BaseCommand = command,
                DriveLetter = Drive.Letter.ToString(),
                ExecutablePath = Options.DiscImageCreatorPath,
            };

            return await ExecuteInternalProgram(parameters);
        }

        #endregion
    }
}
