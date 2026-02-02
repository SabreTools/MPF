using System;
using System.IO;
#if NET40
using System.Threading.Tasks;
#endif
using System.Windows;
using System.Windows.Controls;
using MPF.Frontend;
using MPF.Frontend.ViewModels;
using SabreTools.RedumpLib.Data;
using WinForms = System.Windows.Forms;

namespace MPF.UI.Windows
{
    /// <summary>
    /// Interaction logic for CheckDumpWindow.xaml
    /// </summary>
    public partial class CheckDumpWindow : WindowBase
    {
        /// <summary>
        /// Read-only access to the current check dump view model
        /// </summary>
        public CheckDumpViewModel CheckDumpViewModel => DataContext as CheckDumpViewModel ?? new CheckDumpViewModel();

#if NET35

        #region Settings

        private ComboBox? DumpingProgramComboBox => ItemHelper.FindChild<ComboBox>(this, "DumpingProgramComboBox");
        private Button? InputPathBrowseButton => ItemHelper.FindChild<Button>(this, "InputPathBrowseButton");
        private TextBox? InputPathTextBox => ItemHelper.FindChild<TextBox>(this, "InputPathTextBox");
        private ComboBox? SystemTypeComboBox => ItemHelper.FindChild<ComboBox>(this, "SystemTypeComboBox");

        #endregion

        #region Controls

        private System.Windows.Controls.Button? CheckDumpButton => ItemHelper.FindChild<System.Windows.Controls.Button>(this, "CheckDumpButton");
        private System.Windows.Controls.Button? CancelButton => ItemHelper.FindChild<System.Windows.Controls.Button>(this, "CancelButton");

        #endregion

#endif

        /// <summary>
        /// Constructor
        /// </summary>
        public CheckDumpWindow(MainWindow parent)
        {
#if NET40_OR_GREATER || NETCOREAPP
            InitializeComponent();
#endif

#if NET452_OR_GREATER || NETCOREAPP
            var chrome = new System.Windows.Shell.WindowChrome
            {
                CaptionHeight = 0,
                ResizeBorderThickness = new Thickness(0),
            };
            System.Windows.Shell.WindowChrome.SetWindowChrome(this, chrome);
#endif
        }

        /// <summary>
        /// Handler for CheckDumpWindow OnContentRendered event
        /// </summary>
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // Add the click handlers to the UI
            AddEventHandlers();
        }

        #region UI Functionality

        /// <summary>
        /// Add all event handlers
        /// </summary>
        public void AddEventHandlers()
        {
            // Main buttons
            CheckDumpButton!.Click += OnCheckDumpClick;
            CancelButton!.Click += OnCancelClick;

            // User Area Click
            InputPathBrowseButton!.Click += InputPathBrowseButtonClick;

            // User Area SelectionChanged
            SystemTypeComboBox!.SelectionChanged += SystemTypeComboBoxSelectionChanged;
            DumpingProgramComboBox!.SelectionChanged += DumpingProgramComboBoxSelectionChanged;

            // User Area TextChanged
            InputPathTextBox!.TextChanged += InputPathTextBoxTextChanged;
        }

        /// <summary>
        /// Browse for an input file path
        /// </summary>
        public void BrowseFile()
        {
            // Get the current path, if possible
            string? currentPath = CheckDumpViewModel.InputPath;
            if (string.IsNullOrEmpty(currentPath) && !string.IsNullOrEmpty(CheckDumpViewModel.Options.DefaultOutputPath))
                currentPath = CheckDumpViewModel.Options.DefaultOutputPath!;
            if (string.IsNullOrEmpty(currentPath))
                currentPath = AppDomain.CurrentDomain.BaseDirectory!;

            // Get the full directory
            var directory = Path.GetDirectoryName(Path.GetFullPath(currentPath));

            WinForms.FileDialog fileDialog = new WinForms.OpenFileDialog
            {
                InitialDirectory = directory,
                Filter = "Disc Images|*.iso;*.cue;*.aaruf;*.bca|Log Archives|*_logs.zip|All Files|*.*",
            };
            WinForms.DialogResult result = fileDialog.ShowDialog();

            if (result == WinForms.DialogResult.OK)
            {
                CheckDumpViewModel.InputPath = fileDialog.FileName;
            }
        }

        /// <summary>
        /// Show the media information window
        /// </summary>
        /// <param name="options">Options set to pass to the information window</param>
        /// <param name="submissionInfo">SubmissionInfo object to display and possibly change</param>
        /// <returns>Dialog open result</returns>
        public bool? ShowMediaInformationWindow(Options? options, ref SubmissionInfo? submissionInfo)
        {
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

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for CheckDumpButton Click event
        /// </summary>
        private async void OnCheckDumpClick(object sender, EventArgs e)
        {
            var result = await CheckDumpViewModel.CheckDump(ShowMediaInformationWindow);
            if (result == true)
            {
                bool? checkAgain = DisplayUserMessage("Check Complete", "The dump has been processed successfully! Would you like to check another dump?", 2, false);
                if (checkAgain == false)
                    Close();
                else
                    CheckDumpViewModel.Status = string.Empty;
            }
            else
            {
                string message = result.Message.Length > 0 ? result.Message : "Please check all files exist and try again!";
                DisplayUserMessage("Check Failed", message, 1, false);
            }
        }

        /// <summary>
        /// Handler for CancelButtom Click event
        /// </summary>
        private void OnCancelClick(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handler for DumpingProgramComboBox SelectionChanged event
        /// </summary>
        public void DumpingProgramComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CheckDumpViewModel.CanExecuteSelectionChanged)
                CheckDumpViewModel.ChangeDumpingProgram();
        }

        /// <summary>
        /// Handler for InputPathBrowseButton Click event
        /// </summary>
        public void InputPathBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            BrowseFile();
            if (CheckDumpViewModel.CanExecuteSelectionChanged)
                CheckDumpViewModel.ChangeInputPath();
        }

        /// <summary>
        /// Handler for InputPathTextBox TextChanged event
        /// </summary>
        public void InputPathTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (CheckDumpViewModel.CanExecuteSelectionChanged)
                CheckDumpViewModel.ChangeInputPath();
        }

        /// <summary>
        /// Handler for SystemTypeComboBox SelectionChanged event
        /// </summary>
        public void SystemTypeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CheckDumpViewModel.CanExecuteSelectionChanged)
                CheckDumpViewModel.ChangeSystem();
        }

        #endregion
    }
}
