using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MPF.Avalonia.Services;
using MPF.Avalonia.UserControls;
using MPF.Frontend;
using MPF.Frontend.Tools;
using MPF.Frontend.ViewModels;
using SabreTools.RedumpLib.Data;
using SabreTools.RedumpLib.Tools;
using AvaloniaWindowState = Avalonia.Controls.WindowState;

namespace MPF.Avalonia.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.axaml
    /// </summary>
    public partial class MainWindow : WindowBase
    {
        #region Fields

        /// <summary>
        /// Extra window height added when the log panel is expanded
        /// </summary>
        private const double ExpandedLogPanelExtraHeight = 40;

        /// <summary>
        /// File picker types for browsing output files
        /// </summary>
        private static readonly List<FilePickerFileType> OutputFileTypes =
        [
            new("All Files") { Patterns = ["*"] },
        ];

        /// <summary>
        /// Whether the window may close without prompting the user
        /// </summary>
        private bool _allowCloseWithoutPrompt;

        /// <summary>
        /// Whether a close confirmation dialog is currently pending
        /// </summary>
        private bool _closeConfirmationPending;

        /// <summary>
        /// Last recorded height of the log panel, used to compute window resizing
        /// </summary>
        private double _lastLogPanelHeight;

        /// <summary>
        /// Window height while the log panel is expanded
        /// </summary>
        private double _expandedWindowHeight;

        /// <summary>
        /// Window height while the log panel is collapsed
        /// </summary>
        private double _collapsedWindowHeight;

        /// <summary>
        /// Whether the log panel resize logic has been initialized
        /// </summary>
        private bool _logPanelResizeInitialized;

        #endregion

        /// <summary>
        /// Read-only access to the current main view model
        /// </summary>
        public MainViewModel MainViewModel => DataContext as MainViewModel ?? new MainViewModel();

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            ConfigurePlatformChrome();

            DataContext = new MainViewModel();
            MainViewModel.PropertyChanged += OnMainViewModelPropertyChanged;
            ThemeService.SyncWithSystemTheme(MainViewModel.Options);

            // Set all resources before window loads (Must set English here)
            SetInterfaceLanguage(InterfaceLanguage.English, rebuildNativeMenus: false);

            Opened += OnOpened;
            Closing += MainWindowClosing;
            SizeChanged += OnMainWindowSizeChanged;
        }

        /// <summary>
        /// Handler for the window Opened event; wires up events and performs first-load setup
        /// </summary>
        private void OnOpened(object? sender, EventArgs e)
        {
            // Disable buttons until we load fully
            MainViewModel.StartStopButtonEnabled = false;
            MainViewModel.MediaScanButtonEnabled = false;
            MainViewModel.UpdateVolumeLabelEnabled = false;
            MainViewModel.CopyProtectScanButtonEnabled = false;

            // Add the click handlers to the UI
            AddEventHandlers();

            // Display the debug option in the menu, if necessary
            if (MainViewModel.Options.GUI.ShowDebugViewMenuItem)
                DebugViewMenuItem!.IsVisible = true;

            // On Linux the dumping tool has no console window of its own, so stream its
            // live output into a separate MPF window. Windows/macOS keep the tool's own
            // console (leave this null), preserving the original behavior.
            IToolOutputConsole? toolConsole = OperatingSystem.IsLinux()
                ? new ToolOutputConsole(this, MainViewModel.Options)
                : null;

            MainViewModel.Init(
                LogOutput!.EnqueueLog,
                DisplayUserMessage,
                ShowMediaInformationWindow,
                toolConsole);

            // Pass translation strings to MainViewModel
            var translationStrings = new Dictionary<string, string>
            {
                ["StartDumpingButtonString"] = StringResource("StartDumpingButtonString", "Start Dumping"),
                ["StopDumpingButtonString"] = StringResource("StopDumpingButtonString", "Stop Dumping")
            };
            MainViewModel.TranslateStrings(translationStrings);

            // Set interface language according to the options
            SetInterfaceLanguage(MainViewModel.Options.GUI.DefaultInterfaceLanguage, rebuildNativeMenus: false);
            ConfigurePlatformMenus();

            // Set the UI color scheme according to the options
            ThemeService.Apply(Application.Current!.Resources, MainViewModel.Options);

            // Hide or show the media type box based on program
            SetMediaTypeVisibility();
            WireLogPanelResizing();

            // Check for updates, if necessary
            if (MainViewModel.Options.CheckForUpdatesOnStartup)
                CheckForUpdatesClick(this, new RoutedEventArgs());

            // Handle first-run, if necessary
            if (MainViewModel.Options.FirstRun)
            {
                // Show the options window
                ShowOptionsWindow(StringResource("OptionsFirstRunTitleString", "Welcome to MPF, Explore the Options"));
            }
        }

        #region Platform Chrome and Menus

        /// <summary>
        /// Configure window decorations and title bar layout for the current operating system
        /// </summary>
        private void ConfigurePlatformChrome()
        {
            if (OperatingSystem.IsWindows())
                return;

            SystemDecorations = SystemDecorations.Full;
            if (OperatingSystem.IsMacOS())
            {
                TopMenuBar!.IsVisible = false;
                RootGrid!.RowDefinitions[0].Height = new GridLength(0);
            }
            else
            {
                AppTitleLabel!.IsVisible = false;
                TitleDragArea!.IsVisible = false;
                CloseButton!.IsVisible = false;
            }

            RootBorder!.Padding = new Thickness(2, OperatingSystem.IsMacOS() ? 4 : 2, 2, 8);
        }

        /// <summary>
        /// Refresh the in-window title bar menu headers to the current interface language
        /// </summary>
        /// TODO: Can this be handled by binding?
        private void UpdateTitleBarMenuHeaders()
        {
            FileMenuItem!.Header = StringResource("FileMenuString", "File");
            ToolsMenuItem!.Header = StringResource("ToolsMenuString", "Tools");
            HelpMenuItem!.Header = StringResource("HelpMenuString", "Help");
            LanguagesMenuItem!.Header = StringResource("LanguageMenuString", "ENG");
        }

        #endregion

        #region Interface Language

        /// <summary>
        /// Set the current interface language and refresh all localized menu headers
        /// </summary>
        private void SetInterfaceLanguage(InterfaceLanguage lang, bool rebuildNativeMenus)
        {
            StringResourceLoader.Load(Application.Current!.Resources, lang);
            StringResourceLoader.Load(Resources, lang);

            // Update the labels in MainViewModel
            var translationStrings = new Dictionary<string, string>
            {
                ["StartDumpingButtonString"] = StringResource("StartDumpingButtonString", "Start Dumping"),
                ["StopDumpingButtonString"] = StringResource("StopDumpingButtonString", "Stop Dumping"),
                ["NoSystemSelectedString"] = StringResource("NoSystemSelectedString", "No System Selected")
            };
            MainViewModel.TranslateStrings(translationStrings);

            UpdateTitleBarMenuHeaders();
            if (rebuildNativeMenus)
                UpdateNativeMenuHeaders();
        }

        #endregion

        #region Title Bar

        /// <summary>
        /// Handler for the custom title bar pointer press; drags or toggles the window
        /// </summary>
        private void TitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            PointerPoint point = e.GetCurrentPoint(this);
            if (!point.Properties.IsLeftButtonPressed)
                return;

            if (e.ClickCount == 2)
            {
                ToggleWindowState();
                return;
            }

            BeginMoveDrag(e);
        }

        /// <summary>
        /// Toggle the window between maximized and normal states
        /// </summary>
        private void ToggleWindowState()
            => WindowState = WindowState == AvaloniaWindowState.Maximized
                ? AvaloniaWindowState.Normal
                : AvaloniaWindowState.Maximized;

        #endregion

        #region UI Functionality

        /// <summary>
        /// Add all event handlers
        /// </summary>
        private void AddEventHandlers()
        {
            // Menu Bar Click
            AboutMenuItem!.Click += AboutClick;
            AppExitMenuItem!.Click += AppExitClick;
            CheckForUpdatesMenuItem!.Click += CheckForUpdatesClick;
            DebugViewMenuItem!.Click += DebugViewClick;
            CheckDumpMenuItem!.Click += CheckDumpMenuItemClick;
            CreateIRDMenuItem!.Click += CreateIRDMenuItemClick;
            OptionsMenuItem!.Click += OptionsMenuItemClick;

            // Languages dropdown
            EnglishMenuItem!.Click += LanguageMenuItemClick;
            FrenchMenuItem!.Click += LanguageMenuItemClick;
            GermanMenuItem!.Click += LanguageMenuItemClick;
            ItalianMenuItem!.Click += LanguageMenuItemClick;
            JapaneseMenuItem!.Click += LanguageMenuItemClick;
            KoreanMenuItem!.Click += LanguageMenuItemClick;
            PolishMenuItem!.Click += LanguageMenuItemClick;
            PortugueseMenuItem!.Click += LanguageMenuItemClick;
            RussianMenuItem!.Click += LanguageMenuItemClick;
            SpanishMenuItem!.Click += LanguageMenuItemClick;
            SwedishMenuItem!.Click += LanguageMenuItemClick;
            UkrainianMenuItem!.Click += LanguageMenuItemClick;

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
        private async Task<string?> BrowseOutputFileAsync()
        {
            // Get the current path, if possible
            string currentPath = MainViewModel.OutputPath;
            if (string.IsNullOrEmpty(currentPath) && !string.IsNullOrEmpty(MainViewModel.Options.Dumping.DefaultOutputPath))
                currentPath = Path.Combine(MainViewModel.Options.Dumping.DefaultOutputPath, $"track_{DateTime.Now:yyyyMMdd-HHmm}.bin");
            else if (string.IsNullOrEmpty(currentPath))
                currentPath = $"track_{DateTime.Now:yyyyMMdd-HHmm}.bin";
            if (string.IsNullOrEmpty(currentPath))
                currentPath = Path.Combine(AppContext.BaseDirectory, $"track_{DateTime.Now:yyyyMMdd-HHmm}.bin");

            // Get the full path
            currentPath = Path.GetFullPath(currentPath);

            // Get the filename
            string filename = Path.GetFileName(currentPath);
            if (string.IsNullOrEmpty(filename))
                filename = $"track_{DateTime.Now:yyyyMMdd-HHmm}.bin";

            return await DialogService.SaveFileAsync(
                this,
                StringResource("OutputPathLabelString", "Output Path"),
                filename,
                OutputFileTypes);
        }

        /// <summary>
        /// Check for available updates
        /// </summary>
        /// <param name="showIfSame">True to show the box even if it's the same, false to only show if it's different</param>
        public void CheckForUpdates(bool showIfSame)
        {
            MainViewModel.CheckForUpdates(out bool different, out string message, out var url);
            if (different && MainViewModel.Options.GUI.CopyUpdateUrlToClipboard)
                message += $"{Environment.NewLine}The update URL has been added copied to your clipboard";
            else if (different && !MainViewModel.Options.GUI.CopyUpdateUrlToClipboard)
                message += $"{Environment.NewLine}You are out of date!";
            else
                message += $"{Environment.NewLine}You have the newest version!";

            // If we have a new version, put it in the clipboard
            if (MainViewModel.Options.GUI.CopyUpdateUrlToClipboard && different && !string.IsNullOrEmpty(url))
            {
                try
                {
                    Clipboard?.SetTextAsync(url);
                }
                catch { }
            }

            if (showIfSame || different)
                MessageBoxWindow.ShowAsync(this, StringResource("CheckForUpdatesTitleString", "Check for Updates"), message, 1, different);
        }

        /// <summary>
        /// Ask to confirm quitting, when an operation is running
        /// </summary>
        public void MainWindowClosing(object? sender, WindowClosingEventArgs e)
        {
            if (_allowCloseWithoutPrompt)
                return;

            if (!MainViewModel.AskBeforeQuit)
                return;

            e.Cancel = true;
            if (!_closeConfirmationPending)
                _ = ConfirmCloseAsync();
        }

        /// <summary>
        /// Handler for the window SizeChanged event; keeps the log console height in sync
        /// </summary>
        public void OnMainWindowSizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (!_logPanelResizeInitialized)
                return;

            UpdateLogConsoleHeight();
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
            if (options?.Processing?.ShowDiscEjectReminder == true)
            {
                MessageBoxWindow.ShowAsync(this,
                    StringResource("EjectTitleString", "Eject"),
                    StringResource("EjectMessageString", "It is now safe to eject the disc"),
                    1,
                    true);
            }

            // TODO: Determine the real difference between these paths
            if (Dispatcher.UIThread.CheckAccess())
            {
                var window = new MediaInformationWindow(options ?? MainViewModel.Options, submissionInfo)
                {
                    Focusable = true,
                    ShowActivated = true,
                    ShowInTaskbar = true,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };

                window.Closed += delegate { Activate(); };

                Task<bool?> dialogTask = window.ShowDialog<bool?>(this);
                var frame = new DispatcherFrame();
                dialogTask.ContinueWith(_ => Dispatcher.UIThread.Post(() => frame.Continue = false));
                Dispatcher.UIThread.PushFrame(frame);

                bool? result = dialogTask.GetAwaiter().GetResult();
                if (result == true)
                    submissionInfo = (window.MediaInformationViewModel.SubmissionInfo.Clone() as SubmissionInfo)!;

                return result;
            }
            else
            {
                SubmissionInfo? updatedSubmissionInfo = submissionInfo;
                var dispatcherTask = Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    var window = new MediaInformationWindow(options ?? MainViewModel.Options, updatedSubmissionInfo)
                    {
                        Focusable = true,
                        ShowActivated = true,
                        ShowInTaskbar = true,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    };

                    window.Closed += delegate { Activate(); };

                    bool? result = await window.ShowDialog<bool?>(this);
                    if (result == true)
                        updatedSubmissionInfo = (window.MediaInformationViewModel.SubmissionInfo.Clone() as SubmissionInfo)!;

                    return result;
                });

                bool? result = dispatcherTask.GetAwaiter().GetResult();
                submissionInfo = updatedSubmissionInfo;
                return result;
            }
        }

        /// <summary>
        /// Show the Check Dump window
        /// </summary>
        public void ShowCheckDumpWindow()
        {
            // Hide MainWindow while Check GUI is open
            Hide();

            var window = new CheckDumpWindow(this)
            {
                Focusable = true,
                ShowActivated = true,
                ShowInTaskbar = true,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            window.Closed += delegate
            {
                // Unhide Main window after Check window has been closed
                Show();
                Activate();
            };

            _ = window.ShowDialog(this);
        }

        /// <summary>
        /// Show the Create IRD window
        /// </summary>
        public void ShowCreateIRDWindow()
        {
            // Hide MainWindow while Create IRD UI is open
            Hide();

            var window = new CreateIRDWindow(this)
            {
                Focusable = true,
                ShowActivated = true,
                ShowInTaskbar = true,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            window.Closed += delegate
            {
                // Unhide Main window after Create IRD window has been closed
                Show();
                Activate();
            };

            _ = window.ShowDialog(this);
        }

        /// <summary>
        /// Show the Options window
        /// </summary>
        public async void ShowOptionsWindow(string? title = null)
        {
            var window = new OptionsWindow(MainViewModel.Options)
            {
                Focusable = true,
                ShowActivated = true,
                ShowInTaskbar = true,
                Title = title ?? StringResource("OptionsTitleString", "Options"),
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };

            window.Closed += delegate { Activate(); };
            window.Closed += OnOptionsUpdated;
            _ = await window.ShowDialog<bool?>(this);
        }

        /// <summary>
        /// Set media type combo box visibility based on current program
        /// </summary>
        public void SetMediaTypeVisibility()
        {
            // Only DiscImageCreator uses the media type box
            if (MainViewModel.CurrentProgram != InternalProgram.DiscImageCreator)
            {
                SystemMediaTypeLabel!.Text = StringResource("SystemLabelString", "System Type");
                MediaTypeComboBox!.IsVisible = false;
                return;
            }

            // If there are no media types defined
            if (MainViewModel.MediaTypes is null)
            {
                SystemMediaTypeLabel!.Text = StringResource("SystemLabelString", "System Type");
                MediaTypeComboBox!.IsVisible = false;
                return;
            }

            // Only systems with more than one media type should show the box
            bool visible = MainViewModel.MediaTypes.Count > 1;
            SystemMediaTypeLabel!.Text = visible
                ? StringResource("SystemMediaTypeLabelString", "System/Media Type")
                : StringResource("SystemLabelString", "System Type");
            MediaTypeComboBox!.IsVisible = visible;
        }

        /// <summary>
        /// Update the log console height based on the current log panel state
        /// </summary>
        public void UpdateLogConsoleHeight()
        {
            if (LogPanel is null || LogOutput is null)
                return;

            if (!LogPanel.IsExpanded)
            {
                LogOutput.SetConsoleHeight(LogOutput.DefaultConsoleHeight);
                return;
            }

            LogOutput.SetConsoleHeight(LogOutput.DefaultConsoleHeight);
        }

        /// <summary>
        /// Wire up the log panel so the window resizes when it is expanded or collapsed
        /// </summary>
        public void WireLogPanelResizing()
        {
            if (_logPanelResizeInitialized)
                return;

            Dispatcher.UIThread.Post(() =>
            {
                _expandedWindowHeight = Height;
                if (LogPanel!.IsExpanded)
                    Height = _expandedWindowHeight + ExpandedLogPanelExtraHeight;

                _lastLogPanelHeight = LogPanel.Bounds.Height;
                _logPanelResizeInitialized = true;
                UpdateLogConsoleHeight();
            }, DispatcherPriority.Background);

            LogPanel!.PropertyChanged += (_, args) =>
            {
                if (args.Property != Expander.IsExpandedProperty)
                    return;

                if (!_logPanelResizeInitialized)
                    return;

                double previousLogPanelHeight = _lastLogPanelHeight;
                Dispatcher.UIThread.Post(() =>
                {
                    double currentLogPanelHeight = LogPanel.Bounds.Height;
                    if (previousLogPanelHeight <= 0 || currentLogPanelHeight <= 0)
                    {
                        _lastLogPanelHeight = currentLogPanelHeight;
                        return;
                    }

                    if (LogPanel.IsExpanded)
                    {
                        if (_expandedWindowHeight <= 0)
                            _expandedWindowHeight = Height;

                        Height = _expandedWindowHeight + ExpandedLogPanelExtraHeight;
                    }
                    else
                    {
                        double collapsedDelta = Math.Max(0, previousLogPanelHeight - currentLogPanelHeight);
                        _collapsedWindowHeight = Math.Max(300, _expandedWindowHeight - collapsedDelta + 40);
                        Height = _collapsedWindowHeight;
                    }

                    _lastLogPanelHeight = currentLogPanelHeight;
                    UpdateLogConsoleHeight();
                }, DispatcherPriority.Background);
            };
        }

        /// <summary>
        /// Handler for MainViewModel property changes; normalizes the output path on macOS
        /// </summary>
        public void OnMainViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Frontend.ViewModels.MainViewModel.OutputPath))
                NormalizeMacOutputPath();
        }

        /// <summary>
        /// Prompt the user to confirm closing while a dump may still be running
        /// </summary>
        public async Task ConfirmCloseAsync()
        {
            _closeConfirmationPending = true;

            try
            {
                bool? closeWindow = await MessageBoxWindow.ShowAsyncResult(this, "Exit", "A dump may still be running. Close anyway?", 2, true);
                if (closeWindow == true)
                {
                    _allowCloseWithoutPrompt = true;
                    Close();
                }
            }
            finally
            {
                _allowCloseWithoutPrompt = false;
                _closeConfirmationPending = false;
            }
        }

        /// <summary>
        /// Build the about text
        /// </summary>
        public string CreateAboutText()
        {
            string aboutText = $"{StringResource("AppTitleFullString", "Media Preservation Frontend (MPF)")}"
                + $"{Environment.NewLine}"
                + $"{Environment.NewLine}{StringResource("AboutLine1String", "A community preservation frontend developed in C#.")}"
                + $"{Environment.NewLine}{StringResource("AboutLine2String", "Supports Redumper, Aaru, and DiscImageCreator.")}"
                + $"{Environment.NewLine}{StringResource("AboutLine3String", "Originally created to help the Redump project.")}"
                + $"{Environment.NewLine}"
                + $"{Environment.NewLine}{StringResource("AboutThanksString", "Thanks to everyone who has supported this project!")}"
                + $"{Environment.NewLine}"
                + $"{Environment.NewLine}{StringResource("VersionLabelString", "Version")} {FrontendTool.GetCurrentVersion()}";
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
            var optionsWindow = sender as OptionsWindow;
            if (optionsWindow?.OptionsViewModel is null)
                return;

            // Get if the settings were saved
            bool savedSettings = optionsWindow.OptionsViewModel.SavedSettings;
            var options = optionsWindow.OptionsViewModel.Options;

            // Force a refresh of the path, if necessary
            if (MainViewModel.Options.Dumping.DefaultOutputPath != options.Dumping.DefaultOutputPath)
                MainViewModel.OutputPath = string.Empty;

            // Set the language according to the settings
            if (savedSettings)
            {
                var oldDefaultLang = MainViewModel.Options.GUI.DefaultInterfaceLanguage;
                var newDefaultLang = options.GUI.DefaultInterfaceLanguage;
                if (oldDefaultLang != newDefaultLang)
                {
                    SetInterfaceLanguage(newDefaultLang, rebuildNativeMenus: true);

                    // Uncheck all language menu items
                    foreach (var item in LanguagesMenuItem.Items)
                    {
                        if (item is MenuItem menuItem)
                            menuItem.IsChecked = false;
                    }
                }
            }

            // Update and save options, if necessary
            MainViewModel.UpdateOptions(savedSettings, options);

            // Set the UI color scheme according to the options
            ThemeService.Apply(Resources, options);

            // Hide or show the media type box based on program
            SetMediaTypeVisibility();
        }

        #region Menu Bar

        /// <summary>
        /// Handler for AboutMenuItem Click event
        /// </summary>
        public void AboutClick(object? sender, RoutedEventArgs e)
        {
            string aboutText = CreateAboutText();
            _ = MessageBoxWindow.ShowAsync(this, StringResource("AboutTitleString", "About"), aboutText, 1, false);
        }

        /// <summary>
        /// Handler for AppExitMenuItem Click event
        /// </summary>
        public void AppExitClick(object? sender, RoutedEventArgs e)
            => Close();

        /// <summary>
        /// Handler for CheckDumpMenuItem Click event
        /// </summary>
        public void CheckDumpMenuItemClick(object? sender, RoutedEventArgs e)
            => ShowCheckDumpWindow();

        /// <summary>
        /// Handler for the title bar close button
        /// </summary>
        private void CloseButtonClick(object? sender, RoutedEventArgs e)
            => Close();

        /// <summary>
        /// Handler for CreateIRDMenuItem Click event
        /// </summary>
        public void CreateIRDMenuItemClick(object? sender, RoutedEventArgs e)
            => ShowCreateIRDWindow();

        /// <summary>
        /// Handler for CheckForUpdatesMenuItem Click event
        /// </summary>
        public void CheckForUpdatesClick(object? sender, RoutedEventArgs e)
            => CheckForUpdates(showIfSame: true);

        /// <summary>
        /// Handler for DebugViewMenuItem Click event
        /// </summary>
        public void DebugViewClick(object? sender, RoutedEventArgs e)
            => ShowDebugDiscInfoWindow();

        /// <summary>
        /// Handler for the title bar minimize button
        /// </summary>
        private void MinimizeButtonClick(object? sender, RoutedEventArgs e)
            => WindowState = AvaloniaWindowState.Minimized;

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

            // Don't do anything if language is already checked and being unchecked
            if (!clickedItem.IsChecked)
            {
                clickedItem.IsChecked = true;
                return;
            }

            // Uncheck every item not checked
            var languageMenu = (MenuItem)clickedItem.Parent!;
            foreach (var item in languageMenu.Items)
            {
                if (item is MenuItem menuItem && menuItem != clickedItem)
                    menuItem.IsChecked = false;
            }

            // Change UI language to selected item
            string lang = clickedItem.Header?.ToString() ?? string.Empty;
            SetInterfaceLanguage(EnumExtensions.ToInterfaceLanguage(lang), rebuildNativeMenus: true);

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
            string? output = await MainViewModel.ScanAndShowProtection();

            if (!MainViewModel.LogPanelExpanded)
            {
                if (!string.IsNullOrEmpty(output))
                {
                    await MessageBoxWindow.ShowAsync(this,
                        StringResource("ProtectionDetectedTitleString", "Detected Protection(s)"),
                        output,
                        1,
                        false);
                }
                else
                {
                    await MessageBoxWindow.ShowAsync(this,
                        StringResource("ProtectionErrorTitleString", "Error!"),
                        StringResource("ProtectionErrorMessageString", "An exception occurred, see the log for details"),
                        1,
                        true);
                }
            }
        }

        /// <summary>
        /// Handler for DriveLetterComboBox SelectionChanged event
        /// </summary>
        public void DriveLetterComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
            {
                MainViewModel.InitializeUIValues(removeEventHandlers: true, rebuildPrograms: false, rescanDrives: false);
                SetMediaTypeVisibility();
            }
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
        {
            MainViewModel.InitializeUIValues(removeEventHandlers: true, rebuildPrograms: false, rescanDrives: true);
            SetMediaTypeVisibility();
        }

        /// <summary>
        /// Handler for PhysicalMediaTypeComboBox SelectionChanged event
        /// </summary>
        public void MediaTypeComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
                MainViewModel.ChangePhysicalMediaType(e.RemovedItems, e.AddedItems);
        }

        /// <summary>
        /// Handler for OutputPathBrowseButton Click event
        /// </summary>
        public async void OutputPathBrowseButtonClick(object? sender, RoutedEventArgs e)
        {
            string? selectedPath = await BrowseOutputFileAsync();
            if (!string.IsNullOrWhiteSpace(selectedPath))
            {
                MainViewModel.OutputPath = selectedPath;
                MainViewModel.EnsureMediaInformation();
            }
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
        {
            EnsureOutputPathIsFilePath();
            PrepareMacRedumperParameters();
            MainViewModel.ToggleStartStop();
        }

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

        /// <summary>
        /// Ensure the output path points to a file rather than a directory
        /// </summary>
        /// TODO: Is this needed?
        private void EnsureOutputPathIsFilePath()
        {
            string outputPath = MainViewModel.OutputPath;
            if (string.IsNullOrWhiteSpace(outputPath))
                return;

            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(outputPath);
            }
            catch
            {
                return;
            }

            if (!Directory.Exists(fullPath))
                return;

            MainViewModel.OutputPath = Path.Combine(outputPath, $"track_{DateTime.Now:yyyyMMdd-HHmm}.bin");
            MainViewModel.EnsureMediaInformation();
        }

        #endregion

        #endregion
    }
}
