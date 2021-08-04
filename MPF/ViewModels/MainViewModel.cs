﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinForms = System.Windows.Forms;
using BurnOutSharp;
using MPF.Data;
using MPF.Utilities;
using MPF.Windows;
using WPFCustomMessageBox;

namespace MPF.GUI.ViewModels
{
    public class MainViewModel
    {
        #region Fields

        /// <summary>
        /// Currently selected or detected media type
        /// </summary>
        public MediaType? CurrentMediaType { get; set; }

        /// <summary>
        /// Current list of drives
        /// </summary>
        public List<Drive> Drives { get; set; }

        /// <summary>
        /// Current dumping environment
        /// </summary>
        public DumpEnvironment Env { get; set; }

        #endregion

        #region Lists

        /// <summary>
        /// Current list of supported media types
        /// </summary>
        public List<Element<MediaType>> MediaTypes { get; set; } = new List<Element<MediaType>>();

        /// <summary>
        /// Current list of supported system profiles
        /// </summary>
        public List<KnownSystemComboBoxItem> Systems { get; set; } = KnownSystemComboBoxItem.GenerateElements().ToList();

        #endregion

        #region Private Event Flags

        /// <summary>
        /// Indicates if SelectionChanged events can be executed
        /// </summary>
        private bool _canExecuteSelectionChanged = false;

        /// <summary>
        /// Indicates if TextChanged events can be executed
        /// </summary>
        private bool _canExecuteTextChanged = false;

        #endregion

        /// <summary>
        /// Initialize the main window after loading
        /// </summary>
        public void Init()
        {
            // Load the log output
            App.Instance.LogPanel.IsExpanded = App.Options.OpenLogWindowAtStartup;

            // Disable buttons until we load fully
            App.Instance.StartStopButton.IsEnabled = false;
            App.Instance.MediaScanButton.IsEnabled = false;
            App.Instance.CopyProtectScanButton.IsEnabled = false;

            // Add the click handlers to the UI
            AddEventHandlers();

            // Finish initializing the rest of the values
            InitializeUIValues(removeEventHandlers: false, rescanDrives: true);
        }

        #region Population

        /// <summary>
        /// Get a complete list of active disc drives and fill the combo box
        /// </summary>
        /// <remarks>TODO: Find a way for this to periodically run, or have it hook to a "drive change" event</remarks>
        private void PopulateDrives()
        {
            App.Logger.VerboseLogLn("Scanning for drives..");

            // Always enable the media scan
            App.Instance.MediaScanButton.IsEnabled = true;

            // Populate the list of drives and add it to the combo box
            Drives = Validators.CreateListOfDrives(App.Options.IgnoreFixedDrives);
            App.Instance.DriveLetterComboBox.ItemsSource = Drives;

            if (App.Instance.DriveLetterComboBox.Items.Count > 0)
            {
                App.Logger.VerboseLogLn($"Found {Drives.Count} drives: {string.Join(", ", Drives.Select(d => d.Letter))}");

                // Check for active optical drives first
                int index = Drives.FindIndex(d => d.MarkedActive && d.InternalDriveType == InternalDriveType.Optical);

                // Then we check for floppy drives
                if (index == -1)
                    index = Drives.FindIndex(d => d.MarkedActive && d.InternalDriveType == InternalDriveType.Floppy);

                // Then we try all other drive types
                if (index == -1)
                    index = Drives.FindIndex(d => d.MarkedActive);

                // Set the selected index
                App.Instance.DriveLetterComboBox.SelectedIndex = (index != -1 ? index : 0);
                App.Instance.StatusLabel.Content = "Valid drive found! Choose your Media Type";
                App.Instance.CopyProtectScanButton.IsEnabled = true;

                // Get the current system type
                if (index != -1)
                    DetermineSystemType();

                // Only enable the start/stop if we don't have the default selected
                App.Instance.StartStopButton.IsEnabled = ShouldEnableDumpingButton();
            }
            else
            {
                App.Logger.VerboseLogLn("Found no drives");
                App.Instance.DriveLetterComboBox.SelectedIndex = -1;
                App.Instance.StatusLabel.Content = "No valid drive found!";
                App.Instance.StartStopButton.IsEnabled = false;
                App.Instance.CopyProtectScanButton.IsEnabled = false;
            }

            // Ensure the UI gets updated
            App.Instance.UpdateLayout();
        }

        /// <summary>
        /// Populate media type according to system type
        /// </summary>
        public void PopulateMediaType()
        {
            KnownSystem? currentSystem = App.Instance.SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem;

            if (currentSystem != null)
            {
                var mediaTypeValues = Validators.GetValidMediaTypes(currentSystem);
                MediaTypes = Element<MediaType>.GenerateElements().Where(m => mediaTypeValues.Contains(m.Value)).ToList();
                App.Instance.MediaTypeComboBox.ItemsSource = MediaTypes;

                App.Instance.MediaTypeComboBox.IsEnabled = MediaTypes.Count > 1;
                int currentIndex = MediaTypes.FindIndex(m => m == CurrentMediaType);
                App.Instance.MediaTypeComboBox.SelectedIndex = (currentIndex > -1 ? currentIndex : 0);
            }
            else
            {
                App.Instance.MediaTypeComboBox.IsEnabled = false;
                App.Instance.MediaTypeComboBox.ItemsSource = null;
                App.Instance.MediaTypeComboBox.SelectedIndex = -1;
            }

            // Ensure the UI gets updated
            App.Instance.UpdateLayout();
        }

        #endregion

        #region UI Commands

        /// <summary>
        /// Change the currently selected media type
        /// </summary>
        public void ChangeMediaType(SelectionChangedEventArgs e)
        {
            // Only change the media type if the selection and not the list has changed
            if (e.RemovedItems.Count == 1 && e.AddedItems.Count == 1)
            {
                var selectedMediaType = App.Instance.MediaTypeComboBox.SelectedItem as Element<MediaType>;
                CurrentMediaType = selectedMediaType.Value;
                SetSupportedDriveSpeed();
            }

            GetOutputNames(false);
            EnsureDiscInformation();
        }

        /// <summary>
        /// Change the currently selected system
        /// </summary>
        public void ChangeSystem()
        {
            App.Logger.VerboseLogLn($"Changed system to: {(App.Instance.SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem).Name}");
            PopulateMediaType();
            GetOutputNames(false);
            EnsureDiscInformation();
        }

        /// <summary>
        /// Check for available updates
        /// </summary>
        public void CheckForUpdates()
        {
            (bool different, string message, string url) = Tools.CheckForNewVersion();

            // If we have a new version, put it in the clipboard
            if (different)
                Clipboard.SetText(url);

            App.Logger.SecretLogLn(message);
            CustomMessageBox.Show(App.Instance, message, "Version Update Check", MessageBoxButton.OK, different ? MessageBoxImage.Exclamation : MessageBoxImage.Information);
        }

        /// <summary>
        /// Shutdown the current application
        /// </summary>
        public void ExitApplication() => Application.Current.Shutdown();

        /// <summary>
        /// Set the output directory from a dialog box
        /// </summary>
        public void SetOutputDirectory()
        {
            BrowseFolder();
            EnsureDiscInformation();
        }

        /// <summary>
        /// Show the About text popup
        /// </summary>
        public void ShowAboutText()
        {
            string aboutText = $"Media Preservation Frontend (MPF)"
                + $"{Environment.NewLine}"
                + $"{Environment.NewLine}A community preservation frontend developed in C#."
                + $"{Environment.NewLine}Supports DiscImageCreator, Aaru, and DD for Windows."
                + $"{Environment.NewLine}Originally created to help the Redump project."
                + $"{Environment.NewLine}"
                + $"{Environment.NewLine}Thanks to everyone who has supported this project!"
                + $"{Environment.NewLine}"
                + $"{Environment.NewLine}Version {Tools.GetCurrentVersion()}";

            App.Logger.SecretLogLn(aboutText);
            CustomMessageBox.Show(App.Instance, aboutText, "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Show the Options window
        /// </summary>
        public void ShowOptionsWindow()
        {
            var optionsWindow = new OptionsWindow() { Owner = App.Instance };
            optionsWindow.Closed += OnOptionsUpdated;
            optionsWindow.Show();
        }

        /// <summary>
        /// Toggle the parameters input box
        /// </summary>
        public void ToggleParameters()
        {
            if (App.Instance.EnableParametersCheckBox.IsChecked == true)
            {
                App.Instance.ParametersTextBox.IsEnabled = true;
            }
            else
            {
                App.Instance.ParametersTextBox.IsEnabled = false;
                ProcessCustomParameters();
            }
        }

        /// <summary>
        /// Toggle the Start/Stop button
        /// </summary>
        public void ToggleStartStop()
        {
            // Dump or stop the dump
            if ((string)App.Instance.StartStopButton.Content == Interface.StartDumping)
            {
                StartDumping();
            }
            else if ((string)App.Instance.StartStopButton.Content == Interface.StopDumping)
            {
                App.Logger.VerboseLogLn("Canceling dumping process...");
                Env.CancelDumping();
                App.Instance.CopyProtectScanButton.IsEnabled = true;

                if (Env.Options.EjectAfterDump == true)
                {
                    App.Logger.VerboseLogLn($"Ejecting disc in drive {Env.Drive.Letter}");
                    Env.EjectDisc();
                }

                if (App.Options.DICResetDriveAfterDump)
                {
                    App.Logger.VerboseLogLn($"Resetting drive {Env.Drive.Letter}");
                    Env.ResetDrive();
                }
            }

            // Reset the progress bar
            App.Logger.ResetProgressBar();
        }

        /// <summary>
        /// Update the internal options from a closed OptionsWindow
        /// </summary>
        /// <param name="optionsWindow">OptionsWindow to copy back data from</param>
        public void UpdateOptions(OptionsWindow optionsWindow)
        {
            if (optionsWindow?.OptionsViewModel.SavedSettings == true)
            {
                App.Options = optionsWindow.OptionsViewModel.Options.Clone() as Options;
                InitializeUIValues(removeEventHandlers: true, rescanDrives: true);
            }
        }

        #endregion

        #region UI Functionality

        /// <summary>
        /// Performs UI value setup end to end
        /// </summary>
        /// <param name="removeEventHandlers">Whether event handlers need to be removed first</param>
        /// <param name="rescanDrives">Whether drives should be rescanned or not</param>
        public async void InitializeUIValues(bool removeEventHandlers, bool rescanDrives)
        {
            // Disable the dumping button
            App.Instance.StartStopButton.IsEnabled = false;

            // Set the UI color scheme according to the options
            if (App.Options.EnableDarkMode)
                EnableDarkMode();
            else
                DisableDarkMode();

            // Force the UI to reload after applying the theme
            App.Instance.UpdateLayout();

            // Remove event handlers to ensure ordering
            if (removeEventHandlers)
                DisableEventHandlers();

            // Populate the list of drives and determine the system
            if (rescanDrives)
            {
                App.Instance.StatusLabel.Content = "Creating drive list, please wait!";
                await App.Instance.Dispatcher.InvokeAsync(() => PopulateDrives());
            }
            else
            {
                await App.Instance.Dispatcher.InvokeAsync(() => DetermineSystemType());
            }

            // Determine current media type, if possible
            await App.Instance.Dispatcher.InvokeAsync(() => PopulateMediaType());
            CacheCurrentDiscType();
            SetCurrentDiscType();

            // Set the initial environment and UI values
            SetSupportedDriveSpeed();
            Env = DetermineEnvironment();
            GetOutputNames(true);
            EnsureDiscInformation();

            // Enable event handlers
            EnableEventHandlers();

            // Enable the dumping button, if necessary
            App.Instance.StartStopButton.IsEnabled = ShouldEnableDumpingButton();
        }

        /// <summary>
        /// Add all event handlers
        /// </summary>
        private void AddEventHandlers()
        {
            // Menu Bar Click
            App.Instance.AboutMenuItem.Click += AboutClick;
            App.Instance.AppExitMenuItem.Click += AppExitClick;
            App.Instance.CheckForUpdatesMenuItem.Click += CheckForUpdatesClick;
            App.Instance.OptionsMenuItem.Click += OptionsMenuItemClick;

            // User Area Click
            App.Instance.CopyProtectScanButton.Click += CopyProtectScanButtonClick;
            App.Instance.EnableParametersCheckBox.Click += EnableParametersCheckBoxClick;
            App.Instance.MediaScanButton.Click += MediaScanButtonClick;
            App.Instance.OutputDirectoryBrowseButton.Click += OutputDirectoryBrowseButtonClick;
            App.Instance.StartStopButton.Click += StartStopButtonClick;

            // User Area SelectionChanged
            App.Instance.SystemTypeComboBox.SelectionChanged += SystemTypeComboBoxSelectionChanged;
            App.Instance.MediaTypeComboBox.SelectionChanged += MediaTypeComboBoxSelectionChanged;
            App.Instance.DriveLetterComboBox.SelectionChanged += DriveLetterComboBoxSelectionChanged;
            App.Instance.DriveSpeedComboBox.SelectionChanged += DriveSpeedComboBoxSelectionChanged;

            // User Area TextChanged
            App.Instance.OutputFilenameTextBox.TextChanged += OutputFilenameTextBoxTextChanged;
            App.Instance.OutputDirectoryTextBox.TextChanged += OutputDirectoryTextBoxTextChanged;
        }

        /// <summary>
        /// Enable all textbox and combobox event handlers
        /// </summary>
        private void EnableEventHandlers()
        {
            _canExecuteSelectionChanged = true;
            EnablePathEventHandlers();
        }

        /// <summary>
        /// Disable all textbox and combobox event handlers
        /// </summary>
        private void DisableEventHandlers()
        {
            _canExecuteSelectionChanged = false;
            DisablePathEventHandlers();
        }

        /// <summary>
        /// Enable path textbox event handlers
        /// </summary>
        private void EnablePathEventHandlers()
        {
            _canExecuteTextChanged = true;
        }

        /// <summary>
        /// Disable path textbox event handlers
        /// </summary>
        private void DisablePathEventHandlers()
        {
            _canExecuteTextChanged = false;
        }

        /// <summary>
        /// Disable all UI elements during dumping
        /// </summary>
        private void DisableAllUIElements()
        {
            App.Instance.OptionsMenuItem.IsEnabled = false;
            App.Instance.SystemTypeComboBox.IsEnabled = false;
            App.Instance.MediaTypeComboBox.IsEnabled = false;
            App.Instance.OutputFilenameTextBox.IsEnabled = false;
            App.Instance.OutputDirectoryTextBox.IsEnabled = false;
            App.Instance.OutputDirectoryBrowseButton.IsEnabled = false;
            App.Instance.DriveLetterComboBox.IsEnabled = false;
            App.Instance.DriveSpeedComboBox.IsEnabled = false;
            App.Instance.EnableParametersCheckBox.IsEnabled = false;
            App.Instance.StartStopButton.Content = Interface.StopDumping;
            App.Instance.MediaScanButton.IsEnabled = false;
            App.Instance.CopyProtectScanButton.IsEnabled = false;
        }

        /// <summary>
        /// Enable all UI elements after dumping
        /// </summary>
        private void EnableAllUIElements()
        {
            App.Instance.OptionsMenuItem.IsEnabled = true;
            App.Instance.SystemTypeComboBox.IsEnabled = true;
            App.Instance.MediaTypeComboBox.IsEnabled = true;
            App.Instance.OutputFilenameTextBox.IsEnabled = true;
            App.Instance.OutputDirectoryTextBox.IsEnabled = true;
            App.Instance.OutputDirectoryBrowseButton.IsEnabled = true;
            App.Instance.DriveLetterComboBox.IsEnabled = true;
            App.Instance.DriveSpeedComboBox.IsEnabled = true;
            App.Instance.EnableParametersCheckBox.IsEnabled = true;
            App.Instance.StartStopButton.Content = Interface.StartDumping;
            App.Instance.MediaScanButton.IsEnabled = true;
            App.Instance.CopyProtectScanButton.IsEnabled = true;
        }

        /// <summary>
        /// Recolor all UI elements back to normal values
        /// </summary>
        private void DisableDarkMode()
        {
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
                App.Instance.OutputDirectoryTextBox.Text = folderDialog.SelectedPath;
            }
        }

        /// <summary>
        /// Cache the current disc type to internal variable
        /// </summary>
        private void CacheCurrentDiscType()
        {
            // If the selected item is invalid, we just skip
            if (!(App.Instance.DriveLetterComboBox.SelectedItem is Drive drive))
                return;

            // Get reasonable default values based on the current system
            KnownSystem? currentSystem = Systems[App.Instance.SystemTypeComboBox.SelectedIndex];
            MediaType? defaultMediaType = Validators.GetValidMediaTypes(currentSystem).FirstOrDefault() ?? MediaType.CDROM;
            if (defaultMediaType == MediaType.NONE)
                defaultMediaType = MediaType.CDROM;

            // If we're skipping detection, set the default value
            if (App.Options.SkipMediaTypeDetection)
            {
                App.Logger.VerboseLogLn($"Media type detection disabled, defaulting to {defaultMediaType.LongName()}.");
                CurrentMediaType = defaultMediaType;
            }

            // If the drive is marked active, try to read from it
            else if (drive.MarkedActive)
            {
                App.Logger.VerboseLog($"Trying to detect media type for drive {drive.Letter}.. ");
                (MediaType? detectedMediaType, string errorMessage) = Validators.GetMediaType(drive);

                // If we got an error message, post it to the log
                if (errorMessage != null)
                    App.Logger.VerboseLogLn($"Error in detecting media type: {errorMessage}");

                // If we got either an error or no media, default to the current System default
                if (detectedMediaType == null)
                {
                    App.Logger.VerboseLogLn($"Unable to detect, defaulting to {defaultMediaType.LongName()}.");
                    CurrentMediaType = defaultMediaType;
                }
                else
                {
                    App.Logger.VerboseLogLn($"Detected {CurrentMediaType.LongName()}.");
                    CurrentMediaType = detectedMediaType;
                }
            }

            // All other cases, just use the default
            else
            {
                App.Logger.VerboseLogLn($"Drive marked as empty, defaulting to {defaultMediaType.LongName()}.");
                CurrentMediaType = defaultMediaType;
            }

            // Ensure the UI gets updated
            App.Instance.UpdateLayout();
        }

        /// <summary>
        /// Create a DumpEnvironment with all current settings
        /// </summary>
        /// <returns>Filled DumpEnvironment instance</returns>
        private DumpEnvironment DetermineEnvironment()
        {
            // Populate the new environment
            var env = new DumpEnvironment(App.Options,
                App.Instance.OutputDirectoryTextBox.Text,
                App.Instance.OutputFilenameTextBox.Text,
                App.Instance.DriveLetterComboBox.SelectedItem as Drive,
                App.Instance.SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem,
                App.Instance.MediaTypeComboBox.SelectedItem as Element<MediaType>,
                App.Instance.ParametersTextBox.Text);

            // Disable automatic reprocessing of the textboxes until we're done
            DisablePathEventHandlers();

            // Save the current cursor positions
            int outputDirectorySelectionStart = App.Instance.OutputDirectoryTextBox.SelectionStart;
            int outputFilenameSelectionStart = App.Instance.OutputFilenameTextBox.SelectionStart;

            // Set the new text
            App.Instance.OutputDirectoryTextBox.Text = env.OutputDirectory;
            App.Instance.OutputFilenameTextBox.Text = env.OutputFilename;

            // Set the cursor position back to where it was
            App.Instance.OutputDirectoryTextBox.SelectionStart = outputDirectorySelectionStart;
            App.Instance.OutputDirectoryTextBox.SelectionLength = 0;
            App.Instance.OutputFilenameTextBox.SelectionStart = outputFilenameSelectionStart;
            App.Instance.OutputFilenameTextBox.SelectionLength = 0;

            // Re-enable automatic reprocessing of textboxes
            EnablePathEventHandlers();

            // Ensure the UI gets updated
            App.Instance.UpdateLayout();

            return env;
        }

        /// <summary>
        /// Determine and set the current system type, if Allowed
        /// </summary>
        private void DetermineSystemType()
        {
            if (!App.Options.SkipSystemDetection && App.Instance.DriveLetterComboBox.SelectedIndex > -1)
            {
                App.Logger.VerboseLog($"Trying to detect system for drive {Drives[App.Instance.DriveLetterComboBox.SelectedIndex].Letter}.. ");
                var currentSystem = Validators.GetKnownSystem(Drives[App.Instance.DriveLetterComboBox.SelectedIndex], App.Options.DefaultSystem);
                App.Logger.VerboseLogLn(currentSystem == KnownSystem.NONE ? "unable to detect." : ("detected " + Converters.GetLongName(currentSystem) + "."));

                if (currentSystem != KnownSystem.NONE)
                {
                    int sysIndex = Systems.FindIndex(s => s == currentSystem);
                    App.Instance.SystemTypeComboBox.SelectedIndex = sysIndex;
                }
            }

            // Ensure the UI gets updated
            App.Instance.UpdateLayout();
        }

        /// <summary>
        /// Ensure information is consistent with the currently selected disc type
        /// </summary>
        public void EnsureDiscInformation()
        {
            // Get the current environment information
            Env = DetermineEnvironment();

            // Get the status to write out
            Result result = Validators.GetSupportStatus(Env.System, Env.Type);
            App.Instance.StatusLabel.Content = result.Message;

            // Set the index for the current disc type
            SetCurrentDiscType();

            // Enable or disable the button
            App.Instance.StartStopButton.IsEnabled = result && ShouldEnableDumpingButton();

            // If we're in a type that doesn't support drive speeds
            App.Instance.DriveSpeedComboBox.IsEnabled = Env.Type.DoesSupportDriveSpeed();

            // If input params are not enabled, generate the full parameters from the environment
            if (!App.Instance.ParametersTextBox.IsEnabled)
            {
                string generated = Env.GetFullParameters((int?)App.Instance.DriveSpeedComboBox.SelectedItem);
                if (generated != null)
                    App.Instance.ParametersTextBox.Text = generated;
            }

            // Ensure the UI gets updated
            App.Instance.UpdateLayout();
        }

        /// <summary>
        /// Get the default output directory name from the currently selected drive
        /// </summary>
        /// <param name="driveChanged">Force an updated name if the drive letter changes</param>
        public void GetOutputNames(bool driveChanged)
        {
            Drive drive = App.Instance.DriveLetterComboBox.SelectedItem as Drive;
            KnownSystem? systemType = App.Instance.SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem;
            MediaType? mediaType = App.Instance.MediaTypeComboBox.SelectedItem as Element<MediaType>;

            // Get the extension for the file for the next two statements
            string extension = Env.Parameters?.GetDefaultExtension(mediaType);

            // Disable automatic reprocessing of the textboxes until we're done
            DisablePathEventHandlers();

            // Save the current cursor positions
            int outputDirectorySelectionStart = App.Instance.OutputDirectoryTextBox.SelectionStart;
            int outputFilenameSelectionStart = App.Instance.OutputFilenameTextBox.SelectionStart;

            // Set the output filename, if we changed drives or it's not already
            if (driveChanged || string.IsNullOrEmpty(App.Instance.OutputFilenameTextBox.Text))
                App.Instance.OutputFilenameTextBox.Text = (drive?.VolumeLabel ?? systemType.LongName()) + (extension ?? ".bin");

            // If the extension for the file changed, update that automatically
            else if (Path.GetExtension(App.Instance.OutputFilenameTextBox.Text) != extension)
                App.Instance.OutputFilenameTextBox.Text = Path.GetFileNameWithoutExtension(App.Instance.OutputFilenameTextBox.Text) + (extension ?? ".bin");

            // Set the output directory, if we changed drives or it's not already
            if (driveChanged || string.IsNullOrEmpty(App.Instance.OutputDirectoryTextBox.Text))
                App.Instance.OutputDirectoryTextBox.Text = Path.Combine(App.Options.DefaultOutputPath, Path.GetFileNameWithoutExtension(App.Instance.OutputFilenameTextBox.Text) ?? string.Empty);

            // Set the cursor position back to where it was
            App.Instance.OutputDirectoryTextBox.SelectionStart = outputDirectorySelectionStart;
            App.Instance.OutputDirectoryTextBox.SelectionLength = 0;
            App.Instance.OutputFilenameTextBox.SelectionStart = outputFilenameSelectionStart;
            App.Instance.OutputFilenameTextBox.SelectionLength = 0;

            // Re-enable automatic reprocessing of textboxes
            EnablePathEventHandlers();

            // Ensure the UI gets updated
            App.Instance.UpdateLayout();
        }

        /// <summary>
        /// Process the current custom parameters back into UI values
        /// </summary>
        public void ProcessCustomParameters()
        {
            Env.SetParameters(App.Instance.ParametersTextBox.Text);
            if (Env.Parameters == null)
                return;

            int driveIndex = Drives.Select(d => d.Letter).ToList().IndexOf(Env.Parameters.InputPath[0]);
            if (driveIndex > -1)
                App.Instance.DriveLetterComboBox.SelectedIndex = driveIndex;

            int driveSpeed = Env.Parameters.Speed ?? -1;
            if (driveSpeed > 0)
                App.Instance.DriveSpeedComboBox.SelectedValue = driveSpeed;
            else
                Env.Parameters.Speed = App.Instance.DriveSpeedComboBox.SelectedValue as int?;

            // Disable automatic reprocessing of the textboxes until we're done
            DisablePathEventHandlers();

            // Save the current cursor positions
            int outputDirectorySelectionStart = App.Instance.OutputDirectoryTextBox.SelectionStart;
            int outputFilenameSelectionStart = App.Instance.OutputFilenameTextBox.SelectionStart;

            string trimmedPath = Env.Parameters.OutputPath?.Trim('"') ?? string.Empty;
            string outputDirectory = Path.GetDirectoryName(trimmedPath);
            string outputFilename = Path.GetFileName(trimmedPath);
            (outputDirectory, outputFilename) = DumpEnvironment.NormalizeOutputPaths(outputDirectory, outputFilename, App.Options.InternalProgram == InternalProgram.DiscImageCreator);
            if (!string.IsNullOrWhiteSpace(outputDirectory))
                App.Instance.OutputDirectoryTextBox.Text = outputDirectory;
            else
                outputDirectory = App.Instance.OutputDirectoryTextBox.Text;
            if (!string.IsNullOrWhiteSpace(outputFilename))
                App.Instance.OutputFilenameTextBox.Text = outputFilename;
            else
                outputFilename = App.Instance.OutputFilenameTextBox.Text;

            // Set the cursor position back to where it was
            App.Instance.OutputDirectoryTextBox.SelectionStart = outputDirectorySelectionStart;
            App.Instance.OutputDirectoryTextBox.SelectionLength = 0;
            App.Instance.OutputFilenameTextBox.SelectionStart = outputFilenameSelectionStart;
            App.Instance.OutputFilenameTextBox.SelectionLength = 0;

            // Re-enable automatic reprocessing of textboxes
            EnablePathEventHandlers();

            MediaType? mediaType = Env.Parameters.GetMediaType();
            int mediaTypeIndex = MediaTypes.FindIndex(m => m == mediaType);
            if (mediaTypeIndex > -1)
                App.Instance.MediaTypeComboBox.SelectedIndex = mediaTypeIndex;
        }

        /// <summary>
        /// Scan and show copy protection for the current disc
        /// </summary>
        public async void ScanAndShowProtection()
        {
            // Determine current environment, just in case
            if (Env == null)
                Env = DetermineEnvironment();

            // Pull the drive letter from the UI directly, just in case
            var drive = App.Instance.DriveLetterComboBox.SelectedItem as Drive;
            if (drive.Letter != default(char))
            {
                App.Logger.VerboseLogLn($"Scanning for copy protection in {drive.Letter}");

                var tempContent = App.Instance.StatusLabel.Content;
                App.Instance.StatusLabel.Content = "Scanning for copy protection... this might take a while!";
                App.Instance.StartStopButton.IsEnabled = false;
                App.Instance.MediaScanButton.IsEnabled = false;
                App.Instance.CopyProtectScanButton.IsEnabled = false;

                var progress = new Progress<ProtectionProgress>();
                progress.ProgressChanged += ProgressUpdated;
                (bool success, string output) = await Validators.RunProtectionScanOnPath(drive.Letter + ":\\", App.Options, progress);

                // If SmartE is detected on the current disc, remove `/sf` from the flags for DIC only
                if (Env.Options.InternalProgram == InternalProgram.DiscImageCreator && output.Contains("SmartE"))
                {
                    ((DiscImageCreator.Parameters)Env.Parameters)[DiscImageCreator.FlagStrings.ScanFileProtect] = false;
                    App.Logger.VerboseLogLn($"SmartE detected, removing {DiscImageCreator.FlagStrings.ScanFileProtect} from parameters");
                }

                if (!App.Instance.LogPanel.IsExpanded)
                {
                    if (success)
                        CustomMessageBox.Show(output, "Detected Protection(s)", MessageBoxButton.OK, MessageBoxImage.Information);
                    else
                        CustomMessageBox.Show("An exception occurred, see the log for details", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (success)
                    App.Logger.LogLn($"Detected the following protections in {drive.Letter}:\r\n\r\n{output}");
                else
                    App.Logger.ErrorLogLn($"Path could not be scanned! Exception information:\r\n\r\n{output}");

                App.Instance.StatusLabel.Content = tempContent;
                App.Instance.StartStopButton.IsEnabled = ShouldEnableDumpingButton();
                App.Instance.MediaScanButton.IsEnabled = true;
                App.Instance.CopyProtectScanButton.IsEnabled = true;
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
                App.Instance.MediaTypeComboBox.SelectedIndex = index;
            else
                App.Instance.StatusLabel.Content = $"Disc of type '{Converters.LongName(CurrentMediaType)}' found, but the current system does not support it!";

            // Ensure the UI gets updated
            App.Instance.UpdateLayout();
        }

        /// <summary>
        /// Set the drive speed based on reported maximum and user-defined option
        /// </summary>
        public void SetSupportedDriveSpeed()
        {
            // Set the drive speed list that's appropriate
            var values = Interface.GetSpeedsForMediaType(CurrentMediaType);
            App.Instance.DriveSpeedComboBox.ItemsSource = values;
            App.Logger.VerboseLogLn($"Supported media speeds: {string.Join(", ", values)}");

            // Set the selected speed
            int speed;
            switch (CurrentMediaType)
            {
                case MediaType.CDROM:
                case MediaType.GDROM:
                    speed = App.Options.PreferredDumpSpeedCD;
                    break;
                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    speed = App.Options.PreferredDumpSpeedDVD;
                    break;
                case MediaType.BluRay:
                    speed = App.Options.PreferredDumpSpeedBD;
                    break;
                default:
                    speed = App.Options.PreferredDumpSpeedCD;
                    break;
            }

            App.Logger.VerboseLogLn($"Setting drive speed to: {speed}");
            App.Instance.DriveSpeedComboBox.SelectedValue = speed;

            // Ensure the UI gets updated
            App.Instance.UpdateLayout();
        }

        /// <summary>
        /// Determine if the dumping button should be enabled
        /// </summary>
        private bool ShouldEnableDumpingButton()
        {
            return App.Instance.SystemTypeComboBox.SelectedItem as KnownSystemComboBoxItem != KnownSystem.NONE
                && Drives != null
                && Drives.Count > 0
                && !string.IsNullOrEmpty(App.Instance.ParametersTextBox.Text);
        }

        /// <summary>
        /// Show the disc information window
        /// </summary>
        /// <param name="submissionInfo">SubmissionInfo object to display and possibly change</param>
        /// <returns>Dialog open result</returns>
        private (bool?, SubmissionInfo) ShowDiscInformationWindow(SubmissionInfo submissionInfo)
        {
            if (App.Options.ShowDiscEjectReminder)
                CustomMessageBox.Show(App.Instance, "It is now safe to eject the disc", "Eject", MessageBoxButton.OK, MessageBoxImage.Information);

            var discInformationWindow = new DiscInformationWindow(submissionInfo)
            {
                Owner = App.Instance,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };
            bool? result = discInformationWindow.ShowDialog();

            // Copy back the submission info changes, if necessary
            if (result == true)
                submissionInfo = discInformationWindow.DiscInformationViewModel.SubmissionInfo.Clone() as SubmissionInfo;

            return (result, submissionInfo);
        }

        /// <summary>
        /// Begin the dumping process using the given inputs
        /// </summary>
        public async void StartDumping()
        {
            // One last check to determine environment, just in case
            Env = DetermineEnvironment();

            // If still in custom parameter mode, check that users meant to continue or not
            if (App.Instance.EnableParametersCheckBox.IsChecked == true)
            {
                MessageBoxResult result = CustomMessageBox.Show("It looks like you have custom parameters that have not been saved. Would you like to apply those changes before starting to dump?", "Custom Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    App.Instance.EnableParametersCheckBox.IsChecked = false;
                    App.Instance.ParametersTextBox.IsEnabled = false;
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
                        App.Logger.LogLn("Dumping aborted!");
                        return;
                    }
                }

                // If a complete dump already exists
                (bool foundFiles, List<string> _) = Env.FoundAllFiles(true);
                if (foundFiles)
                {
                    MessageBoxResult mbresult = CustomMessageBox.Show("A complete dump already exists! Are you sure you want to overwrite?", "Overwrite?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    if (mbresult == MessageBoxResult.No || mbresult == MessageBoxResult.Cancel || mbresult == MessageBoxResult.None)
                    {
                        App.Logger.LogLn("Dumping aborted!");
                        return;
                    }
                }

                // Validate that at least some space exists
                // TODO: Tie this to the size of the disc, type of disc, etc.
                string fullPath = Path.GetFullPath(Env.OutputDirectory);
                var driveInfo = new DriveInfo(Path.GetPathRoot(fullPath));
                if (driveInfo.AvailableFreeSpace < Math.Pow(2, 30))
                {
                    MessageBoxResult mbresult = CustomMessageBox.Show("There is less than 1gb of space left on the target drive. Are you sure you want to continue?", "Low Space", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    if (mbresult == MessageBoxResult.No || mbresult == MessageBoxResult.Cancel || mbresult == MessageBoxResult.None)
                    {
                        App.Logger.LogLn("Dumping aborted!");
                        return;
                    }
                }

                // Disable all UI elements apart from dumping button
                DisableAllUIElements();

                // Output to the label and log
                App.Instance.StatusLabel.Content = "Starting dumping process... Please wait!";
                App.Logger.LogLn("Starting dumping process... Please wait!");
                if (App.Options.ToolsInSeparateWindow)
                    App.Logger.LogLn("Look for the separate command window for more details");
                else
                    App.Logger.LogLn("Program outputs may be slow to populate in the log window");

                // Get progress indicators
                var resultProgress = new Progress<Result>();
                resultProgress.ProgressChanged += ProgressUpdated;
                var protectionProgress = new Progress<ProtectionProgress>();
                protectionProgress.ProgressChanged += ProgressUpdated;
                Env.ReportStatus += ProgressUpdated;

                // Run the program with the parameters
                Result result = await Env.Run(resultProgress);
                App.Logger.ResetProgressBar();

                // If we didn't execute a dumping command we cannot get submission output
                if (!Env.Parameters.IsDumpingCommand())
                {
                    App.Logger.LogLn("No dumping command was run, submission information will not be gathered.");
                    App.Instance.StatusLabel.Content = "Execution complete!";

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
                    App.Logger.ErrorLogLn(result.Message);
                    App.Instance.StatusLabel.Content = "Execution failed!";
                }
            }
            catch (Exception ex)
            {
                App.Logger.ErrorLogLn(ex.ToString());
                App.Instance.StatusLabel.Content = "An exception occurred!";
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
        /// Handler for OptionsWindow OnUpdated event
        /// </summary>
        private void OnOptionsUpdated(object sender, EventArgs e) =>
            UpdateOptions(sender as OptionsWindow);

        #region Progress Reporting

        /// <summary>
        /// Handler for Result ProgressChanged event
        /// </summary>
        private void ProgressUpdated(object sender, string value)
        {
            try
            {
                value = value ?? string.Empty;
                App.Logger.LogLn(value);
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
                App.Instance.StatusLabel.Content = value.Message.Split('\n')[0] + " (See log output)";
            else
                App.Instance.StatusLabel.Content = value.Message;

            // Log based on success or failure
            if (value)
                App.Logger.VerboseLogLn(message);
            else
                App.Logger.ErrorLogLn(message);
        }

        /// <summary>
        /// Handler for ProtectionProgress ProgressChanged event
        /// </summary>
        private void ProgressUpdated(object sender, ProtectionProgress value)
        {
            string message = $"{value.Percentage * 100:N2}%: {value.Filename} - {value.Protection}";
            App.Instance.StatusLabel.Content = message;
            App.Logger.VerboseLogLn(message);
        }

        #endregion

        #region Menu Bar

        /// <summary>
        /// Handler for AboutMenuItem Click event
        /// </summary>
        private void AboutClick(object sender, RoutedEventArgs e) =>
            ShowAboutText();

        /// <summary>
        /// Handler for AppExitMenuItem Click event
        /// </summary>
        private void AppExitClick(object sender, RoutedEventArgs e) =>
            ExitApplication();

        /// <summary>
        /// Handler for CheckForUpdatesMenuItem Click event
        /// </summary>
        private void CheckForUpdatesClick(object sender, RoutedEventArgs e) =>
            CheckForUpdates();

        /// <summary>
        /// Handler for OptionsMenuItem Click event
        /// </summary>
        private void OptionsMenuItemClick(object sender, RoutedEventArgs e) =>
            ShowOptionsWindow();

        #endregion

        #region User Area

        /// <summary>
        /// Handler for CopyProtectScanButton Click event
        /// </summary>
        private void CopyProtectScanButtonClick(object sender, RoutedEventArgs e) =>
            ScanAndShowProtection();

        /// <summary>
        /// Handler for DriveLetterComboBox SelectionChanged event
        /// </summary>
        private void DriveLetterComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_canExecuteSelectionChanged)
                InitializeUIValues(removeEventHandlers: true, rescanDrives: false);
        }

        /// <summary>
        /// Handler for DriveSpeedComboBox SelectionChanged event
        /// </summary>
        private void DriveSpeedComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_canExecuteSelectionChanged)
                EnsureDiscInformation();
        }

        /// <summary>
        /// Handler for EnableParametersCheckBox Click event
        /// </summary>
        private void EnableParametersCheckBoxClick(object sender, RoutedEventArgs e) =>
            ToggleParameters();

        /// <summary>
        /// Handler for MediaScanButton Click event
        /// </summary>
        private void MediaScanButtonClick(object sender, RoutedEventArgs e) =>
            InitializeUIValues(removeEventHandlers: true, rescanDrives: true);

        /// <summary>
        /// Handler for MediaTypeComboBox SelectionChanged event
        /// </summary>
        private void MediaTypeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_canExecuteSelectionChanged)
                ChangeMediaType(e);
        }

        /// <summary>
        /// Handler for OutputDirectoryBrowseButton Click event
        /// </summary>
        private void OutputDirectoryBrowseButtonClick(object sender, RoutedEventArgs e) =>
            SetOutputDirectory();

        /// <summary>
        /// Handler for OutputFilenameTextBox TextInput event
        /// </summary>
        private void OutputDirectoryTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_canExecuteTextChanged)
              EnsureDiscInformation();
        }

        /// <summary>
        /// Handler for OutputFilenameTextBox TextInput event
        /// </summary>
        private void OutputFilenameTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_canExecuteTextChanged)
                EnsureDiscInformation();
        }

        /// <summary>
        /// Handler for StartStopButton Click event
        /// </summary>
        private void StartStopButtonClick(object sender, RoutedEventArgs e) =>
            ToggleStartStop();

        /// <summary>
        /// Handler for SystemTypeComboBox SelectionChanged event
        /// </summary>
        public void SystemTypeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_canExecuteSelectionChanged)
                ChangeSystem();
        }

        #endregion

        #endregion // Event Handlers
    }
}
