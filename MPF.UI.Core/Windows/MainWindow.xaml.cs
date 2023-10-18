using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MPF.Core;
using MPF.Core.UI.ViewModels;
using SabreTools.RedumpLib.Data;
using WPFCustomMessageBox;
using WinForms = System.Windows.Forms;

namespace MPF.UI.Core.Windows
{
    public partial class MainWindow : WindowBase
    {
        /// <summary>
        /// Read-only access to the current main view model
        /// </summary>
        public MainViewModel MainViewModel => DataContext as MainViewModel ?? new MainViewModel();

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow() => InitializeComponent();

        /// <summary>
        /// Handler for MainWindow OnContentRendered event
        /// </summary>
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // Disable buttons until we load fully
            MainViewModel.StartStopButtonEnabled = false;
            MainViewModel.MediaScanButtonEnabled = false;
            MainViewModel.UpdateVolumeLabelEnabled = false;
            MainViewModel.CopyProtectScanButtonEnabled = false;

            // Add the click handlers to the UI
            AddEventHandlers();

            // Display the debug option in the menu, if necessary
            if (MainViewModel.Options.ShowDebugViewMenuItem)
                DebugViewMenuItem.Visibility = Visibility.Visible;

            MainViewModel.Init(LogOutput.EnqueueLog, DisplayUserMessage, ShowDiscInformationWindow);

            // Set the UI color scheme according to the options
            ApplyTheme();

            // Check for updates, if necessary
            if (MainViewModel.Options.CheckForUpdatesOnStartup)
                CheckForUpdates(showIfSame: false);

            // Handle first-run, if necessary
            if (MainViewModel.Options.FirstRun)
            {
                // Show the options window
                ShowOptionsWindow("Welcome to MPF, Explore the Options");
            }
        }

        #region UI Functionality

        /// <summary>
        /// Add all event handlers
        /// </summary>
        public void AddEventHandlers()
        {
            // Menu Bar Click
            AboutMenuItem.Click += AboutClick;
            AppExitMenuItem.Click += AppExitClick;
            CheckForUpdatesMenuItem.Click += CheckForUpdatesClick;
            DebugViewMenuItem.Click += DebugViewClick;
            OptionsMenuItem.Click += OptionsMenuItemClick;

            // User Area Click
            CopyProtectScanButton.Click += CopyProtectScanButtonClick;
            EnableParametersCheckBox.Click += EnableParametersCheckBoxClick;
            MediaScanButton.Click += MediaScanButtonClick;
            UpdateVolumeLabel.Click += UpdateVolumeLabelClick;
            OutputPathBrowseButton.Click += OutputPathBrowseButtonClick;
            StartStopButton.Click += StartStopButtonClick;

            // User Area SelectionChanged
            SystemTypeComboBox.SelectionChanged += SystemTypeComboBoxSelectionChanged;
            MediaTypeComboBox.SelectionChanged += MediaTypeComboBoxSelectionChanged;
            DriveLetterComboBox.SelectionChanged += DriveLetterComboBoxSelectionChanged;
            DriveSpeedComboBox.SelectionChanged += DriveSpeedComboBoxSelectionChanged;
            DumpingProgramComboBox.SelectionChanged += DumpingProgramComboBoxSelectionChanged;

            // User Area TextChanged
            OutputPathTextBox.TextChanged += OutputPathTextBoxTextChanged;
        }

        /// <summary>
        /// Browse for an output file path
        /// </summary>
        public void BrowseFile()
        {
            // Get the current path, if possible
            string currentPath = MainViewModel.OutputPath;
            if (string.IsNullOrWhiteSpace(currentPath) && !string.IsNullOrWhiteSpace(MainViewModel.Options.DefaultOutputPath))
                currentPath = Path.Combine(MainViewModel.Options.DefaultOutputPath, "track.bin");
            else if (string.IsNullOrWhiteSpace(currentPath))
                currentPath = "track.bin";
            if (string.IsNullOrWhiteSpace(currentPath))
                currentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "track.bin");

            // Get the full path
            currentPath = Path.GetFullPath(currentPath);

            // Get the directory
            var directory = Path.GetDirectoryName(currentPath);

            // Get the filename
            string filename = Path.GetFileName(currentPath);

            WinForms.FileDialog fileDialog = new WinForms.SaveFileDialog
            {
                FileName = filename,
                InitialDirectory = directory,
            };
            WinForms.DialogResult result = fileDialog.ShowDialog();

            if (result == WinForms.DialogResult.OK)
            {
                MainViewModel.OutputPath = fileDialog.FileName;
            }
        }

        /// <summary>
        /// Check for available updates
        /// </summary>
        /// <param name="showIfSame">True to show the box even if it's the same, false to only show if it's different</param>
        public void CheckForUpdates(bool showIfSame)
        {
            (bool different, string message, var url) = MainViewModel.CheckForUpdates();

            // If we have a new version, put it in the clipboard
            if (different)
                Clipboard.SetText(url);

            if (showIfSame || different)
                CustomMessageBox.Show(this, message, "Version Update Check", MessageBoxButton.OK, different ? MessageBoxImage.Exclamation : MessageBoxImage.Information);
        }

        /// <summary>
        /// Display a user message using a CustomMessageBox
        /// </summary>
        /// <param name="title">Title to display to the user</param>
        /// <param name="message">Message to display to the user</param>
        /// <param name="optionCount">Number of options to display</param>
        /// <param name="flag">true for inquiry, false otherwise</param>
        /// <returns>true for positive, false for negative, null for neutral</returns>
        public bool? DisplayUserMessage(string title, string message, int optionCount, bool flag)
        {
            // Set the correct button style
            MessageBoxButton button;
            switch (optionCount)
            {
                case 1:
                    button = MessageBoxButton.OK;
                    break;
                case 2:
                    button = MessageBoxButton.YesNo;
                    break;
                case 3:
                    button = MessageBoxButton.YesNoCancel;
                    break;

                // This should not happen, but default to "OK"
                default:
                    button = MessageBoxButton.OK;
                    break;
            }

            // Set the correct icon
            MessageBoxImage image = flag ? MessageBoxImage.Question : MessageBoxImage.Exclamation;

            // Display and get the result
            MessageBoxResult result = CustomMessageBox.Show(this, message, title, button, image);
            switch (result)
            {
                case MessageBoxResult.OK:
                case MessageBoxResult.Yes:
                    return true;

                case MessageBoxResult.No:
                    return false;

                case MessageBoxResult.Cancel:
                case MessageBoxResult.None:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Build a dummy SubmissionInfo and display it for testing
        /// </summary>
        public void ShowDebugDiscInfoWindow()
        {
            var submissionInfo = MainViewModel.CreateDebugSubmissionInfo();
            var result = ShowDiscInformationWindow(submissionInfo);
            InfoTool.ProcessSpecialFields(result.Item2);
        }

        /// <summary>
        /// Show the disc information window
        /// </summary>
        /// <param name="submissionInfo">SubmissionInfo object to display and possibly change</param>
        /// <returns>Dialog open result</returns>
#if NET48
        public (bool?, SubmissionInfo) ShowDiscInformationWindow(SubmissionInfo submissionInfo)
#else
        public (bool?, SubmissionInfo?) ShowDiscInformationWindow(SubmissionInfo? submissionInfo)
#endif
        {
            if (MainViewModel.Options.ShowDiscEjectReminder)
                CustomMessageBox.Show(this, "It is now safe to eject the disc", "Eject", MessageBoxButton.OK, MessageBoxImage.Information);

            var discInformationWindow = new DiscInformationWindow(MainViewModel.Options, submissionInfo)
            {
                Focusable = true,
                Owner = this,
                ShowActivated = true,
                ShowInTaskbar = true,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };
            bool? result = discInformationWindow.ShowDialog();

            // Copy back the submission info changes, if necessary
            if (result == true)
#if NET48
                submissionInfo = discInformationWindow.DiscInformationViewModel.SubmissionInfo.Clone() as SubmissionInfo;
#else
                submissionInfo = (discInformationWindow.DiscInformationViewModel.SubmissionInfo.Clone() as SubmissionInfo)!;
#endif

#if NET48
            return (result, submissionInfo);
#else
            return (result, submissionInfo!);
#endif
        }

        /// <summary>
        /// Show the Options window
        /// </summary>
#if NET48
        public void ShowOptionsWindow(string title = null)
#else
        public void ShowOptionsWindow(string? title = null)
#endif
        {
            var optionsWindow = new OptionsWindow(MainViewModel.Options)
            {
                Focusable = true,
                Owner = this,
                ShowActivated = true,
                ShowInTaskbar = true,
                Title = title ?? "Options",
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };

            optionsWindow.Closed += OnOptionsUpdated;
            optionsWindow.Show();
        }

        /// <summary>
        /// Set the UI color scheme according to the options
        /// </summary>
        private void ApplyTheme()
        {
            if (MainViewModel.Options.EnableDarkMode)
                EnableDarkMode();
            else
                EnableLightMode();
        }

        /// <summary>
        /// Recolor all UI elements for light mode
        /// </summary>
        private static void EnableLightMode()
        {
            var theme = new LightModeTheme();
            theme.Apply();
        }

        /// <summary>
        /// Recolor all UI elements for dark mode
        /// </summary>
        private static void EnableDarkMode()
        {
            var theme = new DarkModeTheme();
            theme.Apply();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for OptionsWindow OnUpdated event
        /// </summary>
#if NET48
        public void OnOptionsUpdated(object sender, EventArgs e)
#else
        public void OnOptionsUpdated(object? sender, EventArgs e)
#endif
        {
            // Get the options window
            var optionsWindow = (sender as OptionsWindow);
            if (optionsWindow?.OptionsViewModel == null)
                return;

            bool savedSettings = optionsWindow.OptionsViewModel.SavedSettings;
            var options = optionsWindow.OptionsViewModel.Options;
            MainViewModel.UpdateOptions(savedSettings, options);

            // Set the UI color scheme according to the options
            ApplyTheme();
        }

        #region Menu Bar

        /// <summary>
        /// Handler for AboutMenuItem Click event
        /// </summary>
        public void AboutClick(object sender, RoutedEventArgs e)
        {
            string aboutText = MainViewModel.CreateAboutText();
            CustomMessageBox.Show(this, aboutText, "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Handler for AppExitMenuItem Click event
        /// </summary>
        public void AppExitClick(object sender, RoutedEventArgs e) =>
            Application.Current.Shutdown();

        /// <summary>
        /// Handler for CheckForUpdatesMenuItem Click event
        /// </summary>
        public void CheckForUpdatesClick(object sender, RoutedEventArgs e)
            => CheckForUpdates(showIfSame: true);

        /// <summary>
        /// Handler for DebugViewMenuItem Click event
        /// </summary>
        public void DebugViewClick(object sender, RoutedEventArgs e) =>
            ShowDebugDiscInfoWindow();

        /// <summary>
        /// Handler for OptionsMenuItem Click event
        /// </summary>
        public void OptionsMenuItemClick(object sender, RoutedEventArgs e) =>
            ShowOptionsWindow();

        #endregion

        #region User Area

        /// <summary>
        /// Handler for CopyProtectScanButton Click event
        /// </summary>
        public async void CopyProtectScanButtonClick(object sender, RoutedEventArgs e)
        {
            var (output, error) = await MainViewModel.ScanAndShowProtection();

            if (!MainViewModel.LogPanelExpanded)
            {
                if (!string.IsNullOrEmpty(output) && string.IsNullOrEmpty(error))
                    CustomMessageBox.Show(this, output, "Detected Protection(s)", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    CustomMessageBox.Show(this, "An exception occurred, see the log for details", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handler for DriveLetterComboBox SelectionChanged event
        /// </summary>
        public void DriveLetterComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
                MainViewModel.InitializeUIValues(removeEventHandlers: true, rescanDrives: false);
        }

        /// <summary>
        /// Handler for DriveSpeedComboBox SelectionChanged event
        /// </summary>
        public void DriveSpeedComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
                MainViewModel.EnsureDiscInformation();
        }

        /// <summary>
        /// Handler for DumpingProgramComboBox SelectionChanged event
        /// </summary>
        public void DumpingProgramComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
                MainViewModel.ChangeDumpingProgram();
        }

        /// <summary>
        /// Handler for EnableParametersCheckBox Click event
        /// </summary>
        public void EnableParametersCheckBoxClick(object sender, RoutedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
                MainViewModel.ToggleParameters();
        }

        /// <summary>
        /// Handler for MediaScanButton Click event
        /// </summary>
        public void MediaScanButtonClick(object sender, RoutedEventArgs e) =>
            MainViewModel.InitializeUIValues(removeEventHandlers: true, rescanDrives: true);

        /// <summary>
        /// Handler for MediaTypeComboBox SelectionChanged event
        /// </summary>
        public void MediaTypeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
                MainViewModel.ChangeMediaType(e.RemovedItems, e.AddedItems);
        }

        /// <summary>
        /// Handler for OutputPathBrowseButton Click event
        /// </summary>
        public void OutputPathBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            BrowseFile();
            MainViewModel.EnsureDiscInformation();
        }

        /// <summary>
        /// Handler for OutputPathTextBox TextChanged event
        /// </summary>
        public void OutputPathTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
                MainViewModel.EnsureDiscInformation();
        }

        /// <summary>
        /// Handler for StartStopButton Click event
        /// </summary>
        public void StartStopButtonClick(object sender, RoutedEventArgs e) =>
            MainViewModel.ToggleStartStop();

        /// <summary>
        /// Handler for SystemTypeComboBox SelectionChanged event
        /// </summary>
        public void SystemTypeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
                MainViewModel.ChangeSystem();
        }

        /// <summary>
        /// Handler for UpdateVolumeLabel Click event
        /// </summary>
        public void UpdateVolumeLabelClick(object sender, RoutedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
            {
                if (MainViewModel.Options.FastUpdateLabel)
                    MainViewModel.FastUpdateLabel(removeEventHandlers: true);
                else
                    MainViewModel.InitializeUIValues(removeEventHandlers: true, rescanDrives: false);
            }
        }

        #endregion

        #endregion // Event Handlers
    }
}
