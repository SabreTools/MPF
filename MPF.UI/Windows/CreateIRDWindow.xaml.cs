using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MPF.Frontend.ViewModels;
using WinForms = System.Windows.Forms;

namespace MPF.UI.Windows
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

        private Button? InputPathBrowseButton => ItemHelper.FindChild<Button>(this, "InputPathBrowseButton");
        private TextBox? InputPathTextBox => ItemHelper.FindChild<TextBox>(this, "InputPathTextBox");

        private Button? LogPathBrowseButton => ItemHelper.FindChild<Button>(this, "LogPathBrowseButton");
        private TextBox? LogPathTextBox => ItemHelper.FindChild<TextBox>(this, "LogPathTextBox");
        private Button? KeyPathBrowseButton => ItemHelper.FindChild<Button>(this, "KeyPathBrowseButton");
        private TextBox? KeyPathTextBox => ItemHelper.FindChild<TextBox>(this, "KeyPathTextBox");
        private TextBox? KeyTextBox => ItemHelper.FindChild<TextBox>(this, "KeyTextBox");
        private TextBox? DiscIDTextBox => ItemHelper.FindChild<TextBox>(this, "DiscIDTextBox");
        private Button? PICPathBrowseButton => ItemHelper.FindChild<Button>(this, "PICPathBrowseButton");
        private TextBox? PICPathTextBox => ItemHelper.FindChild<TextBox>(this, "PICPathTextBox");
        private TextBox? PICTextBox => ItemHelper.FindChild<TextBox>(this, "PICTextBox");
        private TextBox? LayerbreakTextBox => ItemHelper.FindChild<TextBox>(this, "LayerbreakTextBox");

        #endregion

        #region Controls

        private System.Windows.Controls.Button? CreateIRDButton => ItemHelper.FindChild<System.Windows.Controls.Button>(this, "CreateIRDButton");
        private System.Windows.Controls.Button? CancelButton => ItemHelper.FindChild<System.Windows.Controls.Button>(this, "CancelButton");

        #endregion

        #region Expanders

        private Expander? KeyExpander => ItemHelper.FindChild<Expander>(this, "KeyExpander");
        private Expander? DiscIDExpander => ItemHelper.FindChild<Expander>(this, "DiscIDExpander");
        private Expander? PICExpander => ItemHelper.FindChild<Expander>(this, "PICExpander");

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
            CreateIRDButton!.Click += OnCreateIRDClick;
            CancelButton!.Click += OnCancelClick;

            // User Area Click
            InputPathBrowseButton!.Click += InputPathBrowseButtonClick;
            LogPathBrowseButton!.Click += LogPathBrowseButtonClick;
            KeyPathBrowseButton!.Click += KeyPathBrowseButtonClick;
            PICPathBrowseButton!.Click += PICPathBrowseButtonClick;

            // User Area TextChanged
            InputPathTextBox!.TextChanged += InputPathTextBoxTextChanged;
            LogPathTextBox!.TextChanged += LogPathTextBoxTextChanged;
            KeyPathTextBox!.TextChanged += KeyPathTextBoxTextChanged;
            KeyTextBox!.TextChanged += KeyTextBoxTextChanged;
            DiscIDTextBox!.TextChanged += DiscIDTextBoxTextChanged;
            PICPathTextBox!.TextChanged += PICPathTextBoxTextChanged;
            PICTextBox!.TextChanged += PICTextBoxTextChanged;
            LayerbreakTextBox!.TextChanged += LayerbreakTextBoxTextChanged;
        }

        /// <summary>
        /// Browse for an input ISO file path
        /// </summary>
        public void BrowseISOFile()
        {
            // Get the current path, if possible
            string? currentPath = CreateIRDViewModel.InputPath;
            if (string.IsNullOrEmpty(currentPath) && !string.IsNullOrEmpty(CreateIRDViewModel.Options.Dumping.DefaultOutputPath))
                currentPath = CreateIRDViewModel.Options.Dumping.DefaultOutputPath!;
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
            if (string.IsNullOrEmpty(currentPath) && !string.IsNullOrEmpty(CreateIRDViewModel.Options.Dumping.DefaultOutputPath))
                currentPath = CreateIRDViewModel.Options.Dumping.DefaultOutputPath!;
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
            if (string.IsNullOrEmpty(currentPath) && !string.IsNullOrEmpty(CreateIRDViewModel.Options.Dumping.DefaultOutputPath))
                currentPath = CreateIRDViewModel.Options.Dumping.DefaultOutputPath!;
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
            if (string.IsNullOrEmpty(currentPath) && !string.IsNullOrEmpty(CreateIRDViewModel.Options.Dumping.DefaultOutputPath))
                currentPath = Path.Combine(CreateIRDViewModel.Options.Dumping.DefaultOutputPath, "game.ird");
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
            if (string.IsNullOrEmpty(currentPath) && !string.IsNullOrEmpty(CreateIRDViewModel.Options.Dumping.DefaultOutputPath))
                currentPath = CreateIRDViewModel.Options.Dumping.DefaultOutputPath!;
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

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for CreateIRDButton Click event
        /// </summary>
        private void OnCreateIRDClick(object sender, EventArgs e)
        {
            KeyExpander?.IsExpanded = false;
            DiscIDExpander?.IsExpanded = false;
            PICExpander?.IsExpanded = false;

            string tempStatus = CreateIRDViewModel.CreateIRDStatus;
            bool[] enabledFields = CreateIRDViewModel.DisableUIFields();
            CreateIRDViewModel.CreateIRDStatus = "Creating IRD... Please Wait";
            string? outputPath = BrowseOutputFile();
            string? errorMessage = "Please provide an output path";
            if (outputPath is not null)
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
