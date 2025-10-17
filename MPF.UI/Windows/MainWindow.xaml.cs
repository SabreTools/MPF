using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
#if NET40
using System.Threading.Tasks;
#endif
using System.Windows;
using System.Windows.Controls;
using MPF.Frontend;
using MPF.Frontend.Tools;
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

        // Buttons
        private MenuItem? AboutMenuItem => ItemHelper.FindChild<MenuItem>(this, "AboutMenuItem");
        private MenuItem? AppExitMenuItem => ItemHelper.FindChild<MenuItem>(this, "AppExitMenuItem");
        private MenuItem? CheckForUpdatesMenuItem => ItemHelper.FindChild<MenuItem>(this, "CheckForUpdatesMenuItem");
        private MenuItem? DebugViewMenuItem => ItemHelper.FindChild<MenuItem>(this, "DebugViewMenuItem");
        private MenuItem? CheckDumpMenuItem => ItemHelper.FindChild<MenuItem>(this, "CheckDumpMenuItem");
        private MenuItem? CreateIRDMenuItem => ItemHelper.FindChild<MenuItem>(this, "CreateIRDMenuItem");
        private MenuItem? OptionsMenuItem => ItemHelper.FindChild<MenuItem>(this, "OptionsMenuItem");

        // Languages
        private MenuItem? EnglishMenuItem => ItemHelper.FindChild<MenuItem>(this, "EnglishMenuItem");
        private MenuItem? KoreanMenuItem => ItemHelper.FindChild<MenuItem>(this, "KoreanMenuItem");

        #endregion

        #region Settings

        private ComboBox? DriveLetterComboBox => ItemHelper.FindChild<ComboBox>(this, "DriveLetterComboBox");
        private ComboBox? DriveSpeedComboBox => ItemHelper.FindChild<ComboBox>(this, "DriveSpeedComboBox");
        private ComboBox? DumpingProgramComboBox => ItemHelper.FindChild<ComboBox>(this, "DumpingProgramComboBox");
        private CheckBox? EnableParametersCheckBox => ItemHelper.FindChild<CheckBox>(this, "EnableParametersCheckBox");
        private ComboBox? MediaTypeComboBox => ItemHelper.FindChild<ComboBox>(this, "MediaTypeComboBox");
        private Button? OutputPathBrowseButton => ItemHelper.FindChild<Button>(this, "OutputPathBrowseButton");
        private TextBox? OutputPathTextBox => ItemHelper.FindChild<TextBox>(this, "OutputPathTextBox");
        private Label? SystemMediaTypeLabel => ItemHelper.FindChild<Label>(this, "SystemMediaTypeLabel");
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

            // Set all resources before window loads (Must set English here)
            SetInterfaceLanguage(InterfaceLanguage.English);
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

            MainViewModel.Init(LogOutput!.EnqueueLog, DisplayUserMessage, ShowMediaInformationWindow);

            // Pass translation strings to MainViewModel
            var translationStrings = new Dictionary<string, string>();
            translationStrings["StartDumpingButtonString"] = (string)Application.Current.FindResource("StartDumpingButtonString");
            translationStrings["StopDumpingButtonString"] = (string)Application.Current.FindResource("StopDumpingButtonString");
            MainViewModel.TranslateStrings(translationStrings);

            // Set interface language according to the options
            SetInterfaceLanguage(MainViewModel.Options.DefaultInterfaceLanguage);

            // Set the UI color scheme according to the options
            ApplyTheme();

            // Hide or show the media type box based on program
            SetMediaTypeVisibility();

            // Check for updates, if necessary
            if (MainViewModel.Options.CheckForUpdatesOnStartup)
                CheckForUpdates(showIfSame: false);

            // Handle first-run, if necessary
            if (MainViewModel.Options.FirstRun)
            {
                // Show the options window
                ShowOptionsWindow((string)Application.Current.FindResource("OptionsFirstRunTitleString"));
            }
        }

        #region Interface Language
        
        /// <summary>
        /// Set the current interface language to a provided InterfaceLanguage
        /// </summary>
        private void SetInterfaceLanguage(InterfaceLanguage lang)
        {
            // Uncheck all language menu items
            foreach (var item in LanguagesMenuItem.Items)
            {
                item.IsChecked = false;
            }

            // Auto detect language
            if (lang != InterfaceLanguage.AutoDetect)
            {
                AutoSetInterfaceLanguage();
                return;
            }

            // Set baseline language (English), required as some translations may not translate all strings
            if (lang != InterfaceLanguage.English)
            {
                var baselineDictionary = new ResourceDictionary();
                baselineDictionary.Source = new Uri("../Resources/Strings.xaml", UriKind.Relative);
                Application.Current.Resources.MergedDictionaries.Add(baselineDictionary);
            }

            var dictionary = new ResourceDictionary();
            dictionary.Source = lang switch
            {
                InterfaceLanguage.English => new Uri("../Resources/Strings.xaml", UriKind.Relative),
                InterfaceLanguage.Korean => new Uri("../Resources/Strings.ko.xaml", UriKind.Relative),
                _ => new Uri("../Resources/Strings.xaml", UriKind.Relative),
            };
            Application.Current.Resources.MergedDictionaries.Add(dictionary);

            // Update the labels in MainViewModel
            var translationStrings = new Dictionary<string, string>();
            translationStrings["StartDumpingButtonString"] = (string)Application.Current.FindResource("StartDumpingButtonString");
            translationStrings["StopDumpingButtonString"] = (string)Application.Current.FindResource("StopDumpingButtonString");
            translationStrings["NoSystemSelectedString"] = (string)Application.Current.FindResource("NoSystemSelectedString");
            MainViewModel.TranslateStrings(translationStrings);
        }

        /// <summary>
        /// Sets the interface language based on system locale
        /// Should only run at app startup, use SetInterfaceLanguage(lang) elsewhere
        /// </summary>
        public void AutoSetInterfaceLanguage()
        {
            // Get current region code to distinguish regional variants of languages
            string region = "";
            try
            {
                // Can throw exception depending on current locale
                region = new RegionInfo(CultureInfo.CurrentUICulture.Name).TwoLetterISORegionName;
            }
            catch { }

            switch (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
            {
                case "en":
                    // English already loaded, don't add any translated text
                    break;
                
                case "ko":
                    // Translate UI elements to Korean
                    SetInterfaceLanguage(InterfaceLanguage.Korean);
                    break;
                
                case "zh":
                    // Check if region uses Traditional or Simplified Chinese
                    string[] traditionalRegions = { "TW", "HK", "MO" };
                    if (Array.Exists(traditionalRegions, r => r.Equals(region, StringComparison.OrdinalIgnoreCase)))
                    {
                        // TODO: Translate UI elements to Traditional Chinese
                    }
                    else
                    {
                        // TODO: Translate UI elements to Simplified Chinese
                    }
                    break;
                
                default:
                    // Unsupported language, don't add any translated text
                    break;
            }
        }

        #endregion

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
            EnglishMenuItem!.Click += LanguageMenuItemClick;
            KoreanMenuItem!.Click += LanguageMenuItemClick;

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
            if (MainViewModel.Options.CopyUpdateUrlToClipboard && different && !string.IsNullOrEmpty(url))
            {
                try
                {
                    Clipboard.SetText(url);
                }
                catch { }
            }

            if (showIfSame || different)
                CustomMessageBox.Show(this, message, (string)Application.Current.FindResource("CheckForUpdatesTitleString"), MessageBoxButton.OK, different ? MessageBoxImage.Exclamation : MessageBoxImage.Information);
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
            _ = ShowMediaInformationWindow(MainViewModel.Options, ref submissionInfo);
            Formatter.ProcessSpecialFields(submissionInfo!);
        }

        /// <summary>
        /// Show the media information window
        /// </summary>
        /// <param name="options">Options set to pass to the information window</param>
        /// <param name="submissionInfo">SubmissionInfo object to display and possibly change</param>
        /// <returns>Dialog open result</returns>
        public bool? ShowMediaInformationWindow(Options? options, ref SubmissionInfo? submissionInfo)
        {
            if (options?.ShowDiscEjectReminder == true)
                CustomMessageBox.Show(this, "It is now safe to eject the disc", "Eject", MessageBoxButton.OK, MessageBoxImage.Information);

            var mediaInformationWindow = new MediaInformationWindow(options ?? new Options(), submissionInfo)
            {
                Focusable = true,
                Owner = this,
                ShowActivated = true,
                ShowInTaskbar = true,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };

            mediaInformationWindow.Closed += delegate { Activate(); };
            bool? result = mediaInformationWindow.ShowDialog();

            // Copy back the submission info changes, if necessary
            if (result == true)
                submissionInfo = (mediaInformationWindow.MediaInformationViewModel.SubmissionInfo.Clone() as SubmissionInfo)!;

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
                Title = title ?? (string)Application.Current.FindResource("OptionsTitleString"),
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };

            optionsWindow.Closed += delegate { Activate(); };
            optionsWindow.Closed += OnOptionsUpdated;
            optionsWindow.Show();
        }

        /// <summary>
        /// Set media type combo box visibility based on current program
        /// </summary>
        public void SetMediaTypeVisibility()
        {
            // Only DiscImageCreator uses the media type box
            if (MainViewModel.CurrentProgram != InternalProgram.DiscImageCreator)
            {
                SystemMediaTypeLabel!.Content = (string)Application.Current.FindResource("SystemLabelString");
                MediaTypeComboBox!.Visibility = Visibility.Hidden;
                return;
            }

            // If there are no media types defined
            if (MainViewModel.MediaTypes == null)
            {
                SystemMediaTypeLabel!.Content = (string)Application.Current.FindResource("SystemLabelString");
                MediaTypeComboBox!.Visibility = Visibility.Hidden;
                return;
            }

            // Only systems with more than one media type should show the box
            bool visible = MainViewModel.MediaTypes.Count > 1;
            SystemMediaTypeLabel!.Content = visible
                ? (string)Application.Current.FindResource("SystemMediaTypeLabelString")
                : (string)Application.Current.FindResource("SystemLabelString");
            MediaTypeComboBox!.Visibility = visible
                ? Visibility.Visible
                : Visibility.Hidden;
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

        /// <summary>
        /// Build the about text 
        /// </summary>
        /// <returns></returns>
        public string CreateAboutText()
        {
            string aboutText = $"{(string)Application.Current.FindResource("AppTitleFullString")}"
                + $"{Environment.NewLine}"
                + $"{Environment.NewLine}{(string)Application.Current.FindResource("AboutLine1String")}"
                + $"{Environment.NewLine}{(string)Application.Current.FindResource("AboutLine2String")}"
                + $"{Environment.NewLine}{(string)Application.Current.FindResource("AboutLine3String")}"
                + $"{Environment.NewLine}"
                + $"{Environment.NewLine}{(string)Application.Current.FindResource("AboutThanksString")}"
                + $"{Environment.NewLine}"
                + $"{Environment.NewLine}{(string)Application.Current.FindResource("VersionLabelString")} {FrontendTool.GetCurrentVersion()}";
            MainViewModel.LogAboutText(aboutText);
            return aboutText;
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

            // Set the language according to the settings
            SetInterfaceLanguage(MainViewModel.Options.DefaultInterfaceLanguage);

            // Set the UI color scheme according to the options
            ApplyTheme();

            // Hide or show the media type box based on program
            SetMediaTypeVisibility();
        }

        #region Menu Bar

        /// <summary>
        /// Handler for AboutMenuItem Click event
        /// </summary>
        public void AboutClick(object sender, RoutedEventArgs e)
        {
            string aboutText = CreateAboutText();
            CustomMessageBox.Show(this, aboutText, (string)Application.Current.FindResource("AboutTitleString"), MessageBoxButton.OK, MessageBoxImage.Information);
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

        /// <summary>
        /// Change UI language
        /// </summary>
        private void LanguageMenuItemClick(object sender, RoutedEventArgs e)
        {
            // Don't do anything if language is already checked and being unchecked
            var clickedItem = (MenuItem)sender;
            if (!clickedItem.IsChecked)
            {
                clickedItem.IsChecked = true;
                return;
            }

            // Change UI language to selected item
            string lang = clickedItem.Header.ToString() ?? "";
            SetInterfaceLanguage(
                lang : lang switch
                {
                    "ENG" => InterfaceLanguage.English,
                    "한국어" => InterfaceLanguage.Korean,
                    _ => InterfaceLanguage.English,
                }
            );

            // Update the labels that don't get updated automatically
            SetMediaTypeVisibility();
        }

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
                MainViewModel.EnsureMediaInformation();
        }

        /// <summary>
        /// Handler for DumpingProgramComboBox SelectionChanged event
        /// </summary>
        public void DumpingProgramComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
            {
                MainViewModel.ChangeDumpingProgram();
                SetMediaTypeVisibility();
            }
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
            MainViewModel.EnsureMediaInformation();
        }

        /// <summary>
        /// Handler for OutputPathTextBox TextChanged event
        /// </summary>
        public void OutputPathTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
                MainViewModel.EnsureMediaInformation();
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
            {
                MainViewModel.ChangeSystem();
                SetMediaTypeVisibility();
            }
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
