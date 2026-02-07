using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using BinaryObjectScanner;
using MPF.ExecutionContexts;
using MPF.Frontend.Tools;
using MPF.Processors;
using Newtonsoft.Json;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;
using Formatting = Newtonsoft.Json.Formatting;

namespace MPF.Frontend
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
        private Drive? _drive;

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
        private readonly Options _options;

        /// <summary>
        /// Processor object representing how to process the outputs
        /// </summary>
        private BaseProcessor? _processor;

        /// <summary>
        /// Currently selected system
        /// </summary>
        private readonly RedumpSystem? _system;

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
            set => _executionContext?.Speed = value;
        }

        /// <inheritdoc cref="Extensions.LongName(RedumpSystem?)/>
        public string? SystemName => _system.LongName();

        #endregion

        #region Event Handlers

        /// <summary>
        /// Generic way of reporting a message
        /// </summary>
        public EventHandler<StringEventArgs>? ReportStatus;

        #endregion

        /// <summary>
        /// Constructor for a full DumpEnvironment object from user information
        /// </summary>
        /// <param name="options"></param>
        /// <param name="outputPath"></param>
        /// <param name="drive"></param>
        /// <param name="system"></param>
        /// <param name="internalProgram"></param>
        public DumpEnvironment(Options options,
            string? outputPath,
            Drive? drive,
            RedumpSystem? system,
            InternalProgram? internalProgram)
        {
            // Set options object
            _options = new Options(options);

            // Output paths
            OutputPath = FrontendTool.NormalizeOutputPaths(outputPath, false);

            // UI information
            _drive = drive;
            _system = system ?? options.Dumping.DefaultSystem;
            _internalProgram = internalProgram ?? options.InternalProgram;
        }

        #region Internal Program Management

        /// <summary>
        /// Check output path for matching logs from all dumping programs
        /// </summary>
        public InternalProgram? CheckForMatchingProgram(MediaType? mediaType, string? outputDirectory, string outputFilename)
        {
            // If a complete dump exists from a different program
            InternalProgram? programFound = null;
            if (programFound is null && _internalProgram != InternalProgram.Redumper)
            {
                var processor = new Redumper(_system);
                var missingFiles = processor.FoundAllFiles(mediaType, outputDirectory, outputFilename);
                if (missingFiles.Count == 0)
                    programFound = InternalProgram.Redumper;
            }

            if (programFound is null && _internalProgram != InternalProgram.DiscImageCreator)
            {
                var processor = new DiscImageCreator(_system);
                var missingFiles = processor.FoundAllFiles(mediaType, outputDirectory, outputFilename);
                if (missingFiles.Count == 0)
                    programFound = InternalProgram.DiscImageCreator;
            }

            if (programFound is null && _internalProgram != InternalProgram.Aaru)
            {
                var processor = new Aaru(_system);
                var missingFiles = processor.FoundAllFiles(mediaType, outputDirectory, outputFilename);
                if (missingFiles.Count == 0)
                    programFound = InternalProgram.Aaru;
            }

            // if (programFound is null && _internalProgram != InternalProgram.Dreamdump)
            // {
            //     var processor = new Dreamdump(_system);
            //     var missingFiles = processor.FoundAllFiles(mediaType, outputDirectory, outputFilename);
            //     if (missingFiles.Count == 0)
            //         programFound = InternalProgram.Dreamdump;
            // }

            return programFound;
        }

        /// <summary>
        /// Check output path for partial logs from all dumping programs
        /// </summary>
        public InternalProgram? CheckForPartialProgram(MediaType? mediaType, string? outputDirectory, string outputFilename)
        {
            // If a complete dump exists from a different program
            InternalProgram? programFound = null;
            if (programFound is null && _internalProgram != InternalProgram.Redumper)
            {
                var processor = new Redumper(_system);
                if (processor.FoundAnyFiles(mediaType, outputDirectory, outputFilename))
                    programFound = InternalProgram.Redumper;
            }

            if (programFound is null && _internalProgram != InternalProgram.DiscImageCreator)
            {
                var processor = new DiscImageCreator(_system);
                if (processor.FoundAnyFiles(mediaType, outputDirectory, outputFilename))
                    programFound = InternalProgram.DiscImageCreator;
            }

            if (programFound is null && _internalProgram != InternalProgram.Aaru)
            {
                var processor = new Aaru(_system);
                if (processor.FoundAnyFiles(mediaType, outputDirectory, outputFilename))
                    programFound = InternalProgram.Aaru;
            }

            // if (programFound is null && _internalProgram != InternalProgram.Dreamdump)
            // {
            //     var processor = new Dreamdump(_system);
            //     if (processor.FoundAnyFiles(mediaType, outputDirectory, outputFilename))
            //         programFound = InternalProgram.Dreamdump;
            // }

            return programFound;
        }

        /// <summary>
        /// Set the parameters object based on the internal program and parameters string
        /// </summary>
        /// <param name="mediaType">MediaType for specialized dumping parameters</param>
        /// <param name="parameters">String representation of the parameters</param>
        public bool SetExecutionContext(MediaType? mediaType, string? parameters)
        {
#pragma warning disable IDE0072
            _executionContext = _internalProgram switch
            {
                InternalProgram.Aaru => new ExecutionContexts.Aaru.ExecutionContext(parameters) { ExecutablePath = _options.Dumping.AaruPath },
                InternalProgram.DiscImageCreator => new ExecutionContexts.DiscImageCreator.ExecutionContext(parameters) { ExecutablePath = _options.Dumping.DiscImageCreatorPath },
                // InternalProgram.Dreamdump => new ExecutionContexts.Dreamdump.ExecutionContext(parameters) { ExecutablePath = _options.Dumping.DreamdumpPath },
                InternalProgram.Redumper => new ExecutionContexts.Redumper.ExecutionContext(parameters) { ExecutablePath = _options.Dumping.RedumperPath },

                // If no dumping program found, set to null
                InternalProgram.NONE => null,
                _ => null,
            };
#pragma warning restore IDE0072

            // Set system, type, and drive
            if (_executionContext is not null)
            {
                _executionContext.RedumpSystem = _system;
                _executionContext.MediaType = mediaType;

                // Set some parameters, if not already set
                OutputPath ??= _executionContext.OutputPath!;
                _drive ??= Drive.Create(InternalDriveType.Optical, _executionContext.InputPath!);
            }

            return _executionContext is not null;
        }

        /// <summary>
        /// Set the processor object based on the internal program
        /// </summary>
        public bool SetProcessor()
        {
            _processor = _internalProgram switch
            {
                InternalProgram.Aaru => new Aaru(_system),
                InternalProgram.CleanRip => new CleanRip(_system),
                InternalProgram.DiscImageCreator => new DiscImageCreator(_system),
                // InternalProgram.Dreamdump => new Dreamdump(_system),
                InternalProgram.PS3CFW => new PS3CFW(_system),
                InternalProgram.Redumper => new Redumper(_system),
                InternalProgram.UmdImageCreator => new UmdImageCreator(_system),
                InternalProgram.XboxBackupCreator => new XboxBackupCreator(_system),

                // If no dumping program found, set to null
                InternalProgram.NONE => null,
                _ => null,
            };

            return _processor is not null;
        }

        /// <summary>
        /// Get the full parameter string for either DiscImageCreator or Aaru
        /// </summary>
        /// <param name="mediaType">MediaType for specialized dumping parameters</param>
        /// <param name="driveSpeed">Nullable int representing the drive speed</param>
        /// <returns>String representing the params, null on error</returns>
        public string? GetFullParameters(MediaType? mediaType, int? driveSpeed)
        {
            // Populate with the correct params for inputs (if we're not on the default option)
            if (_system is not null && mediaType != MediaType.NONE)
            {
                // If drive letter is invalid, skip this
                if (_drive is null)
                    return null;

#pragma warning disable IDE0072
                // Set the proper parameters
                _executionContext = _internalProgram switch
                {
                    InternalProgram.Aaru => new ExecutionContexts.Aaru.ExecutionContext(_system, mediaType, _drive.Name, OutputPath, driveSpeed, _options.Settings),
                    InternalProgram.DiscImageCreator => new ExecutionContexts.DiscImageCreator.ExecutionContext(_system, mediaType, _drive.Name, OutputPath, driveSpeed, _options.Settings),
                    // InternalProgram.Dreamdump => new ExecutionContexts.Dreamdump.ExecutionContext(_system, mediaType, _drive.Name, OutputPath, driveSpeed, _options.Settings),
                    InternalProgram.Redumper => new ExecutionContexts.Redumper.ExecutionContext(_system, mediaType, _drive.Name, OutputPath, driveSpeed, _options.Settings),

                    // If no dumping program found, set to null
                    InternalProgram.NONE => null,
                    _ => null,
                };
#pragma warning restore IDE0072

                // Generate and return the param string
                return _executionContext?.GenerateParameters();
            }

            return null;
        }

        #endregion

        #region Passthrough Functionality

        /// <inheritdoc cref="Extensions.DetectedByWindows(RedumpSystem?)"/>
        public bool DetectedByWindows() => _system.DetectedByWindows();

        /// <summary>
        /// Determine if the media supports drive speeds
        /// </summary>
        /// <param name="type">MediaType value to check</param>
        /// <returns>True if the media has variable dumping speeds, false otherwise</returns>
        public static bool DoesSupportDriveSpeed(MediaType? mediaType)
        {
#pragma warning disable IDE0072
            return mediaType switch
            {
                MediaType.CDROM
                    or MediaType.DVD
                    or MediaType.GDROM
                    or MediaType.HDDVD
                    or MediaType.BluRay
                    or MediaType.NintendoGameCubeGameDisc
                    or MediaType.NintendoWiiOpticalDisc
                    or MediaType.NintendoWiiUOpticalDisc => true,
                _ => false,
            };
#pragma warning restore IDE0072
        }

        /// <inheritdoc cref="BaseProcessor.FoundAllFiles(MediaType?, string?, string)"/>
        public bool FoundAllFiles(MediaType? mediaType, string? outputDirectory, string outputFilename)
        {
            if (_processor is null)
                return false;

            return _processor.FoundAllFiles(mediaType, outputDirectory, outputFilename).Count == 0;
        }

        /// <inheritdoc cref="BaseProcessor.FoundAnyFiles(MediaType?, string?, string)"/>
        public bool FoundAnyFiles(MediaType? mediaType, string? outputDirectory, string outputFilename)
        {
            if (_processor is null)
                return false;

            return _processor.FoundAnyFiles(mediaType, outputDirectory, outputFilename);
        }

        /// <inheritdoc cref="BaseExecutionContext.GetDefaultExtension(MediaType?)"/>
        public string? GetDefaultExtension(MediaType? mediaType)
        {
            if (_executionContext is null)
                return null;

            return _executionContext.GetDefaultExtension(mediaType);
        }

        /// <inheritdoc cref="BaseExecutionContext.GetMediaType()"/>
        public MediaType? GetMediaType()
        {
            if (_executionContext is null)
                return null;

            return _executionContext.GetMediaType();
        }

        /// <summary>
        /// Verify that, given a system and a media type, they are correct
        /// </summary>
        public ResultEventArgs GetSupportStatus(MediaType? mediaType)
        {
            // No system chosen, update status
            if (_system is null)
                return ResultEventArgs.Failure("Please select a valid system");

#pragma warning disable IDE0072
            // If we're on an unsupported type, update the status accordingly
            return mediaType switch
            {
                // Null means it will be handled by the program
                null => ResultEventArgs.Success("Ready to dump"),

                // Fully supported types
                MediaType.BluRay
                    or MediaType.CDROM
                    or MediaType.DVD
                    or MediaType.FloppyDisk
                    or MediaType.HardDisk
                    or MediaType.CompactFlash
                    or MediaType.SDCard
                    or MediaType.FlashDrive
                    or MediaType.HDDVD => ResultEventArgs.Success($"{mediaType.LongName()} ready to dump"),

                // Partially supported types
                MediaType.GDROM
                    or MediaType.NintendoGameCubeGameDisc
                    or MediaType.NintendoWiiOpticalDisc
                    or MediaType.NintendoWiiUOpticalDisc => ResultEventArgs.Success($"{mediaType.LongName()} partially supported for dumping"),

                // Special case for other supported tools
                MediaType.UMD => ResultEventArgs.Failure($"{mediaType.LongName()} supported for submission info parsing"),

                // Specifically unknown type
                MediaType.NONE => ResultEventArgs.Failure("Please select a valid media type"),

                // Undumpable but recognized types
                _ => ResultEventArgs.Failure($"{mediaType.LongName()} media are not supported for dumping"),
            };
#pragma warning restore IDE0072
        }

        /// <inheritdoc cref="BaseExecutionContext.IsDumpingCommand()"/>
        public bool IsDumpingCommand()
        {
            if (_executionContext is null)
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
        /// <param name="mediaType">MediaType for specialized dumping parameters</param>
        /// <param name="progress">Optional result progress callback</param>
        public async Task<ResultEventArgs> Run(MediaType? mediaType, IProgress<ResultEventArgs>? progress = null)
        {
            // If we don't have parameters
            if (_executionContext is null)
                return ResultEventArgs.Failure("Error! Current configuration is not supported!");

            // Build default console progress indicators if none exist
            if (progress is null)
            {
                var temp = new Progress<ResultEventArgs>();
                temp.ProgressChanged += ConsoleLogger.ProgressUpdated;
                progress = temp;
            }

            // Check that we have the basics for dumping
            ResultEventArgs result = IsValidForDump(mediaType);
            if (result == false)
                return result;

            // Execute internal tool
            progress?.Report(ResultEventArgs.Neutral($"Executing {_internalProgram}... please wait!"));

            var directoryName = Path.GetDirectoryName(OutputPath);
            if (!string.IsNullOrEmpty(directoryName))
                Directory.CreateDirectory(directoryName);

#if NET40
            await Task.Factory.StartNew(() => { _executionContext.ExecuteInternalProgram(); return true; });
#else
            await Task.Run(_executionContext.ExecuteInternalProgram);
#endif
            progress?.Report(ResultEventArgs.Success($"{_internalProgram} has finished!"));

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
            ProcessUserInfoDelegate? processUserInfo = null,
            SubmissionInfo? seedInfo = null)
        {
            if (_processor is null)
                return ResultEventArgs.Failure("Error! Current configuration is not supported!");

            // Build default console progress indicators if none exist
            if (resultProgress is null)
            {
                var temp = new Progress<ResultEventArgs>();
                temp.ProgressChanged += ConsoleLogger.ProgressUpdated;
                resultProgress = temp;
            }

            if (protectionProgress is null)
            {
                var temp = new Progress<ProtectionProgress>();
                temp.ProgressChanged += ConsoleLogger.ProgressUpdated;
                protectionProgress = temp;
            }

            resultProgress.Report(ResultEventArgs.Neutral("Gathering submission information... please wait!"));

            // Get the output directory and filename separately
            var outputDirectory = Path.GetDirectoryName(OutputPath);
            var outputFilename = Path.GetFileName(OutputPath);

            // If a standard log zip was provided, replace the suffix with ".tmp" for easier processing
            if (outputFilename.EndsWith("_logs.zip", StringComparison.OrdinalIgnoreCase))
            {
                int zipSuffixIndex = outputFilename.LastIndexOf("_logs.zip", StringComparison.OrdinalIgnoreCase);
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                outputFilename = outputFilename[..zipSuffixIndex] + ".tmp";
#else
                outputFilename = outputFilename.Substring(0, zipSuffixIndex) + ".tmp";
#endif
            }

            // Determine the media type from the processor
            MediaType? mediaType = _processor.DetermineMediaType(outputDirectory, outputFilename);
            if (mediaType is null)
                return ResultEventArgs.Failure("Could not determine the media type from output files...");

            // Extract the information from the output files
            resultProgress.Report(ResultEventArgs.Neutral("Extracting output information from output files..."));
            var submissionInfo = await SubmissionGenerator.ExtractOutputInformation(
                OutputPath,
                _drive,
                _system,
                mediaType,
                _options,
                _processor,
                resultProgress,
                protectionProgress);
            if (submissionInfo is null)
                return ResultEventArgs.Failure("There was an issue extracting information!");
            else
                resultProgress.Report(ResultEventArgs.Success("Extracting information complete!"));

            // Inject seed submission info data, if necessary
            if (seedInfo is not null)
            {
                resultProgress.Report(ResultEventArgs.Neutral("Injecting user-supplied information..."));
                submissionInfo = Builder.InjectSubmissionInformation(submissionInfo, seedInfo);
                resultProgress.Report(ResultEventArgs.Success("Information injection complete!"));
            }

            // Get user-modifiable information if configured to
            if (_options.Processing.MediaInformation.PromptForDiscInformation && processUserInfo is not null)
            {
                resultProgress.Report(ResultEventArgs.Neutral("Waiting for additional media information..."));
                bool? filledInfo = processUserInfo.Invoke(_options, ref submissionInfo);
                if (filledInfo == true)
                    resultProgress.Report(ResultEventArgs.Success("Additional media information added!"));
                else
                    resultProgress.Report(ResultEventArgs.Success("Media information skipped!"));
            }

            // Process special fields for site codes
            resultProgress.Report(ResultEventArgs.Neutral("Processing site codes..."));
            Formatter.ProcessSpecialFields(submissionInfo!);
            resultProgress.Report(ResultEventArgs.Success("Processing complete!"));

            // Format the information for the text output
            resultProgress.Report(ResultEventArgs.Neutral("Formatting information..."));
            var formattedValues = Formatter.FormatOutputData(submissionInfo, _options.Processing.MediaInformation.EnableRedumpCompatibility, out string? formatResult);
            if (formattedValues is null)
                resultProgress.Report(ResultEventArgs.Failure(formatResult));
            else
                resultProgress.Report(ResultEventArgs.Success(formatResult));

            // Get the filename suffix for auto-generated files
            var filenameSuffix = _options.Processing.AddFilenameSuffix ? Path.GetFileNameWithoutExtension(outputFilename) : null;

            // Write the text output
            resultProgress.Report(ResultEventArgs.Neutral("Writing submission information file..."));
            bool txtSuccess = WriteOutputData(outputDirectory, filenameSuffix, formattedValues, out string txtResult);
            if (txtSuccess)
                resultProgress.Report(ResultEventArgs.Success(txtResult));
            else
                resultProgress.Report(ResultEventArgs.Failure(txtResult));

            // Write the copy protection output
            if (submissionInfo?.CopyProtection?.FullProtections is not null && submissionInfo.CopyProtection.FullProtections.Count > 0)
            {
                if (_options.Processing.ProtectionScanning.ScanForProtection)
                {
                    resultProgress.Report(ResultEventArgs.Neutral("Writing protection information file..."));
                    bool scanSuccess = WriteProtectionData(outputDirectory, filenameSuffix, submissionInfo, _options.Processing.ProtectionScanning.HideDriveLetters);
                    if (scanSuccess)
                        resultProgress.Report(ResultEventArgs.Success("Writing complete!"));
                    else
                        resultProgress.Report(ResultEventArgs.Failure("Writing could not complete!"));
                }
            }

            // Write the JSON output, if required
            if (_options.Processing.OutputSubmissionJSON)
            {
                resultProgress.Report(ResultEventArgs.Neutral($"Writing submission information JSON file{(_options.Processing.IncludeArtifacts ? " with artifacts" : string.Empty)}..."));
                bool jsonSuccess = WriteOutputData(outputDirectory, filenameSuffix, submissionInfo, _options.Processing.IncludeArtifacts);
                if (jsonSuccess)
                    resultProgress.Report(ResultEventArgs.Success("Writing complete!"));
                else
                    resultProgress.Report(ResultEventArgs.Failure("Writing could not complete!"));
            }

            // Compress the logs, if required
            if (_options.Processing.CompressLogFiles)
            {
                resultProgress.Report(ResultEventArgs.Neutral("Compressing log files..."));
#if NET40
                await Task.Factory.StartNew(() =>
#else
                await Task.Run(() =>
#endif
                {
                    bool compressSuccess = _processor.CompressLogFiles(mediaType, _options.Processing.LogCompression, outputDirectory, outputFilename, filenameSuffix, out string compressResult);
                    if (compressSuccess)
                        resultProgress.Report(ResultEventArgs.Success(compressResult));
                    else
                        resultProgress.Report(ResultEventArgs.Failure(compressResult));

                    return compressSuccess;
                });
            }

            // Delete unnecessary files, if required
            if (_options.Processing.DeleteUnnecessaryFiles)
            {
                resultProgress.Report(ResultEventArgs.Neutral("Deleting unnecessary files..."));
                bool deleteSuccess = _processor.DeleteUnnecessaryFiles(mediaType, outputDirectory, outputFilename, out string deleteResult);
                if (deleteSuccess)
                    resultProgress.Report(ResultEventArgs.Success(deleteResult));
                else
                    resultProgress.Report(ResultEventArgs.Failure(deleteResult));
            }

            // Create PS3 IRD, if required
            if (_options.Processing.CreateIRDAfterDumping && _system == RedumpSystem.SonyPlayStation3 && mediaType == MediaType.BluRay)
            {
                resultProgress.Report(ResultEventArgs.Neutral("Creating IRD... please wait!"));
                bool deleteSuccess = await IRDTool.WriteIRD(OutputPath, submissionInfo?.Extras?.DiscKey, submissionInfo?.Extras?.DiscID, submissionInfo?.Extras?.PIC, submissionInfo?.SizeAndChecksums.Layerbreak, submissionInfo?.SizeAndChecksums.CRC32);
                if (deleteSuccess)
                    resultProgress.Report(ResultEventArgs.Success("IRD created!"));
                else
                    resultProgress.Report(ResultEventArgs.Failure("Failed to create IRD"));
            }

            resultProgress.Report(ResultEventArgs.Success("Submission information process complete!"));
            return ResultEventArgs.Success();
        }

        /// <summary>
        /// Checks if the parameters are valid
        /// </summary>
        /// <returns>True if the configuration is valid, false otherwise</returns>
        internal bool ParametersValid(MediaType? mediaType)
        {
            // Missing drive means it can never be valid
            if (_drive is null)
                return false;

            bool parametersValid = _executionContext?.IsValid() ?? false;
            bool floppyValid = !(_drive.InternalDriveType == InternalDriveType.Floppy ^ mediaType == MediaType.FloppyDisk);

            // TODO: HardDisk being in the Removable category is a hack, fix this later
            bool removableDiskValid = !((_drive.InternalDriveType == InternalDriveType.Removable || _drive.InternalDriveType == InternalDriveType.HardDisk)
                ^ (mediaType == MediaType.CompactFlash || mediaType == MediaType.SDCard || mediaType == MediaType.FlashDrive || mediaType == MediaType.HardDisk));

            return parametersValid && floppyValid && removableDiskValid;
        }

        /// <summary>
        /// Validate the current environment is ready for a dump
        /// </summary>
        /// <returns>Result instance with the outcome</returns>
        private ResultEventArgs IsValidForDump(MediaType? mediaType)
        {
            // Validate that everything is good
            if (_executionContext is null || !ParametersValid(mediaType))
                return ResultEventArgs.Failure("Error! Current configuration is not supported!");

            // Fix the output paths, just in case
            OutputPath = FrontendTool.NormalizeOutputPaths(OutputPath, false);

            // Validate that the output path isn't on the dumping drive
            if (_drive?.Name is not null && OutputPath.StartsWith(_drive.Name))
                return ResultEventArgs.Failure("Error! Cannot output to same drive that is being dumped!");

            // Validate that the required program exists
            if (!File.Exists(_executionContext.ExecutablePath))
                return ResultEventArgs.Failure($"Error! {_executionContext.ExecutablePath} does not exist!");

            // Validate that the dumping drive doesn't contain the executable
            string fullExecutablePath = Path.GetFullPath(_executionContext.ExecutablePath!);
            if (_drive?.Name is not null && fullExecutablePath.StartsWith(_drive.Name))
                return ResultEventArgs.Failure("Error! Cannot dump same drive that executable resides on!");

            // Validate that the current configuration is supported
            return GetSupportStatus(mediaType);
        }

        #endregion

        #region Information Output

        /// <summary>
        /// Write the data to the output folder
        /// </summary>
        /// <param name="outputDirectory">Output folder to use as the base path</param>
        /// <param name="filenameSuffix">Optional suffix to append to the filename</param>
        /// <param name="lines">Preformatted string of lines to write out to the file</param>
        /// <returns>True on success, false on error</returns>
        private static bool WriteOutputData(string? outputDirectory, string? filenameSuffix, string? lines, out string status)
        {
            // Check to see if the inputs are valid
            if (lines is null)
            {
                status = "No formatted data found to write!";
                return false;
            }

            // Now write out to a generic file
            try
            {
                // Get the file path
                var path = string.Empty;
                if (string.IsNullOrEmpty(outputDirectory) && string.IsNullOrEmpty(filenameSuffix))
                    path = "!submissionInfo.txt";
                else if (string.IsNullOrEmpty(outputDirectory) && !string.IsNullOrEmpty(filenameSuffix))
                    path = $"!submissionInfo_{filenameSuffix}.txt";
                else if (!string.IsNullOrEmpty(outputDirectory) && string.IsNullOrEmpty(filenameSuffix))
                    path = Path.Combine(outputDirectory, "!submissionInfo.txt");
                else if (!string.IsNullOrEmpty(outputDirectory) && !string.IsNullOrEmpty(filenameSuffix))
                    path = Path.Combine(outputDirectory, $"!submissionInfo_{filenameSuffix}.txt");

                using var sw = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.Write), Encoding.UTF8);
                sw.Write(lines);
            }
            catch (Exception ex)
            {
                status = $"Writing could not complete: {ex}";
                return false;
            }

            status = "Writing complete!";
            return true;
        }

        // MOVE TO REDUMPLIB
        /// <summary>
        /// Write the data to the output folder
        /// </summary>
        /// <param name="outputDirectory">Output folder to use as the base path</param>
        /// <param name="filenameSuffix">Optional suffix to append to the filename</param>
        /// <param name="info">SubmissionInfo object representing the JSON to write out to the file</param>
        /// <param name="includedArtifacts">True if artifacts were included, false otherwise</param>
        /// <returns>True on success, false on error</returns>
        private static bool WriteOutputData(string? outputDirectory, string? filenameSuffix, SubmissionInfo? info, bool includedArtifacts)
        {
            // Check to see if the input is valid
            if (info is null)
                return false;

            try
            {
                // Get the output path
                var path = string.Empty;
                if (string.IsNullOrEmpty(outputDirectory) && string.IsNullOrEmpty(filenameSuffix))
                    path = "!submissionInfo.json";
                else if (string.IsNullOrEmpty(outputDirectory) && !string.IsNullOrEmpty(filenameSuffix))
                    path = $"!submissionInfo_{filenameSuffix}.json";
                else if (!string.IsNullOrEmpty(outputDirectory) && string.IsNullOrEmpty(filenameSuffix))
                    path = Path.Combine(outputDirectory, "!submissionInfo.json");
                else if (!string.IsNullOrEmpty(outputDirectory) && !string.IsNullOrEmpty(filenameSuffix))
                    path = Path.Combine(outputDirectory, $"!submissionInfo_{filenameSuffix}.json");

                // Ensure the extension is correct for the output
                if (includedArtifacts)
                    path += ".gz";

                // Create and open the output file
                using var fs = File.Create(path);

                // Create the JSON serializer
                var serializer = new JsonSerializer { Formatting = Formatting.Indented };

                // If we included artifacts, write to a GZip-compressed file
                if (includedArtifacts)
                {
                    using var gs = new GZipStream(fs, CompressionMode.Compress);
                    using var sw = new StreamWriter(gs, Encoding.UTF8);
                    serializer.Serialize(sw, info);
                }
                else
                {
                    using var sw = new StreamWriter(fs, Encoding.UTF8);
                    serializer.Serialize(sw, info);
                }
            }
            catch
            {
                // Absorb the exception
                return false;
            }

            return true;
        }

        // MOVE TO REDUMPLIB
        /// <summary>
        /// Write the protection data to the output folder
        /// </summary>
        /// <param name="outputDirectory">Output folder to use as the base path</param>
        /// <param name="filenameSuffix">Optional suffix to append to the filename</param>
        /// <param name="info">SubmissionInfo object containing the protection information</param>
        /// <param name="hideDriveLetters">True if drive letters are to be removed from output, false otherwise</param>
        /// <returns>True on success, false on error</returns>
        private static bool WriteProtectionData(string? outputDirectory, string? filenameSuffix, SubmissionInfo? info, bool hideDriveLetters)
        {
            // Check to see if the inputs are valid
            if (info?.CopyProtection?.FullProtections is null || info.CopyProtection.FullProtections.Count == 0)
                return true;

            // Now write out to a generic file
            try
            {
                var path = string.Empty;
                if (string.IsNullOrEmpty(outputDirectory) && string.IsNullOrEmpty(filenameSuffix))
                    path = "!protectionInfo.txt";
                else if (string.IsNullOrEmpty(outputDirectory) && !string.IsNullOrEmpty(filenameSuffix))
                    path = $"!protectionInfo{filenameSuffix}.txt";
                else if (!string.IsNullOrEmpty(outputDirectory) && string.IsNullOrEmpty(filenameSuffix))
                    path = Path.Combine(outputDirectory, "!protectionInfo.txt");
                else if (!string.IsNullOrEmpty(outputDirectory) && !string.IsNullOrEmpty(filenameSuffix))
                    path = Path.Combine(outputDirectory, $"!protectionInfo{filenameSuffix}.txt");

                using var sw = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.Write), Encoding.UTF8);

                List<string> sortedKeys = [.. info.CopyProtection.FullProtections.Keys];
                sortedKeys.Sort();

                foreach (string key in sortedKeys)
                {
                    string scanPath = key;
                    if (hideDriveLetters)
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                        scanPath = Path.DirectorySeparatorChar + key[(Path.GetPathRoot(key) ?? string.Empty).Length..];
#else
                        scanPath = Path.DirectorySeparatorChar + key.Substring((Path.GetPathRoot(key) ?? string.Empty).Length);
#endif

                    List<string>? scanResult = info.CopyProtection.FullProtections[key];

                    if (scanResult is null)
                        sw.WriteLine($"{scanPath}: None");
                    else
                        sw.WriteLine($"{scanPath}: {string.Join(", ", [.. scanResult])}");
                }
            }
            catch
            {
                // Absorb the exception
                return false;
            }

            return true;
        }

        #endregion
    }
}
