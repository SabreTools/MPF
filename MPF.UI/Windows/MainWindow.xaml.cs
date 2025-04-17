using System;
using System.ComponentModel;
using System.IO;
#if NET40
using System.Threading.Tasks;
#endif
using System.Windows;
using System.Windows.Controls;
using MPF.Frontend;
using MPF.Frontend.ViewModels;
using MPF.UI.Themes;
using MPF.UI.UserControls;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;
using WPFCustomMessageBox;
using WinForms = System.Windows.Forms;

namespace MPF.UI.Windows
{
    public partial class MainWindow : WindowBase
    {
        /// <summary>
        /// Read-only access to the current main view model
        /// </summary>
        public MainViewModel MainViewModel => DataContext as MainViewModel ?? new MainViewModel();

#if NET35

        #region Top Menu Bar

        private MenuItem? AboutMenuItem => ItemHelper.FindChild<MenuItem>(this, "AboutMenuItem");
        private MenuItem? AppExitMenuItem => ItemHelper.FindChild<MenuItem>(this, "AppExitMenuItem");
        private MenuItem? CheckForUpdatesMenuItem => ItemHelper.FindChild<MenuItem>(this, "CheckForUpdatesMenuItem");
        private MenuItem? DebugViewMenuItem => ItemHelper.FindChild<MenuItem>(this, "DebugViewMenuItem");
        private MenuItem? CheckDumpMenuItem => ItemHelper.FindChild<MenuItem>(this, "CheckDumpMenuItem");
        private MenuItem? CreateIRDMenuItem => ItemHelper.FindChild<MenuItem>(this, "CreateIRDMenuItem");
        private MenuItem? OptionsMenuItem => ItemHelper.FindChild<MenuItem>(this, "OptionsMenuItem");

        #endregion

        #region Settings

        private ComboBox? DriveLetterComboBox => ItemHelper.FindChild<ComboBox>(this, "DriveLetterComboBox");
        private ComboBox? DriveSpeedComboBox => ItemHelper.FindChild<ComboBox>(this, "DriveSpeedComboBox");
        private ComboBox? DumpingProgramComboBox => ItemHelper.FindChild<ComboBox>(this, "DumpingProgramComboBox");
        private CheckBox? EnableParametersCheckBox => ItemHelper.FindChild<CheckBox>(this, "EnableParametersCheckBox");
        private ComboBox? MediaTypeComboBox => ItemHelper.FindChild<ComboBox>(this, "MediaTypeComboBox");
        private Button? OutputPathBrowseButton => ItemHelper.FindChild<Button>(this, "OutputPathBrowseButton");
        private TextBox? OutputPathTextBox => ItemHelper.FindChild<TextBox>(this, "OutputPathTextBox");
        private ComboBox? SystemTypeComboBox => ItemHelper.FindChild<ComboBox>(this, "SystemTypeComboBox");

        #endregion

        #region Controls

        private Button? CopyProtectScanButton => ItemHelper.FindChild<Button>(this, "CopyProtectScanButton");
        private Button? MediaScanButton => ItemHelper.FindChild<Button>(this, "MediaScanButton");
        private Button? StartStopButton => ItemHelper.FindChild<Button>(this, "StartStopButton");
        private Button? UpdateVolumeLabel => ItemHelper.FindChild<Button>(this, "UpdateVolumeLabel");

        #endregion

        #region Status

        private LogOutput? LogOutput => ItemHelper.FindChild<LogOutput>(this, "LogOutput");

        #endregion

#endif

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
#if NET40_OR_GREATER || NETCOREAPP
            InitializeComponent();
            this.Closing += MainWindowClosing;
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
                DebugViewMenuItem!.Visibility = Visibility.Visible;

            MainViewModel.Init(LogOutput!.EnqueueLog, DisplayUserMessage, ShowDiscInformationWindow);

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
            AboutMenuItem!.Click += AboutClick;
            AppExitMenuItem!.Click += AppExitClick;
            CheckForUpdatesMenuItem!.Click += CheckForUpdatesClick;
            DebugViewMenuItem!.Click += DebugViewClick;
            CheckDumpMenuItem!.Click += CheckDumpMenuItemClick;
            CreateIRDMenuItem!.Click += CreateIRDMenuItemClick;
            OptionsMenuItem!.Click += OptionsMenuItemClick;

            // User Area Click
            CopyProtectScanButton!.Click += CopyProtectScanButtonClick;
            EnableParametersCheckBox!.Click += EnableParametersCheckBoxClick;
            MediaScanButton!.Click += MediaScanButtonClick;
            UpdateVolumeLabel!.Click += UpdateVolumeLabelClick;
            OutputPathBrowseButton!.Click += OutputPathBrowseButtonClick;
            StartStopButton!.Click += StartStopButtonClick;

            // User Area SelectionChanged
            SystemTypeComboBox!.SelectionChanged += SystemTypeComboBoxSelectionChanged;
            MediaTypeComboBox!.SelectionChanged += MediaTypeComboBoxSelectionChanged;
            DriveLetterComboBox!.SelectionChanged += DriveLetterComboBoxSelectionChanged;
            DriveSpeedComboBox!.SelectionChanged += DriveSpeedComboBoxSelectionChanged;
            DumpingProgramComboBox!.SelectionChanged += DumpingProgramComboBoxSelectionChanged;

            // User Area TextChanged
            OutputPathTextBox!.TextChanged += OutputPathTextBoxTextChanged;
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
            MainViewModel.CheckForUpdates(out bool different, out string message, out var url);

            // If we have a new version, put it in the clipboard
            if (different && !string.IsNullOrEmpty(url))
                Clipboard.SetText(url);

            if (showIfSame || different)
                CustomMessageBox.Show(this, message, "Version Update Check", MessageBoxButton.OK, different ? MessageBoxImage.Exclamation : MessageBoxImage.Information);
        }

        /// <summary>
        /// Ask to confirm quitting, when an operation is running
        /// </summary>
        public void MainWindowClosing(object? sender, CancelEventArgs e)
        {
            if (MainViewModel.AskBeforeQuit)
            {
                MessageBoxResult result = CustomMessageBox.Show(this, "A dump is still being processed, are you sure you want to quit?", "Quit", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.No)
                    e.Cancel = true;
            }
        }

        /// <summary>
        /// Build a dummy SubmissionInfo and display it for testing
        /// </summary>
        public void ShowDebugDiscInfoWindow()
        {
            var submissionInfo = MainViewModel.CreateDebugSubmissionInfo();
            _ = ShowDiscInformationWindow(MainViewModel.Options, ref submissionInfo);
            Formatter.ProcessSpecialFields(submissionInfo!);
        }

        /// <summary>
        /// Show the disc information window
        /// </summary>
        /// <param name="options">Options set to pass to the information window</param>
        /// <param name="submissionInfo">SubmissionInfo object to display and possibly change</param>
        /// <returns>Dialog open result</returns>
        public bool? ShowDiscInformationWindow(Options? options, ref SubmissionInfo? submissionInfo)
        {
            if (options?.ShowDiscEjectReminder == true)
                CustomMessageBox.Show(this, "It is now safe to eject the disc", "Eject", MessageBoxButton.OK, MessageBoxImage.Information);

            var discInformationWindow = new DiscInformationWindow(options ?? new Options(), submissionInfo)
            {
                Focusable = true,
                Owner = this,
                ShowActivated = true,
                ShowInTaskbar = true,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };

            discInformationWindow.Closed += delegate { Activate(); };
            bool? result = discInformationWindow.ShowDialog();

            // Copy back the submission info changes, if necessary
            if (result == true)
                submissionInfo = (discInformationWindow.DiscInformationViewModel.SubmissionInfo.Clone() as SubmissionInfo)!;

            return result;
        }

        /// <summary>
        /// Show the Check Dump window
        /// </summary>
        public void ShowCheckDumpWindow()
        {
            // Hide MainWindow while Check GUI is open
            Hide();

            var checkDumpWindow = new CheckDumpWindow(this)
            {
                Focusable = true,
                Owner = this,
                ShowActivated = true,
                ShowInTaskbar = true,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };

            checkDumpWindow.Closed += delegate
            {
                // Unhide Main window after Check window has been closed
                Show();
                Activate();
            };
            checkDumpWindow.Show();
        }

        /// <summary>
        /// Show the Create IRD window
        /// </summary>
        public void ShowCreateIRDWindow()
        {
            // Hide MainWindow while Create IRD UI is open
            Hide();

            var createIRDWindow = new CreateIRDWindow(this)
            {
                Focusable = true,
                Owner = this,
                ShowActivated = true,
                ShowInTaskbar = true,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };

            createIRDWindow.Closed += delegate
            {
                // Unhide Main window after Create IRD window has been closed
                Show();
                Activate();
            };
            createIRDWindow.Show();
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

            optionsWindow.Closed += delegate { Activate(); };
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
            else if (MainViewModel.Options.EnablePurpMode)
                theme = new CustomTheme("111111", "9A5EC0");
            else if (IsHexColor(MainViewModel.Options.CustomBackgroundColor) && IsHexColor(MainViewModel.Options.CustomTextColor))
                theme = new CustomTheme(MainViewModel.Options.CustomBackgroundColor, MainViewModel.Options.CustomTextColor);
            else
                theme = new LightModeTheme();

            theme.Apply();
        }

        /// <summary>
        /// Check whether a string represents a valid hex color
        /// </summary>
        public static bool IsHexColor(string? color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return false;

            if (color!.Length == 7 && color[0] == '#')
                color = color.Substring(1);

            if (color.Length != 6)
                return false;

            for (int i = 0; i < color.Length; i++)
            {
                switch (color[i])
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                        continue;
                    default:
                        break;
                }
            }

            return true;
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

            // Get if the settings were saved
            bool savedSettings = optionsWindow.OptionsViewModel.SavedSettings;
            var options = optionsWindow.OptionsViewModel.Options;

            // Force a refresh of the path, if necessary
            if (MainViewModel.Options.DefaultOutputPath != options.DefaultOutputPath)
                MainViewModel.OutputPath = string.Empty;

            // Update and save options, if necessary
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
        /// Handler for CreateIRDMenuItem Click event
        /// </summary>
        public void CreateIRDMenuItemClick(object sender, RoutedEventArgs e) =>
            ShowCreateIRDWindow();

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
            var output = await MainViewModel.ScanAndShowProtection();

            if (!MainViewModel.LogPanelExpanded)
            {
                if (!string.IsNullOrEmpty(output))
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
                MainViewModel.InitializeUIValues(removeEventHandlers: true, rebuildPrograms: false, rescanDrives: false);
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
            MainViewModel.InitializeUIValues(removeEventHandlers: true, rebuildPrograms: false, rescanDrives: true);

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
                    MainViewModel.InitializeUIValues(removeEventHandlers: true, rebuildPrograms: false, rescanDrives: false);
            }
        }

        #endregion

        #endregion // Event Handlers
    }
}
