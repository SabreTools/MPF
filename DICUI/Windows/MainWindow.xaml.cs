using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;
using DICUI.Data;
using DICUI.Utilities;
using DICUI.Web;
using Newtonsoft.Json.Linq;

namespace DICUI.Windows
{
    public partial class MainWindow : Window
    {
        // Private UI-related variables
        private List<Drive> _drives;
        private MediaType? _currentMediaType;
        private List<KnownSystemComboBoxItem> _systems;
        private List<MediaType?> _mediaTypes;
        private bool _alreadyShown;

        private DumpEnvironment _env;

        // Option related
        private Options _options;
        private OptionsWindow _optionsWindow;

        // User input related
        private DiscInformationWindow _discInformationWindow;

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
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                double combinedHeight = this.Height + _logWindow.Height + Constants.LogWindowMarginFromMainWindow;
                Rectangle bounds = GetScaledCoordinates(WinForms.Screen.PrimaryScreen.WorkingArea);

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
            if ((string)StartStopButton.Content == Constants.StartDumping)
            {
                StartDumping();
            }
            else if ((string)StartStopButton.Content == Constants.StopDumping)
            {
                ViewModels.LoggerViewModel.VerboseLogLn("Canceling dumping process...");
                _env.CancelDumping();
                CopyProtectScanButton.IsEnabled = true;

                if (EjectWhenDoneCheckBox.IsChecked == true)
                {
                    ViewModels.LoggerViewModel.VerboseLogLn($"Ejecting disc in drive {_env.Drive.Letter}");
                    _env.EjectDisc();
                }

                if (_options.ResetDriveAfterDump)
                {
                    ViewModels.LoggerViewModel.VerboseLogLn($"Resetting drive {_env.Drive.Letter}");
                    _env.ResetDrive();
                }
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

            ViewModels.LoggerViewModel.VerboseLogLn("Changed system to: {0}", (SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem).Name);
            PopulateMediaType();
            GetOutputNames(false);
            EnsureDiscInformation();
        }

        private void MediaTypeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Only change the media type if the selection and not the list has changed
            if (e.RemovedItems.Count == 1 && e.AddedItems.Count == 1)
            {
                _currentMediaType = MediaTypeComboBox.SelectedItem as MediaType?;
                SetSupportedDriveSpeed();
            }

            GetOutputNames(false);
            EnsureDiscInformation();
        }

        private void DriveLetterComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CacheCurrentDiscType();
            SetCurrentDiscType();
            GetOutputNames(true);
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
            ViewModels.LoggerViewModel.VerboseLogLn(value.Message);
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

        // Toolbar Events

        private void AppExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void AboutClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"darksabre76 - Project Lead / Backend Design"
                + $"{Environment.NewLine}ReignStumble - Former Project Lead / UI Design"
                + $"{Environment.NewLine}Jakz - Primary Feature Contributor"
                + $"{Environment.NewLine}NHellFire - Feature Contributor", "About", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void CheckForUpdatesClick(object sender, RoutedEventArgs e)
        {
            // Get the current internal version
            var assemblyVersion = Assembly.GetEntryAssembly().GetName().Version;
            string version = $"{assemblyVersion.Major}.{assemblyVersion.Minor}" + (assemblyVersion.MajorRevision != 0 ? $".{assemblyVersion.MajorRevision}" : string.Empty);

            // Get the latest tag from GitHub
            using (var client = new CookieAwareWebClient())
            {
                client.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:64.0) Gecko/20100101 Firefox/64.0";

                // TODO: Figure out a better way than having this hardcoded...
                string url = "https://api.github.com/repos/SabreTools/DICUI/releases/latest";
                string latestReleaseJsonString = client.DownloadString(url);
                var latestReleaseJson = JObject.Parse(latestReleaseJsonString);
                string latestTag = latestReleaseJson["tag_name"].ToString();
                string releaseUrl = latestReleaseJson["html_url"].ToString();

                bool different = version != latestTag;

                string message = $"Local version: {version}"
                    + $"{Environment.NewLine}Remote version: {latestTag}"
                    + (different
                        ? $"{Environment.NewLine}The update URL has been added copied to your clipboard"
                        : $"{Environment.NewLine}You have the newest version!");
                
                // If we have a new version, put it in the clipboard
                if (different)
                    Clipboard.SetText(releaseUrl);

                MessageBox.Show(message, "Version Update Check", MessageBoxButton.OK, different ? MessageBoxImage.Exclamation : MessageBoxImage.Information);
            }
        }

        public void OnOptionsUpdated()
        {
            PopulateDrives();
            GetOutputNames(false);
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
                _mediaTypes = Validators.GetValidMediaTypes(currentSystem);
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

            _systems = new List<KnownSystemComboBoxItem>();
            _systems.Add(new KnownSystemComboBoxItem(KnownSystem.NONE));

            foreach (var group in mapping)
            {
                _systems.Add(new KnownSystemComboBoxItem(group.Key));
                group.Value.ForEach(system => _systems.Add(new KnownSystemComboBoxItem(system)));
            }

            SystemTypeComboBox.ItemsSource = _systems;
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
            _drives = Validators.CreateListOfDrives(_options.IgnoreFixedDrives);
            DriveLetterComboBox.ItemsSource = _drives;

            if (DriveLetterComboBox.Items.Count > 0)
            {
                // Check for active optical drives first
                int index = _drives.FindIndex(d => d.MarkedActive && d.InternalDriveType == InternalDriveType.Optical);

                // Then we check for floppy drives
                if (index == -1)
                    index = _drives.FindIndex(d => d.MarkedActive && d.InternalDriveType == InternalDriveType.Floppy);

                // Then we try all other drive types
                if (index == -1)
                    index = _drives.FindIndex(d => d.MarkedActive);

                // Set the selected index
                DriveLetterComboBox.SelectedIndex = (index != -1 ? index : 0);
                StatusLabel.Content = "Valid drive found! Choose your Media Type";
                                CopyProtectScanButton.IsEnabled = true;

                // Get the current media type
                if (!_options.SkipSystemDetection && index != -1)
                {
                    ViewModels.LoggerViewModel.VerboseLog("Trying to detect system for drive {0}.. ", _drives[index].Letter);
                    var currentSystem = Validators.GetKnownSystem(_drives[index]);
                    ViewModels.LoggerViewModel.VerboseLogLn(currentSystem == null || currentSystem == KnownSystem.NONE ? "unable to detect." : ("detected " + currentSystem.LongName() + "."));

                    if (currentSystem != null && currentSystem != KnownSystem.NONE)
                    {
                        int sysIndex = _systems.FindIndex(s => s == currentSystem);
                        SystemTypeComboBox.SelectedIndex = sysIndex;
                    }
                }

                // Only enable the start/stop if we don't have the default selected
                StartStopButton.IsEnabled = (SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem) != KnownSystem.NONE;

                ViewModels.LoggerViewModel.VerboseLogLn("Found {0} drives: {1}", _drives.Count, string.Join(", ", _drives.Select(d => d.Letter)));
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
            // Populate the new environment
            var env = new DumpEnvironment()
            {
                // Paths to tools
                ChefPath = _options.ChefPath,
                CreatorPath = _options.CreatorPath,
                SubdumpPath = _options.SubDumpPath,
                InternalProgram = _options.UseChef ? InternalProgram.DiscImageChef : InternalProgram.DiscImageCreator,

                OutputDirectory = OutputDirectoryTextBox.Text,
                OutputFilename = OutputFilenameTextBox.Text,

                // Get the currently selected options
                Drive = DriveLetterComboBox.SelectedItem as Drive,

                QuietMode = _options.QuietMode,
                ParanoidMode = _options.ParanoidMode,
                ScanForProtection = _options.ScanForProtection,
                RereadAmountC2 = _options.RereadAmountForC2,
                AddPlaceholders = _options.AddPlaceholders,
                PromptForDiscInformation = _options.PromptForDiscInformation,

                ChefParameters = new DiscImageChef.Parameters(ParametersTextBox.Text),
                CreatorParameters = new DiscImageCreator.Parameters(ParametersTextBox.Text),

                Username = _options.Username,
                Password = _options.Password,

                System = SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem,
                Type = MediaTypeComboBox.SelectedItem as MediaType?,
            };

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
            _env.FixOutputPaths();

            try
            {
                // Check for the firmware first for DiscImageCreator
                // TODO: Remove this (and method) once DIC end-to-end logging becomes a thing
                if (_env.InternalProgram != InternalProgram.DiscImageChef && !await _env.DriveHasLatestFimrware())
                {
                    MessageBox.Show($"DiscImageCreator has reported that drive {_env.Drive.Letter} is not updated to the most recent firmware. Please update the firmware for your drive and try again.", "Outdated Firmware", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Validate that the user explicitly wants an inactive drive to be considered for dumping
                if (!_env.Drive.MarkedActive)
                {
                    MessageBoxResult mbresult = MessageBox.Show("The currently selected drive does not appear to contain a disc! Are you sure you want to continue?", "Missing Disc", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    if (mbresult == MessageBoxResult.No || mbresult == MessageBoxResult.Cancel || mbresult == MessageBoxResult.None)
                    {
                        ViewModels.LoggerViewModel.VerboseLogLn("Dumping aborted!");
                        return;
                    }
                }

                // If a complete dump already exists
                if (_env.FoundAllFiles())
                {
                    MessageBoxResult mbresult = MessageBox.Show("A complete dump already exists! Are you sure you want to overwrite?", "Overwrite?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    if (mbresult == MessageBoxResult.No || mbresult == MessageBoxResult.Cancel || mbresult == MessageBoxResult.None)
                    {
                        ViewModels.LoggerViewModel.VerboseLogLn("Dumping aborted!");
                        return;
                    }
                }

                StartStopButton.Content = Constants.StopDumping;
                CopyProtectScanButton.IsEnabled = false;
                StatusLabel.Content = "Beginning dumping process";
                ViewModels.LoggerViewModel.VerboseLogLn("Starting dumping process..");

                var progress = new Progress<Result>();
                progress.ProgressChanged += ProgressUpdated;
                Result result = await _env.Run(progress);

                // If we didn't execute a dumping command we cannot get submission output
                bool isDumpingCommand = false;
                if (_env.InternalProgram == InternalProgram.DiscImageChef)
                    isDumpingCommand = _env.ChefParameters.IsDumpingCommand();
                else
                    isDumpingCommand = _env.CreatorParameters.IsDumpingCommand();

                if (!isDumpingCommand)
                {
                    ViewModels.LoggerViewModel.VerboseLogLn("No dumping command was run, submission information will not be gathered.");
                    StatusLabel.Content = "Execution complete!";
                    StartStopButton.Content = Constants.StartDumping;
                    CopyProtectScanButton.IsEnabled = true;
                    return;
                }

                if (result)
                {
                    // TODO: Remove Chef handling when irrelevant
                    if (_env.InternalProgram == InternalProgram.DiscImageChef)
                    {
                        ViewModels.LoggerViewModel.VerboseLogLn("DiscImageChef does not support split tracks or DiscImageCreator-compatible outputs");
                        ViewModels.LoggerViewModel.VerboseLogLn("No automatic submission information will be gathered for this disc");
                    }
                    else
                    {
                        // Verify dump output and save it
                        result = _env.VerifyAndSaveDumpOutput(progress,
                            EjectWhenDoneCheckBox.IsChecked,
                            _options.ResetDriveAfterDump,
                            (si) =>
                            {
                            // lazy initialization
                            if (_discInformationWindow == null)
                                {
                                    _discInformationWindow = new DiscInformationWindow(this, si);
                                    _discInformationWindow.Closed += delegate
                                    {
                                        _discInformationWindow = null;
                                    };
                                }

                                _discInformationWindow.Owner = this;
                                _discInformationWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                                _discInformationWindow.Refresh();
                                return _discInformationWindow.ShowDialog();
                            }
                        );
                    }
                }

                StatusLabel.Content = result ? "Dumping complete!" : result.Message;
                ViewModels.LoggerViewModel.VerboseLogLn(result ? "Dumping complete!" : result.Message);
            }
            catch
            {
                // No-op, we don't care what it was
            }
            finally
            {
                StartStopButton.Content = Constants.StartDumping;
                CopyProtectScanButton.IsEnabled = true;
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
            StatusLabel.Content = result.Message;

            // Set the index for the current disc type
            SetCurrentDiscType();

            StartStopButton.IsEnabled = result && (_drives != null && _drives.Count > 0 ? true : false);

            // If we're in a type that doesn't support drive speeds
            DriveSpeedComboBox.IsEnabled = _env.Type.DoesSupportDriveSpeed();

            // If input params are not enabled, generate the full parameters from the environment
            if (!ParametersTextBox.IsEnabled)
            {
                string generated = _env.GetFullParameters((int?)DriveSpeedComboBox.SelectedItem);
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
                OutputDirectoryTextBox.Text = Path.Combine(_options.DefaultOutputPath, drive?.VolumeLabel ?? string.Empty);

            // Get the extension for the file for the next two statements
            string extension = null;
            switch (_env.InternalProgram)
            {
                case InternalProgram.DiscImageChef:
                    extension = DiscImageChef.Converters.Extension(mediaType);
                    break;
                case InternalProgram.DiscImageCreator:
                    extension = DiscImageCreator.Converters.Extension(mediaType);
                    break;
            }

            // Set the output filename, if we changed drives or it's not already
            if (driveChanged || string.IsNullOrEmpty(OutputFilenameTextBox.Text))
                OutputFilenameTextBox.Text = (drive?.VolumeLabel ?? systemType.LongName()) + (extension ?? ".bin");

            // If the extension for the file changed, update that automatically
            else if (Path.GetExtension(OutputFilenameTextBox.Text) != extension)
                OutputFilenameTextBox.Text = Path.GetFileNameWithoutExtension(OutputFilenameTextBox.Text) + (extension ?? ".bin");
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

                var tempContent = StatusLabel.Content;
                StatusLabel.Content = "Scanning for copy protection... this might take a while!";
                StartStopButton.IsEnabled = false;
                DiskScanButton.IsEnabled = false;
                CopyProtectScanButton.IsEnabled = false;

                string protections = await Validators.RunProtectionScanOnPath(_env.Drive.Letter + ":\\");

                // If SmartE is detected on the current disc, remove `/sf` from the flags
                if (protections.Contains("SmartE"))
                {
                    _env.CreatorParameters[DiscImageCreator.Flag.ScanFileProtect] = false;
                    ViewModels.LoggerViewModel.VerboseLogLn($"SmartE detected, removing {DiscImageCreator.FlagStrings.ScanFileProtect} from parameters");
                }

                if (!ViewModels.LoggerViewModel.WindowVisible)
                    MessageBox.Show(protections, "Detected Protection", MessageBoxButton.OK, MessageBoxImage.Information);
                ViewModels.LoggerViewModel.VerboseLog("Detected the following protections in {0}:\r\n\r\n{1}", _env.Drive.Letter, protections);

                StatusLabel.Content = tempContent;
                StartStopButton.IsEnabled = true;
                DiskScanButton.IsEnabled = true;
                CopyProtectScanButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Set the drive speed based on reported maximum and user-defined option
        /// </summary>
        private void SetSupportedDriveSpeed()
        {
            // Set the drive speed list that's appropriate
            var values = Constants.GetSpeedsForMediaType(_currentMediaType);
            DriveSpeedComboBox.ItemsSource = values;
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
                case MediaType.NintendoGameCubeGameDisc:
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

            // Set the selected speed
            ViewModels.LoggerViewModel.VerboseLogLn("Setting drive speed to: {0}", preferred);
            DriveSpeedComboBox.SelectedValue = preferred;
        }

        /// <summary>
        /// Cache the current disc type to internal variable
        /// </summary>
        private void CacheCurrentDiscType()
        {
            // Get the drive letter from the selected item
            Drive drive = DriveLetterComboBox.SelectedItem as Drive;
            if (drive == null)
                return;

            // Get the current media type
            if (!_options.SkipMediaTypeDetection)
            {
                ViewModels.LoggerViewModel.VerboseLog("Trying to detect media type for drive {0}.. ", drive.Letter);
                _currentMediaType = Validators.GetMediaType(drive);
                ViewModels.LoggerViewModel.VerboseLogLn(_currentMediaType == null ? "unable to detect." : ("detected " + _currentMediaType.LongName() + "."));
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
                StatusLabel.Content = $"Disc of type '{Converters.LongName(_currentMediaType)}' found, but the current system does not support it!";
        }

        /// <summary>
        /// Process the current custom parameters back into UI values
        /// </summary>
        private void ProcessCustomParameters()
        {
            if (_options.UseChef)
            {
                _env.ChefParameters = new DiscImageChef.Parameters(ParametersTextBox.Text);

                if (_env.ChefParameters == null)
                    return;

                int driveIndex = _drives.Select(d => d.Letter).ToList().IndexOf(_env.ChefParameters.InputValue[0]);
                if (driveIndex > -1)
                    DriveLetterComboBox.SelectedIndex = driveIndex;

                string trimmedPath = _env.ChefParameters.OutputValue?.Trim('"') ?? string.Empty;
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
            }
            else
            {
                _env.CreatorParameters = new DiscImageCreator.Parameters(ParametersTextBox.Text);

                if (_env.CreatorParameters == null)
                    return;

                int driveIndex = _drives.Select(d => d.Letter).ToList().IndexOf(_env.CreatorParameters.DriveLetter[0]);
                if (driveIndex > -1)
                    DriveLetterComboBox.SelectedIndex = driveIndex;

                int driveSpeed = _env.CreatorParameters.DriveSpeed ?? -1;
                if (driveSpeed > 0)
                    DriveSpeedComboBox.SelectedValue = driveSpeed;
                else
                    _env.CreatorParameters.DriveSpeed = (int?)DriveSpeedComboBox.SelectedValue;

                string trimmedPath = _env.CreatorParameters.Filename?.Trim('"') ?? string.Empty;
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

                MediaType? mediaType = DiscImageCreator.Converters.ToMediaType(_env.CreatorParameters.Command);
                int mediaTypeIndex = _mediaTypes.IndexOf(mediaType);
                if (mediaTypeIndex > -1)
                    MediaTypeComboBox.SelectedIndex = mediaTypeIndex;
            }
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
