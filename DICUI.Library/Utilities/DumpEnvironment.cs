using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DICUI.Data;
using DICUI.Web;
using Newtonsoft.Json;

namespace DICUI.Utilities
{
    /// <summary>
    /// Represents the state of all settings to be used during dumping
    /// </summary>
    public class DumpEnvironment
    {
        #region Tool paths

        /// <summary>
        /// Path to DiscImageCreator executable
        /// </summary>
        public string DICPath { get; set; }

        /// <summary>
        /// Path to Subdump executable
        /// </summary>
        public string SubdumpPath { get; set; }

        #endregion

        #region Output paths

        /// <summary>
        /// Base output directory to write files to
        /// </summary>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Base output filename for DiscImageCreator
        /// </summary>
        public string OutputFilename { get; set; }

        #endregion

        #region UI information

        /// <summary>
        /// Drive object representing the current drive
        /// </summary>
        public Drive Drive { get; set; }

        /// <summary>
        /// Currently selected system
        /// </summary>
        public KnownSystem? System { get; set; }

        /// <summary>
        /// Currently selected media type
        /// </summary>
        public MediaType? Type { get; set; }

        /// <summary>
        /// Parameters object representing what to send to DiscImageCreator
        /// </summary>
        public Parameters DICParameters { get; set; }

        /// <summary>
        /// Determines if placeholder values should be set for fields
        /// </summary>
        public bool AddPlaceholders { get; set; }
        
        /// <summary>
        /// Determines if the user should be prompted to input or fix submission data
        /// </summary>
        public bool PromptForDiscInformation { get; set; }

        #endregion

        #region Extra DIC arguments

        /// <summary>
        /// Enable quiet mode (no beeps)
        /// </summary>
        public bool QuietMode { get; set; }

        /// <summary>
        /// Enable paranoid mode (extra flags)
        /// </summary>
        public bool ParanoidMode { get; set; }

        /// <summary>
        /// Scan for copy protection, where applicable
        /// </summary>
        public bool ScanForProtection { get; set; }

        /// <summary>
        /// Number of C2 error reread attempts
        /// </summary>
        public int RereadAmountC2 { get; set; }

        #endregion

        #region Redump login information

        /// <summary>
        /// Redump.org username for pulling existing disc data
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Redump.org password for pulling existing disc data
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Determine if a complete set of Redump credentials might exist
        /// </summary>
        public bool HasRedumpLogin { get => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password); }
        
        #endregion

        #region External process information

        /// <summary>
        /// Process to track DiscImageCreator instances
        /// </summary>
        private Process dicProcess;

        #endregion

        #region Public Functionality

        /// <summary>
        /// Cancel an in-progress dumping process
        /// </summary>
        public void CancelDumping()
        {
            try
            {
                if (dicProcess != null && !dicProcess.HasExited)
                    dicProcess.Kill();
            }
            catch
            { }
        }

        /// <summary>
        /// Gets if the current drive has the latest firmware
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DriveHasLatestFimrware()
        {
            // Validate that the required program exists
            if (!File.Exists(DICPath))
                return false;

            // Use the drive speed command as a quick test
            Process childProcess;
            string output = await Task.Run(() =>
            {
                childProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = DICPath,
                        Arguments = DICCommandStrings.DriveSpeed + " " + Drive.Letter,
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

            // If we get the firmware message
            if (output.Contains("[ERROR] This drive isn't latest firmware. Please update."))
                return false;

            // Otherwise, we know the firmware's good
            return true;
        }

        /// <summary>
        /// Eject the disc using DIC
        /// </summary>
        public async void EjectDisc()
        {
            // Validate that the required program exists
            if (!File.Exists(DICPath))
                return;

            CancelDumping();

            // Validate we're not trying to eject a non-optical
            if (Drive.DriveType != InternalDriveType.Optical)
                return;

            Process childProcess;
            await Task.Run(() =>
            {
                childProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = DICPath,
                        Arguments = DICCommandStrings.Eject + " " + Drive.Letter,
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

                childProcess.Dispose();
            });
        }

        /// <summary>
        /// Fix output paths to strip out any invalid characters
        /// </summary>
        public void FixOutputPaths()
        {
            try
            {
                // Cache if we had a directory separator or not
                bool endedWithDirectorySeparator = OutputDirectory.EndsWith(Path.DirectorySeparatorChar.ToString());
                bool endedWithSpace = OutputDirectory.EndsWith(" ");

                // Combine the path to make things separate easier
                string combinedPath = Path.Combine(OutputDirectory, OutputFilename);

                // If we have have a blank path, just return
                if (string.IsNullOrWhiteSpace(combinedPath))
                    return;

                // Now get the normalized paths
                OutputDirectory = Path.GetDirectoryName(combinedPath);
                OutputFilename = Path.GetFileName(combinedPath);                

                // Take care of extra path characters
                OutputDirectory = new StringBuilder(OutputDirectory.Replace('&', '_'))
                    .Replace(':', '_', 0, OutputDirectory.LastIndexOf(':') == -1 ? 0 : OutputDirectory.LastIndexOf(':')).ToString();
                OutputFilename = new StringBuilder(OutputFilename.Replace('&', '_')).ToString();

                // Sanitize everything else
                foreach (char c in Path.GetInvalidPathChars())
                    OutputDirectory = OutputDirectory.Replace(c, '_');
                foreach (char c in Path.GetInvalidFileNameChars())
                    OutputFilename = OutputFilename.Replace(c, '_');

                // If we had a space at the end before, add it again
                if (endedWithSpace)
                    OutputDirectory += " ";

                // If we had a directory separator at the end before, add it again
                if (endedWithDirectorySeparator)
                    OutputDirectory += Path.DirectorySeparatorChar;

                // If we have a root directory, sanitize
                if (Directory.Exists(OutputDirectory))
                {
                    var possibleRootDir = new DirectoryInfo(OutputDirectory);
                    if (possibleRootDir.Parent == null)
                    {
                        OutputDirectory = OutputDirectory.Replace($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}", $"{Path.DirectorySeparatorChar}");
                    }
                }
            }
            catch
            {
                // We don't care what the error was
                return;
            }
        }

        /// <summary>
        /// Ensures that all required output files have been created
        /// </summary>
        /// <returns></returns>
        public bool FoundAllFiles()
        {
            // First, sanitized the output filename to strip off any potential extension
            string outputFilename = Path.GetFileNameWithoutExtension(OutputFilename);

            // Some disc types are audio-only
            bool audioOnly = (System == KnownSystem.AtariJaguarCD)
                || (System == KnownSystem.AudioCD)
                || (System == KnownSystem.SuperAudioCD);

            // Now ensure that all required files exist
            string combinedBase = Path.Combine(OutputDirectory, outputFilename);
            switch (Type)
            {
                case MediaType.CDROM:
                case MediaType.GDROM: // TODO: Verify GD-ROM outputs this
                    // return File.Exists(combinedBase + ".c2") // Doesn't output on Linux
                    return File.Exists(combinedBase + ".ccd")
                        && File.Exists(combinedBase + ".cue")
                        && File.Exists(combinedBase + ".dat")
                        && File.Exists(combinedBase + ".img")
                        && (audioOnly || File.Exists(combinedBase + ".img_EdcEcc.txt") || File.Exists(combinedBase + ".img_EccEdc.txt"))
                        && (audioOnly || File.Exists(combinedBase + ".scm"))
                        && File.Exists(combinedBase + ".sub")
                        // && File.Exists(combinedBase + "_c2Error.txt") // Doesn't output on Linux
                        && File.Exists(combinedBase + "_cmd.txt")
                        && File.Exists(combinedBase + "_disc.txt")
                        && File.Exists(combinedBase + "_drive.txt")
                        && File.Exists(combinedBase + "_img.cue")
                        && File.Exists(combinedBase + "_mainError.txt")
                        && File.Exists(combinedBase + "_mainInfo.txt")
                        && File.Exists(combinedBase + "_subError.txt")
                        && File.Exists(combinedBase + "_subInfo.txt")
                        // && File.Exists(combinedBase + "_subIntention.txt") // Not guaranteed output
                        && (File.Exists(combinedBase + "_subReadable.txt") || File.Exists(combinedBase + "_sub.txt"))
                        && File.Exists(combinedBase + "_volDesc.txt");
                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    return File.Exists(combinedBase + ".dat")
                        && File.Exists(combinedBase + "_cmd.txt")
                        && File.Exists(combinedBase + "_disc.txt")
                        && File.Exists(combinedBase + "_drive.txt")
                        && File.Exists(combinedBase + "_mainError.txt")
                        && File.Exists(combinedBase + "_mainInfo.txt")
                        && File.Exists(combinedBase + "_volDesc.txt");
                case MediaType.FloppyDisk:
                    return File.Exists(combinedBase + ".dat")
                        && File.Exists(combinedBase + "_cmd.txt")
                       && File.Exists(combinedBase + "_disc.txt");
                case MediaType.UMD:
                    return File.Exists(combinedBase + "_disc.txt")
                        || File.Exists(combinedBase + "_mainError.txt")
                        || File.Exists(combinedBase + "_mainInfo.txt")
                        || File.Exists(combinedBase + "_volDesc.txt");
                default:
                    // Non-dumping commands will usually produce no output, so this is irrelevant
                    return true;
            }
        }

        /// <summary>
        /// Get the full parameter string for DIC
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

                FixOutputPaths();

                // Set the proper parameters
                DICParameters = new Parameters(System, Type, Drive.Letter, Path.Combine(OutputDirectory, OutputFilename), driveSpeed, ParanoidMode, RereadAmountC2);
                if (QuietMode)
                    DICParameters[DICFlag.DisableBeep] = true;

                // Generate and return the param string
                return DICParameters.GenerateParameters();
            }

            return null;
        }

        /// <summary>
        /// Execute a complete dump workflow
        /// </summary>
        public async Task<Result> StartDumping(IProgress<Result> progress)
        {
            Result result = IsValidForDump();

            // Execute DIC and external tools, if needed
            if (Validators.GetSupportStatus(System, Type)
                && !result.Message.Contains("not supported") // Completely unsupported media
                && !result.Message.Contains("submission info")) // Submission info-only media
            {
                // If the environment is invalid, return
                if (!result)
                    return result;

                progress?.Report(Result.Success("Executing DiscImageCreator... please wait!"));
                await Task.Run(() => ExecuteDiskImageCreator());
                progress?.Report(Result.Success("DiscImageCreator has finished!"));

                // Execute additional tools
                progress?.Report(Result.Success("Running any additional tools... please wait!"));
                result = await Task.Run(() => ExecuteAdditionalToolsAfterDIC());
                progress?.Report(result);
            }

            return result;
        }

        /// <summary>
        /// Verify that the current environment has a complete dump and create submission info is possible
        /// </summary>
        /// <returns>Result instance with the outcome</returns>
        public Result VerifyAndSaveDumpOutput(IProgress<Result> progress, Func<SubmissionInfo, bool?> ShowUserPrompt = null)
        {
            progress.Report(Result.Success("Gathering submission information... please wait!"));

            // Check to make sure that the output had all the correct files
            if (!FoundAllFiles())
                return Result.Failure("Error! Please check output directory as dump may be incomplete!");

            progress?.Report(Result.Success("Extracting output information from output files..."));
            SubmissionInfo submissionInfo = ExtractOutputInformation(progress);
            progress?.Report(Result.Success("Extracting information complete!"));

            if (PromptForDiscInformation && ShowUserPrompt != null)
            {
                progress?.Report(Result.Success("Waiting for additional disc information..."));
                bool? filledInfo = ShowUserPrompt(submissionInfo);
                progress?.Report(Result.Success("Additional disc information added!"));
            }

            progress?.Report(Result.Success("Formatting extracted information..."));
            List<string> formattedValues = FormatOutputData(submissionInfo);
            progress?.Report(Result.Success("Formatting complete!"));

            progress?.Report(Result.Success("Writing information to !submissionInfo.txt..."));
            bool success = WriteOutputData(formattedValues);
            success &= WriteOutputData(submissionInfo);

            if (success)
                progress?.Report(Result.Success("Writing complete!"));
            else
                progress?.Report(Result.Failure("Writing could not complete!"));

            progress.Report(Result.Success("All submission information gathered!"));

            return Result.Success();
        }

        #endregion

        #region Internal for Testing Purposes

        /// <summary>
        /// Checks if the parameters are valid
        /// </summary>
        /// <returns>True if the configuration is valid, false otherwise</returns>
        internal bool ParametersValid()
        {
            return DICParameters.IsValid()
                && !(Drive.DriveType == InternalDriveType.Floppy ^ Type == MediaType.FloppyDisk)
                && !(Drive.DriveType == InternalDriveType.HardDisk ^ Type == MediaType.HardDisk)
                && !(Drive.DriveType == InternalDriveType.Removable ^ (Type == MediaType.CompactFlash || Type == MediaType.SDCard || Type == MediaType.FlashDrive));
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Run any additional tools given a DumpEnvironment
        /// </summary>
        /// <returns>Result instance with the outcome</returns>
        private Result ExecuteAdditionalToolsAfterDIC()
        {
            // Special cases
            switch (System)
            {
                case KnownSystem.SegaSaturn:
                    if (!File.Exists(SubdumpPath))
                        return Result.Failure("Error! Could not find subdump!");

                    ExecuteSubdump();
                    return Result.Success("subdump has finished!");
            }

            return Result.Success("No external tools needed!");
        }

        /// <summary>
        /// Run DiscImageCreator
        /// </summary>
        private void ExecuteDiskImageCreator()
        {
            dicProcess = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = DICPath,
                    Arguments = DICParameters.GenerateParameters() ?? "",
                },
            };
            dicProcess.Start();
            dicProcess.WaitForExit();
        }

        /// <summary>
        /// Execute subdump for a (potential) Sega Saturn dump
        /// </summary>
        private async void ExecuteSubdump()
        {
            await Task.Run(() =>
            {
                Process childProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = SubdumpPath,
                        Arguments = "-i " + Drive.Letter + ": -f " + Path.Combine(OutputDirectory, Path.GetFileNameWithoutExtension(OutputFilename) + "_subdump.sub") + "-mode 6 -rereadnum 25 -fix 2",
                    },
                };
                childProcess.Start();
                childProcess.WaitForExit();
            });
        }

        /// <summary>
        /// Extract all of the possible information from a given input combination
        /// </summary>
        /// <param name="driveLetter">Drive letter to check</param>
        /// <returns>SubmissionInfo populated based on outputs, null on error</returns>
        private SubmissionInfo ExtractOutputInformation(IProgress<Result> progress)
        {
            // Ensure the current disc combination should exist
            if (!Validators.GetValidMediaTypes(System).Contains(Type))
                return null;

            // Sanitize the output filename to strip off any potential extension
            string outputFilename = Path.GetFileNameWithoutExtension(OutputFilename);

            // Check that all of the relevant files are there
            if (!FoundAllFiles())
                return null;

            // Create the SubmissionInfo object with all user-inputted values by default
            string combinedBase = Path.Combine(OutputDirectory, outputFilename);
            SubmissionInfo info = new SubmissionInfo()
            {
                CommonDiscInfo = new CommonDiscInfoSection()
                {
                    System = this.System,
                    Media = this.Type,
                    Title = (this.AddPlaceholders ? Template.RequiredValue : ""),
                    ForeignTitleNonLatin = (AddPlaceholders ? Template.OptionalValue : ""),
                    DiscNumberLetter = (AddPlaceholders ? Template.OptionalValue : ""),
                    DiscTitle = (AddPlaceholders ? Template.OptionalValue : ""),
                    Category = Category.Games,
                    Region = null,
                    Languages = null,
                    Serial = (AddPlaceholders ? Template.RequiredIfExistsValue : ""),
                    Barcode = (AddPlaceholders ? Template.OptionalValue : ""),
                    Contents = (AddPlaceholders ? Template.OptionalValue : ""),
                },
                VersionAndEditions = new VersionAndEditionsSection()
                {
                    Version = (AddPlaceholders ? Template.RequiredIfExistsValue : ""),
                    OtherEditions = (AddPlaceholders ? "Original (VERIFY THIS)" : ""),
                },
                TracksAndWriteOffsets = new TracksAndWriteOffsetsSection()
                {
                    ClrMameProData = GetDatfile(combinedBase + ".dat"),
                },
            };

            // First and foremost, we want to get a list of matching IDs for each line in the DAT
            if (!string.IsNullOrEmpty(info.TracksAndWriteOffsets.ClrMameProData) && HasRedumpLogin)
            {
                // Set the current dumper based on username
                info.DumpersAndStatus.Dumpers = new string[] { this.Username };

                info.MatchedIDs = new List<int>();
                using (CookieAwareWebClient wc = new CookieAwareWebClient())
                {
                    // Login to Redump
                    RedumpAccess access = new RedumpAccess();
                    if (access.RedumpLogin(wc, this.Username, this.Password))
                    {
                        // Loop through all of the hashdata to find matching IDs
                        progress?.Report(Result.Success("Finding disc matches on Redump..."));
                        string[] splitData = info.TracksAndWriteOffsets.ClrMameProData.Split('\n');
                        foreach (string hashData in splitData)
                        {
                            if (GetISOHashValues(hashData, out long size, out string crc32, out string md5, out string sha1))
                            {
                                List<int> newIds = access.ProcessSearch(wc, sha1);
                                if (info.MatchedIDs.Any())
                                    info.MatchedIDs = info.MatchedIDs.Intersect(newIds).ToList();
                                else
                                    info.MatchedIDs = newIds;
                            }
                        }

                        progress?.Report(Result.Success("Match finding complete! " + (info.MatchedIDs.Count > 0 ? "Matched IDs: " + string.Join(",", info.MatchedIDs) : "No matches found")));

                        // If we have exactly 1 ID, we can grab a bunch of info from it
                        if (info.MatchedIDs.Count == 1)
                        {
                            progress?.Report(Result.Success($"Filling fields from existing ID {info.MatchedIDs[0]}..."));
                            string discData = access.DownloadSingleSiteID(wc, info.MatchedIDs[0]);
                            info.FillFromDiscPage(discData);
                            progress?.Report(Result.Success("Information filling complete!"));
                        }
                    }
                }
            }

            // Extract info based generically on MediaType
            switch (Type)
            {
                case MediaType.CDROM:
                case MediaType.GDROM: // TODO: Verify GD-ROM outputs this
                    info.CommonDiscInfo.MasteringRingFirstLayerDataSide = (AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    info.CommonDiscInfo.MasteringSIDCodeFirstLayerDataSide = (AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    info.CommonDiscInfo.ToolstampMasteringCodeFirstLayerDataSide = (AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    info.CommonDiscInfo.MouldSIDCodeFirstLayerDataSide = (AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    info.CommonDiscInfo.MouldSIDCodeSecondLayerLabelSide = (AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    info.CommonDiscInfo.AdditionalMouldFirstLayerDataSide = (AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    info.Extras.PVD = GetPVD(combinedBase + "_mainInfo.txt") ?? "Disc has no PVD"; ;

                    long errorCount = -1;
                    if (File.Exists(combinedBase + ".img_EdcEcc.txt"))
                        errorCount = GetErrorCount(combinedBase + ".img_EdcEcc.txt");
                    else if (File.Exists(combinedBase + ".img_EccEdc.txt"))
                        errorCount = GetErrorCount(combinedBase + ".img_EccEdc.txt");

                    info.CommonDiscInfo.ErrorsCount = (errorCount == -1 ? "Error retrieving error count" : errorCount.ToString());
                    info.TracksAndWriteOffsets.Cuesheet = GetFullFile(combinedBase + ".cue") ?? ""; ;

                    string cdWriteOffset = GetWriteOffset(combinedBase + "_disc.txt") ?? "";
                    info.CommonDiscInfo.RingWriteOffset = cdWriteOffset;
                    info.TracksAndWriteOffsets.OtherWriteOffsets = cdWriteOffset;

                    // GD-ROM-specfic options
                    if (Type == MediaType.GDROM)
                        info.Extras.Header = GetSegaHeader(combinedBase + "_mainInfo.txt") ?? "";

                    break;

                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                    bool isXbox = (System == KnownSystem.MicrosoftXBOX || System == KnownSystem.MicrosoftXBOX360);

                    // Get the individual hash data, as per internal
                    if (GetISOHashValues(info.TracksAndWriteOffsets.ClrMameProData, out long size, out string crc32, out string md5, out string sha1))
                    {
                        info.SizeAndChecksums.Size = size;
                        info.SizeAndChecksums.CRC32 = crc32;
                        info.SizeAndChecksums.MD5 = md5;
                        info.SizeAndChecksums.SHA1 = sha1;
                        info.TracksAndWriteOffsets.ClrMameProData = null;
                    }

                    // Deal with the layerbreak
                    string layerbreak = null;
                    if (Type == MediaType.DVD)
                        layerbreak = GetLayerbreak(combinedBase + "_disc.txt", isXbox) ?? "";
                    else if (Type == MediaType.BluRay)
                        layerbreak = (info.SizeAndChecksums.Size > 25025314816 ? "25025314816" : null);

                    // If we have a single-layer disc
                    if (String.IsNullOrWhiteSpace(layerbreak))
                    {
                        info.CommonDiscInfo.MasteringRingFirstLayerDataSide = (AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.MasteringSIDCodeFirstLayerDataSide = (AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.ToolstampMasteringCodeFirstLayerDataSide = (AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.MouldSIDCodeFirstLayerDataSide = (AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.MouldSIDCodeSecondLayerLabelSide = (AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.AdditionalMouldFirstLayerDataSide = (AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.Extras.PVD = GetPVD(combinedBase + "_mainInfo.txt") ?? "";
                    }
                    // If we have a dual-layer disc
                    else
                    {
                        info.CommonDiscInfo.MasteringRingFirstLayerDataSide = (AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.MasteringSIDCodeFirstLayerDataSide = (AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.ToolstampMasteringCodeFirstLayerDataSide = (AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.MouldSIDCodeFirstLayerDataSide = (AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.AdditionalMouldFirstLayerDataSide = (AddPlaceholders ? Template.RequiredIfExistsValue : "");

                        info.CommonDiscInfo.MasteringRingSecondLayerLabelSide = (AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.MasteringSIDCodeSecondLayerLabelSide = (AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.ToolstampMasteringCodeSecondLayerLabelSide = (AddPlaceholders ? Template.RequiredIfExistsValue : "");
                        info.CommonDiscInfo.MouldSIDCodeSecondLayerLabelSide = (AddPlaceholders ? Template.RequiredIfExistsValue : "");

                        info.Extras.PVD = GetPVD(combinedBase + "_mainInfo.txt") ?? "";
                        info.SizeAndChecksums.Layerbreak = Int64.Parse(layerbreak);
                    }

                    // Bluray-specific options
                    if (Type == MediaType.BluRay)
                        info.Extras.PIC = GetPIC(Path.Combine(OutputDirectory, "PIC.bin")) ?? "";

                    break;

                case MediaType.NintendoGameCubeGameDisc:
                    info.Extras.BCA = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case MediaType.NintendoWiiOpticalDisc:
                    info.Extras.DiscKey = (this.AddPlaceholders ? Template.RequiredValue : "");
                    info.Extras.BCA = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case MediaType.UMD:
                    info.Extras.PVD = GetPVD(combinedBase + "_mainInfo.txt") ?? "";
                    info.SizeAndChecksums.CRC32 = (this.AddPlaceholders ? Template.RequiredValue + " [Not automatically generated for UMD]" : "");
                    info.SizeAndChecksums.MD5 = (this.AddPlaceholders ? Template.RequiredValue + " [Not automatically generated for UMD]" : "");
                    info.SizeAndChecksums.SHA1 = (this.AddPlaceholders ? Template.RequiredValue + " [Not automatically generated for UMD]" : "");
                    info.TracksAndWriteOffsets.ClrMameProData = null;

                    if (GetUMDAuxInfo(combinedBase + "_disc.txt", out string title, out Category? umdcat, out string umdversion, out string umdlayer, out long umdsize))
                    {
                        info.CommonDiscInfo.Title = title ?? "";
                        info.CommonDiscInfo.Category = umdcat ?? Category.Games;
                        info.VersionAndEditions.Version = umdversion ?? "";
                        info.SizeAndChecksums.Size = umdsize;

                        if (!String.IsNullOrWhiteSpace(umdlayer))
                            info.SizeAndChecksums.Layerbreak = Int64.Parse(umdlayer ?? "-1");
                    }

                    break;
            }

            // Extract info based specifically on KnownSystem
            switch (System)
            {
                case KnownSystem.AppleMacintosh:
                case KnownSystem.EnhancedCD:
                case KnownSystem.IBMPCCompatible:
                case KnownSystem.RainbowDisc:
                    if (string.IsNullOrWhiteSpace(info.CommonDiscInfo.Comments))
                        info.CommonDiscInfo.Comments += $"[T:ISBN] {(AddPlaceholders ? Template.OptionalValue : "")}";

                    progress?.Report(Result.Success("Running copy protection scan... this might take a while!"));
                    info.CopyProtection.Protection = GetCopyProtection();
                    progress?.Report(Result.Success("Copy protection scan complete!"));

                    if (File.Exists(combinedBase + "_subIntention.txt"))
                    {
                        FileInfo fi = new FileInfo(combinedBase + "_subIntention.txt");
                        if (fi.Length > 0)
                            info.CopyProtection.SecuROMData = GetFullFile(combinedBase + "_subIntention.txt") ?? "";
                    }

                    break;

                case KnownSystem.BandaiPlaydiaQuickInteractiveSystem:
                    info.CommonDiscInfo.EXEDateBuildDate = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.BDVideo:
                    info.CopyProtection.Protection = (AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    break;

                case KnownSystem.CommodoreAmiga:
                    info.CommonDiscInfo.EXEDateBuildDate = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.CommodoreAmigaCD32:
                    info.CommonDiscInfo.EXEDateBuildDate = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.CommodoreAmigaCDTV:
                    info.CommonDiscInfo.EXEDateBuildDate = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.DVDVideo:
                    info.CopyProtection.Protection = GetDVDProtection(combinedBase + "_CSSKey.txt", combinedBase + "_disc.txt") ?? "";
                    break;

                case KnownSystem.FujitsuFMTowns:
                    info.CommonDiscInfo.EXEDateBuildDate = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.IncredibleTechnologiesEagle:
                    info.CommonDiscInfo.EXEDateBuildDate = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.KonamieAmusement:
                    info.CommonDiscInfo.EXEDateBuildDate = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.KonamiFirebeat:
                    info.CommonDiscInfo.EXEDateBuildDate = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.KonamiGVSystem:
                    info.CommonDiscInfo.EXEDateBuildDate = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.KonamiSystem573:
                    info.CommonDiscInfo.EXEDateBuildDate = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.KonamiTwinkle:
                    info.CommonDiscInfo.EXEDateBuildDate = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.MattelHyperscan:
                    info.CommonDiscInfo.EXEDateBuildDate = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.MicrosoftXBOX:
                    if (GetXBOXAuxInfo(combinedBase + "_disc.txt", out string dmihash, out string pfihash, out string sshash, out string ss, out string ssver))
                    {
                        info.CommonDiscInfo.Comments += $"{Template.XBOXDMIHash}: {dmihash ?? ""}\n" +
                            $"{Template.XBOXPFIHash}: {pfihash ?? ""}\n" +
                            $"{Template.XBOXSSHash}: {sshash ?? ""}\n" +
                            $"{Template.XBOXSSVersion}: {ssver ?? ""}\n";
                        info.Extras.SecuritySectorRanges = ss ?? "";
                    }

                    if (GetXBOXDMIInfo(Path.Combine(OutputDirectory, "DMI.bin"), out string serial, out string version, out Region? region))
                    {
                        info.CommonDiscInfo.Serial = serial ?? (this.AddPlaceholders ? Template.RequiredValue : "");
                        info.VersionAndEditions.Version = version ?? (this.AddPlaceholders ? Template.RequiredValue : "");
                        info.CommonDiscInfo.Region = region;
                    }

                    break;

                case KnownSystem.MicrosoftXBOX360:
                    if (GetXBOXAuxInfo(combinedBase + "_disc.txt", out string dmi360hash, out string pfi360hash, out string ss360hash, out string ss360, out string ssver360))
                    {
                        info.CommonDiscInfo.Comments += $"{Template.XBOXDMIHash}: {dmi360hash ?? ""}\n" +
                            $"{Template.XBOXPFIHash}: {pfi360hash ?? ""}\n" +
                            $"{Template.XBOXSSHash}: {ss360hash ?? ""}\n" +
                            $"{Template.XBOXSSVersion}: {ssver360 ?? ""}\n";
                        info.Extras.SecuritySectorRanges = ss360 ?? "";
                    }

                    if (GetXBOX360DMIInfo(Path.Combine(OutputDirectory, "DMI.bin"), out string serial360, out string version360, out Region? region360))
                    {
                        info.CommonDiscInfo.Serial = serial360 ?? (this.AddPlaceholders ? Template.RequiredValue : "");
                        info.VersionAndEditions.Version = version360 ?? (this.AddPlaceholders ? Template.RequiredValue : "");
                        info.CommonDiscInfo.Region = region360;
                    }
                    break;

                case KnownSystem.NamcoSegaNintendoTriforce:
                    info.CommonDiscInfo.EXEDateBuildDate = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.NavisoftNaviken21:
                    info.CommonDiscInfo.EXEDateBuildDate = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.NECPC98:
                    info.CommonDiscInfo.EXEDateBuildDate = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.SegaCDMegaCD:
                    info.Extras.Header = GetSegaHeader(combinedBase + "_mainInfo.txt") ?? "";

                    // Take only the last 16 lines for Sega CD
                    if (!string.IsNullOrEmpty(info.Extras.Header))
                        info.Extras.Header = string.Join("\n", info.Extras.Header.Split('\n').Skip(16));

                    if (GetSegaCDBuildInfo(info.Extras.Header, out string scdSerial, out string fixedDate))
                    {
                        info.CommonDiscInfo.Serial = scdSerial ?? "";
                        info.CommonDiscInfo.EXEDateBuildDate = fixedDate ?? "";
                    }

                    break;

                case KnownSystem.SegaChihiro:
                    info.CommonDiscInfo.EXEDateBuildDate = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.SegaDreamcast:
                    info.CommonDiscInfo.EXEDateBuildDate = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.SegaNaomi:
                    info.CommonDiscInfo.EXEDateBuildDate = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.SegaNaomi2:
                    info.CommonDiscInfo.EXEDateBuildDate = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.SegaSaturn:
                    info.Extras.Header = GetSegaHeader(combinedBase + "_mainInfo.txt") ?? "";

                    // Take only the first 16 lines for Saturn
                    if (!string.IsNullOrEmpty(info.Extras.Header))
                        info.Extras.Header = string.Join("\n", info.Extras.Header.Split('\n').Take(16));

                    if (GetSaturnBuildInfo(info.Extras.Header, out string saturnSerial, out string saturnVersion, out string buildDate))
                    {
                        info.CommonDiscInfo.Serial = saturnSerial ?? "";
                        info.VersionAndEditions.Version = saturnVersion ?? "";
                        info.CommonDiscInfo.EXEDateBuildDate = buildDate ?? "";
                    }

                    break;

                case KnownSystem.SegaTitanVideo:
                    info.CommonDiscInfo.EXEDateBuildDate = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.SNKNeoGeoCD:
                    info.CommonDiscInfo.EXEDateBuildDate = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.SonyPlayStation:
                    info.CommonDiscInfo.EXEDateBuildDate = GetPlayStationEXEDate(Drive?.Letter) ?? "";
                    info.EDC.EDC = GetMissingEDCCount(combinedBase + ".img_EdcEcc.txt") > 0 ? YesNo.No : YesNo.Yes;
                    info.CopyProtection.AntiModchip = GetAntiModchipDetected(combinedBase + "_disc.txt") ? YesNo.Yes : YesNo.No;
                    info.CopyProtection.LibCrypt = YesNo.No;
                    if (File.Exists(combinedBase + "_subIntention.txt"))
                    {
                        FileInfo fi = new FileInfo(combinedBase + "_subIntention.txt");
                        if (fi.Length > 0)
                        {
                            info.CopyProtection.LibCrypt = YesNo.Yes;
                            info.CopyProtection.LibCryptData = GetFullFile(combinedBase + "_subIntention.txt") ?? "";
                        }
                    }

                    break;

                case KnownSystem.SonyPlayStation2:
                    info.CommonDiscInfo.LanguageSelection = new LanguageSelection?[] { LanguageSelection.BiosSettings, LanguageSelection.LanguageSelector, LanguageSelection.OptionsMenu };
                    info.CommonDiscInfo.EXEDateBuildDate = GetPlayStationEXEDate(Drive?.Letter) ?? "";
                    info.VersionAndEditions.Version = GetPlayStation2Version(Drive?.Letter) ?? "";
                    break;
                
                case KnownSystem.SonyPlayStation3:
                    info.Extras.DiscKey = (this.AddPlaceholders ? Template.RequiredValue : "");
                    info.Extras.DiscID = (this.AddPlaceholders ? Template.RequiredValue : "");
                    break;

                case KnownSystem.SonyPlayStation4:
                    info.VersionAndEditions.Version = GetPlayStation4Version(Drive?.Letter) ?? "";
                    break;

                case KnownSystem.ZAPiTGamesGameWaveFamilyEntertainmentSystem:
                    info.CopyProtection.Protection = (AddPlaceholders ? Template.RequiredIfExistsValue : "");
                    break;
            }

            // Comments is one of the few fields with odd handling
            if (string.IsNullOrEmpty(info.CommonDiscInfo.Comments))
                info.CommonDiscInfo.Comments = (AddPlaceholders ? Template.OptionalValue : "");

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
                AddIfExists(output, Template.MediaTypeField, GetFixedMediaType(info.CommonDiscInfo.Media, info.SizeAndChecksums.Layerbreak), 1);
                AddIfExists(output, Template.CategoryField, info.CommonDiscInfo.Category.LongName(), 1);
                AddIfExists(output, Template.MatchingIDsField, info.MatchedIDs, 1);
                AddIfExists(output, Template.RegionField, info.CommonDiscInfo.Region.LongName(), 1);
                AddIfExists(output, Template.LanguagesField, (info.CommonDiscInfo.Languages ?? new Language?[] { null }).Select(l => l.LongName()).ToArray(), 1);
                AddIfExists(output, Template.PlaystationLanguageSelectionViaField, (info.CommonDiscInfo.LanguageSelection ?? new LanguageSelection?[] { }).Select(l => l.ToString()).ToArray(), 1);
                AddIfExists(output, Template.DiscSerialField, info.CommonDiscInfo.Serial, 1);

                // All ringcode information goes in an indented area
                output.Add(""); output.Add("\tRingcode Information:");

                // If we have a dual-layer disc
                if (info.SizeAndChecksums.Layerbreak != default(long))
                {
                    AddIfExists(output, "Inner " + Template.MasteringRingField, info.CommonDiscInfo.MasteringRingFirstLayerDataSide, 2);
                    AddIfExists(output, "Inner " + Template.MasteringSIDField, info.CommonDiscInfo.MasteringSIDCodeFirstLayerDataSide, 2);
                    AddIfExists(output, "Inner " + Template.ToolstampField, info.CommonDiscInfo.ToolstampMasteringCodeFirstLayerDataSide, 2);
                    AddIfExists(output, "Outer " + Template.MasteringRingField, info.CommonDiscInfo.MasteringRingSecondLayerLabelSide, 2);
                    AddIfExists(output, "Outer " + Template.MasteringSIDField, info.CommonDiscInfo.MasteringSIDCodeSecondLayerLabelSide, 2);
                    AddIfExists(output, "Outer " + Template.ToolstampField, info.CommonDiscInfo.ToolstampMasteringCodeSecondLayerLabelSide, 2);
                    AddIfExists(output, "Data-Side " + Template.MouldSIDField, info.CommonDiscInfo.MouldSIDCodeFirstLayerDataSide, 2);
                    AddIfExists(output, "Label-Side " + Template.MouldSIDField, info.CommonDiscInfo.MouldSIDCodeSecondLayerLabelSide, 2);
                    AddIfExists(output, Template.AdditionalMouldField, info.CommonDiscInfo.AdditionalMouldFirstLayerDataSide, 2);
                }
                // If we have a single-layer disc
                else
                {
                    AddIfExists(output, Template.MasteringRingField, info.CommonDiscInfo.MasteringRingFirstLayerDataSide, 2);
                    AddIfExists(output, Template.MasteringSIDField, info.CommonDiscInfo.MasteringSIDCodeFirstLayerDataSide, 2);
                    AddIfExists(output, "Data-Side " + Template.MouldSIDField, info.CommonDiscInfo.MouldSIDCodeFirstLayerDataSide, 2);
                    AddIfExists(output, "Label-Side " + Template.MouldSIDField, info.CommonDiscInfo.MouldSIDCodeSecondLayerLabelSide, 2);
                    AddIfExists(output, Template.AdditionalMouldField, info.CommonDiscInfo.AdditionalMouldFirstLayerDataSide, 2);
                    AddIfExists(output, Template.ToolstampField, info.CommonDiscInfo.ToolstampMasteringCodeFirstLayerDataSide, 2);
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
                if (info.EDC.EDC != YesNo.NULL)
                {
                    output.Add("EDC:");
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
                    AddIfExists(output, Template.PVDField, info.Extras.PVD.Trim(), 1);
                    AddIfExists(output, Template.PlayStation3WiiDiscKeyField, info.Extras.DiscKey, 1);
                    AddIfExists(output, Template.PlayStation3DiscIDField, info.Extras.DiscID, 1);
                    AddIfExists(output, Template.PICField, info.Extras.PIC, 1);
                    AddIfExists(output, Template.HeaderField, info.Extras.Header, 1);
                    AddIfExists(output, Template.GameCubeWiiBCAField, info.Extras.BCA, 1);
                    AddIfExists(output, Template.XBOXSSRanges, info.Extras.SecuritySectorRanges, 1);
                }
                
                // Copy Protection section
                if (info.CopyProtection.Protection != null || info.EDC.EDC != YesNo.NULL)
                {
                    output.Add(""); output.Add("Copy Protection:");
                    if (info.EDC.EDC != YesNo.NULL)
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
                    AddIfExists(output, Template.LayerbreakField, (info.SizeAndChecksums.Layerbreak == default(long) ? null : info.SizeAndChecksums.Layerbreak.ToString()), 1);
                    AddIfExists(output, Template.SizeField, info.SizeAndChecksums.Size.ToString(), 1);
                    AddIfExists(output, Template.CRC32Field, info.SizeAndChecksums.CRC32, 1);
                    AddIfExists(output, Template.MD5Field, info.SizeAndChecksums.MD5, 1);
                    AddIfExists(output, Template.SHA1Field, info.SizeAndChecksums.SHA1, 1);
                }

                // Make sure there aren't any instances of two blank lines in a row
                string last = null;
                for (int i = 0; i < output.Count; )
                {
                    if (output[i] == last)
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

        /// <summary>
        /// Get the adjusted name of the media baed on layers, if applicable
        /// </summary>
        /// <param name="mediaType">MediaType to get the proper name for</param>
        /// <param name="layerbreak">Layerbreak value, as applicable</param>
        /// <returns>String representation of the media, including layer specification</returns>
        private string GetFixedMediaType(MediaType? mediaType, long layerbreak)
        {
            switch (mediaType)
            {
                case MediaType.DVD:
                    if (layerbreak != default(long))
                        return $"{mediaType.LongName()}-9";
                    else
                        return $"{mediaType.LongName()}-5";

                case MediaType.BluRay:
                    if (layerbreak != default(long))
                        return $"{mediaType.LongName()}-50";
                    else
                        return $"{mediaType.LongName()}-25";

                case MediaType.UMD:
                    if (layerbreak != default(long))
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

            FixOutputPaths();

            // Validate that the required program exists
            if (!File.Exists(DICPath))
                return Result.Failure("Error! Could not find DiscImageCreator!");

            return Result.Success();
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
        /// <param name="info">SubmissionInfo object representign the JSON to write out to the file</param>
        /// <returns>True on success, false on error</returns>
        private bool WriteOutputData(SubmissionInfo info)
        {
            // Check to see if the input is valid
            if (info == null)
                return false;

            // Now write out to a generic file
            try
            {
                using (StreamWriter sw = new StreamWriter(File.Open(Path.Combine(OutputDirectory, "!submissionInfo.json"), FileMode.Create, FileAccess.Write)))
                {
                    string json = JsonConvert.SerializeObject(info, Formatting.Indented);
                    sw.WriteLine(json);
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

        #region Information Extraction Methods

        /// <summary>
        /// Get the existance of an anti-modchip string from the input file, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <returns>Antimodchip existance if possible, false on error</returns>
        private bool GetAntiModchipDetected(string disc)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
                return false;

            using (StreamReader sr = File.OpenText(disc))
            {
                try
                {
                    // Check for either antimod string
                    string line = sr.ReadLine().Trim();
                    while (!sr.EndOfStream)
                    {
                        if (line.StartsWith("Detected anti-mod string"))
                            return true;
                        else if (line.StartsWith("No anti-mod string"))
                            return false;

                        line = sr.ReadLine().Trim();
                    }

                    return false;
                }
                catch
                {
                    // We don't care what the exception is right now
                    return false;
                }
            }
        }

        /// <summary>
        /// Get the current copy protection scheme, if possible
        /// </summary>
        /// <returns>Copy protection scheme if possible, null on error</returns>
        private string GetCopyProtection()
        {
            if (ScanForProtection)
                return Task.Run(() => Validators.RunProtectionScanOnPath(Drive.Letter + ":\\")).GetAwaiter().GetResult();

            return "(CHECK WITH PROTECTIONID)";
        }

        /// <summary>
        /// Get the proper datfile from the input file, if possible
        /// </summary>
        /// <param name="dat">.dat file location</param>
        /// <returns>Relevant pieces of the datfile, null on error</returns>
        private string GetDatfile(string dat)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(dat))
                return null;

            using (StreamReader sr = File.OpenText(dat))
            {
                try
                {
                    // Make sure this file is a .dat
                    if (sr.ReadLine() != "<?xml version=\"1.0\" encoding=\"UTF-8\"?>")
                        return null;
                    if (sr.ReadLine() != "<!DOCTYPE datafile PUBLIC \"-//Logiqx//DTD ROM Management Datafile//EN\" \"http://www.logiqx.com/Dats/datafile.dtd\">")
                        return null;

                    // Fast forward to the rom lines
                    while (!sr.ReadLine().TrimStart().StartsWith("<game")) ;
                    sr.ReadLine(); // <category>Games</category>
                    sr.ReadLine(); // <description>Plextor</description>

                    // Now that we're at the relevant entries, read each line in and concatenate
                    string pvd = "", line = sr.ReadLine().Trim();
                    while (line.StartsWith("<rom"))
                    {
                        pvd += line + "\n";
                        line = sr.ReadLine().Trim();
                    }

                    return pvd.TrimEnd('\n');
                }
                catch
                {
                    // We don't care what the exception is right now
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the DVD protection information, if possible
        /// </summary>
        /// <param name="cssKey">_CSSKey.txt file location</param>
        /// <param name="disc">_disc.txt file location</param>
        /// <returns>Formatted string representing the DVD protection, null on error</returns>
        private string GetDVDProtection(string cssKey, string disc)
        {
            // If one of the files doesn't exist, we can't get info from them
            if (!File.Exists(disc))
                return null;

            // Setup all of the individual pieces
            string region = null, rceProtection = null, copyrightProtectionSystemType = null, encryptedDiscKey = null, playerKey = null, decryptedDiscKey = null;

            // Get everything from _disc.txt first
            using (StreamReader sr = File.OpenText(disc))
            {
                try
                {
                    // Fast forward to the copyright information
                    while (!sr.ReadLine().Trim().StartsWith("========== CopyrightInformation ==========")) ;

                    // Now read until we hit the manufacturing information
                    string line = sr.ReadLine().Trim();
                    while (!line.StartsWith("========== ManufacturingInformation =========="))
                    {
                        if (line.StartsWith("CopyrightProtectionType"))
                            copyrightProtectionSystemType = line.Substring("CopyrightProtectionType: ".Length);
                        else if (line.StartsWith("RegionManagementInformation"))
                            region = line.Substring("RegionManagementInformation: ".Length);

                        line = sr.ReadLine().Trim();
                    }
                }
                catch { }
            }

            // Get everything from _CSSKey.txt next, if it exists
            if (File.Exists(cssKey))
            {
                using (StreamReader sr = File.OpenText(cssKey))
                {
                    try
                    {
                        // Read until the end
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine().Trim();

                            if (line.StartsWith("[001]"))
                                encryptedDiscKey = line.Substring("[001]: ".Length);
                            else if (line.StartsWith("PlayerKey"))
                                playerKey = line.Substring("PlayerKey[1]: ".Length);
                            else if (line.StartsWith("DecryptedDiscKey"))
                                decryptedDiscKey = line.Substring("DecryptedDiscKey[020]: ".Length);
                        }
                    }
                    catch { }
                }
            }

            // Now we format everything we can
            string protection = "";
            if (!String.IsNullOrEmpty(region))
                protection += $"Region: {region}\n";
            if (!String.IsNullOrEmpty(rceProtection))
                protection += $"RCE Protection: {rceProtection}\n";
            if (!String.IsNullOrEmpty(copyrightProtectionSystemType))
                protection += $"Copyright Protection System Type: {copyrightProtectionSystemType}\n";
            if (!String.IsNullOrEmpty(encryptedDiscKey))
                protection += $"Encrypted Disc Key: {encryptedDiscKey}\n";
            if (!String.IsNullOrEmpty(playerKey))
                protection += $"Player Key: {playerKey}\n";
            if (!String.IsNullOrEmpty(decryptedDiscKey))
                protection += $"Decrypted Disc Key: {decryptedDiscKey}\n";

            return protection;
        }

        /// <summary>
        /// Get the detected error count from the input files, if possible
        /// </summary>
        /// <param name="edcecc">.img_EdcEcc.txt/.img_EccEdc.txt file location</param>
        /// <returns>Error count if possible, -1 on error</returns>
        private long GetErrorCount(string edcecc)
        {
            // TODO: Better usage of _mainInfo and _c2Error for uncorrectable errors

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(edcecc))
                return -1;

            // First line of defense is the EdcEcc error file
            using (StreamReader sr = File.OpenText(edcecc))
            {
                try
                {
                    // Read in the error count whenever we find it
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine().Trim();

                        if (line.StartsWith("[NO ERROR]"))
                        {
                            return 0;
                        }
                        else if (line.StartsWith("Total errors"))
                        {
                            if (Int64.TryParse(line.Substring("Total errors: ".Length).Trim(), out long te))
                                return te;
                            else
                                return Int64.MinValue;
                        }
                    }

                    // If we haven't found anything, return -1
                    return -1;
                }
                catch
                {
                    // We don't care what the exception is right now
                    return Int64.MaxValue;
                }
            }
        }

        /// <summary>
        /// Get the full lines from the input file, if possible
        /// </summary>
        /// <param name="filename">file location</param>
        /// <returns>Full text of the file, null on error</returns>
        private string GetFullFile(string filename)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(filename))
                return null;

            return string.Join("\n", File.ReadAllLines(filename));
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

        /// <summary>
        /// Get the layerbreak from the input file, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <param name="ignoreFirst">True if the first sector length is to be ignored, false otherwise</param>
        /// <returns>Layerbreak if possible, null on error</returns>
        private string GetLayerbreak(string disc, bool ignoreFirst)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
                return null;

            using (StreamReader sr = File.OpenText(disc))
            {
                try
                {
                    // Fast forward to the layerbreak
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        // We definitely found a single-layer disc
                        if (line.Contains("NumberOfLayers: Single Layer"))
                        {
                            return null;
                        }
                        else if (line.Trim().StartsWith("========== SectorLength =========="))
                        {
                            // Skip the first one and unset the flag
                            if (ignoreFirst)
                                ignoreFirst = false;
                            else
                                break;
                        }

                        line = sr.ReadLine();
                    }

                    // Now that we're at the layerbreak line, attempt to get the decimal version
                    return sr.ReadLine().Trim().Split(' ')[1];
                }
                catch
                {
                    // We don't care what the exception is right now
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the detected missing EDC count from the input files, if possible
        /// </summary>
        /// <param name="edcecc">.img_EdcEcc.txt file location</param>
        /// <returns>Missing EDC count if possible, -1 on error</returns>
        private long GetMissingEDCCount(string edcecc)
        {
            // If one of the files doesn't exist, we can't get info from them
            if (!File.Exists(edcecc))
                return -1;

            // First line of defense is the EdcEcc error file
            using (StreamReader sr = File.OpenText(edcecc))
            {
                try
                {
                    // Fast forward to the PVD
                    string line = sr.ReadLine();
                    while (!line.StartsWith("[INFO] Number of sector(s) where EDC doesn't exist: "))
                        line = sr.ReadLine();

                    return Int64.Parse(line.Remove(0, "[INFO] Number of sector(s) where EDC doesn't exist: ".Length).Trim());
                }
                catch
                {
                    // We don't care what the exception is right now
                    return -1;
                }
            }
        }

        /// <summary>
        /// Get the PVD from the input file, if possible
        /// </summary>
        /// <param name="mainInfo">_mainInfo.txt file location</param>
        /// <returns>Newline-deliminated PVD if possible, null on error</returns>
        private string GetPVD(string mainInfo)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(mainInfo))
                return null;

            using (StreamReader sr = File.OpenText(mainInfo))
            {
                try
                {
                    // Make sure we're in the right sector
                    while (!sr.ReadLine().StartsWith("========== LBA[000016, 0x00010]: Main Channel ==========")) ;

                    // Fast forward to the PVD
                    while (!sr.ReadLine().StartsWith("0310")) ;

                    // Now that we're at the PVD, read each line in and concatenate
                    string pvd = "";
                    for (int i = 0; i < 6; i++)
                        pvd += sr.ReadLine() + "\n"; // 320-370

                    return pvd;
                }
                catch
                {
                    // We don't care what the exception is right now
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the EXE date from a PlayStation disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>EXE date in "yyyy-mm-dd" format if possible, null on error</returns>
        private string GetPlayStationEXEDate(char? driveLetter)
        {
            // If there's no drive letter, we can't do this part
            if (driveLetter == null)
                return null;

            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            if (!Directory.Exists(drivePath))
                return null;

            // Get the two paths that we will need to check
            string psxExePath = Path.Combine(drivePath, "PSX.EXE");
            string systemCnfPath = Path.Combine(drivePath, "SYSTEM.CNF");

            // Try both of the common paths that contain information
            string exeName = null;
            if (File.Exists(psxExePath))
            {
                exeName = "PSX.EXE";
            }
            else if (File.Exists(systemCnfPath))
            {
                // Let's try reading SYSTEM.CNF to find the "BOOT" value
                try
                {
                    using (StreamReader sr = File.OpenText(systemCnfPath))
                    {
                        // Not assuming proper ordering, just in case
                        string line = sr.ReadLine();
                        while (!line.StartsWith("BOOT"))
                            line = sr.ReadLine();

                        // Once it finds the "BOOT" line, extract the name
                        exeName = Regex.Match(line, @"BOOT.*?=\s*cdrom.?:\\?(.*?);.*").Groups[1].Value;
                    }
                }
                catch
                {
                    // We don't care what the error was
                    return null;
                }
            }
            else
            {
                return null;
            }

            // Now that we have the EXE name, try to get the fileinfo for it
            string exePath = Path.Combine(drivePath, exeName);
            if (!File.Exists(exePath))
                return null;

            // Fix the Y2K timestamp issue
            FileInfo fi = new FileInfo(exePath);
            DateTime dt = new DateTime(fi.LastWriteTimeUtc.Year >= 1900 && fi.LastWriteTimeUtc.Year < 1920 ? 2000 + fi.LastWriteTimeUtc.Year % 100 : fi.LastWriteTimeUtc.Year,
                fi.LastWriteTimeUtc.Month, fi.LastWriteTimeUtc.Day);
            return dt.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// Get the version from a PlayStation 2 disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>Game version if possible, null on error</returns>
        private string GetPlayStation2Version(char? driveLetter)
        {
            // If there's no drive letter, we can't do this part
            if (driveLetter == null)
                return null;

            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            if (!Directory.Exists(drivePath))
                return null;

            // If we can't find SYSTEM.CNF, we don't have a PlayStation 2 disc
            string systemCnfPath = Path.Combine(drivePath, "SYSTEM.CNF");
            if (!File.Exists(systemCnfPath))
                return null;

            // Let's try reading SYSTEM.CNF to find the "VER" value
            try
            {
                using (StreamReader sr = File.OpenText(systemCnfPath))
                {
                    // Not assuming proper ordering, just in case
                    string line = sr.ReadLine();
                    while (!line.StartsWith("VER"))
                        line = sr.ReadLine();

                    // Once it finds the "VER" line, extract the version
                    return Regex.Match(line, @"VER\s*=\s*(.*)").Groups[1].Value;
                }
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
        }

        /// <summary>
        /// Get the hex contents of the PIC file
        /// </summary>
        /// <param name="picPath">Path to the PIC.bin file associated with the dump</param>
        /// <returns>PIC data as a hex string if possible, null on error</returns>
        /// <remarks>https://stackoverflow.com/questions/9932096/add-separator-to-string-at-every-n-characters</remarks>
        private string GetPIC(string picPath)
        {
            // If the file doesn't exist, we can't get the info
            if (!File.Exists(picPath))
                return null;

            try
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(picPath)))
                {
                    string hex = BitConverter.ToString(br.ReadBytes(140)).Replace("-", string.Empty);
                    return Regex.Replace(hex, ".{32}", "$0\n");
                }
            }
            catch
            {
                // We don't care what the error was right now
                return null;
            }
        }

        /// <summary>
        /// Get the version from a PlayStation 4 disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>Game version if possible, null on error</returns>
        private string GetPlayStation4Version(char? driveLetter)
        {
            // If there's no drive letter, we can't do this part
            if (driveLetter == null)
                return null;

            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            if (!Directory.Exists(drivePath))
                return null;

            // If we can't find param.sfo, we don't have a PlayStation 4 disc
            string paramSfoPath = Path.Combine(drivePath, "bd", "param.sfo");
            if (!File.Exists(paramSfoPath))
                return null;

            // Let's try reading param.sfo to find the version at the end of the file
            try
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(paramSfoPath)))
                {
                    br.BaseStream.Seek(-0x08, SeekOrigin.End);
                    return new string(br.ReadChars(5));
                }
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
        }

        /// <summary>
        /// Get the header from a Sega Saturn or Sega CD / Mega CD disc, if possible
        /// </summary>
        /// <param name="mainInfo">_mainInfo.txt file location</param>
        /// <returns>Header as a byte array if possible, null on error</returns>
        private string GetSegaHeader(string mainInfo)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(mainInfo))
                return null;

            using (StreamReader sr = File.OpenText(mainInfo))
            {
                try
                {
                    // Make sure we're in the right sector
                    while (!sr.ReadLine().StartsWith("========== LBA[000000, 0000000]: Main Channel ==========")) ;

                    // Fast forward to the header
                    while (!sr.ReadLine().Trim().StartsWith("+0 +1 +2 +3 +4 +5 +6 +7  +8 +9 +A +B +C +D +E +F")) ;

                    // Now that we're at the Header, read each line in and concatenate
                    string header = "";
                    for (int i = 0; i < 32; i++)
                        header += sr.ReadLine() + "\n"; // 0000-01F0

                    return header;
                }
                catch
                {
                    // We don't care what the exception is right now
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the build info from a Sega CD disc, if possible
        /// </summary>
        /// <<param name="segaHeader">String representing a formatter variant of the  Sega CD header</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        /// <remarks>Note that this works for MOST headers, except ones where the copyright stretches > 1 line</remarks>
        private bool GetSegaCDBuildInfo(string segaHeader, out string serial, out string date)
        {
            serial = null; date = null;

            // If the input header is null, we can't do a thing
            if (String.IsNullOrWhiteSpace(segaHeader))
                return false;

            // Now read it in cutting it into lines for easier parsing
            try
            {
                string[] header = segaHeader.Split('\n');
                string serialVersionLine = header[8].Substring(58);
                string dateLine = header[1].Substring(58);
                serial = serialVersionLine.Substring(3, 7);
                date = dateLine.Substring(8).Trim();

                // Properly format the date string, if possible
                string[] dateSplit = date.Split('.');

                if (dateSplit.Length == 1)
                    dateSplit = new string[] { date.Substring(0, 4), date.Substring(4) };

                string month = dateSplit[1];
                switch (month)
                {
                    case "JAN":
                        dateSplit[1] = "01";
                        break;
                    case "FEB":
                        dateSplit[1] = "02";
                        break;
                    case "MAR":
                        dateSplit[1] = "03";
                        break;
                    case "APR":
                        dateSplit[1] = "04";
                        break;
                    case "MAY":
                        dateSplit[1] = "05";
                        break;
                    case "JUN":
                        dateSplit[1] = "06";
                        break;
                    case "JUL":
                        dateSplit[1] = "07";
                        break;
                    case "AUG":
                        dateSplit[1] = "08";
                        break;
                    case "SEP":
                        dateSplit[1] = "09";
                        break;
                    case "OCT":
                        dateSplit[1] = "10";
                        break;
                    case "NOV":
                        dateSplit[1] = "11";
                        break;
                    case "DEC":
                        dateSplit[1] = "12";
                        break;
                    default:
                        dateSplit[1] = "00";
                        break;
                }

                date = string.Join("-", dateSplit);

                return true;
            }
            catch
            {
                // We don't care what the error is
                return false;
            }
        }

        /// <summary>
        /// Get the build info from a Saturn disc, if possible
        /// </summary>
        /// <<param name="segaHeader">String representing a formatter variant of the Saturn header</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private bool GetSaturnBuildInfo(string segaHeader, out string serial, out string version, out string date)
        {
            serial = null; version = null; date = null;

            // If the input header is null, we can't do a thing
            if (String.IsNullOrWhiteSpace(segaHeader))
                return false;

            // Now read it in cutting it into lines for easier parsing
            try
            {
                string[] header = segaHeader.Split('\n');
                string serialVersionLine = header[2].Substring(58);
                string dateLine = header[3].Substring(58);
                serial = serialVersionLine.Substring(0, 8);
                version = serialVersionLine.Substring(10, 6);
                date = dateLine.Substring(0, 8);
                return true;
            }
            catch
            {
                // We don't care what the error is
                return false;
            }
        }

        /// <summary>
        /// Get the UMD auxiliary info from the outputted files, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private bool GetUMDAuxInfo(string disc, out string title, out Category? umdcat, out string umdversion, out string umdlayer, out long umdsize)
        {
            title = null; umdcat = null; umdversion = null; umdlayer = null; umdsize = -1;

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
                return false;

            using (StreamReader sr = File.OpenText(disc))
            {
                try
                {
                    // Loop through everything to get the first instance of each required field
                    string line = string.Empty;
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine().Trim();

                        if (line.StartsWith("TITLE") && title == null)
                            title = line.Substring("TITLE: ".Length);
                        else if (line.StartsWith("DISC_VERSION") && umdversion == null)
                            umdversion = line.Split(' ')[1];
                        else if (line.StartsWith("pspUmdTypes"))
                            umdcat = GetUMDCategory(line.Split(' ')[1]);
                        else if (line.StartsWith("L0 length"))
                            umdlayer = line.Split(' ')[2];
                        else if (line.StartsWith("FileSize:"))
                            umdsize = Int64.Parse(line.Split(' ')[1]);
                    }

                    // If the L0 length is the size of the full disc, there's no layerbreak
                    if (Int64.Parse(umdlayer) * 2048 == umdsize)
                        umdlayer = null;

                    return true;
                }
                catch
                {
                    // We don't care what the exception is right now
                    return false;
                }
            }
        }

        /// <summary>
        /// Determine the category based on the UMDImageCreator string
        /// </summary>
        /// <param name="region">String representing the category</param>
        /// <returns>Category, if possible</returns>
        private Category? GetUMDCategory(string category)
        {
            switch (category)
            {
                case "GAME":
                    return Category.Games;
                case "VIDEO":
                    return Category.Video;
                case "AUDIO":
                    return Category.Audio;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the XBOX/360 auxiliary info from the outputted files, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private bool GetXBOXAuxInfo(string disc, out string dmihash, out string pfihash, out string sshash, out string ss, out string ssver)
        {
            dmihash = null; pfihash = null; sshash = null; ss = null; ssver = null;

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
                return false;

            using (StreamReader sr = File.OpenText(disc))
            {
                try
                {
                    // Fast forward to the Security Sector version and read it
                    while (!sr.ReadLine().Trim().StartsWith("CPR_MAI Key")) ;
                    ssver = sr.ReadLine().Trim().Split(' ')[4]; // "Version of challenge table: <VER>"

                    // Fast forward to the Security Sector Ranges
                    while (!sr.ReadLine().Trim().StartsWith("Number of security sector ranges:")) ;

                    // Now that we're at the ranges, read each line in and concatenate
                    Regex layerRegex = new Regex(@"Layer [01].*, startLBA-endLBA:\s*(\d+)-\s*(\d+)");
                    string line = sr.ReadLine().Trim();
                    while (!line.StartsWith("========== Unlock 2 state(wxripper) =========="))
                    {
                        // If we have a recognized line format, parse it
                        if (line.StartsWith("Layer "))
                        {
                            var match = layerRegex.Match(line);
                            ss += $"{match.Groups[1]}-{match.Groups[2]}\n";
                        }

                        line = sr.ReadLine().Trim();
                    }

                    // Fast forward to the aux hashes
                    while (!line.StartsWith("<rom"))
                        line = sr.ReadLine().Trim();

                    // Read in the hashes to the proper parts
                    while (line.StartsWith("<rom"))
                    {
                        if (line.Contains("SS.bin"))
                            sshash = line;
                        else if (line.Contains("PFI.bin"))
                            pfihash = line;
                        else if (line.Contains("DMI.bin"))
                            dmihash = line;

                        if (sr.EndOfStream)
                            break;

                        line = sr.ReadLine().Trim();
                    }

                    return true;
                }
                catch
                {
                    // We don't care what the exception is right now
                    return false;
                }
            }
        }

        /// <summary>
        /// Get the XOX serial info from the DMI.bin file, if possible
        /// </summary>
        /// <param name="dmi">DMI.bin file location</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private bool GetXBOXDMIInfo(string dmi, out string serial, out string version, out Region? region)
        {
            serial = null; version = null; region = Region.World;

            if (!File.Exists(dmi))
                return false;

            using (BinaryReader br = new BinaryReader(File.OpenRead(dmi)))
            {
                try
                {
                    br.BaseStream.Seek(8, SeekOrigin.Begin);
                    char[] str = br.ReadChars(8);

                    serial = $"{str[0]}{str[1]}-{str[2]}{str[3]}{str[4]}";
                    version = $"1.{str[5]}{str[6]}";
                    region = GetXBOXRegion(str[7]);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Get the XBOX 360 serial info from the DMI.bin file, if possible
        /// </summary>
        /// <param name="dmi">DMI.bin file location</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private bool GetXBOX360DMIInfo(string dmi, out string serial, out string version, out Region? region)
        {
            serial = null; version = null; region = null;

            if (!File.Exists(dmi))
                return false;

            using (BinaryReader br = new BinaryReader(File.OpenRead(dmi)))
            {
                try
                {
                    br.BaseStream.Seek(64, SeekOrigin.Begin);
                    char[] str = br.ReadChars(14);

                    serial = $"{str[0]}{str[1]}-{str[2]}{str[3]}{str[4]}{str[5]}";
                    version = $"1.{str[6]}{str[7]}";
                    region = GetXBOXRegion(str[8]);
                    // str[9], str[10], str[11] - unknown purpose
                    // str[12], str[13] - disc <12> of <13>
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Determine the region based on the XBOX serial character
        /// </summary>
        /// <param name="region">Character denoting the region</param>
        /// <returns>Region, if possible</returns>
        private Region? GetXBOXRegion(char region)
        {
            switch (region)
            {
                case 'W':
                    return Region.World;
                case 'A':
                    return Region.USA;
                case 'J':
                    return Region.JapanAsia;
                case 'E':
                    return Region.Europe;
                case 'K':
                    return Region.USAJapan;
                case 'L':
                    return Region.USAEurope;
                case 'H':
                    return Region.JapanEurope;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the write offset from the input file, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <returns>Sample write offset if possible, null on error</returns>
        private string GetWriteOffset(string disc)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
            {
                return null;
            }

            using (StreamReader sr = File.OpenText(disc))
            {
                try
                {
                    // Fast forward to the offsets
                    while (!sr.ReadLine().Trim().StartsWith("========== Offset")) ;
                    sr.ReadLine(); // Combined Offset
                    sr.ReadLine(); // Drive Offset
                    sr.ReadLine(); // Separator line

                    // Now that we're at the offsets, attempt to get the sample offset
                    return sr.ReadLine().Split(' ').LastOrDefault();
                }
                catch
                {
                    // We don't care what the exception is right now
                    return null;
                }
            }
        }

        #endregion
    }
}
