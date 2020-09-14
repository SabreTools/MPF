using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;
using BurnOutSharp;
using DICUI.Data;
using DICUI.Utilities;
using DICUI.Web;

namespace DICUI.Windows
{
    public partial class MainWindow : Window
    {
        #region Fields

        /// <summary>
        /// Currently selected or detected media type
        /// </summary>
        public MediaType? CurrentMediaType { get; private set; }

        /// <summary>
        /// Current list of drives
        /// </summary>
        public List<Drive> Drives { get; private set; }

        /// <summary>
        /// Current dumping environment
        /// </summary>
        public DumpEnvironment Env { get; private set; }

        /// <summary>
        /// Current list of supported media types
        /// </summary>
        public List<MediaType?> MediaTypes { get; private set; } = new List<MediaType?>();

        /// <summary>
        /// Current list of supported system profiles
        /// </summary>
        public List<KnownSystemComboBoxItem> Systems { get; private set; }

        /// <summary>
        /// Current UI options
        /// </summary>
        public UIOptions UIOptions { get; private set; } = new UIOptions();

        #endregion

        #region Private Instance Variables

        /// <summary>
        /// Determines if the window is already shown or not
        /// </summary>
        private bool alreadyShown;

        /// <summary>
        /// Current attached LogWindow
        /// </summary>
        private readonly LogWindow logWindow;

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            // Load the options
            ViewModels.OptionsViewModel = new OptionsViewModel(UIOptions);

            // Load the log window
            logWindow = new LogWindow(this);
            ViewModels.LoggerViewModel.SetWindow(logWindow);

            // Disable buttons until we load fully
            StartStopButton.IsEnabled = false;
            MediaScanButton.IsEnabled = false;
            CopyProtectScanButton.IsEnabled = false;

            if (UIOptions.Options.OpenLogWindowAtStartup)
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                double combinedHeight = this.Height + logWindow.Height + Constants.LogWindowMarginFromMainWindow;
                Rectangle bounds = GetScaledCoordinates(WinForms.Screen.PrimaryScreen.WorkingArea);

                this.Left = bounds.Left + (bounds.Width - this.Width) / 2;
                this.Top = bounds.Top + (bounds.Height - combinedHeight) / 2;
            }
        }

        #region Helpers

        /// <summary>
        /// Browse for an output folder
        /// </summary>
        private void BrowseFolder()
        {
            WinForms.FolderBrowserDialog folderDialog = new WinForms.FolderBrowserDialog { ShowNewFolderButton = false, SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory };
            WinForms.DialogResult result = folderDialog.ShowDialog();

            if (result == WinForms.DialogResult.OK)
            {
                OutputDirectoryTextBox.Text = folderDialog.SelectedPath;
            }
        }

        /// <summary>
        /// Cache the current disc type to internal variable
        /// </summary>
        private void CacheCurrentDiscType()
        {
            // Get the drive letter from the selected item
            if (DriveLetterComboBox.SelectedItem is Drive drive)
            {
                // Get the current media type
                if (!UIOptions.Options.SkipMediaTypeDetection)
                {
                    ViewModels.LoggerViewModel.VerboseLog("Trying to detect media type for drive {0}.. ", drive.Letter);
                    CurrentMediaType = Validators.GetMediaType(drive);
                    ViewModels.LoggerViewModel.VerboseLogLn(CurrentMediaType == null ? "unable to detect." : ("detected " + CurrentMediaType.LongName() + "."));
                }
            }
        }

        /// <summary>
        /// Create a DumpEnvironment with all current settings
        /// </summary>
        /// <returns>Filled DumpEnvironment instance</returns>
        private DumpEnvironment DetermineEnvironment()
        {
            // Populate the new environment
            var env = new DumpEnvironment(UIOptions.Options,
                OutputDirectoryTextBox.Text,
                OutputFilenameTextBox.Text,
                DriveLetterComboBox.SelectedItem as Drive,
                SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem,
                MediaTypeComboBox.SelectedItem as MediaType?,
                ParametersTextBox.Text);

            // Disable automatic reprocessing of the textboxes until we're done
            OutputDirectoryTextBox.TextChanged -= OutputDirectoryTextBoxTextChanged;
            OutputFilenameTextBox.TextChanged -= OutputFilenameTextBoxTextChanged;

            OutputDirectoryTextBox.Text = env.OutputDirectory;
            OutputFilenameTextBox.Text = env.OutputFilename;

            OutputDirectoryTextBox.TextChanged += OutputDirectoryTextBoxTextChanged;
            OutputFilenameTextBox.TextChanged += OutputFilenameTextBoxTextChanged;

            return env;
        }

        /// <summary>
        /// Ensure information is consistent with the currently selected disc type
        /// </summary>
        private void EnsureDiscInformation()
        {
            // Get the current environment information
            Env = DetermineEnvironment();

            // Take care of null cases
            if (Env.System == null)
                Env.System = KnownSystem.NONE;
            if (Env.Type == null)
                Env.Type = MediaType.NONE;

            // Get the status to write out
            Result result = Validators.GetSupportStatus(Env.System, Env.Type);
            StatusLabel.Content = result.Message;

            // Set the index for the current disc type
            SetCurrentDiscType();

            StartStopButton.IsEnabled = result && (Drives != null && Drives.Count > 0 ? true : false);

            // If we're in a type that doesn't support drive speeds
            DriveSpeedComboBox.IsEnabled = Env.Type.DoesSupportDriveSpeed();

            // If input params are not enabled, generate the full parameters from the environment
            if (!ParametersTextBox.IsEnabled)
            {
                string generated = Env.GetFullParameters((int?)DriveSpeedComboBox.SelectedItem);
                if (generated != null)
                    ParametersTextBox.Text = generated;
            }
        }

        /// <summary>
        /// Get the default output directory name from the currently selected drive
        /// </summary>
        /// <param name="driveChanged">Force an updated name if the drive letter changes</param>
        private void GetOutputNames(bool driveChanged)
        {
            Drive drive = DriveLetterComboBox.SelectedItem as Drive;
            KnownSystem? systemType = SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem;
            MediaType? mediaType = MediaTypeComboBox.SelectedItem as MediaType?;

            // Set the output directory, if we changed drives or it's not already
            if (driveChanged || string.IsNullOrEmpty(OutputDirectoryTextBox.Text))
                OutputDirectoryTextBox.Text = Path.Combine(UIOptions.Options.DefaultOutputPath, drive?.VolumeLabel ?? string.Empty);

            // Get the extension for the file for the next two statements
            string extension = Env.GetExtension(mediaType);

            // Set the output filename, if we changed drives or it's not already
            if (driveChanged || string.IsNullOrEmpty(OutputFilenameTextBox.Text))
                OutputFilenameTextBox.Text = (drive?.VolumeLabel ?? systemType.LongName()) + (extension ?? ".bin");

            // If the extension for the file changed, update that automatically
            else if (Path.GetExtension(OutputFilenameTextBox.Text) != extension)
                OutputFilenameTextBox.Text = Path.GetFileNameWithoutExtension(OutputFilenameTextBox.Text) + (extension ?? ".bin");
        }

        /// <summary>
        /// Get a complete list of active disc drives and fill the combo box
        /// </summary>
        /// <remarks>TODO: Find a way for this to periodically run, or have it hook to a "drive change" event</remarks>
        private void PopulateDrives()
        {
            ViewModels.LoggerViewModel.VerboseLogLn("Scanning for drives..");

            // Always enable the media scan
            MediaScanButton.IsEnabled = true;

            // Populate the list of drives and add it to the combo box
            Drives = Validators.CreateListOfDrives(UIOptions.Options.IgnoreFixedDrives);
            DriveLetterComboBox.ItemsSource = Drives;

            if (DriveLetterComboBox.Items.Count > 0)
            {
                // Check for active optical drives first
                int index = Drives.FindIndex(d => d.MarkedActive && d.InternalDriveType == InternalDriveType.Optical);

                // Then we check for floppy drives
                if (index == -1)
                    index = Drives.FindIndex(d => d.MarkedActive && d.InternalDriveType == InternalDriveType.Floppy);

                // Then we try all other drive types
                if (index == -1)
                    index = Drives.FindIndex(d => d.MarkedActive);

                // Set the selected index
                DriveLetterComboBox.SelectedIndex = (index != -1 ? index : 0);
                StatusLabel.Content = "Valid drive found! Choose your Media Type";
                CopyProtectScanButton.IsEnabled = true;

                // Get the current media type
                if (!UIOptions.Options.SkipSystemDetection && index != -1)
                {
                    ViewModels.LoggerViewModel.VerboseLog("Trying to detect system for drive {0}.. ", Drives[index].Letter);
                    var currentSystem = Validators.GetKnownSystem(Drives[index]);
                    ViewModels.LoggerViewModel.VerboseLogLn(currentSystem == null || currentSystem == KnownSystem.NONE ? "unable to detect." : ("detected " + currentSystem.LongName() + "."));

                    if (currentSystem != null && currentSystem != KnownSystem.NONE)
                    {
                        int sysIndex = Systems.FindIndex(s => s == currentSystem);
                        SystemTypeComboBox.SelectedIndex = sysIndex;
                    }
                }

                // Only enable the start/stop if we don't have the default selected
                StartStopButton.IsEnabled = (SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem) != KnownSystem.NONE;

                ViewModels.LoggerViewModel.VerboseLogLn("Found {0} drives: {1}", Drives.Count, string.Join(", ", Drives.Select(d => d.Letter)));
            }
            else
            {
                DriveLetterComboBox.SelectedIndex = -1;
                StatusLabel.Content = "No valid drive found!";
                StartStopButton.IsEnabled = false;
                CopyProtectScanButton.IsEnabled = false;

                ViewModels.LoggerViewModel.VerboseLogLn("Found no drives");
            }
        }

        /// <summary>
        /// Populate media type according to system type
        /// </summary>
        private void PopulateMediaType()
        {
            KnownSystem? currentSystem = SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem;

            if (currentSystem != null)
            {
                MediaTypes = Validators.GetValidMediaTypes(currentSystem);
                MediaTypeComboBox.ItemsSource = MediaTypes;

                MediaTypeComboBox.IsEnabled = MediaTypes.Count > 1;
                MediaTypeComboBox.SelectedIndex = (MediaTypes.IndexOf(CurrentMediaType) >= 0 ? MediaTypes.IndexOf(CurrentMediaType) : 0);
            }
            else
            {
                MediaTypeComboBox.IsEnabled = false;
                MediaTypeComboBox.ItemsSource = null;
                MediaTypeComboBox.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Get a complete list of supported systems and fill the combo box
        /// </summary>
        private void PopulateSystems()
        {
            var knownSystems = Validators.CreateListOfSystems();

            ViewModels.LoggerViewModel.VerboseLogLn("Populating systems, {0} systems found.", knownSystems.Count);

            Dictionary<KnownSystemCategory, List<KnownSystem?>> mapping = knownSystems
                .GroupBy(s => s.Category())
                .ToDictionary(
                    k => k.Key,
                    v => v
                        .OrderBy(s => s.LongName())
                        .ToList()
                );

            Systems = new List<KnownSystemComboBoxItem>()
            {
                new KnownSystemComboBoxItem(KnownSystem.NONE),
            };

            foreach (var group in mapping)
            {
                Systems.Add(new KnownSystemComboBoxItem(group.Key));
                group.Value.ForEach(system => Systems.Add(new KnownSystemComboBoxItem(system)));
            }

            SystemTypeComboBox.ItemsSource = Systems;
            SystemTypeComboBox.SelectedIndex = 0;

            StartStopButton.IsEnabled = false;
        }

        /// <summary>
        /// Process the current custom parameters back into UI values
        /// </summary>
        private void ProcessCustomParameters()
        {
            Env.SetParameters(ParametersTextBox.Text);
            if (Env.Parameters == null)
                return;

            int driveIndex = Drives.Select(d => d.Letter).ToList().IndexOf(Env.Parameters.InputPath()[0]);
            if (driveIndex > -1)
                DriveLetterComboBox.SelectedIndex = driveIndex;

            int driveSpeed = Env.Parameters.GetSpeed() ?? -1;
            if (driveSpeed > 0)
                DriveSpeedComboBox.SelectedValue = driveSpeed;
            else
                Env.Parameters.SetSpeed((int?)DriveSpeedComboBox.SelectedValue);

            string trimmedPath = Env.Parameters.OutputPath()?.Trim('"') ?? string.Empty;
            string outputDirectory = Path.GetDirectoryName(trimmedPath);
            string outputFilename = Path.GetFileName(trimmedPath);
            if (!string.IsNullOrWhiteSpace(outputDirectory))
                OutputDirectoryTextBox.Text = outputDirectory;
            else
                outputDirectory = OutputDirectoryTextBox.Text;
            if (!string.IsNullOrWhiteSpace(outputFilename))
                OutputFilenameTextBox.Text = outputFilename;
            else
                outputFilename = OutputFilenameTextBox.Text;

            MediaType? mediaType = Env.Parameters.GetMediaType();
            int mediaTypeIndex = MediaTypes.IndexOf(mediaType);
            if (mediaTypeIndex > -1)
                MediaTypeComboBox.SelectedIndex = mediaTypeIndex;
        }

        /// <summary>
        /// Scan and show copy protection for the current disc
        /// </summary>
        private async void ScanAndShowProtection()
        {
            if (Env == null)
                Env = DetermineEnvironment();

            if (Env.Drive.Letter != default(char))
            {
                ViewModels.LoggerViewModel.VerboseLogLn("Scanning for copy protection in {0}", Env.Drive.Letter);

                var tempContent = StatusLabel.Content;
                StatusLabel.Content = "Scanning for copy protection... this might take a while!";
                StartStopButton.IsEnabled = false;
                MediaScanButton.IsEnabled = false;
                CopyProtectScanButton.IsEnabled = false;

                var progress = new Progress<FileProtection>();
                progress.ProgressChanged += ProgressUpdated;
                string protections = await Validators.RunProtectionScanOnPath(Env.Drive.Letter + ":\\", progress);

                // If SmartE is detected on the current disc, remove `/sf` from the flags for DIC only
                if (Env.InternalProgram == InternalProgram.DiscImageCreator && protections.Contains("SmartE"))
                {
                    ((DiscImageCreator.Parameters)Env.Parameters)[DiscImageCreator.Flag.ScanFileProtect] = false;
                    ViewModels.LoggerViewModel.VerboseLogLn($"SmartE detected, removing {DiscImageCreator.FlagStrings.ScanFileProtect} from parameters");
                }

                if (!ViewModels.LoggerViewModel.WindowVisible)
                    MessageBox.Show(protections, "Detected Protection", MessageBoxButton.OK, MessageBoxImage.Information);
                ViewModels.LoggerViewModel.VerboseLog("Detected the following protections in {0}:\r\n\r\n{1}", Env.Drive.Letter, protections);

                StatusLabel.Content = tempContent;
                StartStopButton.IsEnabled = true;
                MediaScanButton.IsEnabled = true;
                CopyProtectScanButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Set the current disc type in the combo box
        /// </summary>
        private void SetCurrentDiscType()
        {
            // If we have an invalid current type, we don't care and return
            if (CurrentMediaType == null || CurrentMediaType == MediaType.NONE)
                return;

            // Now set the selected item, if possible
            int index = MediaTypes.FindIndex(kvp => kvp.Value == CurrentMediaType);
            if (index != -1)
                MediaTypeComboBox.SelectedIndex = index;
            else
                StatusLabel.Content = $"Disc of type '{Converters.LongName(CurrentMediaType)}' found, but the current system does not support it!";
        }

        /// <summary>
        /// Set the drive speed based on reported maximum and user-defined option
        /// </summary>
        private void SetSupportedDriveSpeed()
        {
            // Set the drive speed list that's appropriate
            var values = Interface.GetSpeedsForMediaType(CurrentMediaType);
            DriveSpeedComboBox.ItemsSource = values;
            ViewModels.LoggerViewModel.VerboseLogLn("Supported media speeds: {0}", string.Join(",", values));

            // Set the selected speed
            int speed = UIOptions.GetPreferredDumpSpeedForMediaType(CurrentMediaType);
            ViewModels.LoggerViewModel.VerboseLogLn("Setting drive speed to: {0}", speed);
            DriveSpeedComboBox.SelectedValue = speed;
        }

        /// <summary>
        /// Show the disc information window
        /// </summary>
        /// <param name="submissionInfo">SubmissionInfo object to display and possibly change</param>
        /// <returns>Dialog open result</returns>
        private bool? ShowDiscInformationWindow(SubmissionInfo submissionInfo)
        {
            var discInformationWindow = new DiscInformationWindow();
            discInformationWindow.SubmissionInfo = submissionInfo;
            discInformationWindow.Owner = this;
            discInformationWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            discInformationWindow.Load();
            return discInformationWindow.ShowDialog();
        }

        /// <summary>
        /// Begin the dumping process using the given inputs
        /// </summary>
        private async void StartDumping()
        {
            // One last check to determine environment, just in case
            Env = DetermineEnvironment();

            // If still in custom parameter mode, check that users meant to continue or not
            if (EnableParametersCheckBox.IsChecked == true)
            {
                MessageBoxResult result = MessageBox.Show("It looks like you have custom parameters that have not been saved. Would you like to apply those changes before starting to dump?", "Custom Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    EnableParametersCheckBox.IsChecked = false;
                    ParametersTextBox.IsEnabled = false;
                    ProcessCustomParameters();
                }
                else if (result == MessageBoxResult.Cancel)
                    return;
                // If "No", then we continue with the current known environment
            }

            // Fix the output paths
            Env.FixOutputPaths();

            try
            {
                // Validate that the user explicitly wants an inactive drive to be considered for dumping
                if (!Env.Drive.MarkedActive)
                {
                    MessageBoxResult mbresult = MessageBox.Show("The currently selected drive does not appear to contain a disc! Are you sure you want to continue?", "Missing Disc", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    if (mbresult == MessageBoxResult.No || mbresult == MessageBoxResult.Cancel || mbresult == MessageBoxResult.None)
                    {
                        ViewModels.LoggerViewModel.VerboseLogLn("Dumping aborted!");
                        return;
                    }
                }

                // If a complete dump already exists
                if (Env.FoundAllFiles())
                {
                    MessageBoxResult mbresult = MessageBox.Show("A complete dump already exists! Are you sure you want to overwrite?", "Overwrite?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    if (mbresult == MessageBoxResult.No || mbresult == MessageBoxResult.Cancel || mbresult == MessageBoxResult.None)
                    {
                        ViewModels.LoggerViewModel.VerboseLogLn("Dumping aborted!");
                        return;
                    }
                }

                StartStopButton.Content = Interface.StopDumping;
                CopyProtectScanButton.IsEnabled = false;
                StatusLabel.Content = "Beginning dumping process";
                ViewModels.LoggerViewModel.VerboseLogLn("Starting dumping process..");

                // Get progress indicators
                var resultProgress = new Progress<Result>();
                resultProgress.ProgressChanged += ProgressUpdated;
                var protectionProgress = new Progress<FileProtection>();
                protectionProgress.ProgressChanged += ProgressUpdated;

                // Run the program with the parameters
                Result result = await Env.Run(resultProgress);

                // If we didn't execute a dumping command we cannot get submission output
                if (!Env.Parameters.IsDumpingCommand())
                {
                    ViewModels.LoggerViewModel.VerboseLogLn("No dumping command was run, submission information will not be gathered.");
                    StatusLabel.Content = "Execution complete!";
                    StartStopButton.Content = Interface.StartDumping;
                    CopyProtectScanButton.IsEnabled = true;
                    return;
                }

                if (result)
                {
                    // Verify dump output and save it
                    result = await Env.VerifyAndSaveDumpOutput(resultProgress,
                        protectionProgress,
                        EjectWhenDoneCheckBox.IsChecked,
                        UIOptions.Options.ResetDriveAfterDump,
                        ShowDiscInformationWindow
                    );
                }
            }
            catch
            {
                // No-op, we don't care what it was
            }
            finally
            {
                StartStopButton.Content = Interface.StartDumping;
                CopyProtectScanButton.IsEnabled = true;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for AboutMenuItem Click event
        /// </summary>
        private void AboutClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"darksabre76 - Project Lead / Backend Design"
                + $"{Environment.NewLine}ReignStumble - Former Project Lead / UI Design"
                + $"{Environment.NewLine}Jakz - Primary Feature Contributor"
                + $"{Environment.NewLine}NHellFire - Feature Contributor", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Handler for AppExitMenuItem Click event
        /// </summary>
        private void AppExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Handler for CheckForUpdatesMenuItem Click event
        /// </summary>
        private void CheckForUpdatesClick(object sender, RoutedEventArgs e)
        {
            (bool different, string message, string url) = Tools.CheckForNewVersion();

            // If we have a new version, put it in the clipboard
            if (different)
                Clipboard.SetText(url);

            MessageBox.Show(message, "Version Update Check", MessageBoxButton.OK, different ? MessageBoxImage.Exclamation : MessageBoxImage.Information);
        }

        /// <summary>
        /// Handler for CopyProtectScanButton Click event
        /// </summary>
        private void CopyProtectScanButtonClick(object sender, RoutedEventArgs e)
        {
            ScanAndShowProtection();
        }

        /// <summary>
        /// Handler for DriveLetterComboBox SelectionChanged event
        /// </summary>
        private void DriveLetterComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CacheCurrentDiscType();
            SetCurrentDiscType();
            GetOutputNames(true);
            SetSupportedDriveSpeed();
        }

        /// <summary>
        /// Handler for DriveSpeedComboBox SelectionChanged event
        /// </summary>
        private void DriveSpeedComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnsureDiscInformation();
        }

        /// <summary>
        /// Handler for EnableParametersCheckBox Click event
        /// </summary>
        private void EnableParametersCheckBoxClick(object sender, RoutedEventArgs e)
        {
            if (EnableParametersCheckBox.IsChecked == true)
                ParametersTextBox.IsEnabled = true;
            else
            {
                ParametersTextBox.IsEnabled = false;
                ProcessCustomParameters();
            }
        }

        /// <summary>
        /// Handler for MediaTypeComboBox SelectionChanged event
        /// </summary>
        private void MediaTypeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Only change the media type if the selection and not the list has changed
            if (e.RemovedItems.Count == 1 && e.AddedItems.Count == 1)
            {
                CurrentMediaType = MediaTypeComboBox.SelectedItem as MediaType?;
                SetSupportedDriveSpeed();
            }

            GetOutputNames(false);
            EnsureDiscInformation();
        }

        /// <summary>
        /// Handler for MediaScanButton Click event
        /// </summary>
        private void MediaScanButtonClick(object sender, RoutedEventArgs e)
        {
            PopulateDrives();
        }

        /// <summary>
        /// Handler for MainWindow Activated event
        /// </summary>
        private void MainWindowActivated(object sender, EventArgs e)
        {
            if (logWindow.IsVisible && !this.Topmost)
            {
                logWindow.Topmost = true;
                logWindow.Topmost = false;
            }
        }

        /// <summary>
        /// Handler for MainWindow Closing event
        /// </summary>
        private void MainWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (logWindow.IsVisible)
                logWindow.Close();
        }

        /// <summary>
        /// Handler for MainWindow PositionChanged event
        /// </summary>
        private void MainWindowLocationChanged(object sender, EventArgs e)
        {
            if (logWindow.IsVisible)
                logWindow.AdjustPositionToMainWindow();
        }

        /// <summary>
        /// Handler for MainWindow OnContentRendered event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (alreadyShown)
                return;

            alreadyShown = true;

            if (UIOptions.Options.OpenLogWindowAtStartup)
            {
                //TODO: this should be bound directly to WindowVisible property in two way fashion
                // we need to study how to properly do it in XAML
                ShowLogMenuItem.IsChecked = true;
                ViewModels.LoggerViewModel.WindowVisible = true;
            }

            // Populate the list of systems
            StatusLabel.Content = "Creating system list, please wait!";
            PopulateSystems();

            // Populate the list of drives
            StatusLabel.Content = "Creating drive list, please wait!";
            PopulateDrives();
        }

        /// <summary>
        /// Handler for OptionsWindow OnUpdated event
        /// </summary>
        public void OnOptionsUpdated()
        {
            PopulateDrives();
            GetOutputNames(false);
            SetSupportedDriveSpeed();
            EnsureDiscInformation();
        }

        /// <summary>
        /// Handler for OptionsMenuItem Click event
        /// </summary>
        private void OptionsMenuItemClick(object sender, RoutedEventArgs e)
        {
            // Show the window and wait for the response
            var optionsWindow = new OptionsWindow();
            optionsWindow.UIOptions = UIOptions;
            optionsWindow.Owner = this;
            optionsWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            optionsWindow.Refresh();
            optionsWindow.Show();
        }

        /// <summary>
        /// Handler for OutputDirectoryBrowseButton Click event
        /// </summary>
        private void OutputDirectoryBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            BrowseFolder();
            EnsureDiscInformation();
        }

        /// <summary>
        /// Handler for OutputFilenameTextBox TextInput event
        /// </summary>
        private void OutputDirectoryTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            EnsureDiscInformation();
        }

        /// <summary>
        /// Handler for OutputFilenameTextBox TextInput event
        /// </summary>
        private void OutputFilenameTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            EnsureDiscInformation();
        }

        /// <summary>
        /// Handler for Result ProgressChanged event
        /// </summary>
        private void ProgressUpdated(object sender, Result value)
        {
            StatusLabel.Content = value.Message;
            ViewModels.LoggerViewModel.VerboseLogLn(value.Message);
        }

        /// <summary>
        /// Handler for FileProtection ProgressChanged event
        /// </summary>
        private void ProgressUpdated(object sender, FileProtection value)
        {
            string message = $"{value.Percentage * 100:N2}%: {value.Filename} - {value.Protection}";
            StatusLabel.Content = message;
            ViewModels.LoggerViewModel.VerboseLogLn(message);
        }

        /// <summary>
        /// Handler for StartStopButton Click event
        /// </summary>
        private void StartStopButtonClick(object sender, RoutedEventArgs e)
        {
            // Dump or stop the dump
            if ((string)StartStopButton.Content == Interface.StartDumping)
            {
                StartDumping();
            }
            else if ((string)StartStopButton.Content == Interface.StopDumping)
            {
                ViewModels.LoggerViewModel.VerboseLogLn("Canceling dumping process...");
                Env.CancelDumping();
                CopyProtectScanButton.IsEnabled = true;

                if (EjectWhenDoneCheckBox.IsChecked == true)
                {
                    ViewModels.LoggerViewModel.VerboseLogLn($"Ejecting disc in drive {Env.Drive.Letter}");
                    Env.EjectDisc();
                }

                if (UIOptions.Options.ResetDriveAfterDump)
                {
                    ViewModels.LoggerViewModel.VerboseLogLn($"Resetting drive {Env.Drive.Letter}");
                    Env.ResetDrive();
                }
            }
        }

        /// <summary>
        /// Handler for SystemTypeComboBox SelectionChanged event
        /// </summary>
        private void SystemTypeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If we're on a separator, go to the next item and return
            if ((SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem).IsHeader())
            {
                SystemTypeComboBox.SelectedIndex++;
                return;
            }

            ViewModels.LoggerViewModel.VerboseLogLn("Changed system to: {0}", (SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem).Name);
            PopulateMediaType();
            GetOutputNames(false);
            EnsureDiscInformation();
        }

        #endregion

        #region UI Helpers

        /// <summary>
        /// Get pixel coordinates based on DPI scaling
        /// </summary>
        /// <param name="bounds">Rectangle representing the bounds to transform</param>
        /// <returns>Rectangle representing the scaled bounds</returns>
        private Rectangle GetScaledCoordinates(Rectangle bounds)
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                return new Rectangle(
                TransformCoordinate(bounds.Left, g.DpiX),
                TransformCoordinate(bounds.Top, g.DpiY),
                TransformCoordinate(bounds.Width, g.DpiX),
                TransformCoordinate(bounds.Height, g.DpiY));
            }
        }

        /// <summary>
        /// Transform an individual coordinate using DPI scaling
        /// </summary>
        /// <param name="coord">Current integer coordinate</param>
        /// <param name="dpi">DPI scaling factor</param>
        /// <returns>Scaled integer coordinate</returns>
        private int TransformCoordinate(int coord, float dpi)
        {
            return (int)(coord / ((double)dpi / 96));
        }

        #endregion
    }
}
