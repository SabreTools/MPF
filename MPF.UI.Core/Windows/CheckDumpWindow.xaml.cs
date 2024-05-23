using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MPF.Core.Frontend.ViewModels;
using SabreTools.RedumpLib.Data;
using WPFCustomMessageBox;
using WinForms = System.Windows.Forms;

#pragma warning disable IDE1006 // Naming Styles

namespace MPF.UI.Core.Windows
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

        private ComboBox? _DumpingProgramComboBox => ItemHelper.FindChild<ComboBox>(this, "DumpingProgramComboBox");
        private Button? _InputPathBrowseButton => ItemHelper.FindChild<Button>(this, "InputPathBrowseButton");
        private TextBox? _InputPathTextBox => ItemHelper.FindChild<TextBox>(this, "InputPathTextBox");
        private ComboBox? _MediaTypeComboBox => ItemHelper.FindChild<ComboBox>(this, "MediaTypeComboBox");
        private ComboBox? _SystemTypeComboBox => ItemHelper.FindChild<ComboBox>(this, "SystemTypeComboBox");

        #endregion

        #region Controls

        private System.Windows.Controls.Button? _CheckDumpButton => ItemHelper.FindChild<System.Windows.Controls.Button>(this, "CheckDumpButton");
        private System.Windows.Controls.Button? _CancelButton => ItemHelper.FindChild<System.Windows.Controls.Button>(this, "CancelButton");

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
#if NET35
            _CheckDumpButton!.Click += OnCheckDumpClick;
            _CancelButton!.Click += OnCancelClick;
#else
            CheckDumpButton.Click += OnCheckDumpClick;
            CancelButton.Click += OnCancelClick;
#endif

            // User Area Click
#if NET35
            _InputPathBrowseButton!.Click += InputPathBrowseButtonClick;
#else
            InputPathBrowseButton.Click += InputPathBrowseButtonClick;
#endif

            // User Area SelectionChanged
#if NET35
            _SystemTypeComboBox!.SelectionChanged += SystemTypeComboBoxSelectionChanged;
            _MediaTypeComboBox!.SelectionChanged += MediaTypeComboBoxSelectionChanged;
            _DumpingProgramComboBox!.SelectionChanged += DumpingProgramComboBoxSelectionChanged;
#else
            SystemTypeComboBox.SelectionChanged += SystemTypeComboBoxSelectionChanged;
            MediaTypeComboBox.SelectionChanged += MediaTypeComboBoxSelectionChanged;
            DumpingProgramComboBox.SelectionChanged += DumpingProgramComboBoxSelectionChanged;
#endif

            // User Area TextChanged
#if NET35
            _InputPathTextBox!.TextChanged += InputPathTextBoxTextChanged;
#else
            InputPathTextBox.TextChanged += InputPathTextBoxTextChanged;
#endif
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
                Filter = "Disc Images|*.iso;*.cue;*.aaruf|All Files|*.*",
            };
            WinForms.DialogResult result = fileDialog.ShowDialog();

            if (result == WinForms.DialogResult.OK)
            {
                CheckDumpViewModel.InputPath = fileDialog.FileName;
            }
        }

        /// <summary>
        /// Display a user message using a CustomMessageBox
        /// </summary>
        /// <param name="title">Title to display to the user</param>
        /// <param name="message">Message to display to the user</param>
        /// <param name="optionCount">Number of options to display</param>
        /// <param name="flag">true for inquiry, false otherwise</param>
        /// <returns>true for positive, false for negative, null for neutral</returns>
        public bool? DisplayUserMessage(string title, string message, int optionCount, bool flag)
        {
            // Set the correct button style
            var button = optionCount switch
            {
                1 => MessageBoxButton.OK,
                2 => MessageBoxButton.YesNo,
                3 => MessageBoxButton.YesNoCancel,

                // This should not happen, but default to "OK"
                _ => MessageBoxButton.OK,
            };

            // Set the correct icon
            MessageBoxImage image = flag ? MessageBoxImage.Question : MessageBoxImage.Exclamation;

            // Display and get the result
            MessageBoxResult result = CustomMessageBox.Show(this, message, title, button, image);
            return result switch
            {
                MessageBoxResult.OK or MessageBoxResult.Yes => true,
                MessageBoxResult.No => false,
                _ => null,
            };
        }

        /// <summary>
        /// Show the disc information window
        /// </summary>
        /// <param name="submissionInfo">SubmissionInfo object to display and possibly change</param>
        /// <returns>Dialog open result</returns>
        public (bool?, SubmissionInfo?) ShowDiscInformationWindow(SubmissionInfo? submissionInfo)
        {
            var discInformationWindow = new DiscInformationWindow(CheckDumpViewModel.Options, submissionInfo)
            {
                Focusable = true,
                Owner = this,
                ShowActivated = true,
                ShowInTaskbar = true,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };

            discInformationWindow.Closed += delegate { this.Activate(); };
            bool? result = discInformationWindow.ShowDialog();

            // Copy back the submission info changes, if necessary
            if (result == true)
                submissionInfo = (discInformationWindow.DiscInformationViewModel.SubmissionInfo.Clone() as SubmissionInfo)!;

            return (result, submissionInfo!);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for CheckDumpButton Click event
        /// </summary>
#if NET40
        private void OnCheckDumpClick(object sender, EventArgs e)
        {
            var checkTask = CheckDumpViewModel.CheckDump(ShowDiscInformationWindow);
            checkTask.Wait();
            string? errorMessage = checkTask.Result;
#else
        private async void OnCheckDumpClick(object sender, EventArgs e)
        {
            string? errorMessage = await CheckDumpViewModel.CheckDump(ShowDiscInformationWindow);
#endif
            if (string.IsNullOrEmpty(errorMessage))
            {
                bool? checkAgain = DisplayUserMessage("Check Complete", "The dump has been processed successfully! Would you like to check another dump?", 2, false);
                if (checkAgain == false)
                    Close();
            }
            else
            {
                DisplayUserMessage("Check Failed", errorMessage!, 1, false);
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
        /// Handler for MediaTypeComboBox SelectionChanged event
        /// </summary>
        public void MediaTypeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CheckDumpViewModel.CanExecuteSelectionChanged)
                CheckDumpViewModel.ChangeMediaType();
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
