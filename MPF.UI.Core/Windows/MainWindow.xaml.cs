using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MPF.Core.UI.ViewModels;
using MPF.UI.Core.UserControls;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;
using WPFCustomMessageBox;
using WinForms = System.Windows.Forms;

#pragma warning disable IDE1006 // Naming Styles

namespace MPF.UI.Core.Windows
{
    public partial class MainWindow : WindowBase
    {
        /// <summary>
        /// Read-only access to the current main view model
        /// </summary>
        public MainViewModel MainViewModel => DataContext as MainViewModel ?? new MainViewModel();

#if NET35

        #region Top Menu Bar

        private MenuItem? _AboutMenuItem => ItemHelper.FindChild<MenuItem>(this, "AboutMenuItem");
        private MenuItem? _AppExitMenuItem => ItemHelper.FindChild<MenuItem>(this, "AppExitMenuItem");
        private MenuItem? _CheckForUpdatesMenuItem => ItemHelper.FindChild<MenuItem>(this, "CheckForUpdatesMenuItem");
        private MenuItem? _DebugViewMenuItem => ItemHelper.FindChild<MenuItem>(this, "DebugViewMenuItem");
        private MenuItem? _CheckDumpMenuItem => ItemHelper.FindChild<MenuItem>(this, "CheckDumpMenuItem");
        private MenuItem? _OptionsMenuItem => ItemHelper.FindChild<MenuItem>(this, "OptionsMenuItem");

        #endregion

        #region Settings

        private ComboBox? _DriveLetterComboBox => ItemHelper.FindChild<ComboBox>(this, "DriveLetterComboBox");
        private ComboBox? _DriveSpeedComboBox => ItemHelper.FindChild<ComboBox>(this, "DriveSpeedComboBox");
        private ComboBox? _DumpingProgramComboBox => ItemHelper.FindChild<ComboBox>(this, "DumpingProgramComboBox");
        private CheckBox? _EnableParametersCheckBox => ItemHelper.FindChild<CheckBox>(this, "EnableParametersCheckBox");
        private ComboBox? _MediaTypeComboBox => ItemHelper.FindChild<ComboBox>(this, "MediaTypeComboBox");
        private Button? _OutputPathBrowseButton => ItemHelper.FindChild<Button>(this, "OutputPathBrowseButton");
        private TextBox? _OutputPathTextBox => ItemHelper.FindChild<TextBox>(this, "OutputPathTextBox");
        private ComboBox? _SystemTypeComboBox => ItemHelper.FindChild<ComboBox>(this, "SystemTypeComboBox");

        #endregion

        #region Controls

        private Button? _CopyProtectScanButton => ItemHelper.FindChild<Button>(this, "CopyProtectScanButton");
        private Button? _MediaScanButton => ItemHelper.FindChild<Button>(this, "MediaScanButton");
        private Button? _StartStopButton => ItemHelper.FindChild<Button>(this, "StartStopButton");
        private Button? _UpdateVolumeLabel => ItemHelper.FindChild<Button>(this, "UpdateVolumeLabel");

        #endregion

        #region Status

        private LogOutput? _LogOutput => ItemHelper.FindChild<LogOutput>(this, "LogOutput");

        #endregion

#endif

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
#if NET40_OR_GREATER || NETCOREAPP
            InitializeComponent();
#endif

#if NET452_OR_GREATER || NETCOREAPP
            var chrome = new System.Windows.Shell.WindowChrome
            {
                CaptionHeight = 0,
                ResizeBorderThickness = new Thickness(0),
            };
            System.Windows.Shell.WindowChrome.SetWindowChrome(this, chrome);
#endif
        }

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
#if NET35
                _DebugViewMenuItem!.Visibility = Visibility.Visible;
#else
                DebugViewMenuItem.Visibility = Visibility.Visible;
#endif

#if NET35
            MainViewModel.Init(_LogOutput!.EnqueueLog, DisplayUserMessage, ShowDiscInformationWindow);
#else
            MainViewModel.Init(LogOutput.EnqueueLog, DisplayUserMessage, ShowDiscInformationWindow);
#endif

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
#if NET35
            _AboutMenuItem!.Click += AboutClick;
            _AppExitMenuItem!.Click += AppExitClick;
            _CheckForUpdatesMenuItem!.Click += CheckForUpdatesClick;
            _DebugViewMenuItem!.Click += DebugViewClick;
            _CheckDumpMenuItem!.Click += CheckDumpMenuItemClick;
            _OptionsMenuItem!.Click += OptionsMenuItemClick;
#else
            AboutMenuItem.Click += AboutClick;
            AppExitMenuItem.Click += AppExitClick;
            CheckForUpdatesMenuItem.Click += CheckForUpdatesClick;
            DebugViewMenuItem.Click += DebugViewClick;
            CheckDumpMenuItem.Click += CheckDumpMenuItemClick;
            OptionsMenuItem.Click += OptionsMenuItemClick;
#endif

            // User Area Click
#if NET35
            _CopyProtectScanButton!.Click += CopyProtectScanButtonClick;
            _EnableParametersCheckBox!.Click += EnableParametersCheckBoxClick;
            _MediaScanButton!.Click += MediaScanButtonClick;
            _UpdateVolumeLabel!.Click += UpdateVolumeLabelClick;
            _OutputPathBrowseButton!.Click += OutputPathBrowseButtonClick;
            _StartStopButton!.Click += StartStopButtonClick;
#else
            CopyProtectScanButton.Click += CopyProtectScanButtonClick;
            EnableParametersCheckBox.Click += EnableParametersCheckBoxClick;
            MediaScanButton.Click += MediaScanButtonClick;
            UpdateVolumeLabel.Click += UpdateVolumeLabelClick;
            OutputPathBrowseButton.Click += OutputPathBrowseButtonClick;
            StartStopButton.Click += StartStopButtonClick;
#endif

            // User Area SelectionChanged
#if NET35
            _SystemTypeComboBox!.SelectionChanged += SystemTypeComboBoxSelectionChanged;
            _MediaTypeComboBox!.SelectionChanged += MediaTypeComboBoxSelectionChanged;
            _DriveLetterComboBox!.SelectionChanged += DriveLetterComboBoxSelectionChanged;
            _DriveSpeedComboBox!.SelectionChanged += DriveSpeedComboBoxSelectionChanged;
            _DumpingProgramComboBox!.SelectionChanged += DumpingProgramComboBoxSelectionChanged;
#else
            SystemTypeComboBox.SelectionChanged += SystemTypeComboBoxSelectionChanged;
            MediaTypeComboBox.SelectionChanged += MediaTypeComboBoxSelectionChanged;
            DriveLetterComboBox.SelectionChanged += DriveLetterComboBoxSelectionChanged;
            DriveSpeedComboBox.SelectionChanged += DriveSpeedComboBoxSelectionChanged;
            DumpingProgramComboBox.SelectionChanged += DumpingProgramComboBoxSelectionChanged;
#endif

            // User Area TextChanged
#if NET35
            _OutputPathTextBox!.TextChanged += OutputPathTextBoxTextChanged;
#else
            OutputPathTextBox.TextChanged += OutputPathTextBoxTextChanged;
#endif
        }

        /// <summary>
        /// Browse for an output file path
        /// </summary>
        public void BrowseFile()
        {
            // Get the current path, if possible
            string currentPath = MainViewModel.OutputPath;
            if (string.IsNullOrEmpty(currentPath) && !string.IsNullOrEmpty(MainViewModel.Options.DefaultOutputPath))
                currentPath = Path.Combine(MainViewModel.Options.DefaultOutputPath, "track.bin");
            else if (string.IsNullOrEmpty(currentPath))
                currentPath = "track.bin";
            if (string.IsNullOrEmpty(currentPath))
                currentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "track.bin");

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
            if (different && !string.IsNullOrEmpty(url))
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
            var button = optionCount switch
            {
                1 => MessageBoxButton.OK,
                2 => MessageBoxButton.YesNo,
                3 => MessageBoxButton.YesNoCancel,

                // This should not happen, but default to "OK"
                _ => MessageBoxButton.OK,
            };

            // Set the correct icon
            MessageBoxImage image = flag ? MessageBoxImage.Question : MessageBoxImage.Exclamation;

            // Display and get the result
            MessageBoxResult result = CustomMessageBox.Show(this, message, title, button, image);
            return result switch
            {
                MessageBoxResult.OK or MessageBoxResult.Yes => true,
                MessageBoxResult.No => false,
                _ => null,
            };
        }

        /// <summary>
        /// Build a dummy SubmissionInfo and display it for testing
        /// </summary>
        public void ShowDebugDiscInfoWindow()
        {
            var submissionInfo = MainViewModel.CreateDebugSubmissionInfo();
            var result = ShowDiscInformationWindow(submissionInfo);
            Formatter.ProcessSpecialFields(result.Item2);
        }

        /// <summary>
        /// Show the disc information window
        /// </summary>
        /// <param name="submissionInfo">SubmissionInfo object to display and possibly change</param>
        /// <returns>Dialog open result</returns>
        public (bool?, SubmissionInfo?) ShowDiscInformationWindow(SubmissionInfo? submissionInfo)
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

            discInformationWindow.Closed += delegate { this.Activate(); };
            bool? result = discInformationWindow.ShowDialog();

            // Copy back the submission info changes, if necessary
            if (result == true)
                submissionInfo = (discInformationWindow.DiscInformationViewModel.SubmissionInfo.Clone() as SubmissionInfo)!;

            return (result, submissionInfo!);
        }

        /// <summary>
        /// Show the Check Dump window
        /// </summary>
        public void ShowCheckDumpWindow()
        {
            // Hide MainWindow while Check GUI is open
            this.Hide();

            var checkDumpWindow = new CheckDumpWindow(this)
            {
                Focusable = true,
                Owner = this,
                ShowActivated = true,
                ShowInTaskbar = true,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };

            checkDumpWindow.Closed += delegate {
                // Unhide Main window after Check window has been closed
                this.Show();
                this.Activate();
            };
            checkDumpWindow.Show();
        }

        /// <summary>
        /// Show the Options window
        /// </summary>
        public void ShowOptionsWindow(string? title = null)
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

            optionsWindow.Closed += delegate { this.Activate(); };
            optionsWindow.Closed += OnOptionsUpdated;
            optionsWindow.Show();
        }

        /// <summary>
        /// Set the UI color scheme according to the options
        /// </summary>
        private void ApplyTheme()
        {
            Theme theme;
            if (MainViewModel.Options.EnableDarkMode)
                theme = new DarkModeTheme();
            else
                theme = new LightModeTheme();

            theme.Apply();
        }

#endregion

        #region Event Handlers

        /// <summary>
        /// Handler for OptionsWindow OnUpdated event
        /// </summary>
        public void OnOptionsUpdated(object? sender, EventArgs e)
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
        /// Handler for CheckDumpMenuItem Click event
        /// </summary>
        public void CheckDumpMenuItemClick(object sender, RoutedEventArgs e) =>
            ShowCheckDumpWindow();

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
#if NET40
        public void CopyProtectScanButtonClick(object sender, RoutedEventArgs e)
#else
        public async void CopyProtectScanButtonClick(object sender, RoutedEventArgs e)
#endif
        {
#if NET40
            var (output, error) = MainViewModel.ScanAndShowProtection();
#else
            var (output, error) = await MainViewModel.ScanAndShowProtection();
#endif

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
