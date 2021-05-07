using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WinForms = System.Windows.Forms;
using BurnOutSharp;
using MPF.Data;
using MPF.Utilities;
using WPFCustomMessageBox;

namespace MPF.Windows
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
        public List<Element<MediaType>> MediaTypes { get; private set; } = new List<Element<MediaType>>();

        /// <summary>
        /// Current set of options
        /// </summary>
        public Options Options { get; private set; }

        /// <summary>
        /// Current list of supported system profiles
        /// </summary>
        public List<KnownSystemComboBoxItem> Systems { get; private set; } = KnownSystemComboBoxItem.GenerateElements().ToList();

        #endregion

        #region Private Instance Variables

        /// <summary>
        /// Determines if the window is already shown or not
        /// </summary>
        private bool alreadyShown;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Load the options
            LoadFromConfig();
            ViewModels.OptionsViewModel = new OptionsViewModel(Options);

            // Load the log output
            LogPanel.IsExpanded = Options.OpenLogWindowAtStartup;

            // Disable buttons until we load fully
            StartStopButton.IsEnabled = false;
            MediaScanButton.IsEnabled = false;
            CopyProtectScanButton.IsEnabled = false;

            // Set the UI color scheme according to the options
            if (Options.EnableDarkMode)
                EnableDarkMode();
            else
                DisableDarkMode();
        }

        #region Population

        /// <summary>
        /// Get a complete list of active disc drives and fill the combo box
        /// </summary>
        /// <remarks>TODO: Find a way for this to periodically run, or have it hook to a "drive change" event</remarks>
        private void PopulateDrives()
        {
            LogOutput.VerboseLogLn("Scanning for drives..");

            // Always enable the media scan
            MediaScanButton.IsEnabled = true;

            // Populate the list of drives and add it to the combo box
            Drives = Validators.CreateListOfDrives(Options.IgnoreFixedDrives);
            DriveLetterComboBox.ItemsSource = Drives;

            if (DriveLetterComboBox.Items.Count > 0)
            {
                LogOutput.VerboseLogLn($"Found {Drives.Count} drives: {string.Join(", ", Drives.Select(d => d.Letter))}");

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

                // Get the current system type
                if (index != -1)
                    DetermineSystemType();

                // Only enable the start/stop if we don't have the default selected
                StartStopButton.IsEnabled = ShouldEnableDumpingButton();
            }
            else
            {
                LogOutput.VerboseLogLn("Found no drives");
                DriveLetterComboBox.SelectedIndex = -1;
                StatusLabel.Content = "No valid drive found!";
                StartStopButton.IsEnabled = false;
                CopyProtectScanButton.IsEnabled = false;
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
                var mediaTypeValues = Validators.GetValidMediaTypes(currentSystem);
                MediaTypes = Element<MediaType>.GenerateElements().Where(m => mediaTypeValues.Contains(m.Value)).ToList();
                MediaTypeComboBox.ItemsSource = MediaTypes;

                MediaTypeComboBox.IsEnabled = MediaTypes.Count > 1;
                int currentIndex = MediaTypes.FindIndex(m => m == CurrentMediaType);
                MediaTypeComboBox.SelectedIndex = (currentIndex > -1 ? currentIndex : 0);
            }
            else
            {
                MediaTypeComboBox.IsEnabled = false;
                MediaTypeComboBox.ItemsSource = null;
                MediaTypeComboBox.SelectedIndex = -1;
            }
        }

        #endregion

        #region UI Element Changes

        /// <summary>
        /// Performs UI value setup end to end
        /// </summary>
        /// <param name="removeEventHandlers">Whether event handlers need to be removed first</param>
        /// <param name="rescanDrives">Whether drives should be rescanned or not</param>
        private void InitializeUIValues(bool removeEventHandlers, bool rescanDrives)
        {
            // Disable the dumping button
            StartStopButton.IsEnabled = false;

            // Set the UI color scheme according to the options
            if (Options.EnableDarkMode)
                EnableDarkMode();
            else
                DisableDarkMode();

            // Remove event handlers to ensure ordering
            if (removeEventHandlers)
                RemoveEventHandlers();

            // Populate the list of drives and determine the system
            if (rescanDrives)
            {
                StatusLabel.Content = "Creating drive list, please wait!";
                PopulateDrives();
            }
            else
            {
                DetermineSystemType();
            }

            // Determine current media type, if possible
            PopulateMediaType();
            CacheCurrentDiscType();
            SetCurrentDiscType();

            // Set the initial environment and UI values
            SetSupportedDriveSpeed();
            Env = DetermineEnvironment();
            GetOutputNames(true);
            EnsureDiscInformation();

            // Add event handlers
            AddEventHandlers();

            // Enable the dumping button, if necessary
            StartStopButton.IsEnabled = ShouldEnableDumpingButton();
        }

        /// <summary>
        /// Add all textbox and combobox event handlers
        /// </summary>
        private void AddEventHandlers()
        {
            SystemTypeComboBox.SelectionChanged += SystemTypeComboBoxSelectionChanged;
            MediaTypeComboBox.SelectionChanged += MediaTypeComboBoxSelectionChanged;
            DriveLetterComboBox.SelectionChanged += DriveLetterComboBoxSelectionChanged;
            DriveSpeedComboBox.SelectionChanged += DriveSpeedComboBoxSelectionChanged;
            AddPathEventHandlers();
        }

        /// <summary>
        /// Remove all textbox and combobox event handlers
        /// </summary>
        private void RemoveEventHandlers()
        {
            SystemTypeComboBox.SelectionChanged -= SystemTypeComboBoxSelectionChanged;
            MediaTypeComboBox.SelectionChanged -= MediaTypeComboBoxSelectionChanged;
            DriveLetterComboBox.SelectionChanged -= DriveLetterComboBoxSelectionChanged;
            DriveSpeedComboBox.SelectionChanged -= DriveSpeedComboBoxSelectionChanged;
            RemovePathEventHandlers();
        }

        /// <summary>
        /// Add path textbox event handlers
        /// </summary>
        private void AddPathEventHandlers()
        {
            OutputFilenameTextBox.TextChanged += OutputFilenameTextBoxTextChanged;
            OutputDirectoryTextBox.TextChanged += OutputDirectoryTextBoxTextChanged;
        }

        /// <summary>
        /// Remove path textbox event handlers
        /// </summary>
        private void RemovePathEventHandlers()
        {
            OutputFilenameTextBox.TextChanged -= OutputFilenameTextBoxTextChanged;
            OutputDirectoryTextBox.TextChanged -= OutputDirectoryTextBoxTextChanged;
        }

        /// <summary>
        /// Disable all UI elements during dumping
        /// </summary>
        private void DisableAllUIElements()
        {
            OptionsMenuItem.IsEnabled = false;
            SystemTypeComboBox.IsEnabled = false;
            MediaTypeComboBox.IsEnabled = false;
            OutputFilenameTextBox.IsEnabled = false;
            OutputDirectoryTextBox.IsEnabled = false;
            OutputDirectoryBrowseButton.IsEnabled = false;
            DriveLetterComboBox.IsEnabled = false;
            DriveSpeedComboBox.IsEnabled = false;
            EnableParametersCheckBox.IsEnabled = false;
            StartStopButton.Content = Interface.StopDumping;
            MediaScanButton.IsEnabled = false;
            CopyProtectScanButton.IsEnabled = false;
        }

        /// <summary>
        /// Enable all UI elements after dumping
        /// </summary>
        private void EnableAllUIElements()
        {
            OptionsMenuItem.IsEnabled = true;
            SystemTypeComboBox.IsEnabled = true;
            MediaTypeComboBox.IsEnabled = true;
            OutputFilenameTextBox.IsEnabled = true;
            OutputDirectoryTextBox.IsEnabled = true;
            OutputDirectoryBrowseButton.IsEnabled = true;
            DriveLetterComboBox.IsEnabled = true;
            DriveSpeedComboBox.IsEnabled = true;
            EnableParametersCheckBox.IsEnabled = true;
            StartStopButton.Content = Interface.StartDumping;
            MediaScanButton.IsEnabled = true;
            CopyProtectScanButton.IsEnabled = true;
        }

        /// <summary>
        /// Recolor all UI elements back to normal values
        /// </summary>
        private void DisableDarkMode()
        {
            // TODO: Do the opposite of `EnableDarkMode`

            // Handle application-wide resources
            Application.Current.Resources[SystemColors.ActiveBorderBrushKey] = null;
            Application.Current.Resources[SystemColors.ControlBrushKey] = null;
            Application.Current.Resources[SystemColors.ControlTextBrushKey] = null;
            Application.Current.Resources[SystemColors.GrayTextBrushKey] = null;
            Application.Current.Resources[SystemColors.WindowBrushKey] = null;
            Application.Current.Resources[SystemColors.WindowTextBrushKey] = null;

            // Handle Button-specific resources
            Application.Current.Resources["Button.Disabled.Background"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xF4, 0xF4, 0xF4));
            Application.Current.Resources["Button.MouseOver.Background"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xBE, 0xE6, 0xFD));
            Application.Current.Resources["Button.Pressed.Background"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xC4, 0xE5, 0xF6));
            Application.Current.Resources["Button.Static.Background"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD));

            // Handle ComboBox-specific resources
            Application.Current.Resources["ComboBox.Disabled.Background"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0));
            Application.Current.Resources["ComboBox.Disabled.Editable.Background"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            Application.Current.Resources["ComboBox.Disabled.Editable.Button.Background"] = Brushes.Transparent;
            Application.Current.Resources["ComboBox.MouseOver.Background"] = new LinearGradientBrush(
                Color.FromArgb(0xFF, 0xEC, 0xF4, 0xFC),
                Color.FromArgb(0xFF, 0xDC, 0xEC, 0xFC),
                new Point(0, 0),
                new Point(0, 1));
            Application.Current.Resources["ComboBox.MouseOver.Editable.Background"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            Application.Current.Resources["ComboBox.MouseOver.Editable.Button.Background"] = new LinearGradientBrush(
                Color.FromArgb(0xFF, 0xEB, 0xF4, 0xFC),
                Color.FromArgb(0xFF, 0xDC, 0xEC, 0xFC),
                new Point(0, 0),
                new Point(0, 1));
            Application.Current.Resources["ComboBox.Pressed.Background"] = new LinearGradientBrush(
                Color.FromArgb(0xFF, 0xDA, 0xEC, 0xFC),
                Color.FromArgb(0xFF, 0xC4, 0xE0, 0xFC),
                new Point(0, 0),
                new Point(0, 1));
            Application.Current.Resources["ComboBox.Pressed.Editable.Background"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            Application.Current.Resources["ComboBox.Pressed.Editable.Button.Background"] = new LinearGradientBrush(
                Color.FromArgb(0xFF, 0xDA, 0xEB, 0xFC),
                Color.FromArgb(0xFF, 0xC4, 0xE0, 0xFC),
                new Point(0, 0),
                new Point(0, 1));
            Application.Current.Resources["ComboBox.Static.Background"] = new LinearGradientBrush(
                Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0),
                Color.FromArgb(0xFF, 0xE5, 0xE5, 0xE5),
                new Point(0, 0),
                new Point(0, 1));
            Application.Current.Resources["ComboBox.Static.Editable.Background"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            Application.Current.Resources["ComboBox.Static.Editable.Button.Background"] = Brushes.Transparent;
            Application.Current.Resources["TextBox.Static.Background"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));

            // Handle CustomMessageBox-specific resources
            Application.Current.Resources["CustomMessageBox.Static.Background"] = null;

            // Handle MenuItem-specific resources
            Application.Current.Resources["MenuItem.SubMenu.Background"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0));
            Application.Current.Resources["MenuItem.SubMenu.Border"] = Brushes.DarkGray;

            // Handle ProgressBar-specific resources
            Application.Current.Resources["ProgressBar.Background"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xE6, 0xE6, 0xE6));

            // Handle ScrollViewer-specific resources
            Application.Current.Resources["ScrollViewer.ScrollBar.Background"] = Brushes.LightGray;

            // Handle TabItem-specific resources
            Application.Current.Resources["TabItem.Selected.Background"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            Application.Current.Resources["TabItem.Static.Background"] = new LinearGradientBrush(
                Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0),
                Color.FromArgb(0xFF, 0xE5, 0xE5, 0xE5),
                new Point(0, 0),
                new Point(0, 1));
            Application.Current.Resources["TabItem.Static.Border"] = Brushes.DarkGray;
        }

        /// <summary>
        /// Recolor all UI elements for dark mode
        /// </summary>
        private void EnableDarkMode()
        {
            // Setup needed brushes
            var darkModeBrush = new SolidColorBrush();
            darkModeBrush.Color = Color.FromArgb(0xff, 0x20, 0x20, 0x20);

            // Handle application-wide resources
            Application.Current.Resources[SystemColors.ActiveBorderBrushKey] = Brushes.Black;
            Application.Current.Resources[SystemColors.ControlBrushKey] = darkModeBrush;
            Application.Current.Resources[SystemColors.ControlTextBrushKey] = Brushes.White;
            Application.Current.Resources[SystemColors.GrayTextBrushKey] = Brushes.DarkGray;
            Application.Current.Resources[SystemColors.WindowBrushKey] = darkModeBrush;
            Application.Current.Resources[SystemColors.WindowTextBrushKey] = Brushes.White;

            // Handle Button-specific resources
            Application.Current.Resources["Button.Disabled.Background"] = darkModeBrush;
            Application.Current.Resources["Button.MouseOver.Background"] = darkModeBrush;
            Application.Current.Resources["Button.Pressed.Background"] = darkModeBrush;
            Application.Current.Resources["Button.Static.Background"] = darkModeBrush;

            // Handle ComboBox-specific resources
            Application.Current.Resources["ComboBox.Disabled.Background"] = darkModeBrush;
            Application.Current.Resources["ComboBox.Disabled.Editable.Background"] = darkModeBrush;
            Application.Current.Resources["ComboBox.Disabled.Editable.Button.Background"] = darkModeBrush;
            Application.Current.Resources["ComboBox.MouseOver.Background"] = darkModeBrush;
            Application.Current.Resources["ComboBox.MouseOver.Editable.Background"] = darkModeBrush;
            Application.Current.Resources["ComboBox.MouseOver.Editable.Button.Background"] = darkModeBrush;
            Application.Current.Resources["ComboBox.Pressed.Background"] = darkModeBrush;
            Application.Current.Resources["ComboBox.Pressed.Editable.Background"] = darkModeBrush;
            Application.Current.Resources["ComboBox.Pressed.Editable.Button.Background"] = darkModeBrush;
            Application.Current.Resources["ComboBox.Static.Background"] = darkModeBrush;
            Application.Current.Resources["ComboBox.Static.Editable.Background"] = darkModeBrush;
            Application.Current.Resources["ComboBox.Static.Editable.Button.Background"] = darkModeBrush;
            Application.Current.Resources["TextBox.Static.Background"] = darkModeBrush;

            // Handle CustomMessageBox-specific resources
            Application.Current.Resources["CustomMessageBox.Static.Background"] = darkModeBrush;

            // Handle MenuItem-specific resources
            Application.Current.Resources["MenuItem.SubMenu.Background"] = darkModeBrush;
            Application.Current.Resources["MenuItem.SubMenu.Border"] = Brushes.DarkGray;

            // Handle ProgressBar-specific resources
            Application.Current.Resources["ProgressBar.Background"] = darkModeBrush;

            // Handle ScrollViewer-specific resources
            Application.Current.Resources["ScrollViewer.ScrollBar.Background"] = darkModeBrush;

            // Handle TabItem-specific resources
            Application.Current.Resources["TabItem.Selected.Background"] = darkModeBrush;
            Application.Current.Resources["TabItem.Static.Background"] = darkModeBrush;
            Application.Current.Resources["TabItem.Static.Border"] = Brushes.DarkGray;
        }

        #endregion

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
            // If the selected item is invalid, we just skip
            if (!(DriveLetterComboBox.SelectedItem is Drive drive))
                return;

            // Get reasonable default values based on the current system
            KnownSystem? currentSystem = Systems[SystemTypeComboBox.SelectedIndex];
            MediaType? defaultMediaType = Validators.GetValidMediaTypes(currentSystem).FirstOrDefault() ?? MediaType.CDROM;
            if (defaultMediaType == MediaType.NONE)
                defaultMediaType = MediaType.CDROM;

            // If we're skipping detection, set the default value
            if (Options.SkipMediaTypeDetection)
            {
                LogOutput.VerboseLogLn($"Media type detection disabled, defaulting to {defaultMediaType.LongName()}.");
                CurrentMediaType = defaultMediaType;
            }

            // If the drive is marked active, try to read from it
            else if (drive.MarkedActive)
            {
                LogOutput.VerboseLog($"Trying to detect media type for drive {drive.Letter}.. ");
                (MediaType? detectedMediaType, string errorMessage) = Validators.GetMediaType(drive);

                // If we got an error message, post it to the log
                if (errorMessage != null)
                    LogOutput.VerboseLogLn($"Error in detecting media type: {errorMessage}");

                // If we got either an error or no media, default to the current System default
                if (detectedMediaType == null)
                {
                    LogOutput.VerboseLogLn($"Unable to detect, defaulting to {defaultMediaType.LongName()}.");
                    CurrentMediaType = defaultMediaType;
                }
                else
                {
                    LogOutput.VerboseLogLn($"Detected {CurrentMediaType.LongName()}.");
                    CurrentMediaType = detectedMediaType;
                }
            }

            // All other cases, just use the default
            else
            {
                LogOutput.VerboseLogLn($"Drive marked as empty, defaulting to {defaultMediaType.LongName()}.");
                CurrentMediaType = defaultMediaType;
            }
        }

        /// <summary>
        /// Create a DumpEnvironment with all current settings
        /// </summary>
        /// <returns>Filled DumpEnvironment instance</returns>
        private DumpEnvironment DetermineEnvironment()
        {
            // Populate the new environment
            var env = new DumpEnvironment(Options,
                OutputDirectoryTextBox.Text,
                OutputFilenameTextBox.Text,
                DriveLetterComboBox.SelectedItem as Drive,
                SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem,
                MediaTypeComboBox.SelectedItem as Element<MediaType>,
                ParametersTextBox.Text);

            // Disable automatic reprocessing of the textboxes until we're done
            RemovePathEventHandlers();

            // Save the current cursor positions
            int outputDirectorySelectionStart = OutputDirectoryTextBox.SelectionStart;
            int outputFilenameSelectionStart = OutputFilenameTextBox.SelectionStart;

            // Set the new text
            OutputDirectoryTextBox.Text = env.OutputDirectory;
            OutputFilenameTextBox.Text = env.OutputFilename;

            // Set the cursor position back to where it was
            OutputDirectoryTextBox.SelectionStart = outputDirectorySelectionStart;
            OutputDirectoryTextBox.SelectionLength = 0;
            OutputFilenameTextBox.SelectionStart = outputFilenameSelectionStart;
            OutputFilenameTextBox.SelectionLength = 0;

            // Re-enable automatic reprocessing of textboxes
            AddPathEventHandlers();

            return env;
        }

        /// <summary>
        /// Determine and set the current system type, if Allowed
        /// </summary>
        private void DetermineSystemType()
        {
            if (!Options.SkipSystemDetection && DriveLetterComboBox.SelectedIndex > -1)
            {
                LogOutput.VerboseLog($"Trying to detect system for drive {Drives[DriveLetterComboBox.SelectedIndex].Letter}.. ");
                var currentSystem = Validators.GetKnownSystem(Drives[DriveLetterComboBox.SelectedIndex], Options.DefaultSystem);
                LogOutput.VerboseLogLn(currentSystem == KnownSystem.NONE ? "unable to detect." : ("detected " + Converters.GetLongName(currentSystem) + "."));

                if (currentSystem != KnownSystem.NONE)
                {
                    int sysIndex = Systems.FindIndex(s => s == currentSystem);
                    SystemTypeComboBox.SelectedIndex = sysIndex;
                }
            }
        }

        /// <summary>
        /// Ensure information is consistent with the currently selected disc type
        /// </summary>
        private void EnsureDiscInformation()
        {
            // Get the current environment information
            Env = DetermineEnvironment();

            // Get the status to write out
            Result result = Validators.GetSupportStatus(Env.System, Env.Type);
            StatusLabel.Content = result.Message;

            // Set the index for the current disc type
            SetCurrentDiscType();

            // Enable or disable the button
            StartStopButton.IsEnabled = result && ShouldEnableDumpingButton();

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
            MediaType? mediaType = MediaTypeComboBox.SelectedItem as Element<MediaType>;

            // Get the extension for the file for the next two statements
            string extension = Env.Parameters?.GetDefaultExtension(mediaType);

            // Disable automatic reprocessing of the textboxes until we're done
            RemovePathEventHandlers();

            // Save the current cursor positions
            int outputDirectorySelectionStart = OutputDirectoryTextBox.SelectionStart;
            int outputFilenameSelectionStart = OutputFilenameTextBox.SelectionStart;

            // Set the output filename, if we changed drives or it's not already
            if (driveChanged || string.IsNullOrEmpty(OutputFilenameTextBox.Text))
                OutputFilenameTextBox.Text = (drive?.VolumeLabel ?? systemType.LongName()) + (extension ?? ".bin");

            // If the extension for the file changed, update that automatically
            else if (Path.GetExtension(OutputFilenameTextBox.Text) != extension)
                OutputFilenameTextBox.Text = Path.GetFileNameWithoutExtension(OutputFilenameTextBox.Text) + (extension ?? ".bin");

            // Set the output directory, if we changed drives or it's not already
            if (driveChanged || string.IsNullOrEmpty(OutputDirectoryTextBox.Text))
                OutputDirectoryTextBox.Text = Path.Combine(Options.DefaultOutputPath, Path.GetFileNameWithoutExtension(OutputFilenameTextBox.Text) ?? string.Empty);

            // Set the cursor position back to where it was
            OutputDirectoryTextBox.SelectionStart = outputDirectorySelectionStart;
            OutputDirectoryTextBox.SelectionLength = 0;
            OutputFilenameTextBox.SelectionStart = outputFilenameSelectionStart;
            OutputFilenameTextBox.SelectionLength = 0;

            // Re-enable automatic reprocessing of textboxes
            AddPathEventHandlers();
        }

        /// <summary>
        /// Load the current set of options from the application configuration
        /// </summary>
        private void LoadFromConfig()
        {
            Configuration configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var settings = configFile.AppSettings.Settings;
            var dict = new Dictionary<string, string>();

            foreach (string key in settings.AllKeys)
            {
                dict[key] = settings[key]?.Value ?? string.Empty;
            }

            Options = new Options(dict);
        }

        /// <summary>
        /// Process the current custom parameters back into UI values
        /// </summary>
        private void ProcessCustomParameters()
        {
            Env.SetParameters(ParametersTextBox.Text);
            if (Env.Parameters == null)
                return;

            int driveIndex = Drives.Select(d => d.Letter).ToList().IndexOf(Env.Parameters.InputPath[0]);
            if (driveIndex > -1)
                DriveLetterComboBox.SelectedIndex = driveIndex;

            int driveSpeed = Env.Parameters.Speed ?? -1;
            if (driveSpeed > 0)
                DriveSpeedComboBox.SelectedValue = driveSpeed;
            else
                Env.Parameters.Speed = DriveSpeedComboBox.SelectedValue as int?;

            // Disable automatic reprocessing of the textboxes until we're done
            RemovePathEventHandlers();

            // Save the current cursor positions
            int outputDirectorySelectionStart = OutputDirectoryTextBox.SelectionStart;
            int outputFilenameSelectionStart = OutputFilenameTextBox.SelectionStart;

            string trimmedPath = Env.Parameters.OutputPath?.Trim('"') ?? string.Empty;
            string outputDirectory = Path.GetDirectoryName(trimmedPath);
            string outputFilename = Path.GetFileName(trimmedPath);
            (outputDirectory, outputFilename) = DumpEnvironment.NormalizeOutputPaths(outputDirectory, outputFilename, Options.InternalProgram == InternalProgram.DiscImageCreator);
            if (!string.IsNullOrWhiteSpace(outputDirectory))
                OutputDirectoryTextBox.Text = outputDirectory;
            else
                outputDirectory = OutputDirectoryTextBox.Text;
            if (!string.IsNullOrWhiteSpace(outputFilename))
                OutputFilenameTextBox.Text = outputFilename;
            else
                outputFilename = OutputFilenameTextBox.Text;

            // Set the cursor position back to where it was
            OutputDirectoryTextBox.SelectionStart = outputDirectorySelectionStart;
            OutputDirectoryTextBox.SelectionLength = 0;
            OutputFilenameTextBox.SelectionStart = outputFilenameSelectionStart;
            OutputFilenameTextBox.SelectionLength = 0;

            // Re-enable automatic reprocessing of textboxes
            AddPathEventHandlers();

            MediaType? mediaType = Env.Parameters.GetMediaType();
            int mediaTypeIndex = MediaTypes.FindIndex(m => m == mediaType);
            if (mediaTypeIndex > -1)
                MediaTypeComboBox.SelectedIndex = mediaTypeIndex;
        }

        /// <summary>
        /// Save the current set of options to the application configuration
        /// </summary>
        private void SaveToConfig()
        {
            Configuration configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Loop through all settings in Options and save them, overwriting existing settings
            foreach (var kvp in Options)
            {
                configFile.AppSettings.Settings.Remove(kvp.Key);
                configFile.AppSettings.Settings.Add(kvp.Key, kvp.Value);
            }

            configFile.Save(ConfigurationSaveMode.Modified);
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
            var drive = DriveLetterComboBox.SelectedItem as Drive;
            if (drive.Letter != default(char))
            {
                LogOutput.VerboseLogLn($"Scanning for copy protection in {drive.Letter}");

                var tempContent = StatusLabel.Content;
                StatusLabel.Content = "Scanning for copy protection... this might take a while!";
                StartStopButton.IsEnabled = false;
                MediaScanButton.IsEnabled = false;
                CopyProtectScanButton.IsEnabled = false;

                var progress = new Progress<ProtectionProgress>();
                progress.ProgressChanged += ProgressUpdated;
                (bool success, string output) = await Validators.RunProtectionScanOnPath(drive.Letter + ":\\", this.Options, progress);

                // If SmartE is detected on the current disc, remove `/sf` from the flags for DIC only
                if (Env.Options.InternalProgram == InternalProgram.DiscImageCreator && output.Contains("SmartE"))
                {
                    ((DiscImageCreator.Parameters)Env.Parameters)[DiscImageCreator.FlagStrings.ScanFileProtect] = false;
                    LogOutput.VerboseLogLn($"SmartE detected, removing {DiscImageCreator.FlagStrings.ScanFileProtect} from parameters");
                }

                if (!LogPanel.IsExpanded)
                {
                    if (success)
                        CustomMessageBox.Show(output, "Detected Protection(s)", MessageBoxButton.OK, MessageBoxImage.Information);
                    else
                        CustomMessageBox.Show("An exception occurred, see the log for details", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                
                if (success)
                    LogOutput.LogLn($"Detected the following protections in {drive.Letter}:\r\n\r\n{output}");
                else
                    LogOutput.ErrorLogLn($"Path could not be scanned! Exception information:\r\n\r\n{output}");

                StatusLabel.Content = tempContent;
                StartStopButton.IsEnabled = ShouldEnableDumpingButton();
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
            LogOutput.VerboseLogLn($"Supported media speeds: {string.Join(", ", values)}");

            // Set the selected speed
            int speed;
            switch (CurrentMediaType)
            {
                case MediaType.CDROM:
                case MediaType.GDROM:
                    speed = Options.PreferredDumpSpeedCD;
                    break;
                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    speed = Options.PreferredDumpSpeedDVD;
                    break;
                case MediaType.BluRay:
                    speed = Options.PreferredDumpSpeedBD;
                    break;
                default:
                    speed = Options.PreferredDumpSpeedCD;
                    break;
            }

            LogOutput.VerboseLogLn($"Setting drive speed to: {speed}");
            DriveSpeedComboBox.SelectedValue = speed;
        }

        /// <summary>
        /// Show the disc information window
        /// </summary>
        /// <param name="submissionInfo">SubmissionInfo object to display and possibly change</param>
        /// <returns>Dialog open result</returns>
        private bool? ShowDiscInformationWindow(SubmissionInfo submissionInfo)
        {
            var discInformationWindow = new DiscInformationWindow(submissionInfo)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };
            return discInformationWindow.ShowDialog();
        }

        /// <summary>
        /// Determine if the dumping button should be enabled
        /// </summary>
        private bool ShouldEnableDumpingButton()
        {
            return SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem != KnownSystem.NONE
                && Drives != null
                && Drives.Count > 0
                && !string.IsNullOrEmpty(ParametersTextBox.Text);
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
                MessageBoxResult result = CustomMessageBox.Show("It looks like you have custom parameters that have not been saved. Would you like to apply those changes before starting to dump?", "Custom Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
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

            try
            {
                // Validate that the user explicitly wants an inactive drive to be considered for dumping
                if (!Env.Drive.MarkedActive)
                {
                    MessageBoxResult mbresult = CustomMessageBox.Show("The currently selected drive does not appear to contain a disc! Are you sure you want to continue?", "Missing Disc", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    if (mbresult == MessageBoxResult.No || mbresult == MessageBoxResult.Cancel || mbresult == MessageBoxResult.None)
                    {
                        LogOutput.LogLn("Dumping aborted!");
                        return;
                    }
                }

                // If a complete dump already exists
                (bool foundFiles, List<string> _) = Env.FoundAllFiles();
                if (foundFiles)
                {
                    MessageBoxResult mbresult = CustomMessageBox.Show("A complete dump already exists! Are you sure you want to overwrite?", "Overwrite?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    if (mbresult == MessageBoxResult.No || mbresult == MessageBoxResult.Cancel || mbresult == MessageBoxResult.None)
                    {
                        LogOutput.LogLn("Dumping aborted!");
                        return;
                    }
                }

                // Disable all UI elements apart from dumping button
                DisableAllUIElements();

                // Output to the label and log
                StatusLabel.Content = "Starting dumping process... Please wait!";
                LogOutput.LogLn("Starting dumping process... Please wait!");
                if (this.Options.ToolsInSeparateWindow)
                    LogOutput.LogLn("Look for the separate command window for more details");
                else
                    LogOutput.LogLn("Program outputs may be slow to populate in the log window");

                // Get progress indicators
                var resultProgress = new Progress<Result>();
                resultProgress.ProgressChanged += ProgressUpdated;
                var protectionProgress = new Progress<ProtectionProgress>();
                protectionProgress.ProgressChanged += ProgressUpdated;
                Env.ReportStatus += ProgressUpdated;

                // Run the program with the parameters
                Result result = await Env.Run(resultProgress);
                LogOutput.ResetProgressBar();

                // If we didn't execute a dumping command we cannot get submission output
                if (!Env.Parameters.IsDumpingCommand())
                {
                    LogOutput.LogLn("No dumping command was run, submission information will not be gathered.");
                    StatusLabel.Content = "Execution complete!";

                    // Reset all UI elements
                    EnableAllUIElements();
                    return;
                }

                // Verify dump output and save it
                if (result)
                {
                    result = await Env.VerifyAndSaveDumpOutput(resultProgress, protectionProgress, ShowDiscInformationWindow);
                }
                else
                {
                    LogOutput.ErrorLogLn(result.Message);
                    StatusLabel.Content = "Execution failed!";
                }
            }
            catch (Exception ex)
            {
                LogOutput.ErrorLogLn(ex.ToString());
                StatusLabel.Content = "An exception occurred!";
            }
            finally
            {
                // Reset all UI elements
                EnableAllUIElements();
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for AboutMenuItem Click event
        /// </summary>
        private void AboutClick(object sender, RoutedEventArgs e)
        {
            string aboutText = $"Media Presentation Frontend (MPF)"
                + $"{Environment.NewLine}"
                + $"{Environment.NewLine}A community preservation frontend developed in C#."
                + $"{Environment.NewLine}Supports DiscImageCreator, Aaru, and DD for Windows."
                + $"{Environment.NewLine}Originally created to help the Redump project."
                + $"{Environment.NewLine}"
                + $"{Environment.NewLine}Thanks to everyone who has supported this project!"
                + $"{Environment.NewLine}"
                + $"{Environment.NewLine}Version {Tools.GetCurrentVersion()}";

            LogOutput.SecretLogLn(aboutText);
            CustomMessageBox.Show(aboutText, "About", MessageBoxButton.OK, MessageBoxImage.Information);
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

            CustomMessageBox.Show(message, "Version Update Check", MessageBoxButton.OK, different ? MessageBoxImage.Exclamation : MessageBoxImage.Information);
        }

        /// <summary>
        /// Handler for CloseButton Click event
        /// </summary>
        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
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
            InitializeUIValues(removeEventHandlers: true, rescanDrives: false);
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
                var selectedMediaType = MediaTypeComboBox.SelectedItem as Element<MediaType>;
                CurrentMediaType = selectedMediaType.Value;
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
            InitializeUIValues(removeEventHandlers: true, rescanDrives: true);
        }

        /// <summary>
        /// Handler for MinimizeButton Click event
        /// </summary>
        private void MinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
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

            // Populate the list of media types for system
            StatusLabel.Content = "Creating media type list, please wait!";
            PopulateMediaType();

            // Initialize drives and UI values
            InitializeUIValues(removeEventHandlers: false, rescanDrives: true);
        }

        /// <summary>
        /// Handler for DiscInformationWindow OnUpdated event
        /// </summary>
        private void OnOptionsUpdated(object sender, EventArgs e)
        {
            var optionsWindow = sender as OptionsWindow;
            if (optionsWindow?.SavedSettings == true)
            {
                this.Options = optionsWindow.Options.Clone() as Options;
                SaveToConfig();
                InitializeUIValues(removeEventHandlers: true, rescanDrives: true);
            }
        }

        /// <summary>
        /// Handler for OptionsMenuItem Click event
        /// </summary>
        private void OptionsMenuItemClick(object sender, RoutedEventArgs e)
        {
            var optionsWindow = new OptionsWindow(Options) { Owner = this };
            optionsWindow.Closed += OnOptionsUpdated;
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
        private void ProgressUpdated(object sender, string value)
        {
            try
            {
                value = value ?? string.Empty;
                LogOutput.LogLn(value);
            }
            catch { }
        }

        /// <summary>
        /// Handler for Result ProgressChanged event
        /// </summary>
        private void ProgressUpdated(object sender, Result value)
        {
            string message = value?.Message;

            // Update the label with only the first line of output
            if (message.Contains("\n"))
                StatusLabel.Content = value.Message.Split('\n')[0] + " (See log output)";
            else
                StatusLabel.Content = value.Message;

            // Log based on success or failure
            if (value)
                LogOutput.VerboseLogLn(message);
            else
                LogOutput.ErrorLogLn(message);
        }

        /// <summary>
        /// Handler for ProtectionProgress ProgressChanged event
        /// </summary>
        private void ProgressUpdated(object sender, ProtectionProgress value)
        {
            string message = $"{value.Percentage * 100:N2}%: {value.Filename} - {value.Protection}";
            StatusLabel.Content = message;
            LogOutput.VerboseLogLn(message);
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
                LogOutput.VerboseLogLn("Canceling dumping process...");
                Env.CancelDumping();
                CopyProtectScanButton.IsEnabled = true;

                if (Env.Options.EjectAfterDump == true)
                {
                    LogOutput.VerboseLogLn($"Ejecting disc in drive {Env.Drive.Letter}");
                    Env.EjectDisc();
                }

                if (Options.DICResetDriveAfterDump)
                {
                    LogOutput.VerboseLogLn($"Resetting drive {Env.Drive.Letter}");
                    Env.ResetDrive();
                }
            }

            // Reset the progress bar
            LogOutput.ResetProgressBar();
        }

        /// <summary>
        /// Handler for SystemTypeComboBox SelectionChanged event
        /// </summary>
        private void SystemTypeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LogOutput.VerboseLogLn($"Changed system to: {(SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem).Name}");
            PopulateMediaType();
            GetOutputNames(false);
            EnsureDiscInformation();
        }

        /// <summary>
        /// Handler for Title MouseDown event
        /// </summary>
        private void TitleMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        #endregion
    }
}
