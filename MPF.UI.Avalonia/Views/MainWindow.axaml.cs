using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MPF.Frontend;
using MPF.Frontend.Tools;
using MPF.Frontend.ViewModels;
using MPF.UI.Avalonia.Services;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;

namespace MPF.UI.Avalonia.Views
{
    public partial class MainWindow : WindowBase
    {
        /// <summary>
        /// Read-only access to the current main view model
        /// </summary>
        public MainViewModel MainViewModel { get; }

        /// <summary>
        /// Service that bridges the synchronous DisplayUserMessage delegate to the async message box
        /// </summary>
        private readonly DialogService _dialogService;

        /// <summary>
        /// Set to true once OnLoaded initialization has run, to avoid running it twice
        /// </summary>
        private bool _initialized;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            MainViewModel = new MainViewModel();
            DataContext = MainViewModel;

            _dialogService = new DialogService(() => this);

            this.Closing += MainWindowClosing;

            // Set all resources before window loads (Must set English here)
            SetInterfaceLanguage(InterfaceLanguage.English);
        }

        /// <summary>
        /// Handler for MainWindow Loaded event (replaces WPF OnContentRendered)
        /// </summary>
        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            // Only run the initialization once
            if (_initialized)
                return;
            _initialized = true;

            // Disable buttons until we load fully
            MainViewModel.StartStopButtonEnabled = false;
            MainViewModel.MediaScanButtonEnabled = false;
            MainViewModel.UpdateVolumeLabelEnabled = false;
            MainViewModel.CopyProtectScanButtonEnabled = false;

            // Display the debug option in the menu, if necessary
            if (MainViewModel.Options.GUI.ShowDebugViewMenuItem)
                DebugViewMenuItem.IsVisible = true;

            MainViewModel.Init(LogOutput.EnqueueLog, _dialogService.DisplayUserMessage, ShowMediaInformationWindow);

            // Pass translation strings to MainViewModel
            var translationStrings = new Dictionary<string, string>
            {
                ["StartDumpingButtonString"] = FindResourceString("StartDumpingButtonString"),
                ["StopDumpingButtonString"] = FindResourceString("StopDumpingButtonString")
            };
            MainViewModel.TranslateStrings(translationStrings);

            // Set interface language according to the options
            SetInterfaceLanguage(MainViewModel.Options.GUI.DefaultInterfaceLanguage);

            // Set the UI color scheme according to the options
            ApplyTheme();

            // Hide or show the media type box based on program
            SetMediaTypeVisibility();

            // Check for updates, if necessary
            if (MainViewModel.Options.CheckForUpdatesOnStartup)
                _ = CheckForUpdates(showIfSame: false);

            // Handle first-run, if necessary
            if (MainViewModel.Options.FirstRun)
            {
                // Show the options window
                ShowOptionsWindow(FindResourceString("OptionsFirstRunTitleString"));
            }
        }

        #region Interface Language

        /// <summary>
        /// Set the current interface language to a provided InterfaceLanguage
        /// </summary>
        /// <remarks>
        /// TODO(Task 3 followup): only the English string dictionary (Strings.axaml) has been ported
        /// to the Avalonia project so far. Non-English dictionaries (Strings.de.axaml, etc.) do not yet
        /// exist as Avalonia resources, so this method currently only keeps the ViewModel translation
        /// strings in sync. When the localized dictionaries are ported, merge the chosen one into
        /// Application.Current.Resources here (mirroring the WPF MergedDictionaries logic).
        /// </remarks>
        private void SetInterfaceLanguage(InterfaceLanguage lang)
        {
            // Auto detect language
            if (lang == InterfaceLanguage.AutoDetect)
            {
                AutoSetInterfaceLanguage();
                return;
            }

            // Update the labels in MainViewModel from whatever resources are currently loaded
            var translationStrings = new Dictionary<string, string>
            {
                ["StartDumpingButtonString"] = FindResourceString("StartDumpingButtonString"),
                ["StopDumpingButtonString"] = FindResourceString("StopDumpingButtonString"),
                ["NoSystemSelectedString"] = FindResourceString("NoSystemSelectedString")
            };
            MainViewModel.TranslateStrings(translationStrings);
        }

        /// <summary>
        /// Sets the interface language based on system locale
        /// Should only run at app startup, use SetInterfaceLanguage(lang) elsewhere
        /// </summary>
        public void AutoSetInterfaceLanguage()
        {
            // Get current region code to distinguish regional variants of languages
            string region = string.Empty;
            try
            {
                // Can throw exception depending on current locale
                region = new RegionInfo(CultureInfo.CurrentUICulture.Name).TwoLetterISORegionName;
            }
            catch { }

            switch (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
            {
                case "en": SetInterfaceLanguage(InterfaceLanguage.English); break;
                case "fr": SetInterfaceLanguage(InterfaceLanguage.French); break;
                case "de": SetInterfaceLanguage(InterfaceLanguage.German); break;
                case "it": SetInterfaceLanguage(InterfaceLanguage.Italian); break;
                case "ja": SetInterfaceLanguage(InterfaceLanguage.Japanese); break;
                case "ko": SetInterfaceLanguage(InterfaceLanguage.Korean); break;
                case "pl": SetInterfaceLanguage(InterfaceLanguage.Polish); break;
                case "pt": SetInterfaceLanguage(InterfaceLanguage.Portuguese); break;
                case "ru": SetInterfaceLanguage(InterfaceLanguage.Russian); break;
                case "es": SetInterfaceLanguage(InterfaceLanguage.Spanish); break;
                case "sv": SetInterfaceLanguage(InterfaceLanguage.Swedish); break;
                case "uk": SetInterfaceLanguage(InterfaceLanguage.Ukrainian); break;

                // Traditional or Simplified Chinese
                case "zh":
                    string[] traditionalRegions = ["TW", "HK", "MO"];
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
        /// Look up a string resource by key, returning an empty string if it is missing.
        /// Replaces WPF's <c>(string)Application.Current.FindResource("X")</c>.
        /// </summary>
        private string FindResourceString(string key)
            => this.TryFindResource(key, out var value) && value is string s ? s : string.Empty;

        /// <summary>
        /// Browse for an output file path
        /// </summary>
        public async System.Threading.Tasks.Task BrowseFile()
        {
            // Get the current path, if possible
            string currentPath = MainViewModel.OutputPath;
            if (string.IsNullOrEmpty(currentPath) && !string.IsNullOrEmpty(MainViewModel.Options.Dumping.DefaultOutputPath))
                currentPath = Path.Combine(MainViewModel.Options.Dumping.DefaultOutputPath, $"track_{DateTime.Now:yyyyMMdd-HHmm}.bin");
            else if (string.IsNullOrEmpty(currentPath))
                currentPath = $"track_{DateTime.Now:yyyyMMdd-HHmm}.bin";
            if (string.IsNullOrEmpty(currentPath))
                currentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, $"track_{DateTime.Now:yyyyMMdd-HHmm}.bin");

            // Get the full path
            currentPath = Path.GetFullPath(currentPath);

            // Get the filename
            string filename = Path.GetFileName(currentPath);

            string? selectedPath = await StorageDialogs.SaveFileAsync(this, FindResourceString("BrowseButtonString"), filename);
            if (!string.IsNullOrEmpty(selectedPath))
                MainViewModel.OutputPath = selectedPath!;
        }

        /// <summary>
        /// Check for available updates
        /// </summary>
        /// <param name="showIfSame">True to show the box even if it's the same, false to only show if it's different</param>
        public async System.Threading.Tasks.Task CheckForUpdates(bool showIfSame)
        {
            MainViewModel.CheckForUpdates(out bool different, out string message, out var url);
            if (different)
                message += $"{Environment.NewLine}The update URL has been added copied to your clipboard";
            else
                message += $"{Environment.NewLine}You have the newest version!";

            // If we have a new version, put it in the clipboard
            if (MainViewModel.Options.GUI.CopyUpdateUrlToClipboard && different && !string.IsNullOrEmpty(url))
            {
                try
                {
                    IClipboard? clipboard = Clipboard;
                    if (clipboard is not null)
                        await clipboard.SetTextAsync(url);
                }
                catch { }
            }

            if (showIfSame || different)
                await MessageBoxWindow.ShowAsync(this, FindResourceString("CheckForUpdatesTitleString"), message, MessageBoxButtons.Ok);
        }

        /// <summary>
        /// Ask to confirm quitting, when an operation is running
        /// </summary>
        public async void MainWindowClosing(object? sender, WindowClosingEventArgs e)
        {
            if (MainViewModel.AskBeforeQuit)
            {
                // Cancel the close while we ask; if confirmed, close programmatically
                e.Cancel = true;
                bool? result = await MessageBoxWindow.ShowAsync(this,
                    FindResourceString("QuitTitleString"), FindResourceString("QuitMessageString"), MessageBoxButtons.YesNo);
                if (result == true)
                {
                    // Don't ask again on the second pass
                    MainViewModel.AskBeforeQuit = false;
                    Close();
                }
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
        /// <remarks>This implements <see cref="ProcessUserInfoDelegate"/> (synchronous, ref param).</remarks>
        public bool? ShowMediaInformationWindow(Options? options, ref SubmissionInfo? submissionInfo)
        {
            // ProcessUserInfoDelegate is synchronous and uses a ref param, but it is invoked from
            // a background (dumping) thread. Bridge to the UI thread and block the caller. A ref
            // cannot be captured in a lambda, so copy the value in and out.
            var opts = options ?? new Options();
            var info = submissionInfo;
            var (result, updated) = global::Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (opts.Processing?.ShowDiscEjectReminder == true)
                {
                    await MessageBoxWindow.ShowAsync(this,
                        FindResourceString("EjectTitleString"),
                        FindResourceString("EjectMessageString"),
                        MessageBoxButtons.Ok);
                }

                var win = new MediaInformationWindow(opts, info);
                bool? r = await win.ShowDialog<bool?>(this);

                SubmissionInfo? outInfo = info;
                if (r == true)
                    outInfo = win.MediaInformationViewModel.SubmissionInfo.Clone() as SubmissionInfo;

                return (r, outInfo);
            }).GetAwaiter().GetResult();

            submissionInfo = updated;
            return result;
        }

        /// <summary>
        /// Show the Check Dump window.
        /// Mirrors WPF behavior: hide MainWindow while CheckDump is open, re-show when it closes.
        /// </summary>
        public void ShowCheckDumpWindow()
        {
            Hide();
            var win = new CheckDumpWindow(this);
            win.Closed += (_, _) =>
            {
                Show();
                Activate();
            };
            win.Show();
        }

        /// <summary>
        /// Show the Create IRD window
        /// </summary>
        public async void ShowCreateIRDWindow()
        {
            // TODO(Task 12): construct and show the real CreateIRDWindow.
            await MessageBoxWindow.ShowAsync(this, FindResourceString("CreatePS3IRDDumpMenuItemString"),
                "This window is not yet available in the macOS UI.", MessageBoxButtons.Ok);
        }

        /// <summary>
        /// Show the Options window modally and apply any saved changes.
        /// Replaces WPF's non-modal Show() + Closed event approach with modal ShowDialog.
        /// </summary>
        public async void ShowOptionsWindow(string? title = null)
        {
            var optionsWindow = new OptionsWindow(MainViewModel.Options)
            {
                Title = title ?? FindResourceString("OptionsTitleString"),
            };

            await optionsWindow.ShowDialog(this);

            OnOptionsUpdated(optionsWindow);
        }

        /// <summary>
        /// Apply changes from a closed OptionsWindow to the MainViewModel.
        /// Ported from WPF MainWindow.OnOptionsUpdated (formerly wired as a Closed event handler).
        /// </summary>
        private void OnOptionsUpdated(OptionsWindow optionsWindow)
        {
            var ovm = optionsWindow.OptionsViewModel;

            bool savedSettings = ovm.SavedSettings;
            var options = ovm.Options;

            // Force a refresh of the output path if the default changed
            if (MainViewModel.Options.Dumping.DefaultOutputPath != options.Dumping.DefaultOutputPath)
                MainViewModel.OutputPath = string.Empty;

            // Apply language change when settings were saved
            if (savedSettings)
            {
                var oldDefaultLang = MainViewModel.Options.GUI.DefaultInterfaceLanguage;
                var newDefaultLang = options.GUI.DefaultInterfaceLanguage;
                if (oldDefaultLang != newDefaultLang)
                {
                    SetInterfaceLanguage(newDefaultLang);

                    // Uncheck all language menu items (no language is "active" from the options)
                    foreach (var item in LanguagesMenuItem.Items)
                    {
                        if (item is MenuItem menuItem)
                            menuItem.IsChecked = false;
                    }
                }
            }

            // Update and save options, if necessary
            MainViewModel.UpdateOptions(savedSettings, options);

            // Re-apply theme and media-type visibility
            ApplyTheme();
            SetMediaTypeVisibility();
        }

        /// <summary>
        /// Set media type combo box visibility based on current program
        /// </summary>
        public void SetMediaTypeVisibility()
        {
            // Only DiscImageCreator uses the media type box
            if (MainViewModel.CurrentProgram != InternalProgram.DiscImageCreator)
            {
                SystemMediaTypeLabel.Text = FindResourceString("SystemLabelString");
                MediaTypeComboBox.IsVisible = false;
                return;
            }

            // If there are no media types defined
            if (MainViewModel.MediaTypes is null)
            {
                SystemMediaTypeLabel.Text = FindResourceString("SystemLabelString");
                MediaTypeComboBox.IsVisible = false;
                return;
            }

            // Only systems with more than one media type should show the box
            bool visible = MainViewModel.MediaTypes.Count > 1;
            SystemMediaTypeLabel.Text = visible
                ? FindResourceString("SystemMediaTypeLabelString")
                : FindResourceString("SystemLabelString");
            MediaTypeComboBox.IsVisible = visible;
        }

        /// <summary>
        /// Set the UI color scheme according to the options
        /// </summary>
        /// <remarks>
        /// TODO(Task 8 followup): the WPF code instantiated DarkModeTheme/LightModeTheme/CustomTheme
        /// (concrete Theme subclasses that do not yet exist in the Avalonia project). The ported
        /// <see cref="Theme"/> base class is abstract and the brush keys have safe light-theme defaults,
        /// so theme application is currently a no-op. Port the concrete themes in a later task and
        /// replicate the option-based selection (EnableDarkMode / EnablePurpMode / custom hex colors).
        /// </remarks>
        private void ApplyTheme()
        {
            // No concrete Theme subclasses ported yet; defaults from Brushes.axaml apply.
        }

        /// <summary>
        /// Check whether a string represents a valid hex color
        /// </summary>
        public static bool IsHexColor(string? color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return false;

            if (color!.Length == 7 && color[0] == '#')
                color = color[1..];

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
        public string CreateAboutText()
        {
            string aboutText = $"{FindResourceString("AppTitleFullString")}"
                + $"{Environment.NewLine}"
                + $"{Environment.NewLine}{FindResourceString("AboutLine1String")}"
                + $"{Environment.NewLine}{FindResourceString("AboutLine2String")}"
                + $"{Environment.NewLine}{FindResourceString("AboutLine3String")}"
                + $"{Environment.NewLine}"
                + $"{Environment.NewLine}{FindResourceString("AboutThanksString")}"
                + $"{Environment.NewLine}macOS UI: Oliver Köster"
                + $"{Environment.NewLine}"
                + $"{Environment.NewLine}{FindResourceString("VersionLabelString")} {FrontendTool.GetCurrentVersion()}";
            MainViewModel.LogAboutText(aboutText);
            return aboutText;
        }

        #endregion

        #region Event Handlers

        #region Menu Bar

        /// <summary>
        /// Handler for AboutMenuItem Click event
        /// </summary>
        public async void AboutClick(object? sender, RoutedEventArgs e)
        {
            string aboutText = CreateAboutText();
            await MessageBoxWindow.ShowAsync(this, FindResourceString("AboutTitleString"), aboutText, MessageBoxButtons.Ok);
        }

        /// <summary>
        /// Handler for AppExitMenuItem Click event
        /// </summary>
        public void AppExitClick(object? sender, RoutedEventArgs e)
        {
            if (global::Avalonia.Application.Current?.ApplicationLifetime
                is global::Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                desktop.Shutdown();
            else
                Close();
        }

        /// <summary>
        /// Handler for CheckDumpMenuItem Click event
        /// </summary>
        public void CheckDumpMenuItemClick(object? sender, RoutedEventArgs e)
            => ShowCheckDumpWindow();

        /// <summary>
        /// Handler for CreateIRDMenuItem Click event
        /// </summary>
        public void CreateIRDMenuItemClick(object? sender, RoutedEventArgs e)
            => ShowCreateIRDWindow();

        /// <summary>
        /// Handler for CheckForUpdatesMenuItem Click event
        /// </summary>
        public async void CheckForUpdatesClick(object? sender, RoutedEventArgs e)
            => await CheckForUpdates(showIfSame: true);

        /// <summary>
        /// Handler for DebugViewMenuItem Click event
        /// </summary>
        public void DebugViewClick(object? sender, RoutedEventArgs e)
            => ShowDebugDiscInfoWindow();

        /// <summary>
        /// Handler for OptionsMenuItem Click event
        /// </summary>
        public void OptionsMenuItemClick(object? sender, RoutedEventArgs e)
            => ShowOptionsWindow();

        /// <summary>
        /// Change UI language
        /// </summary>
        private void LanguageMenuItemClick(object? sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem clickedItem)
                return;

            // Uncheck every sibling item and check the clicked one
            if (clickedItem.Parent is MenuItem languageMenu)
            {
                foreach (var item in languageMenu.Items)
                {
                    if (item is MenuItem menuItem)
                        menuItem.IsChecked = menuItem == clickedItem;
                }
            }

            // Change UI language to selected item
            string lang = clickedItem.Header?.ToString() ?? string.Empty;
            SetInterfaceLanguage(EnumExtensions.ToInterfaceLanguage(lang));

            // Update the labels that don't get updated automatically
            SetMediaTypeVisibility();
        }

        #endregion

        #region User Area

        /// <summary>
        /// Handler for CopyProtectScanButton Click event
        /// </summary>
        public async void CopyProtectScanButtonClick(object? sender, RoutedEventArgs e)
        {
            var output = await MainViewModel.ScanAndShowProtection();

            if (!MainViewModel.LogPanelExpanded)
            {
                if (!string.IsNullOrEmpty(output))
                    await MessageBoxWindow.ShowAsync(this, FindResourceString("ProtectionDetectedTitleString"),
                        output!, MessageBoxButtons.Ok);
                else
                    await MessageBoxWindow.ShowAsync(this, FindResourceString("ProtectionErrorTitleString"),
                        FindResourceString("ProtectionErrorMessageString"), MessageBoxButtons.Ok);
            }
        }

        /// <summary>
        /// Handler for DriveLetterComboBox SelectionChanged event
        /// </summary>
        public void DriveLetterComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
                MainViewModel.InitializeUIValues(removeEventHandlers: true, rebuildPrograms: false, rescanDrives: false);
        }

        /// <summary>
        /// Handler for DriveSpeedComboBox SelectionChanged event
        /// </summary>
        public void DriveSpeedComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
                MainViewModel.EnsureMediaInformation();
        }

        /// <summary>
        /// Handler for DumpingProgramComboBox SelectionChanged event
        /// </summary>
        public void DumpingProgramComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
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
        public void EnableParametersCheckBoxClick(object? sender, RoutedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
                MainViewModel.ToggleParameters();
        }

        /// <summary>
        /// Handler for MediaScanButton Click event
        /// </summary>
        public void MediaScanButtonClick(object? sender, RoutedEventArgs e)
            => MainViewModel.InitializeUIValues(removeEventHandlers: true, rebuildPrograms: false, rescanDrives: true);

        /// <summary>
        /// Handler for MediaTypeComboBox SelectionChanged event
        /// </summary>
        public void MediaTypeComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
                MainViewModel.ChangeMediaType(e.RemovedItems, e.AddedItems);
        }

        /// <summary>
        /// Handler for OutputPathBrowseButton Click event
        /// </summary>
        public async void OutputPathBrowseButtonClick(object? sender, RoutedEventArgs e)
        {
            await BrowseFile();
            MainViewModel.EnsureMediaInformation();
        }

        /// <summary>
        /// Handler for OutputPathTextBox TextChanged event
        /// </summary>
        public void OutputPathTextBoxTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
                MainViewModel.EnsureMediaInformation();
        }

        /// <summary>
        /// Handler for StartStopButton Click event
        /// </summary>
        public void StartStopButtonClick(object? sender, RoutedEventArgs e)
            => MainViewModel.ToggleStartStop();

        /// <summary>
        /// Handler for SystemTypeComboBox SelectionChanged event
        /// </summary>
        public void SystemTypeComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
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
        public void UpdateVolumeLabelClick(object? sender, RoutedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
            {
                if (MainViewModel.Options.GUI.FastUpdateLabel)
                    MainViewModel.FastUpdateLabel(removeEventHandlers: true);
                else
                    MainViewModel.InitializeUIValues(removeEventHandlers: true, rebuildPrograms: false, rescanDrives: false);
            }
        }

        #endregion

        #endregion // Event Handlers
    }
}
