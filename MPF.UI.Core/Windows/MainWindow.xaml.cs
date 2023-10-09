using System;
using System.Windows;
using System.Windows.Controls;
using MPF.Core;
using MPF.UI.Core.ViewModels;

namespace MPF.UI.Core.Windows
{
    public partial class MainWindow : WindowBase
    {
        /// <summary>
        /// Read-only access to the current main view model
        /// </summary>
        public MainViewModel MainViewModel => DataContext as MainViewModel;

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

            MainViewModel.Init(this);
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
        /// Build a dummy SubmissionInfo and display it for testing
        /// </summary>
        public void ShowDebugDiscInfoWindow()
        {
            var submissionInfo = MainViewModel.CreateDebugSubmissionInfo();
            var result = MainViewModel.ShowDiscInformationWindow(submissionInfo);
            InfoTool.ProcessSpecialFields(result.Item2);
        }

        /// <summary>
        /// Show the Options window
        /// </summary>
        public void ShowOptionsWindow()
        {
            var optionsWindow = new OptionsWindow(MainViewModel.Options)
            {
                Focusable = true,
                Owner = this,
                ShowActivated = true,
                ShowInTaskbar = true,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };
            optionsWindow.Closed += MainViewModel.OnOptionsUpdated;
            optionsWindow.Show();
        }

        #endregion

        #region Event Handlers

        #region Menu Bar

        /// <summary>
        /// Handler for AboutMenuItem Click event
        /// </summary>
        public void AboutClick(object sender, RoutedEventArgs e) =>
            MainViewModel.ShowAboutText();

        /// <summary>
        /// Handler for AppExitMenuItem Click event
        /// </summary>
        public void AppExitClick(object sender, RoutedEventArgs e) =>
            MainViewModel.ExitApplication();

        /// <summary>
        /// Handler for CheckForUpdatesMenuItem Click event
        /// </summary>
        public void CheckForUpdatesClick(object sender, RoutedEventArgs e) =>
            MainViewModel.CheckForUpdates(showIfSame: true);

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
        public void CopyProtectScanButtonClick(object sender, RoutedEventArgs e) =>
            MainViewModel.ScanAndShowProtection();

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
                MainViewModel.ChangeMediaType(e);
        }

        /// <summary>
        /// Handler for OutputPathBrowseButton Click event
        /// </summary>
        public void OutputPathBrowseButtonClick(object sender, RoutedEventArgs e) =>
            MainViewModel.SetOutputPath();

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
