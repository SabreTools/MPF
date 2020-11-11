using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BurnOutSharp;
using MPF.Data;
using MPF.Utilities;
using MPF.Web;

namespace MPF.Avalonia
{
    public class MainWindow : Window
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
        /// Current attached LogWindow
        /// </summary>
        private LogWindow logWindow;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            // Load the options
            ViewModels.OptionsViewModel = new OptionsViewModel(UIOptions);

            // Load the log window
            logWindow = new LogWindow();
            logWindow.Owner = this;
            ViewModels.LoggerViewModel.SetWindow(logWindow);

            // Disable buttons until we load
            this.Find<Button>("StartStopButton").IsEnabled = false;
            this.Find<Button>("MediaScanButton").IsEnabled = false;
            this.Find<Button>("CopyProtectScanButton").IsEnabled = false;

            // TODO: If log window open, "dock" it to the current Window

            if (UIOptions.Options.OpenLogWindowAtStartup)
            {
                //TODO: this should be bound directly to WindowVisible property in two way fashion
                // we need to study how to properly do it in XAML
                this.Find<CheckBox>("ShowLogCheckBox").IsChecked = true;
                ViewModels.LoggerViewModel.WindowVisible = true;
            }

            // Populate the list of systems
            this.Find<TextBlock>("StatusLabel").Text = "Creating system list, please wait!";
            PopulateSystems();

            // Populate the list of drives
            this.Find<TextBlock>("StatusLabel").Text = "Creating drive list, please wait!";
            PopulateDrives();
        }

        #region Helpers

        /// <summary>
        /// Browse for an output folder
        /// </summary>
        private async void BrowseFolder()
        {
            OpenFolderDialog folderDialog = new OpenFolderDialog { Directory = AppDomain.CurrentDomain.BaseDirectory };
            string directory = await folderDialog.ShowAsync(this);
            if (!string.IsNullOrWhiteSpace(directory))
                this.Find<TextBox>("OutputDirectoryTextBox").Text = directory;
        }

        /// <summary>
        /// Cache the current disc type to internal variable
        /// </summary>
        private void CacheCurrentDiscType()
        {
            // Get the drive letter from the selected item
            if (this.Find<ComboBox>("DriveLetterComboBox").SelectedItem is Drive drive)
            {
                // Get the current media type, if possible
                if (UIOptions.Options.SkipMediaTypeDetection)
                {
                    ViewModels.LoggerViewModel.VerboseLog("Media type detection disabled, defaulting to CD-ROM");
                    CurrentMediaType = MediaType.CDROM;
                }
                else
                {
                    ViewModels.LoggerViewModel.VerboseLog("Trying to detect media type for drive {0}.. ", drive.Letter);
                    CurrentMediaType = Validators.GetMediaType(drive);
                    ViewModels.LoggerViewModel.VerboseLogLn(CurrentMediaType == null ? "unable to detect, defaulting to CD-ROM." : ($"detected {CurrentMediaType.LongName()}."));
                    if (CurrentMediaType == null)
                        CurrentMediaType = MediaType.CDROM;
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
                this.Find<TextBox>("OutputDirectoryTextBox").Text,
                this.Find<TextBox>("OutputFilenameTextBox").Text,
                this.Find<ComboBox>("DriveLetterComboBox").SelectedItem as Drive,
                this.Find<ComboBox>("SystemTypeComboBox").SelectedItem as KnownSystemComboBoxItem,
                this.Find<ComboBox>("MediaTypeComboBox").SelectedItem as MediaType?,
                this.Find<TextBox>("ParametersTextBox").Text);

            // Disable automatic reprocessing of the textboxes until we're done
            this.Find<TextBox>("OutputDirectoryTextBox").TextInput -= OutputDirectoryTextBoxTextChanged;
            this.Find<TextBox>("OutputFilenameTextBox").TextInput -= OutputFilenameTextBoxTextChanged;

            this.Find<TextBox>("OutputDirectoryTextBox").Text = env.OutputDirectory;
            this.Find<TextBox>("OutputFilenameTextBox").Text = env.OutputFilename;

            this.Find<TextBox>("OutputDirectoryTextBox").TextInput += OutputDirectoryTextBoxTextChanged;
            this.Find<TextBox>("OutputFilenameTextBox").TextInput += OutputFilenameTextBoxTextChanged;

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
            this.Find<TextBlock>("StatusLabel").Text = result.Message;

            // Set the index for the current disc type
            SetCurrentDiscType();

            this.Find<Button>("StartStopButton").IsEnabled = result && (Drives != null && Drives.Count > 0 ? true : false);

            // If we're in a type that doesn't support drive speeds
            this.Find<ComboBox>("DriveSpeedComboBox").IsEnabled = Env.Type.DoesSupportDriveSpeed();

            // If input params are not enabled, generate the full parameters from the environment
            if (!this.Find<TextBox>("ParametersTextBox").IsEnabled)
            {
                string generated = Env.GetFullParameters((int?)this.Find<ComboBox>("DriveSpeedComboBox").SelectedItem);
                if (generated != null)
                    this.Find<TextBox>("ParametersTextBox").Text = generated;
            }
        }

        /// <summary>
        /// Get the default output directory name from the currently selected drive
        /// </summary>
        /// <param name="driveChanged">Force an updated name if the drive letter changes</param>
        private void GetOutputNames(bool driveChanged)
        {
            Drive drive = this.Find<ComboBox>("DriveLetterComboBox").SelectedItem as Drive;
            KnownSystem? systemType = this.Find<ComboBox>("SystemTypeComboBox").SelectedItem as KnownSystemComboBoxItem;
            MediaType? mediaType = this.Find<ComboBox>("MediaTypeComboBox").SelectedItem as MediaType?;

            // Set the output directory, if we changed drives or it's not already
            if (driveChanged || string.IsNullOrEmpty(this.Find<TextBox>("OutputDirectoryTextBox").Text))
                this.Find<TextBox>("OutputDirectoryTextBox").Text = Path.Combine(UIOptions.Options.DefaultOutputPath, drive?.VolumeLabel ?? string.Empty);

            // Get the extension for the file for the next two statements
            string extension = Env?.GetExtension(mediaType);

            // Set the output filename, if we changed drives or it's not already
            if (driveChanged || string.IsNullOrEmpty(this.Find<TextBox>("OutputFilenameTextBox").Text))
                this.Find<TextBox>("OutputFilenameTextBox").Text = (drive?.VolumeLabel ?? systemType.LongName()) + (extension ?? ".bin");

            // If the extension for the file changed, update that automatically
            else if (Path.GetExtension(this.Find<TextBox>("OutputFilenameTextBox").Text) != extension)
                this.Find<TextBox>("OutputFilenameTextBox").Text = Path.GetFileNameWithoutExtension(this.Find<TextBox>("OutputFilenameTextBox").Text) + (extension ?? ".bin");
        }

        /// <summary>
        /// Get a complete list of active disc drives and fill the combo box
        /// </summary>
        /// <remarks>TODO: Find a way for this to periodically run, or have it hook to a "drive change" event</remarks>
        private void PopulateDrives()
        {
            ViewModels.LoggerViewModel.VerboseLogLn("Scanning for drives..");

            // Always enable the disk scan
            this.Find<Button>("MediaScanButton").IsEnabled = true;

            // Populate the list of drives and add it to the combo box
            Drives = Validators.CreateListOfDrives(UIOptions.Options.IgnoreFixedDrives);
            this.Find<ComboBox>("DriveLetterComboBox").Items = Drives;

            if (Drives.Any())
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
                this.Find<ComboBox>("DriveLetterComboBox").SelectedIndex = (index != -1 ? index : 0);
                this.Find<TextBlock>("StatusLabel").Text = "Valid drive found! Choose your Media Type";
                this.Find<Button>("CopyProtectScanButton").IsEnabled = true;

                // Get the current media type
                if (!UIOptions.Options.SkipSystemDetection && index != -1)
                {
                    ViewModels.LoggerViewModel.VerboseLog("Trying to detect system for drive {0}.. ", Drives[index].Letter);
                    var currentSystem = Validators.GetKnownSystem(Drives[index]);
                    ViewModels.LoggerViewModel.VerboseLogLn(currentSystem == null || currentSystem == KnownSystem.NONE ? "unable to detect." : ("detected " + currentSystem.LongName() + "."));

                    if (currentSystem != null && currentSystem != KnownSystem.NONE)
                    {
                        int sysIndex = Systems.FindIndex(s => s == currentSystem);
                        this.Find<ComboBox>("SystemTypeComboBox").SelectedIndex = sysIndex;
                    }
                }

                // Only enable the start/stop if we don't have the default selected
                this.Find<Button>("StartStopButton").IsEnabled = (this.Find<ComboBox>("SystemTypeComboBox").SelectedItem as KnownSystemComboBoxItem) != KnownSystem.NONE;

                ViewModels.LoggerViewModel.VerboseLogLn("Found {0} drives: {1}", Drives.Count, string.Join(", ", Drives.Select(d => d.Letter)));
            }
            else
            {
                this.Find<ComboBox>("DriveLetterComboBox").SelectedIndex = -1;
                this.Find<TextBlock>("StatusLabel").Text = "No valid drive found!";
                this.Find<Button>("StartStopButton").IsEnabled = false;
                this.Find<Button>("CopyProtectScanButton").IsEnabled = false;

                ViewModels.LoggerViewModel.VerboseLogLn("Found no drives");
            }
        }

        /// <summary>
        /// Populate media type according to system type
        /// </summary>
        private void PopulateMediaType()
        {
            KnownSystem? currentSystem = this.Find<ComboBox>("SystemTypeComboBox").SelectedItem as KnownSystemComboBoxItem;

            if (currentSystem != null)
            {
                MediaTypes = Validators.GetValidMediaTypes(currentSystem);
                this.Find<ComboBox>("MediaTypeComboBox").Items = MediaTypes;
                this.Find<ComboBox>("MediaTypeComboBox").IsEnabled = MediaTypes.Count > 1;
                this.Find<ComboBox>("MediaTypeComboBox").SelectedIndex = (MediaTypes.IndexOf(CurrentMediaType) >= 0 ? MediaTypes.IndexOf(CurrentMediaType) : 0);
            }
            else
            {
                this.Find<ComboBox>("MediaTypeComboBox").Items = null;
                this.Find<ComboBox>("MediaTypeComboBox").IsEnabled = false;
                this.Find<ComboBox>("MediaTypeComboBox").SelectedIndex = -1;
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

            this.Find<ComboBox>("SystemTypeComboBox").Items = Systems;
            this.Find<ComboBox>("SystemTypeComboBox").SelectedIndex = 0;

            this.Find<Button>("StartStopButton").IsEnabled = false;
        }

        /// <summary>
        /// Process the current custom parameters back into UI values
        /// </summary>
        private void ProcessCustomParameters()
        {
            Env.SetParameters(this.Find<TextBox>("ParametersTextBox").Text);
            if (Env.Parameters == null)
                return;

            int driveIndex = Drives.Select(d => d.Letter).ToList().IndexOf(Env.Parameters.InputPath()[0]);
            if (driveIndex > -1)
                this.Find<ComboBox>("DriveLetterComboBox").SelectedIndex = driveIndex;

            int driveSpeed = Env.Parameters.GetSpeed() ?? -1;
            if (driveSpeed > 0)
                this.Find<ComboBox>("DriveSpeedComboBox").SelectedItem = driveSpeed;
            else
                Env.Parameters.SetSpeed((int?)this.Find<ComboBox>("DriveSpeedComboBox").SelectedItem);

            string trimmedPath = Env.Parameters.OutputPath()?.Trim('"') ?? string.Empty;
            string outputDirectory = Path.GetDirectoryName(trimmedPath);
            string outputFilename = Path.GetFileName(trimmedPath);
            if (!string.IsNullOrWhiteSpace(outputDirectory))
                this.Find<TextBox>("OutputDirectoryTextBox").Text = outputDirectory;
            else
                outputDirectory = this.Find<TextBox>("OutputDirectoryTextBox").Text;

            if (!string.IsNullOrWhiteSpace(outputFilename))
                this.Find<TextBox>("OutputFilenameTextBox").Text = outputFilename;
            else
                outputFilename = this.Find<TextBox>("OutputFilenameTextBox").Text;

            MediaType? mediaType = Env.Parameters.GetMediaType();
            int mediaTypeIndex = MediaTypes.IndexOf(mediaType);
            if (mediaTypeIndex > -1)
                this.Find<ComboBox>("MediaTypeComboBox").SelectedIndex = mediaTypeIndex;
        }

        /// <summary>
        /// Scan and show copy protection for the current disc
        /// </summary>
        private async void ScanAndShowProtection()
        {
            // Determine current environment, just in case
            if (Env == null)
                Env = DetermineEnvironment();

            // Pull the drive letter from the UI directly, just in case
            var drive = this.Find<ComboBox>("DriveLetterComboBox").SelectedItem as Drive;
            if (drive.Letter != default(char))
            {
                ViewModels.LoggerViewModel.VerboseLogLn("Scanning for copy protection in {0}", drive.Letter);

                var tempContent = this.Find<TextBlock>("StatusLabel").Text;
                this.Find<TextBlock>("StatusLabel").Text = "Scanning for copy protection... this might take a while!";
                this.Find<Button>("StartStopButton").IsEnabled = false;
                this.Find<Button>("MediaScanButton").IsEnabled = false;
                this.Find<Button>("CopyProtectScanButton").IsEnabled = false;

                var progress = new Progress<FileProtection>();
                progress.ProgressChanged += ProgressUpdated;
                string protections = await Validators.RunProtectionScanOnPath(drive.Letter + ":\\");

                // If SmartE is detected on the current disc, remove `/sf` from the flags for DIC only
                if (Env.InternalProgram == InternalProgram.DiscImageCreator && protections.Contains("SmartE"))
                {
                    ((DiscImageCreator.Parameters)Env.Parameters)[DiscImageCreator.Flag.ScanFileProtect] = false;
                    ViewModels.LoggerViewModel.VerboseLogLn($"SmartE detected, removing {DiscImageCreator.FlagStrings.ScanFileProtect} from parameters");
                }

                if (!ViewModels.LoggerViewModel.WindowVisible)
                {
                    new Window()
                    {
                        Title = "Detected Protection",
                        Content = protections,
                        SizeToContent = SizeToContent.WidthAndHeight,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    }.ShowDialog(this);
                }

                ViewModels.LoggerViewModel.VerboseLog("Detected the following protections in {0}:\r\n\r\n{1}", drive.Letter, protections);

                this.Find<TextBlock>("StatusLabel").Text = tempContent;
                this.Find<Button>("StartStopButton").IsEnabled = true;
                this.Find<Button>("MediaScanButton").IsEnabled = true;
                this.Find<Button>("CopyProtectScanButton").IsEnabled = true;
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
                this.Find<ComboBox>("MediaTypeComboBox").SelectedIndex = index;
            else
                this.Find<TextBlock>("StatusLabel").Text = $"Disc of type '{Converters.LongName(CurrentMediaType)}' found, but the current system does not support it!";
        }

        /// <summary>
        /// Set the drive speed based on reported maximum and user-defined option
        /// </summary>
        private void SetSupportedDriveSpeed()
        {
            // Set the drive speed list that's appropriate
            var values = Interface.GetSpeedsForMediaType(CurrentMediaType);
            this.Find<ComboBox>("DriveSpeedComboBox").Items = values;
            ViewModels.LoggerViewModel.VerboseLogLn("Supported media speeds: {0}", string.Join(",", values));

            // Set the selected speed
            int speed = UIOptions.GetPreferredDumpSpeedForMediaType(CurrentMediaType);
            ViewModels.LoggerViewModel.VerboseLogLn("Setting drive speed to: {0}", speed);
            this.Find<ComboBox>("DriveSpeedComboBox").SelectedItem = speed;
        }

        /// <summary>
        /// Show the disc information window
        /// </summary>
        /// <param name="submissionInfo">SubmissionInfo object to display and possibly change</param>
        /// <returns>Dialog open result</returns>
        private bool? ShowDiscInformationWindow(SubmissionInfo submissionInfo)
        {
            var discInformationWindow = new DiscInformationWindow(submissionInfo);
            discInformationWindow.Load();
            discInformationWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            discInformationWindow.ShowDialog(this).ConfigureAwait(false).GetAwaiter().GetResult();
            return true;
        }

        /// <summary>
        /// Begin the dumping process using the given inputs
        /// </summary>
        private async void StartDumping()
        {
            // One last check to determine environment, just in case
            Env = DetermineEnvironment();

            // If still in custom parameter mode, check that users meant to continue or not
            if (this.Find<CheckBox>("EnableParametersCheckBox").IsChecked == true)
            {
                MessageBoxResult result = await MessageBox.Show(this, "It looks like you have custom parameters that have not been saved. Would you like to apply those changes before starting to dump?", "Custom Changes", MessageBoxButtons.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    this.Find<CheckBox>("EnableParametersCheckBox").IsChecked = false;
                    this.Find<TextBox>("ParametersTextBox").IsEnabled = false;
                    ProcessCustomParameters();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
                // If "No", then we continue with the current known environment
            }

            // Fix the output paths
            Env.FixOutputPaths();

            try
            {
                // Validate that the user explicitly wants an inactive drive to be considered for dumping
                if (!Env.Drive.MarkedActive)
                {
                    MessageBoxResult mbresult = await MessageBox.Show(this, "The currently selected drive does not appear to contain a disc! Are you sure you want to continue?", "Missing Disc", MessageBoxButtons.YesNo);
                    if (mbresult == MessageBoxResult.No || mbresult == MessageBoxResult.Cancel)
                    {
                        ViewModels.LoggerViewModel.VerboseLogLn("Dumping aborted!");
                        return;
                    }
                }

                // If a complete dump already exists
                if (Env.FoundAllFiles())
                {
                    MessageBoxResult mbresult = await MessageBox.Show(this, "A complete dump already exists! Are you sure you want to overwrite?", "Overwrite?", MessageBoxButtons.YesNo);
                    if (mbresult == MessageBoxResult.No || mbresult == MessageBoxResult.Cancel)
                    {
                        ViewModels.LoggerViewModel.VerboseLogLn("Dumping aborted!");
                        return;
                    }
                }

                this.Find<Button>("StartStopButton").Content = Interface.StopDumping;
                this.Find<Button>("CopyProtectScanButton").IsEnabled = false;
                this.Find<TextBlock>("StatusLabel").Text = "Beginning dumping process";
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
                    this.Find<TextBlock>("StatusLabel").Text = "Execution complete!";
                    this.Find<Button>("StartStopButton").Content = Interface.StartDumping;
                    this.Find<Button>("CopyProtectScanButton").IsEnabled = true;
                    return;
                }

                if (result)
                {
                    // Verify dump output and save it
                    result = await Env.VerifyAndSaveDumpOutput(resultProgress,
                        protectionProgress,
                        this.Find<CheckBox>("EjectWhenDoneCheckBox").IsChecked,
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
                this.Find<Button>("StartStopButton").Content = Interface.StartDumping;
                this.Find<Button>("CopyProtectScanButton").IsEnabled = true;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for AboutMenuItem Click event
        /// </summary>
        private void AboutMenuItemClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this, $"darksabre76 - Project Lead / Backend Design"
                + $"{Environment.NewLine}ReignStumble - Former Project Lead / UI Design"
                + $"{Environment.NewLine}Jakz - Primary Feature Contributor"
                + $"{Environment.NewLine}NHellFire - Feature Contributor", "About", MessageBoxButtons.Ok);
        }

        /// <summary>
        /// Handler for AppExitMenuItem Click event
        /// </summary>
        private void AppExitMenuItemClick(object sender, RoutedEventArgs e)
        {
            logWindow?.Close();
            Close();
        }

        /// <summary>
        /// Handler for CheckForUpdatesMenuItem Click event
        /// </summary>
        private void CheckForUpdatesMenuItemClick(object sender, RoutedEventArgs e)
        {
            (bool different, string message, string url) = Tools.CheckForNewVersion();

            // If we have a new version, put it in the clipboard
            if (different)
                Application.Current.Clipboard.SetTextAsync(url);

            MessageBox.Show(this, message, "Version Update Check", MessageBoxButtons.Ok);
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
            if (this.Find<CheckBox>("EnableParametersCheckBox").IsChecked == true)
                this.Find<TextBox>("ParametersTextBox").IsEnabled = true;
            else
            {
                this.Find<TextBox>("ParametersTextBox").IsEnabled = false;
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
                CurrentMediaType = this.Find<ComboBox>("MediaTypeComboBox").SelectedItem as MediaType?;
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
        private void MainWindowClosing(object sender, CancelEventArgs e)
        {
            if (logWindow.IsVisible)
                logWindow?.Close();
        }

        /// <summary>
        /// Handler for MainWindow PositionChanged event
        /// </summary>
        private void MainWindowLocationChanged(object sender, PixelPointEventArgs e)
        {
            if (logWindow.IsVisible)
                logWindow.AdjustPositionToMainWindow();
        }

        /// <summary>
        /// Handler for OptionsMenuItem Click event
        /// </summary>
        private async void OptionsMenuItemClick(object sender, RoutedEventArgs e)
        {
            // Show the window and wait for the response
            var optionsWindow = new OptionsWindow();
            optionsWindow.UIOptions = UIOptions;
            optionsWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            optionsWindow.Refresh();
            await optionsWindow.ShowDialog(this);

            // Set any new options
            PopulateDrives();
            GetOutputNames(false);
            SetSupportedDriveSpeed();
            EnsureDiscInformation();
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
        // TODO: Not triggered properly
        private void OutputDirectoryTextBoxTextChanged(object sender, TextInputEventArgs e)
        {
            EnsureDiscInformation();
        }

        /// <summary>
        /// Handler for OutputFilenameTextBox TextInput event
        /// </summary>
        // TODO: Not triggered properly
        private void OutputFilenameTextBoxTextChanged(object sender, TextInputEventArgs e)
        {
            EnsureDiscInformation();
        }

        /// <summary>
        /// Handler for Result ProgressChanged event
        /// </summary>
        private void ProgressUpdated(object sender, Result value)
        {
            this.Find<TextBlock>("StatusLabel").Text = value.Message;
            ViewModels.LoggerViewModel.VerboseLogLn(value.Message);
        }

        /// <summary>
        /// Handler for FileProtection ProgressChanged event
        /// </summary>
        private void ProgressUpdated(object sender, FileProtection value)
        {
            string message = $"{value.Percentage * 100:N2}%: {value.Filename} - {value.Protection}";
            this.Find<TextBlock>("StatusLabel").Text = message;
            ViewModels.LoggerViewModel.VerboseLogLn(message);
        }

        /// <summary>
        /// Handler for StartStopButton Click event
        /// </summary>
        private void StartStopButtonClick(object sender, RoutedEventArgs e)
        {
            // Dump or stop the dump
            if ((string)this.Find<Button>("StartStopButton").Content == Interface.StartDumping)
            {
                StartDumping();
            }
            else if ((string)this.Find<Button>("StartStopButton").Content == Interface.StopDumping)
            {
                ViewModels.LoggerViewModel.VerboseLogLn("Canceling dumping process...");
                Env.CancelDumping();
                this.Find<Button>("CopyProtectScanButton").IsEnabled = true;

                if (this.Find<CheckBox>("EjectWhenDoneCheckBox").IsChecked == true)
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
            if ((this.Find<ComboBox>("SystemTypeComboBox").SelectedItem as KnownSystemComboBoxItem).IsHeader())
            {
                this.Find<ComboBox>("SystemTypeComboBox").SelectedIndex++;
                return;
            }

            ViewModels.LoggerViewModel.VerboseLogLn("Changed system to: {0}", (this.Find<ComboBox>("SystemTypeComboBox").SelectedItem as KnownSystemComboBoxItem).Name);
            PopulateMediaType();
            GetOutputNames(false);
            EnsureDiscInformation();
        }

        #endregion
    }
}
