using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using DICUI.Data;
using DICUI.Utilities;

namespace DICUI.Forms.Windows
{
    public partial class MainWindow : Form
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
            StartStopButton.Enabled = false;
            DiscScanButton.Enabled = false;
            CopyProtectScanButton.Enabled = false;

            if (_options.OpenLogWindowAtStartup)
            {
                this.StartPosition = FormStartPosition.CenterScreen;
                double combinedHeight = this.Height + _logWindow.Height + Constants.LogWindowMarginFromMainWindow;
                Rectangle bounds = GetScaledCoordinates(Screen.PrimaryScreen.WorkingArea);

                this.Left = (int)(bounds.Left + (bounds.Width - this.Width) / 2);
                this.Top = (int)(bounds.Top + (bounds.Height - combinedHeight) / 2);
            }
        }

        #region Events

        private void OnContentRendered(object sender, EventArgs e)
        {
            if (_alreadyShown)
                return;

            _alreadyShown = true;

            if (_options.OpenLogWindowAtStartup)
            {
                //TODO: this should be bound directly to WindowVisible property in two way fashion
                // we need to study how to properly do it in XAML
                showLogWindowToolStripMenuItem.Checked = true;
                ViewModels.LoggerViewModel.WindowVisible = true;
            }

            // Populate the list of systems
            StatusLabel.Text = "Creating system list, please wait!";
            PopulateSystems();

            // Populate the list of drives
            StatusLabel.Text = "Creating drive list, please wait!";
            PopulateDrives();

            // Set the position of the log window
            if (_logWindow.Visible)
            {
                _logWindow.AdjustPositionToMainWindow();
                this.Focus();
            }
        }

        private void StartStopButtonClick(object sender, EventArgs e)
        {
            // Dump or stop the dump
            if (StartStopButton.Text == Constants.StartDumping)
            {
                StartDumping();
            }
            else if (StartStopButton.Text == Constants.StopDumping)
            {
                ViewModels.LoggerViewModel.VerboseLogLn("Canceling dumping process...");
                _env.CancelDumping();
                CopyProtectScanButton.Enabled = true;

                if (EjectWhenDoneCheckBox.Checked == true)
                {
                    ViewModels.LoggerViewModel.VerboseLogLn($"Ejecting disc in drive {_env.Drive.Letter}");
                    _env.EjectDisc();
                }
            }
        }

        private void OutputDirectoryBrowseButtonClick(object sender, EventArgs e)
        {
            BrowseFolder();
            EnsureDiscInformation();
        }

        private void DiscScanButtonClick(object sender, EventArgs e)
        {
            PopulateDrives();
        }

        private void CopyProtectScanButtonClick(object sender, EventArgs e)
        {
            ScanAndShowProtection();
        }

        private void SystemTypeComboBoxSelectionChanged(object sender, EventArgs e)
        {
            // If we're on a separator, go to the next item and return
            if ((SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem).IsHeader())
            {
                SystemTypeComboBox.SelectedIndex++;
                return;
            }

            ViewModels.LoggerViewModel.VerboseLogLn("Changed system to: {0}", (SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem).Name);
            PopulateMediaType();
            EnsureDiscInformation();
        }

        private void MediaTypeComboBoxSelectionChanged(object sender, EventArgs e)
        {
            _currentMediaType = MediaTypeComboBox.SelectedItem as MediaType?;
            SetSupportedDriveSpeed();
            GetOutputNames();
            EnsureDiscInformation();
        }

        private void DriveLetterComboBoxSelectionChanged(object sender, EventArgs e)
        {
            CacheCurrentDiscType();
            SetCurrentDiscType();
            GetOutputNames();
            SetSupportedDriveSpeed();
        }

        private void DriveSpeedComboBoxSelectionChanged(object sender, EventArgs e)
        {
            EnsureDiscInformation();
        }

        private void OutputFilenameTextBoxTextChanged(object sender, EventArgs e)
        {
            EnsureDiscInformation();
        }

        private void OutputDirectoryTextBoxTextChanged(object sender, EventArgs e)
        {
            EnsureDiscInformation();
        }

        private void ProgressUpdated(object sender, Result value)
        {
            StatusLabel.Text = value.Message;
            ViewModels.LoggerViewModel.VerboseLogLn(value.Message);
        }

        private void MainWindowLocationChanged(object sender, EventArgs e)
        {
            if (_logWindow.Visible)
                _logWindow.AdjustPositionToMainWindow();
        }

        private void MainWindowActivated(object sender, EventArgs e)
        {
            if (!_logWindow.Visible)
                _logWindow.BringToFront();
            this.BringToFront();
        }

        private void MainWindowClosing(object sender, FormClosingEventArgs e)
        {
            if (_logWindow.Visible)
                _logWindow.Close();
        }

        private void EnableParametersCheckBoxClick(object sender, EventArgs e)
        {
            if (EnableParametersCheckBox.Checked)
            {
                ParametersTextBox.ReadOnly = false;
                ParametersTextBox.Enabled = true;
            }
            else
            {
                ParametersTextBox.ReadOnly = true;
                ParametersTextBox.Enabled = false;
                ProcessCustomParameters();
            }
        }

        // Toolbar Events

        private void AppExitClick(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void AboutClick(object sender, EventArgs e)
        {
            System.Windows.MessageBox.Show($"ReignStumble - Project Lead / UI Design{Environment.NewLine}" +
                $"darksabre76 - Project Co-Lead / Backend Design{Environment.NewLine}" +
                $"Jakz - Feature Contributor{Environment.NewLine}" +
                $"NHellFire - Feature Contributor{Environment.NewLine}" +
                $"Dizzzy - Concept/Ideas/Beta tester{Environment.NewLine}", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OptionsClick(object sender, EventArgs e)
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
            _optionsWindow.StartPosition = FormStartPosition.CenterParent;
            _optionsWindow.RefreshSettings();
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

                var comboBoxItems = new List<MediaTypeComboBoxItem>();
                foreach (var mediaType in _mediaTypes)
                    comboBoxItems.Add(new MediaTypeComboBoxItem(mediaType));

                MediaTypeComboBox.DataSource = comboBoxItems;

                MediaTypeComboBox.Enabled = _mediaTypes.Count > 1;
                MediaTypeComboBox.SelectedIndex = (_mediaTypes.IndexOf(_currentMediaType) >= 0 ? _mediaTypes.IndexOf(_currentMediaType) : 0);
            }
            else
            {
                MediaTypeComboBox.Enabled = false;
                MediaTypeComboBox.DataSource = null;
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

            Dictionary<KnownSystemCategory, List<KnownSystem?>> mapping = _systems
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

            SystemTypeComboBox.DataSource = comboBoxItems;
            SystemTypeComboBox.SelectedIndex = 0;

            StartStopButton.Enabled = false;
        }

        /// <summary>
        /// Get a complete list of active disc drives and fill the combo box
        /// </summary>
        /// <remarks>TODO: Find a way for this to periodically run, or have it hook to a "drive change" event</remarks>
        private void PopulateDrives()
        {
            ViewModels.LoggerViewModel.VerboseLogLn("Scanning for drives..");

            // Always enable the disk scan
            DiscScanButton.Enabled = true;

            // Populate the list of drives and add it to the combo box
            _drives = Validators.CreateListOfDrives();
            DriveLetterComboBox.DataSource = _drives;

            if (DriveLetterComboBox.Items.Count > 0)
            {
                int index = _drives.FindIndex(d => d.MarkedActive);
                DriveLetterComboBox.SelectedIndex = (index != -1 ? index : 0);
                StatusLabel.Text = "Valid drive found! Choose your Media Type";
                StartStopButton.Enabled = true;
                CopyProtectScanButton.Enabled = true;

                ViewModels.LoggerViewModel.VerboseLogLn("Found {0} drives: {1}", _drives.Count, String.Join(", ", _drives.Select(d => d.Letter)));
            }
            else
            {
                DriveLetterComboBox.SelectedIndex = -1;
                StatusLabel.Text = "No valid drive found!";
                StartStopButton.Enabled = false;
                CopyProtectScanButton.Enabled = false;

                ViewModels.LoggerViewModel.VerboseLogLn("Found no drives");
            }
        }

        /// <summary>
        /// Browse for an output folder
        /// </summary>
        private void BrowseFolder()
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog { ShowNewFolderButton = false, SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory };
            DialogResult result = folderDialog.ShowDialog();

            if (result == DialogResult.OK)
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
            // Populate the new environment
            var env = new DumpEnvironment()
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
                ScanForProtection = _options.ScanForProtection,
                RereadAmountC2 = _options.RereadAmountForC2,

                System = SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem,
                Type = MediaTypeComboBox.SelectedItem as MediaTypeComboBoxItem,
            };

            // Fix the output paths
            env.FixOutputPaths();

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
        /// Begin the dumping process using the given inputs
        /// </summary>
        private async void StartDumping()
        {
            if (_env == null)
                _env = DetermineEnvironment();

            // If still in custom parameter mode, check that users meant to continue or not
            if (EnableParametersCheckBox.Checked == true)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("It looks like you have custom parameters that have not been saved. Would you like to apply those changes before starting to dump?", "Custom Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    EnableParametersCheckBox.Checked = false;
                    ParametersTextBox.Enabled = false;
                    ProcessCustomParameters();
                }
                else if (result == MessageBoxResult.Cancel)
                    return;
                // If "No", then we continue with the current known environment
            }

            try
            {
                // Check for the firmware first
                // TODO: Remove this (and method) once DIC end-to-end logging becomes a thing
                if (!await _env.DriveHasLatestFimrware())
                {
                    System.Windows.MessageBox.Show($"DiscImageCreator has reported that drive {_env.Drive.Letter} is not updated to the most recent firmware. Please update the firmware for your drive and try again.", "Outdated Firmware", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Validate that the user explicitly wants an inactive drive to be considered for dumping
                if (!_env.Drive.MarkedActive)
                {
                    MessageBoxResult mbresult = System.Windows.MessageBox.Show("The currently selected drive does not appear to contain a disc! Are you sure you want to continue?", "Missing Disc", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    if (mbresult == MessageBoxResult.No || mbresult == MessageBoxResult.Cancel || mbresult == MessageBoxResult.None)
                    {
                        ViewModels.LoggerViewModel.VerboseLogLn("Dumping aborted!");
                        return;
                    }
                }

                // If a complete dump already exists
                if (_env.FoundAllFiles())
                {
                    MessageBoxResult mbresult = System.Windows.MessageBox.Show("A complete dump already exists! Are you sure you want to overwrite?", "Overwrite?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    if (mbresult == MessageBoxResult.No || mbresult == MessageBoxResult.Cancel || mbresult == MessageBoxResult.None)
                    {
                        ViewModels.LoggerViewModel.VerboseLogLn("Dumping aborted!");
                        return;
                    }
                }

                StartStopButton.Text = Constants.StopDumping;
                CopyProtectScanButton.Enabled = false;
                StatusLabel.Text = "Beginning dumping process";
                ViewModels.LoggerViewModel.VerboseLogLn("Starting dumping process..");

                var progress = new Progress<Result>();
                progress.ProgressChanged += ProgressUpdated;
                Result result = await _env.StartDumping(progress);

                StatusLabel.Text = result ? "Dumping complete!" : result.Message;
                ViewModels.LoggerViewModel.VerboseLogLn(result ? "Dumping complete!" : result.Message);
            }
            catch
            {
                // No-op, we don't care what it was
            }
            finally
            {
                StartStopButton.Text = Constants.StartDumping;
                CopyProtectScanButton.Enabled = true;
            }

            if (EjectWhenDoneCheckBox.Checked == true)
            {
                ViewModels.LoggerViewModel.VerboseLogLn($"Ejecting disc in drive {_env.Drive.Letter}");
                _env.EjectDisc();
            }
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
            StatusLabel.Text = result.Message;

            // Set the index for the current disc type
            SetCurrentDiscType();

            StartStopButton.Enabled = result && (_drives != null && _drives.Count > 0 ? true : false);

            // If we're in a type that doesn't support drive speeds
            DriveSpeedComboBox.Enabled = _env.Type.DoesSupportDriveSpeed();

            // If input params are not enabled, generate the full parameters from the environment
            if (!ParametersTextBox.Enabled)
            {
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

            OutputDirectoryTextBox.Text = Path.Combine(_options.DefaultOutputPath, drive?.VolumeLabel ?? string.Empty);
            OutputFilenameTextBox.Text = (drive?.VolumeLabel ?? "disc") + (mediaType.Extension() ?? ".bin");
        }

        /// <summary>
        /// Scan and show copy protection for the current disc
        /// </summary>
        private async void ScanAndShowProtection()
        {
            if (_env == null)
                _env = DetermineEnvironment();

            if (_env.Drive.Letter != default(char))
            {
                ViewModels.LoggerViewModel.VerboseLogLn("Scanning for copy protection in {0}", _env.Drive.Letter);

                var tempContent = StatusLabel.Text;
                StatusLabel.Text = "Scanning for copy protection... this might take a while!";
                StartStopButton.Enabled = false;
                DiscScanButton.Enabled = false;
                CopyProtectScanButton.Enabled = false;

                string protections = await Validators.RunProtectionScanOnPath(_env.Drive.Letter + ":\\");
                if (!ViewModels.LoggerViewModel.WindowVisible)
                    System.Windows.MessageBox.Show(protections, "Detected Protection", MessageBoxButton.OK, MessageBoxImage.Information);
                ViewModels.LoggerViewModel.VerboseLog("Detected the following protections in {0}:\r\n\r\n{1}", _env.Drive.Letter, protections);

                StatusLabel.Text = tempContent;
                StartStopButton.Enabled = true;
                DiscScanButton.Enabled = true;
                CopyProtectScanButton.Enabled = true;
            }
        }

        /// <summary>
        /// Set the drive speed based on reported maximum and user-defined option
        /// </summary>
        private void SetSupportedDriveSpeed()
        {
            // Set the drive speed list that's appropriate
            var values = Constants.GetSpeedsForMediaType(_currentMediaType);
            DriveSpeedComboBox.DataSource = values;
            ViewModels.LoggerViewModel.VerboseLogLn("Supported media speeds: {0}", string.Join(",", values));

            // Find the minimum set to compare against
            int preferred = 100;
            switch (_currentMediaType)
            {
                case MediaType.CDROM:
                case MediaType.GDROM:
                    preferred = _options.PreferredDumpSpeedCD;
                    break;
                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.NintendoGameCube:
                case MediaType.NintendoWiiOpticalDisc:
                    preferred = _options.PreferredDumpSpeedDVD;
                    break;
                case MediaType.BluRay:
                    preferred = _options.PreferredDumpSpeedBD;
                    break;
                default:
                    preferred = _options.PreferredDumpSpeedCD;
                    break;
            }

            // Choose the lower of the two speeds between the allowed speeds and the user-defined one
            // TODO: Inform more users about setting preferences in the settings so this comparison doesn't need to happen
            int chosenSpeed = Math.Min(
                values.Where(s => s <= values[values.Count / 2]).Last(),
                preferred
            );

            // Set the selected speed
            ViewModels.LoggerViewModel.VerboseLogLn("Setting drive speed to: {0}", chosenSpeed);
            DriveSpeedComboBox.SelectedIndex = values.ToList().IndexOf(chosenSpeed);
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
                ViewModels.LoggerViewModel.VerboseLogLn(_currentMediaType == null ? "unable to detect." : ("detected " + _currentMediaType.Name() + "."));
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
                StatusLabel.Text = $"Disc of type '{Converters.MediaTypeToString(_currentMediaType)}' found, but the current system does not support it!";
        }

        /// <summary>
        /// Process the current custom parameters back into UI values
        /// </summary>
        private void ProcessCustomParameters()
        {
            _env.DICParameters = new Parameters(ParametersTextBox.Text);

            int driveIndex = _drives.Select(d => d.Letter).ToList().IndexOf(_env.DICParameters.DriveLetter[0]);
            if (driveIndex > -1)
                DriveLetterComboBox.SelectedIndex = driveIndex;

            int driveSpeed = _env.DICParameters.DriveSpeed ?? -1;
            if (driveSpeed > 0)
                DriveSpeedComboBox.SelectedIndex = ((IReadOnlyList<int>)(DriveSpeedComboBox.DataSource)).ToList().IndexOf(driveSpeed);
            else
                _env.DICParameters.DriveSpeed = (int?)DriveSpeedComboBox.SelectedValue;

            string trimmedPath = _env.DICParameters.Filename?.Trim('"') ?? string.Empty;
            string outputDirectory = Path.GetDirectoryName(trimmedPath);
            string outputFilename = Path.GetFileName(trimmedPath);
            if (!String.IsNullOrWhiteSpace(outputDirectory))
                OutputDirectoryTextBox.Text = outputDirectory;
            else
                outputDirectory = OutputDirectoryTextBox.Text;
            if (!String.IsNullOrWhiteSpace(outputFilename))
                OutputFilenameTextBox.Text = outputFilename;
            else
                outputFilename = OutputFilenameTextBox.Text;

            MediaType? mediaType = Converters.BaseCommmandToMediaType(_env.DICParameters.Command);
            int mediaTypeIndex = _mediaTypes.IndexOf(mediaType);
            if (mediaTypeIndex > -1)
                MediaTypeComboBox.SelectedIndex = mediaTypeIndex;
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
