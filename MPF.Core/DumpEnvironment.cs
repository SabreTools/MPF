using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using BurnOutSharp;
using MPF.Core.Data;
using MPF.Core.Utilities;
using MPF.Core.Modules;
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
#if NET48
        public Drive Drive { get; private set; }
#else
        public Drive? Drive { get; private set; }
#endif

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
        public Data.Options Options { get; private set; }

        /// <summary>
        /// Parameters object representing what to send to the internal program
        /// </summary>
#if NET48
        public BaseParameters Parameters { get; private set; }
#else
        public BaseParameters? Parameters { get; private set; }
#endif

        #endregion

        #region Event Handlers

        /// <summary>
        /// Generic way of reporting a message
        /// </summary>
#if NET48
        public EventHandler<string> ReportStatus;
#else
        public EventHandler<string>? ReportStatus;
#endif

        /// <summary>
        /// Queue of items that need to be logged
        /// </summary>
#if NET48
        private ProcessingQueue<string> outputQueue;
#else
        private ProcessingQueue<string>? outputQueue;
#endif

        /// <summary>
        /// Event handler for data returned from a process
        /// </summary>
#if NET48
        private void OutputToLog(object proc, string args) => outputQueue?.Enqueue(args);
#else
        private void OutputToLog(object? proc, string args) => outputQueue?.Enqueue(args);
#endif

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
        public DumpEnvironment(Data.Options options,
            string outputPath,
#if NET48
            Drive drive,
#else
            Drive? drive,
#endif
            RedumpSystem? system,
            MediaType? type,
            InternalProgram? internalProgram,
#if NET48
            string parameters)
#else
            string? parameters)
#endif
        {
            // Set options object
            Options = options;

            // Output paths
            OutputPath = InfoTool.NormalizeOutputPaths(outputPath, true);

            // UI information
            Drive = drive;
            System = system ?? options.DefaultSystem;
            Type = type ?? MediaType.NONE;
            InternalProgram = internalProgram ?? options.InternalProgram;

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
            if (Parameters?.InternalProgram != InternalProgram.DiscImageCreator)
                return;

            try
            {
                // Normalize the output path
                string outputPath = InfoTool.NormalizeOutputPaths(OutputPath, true);

                // Replace all instances in the output directory
                var outputDirectory = Path.GetDirectoryName(outputPath);
                outputDirectory = outputDirectory?.Replace(".", "_");

                // Replace all instances in the output filename
                string outputFilename = Path.GetFileNameWithoutExtension(outputPath);
                outputFilename = outputFilename.Replace(".", "_");

                // Get the extension for recreating the path
                string outputExtension = Path.GetExtension(outputPath).TrimStart('.');

                // Rebuild the output path
                if (string.IsNullOrWhiteSpace(outputDirectory))
                {
                    if (string.IsNullOrWhiteSpace(outputExtension))
                        OutputPath = outputFilename;
                    else
                        OutputPath = $"{outputFilename}.{outputExtension}";
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(outputExtension))
                        OutputPath = Path.Combine(outputDirectory, outputFilename);
                    else
                        OutputPath = Path.Combine(outputDirectory, $"{outputFilename}.{outputExtension}");
                }

                // Assign the path to the filename as well for dumping
                ((Modules.DiscImageCreator.Parameters)Parameters).Filename = OutputPath;
            }
            catch { }
        }

        /// <summary>
        /// Set the parameters object based on the internal program and parameters string
        /// </summary>
        /// <param name="parameters">String representation of the parameters</param>
#if NET48
        public void SetParameters(string parameters)
#else
        public void SetParameters(string? parameters)
#endif
        {
#if NET48
            switch (InternalProgram)
            {
                // Dumping support
                case InternalProgram.Aaru:
                    Parameters = new Modules.Aaru.Parameters(parameters) { ExecutablePath = Options.AaruPath };
                    break;

                case InternalProgram.DiscImageCreator:
                    Parameters = new Modules.DiscImageCreator.Parameters(parameters) { ExecutablePath = Options.DiscImageCreatorPath };
                    break;

                case InternalProgram.Redumper:
                    Parameters = new Modules.Redumper.Parameters(parameters) { ExecutablePath = Options.RedumperPath };
                    break;

                // Verification support only
                case InternalProgram.CleanRip:
                    Parameters = new Modules.CleanRip.Parameters(parameters) { ExecutablePath = null };
                    break;

                case InternalProgram.DCDumper:
                    Parameters = null; // TODO: Create correct parameter type when supported
                    break;

                case InternalProgram.UmdImageCreator:
                    Parameters = new Modules.UmdImageCreator.Parameters(parameters) { ExecutablePath = null };
                    break;

                // This should never happen, but it needs a fallback
                default:
                    Parameters = new Modules.DiscImageCreator.Parameters(parameters) { ExecutablePath = Options.DiscImageCreatorPath };
                    break;
            }
#else
            Parameters = InternalProgram switch
            {
                // Dumping support
                InternalProgram.Aaru => new Modules.Aaru.Parameters(parameters) { ExecutablePath = Options.AaruPath },
                InternalProgram.DiscImageCreator => new Modules.DiscImageCreator.Parameters(parameters) { ExecutablePath = Options.DiscImageCreatorPath },
                InternalProgram.Redumper => new Modules.Redumper.Parameters(parameters) { ExecutablePath = Options.RedumperPath },

                // Verification support only
                InternalProgram.CleanRip => new Modules.CleanRip.Parameters(parameters) { ExecutablePath = null },
                InternalProgram.DCDumper => null, // TODO: Create correct parameter type when supported
                InternalProgram.UmdImageCreator => new Modules.UmdImageCreator.Parameters(parameters) { ExecutablePath = null },

                // This should never happen, but it needs a fallback
                _ => new Modules.DiscImageCreator.Parameters(parameters) { ExecutablePath = Options.DiscImageCreatorPath },
            };
#endif

            // Set system and type
            if (Parameters != null)
            {
                Parameters.System = System;
                Parameters.Type = Type;
            }
        }

        /// <summary>
        /// Get the full parameter string for either DiscImageCreator or Aaru
        /// </summary>
        /// <param name="driveSpeed">Nullable int representing the drive speed</param>
        /// <returns>String representing the params, null on error</returns>
#if NET48
        public string GetFullParameters(int? driveSpeed)
#else
        public string? GetFullParameters(int? driveSpeed)
#endif
        {
            // Populate with the correct params for inputs (if we're not on the default option)
            if (System != null && Type != MediaType.NONE)
            {
                // If drive letter is invalid, skip this
                if (Drive == null)
                    return null;

                // Set the proper parameters
#if NET48
                switch (InternalProgram)
                {
                    case InternalProgram.Aaru:
                        Parameters = new Modules.Aaru.Parameters(System, Type, Drive.Name, OutputPath, driveSpeed, Options);
                        break;

                    case InternalProgram.DiscImageCreator:
                        Parameters = new Modules.DiscImageCreator.Parameters(System, Type, Drive.Name, OutputPath, driveSpeed, Options);
                        break;

                    case InternalProgram.Redumper:
                        Parameters = new Modules.Redumper.Parameters(System, Type, Drive.Name, OutputPath, driveSpeed, Options);
                        break;

                    // This should never happen, but it needs a fallback
                    default:
                        Parameters = new Modules.DiscImageCreator.Parameters(System, Type, Drive.Name, OutputPath, driveSpeed, Options);
                        break;
                }
#else
                Parameters = InternalProgram switch
                {
                    InternalProgram.Aaru => new Modules.Aaru.Parameters(System, Type, Drive.Name, OutputPath, driveSpeed, Options),
                    InternalProgram.DiscImageCreator => new Modules.DiscImageCreator.Parameters(System, Type, Drive.Name, OutputPath, driveSpeed, Options),
                    InternalProgram.Redumper => new Modules.Redumper.Parameters(System, Type, Drive.Name, OutputPath, driveSpeed, Options),

                    // This should never happen, but it needs a fallback
                    _ => new Modules.DiscImageCreator.Parameters(System, Type, Drive.Name, OutputPath, driveSpeed, Options),
                };
#endif

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
        public void CancelDumping() => Parameters?.KillInternalProgram();

        /// <summary>
        /// Eject the disc using DiscImageCreator
        /// </summary>
#if NET48
        public async Task<string> EjectDisc() =>
#else
        public async Task<string?> EjectDisc() =>
#endif
        await RunStandaloneDiscImageCreatorCommand(Modules.DiscImageCreator.CommandStrings.Eject);

        /// <summary>
        /// Reset the current drive using DiscImageCreator
        /// </summary>
#if NET48
        public async Task<string> ResetDrive() =>
#else
        public async Task<string?> ResetDrive() =>
#endif
            await RunStandaloneDiscImageCreatorCommand(Modules.DiscImageCreator.CommandStrings.Reset);

        /// <summary>
        /// Execute the initial invocation of the dumping programs
        /// </summary>
        /// <param name="progress">Optional result progress callback</param>
#if NET48
        public async Task<Result> Run(IProgress<Result> progress = null)
#else
        public async Task<Result> Run(IProgress<Result>? progress = null)
#endif
        {
            // If we don't have parameters
            if (Parameters == null)
                return Result.Failure("Error! Current configuration is not supported!");

            // Check that we have the basics for dumping
            Result result = IsValidForDump();
            if (!result)
                return result;

            // Invoke output processing, if needed
            if (!Options.ToolsInSeparateWindow)
            {
                outputQueue = new ProcessingQueue<string>(ProcessOutputs);
                if (Parameters.ReportStatus != null)
                    Parameters.ReportStatus += OutputToLog;
            }

            // Execute internal tool
            progress?.Report(Result.Success($"Executing {InternalProgram}... {(Options.ToolsInSeparateWindow ? "please wait!" : "see log for output!")}"));

            var directoryName = Path.GetDirectoryName(OutputPath);
            if (!string.IsNullOrWhiteSpace(directoryName))
                Directory.CreateDirectory(directoryName);

            await Task.Run(() => Parameters.ExecuteInternalProgram(Options.ToolsInSeparateWindow));
            progress?.Report(Result.Success($"{InternalProgram} has finished!"));

            // Remove event handler if needed
            if (!Options.ToolsInSeparateWindow)
            {
                outputQueue?.Dispose();
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
        /// <param name="seedInfo">A seed SubmissionInfo object that contains user data</param>
        /// <returns>Result instance with the outcome</returns>
        public async Task<Result> VerifyAndSaveDumpOutput(
#if NET48
            IProgress<Result> resultProgress = null,
            IProgress<ProtectionProgress> protectionProgress = null,
            Func<SubmissionInfo, (bool?, SubmissionInfo)> processUserInfo = null,
            SubmissionInfo seedInfo = null)
#else
            IProgress<Result>? resultProgress = null,
            IProgress<ProtectionProgress>? protectionProgress = null,
            Func<SubmissionInfo?, (bool?, SubmissionInfo?)>? processUserInfo = null,
            SubmissionInfo? seedInfo = null)
#endif
        {
            resultProgress?.Report(Result.Success("Gathering submission information... please wait!"));

            // Get the output directory and filename separately
            var outputDirectory = Path.GetDirectoryName(OutputPath);
            var outputFilename = Path.GetFileName(OutputPath);

            // Check to make sure that the output had all the correct files
            (bool foundFiles, List<string> missingFiles) = InfoTool.FoundAllFiles(outputDirectory, outputFilename, Parameters, false);
            if (!foundFiles)
            {
                resultProgress?.Report(Result.Failure($"There were files missing from the output:\n{string.Join("\n", missingFiles)}"));
                return Result.Failure("Error! Please check output directory as dump may be incomplete!");
            }

            // Extract the information from the output files
            resultProgress?.Report(Result.Success("Extracting output information from output files..."));
            var submissionInfo = await SubmissionInfoTool.ExtractOutputInformation(
                OutputPath,
                Drive,
                System,
                Type,
                Options,
                Parameters,
                resultProgress,
                protectionProgress);
            resultProgress?.Report(Result.Success("Extracting information complete!"));

            // Inject seed submission info data, if necessary
            if (seedInfo != null)
            {
                resultProgress?.Report(Result.Success("Injecting user-supplied information..."));
                InjectSubmissionInformation(submissionInfo, seedInfo);
                resultProgress?.Report(Result.Success("Information injection complete!"));
            }

            // Eject the disc automatically if configured to
            if (Options.EjectAfterDump == true)
            {
                resultProgress?.Report(Result.Success($"Ejecting disc in drive {Drive?.Name}"));
                await EjectDisc();
            }

            // Reset the drive automatically if configured to
            if (InternalProgram == InternalProgram.DiscImageCreator && Options.DICResetDriveAfterDump)
            {
                resultProgress?.Report(Result.Success($"Resetting drive {Drive?.Name}"));
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
            (var formattedValues, var formatResult) = InfoTool.FormatOutputData(submissionInfo, Options);
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
                (bool compressSuccess, string compressResult) = InfoTool.CompressLogFiles(outputDirectory, outputFilename, Parameters);
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
            // Missing drive means it can never be valid
            if (Drive == null)
                return false;

            bool parametersValid = Parameters?.IsValid() ?? false;
            bool floppyValid = !(Drive.InternalDriveType == InternalDriveType.Floppy ^ Type == MediaType.FloppyDisk);

            // TODO: HardDisk being in the Removable category is a hack, fix this later
            bool removableDiskValid = !((Drive.InternalDriveType == InternalDriveType.Removable || Drive.InternalDriveType == InternalDriveType.HardDisk)
                ^ (Type == MediaType.CompactFlash || Type == MediaType.SDCard || Type == MediaType.FlashDrive || Type == MediaType.HardDisk));

            return parametersValid && floppyValid && removableDiskValid;
        }

        /// <summary>
        /// Run internal program async with an input set of parameters
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>Standard output from commandline window</returns>
        private static async Task<string> ExecuteInternalProgram(BaseParameters parameters)
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
        /// Inject information from a seed SubmissionInfo into the existing one
        /// </summary>
        /// <param name="info">Existing submission information</param>
        /// <param name="seed">User-supplied submission information</param>
#if NET48
        private static void InjectSubmissionInformation(SubmissionInfo info, SubmissionInfo seed)
#else
        private static void InjectSubmissionInformation(SubmissionInfo? info, SubmissionInfo? seed)
#endif
        {
            // If we have any invalid info
            if (seed == null)
                return;

            // Ensure that required sections exist
            info = SubmissionInfoTool.EnsureAllSections(info);

            // Otherwise, inject information as necessary
            if (info.CommonDiscInfo != null && seed.CommonDiscInfo != null)
            {
                // Info that only overwrites if supplied
                if (!string.IsNullOrWhiteSpace(seed.CommonDiscInfo.Title)) info.CommonDiscInfo.Title = seed.CommonDiscInfo.Title;
                if (!string.IsNullOrWhiteSpace(seed.CommonDiscInfo.ForeignTitleNonLatin)) info.CommonDiscInfo.ForeignTitleNonLatin = seed.CommonDiscInfo.ForeignTitleNonLatin;
                if (!string.IsNullOrWhiteSpace(seed.CommonDiscInfo.DiscNumberLetter)) info.CommonDiscInfo.DiscNumberLetter = seed.CommonDiscInfo.DiscNumberLetter;
                if (!string.IsNullOrWhiteSpace(seed.CommonDiscInfo.DiscTitle)) info.CommonDiscInfo.DiscTitle = seed.CommonDiscInfo.DiscTitle;
                if (seed.CommonDiscInfo.Category != null) info.CommonDiscInfo.Category = seed.CommonDiscInfo.Category;
                if (seed.CommonDiscInfo.Region != null) info.CommonDiscInfo.Region = seed.CommonDiscInfo.Region;
                if (seed.CommonDiscInfo.Languages != null) info.CommonDiscInfo.Languages = seed.CommonDiscInfo.Languages;
                if (seed.CommonDiscInfo.LanguageSelection != null) info.CommonDiscInfo.LanguageSelection = seed.CommonDiscInfo.LanguageSelection;
                if (!string.IsNullOrWhiteSpace(seed.CommonDiscInfo.Serial)) info.CommonDiscInfo.Serial = seed.CommonDiscInfo.Serial;
                if (!string.IsNullOrWhiteSpace(seed.CommonDiscInfo.Barcode)) info.CommonDiscInfo.Barcode = seed.CommonDiscInfo.Barcode;
                if (!string.IsNullOrWhiteSpace(seed.CommonDiscInfo.Comments)) info.CommonDiscInfo.Comments = seed.CommonDiscInfo.Comments;
                if (seed.CommonDiscInfo.CommentsSpecialFields != null) info.CommonDiscInfo.CommentsSpecialFields = seed.CommonDiscInfo.CommentsSpecialFields;
                if (!string.IsNullOrWhiteSpace(seed.CommonDiscInfo.Contents)) info.CommonDiscInfo.Contents = seed.CommonDiscInfo.Contents;
                if (seed.CommonDiscInfo.ContentsSpecialFields != null) info.CommonDiscInfo.ContentsSpecialFields = seed.CommonDiscInfo.ContentsSpecialFields;

                // Info that always overwrites
                info.CommonDiscInfo.Layer0MasteringRing = seed.CommonDiscInfo.Layer0MasteringRing;
                info.CommonDiscInfo.Layer0MasteringSID = seed.CommonDiscInfo.Layer0MasteringSID;
                info.CommonDiscInfo.Layer0ToolstampMasteringCode = seed.CommonDiscInfo.Layer0ToolstampMasteringCode;
                info.CommonDiscInfo.Layer0MouldSID = seed.CommonDiscInfo.Layer0MouldSID;
                info.CommonDiscInfo.Layer0AdditionalMould = seed.CommonDiscInfo.Layer0AdditionalMould;

                info.CommonDiscInfo.Layer1MasteringRing = seed.CommonDiscInfo.Layer1MasteringRing;
                info.CommonDiscInfo.Layer1MasteringSID = seed.CommonDiscInfo.Layer1MasteringSID;
                info.CommonDiscInfo.Layer1ToolstampMasteringCode = seed.CommonDiscInfo.Layer1ToolstampMasteringCode;
                info.CommonDiscInfo.Layer1MouldSID = seed.CommonDiscInfo.Layer1MouldSID;
                info.CommonDiscInfo.Layer1AdditionalMould = seed.CommonDiscInfo.Layer1AdditionalMould;

                info.CommonDiscInfo.Layer2MasteringRing = seed.CommonDiscInfo.Layer2MasteringRing;
                info.CommonDiscInfo.Layer2MasteringSID = seed.CommonDiscInfo.Layer2MasteringSID;
                info.CommonDiscInfo.Layer2ToolstampMasteringCode = seed.CommonDiscInfo.Layer2ToolstampMasteringCode;

                info.CommonDiscInfo.Layer3MasteringRing = seed.CommonDiscInfo.Layer3MasteringRing;
                info.CommonDiscInfo.Layer3MasteringSID = seed.CommonDiscInfo.Layer3MasteringSID;
                info.CommonDiscInfo.Layer3ToolstampMasteringCode = seed.CommonDiscInfo.Layer3ToolstampMasteringCode;
            }

            if (info.VersionAndEditions != null && seed.VersionAndEditions != null)
            {
                // Info that only overwrites if supplied
                if (!string.IsNullOrWhiteSpace(seed.VersionAndEditions.Version)) info.VersionAndEditions.Version = seed.VersionAndEditions.Version;
                if (!string.IsNullOrWhiteSpace(seed.VersionAndEditions.OtherEditions)) info.VersionAndEditions.OtherEditions = seed.VersionAndEditions.OtherEditions;
            }

            if (info.CopyProtection != null && seed.CopyProtection != null)
            {
                // Info that only overwrites if supplied
                if (!string.IsNullOrWhiteSpace(seed.CopyProtection.Protection)) info.CopyProtection.Protection = seed.CopyProtection.Protection;
            }
        }

        /// <summary>
        /// Validate the current environment is ready for a dump
        /// </summary>
        /// <returns>Result instance with the outcome</returns>
        private Result IsValidForDump()
        {
            // Validate that everything is good
            if (Parameters == null || !ParametersValid())
                return Result.Failure("Error! Current configuration is not supported!");

            // Fix the output paths, just in case
            OutputPath = InfoTool.NormalizeOutputPaths(OutputPath, true);

            // Validate that the output path isn't on the dumping drive
            if (Drive?.Name != null && OutputPath.StartsWith(Drive.Name))
                return Result.Failure("Error! Cannot output to same drive that is being dumped!");

            // Validate that the required program exists
            if (!File.Exists(Parameters.ExecutablePath))
                return Result.Failure($"Error! {Parameters.ExecutablePath} does not exist!");

            // Validate that the dumping drive doesn't contain the executable
            string fullExecutablePath = Path.GetFullPath(Parameters.ExecutablePath);
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
#if NET48
        private async Task<string> RunStandaloneDiscImageCreatorCommand(string command)
#else
        private async Task<string?> RunStandaloneDiscImageCreatorCommand(string command)
#endif
        {
            // Validate that DiscImageCreator is all set
            if (!RequiredProgramsExist())
                return null;

            // Validate we're not trying to eject a non-optical
            if (Drive == null || Drive.InternalDriveType != InternalDriveType.Optical)
                return null;

            CancelDumping();

            var parameters = new Modules.DiscImageCreator.Parameters(string.Empty)
            {
                BaseCommand = command,
                DrivePath = Drive.Name,
                ExecutablePath = Options.DiscImageCreatorPath,
            };

            return await ExecuteInternalProgram(parameters);
        }

        #endregion
    }
}
