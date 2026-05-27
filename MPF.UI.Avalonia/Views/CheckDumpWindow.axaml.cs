// MPF cross-platform (Avalonia) UI — contributed by Knutwurst (https://github.com/knutwurst)
using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MPF.Frontend;
using MPF.Frontend.ViewModels;
using MPF.UI.Avalonia.Services;
using SabreTools.RedumpLib.Data;

namespace MPF.UI.Avalonia.Views
{
    /// <summary>
    /// Interaction logic for CheckDumpWindow.axaml
    /// </summary>
    public partial class CheckDumpWindow : WindowBase
    {
        /// <summary>
        /// Read-only access to the current check dump view model
        /// </summary>
        public CheckDumpViewModel CheckDumpViewModel => (CheckDumpViewModel)DataContext!;

        /// <summary>
        /// Parameterless constructor required by the Avalonia XAML compiler.
        /// For runtime use, call the <see cref="CheckDumpWindow(MainWindow)"/> overload.
        /// </summary>
        public CheckDumpWindow() : this(null!) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">Owning MainWindow, used to copy Options.</param>
        public CheckDumpWindow(MainWindow parent)
        {
            InitializeComponent();

            var vm = new CheckDumpViewModel();
            // The CheckDumpViewModel loads its own Options from config, which mirrors what WPF does
            // (the WPF ctor has no explicit Options copy from parent — the VM ctor calls OptionsLoader).
            DataContext = vm;
        }

        #region UI Functionality

        /// <summary>
        /// Look up a string resource by key, returning an empty string if missing.
        /// </summary>
        private string FindResourceString(string key)
            => this.TryFindResource(key, out var value) && value is string s ? s : string.Empty;

        /// <summary>
        /// Browse for an input file path using the Avalonia file picker.
        /// </summary>
        public async Task BrowseFile()
        {
            // Replicate WPF default-directory logic
            string? currentPath = CheckDumpViewModel.InputPath;
            if (string.IsNullOrEmpty(currentPath) && !string.IsNullOrEmpty(CheckDumpViewModel.Options.Dumping.DefaultOutputPath))
                currentPath = CheckDumpViewModel.Options.Dumping.DefaultOutputPath!;
            if (string.IsNullOrEmpty(currentPath))
                currentPath = AppDomain.CurrentDomain.BaseDirectory!;

            string? selectedPath = await StorageDialogs.OpenFileAsync(
                this,
                FindResourceString("BrowseButtonString"),
                ("Disc Images", ["*.iso", "*.cue", "*.aaruf", "*.bca"]),
                ("Log Archives", ["*_logs.zip"]),
                ("All Files", ["*.*"]));

            if (!string.IsNullOrEmpty(selectedPath))
                CheckDumpViewModel.InputPath = selectedPath!;
        }

        /// <summary>
        /// Show the media information window as a synchronous callback (bridge for ProcessUserInfoDelegate).
        /// Called from a background thread via Task.Run; uses Dispatcher.UIThread.InvokeAsync to marshal
        /// onto the UI thread and block the caller until the dialog is closed.
        /// </summary>
        /// <param name="options">Options to pass to the information window.</param>
        /// <param name="submissionInfo">SubmissionInfo object to display and possibly update.</param>
        /// <returns>Dialog result (true = accepted, false = rejected, null = cancelled).</returns>
        public bool? ShowMediaInformationWindow(Options? options, ref SubmissionInfo? submissionInfo)
        {
            var opts = options ?? new Options();
            var info = submissionInfo;

            var (r, updated) = global::Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var win = new MediaInformationWindow(opts, info);
                bool? res = await win.ShowDialog<bool?>(this);

                SubmissionInfo? outInfo = info;
                if (res == true)
                    outInfo = win.MediaInformationViewModel.SubmissionInfo.Clone() as SubmissionInfo;

                return (res, outInfo);
            }).GetAwaiter().GetResult();

            submissionInfo = updated;
            return r;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for CheckDumpButton Click event.
        /// Runs CheckDump on a background thread to avoid a UI-thread deadlock with the
        /// synchronous ShowMediaInformationWindow callback.
        /// </summary>
        private async void OnCheckDumpClick(object? sender, RoutedEventArgs e)
        {
            var result = await Task.Run(() => CheckDumpViewModel.CheckDump(ShowMediaInformationWindow));

            if (result == true)
            {
                bool? checkAgain = await MessageBoxWindow.ShowAsync(
                    this,
                    "Check Complete",
                    "The dump has been processed successfully! Would you like to check another dump?",
                    MessageBoxButtons.YesNo);

                // YesNo: true = Yes (check again), false = No (close)
                if (checkAgain == false)
                    Close();
                else
                    CheckDumpViewModel.Status = string.Empty;
            }
            else
            {
                string message = result.Message?.Length > 0
                    ? result.Message
                    : "Please check all files exist and try again!";
                await MessageBoxWindow.ShowAsync(this, "Check Failed", message, MessageBoxButtons.Ok);
            }
        }

        /// <summary>
        /// Handler for CancelButton Click event.
        /// </summary>
        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handler for DumpingProgramComboBox SelectionChanged event.
        /// </summary>
        public void DumpingProgramComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (CheckDumpViewModel.CanExecuteSelectionChanged)
                CheckDumpViewModel.ChangeDumpingProgram();
        }

        /// <summary>
        /// Handler for InputPathBrowseButton Click event.
        /// </summary>
        public async void InputPathBrowseButtonClick(object? sender, RoutedEventArgs e)
        {
            await BrowseFile();
            if (CheckDumpViewModel.CanExecuteSelectionChanged)
                CheckDumpViewModel.ChangeInputPath();
        }

        /// <summary>
        /// Handler for InputPathTextBox TextChanged event.
        /// </summary>
        public void InputPathTextBoxTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (CheckDumpViewModel.CanExecuteSelectionChanged)
                CheckDumpViewModel.ChangeInputPath();
        }

        /// <summary>
        /// Handler for SystemTypeComboBox SelectionChanged event.
        /// </summary>
        public void SystemTypeComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (CheckDumpViewModel.CanExecuteSelectionChanged)
                CheckDumpViewModel.ChangeSystem();
        }

        #endregion
    }
}
