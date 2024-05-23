using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MPF.Core.Frontend.ViewModels;
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
        private TextBox? _DiscIDTextBox => ItemHelper.FindChild<TextBox>(this, "DiscIDTextBox");
        private Button? _PICPathBrowseButton => ItemHelper.FindChild<Button>(this, "PICPathBrowseButton");
        private TextBox? _PICPathTextBox => ItemHelper.FindChild<TextBox>(this, "PICPathTextBox");
        private TextBox? _PICTextBox => ItemHelper.FindChild<TextBox>(this, "PICTextBox");
        private TextBox? _LayerbreakTextBox => ItemHelper.FindChild<TextBox>(this, "LayerbreakTextBox");

        #endregion

        #region Controls

        private System.Windows.Controls.Button? _CreateIRDButton => ItemHelper.FindChild<System.Windows.Controls.Button>(this, "CreateIRDButton");
        private System.Windows.Controls.Button? _CancelButton => ItemHelper.FindChild<System.Windows.Controls.Button>(this, "CancelButton");

        #endregion

        #region Expanders

        private Expander? _KeyExpander => ItemHelper.FindChild<Expander>(this, "KeyExpander");
        private Expander? _DiscIDExpander => ItemHelper.FindChild<Expander>(this, "DiscIDExpander");
        private Expander? _PICExpander => ItemHelper.FindChild<Expander>(this, "PICExpander");

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
            _PICPathBrowseButton!.Click += PICPathBrowseButtonClick;
#else
            InputPathBrowseButton.Click += InputPathBrowseButtonClick;
            LogPathBrowseButton.Click += LogPathBrowseButtonClick;
            KeyPathBrowseButton.Click += KeyPathBrowseButtonClick;
            PICPathBrowseButton.Click += PICPathBrowseButtonClick;
#endif

            // User Area TextChanged
#if NET35
            _InputPathTextBox!.TextChanged += InputPathTextBoxTextChanged;
            _LogPathTextBox!.TextChanged += LogPathTextBoxTextChanged;
            _KeyPathTextBox!.TextChanged += KeyPathTextBoxTextChanged;
            _KeyTextBox!.TextChanged += KeyTextBoxTextChanged;
            _DiscIDTextBox!.TextChanged += DiscIDTextBoxTextChanged;
            _PICPathTextBox!.TextChanged += PICPathTextBoxTextChanged;
            _PICTextBox!.TextChanged += PICTextBoxTextChanged;
            _LayerbreakTextBox!.TextChanged += LayerbreakTextBoxTextChanged;
#else
            InputPathTextBox.TextChanged += InputPathTextBoxTextChanged;
            LogPathTextBox.TextChanged += LogPathTextBoxTextChanged;
            KeyPathTextBox.TextChanged += KeyPathTextBoxTextChanged;
            KeyTextBox.TextChanged += KeyTextBoxTextChanged;
            DiscIDTextBox.TextChanged += DiscIDTextBoxTextChanged;
            PICPathTextBox.TextChanged += PICPathTextBoxTextChanged;
            PICTextBox.TextChanged += PICTextBoxTextChanged;
            LayerbreakTextBox.TextChanged += LayerbreakTextBoxTextChanged;
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
                Filter = "ISO|*.iso|All Files|*.*",
            };
            WinForms.DialogResult result = fileDialog.ShowDialog();

            if (result == WinForms.DialogResult.OK)
            {
                CreateIRDViewModel.InputPath = fileDialog.FileName;
            }
        }

        /// <summary>
        /// Browse for an .getkey.log file path
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
                Filter = "GetKey Log|*.getkey.log|All Files|*.*",
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
                Filter = "Key|*.key|All Files|*.*",
            };
            WinForms.DialogResult result = fileDialog.ShowDialog();

            if (result == WinForms.DialogResult.OK)
            {
                CreateIRDViewModel.KeyPath = fileDialog.FileName;
            }
        }

        /// <summary>
        /// Browse for an IRD output path
        /// </summary>
        /// <returns>Output path if provided, else null</returns>
        public string? BrowseOutputFile()
        {
            // Get the current path, if possible
            string? currentPath = CreateIRDViewModel.InputPath;
            if (string.IsNullOrEmpty(currentPath) && !string.IsNullOrEmpty(CreateIRDViewModel.Options.DefaultOutputPath))
                currentPath = Path.Combine(CreateIRDViewModel.Options.DefaultOutputPath, "game.ird");
            else if (string.IsNullOrEmpty(currentPath))
                currentPath = "game.ird";
            if (string.IsNullOrEmpty(currentPath))
                currentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "game.ird");

            // Get the full path
            currentPath = Path.GetFullPath(currentPath);

            // Get the directory
            var directory = Path.GetDirectoryName(currentPath);

            // Get the filename
            string filename = Path.ChangeExtension(Path.GetFileName(currentPath), ".ird");

            WinForms.FileDialog fileDialog = new WinForms.SaveFileDialog
            {
                FileName = filename,
                InitialDirectory = directory,
                Filter = "IRD File|*.ird|All Files|*.*"
            };
            WinForms.DialogResult result = fileDialog.ShowDialog();
            if (result == WinForms.DialogResult.OK)
                return fileDialog.FileName;
            else
                return null;
        }

        /// <summary>
        /// Browse for an PIC file path
        /// </summary>
        public void BrowsePICFile()
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
                Filter = "PIC|*.physical;*_PIC.bin;*.PIC|All Files|*.*",
            };
            WinForms.DialogResult result = fileDialog.ShowDialog();

            if (result == WinForms.DialogResult.OK)
            {
                CreateIRDViewModel.PICPath = fileDialog.FileName;
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
#if NET35
            if (_KeyExpander != null) _KeyExpander.IsExpanded = false;
            if (_DiscIDExpander != null) _DiscIDExpander.IsExpanded = false;
            if (_PICExpander != null) _PICExpander.IsExpanded = false;
#else
            KeyExpander.IsExpanded = false;
            DiscIDExpander.IsExpanded = false;
            PICExpander.IsExpanded = false;
#endif
            string tempStatus = CreateIRDViewModel.CreateIRDStatus;
            bool[] enabledFields = CreateIRDViewModel.DisableUIFields();
            CreateIRDViewModel.CreateIRDStatus = "Creating IRD... Please Wait";
            string? outputPath = BrowseOutputFile();
            string? errorMessage = "Please provide an output path";
            if (outputPath != null)
            {
                errorMessage = CreateIRDViewModel.CreateIRD(outputPath);
            }
            if (string.IsNullOrEmpty(errorMessage))
            {
                bool? checkAgain = DisplayUserMessage("IRD Create", "An IRD has been created successfully! Would you like to create another IRD?", 2, false);
                if (checkAgain == false)
                    Close();
                else
                    CreateIRDViewModel.ResetFields();
            }
            else
            {
                DisplayUserMessage("Failed to create IRD", errorMessage!, 1, false);
                CreateIRDViewModel.ReenableUIFields(enabledFields);
                CreateIRDViewModel.CreateIRDStatus = tempStatus;
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
        /// Handler for DiscIDTextBox TextChanged event
        /// </summary>
        public void DiscIDTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangeDiscID();
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

        /// <summary>
        /// Handler for PICPathBrowseButton Click event
        /// </summary>
        public void PICPathBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            BrowsePICFile();
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangePICPath();
        }

        /// <summary>
        /// Handler for PICPathTextBox TextChanged event
        /// </summary>
        public void PICPathTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangePICPath();
        }

        /// <summary>
        /// Handler for PICTextBox TextChanged event
        /// </summary>
        public void PICTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangePIC();
        }

        /// <summary>
        /// Handler for LayerbreakTextBox TextChanged event
        /// </summary>
        public void LayerbreakTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangeLayerbreak();
        }

        #endregion
    }
}
