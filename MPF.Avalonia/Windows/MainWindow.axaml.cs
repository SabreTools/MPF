using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MPF.Avalonia.Services;
using MPF.Avalonia.UserControls;
using MPF.Frontend;
using MPF.Frontend.ViewModels;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;

namespace MPF.Avalonia.Windows
{
    public partial class MainWindow : WindowBase
    {
        private bool _allowCloseWithoutPrompt;
        private bool _closeConfirmationPending;

        public MainViewModel MainViewModel => DataContext as MainViewModel ?? new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            Opened += OnOpened;
            Closing += MainWindowClosing;
        }

        private void OnOpened(object? sender, EventArgs e)
        {
            WireEvents();
            StringResourceLoader.Load(Resources, MainViewModel.Options.GUI.DefaultInterfaceLanguage);
            ThemeService.Apply(global::Avalonia.Application.Current!.Resources, MainViewModel.Options);

            MainViewModel.Init(
                this.FindControl<LogOutput>("LogOutput")!.EnqueueLog,
                DisplayUserMessage,
                ShowMediaInformationWindow);

            SetMediaTypeVisibility();
        }

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
            if (result == true)
            {
                MainViewModel.Options = window.OptionsViewModel.Options;
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
