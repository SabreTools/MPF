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
using MPF.Frontend.ViewModels;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;
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
        /// Menu item control names for the selectable interface languages
        /// </summary>
        private static readonly string[] LanguageMenuItemNames =
        [
            "EnglishMenuItem",
            "GermanMenuItem",
            "SpanishMenuItem",
            "FrenchMenuItem",
            "ItalianMenuItem",
            "JapaneseMenuItem",
            "KoreanMenuItem",
            "PolishMenuItem",
            "PortugueseMenuItem",
            "RussianMenuItem",
            "SwedishMenuItem",
            "UkrainianMenuItem",
        ];

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
            Opened += OnOpened;
            Closing += MainWindowClosing;
            SizeChanged += OnMainWindowSizeChanged;
        }

        /// <summary>
        /// Handler for the window Opened event; wires up events and performs first-load setup
        /// </summary>
        private void OnOpened(object? sender, EventArgs e)
        {
            WireEvents();
            ApplyLanguage(MainViewModel.Options.GUI.DefaultInterfaceLanguage, rebuildNativeMenus: false);
            ThemeService.Apply(Application.Current!.Resources, MainViewModel.Options);
            ConfigurePlatformMenus();

            MainViewModel.Init(
                this.FindControl<LogOutput>("LogOutput")!.EnqueueLog,
                DisplayUserMessage,
                ShowMediaInformationWindow);

            SetMediaTypeVisibility();
            WireLogPanelResizing();

            if (MainViewModel.Options.CheckForUpdatesOnStartup)
                CheckForUpdatesClick(this, new RoutedEventArgs());

            if (MainViewModel.Options.FirstRun)
                ShowOptionsWindow(StringResource("OptionsFirstRunTitleString", "Welcome to MPF, Explore the Options"));
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
                this.FindControl<Control>("TopMenuBar")!.IsVisible = false;
                this.FindControl<Grid>("RootGrid")!.RowDefinitions[0].Height = new GridLength(0);
            }
            else
            {
                this.FindControl<TextBlock>("AppTitleLabel")!.IsVisible = false;
                this.FindControl<Control>("TitleDragArea")!.IsVisible = false;
                this.FindControl<Button>("CloseButton")!.IsVisible = false;
            }

            this.FindControl<Border>("RootBorder")!.Padding = new Thickness(2, OperatingSystem.IsMacOS() ? 4 : 2, 2, 8);
        }

        /// <summary>
        /// Set the current interface language and refresh all localized menu headers
        /// </summary>
        private void ApplyLanguage(InterfaceLanguage language, bool rebuildNativeMenus)
        {
            StringResourceLoader.Load(Application.Current!.Resources, language);
            StringResourceLoader.Load(Resources, language);

            UpdateTitleBarMenuHeaders();
            if (rebuildNativeMenus)
                UpdateNativeMenuHeaders();
        }

        /// <summary>
        /// Refresh the in-window title bar menu headers to the current interface language
        /// </summary>
        private void UpdateTitleBarMenuHeaders()
        {
            this.FindControl<MenuItem>("FileMenuItem")!.Header = StringResource("FileMenuString", "File");
            this.FindControl<MenuItem>("ToolsMenuItem")!.Header = StringResource("ToolsMenuString", "Tools");
            this.FindControl<MenuItem>("HelpMenuItem")!.Header = StringResource("HelpMenuString", "Help");
            this.FindControl<MenuItem>("LanguagesMenuItem")!.Header = StringResource("LanguageMenuString", "ENG");
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
        /// Handler for the title bar minimize button
        /// </summary>
        private void MinimizeButtonClick(object? sender, RoutedEventArgs e)
            => WindowState = AvaloniaWindowState.Minimized;

        /// <summary>
        /// Handler for the title bar close button
        /// </summary>
        private void CloseButtonClick(object? sender, RoutedEventArgs e)
            => Close();

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
        private void WireEvents()
        {
            this.FindControl<MenuItem>("AboutMenuItem")!.Click += AboutClick;
            this.FindControl<MenuItem>("AppExitMenuItem")!.Click += AppExitClick;
            this.FindControl<MenuItem>("CheckForUpdatesMenuItem")!.Click += CheckForUpdatesClick;
            this.FindControl<MenuItem>("CheckDumpMenuItem")!.Click += CheckDumpMenuItemClick;
            this.FindControl<MenuItem>("CreateIRDMenuItem")!.Click += CreateIRDMenuItemClick;
            this.FindControl<MenuItem>("DebugViewMenuItem")!.Click += DebugViewClick;
            this.FindControl<MenuItem>("OptionsMenuItem")!.Click += OptionsMenuItemClick;

            foreach (string name in LanguageMenuItemNames)
            {
                this.FindControl<MenuItem>(name)!.Click += LanguageMenuItemClick;
            }

            this.FindControl<Button>("CopyProtectScanButton")!.Click += CopyProtectScanButtonClick;
            this.FindControl<ComboBox>("DriveLetterComboBox")!.SelectionChanged += DriveLetterComboBoxSelectionChanged;
            this.FindControl<ComboBox>("DriveSpeedComboBox")!.SelectionChanged += DriveSpeedComboBoxSelectionChanged;
            this.FindControl<ComboBox>("DumpingProgramComboBox")!.SelectionChanged += DumpingProgramComboBoxSelectionChanged;
            this.FindControl<CheckBox>("EnableParametersCheckBox")!.Click += EnableParametersCheckBoxClick;
            this.FindControl<Button>("MediaScanButton")!.Click += MediaScanButtonClick;
            this.FindControl<ComboBox>("MediaTypeComboBox")!.SelectionChanged += MediaTypeComboBoxSelectionChanged;
            this.FindControl<Button>("OutputPathBrowseButton")!.Click += OutputPathBrowseButtonClick;
            this.FindControl<TextBox>("OutputPathTextBox")!.TextChanged += OutputPathTextBoxTextChanged;
            this.FindControl<Button>("StartStopButton")!.Click += StartStopButtonClick;
            this.FindControl<ComboBox>("SystemTypeComboBox")!.SelectionChanged += SystemTypeComboBoxSelectionChanged;
            this.FindControl<Button>("UpdateVolumeLabel")!.Click += UpdateVolumeLabelClick;
        }

        /// <summary>
        /// Wire up the log panel so the window resizes when it is expanded or collapsed
        /// </summary>
        private void WireLogPanelResizing()
        {
            if (_logPanelResizeInitialized)
                return;

            Expander logPanel = this.FindControl<Expander>("LogPanel")!;

            Dispatcher.UIThread.Post(() =>
            {
                _expandedWindowHeight = Height;
                if (logPanel.IsExpanded)
                    Height = _expandedWindowHeight + ExpandedLogPanelExtraHeight;

                _lastLogPanelHeight = logPanel.Bounds.Height;
                _logPanelResizeInitialized = true;
                UpdateLogConsoleHeight();
            }, DispatcherPriority.Background);

            logPanel.PropertyChanged += (_, args) =>
            {
                if (args.Property != Expander.IsExpandedProperty)
                    return;

                if (!_logPanelResizeInitialized)
                    return;

                double previousLogPanelHeight = _lastLogPanelHeight;
                Dispatcher.UIThread.Post(() =>
                {
                    double currentLogPanelHeight = logPanel.Bounds.Height;
                    if (previousLogPanelHeight <= 0 || currentLogPanelHeight <= 0)
                    {
                        _lastLogPanelHeight = currentLogPanelHeight;
                        return;
                    }

                    if (logPanel.IsExpanded)
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
        /// Handler for the window SizeChanged event; keeps the log console height in sync
        /// </summary>
        private void OnMainWindowSizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (!_logPanelResizeInitialized)
                return;

            UpdateLogConsoleHeight();
        }

        /// <summary>
        /// Update the log console height based on the current log panel state
        /// </summary>
        private void UpdateLogConsoleHeight()
        {
            Expander? logPanel = this.FindControl<Expander>("LogPanel");
            LogOutput? logOutput = this.FindControl<LogOutput>("LogOutput");
            if (logPanel is null || logOutput is null)
                return;

            if (!logPanel.IsExpanded)
            {
                logOutput.SetConsoleHeight(LogOutput.DefaultConsoleHeight);
                return;
            }

            logOutput.SetConsoleHeight(LogOutput.DefaultConsoleHeight);
        }

        /// <summary>
        /// Show the media information window
        /// </summary>
        /// <param name="options">Options set to pass to the information window</param>
        /// <param name="submissionInfo">SubmissionInfo object to display and possibly change</param>
        /// <returns>Dialog open result</returns>
        private bool? ShowMediaInformationWindow(Options? options, ref SubmissionInfo? submissionInfo)
        {
            var dialogOptions = options ?? MainViewModel.Options;
            SubmissionInfo? updatedSubmissionInfo = submissionInfo;

            if (Dispatcher.UIThread.CheckAccess())
            {
                var window = new MediaInformationWindow(dialogOptions, updatedSubmissionInfo)
                {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };

                Task<bool?> dialogTask = window.ShowDialog<bool?>(this);
                var frame = new DispatcherFrame();
                dialogTask.ContinueWith(_ => Dispatcher.UIThread.Post(() => frame.Continue = false));
                Dispatcher.UIThread.PushFrame(frame);

                bool? dialogResult = dialogTask.GetAwaiter().GetResult();
                if (dialogResult == true)
                    updatedSubmissionInfo = window.MediaInformationViewModel.SubmissionInfo;

                submissionInfo = updatedSubmissionInfo;
                return dialogResult;
            }

            var dispatcherTask = Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var window = new MediaInformationWindow(dialogOptions, updatedSubmissionInfo)
                {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };

                bool? result = await window.ShowDialog<bool?>(this);
                if (result == true)
                    updatedSubmissionInfo = window.MediaInformationViewModel.SubmissionInfo;

                return result;
            });

            bool? result = dispatcherTask.GetAwaiter().GetResult();
            submissionInfo = updatedSubmissionInfo;
            return result;
        }

        /// <summary>
        /// Browse for an output file path
        /// </summary>
        private async Task<string?> BrowseOutputFileAsync()
        {
            string currentPath = MainViewModel.OutputPath;
            if (string.IsNullOrEmpty(currentPath) && !string.IsNullOrEmpty(MainViewModel.Options.Dumping.DefaultOutputPath))
                currentPath = Path.Combine(MainViewModel.Options.Dumping.DefaultOutputPath, $"track_{DateTime.Now:yyyyMMdd-HHmm}.bin");
            else if (string.IsNullOrEmpty(currentPath))
                currentPath = $"track_{DateTime.Now:yyyyMMdd-HHmm}.bin";
            if (string.IsNullOrEmpty(currentPath))
                currentPath = Path.Combine(AppContext.BaseDirectory, $"track_{DateTime.Now:yyyyMMdd-HHmm}.bin");

            currentPath = Path.GetFullPath(currentPath);
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
        /// Handler for MainViewModel property changes; normalizes the output path on macOS
        /// </summary>
        private void OnMainViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Frontend.ViewModels.MainViewModel.OutputPath))
                NormalizeMacOutputPath();
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
        /// Prompt the user to confirm closing while a dump may still be running
        /// </summary>
        private async Task ConfirmCloseAsync()
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
        /// Show the Check Dump window
        /// </summary>
        public void ShowCheckDumpWindow()
        {
            var window = new CheckDumpWindow(this) { WindowStartupLocation = WindowStartupLocation.CenterOwner };
            _ = window.ShowDialog(this);
        }

        /// <summary>
        /// Show the Create IRD window
        /// </summary>
        public void ShowCreateIRDWindow()
        {
            var window = new CreateIRDWindow(this) { WindowStartupLocation = WindowStartupLocation.CenterOwner };
            _ = window.ShowDialog(this);
        }

        /// <summary>
        /// Show the Options window
        /// </summary>
        public async void ShowOptionsWindow(string? title = null)
        {
            var window = new OptionsWindow(MainViewModel.Options)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Title = title ?? StringResource("OptionsTitleString", "Options"),
            };

            bool? result = await window.ShowDialog<bool?>(this);
            bool savedSettings = result == true && window.OptionsViewModel.SavedSettings;
            MainViewModel.UpdateOptions(savedSettings, window.OptionsViewModel.Options);

            if (savedSettings)
            {
                ApplyLanguage(MainViewModel.Options.GUI.DefaultInterfaceLanguage, rebuildNativeMenus: true);
                ThemeService.Apply(Application.Current!.Resources, MainViewModel.Options);
                SetMediaTypeVisibility();
            }
        }

        /// <summary>
        /// Set media type combo box visibility based on current program
        /// </summary>
        public void SetMediaTypeVisibility()
        {
            this.FindControl<ComboBox>("MediaTypeComboBox")!.IsVisible = MainViewModel.CurrentProgram == InternalProgram.DiscImageCreator;
        }

        /// <summary>
        /// Build the about text
        /// </summary>
        private string CreateAboutText()
        {
            string version = typeof(MainWindow).Assembly.GetName().Version?.ToString() ?? "Unknown";
            return $"{StringResource("AppTitleFullString", "Media Preservation Frontend (MPF)")}\n\n"
                + $"{StringResource("AboutLine1String", "A community preservation frontend developed in C#.")}\n"
                + $"{StringResource("AboutLine2String", "Supports Redumper, Aaru, and DiscImageCreator.")}\n"
                + $"{StringResource("AboutLine3String", "Originally created to help the Redump project.")}\n\n"
                + $"{StringResource("VersionLabelString", "Version")}: {version}";
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for AboutMenuItem Click event
        /// </summary>
        public void AboutClick(object? sender, RoutedEventArgs e)
            => _ = MessageBoxWindow.ShowAsync(this, StringResource("AboutTitleString", "About"), CreateAboutText(), 1, false);

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
        /// Handler for CreateIRDMenuItem Click event
        /// </summary>
        public void CreateIRDMenuItemClick(object? sender, RoutedEventArgs e)
            => ShowCreateIRDWindow();

        /// <summary>
        /// Handler for CheckForUpdatesMenuItem Click event
        /// </summary>
        public void CheckForUpdatesClick(object? sender, RoutedEventArgs e)
        {
            MainViewModel.CheckForUpdates(out bool different, out string message, out _);
            _ = MessageBoxWindow.ShowAsync(this, StringResource("CheckForUpdatesTitleString", "Check for Updates"), message, 1, different);
        }

        /// <summary>
        /// Build a dummy SubmissionInfo and display it for testing
        /// </summary>
        public void ShowDebugDiscInfoWindow()
        {
            SubmissionInfo? submissionInfo = MainViewModel.CreateDebugSubmissionInfo();
            _ = ShowDebugDiscInfoWindowAsync(submissionInfo);
        }

        /// <summary>
        /// Show the debug submission info in the media information window and process the result
        /// </summary>
        private async Task ShowDebugDiscInfoWindowAsync(SubmissionInfo submissionInfo)
        {
            var window = new MediaInformationWindow(MainViewModel.Options, submissionInfo, showPcMacHybridAlways: true)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };

            bool? result = await window.ShowDialog<bool?>(this);
            if (result == true)
                submissionInfo = window.MediaInformationViewModel.SubmissionInfo;

            Formatter.ProcessSpecialFields(submissionInfo);
        }

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
            if (sender is not MenuItem menuItem)
                return;

            InterfaceLanguage language = menuItem.Name switch
            {
                "GermanMenuItem" => InterfaceLanguage.German,
                "SpanishMenuItem" => InterfaceLanguage.Spanish,
                "FrenchMenuItem" => InterfaceLanguage.French,
                "ItalianMenuItem" => InterfaceLanguage.Italian,
                "JapaneseMenuItem" => InterfaceLanguage.Japanese,
                "KoreanMenuItem" => InterfaceLanguage.Korean,
                "PolishMenuItem" => InterfaceLanguage.Polish,
                "PortugueseMenuItem" => InterfaceLanguage.Portuguese,
                "RussianMenuItem" => InterfaceLanguage.Russian,
                "SwedishMenuItem" => InterfaceLanguage.Swedish,
                "UkrainianMenuItem" => InterfaceLanguage.Ukrainian,
                _ => InterfaceLanguage.English,
            };

            MainViewModel.Options.GUI.DefaultInterfaceLanguage = language;
            MainViewModel.Options = new Options(MainViewModel.Options);
            ApplyLanguage(language, rebuildNativeMenus: true);
        }

        /// <summary>
        /// Handler for CopyProtectScanButton Click event
        /// </summary>
        public async void CopyProtectScanButtonClick(object? sender, RoutedEventArgs e)
        {
            string? output = await MainViewModel.ScanAndShowProtection();
            if (!MainViewModel.LogPanelExpanded)
            {
                string title = StringResource("ProtectionDetectedTitleString", "Protection");
                string fallback = StringResource("ProtectionErrorMessageString", "No protection information could be retrieved.");
                _ = MessageBoxWindow.ShowAsync(this, title, string.IsNullOrWhiteSpace(output) ? fallback : output, 1, false);
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
        /// Ensure the output path points to a file rather than a directory
        /// </summary>
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
    }
}
