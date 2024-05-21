using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinaryObjectScanner;
using MPF.Core.Data;
using MPF.Core.ExecutionContexts;
using MPF.Core.Processors;
using MPF.Core.Utilities;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;

namespace MPF.Core
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
        public Drive? Drive { get; private set; }

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
        /// ExecutionContext object representing how to invoke the internal program
        /// </summary>
        public BaseExecutionContext? ExecutionContext { get; private set; }

        /// <summary>
        /// Processor object representing how to process the outputs
        /// </summary>
        public BaseProcessor? Processor { get; private set; }

        /// <summary>
        /// Options object representing user-defined options
        /// </summary>
        private readonly Data.Options _options;

        #endregion

        #region Event Handlers

        /// <summary>
        /// Generic way of reporting a message
        /// </summary>
#if NET20 || NET35 || NET40
        public EventHandler<BaseExecutionContext.StringEventArgs>? ReportStatus;
#else
        public EventHandler<string>? ReportStatus;
#endif

        /// <summary>
        /// Queue of items that need to be logged
        /// </summary>
        private ProcessingQueue<string>? outputQueue;

        /// <summary>
        /// Event handler for data returned from a process
        /// </summary>
#if NET20 || NET35 || NET40
        private void OutputToLog(object? proc, BaseExecutionContext.StringEventArgs args) => outputQueue?.Enqueue(args.Value);
#else
        private void OutputToLog(object? proc, string args) => outputQueue?.Enqueue(args);
#endif

        /// <summary>
        /// Process the outputs in the queue
        /// </summary>
#if NET20 || NET35 || NET40
        private void ProcessOutputs(string nextOutput) => ReportStatus?.Invoke(this, new BaseExecutionContext.StringEventArgs { Value = nextOutput });
#else
        private void ProcessOutputs(string nextOutput) => ReportStatus?.Invoke(this, nextOutput);
#endif

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
        public DumpEnvironment(Data.Options options,
            string outputPath,
            Drive? drive,
            RedumpSystem? system,
            MediaType? type,
            InternalProgram? internalProgram,
            string? parameters)
        {
            // Set options object
            _options = options;

            // Output paths
            OutputPath = InfoTool.NormalizeOutputPaths(outputPath, false);

            // UI information
            Drive = drive;
            System = system ?? options.DefaultSystem;
            Type = type ?? MediaType.NONE;
            InternalProgram = internalProgram ?? options.InternalProgram;

            // Dumping program
            SetExecutionContext(parameters);
            SetProcessor();
        }

        #region Internal Program Management

        /// <summary>
        /// Set the parameters object based on the internal program and parameters string
        /// </summary>
        /// <param name="parameters">String representation of the parameters</param>
        public void SetExecutionContext(string? parameters)
        {
            ExecutionContext = InternalProgram switch
            {
                InternalProgram.Aaru => new ExecutionContexts.Aaru.ExecutionContext(parameters) { ExecutablePath = _options.AaruPath },
                InternalProgram.DiscImageCreator => new ExecutionContexts.DiscImageCreator.ExecutionContext(parameters) { ExecutablePath = _options.DiscImageCreatorPath },
                InternalProgram.Redumper => new ExecutionContexts.Redumper.ExecutionContext(parameters) { ExecutablePath = _options.RedumperPath },

                // If no dumping program found, set to null
                InternalProgram.NONE => null,
                _ => null,
            };

            // Set system and type
            if (ExecutionContext != null)
            {
                ExecutionContext.System = System;
                ExecutionContext.Type = Type;
            }
        }

        /// <summary>
        /// Set the processor object based on the internal program
        /// </summary>
        public void SetProcessor()
        {
            Processor = InternalProgram switch
            {
                InternalProgram.Aaru => new Processors.Aaru(System, Type),
                InternalProgram.CleanRip => new CleanRip(System, Type),
                InternalProgram.DiscImageCreator => new DiscImageCreator(System, Type),
                InternalProgram.PS3CFW => new PS3CFW(System, Type),
                InternalProgram.Redumper => new Redumper(System, Type),
                InternalProgram.UmdImageCreator => new UmdImageCreator(System, Type),
                InternalProgram.XboxBackupCreator => new XboxBackupCreator(System, Type),

                // If no dumping program found, set to null
                InternalProgram.NONE => null,
                _ => null,
            };
        }

        /// <summary>
        /// Get the full parameter string for either DiscImageCreator or Aaru
        /// </summary>
        /// <param name="driveSpeed">Nullable int representing the drive speed</param>
        /// <returns>String representing the params, null on error</returns>
        public string? GetFullParameters(int? driveSpeed)
        {
            // Populate with the correct params for inputs (if we're not on the default option)
            if (System != null && Type != MediaType.NONE)
            {
                // If drive letter is invalid, skip this
                if (Drive == null)
                    return null;

                // Set the proper parameters
                ExecutionContext = InternalProgram switch
                {
                    InternalProgram.Aaru => new ExecutionContexts.Aaru.ExecutionContext(System, Type, Drive.Name, OutputPath, driveSpeed, _options),
                    InternalProgram.DiscImageCreator => new ExecutionContexts.DiscImageCreator.ExecutionContext(System, Type, Drive.Name, OutputPath, driveSpeed, _options),
                    InternalProgram.Redumper => new ExecutionContexts.Redumper.ExecutionContext(System, Type, Drive.Name, OutputPath, driveSpeed, _options),

                    // If no dumping program found, set to null
                    InternalProgram.NONE => null,
                    _ => null,
                };

                // Generate and return the param string
                return ExecutionContext?.GenerateParameters();
            }

            return null;
        }

        #endregion

        #region Dumping

        /// <summary>
        /// Cancel an in-progress dumping process
        /// </summary>
        public void CancelDumping() => ExecutionContext?.KillInternalProgram();

        /// <summary>
        /// Eject the disc using DiscImageCreator
        /// </summary>
        public async Task<string?> EjectDisc() =>
            await RunStandaloneDiscImageCreatorCommand(ExecutionContexts.DiscImageCreator.CommandStrings.Eject);

        /// <summary>
        /// Reset the current drive using DiscImageCreator
        /// </summary>
        public async Task<string?> ResetDrive() =>
            await RunStandaloneDiscImageCreatorCommand(ExecutionContexts.DiscImageCreator.CommandStrings.Reset);

        /// <summary>
        /// Execute the initial invocation of the dumping programs
        /// </summary>
        /// <param name="progress">Optional result progress callback</param>
#if NET40
        public Result Run(IProgress<Result>? progress = null)
#else
        public async Task<Result> Run(IProgress<Result>? progress = null)
#endif
        {
            // If we don't have parameters
            if (ExecutionContext == null)
                return Result.Failure("Error! Current configuration is not supported!");

            // Check that we have the basics for dumping
            Result result = IsValidForDump();
            if (!result)
                return result;

            // Invoke output processing, if needed
            if (!_options.ToolsInSeparateWindow)
            {
                outputQueue = new ProcessingQueue<string>(ProcessOutputs);
                if (ExecutionContext.ReportStatus != null)
                    ExecutionContext.ReportStatus += OutputToLog;
            }

            // Execute internal tool
            progress?.Report(Result.Success($"Executing {InternalProgram}... {(_options.ToolsInSeparateWindow ? "please wait!" : "see log for output!")}"));

            var directoryName = Path.GetDirectoryName(OutputPath);
            if (!string.IsNullOrEmpty(directoryName))
                Directory.CreateDirectory(directoryName);

#if NET40
            var executeTask = Task.Factory.StartNew(() => ExecutionContext.ExecuteInternalProgram(_options.ToolsInSeparateWindow));
            executeTask.Wait();
#else
            await Task.Run(() => ExecutionContext.ExecuteInternalProgram(_options.ToolsInSeparateWindow));
#endif
            progress?.Report(Result.Success($"{InternalProgram} has finished!"));

            // Remove event handler if needed
            if (!_options.ToolsInSeparateWindow)
            {
                outputQueue?.Dispose();
                ExecutionContext.ReportStatus -= OutputToLog;
            }

            return result;
        }

        /// <summary>
        /// Verify that the current environment has a complete dump and create submission info is possible
        /// </summary>
        /// <param name="resultProgress">Optional result progress callback</param>
        /// <param name="protectionProgress">Optional protection progress callback</param>
        /// <param name="processUserInfo">Optional user prompt to deal with submission information</param>
        /// <param name="seedInfo">A seed SubmissionInfo object that contains user data</param>
        /// <returns>Result instance with the outcome</returns>
        public async Task<Result> VerifyAndSaveDumpOutput(
            IProgress<Result>? resultProgress = null,
            IProgress<ProtectionProgress>? protectionProgress = null,
            Func<SubmissionInfo?, (bool?, SubmissionInfo?)>? processUserInfo = null,
            SubmissionInfo? seedInfo = null)
        {
            if (ExecutionContext == null && Processor == null)
                return Result.Failure("Error! Current configuration is not supported!");

            resultProgress?.Report(Result.Success("Gathering submission information... please wait!"));

            // Get the output directory and filename separately
            var outputDirectory = Path.GetDirectoryName(OutputPath);
            var outputFilename = Path.GetFileName(OutputPath);

            // Check to make sure that the output had all the correct files
            (bool foundFiles, List<string> missingFiles) = Processor.FoundAllFiles(outputDirectory, outputFilename, false);
            if (!foundFiles)
            {
                resultProgress?.Report(Result.Failure($"There were files missing from the output:\n{string.Join("\n", [.. missingFiles])}"));
                return Result.Failure("Error! Please check output directory as dump may be incomplete!");
            }

            // Extract the information from the output files
            resultProgress?.Report(Result.Success("Extracting output information from output files..."));
            var submissionInfo = await SubmissionInfoTool.ExtractOutputInformation(
                OutputPath,
                Drive,
                System,
                Type,
                _options,
                ExecutionContext,
                Processor,
                resultProgress,
                protectionProgress);
            resultProgress?.Report(Result.Success("Extracting information complete!"));

            // Inject seed submission info data, if necessary
            if (seedInfo != null)
            {
                resultProgress?.Report(Result.Success("Injecting user-supplied information..."));
                Builder.InjectSubmissionInformation(submissionInfo, seedInfo);
                resultProgress?.Report(Result.Success("Information injection complete!"));
            }

            // Eject the disc automatically if configured to
            if (_options.EjectAfterDump == true)
            {
                resultProgress?.Report(Result.Success($"Ejecting disc in drive {Drive?.Name}"));
                await EjectDisc();
            }

            // Reset the drive automatically if configured to
            if (InternalProgram == InternalProgram.DiscImageCreator && _options.DICResetDriveAfterDump)
            {
                resultProgress?.Report(Result.Success($"Resetting drive {Drive?.Name}"));
                await ResetDrive();
            }

            // Get user-modifiable information if confugured to
            if (_options.PromptForDiscInformation && processUserInfo != null)
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
            Formatter.ProcessSpecialFields(submissionInfo);
            resultProgress?.Report(Result.Success("Processing complete!"));

            // Format the information for the text output
            resultProgress?.Report(Result.Success("Formatting information..."));
            (var formattedValues, var formatResult) = Formatter.FormatOutputData(submissionInfo, _options.EnableRedumpCompatibility);
            if (formattedValues == null)
                resultProgress?.Report(Result.Failure(formatResult));
            else
                resultProgress?.Report(Result.Success(formatResult));

            // Get the filename suffix for auto-generated files
            var filenameSuffix = _options.AddFilenameSuffix ? Path.GetFileNameWithoutExtension(outputFilename) : null;

            // Write the text output
            resultProgress?.Report(Result.Success("Writing information to !submissionInfo.txt..."));
            (bool txtSuccess, string txtResult) = InfoTool.WriteOutputData(outputDirectory, filenameSuffix, formattedValues);
            if (txtSuccess)
                resultProgress?.Report(Result.Success(txtResult));
            else
                resultProgress?.Report(Result.Failure(txtResult));

            // Write the copy protection output
            if (submissionInfo?.CopyProtection?.FullProtections != null && submissionInfo.CopyProtection.FullProtections.Any())
            {
                if (_options.ScanForProtection && _options.OutputSeparateProtectionFile)
                {
                    resultProgress?.Report(Result.Success("Writing protection to !protectionInfo.txt..."));
                    bool scanSuccess = InfoTool.WriteProtectionData(outputDirectory, filenameSuffix, submissionInfo, _options.HideDriveLetters);
                    if (scanSuccess)
                        resultProgress?.Report(Result.Success("Writing complete!"));
                    else
                        resultProgress?.Report(Result.Failure("Writing could not complete!"));
                }
            }

            // Write the JSON output, if required
            if (_options.OutputSubmissionJSON)
            {
                resultProgress?.Report(Result.Success($"Writing information to !submissionInfo.json{(_options.IncludeArtifacts ? ".gz" : string.Empty)}..."));
                bool jsonSuccess = InfoTool.WriteOutputData(outputDirectory, filenameSuffix, submissionInfo, _options.IncludeArtifacts);
                if (jsonSuccess)
                    resultProgress?.Report(Result.Success("Writing complete!"));
                else
                    resultProgress?.Report(Result.Failure("Writing could not complete!"));
            }

            // Compress the logs, if required
            if (_options.CompressLogFiles)
            {
                resultProgress?.Report(Result.Success("Compressing log files..."));
                (bool compressSuccess, string compressResult) = InfoTool.CompressLogFiles(outputDirectory, filenameSuffix, outputFilename, Processor);
                if (compressSuccess)
                    resultProgress?.Report(Result.Success(compressResult));
                else
                    resultProgress?.Report(Result.Failure(compressResult));
            }

            // Delete unnecessary files, if required
            if (_options.DeleteUnnecessaryFiles)
            {
                resultProgress?.Report(Result.Success("Deleting unnecessary files..."));
                (bool deleteSuccess, string deleteResult) = InfoTool.DeleteUnnecessaryFiles(outputDirectory, outputFilename, Processor);
                if (deleteSuccess)
                    resultProgress?.Report(Result.Success(deleteResult));
                else
                    resultProgress?.Report(Result.Failure(deleteResult));
            }

            // Create PS3 IRD, if required
            if (_options.CreateIRDAfterDumping && System == RedumpSystem.SonyPlayStation3 && Type == MediaType.BluRay)
            {
                resultProgress?.Report(Result.Success("Creating IRD... please wait!"));
                (bool deleteSuccess, string deleteResult) = await InfoTool.WriteIRD(OutputPath, submissionInfo?.Extras?.DiscKey, submissionInfo?.Extras?.DiscID, submissionInfo?.Extras?.PIC, submissionInfo?.SizeAndChecksums?.Layerbreak, submissionInfo?.SizeAndChecksums?.CRC32);
                if (deleteSuccess)
                    resultProgress?.Report(Result.Success(deleteResult));
                else
                    resultProgress?.Report(Result.Failure(deleteResult));
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
            // Missing drive means it can never be valid
            if (Drive == null)
                return false;

            bool parametersValid = ExecutionContext?.IsValid() ?? false;
            bool floppyValid = !(Drive.InternalDriveType == InternalDriveType.Floppy ^ Type == MediaType.FloppyDisk);

            // TODO: HardDisk being in the Removable category is a hack, fix this later
            bool removableDiskValid = !((Drive.InternalDriveType == InternalDriveType.Removable || Drive.InternalDriveType == InternalDriveType.HardDisk)
                ^ (Type == MediaType.CompactFlash || Type == MediaType.SDCard || Type == MediaType.FlashDrive || Type == MediaType.HardDisk));

            return parametersValid && floppyValid && removableDiskValid;
        }

        /// <summary>
        /// Run internal program async with an input set of parameters
        /// </summary>
        /// <param name="executionContext">ExecutionContext object representing how to invoke the internal program</param>
        /// <returns>Standard output from commandline window</returns>
        private static async Task<string> ExecuteInternalProgram(BaseExecutionContext parameters)
        {
            Process childProcess;
#if NET40
            string output = await Task.Factory.StartNew(() =>
#else
            string output = await Task.Run(() =>
#endif
            {
                childProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = parameters.ExecutablePath!,
                        Arguments = parameters.GenerateParameters()!,
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
            if (ExecutionContext == null || !ParametersValid())
                return Result.Failure("Error! Current configuration is not supported!");

            // Fix the output paths, just in case
            OutputPath = InfoTool.NormalizeOutputPaths(OutputPath, false);

            // Validate that the output path isn't on the dumping drive
            if (Drive?.Name != null && OutputPath.StartsWith(Drive.Name))
                return Result.Failure("Error! Cannot output to same drive that is being dumped!");

            // Validate that the required program exists
            if (!File.Exists(ExecutionContext.ExecutablePath))
                return Result.Failure($"Error! {ExecutionContext.ExecutablePath} does not exist!");

            // Validate that the dumping drive doesn't contain the executable
            string fullExecutablePath = Path.GetFullPath(ExecutionContext.ExecutablePath!);
            if (Drive?.Name != null && fullExecutablePath.StartsWith(Drive.Name))
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
            if (string.IsNullOrEmpty(_options.DiscImageCreatorPath))
                return false;

            // Validate that the required program exists
            if (!File.Exists(_options.DiscImageCreatorPath))
                return false;

            return true;
        }

        /// <summary>
        /// Run a standalone DiscImageCreator command
        /// </summary>
        /// <param name="command">Command string to run</param>
        /// <returns>The output of the command on success, null on error</returns>
        private async Task<string?> RunStandaloneDiscImageCreatorCommand(string command)
        {
            // Validate that DiscImageCreator is all set
            if (!RequiredProgramsExist())
                return null;

            // Validate we're not trying to eject a non-optical
            if (Drive == null || Drive.InternalDriveType != InternalDriveType.Optical)
                return null;

            CancelDumping();

            var parameters = new ExecutionContexts.DiscImageCreator.ExecutionContext(string.Empty)
            {
                BaseCommand = command,
                DrivePath = Drive.Name,
                ExecutablePath = _options.DiscImageCreatorPath,
            };

            return await ExecuteInternalProgram(parameters);
        }

        #endregion
    }
}
