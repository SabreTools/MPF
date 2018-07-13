using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using DICUI.Data;
using DICUI.UI;

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
            ViewModels.LoggerViewModel.VerboseLogLn("Ejecting Disc..");
            
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
        /// Get disc speed using DIC
        /// </summary>
        /// <returns>Drive speed if possible, -1 on error</returns>
        public async Task<int> GetDiscSpeed()
        {
            // Validate that the required program exists
            if (!File.Exists(DICPath))
                return -1;

            // Validate that the drive is set up
            if (Drive == null)
                return -1;

            // Validate we're not trying to get the speed for a floppy disk
            if (IsFloppy)
                return -1;

            // Make sure that the current drive is active
            if (!Drive.MarkedActive)
                return -1;

            // Get the drive speed directly
            //int speed = Validators.GetDriveSpeed(Drive);
            //int speed = Validators.GetDriveSpeedEx(Drive, _currentMediaType);

            // Get the drive speed from DIC, if possible
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
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true,
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

            // If a drive or argument was invalid, just exit
            if (output.Contains("Invalid argument"))
            {
                return -1;
            }
            // If we get that the firmware is out of date, tell the user
            else if (output.Contains("[ERROR] This drive isn't latest firmware. Please update."))
            {
                MessageBox.Show($"DiscImageCreator has reported that drive {Drive.Letter} is not updated to the most recent firmware. Please update the firmware for your drive and try again.", "Outdated Firmware", MessageBoxButton.OK, MessageBoxImage.Error);
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

            // is something is wrong in environment return
            if (!result)
                return result;

            // execute DIC
            await Task.Run(() => ExecuteDiskImageCreator());
            progress?.Report(Result.Success("DiscImageCreator has finished!"));

            // execute additional tools
            result = ExecuteAdditionalToolsAfterDIC();
            progress?.Report(result);

            // is something is wrong with additional tools report and return
            // TODO: don't return, just keep generating output from DIC
            /*if (!result.Item1)
            {
                StatusLabel.Content = result.Item2;
                StartStopButton.Content = UIElements.StartDumping;
                return;
            }*/

            // Verify dump output and save it
            progress?.Report(Result.Success("Gathering submission information..."));
            result = VerifyAndSaveDumpOutput();

            return result;
        }

        #endregion

        #region Internal for Testing Purposes

        /// <summary>
        /// Adjust the current environment if we are given custom parameters
        /// </summary>
        internal void AdjustForCustomConfiguration()
        {
            // If we have a custom configuration, we need to extract the best possible information from it
            if (System == KnownSystem.Custom)
            {
                DICParameters.DetermineFlags(out Type, out System, out string letter, out string path);
                if (Type == MediaType.Floppy)
                    Drive = Drive.Floppy(String.IsNullOrWhiteSpace(letter) ? new char() : letter[0]);
                else
                    Drive = Drive.Optical(String.IsNullOrWhiteSpace(letter) ? new char() : letter[0], "", true);
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
        internal void FixOutputPaths()
        {
            // Only fix OutputDirectory if it's not blank or null
            if (!String.IsNullOrWhiteSpace(OutputDirectory))
                OutputDirectory = OutputDirectory.Replace('.', '_').Replace('&', '_');

            // Only fix OutputFilename if it's not blank or null
            if (!String.IsNullOrWhiteSpace(OutputFilename))
                OutputFilename = new StringBuilder(OutputFilename.Replace('&', '_')).Replace('.', '_', 0, OutputFilename.LastIndexOf('.')).ToString();
        }

        /// <summary>
        /// Checks if the parameters are valid
        /// </summary>
        /// <returns>True if the configuration is valid, false otherwise</returns>
        internal bool ParametersValid()
        {
            return DICParameters.IsValid() && !(IsFloppy ^ Type == MediaType.Floppy);
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
                    break;
            }

            return Result.Success();
        }

        /// <summary>
        /// Run DiscImageCreator
        /// </summary>
        private void ExecuteDiskImageCreator()
        {
            ViewModels.LoggerViewModel.VerboseLogLn("Launching DiskImageCreator process.");

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
        private Dictionary<string, string> ExtractOutputInformation()
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
                case MediaType.CD:
                case MediaType.GDROM: // TODO: Verify GD-ROM outputs this
                    mappings[Template.MasteringRingField] = Template.RequiredIfExistsValue;
                    mappings[Template.MasteringSIDField] = Template.RequiredIfExistsValue;
                    mappings[Template.MouldSIDField] = Template.RequiredIfExistsValue;
                    mappings[Template.AdditionalMouldField] = Template.RequiredIfExistsValue;
                    mappings[Template.ToolstampField] = Template.RequiredIfExistsValue;
                    mappings[Template.PVDField] = GetPVD(combinedBase + "_mainInfo.txt") ?? "";
                    mappings[Template.ErrorCountField] = GetErrorCount(combinedBase + ".img_EdcEcc.txt",
                        combinedBase + "_c2Error.txt",
                        combinedBase + "_mainError.txt").ToString();
                    mappings[Template.CuesheetField] = GetFullFile(combinedBase + ".cue") ?? "";
                    mappings[Template.WriteOffsetField] = GetWriteOffset(combinedBase + "_disc.txt") ?? "";

                    // System-specific options
                    switch (System)
                    {
                        case KnownSystem.AppleMacintosh:
                        case KnownSystem.IBMPCCompatible:
                            mappings[Template.ISBNField] = Template.OptionalValue;
                            mappings[Template.CopyProtectionField] = GetCopyProtection() ?? Template.RequiredIfExistsValue;
                            if (File.Exists(combinedBase + "_subIntention.txt"))
                            {
                                FileInfo fi = new FileInfo(combinedBase + "_subIntention.txt");
                                if (fi.Length > 0)
                                {
                                    mappings[Template.SubIntentionField] = GetFullFile(combinedBase + "_subIntention.txt") ?? "";
                                }
                            }
                            break;
                        case KnownSystem.SegaSaturn:
                            mappings[Template.SaturnHeaderField] = GetSaturnHeader(GetFirstTrack()) ?? "";
                            if (GetSaturnBuildInfo(mappings[Template.SaturnHeaderField], out string serial, out string version, out string buildDate))
                            {
                                mappings[Template.DiscSerialField] = serial ?? "";
                                mappings[Template.VersionField] = version ?? "";
                                mappings[Template.SaturnBuildDateField] = buildDate ?? "";
                            }
                            break;
                        case KnownSystem.SonyPlayStation:
                            mappings[Template.PlaystationEXEDateField] = GetPlayStationEXEDate(Drive.Letter) ?? "";
                            mappings[Template.PlayStationEDCField] = GetMissingEDCCount(combinedBase + ".img_eccEdc.txt") > 0 ? "No" : "Yes"; // TODO: This needs to be verified
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
                    string layerbreak = GetLayerbreak(combinedBase + "_disc.txt") ?? "";

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
                        mappings[Template.MouldSIDField] = Template.RequiredIfExistsValue;
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
                        mappings[Template.MouldSIDField] = Template.RequiredIfExistsValue;
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
                        case KnownSystem.IBMPCCompatible:
                            mappings[Template.ISBNField] = Template.OptionalValue;
                            mappings[Template.CopyProtectionField] = GetCopyProtection() ?? Template.RequiredIfExistsValue;
                            if (File.Exists(combinedBase + "_subIntention.txt"))
                            {
                                FileInfo fi = new FileInfo(combinedBase + "_subIntention.txt");
                                if (fi.Length > 0)
                                {
                                    mappings[Template.SubIntentionField] = GetFullFile(combinedBase + "_subIntention.txt") ?? "";
                                }
                            }
                            break;
                        case KnownSystem.MicrosoftXBOX:
                        case KnownSystem.MicrosoftXBOX360XDG2:
                        case KnownSystem.MicrosoftXBOX360XDG3:
                            if (GetXBOXAuxInfo(combinedBase + "_disc.txt", out string dmihash, out string pfihash, out string sshash, out string ss))
                            {
                                mappings[Template.XBOXDMIHash] = dmihash ?? "";
                                mappings[Template.XBOXPFIHash] = pfihash ?? "";
                                mappings[Template.XBOXSSHash] = sshash ?? "";
                                mappings[Template.XBOXSSRanges] = ss ?? "";
                            }
                            break;
                        case KnownSystem.SonyPlayStation2:
                            mappings[Template.PlaystationEXEDateField] = GetPlayStationEXEDate(Drive.Letter) ?? "";
                            mappings[Template.VersionField] = GetPlayStation2Version(Drive.Letter) ?? "";
                            break;
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
                    case KnownSystem.SegaSaturn:
                        output.Add(Template.SaturnBuildDateField + ": " + info[Template.SaturnBuildDateField]);
                        break;
                    case KnownSystem.SonyPlayStation:
                    case KnownSystem.SonyPlayStation2:
                        output.Add(Template.PlaystationEXEDateField + ": " + info[Template.PlaystationEXEDateField]);
                        break;
                }
                output.Add("Ringcode Information:");
                switch (Type)
                {
                    case MediaType.CD:
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
                            output.Add("\t" + Template.MouldSIDField + ": " + info[Template.MouldSIDField]);
                            output.Add("\t" + Template.AdditionalMouldField + ": " + info[Template.AdditionalMouldField]);
                            output.Add("\tOuter " + Template.ToolstampField + ": " + info["Outer " + Template.ToolstampField]);
                            output.Add("\tInner " + Template.ToolstampField + ": " + info["Inner " + Template.ToolstampField]);
                        }
                        // If we have a single-layer disc
                        else
                        {
                            output.Add("\t" + Template.MasteringRingField + ": " + info[Template.MasteringRingField]);
                            output.Add("\t" + Template.MasteringSIDField + ": " + info[Template.MasteringSIDField]);
                            output.Add("\t" + Template.MouldSIDField + ": " + info[Template.MouldSIDField]);
                            output.Add("\t" + Template.AdditionalMouldField + ": " + info[Template.AdditionalMouldField]);
                            output.Add("\t" + Template.ToolstampField + ": " + info[Template.ToolstampField]);
                        }
                        break;
                }
                output.Add(Template.BarcodeField + ": " + info[Template.BarcodeField]);
                switch (System)
                {
                    case KnownSystem.AppleMacintosh:
                    case KnownSystem.IBMPCCompatible:
                        output.Add(Template.ISBNField + ": " + info[Template.ISBNField]);
                        break;
                }
                switch (Type)
                {
                    case MediaType.CD:
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
                    case KnownSystem.SegaSaturn:
                        output.Add(Template.SaturnHeaderField + ":"); output.Add("");
                        output.AddRange(info[Template.SaturnHeaderField].Split('\n')); output.Add("");
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
                    case KnownSystem.IBMPCCompatible:
                        output.Add(Template.CopyProtectionField + ": " + info[Template.CopyProtectionField]); output.Add("");
                        break;
                    case KnownSystem.MicrosoftXBOX:
                    case KnownSystem.MicrosoftXBOX360XDG2:
                    case KnownSystem.MicrosoftXBOX360XDG3:
                        output.Add(Template.XBOXDMIHash + ": " + info[Template.XBOXDMIHash]);
                        output.Add(Template.XBOXPFIHash + ": " + info[Template.XBOXPFIHash]);
                        output.Add(Template.XBOXSSHash + ": " + info[Template.XBOXSSHash]); output.Add("");
                        output.Add(Template.XBOXSSRanges + ":"); output.Add("");
                        output.AddRange(info[Template.XBOXSSRanges].Split('\n'));
                        break;
                }
                if (info.ContainsKey(Template.SubIntentionField))
                {
                    output.Add(Template.SubIntentionField + ":"); output.Add("");
                    output.AddRange(info[Template.SubIntentionField].Split('\n')); output.Add("");
                }
                switch (Type)
                {
                    case MediaType.CD:
                    case MediaType.GDROM:
                        output.Add(Template.CuesheetField + ":"); output.Add("");
                        output.AddRange(info[Template.CuesheetField].Split('\n')); output.Add("");
                        output.Add(Template.WriteOffsetField + ": " + info[Template.WriteOffsetField]); output.Add("");
                        break;
                }
                output.Add(Template.DATField + ":"); output.Add("");
                output.AddRange(info[Template.DATField].Split('\n'));

                return output;
            }
            catch
            {
                // We don't care what the error is
                return null;
            }
        }

        /// <summary>
        /// Ensures that all required output files have been created
        /// </summary>
        /// <returns></returns>
        private bool FoundAllFiles()
        {
            // First, sanitized the output filename to strip off any potential extension
            string outputFilename = Path.GetFileNameWithoutExtension(OutputFilename);

            // Now ensure that all required files exist
            string combinedBase = Path.Combine(OutputDirectory, outputFilename);
            switch (Type)
            {
                case MediaType.CD:
                case MediaType.GDROM: // TODO: Verify GD-ROM outputs this
                    return File.Exists(combinedBase + ".c2")
                        && File.Exists(combinedBase + ".ccd")
                        && File.Exists(combinedBase + ".cue")
                        && File.Exists(combinedBase + ".dat")
                        && File.Exists(combinedBase + ".img")
                        && File.Exists(combinedBase + ".img_EdcEcc.txt")
                        && File.Exists(combinedBase + ".scm")
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
                        && File.Exists(combinedBase + "_subReadable.txt")
                        && File.Exists(combinedBase + "_volDesc.txt");
                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                case MediaType.GameCubeGameDisc:
                case MediaType.WiiOpticalDisc:
                    return File.Exists(combinedBase + ".dat")
                        && File.Exists(combinedBase + "_cmd.txt")
                        && File.Exists(combinedBase + "_disc.txt")
                        && File.Exists(combinedBase + "_drive.txt")
                        && File.Exists(combinedBase + "_mainError.txt")
                        && File.Exists(combinedBase + "_mainInfo.txt")
                        && File.Exists(combinedBase + "_volDesc.txt");
                case MediaType.Floppy:
                    return File.Exists(combinedBase + ".dat")
                        && File.Exists(combinedBase + "_cmd.txt")
                       && File.Exists(combinedBase + "_disc.txt");
                default:
                    // Non-dumping commands will usually produce no output, so this is irrelevant
                    return true;
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
            MessageBoxResult result = MessageBox.Show("Would you like to scan for copy protection? Warning: This may take a long time depending on the size of the disc!", "Copy Protection Scan", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No || result == MessageBoxResult.Cancel || result == MessageBoxResult.None)
            {
                return "(CHECK WITH PROTECTIONID)";
            }

            return Task.Run(() => Validators.RunProtectionScanOnPath(Drive.Letter + ":\\")).Result;
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
        /// Get the detected error count from the input files, if possible
        /// </summary>
        /// <param name="edcecc">.img_EdcEcc.txt file location</param>
        /// <param name="c2Error">_c2Error.txt file location</param>
        /// <param name="mainError">_mainError.txt file location</param>
        /// <returns>Error count if possible, -1 on error</returns>
        private long GetErrorCount(string edcecc, string c2Error, string mainError)
        {
            // If one of the files doesn't exist, we can't get info from them
            if (!File.Exists(edcecc) || !File.Exists(c2Error) || !File.Exists(mainError))
            {
                return -1;
            }

            // First off, if the mainError file has any contents, we have an uncorrectable error
            if (new FileInfo(mainError).Length > 0)
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
                    while (!line.StartsWith("[NO ERROR]")
                        && !line.StartsWith("Total errors:"))
                    {
                        line = sr.ReadLine();
                    }

                    // Now that we're at the error line, determine what the value should be
                    if (line.StartsWith("[NO ERROR]"))
                    {
                        return 0;
                    }
                    else if (line.StartsWith("Total errors:"))
                    {
                        return Int64.Parse(line.Remove(0, "Total errors:".Length).Trim());
                    }

                    return -1;
                }
                catch
                {
                    // We don't care what the exception is right now
                    return -1;
                }
            }
        }

        /// <summary>
        /// Attempts to find the first track of a dumped disc based on the inputs
        /// </summary>
        /// <returns>Proper path to first track, null on error</returns>
        /// <remarks>
        /// By default, this assumes that the outputFilename doesn't contain a proper path, and just a name.
        /// This can lead to a situation where the outputFilename contains a path, but only the filename gets
        /// used in the processing and can lead to a "false null" return
        /// </remarks>
        private string GetFirstTrack()
        {
            // First, sanitized the output filename to strip off any potential extension
            string outputFilename = Path.GetFileNameWithoutExtension(OutputFilename);

            // Go through all standard output naming schemes
            string combinedBase = Path.Combine(OutputDirectory, outputFilename);
            if (File.Exists(combinedBase + ".bin"))
            {
                return combinedBase + ".bin";
            }
            if (File.Exists(combinedBase + " (Track 1).bin"))
            {
                return combinedBase + " (Track 1).bin";
            }
            if (File.Exists(combinedBase + " (Track 01).bin"))
            {
                return combinedBase + " (Track 01).bin";
            }
            if (File.Exists(combinedBase + ".iso"))
            {
                return Path.Combine(combinedBase + ".iso");
            }

            return null;
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
        /// <returns>Layerbreak if possible, null on error</returns>
        private string GetLayerbreak(string disc)
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
                    while (!sr.ReadLine().Trim().StartsWith("EndDataSector")) ;

                    // Now that we're at the layerbreak line, attempt to get the decimal version
                    return sr.ReadLine().Split(' ')[1];
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
                    while (!line.StartsWith("[INFO]"))
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
                    // Fast forward to the PVD
                    while (!sr.ReadLine().StartsWith("0310")) ;

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

            // If we can't find SYSTEM.CNF, we don't have a PlayStation disc
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
                    return Regex.Match(line, @"VER = (.*)").Groups[1].Value;
                }
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
        }

        /// <summary>
        /// Get the header from a Saturn disc, if possible
        /// </summary>
        /// <param name="firstTrackPath">Path to the first track to check</param>
        /// <returns>Header as a byte array if possible, null on error</returns>
        private string GetSaturnHeader(string firstTrackPath)
        {
            // If the file doesn't exist, we can't get the header
            if (!File.Exists(firstTrackPath))
            {
                return null;
            }

            // Try to open the file and read the correct number of bytes
            try
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(firstTrackPath)))
                {
                    br.ReadBytes(0x10);
                    byte[] headerBytes = br.ReadBytes(0x100);

                    // Now format the bytes in a way we like
                    string headerString = "";
                    int ptr = 0;
                    while (ptr < headerBytes.Length)
                    {
                        byte[] sub = new byte[16];
                        Array.Copy(headerBytes, ptr, sub, 0, 16);
                        headerString += ptr.ToString("X").PadLeft(4, '0') + " : "
                            + BitConverter.ToString(sub).Replace("-", " ") + "   "
                            + Encoding.ASCII.GetString(sub) + "\n";
                        ptr += 16;
                    }

                    return headerString.TrimEnd('\n');
                }
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
        }

        /// <summary>
        /// Get the build info from a Saturn disc, if possible
        /// </summary>
        /// <<param name="saturnHeader">String representing a formatter variant of the Saturn header</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private bool GetSaturnBuildInfo(string saturnHeader, out string serial, out string version, out string date)
        {
            serial = null; version = null; date = null;

            // If the input header is null, we can't do a thing
            if (String.IsNullOrWhiteSpace(saturnHeader))
            {
                return false;
            }

            // Now read it in cutting it into lines for easier parsing
            try
            {
                string[] header = saturnHeader.Split('\n');
                string serialVersionLine = header[2].Substring(57);
                string dateLine = header[3].Substring(57);
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
        /// Get the XBOX/360 auxiliary info from the outputted files, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private bool GetXBOXAuxInfo(string disc, out string dmihash, out string pfihash, out string sshash, out string ss)
        {
            dmihash = null; pfihash = null; sshash = null; ss = null;

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
            {
                return false;
            }

            using (StreamReader sr = File.OpenText(disc))
            {
                try
                {
                    // Fast forward to the Security Sector Ranges
                    while (!sr.ReadLine().Trim().StartsWith("Number of security sector ranges:")) ;

                    // Now that we're at the ranges, read each line in and concatenate
                    // TODO: Make this output like the old method (startlba-endlba)
                    string line = sr.ReadLine();
                    while (!line.Trim().StartsWith("========== Unlock 2 state(wxripper) =========="))
                    {
                        ss += line + "\n";
                        line = sr.ReadLine();
                    }

                    // Fast forward to the aux hashes
                    while (!line.Trim().StartsWith("<rom"))
                    {
                        line = sr.ReadLine();
                    }

                    // Read in the hashes to the proper parts
                    sshash = line.Trim();
                    pfihash = sr.ReadLine().Trim();
                    dmihash = sr.ReadLine().Trim();

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

            AdjustForCustomConfiguration();
            FixOutputPaths();

            // Validate that the required program exists
            if (!File.Exists(DICPath))
                return Result.Failure("Error! Could not find DiscImageCreator!");

            // Validate that the user explicitly wants an inactive drive to be considered for dumping
            if (!Drive.MarkedActive)
            {
                MessageBoxResult result = MessageBox.Show("The currently selected drive does not appear to contain a disc! Are you sure you want to continue?", "Missing Disc", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.No || result == MessageBoxResult.Cancel || result == MessageBoxResult.None)
                {
                    return Result.Failure("Dumping aborted!");
                }
            }

            // If a complete dump already exists
            if (FoundAllFiles())
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
        /// Verify that the current environment has a complete dump and create submission info is possible
        /// </summary>
        /// <returns>Result instance with the outcome</returns>
        private Result VerifyAndSaveDumpOutput()
        {
            // Check to make sure that the output had all the correct files
            if (!FoundAllFiles())
                return Result.Failure("Error! Please check output directory as dump may be incomplete!");

            Dictionary<string, string> templateValues = ExtractOutputInformation();
            List<string> formattedValues = FormatOutputData(templateValues);
            bool success = WriteOutputData(formattedValues);

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
            {
                return false;
            }

            // Then, sanitized the output filename to strip off any potential extension
            string outputFilename = Path.GetFileNameWithoutExtension(OutputFilename);

            // Now write out to a generic file
            try
            {
                using (StreamWriter sw = new StreamWriter(File.Open(Path.Combine(OutputDirectory, "!submissionInfo.txt"), FileMode.Create, FileAccess.Write)))
                {
                    foreach (string line in lines)
                    {
                        sw.WriteLine(line);
                    }
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
