using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BurnOutSharp;
using MPF.Data;
using MPF.Redump;
using Newtonsoft.Json;

namespace MPF.Utilities
{
    /// <summary>
    /// Represents the state of all settings to be used during dumping
    /// </summary>
    public class DumpEnvironment
    {
        #region Output paths

        /// <summary>
        /// Base output directory to write files to
        /// </summary>
        public string OutputDirectory { get; private set; }

        /// <summary>
        /// Base output filename for output
        /// </summary>
        public string OutputFilename { get; private set; }

        #endregion

        #region UI information

        /// <summary>
        /// Drive object representing the current drive
        /// </summary>
        public Drive Drive { get; private set; }

        /// <summary>
        /// Currently selected system
        /// </summary>
        public KnownSystem? System { get; private set; }

        /// <summary>
        /// Currently selected media type
        /// </summary>
        public MediaType? Type { get; private set; }

        /// <summary>
        /// Options object representing user-defined options
        /// </summary>
        public Options Options { get; private set; }

        /// <summary>
        /// Parameters object representing what to send to the internal program
        /// </summary>
        public BaseParameters Parameters { get; private set; }

        #endregion
        
        #region Event Handlers

        /// <summary>
        /// Geneeic way of reporting a message
        /// </summary>
        /// <param name="message">String value to report</param>
        public EventHandler<string> ReportStatus;

        /// <summary>
        /// Event handler for data returned from a process
        /// </summary>
        private void OutputToLog(object proc, string args)
        {
            ReportStatus.Invoke(this, args);
        }

        #endregion

        /// <summary>
        /// Constructor for a full DumpEnvironment object from user information
        /// </summary>
        /// <param name="options"></param>
        /// <param name="outputDirectory"></param>
        /// <param name="outputFilename"></param>
        /// <param name="drive"></param>
        /// <param name="system"></param>
        /// <param name="type"></param>
        /// <param name="parameters"></param>
        public DumpEnvironment(Options options,
            string outputDirectory,
            string outputFilename,
            Drive drive,
            KnownSystem? system,
            MediaType? type,
            string parameters)
        {
            // Set options object
            this.Options = options;

            // Output paths
            (this.OutputDirectory, this.OutputFilename) = NormalizeOutputPaths(outputDirectory, outputFilename);

            // UI information
            this.Drive = drive;
            this.System = system ?? options.DefaultSystem;
            this.Type = type ?? MediaType.NONE;
            
            // Dumping program
            SetParameters(parameters);
        }

        #region Public Functionality

        /// <summary>
        /// Set the parameters object based on the internal program and parameters string
        /// </summary>
        /// <param name="parameters">String representation of the parameters</param>
        public void SetParameters(string parameters)
        {
            switch (Options.InternalProgram)
            {
                // Dumping support
                case InternalProgram.Aaru:
                    this.Parameters = new Aaru.Parameters(parameters);
                    this.Parameters.ExecutablePath = Options.AaruPath;
                    break;

                case InternalProgram.DD:
                    this.Parameters = new DD.Parameters(parameters);
                    this.Parameters.ExecutablePath = Options.DDPath;
                    break;

                case InternalProgram.DiscImageCreator:
                    this.Parameters = new DiscImageCreator.Parameters(parameters);
                    this.Parameters.ExecutablePath = Options.DiscImageCreatorPath;
                    break;

                // Verification support only
                case InternalProgram.CleanRip:
                    this.Parameters = new CleanRip.Parameters(parameters);
                    this.Parameters.ExecutablePath = null;
                    break;

                case InternalProgram.DCDumper:
                    this.Parameters = null; // TODO: Create correct parameter type when supported
                    this.Parameters.ExecutablePath = null;
                    break;

                case InternalProgram.UmdImageCreator:
                    this.Parameters = new UmdImageCreator.Parameters(parameters);
                    this.Parameters.ExecutablePath = null;
                    break;

                // This should never happen, but it needs a fallback
                default:
                    this.Parameters = new DiscImageCreator.Parameters(parameters);
                    this.Parameters.ExecutablePath = Options.DiscImageCreatorPath;
                    break;
            }

            // Set system and type
            this.Parameters.System = this.System;
            this.Parameters.Type = this.Type;
        }

        /// <summary>
        /// Ensures that all required output files have been created
        /// </summary>
        /// <param name="progress">Optional result progress callback</param>
        /// <returns>Tuple of true if all required files exist, false otherwise and a list representing missing files</returns>
        public (bool, List<string>) FoundAllFiles()
        {
            // First, sanitized the output filename to strip off any potential extension
            string outputFilename = Path.GetFileNameWithoutExtension(OutputFilename);

            // Then get the base path for all checking
            string basePath = Path.Combine(OutputDirectory, outputFilename);

            // Finally, let the parameters say if all files exist
            return Parameters.CheckAllOutputFilesExist(basePath);
        }

        /// <summary>
        /// Get the full parameter string for either DiscImageCreator or Aaru
        /// </summary>
        /// <param name="driveSpeed">Nullable int representing the drive speed</param>
        /// <returns>String representing the params, null on error</returns>
        public string GetFullParameters(int? driveSpeed)
        {
            // Populate with the correct params for inputs (if we're not on the default option)
            if (System != KnownSystem.NONE && Type != MediaType.NONE)
            {
                // If drive letter is invalid, skip this
                if (Drive == null)
                    return null;

                // Set the proper parameters
                string filename = OutputDirectory + Path.DirectorySeparatorChar + OutputFilename;
                switch (Options.InternalProgram)
                {
                    case InternalProgram.Aaru:
                        Parameters = new Aaru.Parameters(System, Type, Drive.Letter, filename, driveSpeed, Options);
                        break;

                    case InternalProgram.DD:
                        Parameters = new DD.Parameters(System, Type, Drive.Letter, filename, driveSpeed, Options);
                        break;

                    case InternalProgram.DiscImageCreator:
                        Parameters = new DiscImageCreator.Parameters(System, Type, Drive.Letter, filename, driveSpeed, Options);
                        break;

                    // This should never happen, but it needs a fallback
                    default:
                        Parameters = new DiscImageCreator.Parameters(System, Type, Drive.Letter, filename, driveSpeed, Options);
                        break;
                }

                // Generate and return the param string
                return Parameters.GenerateParameters();
            }

            return null;
        }

        /// <summary>
        /// Normalize a split set of paths
        /// </summary>
        /// <param name="directory">Directory name to normalize</param>
        /// <param name="filename">Filename to normalize</param>
        public static (string, string) NormalizeOutputPaths(string directory, string filename)
        {
            try
            {
                // Cache if we had a directory separator or not
                bool endedWithDirectorySeparator = directory.EndsWith(Path.DirectorySeparatorChar.ToString())
                    || directory.EndsWith(Path.AltDirectorySeparatorChar.ToString());
                bool endedWithSpace = directory.EndsWith(" ");

                // Combine the path to make things separate easier
                string combinedPath = Path.Combine(directory, filename);

                // If we have have a blank path, just return
                if (string.IsNullOrWhiteSpace(combinedPath))
                    return (directory, filename);

                // Now get the normalized paths
                directory = Path.GetDirectoryName(combinedPath);
                filename = Path.GetFileName(combinedPath);

                // Take care of extra path characters
                directory = new StringBuilder(directory)
                    .Replace(':', '_', 0, directory.LastIndexOf(':') == -1 ? 0 : directory.LastIndexOf(':')).ToString();

                // Sanitize everything else
                foreach (char c in Path.GetInvalidPathChars())
                    directory = directory.Replace(c, '_');
                foreach (char c in Path.GetInvalidFileNameChars())
                    filename = filename.Replace(c, '_');

                // If we had a space at the end before, add it again
                if (endedWithSpace)
                    directory += " ";

                // If we had a directory separator at the end before, add it again
                if (endedWithDirectorySeparator)
                    directory += Path.DirectorySeparatorChar;

                // If we have a root directory, sanitize
                if (Directory.Exists(directory))
                {
                    var possibleRootDir = new DirectoryInfo(directory);
                    if (possibleRootDir.Parent == null)
                        directory = directory.Replace($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}", $"{Path.DirectorySeparatorChar}");
                }
            }
            catch { }

            return (directory, filename);
        }

        #endregion

        #region Dumping

        /// <summary>
        /// Cancel an in-progress dumping process
        /// </summary>
        public void CancelDumping()
        {
            Parameters.KillInternalProgram();
        }

        /// <summary>
        /// Eject the disc using DiscImageCreator
        /// </summary>
        public async void EjectDisc()
        {
            // Validate that the path is configured
            if (string.IsNullOrWhiteSpace(Options.DiscImageCreatorPath))
                return;

            // Validate that the required program exists
            if (!File.Exists(Options.DiscImageCreatorPath))
                return;

            CancelDumping();

            // Validate we're not trying to eject a non-optical
            if (Drive.InternalDriveType != InternalDriveType.Optical)
                return;

            var parameters = new DiscImageCreator.Parameters(string.Empty)
            {
                BaseCommand = DiscImageCreator.Command.Eject,
                DriveLetter = Drive.Letter.ToString(),
                ExecutablePath = Options.DiscImageCreatorPath,
            };

            await ExecuteInternalProgram(parameters);
        }

        /// <summary>
        /// Reset the current drive using DiscImageCreator
        /// </summary>
        public async void ResetDrive()
        {
            // Validate that the path is configured
            if (string.IsNullOrWhiteSpace(Options.DiscImageCreatorPath))
                return;

            // Validate that the required program exists
            if (!File.Exists(Options.DiscImageCreatorPath))
                return;

            // Precautionary check for dumping, just in case
            CancelDumping();

            // Validate we're not trying to reset a non-optical
            if (Drive.InternalDriveType != InternalDriveType.Optical)
                return;

            DiscImageCreator.Parameters parameters = new DiscImageCreator.Parameters(string.Empty)
            {
                BaseCommand = DiscImageCreator.Command.Reset,
                DriveLetter = Drive.Letter.ToString(),
                ExecutablePath = Options.DiscImageCreatorPath,
            };

            await ExecuteInternalProgram(parameters);
        }

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

            // Execute internal tool
            progress?.Report(Result.Success($"Executing {Options.InternalProgram}... {(Options.ToolsInSeparateWindow ? "please wait!" : "see log for output!")}"));
            Directory.CreateDirectory(OutputDirectory);
            Parameters.ReportStatus += OutputToLog;
            await Task.Run(() => Parameters.ExecuteInternalProgram(Options.ToolsInSeparateWindow));
            progress?.Report(Result.Success($"{Options.InternalProgram} has finished!"));

            // Execute additional tools
            progress?.Report(Result.Success("Running any additional tools... see log for output!"));
            result = await Task.Run(() => ExecuteAdditionalTools());
            progress?.Report(result);

            return result;
        }

        /// <summary>
        /// Verify that the current environment has a complete dump and create submission info is possible
        /// </summary>
        /// <param name="resultProgress">Optional result progress callback</param>
        /// <param name="protectionProgress">Optional protection progress callback</param>
        /// <param name="showUserPrompt">Optional user prompt to deal with submsision information</param>
        /// <returns>Result instance with the outcome</returns>
        public async Task<Result> VerifyAndSaveDumpOutput(
            IProgress<Result> resultProgress = null,
            IProgress<ProtectionProgress> protectionProgress = null,
            Func<SubmissionInfo, bool?> showUserPrompt = null)
        {
            resultProgress?.Report(Result.Success("Gathering submission information... please wait!"));

            // Check to make sure that the output had all the correct files
            (bool foundFiles, List<string> missingFiles) = FoundAllFiles();
            if (!foundFiles)
            {
                resultProgress.Report(Result.Failure($"There were files missing from the output:\n{string.Join("\n", missingFiles)}"));
                return Result.Failure("Error! Please check output directory as dump may be incomplete!");
            }

            // Extract the information from the output files
            resultProgress?.Report(Result.Success("Extracting output information from output files..."));
            SubmissionInfo submissionInfo = await ExtractOutputInformation(resultProgress, protectionProgress);
            resultProgress?.Report(Result.Success("Extracting information complete!"));

            // Eject the disc automatically if confugured to
            if (Options.EjectAfterDump == true)
            {
                resultProgress?.Report(Result.Success($"Ejecting disc in drive {Drive.Letter}"));
                EjectDisc();
            }

            // Reset the drive automatically if confugured to
            if (Options.InternalProgram == InternalProgram.DiscImageCreator && Options.DICResetDriveAfterDump)
            {
                resultProgress?.Report(Result.Success($"Resetting drive {Drive.Letter}"));
                ResetDrive();
            }

            // Get user-modifyable information if confugured to
            if (Options.PromptForDiscInformation && showUserPrompt != null)
            {
                resultProgress?.Report(Result.Success("Waiting for additional disc information..."));
                bool? filledInfo = showUserPrompt(submissionInfo);
                if (filledInfo == true)
                    resultProgress?.Report(Result.Success("Additional disc information added!"));
                else
                    resultProgress?.Report(Result.Success("Disc information skipped!"));
            }

            // Format the information for the text output
            resultProgress?.Report(Result.Success("Formatting information..."));
            List<string> formattedValues = FormatOutputData(submissionInfo);
            resultProgress?.Report(Result.Success("Formatting complete!"));

            // Write the text output
            resultProgress?.Report(Result.Success("Writing information to !submissionInfo.txt..."));
            bool success = WriteOutputData(formattedValues);
            if (success)
                resultProgress?.Report(Result.Success("Writing complete!"));
            else
                resultProgress?.Report(Result.Failure("Writing could not complete!"));

            // Write the JSON output
            resultProgress?.Report(Result.Success("Writing information to !submissionInfo.json.gz..."));
            success = WriteOutputData(submissionInfo);
            if (success)
                resultProgress?.Report(Result.Success("Writing complete!"));
            else
                resultProgress?.Report(Result.Failure("Writing could not complete!"));

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
        private Result ExecuteAdditionalTools()
        {
            return Result.Success("No external tools needed!");
        }

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
        /// Extract all of the possible information from a given input combination
        /// </summary>
        /// <param name="resultProgress">Optional result progress callback</param>
        /// <param name="protectionProgress">Optional protection progress callback</param>
        /// <returns>SubmissionInfo populated based on outputs, null on error</returns>
        private async Task<SubmissionInfo> ExtractOutputInformation(
            IProgress<Result> resultProgress = null,
            IProgress<ProtectionProgress> protectionProgress = null)
        {
            // Ensure the current disc combination should exist
            if (!Validators.GetValidMediaTypes(System).Contains(Type))
                return null;

            // Sanitize the output filename to strip off any potential extension
            string outputFilename = Path.GetFileNameWithoutExtension(OutputFilename);

            // Check that all of the relevant files are there
            (bool foundFiles, List<string> missingFiles) = FoundAllFiles();
            if (!foundFiles)
            {
                resultProgress.Report(Result.Failure($"There were files missing from the output:\n{string.Join("\n", missingFiles)}"));
                return null;
            }

            // Create the SubmissionInfo object with all user-inputted values by default
            string combinedBase = Path.Combine(OutputDirectory, outputFilename);
            SubmissionInfo info = new SubmissionInfo()
            {
                CommonDiscInfo = new CommonDiscInfoSection()
                {
                    System = this.System,
                    Media = this.Type,
                    Title = (Options.AddPlaceholders ? Template.RequiredValue : ""),
                    ForeignTitleNonLatin = (Options.AddPlaceholders ? Template.OptionalValue : ""),
                    DiscNumberLetter = (Options.AddPlaceholders ? Template.OptionalValue : ""),
                    DiscTitle = (Options.AddPlaceholders ? Template.OptionalValue : ""),
                    Category = null,
                    Region = null,
                    Languages = null,
                    Serial = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : ""),
                    Barcode = (Options.AddPlaceholders ? Template.OptionalValue : ""),
                    Contents = (Options.AddPlaceholders ? Template.OptionalValue : ""),
                },
                VersionAndEditions = new VersionAndEditionsSection()
                {
                    Version = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : ""),
                    OtherEditions = (Options.AddPlaceholders ? "Original (VERIFY THIS)" : ""),
                },
                TracksAndWriteOffsets = new TracksAndWriteOffsetsSection(),
            };

            // Get specific tool output handling
            Parameters.GenerateSubmissionInfo(info, combinedBase, this.Drive);

            // Get a list of matching IDs for each line in the DAT
            if (!string.IsNullOrEmpty(info.TracksAndWriteOffsets.ClrMameProData) && Options.HasRedumpLogin)
            {
                // Set the current dumper based on username
                info.DumpersAndStatus.Dumpers = new string[] { Options.RedumpUsername };

                info.MatchedIDs = new List<int>();
                using (RedumpWebClient wc = new RedumpWebClient())
                {
                    // Login to Redump
                    bool? loggedIn = wc.Login(Options.RedumpUsername, Options.RedumpPassword);
                    if (loggedIn == null)
                    {
                        resultProgress?.Report(Result.Failure("There was an unknown error connecting to Redump"));
                    }
                    else if (loggedIn == true)
                    {
                        // Loop through all of the hashdata to find matching IDs
                        resultProgress?.Report(Result.Success("Finding disc matches on Redump..."));
                        string[] splitData = info.TracksAndWriteOffsets.ClrMameProData.Split('\n');
                        foreach (string hashData in splitData)
                        {
                            if (GetISOHashValues(hashData, out long _, out string _, out string _, out string sha1))
                            {
                                // Get all matching IDs for the track
                                List<int> newIds = wc.ListSearchResults(sha1);

                                // If we got null back, there was an error
                                if (newIds == null)
                                {
                                    resultProgress?.Report(Result.Failure("There was an unknown error retrieving information from Redump"));
                                    break;
                                }

                                // If no IDs match any track, then we don't match a disc at all
                                if (!newIds.Any())
                                {
                                    info.MatchedIDs = new List<int>();
                                    break;
                                }

                                // If we have multiple tracks, only take IDs that are in common
                                if (info.MatchedIDs.Any())
                                    info.MatchedIDs = info.MatchedIDs.Intersect(newIds).ToList();

                                // If we're on the first track, all IDs are added
                                else
                                    info.MatchedIDs = newIds;
                            }
                        }

                        resultProgress?.Report(Result.Success("Match finding complete! " + (info.MatchedIDs.Count > 0 ? "Matched IDs: " + string.Join(",", info.MatchedIDs) : "No matches found")));

                        // If we have exactly 1 ID, we can grab a bunch of info from it
                        if (info.MatchedIDs.Count == 1)
                        {
                            resultProgress?.Report(Result.Success($"Filling fields from existing ID {info.MatchedIDs[0]}..."));
                            wc.FillFromId(info, info.MatchedIDs[0]);
                            resultProgress?.Report(Result.Success("Information filling complete!"));
                        }
                    }
                }
            }

            // If we have both ClrMamePro and Size and Checksums data, remove the ClrMamePro
            if (!string.IsNullOrWhiteSpace(info.SizeAndChecksums.CRC32))
                info.TracksAndWriteOffsets.ClrMameProData = null;

            // Extract info based generically on MediaType
            switch (Type)
            {
                case MediaType.CDROM:
                case MediaType.GDROM: // TODO: Verify GD-ROM outputs this
                    info.CommonDiscInfo.Layer0MasteringRing = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    info.CommonDiscInfo.Layer0MasteringSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    info.CommonDiscInfo.Layer0ToolstampMasteringCode = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    info.CommonDiscInfo.Layer0MouldSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    info.CommonDiscInfo.Layer1MouldSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    info.CommonDiscInfo.Layer0AdditionalMould = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    break;

                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                    // If we have a single-layer disc
                    if (info.SizeAndChecksums.Layerbreak == default)
                    {
                        info.CommonDiscInfo.Layer0MasteringRing = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer0MasteringSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer0MouldSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer1MouldSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer0AdditionalMould = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    }
                    // If we have a dual-layer disc
                    else
                    {
                        info.CommonDiscInfo.Layer0MasteringRing = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer0MasteringSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer0MouldSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer0AdditionalMould = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");

                        info.CommonDiscInfo.Layer1MasteringRing = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer1MasteringSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer1ToolstampMasteringCode = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer1MouldSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    }

                    break;

                case MediaType.NintendoGameCubeGameDisc:
                    info.CommonDiscInfo.Layer0MasteringRing = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    info.CommonDiscInfo.Layer0MasteringSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    info.CommonDiscInfo.Layer0ToolstampMasteringCode = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    info.CommonDiscInfo.Layer0MouldSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    info.CommonDiscInfo.Layer1MouldSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    info.CommonDiscInfo.Layer0AdditionalMould = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    info.Extras.BCA = info.Extras.BCA ?? (Options.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case MediaType.NintendoWiiOpticalDisc:
                    // If we have a single-layer disc
                    if (info.SizeAndChecksums.Layerbreak == default)
                    {
                        info.CommonDiscInfo.Layer0MasteringRing = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer0MasteringSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer0MouldSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer1MouldSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer0AdditionalMould = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    }
                    // If we have a dual-layer disc
                    else
                    {
                        info.CommonDiscInfo.Layer0MasteringRing = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer0MasteringSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer0MouldSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer0AdditionalMould = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");

                        info.CommonDiscInfo.Layer1MasteringRing = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer1MasteringSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer1ToolstampMasteringCode = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer1MouldSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    }

                    info.Extras.DiscKey = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    info.Extras.BCA = info.Extras.BCA ?? (Options.AddPlaceholders ? Template.RequiredValue : "");

                    break;

                case MediaType.UMD:
                    // If we have a single-layer disc
                    if (info.SizeAndChecksums.Layerbreak == default)
                    {
                        info.CommonDiscInfo.Layer0MasteringRing = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer0MasteringSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer0MouldSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    }
                    // If we have a dual-layer disc
                    else
                    {
                        info.CommonDiscInfo.Layer0MasteringRing = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer0MasteringSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer0MouldSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");

                        info.CommonDiscInfo.Layer1MasteringRing = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer1MasteringSID = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.Layer1ToolstampMasteringCode = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    }

                    info.SizeAndChecksums.CRC32 = info.SizeAndChecksums.CRC32 == null ? (Options.AddPlaceholders ? Template.RequiredValue + " [Not automatically generated for UMD]" : "") : info.SizeAndChecksums.CRC32;
                    info.SizeAndChecksums.MD5 = info.SizeAndChecksums.MD5 == null ? (Options.AddPlaceholders ? Template.RequiredValue + " [Not automatically generated for UMD]" : "") : info.SizeAndChecksums.MD5;
                    info.SizeAndChecksums.SHA1 = info.SizeAndChecksums.SHA1 == null ? (Options.AddPlaceholders ? Template.RequiredValue + " [Not automatically generated for UMD]" : "") : info.SizeAndChecksums.SHA1;
                    info.TracksAndWriteOffsets.ClrMameProData = null;
                    break;
            }

            // Extract info based specifically on KnownSystem
            switch (System)
            {
                case KnownSystem.AcornArchimedes:
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? RedumpRegion.UK;
                    break;

                case KnownSystem.AppleMacintosh:
                case KnownSystem.EnhancedCD:
                case KnownSystem.IBMPCCompatible:
                case KnownSystem.RainbowDisc:
                    if (string.IsNullOrWhiteSpace(info.CommonDiscInfo.Comments))
                        info.CommonDiscInfo.Comments += $"[T:ISBN] {(Options.AddPlaceholders ? Template.OptionalValue : "")}";

                    resultProgress?.Report(Result.Success("Running copy protection scan... this might take a while!"));
                    info.CopyProtection.Protection = await GetCopyProtection(protectionProgress);
                    resultProgress?.Report(Result.Success("Copy protection scan complete!"));

                    break;

                case KnownSystem.AudioCD:
                case KnownSystem.DVDAudio:
                case KnownSystem.SuperAudioCD:
                    info.CommonDiscInfo.Category = info.CommonDiscInfo.Category ?? RedumpDiscCategory.Audio;
                    break;

                case KnownSystem.BandaiPlaydiaQuickInteractiveSystem:
                    info.CommonDiscInfo.EXEDateBuildDate = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? RedumpRegion.Japan;
                    break;

                case KnownSystem.BDVideo:
                    info.CommonDiscInfo.Category = info.CommonDiscInfo.Category ?? RedumpDiscCategory.BonusDiscs;
                    info.CopyProtection.Protection = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    break;

                case KnownSystem.CommodoreAmiga:
                    info.CommonDiscInfo.EXEDateBuildDate = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.CommodoreAmigaCD32:
                    info.CommonDiscInfo.EXEDateBuildDate = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? RedumpRegion.Europe;
                    break;

                case KnownSystem.CommodoreAmigaCDTV:
                    info.CommonDiscInfo.EXEDateBuildDate = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? RedumpRegion.Europe;
                    break;

                case KnownSystem.DVDVideo:
                    info.CommonDiscInfo.Category = info.CommonDiscInfo.Category ?? RedumpDiscCategory.BonusDiscs;
                    break;

                case KnownSystem.FujitsuFMTowns:
                    info.CommonDiscInfo.EXEDateBuildDate = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? RedumpRegion.Japan;
                    break;

                case KnownSystem.FujitsuFMTownsMarty:
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? RedumpRegion.Japan;
                    break;

                case KnownSystem.IncredibleTechnologiesEagle:
                    info.CommonDiscInfo.EXEDateBuildDate = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.KonamieAmusement:
                    info.CommonDiscInfo.EXEDateBuildDate = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.KonamiFirebeat:
                    info.CommonDiscInfo.EXEDateBuildDate = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.KonamiGVSystem:
                    info.CommonDiscInfo.EXEDateBuildDate = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.KonamiSystem573:
                    info.CommonDiscInfo.EXEDateBuildDate = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.KonamiTwinkle:
                    info.CommonDiscInfo.EXEDateBuildDate = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.MattelHyperscan:
                    info.CommonDiscInfo.EXEDateBuildDate = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.NamcoSegaNintendoTriforce:
                    info.CommonDiscInfo.EXEDateBuildDate = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.NavisoftNaviken21:
                    info.CommonDiscInfo.EXEDateBuildDate = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? RedumpRegion.Japan;
                    break;

                case KnownSystem.NECPC88:
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? RedumpRegion.Japan;
                    break;

                case KnownSystem.NECPC98:
                    info.CommonDiscInfo.EXEDateBuildDate = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? RedumpRegion.Japan;
                    break;

                case KnownSystem.NECPCFX:
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? RedumpRegion.Japan;
                    break;

                case KnownSystem.SegaChihiro:
                    info.CommonDiscInfo.EXEDateBuildDate = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.SegaDreamcast:
                    info.CommonDiscInfo.EXEDateBuildDate = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.SegaNaomi:
                    info.CommonDiscInfo.EXEDateBuildDate = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.SegaNaomi2:
                    info.CommonDiscInfo.EXEDateBuildDate = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.SegaTitanVideo:
                    info.CommonDiscInfo.EXEDateBuildDate = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.SharpX68000:
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? RedumpRegion.Japan;
                    break;

                case KnownSystem.SNKNeoGeoCD:
                    info.CommonDiscInfo.EXEDateBuildDate = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.SonyPlayStation2:
                    info.CommonDiscInfo.LanguageSelection = new RedumpLanguageSelection?[] { RedumpLanguageSelection.BiosSettings, RedumpLanguageSelection.LanguageSelector, RedumpLanguageSelection.OptionsMenu };
                    break;

                case KnownSystem.SonyPlayStation3:
                    info.Extras.DiscKey = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    info.Extras.DiscID = (Options.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.TomyKissSite:
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? RedumpRegion.Japan;
                    break;

                case KnownSystem.ZAPiTGamesGameWaveFamilyEntertainmentSystem:
                    info.CopyProtection.Protection = (Options.AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    break;
            }

            // Set the category if it's not overriden
            info.CommonDiscInfo.Category = info.CommonDiscInfo.Category ?? RedumpDiscCategory.Games;

            // Comments is one of the few fields with odd handling
            if (string.IsNullOrEmpty(info.CommonDiscInfo.Comments))
                info.CommonDiscInfo.Comments = (Options.AddPlaceholders ? Template.OptionalValue : "");

            return info;
        }

        /// <summary>
        /// Format the output data in a human readable way, separating each printed line into a new item in the list
        /// </summary>
        /// <param name="info">Information object that should contain normalized values</param>
        /// <returns>List of strings representing each line of an output file, null on error</returns>
        private List<string> FormatOutputData(SubmissionInfo info)
        {
            // Check to see if the inputs are valid
            if (info == null)
                return null;

            try
            {
                // Common Disc Info section
                List<string> output = new List<string> { "Common Disc Info:" };
                AddIfExists(output, Template.TitleField, info.CommonDiscInfo.Title, 1);
                AddIfExists(output, Template.ForeignTitleField, info.CommonDiscInfo.ForeignTitleNonLatin, 1);
                AddIfExists(output, Template.DiscNumberField, info.CommonDiscInfo.DiscNumberLetter, 1);
                AddIfExists(output, Template.DiscTitleField, info.CommonDiscInfo.DiscTitle, 1);
                AddIfExists(output, Template.SystemField, info.CommonDiscInfo.System.LongName(), 1);
                AddIfExists(output, Template.MediaTypeField, GetFixedMediaType(
                        info.CommonDiscInfo.Media,
                        info.SizeAndChecksums.Layerbreak,
                        info.SizeAndChecksums.Layerbreak2,
                        info.SizeAndChecksums.Layerbreak3),
                    1);
                AddIfExists(output, Template.CategoryField, info.CommonDiscInfo.Category.LongName(), 1);
                AddIfExists(output, Template.MatchingIDsField, info.MatchedIDs, 1);
                AddIfExists(output, Template.RegionField, info.CommonDiscInfo.Region.LongName(), 1);
                AddIfExists(output, Template.LanguagesField, (info.CommonDiscInfo.Languages ?? new RedumpLanguage?[] { null }).Select(l => l.LongName()).ToArray(), 1);
                AddIfExists(output, Template.PlaystationLanguageSelectionViaField, (info.CommonDiscInfo.LanguageSelection ?? new RedumpLanguageSelection?[] { }).Select(l => l.ToString()).ToArray(), 1);
                AddIfExists(output, Template.DiscSerialField, info.CommonDiscInfo.Serial, 1);

                // All ringcode information goes in an indented area
                output.Add(""); output.Add("\tRingcode Information:");

                // If we have a triple-layer disc
                if (info.SizeAndChecksums.Layerbreak3 != default)
                {
                    AddIfExists(output, "Layer 0 (Inner) " + Template.MasteringRingField, info.CommonDiscInfo.Layer0MasteringRing, 2);
                    AddIfExists(output, "Layer 0 (Inner) " + Template.MasteringSIDField, info.CommonDiscInfo.Layer0MasteringSID, 2);
                    AddIfExists(output, "Layer 0 (Inner) " + Template.ToolstampField, info.CommonDiscInfo.Layer0ToolstampMasteringCode, 2);
                    AddIfExists(output, "Data Side " + Template.MouldSIDField, info.CommonDiscInfo.Layer0MouldSID, 2);
                    AddIfExists(output, "Data Side " + Template.AdditionalMouldField, info.CommonDiscInfo.Layer0AdditionalMould, 2);

                    AddIfExists(output, "Layer 1 " + Template.MasteringRingField, info.CommonDiscInfo.Layer1MasteringRing, 2);
                    AddIfExists(output, "Layer 1 " + Template.MasteringSIDField, info.CommonDiscInfo.Layer1MasteringSID, 2);
                    AddIfExists(output, "Layer 1 " + Template.ToolstampField, info.CommonDiscInfo.Layer1ToolstampMasteringCode, 2);
                    AddIfExists(output, "Label Side " + Template.MouldSIDField, info.CommonDiscInfo.Layer1MouldSID, 2);
                    AddIfExists(output, "Label Side " + Template.AdditionalMouldField, info.CommonDiscInfo.Layer1AdditionalMould, 2);

                    AddIfExists(output, "Layer 2 " + Template.MasteringRingField, info.CommonDiscInfo.Layer2MasteringRing, 2);
                    AddIfExists(output, "Layer 2 " + Template.MasteringSIDField, info.CommonDiscInfo.Layer2MasteringSID, 2);
                    AddIfExists(output, "Layer 2 " + Template.ToolstampField, info.CommonDiscInfo.Layer2ToolstampMasteringCode, 2);

                    AddIfExists(output, "Layer 3 (Outer) " + Template.MasteringRingField, info.CommonDiscInfo.Layer3MasteringRing, 2);
                    AddIfExists(output, "Layer 3 (Outer) " + Template.MasteringSIDField, info.CommonDiscInfo.Layer3MasteringSID, 2);
                    AddIfExists(output, "Layer 3 (Outer) " + Template.ToolstampField, info.CommonDiscInfo.Layer3ToolstampMasteringCode, 2);
                }
                // If we have a triple-layer disc
                else if (info.SizeAndChecksums.Layerbreak2 != default)
                {
                    AddIfExists(output, "Layer 0 (Inner) " + Template.MasteringRingField, info.CommonDiscInfo.Layer0MasteringRing, 2);
                    AddIfExists(output, "Layer 0 (Inner) " + Template.MasteringSIDField, info.CommonDiscInfo.Layer0MasteringSID, 2);
                    AddIfExists(output, "Layer 0 (Inner) " + Template.ToolstampField, info.CommonDiscInfo.Layer0ToolstampMasteringCode, 2);
                    AddIfExists(output, "Data Side " + Template.MouldSIDField, info.CommonDiscInfo.Layer0MouldSID, 2);
                    AddIfExists(output, "Data Side " + Template.AdditionalMouldField, info.CommonDiscInfo.Layer0AdditionalMould, 2);

                    AddIfExists(output, "Layer 1 " + Template.MasteringRingField, info.CommonDiscInfo.Layer1MasteringRing, 2);
                    AddIfExists(output, "Layer 1 " + Template.MasteringSIDField, info.CommonDiscInfo.Layer1MasteringSID, 2);
                    AddIfExists(output, "Layer 1 " + Template.ToolstampField, info.CommonDiscInfo.Layer1ToolstampMasteringCode, 2);
                    AddIfExists(output, "Label Side " + Template.MouldSIDField, info.CommonDiscInfo.Layer1MouldSID, 2);
                    AddIfExists(output, "Label Side " + Template.AdditionalMouldField, info.CommonDiscInfo.Layer1AdditionalMould, 2);

                    AddIfExists(output, "Layer 2 (Outer) " + Template.MasteringRingField, info.CommonDiscInfo.Layer2MasteringRing, 2);
                    AddIfExists(output, "Layer 2 (Outer) " + Template.MasteringSIDField, info.CommonDiscInfo.Layer2MasteringSID, 2);
                    AddIfExists(output, "Layer 2 (Outer) " + Template.ToolstampField, info.CommonDiscInfo.Layer2ToolstampMasteringCode, 2);
                }
                // If we have a dual-layer disc
                else if (info.SizeAndChecksums.Layerbreak != default)
                {
                    AddIfExists(output, "Layer 0 (Inner) " + Template.MasteringRingField, info.CommonDiscInfo.Layer0MasteringRing, 2);
                    AddIfExists(output, "Layer 0 (Inner) " + Template.MasteringSIDField, info.CommonDiscInfo.Layer0MasteringSID, 2);
                    AddIfExists(output, "Layer 0 (Inner) " + Template.ToolstampField, info.CommonDiscInfo.Layer0ToolstampMasteringCode, 2);
                    AddIfExists(output, "Data Side " + Template.MouldSIDField, info.CommonDiscInfo.Layer0MouldSID, 2);
                    AddIfExists(output, "Data Side " + Template.AdditionalMouldField, info.CommonDiscInfo.Layer0AdditionalMould, 2);

                    AddIfExists(output, "Layer 1 (Outer) " + Template.MasteringRingField, info.CommonDiscInfo.Layer1MasteringRing, 2);
                    AddIfExists(output, "Layer 1 (Outer) " + Template.MasteringSIDField, info.CommonDiscInfo.Layer1MasteringSID, 2);
                    AddIfExists(output, "Layer 1 (Outer) " + Template.ToolstampField, info.CommonDiscInfo.Layer1ToolstampMasteringCode, 2);
                    AddIfExists(output, "Label Side " + Template.MouldSIDField, info.CommonDiscInfo.Layer1MouldSID, 2);
                    AddIfExists(output, "Label Side " + Template.AdditionalMouldField, info.CommonDiscInfo.Layer1AdditionalMould, 2);
                }
                // If we have a single-layer disc
                else
                {
                    AddIfExists(output, "Data Side " + Template.MasteringRingField, info.CommonDiscInfo.Layer0MasteringRing, 2);
                    AddIfExists(output, "Data Side " + Template.MasteringSIDField, info.CommonDiscInfo.Layer0MasteringSID, 2);
                    AddIfExists(output, "Data Side " + Template.ToolstampField, info.CommonDiscInfo.Layer0ToolstampMasteringCode, 2);
                    AddIfExists(output, "Data Side " + Template.MouldSIDField, info.CommonDiscInfo.Layer0MouldSID, 2);
                    AddIfExists(output, "Data Side " + Template.AdditionalMouldField, info.CommonDiscInfo.Layer0AdditionalMould, 2);

                    AddIfExists(output, "Label Side " + Template.MouldSIDField, info.CommonDiscInfo.Layer1MouldSID, 2);
                    AddIfExists(output, "Label Side " + Template.AdditionalMouldField, info.CommonDiscInfo.Layer1AdditionalMould, 2);
                }

                AddIfExists(output, Template.BarcodeField, info.CommonDiscInfo.Barcode, 1);
                AddIfExists(output, Template.EXEDateBuildDate, info.CommonDiscInfo.EXEDateBuildDate, 1);
                AddIfExists(output, Template.ErrorCountField, info.CommonDiscInfo.ErrorsCount, 1);
                AddIfExists(output, Template.CommentsField, info.CommonDiscInfo.Comments.Trim(), 1);
                AddIfExists(output, Template.ContentsField, info.CommonDiscInfo.Contents.Trim(), 1);

                // Version and Editions section
                output.Add(""); output.Add("Version and Editions:");
                AddIfExists(output, Template.VersionField, info.VersionAndEditions.Version, 1);
                AddIfExists(output, Template.EditionField, info.VersionAndEditions.OtherEditions, 1);

                // EDC section
                if (info.CommonDiscInfo.System == KnownSystem.SonyPlayStation)
                {
                    output.Add(""); output.Add("EDC:");
                    AddIfExists(output, Template.PlayStationEDCField, info.EDC.EDC.LongName(), 1);
                }

                // Parent/Clone Relationship section
                // output.Add(""); output.Add("Parent/Clone Relationship:");
                // AddIfExists(output, Template.ParentIDField, info.ParentID);
                // AddIfExists(output, Template.RegionalParentField, info.RegionalParent.ToString());

                // Extras section
                if (info.Extras.PVD != null || info.Extras.PIC != null || info.Extras.BCA != null)
                {
                    output.Add(""); output.Add("Extras:");
                    AddIfExists(output, Template.PVDField, info.Extras.PVD?.Trim(), 1);
                    AddIfExists(output, Template.PlayStation3WiiDiscKeyField, info.Extras.DiscKey, 1);
                    AddIfExists(output, Template.PlayStation3DiscIDField, info.Extras.DiscID, 1);
                    AddIfExists(output, Template.PICField, info.Extras.PIC, 1);
                    AddIfExists(output, Template.HeaderField, info.Extras.Header, 1);
                    AddIfExists(output, Template.GameCubeWiiBCAField, info.Extras.BCA, 1);
                    AddIfExists(output, Template.XBOXSSRanges, info.Extras.SecuritySectorRanges, 1);
                }

                // Copy Protection section
                if (info.CopyProtection.Protection != null
                    || info.CopyProtection.AntiModchip != YesNo.NULL
                    || info.CopyProtection.LibCrypt != YesNo.NULL)
                {
                    output.Add(""); output.Add("Copy Protection:");
                    if (info.CommonDiscInfo.System == KnownSystem.SonyPlayStation)
                    {
                        AddIfExists(output, Template.PlayStationAntiModchipField, info.CopyProtection.AntiModchip.LongName(), 1);
                        AddIfExists(output, Template.PlayStationLibCryptField, info.CopyProtection.LibCrypt.LongName(), 1);
                        AddIfExists(output, Template.SubIntentionField, info.CopyProtection.LibCryptData, 1);
                    }

                    AddIfExists(output, Template.CopyProtectionField, info.CopyProtection.Protection, 1);
                    AddIfExists(output, Template.SubIntentionField, info.CopyProtection.SecuROMData, 1);
                }

                // Dumpers and Status section
                // output.Add(""); output.Add("Dumpers and Status");
                // AddIfExists(output, Template.StatusField, info.Status.Name());
                // AddIfExists(output, Template.OtherDumpersField, info.OtherDumpers);

                // Tracks and Write Offsets section
                if (!string.IsNullOrWhiteSpace(info.TracksAndWriteOffsets.ClrMameProData))
                {
                    output.Add(""); output.Add("Tracks and Write Offsets:");
                    AddIfExists(output, Template.DATField, info.TracksAndWriteOffsets.ClrMameProData + "\n", 1);
                    AddIfExists(output, Template.CuesheetField, info.TracksAndWriteOffsets.Cuesheet, 1);
                    AddIfExists(output, Template.WriteOffsetField, info.TracksAndWriteOffsets.OtherWriteOffsets, 1);
                }
                // Size & Checksum section
                else
                {
                    output.Add(""); output.Add("Size & Checksum:");
                    AddIfExists(output, Template.LayerbreakField, (info.SizeAndChecksums.Layerbreak == default ? null : info.SizeAndChecksums.Layerbreak.ToString()), 1);
                    AddIfExists(output, Template.SizeField, info.SizeAndChecksums.Size.ToString(), 1);
                    AddIfExists(output, Template.CRC32Field, info.SizeAndChecksums.CRC32, 1);
                    AddIfExists(output, Template.MD5Field, info.SizeAndChecksums.MD5, 1);
                    AddIfExists(output, Template.SHA1Field, info.SizeAndChecksums.SHA1, 1);
                }

                // Make sure there aren't any instances of two blank lines in a row
                string last = null;
                for (int i = 0; i < output.Count;)
                {
                    if (output[i] == last && string.IsNullOrWhiteSpace(last))
                    {
                        output.RemoveAt(i);
                    }
                    else
                    {
                        last = output[i];
                        i++;
                    }
                }

                return output;
            }
            catch
            {
                // We don't care what the error is
                return null;
            }
        }

        /// <summary>
        /// Get the adjusted name of the media baed on layers, if applicable
        /// </summary>
        /// <param name="mediaType">MediaType to get the proper name for</param>
        /// <param name="layerbreak">First layerbreak value, as applicable</param>
        /// <param name="layerbreak2">Second layerbreak value, as applicable</param>
        /// <param name="layerbreak3">Third ayerbreak value, as applicable</param>
        /// <returns>String representation of the media, including layer specification</returns>
        private string GetFixedMediaType(MediaType? mediaType, long layerbreak, long layerbreak2, long layerbreak3)
        {
            switch (mediaType)
            {
                case MediaType.DVD:
                    if (layerbreak != default)
                        return $"{mediaType.LongName()}-9";
                    else
                        return $"{mediaType.LongName()}-5";

                // TODO: Figure out what the addtional layerbreaks indicate
                case MediaType.BluRay:
                    if (layerbreak != default)
                        return $"{mediaType.LongName()}-50";
                    else
                        return $"{mediaType.LongName()}-25";

                case MediaType.UMD:
                    if (layerbreak != default)
                        return $"{mediaType.LongName()}-DL";
                    else
                        return $"{mediaType.LongName()}-SL";

                default:
                    return mediaType.LongName();
            }
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
            (OutputDirectory, OutputFilename) = NormalizeOutputPaths(OutputDirectory, OutputFilename);

            // Validate that the output path isn't on the dumping drive
            string fullOutputPath = Path.GetFullPath(Path.Combine(OutputDirectory, OutputFilename));
            if (fullOutputPath[0] == Drive.Letter)
                return Result.Failure($"Error! Cannot output to same drive that is being dumped!");

            // Validate that the required program exists
            if (!File.Exists(Parameters.ExecutablePath))
                return Result.Failure($"Error! {Parameters.ExecutablePath} does not exist!");

            // Validate that the dumping drive doesn't contain the executable
            string fullExecutablePath = Path.GetFullPath(Parameters.ExecutablePath);
            if (fullExecutablePath[0] == Drive.Letter)
                return Result.Failure("$Error! Cannot dump same drive that executable resides on!");

            // Validate that the current configuration is supported
            return Validators.GetSupportStatus(System, Type);
        }

        /// <summary>
        /// Write the data to the output folder
        /// </summary>
        /// <param name="lines">Preformatted list of lines to write out to the file</param>
        /// <returns>True on success, false on error</returns>
        private bool WriteOutputData(List<string> lines)
        {
            // Check to see if the inputs are valid
            if (lines == null)
                return false;

            // Now write out to a generic file
            try
            {
                using (StreamWriter sw = new StreamWriter(File.Open(Path.Combine(OutputDirectory, "!submissionInfo.txt"), FileMode.Create, FileAccess.Write)))
                {
                    foreach (string line in lines)
                        sw.WriteLine(line);
                }
            }
            catch
            {
                // We don't care what the error is right now
                return false;
            }

            return true;
        }

        /// <summary>
        /// Write the data to the output folder
        /// </summary>
        /// <param name="info">SubmissionInfo object representing the JSON to write out to the file</param>
        /// <returns>True on success, false on error</returns>
        private bool WriteOutputData(SubmissionInfo info)
        {
            // Check to see if the input is valid
            if (info == null)
                return false;

            // Now write out to the JSON
            try
            {
                using (var fs = File.Create(Path.Combine(OutputDirectory, "!submissionInfo.json.gz")))
                using (var gs = new GZipStream(fs, CompressionMode.Compress))
                {
                    string json = JsonConvert.SerializeObject(info, Formatting.Indented);
                    byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                    gs.Write(jsonBytes, 0, jsonBytes.Length);
                }
            }
            catch
            {
                // We don't care what the error is right now
                return false;
            }

            return true;
        }

        #endregion

        #region Information Extraction

        /// <summary>
        /// Get the current copy protection scheme, if possible
        /// </summary>
        /// <param name="progress">Optional progress callback</param>
        /// <returns>Copy protection scheme if possible, null on error</returns>
        private async Task<string> GetCopyProtection(IProgress<ProtectionProgress> progress = null)
        {
            if (Options.ScanForProtection)
            {
                (bool success, string output) = await Validators.RunProtectionScanOnPath($"{Drive.Letter}:\\", progress);
                if (success)
                    return output;
                else
                    return "An error occurred while scanning!";
            }

            return "(CHECK WITH PROTECTIONID)";
        }

        /// <summary>
        /// Get the split values for ISO-based media
        /// </summary>
        /// <param name="hashData">String representing the combined hash data</param>
        /// <returns>True if extraction was successful, false otherwise</returns>
        private bool GetISOHashValues(string hashData, out long size, out string crc32, out string md5, out string sha1)
        {
            size = -1; crc32 = null; md5 = null; sha1 = null;

            if (string.IsNullOrWhiteSpace(hashData))
                return false;

            Regex hashreg = new Regex(@"<rom name="".*?"" size=""(.*?)"" crc=""(.*?)"" md5=""(.*?)"" sha1=""(.*?)""");
            Match m = hashreg.Match(hashData);
            if (m.Success)
            {
                Int64.TryParse(m.Groups[1].Value, out size);
                crc32 = m.Groups[2].Value;
                md5 = m.Groups[3].Value;
                sha1 = m.Groups[4].Value;
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Information Formatting

        /// <summary>
        /// Add the properly formatted key and value, if possible
        /// </summary>
        /// <param name="output">Output list</param>
        /// <param name="key">Name of the output key to write</param>
        /// <param name="value">Name of the output value to write</param>
        /// <param name="indent">Number of tabs to indent the line</param>
        private void AddIfExists(List<string> output, string key, string value, int indent)
        {
            // If there's no valid value to write
            if (value == null)
                return;

            string prefix = "";
            for (int i = 0; i < indent; i++)
                prefix += "\t";

            // If the value contains a newline
            value = value.Replace("\r\n", "\n");
            if (value.Contains("\n"))
            {
                output.Add(prefix + key + ":"); output.Add("");
                string[] values = value.Split('\n');
                foreach (string val in values)
                    output.Add(val);

                output.Add("");
            }

            // For all regular values
            else
            {
                output.Add(prefix + key + ": " + value);
            }
        }

        /// <summary>
        /// Add the properly formatted key and value, if possible
        /// </summary>
        /// <param name="output">Output list</param>
        /// <param name="key">Name of the output key to write</param>
        /// <param name="value">Name of the output value to write</param>
        /// <param name="indent">Number of tabs to indent the line</param>
        private void AddIfExists(List<string> output, string key, string[] value, int indent)
        {
            // If there's no valid value to write
            if (value == null || value.Length == 0)
                return;

            AddIfExists(output, key, string.Join(", ", value), indent);
        }

        /// <summary>
        /// Add the properly formatted key and value, if possible
        /// </summary>
        /// <param name="output">Output list</param>
        /// <param name="key">Name of the output key to write</param>
        /// <param name="value">Name of the output value to write</param>
        /// <param name="indent">Number of tabs to indent the line</param>
        private void AddIfExists(List<string> output, string key, List<int> value, int indent)
        {
            // If there's no valid value to write
            if (value == null || value.Count() == 0)
                return;

            AddIfExists(output, key, string.Join(", ", value.Select(o => o.ToString())), indent);
        }

        #endregion
    }
}
