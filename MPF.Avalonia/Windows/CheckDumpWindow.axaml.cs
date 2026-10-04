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
    /// <summary>
    /// Interaction logic for CheckDumpWindow.axaml
    /// </summary>
    public partial class CheckDumpWindow : WindowBase
    {
        /// <summary>
        /// File picker types for browsing dump input files
        /// </summary>
        private static readonly List<FilePickerFileType> InputFileTypes =
        [
            new("Disc Images") { Patterns = ["*.iso", "*.cue", "*.aaruf", "*.bca"] },
            new("Log Archives") { Patterns = ["*_logs.zip"] },
            FilePickerFileTypes.All,
        ];

        /// <summary>
        /// Parent window used as the dialog owner, if any
        /// </summary>
#pragma warning disable IDE0052 // Remove unread private members
        private MainWindow? _parent;
#pragma warning restore IDE0052 // Remove unread private members

        /// <summary>
        /// Read-only access to the current check dump view model
        /// </summary>
        public CheckDumpViewModel CheckDumpViewModel => DataContext as CheckDumpViewModel ?? new CheckDumpViewModel();

        public CheckDumpWindow()
        {
            InitializeWindow(null);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public CheckDumpWindow(MainWindow parent)
        {
            InitializeWindow(parent);
        }

        /// <summary>
        /// Shared initialization for both constructors; sets the view model and wires events once opened
        /// </summary>
        private void InitializeWindow(MainWindow? parent)
        {
            InitializeComponent();
            _parent = parent;
            DataContext = new CheckDumpViewModel();
        }

        #region UI Functionality

        /// <summary>
        /// Show the media information window
        /// </summary>
        /// <param name="options">Options set to pass to the information window</param>
        /// <param name="submissionInfo">SubmissionInfo object to display and possibly change</param>
        /// <returns>Dialog open result</returns>
        private bool? ShowMediaInformationWindow(Options? options, ref SubmissionInfo? submissionInfo)
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
                var window = new MediaInformationWindow(options ?? CheckDumpViewModel.Options, submissionInfo)
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
                    submissionInfo = window.MediaInformationViewModel.SubmissionInfo.Clone() as SubmissionInfo;

                return result;
            }
            else
            {
                SubmissionInfo? updatedSubmissionInfo = submissionInfo;
                var dispatcherTask = Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    var window = new MediaInformationWindow(options ?? CheckDumpViewModel.Options, updatedSubmissionInfo)
                    {
                        Focusable = true,
                        ShowActivated = true,
                        ShowInTaskbar = true,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    };

                    window.Closed += delegate { Activate(); };

                    bool? result = await window.ShowDialog<bool?>(this);
                    if (result == true)
                        updatedSubmissionInfo = window.MediaInformationViewModel.SubmissionInfo.Clone() as SubmissionInfo;

                    return result;
                });

                bool? result = dispatcherTask.GetAwaiter().GetResult();
                submissionInfo = updatedSubmissionInfo;
                return result;
            }
        }

        /// <summary>
        /// Browse for an input file path
        /// </summary>
        private async Task<string?> BrowseFileAsync()
        {
            return await DialogService.OpenFileAsync(this, StringResource("InputPathLabelString", "Input Path"), InputFileTypes);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for CheckDumpButton Click event
        /// </summary>
        private async void OnCheckDumpClick(object? sender, RoutedEventArgs e)
        {
            var result = await CheckDumpViewModel.CheckDump(ShowMediaInformationWindow);
            if (result == true)
            {
                bool? checkAgain = await DisplayUserMessageAsync("Check Complete", "The dump has been processed successfully! Would you like to check another dump?", 2, false);
                if (checkAgain == false)
                    Close();
                else
                    CheckDumpViewModel.Status = string.Empty;
            }
            else
            {
                await DisplayUserMessageAsync("Check Failed", string.IsNullOrEmpty(result.Message) ? "Please check all files exist and try again!" : result.Message, 1, false);
            }
        }

        /// <summary>
        /// Handler for CancelButton Click event
        /// </summary>
        private void OnCancelClick(object? sender, RoutedEventArgs e)
            => Close(false);

        /// <summary>
        /// Handler for DumpingProgramComboBox SelectionChanged event
        /// </summary>
        public void DumpingProgramComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (CheckDumpViewModel.CanExecuteSelectionChanged)
                CheckDumpViewModel.ChangeDumpingProgram();
        }

        /// <summary>
        /// Handler for InputPathBrowseButton Click event
        /// </summary>
        public async void InputPathBrowseButtonClick(object? sender, RoutedEventArgs e)
        {
            string? selectedPath = await BrowseFileAsync();
            if (!string.IsNullOrWhiteSpace(selectedPath))
                CheckDumpViewModel.InputPath = selectedPath;

            if (CheckDumpViewModel.CanExecuteSelectionChanged)
                CheckDumpViewModel.ChangeInputPath();
        }

        /// <summary>
        /// Handler for InputPathTextBox TextChanged event
        /// </summary>
        public void InputPathTextBoxTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (CheckDumpViewModel.CanExecuteSelectionChanged)
                CheckDumpViewModel.ChangeInputPath();
        }

        /// <summary>
        /// Handler for SystemTypeComboBox SelectionChanged event
        /// </summary>
        public void SystemTypeComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (CheckDumpViewModel.CanExecuteSelectionChanged)
                CheckDumpViewModel.ChangeSystem();
        }

        #endregion
    }
}
