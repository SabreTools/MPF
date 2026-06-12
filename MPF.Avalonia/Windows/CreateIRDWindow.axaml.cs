using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MPF.Avalonia.Services;
using MPF.Frontend.ViewModels;

namespace MPF.Avalonia.Windows
{
    /// <summary>
    /// Interaction logic for CreateIRDWindow.axaml
    /// </summary>
    public partial class CreateIRDWindow : WindowBase
    {
        /// <summary>
        /// File picker types for saving an IRD file
        /// </summary>
        private static readonly List<FilePickerFileType> IrdFileTypes =
        [
            new("IRD File") { Patterns = ["*.ird"] },
        ];

        /// <summary>
        /// Read-only access to the current create IRD view model
        /// </summary>
        public CreateIRDViewModel CreateIRDViewModel => DataContext as CreateIRDViewModel ?? new CreateIRDViewModel();

        public CreateIRDWindow()
        {
            InitializeWindow();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public CreateIRDWindow(MainWindow parent)
        {
            InitializeWindow();
        }

        /// <summary>
        /// Shared initialization for both constructors; sets the view model and wires events once opened
        /// </summary>
        private void InitializeWindow()
        {
            InitializeComponent();
            DataContext = new CreateIRDViewModel();
            Opened += WireEvents;
        }

        #region UI Functionality

        /// <summary>
        /// Add all event handlers once the window has opened
        /// </summary>
        private void WireEvents(object? sender, EventArgs e)
        {
            this.FindControl<Button>("CreateIRDButton")!.Click += OnCreateIRDClick;
            this.FindControl<Button>("CancelButton")!.Click += OnCancelClick;
            this.FindControl<Button>("InputPathBrowseButton")!.Click += InputPathBrowseButtonClick;
            this.FindControl<Button>("LogPathBrowseButton")!.Click += LogPathBrowseButtonClick;
            this.FindControl<Button>("KeyPathBrowseButton")!.Click += KeyPathBrowseButtonClick;
            this.FindControl<Button>("PICPathBrowseButton")!.Click += PICPathBrowseButtonClick;

            this.FindControl<TextBox>("InputPathTextBox")!.TextChanged += (_, _) => CreateIRDViewModel.ChangeInputPath();
            this.FindControl<TextBox>("LogPathTextBox")!.TextChanged += (_, _) => CreateIRDViewModel.ChangeLogPath();
            this.FindControl<TextBox>("KeyPathTextBox")!.TextChanged += (_, _) => CreateIRDViewModel.ChangeKeyPath();
            this.FindControl<TextBox>("KeyTextBox")!.TextChanged += (_, _) => CreateIRDViewModel.ChangeKey();
            this.FindControl<TextBox>("DiscIDTextBox")!.TextChanged += (_, _) => CreateIRDViewModel.ChangeDiscID();
            this.FindControl<TextBox>("PICPathTextBox")!.TextChanged += (_, _) => CreateIRDViewModel.ChangePICPath();
            this.FindControl<TextBox>("PICTextBox")!.TextChanged += (_, _) => CreateIRDViewModel.ChangePIC();
            this.FindControl<TextBox>("LayerbreakTextBox")!.TextChanged += (_, _) => CreateIRDViewModel.ChangeLayerbreak();
        }

        /// <summary>
        /// Browse for a single input file matching the given patterns
        /// </summary>
        private async Task<string?> OpenFileAsync(string title, params string[] patterns)
        {
            List<FilePickerFileType> fileTypes = [
                new(title) { Patterns = patterns },
                FilePickerFileTypes.All,
            ];

            return await DialogService.OpenFileAsync(this, title, fileTypes);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for CreateIRDButton Click event
        /// </summary>
        private async void OnCreateIRDClick(object? sender, RoutedEventArgs e)
        {
            string? outputPath = await DialogService.SaveFileAsync(this, "Save IRD", "game.ird", IrdFileTypes);

            if (string.IsNullOrWhiteSpace(outputPath))
                return;

            string? result = CreateIRDViewModel.CreateIRD(outputPath);
            if (string.IsNullOrWhiteSpace(result))
            {
                bool? createAgain = await DisplayUserMessageAsync("IRD Create", "An IRD has been created successfully! Would you like to create another IRD?", 2, false);
                if (createAgain == false)
                    Close();
                else
                    CreateIRDViewModel.ResetFields();
            }
            else
            {
                await DisplayUserMessageAsync("Failed to create IRD", result, 1, false);
            }
        }

        /// <summary>
        /// Handler for CancelButton Click event
        /// </summary>
        private void OnCancelClick(object? sender, RoutedEventArgs e)
            => Close(false);

        /// <summary>
        /// Handler for InputPathBrowseButton Click event
        /// </summary>
        public async void InputPathBrowseButtonClick(object? sender, RoutedEventArgs e)
        {
            string? path = await OpenFileAsync("ISO", "*.iso");
            if (!string.IsNullOrWhiteSpace(path))
                CreateIRDViewModel.InputPath = path;
        }

        /// <summary>
        /// Handler for LogPathBrowseButton Click event
        /// </summary>
        public async void LogPathBrowseButtonClick(object? sender, RoutedEventArgs e)
        {
            string? path = await OpenFileAsync("GetKey Log", "*.getkey.log");
            if (!string.IsNullOrWhiteSpace(path))
                CreateIRDViewModel.LogPath = path;
        }

        /// <summary>
        /// Handler for KeyPathBrowseButton Click event
        /// </summary>
        public async void KeyPathBrowseButtonClick(object? sender, RoutedEventArgs e)
        {
            string? path = await OpenFileAsync("Key", "*.key");
            if (!string.IsNullOrWhiteSpace(path))
                CreateIRDViewModel.KeyPath = path;
        }

        /// <summary>
        /// Handler for PICPathBrowseButton Click event
        /// </summary>
        public async void PICPathBrowseButtonClick(object? sender, RoutedEventArgs e)
        {
            string? path = await OpenFileAsync("PIC", "*.physical", "*_PIC.bin", "*.PIC");
            if (!string.IsNullOrWhiteSpace(path))
                CreateIRDViewModel.PICPath = path;
        }

        #endregion
    }
}
