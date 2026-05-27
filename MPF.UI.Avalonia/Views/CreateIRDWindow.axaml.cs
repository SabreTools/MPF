using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MPF.Frontend.ViewModels;
using MPF.UI.Avalonia.Services;

namespace MPF.UI.Avalonia.Views
{
    /// <summary>
    /// Interaction logic for CreateIRDWindow.axaml
    /// </summary>
    public partial class CreateIRDWindow : WindowBase
    {
        /// <summary>
        /// Read-only access to the current create IRD view model
        /// </summary>
        public CreateIRDViewModel CreateIRDViewModel => (CreateIRDViewModel)DataContext!;

        /// <summary>
        /// Parameterless constructor required by the Avalonia XAML previewer.
        /// For runtime use, call the <see cref="CreateIRDWindow(MainWindow)"/> overload.
        /// </summary>
        public CreateIRDWindow() : this(null!) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">Owning MainWindow (mirrors WPF convention; VM loads its own options).</param>
        public CreateIRDWindow(MainWindow parent)
        {
            InitializeComponent();

            // The CreateIRDViewModel loads its own Options from config,
            // which mirrors what WPF does (the WPF ctor passes no options from parent).
            DataContext = new CreateIRDViewModel();
        }

        #region UI Functionality

        /// <summary>
        /// Look up a string resource by key, returning an empty string if missing.
        /// </summary>
        private string FindResourceString(string key)
            => this.TryFindResource(key, out var value) && value is string s ? s : string.Empty;

        /// <summary>
        /// Browse for an input ISO file path.
        /// Replicates WPF BrowseISOFile default-directory logic.
        /// </summary>
        public async Task BrowseISOFile()
        {
            string? currentPath = CreateIRDViewModel.InputPath;
            if (string.IsNullOrEmpty(currentPath) && !string.IsNullOrEmpty(CreateIRDViewModel.Options.Dumping.DefaultOutputPath))
                currentPath = CreateIRDViewModel.Options.Dumping.DefaultOutputPath!;
            if (string.IsNullOrEmpty(currentPath))
                currentPath = AppDomain.CurrentDomain.BaseDirectory!;

            string? selectedPath = await StorageDialogs.OpenFileAsync(
                this,
                FindResourceString("BrowseButtonString"),
                ("ISO", ["*.iso"]),
                ("All Files", ["*.*"]));

            if (!string.IsNullOrEmpty(selectedPath))
                CreateIRDViewModel.InputPath = selectedPath!;
        }

        /// <summary>
        /// Browse for a .getkey.log file path.
        /// Replicates WPF BrowseLogFile default-directory logic.
        /// </summary>
        public async Task BrowseLogFile()
        {
            string? currentPath = CreateIRDViewModel.LogPath;
            if (string.IsNullOrEmpty(currentPath) && !string.IsNullOrEmpty(CreateIRDViewModel.Options.Dumping.DefaultOutputPath))
                currentPath = CreateIRDViewModel.Options.Dumping.DefaultOutputPath!;
            if (string.IsNullOrEmpty(currentPath))
                currentPath = AppDomain.CurrentDomain.BaseDirectory!;

            string? selectedPath = await StorageDialogs.OpenFileAsync(
                this,
                FindResourceString("BrowseButtonString"),
                ("GetKey Log", ["*.getkey.log"]),
                ("All Files", ["*.*"]));

            if (!string.IsNullOrEmpty(selectedPath))
                CreateIRDViewModel.LogPath = selectedPath!;
        }

        /// <summary>
        /// Browse for a key file path.
        /// Replicates WPF BrowseKeyFile default-directory logic.
        /// </summary>
        public async Task BrowseKeyFile()
        {
            string? currentPath = CreateIRDViewModel.LogPath;
            if (string.IsNullOrEmpty(currentPath) && !string.IsNullOrEmpty(CreateIRDViewModel.Options.Dumping.DefaultOutputPath))
                currentPath = CreateIRDViewModel.Options.Dumping.DefaultOutputPath!;
            if (string.IsNullOrEmpty(currentPath))
                currentPath = AppDomain.CurrentDomain.BaseDirectory!;

            string? selectedPath = await StorageDialogs.OpenFileAsync(
                this,
                FindResourceString("BrowseButtonString"),
                ("Key", ["*.key"]),
                ("All Files", ["*.*"]));

            if (!string.IsNullOrEmpty(selectedPath))
                CreateIRDViewModel.KeyPath = selectedPath!;
        }

        /// <summary>
        /// Browse for an IRD output path (save dialog).
        /// Replicates WPF BrowseOutputFile logic.
        /// </summary>
        /// <returns>The chosen output path, or null if cancelled.</returns>
        public async Task<string?> BrowseOutputFile()
        {
            string? currentPath = CreateIRDViewModel.InputPath;
            if (string.IsNullOrEmpty(currentPath) && !string.IsNullOrEmpty(CreateIRDViewModel.Options.Dumping.DefaultOutputPath))
                currentPath = Path.Combine(CreateIRDViewModel.Options.Dumping.DefaultOutputPath!, "game.ird");
            else if (string.IsNullOrEmpty(currentPath))
                currentPath = "game.ird";
            if (string.IsNullOrEmpty(currentPath))
                currentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "game.ird");

            currentPath = Path.GetFullPath(currentPath);
            string filename = Path.ChangeExtension(Path.GetFileName(currentPath), ".ird");

            return await StorageDialogs.SaveFileAsync(
                this,
                FindResourceString("BrowseButtonString"),
                filename,
                ("IRD File", ["*.ird"]),
                ("All Files", ["*.*"]));
        }

        /// <summary>
        /// Browse for a PIC file path.
        /// Replicates WPF BrowsePICFile default-directory logic.
        /// </summary>
        public async Task BrowsePICFile()
        {
            string? currentPath = CreateIRDViewModel.LogPath;
            if (string.IsNullOrEmpty(currentPath) && !string.IsNullOrEmpty(CreateIRDViewModel.Options.Dumping.DefaultOutputPath))
                currentPath = CreateIRDViewModel.Options.Dumping.DefaultOutputPath!;
            if (string.IsNullOrEmpty(currentPath))
                currentPath = AppDomain.CurrentDomain.BaseDirectory!;

            string? selectedPath = await StorageDialogs.OpenFileAsync(
                this,
                FindResourceString("BrowseButtonString"),
                ("PIC", ["*.physical", "*_PIC.bin", "*.PIC"]),
                ("All Files", ["*.*"]));

            if (!string.IsNullOrEmpty(selectedPath))
                CreateIRDViewModel.PICPath = selectedPath!;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for CreateIRDButton Click event.
        /// Ports WPF OnCreateIRDClick: collapse expanders, disable UI, prompt for output path,
        /// call VM.CreateIRD (synchronous), then show success/failure message box.
        /// CreateIRD is CPU-bound but not async (no ProcessUserInfoDelegate), so we run it on
        /// a background thread to keep the UI responsive.
        /// </summary>
        private async void OnCreateIRDClick(object? sender, RoutedEventArgs e)
        {
            // Collapse all expanders (mirrors WPF)
            KeyExpander.IsExpanded = false;
            DiscIDExpander.IsExpanded = false;
            PICExpander.IsExpanded = false;

            // Save status and disable UI
            string tempStatus = CreateIRDViewModel.CreateIRDStatus;
            bool[] enabledFields = CreateIRDViewModel.DisableUIFields();
            CreateIRDViewModel.CreateIRDStatus = "Creating IRD... Please Wait";

            // Ask for output path (must be on UI thread)
            string? outputPath = await BrowseOutputFile();
            string? errorMessage = "Please provide an output path";

            if (outputPath is not null)
            {
                // Run the synchronous (CPU-bound) CreateIRD on a background thread
                errorMessage = await Task.Run(() => CreateIRDViewModel.CreateIRD(outputPath));
            }

            if (string.IsNullOrEmpty(errorMessage))
            {
                bool? checkAgain = await MessageBoxWindow.ShowAsync(
                    this,
                    "IRD Create",
                    "An IRD has been created successfully! Would you like to create another IRD?",
                    MessageBoxButtons.YesNo);

                // YesNo: true = Yes (create another), false = No (close)
                if (checkAgain == false)
                    Close();
                else
                    CreateIRDViewModel.ResetFields();
            }
            else
            {
                await MessageBoxWindow.ShowAsync(
                    this,
                    "Failed to create IRD",
                    errorMessage!,
                    MessageBoxButtons.Ok);

                CreateIRDViewModel.ReenableUIFields(enabledFields);
                CreateIRDViewModel.CreateIRDStatus = tempStatus;
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
        /// Handler for InputPathBrowseButton Click event.
        /// </summary>
        public async void InputPathBrowseButtonClick(object? sender, RoutedEventArgs e)
        {
            await BrowseISOFile();
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangeInputPath();
        }

        /// <summary>
        /// Handler for InputPathTextBox TextChanged event.
        /// </summary>
        public void InputPathTextBoxTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangeInputPath();
        }

        /// <summary>
        /// Handler for LogPathBrowseButton Click event.
        /// </summary>
        public async void LogPathBrowseButtonClick(object? sender, RoutedEventArgs e)
        {
            await BrowseLogFile();
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangeLogPath();
        }

        /// <summary>
        /// Handler for LogPathTextBox TextChanged event.
        /// </summary>
        public void LogPathTextBoxTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangeLogPath();
        }

        /// <summary>
        /// Handler for KeyPathBrowseButton Click event.
        /// </summary>
        public async void KeyPathBrowseButtonClick(object? sender, RoutedEventArgs e)
        {
            await BrowseKeyFile();
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangeKeyPath();
        }

        /// <summary>
        /// Handler for KeyPathTextBox TextChanged event.
        /// </summary>
        public void KeyPathTextBoxTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangeKeyPath();
        }

        /// <summary>
        /// Handler for KeyTextBox TextChanged event.
        /// </summary>
        public void KeyTextBoxTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangeKey();
        }

        /// <summary>
        /// Handler for DiscIDTextBox TextChanged event.
        /// </summary>
        public void DiscIDTextBoxTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangeDiscID();
        }

        /// <summary>
        /// Handler for PICPathBrowseButton Click event.
        /// </summary>
        public async void PICPathBrowseButtonClick(object? sender, RoutedEventArgs e)
        {
            await BrowsePICFile();
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangePICPath();
        }

        /// <summary>
        /// Handler for PICPathTextBox TextChanged event.
        /// </summary>
        public void PICPathTextBoxTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangePICPath();
        }

        /// <summary>
        /// Handler for PICTextBox TextChanged event.
        /// </summary>
        public void PICTextBoxTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangePIC();
        }

        /// <summary>
        /// Handler for LayerbreakTextBox TextChanged event.
        /// </summary>
        public void LayerbreakTextBoxTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (CreateIRDViewModel.CanExecuteSelectionChanged)
                CreateIRDViewModel.ChangeLayerbreak();
        }

        #endregion
    }
}
