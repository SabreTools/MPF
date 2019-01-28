using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DICUI.Data;

namespace DICUI.Utilities
{
    /// <summary>
    /// Represents information for a single drive
    /// </summary>
    public class Drive
    {
        public char Letter { get; private set; }
        public bool IsFloppy { get; private set; }
        public string VolumeLabel { get; private set; }
        public bool MarkedActive { get; private set; }

        private Drive(char letter, string volumeLabel, bool isFloppy, bool markedActive)
        {
            this.Letter = letter;
            this.IsFloppy = isFloppy;
            this.VolumeLabel = volumeLabel;
            this.MarkedActive = markedActive;
        }

        public static Drive Floppy(char letter) => new Drive(letter, null, true, true);
        public static Drive Optical(char letter, string volumeLabel, bool active) => new Drive(letter, volumeLabel, false, active);
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
        public Drive Drive;
        public KnownSystem? System;
        public MediaType? Type;
        public bool IsFloppy { get => Drive.IsFloppy; }
        public Parameters DICParameters;

        // extra DIC arguments
        public bool QuietMode;
        public bool ParanoidMode;
        public bool ScanForProtection;
        public int RereadAmountC2;

        // External process information
        private Process dicProcess;

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
        /// Eject the disc using DIC
        /// </summary>
        public async void EjectDisc()
        {
            // Validate that the required program exists
            if (!File.Exists(DICPath))
                return;

            CancelDumping();

            // Validate we're not trying to eject a floppy disk
            if (IsFloppy)
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

            // Verify dump output and save it
            progress?.Report(Result.Success("Gathering submission information... please wait!"));
            result = await Task.Run(() => VerifyAndSaveDumpOutput(progress));
            progress?.Report(Result.Success("All submission information gathered!"));

            return result;
        }

        #endregion

        #region Public for Testing Purposes

        /// <summary>
        /// Fix output paths to strip out any invalid characters
        /// </summary>
        public void FixOutputPaths()
        {
            try
            {
                // Cache if we had a directory separator or not
                bool endedWithDirectorySeparator = OutputDirectory.EndsWith(Path.DirectorySeparatorChar.ToString());

                // Normalize the path so the below filters work better
                string combinedPath = Path.Combine(OutputDirectory, OutputFilename);
                OutputDirectory = Path.GetDirectoryName(combinedPath);
                OutputFilename = Path.GetFileName(combinedPath);

                // If we have either a blank filename or path, just exit
                if (String.IsNullOrWhiteSpace(OutputFilename) || String.IsNullOrWhiteSpace(OutputDirectory))
                    return;

                // Take care of extra path characters
                OutputDirectory = new StringBuilder(OutputDirectory.Replace('.', '_').Replace('&', '_'))
                    .Replace(':', '_', 0, OutputDirectory.LastIndexOf(':') == -1 ? 0 : OutputDirectory.LastIndexOf(':')).ToString();
                OutputFilename = new StringBuilder(OutputFilename.Replace('&', '_'))
                    .Replace('.', '_', 0, OutputFilename.LastIndexOf('.') == -1 ? 0 : OutputFilename.LastIndexOf('.')).ToString();

                // Sanitize everything else
                foreach (char c in Path.GetInvalidPathChars())
                    OutputDirectory = OutputDirectory.Replace(c, '_');
                foreach (char c in Path.GetInvalidFileNameChars())
                    OutputFilename = OutputFilename.Replace(c, '_');

                // If we had a directory separator at the end before, add it again
                if (endedWithDirectorySeparator)
                    OutputDirectory += Path.DirectorySeparatorChar;
            }
            catch
            {
                // We don't care what the error was
                return;
            }
        }

        /// <summary>
        /// Checks if the parameters are valid
        /// </summary>
        /// <returns>True if the configuration is valid, false otherwise</returns>
        public bool ParametersValid()
        {
            return DICParameters.IsValid() && !(IsFloppy ^ Type == MediaType.FloppyDisk);
        }

        /// <summary>
        /// Ensures that all required output files have been created
        /// </summary>
        /// <returns></returns>
        public bool FoundAllFiles()
        {
            // First, sanitized the output filename to strip off any potential extension
            string outputFilename = Path.GetFileNameWithoutExtension(OutputFilename);

            // Now ensure that all required files exist
            string combinedBase = Path.Combine(OutputDirectory, outputFilename);
            switch (Type)
            {
                case MediaType.CDROM:
                case MediaType.GDROM: // TODO: Verify GD-ROM outputs this
                    return File.Exists(combinedBase + ".c2")
                        && File.Exists(combinedBase + ".ccd")
                        && File.Exists(combinedBase + ".cue")
                        && File.Exists(combinedBase + ".dat")
                        && File.Exists(combinedBase + ".img")
                        && (System == KnownSystem.AudioCD || File.Exists(combinedBase + ".img_EdcEcc.txt"))
                        && (System == KnownSystem.AudioCD || File.Exists(combinedBase + ".scm"))
                        && File.Exists(combinedBase + ".sub")
                        && File.Exists(combinedBase + "_c2Error.txt")
                        && File.Exists(combinedBase + "_cmd.txt")
                        && File.Exists(combinedBase + "_disc.txt")
                        && File.Exists(combinedBase + "_drive.txt")
                        && File.Exists(combinedBase + "_img.cue")
                        && File.Exists(combinedBase + "_mainError.txt")
                        && File.Exists(combinedBase + "_mainInfo.txt")
                        && File.Exists(combinedBase + "_subError.txt")
                        && File.Exists(combinedBase + "_subInfo.txt")
                        // && File.Exists(combinedBase + "_subIntention.txt")
                        && (File.Exists(combinedBase + "_subReadable.txt") || File.Exists(combinedBase + "_sub.txt"))
                        && File.Exists(combinedBase + "_volDesc.txt");
                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                case MediaType.NintendoGameCube:
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
        /// <returns>Dictionary containing mapped output values, null on error</returns>
        /// <remarks>TODO: Make sure that all special formats are accounted for</remarks>
        private Dictionary<string, string> ExtractOutputInformation(IProgress<Result> progress)
        {
            // Ensure the current disc combination should exist
            if (!Validators.GetValidMediaTypes(System).Contains(Type))
            {
                return null;
            }

            // Sanitize the output filename to strip off any potential extension
            string outputFilename = Path.GetFileNameWithoutExtension(OutputFilename);

            // Check that all of the relevant files are there
            if (!FoundAllFiles())
            {
                return null;
            }

            // Create the output dictionary with all user-inputted values by default
            string combinedBase = Path.Combine(OutputDirectory, outputFilename);
            Dictionary<string, string> mappings = new Dictionary<string, string>
            {
                { Template.TitleField, Template.RequiredValue },
                { Template.DiscNumberField, Template.OptionalValue },
                { Template.DiscTitleField, Template.OptionalValue },
                { Template.SystemField, System.Name() },
                { Template.MediaTypeField, Type.Name() },
                { Template.CategoryField, "Games" },
                { Template.RegionField, "World (CHANGE THIS)" },
                { Template.LanguagesField, "Klingon (CHANGE THIS)" },
                { Template.DiscSerialField, Template.RequiredIfExistsValue },
                { Template.BarcodeField, Template.OptionalValue},
                { Template.CommentsField, Template.OptionalValue },
                { Template.ContentsField, Template.OptionalValue },
                { Template.VersionField, Template.RequiredIfExistsValue },
                { Template.EditionField, "Original (VERIFY THIS)" },
                { Template.DATField, GetDatfile(combinedBase + ".dat") },
            };

            // Now we want to do a check by MediaType and extract all required info
            switch (Type)
            {
                case MediaType.CDROM:
                case MediaType.GDROM: // TODO: Verify GD-ROM outputs this
                    mappings[Template.MasteringRingField] = Template.RequiredIfExistsValue;
                    mappings[Template.MasteringSIDField] = Template.RequiredIfExistsValue;
                    mappings[Template.MouldSIDField] = Template.RequiredIfExistsValue;
                    mappings[Template.AdditionalMouldField] = Template.RequiredIfExistsValue;
                    mappings[Template.ToolstampField] = Template.RequiredIfExistsValue;
                    mappings[Template.PVDField] = GetPVD(combinedBase + "_mainInfo.txt") ?? "Disc has no PVD";
                    mappings[Template.ErrorCountField] = GetErrorCount(combinedBase + ".img_EdcEcc.txt").ToString();
                    mappings[Template.CuesheetField] = GetFullFile(combinedBase + ".cue") ?? "";
                    mappings[Template.WriteOffsetField] = GetWriteOffset(combinedBase + "_disc.txt") ?? "";

                    // System-specific options
                    switch (System)
                    {
                        case KnownSystem.AppleMacintosh:
                        case KnownSystem.EnhancedCD:
                        case KnownSystem.EnhancedDVD:
                        case KnownSystem.EnhancedBD:
                        case KnownSystem.IBMPCCompatible:
                        case KnownSystem.RainbowDisc:
                            mappings[Template.ISBNField] = Template.OptionalValue;

                            progress?.Report(Result.Success("Running copy protection scan... this might take a while!"));
                            mappings[Template.CopyProtectionField] = GetCopyProtection();
                            progress?.Report(Result.Success("Copy protection scan complete!"));

                            if (File.Exists(combinedBase + "_subIntention.txt"))
                            {
                                FileInfo fi = new FileInfo(combinedBase + "_subIntention.txt");
                                if (fi.Length > 0)
                                {
                                    mappings[Template.SubIntentionField] = GetFullFile(combinedBase + "_subIntention.txt") ?? "";
                                }
                            }

                            break;
                        case KnownSystem.SegaCDMegaCD:
                            mappings[Template.SegaHeaderField] = GetSegaHeader(combinedBase + "_mainInfo.txt") ?? "";

                            // Take only the last 16 lines for Sega CD
                            if (!string.IsNullOrEmpty(mappings[Template.SegaHeaderField]))
                                mappings[Template.SegaHeaderField] = string.Join("\n", mappings[Template.SegaHeaderField].Split('\n').Skip(16));

                            if (GetSegaCDBuildInfo(mappings[Template.SegaHeaderField], out string scdSerial, out string fixedDate))
                            {
                                mappings[Template.DiscSerialField] = scdSerial ?? "";
                                mappings[Template.SegaBuildDateField] = fixedDate ?? "";
                            }

                            break;
                        case KnownSystem.SegaSaturn:
                            mappings[Template.SegaHeaderField] = GetSegaHeader(combinedBase + "_mainInfo.txt") ?? "";

                            // Take only the first 16 lines for Saturn
                            if (!string.IsNullOrEmpty(mappings[Template.SegaHeaderField]))
                                mappings[Template.SegaHeaderField] = string.Join("\n", mappings[Template.SegaHeaderField].Split('\n').Take(16));

                            if (GetSaturnBuildInfo(mappings[Template.SegaHeaderField], out string saturnSerial, out string version, out string buildDate))
                            {
                                mappings[Template.DiscSerialField] = saturnSerial ?? "";
                                mappings[Template.VersionField] = version ?? "";
                                mappings[Template.SegaBuildDateField] = buildDate ?? "";
                            }

                            break;
                        case KnownSystem.SonyPlayStation:
                            mappings[Template.PlaystationEXEDateField] = GetPlayStationEXEDate(Drive.Letter) ?? "";
                            mappings[Template.PlayStationEDCField] = GetMissingEDCCount(combinedBase + ".img_EdcEcc.txt") > 0 ? "No" : "Yes";
                            mappings[Template.PlayStationAntiModchipField] = GetAntiModchipDetected(combinedBase + "_disc.txt") ? "Yes" : "No";
                            mappings[Template.PlayStationLibCryptField] = "No";
                            if (File.Exists(combinedBase + "_subIntention.txt"))
                            {
                                FileInfo fi = new FileInfo(combinedBase + "_subIntention.txt");
                                if (fi.Length > 0)
                                {
                                    mappings[Template.PlayStationLibCryptField] = "Yes";
                                    mappings[Template.SubIntentionField] = GetFullFile(combinedBase + "_subIntention.txt") ?? "";
                                }
                            }

                            break;
                        case KnownSystem.SonyPlayStation2:
                            mappings[Template.PlaystationEXEDateField] = GetPlayStationEXEDate(Drive.Letter) ?? "";
                            mappings[Template.VersionField] = GetPlayStation2Version(Drive.Letter) ?? "";
                            break;
                    }

                    break;
                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                    bool isXbox = (System == KnownSystem.MicrosoftXBOX || System == KnownSystem.MicrosoftXBOX360);
                    string layerbreak = GetLayerbreak(combinedBase + "_disc.txt", isXbox) ?? "";

                    // If we have a single-layer disc
                    if (String.IsNullOrWhiteSpace(layerbreak))
                    {
                        switch (Type)
                        {
                            case MediaType.DVD:
                                mappings[Template.MediaTypeField] += "-5";
                                break;
                            case MediaType.BluRay:
                                mappings[Template.MediaTypeField] += "-25";
                                break;
                        }
                        mappings[Template.MasteringRingField] = Template.RequiredIfExistsValue;
                        mappings[Template.MasteringSIDField] = Template.RequiredIfExistsValue;
                        mappings["Label-Side " + Template.MouldSIDField] = Template.RequiredIfExistsValue;
                        mappings["Data-Side " + Template.MouldSIDField] = Template.RequiredIfExistsValue;
                        mappings[Template.AdditionalMouldField] = Template.RequiredIfExistsValue;
                        mappings[Template.ToolstampField] = Template.RequiredIfExistsValue;
                        mappings[Template.PVDField] = GetPVD(combinedBase + "_mainInfo.txt") ?? "";
                    }
                    // If we have a dual-layer disc
                    else
                    {
                        switch (Type)
                        {
                            case MediaType.DVD:
                                mappings[Template.MediaTypeField] += "-9";
                                break;
                            case MediaType.BluRay:
                                mappings[Template.MediaTypeField] += "-50";
                                break;
                        }
                        mappings["Outer " + Template.MasteringRingField] = Template.RequiredIfExistsValue;
                        mappings["Inner " + Template.MasteringRingField] = Template.RequiredIfExistsValue;
                        mappings["Outer " + Template.MasteringSIDField] = Template.RequiredIfExistsValue;
                        mappings["Inner " + Template.MasteringSIDField] = Template.RequiredIfExistsValue;
                        mappings["Label-Side " + Template.MouldSIDField] = Template.RequiredIfExistsValue;
                        mappings["Data-Side " + Template.MouldSIDField] = Template.RequiredIfExistsValue;
                        mappings[Template.AdditionalMouldField] = Template.RequiredIfExistsValue;
                        mappings["Outer " + Template.ToolstampField] = Template.RequiredIfExistsValue;
                        mappings["Inner " + Template.ToolstampField] = Template.RequiredIfExistsValue;
                        mappings[Template.PVDField] = GetPVD(combinedBase + "_mainInfo.txt") ?? "";
                        mappings[Template.LayerbreakField] = layerbreak;
                    }

                    // System-specific options
                    switch (System)
                    {
                        case KnownSystem.AppleMacintosh:
                        case KnownSystem.EnhancedCD:
                        case KnownSystem.EnhancedDVD:
                        case KnownSystem.EnhancedBD:
                        case KnownSystem.IBMPCCompatible:
                        case KnownSystem.RainbowDisc:
                            mappings[Template.ISBNField] = Template.OptionalValue;

                            progress?.Report(Result.Success("Running copy protection scan... this might take a while!"));
                            mappings[Template.CopyProtectionField] = GetCopyProtection();
                            progress?.Report(Result.Success("Copy protection scan complete!"));

                            if (File.Exists(combinedBase + "_subIntention.txt"))
                            {
                                FileInfo fi = new FileInfo(combinedBase + "_subIntention.txt");
                                if (fi.Length > 0)
                                {
                                    mappings[Template.SubIntentionField] = GetFullFile(combinedBase + "_subIntention.txt") ?? "";
                                }
                            }

                            break;
                        case KnownSystem.DVDVideo:
                            mappings[Template.CopyProtectionField] = GetDVDProtection(combinedBase + "_CSSKey.txt", combinedBase + "_disc.txt") ?? "";
                            break;
                        case KnownSystem.MicrosoftXBOX:
                        case KnownSystem.MicrosoftXBOX360:
                            if (GetXBOXAuxInfo(combinedBase + "_disc.txt", out string dmihash, out string pfihash, out string sshash, out string ss, out string ssver))
                            {
                                mappings[Template.XBOXDMIHash] = dmihash ?? "";
                                mappings[Template.XBOXPFIHash] = pfihash ?? "";
                                mappings[Template.XBOXSSHash] = sshash ?? "";
                                mappings[Template.XBOXSSVersion] = ssver ?? "";
                                mappings[Template.XBOXSSRanges] = ss ?? "";
                            }

                            break;
                        case KnownSystem.SonyPlayStation2:
                            mappings[Template.PlaystationEXEDateField] = GetPlayStationEXEDate(Drive.Letter) ?? "";
                            mappings[Template.VersionField] = GetPlayStation2Version(Drive.Letter) ?? "";
                            break;
                        case KnownSystem.SonyPlayStation4:
                            mappings[Template.PlayStation4PICField] = GetPlayStation4PIC(Path.Combine(OutputDirectory, "PIC.bin")) ?? "";
                            mappings[Template.VersionField] = GetPlayStation4Version(Drive.Letter) ?? "";
                            break;
                    }
                    break;

                case MediaType.UMD:
                    mappings[Template.PVDField] = GetPVD(combinedBase + "_mainInfo.txt") ?? "";
                    mappings[Template.DATField] = Template.RequiredValue + " [Not automatically generated for UMD]";
                    if (GetUMDAuxInfo(combinedBase + "_disc.txt", out string title, out string umdversion, out string umdlayer))
                    {
                        mappings[Template.TitleField] = title ?? "";
                        mappings[Template.VersionField] = umdversion ?? "";
                        if (!String.IsNullOrWhiteSpace(umdlayer))
                            mappings[Template.LayerbreakField] = umdlayer ?? "";
                    }

                    break;
            }

            return mappings;
        }

        /// <summary>
        /// Format the output data in a human readable way, separating each printed line into a new item in the list
        /// </summary>
        /// <param name="info">Information dictionary that should contain normalized values</param>
        /// <returns>List of strings representing each line of an output file, null on error</returns>
        /// <remarks>TODO: Get full list of customizable stuff for other systems</remarks>
        private List<string> FormatOutputData(Dictionary<string, string> info)
        {
            // Check to see if the inputs are valid
            if (info == null)
            {
                return null;
            }

            try
            {
                List<string> output = new List<string>();

                output.Add(Template.TitleField + ": " + info[Template.TitleField]);
                output.Add(Template.DiscNumberField + ": " + info[Template.DiscNumberField]);
                output.Add(Template.DiscTitleField + ": " + info[Template.DiscTitleField]);
                output.Add(Template.SystemField + ": " + info[Template.SystemField]);
                output.Add(Template.MediaTypeField + ": " + info[Template.MediaTypeField]);
                output.Add(Template.CategoryField + ": " + info[Template.CategoryField]);
                output.Add(Template.RegionField + ": " + info[Template.RegionField]);
                output.Add(Template.LanguagesField + ": " + info[Template.LanguagesField]);
                output.Add(Template.DiscSerialField + ": " + info[Template.DiscSerialField]);
                switch (System)
                {
                    case KnownSystem.SegaCDMegaCD:
                    case KnownSystem.SegaSaturn:
                        output.Add(Template.SegaBuildDateField + ": " + info[Template.SegaBuildDateField]);
                        break;
                    case KnownSystem.SonyPlayStation:
                    case KnownSystem.SonyPlayStation2:
                        output.Add(Template.PlaystationEXEDateField + ": " + info[Template.PlaystationEXEDateField]);
                        break;
                }
                output.Add("Ringcode Information:");
                switch (Type)
                {
                    case MediaType.CDROM:
                    case MediaType.GDROM:
                    case MediaType.DVD:
                    case MediaType.HDDVD:
                    case MediaType.BluRay:
                        // If we have a dual-layer disc
                        if (info.ContainsKey(Template.LayerbreakField))
                        {
                            output.Add("\tOuter " + Template.MasteringRingField + ": " + info["Outer " + Template.MasteringRingField]);
                            output.Add("\tInner " + Template.MasteringRingField + ": " + info["Inner " + Template.MasteringRingField]);
                            output.Add("\tOuter " + Template.MasteringSIDField + ": " + info["Outer " + Template.MasteringSIDField]);
                            output.Add("\tInner " + Template.MasteringSIDField + ": " + info["Inner " + Template.MasteringSIDField]);
                            output.Add("\tLabel-Side " + Template.MouldSIDField + ": " + info["Label-Side " + Template.MouldSIDField]);
                            output.Add("\tData-Side " + Template.MouldSIDField + ": " + info["Data-Side " + Template.MouldSIDField]);
                            output.Add("\t" + Template.AdditionalMouldField + ": " + info[Template.AdditionalMouldField]);
                            output.Add("\tOuter " + Template.ToolstampField + ": " + info["Outer " + Template.ToolstampField]);
                            output.Add("\tInner " + Template.ToolstampField + ": " + info["Inner " + Template.ToolstampField]);
                        }
                        // If we have a single-layer disc
                        else
                        {
                            output.Add("\t" + Template.MasteringRingField + ": " + info[Template.MasteringRingField]);
                            output.Add("\t" + Template.MasteringSIDField + ": " + info[Template.MasteringSIDField]);
                            output.Add("\tLabel-Side " + Template.MouldSIDField + ": " + info["Label-Side " + Template.MouldSIDField]);
                            output.Add("\tData-Side " + Template.MouldSIDField + ": " + info["Data-Side " + Template.MouldSIDField]);
                            output.Add("\t" + Template.AdditionalMouldField + ": " + info[Template.AdditionalMouldField]);
                            output.Add("\t" + Template.ToolstampField + ": " + info[Template.ToolstampField]);
                        }
                        break;
                }
                output.Add(Template.BarcodeField + ": " + info[Template.BarcodeField]);
                switch (System)
                {
                    case KnownSystem.AppleMacintosh:
                    case KnownSystem.EnhancedCD:
                    case KnownSystem.EnhancedDVD:
                    case KnownSystem.EnhancedBD:
                    case KnownSystem.IBMPCCompatible:
                    case KnownSystem.RainbowDisc:
                        output.Add(Template.ISBNField + ": " + info[Template.ISBNField]);
                        break;
                }
                switch (Type)
                {
                    case MediaType.CDROM:
                    case MediaType.GDROM:
                        output.Add(Template.ErrorCountField + ": " + info[Template.ErrorCountField]);
                        break;
                }
                output.Add(Template.CommentsField + ": " + info[Template.CommentsField]);
                output.Add(Template.ContentsField + ": " + info[Template.ContentsField]);
                output.Add(Template.VersionField + ": " + info[Template.VersionField]);
                output.Add(Template.EditionField + ": " + info[Template.EditionField]);
                switch (System)
                {
                    case KnownSystem.SegaCDMegaCD:
                    case KnownSystem.SegaSaturn:
                        output.Add(Template.SegaHeaderField + ":"); output.Add("");
                        output.AddRange(info[Template.SegaHeaderField].Split('\n')); output.Add("");
                        break;
                    case KnownSystem.SonyPlayStation:
                        output.Add(Template.PlayStationEDCField + ": " + info[Template.PlayStationEDCField]);
                        output.Add(Template.PlayStationAntiModchipField + ": " + info[Template.PlayStationAntiModchipField]);
                        output.Add(Template.PlayStationLibCryptField + ": " + info[Template.PlayStationLibCryptField]);
                        break;
                }
                switch (Type)
                {
                    case MediaType.DVD:
                    case MediaType.BluRay:
                    case MediaType.UMD:
                        // If we have a dual-layer disc
                        if (info.ContainsKey(Template.LayerbreakField))
                        {
                            output.Add(Template.LayerbreakField + ": " + info[Template.LayerbreakField]);
                        }
                        break;
                }
                output.Add(Template.PVDField + ":"); output.Add("");
                output.AddRange(info[Template.PVDField].Split('\n'));
                switch (System)
                {
                    case KnownSystem.AppleMacintosh:
                    case KnownSystem.EnhancedCD:
                    case KnownSystem.EnhancedDVD:
                    case KnownSystem.EnhancedBD:
                    case KnownSystem.IBMPCCompatible:
                    case KnownSystem.RainbowDisc:
                        output.Add(Template.CopyProtectionField + ": " + info[Template.CopyProtectionField]); output.Add("");
                        break;
                    case KnownSystem.DVDVideo:
                        output.Add(Template.CopyProtectionField + ":"); output.Add("");
                        output.AddRange(info[Template.CopyProtectionField].Split('\n'));
                        break;
                    case KnownSystem.MicrosoftXBOX:
                    case KnownSystem.MicrosoftXBOX360:
                        if (Type == MediaType.DVD)
                        {
                            output.Add(Template.XBOXDMIHash + ": " + info[Template.XBOXDMIHash]);
                            output.Add(Template.XBOXPFIHash + ": " + info[Template.XBOXPFIHash]);
                            output.Add(Template.XBOXSSHash + ": " + info[Template.XBOXSSHash]); output.Add("");
                            output.Add(Template.XBOXSSVersion + ": " + info[Template.XBOXSSVersion]);
                            output.Add(Template.XBOXSSRanges + ":"); output.Add("");
                            output.AddRange(info[Template.XBOXSSRanges].Split('\n'));
                        }
                        break;
                    case KnownSystem.SonyPlayStation4:
                        output.Add(Template.PlayStation4PICField + ":"); output.Add("");
                        output.AddRange(info[Template.PlayStation4PICField].Split('\n'));
                        break;
                }
                if (info.ContainsKey(Template.SubIntentionField))
                {
                    output.Add(Template.SubIntentionField + ":"); output.Add("");
                    output.AddRange(info[Template.SubIntentionField].Split('\n')); output.Add("");
                }
                if (info.ContainsKey(Template.DATField))
                {
                    output.Add(Template.DATField + ":"); output.Add("");
                    output.AddRange(info[Template.DATField].Split('\n')); output.Add("");
                }
                switch (Type)
                {
                    case MediaType.CDROM:
                    case MediaType.GDROM:
                        output.Add(Template.CuesheetField + ":"); output.Add("");
                        output.AddRange(info[Template.CuesheetField].Split('\n')); output.Add("");
                        output.Add(Template.WriteOffsetField + ": " + info[Template.WriteOffsetField]);
                        break;
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
        /// Get the existance of an anti-modchip string from the input file, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <returns>Antimodchip existance if possible, false on error</returns>
        private bool GetAntiModchipDetected(string disc)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
            {
                return false;
            }

            using (StreamReader sr = File.OpenText(disc))
            {
                try
                {
                    // Check for either antimod string
                    string line = sr.ReadLine().Trim();
                    while (!sr.EndOfStream)
                    {
                        if (line.StartsWith("Detected anti-mod string"))
                        {
                            return true;
                        }
                        else if (line.StartsWith("No anti-mod string"))
                        {
                            return false;
                        }

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
            {
                return null;
            }

            using (StreamReader sr = File.OpenText(dat))
            {
                try
                {
                    // Make sure this file is a .dat
                    if (sr.ReadLine() != "<?xml version=\"1.0\" encoding=\"UTF-8\"?>")
                    {
                        return null;
                    }
                    if (sr.ReadLine() != "<!DOCTYPE datafile PUBLIC \"-//Logiqx//DTD ROM Management Datafile//EN\" \"http://www.logiqx.com/Dats/datafile.dtd\">")
                    {
                        return null;
                    }

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
            {
                return null;
            }

            // Setup all of the individual pieces
            string region = null, rceProtection = null, copyrightProtectionSystemType = null, encryptedDiscKey = null, playerKey = null, decryptedDiscKey = null;

            // Get everything from _disc.txt first
            using (StreamReader sr = File.OpenText(disc))
            {
                try
                {
                    // Fast forward to the copyright information
                    while (!sr.ReadLine().Trim().StartsWith("========== CopyrightInformation =========="));

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
        /// <param name="edcecc">.img_EdcEcc.txt file location</param>
        /// <returns>Error count if possible, -1 on error</returns>
        private long GetErrorCount(string edcecc)
        {
            // TODO: Better usage of _mainInfo and _c2Error for uncorrectable errors

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(edcecc))
            {
                return -1;
            }

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
                            return 0;
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
            {
                return null;
            }

            return string.Join("\n", File.ReadAllLines(filename));
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
            {
                return null;
            }

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
                            return null;
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
            {
                return -1;
            }

            // First line of defense is the EdcEcc error file
            using (StreamReader sr = File.OpenText(edcecc))
            {
                try
                {
                    // Fast forward to the PVD
                    string line = sr.ReadLine();
                    while (!line.StartsWith("[INFO] Number of sector(s) where EDC doesn't exist: "))
                    {
                        line = sr.ReadLine();
                    }

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
            {
                return null;
            }

            using (StreamReader sr = File.OpenText(mainInfo))
            {
                try
                {
                    // Make sure we're in the right sector
                    while (!sr.ReadLine().StartsWith("========== LBA[000016, 0x00010]: Main Channel =========="));

                    // Fast forward to the PVD
                    while (!sr.ReadLine().StartsWith("0310"));

                    // Now that we're at the PVD, read each line in and concatenate
                    string pvd = "";
                    for (int i = 0; i < 6; i++)
                    {
                        pvd += sr.ReadLine() + "\n"; // 320-370
                    }

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
        private string GetPlayStationEXEDate(char driveLetter)
        {
            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            if (!Directory.Exists(drivePath))
            {
                return null;
            }

            // If we can't find SYSTEM.CNF, we don't have a PlayStation disc
            string systemCnfPath = Path.Combine(drivePath, "SYSTEM.CNF");
            if (!File.Exists(systemCnfPath))
            {
                return null;
            }

            // Let's try reading SYSTEM.CNF to find the "BOOT" value
            string exeName = null;
            try
            {
                using (StreamReader sr = File.OpenText(systemCnfPath))
                {
                    // Not assuming proper ordering, just in case
                    string line = sr.ReadLine();
                    while (!line.StartsWith("BOOT"))
                    {
                        line = sr.ReadLine();
                    }

                    // Once it finds the "BOOT" line, extract the name
                    exeName = Regex.Match(line, @"BOOT.? = cdrom.?:\\(.*?);.*").Groups[1].Value;
                }
            }
            catch
            {
                // We don't care what the error was
                return null;
            }

            // Now that we have the EXE name, try to get the fileinfo for it
            string exePath = Path.Combine(drivePath, exeName);
            if (!File.Exists(exePath))
            {
                return null;
            }

            FileInfo fi = new FileInfo(exePath);
            return fi.LastWriteTimeUtc.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// Get the version from a PlayStation 2 disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>Game version if possible, null on error</returns>
        private string GetPlayStation2Version(char driveLetter)
        {
            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            if (!Directory.Exists(drivePath))
            {
                return null;
            }

            // If we can't find SYSTEM.CNF, we don't have a PlayStation 2 disc
            string systemCnfPath = Path.Combine(drivePath, "SYSTEM.CNF");
            if (!File.Exists(systemCnfPath))
            {
                return null;
            }

            // Let's try reading SYSTEM.CNF to find the "VER" value
            try
            {
                using (StreamReader sr = File.OpenText(systemCnfPath))
                {
                    // Not assuming proper ordering, just in case
                    string line = sr.ReadLine();
                    while (!line.StartsWith("VER"))
                    {
                        line = sr.ReadLine();
                    }

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
        private string GetPlayStation4PIC(string picPath)
        {
            // If the file doesn't exist, we can't get the info
            if (!File.Exists(picPath))
            {
                return null;
            }

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
        private string GetPlayStation4Version(char driveLetter)
        {
            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            if (!Directory.Exists(drivePath))
            {
                return null;
            }

            // If we can't find param.sfo, we don't have a PlayStation 4 disc
            string paramSfoPath = Path.Combine(drivePath, "bd", "param.sfo");
            if (!File.Exists(paramSfoPath))
            {
                return null;
            }

            // Let's try reading param.sfo to find the version at the end of the file
            try
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(paramSfoPath)))
                {
                    br.BaseStream.Seek(0x9A4, SeekOrigin.Begin);
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
            {
                return null;
            }

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
                    {
                        header += sr.ReadLine() + "\n"; // 0000-01F0
                    }

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
            {
                return false;
            }

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
            {
                return false;
            }

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
        private bool GetUMDAuxInfo(string disc, out string title, out string umdversion, out string umdlayer)
        {
            title = null; umdversion = null; umdlayer = null;

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
            {
                return false;
            }

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
                        else if (line.StartsWith("version") && umdversion == null)
                            umdversion = line.Substring("version: ".Length);
                        else if (line.StartsWith("L0 length"))
                            umdlayer = line.Split(' ')[2];
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
        /// Get the XBOX/360 auxiliary info from the outputted files, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private bool GetXBOXAuxInfo(string disc, out string dmihash, out string pfihash, out string sshash, out string ss, out string ssver)
        {
            dmihash = null; pfihash = null; sshash = null; ss = null; ssver = null;

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
            {
                return false;
            }

            using (StreamReader sr = File.OpenText(disc))
            {
                try
                {
                    // Fast forward to the Security Sector version and read it
                    while (!sr.ReadLine().Trim().StartsWith("CPR_MAI Key"));
                    ssver = sr.ReadLine().Trim().Split(' ')[4]; // "Version of challenge table: <VER>"

                    // Fast forward to the Security Sector Ranges
                    while (!sr.ReadLine().Trim().StartsWith("Number of security sector ranges:"));

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
                    {
                        line = sr.ReadLine().Trim();
                    }

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
        /// Verify that the current environment has a complete dump and create submission info is possible
        /// </summary>
        /// <returns>Result instance with the outcome</returns>
        private Result VerifyAndSaveDumpOutput(IProgress<Result> progress)
        {
            // Check to make sure that the output had all the correct files
            if (!FoundAllFiles())
                return Result.Failure("Error! Please check output directory as dump may be incomplete!");

            progress?.Report(Result.Success("Extracting output information from output files..."));
            Dictionary<string, string> templateValues = ExtractOutputInformation(progress);
            progress?.Report(Result.Success("Extracting information complete!"));

            progress?.Report(Result.Success("Formatting extracted information..."));
            List<string> formattedValues = FormatOutputData(templateValues);
            progress?.Report(Result.Success("Formatting complete!"));

            progress?.Report(Result.Success("Writing information to !submissionInfo.txt..."));
            bool success = WriteOutputData(formattedValues);
            progress?.Report(Result.Success("Writing complete!"));

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

        #endregion
    }
}
