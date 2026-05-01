using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
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
    public partial class MainWindow : WindowBase
    {
        private const double ExpandedLogPanelExtraHeight = 40;
        private bool _allowCloseWithoutPrompt;
        private bool _closeConfirmationPending;
        private double _lastLogPanelHeight;
        private double _expandedWindowHeight;
        private double _collapsedWindowHeight;
        private bool _logPanelResizeInitialized;

        public MainViewModel MainViewModel => DataContext as MainViewModel ?? new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            ThemeService.SyncWithSystemTheme(MainViewModel.Options);
            Opened += OnOpened;
            Closing += MainWindowClosing;
            SizeChanged += OnMainWindowSizeChanged;
        }

        private void OnOpened(object? sender, EventArgs e)
        {
            WireEvents();
            StringResourceLoader.Load(Resources, MainViewModel.Options.GUI.DefaultInterfaceLanguage);
            ThemeService.Apply(global::Avalonia.Application.Current!.Resources, MainViewModel.Options);
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

        private void ConfigurePlatformMenus()
        {
            if (!OperatingSystem.IsMacOS())
                return;

            this.FindControl<Control>("TopMenuBar")!.IsVisible = false;
            this.FindControl<Grid>("RootGrid")!.RowDefinitions[0].Height = new GridLength(0);
            this.FindControl<Border>("RootBorder")!.Padding = new Thickness(10, 2, 10, 6);

            var nativeMenu = new NativeMenu();
            nativeMenu.Add(CreateNativeMenuGroup(
                StringResource("FileMenuString", "File"),
                CreateNativeMenuItem(StringResource("ExitMenuItemString", "Exit"), () => AppExitClick(this, new RoutedEventArgs()))));
            nativeMenu.Add(CreateNativeMenuGroup(
                StringResource("ToolsMenuString", "Tools"),
                CreateNativeMenuItem(StringResource("CheckDumpMenuItemString", "Check Dump"), () => CheckDumpMenuItemClick(this, new RoutedEventArgs())),
                CreateNativeMenuItem(StringResource("CreatePS3IRDDumpMenuItemString", "Create PS3 IRD"), () => CreateIRDMenuItemClick(this, new RoutedEventArgs())),
                CreateNativeMenuItem(StringResource("OptionsDumpMenuItemString", "Options"), () => OptionsMenuItemClick(this, new RoutedEventArgs())),
                CreateNativeMenuItem(StringResource("DebugInfoWindowMenuItemString", "Debug Info Window"), () => DebugViewClick(this, new RoutedEventArgs()))));
            nativeMenu.Add(CreateNativeMenuGroup(
                "Language",
                CreateNativeMenuItem("English", () => LanguageMenuItemClick(this.FindControl<MenuItem>("EnglishMenuItem"), new RoutedEventArgs())),
                CreateNativeMenuItem("Deutsch", () => LanguageMenuItemClick(this.FindControl<MenuItem>("GermanMenuItem"), new RoutedEventArgs())),
                CreateNativeMenuItem("Español", () => LanguageMenuItemClick(this.FindControl<MenuItem>("SpanishMenuItem"), new RoutedEventArgs())),
                CreateNativeMenuItem("Français", () => LanguageMenuItemClick(this.FindControl<MenuItem>("FrenchMenuItem"), new RoutedEventArgs())),
                CreateNativeMenuItem("Italiano", () => LanguageMenuItemClick(this.FindControl<MenuItem>("ItalianMenuItem"), new RoutedEventArgs())),
                CreateNativeMenuItem("日本語", () => LanguageMenuItemClick(this.FindControl<MenuItem>("JapaneseMenuItem"), new RoutedEventArgs())),
                CreateNativeMenuItem("한국어", () => LanguageMenuItemClick(this.FindControl<MenuItem>("KoreanMenuItem"), new RoutedEventArgs())),
                CreateNativeMenuItem("Polski", () => LanguageMenuItemClick(this.FindControl<MenuItem>("PolishMenuItem"), new RoutedEventArgs())),
                CreateNativeMenuItem("Português", () => LanguageMenuItemClick(this.FindControl<MenuItem>("PortugueseMenuItem"), new RoutedEventArgs())),
                CreateNativeMenuItem("Русский", () => LanguageMenuItemClick(this.FindControl<MenuItem>("RussianMenuItem"), new RoutedEventArgs())),
                CreateNativeMenuItem("Svenska", () => LanguageMenuItemClick(this.FindControl<MenuItem>("SwedishMenuItem"), new RoutedEventArgs())),
                CreateNativeMenuItem("Українська", () => LanguageMenuItemClick(this.FindControl<MenuItem>("UkrainianMenuItem"), new RoutedEventArgs()))));
            nativeMenu.Add(CreateNativeMenuGroup(
                StringResource("HelpMenuString", "Help"),
                CreateNativeMenuItem(StringResource("AboutMenuItemString", "About"), () => AboutClick(this, new RoutedEventArgs())),
                CreateNativeMenuItem(StringResource("CheckForUpdateMenuItemString", "Check for Updates"), () => CheckForUpdatesClick(this, new RoutedEventArgs()))));

            NativeMenu.SetMenu(this, nativeMenu);
        }

        private NativeMenuItem CreateNativeMenuGroup(string header, params NativeMenuItem[] items)
        {
            var group = new NativeMenuItem { Header = CleanMenuHeader(header) };
            var menu = new NativeMenu();
            foreach (NativeMenuItem item in items)
                menu.Add(item);

            group.Menu = menu;
            return group;
        }

        private static NativeMenuItem CreateNativeMenuItem(string header, Action action)
        {
            var item = new NativeMenuItem { Header = CleanMenuHeader(header) };
            item.Click += (_, _) => action();
            return item;
        }

        private static string CleanMenuHeader(string header)
            => header.Replace("_", string.Empty);

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

        private void MinimizeButtonClick(object? sender, RoutedEventArgs e)
            => WindowState = AvaloniaWindowState.Minimized;

        private void CloseButtonClick(object? sender, RoutedEventArgs e)
            => Close();

        private void ToggleWindowState()
            => WindowState = WindowState == AvaloniaWindowState.Maximized
                ? AvaloniaWindowState.Normal
                : AvaloniaWindowState.Maximized;

        private void WireEvents()
        {
            this.FindControl<MenuItem>("AboutMenuItem")!.Click += AboutClick;
            this.FindControl<MenuItem>("AppExitMenuItem")!.Click += AppExitClick;
            this.FindControl<MenuItem>("CheckForUpdatesMenuItem")!.Click += CheckForUpdatesClick;
            this.FindControl<MenuItem>("CheckDumpMenuItem")!.Click += CheckDumpMenuItemClick;
            this.FindControl<MenuItem>("CreateIRDMenuItem")!.Click += CreateIRDMenuItemClick;
            this.FindControl<MenuItem>("DebugViewMenuItem")!.Click += DebugViewClick;
            this.FindControl<MenuItem>("OptionsMenuItem")!.Click += OptionsMenuItemClick;

            foreach (string name in new[]
            {
                "EnglishMenuItem", "GermanMenuItem", "SpanishMenuItem", "FrenchMenuItem", "ItalianMenuItem",
                "JapaneseMenuItem", "KoreanMenuItem", "PolishMenuItem", "PortugueseMenuItem",
                "RussianMenuItem", "SwedishMenuItem", "UkrainianMenuItem"
            })
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

        private void OnMainWindowSizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (!_logPanelResizeInitialized)
                return;

            UpdateLogConsoleHeight();
        }

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

            double expandedBaseHeight = _expandedWindowHeight + ExpandedLogPanelExtraHeight;
            double extraHeight = Math.Max(0, Height - expandedBaseHeight);
            logOutput.SetConsoleHeight(LogOutput.DefaultConsoleHeight + extraHeight);
        }

        private bool? ShowMediaInformationWindow(Options? options, ref SubmissionInfo? submissionInfo)
        {
            var dialogOptions = options ?? MainViewModel.Options;
            SubmissionInfo? updatedSubmissionInfo = submissionInfo;
            var dialogTask = Dispatcher.UIThread.InvokeAsync(async () =>
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

            bool? result = dialogTask.GetAwaiter().GetResult();
            submissionInfo = updatedSubmissionInfo;
            return result;
        }

        private async Task<string?> BrowseFolderAsync()
            => await DialogService.OpenFolderAsync(this, StringResource("OutputPathLabelString", "Output Path"));

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

        public void ShowCheckDumpWindow()
        {
            var window = new CheckDumpWindow(this) { WindowStartupLocation = WindowStartupLocation.CenterOwner };
            _ = window.ShowDialog(this);
        }

        public void ShowCreateIRDWindow()
        {
            var window = new CreateIRDWindow(this) { WindowStartupLocation = WindowStartupLocation.CenterOwner };
            _ = window.ShowDialog(this);
        }

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
                StringResourceLoader.Load(global::Avalonia.Application.Current!.Resources, MainViewModel.Options.GUI.DefaultInterfaceLanguage);
                ThemeService.Apply(global::Avalonia.Application.Current.Resources, MainViewModel.Options);
                SetMediaTypeVisibility();
            }
        }

        public void SetMediaTypeVisibility()
        {
            this.FindControl<ComboBox>("MediaTypeComboBox")!.IsVisible = MainViewModel.MediaTypeComboBoxEnabled;
        }

        private string CreateAboutText()
        {
            string version = typeof(MainWindow).Assembly.GetName().Version?.ToString() ?? "Unknown";
            return $"{StringResource("AppTitleFullString", "Media Preservation Frontend (MPF)")}\n\n"
                + $"{StringResource("AboutLine1String", "A community preservation frontend developed in C#.")}\n"
                + $"{StringResource("AboutLine2String", "Supports Redumper, Aaru, and DiscImageCreator.")}\n"
                + $"{StringResource("AboutLine3String", "Originally created to help the Redump project.")}\n\n"
                + $"{StringResource("VersionLabelString", "Version")}: {version}";
        }

        public void AboutClick(object? sender, RoutedEventArgs e)
            => _ = MessageBoxWindow.ShowAsync(this, StringResource("AboutTitleString", "About"), CreateAboutText(), 1, false);

        public void AppExitClick(object? sender, RoutedEventArgs e)
            => Close();

        public void CheckDumpMenuItemClick(object? sender, RoutedEventArgs e)
            => ShowCheckDumpWindow();

        public void CreateIRDMenuItemClick(object? sender, RoutedEventArgs e)
            => ShowCreateIRDWindow();

        public void CheckForUpdatesClick(object? sender, RoutedEventArgs e)
        {
            MainViewModel.CheckForUpdates(out bool different, out string message, out var url);
            _ = MessageBoxWindow.ShowAsync(this, StringResource("CheckForUpdatesTitleString", "Check for Updates"), message, 1, different);
        }

        public void DebugViewClick(object? sender, RoutedEventArgs e)
            => _ = new MediaInformationWindow(MainViewModel.Options, null).ShowDialog(this);

        public void OptionsMenuItemClick(object? sender, RoutedEventArgs e)
            => ShowOptionsWindow();

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
            StringResourceLoader.Load(global::Avalonia.Application.Current!.Resources, language);
        }

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

        public void DriveLetterComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
                MainViewModel.InitializeUIValues(removeEventHandlers: true, rebuildPrograms: false, rescanDrives: false);
        }

        public void DriveSpeedComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
                MainViewModel.EnsureMediaInformation();
        }

        public void DumpingProgramComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
            {
                MainViewModel.ChangeDumpingProgram();
                SetMediaTypeVisibility();
            }
        }

        public void EnableParametersCheckBoxClick(object? sender, RoutedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
                MainViewModel.ToggleParameters();
        }

        public void MediaScanButtonClick(object? sender, RoutedEventArgs e)
            => MainViewModel.InitializeUIValues(removeEventHandlers: true, rebuildPrograms: false, rescanDrives: true);

        public void MediaTypeComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
                MainViewModel.ChangeMediaType(e.RemovedItems, e.AddedItems);
        }

        public async void OutputPathBrowseButtonClick(object? sender, RoutedEventArgs e)
        {
            string? selectedPath = await BrowseFolderAsync();
            if (!string.IsNullOrWhiteSpace(selectedPath))
                MainViewModel.OutputPath = selectedPath;
        }

        public void OutputPathTextBoxTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
                MainViewModel.EnsureMediaInformation();
        }

        public void StartStopButtonClick(object? sender, RoutedEventArgs e)
            => MainViewModel.ToggleStartStop();

        public void SystemTypeComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (MainViewModel.CanExecuteSelectionChanged)
            {
                MainViewModel.ChangeSystem();
                SetMediaTypeVisibility();
            }
        }

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
    }
}
