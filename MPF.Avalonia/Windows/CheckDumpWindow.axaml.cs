using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MPF.Avalonia.Services;
using MPF.Frontend;
using MPF.Frontend.ViewModels;
using SabreTools.RedumpLib.Data;

namespace MPF.Avalonia.Windows
{
    public partial class CheckDumpWindow : WindowBase
    {
        private MainWindow? _parent;

        public CheckDumpViewModel CheckDumpViewModel => DataContext as CheckDumpViewModel ?? new CheckDumpViewModel();

        public CheckDumpWindow()
        {
            InitializeWindow(null);
        }

        public CheckDumpWindow(MainWindow parent)
        {
            InitializeWindow(parent);
        }

        private void InitializeWindow(MainWindow? parent)
        {
            InitializeComponent();
            _parent = parent;
            DataContext = new CheckDumpViewModel();
            Opened += (_, _) => WireEvents();
        }

        private void WireEvents()
        {
            this.FindControl<Button>("CheckDumpButton")!.Click += OnCheckDumpClick;
            this.FindControl<Button>("CancelButton")!.Click += OnCancelClick;
            this.FindControl<Button>("InputPathBrowseButton")!.Click += InputPathBrowseButtonClick;
            this.FindControl<ComboBox>("SystemTypeComboBox")!.SelectionChanged += SystemTypeComboBoxSelectionChanged;
            this.FindControl<ComboBox>("DumpingProgramComboBox")!.SelectionChanged += DumpingProgramComboBoxSelectionChanged;
            this.FindControl<TextBox>("InputPathTextBox")!.TextChanged += InputPathTextBoxTextChanged;
        }

        private bool? ShowMediaInformationWindow(Options? options, ref SubmissionInfo? submissionInfo)
        {
            var dialogOptions = options ?? CheckDumpViewModel.Options;
            SubmissionInfo? updatedSubmissionInfo = submissionInfo;
            Window owner = _parent ?? (Window)this;
            var dialogTask = Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var window = new MediaInformationWindow(dialogOptions, updatedSubmissionInfo)
                {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };

                bool? result = await window.ShowDialog<bool?>(owner);
                if (result == true)
                    updatedSubmissionInfo = window.MediaInformationViewModel.SubmissionInfo;

                return result;
            });

            bool? result = dialogTask.GetAwaiter().GetResult();
            submissionInfo = updatedSubmissionInfo;
            return result;
        }

        private async Task<string?> BrowseFileAsync()
        {
            return await DialogService.OpenFileAsync(this, StringResource("InputPathLabelString", "Input Path"), new List<FilePickerFileType>
            {
                new("Disc Images") { Patterns = new[] { "*.iso", "*.cue", "*.aaruf", "*.bca" } },
                new("Log Archives") { Patterns = new[] { "*_logs.zip" } },
                FilePickerFileTypes.All,
            });
        }

        private async void OnCheckDumpClick(object? sender, RoutedEventArgs e)
        {
            var result = await CheckDumpViewModel.CheckDump(ShowMediaInformationWindow);
            if (result == true)
                DisplayUserMessage("Check Complete", "The dump has been processed successfully.", 1, false);
            else
                DisplayUserMessage("Check Failed", string.IsNullOrEmpty(result.Message) ? "Please check all files exist and try again!" : result.Message, 1, false);
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
            => Close(false);

        public void DumpingProgramComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (CheckDumpViewModel.CanExecuteSelectionChanged)
                CheckDumpViewModel.ChangeDumpingProgram();
        }

        public async void InputPathBrowseButtonClick(object? sender, RoutedEventArgs e)
        {
            string? selectedPath = await BrowseFileAsync();
            if (!string.IsNullOrWhiteSpace(selectedPath))
            {
                CheckDumpViewModel.InputPath = selectedPath;
                if (CheckDumpViewModel.CanExecuteSelectionChanged)
                    CheckDumpViewModel.ChangeInputPath();
            }
        }

        public void InputPathTextBoxTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (CheckDumpViewModel.CanExecuteSelectionChanged)
                CheckDumpViewModel.ChangeInputPath();
        }

        public void SystemTypeComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (CheckDumpViewModel.CanExecuteSelectionChanged)
                CheckDumpViewModel.ChangeSystem();
        }
    }
}
