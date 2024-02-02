using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MPF.Core.UI.ViewModels;
using SabreTools.RedumpLib.Data;
using WPFCustomMessageBox;
using WinForms = System.Windows.Forms;

#pragma warning disable IDE1006 // Naming Styles

namespace MPF.UI.Core.Windows
{
    /// <summary>
    /// Interaction logic for CreateIRDWindow.xaml
    /// </summary>
    public partial class CreateIRDWindow : WindowBase
    {
        /// <summary>
        /// Read-only access to the current check dump view model
        /// </summary>
        public CreateIRDViewModel CreateIRDViewModel => DataContext as CreateIRDViewModel ?? new CreateIRDViewModel();

#if NET35

        #region Settings

        private Button? _InputPathBrowseButton => ItemHelper.FindChild<Button>(this, "InputPathBrowseButton");
        private TextBox? _InputPathTextBox => ItemHelper.FindChild<TextBox>(this, "InputPathTextBox");
        private Button? _LogPathBrowseButton => ItemHelper.FindChild<Button>(this, "LogPathBrowseButton");
        private TextBox? _LogPathTextBox => ItemHelper.FindChild<TextBox>(this, "LogPathTextBox");
        private Button? _KeyPathBrowseButton => ItemHelper.FindChild<Button>(this, "KeyPathBrowseButton");
        private TextBox? _KeyPathTextBox => ItemHelper.FindChild<TextBox>(this, "KeyPathTextBox");
        private TextBox? _KeyTextBox => ItemHelper.FindChild<TextBox>(this, "KeyTextBox");

        #endregion

        #region Controls

        private System.Windows.Controls.Button? _CreateIRDButton => ItemHelper.FindChild<System.Windows.Controls.Button>(this, "CreateIRDButton");
        private System.Windows.Controls.Button? _CancelButton => ItemHelper.FindChild<System.Windows.Controls.Button>(this, "CancelButton");

        #endregion
        
#endif

        /// <summary>
        /// Constructor
        /// </summary>
        public CreateIRDWindow(MainWindow parent)
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
            _CreateIRDButton!.Click += OnCreateIRDClick;
            _CancelButton!.Click += OnCancelClick;
#else
            CreateIRDButton.Click += OnCreateIRDClick;
            CancelButton.Click += OnCancelClick;
#endif

            // User Area Click
#if NET35
            _InputPathBrowseButton!.Click += InputPathBrowseButtonClick;
            _LogPathBrowseButton!.Click += LogPathBrowseButtonClick;
            _KeyPathBrowseButton!.Click += KeyPathBrowseButtonClick;
#else
            InputPathBrowseButton.Click += InputPathBrowseButtonClick;
            LogPathBrowseButton.Click += LogPathBrowseButtonClick;
            KeyPathBrowseButton.Click += KeyPathBrowseButtonClick;
#endif

            // User Area TextChanged
#if NET35
            _InputPathTextBox!.TextChanged += InputPathTextBoxTextChanged;
            _LogPathTextBox!.TextChanged += LogPathTextBoxTextChanged;
            _KeyPathTextBox!.TextChanged += KeyPathTextBoxTextChanged;
            _KeyTextBox!.TextChanged += KeyTextBoxTextChanged;
#else
            InputPathTextBox.TextChanged += InputPathTextBoxTextChanged;
            LogPathTextBox.TextChanged += LogPathTextBoxTextChanged;
            KeyPathTextBox.TextChanged += KeyPathTextBoxTextChanged;
            KeyTextBox.TextChanged += KeyTextBoxTextChanged;
#endif
        }

        /// <summary>
        /// Browse for an input ISO file path
        /// </summary>
        public void BrowseISOFile()
        {
            // Get the current path, if possible
            string? currentPath = CreateIRDViewModel.InputPath;
            if (string.IsNullOrEmpty(currentPath) && !string.IsNullOrEmpty(CreateIRDViewModel.Options.DefaultOutputPath))
                currentPath = CreateIRDViewModel.Options.DefaultOutputPath!;
            if (string.IsNullOrEmpty(currentPath))
                currentPath = AppDomain.CurrentDomain.BaseDirectory!;

            // Get the full directory
            var directory = Path.GetDirectoryName(Path.GetFullPath(currentPath));

            WinForms.FileDialog fileDialog = new WinForms.OpenFileDialog
            {
                InitialDirectory = directory,
            };
            WinForms.DialogResult result = fileDialog.ShowDialog();

            if (result == WinForms.DialogResult.OK)
            {
                CreateIRDViewModel.InputPath = fileDialog.FileName;
            }
        }

        /// <summary>
        /// Browse for an ManaGunZ log file path
        /// </summary>
        public void BrowseLogFile()
        {
            // Get the current path, if possible
            string? currentPath = CreateIRDViewModel.LogPath;
            if (string.IsNullOrEmpty(currentPath) && !string.IsNullOrEmpty(CreateIRDViewModel.Options.DefaultOutputPath))
                currentPath = CreateIRDViewModel.Options.DefaultOutputPath!;
            if (string.IsNullOrEmpty(currentPath))
                currentPath = AppDomain.CurrentDomain.BaseDirectory!;

            // Get the full directory
            var directory = Path.GetDirectoryName(Path.GetFullPath(currentPath));

            WinForms.FileDialog fileDialog = new WinForms.OpenFileDialog
            {
                InitialDirectory = directory,
            };
            WinForms.DialogResult result = fileDialog.ShowDialog();

            if (result == WinForms.DialogResult.OK)
            {
                CreateIRDViewModel.LogPath = fileDialog.FileName;
            }
        }

        /// <summary>
        /// Browse for an key file path
        /// </summary>
        public void BrowseKeyFile()
        {
            // Get the current path, if possible
            string? currentPath = CreateIRDViewModel.LogPath;
            if (string.IsNullOrEmpty(currentPath) && !string.IsNullOrEmpty(CreateIRDViewModel.Options.DefaultOutputPath))
                currentPath = CreateIRDViewModel.Options.DefaultOutputPath!;
            if (string.IsNullOrEmpty(currentPath))
                currentPath = AppDomain.CurrentDomain.BaseDirectory!;

            // Get the full directory
            var directory = Path.GetDirectoryName(Path.GetFullPath(currentPath));

            WinForms.FileDialog fileDialog = new WinForms.OpenFileDialog
            {
                InitialDirectory = directory,
            };
            WinForms.DialogResult result = fileDialog.ShowDialog();

            if (result == WinForms.DialogResult.OK)
            {
                CreateIRDViewModel.KeyPath = fileDialog.FileName;
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

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for CreateIRDButton Click event
        /// </summary>
        private void OnCreateIRDClick(object sender, EventArgs e)
        {
            string? errorMessage = CreateIRDViewModel.CreateIRD();
            if (string.IsNullOrEmpty(errorMessage))
            {
                bool? checkAgain = DisplayUserMessage("IRD Create", "An IRD has been created successfully! Would you like to create another IRD?", 2, false);
                if (checkAgain == false)
                    Close();
            }
            else
            {
                DisplayUserMessage("Failed to create IRD", errorMessage!, 1, false);
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
        /// Handler for InputPathBrowseButton Click event
        /// </summary>
        public void InputPathBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            BrowseISOFile();
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangeInputPath();
        }

        /// <summary>
        /// Handler for InputPathTextBox TextChanged event
        /// </summary>
        public void InputPathTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangeInputPath();
        }

        /// <summary>
        /// Handler for LogPathBrowseButton Click event
        /// </summary>
        public void LogPathBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            BrowseLogFile();
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangeLogPath();
        }

        /// <summary>
        /// Handler for LogPathTextBox TextChanged event
        /// </summary>
        public void LogPathTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangeLogPath();
        }

        /// <summary>
        /// Handler for KeyPathBrowseButton Click event
        /// </summary>
        public void KeyPathBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            BrowseKeyFile();
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangeKeyPath();
        }

        /// <summary>
        /// Handler for KeyPathTextBox TextChanged event
        /// </summary>
        public void KeyPathTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangeKeyPath();
        }

        /// <summary>
        /// Handler for KeyTextBox TextChanged event
        /// </summary>
        public void KeyTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangeKey();
        }

        #endregion
    }
}
