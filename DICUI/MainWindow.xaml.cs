using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;
using DICUI.Data;
using DICUI.Utilities;
using DICUI.UI;

namespace DICUI
{
    public partial class MainWindow : Window
    {
        // Private UI-related variables
        private List<Drive> _drives;
        private MediaType? _currentMediaType;
        private List<KnownSystem?> _systems;
        private List<MediaType?> _mediaTypes;
        private bool _alreadyShown;

        private DumpEnvironment _env;

        // Option related
        private Options _options;
        private OptionsWindow _optionsWindow;

        private LogWindow _logWindow;

        public MainWindow()
        {
            InitializeComponent();

            // Initializes and load Options object
            _options = new Options();
            _options.Load();
            ViewModels.OptionsViewModel = new OptionsViewModel(_options);

            _logWindow = new LogWindow(this);
            ViewModels.LoggerViewModel.SetWindow(_logWindow);

            // Disable buttons until we load fully
            StartStopButton.IsEnabled = false;
            DiskScanButton.IsEnabled = false;
            CopyProtectScanButton.IsEnabled = false;

            if (_options.OpenLogWindowAtStartup)
            {
                System.Drawing.Rectangle bounds = WinForms.Screen.PrimaryScreen.WorkingArea;

                this.WindowStartupLocation = WindowStartupLocation.Manual;
                double combinedHeight = this.Height + _logWindow.Height + UIElements.LogWindowMarginFromMainWindow;

                this.Left = bounds.Left + (bounds.Width - this.Width) / 2;
                this.Top = bounds.Top + (bounds.Height - combinedHeight) / 2;
            }
        }


        #region Events

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (_alreadyShown)
                return;

            _alreadyShown = true;

            if (_options.OpenLogWindowAtStartup)
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

        private void StartStopButtonClick(object sender, RoutedEventArgs e)
        {
            // Dump or stop the dump
            if ((string)StartStopButton.Content == UIElements.StartDumping)
            {
                StartDumping();
            }
            else if ((string)StartStopButton.Content == UIElements.StopDumping)
            {
                _env.CancelDumping();
                CopyProtectScanButton.IsEnabled = true;

                if (EjectWhenDoneCheckBox.IsChecked == true)
                    _env.EjectDisc();
            }
        }

        private void OutputDirectoryBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            BrowseFolder();
            EnsureDiscInformation();
        }

        private void DiskScanButtonClick(object sender, RoutedEventArgs e)
        {
            PopulateDrives();
        }

        private void CopyProtectScanButtonClick(object sender, RoutedEventArgs e)
        {
            ScanAndShowProtection();
        }

        private void SystemTypeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If we're on a separator, go to the next item and return
            if ((SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem).IsHeader())
            {
                SystemTypeComboBox.SelectedIndex++;
                return;
            }

            PopulateMediaType();
            EnsureDiscInformation();
        }

        private void MediaTypeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Only change the media type if the selection and not the list has changed
            if (e.RemovedItems.Count == 1 && e.AddedItems.Count == 1)
            {
                _currentMediaType = MediaTypeComboBox.SelectedItem as MediaType?;
            }

            // TODO: This is giving people the benefit of the doubt that their change is valid
            SetSupportedDriveSpeed();
            GetOutputNames();
            EnsureDiscInformation();
        }

        private void DriveLetterComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CacheCurrentDiscType();
            SetCurrentDiscType();
            GetOutputNames();
            SetSupportedDriveSpeed();
        }

        private void DriveSpeedComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnsureDiscInformation();
        }

        private void OutputFilenameTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            EnsureDiscInformation();
        }

        private void OutputDirectoryTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            EnsureDiscInformation();
        }

        private void ProgressUpdated(object sender, Result value)
        {
            StatusLabel.Content = value.Message;
        }

        private void MainWindowLocationChanged(object sender, EventArgs e)
        {
            if (_logWindow.IsVisible)
                _logWindow.AdjustPositionToMainWindow();
        }

        private void MainWindowActivated(object sender, EventArgs e)
        {
            if (_logWindow.IsVisible && !this.Topmost)
            {
                _logWindow.Topmost = true;
                _logWindow.Topmost = false;
            }
        }

        private void MainWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_logWindow.IsVisible)
                _logWindow.Close();
        }

        // Toolbar Events

        private void AppExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void AboutClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"ReignStumble - Project Lead / UI Design{Environment.NewLine}" +
                $"darksabre76 - Project Co-Lead / Backend Design{Environment.NewLine}" +
                $"Jakz - Feature Contributor{Environment.NewLine}" +
                $"NHellFire - Feature Contributor{Environment.NewLine}" +
                $"Dizzzy - Concept/Ideas/Beta tester{Environment.NewLine}", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OptionsClick(object sender, RoutedEventArgs e)
        {
            // lazy initialization
            if (_optionsWindow == null)
            {
                _optionsWindow = new OptionsWindow(this, _options);
                _optionsWindow.Closed += delegate
                {
                    _optionsWindow = null;
                };
            }

            _optionsWindow.Owner = this;
            _optionsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            _optionsWindow.Refresh();
            _optionsWindow.Show();
        }

        public void OnOptionsUpdated()
        {
            GetOutputNames();
            SetSupportedDriveSpeed();
            EnsureDiscInformation();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Populate media type according to system type
        /// </summary>
        private void PopulateMediaType()
        {
            KnownSystem? currentSystem = SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem;

            if (currentSystem != null)
            {
                _mediaTypes = Validators.GetValidMediaTypes(currentSystem).ToList();
                MediaTypeComboBox.ItemsSource = _mediaTypes;

                MediaTypeComboBox.IsEnabled = _mediaTypes.Count > 1;
                MediaTypeComboBox.SelectedIndex = (_mediaTypes.IndexOf(_currentMediaType) >= 0 ? _mediaTypes.IndexOf(_currentMediaType) : 0);
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
            _systems = Validators.CreateListOfSystems();

            ViewModels.LoggerViewModel.VerboseLogLn("Populating systems, {0} systems found.", _systems.Count);

            Dictionary <KnownSystemCategory, List<KnownSystem?>> mapping = _systems
                .GroupBy(s => s.Category())
                .ToDictionary(
                    k => k.Key,
                    v => v
                        .OrderBy(s => s.Name())
                        .ToList()
                );

            List<KnownSystemComboBoxItem> comboBoxItems = new List<KnownSystemComboBoxItem>();

            comboBoxItems.Add(new KnownSystemComboBoxItem(KnownSystem.NONE));

            foreach (var group in mapping)
            {
                comboBoxItems.Add(new KnownSystemComboBoxItem(group.Key));
                group.Value.ForEach(system => comboBoxItems.Add(new KnownSystemComboBoxItem(system)));
            }

            SystemTypeComboBox.ItemsSource = comboBoxItems;
            SystemTypeComboBox.SelectedIndex = 0;

            StartStopButton.IsEnabled = false;
        }

        /// <summary>
        /// Get a complete list of active disc drives and fill the combo box
        /// </summary>
        /// <remarks>TODO: Find a way for this to periodically run, or have it hook to a "drive change" event</remarks>
        private void PopulateDrives()
        {
            ViewModels.LoggerViewModel.VerboseLogLn("Scanning for drives..");
            
            // Always enable the disk scan
            DiskScanButton.IsEnabled = true;

            // Populate the list of drives and add it to the combo box
            _drives = Validators.CreateListOfDrives();
            DriveLetterComboBox.ItemsSource = _drives;

            if (DriveLetterComboBox.Items.Count > 0)
            {
                int index = _drives.FindIndex(d => d.MarkedActive);
                DriveLetterComboBox.SelectedIndex = (index != -1 ? index : 0);
                StatusLabel.Content = "Valid media found! Choose your Media Type";
                StartStopButton.IsEnabled = true;
                CopyProtectScanButton.IsEnabled = true;

                ViewModels.LoggerViewModel.VerboseLogLn("Found {0} drives containing media: {1}", _drives.Count, String.Join(", ", _drives.Select(d => d.Letter)));
            }
            else
            {
                DriveLetterComboBox.SelectedIndex = -1;
                StatusLabel.Content = "No valid media found!";
                StartStopButton.IsEnabled = false;
                CopyProtectScanButton.IsEnabled = false;

                ViewModels.LoggerViewModel.VerboseLogLn("Found no drives contaning valid media.");
            }
        }

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
        /// Create a DumpEnvironment with all current settings
        /// </summary>
        /// <returns>Filled DumpEnvironment instance</returns>
        private DumpEnvironment DetermineEnvironment()
        {
            return new DumpEnvironment()
            {
                // Paths to tools
                SubdumpPath = _options.SubDumpPath,
                DICPath = _options.DICPath,

                OutputDirectory = OutputDirectoryTextBox.Text,
                OutputFilename = OutputFilenameTextBox.Text,

                // Get the currently selected options
                Drive = DriveLetterComboBox.SelectedItem as Drive,

                DICParameters = new Parameters(ParametersTextBox.Text),

                QuietMode = _options.QuietMode,
                ParanoidMode = _options.ParanoidMode,
                RereadAmountC2 = _options.RereadAmountForC2,

                System = SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem,
                Type = MediaTypeComboBox.SelectedItem as MediaType?
            };
        }

        /// <summary>
        /// Begin the dumping process using the given inputs
        /// </summary>
        /// <remarks>
        /// TODO: Add the scan and output steps back here for label updates
        /// </remarks>
        private async void StartDumping()
        {
            _env = DetermineEnvironment();

            StartStopButton.Content = UIElements.StopDumping;
            CopyProtectScanButton.IsEnabled = false;
            StatusLabel.Content = "Beginning dumping process";
            ViewModels.LoggerViewModel.VerboseLogLn("Starting dumping process..");

            var progress = new Progress<Result>();
            progress.ProgressChanged += ProgressUpdated;
            Result result = await _env.StartDumping(progress);

            StatusLabel.Content = result ? "Dumping complete!" : result.Message;
            StartStopButton.Content = UIElements.StartDumping;
            CopyProtectScanButton.IsEnabled = true;

            if (EjectWhenDoneCheckBox.IsChecked == true)
                _env.EjectDisc();
        }

        /// <summary>
        /// Ensure information is consistent with the currently selected disc type
        /// </summary>
        private void EnsureDiscInformation()
        {
            // Get the current environment information
            _env = DetermineEnvironment();

            // Take care of null cases
            if (_env.System == null)
                _env.System = KnownSystem.NONE;
            if (_env.Type == null)
                _env.Type = MediaType.NONE;

            // Get the status to write out
            Result result = Validators.GetSupportStatus(_env.System, _env.Type);
            StatusLabel.Content = result.Message;

            // Set the index for the current disc type
            SetCurrentDiscType();

            StartStopButton.IsEnabled = result && (_drives != null && _drives.Count > 0 ? true : false);

            // If we're in a type that doesn't support drive speeds
            DriveSpeedComboBox.IsEnabled = _env.Type.DoesSupportDriveSpeed() && _env.System.DoesSupportDriveSpeed();

            // Special case for Custom input
            if (_env.System == KnownSystem.Custom)
            {
                ParametersTextBox.IsEnabled = true;
                OutputFilenameTextBox.IsEnabled = false;
                OutputDirectoryTextBox.IsEnabled = false;
                OutputDirectoryBrowseButton.IsEnabled = false;
                DriveLetterComboBox.IsEnabled = false;
                DriveSpeedComboBox.IsEnabled = false;
                StartStopButton.IsEnabled = (_drives.Count > 0 ? true : false);
                StatusLabel.Content = "User input mode";
            }
            else
            {
                ParametersTextBox.IsEnabled = false;
                OutputFilenameTextBox.IsEnabled = true;
                OutputDirectoryTextBox.IsEnabled = true;
                OutputDirectoryBrowseButton.IsEnabled = true;
                DriveLetterComboBox.IsEnabled = true;

                // Generate the full parameters from the environment
                string generated = _env.GetFullParameters((int?)DriveSpeedComboBox.SelectedItem);
                if (generated != null)
                    ParametersTextBox.Text = generated;
            }
        }

        /// <summary>
        /// Get the default output directory name from the currently selected drive
        /// </summary>
        private void GetOutputNames()
        {
            Drive drive = DriveLetterComboBox.SelectedItem as Drive;
            KnownSystem? systemType = SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem;
            MediaType? mediaType = MediaTypeComboBox.SelectedItem as MediaType?;

            if (drive != null
                && !String.IsNullOrWhiteSpace(drive.VolumeLabel)
                && !drive.IsFloppy
                && systemType != null
                && mediaType != null)
            {
                OutputDirectoryTextBox.Text = Path.Combine(_options.DefaultOutputPath, drive.VolumeLabel);
                OutputFilenameTextBox.Text = drive.VolumeLabel + mediaType.Extension();
            }
            else
            {
                OutputDirectoryTextBox.Text = _options.DefaultOutputPath;
                OutputFilenameTextBox.Text = "disc.bin";
            }
        }

        /// <summary>
        /// Scan and show copy protection for the current disc
        /// </summary>
        private async void ScanAndShowProtection()
        {
            var env = DetermineEnvironment();
            if (env.Drive.Letter != default(char))
            {
                ViewModels.LoggerViewModel.VerboseLogLn("Scanning for copy protection in {0}", _env.Drive.Letter);

                var tempContent = StatusLabel.Content;
                StatusLabel.Content = "Scanning for copy protection... this might take a while!";
                StartStopButton.IsEnabled = false;
                DiskScanButton.IsEnabled = false;
                CopyProtectScanButton.IsEnabled = false;

                string protections = await Validators.RunProtectionScanOnPath(env.Drive.Letter + ":\\");
                if (!ViewModels.LoggerViewModel.WindowVisible)
                    MessageBox.Show(protections, "Detected Protection", MessageBoxButton.OK, MessageBoxImage.Information);
                ViewModels.LoggerViewModel.VerboseLog("Detected the following protections in {0}:\r\n\r\n{1}", env.Drive.Letter, protections);

                StatusLabel.Content = tempContent;
                StartStopButton.IsEnabled = true;
                DiskScanButton.IsEnabled = true;
                CopyProtectScanButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Set the drive speed based on reported maximum and user-defined option
        /// </summary>
        private async void SetSupportedDriveSpeed()
        {
            // Set the drive speed list that's appropriate
            var values = AllowedSpeeds.GetForMediaType(_currentMediaType);
            DriveSpeedComboBox.ItemsSource = values;
            DriveSpeedComboBox.SelectedIndex = values.Count / 2;

            // Get the current environment
            _env = DetermineEnvironment();

            // Get the drive speed
            int speed = await _env.GetDiscSpeed();

            // If we have an invalid speed, we need to jump out
            // TODO: Should we disable dumping in this case?
            if (speed == -1)
                return;

            ViewModels.LoggerViewModel.VerboseLogLn("Determined max drive speed for {0}: {1}.", _env.Drive.Letter, speed);

            // Choose the lower of the two speeds between the allowed speeds and the user-defined one
            int chosenSpeed = Math.Min(
                AllowedSpeeds.GetForMediaType(_currentMediaType).Where(s => s <= speed).Last(),
                _options.preferredDumpSpeedCD
            );

            DriveSpeedComboBox.SelectedValue = chosenSpeed;
        }

        /// <summary>
        /// Cache the current disc type to internal variable
        /// </summary>
        private void CacheCurrentDiscType()
        {
            // Get the drive letter from the selected item
            Drive drive = DriveLetterComboBox.SelectedItem as Drive;
            if (drive == null || drive.IsFloppy)
                return;

            // Get the current optical disc type
            if (!_options.SkipMediaTypeDetection)
            {
                ViewModels.LoggerViewModel.VerboseLog("Trying to detect media type for drive {0}.. ", drive.Letter);
                _currentMediaType = Validators.GetDiscType(drive.Letter);
                ViewModels.LoggerViewModel.VerboseLogLn(_currentMediaType != null ? "unable to detect." : ("detected " + _currentMediaType.Name() + "."));
            }
        }

        /// <summary>
        /// Set the current disc type in the combo box
        /// </summary>
        private void SetCurrentDiscType()
        {
            // If we have an invalid current type, we don't care and return
            if (_currentMediaType == null || _currentMediaType == MediaType.NONE)
                return;

            // Now set the selected item, if possible
            int index = _mediaTypes.FindIndex(kvp => kvp.Value == _currentMediaType);
            if (index != -1)
                MediaTypeComboBox.SelectedIndex = index;
            else
                StatusLabel.Content = $"Disc of type '{Converters.MediaTypeToString(_currentMediaType)}' found, but the current system does not support it!";
        }

        #endregion
    }
}
