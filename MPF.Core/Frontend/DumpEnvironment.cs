using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinaryObjectScanner;
using MPF.Core.ExecutionContexts;
using MPF.Core.Processors;
using MPF.Core.Utilities;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;

namespace MPF.Core.Frontend
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
        private readonly Drive? _drive;

        /// <summary>
        /// ExecutionContext object representing how to invoke the internal program
        /// </summary>
        private BaseExecutionContext? _executionContext;

        /// <summary>
        /// Currently selected dumping program
        /// </summary>
        private readonly InternalProgram _internalProgram;

        /// <summary>
        /// Options object representing user-defined options
        /// </summary>
        private readonly Frontend.Options _options;

        /// <summary>
        /// Processor object representing how to process the outputs
        /// </summary>
        private BaseProcessor? _processor;

        /// <summary>
        /// Currently selected system
        /// </summary>
        private readonly RedumpSystem? _system;

        /// <summary>
        /// Currently selected media type
        /// </summary>
        private readonly MediaType? _type;

        #endregion

        #region Passthrough Fields

        /// <inheritdoc cref="BaseExecutionContext.InputPath"/>
        public string? ContextInputPath => _executionContext?.InputPath;

        /// <inheritdoc cref="BaseExecutionContext.OutputPath"/>
        public string? ContextOutputPath => _executionContext?.OutputPath;

        /// <inheritdoc cref="Drive.MarkedActive/>
        public bool DriveMarkedActive => _drive?.MarkedActive ?? false;

        /// <inheritdoc cref="Drive.Name/>
        public string? DriveName => _drive?.Name;

        /// <inheritdoc cref="BaseExecutionContext.Speed"/>
        public int? Speed
        {
            get => _executionContext?.Speed;
            set
            {
                if (_executionContext != null)
                    _executionContext.Speed = value;
            }
        }

        /// <inheritdoc cref="Extensions.LongName(RedumpSystem?)/>
        public string? SystemName => _system.LongName();

        #endregion

        #region Event Handlers

        /// <summary>
        /// Generic way of reporting a message
        /// </summary>
        public EventHandler<StringEventArgs>? ReportStatus;

        /// <summary>
        /// Queue of items that need to be logged
        /// </summary>
        private ProcessingQueue<string>? outputQueue;

        /// <summary>
        /// Event handler for data returned from a process
        /// </summary>
        private void OutputToLog(object? proc, StringEventArgs args) => outputQueue?.Enqueue(args);

        /// <summary>
        /// Process the outputs in the queue
        /// </summary>
        private void ProcessOutputs(string nextOutput) => ReportStatus?.Invoke(this, nextOutput);

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
        public DumpEnvironment(Frontend.Options options,
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
            _drive = drive;
            _system = system ?? options.DefaultSystem;
            _type = type ?? MediaType.NONE;
            _internalProgram = internalProgram ?? options.InternalProgram;

            // Dumping program
            SetExecutionContext(parameters);
            SetProcessor();
        }

        #region Internal Program Management

        /// <summary>
        /// Check output path for matching logs from all dumping programs
        /// </summary>
        public InternalProgram? CheckForMatchingProgram(string? outputDirectory, string outputFilename)
        {
            // If a complete dump exists from a different program
            InternalProgram? programFound = null;
            if (programFound == null && _internalProgram != InternalProgram.Aaru)
            {
                var processor = new Processors.Aaru(_system, _type);
                (bool foundOtherFiles, _) = processor.FoundAllFiles(outputDirectory, outputFilename, true);
                if (foundOtherFiles)
                    programFound = InternalProgram.Aaru;
            }
            if (programFound == null && _internalProgram != InternalProgram.DiscImageCreator)
            {
                var processor = new Processors.DiscImageCreator(_system, _type);
                (bool foundOtherFiles, _) = processor.FoundAllFiles(outputDirectory, outputFilename, true);
                if (foundOtherFiles)
                    programFound = InternalProgram.DiscImageCreator;
            }
            if (programFound == null && _internalProgram != InternalProgram.Redumper)
            {
                var processor = new Processors.Redumper(_system, _type);
                (bool foundOtherFiles, _) = processor.FoundAllFiles(outputDirectory, outputFilename, true);
                if (foundOtherFiles)
                    programFound = InternalProgram.Redumper;
            }

            return programFound;
        }

        /// <summary>
        /// Set the parameters object based on the internal program and parameters string
        /// </summary>
        /// <param name="parameters">String representation of the parameters</param>
        public bool SetExecutionContext(string? parameters)
        {
            _executionContext = _internalProgram switch
            {
                InternalProgram.Aaru => new ExecutionContexts.Aaru.ExecutionContext(parameters) { ExecutablePath = _options.AaruPath },
                InternalProgram.DiscImageCreator => new ExecutionContexts.DiscImageCreator.ExecutionContext(parameters) { ExecutablePath = _options.DiscImageCreatorPath },
                InternalProgram.Redumper => new ExecutionContexts.Redumper.ExecutionContext(parameters) { ExecutablePath = _options.RedumperPath },

                // If no dumping program found, set to null
                InternalProgram.NONE => null,
                _ => null,
            };

            // Set system and type
            if (_executionContext != null)
            {
                _executionContext.System = _system;
                _executionContext.Type = _type;
            }

            return _executionContext != null;
        }

        /// <summary>
        /// Set the processor object based on the internal program
        /// </summary>
        public bool SetProcessor()
        {
            _processor = _internalProgram switch
            {
                InternalProgram.Aaru => new Processors.Aaru(_system, _type),
                InternalProgram.CleanRip => new CleanRip(_system, _type),
                InternalProgram.DiscImageCreator => new DiscImageCreator(_system, _type),
                InternalProgram.PS3CFW => new PS3CFW(_system, _type),
                InternalProgram.Redumper => new Redumper(_system, _type),
                InternalProgram.UmdImageCreator => new UmdImageCreator(_system, _type),
                InternalProgram.XboxBackupCreator => new XboxBackupCreator(_system, _type),

                // If no dumping program found, set to null
                InternalProgram.NONE => null,
                _ => null,
            };

            return _processor != null;
        }

        /// <summary>
        /// Get the full parameter string for either DiscImageCreator or Aaru
        /// </summary>
        /// <param name="driveSpeed">Nullable int representing the drive speed</param>
        /// <returns>String representing the params, null on error</returns>
        public string? GetFullParameters(int? driveSpeed)
        {
            // Populate with the correct params for inputs (if we're not on the default option)
            if (_system != null && _type != MediaType.NONE)
            {
                // If drive letter is invalid, skip this
                if (_drive == null)
                    return null;

                // Set the proper parameters
                _executionContext = _internalProgram switch
                {
                    InternalProgram.Aaru => new ExecutionContexts.Aaru.ExecutionContext(_system, _type, _drive.Name, OutputPath, driveSpeed, _options),
                    InternalProgram.DiscImageCreator => new ExecutionContexts.DiscImageCreator.ExecutionContext(_system, _type, _drive.Name, OutputPath, driveSpeed, _options),
                    InternalProgram.Redumper => new ExecutionContexts.Redumper.ExecutionContext(_system, _type, _drive.Name, OutputPath, driveSpeed, _options),

                    // If no dumping program found, set to null
                    InternalProgram.NONE => null,
                    _ => null,
                };

                // Generate and return the param string
                return _executionContext?.GenerateParameters();
            }

            return null;
        }

        #endregion

        #region Passthrough Functionality

        /// <inheritdoc cref="Extensions.DetectedByWindows(RedumpSystem?)"/>
        public bool DetectedByWindows() => _system.DetectedByWindows();

        /// <inheritdoc cref="EnumExtensions.DoesSupportDriveSpeed(MediaType?)"/>
        public bool DoesSupportDriveSpeed() => _type.DoesSupportDriveSpeed();

        /// <inheritdoc cref="BaseProcessor.FoundAllFiles(string?, string, bool)"/>
        public bool FoundAllFiles(string? outputDirectory, string outputFilename, bool preCheck)
        {
            if (_processor == null)
                return false;

            return _processor.FoundAllFiles(outputDirectory, outputFilename, preCheck).Item1;
        }

        /// <inheritdoc cref="BaseExecutionContext.GetDefaultExtension(MediaType?)"/>
        public string? GetDefaultExtension(MediaType? mediaType)
        {
            if (_executionContext == null)
                return null;

            return _executionContext.GetDefaultExtension(mediaType);
        }

        /// <inheritdoc cref="BaseExecutionContext.GetMediaType()"/>
        public MediaType? GetMediaType()
        {
            if (_executionContext == null)
                return null;

            return _executionContext.GetMediaType();
        }

        /// <inheritdoc cref="Tools.GetSupportStatus(RedumpSystem?, MediaType?)"/>
        public ResultEventArgs GetSupportStatus() => Tools.GetSupportStatus(_system, _type);

        /// <inheritdoc cref="BaseExecutionContext.IsDumpingCommand()"/>
        public bool IsDumpingCommand()
        {
            if (_executionContext == null)
                return false;

            return _executionContext.IsDumpingCommand();
        }

        /// <inheritdoc cref="Drive.RefreshDrive"/>
        public void RefreshDrive() => _drive?.RefreshDrive();

        #endregion

        #region Dumping

        /// <summary>
        /// Cancel an in-progress dumping process
        /// </summary>
        public void CancelDumping() => _executionContext?.KillInternalProgram();

        /// <summary>
        /// Execute the initial invocation of the dumping programs
        /// </summary>
        /// <param name="progress">Optional result progress callback</param>
#if NET40
        public ResultEventArgs Run(IProgress<ResultEventArgs>? progress = null)
#else
        public async Task<ResultEventArgs> Run(IProgress<ResultEventArgs>? progress = null)
#endif
        {
            // If we don't have parameters
            if (_executionContext == null)
                return ResultEventArgs.Failure("Error! Current configuration is not supported!");

            // Check that we have the basics for dumping
            ResultEventArgs result = IsValidForDump();
            if (!result)
                return result;

            // Invoke output processing, if needed
            if (!_options.ToolsInSeparateWindow)
            {
                outputQueue = new ProcessingQueue<string>(ProcessOutputs);
                if (_executionContext.ReportStatus != null)
                    _executionContext.ReportStatus += OutputToLog;
            }

            // Execute internal tool
            progress?.Report(ResultEventArgs.Success($"Executing {_internalProgram}... {(_options.ToolsInSeparateWindow ? "please wait!" : "see log for output!")}"));

            var directoryName = Path.GetDirectoryName(OutputPath);
            if (!string.IsNullOrEmpty(directoryName))
                Directory.CreateDirectory(directoryName);

#if NET40
            var executeTask = Task.Factory.StartNew(() => _executionContext.ExecuteInternalProgram(_options.ToolsInSeparateWindow));
            executeTask.Wait();
#else
            await Task.Run(() => _executionContext.ExecuteInternalProgram(_options.ToolsInSeparateWindow));
#endif
            progress?.Report(ResultEventArgs.Success($"{_internalProgram} has finished!"));

            // Remove event handler if needed
            if (!_options.ToolsInSeparateWindow)
            {
                outputQueue?.Dispose();
                _executionContext.ReportStatus -= OutputToLog;
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
        public async Task<ResultEventArgs> VerifyAndSaveDumpOutput(
            IProgress<ResultEventArgs>? resultProgress = null,
            IProgress<ProtectionProgress>? protectionProgress = null,
            Func<SubmissionInfo?, (bool?, SubmissionInfo?)>? processUserInfo = null,
            SubmissionInfo? seedInfo = null)
        {
            if (_processor == null)
                return ResultEventArgs.Failure("Error! Current configuration is not supported!");

            resultProgress?.Report(ResultEventArgs.Success("Gathering submission information... please wait!"));

            // Get the output directory and filename separately
            var outputDirectory = Path.GetDirectoryName(OutputPath);
            var outputFilename = Path.GetFileName(OutputPath);

            // Check to make sure that the output had all the correct files
            (bool foundFiles, List<string> missingFiles) = _processor.FoundAllFiles(outputDirectory, outputFilename, false);
            if (!foundFiles)
            {
                resultProgress?.Report(ResultEventArgs.Failure($"There were files missing from the output:\n{string.Join("\n", [.. missingFiles])}"));
                return ResultEventArgs.Failure("Error! Please check output directory as dump may be incomplete!");
            }

            // Extract the information from the output files
            resultProgress?.Report(ResultEventArgs.Success("Extracting output information from output files..."));
            var submissionInfo = await SubmissionInfoTool.ExtractOutputInformation(
                OutputPath,
                _drive,
                _system,
                _type,
                _options,
                _processor,
                resultProgress,
                protectionProgress);
            resultProgress?.Report(ResultEventArgs.Success("Extracting information complete!"));

            // Inject seed submission info data, if necessary
            if (seedInfo != null)
            {
                resultProgress?.Report(ResultEventArgs.Success("Injecting user-supplied information..."));
                Builder.InjectSubmissionInformation(submissionInfo, seedInfo);
                resultProgress?.Report(ResultEventArgs.Success("Information injection complete!"));
            }

            // Get user-modifiable information if confugured to
            if (_options.PromptForDiscInformation && processUserInfo != null)
            {
                resultProgress?.Report(ResultEventArgs.Success("Waiting for additional disc information..."));

                bool? filledInfo;
                (filledInfo, submissionInfo) = processUserInfo(submissionInfo);

                if (filledInfo == true)
                    resultProgress?.Report(ResultEventArgs.Success("Additional disc information added!"));
                else
                    resultProgress?.Report(ResultEventArgs.Success("Disc information skipped!"));
            }

            // Process special fields for site codes
            resultProgress?.Report(ResultEventArgs.Success("Processing site codes..."));
            Formatter.ProcessSpecialFields(submissionInfo);
            resultProgress?.Report(ResultEventArgs.Success("Processing complete!"));

            // Format the information for the text output
            resultProgress?.Report(ResultEventArgs.Success("Formatting information..."));
            (var formattedValues, var formatResult) = Formatter.FormatOutputData(submissionInfo, _options.EnableRedumpCompatibility);
            if (formattedValues == null)
                resultProgress?.Report(ResultEventArgs.Failure(formatResult));
            else
                resultProgress?.Report(ResultEventArgs.Success(formatResult));

            // Get the filename suffix for auto-generated files
            var filenameSuffix = _options.AddFilenameSuffix ? Path.GetFileNameWithoutExtension(outputFilename) : null;

            // Write the text output
            resultProgress?.Report(ResultEventArgs.Success("Writing information to !submissionInfo.txt..."));
            (bool txtSuccess, string txtResult) = InfoTool.WriteOutputData(outputDirectory, filenameSuffix, formattedValues);
            if (txtSuccess)
                resultProgress?.Report(ResultEventArgs.Success(txtResult));
            else
                resultProgress?.Report(ResultEventArgs.Failure(txtResult));

            // Write the copy protection output
            if (submissionInfo?.CopyProtection?.FullProtections != null && submissionInfo.CopyProtection.FullProtections.Any())
            {
                if (_options.ScanForProtection && _options.OutputSeparateProtectionFile)
                {
                    resultProgress?.Report(ResultEventArgs.Success("Writing protection to !protectionInfo.txt..."));
                    bool scanSuccess = InfoTool.WriteProtectionData(outputDirectory, filenameSuffix, submissionInfo, _options.HideDriveLetters);
                    if (scanSuccess)
                        resultProgress?.Report(ResultEventArgs.Success("Writing complete!"));
                    else
                        resultProgress?.Report(ResultEventArgs.Failure("Writing could not complete!"));
                }
            }

            // Write the JSON output, if required
            if (_options.OutputSubmissionJSON)
            {
                resultProgress?.Report(ResultEventArgs.Success($"Writing information to !submissionInfo.json{(_options.IncludeArtifacts ? ".gz" : string.Empty)}..."));
                bool jsonSuccess = InfoTool.WriteOutputData(outputDirectory, filenameSuffix, submissionInfo, _options.IncludeArtifacts);
                if (jsonSuccess)
                    resultProgress?.Report(ResultEventArgs.Success("Writing complete!"));
                else
                    resultProgress?.Report(ResultEventArgs.Failure("Writing could not complete!"));
            }

            // Compress the logs, if required
            if (_options.CompressLogFiles)
            {
                resultProgress?.Report(ResultEventArgs.Success("Compressing log files..."));
                (bool compressSuccess, string compressResult) = InfoTool.CompressLogFiles(outputDirectory, filenameSuffix, outputFilename, _processor);
                if (compressSuccess)
                    resultProgress?.Report(ResultEventArgs.Success(compressResult));
                else
                    resultProgress?.Report(ResultEventArgs.Failure(compressResult));
            }

            // Delete unnecessary files, if required
            if (_options.DeleteUnnecessaryFiles)
            {
                resultProgress?.Report(ResultEventArgs.Success("Deleting unnecessary files..."));
                (bool deleteSuccess, string deleteResult) = InfoTool.DeleteUnnecessaryFiles(outputDirectory, outputFilename, _processor);
                if (deleteSuccess)
                    resultProgress?.Report(ResultEventArgs.Success(deleteResult));
                else
                    resultProgress?.Report(ResultEventArgs.Failure(deleteResult));
            }

            // Create PS3 IRD, if required
            if (_options.CreateIRDAfterDumping && _system == RedumpSystem.SonyPlayStation3 && _type == MediaType.BluRay)
            {
                resultProgress?.Report(ResultEventArgs.Success("Creating IRD... please wait!"));
                (bool deleteSuccess, string deleteResult) = await InfoTool.WriteIRD(OutputPath, submissionInfo?.Extras?.DiscKey, submissionInfo?.Extras?.DiscID, submissionInfo?.Extras?.PIC, submissionInfo?.SizeAndChecksums?.Layerbreak, submissionInfo?.SizeAndChecksums?.CRC32);
                if (deleteSuccess)
                    resultProgress?.Report(ResultEventArgs.Success(deleteResult));
                else
                    resultProgress?.Report(ResultEventArgs.Failure(deleteResult));
            }

            resultProgress?.Report(ResultEventArgs.Success("Submission information process complete!"));
            return ResultEventArgs.Success();
        }

        /// <summary>
        /// Checks if the parameters are valid
        /// </summary>
        /// <returns>True if the configuration is valid, false otherwise</returns>
        internal bool ParametersValid()
        {
            // Missing drive means it can never be valid
            if (_drive == null)
                return false;

            bool parametersValid = _executionContext?.IsValid() ?? false;
            bool floppyValid = !(_drive.InternalDriveType == InternalDriveType.Floppy ^ _type == MediaType.FloppyDisk);

            // TODO: HardDisk being in the Removable category is a hack, fix this later
            bool removableDiskValid = !((_drive.InternalDriveType == InternalDriveType.Removable || _drive.InternalDriveType == InternalDriveType.HardDisk)
                ^ (_type == MediaType.CompactFlash || _type == MediaType.SDCard || _type == MediaType.FlashDrive || _type == MediaType.HardDisk));

            return parametersValid && floppyValid && removableDiskValid;
        }

        /// <summary>
        /// Validate the current environment is ready for a dump
        /// </summary>
        /// <returns>Result instance with the outcome</returns>
        private ResultEventArgs IsValidForDump()
        {
            // Validate that everything is good
            if (_executionContext == null || !ParametersValid())
                return ResultEventArgs.Failure("Error! Current configuration is not supported!");

            // Fix the output paths, just in case
            OutputPath = InfoTool.NormalizeOutputPaths(OutputPath, false);

            // Validate that the output path isn't on the dumping drive
            if (_drive?.Name != null && OutputPath.StartsWith(_drive.Name))
                return ResultEventArgs.Failure("Error! Cannot output to same drive that is being dumped!");

            // Validate that the required program exists
            if (!File.Exists(_executionContext.ExecutablePath))
                return ResultEventArgs.Failure($"Error! {_executionContext.ExecutablePath} does not exist!");

            // Validate that the dumping drive doesn't contain the executable
            string fullExecutablePath = Path.GetFullPath(_executionContext.ExecutablePath!);
            if (_drive?.Name != null && fullExecutablePath.StartsWith(_drive.Name))
                return ResultEventArgs.Failure("Error! Cannot dump same drive that executable resides on!");

            // Validate that the current configuration is supported
            return Tools.GetSupportStatus(_system, _type);
        }

        #endregion
    }
}
